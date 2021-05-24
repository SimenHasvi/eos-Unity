using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

// this class connects to the camera and send the landmarks over to eos for computation
[RequireComponent(typeof(WebCamInterface))]
public class camScript : MonoBehaviour
{
    public Text infoText;

    private static double[] _landmarks = new double[0];

    // start the camera
    private void Start()
    {
        WebCamInterface.Initialize();
        StartCoroutine(WebCamInterface.StartCamera(WebCamInterface.CamDevices[0], 1, () =>
        {
            gameObject.GetComponent<RawImage>().texture = WebCamInterface.DisplayTexture;
            gameObject.GetComponent<RawImage>().color = Color.white;
        }));
    }

    // Get a new frame from the camera
    private void Update()
    {
        if (!WebCamInterface.started) return;
        // if space is clicked we do texture fitting
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (!WebCamInterface.DetectLandmarks(ref _landmarks, false, 0, -0.5f)) return;
            StartCoroutine(FitShapeAndTexture(FaceTextureProcessor.CopyTexture(WebCamInterface.DisplayTexture), _landmarks));
        }
        /*
        else if (Input.GetKeyDown(KeyCode.P))
        {
            var image = new Texture2D(0, 0);
            image.LoadImage(File.ReadAllBytes("image_goes_here.jpg"));
            if (WebCamInterface.DetectLandmarksFromFile(image, ref _landmarks))
            {
                StartCoroutine(FitShapeAndTexture(image, _landmarks));
            }
            else
            {
                Debug.LogWarning("No face detected in the image");
            }
        }
        */
        else
        {
            if (!WebCamInterface.DetectLandmarks(ref _landmarks, true, 40, -0.5f)) return;
            eos.FitExpressionAndRotation(WebCamInterface.Width(), WebCamInterface.Height(), _landmarks, BlendshapeSettings.modifiers);
        }
    }

    // calls the fit-shape and texture in eos
    // WILL HOLD UP THE PROGRAM WHILE RUNNING
    private IEnumerator FitShapeAndTexture(Texture2D image, double[] landmarks)
    {
        infoText.text = "Fitting shape and texture...";
        yield return null;
        eos.FitShapeAndTexture(image, _landmarks);
        infoText.text = "Press [SPACE] to capture new shape and texture.";
    }

    private void OnDestroy()
    {
        WebCamInterface.StopCamera();
    }
}