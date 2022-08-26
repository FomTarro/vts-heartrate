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

    public override void Refresh()
    {
        RefreshValues(HeartrateManager.Instance.Plugin.GetPorts().Keys);
    }
}
