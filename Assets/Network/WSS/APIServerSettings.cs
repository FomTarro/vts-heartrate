using System.Collections.Generic;
using UnityEngine;
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
    private RectTransform _authList = null;
    [SerializeField]
    private PluginAuthEntry _authPrefab = null;
    private Dictionary<string, PluginAuthEntry> _pluginAuthEntries = new Dictionary<string, PluginAuthEntry>();

    // Start is called before the first frame update
    void Start()
    {
        this._portInput.onEndEdit.AddListener((v) => { ValidatePortValue(v); });
        ValidatePortValue(APIManager.Instance.Port + "");
        RestartServer();
    }

    // Update is called once per frame
    private void Update()
    {
        foreach(APIManager.PluginData plugin in APIManager.Instance.ApprovedPlugins){
            if(!this._pluginAuthEntries.ContainsKey(plugin.token)){
                PluginAuthEntry entry = Instantiate<PluginAuthEntry>(this._authPrefab, 
                    Vector3.zero, 
                    Quaternion.identity, 
                    this._authList);
                entry.Configure(plugin);
                entry.gameObject.SetActive(true);
                this._pluginAuthEntries.Add(plugin.token, entry);
            }
        }
        // List<string> tokens = new List<string>(this._pluginAuthEntries.Keys);
        // foreach(string token in tokens){
        //     if(this._pluginAuthEntries[token] == null){
        //         this._pluginAuthEntries.Remove(token);
        //     }
        // }
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
