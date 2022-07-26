﻿using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SaveDataManager : Singleton<SaveDataManager>
{
    private string GLOBAL_SAVE_DIRECTORY = "";
    private string GLOBAL_SAVE_FILE_PATH = "";
    private string MODEL_SAVE_DIRECTORY = "";
    private string PLUGINS_SAVE_DIRECTORY = "";
    private string PLUGINS_SAVE_FILE_PATH = "";
    public string SaveDirectory { get { return this.GLOBAL_SAVE_DIRECTORY; } }

    public override void Initialize()
    {
        this.GLOBAL_SAVE_DIRECTORY = Application.persistentDataPath;
        this.GLOBAL_SAVE_FILE_PATH = Path.Combine(this.GLOBAL_SAVE_DIRECTORY, "save.json");
        this.MODEL_SAVE_DIRECTORY = Path.Combine(this.GLOBAL_SAVE_DIRECTORY, "models");
        this.PLUGINS_SAVE_DIRECTORY = Path.Combine(this.GLOBAL_SAVE_DIRECTORY, "plugins");
        this.PLUGINS_SAVE_FILE_PATH =  Path.Combine(this.PLUGINS_SAVE_DIRECTORY, "plugins.json");
        CreateDirectoryIfNotFound(this.GLOBAL_SAVE_DIRECTORY);
        CreateDirectoryIfNotFound(this.MODEL_SAVE_DIRECTORY);
        CreateDirectoryIfNotFound(this.PLUGINS_SAVE_DIRECTORY);
    }

    private void CreateDirectoryIfNotFound(string path){
        if(!Directory.Exists(path)){
            Directory.CreateDirectory(path);
        }
    }

    #region Global Settings

    public HeartratePlugin.GlobalSaveData ReadGlobalSaveData(){
        HeartratePlugin.GlobalSaveData data = new HeartratePlugin.GlobalSaveData();
        if(File.Exists(this.GLOBAL_SAVE_FILE_PATH)){
            string content = File.ReadAllText(this.GLOBAL_SAVE_FILE_PATH);
            data = JsonUtility.FromJson<HeartratePlugin.GlobalSaveData>(content);
            data = ModernizeLegacyGlobalSaveData(data, content);
        }
        return data;
    }

    public void WriteGlobalSaveData(HeartratePlugin.GlobalSaveData data){
        File.WriteAllText(this.GLOBAL_SAVE_FILE_PATH, data.ToString());
    }

    private HeartratePlugin.GlobalSaveData ModernizeLegacyGlobalSaveData(HeartratePlugin.GlobalSaveData data, string content){
        string version = data.version;
        if(VersionUtils.IsOlderThan(version, "1.0.0")){
            LegacyGlobalSaveData_v0_1_0 legacyData = JsonUtility.FromJson<LegacyGlobalSaveData_v0_1_0>(content);
            return Modernize_v1_0_0_to_v1_1_0(Modernize_v0_1_0_to_v1_0_0(legacyData));
        }else if(VersionUtils.IsOlderThan(version, "1.1.0")){
                return Modernize_v1_0_0_to_v1_1_0(data);
        }else{
            return data;
        }
    }

    private HeartratePlugin.GlobalSaveData Modernize_v0_1_0_to_v1_0_0(LegacyGlobalSaveData_v0_1_0 legacyData){
        Debug.Log("Migrating global save file data from v0.1.0 to v 1.0.0...");
        if(legacyData.colors != null && legacyData.colors.Count > 0){
            // make a new ModelSaveData, apply it to the first loaded model.
            HeartratePlugin.ModelSaveData modelData = new HeartratePlugin.ModelSaveData();
            modelData.colors = legacyData.colors;
            HeartrateManager.Instance.Plugin.FromModelSaveData(modelData);
        }
        HeartratePlugin.GlobalSaveData data = new HeartratePlugin.GlobalSaveData();
        data.inputs = legacyData.inputs;
        data.activeInput = HeartrateInputModule.InputType.SLIDER;
        data.maxRate = legacyData.maxRate;
        data.minRate = legacyData.minRate;
        return data;
    }

    private HeartratePlugin.GlobalSaveData Modernize_v1_0_0_to_v1_1_0(HeartratePlugin.GlobalSaveData legacyData){
        Debug.Log("Migrating global save file data from v1.0.0 to v 1.1.0...");
        HeartratePlugin.GlobalSaveData data = new HeartratePlugin.GlobalSaveData();
        data.version = legacyData.version;
        data.inputs = legacyData.inputs;
        data.activeInput = HeartrateInputModule.InputType.SLIDER;
        data.maxRate = legacyData.maxRate;
        data.minRate = legacyData.minRate;
        return data;
    }

    #endregion

    #region Model Settings

    public HeartratePlugin.ModelSaveData ReadModelData(string modelID){
        HeartratePlugin.ModelSaveData data = new HeartratePlugin.ModelSaveData();
        Debug.Log("Loading Model: " + modelID);
        string filePath = Path.Combine(this.MODEL_SAVE_DIRECTORY, modelID+".json");
        if(File.Exists(filePath)){
            string text = File.ReadAllText(filePath);
            data = JsonUtility.FromJson<HeartratePlugin.ModelSaveData>(text);
            data = ModernizeLegacyModelSaveData(data, text);
        }
        return data;
    }

    public void WriteModelSaveData(HeartratePlugin.ModelSaveData data){
        string filePath = Path.Combine(this.MODEL_SAVE_DIRECTORY, data.modelID+".json");
        File.WriteAllText(filePath, data.ToString());
    }

    public Dictionary<string, string> GetModelDataNameMap(){
        Dictionary<string, string> dict = new Dictionary<string, string>();
        foreach(string s in Directory.GetFiles(this.MODEL_SAVE_DIRECTORY)){
            string text = File.ReadAllText(s);
            HeartratePlugin.ModelSaveData data = JsonUtility.FromJson<HeartratePlugin.ModelSaveData>(text);
            dict.Add(string.Format("{0}<size=0>{1}</size>", data.modelName, data.modelID), data.modelID);
        }
        return dict;
    }

    public void CopyModelSaveData(string modelID, string modelName){
        Dictionary<string, string> strings = new Dictionary<string, string>();
        strings.Add("output_copy_profile_warning_populated", 
            string.Format(Localization.LocalizationManager.Instance.GetString("output_copy_profile_warning"), 
                modelName, 
                HeartrateManager.Instance.Plugin.CurrentModelName));
        Localization.LocalizationManager.Instance.AddStrings(strings, Localization.LocalizationManager.Instance.CurrentLanguage);
        UIManager.Instance.ShowPopUp(
            "output_copy_profile_title",
            "output_copy_profile_warning_populated",
            new PopUp.PopUpOption(
                "output_copy_profile_button_yes",
                true,
                () => {
                    HeartrateManager.Instance.Plugin.FromModelSaveData(ReadModelData(modelID));
                    UIManager.Instance.HidePopUp();
                }),
            new PopUp.PopUpOption(
                "output_copy_profile_button_no",
                false,
                () => {
                    UIManager.Instance.HidePopUp();
                })
        );
    }

    private HeartratePlugin.ModelSaveData ModernizeLegacyModelSaveData(HeartratePlugin.ModelSaveData data, string content){
        string version = data.version;
        if(VersionUtils.IsOlderThan(version, "1.1.0")){
            LegacyModelSaveData_v1_0_0 legacyData = JsonUtility.FromJson<LegacyModelSaveData_v1_0_0>(content);
            return Modernize_v1_1_0_to_v_1_2_0(Modernize_v1_0_0_to_v_1_1_0(legacyData));
        }else if(VersionUtils.IsOlderThan(version, "1.2.0")){
            LegacyModelSaveData_v1_1_0 legacyData_v1_1_0 = JsonUtility.FromJson<LegacyModelSaveData_v1_1_0>(content);
            return Modernize_v1_1_0_to_v_1_2_0(legacyData_v1_1_0);
        }else{
            return data;
        }
    }

    private LegacyModelSaveData_v1_1_0 Modernize_v1_0_0_to_v_1_1_0(LegacyModelSaveData_v1_0_0 legacyData){
        Debug.Log("Migrating model save file data from v1.0.0 to v 1.1.0...");
        LegacyModelSaveData_v1_1_0 data = new LegacyModelSaveData_v1_1_0();
        data.colors = legacyData.colors;
        data.hotkeys = new List<HotkeyModule.SaveData>();
        data.modelID = legacyData.modelID;
        data.modelName = legacyData.modelName;
        data.version = legacyData.version;
        data.expressions = new List<ExpressionModule.SaveData>();

        foreach(LegacyExpressionSaveData_v1_0_0 legacyExpression in legacyData.expressions){
            ExpressionModule.SaveData expressionData = new ExpressionModule.SaveData();
            expressionData.threshold = legacyExpression.threshold;
            expressionData.expressionFile = legacyExpression.expressionFile;
            expressionData.behavior = legacyExpression.shouldActivate ? 
                ExpressionModule.TriggerBehavior.ACTIVATE_ABOVE_DEACTIVATE_BELOW : 
                ExpressionModule.TriggerBehavior.DEACTIVATE_ABOVE_ACTIVATE_BELOW;
            data.expressions.Add(expressionData);
        }
        return data;
    }

    private HeartratePlugin.ModelSaveData Modernize_v1_1_0_to_v_1_2_0(LegacyModelSaveData_v1_1_0 legacyData){
        Debug.Log("Migrating model save file data from v1.1.0 to v 1.2.0...");
        HeartratePlugin.ModelSaveData data = new HeartratePlugin.ModelSaveData();
        data.colors = legacyData.colors;
        data.hotkeys = new List<HotkeyModule.SaveData>();
        data.modelID = legacyData.modelID;
        data.modelName = legacyData.modelName;
        data.version = legacyData.version;
        data.expressions = legacyData.expressions;
        data.hotkeys = legacyData.hotkeys;
        //TODO: move each model's settings into DEFAULT profile for each file.
        return data;
    }

    #endregion

    #region Plugin Tokens

    public APIManager.TokenSaveData ReadTokenSaveData(){
        APIManager.TokenSaveData data = new APIManager.TokenSaveData();
        if(File.Exists(this.PLUGINS_SAVE_FILE_PATH)){
            string text = File.ReadAllText(this.PLUGINS_SAVE_FILE_PATH);
            data = JsonUtility.FromJson<APIManager.TokenSaveData>(text);
        }
        return data;
    }

    public void WriteTokenSaveData(APIManager.TokenSaveData data){
        File.WriteAllText(this.PLUGINS_SAVE_FILE_PATH, data.ToString());
    }

    #endregion
}