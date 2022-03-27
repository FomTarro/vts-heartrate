using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AntPlusDeviceSelector : RefreshableDropdown
{
    private List<string> _devices = new List<string>();
    private AntDevice _device = null;
    public AntDevice Device { get { return AntPlusManager.Instance.Devices[this._dropdown.value]; } }
    public override void Refresh()
    {
        AntPlusManager.Instance.StartScan();
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
