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
}
