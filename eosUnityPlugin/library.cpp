/********************
* THIS FILE IS MADE BY SIMEN GAD HASVI
* THE OTHER DEPENDENCIES ARE NOT
* load_blendshapes_custom in include/eos/morphablemodel/Blendshape.hpp was added by me
* fit_blendshapes_to_landmarks_nnls in include/eos/fitting/blendshape_fitting.hpp was slightly modified by me
********************/


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

// Use C linkage on the functions in this block
extern "C" {
    // Variables which are loaded at initialization
    morphablemodel::MorphableModel morphable_model;
    core::LandmarkMapper landmark_mapper;
    vector<morphablemodel::Blendshape> blendshapes;
    fitting::ModelContour model_contour;
    fitting::ContourLandmarks ibug_contour;
    morphablemodel::EdgeTopology edge_topology;

    // Variables used at runtime
    core::Mesh mesh;
    Eigen::VectorXf sample;
    std::vector<float> pca_coeffs;
    fitting::RenderingParameters rendering_params;
    std::vector<float> blendshape_coeffs;
    std::vector<Eigen::Vector2f> fitted_image_points;
    fitting::ScaledOrthoProjectionParameters pose;

    // initialize the plugin
    int initialize(
            char* modelfile,
            char* mappingsfile,
            char* blendshapesfile,
            char* contourfile,
            char* edgetopologyfile,
            float *vertices_out,
            int *triangles_out,
            float *texcoords_out) {
        morphable_model = morphablemodel::load_model(modelfile);
        landmark_mapper = core::LandmarkMapper(mappingsfile);
        blendshapes = morphablemodel::load_blendshapes(blendshapesfile);
        blendshape_coeffs = vector<float>(blendshapes.size());
        morphable_model = morphablemodel::MorphableModel(
                morphable_model.get_shape_model(),
                blendshapes,
                morphable_model.get_color_model(),
                cpp17::nullopt,
                morphable_model.get_texture_coordinates());
        model_contour = fitting::ModelContour::load(contourfile);
        ibug_contour = fitting::ContourLandmarks::load(mappingsfile);
        edge_topology = morphablemodel::load_edge_topology(edgetopologyfile);

        mesh = morphable_model.get_mean();
        sample = morphable_model.get_shape_model().draw_sample(pca_coeffs);

        for (int i = 0; i < mesh.vertices.size(); ++i) {
            vertices_out[i*3] = mesh.vertices[i].x();
            vertices_out[(i*3)+1] = mesh.vertices[i].y();
            vertices_out[(i*3)+2] = mesh.vertices[i].z();
        }
        for (int i = 0; i < mesh.tvi.size(); ++i) {
            triangles_out[i*3] = mesh.tvi[i][0];
            triangles_out[(i*3)+1] = mesh.tvi[i][1];
            triangles_out[(i*3)+2] = mesh.tvi[i][2];
        }
        for (int i = 0; i < mesh.texcoords.size(); ++i) {
            texcoords_out[i * 2] = mesh.texcoords[i][0];
            texcoords_out[i * 2 + 1] = mesh.texcoords[i][1];
        }

        return blendshapes.size();
    }

    // initialize the plugin with custom blendshapes
    int initialize_custom(
            char* modelfile,
            char* mappingsfile,
            char* blendshapesfile,
            char* contourfile,
            char* edgetopologyfile,
            float *vertices_out,
            int *triangles_out,
            float *texcoords_out) {
        morphable_model = morphablemodel::load_model(modelfile);
        landmark_mapper = core::LandmarkMapper(mappingsfile);
        blendshapes = morphablemodel::load_blendshapes_custom(blendshapesfile);
        blendshape_coeffs = vector<float>(blendshapes.size());
        morphable_model = morphablemodel::MorphableModel(
                morphable_model.get_shape_model(),
                blendshapes,
                morphable_model.get_color_model(),
                cpp17::nullopt,
                morphable_model.get_texture_coordinates());
        model_contour = fitting::ModelContour::load(contourfile);
        ibug_contour = fitting::ContourLandmarks::load(mappingsfile);
        edge_topology = morphablemodel::load_edge_topology(edgetopologyfile);

        mesh = morphable_model.get_mean();
        sample = morphable_model.get_shape_model().draw_sample(pca_coeffs);

        for (int i = 0; i < mesh.vertices.size(); ++i) {
            vertices_out[i*3] = mesh.vertices[i].x();
            vertices_out[(i*3)+1] = mesh.vertices[i].y();
            vertices_out[(i*3)+2] = mesh.vertices[i].z();
        }
        for (int i = 0; i < mesh.tvi.size(); ++i) {
            triangles_out[i*3] = mesh.tvi[i][0];
            triangles_out[(i*3)+1] = mesh.tvi[i][1];
            triangles_out[(i*3)+2] = mesh.tvi[i][2];
        }
        for (int i = 0; i < mesh.texcoords.size(); ++i) {
            texcoords_out[i * 2] = mesh.texcoords[i][0];
            texcoords_out[i * 2 + 1] = mesh.texcoords[i][1];
        }

        return blendshapes.size();
    }

    // fit the shape of the landmarks
    void fit_shape(int width, int height, double *landmarksArray, float *vertices_out) {

        LandmarkCollection<Vector2f> landmarks;
        landmarks.reserve(68);
        for (int i = 0; i < 68; ++i) {
            Landmark<Vector2f> landmark;
            landmark.name = std::to_string(i+1);
            landmark.coordinates[0] = landmarksArray[i*2] - 1;
            landmark.coordinates[1] = landmarksArray[i*2+1] - 1;
            landmarks.emplace_back(landmark);
        }

        vector<Vector4f> model_points;
        vector<int> vertex_indices;
        vector<Vector2f> image_points;

        for (auto & landmark : landmarks) {
            auto converted_name = landmark_mapper.convert(landmark.name);
            if (!converted_name) continue;
            int vertex_idx = std::stoi(converted_name.value());
            auto vertex = morphable_model.get_shape_model().get_mean_at_point(vertex_idx);
            model_points.emplace_back(Vector4f(vertex.x(), vertex.y(), vertex.z(), 1.0f));
            vertex_indices.emplace_back(vertex_idx);
            image_points.emplace_back(landmark.coordinates);
        }

        pose = fitting::estimate_orthographic_projection_linear(
                image_points,
                model_points,
                true,
                height);
        rendering_params = fitting::RenderingParameters(pose, width, height);

        const Eigen::Matrix<float, 3, 4> affine_from_ortho = fitting::get_3x4_affine_camera_matrix(
                rendering_params,
                width,
                height);

        pca_coeffs = fitting::fit_shape_to_landmarks_linear(
                morphable_model.get_shape_model(),
                affine_from_ortho,
                image_points,
                vertex_indices);

        mesh = morphable_model.draw_sample(pca_coeffs, vector<float>());
        sample = morphable_model.get_shape_model().draw_sample(pca_coeffs);

        for (int i = 0; i < mesh.vertices.size(); ++i) {
            vertices_out[i*3] = mesh.vertices[i].x();
            vertices_out[(i*3)+1] = mesh.vertices[i].y();
            vertices_out[(i*3)+2] = mesh.vertices[i].z();
        }
    }

    // fit the shape and texture of the incomming landmarks and image
    void fit_shape_texture(int *img_res, double *landmarksArray, float *vertices_out, int *image) {

        fit_shape(img_res[0], img_res[1], landmarksArray, vertices_out);

        eos::core::Image4u converted_image(img_res[1], img_res[0]);
        for (int r = 0; r < img_res[1]; ++r) {
            for (int c = 0; c < img_res[0]; ++c) {
                converted_image(r, c)[0] = image[(abs(r - (img_res[1] - 1)) * img_res[0] + c) * 3];
                converted_image(r, c)[1] = image[(abs(r - (img_res[1] - 1)) * img_res[0] + c) * 3 + 1];
                converted_image(r, c)[2] = image[(abs(r - (img_res[1] - 1)) * img_res[0] + c) * 3 + 2];
                converted_image(r, c)[3] = 255;
            }
        }

        const core::Image4u texturemap = render::extract_texture(
                mesh,
                rendering_params.get_modelview(),
                rendering_params.get_projection(),
                render::ProjectionType::Orthographic,
                converted_image);

        img_res[0] = texturemap.width();
        img_res[1] = texturemap.height();

        for (int r = 0; r < img_res[1]; ++r) {
            for (int c = 0; c < img_res[0]; ++c) {
                image[(r * img_res[0] + c) * 3] = texturemap(r, c)[0];
                image[(r * img_res[0] + c) * 3 + 1] = texturemap(r, c)[1];
                image[(r * img_res[0] + c) * 3 + 2] = texturemap(r, c)[2];
            }
        }
    }

    // apply expression to the face
    void apply_expression(float *expression_coeffs, float *vertices_out) {

        for (int i = 0; i < blendshapes.size(); ++i) {
            blendshape_coeffs[i] = expression_coeffs[i];
        }

        mesh = morphablemodel::sample_to_mesh(
                sample + draw_sample(morphable_model.get_expression_model().value(), blendshape_coeffs),
                morphable_model.get_color_model().get_mean(),
                morphable_model.get_shape_model().get_triangle_list(),
                morphable_model.get_color_model().get_triangle_list(),
                morphable_model.get_texture_coordinates(),
                morphable_model.get_texture_triangle_indices());

        for (int i = 0; i < mesh.vertices.size(); ++i) {
            vertices_out[i*3] = mesh.vertices[i].x();
            vertices_out[(i*3)+1] = mesh.vertices[i].y();
            vertices_out[(i*3)+2] = mesh.vertices[i].z();
        }
    }

    // apply expression to the given face sample
    void apply_expression_to_sample(float *expression_coeffs, float *shape_sample) {
        for (int i = 0; i < sample.size(); ++i) {
            sample[i] = shape_sample[i];
        }
        apply_expression(expression_coeffs, shape_sample);
    }

    // fit the shape, expression, and rotation with the given landmarks
    void fit_shape_expression_rotation(int width, int height, double *landmarksArray, float *rotation_out, float *vertices_out) {
        LandmarkCollection<Vector2f> landmarks;
        landmarks.reserve(68);
        for (int i = 0; i < 68; ++i) {
            Landmark<Vector2f> landmark;
            landmark.name = std::to_string(i+1);
            landmark.coordinates[0] = landmarksArray[i*2] - 1;
            landmark.coordinates[1] = landmarksArray[i*2+1] - 1;
            landmarks.emplace_back(landmark);
        }

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
                pca_coeffs,
                blendshape_coeffs,
                fitted_image_points);

        sample = morphable_model.get_shape_model().draw_sample(pca_coeffs);

        for (int i = 0; i < mesh.vertices.size(); ++i) {
            vertices_out[i*3] = mesh.vertices[i].x();
            vertices_out[(i*3)+1] = mesh.vertices[i].y();
            vertices_out[(i*3)+2] = mesh.vertices[i].z();
        }

        rotation_out[0] = rendering_params.get_rotation().x;
        rotation_out[1] = rendering_params.get_rotation().y;
        rotation_out[2] = rendering_params.get_rotation().z;
        rotation_out[3] = rendering_params.get_rotation().w;
    }

    // fit expression and rotation with the given landmarks
    void fit_expression_rotation(int width, int height, double *landmarksArray, float *rotation_out, float *blendshapes_out, float *vertices_out, float *blendshape_modifiers) {
        LandmarkCollection<Vector2f> landmarks;
        landmarks.reserve(68);
        for (int i = 0; i < 68; ++i) {
            Landmark<Vector2f> landmark;
            landmark.name = std::to_string(i+1);
            landmark.coordinates[0] = landmarksArray[i*2] - 1;
            landmark.coordinates[1] = landmarksArray[i*2+1] - 1;
            landmarks.emplace_back(landmark);
        }

        vector<Vector4f> model_points;
        vector<int> vertex_indices;
        vector<Vector2f> image_points;

        for (auto & landmark : landmarks) {
            auto converted_name = landmark_mapper.convert(landmark.name);
            if (!converted_name) continue;
            int vertex_idx = std::stoi(converted_name.value());
            auto vertex = morphable_model.get_shape_model().get_mean_at_point(vertex_idx);
            model_points.emplace_back(Vector4f(vertex.x(), vertex.y(), vertex.z(), 1.0f));
            vertex_indices.emplace_back(vertex_idx);
            image_points.emplace_back(landmark.coordinates);
        }

        pose = fitting::estimate_orthographic_projection_linear(
                image_points,
                model_points,
                true,
                height);

        rendering_params = fitting::RenderingParameters(pose, width, height);

        const Eigen::Matrix<float, 3, 4> affine_from_ortho = fitting::get_3x4_affine_camera_matrix(
                rendering_params,
                width,
                height);

        blendshape_coeffs = fitting::fit_expressions(
                morphable_model.get_expression_model().value(),
                morphable_model.get_shape_model().draw_sample(pca_coeffs),
                affine_from_ortho,
                image_points,
                vertex_indices,
                cpp17::nullopt,
                cpp17::nullopt);

        for (int i = 0; i < blendshape_coeffs.size(); ++i) {
            //add multiplier
            blendshape_coeffs[i] = blendshape_coeffs[i] * blendshape_modifiers[i*3+2] >=1 ? 1 : blendshape_coeffs[i] * blendshape_modifiers[i*3+2];
            //add the thresholds
            if (blendshape_coeffs[i] <= blendshape_modifiers[i*3])
                blendshape_coeffs[i] = 0.0f;
            if (blendshape_coeffs[i] >= blendshape_modifiers[i*3+1])
                blendshape_coeffs[i] = 1.0f;

            blendshapes_out[i] = blendshape_coeffs[i];
        }

        Eigen::VectorXf shape_instance = sample + draw_sample(morphable_model.get_expression_model().value(), blendshape_coeffs);
        for (int i = 0; i < shape_instance.size(); ++i) {
            vertices_out[i] = shape_instance[i];
        }

        rotation_out[0] = rendering_params.get_rotation().x;
        rotation_out[1] = rendering_params.get_rotation().y;
        rotation_out[2] = rendering_params.get_rotation().z;
        rotation_out[3] = rendering_params.get_rotation().w;
    }

    // fit the expression and rotation with the given landmarks to the given face sample
    void fit_expression_rotation_to_sample(int width, int height, double *landmarksArray, float *rotation_out, float *blendshapes_out, float *shape_sample, float *blendshape_modifiers) {
        for (int i = 0; i < sample.size(); ++i) {
            sample[i] = shape_sample[i];
        }
        fit_expression_rotation(width, height, landmarksArray, rotation_out, blendshapes_out, shape_sample, blendshape_modifiers);
    }
}
