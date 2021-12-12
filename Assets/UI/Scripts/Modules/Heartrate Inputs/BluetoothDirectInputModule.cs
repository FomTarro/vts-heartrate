using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BluetoothDirectInputModule : HeartrateInputModule
{
    [SerializeField]
    private Dropdown _devices = null;

    private List<BluetoothAdapter.BleDevice> _devicesList = new List<BluetoothAdapter.BleDevice>();

    private void Awake(){
        BluetoothAdapter.Instance.onDeviceScanComplete.AddListener(PopulateDropdown);
    }

    public void PopulateDropdown(List<BluetoothAdapter.BleDevice> list){
        this._devicesList = list;
        List<string> devices = new List<string>();
        foreach(BluetoothAdapter.BleDevice device in list){
            devices.Add(device.name);
        }
        this._devices.ClearOptions ();
        this._devices.AddOptions(devices);
    }

    public void SelectDevice(){
        BluetoothAdapter.Instance.ScanDeviceForData(this._devicesList[this._devices.value]);
    }

    public override int GetHeartrate()
    {
        return 0;
    }

    protected override void FromValues(SaveData.Values values)
    {
        
    }

    protected override SaveData.Values ToValues()
    {
        SaveData.Values values = new SaveData.Values();
        return values;
    }

    protected override void OnStatusChange(bool isActive)
    {

    }
}
