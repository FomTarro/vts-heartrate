using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ProfileSelector : RefreshableDropdown
{
    // key is ID, value is readable name
    private Dictionary<string, string> _idNameMap = new Dictionary<string, string>();
    [SerializeField]
    private TMP_Text _fileNameDisplay = null;

    protected override void SetValue(int index)
    {
        // do nothing
        string name = this._dropdown.options[this._dropdown.value].text;
        string id = this._idNameMap[name];
        this._fileNameDisplay.text = string.Format("{0}.json", id);
    }

    public override void Refresh()
    {
        this._idNameMap = HeartrateManager.Instance.Plugin.GetModelDataNameMap();
        RefreshValues(this._idNameMap.Keys);
        SetValue(this._dropdown.value);
    }

    public void CopyProfile(){
        string name = this._dropdown.options[this._dropdown.value].text;
        string id = this._idNameMap[name];
        HeartrateManager.Instance.Plugin.CopyModelData(id, name);
    }
}
