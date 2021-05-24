using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

// class to manage the blendshape modifiers
public class BlendshapeSettings : MonoBehaviour
{

    public Dropdown SelectBlendshapeDropdown;
    public Slider LowerThresholdSlider;
    public Slider UpperThresholdSlider;
    public Slider MultiplierSlider;

    public Text LowerThresholdText;
    public Text UpperThresholdText;
    public Text MultiplierText;

    public GameObject SliderValueArea;
    public GameObject ValueSliderPrefab;
    public Slider AudioSlider;
    
    public static float[] modifiers;

    void Start()
    {
        var options = new List<string>(eos.ReadBlendshapesFromFile(Path.Combine(Application.streamingAssetsPath,
            "share/custom_expression_blendshapes_3448.txt")));
        SelectBlendshapeDropdown.ClearOptions();
        SelectBlendshapeDropdown.AddOptions(options);
        modifiers = new float[options.Count * 3];
        for (var i = 0; i < options.Count; i++)
        {
            modifiers[i * 3] = 0.0f; //lower threshold
            modifiers[i * 3 + 1] = 1.0f; //upper threshold
            modifiers[i * 3 + 2] = 1.0f; //multiplier
        }
        
        AudioSlider = Instantiate(ValueSliderPrefab, SliderValueArea.transform, true).GetComponent<Slider>();
        AudioSlider.GetComponentInChildren<Text>().text = "Audio Level";

        for (var i = 0; i < SelectBlendshapeDropdown.options.Count; i++)
        {
            var blendshape = SelectBlendshapeDropdown.options[i].text;
            var slider = Instantiate(ValueSliderPrefab, SliderValueArea.transform, true).GetComponent<Slider>();
            var text = slider.GetComponentInChildren<Text>();
            text.text = blendshape;
            var blendshapeIndex = i;
            eos.OnAfterExpression += blendshapes =>
            {
                slider.value = blendshapes[blendshapeIndex];
                text.text = blendshape + " (" + slider.value +")";
            };
        }

        SelectBlendshapeDropdown.value = 0;
        SelectBlendshapeDropdown.onValueChanged.AddListener(delegate(int i)
        {
            LowerThresholdSlider.value = LowerThreshold(i);
            LowerThresholdText.text = FloatToString(LowerThreshold(i));
            UpperThresholdSlider.value = UpperThreshold(i);
            UpperThresholdText.text = FloatToString(UpperThreshold(i));
            MultiplierSlider.value = Multiplier(i);
            MultiplierText.text = FloatToString(Multiplier(i));
        });

        LowerThresholdSlider.value = LowerThreshold(SelectBlendshapeDropdown.value);
        LowerThresholdText.text = FloatToString(LowerThreshold(SelectBlendshapeDropdown.value));
        LowerThresholdSlider.onValueChanged.AddListener(delegate(float value)
        {
            SetLowerThreshold(SelectBlendshapeDropdown.value, value);
            LowerThresholdText.text = FloatToString(LowerThreshold(SelectBlendshapeDropdown.value));
        });
        
        UpperThresholdSlider.value = LowerThreshold(SelectBlendshapeDropdown.value);
        UpperThresholdText.text = FloatToString(UpperThreshold(SelectBlendshapeDropdown.value));
        UpperThresholdSlider.onValueChanged.AddListener(delegate(float value)
        {
            SetUpperThreshold(SelectBlendshapeDropdown.value, value);
            UpperThresholdText.text = FloatToString(UpperThreshold(SelectBlendshapeDropdown.value));
        });
        
        MultiplierSlider.value = LowerThreshold(SelectBlendshapeDropdown.value);
        MultiplierText.text = FloatToString(Multiplier(SelectBlendshapeDropdown.value));
        MultiplierSlider.onValueChanged.AddListener(delegate(float value)
        {
            SetMultiplier(SelectBlendshapeDropdown.value, value);
            MultiplierText.text = FloatToString(Multiplier(SelectBlendshapeDropdown.value));
        });
    }

    private void Update()
    {
        AudioSlider.value = AudioInputHandler.MicMean;
    }

    private float LowerThreshold(int i)
    {
        return modifiers[i * 3];
    }

    private void SetLowerThreshold(int i, float value)
    {
        modifiers[i * 3] = value;
    }
    
    private float UpperThreshold(int i)
    {
        return modifiers[i * 3 + 1];
    }
    
    private void SetUpperThreshold(int i, float value)
    {
        modifiers[i * 3 + 1] = value;
    }
    
    private float Multiplier(int i)
    {
        return modifiers[i * 3 + 2];
    }
    
    private void SetMultiplier(int i, float value)
    {
        modifiers[i * 3 + 2] = value;
    }

    private string FloatToString(float f)
    {
        var s = Math.Round(f, 2) + "";
        while (s.Length < 5) s = " " + s;
        return s;
    }

    private void ReadBlendshapesFromFile(string filePath)
    {
        var options = new List<string>();
        using (var file = new StreamReader(filePath))
        {
            string line;
            while ((line = file.ReadLine()) != null)
            {
                var lineList = new List<string>(line.Split(' '));
                options.Add(lineList[0]);
            }
        }
        SelectBlendshapeDropdown.ClearOptions();
        SelectBlendshapeDropdown.AddOptions(options);
        modifiers = new float[options.Count * 3];
        for (var i = 0; i < options.Count; i++)
        {
            modifiers[i * 3] = 0.0f; //lower threshold
            modifiers[i * 3 + 1] = 1.0f; //upper threshold
            modifiers[i * 3 + 2] = 1.0f; //multiplier
        }
    }
}
