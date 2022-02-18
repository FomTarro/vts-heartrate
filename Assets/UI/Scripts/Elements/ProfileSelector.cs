using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProfileSelector : RefreshableDropdown
{
    private Dictionary<string, string> _idNameMap = new Dictionary<string, string>();

    protected override void SetValue(int index)
    {
        // do nothing
    }

    public override void Refresh()
    {
        this._idNameMap = HeartrateManager.Instance.Plugin.GetModelDataNameMap();
        RefreshValues(this._idNameMap.Keys);
    }

    public void CopyProfile(){
        string name = this._dropdown.options[this._dropdown.value].text;
        string id = this._idNameMap[name];
        HeartrateManager.Instance.Plugin.CopyModelData(id, name);
    }
}
