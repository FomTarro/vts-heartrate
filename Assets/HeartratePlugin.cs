using VTS.Networking.Impl;
using VTS.Models.Impl;
using VTS.Models;
using VTS;

using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;
public class HeartratePlugin : VTSPlugin
{
    private string SAVE_PATH = "";

    [SerializeField]
    private List<ColorInputModule> _colors = new List<ColorInputModule>();

    [SerializeField]
    [Range(50, 120)]
    private int _heartRate = 70;
    private int _maxRate = 100;
    private int _minRate = 70;

    // Start is called before the first frame update
    private void Start()
    {
        this.SAVE_PATH = Path.Combine(Application.persistentDataPath, "save.json");
        Load(); 
        // Everything you need to get started!
        Initialize(
            new WebSocketImpl(),
            new JsonUtilityImpl(),
            new TokenStorageImpl(),
            () => {Debug.Log("Connected!");},
            () => {Debug.LogWarning("Disconnected!");},
            () => {Debug.LogError("Error!");});
    }

    private void OnApplicationQuit(){
        Save();
    }

    private void FixedUpdate(){
        foreach(ColorInputModule module in this._colors){
            ArtMeshMatcher matcher = new ArtMeshMatcher();
            matcher.tintAll = false;
            matcher.nameContains = module.ModuleMatchers;
            this.TintArtMesh(
                Color32.Lerp(Color.white, module.ModuleColor, 
                ((float)(this._heartRate-this._minRate)/(float)(this._maxRate - this._minRate))),  
                0.5f, 
                matcher,
                (success) => {

                },
                (error) => {

                });
        }
    }

    public void SetHeartRate(int rate){
        this._heartRate = rate;
    }

    private void Save(){
        SaveData data = new SaveData();
        foreach(ColorInputModule module in this._colors){
            data.modules.Add(module.ToSaveData());
        }
        File.WriteAllText(this.SAVE_PATH, data.ToString());
    }

    private void Load(){
        if(File.Exists(this.SAVE_PATH)){
            string text = File.ReadAllText(this.SAVE_PATH);
            SaveData data = JsonUtility.FromJson<SaveData>(text);
            foreach(ColorInputModule.SaveData module in data.modules){
                // TODO instantiate from prefab, and populate
                this._colors[0].FromSaveData(module);
                // Load data
            }
        }
    }

    [System.Serializable]
    public class SaveData{
        public List<ColorInputModule.SaveData> modules = new List<ColorInputModule.SaveData>();

        public override string ToString()
        {
            return JsonUtility.ToJson(this);
        }
    }
}
