using UnityEngine;
using TMPro;

public class CurrentProfileInfoModule : MonoBehaviour {

    [SerializeField]
    private TMP_Text _modelName = null;
    [SerializeField]
    private TMP_Text _minimizedText = null;
    [SerializeField]
    private TMP_InputField _profileName = null;

    private SaveDataManager.ModelProfileInfo _data;
    public void FromSaveData(SaveDataManager.ModelProfileInfo data){
        this._data = data;
        this._modelName.text = data.modelName;
        this._minimizedText.text = data.DisplayName;
        this._profileName.text = data.profileName;
        this._profileName.onEndEdit.AddListener(RenameProfile);
    }

    private void RenameProfile(string value){
        SaveDataManager.Instance.RenameModelProfile(value);
    }
}
