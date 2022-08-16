using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CurrentProfileDisplay : MonoBehaviour
{
    [SerializeField]
    private TMP_Text _text;
    [SerializeField]
    private TMP_InputField _inputField = null;

    void Start()
    {
        // this._inputField.onTextSelection.AddListener((a, b, c) => { this._pollProfileName = false; });
        this._inputField.onEndEdit.AddListener(RenameProfile);
        SaveDataManager.Instance.RegisterEventCallback(SaveDataManager.SaveDataEventType.PROFILE_READ, Refresh);
        SaveDataManager.Instance.RegisterEventCallback(SaveDataManager.SaveDataEventType.PROFILE_WRITE, Refresh);
        Refresh();
    }

    private void Refresh(){
        this._text.text = SaveDataManager.Instance.CurrentProfile.modelName;
        this._inputField.text = SaveDataManager.Instance.CurrentProfile.profileName;
        this._inputField.interactable = !SaveDataManager.Instance.IsDefaultProfile();
    }

    // // Update is called once per frame
    // void Update()
    // {
    //     this._text.text = SaveDataManager.Instance.CurrentProfile.modelName;
    // }

    private void RenameProfile(string value){
        SaveDataManager.Instance.RenameModelProfile(value);
    }
}
