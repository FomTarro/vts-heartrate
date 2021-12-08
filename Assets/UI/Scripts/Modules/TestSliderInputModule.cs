using UnityEngine;
using UnityEngine.UI;

public class TestSliderInputModule : HeartrateInputModule
{
    [SerializeField]
    private Slider _slider = null;

    public override int GetHeartrate()
    {
        return (int)this._slider.value;
    }

    protected override void FromValues(SaveData.Values values)
    {
        this._slider.value = values.value;
    }

    protected override SaveData.Values ToValues()
    {
        SaveData.Values values = new SaveData.Values();
        values.value = this._slider.value;
        return values;
    }
}
