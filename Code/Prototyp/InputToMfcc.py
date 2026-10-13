## @file
#  @brief Extracts MFCC+delta feature sequences from audio and serves them to the C# app.
#
#  This script can run in two modes:
#   - Direct: reads a WAV file (default `input_temp.wav`) and writes a CSV file.
#   - Server (`--server`): a persistent worker that avoids paying the librosa/numba import
#     cost (several seconds) for every recording. It accepts line-based commands over stdin:
#       FILE\t<input.wav>\t<output.csv>   -> "OK <frames> <dims>" or "ERR <message>"
#       PCM\t<sample_rate>\t<count>       -> followed by <count> Int16 (little-endian) samples,
#                                            answers "OK <frames> <dims>" then one feature row per line
#       QUIT                              -> exits the worker
#
#  In both modes the audio is resampled to 16 kHz, band-pass filtered, trimmed, computed into
#  MFCC (without the energy coefficient c0) plus first derivatives and per-frame normalized.

import os
import sys
import csv
import numpy as np
import librosa
from scipy.signal import butter, lfilter

# Fix for numba/librosa caching issues in restricted directories
os.environ["NUMBA_CACHE_DIR"] = os.path.join(os.getcwd(), "numba_cache")

TARGET_SR = 16000
N_MFCC = 13
LOWCUT = 300.0
HIGHCUT = 3400.0
TRIM_DB = 30.0
# Very short recordings are zero-padded up to this length so that the delta
# computation (which needs at least `width` frames) never fails. Kept short so
# that normal reference utterances (>= 10 frames) are not altered.
MIN_SAMPLES = int(0.30 * TARGET_SR)

## @brief Applies a Butterworth bandpass filter to the given audio signal.
#  @param signal Input audio signal as a NumPy array.
#  @param sr Sampling rate of the signal (default: 16000 Hz).
#  @param lowcut Lower frequency bound of the bandpass filter (default: 300 Hz).
#  @param highcut Upper frequency bound of the bandpass filter (default: 3400 Hz).
#  @param order Order of the filter (default: 6).
#  @return Filtered audio signal.
def bandpass(signal, sr=TARGET_SR, lowcut=LOWCUT, highcut=HIGHCUT, order=6):
    nyquist = 0.5 * sr
    low = lowcut / nyquist
    high = highcut / nyquist
    b, a = butter(order, [low, high], btype='band')
    return lfilter(b, a, signal)

## @brief Extracts a normalized MFCC(+delta) feature sequence from a floating point signal.
#  @param y Audio samples in [-1, 1] as a NumPy array.
#  @param sr Sampling rate of `y`.
#  @param n_mfcc Number of MFCC coefficients to extract (default: 13).
#  @return A (T x D) NumPy array; one feature vector (frame) per row.
def _features_from_signal(y, sr, n_mfcc=N_MFCC):
    if y.size == 0:
        return np.zeros((0, 2 * (n_mfcc - 1)))

    if sr != TARGET_SR:
        y = librosa.resample(y, orig_sr=sr, target_sr=TARGET_SR)
        sr = TARGET_SR

    y = bandpass(y, sr=sr)
    trimmed, _ = librosa.effects.trim(y, top_db=TRIM_DB)
    if trimmed.size > 0:
        y = trimmed
    if np.max(np.abs(y)) > 0:
        y = y / np.max(np.abs(y))

    # Zero-pad very short signals so delta (width 9) is well defined.
    if y.size < MIN_SAMPLES:
        y = np.pad(y, (0, MIN_SAMPLES - y.size))

    mfcc = librosa.feature.mfcc(y=y, sr=sr, n_mfcc=n_mfcc)
    mfcc = mfcc[1:]  # drop the energy coefficient c0

    # Use width 9 normally; shrink for unexpectedly short sequences so it never raises.
    width = min(9, mfcc.shape[1])
    if width % 2 == 0:
        width -= 1
    width = max(width, 1)
    delta = librosa.feature.delta(mfcc, width=width)
    features = np.vstack([mfcc, delta])  # (D, T)

    # Per-frame normalization across the coefficient dimension (removes per-frame offset/scale)
    features = (features - features.mean(axis=0, keepdims=True)) / (
        features.std(axis=0, keepdims=True) + 1e-9)

    return features.T  # (T, D)

## @brief Extracts a normalized MFCC(+delta) feature sequence from a WAV file.
#  @param file_path Full path to the .wav file.
#  @param sr Target sample rate for loading the audio (default: 16000 Hz).
#  @param n_mfcc Number of MFCC coefficients to extract (default: 13).
#  @return A (T x D) NumPy array; one feature vector (frame) per row.
def extract_features(file_path, sr=TARGET_SR, n_mfcc=N_MFCC):
    y, sr = librosa.load(file_path, sr=sr)
    return _features_from_signal(y, sr, n_mfcc)

## @brief Extracts a normalized MFCC(+delta) feature sequence from raw Int16 PCM.
#  @param samples Int16 PCM samples as a NumPy array.
#  @param sr Sampling rate of the PCM samples.
#  @param n_mfcc Number of MFCC coefficients to extract (default: 13).
#  @return A (T x D) NumPy array; one feature vector (frame) per row.
def extract_features_from_pcm(samples, sr, n_mfcc=N_MFCC):
    y = np.asarray(samples).astype(np.float32) / 32768.0
    return _features_from_signal(y, sr, n_mfcc)

## @brief Saves a feature sequence to a CSV file (one frame per row).
#  @param features A (T x D) NumPy array.
#  @param output_path Path to the output CSV file (default: "mfcc_vector.csv").
def save_features_to_csv(features, output_path="mfcc_vector.csv"):
    with open(output_path, "w", newline="") as f:
        writer = csv.writer(f)
        for frame in features:
            writer.writerow([f"{v:.6f}" for v in frame])

## @brief Reads exactly `count` bytes from a binary stream.
#  @param stream Binary input stream.
#  @param count Number of bytes to read.
#  @return The read bytes.
def _read_exact(stream, count):
    buf = bytearray()
    while len(buf) < count:
        chunk = stream.read(count - len(buf))
        if not chunk:
            raise EOFError("unexpected end of input")
        buf.extend(chunk)
    return bytes(buf)

## @brief Writes a feature sequence to a binary stream (header line + one row per frame).
#  @param out Binary output stream.
#  @param features A (T x D) NumPy array.
def _write_features(out, features):
    rows, cols = features.shape
    out.write(f"OK {rows} {cols}\n".encode())
    for frame in features:
        out.write((",".join(f"{v:.6f}" for v in frame) + "\n").encode())
    out.flush()

## @brief Runs the persistent worker loop that processes requests from stdin.
def serve():
    inp = sys.stdin.buffer
    out = sys.stdout.buffer
    out.write(b"READY\n")
    out.flush()
    while True:
        line = inp.readline()
        if not line:
            break
        parts = line.decode(errors="replace").rstrip("\r\n").split("\t")
        command = parts[0]
        if not command:
            continue
        if command == "QUIT":
            break
        try:
            if command == "FILE":
                input_wav = parts[1]
                output_csv = parts[2] if len(parts) > 2 else "mfcc_vector.csv"
                features = extract_features(input_wav)
                save_features_to_csv(features, output_csv)
                out.write(f"OK {features.shape[0]} {features.shape[1]}\n".encode())
                out.flush()
            elif command == "PCM":
                sr = int(parts[1])
                count = int(parts[2])
                data = _read_exact(inp, count * 2)
                samples = np.frombuffer(data, dtype="<i2")
                features = extract_features_from_pcm(samples, sr)
                _write_features(out, features)
            else:
                out.write(b"ERR unknown command\n")
                out.flush()
        except Exception as exc:  # keep the worker alive on per-request errors
            out.write(f"ERR {type(exc).__name__}: {exc}\n".encode())
            out.flush()

## @brief Main execution block.
if __name__ == "__main__":
    if len(sys.argv) > 1 and sys.argv[1] == "--server":
        serve()
    else:
        input_wav = sys.argv[1] if len(sys.argv) > 1 else "input_temp.wav"
        output_csv = sys.argv[2] if len(sys.argv) > 2 else "mfcc_vector.csv"

        if os.path.exists(input_wav):
            features = extract_features(input_wav)
            save_features_to_csv(features, output_csv)
            print(f"MFCC geschrieben in {output_csv} ({features.shape[0]} x {features.shape[1]})")
        else:
            print("Datei nicht gefunden:", input_wav)
