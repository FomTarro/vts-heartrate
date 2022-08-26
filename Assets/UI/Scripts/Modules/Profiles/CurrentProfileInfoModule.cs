using UnityEngine;
using TMPro;

public class CurrentProfileInfoModule : MonoBehaviour {

    [SerializeField]
    private TMP_Text _modelName = null;
    [SerializeField]
    private TMP_Text _minimizedText = null;
    [SerializeField]
    private TMP_InputField _profileName = null;

    [SerializeField]
    private TMP_Text _fileName = null;
    [SerializeField]
    private TMP_Text _currentProfileNavBar = null;

    private ProfileManager.ProfileData _data;
    private void Start(){
        this._profileName.onEndEdit.AddListener(RenameProfile);
    }
    public void FromSaveData(ProfileManager.ProfileData data){
        this._data = data;
        this._modelName.text = data.modelName;
        this._minimizedText.text = data.DisplayName;
        this._profileName.text = data.profileName;
        this._fileName.text = data.FileName + ".json";
        this._currentProfileNavBar.text = string.Format(Localization.LocalizationManager.Instance.GetString("output_current_profile_display"), data.DisplayName);
    }

    private void RenameProfile(string value){
        ProfileManager.Instance.RenameModelProfile(value, FromSaveData, FromSaveData);
    }
}
