using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class APIEndpointStatistics : MonoBehaviour
{

    [SerializeField]
    private TMP_Text _connections = null;
    [SerializeField]
    private TMP_Text _url = null;
    [SerializeField]
    private TMP_Text _messages = null;
    [SerializeField]
    private string _messageKey = "settings_api_server_messages_sent";

    public void SetStatistics(int connections, string url, long messageCount){
        this._connections.text = string.Format(
            Localization.LocalizationManager.Instance.GetString("settings_api_server_connection_count"),
            connections);
        this._url.text = url;
        this._messages.text = string.Format(
            Localization.LocalizationManager.Instance.GetString(this._messageKey),
            messageCount);
    }
}
