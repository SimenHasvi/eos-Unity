using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using DlibFaceLandmarkDetector;
using UnityEngine;
using Debug = UnityEngine.Debug;

// manages the connection with the web camera device
public static class WebCamInterface
{
    
    public static readonly List<string> CamDevices = new List<string>();
    public static Texture2D DisplayTexture;
    public static readonly List<(int, int)> AvailableResolutions = new List<(int, int)>();
    public static bool started = false;
    public static bool initialized = false;
    public static bool paused = false;
    
    public delegate void AfterStartedDelegate();

    private static WebCamTexture camTexture;
    private static Color32[] colors;
    private static bool _landmarksUpdated = false;
    private static int _maxWidth = 1280;
    private static int _maxHeight = 720;
    private static readonly string _dlibShapePredictorFileName = "sp_human_face_68.dat";
    private static FaceLandmarkDetector _faceLandmarkDetector;

    private static List<Rect> faces;
    private static int counter = 3;
    
    private static double landmarksSum = 0;
    
    
    public static void Initialize()
    {
        if (initialized) return;
        _faceLandmarkDetector = new FaceLandmarkDetector(Path.Combine(Application.streamingAssetsPath, _dlibShapePredictorFileName));
        foreach (var device in WebCamTexture.devices) CamDevices.Add(device.name);
        FindAvailableResolutions();
        initialized = true;
    }

    public static IEnumerator StartCamera(string camDevice, int resolution, AfterStartedDelegate onStarted)
    {
        while (!initialized) yield return null;
        if (started) StopCamera();
        //camTexture = new WebCamTexture(camDevice, _maxWidth, _maxHeight);
        camTexture = new WebCamTexture(camDevice, AvailableResolutions[resolution].Item1, AvailableResolutions[resolution].Item2);
        camTexture.Play();
        
        while (camTexture.width <= 16) yield return null;

        DisplayTexture = new Texture2D(camTexture.width, camTexture.height, TextureFormat.RGBA32, false);
        colors = new Color32[camTexture.width * camTexture.height];
        started = true;
        onStarted.Invoke();
        Debug.Log("Started camera: " + camTexture.deviceName + " " + camTexture.width + "x" + camTexture.height);
    }
    
    // Detect landmarks from the last captured frame
    public static bool DetectLandmarks(ref double[] landmarks, bool drawResult, float differenceThreshold, float sensitivity = 0)
    {
        if (!started) return false;

        camTexture.GetPixels32(colors);

        _faceLandmarkDetector.SetImage<Color32>(colors, camTexture.width, camTexture.height, 4, true);

        if (counter == 3)
        {
            faces = _faceLandmarkDetector.Detect(sensitivity);
            counter = 0;
        }
        else counter++;

        if (faces.Count <= 0)
        {
            DisplayTexture.SetPixels32(colors);
            DisplayTexture.Apply();
            return false;
        }
        
        landmarks = _faceLandmarkDetector.DetectLandmarkArray(faces[0]);
        
        var sum = landmarks.Sum();
        if (Math.Abs(sum - landmarksSum) < differenceThreshold) return false;
        landmarksSum = sum;

        if (drawResult)
        {
            _faceLandmarkDetector.DrawDetectLandmarkResult<Color32>(colors, camTexture.width, camTexture.height, 4, true, 0, 255, 0, 255);
            _faceLandmarkDetector.DrawDetectResult<Color32>(colors, camTexture.width, camTexture.height, 4, true, 255, 0, 0, 255, 2);
        }
        DisplayTexture.SetPixels32(colors);
        DisplayTexture.Apply();
        
        return true;
    }

    public static bool DetectLandmarksFromFile(Texture2D image, ref double[] landmarks, float sensitivity = 0)
    {
        _faceLandmarkDetector.SetImage<Color32>(image.GetPixels32(), image.width, image.height, 4, true);
        faces = _faceLandmarkDetector.Detect(sensitivity);
        if (faces.Count <= 0) return false;
        landmarks = _faceLandmarkDetector.DetectLandmarkArray(faces[0]);
        return true;
    }

    public static void StopCamera()
    {
        if (!started) return;
        camTexture.Stop();
        started = false;
    }

    private static void FindAvailableResolutions()
    {
        AvailableResolutions.Add((_maxWidth, _maxHeight));
        for (var i = 2; i < 10; i++)
        {
            if (i % 2 != 0 || _maxWidth % i != 0 || _maxHeight % i != 0) continue;
            AvailableResolutions.Add((_maxWidth/i, _maxHeight/i));
        }
    }

    public static int Width()
    {
        return camTexture.width;
    }
    
    public static int Height()
    {
        return camTexture.height;
    }
}