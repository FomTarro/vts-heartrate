using UnityEngine;
using TMPro;

public class ProfileInfoModule : MonoBehaviour{

    [SerializeField]
    private TMP_Text _title = null;
    [SerializeField]
    private TMP_Text _fileName = null;

    private ProfileManager.ProfileData _data;


    public void Load(){
        ProfileManager.Instance.LoadModelProfile(this._data);
    }

    public void Clone(){
        ProfileManager.Instance.CopyModelProfile(this._data);
    }

    public void Delete(){
        ProfileManager.Instance.DeleteModelProfile(this._data);
    }

    public void FromSaveData(ProfileManager.ProfileData data){
        this._data = data;
        this._title.text = data.profileName;
        this._fileName.text = string.Format(Localization.LocalizationManager.Instance.GetString("output_profile_file_name"), data.FileName);
    }
}
