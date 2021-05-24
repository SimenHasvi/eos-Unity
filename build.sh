#! /bin/sh

cd eosUnityPlugin
./build.sh
cd ..
mv eosUnityPlugin/libeosUnityPlugin.so eosUnity/Assets/Plugins/eosUnityPlugin
mv eosUnityPlugin/eos/share/* eosUnity/Assets/StreamingAssets/share
