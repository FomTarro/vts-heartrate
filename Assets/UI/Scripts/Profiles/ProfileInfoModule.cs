using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UI.InputTools;
using TMPro;

public class ProfileInfoModule : MonoBehaviour{

    [SerializeField]
    private TMP_Text _title = null;
    [SerializeField]
    private ExtendedButton _loadButton = null;

    private SaveDataManager.ModelProfileInfo _data;

    // Start is called before the first frame update
    void Start(){
        
    }

    public void Clone(){
        // HeartrateManager.Instance.Plugin.CreateHotkeyModule(this.ToSaveData());
    }

    public void Delete(){
        SaveDataManager.Instance.DeleteModelProfile(this._data);
    }

    public void FromSaveData(SaveDataManager.ModelProfileInfo data){
        this._data = data;
        this._title.text = data.DisplayName;
        this._loadButton.onPointerUp.AddListener(() => {
            SaveDataManager.Instance.LoadModelProfile(this._data);
        });
    }
}
