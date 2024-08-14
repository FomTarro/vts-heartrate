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
    public float Modifier
    {
        get { return this._slider.value; }
    }

    private bool loaded = false;

    public EffectConfigs Effect { get; private set; }
    public Effects ParentEffect { get; private set; }
    public void Initialize(Effects parentEffect, PostProcessingEffectConfig config)
    {
        if (!loaded)
        {
            FromSaveData(new SaveData());
        }
        this.Effect = config.enumID;
        this.ParentEffect = parentEffect;
        this._effectName.text = config.internalID;
    }

    public void Reset()
    {
        this._dropdown.SetValueWithoutNotify(0);
        this._slider.value = 0;
        OnValueChange(0);
        UIManager.Instance.HidePopUp();
    }

    private void OnValueChange(float value)
    {
        this._sliderValueDisplay.text = string.Format("{0:0.00}", value) + " +";
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
        data.modifier = this.Modifier;
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
        this._slider.value = data.modifier;
        this._slider.onValueChanged.AddListener(OnValueChange);
        OnValueChange(data.modifier);
        this.loaded = true;
    }
}
