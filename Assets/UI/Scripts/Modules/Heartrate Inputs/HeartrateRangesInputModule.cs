using UnityEngine;
using TMPro;

public class HeartrateRangesInputModule : MonoBehaviour
{
    [SerializeField]
    private TMP_InputField _minField = null;
    [SerializeField]
    private TMP_InputField _maxField = null;

    [SerializeField]
    private TMP_InputField _offsetField = null;


    void Start()
    {
        this._minField.onEndEdit.AddListener(SetMinRate);
        this._maxField.onEndEdit.AddListener(SetMaxRate);
        this._offsetField.onEndEdit.AddListener(SetOffset);
    }

    public void SetMinRate(string s)
    {
        int rate = Mathf.Clamp(MathUtils.StringToInt(s), 0, 255);
        HeartrateManager.Instance.Plugin.SetMinRate(rate);
        this._minField.text = rate.ToString();
    }

    public void SetMaxRate(string s)
    {
        int rate = Mathf.Clamp(MathUtils.StringToInt(s), 0, 255);
        HeartrateManager.Instance.Plugin.SetMaxRate(rate);
        this._maxField.text = rate.ToString();
    }

    public void SetOffset(string s)
    {
        int rate = Mathf.Clamp(MathUtils.StringToInt(s), -255, 255);
        HeartrateManager.Instance.Plugin.SetOffset(rate);
        this._offsetField.text = rate.ToString();
    }
}
