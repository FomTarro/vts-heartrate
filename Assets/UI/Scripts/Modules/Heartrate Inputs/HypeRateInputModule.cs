using UnityEngine;
using TMPro;

public class HypeRateInputModule : HeartrateInputModule
{
    [SerializeField]
    private StatusIndicator _status = null;
    [SerializeField]
    private TMP_InputField _input = null;

    private void Start(){
        this._input.onEndEdit.AddListener(SetAuthToken);
    }

    public override int GetHeartrate()
    {
        return HypeRateManager.Instance.Heartrate;
    }

    protected override void FromValues(SaveData.Values values)
    {
        this._input.text = values.authToken;
        SetAuthToken(values.authToken);
    }

    public void SetAuthToken(string token){
        HypeRateManager.Instance.SetHyperateID(token);
    }

    public void Connect(){
        HypeRateManager.Instance.Connect(this._status.SetStatus);
    }

    public void Disconnect(){
        HypeRateManager.Instance.Disconnect(this._status.SetStatus);
    }

    protected override void OnStatusChange(bool isActive)
    {
        if(isActive){
            Connect();
        }else{
            Disconnect();
        }
    }

    protected override SaveData.Values ToValues()
    {
       SaveData.Values data = new SaveData.Values();
       data.authToken = HypeRateManager.Instance.HypeRateID;
       return data;
    }
}
