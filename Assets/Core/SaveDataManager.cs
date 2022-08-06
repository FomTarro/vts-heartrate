using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Events;

public class SaveDataManager : Singleton<SaveDataManager>
{
    private string GLOBAL_SAVE_DIRECTORY = "";
    private string GLOBAL_SAVE_FILE_PATH = "";
    private string MODEL_SAVE_DIRECTORY = "";
    private string PLUGINS_SAVE_DIRECTORY = "";
    private string PLUGINS_SAVE_FILE_PATH = "";
    public string SaveDirectory { get { return this.GLOBAL_SAVE_DIRECTORY; } }

    private ModelProfileInfo _currentProfile = new ModelProfileInfo(
        ModelProfileInfo.NAME_NO_VTS_MODEL_LOADED, 
        ModelProfileInfo.NAME_NO_VTS_MODEL_LOADED, 
        ModelProfileInfo.PROFILE_DEFAULT,
        null);

    public ModelProfileInfo CurrentProfile { get { return this._currentProfile; } }

    private UnityEvent _onProfileLoaded = new UnityEvent();
    private UnityEvent _onProfileSaved = new UnityEvent();

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
            LegacyModelSaveData_v1_0_0 modelData = new LegacyModelSaveData_v1_0_0();
            modelData.colors = legacyData.colors;
            HeartrateManager.Instance.Plugin.FromModelSaveData(
                ModernizeLegacyModelSaveData(new HeartratePlugin.ModelSaveData(), modelData.ToString()));
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

    public HeartratePlugin.ModelSaveData ReadModelData(ModelProfileInfo info){
        HeartratePlugin.ModelSaveData data = new HeartratePlugin.ModelSaveData();
        Debug.Log("Loading Model: " + info.FileName);
        string filePath = Path.Combine(this.MODEL_SAVE_DIRECTORY, info.FileName + ".json");
        if(File.Exists(filePath)){
            Debug.Log(string.Format("Reading from path: {0}", info.FileName));
            string text = File.ReadAllText(filePath);
            data = JsonUtility.FromJson<HeartratePlugin.ModelSaveData>(text);
            data = ModernizeLegacyModelSaveData(data, text);
        }
        this._onProfileLoaded.Invoke();
        return data;
    }

    public void WriteModelSaveData(HeartratePlugin.ModelSaveData data){
        string filePath = Path.Combine(this.MODEL_SAVE_DIRECTORY, this._currentProfile.FileName + ".json");
        Debug.Log(string.Format("Writing to path: {0}", this._currentProfile.FileName));
        File.WriteAllText(filePath, data.ToString());
        this._onProfileSaved.Invoke();
    }

    private void DeleteModelData(ModelProfileInfo info){
        string filePath = Path.Combine(this.MODEL_SAVE_DIRECTORY, info.FileName + ".json");
        if(File.Exists(filePath)){
            Debug.Log(string.Format("Deleting from path: {0}", info.FileName));
            File.Delete(filePath);
            this._onProfileSaved.Invoke();
        }
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
        data.hotkeys = legacyData.hotkeys;
        data.modelID = legacyData.modelID;
        data.modelName = legacyData.modelName;
        data.version = legacyData.version;
        data.expressions = legacyData.expressions;
        data.profileName = ModelProfileInfo.PROFILE_DEFAULT;
        return data;
    }

    #endregion

    #region Profile Settings 

    public Dictionary<string, ModelProfileInfo> GetModelProfileMap(){
        Dictionary<string, ModelProfileInfo> dict = new Dictionary<string, ModelProfileInfo>();
        foreach(string s in Directory.GetFiles(this.MODEL_SAVE_DIRECTORY)){
            string text = File.ReadAllText(s);
            HeartratePlugin.ModelSaveData data = JsonUtility.FromJson<HeartratePlugin.ModelSaveData>(text);
            ModelProfileInfo info = new ModelProfileInfo(data.modelName, data.modelID, data.profileName, data.uuid);
            dict.Add(string.Format("{0}<size=0>{1}</size> ({2})", data.modelName, data.uuid, data.profileName), info);
        }
        return dict;
    }

    public void SetCurrentProfileInfo(string modelName, string modelID, string profileName = null, string profileID = null){
        // save current profile
        WriteModelSaveData(HeartrateManager.Instance.Plugin.ToModelSaveData());
        // make new profile
        this._currentProfile = new ModelProfileInfo(modelName, modelID, profileName, profileID);
        Debug.Log(String.Format("Setting Profile: {0} {1} {2}", modelName, profileName, profileID));
    }

    public void CreateNewProfileForCurrentModel(){
        string profileID = GenerateUUID();
        string profileName = String.Format("{0}_{1}", ModelProfileInfo.PROFILE_NEW, profileID);
        SetCurrentProfileInfo(
            this._currentProfile.modelName,
            this._currentProfile.modelID,
            profileName,
            profileID);
    }

    public void CreateDefaultUnloadedProfile(){
        SetCurrentProfileInfo(
            ModelProfileInfo.NAME_NO_VTS_MODEL_LOADED, 
            ModelProfileInfo.NAME_NO_VTS_MODEL_LOADED,
            ModelProfileInfo.PROFILE_DEFAULT);
    }

    public void CopyModelProfile(ModelProfileInfo info){
        Dictionary<string, string> strings = new Dictionary<string, string>();
        strings.Add("output_copy_profile_warning_populated", 
            string.Format(Localization.LocalizationManager.Instance.GetString("output_copy_profile_warning"), 
                info.DisplayName, 
                this._currentProfile.DisplayName));
        Localization.LocalizationManager.Instance.AddStrings(strings, Localization.LocalizationManager.Instance.CurrentLanguage);
        UIManager.Instance.ShowPopUp(
            "output_copy_profile_title",
            "output_copy_profile_warning_populated",
            new PopUp.PopUpOption(
                "output_copy_profile_button_yes",
                ColorUtils.ColorPreset.GREEN,
                () => {
                    // explicitly DO NOT make a new profile, just load model settings from a different file
                    HeartrateManager.Instance.Plugin.FromModelSaveData(ReadModelData(info));
                    WriteModelSaveData(HeartrateManager.Instance.Plugin.ToModelSaveData());
                    UIManager.Instance.HidePopUp();
                }),
            new PopUp.PopUpOption(
                "output_copy_profile_button_no",
                ColorUtils.ColorPreset.WHITE,
                () => {
                    UIManager.Instance.HidePopUp();
                })
        );
    }

    public void LoadModelProfile(ModelProfileInfo info){
        // Make sure we're loading from the same model we have open in VTS
        // Otherwise, hotkeys/expression dropdown values won't match file, could overwrite with blanks
        if(info.modelID.Equals(this._currentProfile.modelID)){
            SetCurrentProfileInfo(info.modelName, info.modelID, info.profileName, info.profileID);
            HeartrateManager.Instance.Plugin.FromModelSaveData(ReadModelData(info));
        }else{
            Debug.LogWarning("Can't load settings from a different model!");
            Dictionary<string, string> strings = new Dictionary<string, string>();
            strings.Add("output_load_profile_error_populated", 
                string.Format(Localization.LocalizationManager.Instance.GetString("output_load_profile_error"), 
                    this._currentProfile.modelName,
                    info.modelName));
            Localization.LocalizationManager.Instance.AddStrings(strings, Localization.LocalizationManager.Instance.CurrentLanguage);
            UIManager.Instance.ShowPopUp(
            "output_load_profile_warning_title",
            "output_load_profile_error_populated");
        }
    }

    public void DeleteModelProfile(ModelProfileInfo info){
        Dictionary<string, string> strings = new Dictionary<string, string>();
        strings.Add("output_delete_profile_warning_populated", 
            string.Format(Localization.LocalizationManager.Instance.GetString("output_delete_profile_warning"), 
                info.DisplayName));
        Localization.LocalizationManager.Instance.AddStrings(strings, Localization.LocalizationManager.Instance.CurrentLanguage);
        UIManager.Instance.ShowPopUp(
            "output_delete_profile_title",
            "output_delete_profile_warning_populated",
            new PopUp.PopUpOption(
                "button_generic_delete",
                ColorUtils.ColorPreset.RED,
                () => {
                    if(info.Equals(this._currentProfile)){
                        // load default for model if we delete the profile we currently have open
                        // TODO: this is saving the profile we just deleted again... whoops.
                        SetCurrentProfileInfo(info.modelName, info.modelID, ModelProfileInfo.PROFILE_DEFAULT, info.profileID); 
                        DeleteModelData(info);
                        HeartrateManager.Instance.Plugin.FromModelSaveData(ReadModelData(this._currentProfile));
                        WriteModelSaveData(HeartrateManager.Instance.Plugin.ToModelSaveData());
                    }else{
                        // just delete
                        DeleteModelData(info);
                    }
                    UIManager.Instance.HidePopUp();
                }),
            new PopUp.PopUpOption(
                "button_generic_cancel",
                ColorUtils.ColorPreset.WHITE,
                () => {
                    UIManager.Instance.HidePopUp();
                })
        );
    }

    public void RenameModelProfile(string newProfileName){
        string errorKey = string.Empty;
        if(newProfileName.Length <= 0 || _currentProfile.profileName.Equals(newProfileName)){
            Debug.Log("Reverting rename attempt...");
            this._onProfileLoaded.Invoke();
            return;
        }
    
        if(!IsProfileNameUnique(this._currentProfile.modelID, newProfileName)){
            errorKey = "output_rename_profile_error_not_unique";   
        }
        if(errorKey.Length > 0){
            Debug.Log(errorKey);
            UIManager.Instance.ShowPopUp(
                "output_rename_profile_error_title",
                errorKey
            );
            this._onProfileLoaded.Invoke();
        }else{
            Dictionary<string, string> strings = new Dictionary<string, string>();
            strings.Add("output_rename_profile_confirm_warning_populated", 
                string.Format(Localization.LocalizationManager.Instance.GetString("output_rename_profile_confirm_warning"), 
                    this._currentProfile.profileName, newProfileName));
            Localization.LocalizationManager.Instance.AddStrings(strings, Localization.LocalizationManager.Instance.CurrentLanguage);
            UIManager.Instance.ShowPopUp(
                "output_rename_profile_confirm_title",
                "output_rename_profile_confirm_warning_populated",
                new PopUp.PopUpOption(
                    "output_rename_profile_confirm_button_yes",
                    ColorUtils.ColorPreset.GREEN,
                    () => {
                        // make new profile with the new name
                        SetCurrentProfileInfo(
                            CurrentProfile.modelName,
                            CurrentProfile.modelID,
                            newProfileName,
                            CurrentProfile.profileID);
                        // save it!
                        WriteModelSaveData(HeartrateManager.Instance.Plugin.ToModelSaveData());
                        UIManager.Instance.HidePopUp();
                    }),
                new PopUp.PopUpOption(
                    "output_rename_profile_confirm_button_no",
                    ColorUtils.ColorPreset.WHITE,
                    () => {
                        UIManager.Instance.HidePopUp();
                    })
            );
        }
    }

    public void RegisterProfileLoadedCallback(System.Action onLoaded){
        this._onProfileLoaded.AddListener(new UnityAction(onLoaded));
    }

    public void RegisterProfileSavedCallback(System.Action onSaved){
        this._onProfileSaved.AddListener(new UnityAction(onSaved));
    }

    private bool IsProfileNameUnique(string modelID, string newProfileName){
        Dictionary<string, ModelProfileInfo> otherProfiles = GetModelProfileMap(); 
        foreach(ModelProfileInfo profile in otherProfiles.Values){
            if(profile.modelID.Equals(modelID) && profile.profileName.Equals(newProfileName)){
                return false;
            }
        }
        return true;
    }

    public bool IsModelLoaded(){
        return !ModelProfileInfo.NAME_NO_VTS_MODEL_LOADED.Equals(this._currentProfile.modelName);
    }

    public bool IsDefaultProfile(){
        return ModelProfileInfo.PROFILE_DEFAULT.Equals(this._currentProfile.profileName);
    }

    public struct ModelProfileInfo {
        public ModelProfileInfo(string name, string ID, string profile, string profileID){
            this.modelName = name == null || name.Length <= 0 ? NAME_NO_VTS_MODEL_LOADED : name;
            this.modelID = ID == null || ID.Length <= 0 ? NAME_NO_VTS_MODEL_LOADED : ID;
            this.profileName = profile == null || profile.Length <= 0 ? PROFILE_DEFAULT : profile;
            this.profileID = profileID == null || profileID.Length <= 0 ? GenerateUUID() : profileID;
        }
        public readonly string modelName;
        public readonly string modelID;
        public readonly string profileName;
        public readonly string profileID;

        // constants
        public const string NAME_NO_VTS_MODEL_LOADED = "NO_MODEL";
        public const string PROFILE_DEFAULT = "DEFAULT";
        public const string PROFILE_NEW = "NEW_PROFILE";

        public string FileName { get { return
            this.profileName == null || this.profileName.Length <= 0 || PROFILE_DEFAULT.Equals(this.profileName) 
            ? this.modelID : String.Format("{0}_{1}", this.modelID, this.profileID); 
        } }

        public string DisplayName { get { return 
            String.Format("{0} ({1})", this.modelName, this.profileName); 
        } }
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

    #region Helper Methods

    private static string GenerateUUID(){
        return System.Guid.NewGuid().ToString().Replace("-", "");
    }

    #endregion
}
