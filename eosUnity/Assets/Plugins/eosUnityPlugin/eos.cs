using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using UnityEngine;
using Debug = UnityEngine.Debug;

// This is the interface which talks to the eos native code
public static class eos
{
    public static Vector3[] MeanShape = new Vector3[3448];
    public static Vector2[] Texcoords = new Vector2[3448];
    public static int[] Triangles = new int[20208];
    public static float[] ShapeSample = new float[3448*3];
    
    public static Vector3[] Vertices = new Vector3[3448];
    public static Quaternion Rotation = Quaternion.identity;
    public static Texture2D Texture;
    
    public delegate void AfterFittingDelegate();
    public delegate void AfterExpressionDelegate(float[] blendshapes);
    public static event AfterFittingDelegate OnAfterTexture;
    public static event AfterFittingDelegate OnAfterFitting;
    public static event AfterExpressionDelegate OnAfterExpression;

    private static float[] _verticesOut = new float[3448*3];
    private static float[] _rotationOut = new float[4];
    private static float[] _blendshapesOut = new float[0];
    private static int blendshapesCount;
    private static string[] blendshapes;

    // initialize the library, the mean shape and triangles are available after this
    public static void Initialize(
        string modelfile,
        string mappingsfile,
        string blendshapesfile,
        string contourfile,
        string edgetopologyfile)
    {
        var texcoordsOut = new float[3448*2];
        blendshapesCount = initialize(modelfile, mappingsfile, blendshapesfile, contourfile, edgetopologyfile, ShapeSample, Triangles, texcoordsOut);
        _blendshapesOut = new float[blendshapesCount];
        for (var i = 0; i < MeanShape.Length; i++)
        {
            MeanShape[i].x = ShapeSample[i * 3];
            MeanShape[i].y = ShapeSample[i * 3 + 1];
            MeanShape[i].z = ShapeSample[i * 3 + 2];
        }
        for (var i = 0; i < Texcoords.Length; i++)
        {
            Texcoords[i].x = texcoordsOut[i * 2];
            Texcoords[i].y = texcoordsOut[i * 2 + 1];
        }
    }
    
    // initialize the library, the meanshape and triangles are available after this
    public static void InitializeCustom(
        string modelfile,
        string mappingsfile,
        string blendshapesfile,
        string contourfile,
        string edgetopologyfile)
    {
        
        var texcoordsOut = new float[3448*2];
        blendshapes = ReadBlendshapesFromFile(blendshapesfile);
        blendshapesCount = initialize_custom(modelfile, mappingsfile, blendshapesfile, contourfile, edgetopologyfile, ShapeSample, Triangles, texcoordsOut);
        _blendshapesOut = new float[blendshapesCount];
        for (var i = 0; i < MeanShape.Length; i++)
        {
            MeanShape[i].x = ShapeSample[i * 3];
            MeanShape[i].y = ShapeSample[i * 3 + 1];
            MeanShape[i].z = ShapeSample[i * 3 + 2];
        }
        for (var i = 0; i < Texcoords.Length; i++)
        {
            Texcoords[i].x = texcoordsOut[i * 2];
            Texcoords[i].y = texcoordsOut[i * 2 + 1];
        }
    }

    // fit the shape and texture, this is available in Vertices, Rotation, and Texture
    public static void FitShapeAndTexture(Texture2D image, double[] landmarks)
    {
        var width = image.width;
        var height = image.height;
        var imgRes = new[] {width, height};
        var imageOut = width * height * 3 > 512 * 512 * 3 ? new int[width * height * 3] : new int[512 * 512 * 3];

        for (var r = 0; r < height; r++)
        {
            for (var c = 0; c < width; c++)
            {
                imageOut[(r * width + c) * 3] = (int) (255 * image.GetPixel(c, r).r);
                imageOut[(r * width + c) * 3 + 1] = (int) (255 * image.GetPixel(c, r).g);
                imageOut[(r * width + c) * 3 + 2] = (int) (255 * image.GetPixel(c, r).b);
            }
        }
        fit_shape_texture(imgRes, landmarks, ShapeSample, imageOut);

        for (var i = 0; i < Vertices.Length; i++)
        {
            Vertices[i].x = ShapeSample[i * 3];
            Vertices[i].y = ShapeSample[i * 3 + 1];
            Vertices[i].z = ShapeSample[i * 3 + 2];
        }

        width = imgRes[0];
        height = imgRes[1];

        Texture = new Texture2D(width, height, TextureFormat.RGB24, false);
        for (var r = 0; r < height; r++) 
        {
            for (var c = 0; c < width; c++)
            {
                var color = new Color32(
                    (byte)imageOut[(r * width + c) * 3],
                    (byte)imageOut[(r * width + c) * 3 + 1],
                    (byte)imageOut[(r * width + c) * 3 + 2],
                    255);
                Texture.SetPixel(c, r, color);
            }
        }
        Texture.Apply();

        Texture = FaceTextureProcessor.CropFace(Texture);
        Texture = FaceTextureProcessor.SmoothFillHolesFromCenter(Texture, 3, Color.black, Color.white);
        var scleraColorAddend = FaceTextureProcessor.GetScleraBrightnessAddend(Texture);
        Texture = FaceTextureProcessor.AdjustBrightness(Texture, scleraColorAddend);
        Texture = FaceTextureProcessor.SetContrast(Texture, (scleraColorAddend.r + scleraColorAddend.g + scleraColorAddend.b)/3 * 100);

        OnAfterTexture?.Invoke();
        OnAfterFitting?.Invoke();
    }

    /* DOESNT WORK
    public static void FitShapeRotationExpresison(int width, int height, double[] landmarks)
    {
        fit_shape_expression_rotation(width, height, landmarks, _rotationOut, _verticesOut);
        for (var i = 0; i < Vertices.Length; i++)
        {
            Vertices[i].x = _verticesOut[i * 3];
            Vertices[i].y = _verticesOut[i * 3 + 1];
            Vertices[i].z = _verticesOut[i * 3 + 2];
        }
        Rotation.x = _rotationOut[0];
        Rotation.y = _rotationOut[1];
        Rotation.z = _rotationOut[2];
        Rotation.w = _rotationOut[3];
        OnAfterFitting?.Invoke();
    }
    */
    
    // Fit the shape to the landmarks, available in Vertices afterwards
    public static void FitShape(int width, int height, double[] landmarks)
    {
        fit_shape(width, height, landmarks, ShapeSample);
        for (var i = 0; i < Vertices.Length; i++)
        {
            Vertices[i].x = ShapeSample[i * 3];
            Vertices[i].y = ShapeSample[i * 3 + 1];
            Vertices[i].z = ShapeSample[i * 3 + 2];
        }
        OnAfterFitting?.Invoke();
    }

    // Apply the expression described by the given blendshape weights. Available in Vertices afterwards.
    public static void ApplyExpression(float[] expressionCoeffs)
    {
        apply_expression(expressionCoeffs, _verticesOut);
        for (var i = 0; i < Vertices.Length; i++)
        {
            Vertices[i].x = _verticesOut[i * 3];
            Vertices[i].y = _verticesOut[i * 3 + 1];
            Vertices[i].z = _verticesOut[i * 3 + 2];
        }
        OnAfterFitting?.Invoke();
        OnAfterExpression?.Invoke(expressionCoeffs);
    }
    
    // Apply the expression to the given shape, described by the given blendshape weights. Available in Vertices afterwards.
    public static void ApplyExpressionToSample(float[] expressionCoeffs, float[] shapeSample)
    {
        Debug.Assert(expressionCoeffs.Length >= blendshapesCount);
        apply_expression_to_sample(expressionCoeffs, shapeSample);
        for (var i = 0; i < Vertices.Length; i++)
        {
            Vertices[i].x = shapeSample[i * 3];
            Vertices[i].y = shapeSample[i * 3 + 1];
            Vertices[i].z = shapeSample[i * 3 + 2];
        }
        OnAfterFitting?.Invoke();
    }
    
    // Add an expression on top of the existing one, available in Vertices
    public static void AddExpression(float[] expressionCoeffs)
    {
        for (var i = 0; i < blendshapesCount; i++)
        {
            expressionCoeffs[i] += _blendshapesOut[i];
        }
        apply_expression(expressionCoeffs, _verticesOut);
        for (var i = 0; i < Vertices.Length; i++)
        {
            Vertices[i].x = _verticesOut[i * 3];
            Vertices[i].y = _verticesOut[i * 3 + 1];
            Vertices[i].z = _verticesOut[i * 3 + 2];
        }
        OnAfterFitting?.Invoke();
        OnAfterExpression?.Invoke(expressionCoeffs);
    }
    
    // Add an expression to the give shape sample on top of the existing one, available in Vertices
    public static void AddExpressionToSample(float[] expressionCoeffs, float[] shapeSample)
    {
        Debug.Assert(expressionCoeffs.Length >= blendshapesCount);
        for (var i = 0; i < blendshapesCount; i++)
        {
            expressionCoeffs[i] += _blendshapesOut[i];
        }
        apply_expression_to_sample(expressionCoeffs, shapeSample);
        for (var i = 0; i < Vertices.Length; i++)
        {
            Vertices[i].x = shapeSample[i * 3];
            Vertices[i].y = shapeSample[i * 3 + 1];
            Vertices[i].z = shapeSample[i * 3 + 2];
        }
        OnAfterFitting?.Invoke();
    }

    // Get the expression and rotation from the landmarks, modifiers will modify the expression
    // Results are available in Vertices and Rotation
    public static void FitExpressionAndRotation(int width, int height, double[] landmarks, float[] modifiers)
    {
        fit_expression_rotation(width, height, landmarks, _rotationOut, _blendshapesOut, _verticesOut, modifiers);
        for (var i = 0; i < Vertices.Length; i++)
        {
            Vertices[i].x = _verticesOut[i * 3];
            Vertices[i].y = _verticesOut[i * 3 + 1];
            Vertices[i].z = _verticesOut[i * 3 + 2];
        }
        Rotation.x = _rotationOut[0];
        Rotation.y = _rotationOut[1];
        Rotation.z = _rotationOut[2];
        Rotation.w = _rotationOut[3];
        OnAfterFitting?.Invoke();
        OnAfterExpression?.Invoke(_blendshapesOut);
    }

    // Same as FitExpressionAndRotation but it is applied to a given shape sample
    public static void FitExpressionAndRotationToSample(int width, int height, double[] landmarks, float[] shapeSample, float[] modifiers)
    {
        fit_expression_rotation_to_sample(width, height, landmarks, _rotationOut, _blendshapesOut, shapeSample, modifiers);
        for (var i = 0; i < Vertices.Length; i++)
        {
            Vertices[i].x = shapeSample[i * 3];
            Vertices[i].y = shapeSample[i * 3 + 1];
            Vertices[i].z = shapeSample[i * 3 + 2];
        }
        Rotation.x = _rotationOut[0];
        Rotation.y = _rotationOut[1];
        Rotation.z = _rotationOut[2];
        Rotation.w = _rotationOut[3];
        OnAfterFitting?.Invoke();
    }

    // Create a list of blendshape coefficients using the blendshape names
    public static float[] GetBlendshapeCoeffs((string, float)[] coeffs)
    {
        var blendshapeCoeffs = new float[blendshapesCount];
        for (var i = 0; i < blendshapesCount; i++)
        {
            blendshapeCoeffs[i] = 0;
            foreach (var (name, value) in coeffs)
            {
                if (name != blendshapes[i]) continue;
                blendshapeCoeffs[i] = value;
                break;
            }
        }

        return blendshapeCoeffs;
    }
    
    // Read the blendshapes from a file
    public static string[] ReadBlendshapesFromFile(string filePath)
    {
        var names = new List<string>();
        using (var file = new StreamReader(filePath))
        {
            string line;
            while ((line = file.ReadLine()) != null)
            {
                var lineList = new List<string>(line.Split(' '));
                names.Add(lineList[0]);
            }
        }

        return names.ToArray();
    }

    [DllImport("eosUnityPlugin")]
    private static extern int initialize(
        string modelfile,
        string mappingsfile,
        string blendshapesfile,
        string contourfile,
        string edgetopologyfile,
        float[] meanVerticesOut,
        int[] trianglesOut,
        float[] texcoordsOut);
    
    [DllImport("eosUnityPlugin")]
    private static extern int initialize_custom(
        string modelfile,
        string mappingsfile,
        string blendshapesfile,
        string contourfile,
        string edgetopologyfile,
        float[] meanVerticesOut,
        int[] trianglesOut,
        float[] texcoordsOut);

    [DllImport("eosUnityPlugin")]
    private static extern void apply_expression(float[] expresssionCoeffs, float[] verticesOut);
    
    [DllImport("eosUnityPlugin")]
    private static extern void apply_expression_to_sample(float[] expresssionCoeffs, float[] shape_sample);

    [DllImport("eosUnityPlugin")]
    private static extern void fit_shape(int width, int height, double[] landmarks, float[] verticesOut);
    
    [DllImport("eosUnityPlugin")]
    private static extern void fit_shape_texture(int[] imgRes, double[] landmarks, float[] verticesOut, int[] image);
    
    [DllImport("eosUnityPlugin")]
    private static extern void fit_shape_expression_rotation(int width, int height, double[] landmarks, float[] rotationOut, float[] verticesOut);
    
    [DllImport("eosUnityPlugin")]
    private static extern void fit_expression_rotation(int width, int height, double[] landmarks, float[] rotationOut, float[] blendshapesOut, float[] verticesOut, float[] modifiers);
    
    // shape_sample is the same as verticesOut after shape fitting (not after expression fitting)
    [DllImport("eosUnityPlugin")]
    private static extern void fit_expression_rotation_to_sample(int width, int height, double[] landmarks, float[] rotationOut, float[] blendshapesOut, float[] shape_sample, float[] modifiers);
}

// this class allows for different face instances at the same time
public class Face
{
    public Vector3[] Vertices = new Vector3[3448];
    public Quaternion Rotation = Quaternion.identity;
    public Texture2D Texture;
    
    public static event eos.AfterFittingDelegate OnAfterTexture;
    public static event eos.AfterFittingDelegate OnAfterFitting;
    
    private float[] _shapeSample = new float[3448*3];

    public Face()
    {
        Vertices = eos.MeanShape;
        _shapeSample = eos.ShapeSample;
    }

    public void FitShapeAndTexture(Texture2D image, double[] landmarks)
    {
        eos.FitShapeAndTexture(image, landmarks);
        Texture = eos.Texture;
        Vertices = eos.Vertices;
        _shapeSample = eos.ShapeSample;
        OnAfterFitting?.Invoke();
        OnAfterTexture?.Invoke();
    }

    public void FitShape(int width, int height, double[] landmarks)
    {
        eos.FitShape(width, height, landmarks);
        Vertices = eos.Vertices;
        _shapeSample = eos.ShapeSample;
        OnAfterFitting?.Invoke();
    }

    public void FitExpressionAndRotation(int width, int height, double[] landmarks, float[] modifiers)
    {
        eos.FitExpressionAndRotationToSample(width, height, landmarks, _shapeSample, modifiers);
        Vertices = eos.Vertices;
        Rotation = eos.Rotation;
        OnAfterFitting?.Invoke();
    }

    public void AddExpression(float[] expressionCoeffs)
    {
        eos.AddExpressionToSample(expressionCoeffs, _shapeSample);
        Vertices = eos.Vertices;
        OnAfterFitting?.Invoke();
    }
}