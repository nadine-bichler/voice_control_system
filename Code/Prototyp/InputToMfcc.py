import numpy as np
import librosa
import os
import soundfile as sf
from scipy.signal import butter, lfilter

def extract_mfcc_from_wav(file_path, sr=44100, n_mfcc=13):
    y, sr = librosa.load(file_path, sr=sr)
    mfcc = librosa.feature.mfcc(
        y=y,
        sr=sr,
        n_mfcc=n_mfcc
    )
    return np.mean(mfcc, axis=1)

def bandpass(signal, sr = 44100, lowcut = 300.0, highcut = 1800.0, order = 6):
    nyquist = 0.5 * sr
    low = lowcut / nyquist
    high = highcut / nyquist
    b,a = butter(order, [low, high], btype='band')
    filtered_signal = lfilter(b,a,signal)
    return filtered_signal


if __name__ == "__main__":
    # PCM-Werte aus CSV laden (eine Zeile mit Float-Werten)
    pcm = np.loadtxt("input_signal.csv", delimiter=",", dtype=np.float32)

    #normalisierung auf [-1, 1] (wie librosa.load)
    #if np.max(np.abs(pcm)) > 0:
        #pcm = pcm / np.max(np.abs(pcm))

    #bandpass
    #pcm_filtered = bandpass(pcm)

    #WAV speichern
    sf.write("input_temp.wav", pcm, 44100)

    #mfcc aus WAV
    mfcc_vector = extract_mfcc_from_wav("input_temp.wav")
    print("MFCC:", mfcc_vector)


    
    # als CSV speichern
    with open("mfcc_vector.csv", "w") as f:
        f.write(",".join(map(str, mfcc_vector)))


# Nach dem Schreiben:
print("MFCC geschrieben in mfcc_vector.csv")

# Optional: prüfe wirkliches Schreiben
if os.path.exists("mfcc_vector.csv"):
    print("Datei existiert.")
else:
    print("Datei NICHT gefunden.")


