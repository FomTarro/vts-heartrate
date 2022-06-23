﻿using UnityEngine;
using TMPro;

public class APIServerSettings : MonoBehaviour
{
    [SerializeField]
    private StatusIndicator _status = null;

    [SerializeField]
    private TMP_InputField _portInput = null;

    [SerializeField]
    private APIEndpointStatisticsDisplay _dataStats = null;
    [SerializeField]
    private APIEndpointStatisticsDisplay _eventStats = null;
    [SerializeField]
    private APIEndpointStatisticsDisplay _inputStats = null;

    [SerializeField]
    private TMP_Text _wsUrl = null;

    // Start is called before the first frame update
    void Start()
    {
        this._portInput.onEndEdit.AddListener((v) => { ValidatePortValue(v); });
        ValidatePortValue(APIManager.Instance.Port + "");
        RestartServer();
    }

    // Update is called once per frame
    void Update()
    {
        int port = APIManager.Instance.Port;
        this._dataStats.SetStatistics(APIManager.Instance.DataEndpoint.Stats);
        this._eventStats.SetStatistics(APIManager.Instance.EventsEndpoint.Stats);
        this._inputStats.SetStatistics(APIManager.Instance.InputEndpoint.Stats);
    }

    public int ValidatePortValue(string value){
        int port = MathUtils.StringToInt(value);
        if (port <= 0 || port > 65535){
            port = 8080;
        }
        this._portInput.text = port + "";
        APIManager.Instance.SetPort(port);
        return port;
    }

    public void RestartServer(){
        int port = ValidatePortValue(this._portInput.text);
        APIManager.Instance.StartOnPort(port, this._status.SetStatus);
    }

    public void StopServer(){
        APIManager.Instance.Stop(this._status.SetStatus);
    }
}
