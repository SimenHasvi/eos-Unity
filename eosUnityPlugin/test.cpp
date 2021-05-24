#include "eos/core/Landmark.hpp"
#include "eos/core/LandmarkMapper.hpp"
#include "eos/core/write_obj.hpp"
#include "eos/fitting/fitting.hpp"
#include "eos/morphablemodel/Blendshape.hpp"
#include "eos/morphablemodel/MorphableModel.hpp"
#include "eos/render/texture_extraction.hpp"
#include "eos/cpp17/optional.hpp"
#include "eos/core/Image.hpp"

#include "Eigen/Core"

#include <string>
#include <vector>

using namespace eos;
using eos::core::Landmark;
using eos::core::LandmarkCollection;
using std::cout;
using std::endl;
using std::string;
using std::vector;
using Eigen::Vector2f;
using Eigen::Vector4f;

string share_folder = "/home/simen/MEGAsync/HKU-RA/face_reconstruction/eos_demo/share/";
string modelfile = share_folder + "lyhm.bin";
string mappingsfile = share_folder + "ibug_to_lyhm.txt";
string contourfile = share_folder + "lyhm_model_contours.json";
string edgetopologyfile = share_folder + "LYHM_edge_topology.json";

/*
morphablemodel::MorphableModel morphable_model;
core::LandmarkMapper landmark_mapper;
fitting::ModelContour model_contour;
fitting::ContourLandmarks ibug_contour;
morphablemodel::EdgeTopology edge_topology;

eos::core::LandmarkCollection<Vector2f> read_landmarks(string filename){
    LandmarkCollection<Vector2f> landmarks;
    landmarks.reserve(68);

    std::ifstream file(filename);
    if (!file)
    {
        throw std::runtime_error("Error opening given file: " + filename);
    }

    std::string line;
    int lineNum = 0;
    while (std::getline(file, line)) {
        if (lineNum++ == 0) continue; //this is the camera resolution
        std::istringstream buf(line);
        int x;
        int y;
        buf >> x >> y;
        Landmark<Vector2f> landmark;
        landmark.name = std::to_string(lineNum);
        landmark.coordinates[0] = x - 1;
        landmark.coordinates[1] = y - 1;
        landmarks.emplace_back(landmark);
    }

    return landmarks;
}

eos::core::Mesh fit(int width, int height, LandmarkCollection<Vector2f> landmarks){

    core::Mesh mesh;
    fitting::RenderingParameters rendering_params;
    std::vector<float> pca_shape_coeffs;
    std::vector<float> pca_expression_coeffs;
    std::vector<Eigen::Vector2f> fitted_image_points;

    std::tie(mesh, rendering_params) = fitting::fit_shape_and_pose(
            morphable_model,
            landmarks,
            landmark_mapper,
            width,
            height,
            edge_topology,
            ibug_contour,
            model_contour,
            5,
            cpp17::nullopt,
            30.0f,
            cpp17::nullopt,
            cpp17::nullopt,
            pca_shape_coeffs,
            pca_expression_coeffs,
            fitted_image_points);

    return mesh;
}

*/
int main() {

    /*
    morphable_model = morphablemodel::load_model(modelfile);
    landmark_mapper = core::LandmarkMapper(mappingsfile);
    model_contour = fitting::ModelContour::load(contourfile);
    ibug_contour = fitting::ContourLandmarks::load(mappingsfile);
    edge_topology = morphablemodel::load_edge_topology(edgetopologyfile);

    morphable_model = morphablemodel::MorphableModel(morphable_model.get_shape_model(), morphable_model.get_shape_model(), morphable_model.get_color_model());

    eos::core::LandmarkCollection<Vector2f> landmarks = read_landmarks(share_folder + "landmarks.pts");

    eos::core::Mesh mesh = fit(640, 480, landmarks);
    */

    return 0;
}


