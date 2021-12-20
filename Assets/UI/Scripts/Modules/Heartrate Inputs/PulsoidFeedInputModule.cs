using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PulsoidFeedInputModule : HeartrateInputModule
{
    [SerializeField]
    private InputField _input = null;

    [SerializeField]
    private StatusIndicator _status = null;

    public void SetURL(string url){
        PulsoidManager.Instance.SetFeedURL(url);
    }

    public override int GetHeartrate()
    {
        return PulsoidManager.Instance.FeedHeartrate;
    }

    protected override void FromValues(SaveData.Values values)
    {
        this._input.text = values.path;
        SetURL(values.path);
    }

    protected override SaveData.Values ToValues()
    {
        SaveData.Values values = new SaveData.Values();
        values.path = this._input.text;
        return values;
    }

    protected override void OnStatusChange(bool isActive)
    {
        PulsoidManager.Instance.ToggleFeedRequestLoop(isActive, this._status.SetStatus);
    }
}
