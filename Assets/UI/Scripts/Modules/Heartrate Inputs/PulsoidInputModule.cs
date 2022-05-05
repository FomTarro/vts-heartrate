using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PulsoidInputModule : HeartrateInputModule
{
    [SerializeField]
    private TMP_InputField _input = null;

    [SerializeField]
    private StatusIndicator _status = null;

    public void Login(){
        PulsoidManager.Instance.Login();
    }

    public void SetAuthToken(string token){
        PulsoidManager.Instance.SetAuthToken(token);
    }

    public override int GetHeartrate()
    {
        return PulsoidManager.Instance.AppHeartrate;
    }

    protected override void FromValues(SaveData.Values values)
    {
        this._input.text = values.authToken;
        SetAuthToken(values.authToken);
    }

    protected override SaveData.Values ToValues()
    {
        SaveData.Values values = new SaveData.Values();
        values.authToken = this._input.text;
        return values;
    }

    protected override void OnStatusChange(bool isActive)
    {
        PulsoidManager.Instance.ToggleAppRequestLoop(isActive, this._status.SetStatus);
    }
}
