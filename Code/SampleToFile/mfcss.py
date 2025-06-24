## @file
#  @brief Extracts MFCC features from WAV files and exports them to CSV format.
#         Applies bandpass filtering before MFCC extraction.
#
#  This script iterates through all WAV files in a given input directory, processes
#  them to extract MFCC feature vectors, and saves the result in CSV format.
#  A bandpass filter is applied to clean the signal before feature extraction.
#
#  Dependencies: os, numpy, librosa, soundfile, scipy, csv

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
#  @param sr Sample rate for reading the audio file (default: 44100 Hz).
#  @param n_mfcc Number of MFCC coefficients to extract (default: 13).
#  @return A 26-dimensional feature vector consisting of mean and standard deviation of MFCCs.
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

## @brief Applies a Butterworth bandpass filter to an audio signal.
#  @param signal Input audio signal as a NumPy array.
#  @param sr Sampling rate of the signal (default: 44100 Hz).
#  @param lowcut Lower frequency bound of the bandpass filter (default: 500 Hz).
#  @param highcut Upper frequency bound of the bandpass filter (default: 3000 Hz).
#  @param order Order of the Butterworth filter (default: 6).
#  @return The filtered audio signal.
def bandpass(signal, sr=44100, lowcut=500.0, highcut=3000.0, order=6):
    nyquist = 0.5 * sr
    low = lowcut / nyquist
    high = highcut / nyquist
    b, a = butter(order, [low, high], btype='band')
    filtered_signal = lfilter(b, a, signal)
    return filtered_signal

# Determine base directory → where this script is located
base_path = os.path.dirname(os.path.abspath(__file__))

# Set input folder relative to script location
input_folder = os.path.join(base_path, "wavTest")

## @brief Main processing loop for extracting MFCCs from all WAV files in the input directory.
for filename in os.listdir(input_folder):
    if filename.endswith(".wav"):
        file_path = os.path.join(input_folder, filename)
        mfcc_vector = extract_mfcc_from_wav(file_path)

        output_path = os.path.join(input_folder, filename.replace(".wav", ".csv"))
        # Write the MFCC vector to a CSV file
        with open(output_path, "w", newline="") as f:
            writer = csv.writer(f)
            writer.writerow(mfcc_vector)
        print(f"Exported {output_path}")
