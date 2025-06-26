## @file
#  @brief Extracts MFCC features from a single WAV file and saves them to a CSV file.
#
#  This script is intended to be run directly. It checks for the existence of a WAV file
#  named `input_temp.wav`, extracts MFCC features from it using a bandpass filter, and
#  writes the resulting feature vector to a CSV file.

import os
import numpy as np
import librosa
import soundfile as sf
from scipy.signal import butter, lfilter
import csv

# Fix for numba/librosa caching issues in restricted directories
os.environ["NUMBA_CACHE_DIR"] = os.path.join(os.getcwd(), "numba_cache")

## @brief Extracts MFCC features from a WAV audio file.
#  @param file_path Full path to the .wav file.
#  @param sr Sample rate for loading the audio (default: 44100 Hz).
#  @param n_mfcc Number of MFCC coefficients to extract (default: 13).
#  @return A 26-dimensional NumPy array containing the mean and standard deviation of the MFCCs.
def extract_mfcc_from_wav(file_path, sr=44100, n_mfcc=13):
    y, sr = librosa.load(file_path, sr=sr)
    y = bandpass(y)
    if np.max(np.abs(y)) > 0:
        y = y / np.max(np.abs(y))
    mfcc = librosa.feature.mfcc(y=y, sr=sr, n_mfcc=n_mfcc)

    mfcc_mean = np.mean(mfcc, axis=1)
    mfcc_std = np.std(mfcc, axis=1)
    mfcc_vector = np.concatenate([mfcc_mean, mfcc_std])  # 26-dim instead of 13

    return mfcc_vector

## @brief Saves a given MFCC vector to a CSV file.
#  @param mfcc_vector NumPy array or list containing MFCC features.
#  @param output_path Path to the output CSV file (default: "mfcc_vector.csv").
def save_mfcc_to_csv(mfcc_vector, output_path="mfcc_vector.csv"):
    with open(output_path, "w", newline="") as f:
        writer = csv.writer(f)
        writer.writerow(mfcc_vector)

## @brief Applies a Butterworth bandpass filter to the given audio signal.
#  @param signal Input audio signal as a NumPy array.
#  @param sr Sampling rate of the signal (default: 44100 Hz).
#  @param lowcut Lower frequency bound of the bandpass filter (default: 500 Hz).
#  @param highcut Upper frequency bound of the bandpass filter (default: 3000 Hz).
#  @param order Order of the filter (default: 6).
#  @return Filtered audio signal.
def bandpass(signal, sr=44100, lowcut=500.0, highcut=3000.0, order=6):
    nyquist = 0.5 * sr
    low = lowcut / nyquist
    high = highcut / nyquist
    b, a = butter(order, [low, high], btype='band')
    return lfilter(b, a, signal)

## @brief Main execution block.
#  Loads a hardcoded WAV file ("input_temp.wav"), extracts MFCCs, and saves them to a CSV file.
if __name__ == "__main__":

    input_wav = "input_temp.wav"
    if os.path.exists(input_wav):
        mfcc_vector = extract_mfcc_from_wav(input_wav)
        save_mfcc_to_csv(mfcc_vector)
        print("MFCC geschrieben in mfcc_vector.csv")
    else:
        print("Datei nicht gefunden:", input_wav)
