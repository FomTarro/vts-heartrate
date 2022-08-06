using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ProfileSelector : RefreshableDropdown
{
    // key is ID, value is readable name
    private Dictionary<string, SaveDataManager.ModelProfileInfo> _idNameMap = new Dictionary<string, SaveDataManager.ModelProfileInfo>();

    protected override void Initialize()
    {
        UIManager.Instance.RegisterTabCallback(UIManager.Tabs.OUTPUTS, Refresh);
        SaveDataManager.Instance.RegisterProfileLoadedCallback(Refresh);
        SaveDataManager.Instance.RegisterProfileSavedCallback(Refresh);
    }

    protected override void SetValue(int index)
    {
        // No need to do anything here
    }

    public override void Refresh()
    {
        this._idNameMap = SaveDataManager.Instance.GetModelProfileMap();
        RefreshValues(this._idNameMap.Keys);
    }

    public void CopyProfile(){
        if(this._dropdown.options.Count > 0){
            string name = this._dropdown.options[this._dropdown.value].text;
            SaveDataManager.ModelProfileInfo info = this._idNameMap[name];
            SaveDataManager.Instance.CopyModelProfile(info);
        }
    }

    public void DeleteProfile(){
        if(this._dropdown.options.Count > 0){
            string key = this._dropdown.options[this._dropdown.value].text;
            SaveDataManager.Instance.DeleteModelProfile(this._idNameMap[key]);
        }
    }

    public void LoadProfile(){
        if(this._dropdown.options.Count > 0){
            string key = this._dropdown.options[this._dropdown.value].text;
            SaveDataManager.Instance.LoadModelProfile(this._idNameMap[key]);
        }
    }
}
