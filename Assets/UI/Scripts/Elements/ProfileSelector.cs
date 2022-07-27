using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ProfileSelector : RefreshableDropdown
{
    // key is ID, value is readable name
    private Dictionary<string, HeartratePlugin.ModelSaveData> _idNameMap = new Dictionary<string, HeartratePlugin.ModelSaveData>();
    [SerializeField]
    private TMP_Text _fileNameDisplay = null;

    protected override void SetValue(int index)
    {
        // do nothing
        string name = this._dropdown.options[this._dropdown.value].text;
        HeartratePlugin.ModelSaveData data = this._idNameMap[name];
        this._fileNameDisplay.text = string.Format("{0}.json", data.FileName);
    }

    public override void Refresh()
    {
        this._idNameMap = SaveDataManager.Instance.GetModelDataNameMap();
        RefreshValues(this._idNameMap.Keys);
        SetValue(this._dropdown.value);
    }

    public void CopyProfile(){
        string name = this._dropdown.options[this._dropdown.value].text;
        string fileName = this._idNameMap[name].FileName;
        SaveDataManager.Instance.CopyModelSaveData(fileName, name);
    }
}
