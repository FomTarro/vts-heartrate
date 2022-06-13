using UnityEngine;
using TMPro;

public class APIServerSettings : MonoBehaviour
{
    [SerializeField]
    private StatusIndicator _status = null;

    [SerializeField]
    private TMP_InputField _portInput = null;

    [SerializeField]
    private TMP_Text _connectionCount = null;

    // Start is called before the first frame update
    void Start()
    {
        this._portInput.onEndEdit.AddListener((v) => { ValidatePortValue(v); });
        ValidatePortValue(APIManager.Instance.Port + "");
    }

    // Update is called once per frame
    void Update()
    {
        this._connectionCount.text = APIManager.Instance.TotalClientCount + "";
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
}
