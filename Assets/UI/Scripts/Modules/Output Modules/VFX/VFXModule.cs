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

    public List<PostProcessingValue> EffectParameters
    {
        get
        {
            List<PostProcessingValue> values = new List<PostProcessingValue>();
            foreach (EffectParameterEntry item in this._effectParameterList)
            {
                if (item.gameObject.activeSelf)
                {
                    values.Add(new PostProcessingValue(item.Effect, item.Value));
                }
            }
            return values;
        }
    }

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

    public void Reset(bool hardReset = true)
    {
        Debug.Log("Resetting VFX module...");
        VTSPostProcessingUpdateOptions options = new VTSPostProcessingUpdateOptions();
        options.setPostProcessingValues = true;
        List<PostProcessingValue> values = new List<PostProcessingValue>();
        foreach (EffectParameterEntry item in this._effectParameterList)
        {
            if (item.gameObject.activeSelf)
            {
                values.Add(new PostProcessingValue(item.Effect, 0));
                if (hardReset)
                {
                    item.Reset();
                }
            }
        }
        if (HeartrateManager.Instance.Plugin.IsAuthenticated)
        {
            HeartrateManager.Instance.Plugin.SetPostProcessingEffectValues(options, values.ToArray(),
            (s) => { },
            (e) =>
            {
                Debug.LogError(string.Format("Error while setting VFX Config in VTube Studio: {0} - {1}",
                    e.data.errorID, e.data.message));
            });
        }
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
            PopulateOptionsForEffect(((VFXDropdownOption)this._dropdown.options[index]).Data);
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
        foreach (PostProcessingEffect effect in effects)
        {
            VFXDropdownOption opt = new VFXDropdownOption();
            opt.text = string.Format("{0}", effect.internalID);
            opt.Data = effect;
            effectNames.Add(opt);
            // used to instantiate an item for every property of every effect, almost 200/per module at time of writing
            // instead, we now create as many as needed for any given selected effect.
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

    private void PopulateOptionsForEffect(PostProcessingEffect effect)
    {
        int i = 0;
        foreach (PostProcessingEffectConfig property in effect.configEntries)
        {
            // first we add any entries that don't exist in the list already
            if (property.type.ToLower().Equals("float"))
            {
                if (this._effectParameterList.Count <= i)
                {
                    EffectParameterEntry instance = Instantiate<EffectParameterEntry>(this._effectParameterPrefab, Vector3.zero, Quaternion.identity, this._parameterListParent);
                    instance.name = effect.enumID + " - " + property.enumID + " - " + i;
                    instance.Initialize(effect.enumID, property);
                    instance.gameObject.SetActive(false);
                    this._effectParameterList.Add(instance);
                }
                else
                {
                    EffectParameterEntry entry = this._effectParameterList[i];
                    entry.name = effect.enumID + " - " + property.enumID + " - " + i;
                    entry.Initialize(effect.enumID, property);
                    entry.gameObject.SetActive(false);
                }
                i++;
                // EffectParameterEntry entry = this._effectParameterList.Find(item => item.Effect == property.enumID);
                // if (entry != null)
                // {
                //     entry.Initialize(effect.enumID, property);
                //     entry.name = effect.enumID + " - " + property.enumID + " - " + i;
                // }
                // else
                // {
                //     EffectParameterEntry instance = Instantiate<EffectParameterEntry>(this._effectParameterPrefab, Vector3.zero, Quaternion.identity, this._parameterListParent);
                //     entry.name = effect.enumID + " - " + property.enumID + " - " + i;
                //     instance.Initialize(effect.enumID, property);
                //     instance.gameObject.SetActive(false);
                //     this._effectParameterList.Add(instance);
                // }
            }
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
        Debug.Log("Loading VFX module...");
        this._dropdown.onValueChanged.AddListener((i) =>
        {
            // Only reset on dropdwon change, not on programatic set
            // This prevents the initial population of the dropdown from wiping loaded data that is waiting to be relevant
            Reset();
            OnEffectSelectionChanged(((VFXDropdownOption)this._dropdown.options[i]).Data.enumID);
        });
        OnEffectSelectionChanged(data.effectID);
        foreach (EffectParameterEntry.SaveData item in data.effectConfigs)
        {
            // create as many effects parameters as we think we will need
            EffectParameterEntry instance = Instantiate<EffectParameterEntry>(this._effectParameterPrefab, Vector3.zero, Quaternion.identity, this._parameterListParent);
            instance.FromSaveData(item);
            instance.gameObject.SetActive(false);
            this._effectParameterList.Add(instance);
        }
        RefreshEffectList();
    }
}
