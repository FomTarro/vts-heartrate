using UnityEngine;
using UnityEngine.UI;

public class PulsoidInputModule : HeartrateInputModule
{
    public override int GetHeartrate()
    {
        return 0;
    }

    protected override void FromValues(SaveData.Values values)
    {
        
    }

    protected override SaveData.Values ToValues()
    {
        SaveData.Values values = new SaveData.Values();
        return values;
    }

    public void Login(){
        PulsoidManager.Instance.Login();
    }
}
