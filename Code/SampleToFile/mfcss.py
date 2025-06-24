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
    mfcc_vector = np.concatenate([mfcc_mean, mfcc_std])  # 26-dim statt 12

    return mfcc_vector

def bandpass(signal, sr = 44100, lowcut = 500.0, highcut = 3000.0, order = 6):
    nyquist = 0.5 * sr
    low = lowcut / nyquist
    high = highcut / nyquist
    b,a = butter(order, [low, high], btype='band')
    filtered_signal = lfilter(b,a,signal)
    return filtered_signal

# Basisverzeichnis ermitteln → dort wo mfcss.py liegt
base_path = os.path.dirname(os.path.abspath(__file__))

# Ordner relativ zu diesem Pfad setzen
input_folder = os.path.join(base_path, "wavTest")


for filename in os.listdir(input_folder):                   #Schleife über alle Dateien im Eingabeordner
    if filename.endswith(".wav"):                           #nur WAV-Dateien verarbeiten
        file_path = os.path.join(input_folder, filename)    #vollständiger Pfad zur WAV-Datei erstellen  
        mfcc_vector=extract_mfcc_from_wav(file_path)

        output_path = os.path.join(input_folder, filename.replace(".wav", ".csv"))
        #in csv-datei schreiben
        with open(output_path, "w", newline="") as f:
            writer = csv.writer(f)
            writer.writerow(mfcc_vector)                           
        #bestätigungsausgabe
        print(f"Exported {output_path}")