using UnityEngine;
using System.Collections.Generic;

public class PortSelector : RefreshableDropdown {

    protected override void Initialize(){
        UIManager.Instance.RegisterEventCallback(UIManager.Tabs.SETTINGS, Refresh);
    }

    protected override void SetValue(int index){
        HeartrateManager.Instance.Plugin.SetPort(int.Parse(this._dropdown.options[index].text));
        HeartrateManager.Instance.Plugin.Connect();
        // Set display value to actual port
        UpdateDisplay();
    }

    public override void Refresh(){
        List<int> sortedPorts = new List<int>(HeartrateManager.Instance.Plugin.GetPorts().Keys);
        sortedPorts.Sort();
        RefreshValues(sortedPorts);
        // Set display value to actual port
        UpdateDisplay();
    }

    private void UpdateDisplay(){
        int index = StringToIndex(HeartrateManager.Instance.Plugin.GetPort().ToString());
        if(index > -1 && index < this._dropdown.options.Count){
            this._dropdown.SetValueWithoutNotify(index);
        }
    }
}
