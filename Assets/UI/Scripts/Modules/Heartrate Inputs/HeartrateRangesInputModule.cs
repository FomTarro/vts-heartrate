using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HeartrateRangesInputModule : MonoBehaviour
{
    [SerializeField]
    private InputField _minField = null;
    [SerializeField]
    private InputField _maxField = null;

    
    void Start()
    {
        this._minField.onEndEdit.AddListener(SetMinRate);
        this._maxField.onEndEdit.AddListener(SetMaxRate);
    }

    public void SetMinRate(string s){
        int rate = RateToInt(s);
        HeartrateManager.Instance.Plugin.SetMinRate(rate);
        this._minField.text = rate.ToString();
    }

    public void SetMaxRate(string s){
        int rate = RateToInt(s);
        HeartrateManager.Instance.Plugin.SetMaxRate(rate);
        this._maxField.text = rate.ToString();
    }

    private int RateToInt(string rate){
        try{
            return int.Parse(rate);
        }catch(System.Exception e){
            Debug.LogWarning(e);
        }
        return 0;
    }
}
