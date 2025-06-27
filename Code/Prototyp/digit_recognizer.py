# -*- coding: utf-8 -*-
import os
import numpy as np
import librosa
from scipy.signal import butter, lfilter
from sklearn.preprocessing import StandardScaler, normalize

# Bandpassfilter anwenden
def bandpass(signal, sr=44100, lowcut=500.0, highcut=3000.0, order=6):
    nyquist = 0.5 * sr
    low = lowcut / nyquist
    high = highcut / nyquist
    b, a = butter(order, [low, high], btype='band')
    return lfilter(b, a, signal)

# Trimmen von Stille basierend auf Energie (RMS)
def trim_silence(y, sr=44100, threshold=0.2):
    energy = librosa.feature.rms(y=y)[0]
    frames = np.nonzero(energy > threshold)[0]
    if len(frames) > 0:
        start = librosa.frames_to_samples(frames[0])
        end = librosa.frames_to_samples(frames[-1] + 1)
        return y[start:end]
    return y  # Rückgabe unverändert, falls keine laute Stelle gefunden

# MFCC-Vektor extrahieren
def extract_mfcc(file_path, sr=44100, n_mfcc=13):
    y, sr = librosa.load(file_path, sr=sr)

    # Schritt 1: Trimmen
    y = trim_silence(y, sr)

    # Schritt 2: Bandpassfilter
    y = bandpass(y, sr)

    # Schritt 3: Normalisierung (Peak-Normalisierung)
    if np.max(np.abs(y)) > 0:
        y = y / np.max(np.abs(y))

    # Schritt 4: MFCC berechnen
    mfcc = librosa.feature.mfcc(y=y, sr=sr, n_mfcc=n_mfcc)

    # Schritt 5: Standardisierung (pro Koeffizient)
    scaler = StandardScaler()
    mfcc_scaled = scaler.fit_transform(mfcc.T).T  # Transpose, dann zurück

    # Optional: L2-Normalisierung
    mfcc_scaled = normalize(mfcc_scaled, axis=1)

    # Schritt 6: Mittelwert & Standardabweichung als Feature-Vektor (26-dim)
    mfcc_mean = np.mean(mfcc_scaled, axis=1)
    mfcc_std = np.std(mfcc_scaled, axis=1)
    return np.concatenate([mfcc_mean, mfcc_std])

# Ähnlichkeitsmaß: Kosinus
def cosine_similarity(a, b):
    return np.dot(a, b) / (np.linalg.norm(a) * np.linalg.norm(b))

# Digit erkennen durch Vergleich mit Samples
def recognize_digit(input_mfcc, sample_folder, threshold=0.8):
    best_digit = -1
    best_score = 0.0

    for digit in range(1, 10):
        digit_files = [f for f in os.listdir(sample_folder) if f.endswith(f"{digit}.csv")]
        similarities = []

        for file in digit_files:
            sample_path = os.path.join(sample_folder, file)
            try:
                sample_vector = np.loadtxt(sample_path, delimiter=",")
            except Exception as e:
                print(f"Fehler beim Laden von {file}: {e}")
                continue

            if len(sample_vector) != len(input_mfcc):
                continue

            score = cosine_similarity(input_mfcc, sample_vector)
            similarities.append(score)

        if similarities:
            avg_score = np.mean(similarities)
            if avg_score > best_score:
                best_score = avg_score
                best_digit = digit

    if best_score >= threshold:
        print(f"{best_digit},{best_score}")  # Nur wenn tatsächlich erkannt
        return best_digit
    else:
        return -1

# Hauptfunktion
if __name__ == "__main__":
    input_wav = "input_temp.wav"
    current_dir = os.path.dirname(os.path.abspath(__file__))
    sample_folder = os.path.abspath(os.path.join(current_dir, "..", "SampleToFile", "WAV_samples"))

    if not os.path.isfile(input_wav):
        print(f"Eingabedatei nicht gefunden: {input_wav}")
        exit(1)

    if not os.path.isdir(sample_folder):
        print(f"Sample-Ordner nicht gefunden: {sample_folder}")
        exit(1)

    mfcc_vector = extract_mfcc(input_wav)
    recognized = recognize_digit(mfcc_vector, sample_folder)

    if recognized == -1:
        print("Keine Ziffer erkannt.")
    else:
        print(f"Erkannte Ziffer: {recognized}")
