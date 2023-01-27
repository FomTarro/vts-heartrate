using UnityEngine;

public class WebSocketInput : HeartrateInputModule {
    
	[SerializeField]
	private EndpointStatisticsDisplay _inputStats = null;

	private void Update() {
		this._inputStats.SetStatistics(APIManager.Instance.GetInputStatistics());
	}

	public override int GetHeartrate() {
		return APIManager.Instance.Heartrate;
	}

	protected override void FromValues(SaveData.Values values) {

	}

	protected override void OnStatusChange(bool isActive) {

	}

	protected override SaveData.Values ToValues() {
		return new SaveData.Values();
	}
}
