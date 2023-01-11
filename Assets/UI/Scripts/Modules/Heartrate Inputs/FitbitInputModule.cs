using TMPro;
using UnityEngine;

public class FitbitInputModule : HeartrateInputModule {

	[SerializeField]
	private StatusIndicator _statusIndicator = null;
	private HttpUtils.ConnectionStatus _status = new HttpUtils.ConnectionStatus();

	[SerializeField]
	private TMP_InputField _localIPField = null;
	[SerializeField]
	private TMP_InputField _localPortField = null;
	[SerializeField]
	private FitbitModelDropdown _modelDropdown = null;

	private void Update() {
		if (FitbitManager.Instance.IsConnected) {
			this._status.status = HttpUtils.ConnectionStatus.Status.CONNECTED;
		}
		else {
			this._status.status = HttpUtils.ConnectionStatus.Status.DISCONNECTED;
		}
		this._statusIndicator.SetStatus(this._status);
		this._localIPField.text = FitbitManager.Instance.LocalIP;
	}

	public override int GetHeartrate() {
		return FitbitManager.Instance.Heartrate;
	}

	protected override void FromValues(SaveData.Values values) {
		FitbitManager.Instance.SetPort(values.port);
        FitbitManager.Instance.SetSelectedModel((FitbitManager.FitbitModel)(int)values.value);
		this._localPortField.text = string.Format("{0}", values.port);
	}

	protected override void OnStatusChange(bool isActive) {

	}

	protected override SaveData.Values ToValues() {
		SaveData.Values values = new SaveData.Values();
		values.port = 9000;
        values.value = (float)FitbitManager.Instance.SelectedModel;
		return values;
	}
}
