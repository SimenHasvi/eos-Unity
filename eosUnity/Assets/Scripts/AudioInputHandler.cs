using System;
using UnityEngine;

// Class to measure the audio loudness from the microphone (discontinued)
public class AudioInputHandler : MonoBehaviour
{
    private AudioClip _mic;
    private const int SampleWindow = 128;

    public static float MicLoudness = 0;
    public static float MicMean = 0;

    private void Start()
    {
        //_mic = Microphone.Start(Microphone.devices[0], true, 999, 44100);
    }
    
    void Update()
    {
        //MicLoudness = MaxAudioLevel() > 0.1 ? MaxAudioLevel() : 0;
        //MicMean = MeanAudioLevel() > 0.01 ? MeanAudioLevel() : 0;
    }
    
    private float MeanAudioLevel()
    {
        float levelMean = 0;
        var waveData = new float[SampleWindow];
        var micPosition = Microphone.GetPosition(null)-(SampleWindow+1);
        if (micPosition < 0) return 0;
        _mic.GetData(waveData, micPosition);
        for (var i = 0; i < SampleWindow; i++) {
            levelMean += waveData[i] * waveData[i];
        }
        return levelMean / SampleWindow;
    }

    private float MaxAudioLevel()
    {
        float levelMax = 0;
        var waveData = new float[SampleWindow];
        var micPosition = Microphone.GetPosition(null)-(SampleWindow+1);
        if (micPosition < 0) return 0;
        _mic.GetData(waveData, micPosition);
        for (var i = 0; i < SampleWindow; i++) {
            var wavePeak = waveData[i] * waveData[i];
            if (levelMax < wavePeak) {
                levelMax = wavePeak;
            }
        }
        return levelMax;
    }
}