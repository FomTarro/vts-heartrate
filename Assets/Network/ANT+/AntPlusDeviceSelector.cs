using System;
using System.Collections.Generic;
using UnityEngine;

public class AntPlusDeviceSelector : RefreshableDropdown
{
    private List<string> _devices = new List<string>();
    public AntDevice Device { get { return AntPlusManager.Instance.Devices.Count > 0 ? 
    AntPlusManager.Instance.Devices[this._dropdown.value] : null; } }
    public override void Refresh()
    {
        try{
            AntPlusManager.Instance.StartScan();
        }catch(Exception e){
            Debug.LogError(e);
        }
    }

    private void Update(){
        _devices = new List<string>();
        foreach(AntDevice device in AntPlusManager.Instance.Devices){
            _devices.Add(device.name);
        }
        RefreshValues(_devices);
    }

    protected override void SetValue(int index){

    }
}
