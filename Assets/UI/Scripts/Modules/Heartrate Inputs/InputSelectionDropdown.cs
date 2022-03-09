using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputSelectionDropdown : RefreshableDropdown
{
    private Dictionary<string, HeartrateInputModule> _modules = new Dictionary<string, HeartrateInputModule>();

    public override void Refresh()
    {
        this._modules.Clear();
        foreach(HeartrateInputModule module in HeartrateManager.Instance.Plugin.HeartrateInputs){
            this._modules.Add(module.ToString(), module);
        }
        RefreshValues(HeartrateManager.Instance.Plugin.HeartrateInputs);
    }

    protected override void SetValue(int index)
    {
        this._modules[this._dropdown.options[index].text].SetStatus(true);
    }

    private void Update(){
        if(HeartrateManager.Instance.Plugin.ActiveInputModule != null && 
            (this._dropdown.options[this._dropdown.value].text != HeartrateManager.Instance.Plugin.ActiveInputModule)){
            int index = this.StringToIndex(HeartrateManager.Instance.Plugin.ActiveInputModule);
            this._dropdown.SetValueWithoutNotify(index);
            this.SetValue(index);
        }
    }
}
