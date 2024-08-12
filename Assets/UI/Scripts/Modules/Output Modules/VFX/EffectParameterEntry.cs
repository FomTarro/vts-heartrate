using UnityEngine;
using TMPro;
using VTS.Core;
using System;
using System.Collections.Generic;

public class EffectParameterEntry : MonoBehaviour
{
    [SerializeField]
    private TMP_Text _effectName = null;
    [SerializeField]
    private TMP_Dropdown _dropdown = null;
    private bool loaded = false;
    public string SelectedParameter
    {
        get
        {
            return this._dropdown.options.Count > this._dropdown.value ? this._dropdown.options[this._dropdown.value].text : "NONE";
        }
    }

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
    }

    [Serializable]
    public class SaveData
    {
        public EffectConfigs effect;
        public string drivingParameter;

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
        this.loaded = true;
    }
}
