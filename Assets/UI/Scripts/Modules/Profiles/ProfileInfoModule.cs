using UnityEngine;
using TMPro;

public class ProfileInfoModule : MonoBehaviour{

    [SerializeField]
    private TMP_Text _title = null;
    [SerializeField]
    private TMP_Text _fileName = null;
    [SerializeField]
    private RectTransform _loadButton = null;
    [SerializeField]
    private RectTransform[] _otherButtons = new RectTransform[0];

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
        if(!ProfileManager.Instance.CurrentProfile.modelID.Equals(data.modelID)){
            this._loadButton.gameObject.SetActive(false);
            foreach(RectTransform rect in this._otherButtons){
                rect.localPosition = new Vector3(rect.localPosition.x + 40, rect.localPosition.y, rect.localPosition.z);
            }
        }
    }
}
