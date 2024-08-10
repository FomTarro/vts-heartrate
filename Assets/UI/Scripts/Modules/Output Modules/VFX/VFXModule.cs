using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using VTS.Core;

public class VFXModule : MonoBehaviour
{
    [SerializeField]
    private TMP_Dropdown _dropdown = null;

    [SerializeField]
    private RectTransform _parameterListParent = null;

    [SerializeField]
    private EffectParameterEntry _effectParameterPrefab = null;

    private static List<PostProcessingEffect> _effects = new List<PostProcessingEffect>();
    private string _waitingOn = null;
    public string SelectedEffect
    {
        get
        {
            return this._waitingOn == null ?
            (this._dropdown.value < _effects.Count ?
                _effects[this._dropdown.value].internalID : null) :
            this._waitingOn;
        }
    }

    [SerializeField]
    private TMP_Text _minimizedSummary = null;


    public void Clone()
    {
        HeartrateManager.Instance.Plugin.CreateVFXModule(this.ToSaveData());
    }

    public void Delete()
    {
        HeartrateManager.Instance.Plugin.DestroyVFXModule(this);
    }

    public void RefreshVFXList()
    {
        int currentIndex = this._dropdown.value;
        string currentEffect = this._dropdown.options.Count > 0 ? this._dropdown.options[currentIndex].text : null;
        this._dropdown.ClearOptions();
        _effects = HeartrateManager.Instance.Plugin.VFXList;
        List<string> effectNames = new List<string>();
        foreach (PostProcessingEffect effect in _effects)
        {
            effectNames.Add(string.Format("<size=0>{0}</size>{1}", effect.internalID, effect.enumID));
        }
        this._dropdown.AddOptions(effectNames);
        this._dropdown.RefreshShownValue();
        if (this._waitingOn != null)
        {
            SetEffect(this._waitingOn);
        }
        else
        {
            SetEffect(currentEffect);
        }
    }

    private int EffectToIndex(string effectID)
    {
        return effectID == null
            ? -1
            : this._dropdown.options.FindIndex((o) => { return o.text.Contains(effectID); });
    }

    private void SetEffect(string effectID)
    {
        // index will only be -1 if the desired item is not in the list
        int index = EffectToIndex(effectID);
        if (index < 0)
        {
            this._waitingOn = effectID;
        }
        else if (this._dropdown.options.Count > 0 && this._dropdown.options.Count > index)
        {
            this._dropdown.SetValueWithoutNotify(index);
            // finally found what we were waiting for
            this._waitingOn = null;
        }
        this._minimizedSummary.text = string.Format("({0})", GetMinimizedText());


    }

    private string GetMinimizedText()
    {
        string name = this._dropdown.options.Count > 0 && _effects.Count >= this._dropdown.options.Count
            ? string.Format("{0}", _effects[this._dropdown.value].enumID)
            : "NO EFFECT SET";
        if (name.Length > 48)
        {
            return string.Format("{0}...", name.Substring(0, 45));
        }
        else
        {
            return name;
        }
    }

    public void Apply(Dictionary<string, float> parameter)
    {

    }

    [System.Serializable]
    public class SaveData
    {
        public string effectID;

        public override string ToString()
        {
            return JsonUtility.ToJson(this);
        }
    }

    public SaveData ToSaveData()
    {
        SaveData data = new SaveData();
        data.effectID = this.SelectedEffect;
        return data;
    }

    public void FromSaveData(SaveData data)
    {
        // TODO;
    }
}
