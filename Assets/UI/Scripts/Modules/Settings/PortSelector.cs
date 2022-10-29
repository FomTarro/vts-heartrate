using UnityEngine;
using System.Collections.Generic;

public class PortSelector : RefreshableDropdown
{
    private List<string> _portNumbers = new List<string>();

    protected override void Initialize(){
        UIManager.Instance.RegisterEventCallback(UIManager.Tabs.SETTINGS, Refresh);
    }

    protected override void SetValue(int index){
        HeartrateManager.Instance.Plugin.SetPort(int.Parse(this._dropdown.options[index].text));
        HeartrateManager.Instance.Plugin.Connect();
    }

    public override void Refresh(){
        int optionCount = this._dropdown.options != null ? this._dropdown.options.Count : 0;
        RefreshValues(HeartrateManager.Instance.Plugin.GetPorts().Keys);
        int newOptionsCount = this._dropdown.options != null ? this._dropdown.options.Count : 0;
        if(optionCount == 0 && newOptionsCount > 0){
            int port = int.Parse(this._dropdown.options[0].text);
            Debug.Log(string.Format("Setting default port: {0}", port));
            HeartrateManager.Instance.Plugin.SetPort(port);
        }
    }
}
