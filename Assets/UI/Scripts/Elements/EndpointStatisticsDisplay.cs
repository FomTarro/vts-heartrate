using TMPro;
using UnityEngine;

public class EndpointStatisticsDisplay : MonoBehaviour {

	[SerializeField]
	private TMP_Text _connections = null;
	[SerializeField]
	private TMP_Text _url = null;
	[SerializeField]
	private TMP_Text _messagesIn = null;
	[SerializeField]
	private TMP_Text _messagesOut = null;

	public void SetStatistics(WebSocketServer.EndpointStatistics stats){
	    this._connections.text = string.Format(
	        Localization.LocalizationManager.Instance.GetString("settings_api_server_connection_count"),
	        stats.ActiveConnections);
	    this._url.text = string.Format("ws://localhost:{0}{1}", APIManager.Instance.Port, stats.Endpoint.Path);
		this._messagesIn.text = string.Format(
	        Localization.LocalizationManager.Instance.GetString("settings_api_server_messages_received"),
	        stats.MessagesIn);
	    this._messagesOut.text = string.Format(
	        Localization.LocalizationManager.Instance.GetString("settings_api_server_messages_sent"),
	        stats.MessagesOut);
	}
}
