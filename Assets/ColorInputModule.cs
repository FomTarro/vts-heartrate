using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ColorInputModule : MonoBehaviour
{

    [SerializeField]
    private Color32 _color = Color.white;
    public Color32 ModuleColor { get { return this._color; } }
    private string[] _matchers = new string[0];
    public String[] ModuleMatchers { get { return this._matchers; } }

    [SerializeField]
    private InputField _redField = null;
    [SerializeField]
    private InputField _greenField = null;
    [SerializeField]
    private InputField _blueField = null;
    [SerializeField]
    private InputField _alphaField = null;
    [SerializeField]
    private InputField _matchersField = null;

    public void SetRed(string value){
        this._color = new Color32(
            StringToByte(value), 
            this._color.g, 
            this._color.b, 
            this._color.a);
    }

    public void SetGreen(string value){
        this._color = new Color32(
            this._color.r, 
            StringToByte(value), 
            this._color.b,
            this._color.a);
    }

    public void SetBlue(string value){
        this._color = new Color32(
            this._color.r, 
            this._color.g, 
            StringToByte(value), 
            this._color.a);
    }
    public void SetAlpha(string value){
        this._color = new Color32(
            this._color.r, 
            this._color.g, 
            this._color.b,
            StringToByte(value));
    }
    public void SetMatchers(string value){
        string[] sanitized = value.Trim().Split(' ', ',');
        this._matchers = sanitized;
    }


    private byte StringToByte(string value){
        try{
            return Convert.ToByte(value);
        }catch(Exception e){
            Debug.LogWarning(e);
            return 0;
        }
    }

    [System.Serializable]
    public class SaveData{
        public Color32 color;
        public string[] matchers;

        public override string ToString()
        {
            return JsonUtility.ToJson(this);
        }
    }

    public SaveData ToSaveData(){
        SaveData data = new SaveData();
        data.color = this._color;
        data.matchers = this._matchers;

        return data;
    }

    public void FromSaveData(SaveData data){
        this._color = data.color;
        this._matchers = data.matchers;

        this._redField.text = this._color.r.ToString();
        this._greenField.text = this._color.g.ToString();
        this._blueField.text = this._color.b.ToString();
        this._alphaField.text = this._color.a.ToString();
        this._matchersField.text = String.Join(",", this._matchers);
    }
}
