#cmake_minimum_required(VERSION 3.17)
project(eosUnityPlugin)

set(CMAKE_CXX_STANDARD 14)

include_directories("eos/include", "eos/3rdparty")

set(HEADERS
    ${CMAKE_CURRENT_SOURCE_DIR}/eos/include/eos/core/Landmark.hpp
    ${CMAKE_CURRENT_SOURCE_DIR}/eos/include/eos/core/LandmarkMapper.hpp
    ${CMAKE_CURRENT_SOURCE_DIR}/eos/include/eos/core/read_pts_landmarks.hpp
    ${CMAKE_CURRENT_SOURCE_DIR}/eos/include/eos/core/Image.hpp
    ${CMAKE_CURRENT_SOURCE_DIR}/eos/include/eos/core/image/Pixel.hpp
    ${CMAKE_CURRENT_SOURCE_DIR}/eos/include/eos/core/image/PixelTraits.hpp
    ${CMAKE_CURRENT_SOURCE_DIR}/eos/include/eos/core/image/utils.hpp
    ${CMAKE_CURRENT_SOURCE_DIR}/eos/include/eos/core/image/resize.hpp
    ${CMAKE_CURRENT_SOURCE_DIR}/eos/include/eos/core/image/opencv_interop.hpp
    ${CMAKE_CURRENT_SOURCE_DIR}/eos/include/eos/core/Mesh.hpp
    ${CMAKE_CURRENT_SOURCE_DIR}/eos/include/eos/core/read_obj.hpp
    ${CMAKE_CURRENT_SOURCE_DIR}/eos/include/eos/core/write_obj.hpp
    ${CMAKE_CURRENT_SOURCE_DIR}/eos/include/eos/core/Rect.hpp
    ${CMAKE_CURRENT_SOURCE_DIR}/eos/include/eos/morphablemodel/PcaModel.hpp
    ${CMAKE_CURRENT_SOURCE_DIR}/eos/include/eos/morphablemodel/MorphableModel.hpp
    ${CMAKE_CURRENT_SOURCE_DIR}/eos/include/eos/morphablemodel/Blendshape.hpp
    ${CMAKE_CURRENT_SOURCE_DIR}/eos/include/eos/morphablemodel/ExpressionModel.hpp
    ${CMAKE_CURRENT_SOURCE_DIR}/eos/include/eos/morphablemodel/coefficients.hpp
    ${CMAKE_CURRENT_SOURCE_DIR}/eos/include/eos/morphablemodel/EdgeTopology.hpp
    ${CMAKE_CURRENT_SOURCE_DIR}/eos/include/eos/morphablemodel/io/cvssp.hpp
    ${CMAKE_CURRENT_SOURCE_DIR}/eos/include/eos/morphablemodel/io/eigen_cerealisation.hpp
    ${CMAKE_CURRENT_SOURCE_DIR}/eos/include/eos/pca/pca.hpp
    ${CMAKE_CURRENT_SOURCE_DIR}/eos/include/eos/fitting/orthographic_camera_estimation_linear.hpp
    ${CMAKE_CURRENT_SOURCE_DIR}/eos/include/eos/fitting/nonlinear_camera_estimation.hpp
    ${CMAKE_CURRENT_SOURCE_DIR}/eos/include/eos/fitting/detail/nonlinear_camera_estimation_detail.hpp
    ${CMAKE_CURRENT_SOURCE_DIR}/eos/include/eos/fitting/detail/glm_cerealisation.hpp
    ${CMAKE_CURRENT_SOURCE_DIR}/eos/include/eos/fitting/linear_shape_fitting.hpp
    ${CMAKE_CURRENT_SOURCE_DIR}/eos/include/eos/fitting/contour_correspondence.hpp
    ${CMAKE_CURRENT_SOURCE_DIR}/eos/include/eos/fitting/blendshape_fitting.hpp
    ${CMAKE_CURRENT_SOURCE_DIR}/eos/include/eos/fitting/closest_edge_fitting.hpp
    ${CMAKE_CURRENT_SOURCE_DIR}/eos/include/eos/fitting/fitting.hpp
    ${CMAKE_CURRENT_SOURCE_DIR}/eos/include/eos/fitting/multi_image_fitting.hpp
    ${CMAKE_CURRENT_SOURCE_DIR}/eos/include/eos/fitting/ceres_nonlinear.hpp
    ${CMAKE_CURRENT_SOURCE_DIR}/eos/include/eos/fitting/RenderingParameters.hpp
    ${CMAKE_CURRENT_SOURCE_DIR}/eos/include/eos/fitting/FittingResult.hpp
    ${CMAKE_CURRENT_SOURCE_DIR}/eos/include/eos/render/normals.hpp
    ${CMAKE_CURRENT_SOURCE_DIR}/eos/include/eos/render/transforms.hpp
    ${CMAKE_CURRENT_SOURCE_DIR}/eos/include/eos/render/ray_triangle_intersect.hpp
    ${CMAKE_CURRENT_SOURCE_DIR}/eos/include/eos/render/draw_utils.hpp
    ${CMAKE_CURRENT_SOURCE_DIR}/eos/include/eos/render/opencv/draw_utils.hpp
    ${CMAKE_CURRENT_SOURCE_DIR}/eos/include/eos/render/render.hpp
    ${CMAKE_CURRENT_SOURCE_DIR}/eos/include/eos/render/detail/TriangleToRasterize.hpp
    ${CMAKE_CURRENT_SOURCE_DIR}/eos/include/eos/render/detail/plane.hpp
    ${CMAKE_CURRENT_SOURCE_DIR}/eos/include/eos/render/detail/utils.hpp
    ${CMAKE_CURRENT_SOURCE_DIR}/eos/include/eos/render/ProjectionType.hpp
    ${CMAKE_CURRENT_SOURCE_DIR}/eos/include/eos/render/detail/RayDirection.hpp
    ${CMAKE_CURRENT_SOURCE_DIR}/eos/include/eos/render/vertex_visibility.hpp
    ${CMAKE_CURRENT_SOURCE_DIR}/eos/include/eos/render/texture_extraction.hpp
    ${CMAKE_CURRENT_SOURCE_DIR}/eos/include/eos/render/detail/texturing.hpp
    ${CMAKE_CURRENT_SOURCE_DIR}/eos/include/eos/render/SoftwareRenderer.hpp
    ${CMAKE_CURRENT_SOURCE_DIR}/eos/include/eos/render/VertexShader.hpp
    ${CMAKE_CURRENT_SOURCE_DIR}/eos/include/eos/render/Rasterizer.hpp
    ${CMAKE_CURRENT_SOURCE_DIR}/eos/include/eos/render/FragmentShader.hpp
    ${CMAKE_CURRENT_SOURCE_DIR}/eos/include/eos/render/Texture.hpp
    ${CMAKE_CURRENT_SOURCE_DIR}/eos/include/eos/render/detail/Vertex.hpp
    ${CMAKE_CURRENT_SOURCE_DIR}/eos/include/eos/video/Keyframe.hpp
    ${CMAKE_CURRENT_SOURCE_DIR}/eos/include/eos/video/keyframe_merging.hpp
    ${CMAKE_CURRENT_SOURCE_DIR}/eos/include/eos/cpp17/optional.hpp
    ${CMAKE_CURRENT_SOURCE_DIR}/eos/include/eos/cpp17/optional_serialization.hpp
    ${CMAKE_CURRENT_SOURCE_DIR}/eos/include/eos/cpp17/detail/akrzemi1_optional.hpp
    ${CMAKE_CURRENT_SOURCE_DIR}/eos/include/eos/cpp17/detail/akrzemi1_optional_serialization.hpp
    ${CMAKE_CURRENT_SOURCE_DIR}/eos/include/eos/cpp17/variant.hpp
    ${CMAKE_CURRENT_SOURCE_DIR}/eos/include/eos/cpp17/variant_serialization.hpp
    ${CMAKE_CURRENT_SOURCE_DIR}/eos/include/eos/cpp17/detail/mpark_variant.hpp
    ${CMAKE_CURRENT_SOURCE_DIR}/eos/include/eos/cpp17/detail/mpark_variant_serialization.hpp
    ${CMAKE_CURRENT_SOURCE_DIR}/eos/include/eos/cpp17/clamp.hpp)

set(EIGEN3_INCLUDE_DIR eos/3rdparty/eigen/)
set(CEREAL_INCLUDE_DIR eos/3rdparty/cereal/include)
set(glm_INCLUDE_DIR eos/3rdparty/glm)
set(nanoflann_INCLUDE_DIR eos/3rdparty/nanoflann/include)
set(eigen3_nnls_INCLUDE_DIR eos/3rdparty/eigen3-nnls/src)
set(toml11_INCLUDE_DIR eos/3rdparty/toml11)

add_library(eosUnityPlugin SHARED library.cpp ${HEADERS})
target_include_directories(eosUnityPlugin PUBLIC
    ${EIGEN3_INCLUDE_DIR}
    ${CEREAL_INCLUDE_DIR}
    ${glm_INCLUDE_DIR}
    ${nanoflann_INCLUDE_DIR}
    ${eigen3_nnls_INCLUDE_DIR}
    ${toml11_INCLUDE_DIR}
)

#add_executable(eosUnityTest test.cpp ${HEADERS})
#target_include_directories(eosUnityTest PUBLIC
#        ${EIGEN3_INCLUDE_DIR}
#        ${CEREAL_INCLUDE_DIR}
#        ${glm_INCLUDE_DIR}
#        ${nanoflann_INCLUDE_DIR}
#        ${eigen3_nnls_INCLUDE_DIR}
#        ${toml11_INCLUDE_DIR}
#)
