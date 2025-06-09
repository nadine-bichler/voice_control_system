import librosa
import numpy as np
import csv
import os

input_folder = "./WAV_samples"  #hier liegen WAV-Dateien
output_folder = "./mfcc_csv"    #hier werden csv gespeichert

os.makedirs(output_folder, exist_ok = true)

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

        print(f"Exported {output_path}")