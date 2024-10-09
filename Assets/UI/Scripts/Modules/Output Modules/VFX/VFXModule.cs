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

    private List<EffectParameterEntry> _effectParameterList = new List<EffectParameterEntry>();

    // Because the dropdown is populated by an async method, 
    // we load the expression that should be selected from a profile load into this buffer
    // until the async method resolves.
    private int _waitingOnID = -1;
    private Effects SelectedEffect
    {
        get
        {
            return this._waitingOnID != -1 ?
            (Effects)this._waitingOnID :
            GetDataFromDropdownSelection().enumID;
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
        Reset();
        HeartrateManager.Instance.Plugin.DestroyVFXModule(this);
    }

    public void Reset()
    {
        VTSPostProcessingUpdateOptions options = new VTSPostProcessingUpdateOptions();
        options.setPostProcessingValues = true;
        List<PostProcessingValue> values = new List<PostProcessingValue>();
        foreach (EffectParameterEntry item in this._effectParameterList)
        {
            if (item.gameObject.activeSelf)
            {
                values.Add(new PostProcessingValue(item.Effect, 0));
                item.Reset();
            }
        }
        HeartrateManager.Instance.Plugin.SetPostProcessingEffectValues(options, values.ToArray());
    }

    public void ResetAllEffects()
    {
        UIManager.Instance.ShowPopUp(
            "output_vfx_reset_button",
            "output_vfx_reset_warning",
            new PopUp.PopUpOption(
                "button_generic_confirm",
                ColorUtils.ColorPreset.RED,
                () =>
                {
                    Reset();
                    UIManager.Instance.HidePopUp();
                }
            ),
            new PopUp.PopUpOption(
                "button_generic_cancel",
                ColorUtils.ColorPreset.GREY,
                () => { UIManager.Instance.HidePopUp(); }
            )
        );
    }

    public void ApplyEffect()
    {
        if (HeartrateManager.Instance.Plugin.IsAuthenticated)
        {
            VTSPostProcessingUpdateOptions options = new VTSPostProcessingUpdateOptions();
            options.setPostProcessingValues = true;
            options.postProcessingOn = true;
            List<PostProcessingValue> values = new List<PostProcessingValue>();
            foreach (EffectParameterEntry item in this._effectParameterList)
            {
                if (item.gameObject.activeSelf)
                {
                    // float mapped = MathUtils.Normalize(combined, -1, 2)
                    values.Add(new PostProcessingValue(item.Effect, item.Value));
                    item.UpdateDisplay();
                }
            }
            HeartrateManager.Instance.Plugin.SetPostProcessingEffectValues(options, values.ToArray());
        }
    }

    private void OnEffectSelectionChanged(Effects effectID)
    {
        // try to find the index in the list of the effect with the given UUID
        int index =
            (int)effectID == -1
            ? -1
            : this._dropdown.options.FindIndex((o) => { return ((VFXDropdownOption)o).Data.enumID == effectID; });
        if (index < 0)
        {
            // if we can't find that UUID, we store it in a "pending" variable
            // Which we will check every time new options get loaded in
            this._waitingOnID = (int)effectID;
        }
        else if (this._dropdown.options.Count > 0 && this._dropdown.options.Count > index)
        {
            // if we can find the UUID, we clear our "pending" variable
            // and just set the correct option
            Reset();
            this._dropdown.SetValueWithoutNotify(index);
            this._waitingOnID = -1;
        }
        foreach (EffectParameterEntry item in this._effectParameterList)
        {
            if (item.ParentEffect == this.SelectedEffect)
            {
                item.gameObject.SetActive(true);
            }
            else
            {
                item.gameObject.SetActive(false);
            }
        }
        this._minimizedSummary.text = string.Format("({0})", GetMinimizedText());
    }

    private string GetMinimizedText()
    {
        string name = GetDataFromDropdownSelection().internalID != null && GetDataFromDropdownSelection().internalID.Length > 0
            ? string.Format("{0}", GetDataFromDropdownSelection().internalID)
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


    public void RefreshEffectList()
    {
        PostProcessingEffect currentSelection = GetDataFromDropdownSelection();
        this._dropdown.ClearOptions();
        List<PostProcessingEffect> effects = HeartrateManager.Instance.Plugin.VFXList;
        List<TMP_Dropdown.OptionData> effectNames = new List<TMP_Dropdown.OptionData>();
        foreach (PostProcessingEffect data in effects)
        {
            VFXDropdownOption opt = new VFXDropdownOption();
            opt.text = string.Format("{0}", data.internalID);
            opt.Data = data;
            effectNames.Add(opt);
            foreach (PostProcessingEffectConfig config in data.configEntries)
            {
                // first we add any entries that don't exist in the list already
                if (config.type.ToLower().Equals("float"))
                {
                    EffectParameterEntry entry = this._effectParameterList.Find(item => item.Effect == config.enumID);
                    if (entry != null)
                    {
                        entry.Initialize(data.enumID, config);
                    }
                    else
                    {
                        EffectParameterEntry instance = Instantiate<EffectParameterEntry>(this._effectParameterPrefab, Vector3.zero, Quaternion.identity, this._parameterListParent);
                        instance.Initialize(data.enumID, config);
                        instance.gameObject.SetActive(false);
                        this._effectParameterList.Add(instance);
                    }
                }
            }
        }
        this._dropdown.AddOptions(effectNames);
        this._dropdown.RefreshShownValue();
        if (this._waitingOnID != -1)
        {
            OnEffectSelectionChanged((Effects)this._waitingOnID);
        }
        else
        {
            OnEffectSelectionChanged(currentSelection.enumID);
        }
    }

    private PostProcessingEffect GetDataFromDropdownSelection()
    {
        return this._dropdown.options.Count > 0 && this._dropdown.value >= 0
        ? ((VFXDropdownOption)this._dropdown.options[this._dropdown.value]).Data
        : new PostProcessingEffect();
    }

    // Extension class to strap arbitrary data onto a Unity Dropdown option
    public class VFXDropdownOption : ExtendedDropdownOption<PostProcessingEffect> { }

    [Serializable]
    public class SaveData
    {
        public Effects effectID;
        public List<EffectParameterEntry.SaveData> effectConfigs = new List<EffectParameterEntry.SaveData>();

        public override string ToString()
        {
            return JsonUtility.ToJson(this);
        }
    }

    public SaveData ToSaveData()
    {
        SaveData data = new SaveData();
        data.effectID = this.SelectedEffect;
        foreach (EffectParameterEntry item in this._effectParameterList)
        {
            if (item.gameObject.activeSelf)
            {
                data.effectConfigs.Add(item.ToSaveData());
            }
        }
        return data;
    }

    public void FromSaveData(SaveData data)
    {
        this._dropdown.onValueChanged.AddListener((i) =>
        {
            OnEffectSelectionChanged(((VFXDropdownOption)this._dropdown.options[i]).Data.enumID);
        });
        OnEffectSelectionChanged(data.effectID);
        foreach (EffectParameterEntry.SaveData item in data.effectConfigs)
        {
            EffectParameterEntry instance = Instantiate<EffectParameterEntry>(this._effectParameterPrefab, Vector3.zero, Quaternion.identity, this._parameterListParent);
            instance.FromSaveData(item);
            instance.gameObject.SetActive(false);
            this._effectParameterList.Add(instance);
        }
        RefreshEffectList();
    }
}
