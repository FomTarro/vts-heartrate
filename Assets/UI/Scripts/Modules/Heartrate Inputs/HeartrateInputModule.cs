using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class HeartrateInputModule : MonoBehaviour
{
    [SerializeField]
    private InputType _type;
    public InputType Type { get { return this._type; } }

    [SerializeField]
    private Toggle _toggle = null;

    public bool IsActive { get { return this._toggle.isOn; } } 

    [SerializeField]
    private Text _label = null;

    public void Start(){
        this._toggle.onValueChanged.AddListener(SetStatus);
    }

    private void OnValidate(){
        this._toggle = GetComponentInChildren<Toggle>();
    }

    /// <summary>
    /// Sets the active status of the input module. 
    /// </summary>
    /// <param name="value"></param>
    public void SetStatus(bool value){
        if(value){
            Activate();
        }else{
            Deactivate();
        }
    }

    protected void Activate(){
        HeartrateManager.Instance.Plugin.SetActiveHeartrateInput(this);
        this._toggle.isOn = true;
        OnStatusChange(true);
    }

    protected void Deactivate(){
        this._toggle.isOn = false;
        OnStatusChange(false);
    }

    protected abstract void OnStatusChange(bool isActive);

    public abstract int GetHeartrate();

    [System.Serializable]
    public enum InputType : int {
        SLIDER = 1,
        FILE = 2,
        // BLUETOOTH_DEVICE = 3,
        PULSOID = 4,
        PULSOID_RSS = 5
    }

    [System.Serializable]
    public class SaveData{
        public bool isActive = false;
        public InputType type;
        public Values values = new Values();

        public override string ToString()
        {
            return JsonUtility.ToJson(this);
        }

        [System.Serializable]
        public class Values { 
            public float value;
            public string path;
            public string authToken;

            public override string ToString()
            {
                return JsonUtility.ToJson(this);
            }
        }
    }

    protected abstract SaveData.Values ToValues();
    protected abstract void FromValues(SaveData.Values values);

    public SaveData ToSaveData(){
        SaveData data = new SaveData();
        data.isActive = this.IsActive;
        data.type = this.Type;
        data.values = ToValues();
        return data;
    }

    public void FromSaveData(SaveData data){
        FromValues(data.values);
        SetStatus(data.isActive);
    }

    public override string ToString()
    {
        return this._label.text;
    }
}
