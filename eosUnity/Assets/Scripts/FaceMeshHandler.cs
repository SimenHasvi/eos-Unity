using System;
using System.IO;
using UnityEngine;

// class to manage the face mesh and texture
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class FaceMeshHandler : MonoBehaviour
{
    private Mesh _mesh;
    private Transform _transform;
    
    void Start()
    {
        eos.InitializeCustom(
            Path.Combine(Application.streamingAssetsPath, "share/sfm_shape_3448.bin"),
            Path.Combine(Application.streamingAssetsPath, "share/ibug_to_sfm.txt"),
            Path.Combine(Application.streamingAssetsPath, "share/custom_expression_blendshapes_3448.txt"),
            Path.Combine(Application.streamingAssetsPath, "share/sfm_model_contours.json"),
            Path.Combine(Application.streamingAssetsPath, "share/sfm_3448_edge_topology.json"));
        
        /*
        eos.Initialize(
            Path.Combine(Application.streamingAssetsPath, "share/sfm_shape_3448.bin"),
            Path.Combine(Application.streamingAssetsPath, "share/ibug_to_sfm.txt"),
            Path.Combine(Application.streamingAssetsPath, "share/expression_blendshapes_3448.bin"),
            Path.Combine(Application.streamingAssetsPath, "share/sfm_model_contours.json"),
            Path.Combine(Application.streamingAssetsPath, "share/sfm_3448_edge_topology.json"));
        */

        _mesh = GetComponent<MeshFilter>().mesh;
        _transform = transform;
        
        _mesh.vertices = eos.MeanShape;
        _mesh.triangles = eos.Triangles;
        _mesh.RecalculateNormals();
        _mesh.uv = eos.Texcoords;
        
        LoadTextureFromFile();

        eos.OnAfterFitting += AfterFitting;
        eos.OnAfterTexture += AfterTexture;
    }

    private void LateUpdate()
    {
        //eos.AddExpression(eos.GetBlendshapeCoeffs(new []{("mouth_open", AudioInputHandler.MicMean)}));
    }

    // load a texture from file
    private void LoadTextureFromFile()
    {
        var fileData = File.ReadAllBytes(Path.Combine(Application.streamingAssetsPath, "facetexture.png"));
        var tex = new Texture2D(2, 2);
        tex.LoadImage(fileData);
        GetComponent<Renderer>().material.mainTexture = tex;
    }

    // after the eos library has done its fitting we apply the new mesh
    private void AfterFitting()
    {
        _mesh.vertices = eos.Vertices;
        _transform.rotation = eos.Rotation;
    }
    
    // after the eos library is done fitting texture we apply it to this object
    private void AfterTexture()
    {
        GetComponent<Renderer>().material.mainTexture = eos.Texture;
        File.WriteAllBytes(Path.Combine(Application.streamingAssetsPath, "facetexture.png"), eos.Texture.EncodeToPNG());
    }

}
