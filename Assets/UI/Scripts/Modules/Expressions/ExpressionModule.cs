using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ExpressionModule : MonoBehaviour
{
    [SerializeField]
    private InputField _threshold = null;
    public int Threshold { get { return this.StringToByte(this._threshold.text); }}
    [SerializeField]
    private Toggle _activate = null;
    public bool ShouldActivate { get { return this._activate.isOn; } }
    [SerializeField]
    private Dropdown _dropdown = null;
    public string SelectedExpression { get { 
        return this._waitingOn == null ? 
        HeartrateManager.Instance.Plugin.Expressions[this._dropdown.value] : 
        this._waitingOn; 
    }}

    private int ExpressionToIndex(string expressionFile){
        return this._dropdown.options.FindIndex((o) 
            => { return o.text.Equals(expressionFile); });
    }

    private void SetExpression(string expressionFile){
        int index = ExpressionToIndex(expressionFile);
        if(index < 0){
            this._waitingOn = expressionFile;
        }
        else if(this._dropdown.options.Count > 0){
            this._dropdown.SetValueWithoutNotify(index);
        }
    }

    private string _waitingOn = null;

    public void RefreshExpressionList(){
        int currentIndex = this._dropdown.value;
        string expressionFile = this._dropdown.options.Count > 0 ? 
                                this._dropdown.options[currentIndex].text : 
                                null;
        this._dropdown.ClearOptions();
        this._dropdown.AddOptions(HeartrateManager.Instance.Plugin.Expressions);
        this._dropdown.RefreshShownValue();
        if(this._waitingOn != null){
            SetExpression(this._waitingOn);
            this._waitingOn = null;
        }else{
            SetExpression(expressionFile);
        }
    }

    [System.Serializable]
    public class SaveData {

        public string expressionFile;
        public int threshold;
        public bool shouldActivate;

        public override string ToString()
        {
            return JsonUtility.ToJson(this);
        }
    }

    public SaveData ToSaveData(){
        SaveData data = new SaveData();
        data.threshold = this.Threshold;
        data.shouldActivate = this.ShouldActivate;
        data.expressionFile = this.SelectedExpression;
        return data;
    }

    public void FromSaveData(SaveData data){
        this._threshold.text = data.threshold.ToString();
        this._activate.isOn = data.shouldActivate;
        SetExpression(data.expressionFile);
    }

    private byte StringToByte(string value){
        try{
            return Convert.ToByte(value);
        }catch(Exception e){
            Debug.LogWarning(e);
            return 0;
        }
    }
}
