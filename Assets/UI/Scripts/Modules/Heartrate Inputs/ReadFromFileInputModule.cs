using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class ReadFromFileInputModule : HeartrateInputModule
{
    [SerializeField]
    private InputField _input = null;
    private string _path = "";

    public void SetPath(string path){
        this._path = path;
    }
    public override int GetHeartrate()
    {
        try{
            if(File.Exists(_path)){
                string content = File.ReadAllText(_path);
                return int.Parse(content);
            }
        }catch(System.Exception e){
            Debug.LogWarning(e);
        }

        return 0;
    }

    protected override SaveData.Values ToValues()
    {
        SaveData.Values values = new SaveData.Values();
        values.path = this._path;
        return values;
    }

    protected override void FromValues(SaveData.Values values)
    {
        if(!string.IsNullOrEmpty(values.path)){
            this._path = values.path;
            this._input.text = this._path;
        }
    }

    protected override void OnStatusChange(bool isActive)
    {

    }
}
