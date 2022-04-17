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
                HeartrateManager.Instance.Plugin.Hotkeys[this._dropdown.value].name : null) :
            this._waitingOn;
        }
    }

    [SerializeField]
    private Dropdown _behavior = null;
    public TriggerBehavior Behavior { get { return (TriggerBehavior)this._behavior.value; } }
    private TriggerBehavior _priorBehavior = TriggerBehavior.ACTIVATE_ABOVE_ACTIVATE_BELOW;

    public void Clone()
    {
        //HeartrateManager.Instance.Plugin.CreateHotkeyModule(this.ToSaveData());
    }

    public void Delete()
    {
        //HeartrateManager.Instance.Plugin.DestroyHotkeyModule(this);
    }

    public void CheckModuleCondition(int priorHeartrate, int currentHeartrate){
    
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

    private void Update(){
        RefreshHotkeyList();
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

    public enum TriggerBehavior : int
    {
        ACTIVATE_ABOVE_ACTIVATE_BELOW = 0,
        ACTIVATE_ABOVE = 1,
        ACTIVATE_BELOW = 2,
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
