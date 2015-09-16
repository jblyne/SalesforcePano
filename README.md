# SalesforcePano

This repo contains a Unity app that connects to Salesforce that displays panoramic images for Google Cardboard. It was created to demonstrate a practical way to use VR with Salesforce and was first released at Dreamforce 2015 during the session titled “A Literal 360 Degree View Of Your Data w/Google Cardboard”.

Unity Xcode iOS Project (first time setup)

When you first build the Unity code and create the Xcode project, be sure to follow these steps to get it up and running:

1. Add the CoreText and Photos framework as well as libc++.dylib.
2. Go to File -> Add File to Unity-iPhone and select the resources.bundle file in [your Unity project]/Assets/Plugins/iOS folder.

The base code for this project was taken with permission from BubblePix (www.bubblepix.com) and can be found here: https://github.com/fluidpixel/BubblePixVR

Special thanks to BubblePix and the Salesforce team behind the Salesforce Wear Pack for Myo.