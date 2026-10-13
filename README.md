# Project_1_Team_1
# Prototype: Voice Control for Injection Molding Machines

## Project Overview & Focus

This prototype serves to evaluate a voice-based control system for modern injection molding machines from Kunststoff4All. Based on a Windows industrial PC (the platform of the existing presentation layer), it investigates whether core functions can be controlled error-free and ergonomically via simple voice commands (digits 1–9).

**Important Project Note:**  
The primary focus of this project was on methodological and standard-compliant design as well as the creation of the required project documentation (requirements specification, functional specification, and architectural description) in accordance with Kunststoff4All's internal V-model standard. The programmatic implementation of the WPF prototype was therefore primarily realized using Artificial Intelligence (AI) to efficiently ensure compliance with the software architecture and quality standards.

## Core Technical Features

- **Audio Capture:** Use of the industrial PC's standard microphone for real-time recording of ambient noise and voice commands.
- **Signal Processing:** Visual representation of the audio signal as a time-domain signal directly in the GUI.
- **Frequency Analysis:** Implementation of a Fourier transformation (FFT) for frequency analysis of the time-domain signal to isolate and detect voice commands (digits 1–9).
- **Modular Architecture:** Strictly modular structure of the internal algorithms (audio-in, FFT processing, recognition logic) to enable seamless reusability of code components in the productive Presentation Layer (PL).

## GUI & Software Ergonomics
The user interface was implemented in WPF (Windows Presentation Foundation) and meets common software ergonomic requirements for industrial use:
- **Modern UI Design:** An appealing, professional appearance to convince management and the Product Owner (PO).
- **Ease of Use:** Clear visual feedback upon recognition of voice commands.
- **Real-Time Visualization:** Direct display of the audio time-domain signal and the recognized command on the GUI.

## Lessons Learned: 
In retrospect and to optimize future project phases, the team evaluated that alternative, uniform templates for creating the documents would have been more effective.
