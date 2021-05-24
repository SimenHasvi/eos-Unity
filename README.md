# eos-Unity

A Unity plugin to add the morphable model fitting from eos fitting-library to Unity. I will likely keep adding functionality to this over time.

Contains the plugin and a demonstration/debug program which uses the plugin.

To build the plugin you first need to clone the eos repository (remember to use --recursive) in the eosUnityPlugin folder, replace the eos files with the ones in the modified folder, then build using cmake. The resulting dynamic library can then be added to the unity project. Also remember to copy over the share folder from eos to the StreamingAssets folder in unity for the morphable model. On Linux you can run the build.sh script to handle all of this.

The demo program shows fitting for both shape and texture, as well as real-time blendshape fitting. It uses custom blendshapes for better expressions. It uses [Dlib FaceLandmark Detector](https://assetstore.unity.com/packages/tools/integration/dlib-facelandmark-detector-64314) for the landmark detection, which you need to buy yourself of find an alternative. This demo also contains some functionality which I added myself, such as better textures.

Please be reminded that the eos library and the Surrey Face Model is not made by me so make sure you have a look at the licence at their github page before you use it.

Source for eos and SFM: (github page [here](https://github.com/patrikhuber/eos/tree/master/include/eos))

- *A Multiresolution 3D Morphable Face Model and Fitting Framework*,
   P. Huber, G. Hu, R. Tena, P. Mortazavian, W. Koppen, W. Christmas, M. 
  Rätsch, J. Kittler, International Conference on Computer Vision Theory 
  and Applications (VISAPP) 2016, Rome, Italy [[PDF]](http://www.patrikhuber.ch/files/3DMM_Framework_VISAPP_2016.pdf).
