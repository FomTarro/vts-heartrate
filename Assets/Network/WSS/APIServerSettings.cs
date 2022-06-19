using UnityEngine;
using TMPro;

public class APIServerSettings : MonoBehaviour
{
    [SerializeField]
    private StatusIndicator _status = null;

    [SerializeField]
    private TMP_InputField _portInput = null;

    [SerializeField]
    private APIEndpointStatistics _dataStats = null;
    [SerializeField]
    private APIEndpointStatistics _eventStats = null;
    [SerializeField]
    private APIEndpointStatistics _inputStats = null;

    [SerializeField]
    private TMP_Text _wsUrl = null;

    private const string SERVER_URL = "ws://localhost:{0}/{1}";

    // Start is called before the first frame update
    void Start()
    {
        this._portInput.onEndEdit.AddListener((v) => { ValidatePortValue(v); });
        ValidatePortValue(APIManager.Instance.Port + "");
    }

    // Update is called once per frame
    void Update()
    {
        int port = APIManager.Instance.Port;
        this._dataStats.SetStatistics(
            APIManager.Instance.DataClientCount, 
            string.Format(SERVER_URL, port, "data"),
            APIManager.Instance.DataMessages);
        this._eventStats.SetStatistics(
            APIManager.Instance.EventClientCount, 
            string.Format(SERVER_URL, port, "events"),
            APIManager.Instance.EventMessages);
        this._inputStats.SetStatistics(
            APIManager.Instance.InputClientCount, 
            string.Format(SERVER_URL, port, "input"),
            APIManager.Instance.InputMessages);
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
