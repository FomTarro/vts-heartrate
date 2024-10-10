using UnityEngine;
using TMPro;
using VTS.Core;
using System;
using System.Collections.Generic;
using UnityEngine.UI;

public class EffectParameterEntry : MonoBehaviour
{
    [SerializeField]
    private TMP_Text _effectName = null;
    [SerializeField]
    private TMP_Dropdown _dropdown = null;
    public string SelectedParameter
    {
        get
        {
            return this._dropdown.options.Count > this._dropdown.value ? this._dropdown.options[this._dropdown.value].text : "NONE";
        }
    }

    [SerializeField]
    private Slider _slider = null;
    [SerializeField]
    private TMP_Text _sliderValueDisplay = null;
    [SerializeField]
    private TMP_Text _paramValueDisplay = null;
    [SerializeField]
    private TMP_Text _calculatedValueDisplay = null;
    [SerializeField]
    private TMP_Text _minimizedValueDisplay = null;
    public float Modifier
    {
        get { return MathUtils.Normalize(this._slider.value, -1f, 1f, Min, Max); }
    }
    public float Param
    {
        get
        {
            float param = HeartrateManager.Instance.Plugin.ParameterMap.ContainsKey(SelectedParameter) ? HeartrateManager.Instance.Plugin.ParameterMap[SelectedParameter] : 0;
            return param;
        }
    }

    private bool loaded = false;

    public EffectConfigs Effect { get; private set; }
    public Effects ParentEffect { get; private set; }

    [SerializeField]
    private float _min = 0;
    public float Min { get { return this._min; } }
    [SerializeField]
    private float _max = 1;
    public float Max { get { return this._max; } }

    public float Range { get { return this._max - this._min; } }
    public float Value { get { return Mathf.Max(Min, Mathf.Min(Max, this.Param * this.Modifier)); } }

    public void Initialize(Effects parentEffect, PostProcessingEffectConfig config)
    {
        if (!loaded)
        {
            FromSaveData(new SaveData());
        }
        this.Effect = config.enumID;
        this.ParentEffect = parentEffect;
        this._effectName.text = config.internalID;
        this._min = config.floatMin;
        this._max = config.floatMax;
    }

    public void Reset()
    {
        this._dropdown.SetValueWithoutNotify(0);
        this._slider.value = 0;
        OnValueChange(0);
    }

    private void Update()
    {
        UpdateDisplay();
    }

    public void UpdateDisplay()
    {
        this._paramValueDisplay.text = string.Format("{0:0.00}", Param);
        this._sliderValueDisplay.text = string.Format("{0:0.00}", Modifier);
        this._calculatedValueDisplay.text = string.Format("{0:0.00}", Value);
        this._minimizedValueDisplay.text = string.Format("({0:0.00})", Value);
    }

    private void OnValueChange(float value)
    {
        UpdateDisplay();
    }

    private void OnDropdownChange(int index)
    {
        this._slider.value = 0;
        OnValueChange(0);
        UpdateDisplay();
    }
    [Serializable]
    public class SaveData
    {
        public EffectConfigs effect;
        public string drivingParameter;
        public float modifier = 0;

        public override string ToString()
        {
            return JsonUtility.ToJson(this);
        }
    }

    public SaveData ToSaveData()
    {
        SaveData data = new SaveData();
        data.effect = this.Effect;
        data.drivingParameter = this.SelectedParameter;
        data.modifier = this._slider.value;
        return data;
    }

    public void FromSaveData(SaveData data)
    {
        this.Effect = data.effect;
        List<string> opts = new List<string>();
        opts.Add("NONE");
        foreach (string param in HeartrateManager.Instance.Plugin.ParameterMap.Keys)
        {
            opts.Add(param);
        }
        this._dropdown.AddOptions(opts);
        this._dropdown.SetValueWithoutNotify(this._dropdown.options.FindIndex(opt => opt.text.Equals(data.drivingParameter)));
        this._dropdown.onValueChanged.AddListener(OnDropdownChange);
        this._slider.value = data.modifier;
        this._slider.onValueChanged.AddListener(OnValueChange);
        OnValueChange(data.modifier);
        this.loaded = true;
    }
}
