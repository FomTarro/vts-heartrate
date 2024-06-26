﻿using UnityEngine;

public class AntPlusInputModule : HeartrateInputModule
{
    [SerializeField]
    private AntPlusDeviceSelector _selector = null; 
    [SerializeField]
    private StatusIndicator _status = null;


    public override int GetHeartrate(){
        return AntPlusManager.Instance.Heartrate;
    }

    protected override void FromValues(SaveData.Values values){
        // No settings to load
    }

    public void Connect(){
        AntPlusManager.Instance.ConnectToDevice(this._selector.Device, this._status.SetStatus);
    }

    public void Disconnect(){
        AntPlusManager.Instance.DisconnectFromDevice(this._status.SetStatus);
    }

    protected override SaveData.Values ToValues()
    {
        return new SaveData.Values();
    }

    protected override void OnStatusChange(bool isActive)
    {
        if(!isActive){
            AntPlusManager.Instance.DisconnectFromDevice(this._status.SetStatus);
        }
    }
}
