using TMPro;
using UnityEngine;

public class FitbitInputModule : HeartrateInputModule
{

	[SerializeField]
	private StatusIndicator _statusIndicator = null;
	private HttpUtils.ConnectionStatus _status = new HttpUtils.ConnectionStatus();

	[SerializeField]
	private TMP_InputField _localIPField = null;
	[SerializeField]
	private TMP_InputField _localPortField = null;

	private void Start()
	{
		this._localPortField.onEndEdit.AddListener(SetPort);
	}

	public void SetPort(string port)
	{
		int portAsInt = MathUtils.StringToInt(port);
		if (portAsInt <= 0)
		{
			SetPort(FitbitManager.Instance.Port);
		}
		else
		{
			SetPort(portAsInt);
		}
	}

	private void SetPort(int port)
	{
		FitbitManager.Instance.StartOnPort(port, this._statusIndicator.SetStatus);
		this._localPortField.text = string.Format("{0}", FitbitManager.Instance.Port);
		this._localIPField.text = string.Format("{0}", FitbitManager.Instance.LocalIP);
	}

	public override int GetHeartrate()
	{
		return FitbitManager.Instance.Heartrate;
	}

	protected override void FromValues(SaveData.Values values)
	{
		SetPort(values.port);
		FitbitManager.Instance.SetSelectedModel((FitbitManager.FitbitModel)(int)values.value);
	}

	protected override void OnStatusChange(bool isActive)
	{
		if (isActive)
		{
			SetPort(FitbitManager.Instance.Port);
			UIManager.Instance.ShowPopUp(
				"input_fitbit_retiring_title",
				"input_fitbit_retiring_tooltip",
				new PopUp.PopUpOption("input_fitbit_retiring_button_visit",
				ColorUtils.ColorPreset.BLUE,
				() =>
				{
					Application.OpenURL("https://blog.pulsoid.net/post/pulsoid-x-fitbit");
				})
			);
		}
		else
		{
			FitbitManager.Instance.Stop();
		}
	}

	protected override SaveData.Values ToValues()
	{
		SaveData.Values values = new SaveData.Values();
		values.port = FitbitManager.Instance.Port;
		values.value = (float)FitbitManager.Instance.SelectedModel;
		return values;
	}
}
