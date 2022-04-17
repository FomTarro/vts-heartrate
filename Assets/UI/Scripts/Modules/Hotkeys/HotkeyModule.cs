using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using VTS.Models;

public class HotkeyModule : MonoBehaviour
{
    [SerializeField]
    private InputField _threshold = null;
    public int Threshold { get { return MathUtils.StringToByte(this._threshold.text); } }
    private int _priorThreshold = 0;
    [SerializeField]
    private Dropdown _dropdown = null;
    // Because the dropdown is populated by an async method, 
    // we load the expression that should be selected from a profile load into this buffer
    // until the async method resolves.
    private string _waitingOn = null;
    public string SelectedHotkey
    {
        get
        {
            return this._waitingOn == null ?
            (this._dropdown.value < HeartrateManager.Instance.Plugin.Hotkeys.Count ?
                HeartrateManager.Instance.Plugin.Hotkeys[this._dropdown.value].id : null) :
            this._waitingOn;
        }
    }

    [SerializeField]
    private Dropdown _behavior = null;
    public TriggerBehavior Behavior { get { return (TriggerBehavior)this._behavior.value; } }
    private TriggerBehavior _priorBehavior = TriggerBehavior.ACTIVATE_ABOVE_ACTIVATE_BELOW;

    public void Clone()
    {
        HeartrateManager.Instance.Plugin.CreateHotkeyModule(this.ToSaveData());
    }

    public void Delete()
    {
        HeartrateManager.Instance.Plugin.DestroyHotkeyModule(this);
    }

    public void CheckModuleCondition(int priorHeartrate, int currentHeartrate){
        if(this._priorBehavior != this.Behavior){
            // forces a re-assessment of the module status
            // TODO: if the behavior changes from ABOVE AND BELOW to just ABOVE, 
            // it will try to trigger AGAIN which just turns it off
            this._priorThreshold = -1;
        }
        // rising edge
        if(
        (this._priorThreshold != this.Threshold && currentHeartrate >= this.Threshold) ||
        (priorHeartrate < this.Threshold && currentHeartrate >= this.Threshold)){
            if(
                this.Behavior == TriggerBehavior.ACTIVATE_ABOVE_ACTIVATE_BELOW || 
                this.Behavior == TriggerBehavior.ACTIVATE_ABOVE){
                HeartrateManager.Instance.Plugin.TriggerHotkey(this.SelectedHotkey, 
                (s) => {},
                (e) => {});
            }
        // falling edge
        }else if(
        (this._priorThreshold != this.Threshold && currentHeartrate < this.Threshold) ||
        (priorHeartrate >= this.Threshold && currentHeartrate < this.Threshold)){
            if(
                this.Behavior == TriggerBehavior.ACTIVATE_ABOVE_ACTIVATE_BELOW || 
                this.Behavior == TriggerBehavior.ACTIVATE_BELOW){
                HeartrateManager.Instance.Plugin.TriggerHotkey(this.SelectedHotkey, 
                (s) => {},
                (e) => {});
            }
        }
        this._priorThreshold = this.Threshold;
        this._priorBehavior = this.Behavior;
    }

    private int HotkeyToIndex(string hotkeyName)
    {
        return this._dropdown.options.FindIndex((o)
            =>
        { return o.text.Equals(hotkeyName); });
    }

    private void SetHotkey(string hotkey)
    {
        int index = HotkeyToIndex(hotkey);
        if (index < 0)
        {
            this._waitingOn = hotkey;
        }
        else if (this._dropdown.options.Count > 0)
        {
            this._dropdown.SetValueWithoutNotify(index);
        }
    }

    // TODO: consolidate this behavior into RefreshableDropdown
    public void RefreshHotkeyList()
    {
        int currentIndex = this._dropdown.value;
        string hotkey = this._dropdown.options.Count > 0 ?
                            this._dropdown.options[currentIndex].text:
                            null;
        this._dropdown.ClearOptions();
        List<string> hotkeyNames = new List<string>();
        foreach(HotkeyListItem data in HeartrateManager.Instance.Plugin.Hotkeys){
            hotkeyNames.Add(data.name);
        }
        this._dropdown.AddOptions(hotkeyNames);
        this._dropdown.RefreshShownValue();
        if (this._waitingOn != null)
        {
            SetHotkey(this._waitingOn);
            this._waitingOn = null;
        }
        else
        {
            SetHotkey(hotkey);
        }
    }

        [System.Serializable]
    public class SaveData
    {
        public string hotkeyID;
        public int threshold;
        public TriggerBehavior behavior;

        public override string ToString()
        {
            return JsonUtility.ToJson(this);
        }
    }

    public SaveData ToSaveData()
    {
        SaveData data = new SaveData();
        data.threshold = this.Threshold;
        data.behavior = this.Behavior;
        data.hotkeyID = this.SelectedHotkey;
        return data;
    }

    public void FromSaveData(SaveData data)
    {
        this._behavior.ClearOptions();
        this._behavior.AddOptions(Names());
        this._threshold.text = data.threshold.ToString();
        this._behavior.SetValueWithoutNotify((int)data.behavior);
        SetHotkey(data.hotkeyID);
    }

    public enum TriggerBehavior : int
    {
        ACTIVATE_ABOVE_ACTIVATE_BELOW = 0,
        ACTIVATE_ABOVE = 1,
        ACTIVATE_BELOW = 2,
    }
    private static List<string> Names(){
        return new List<String> ( new String[] {
            "Activate above, Activate below",
            "Activate above",
            "Activate below",

        } ); 
    }
}

public struct HotkeyListItem{
    public string name;
    public string id;
    public HotkeyListItem(string name, string id){
        this.name = name;
        this.id = id;
    }
}
