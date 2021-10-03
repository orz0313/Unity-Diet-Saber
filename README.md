# orz0313-UnityDietSaber
A game like BeatSaber but use webcam as input.

## Outline
This is the prototype of a BeatSaber-like game in the process of design thinking , which use webcam and Barracuda to capture and calculate your motion.</br>
※Without GPU, it does not work fine basically

## Install

1. Download .onnx file "Resnet34_3inputs_448x448_20200609.onnx "
1.1 https://digital-standard.com/threedpose/models/Resnet34_3inputs_448x448_20200609.onnx

2. Make a reference to BarracudaRunner.NN_Model</br>
2.1 Put your file "Resnet34_3inputs_448x448_20200609.onnx" in the path DietSaber/Assets/Scripts/3DPose/Model</br>
2.2 Open the PlayScene in DietSaber/Assets/Scenes</br>
2.3 Find and drag the .onnx file into the "NN Model" slot in Gameobject named "BarracudaRunner" in Inspector view.</br>

## Tutorial

1. Open the MainScene in DietSaber/Assets/Scenes
2. Start Runtime
3. Click the Start button to load PlayScene
4. Slice the cubes
5. press ESC to go back to MainScene

### Note
1. Since the trained model "Resnet34_3inputs_448x448_20200609.onnx " could not support multi target estimation , keep only one person in the camera view.</br>
2. Keep your whole body in your camera. If your limbs stick out , it will make incorrect calculation.</br>
2.1 You could make your input image size rearranged by change the number "Video Background Scale" which is in the script "Video Capture" of the Gameobject "MainTexture".</br>
3. To avoid misestimation , do not wear cloth which is the same color as background.</br>

## License
Non-commercial use

The videos named as "onegai_darling.mp4" contained in this code for debuging is not copyright free.

UnityChan license is at the followeing URL:
https://unity-chan.com/contents/license_en/
