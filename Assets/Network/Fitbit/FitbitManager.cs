using System.Collections.Generic;
using UnityEngine;

public class FitbitManager : Singleton<FitbitManager> {

	private int _heartrate = 0;
	public int Heartrate { get { return this._heartrate; } }

	[SerializeField]
	private HttpServer _server = null;
	public int Port { get { return this._server.Port; } }

	[SerializeField]
	private List<FitbitModelSDKMap> _modelsToSDK = new List<FitbitModelSDKMap>();
	public List<FitbitModelSDKMap> ModelsToSDKMap { get { return this._modelsToSDK; } }

	private FitbitModel _selectedModel = FitbitModel.VERSA_3;
	public FitbitModel SelectedModel { get { return this._selectedModel; } }

	private const string GALLERY_URL = "https://gallery.fitbit.com/details/{0}";
	private const string GALLERY_SDK6_1_UUID = "e9a96623-7520-4f74-8aa4-9bae38702d54";
	private const string GALLERY_SDK4_3_UUID = "e9a96623-7520-4f74-8aa4-9bae38702d54";
	private const int QR_PIXELS_PER_MODULE = 20;
	private static QRCodeGenerator QR_GENERATOR = new QRCodeGenerator();

	float _timeout = 2f;
	public bool IsConnected { get { return this._timeout > 0f; } }
	public string LocalIP { get { return string.Format("{0}:{1}", HttpUtils.GetLocalIPAddress(), this._server.Port); } }

	public override void Initialize() {

	}

	public void SetPort(int port) {
		try {
			this._server.SetPort(port);
			this._server.StartServer();
			Debug.LogFormat("Fitbit Server started on port {0}.", port);
		}
		catch (System.Exception e) {
			// TODO: let the user know if the server couldn't be started.
		}
	}

	public void SetSelectedModel(FitbitModel model){
		this._selectedModel = model;
	}

	public Texture2D GetGalleryLinkForSDK(FitbitSDKVersion sdk) {
		string toEncode = "";
		switch (sdk) {
			case FitbitManager.FitbitSDKVersion.SDK_4_3:
				toEncode = string.Format(GALLERY_URL, GALLERY_SDK4_3_UUID);
				break;
			case FitbitManager.FitbitSDKVersion.SDK_6_1:
				toEncode = string.Format(GALLERY_URL, GALLERY_SDK6_1_UUID);
				break;
		}
		return EncodeString(
			toEncode, 
			Color.black, 
			Color.white);
	}

	public void ReceiveData(string body) {
		FitbitResult result = JsonUtility.FromJson<FitbitResult>(body);
		this._heartrate = result.value;
		this._timeout = 2f;
	}

	private void Update() {
		this._timeout = this._timeout - Time.deltaTime;
	}

	/// <summary>
	/// Encode text in to a QR Code and define the colors.
	/// </summary>
	/// <param name="text"></param>
	/// <param name="darkColor"></param>
	/// <param name="lightColor"></param>
	/// <returns></returns>
	private static Texture2D EncodeString(string text, Color darkColor, Color lightColor) {
		QRCodeGenerator.QRCode qrCode = QR_GENERATOR.CreateQrCode(text, QRCodeGenerator.ECCLevel.L);
		Texture2D qrTexture = qrCode.GetGraphic(QR_PIXELS_PER_MODULE, darkColor, lightColor);
		return qrTexture;
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
