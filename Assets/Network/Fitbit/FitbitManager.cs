using System.Collections.Generic;
using UnityEngine;

public class FitbitManager : Singleton<FitbitManager> {

	private int _appHeartrate = 0;
	public int AppHeartrate { get { return this._appHeartrate; } }
	[SerializeField]
	private WebServer _server = null;

	[SerializeField]
	private List<FitbitModelSDKMap> _modelsToSDK = new List<FitbitModelSDKMap>();
	public List<FitbitModelSDKMap> ModelsToSDKMap { get { return this._modelsToSDK; } }

	private string _appGalleryUrl = "https://www.skeletom.net";
	public string AppGalleryURL { get { return this._appGalleryUrl; } }
	float _timeout = 2f;
	public bool IsConnected { get { return this._timeout > 0f; } }

    public string LocalIP { get { return string.Format("{0}:{1}", HttpUtils.GetLocalIPAddress(), this._server.Port); } }
	public override void Initialize() {
		// SetPort(APIManager.Instance.Port);
		SetPort(9000);
	}

	public void SetPort(int port) {
		Debug.LogFormat("FitBit Server started on port {0}.", port);
		this._server.SetPort(port);
	}

	public void SetSDK(FitbitSDKVersion sdk) {
		switch (sdk) {
			case FitbitManager.FitbitSDKVersion.SDK_4_3:
				this._appGalleryUrl = "https://www.skeletom.net/stream";
				break;
			case FitbitManager.FitbitSDKVersion.SDK_6_1:
				this._appGalleryUrl = "https://www.fitbit.com/";
				break;
		}
	}

	public void ReceiveData(string body) {
		FitbitResult result = JsonUtility.FromJson<FitbitResult>(body);
		this._appHeartrate = result.value;
		this._timeout = 2f;
	}

	private void Update() {
		this._timeout = this._timeout - Time.deltaTime;
	}

	[System.Serializable]
	public class FitbitResult {
		public int value;
	}

	public enum FitbitModel {
		VERSA_3,
		SENSE,
		IONIC,
		VERSA_2,
		VERSA_LITE,
		VERSA
	}

	public enum FitbitSDKVersion {
		SDK_6_1,
		SDK_4_3,
	}

	[System.Serializable]
	public struct FitbitModelSDKMap {
		public string key;
		public FitbitModel model;
		public FitbitSDKVersion sdk;
	}
}
