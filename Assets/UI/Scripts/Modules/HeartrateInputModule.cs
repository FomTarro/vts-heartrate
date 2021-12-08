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

    public void Start(){
        this._toggle.onValueChanged.AddListener(OnToggle);
    }

    public void OnToggle(bool value){
        if(value){
            Activate();
        }
    }

    public void Activate(){
        HeartrateManager.Instance.Plugin.SetActiveHeartrateInput(this);
    }

    public void Deactivate(){
        this._toggle.isOn = false;
    }

    public abstract int GetHeartrate();

    [System.Serializable]
    public enum InputType : int {
        SLIDER = 1,
        FILE = 2,
        DEVICE = 3,
        PULSOID = 4,
        WEB = 5
    }

    [System.Serializable]
    public class SaveData{
        public bool isActive = false;
        public InputType type;
        public Values values;

        public override string ToString()
        {
            return JsonUtility.ToJson(this);
        }

        [System.Serializable]
        public class Values { 
            public float value;
            public string path;

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
        if(data.isActive){
            this._toggle.isOn = true;
        }
        FromValues(data.values);
    }
}
