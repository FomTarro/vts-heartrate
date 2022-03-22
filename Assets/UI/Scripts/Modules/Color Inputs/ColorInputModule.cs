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

    [SerializeField]
    private Image _background = null;

    private void Start(){
        this._redField.onEndEdit.AddListener(SetRed);
        this._greenField.onEndEdit.AddListener(SetGreen);
        this._blueField.onEndEdit.AddListener(SetBlue);
        this._alphaField.onEndEdit.AddListener(SetAlpha);
        this._matchersField.onEndEdit.AddListener(SetMatchers);
    }

    public void Clone(){
        HeartrateManager.Instance.Plugin.CreateColorInputModule(this.ToSaveData());
    }

    public void Delete(){
        HeartrateManager.Instance.Plugin.DestroyColorInputModule(this);
    }

    public void SetRed(string value){
        byte v = MathUtils.StringToByte(value);
        this._color = new Color32(
            v, 
            this._color.g, 
            this._color.b, 
            this._color.a);
        this._redField.text = v.ToString();
        this._background.color = this._color;
    }
    
    public void SetGreen(string value){
        byte v = MathUtils.StringToByte(value);
        this._color = new Color32(
            this._color.r, 
            v, 
            this._color.b,
            this._color.a);
        this._greenField.text = v.ToString();
        this._background.color = this._color;
    }

    public void SetBlue(string value){
        byte v = MathUtils.StringToByte(value);
        this._color = new Color32(
            this._color.r, 
            this._color.g, 
            v, 
            this._color.a);
        this._blueField.text = v.ToString();
        this._background.color = this._color;
    }

    public void SetAlpha(string value){
        byte v = MathUtils.StringToByte(value);
        this._color = new Color32(
            this._color.r, 
            this._color.g, 
            this._color.b,
            v);
        this._alphaField.text = v.ToString();
        this._background.color = this._color;
    }

    public void SetMatchers(string value){
        string[] split = value.Trim().Split(' ', ',');
        List<string> sanitized = new List<string>();
        for(int i = 0; i < split.Length; i++){
            split[i] = split[i].Trim();
            if(split[i].Length > 0){
                sanitized.Add(split[i]);
            }
        }
        this._matchers = sanitized.ToArray();
        this._matchersField.text = string.Join(",", sanitized);
    }

    [System.Serializable]
    public class SaveData{
        public Color32 color = Color.white;
        public string[] matchers = new string[0];

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
        SetRed(data.color.r.ToString());
        SetGreen(data.color.g.ToString());
        SetBlue(data.color.b.ToString());
        SetAlpha(data.color.a.ToString());
        SetMatchers(string.Join(",", data.matchers));
    }
}
