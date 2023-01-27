using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class APIEndpointStatisticsDisplay : MonoBehaviour {

	[SerializeField]
	private TMP_Text _connections = null;
	[SerializeField]
	private TMP_Text _url = null;
	[SerializeField]
	private TMP_Text _messages = null;
	[SerializeField]
	private string _messageKey = "settings_api_server_messages_sent";

	// public void SetStatistics(APIManager.APIEndpoint.Statistics stats){
	//     this._connections.text = string.Format(
	//         Localization.LocalizationManager.Instance.GetString("settings_api_server_connection_count"),
	//         stats.clients);
	//     this._url.text = stats.url;
	//     this._messages.text = string.Format(
	//         Localization.LocalizationManager.Instance.GetString(this._messageKey),
	//         stats.messages);
	// }
}
