public class WebSocketInput : HeartrateInputModule
{
    public override int GetHeartrate()
    {
       return APIManager.Instance.Heartrate;
    }

    protected override void FromValues(SaveData.Values values)
    {

    }

    protected override void OnStatusChange(bool isActive)
    {

    }

    protected override SaveData.Values ToValues()
    {
        return new SaveData.Values();
    }
}
