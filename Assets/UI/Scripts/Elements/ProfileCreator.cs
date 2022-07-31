using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ProfileCreator : MonoBehaviour
{

    [SerializeField]
    private TMP_InputField _inputField = null;
    private bool _pollProfileName = true;
    // Start is called before the first frame update
    void Start()
    {
        this._inputField.onTextSelection.AddListener((a, b, c) => { this._pollProfileName = false; });
        this._inputField.onEndEdit.AddListener(RenameProfile);
    }

    private void RenameProfile(string value){
        //TODO: this will actually need to be in a "RENAME" function
        SaveDataManager.Instance.RenameModelProfile(value);
        this._pollProfileName = true;
    }

    public void CreateNewProfile(){
        string profileName = String.Format("NEW_PROFILE_{0}", DateTime.Now.ToString("yyyy-dd-MM-HH-mm-ss"));
        SaveDataManager.Instance.WriteModelSaveData(HeartrateManager.Instance.Plugin.ToModelSaveData());
        HeartrateManager.Instance.Plugin.FromModelSaveData(new HeartratePlugin.ModelSaveData());
        SaveDataManager.Instance.CreateNewModelProfile(
            SaveDataManager.Instance.CurrentProfile.modelName,
            SaveDataManager.Instance.CurrentProfile.modelID,
            profileName);
    }

    // Update is called once per frame
    void Update()
    {
        if(this._pollProfileName){
            this._inputField.text = SaveDataManager.Instance.CurrentProfile.profileName;
        }
    }
}
