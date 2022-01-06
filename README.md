# GloveHero

Group project carried out for the course *Mechatronic Systems for Rehabilitation* at Sorbonne Université (UPMC), *Master Ingénierie pour la Santé*.

![glove_photo.jpg](https://github.com/MikFerrari/GloveHero/blob/main/glove_photo.jpg "Glove Photo")

The code allows to program an Arduino board, design a videogame using Unity and carry out signal analysis for system calibration using MATLAB.
The executable of the game is also included.

The purpose of the project is to **make rehabilitation sessions much more interactive and stimulating for the patient**.

The repository is structured as follows:
- **Arduino**: Arduino sketches that allow to acquire data, to be later analysed, or to deploy code to hardware to make the game work (real-time prediction of gestures)
- **MATLAB**: MATLAB scripts, functions and data used for signal acquisition from Arduino, threshold calibration, gestures labelling and classifier training
- **Unity**: Code used to program the game (folder: *GloveHero*) and executable to launch the game (folder: *GloveBeats*)

Authors: *Crotti Matteo, Ferrari Michele, Monti Riccardo*