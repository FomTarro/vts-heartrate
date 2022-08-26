using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StatusIndicator : MonoBehaviour
{
    [SerializeField]
    private string _prefix = "Status";
    [SerializeField]
    private Image _indicatorImage = null;
    [SerializeField]
    private TMP_Text _statusText = null;

    [SerializeField]
    private Color32 _connectedColor = Color.green;
    [SerializeField]
    private Color32 _connectingColor = Color.yellow;
    [SerializeField]
    private Color32 _errorColor = Color.red;
    [SerializeField]
    private Color32 _disconnectedColor = Color.grey;
    
    private void Awake(){
        HttpUtils.ConnectionStatus status = new HttpUtils.ConnectionStatus();
        status.status = HttpUtils.ConnectionStatus.Status.DISCONNECTED;
        SetStatus(status);
    }

    public void SetStatus(HttpUtils.ConnectionStatus status){
        switch(status.status){
            case HttpUtils.ConnectionStatus.Status.DISCONNECTED:
                this._indicatorImage.color = this._disconnectedColor;
                break;
            case HttpUtils.ConnectionStatus.Status.ERROR:
                this._indicatorImage.color = this._errorColor;
                break;
            case HttpUtils.ConnectionStatus.Status.CONNECTING:
                this._indicatorImage.color = this._connectingColor;
                break;
            case HttpUtils.ConnectionStatus.Status.CONNECTED:
                this._indicatorImage.color = this._connectedColor;
                break;
        }

        this._statusText.text = string.Format("{0}: {1}", this._prefix, status.message != null ? status.message : status.status.ToString().ToLower());
    }
}
