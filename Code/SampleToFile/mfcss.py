import librosa
import numpy as np
import csv
import os

# Basisverzeichnis ermitteln → dort wo mfcss.py liegt
base_path = os.path.dirname(os.path.abspath(__file__))

# Ordner relativ zu diesem Pfad setzen
input_folder = os.path.join(base_path, "WAV_samples")


for filename in os.listdir(input_folder):                   #Schleife über alle Dateien im Eingabeordner
    if filename.endswith(".wav"):                           #nur WAV-Dateien verarbeiten
        file_path = os.path.join(input_folder, filename)    #vollständiger Pfad zur WAV-Datei erstellen  
        y, sr = librosa.load(file_path, sr=44100)           #audio mit librosa laden (y = audio als numpy-Array, sr = samplingrate auf 10kHz)

        mfcc = librosa.feature.mfcc(y=y, sr=sr, n_mfcc=13) #MFCCs berechnen mit 13 Koeffizienten pro frame
        mfcc_mean = np.mean(mfcc, axis=1)                   #arithmetisches mittel aller 220 werte

        output_path = os.path.join(input_folder, filename.replace(".wav", ".csv"))
        #in csv-datei schreiben
        with open(output_path, "w", newline="") as f:
            writer = csv.writer(f)
            writer.writerow(mfcc_mean)                           
        #bestätigungsausgabe
        print(f"Exported {output_path}")