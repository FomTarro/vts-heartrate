using UnityEngine;
using UnityEngine.UI;

public class VTSConnectionStatus : MonoBehaviour
{
    [SerializeField]
    private Text _connectionText = null;
    public void SetStatus(ConnectionStatus status){
        if(this._connectionText != null){
            this._connectionText.text = status.ToString();
        }
    }

    public enum ConnectionStatus{
        CONNECTING = 0,
        CONNECTED = 1,
        ERROR = 2,
        DISCONNECTED = 3,
    }
}
