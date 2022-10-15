using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ProfileManager : Singleton<ProfileManager>{

    private ProfileData _currentProfile = new ProfileData(
        ProfileData.NAME_NO_VTS_MODEL_LOADED, 
        ProfileData.NAME_NO_VTS_MODEL_LOADED, 
        ProfileData.PROFILE_DEFAULT,
        null);

    public ProfileData CurrentProfile { get { return this._currentProfile; } }

    [Header("UI")]
    [SerializeField]
    private RectTransform _profileModulesParent = null;
    private List<GameObject> _profileModules = new List<GameObject>();
    [SerializeField]
    private ProfileInfoModule _profilePrefab = null;
    [SerializeField]
    private ProfileSpacer _spacerPrefab = null;
    [SerializeField]
    private CurrentProfileInfoModule _currentProfileModule = null;
    [SerializeField]
    private TMP_Text _currentProfileNavbar = null;

    public override void Initialize(){
        SaveDataManager.Instance.RegisterEventCallback(SaveDataManager.SaveDataEventType.FILE_READ, PopulateProfilesTab);
        SaveDataManager.Instance.RegisterEventCallback(SaveDataManager.SaveDataEventType.FILE_WRITE, PopulateProfilesTab);
    }

    public void SetCurrentProfileInfo(string modelName, string modelID, string profileName = null, string profileID = null){
        // save current profile
        SaveDataManager.Instance.WriteModelSaveData(HeartrateManager.Instance.Plugin.ToModelSaveData());
        // make new profile
        this._currentProfile = new ProfileData(modelName, modelID, profileName, profileID);
        Debug.Log(String.Format("Setting Profile: {0} {1} {2}", modelName, profileName, profileID));
    }

    public void CreateNewProfileForCurrentModel(){
        string profileID = GenerateUUID();
        string profileName = String.Format("{0}", ProfileData.PROFILE_NEW);
        SetCurrentProfileInfo(
            CurrentProfile.modelName,
            CurrentProfile.modelID,
            profileName,
            profileID);
    }

    public void CreateDefaultNoModelProfile(){
        SetCurrentProfileInfo(
            ProfileData.NAME_NO_VTS_MODEL_LOADED, 
            ProfileData.NAME_NO_VTS_MODEL_LOADED,
            ProfileData.PROFILE_DEFAULT);
    }

    public void CopyModelProfile(ProfileData info){
        Dictionary<string, string> strings = new Dictionary<string, string>();
        strings.Add("output_copy_profile_warning_populated", 
            string.Format(Localization.LocalizationManager.Instance.GetString("output_copy_profile_warning"), 
                info.DisplayName, 
                CurrentProfile.DisplayName));
        Localization.LocalizationManager.Instance.AddStrings(strings, Localization.LocalizationManager.Instance.CurrentLanguage);
        UIManager.Instance.ShowPopUp(
            "output_copy_profile_title",
            "output_copy_profile_warning_populated",
            new PopUp.PopUpOption(
                "output_copy_profile_button_yes",
                ColorUtils.ColorPreset.GREEN,
                () => {
                    // explicitly DO NOT make a new profile, just load model settings from a different file
                    HeartrateManager.Instance.Plugin.FromModelSaveData(SaveDataManager.Instance.ReadModelData(info));
                    SaveDataManager.Instance.WriteModelSaveData(HeartrateManager.Instance.Plugin.ToModelSaveData());
                    UIManager.Instance.HidePopUp();
                }),
            new PopUp.PopUpOption(
                "output_copy_profile_button_no",
                ColorUtils.ColorPreset.GREY,
                () => {
                    UIManager.Instance.HidePopUp();
                })
        );
    }

    public void LoadModelProfile(ProfileData info){
        // Make sure we're loading from the same model we have open in VTS
        // Otherwise, hotkeys/expression dropdown values won't match file, could overwrite with blanks
        if(info.modelID.Equals(this.CurrentProfile.modelID)){
            SetCurrentProfileInfo(info.modelName, info.modelID, info.profileName, info.profileID);
            HeartrateManager.Instance.Plugin.FromModelSaveData(SaveDataManager.Instance.ReadModelData(info));
        }else{
            Debug.LogWarning("Can't load settings from a different model!");
            Dictionary<string, string> strings = new Dictionary<string, string>();
            strings.Add("output_load_profile_error_populated", 
                string.Format(Localization.LocalizationManager.Instance.GetString("output_load_profile_error"), 
                    this.CurrentProfile.modelName,
                    info.modelName));
            Localization.LocalizationManager.Instance.AddStrings(strings, Localization.LocalizationManager.Instance.CurrentLanguage);
            UIManager.Instance.ShowPopUp(
            "output_load_profile_warning_title",
            "output_load_profile_error_populated");
        }
    }

    public void DeleteModelProfile(ProfileData info){
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
                    if(info.Equals(this.CurrentProfile)){
                        // load default for model if we delete the profile we currently have open
                        SetCurrentProfileInfo(info.modelName, info.modelID); 
                        SaveDataManager.Instance.DeleteProfileData(info);
                        HeartrateManager.Instance.Plugin.FromModelSaveData(SaveDataManager.Instance.ReadModelData(this.CurrentProfile));
                        SaveDataManager.Instance.WriteModelSaveData(HeartrateManager.Instance.Plugin.ToModelSaveData());
                    }else{
                        // just delete
                        SaveDataManager.Instance.DeleteProfileData(info);
                    }
                    UIManager.Instance.HidePopUp();
                }),
            new PopUp.PopUpOption(
                "button_generic_cancel",
                ColorUtils.ColorPreset.GREY,
                () => {
                    UIManager.Instance.HidePopUp();
                })
        );
    }

    public void RenameModelProfile(string newProfileName, Action<ProfileData> onSuccess, Action<ProfileData> onError){
        string errorKey = string.Empty;
        if(newProfileName.Length <= 0 || this.CurrentProfile.profileName.Equals(newProfileName)){
            Debug.Log("Reverting rename attempt...");
            onError(this.CurrentProfile);
        }else{
            if(!IsProfileNameUnique(this.CurrentProfile.modelID, newProfileName)){
                errorKey = "output_rename_profile_error_not_unique";   
            }else if(IsDefaultProfile()){
                errorKey = "output_rename_profile_error_default_rename";
            }
            if(errorKey.Length > 0){
                UIManager.Instance.ShowPopUp(
                    "output_rename_profile_error_title",
                    errorKey
                );
                onError(this.CurrentProfile);
            }else{
                Dictionary<string, string> strings = new Dictionary<string, string>();
                strings.Add("output_rename_profile_confirm_warning_populated", 
                    string.Format(Localization.LocalizationManager.Instance.GetString("output_rename_profile_confirm_warning"), 
                        this.CurrentProfile.profileName, newProfileName));
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
                                this.CurrentProfile.modelName,
                                this.CurrentProfile.modelID,
                                newProfileName,
                                this.CurrentProfile.profileID);
                            // save it!
                            SaveDataManager.Instance.WriteModelSaveData(HeartrateManager.Instance.Plugin.ToModelSaveData());
                            // ExecuteWriteCallbacks();
                            UIManager.Instance.HidePopUp();
                            onSuccess(this.CurrentProfile);
                        }),
                    new PopUp.PopUpOption(
                        "output_rename_profile_confirm_button_no",
                        ColorUtils.ColorPreset.GREY,
                        () => {
                            UIManager.Instance.HidePopUp();
                            onError(this.CurrentProfile);
                        })
                );
            }
        }
    }

    #region  UI Hooks

    private void PopulateProfilesTab(){
        List<GameObject> tempProfiles = new List<GameObject>(this._profileModules);
        foreach(GameObject go in tempProfiles){
            Destroy(go.gameObject);
        }
        this._profileModules.Clear();
        this._currentProfileModule.FromSaveData(this.CurrentProfile);
        this._currentProfileNavbar.text = string.Format(Localization.LocalizationManager.Instance.GetString("output_current_profile_display"), this.CurrentProfile.DisplayName);
        List<List<ProfileData>> sorted = SortProfileList(SaveDataManager.Instance.GetProfileList());
        for(int i = 0; i < sorted.Count; i++){
            List<ProfileData> list = sorted[i];
            ProfileSpacer spacer = Instantiate<ProfileSpacer>(this._spacerPrefab, Vector3.zero, Quaternion.identity, this._profileModulesParent);
            spacer.transform.SetSiblingIndex(GetModuleNewChildIndex());
            spacer.Configure(list[0].modelName);
            spacer.ToggleCollapse(list[0].modelID.Equals(ProfileManager.Instance.CurrentProfile.modelID));
            this._profileModules.Add(spacer.gameObject);
            foreach(ProfileData profile in list){
                if(!profile.Equals(this.CurrentProfile)){
                    ProfileInfoModule instance = Instantiate<ProfileInfoModule>(this._profilePrefab, Vector3.zero, Quaternion.identity, spacer.Content);
                    instance.transform.SetSiblingIndex(GetModuleNewChildIndex());
                    instance.FromSaveData(profile);
                }
            }
        }
    }

    private List<List<ProfileData>> SortProfileList(List<ProfileData> unsorted){
        Dictionary<SortTuple, List<ProfileData>> uniqueModels = new Dictionary<SortTuple, List<ProfileData>>();
        foreach(ProfileData profile in unsorted){
            SortTuple tuple = new SortTuple(profile.modelName, profile.modelID);
            if(!uniqueModels.ContainsKey(tuple)){
                uniqueModels.Add(tuple, new List<ProfileData>());
            }
            uniqueModels[tuple].Add(profile);
        }
        foreach(SortTuple key in uniqueModels.Keys){
            uniqueModels[key].Sort((a, b) => {
                return a.profileName.Equals(ProfileData.PROFILE_DEFAULT) ? -1 : a.profileName.CompareTo(b.profileName); 
            });
        }
        List<List<ProfileData>> sorted = new List<List<ProfileData>>();
        List<SortTuple> sortedNames = new List<SortTuple>(uniqueModels.Keys);
        sortedNames.Sort((a, b) => 
            { return a.name.CompareTo(b.name); });
        SortTuple currentModelTuple = new SortTuple(this.CurrentProfile.modelName, this.CurrentProfile.modelID);
        if(uniqueModels.ContainsKey(currentModelTuple)){
            sorted.Add(uniqueModels[currentModelTuple]);
        }
        foreach(SortTuple key in sortedNames){
            if(!key.Equals(currentModelTuple)){
                sorted.Add(uniqueModels[key]);
            }
        }
        sorted.Reverse();
        return sorted;
    }

    private struct SortTuple{
        public string name;
        public string id;
        public SortTuple(string name, string id){
            this.name = name;
            this.id = id;
        }
    }

    public void CreateAndLoadNewProfile(){
        CreateNewProfileForCurrentModel();
        HeartrateManager.Instance.Plugin.FromModelSaveData(new HeartratePlugin.ModelSaveData());
        SaveDataManager.Instance.WriteModelSaveData(HeartrateManager.Instance.Plugin.ToModelSaveData());
    }

    private int GetModuleNewChildIndex(){
        return 2;
    }

    #endregion

    #region Helper Methods

    private static string GenerateUUID(){
        return System.Guid.NewGuid().ToString().Replace("-", "");
    }

    public bool IsModelLoaded(){
        return !ProfileData.NAME_NO_VTS_MODEL_LOADED.Equals(this.CurrentProfile.modelName);
    }

    public bool IsDefaultProfile(){
        return this.CurrentProfile.modelID.Equals(this.CurrentProfile.profileID);
    }

    private bool IsProfileNameUnique(string modelID, string newProfileName){
        foreach(ProfileData profile in SaveDataManager.Instance.GetProfileList()){
            if(profile.modelID.Equals(modelID) && profile.profileName.Equals(newProfileName)){
                return false;
            }
        }
        return true;
    }

    #endregion

    public struct ProfileData {
        public ProfileData(string name, string ID, string profile, string profileID){
            this.modelName = name == null || name.Length <= 0 ? NAME_NO_VTS_MODEL_LOADED : name;
            this.modelID = ID == null || ID.Length <= 0 ? NAME_NO_VTS_MODEL_LOADED : ID;
            this.profileName = profile == null || profile.Length <= 0 ? PROFILE_DEFAULT : profile;
            // we recycle the model ID as the profile ID for the default profile
            this.profileID = PROFILE_DEFAULT.Equals(this.profileName) ? this.modelID : profileID == null || profileID.Length <= 0 ? GenerateUUID() : profileID;
        }
        public readonly string modelName;
        public readonly string modelID;
        public readonly string profileName;
        public readonly string profileID;

        // constants
        public const string NAME_NO_VTS_MODEL_LOADED = "NO_MODEL";
        public const string PROFILE_DEFAULT = "DEFAULT";
        public const string PROFILE_NEW = "NEW_PROFILE";

        public string FileName { get { return this.profileID; } }

        public string DisplayName { get { return 
            String.Format("{0} ({1})", this.modelName, this.profileName); 
        } }
    }
}
