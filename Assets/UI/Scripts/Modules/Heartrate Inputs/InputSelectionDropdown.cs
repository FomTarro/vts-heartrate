using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Localization;

public class InputSelectionDropdown : RefreshableDropdown
{
    private Dictionary<string, HeartrateInputModule> _modules = new Dictionary<string, HeartrateInputModule>();

    public override void Refresh()
    {
        this._modules.Clear();
        foreach(HeartrateInputModule module in HeartrateManager.Instance.Plugin.HeartrateInputs){
            this._modules.Add(LocalizationManager.Instance.GetString(module.ToString()), module);
        }
        RefreshValues(this._modules.Keys);
    }

    protected override void SetValue(int index)
    {
        this._modules[this._dropdown.options[index].text].Activate();
    }

    private void Update(){
        if(HeartrateManager.Instance.Plugin.ActiveInputModule != null && this._dropdown.value > 0 && 
            (this._dropdown.options[this._dropdown.value].text != HeartrateManager.Instance.Plugin.ActiveInputModule)){
            int index = this.StringToIndex(HeartrateManager.Instance.Plugin.ActiveInputModule);
            this._dropdown.SetValueWithoutNotify(index);
            this.SetValue(index);
        }
    }
}
