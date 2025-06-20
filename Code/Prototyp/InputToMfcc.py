import os
import numpy as np
import librosa
import soundfile as sf
from scipy.signal import butter, lfilter

# 🛠 Fix für numba/librosa Caching-Probleme in geschützten Ordnern
os.environ["NUMBA_CACHE_DIR"] = os.path.join(os.getcwd(), "numba_cache")

def extract_mfcc_from_wav(file_path, sr=44100, n_mfcc=13):
    y, sr = librosa.load(file_path, sr=sr)
    mfcc = librosa.feature.mfcc(y=y, sr=sr, n_mfcc=n_mfcc)
    return np.mean(mfcc, axis=1)

def bandpass(signal, sr=44100, lowcut=300.0, highcut=1800.0, order=6):
    nyquist = 0.5 * sr
    low = lowcut / nyquist
    high = highcut / nyquist
    b, a = butter(order, [low, high], btype='band')
    return lfilter(b, a, signal)

if __name__ == "__main__":
    try:
        print("Lade input_signal.csv ...")
        pcm = np.loadtxt("input_signal.csv", delimiter=",", dtype=np.float32)

        # 🧼 Optional: Bandpass-Filter aktivieren
        # pcm = bandpass(pcm)

        print("Speichere input_temp.wav ...")
        sf.write("input_temp.wav", pcm, 44100)

        print("Extrahiere MFCC ...")
        mfcc_vector = extract_mfcc_from_wav("input_temp.wav")

        print("MFCC:", mfcc_vector)

        with open("mfcc_vector.csv", "w") as f:
            f.write(",".join(map(str, mfcc_vector)))

        print("MFCC geschrieben in mfcc_vector.csv")

        if os.path.exists("mfcc_vector.csv"):
            print("Datei existiert.")
        else:
            print("Datei NICHT gefunden.")

    except Exception as e:
        print("Fehler im Skript:")
        print(e)
