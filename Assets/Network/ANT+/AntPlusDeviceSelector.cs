using System;
using System.Collections.Generic;
using UnityEngine;

public class AntPlusDeviceSelector : RefreshableDropdown
{
    [SerializeField]
    private StatusIndicator _status = null;
    private List<string> _devices = new List<string>();
    public AntDevice Device { get { return AntPlusManager.Instance.Devices.Count > 0 ? 
    AntPlusManager.Instance.Devices[this._dropdown.value] : null; } }
    public override void Refresh(){
        AntPlusManager.Instance.StartScan(this._status.SetStatus);
    }

    private void Update(){
        _devices = new List<string>();
        foreach(AntDevice device in AntPlusManager.Instance.Devices){
            _devices.Add(device.name);
        }
        RefreshValues(_devices);
    }

    protected override void Initialize(){

    }

    protected override void SetValue(int index){

    }

}
