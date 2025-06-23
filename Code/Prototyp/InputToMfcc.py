import os
import numpy as np
import librosa
import soundfile as sf
from scipy.signal import butter, lfilter
import csv

# Fix fuer numba/librosa Caching-Probleme in geschaetzten Ordnern
os.environ["NUMBA_CACHE_DIR"] = os.path.join(os.getcwd(), "numba_cache")

def extract_mfcc_from_wav(file_path, sr=44100, n_mfcc=13):
    y, sr = librosa.load(file_path, sr=sr)
    y = bandpass(y)
    if np.max(np.abs(y)) > 0:
        y = y / np.max(np.abs(y))
    mfcc = librosa.feature.mfcc(y=y, sr=sr, n_mfcc=n_mfcc)
 
    mfcc_mean = np.mean(mfcc, axis=1)
    mfcc_std = np.std(mfcc, axis=1)
    mfcc_vector = np.concatenate([mfcc_mean, mfcc_std])  # 24-dim statt 12

    return mfcc_vector

def save_mfcc_to_csv(mfcc_vector, output_path="mfcc_vector.csv"):
    with open(output_path, "w", newline="") as f:
            writer = csv.writer(f)
            writer.writerow(mfcc_vector)

def bandpass(signal, sr = 44100, lowcut = 300.0, highcut = 1800.0, order = 6):
    nyquist = 0.5 * sr
    low = lowcut / nyquist
    high = highcut / nyquist
    b, a = butter(order, [low, high], btype='band')
    return lfilter(b, a, signal)

if __name__ == "__main__":

    input_wav = "input_temp.wav"
    if os.path.exists(input_wav):
        mfcc_vector = extract_mfcc_from_wav(input_wav)
        save_mfcc_to_csv(mfcc_vector)
        print("MFCC geschrieben in mfcc_vector.csv")
    else:
        print("Datei nicht gefunden:", input_wav)
