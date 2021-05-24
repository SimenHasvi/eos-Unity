using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

// Helper functions to modify the textures
public static class FaceTextureProcessor
{

    // mask files to locate important regions in the texture
    private static readonly string EyeMask = Path.Combine(Application.streamingAssetsPath, "eyemask.png");
    private static readonly string FaceMask = Path.Combine(Application.streamingAssetsPath, "facemask.png");

    // Get the addend color which would make the sclera grey
    public static Color GetScleraBrightnessAddend(Texture2D tex)
    {
        return new Color(0.5f, 0.5f, 0.5f) - GetScleraReferenceColor(tex);
    }

    // adjust the brightness by adding a given color to all the pixels
    public static Texture2D AdjustBrightness(Texture2D tex, Color addend)
    {
        for (var x = 0; x < tex.width; x++)
        {
            for (var y = 0; y < tex.height; y++)
            {
                tex.SetPixel(x, y, tex.GetPixel(x, y) + addend);
            }
        }
        tex.Apply();
        return tex;
    }
    
    // set the contrast of the image, threshold sets the intensity
    public static Texture2D SetContrast(Texture2D tex, float threshold)
    {
        var contrast = Math.Pow((100.0 + threshold) / 100.0, 2);
 
        for (var y = 0; y < tex.height; y++)
        {
            for (var x = 0; x < tex.width; x++) 
            {
                Color32 oldColor = tex.GetPixel(x, y);
                var red = ((((oldColor.r / 255.0) - 0.5) * contrast) + 0.5) * 255.0;
                var green = ((((oldColor.g / 255.0) - 0.5) * contrast) + 0.5) * 255.0;
                var blue = ((((oldColor.b / 255.0) - 0.5) * contrast) + 0.5) * 255.0;
                if (red > 255) red = 255;
                if (red < 0) red = 0;
                if (green > 255) green = 255;
                if (green < 0) green = 0;
                if (blue > 255) blue = 255;
                if (blue < 0) blue = 0;
 
                var newColor = new Color32((byte)(int)red, (byte)(int)green, (byte)(int)blue, 255);
                tex.SetPixel(x, y, newColor);
            }
        }
        tex.Apply();
        return tex;
    }

    // basic smoothing
    public static Texture2D Smoothen(Texture2D tex, int strength)
    {
        var newTex = new Texture2D(tex.width, tex.height);
        for (var x = 0; x < tex.width; x++)
        {
            for (var y = 0; y < tex.height; y++)
            {
                newTex.SetPixel(x, y, AverageColor(GetSurroundingColors(tex, x, y, strength)));
            }
        }

        newTex.Apply();
        return newTex;
    }

    // smoothing that will also fill holes
    public static Texture2D SmoothFillHoles(Texture2D tex, int radius, params Color[] bgColors)
    {
        for (var x = 0; x < tex.width; x++)
        {
            for (var y = 0; y < tex.height; y++)
            {
                tex.SetPixel(x, y, AverageColor(GetSurroundingColors(tex, x, y, radius, bgColors)));
            }
        }

        tex.Apply();
        return tex;
    }

    // Fast version of the fill holes algorithm
    public static Texture2D FillHolesFast(Texture2D tex, params Color[] bgColors)
    {
        var c = Color.black;
        for (var x = 0; x < tex.width; x++)
        {
            for (var y = 0; y < tex.height; y++)
            {
                if (bgColors.Contains(tex.GetPixel(x, y))) tex.SetPixel(x, y, c);
                c = tex.GetPixel(x, y);
            }
        }

        tex.Apply();
        return tex;
    }

    // crop the face with the face mask
    public static Texture2D CropFace(Texture2D tex)
    {
        var faceReference = new Texture2D(1, 1);
        faceReference.LoadImage(File.ReadAllBytes(FaceMask));
        for (var x = 0; x < tex.width; x++)
        {
            for (var y = 0; y < tex.height; y++)
            {
                if (faceReference.GetPixel(x, y) == Color.black)
                {
                    tex.SetPixel(x, y, Color.black);
                }
            }
        }
        tex.Apply();
        return tex;
    }

    // Fill smooth and fill holes, from the center and outwards
    public static Texture2D SmoothFillHolesFromCenter(Texture2D tex, int radius, params Color[] bgColors)
    {
        var centerX = tex.width / 2;
        var centerY = tex.height / 2;

        for (var layer = 0; layer <= centerX; layer++)
        {
            for (var x = centerX+1; x <= centerX+layer; x++)
            {
                tex.SetPixel(x, centerY+layer, AverageColor(GetSurroundingColors(tex, x, centerY+layer, radius, bgColors)));
                tex.SetPixel(x, centerY-layer, AverageColor(GetSurroundingColors(tex, x, centerY-layer, radius, bgColors)));
            }
            for (var x = centerX; x >= centerX-layer; x--)
            {
                tex.SetPixel(x, centerY+layer, AverageColor(GetSurroundingColors(tex, x, centerY+layer, radius, bgColors)));
                tex.SetPixel(x, centerY-layer, AverageColor(GetSurroundingColors(tex, x, centerY-layer, radius, bgColors)));
            }
            for (var y = centerY+1; y <= centerY+layer; y++)
            {
                tex.SetPixel(centerX+layer, y, AverageColor(GetSurroundingColors(tex, centerX+layer, y, radius, bgColors)));
                tex.SetPixel(centerX-layer, y, AverageColor(GetSurroundingColors(tex, centerX-layer, y, radius, bgColors)));
            }
            for (var y = centerY; y >= centerY-layer; y--)
            {
                tex.SetPixel(centerX+layer, y, AverageColor(GetSurroundingColors(tex, centerX+layer, y, radius, bgColors)));
                tex.SetPixel(centerX-layer, y, AverageColor(GetSurroundingColors(tex, centerX-layer, y, radius, bgColors)));
            }
        }
        tex.Apply();
        return tex;
    }

    // the fastest hole filling
    public static Texture2D FillHolesFaster(Texture2D tex, int radius, params Color[] bgColors)
    {
        for (var x = 0; x < tex.width; x++)
        {
            for (var y = 0; y < tex.height; y++)
            {
                if (bgColors.Contains(tex.GetPixel(x, y)))
                {
                    tex.SetPixel(x, y, AverageColor(GetSurroundingColors(tex, x, y, radius, bgColors)));
                }
            }
        }

        tex.Apply();
        return tex;
    }

    // slowest hole filling, results are the best
    public static Texture2D FillHolesSlow(Texture2D tex, int radius, params Color[] bgColors)
    {
        var newTex = new Texture2D(tex.width, tex.height);
        for (var x = 0; x < tex.width; x++)
        {
            for (var y = 0; y < tex.height; y++)
            {
                if (bgColors.Contains(tex.GetPixel(x, y)))
                {
                    newTex.SetPixel(x, y, AverageColor(GetSurroundingColors(tex, x, y, radius, bgColors)));
                }
                else
                {
                    newTex.SetPixel(x, y, tex.GetPixel(x, y));
                }
            }
        }

        newTex.Apply();
        return newTex;
    }

    // copy a texture2D object
    public static Texture2D CopyTexture(Texture2D tex)
    {
        var newTex = new Texture2D(tex.width, tex.height);
        for (var x = 0; x < tex.width; x++)
        {
            for (var y = 0; y < tex.height; y++)
            {
                newTex.SetPixel(x, y, tex.GetPixel(x, y));
            }
        }
        newTex.Apply();
        return newTex;
    }

    // Get the average color of the given colors
    private static Color AverageColor(params Color[] colors)
    {
        var (r, g, b, n) = (0f, 0f, 0f, 0);
        foreach (var c in colors)
        {
            r += c.r;
            g += c.g;
            b += c.b;
            n++;
        }
        return new Color(r / n, g / n, b / n);
    }

    // separates the brightness and color components
    private static (float, float, float, float) Dismantle(Color c)
    {
        var minComponent = (c.r < c.g ? c.r : c.g) < c.b ? (c.r < c.g ? c.r : c.g) : c.b;
        return (c.r - minComponent, c.g - minComponent, c.b - minComponent, minComponent);
    }

    // Get the surrounding colors in a given radius around the given point, excludes the given colors
    private static Color[] GetSurroundingColors(Texture2D tex, int xPos, int yPos, int r, params Color[] excludeColors)
    {
        var colors = new List<Color>();
        for (var x = xPos - r; x < xPos + r; x++)
        {
            for (var y = yPos - r; y <yPos + r; y++)
            {
                if (x < 0 || x >= tex.width || y < 0 || y >= tex.height) continue;
                if (excludeColors.Contains(tex.GetPixel(x, y))) continue;
                colors.Add(tex.GetPixel(x, y));
            }
        }
        return colors.ToArray();
    }

    // Get the brightest color in the eye (sclera color)
    private static Color GetScleraReferenceColor(Texture2D tex)
    {
        var eyeReference = new Texture2D(1, 1);
        eyeReference.LoadImage(File.ReadAllBytes(EyeMask));
        var scleraColor = Color.black;
        for (var x = 0; x < tex.width; x++)
        {
            for (var y = 0; y < tex.height; y++)
            {
                if (eyeReference.GetPixel(x, y) != Color.white) continue;
                var c = tex.GetPixel(x, y);
                if (GetColorBrightness(c) > GetColorBrightness(scleraColor)) scleraColor = c;
            }
        }

        return scleraColor;
    }

    // get the brightness of the given color
    private static float GetColorBrightness(Color c)
    {
        return (c.r + c.g + c.b) / 3;
    }

    // Get a histogram of the given texture
    private static (int[], int[], int[]) GetHistogram(Texture2D tex)
    {
        var rHist = new int[256];
        var gHist = new int[256];
        var bHist = new int[256];
        
        for (var x = 0; x < tex.width; x++)
        {
            for (var y = 0; y < tex.height; y++)
            {
                Color32 c = tex.GetPixel(x, y);
                rHist[c.r]++;
                gHist[c.g]++;
                bHist[c.b]++;
            }
        }
        
        return (rHist, gHist, bHist);
    }
}
