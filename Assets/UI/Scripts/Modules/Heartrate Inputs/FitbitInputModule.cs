using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class FitbitInputModule : HeartrateInputModule {

    [SerializeField]
    private StatusIndicator _statusIndicator = null;
    private HttpUtils.ConnectionStatus _status = new HttpUtils.ConnectionStatus();

    [SerializeField]
    private TMP_InputField _localIPField = null;

    private void Update(){
        if(FitbitManager.Instance.IsConnected){
            this._status.status = HttpUtils.ConnectionStatus.Status.CONNECTED;
        }else{
            this._status.status = HttpUtils.ConnectionStatus.Status.DISCONNECTED;
        }
        this._statusIndicator.SetStatus(this._status);
        this._localIPField.text = FitbitManager.Instance.LocalIP;
    }

	public override int GetHeartrate() {
		return FitbitManager.Instance.AppHeartrate;
	}

	protected override void FromValues(SaveData.Values values) {
		
	}

	protected override void OnStatusChange(bool isActive) {
		
	}

	protected override SaveData.Values ToValues() {
		SaveData.Values values = new SaveData.Values();
        values.port = 9000;
        return values;
	}

    public void GoToAppGallery(){
        Application.OpenURL(FitbitManager.Instance.AppGalleryURL);
    }
}
