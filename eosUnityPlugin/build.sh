#! /bin/sh

git clone https://github.com/patrikhuber/eos --recursive
cp modified/Blendshape.hpp eos/include/eos/morphablemodel/Blendshape.hpp
cp modified/blendshape_fitting.hpp eos/include/eos/fitting/blendshape_fitting.hpp
cmake CMakeLists.txt
make
