using UnityEngine;
using UnityEngine.UI;

public class ExpressionModule : MonoBehaviour
{
    [SerializeField]
    private InputField _threshold = null;
    public int Threshold { get { return MathUtils.StringToByte(this._threshold.text); }}

    // TODO: this shouldn't be public, but it's probably the easiest way 
    public int PriorThreshold = 0;

    [SerializeField]
    private Toggle _activate = null;
    public bool ShouldActivate { get { return this._activate.isOn; } }
    [SerializeField]
    private Dropdown _dropdown = null;
    public string SelectedExpression { get { 
        return this._waitingOn == null ? 
        (this._dropdown.value < HeartrateManager.Instance.Plugin.Expressions.Count ? 
            HeartrateManager.Instance.Plugin.Expressions[this._dropdown.value] : null) :
        this._waitingOn; 
    }}

    public void Clone(){
        HeartrateManager.Instance.Plugin.CreateExpressionModule(this.ToSaveData());
    }

    public void Delete(){
        HeartrateManager.Instance.Plugin.DestroyExpressionModule(this);
    }

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

    // Because the dropdown is populated by an async method, 
    // we load the expression that should be selected from a profile load into this buffer
    // until the async method resolves.
    private string _waitingOn = null;

    // TODO: consolidate this behavior into RefreshableDropdown
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
        public bool shouldActivate = true;

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
