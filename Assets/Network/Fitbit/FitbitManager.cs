using UnityEngine;
using System.Collections.Generic;

public class FitbitManager : Singleton<FitbitManager>{

    private int _appHeartrate = 0;
    public int AppHeartrate { get { return this._appHeartrate; } }
    [SerializeField]
    private WebServer _server = null;

    [SerializeField]
    private List<FitBitModelSDKMap> _modelsToSDK = new List<FitBitModelSDKMap>();
    public List<FitBitModelSDKMap> ModelsToSDKMap { get { return this._modelsToSDK; } }

    private string _appGalleryUrl = "https://www.skeletom.net";
    public string AppGalleryURL { get { return this._appGalleryUrl; } }

	public override void Initialize() {
       SetPort(9000);
	}

    public void SetPort(int port){
        Debug.LogFormat("FitBit Server started on port {0}.", port);
        this._server.SetPort(port);
    }

    public void SetSDK(FitbitManager.FitbitSDKVersion sdk){
        switch (sdk){
            case FitbitManager.FitbitSDKVersion.SDK_4_3:
                this._appGalleryUrl = "https://www.skeletom.net/stream";
                break;
            case FitbitManager.FitbitSDKVersion.SDK_6_1:
                this._appGalleryUrl = "https://www.fitbit.com/";
            break;
        }
    }

    public void ReceiveData(string body){
        FitBitResult result = JsonUtility.FromJson<FitBitResult>(body);
        this._appHeartrate = result.value;
        this._timeout = 2f;
    }

    float _timeout = 2f;
    public bool IsConnected { get { return this._timeout > 0f; } }
    private void Update(){
        this._timeout = this._timeout - Time.deltaTime;
    }

    [System.Serializable]
    public class FitBitResult {
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
    public struct FitBitModelSDKMap{
        public string key;
        public FitbitModel model;
        public FitbitSDKVersion sdk;
    }
}
