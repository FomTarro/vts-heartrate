using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ExpressionModule : MonoBehaviour
{
    [SerializeField]
    private InputField _threshold = null;
    // TODO: MAKE THIS SAFE LATER
    public int Threshold { get { return int.Parse(this._threshold.text); }}
    [SerializeField]
    private Toggle _activate = null;
    public bool ShouldActivate { get { return this._activate.isOn; } }
    [SerializeField]
    private Dropdown _dropdown = null;
    public string SelectedExpression { get { 
        return HeartrateManager.Instance.Plugin.Expressions[this._dropdown.value]; 
    }}

    private int ExpressionToIndex(string expressionFile){
        return Mathf.Max(0, this._dropdown.options.FindIndex((o) 
            => { return o.text.Equals(expressionFile); }));
    }

    private void SetExpression(string expressionFile){
        if(this._dropdown.options.Count > 0){
            this._dropdown.SetValueWithoutNotify(ExpressionToIndex(expressionFile));
        }
    }

    public void RefreshExpressionList(){
        int currentIndex = this._dropdown.value;
        string expressionFile = this._dropdown.options.Count > 0 ? 
                                this._dropdown.options[currentIndex].text : 
                                null;
        this._dropdown.ClearOptions();
        this._dropdown.AddOptions(HeartrateManager.Instance.Plugin.Expressions);
        this._dropdown.RefreshShownValue();
        SetExpression(expressionFile);
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
}
