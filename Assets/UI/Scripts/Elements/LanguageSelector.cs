using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Localization;

public class LanguageSelector : RefreshableDropdown
{
    protected override void SetValue(int index)
    {
        SupportedLanguage language = (SupportedLanguage)index+1;
        LocalizationManager.Instance.SwitchLanguage(language);
    }

    public override void Refresh()
    {
        List<string> languageOptions = new List<string>();
        foreach(SupportedLanguage language in 
        System.Enum.GetValues(typeof(SupportedLanguage))){
            languageOptions.Add(LocalizationManager.Instance.GetString("language", language));
        }
        RefreshValues(languageOptions);
    }
}
