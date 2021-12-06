using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class ReadFromFileInputModule : HeartrateInputModule
{
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
}
