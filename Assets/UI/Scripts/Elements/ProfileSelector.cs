using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ProfileSelector : RefreshableDropdown
{
    // key is ID, value is readable name
    private Dictionary<string, SaveDataManager.ModelProfileInfo> _idNameMap = new Dictionary<string, SaveDataManager.ModelProfileInfo>();
    [SerializeField]
    private TMP_Text _fileNameDisplay = null;

    protected override void Initialize()
    {
        UIManager.Instance.RegisterTabCallback(UIManager.Tabs.OUTPUTS, Refresh);
    }

    protected override void SetValue(int index)
    {
        string key = this._dropdown.options[this._dropdown.value].text;
        SaveDataManager.ModelProfileInfo data = this._idNameMap[key];
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
        SaveDataManager.ModelProfileInfo info = this._idNameMap[name];
        SaveDataManager.Instance.CopyModelProfile(info);
    }

    public void DeleteProfile(){
        string key = this._dropdown.options[this._dropdown.value].text;
        SaveDataManager.Instance.DeleteModelProfile(this._idNameMap[key]);
    }

    public void LoadProfile(){
        string key = this._dropdown.options[this._dropdown.value].text;
        SaveDataManager.Instance.LoadModelProfile(this._idNameMap[key]);
    }
}
