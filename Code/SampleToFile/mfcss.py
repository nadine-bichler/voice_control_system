import os
import numpy as np
import librosa
import soundfile as sf
from scipy.signal import butter, lfilter
import csv

def bandpass(signal, sr = 44100, lowcut = 300.0, highcut = 1800.0, order = 6):
    nyquist = 0.5 * sr
    low = lowcut / nyquist
    high = highcut / nyquist
    b,a = butter(order, [low, high], btype='band')
    filtered_signal = lfilter(b,a,signal)
    return filtered_signal

# Basisverzeichnis ermitteln → dort wo mfcss.py liegt
base_path = os.path.dirname(os.path.abspath(__file__))

# Ordner relativ zu diesem Pfad setzen
input_folder = os.path.join(base_path, "WAV_samples")


for filename in os.listdir(input_folder):                   #Schleife über alle Dateien im Eingabeordner
    print("in python script")
    if filename.endswith(".wav"):                           #nur WAV-Dateien verarbeiten
        file_path = os.path.join(input_folder, filename)    #vollständiger Pfad zur WAV-Datei erstellen  
        y, sr = librosa.load(file_path, sr=44100)           #audio mit librosa laden (y = audio als numpy-Array, sr = samplingrate auf 10kHz)

        y = bandpass(y)

        if np.max(np.abs(y)) > 0:
            y = y / np.max(np.abs(y))

        mfcc = librosa.feature.mfcc(y=y, sr=44100, n_mfcc=13) #MFCCs berechnen mit 13 Koeffizienten pro frame
        mfcc_mean = np.mean(mfcc, axis=1)
        mfcc_std = np.std(mfcc, axis=1)
        mfcc_vector = np.concatenate([mfcc_mean, mfcc_std])  # 24-dim statt 12

        output_path = os.path.join(input_folder, filename.replace(".wav", ".csv"))
        #in csv-datei schreiben
        with open(output_path, "w", newline="") as f:
            writer = csv.writer(f)
            writer.writerow(mfcc_vector)                           
        #bestätigungsausgabe
        print(f"Exported {output_path}")