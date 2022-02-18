using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PortSelector : RefreshableDropdown
{
    private List<string> _portNumbers = new List<string>();

    public void Start(){
        this._dropdown.onValueChanged.AddListener((i) => { 
            SetValue(i);
        });
    }

    protected override void SetValue(int index){
        HeartrateManager.Instance.Plugin.SetPort(int.Parse(this._dropdown.options[index].text));
        HeartrateManager.Instance.Plugin.Connect();
    }

    public override void Refresh()
    {
        RefreshValues(HeartrateManager.Instance.Plugin.GetPorts().Keys);
    }
}
