using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Localization;

public class LanguageSelector : MonoBehaviour
{
    [SerializeField]
    private Dropdown _dropdown = null;

    // Start is called before the first frame update

    private void OnEnable(){
        this._dropdown.SetValueWithoutNotify((int)Localization.LocalizationManager.Instance.CurrentLanguage - 1);
    }
    void Start()
    {
        List<string> languageOptions = new List<string>();
        foreach(SupportedLanguage language in 
        System.Enum.GetValues(typeof(SupportedLanguage))){
            languageOptions.Add(LocalizationManager.Instance.GetString("language", language));
        }
        this._dropdown.ClearOptions();
        this._dropdown.AddOptions(languageOptions);
        this._dropdown.onValueChanged.AddListener(SelectLanguage);
    }

    private void SelectLanguage(int index){
        SupportedLanguage language = (SupportedLanguage)index+1;
        LocalizationManager.Instance.SwitchLanguage(language);
    }
}
