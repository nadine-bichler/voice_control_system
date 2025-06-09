<<<<<<< HEAD
﻿import librosa
import numpy as np
import csv
import os

input_folder = "./WAV_samples"  #hier liegen WAV-Dateien
output_folder = "./mfcc_csv"    #hier werden csv gespeichert

os.makedirs(output_folder, exist_ok = True)

for filename in os.listdir(input_folder):
    if filename.endswith(".wav"):
        file_path = os.path.join(input_folder, filename)
        y, sr = librosa.load(file_path, sr=16000)

        mfcc = librosa.feature.mfcc(y=y, sr=sr, n_mfcc=13)
        mfcc_mean = np.mean(mfcc, axis=1)   #13 dimensionaler vektor

        output_path = os.path.join(output_folder, filename.replace(".wav", ".csv"))
        with open(output_path, "w", newline="") as f:
            writer = csv.writer(f)
            writer.writerow(mfcc_mean)

=======
import librosa
import numpy as np
import csv
import os

input_folder = "./WAV_samples"                              #hier liegen WAV-Dateien
output_folder = "./mfcc_csv"                                #hier werden csv gespeichert

os.makedirs(output_folder, exist_ok = true)                 #falls Ordner nicht vorhanden, neuen anlegen

for filename in os.listdir(input_folder):                   #Schleife über alle Dateien im Eingabeordner
    if filename.endswith(".wav"):                           #nur WAV-Dateien verarbeiten
        file_path = os.path.join(input_folder, filename)    #vollständiger Pfad zur WAV-Datei erstellen  
        y, sr = librosa.load(file_path, sr=16000)           #audio mit librosa laden (y = audio als numpy-Array, sr = samplingrate auf 10kHz)

        mfcc = librosa.feature.mfcc(y=y, sr=sr, n_mfcc=13)  #MFCCs berechnen
        mfcc_mean = np.mean(mfcc, axis=1)                   #13 dimensionaler vektor, mittelfeld über alle Zeitfenster berechnen

        output_path = os.path.join(output_folder, filename.replace(".wav", ".csv"))
        #in csv-datei schreiben
        with open(output_path, "w", newline="") as f:
            writer = csv.writer(f)
            writer.writerow(mfcc_mean)
        #bestätigungsausgabe
>>>>>>> 35d96d2a250bb36271c95318b44b6f9703da8ca5
        print(f"Exported {output_path}")