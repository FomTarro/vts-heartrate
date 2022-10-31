using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class HeartrateManager : Singleton<HeartrateManager>
{
    [SerializeField]
    private HeartratePlugin _plugin = null;
    public HeartratePlugin Plugin { get { return this._plugin;} }

    private const string VERSION_URL = @"https://www.skeletom.net/vts-heartrate/version";

    public override void Initialize(){
        Application.targetFrameRate = 30;
        this.Plugin.OnLaunch();
        CheckVersion();
    }
    
    #region Version
    private void CheckVersion(){
        StartCoroutine(HttpUtils.GetRequest(
            VERSION_URL,
            (e) => {
                Debug.LogError(e.message);
                Dictionary<string, string> strings = new Dictionary<string, string>();
                    strings.Add("error_cannot_resolve_tooltip_populated", 
                        string.Format(Localization.LocalizationManager.Instance.GetString("error_cannot_resolve_tooltip"), e.message));
                    Localization.LocalizationManager.Instance.AddStrings(strings, Localization.LocalizationManager.Instance.CurrentLanguage);
                UIManager.Instance.ShowPopUp(
                    "error_generic_title",
                    "error_cannot_resolve_tooltip_populated",
                    new PopUp.PopUpOption(
                            "settings_feedback_button_tweet", 
                            ColorUtils.ColorPreset.GREEN, 
                            () => { Application.OpenURL("https://twitter.com/intent/tweet?text=@FomTarro"); }),
                    new PopUp.PopUpOption(
                            "settings_feedback_button_email", 
                            ColorUtils.ColorPreset.GREEN, 
                            () => { Application.OpenURL("mailto:tom@skeletom.net"); })
                );
            },
            (s) => {
                VersionUtils.VersionInfo info = JsonUtility.FromJson<VersionUtils.VersionInfo>(s);
                Debug.Log(VersionUtils.CompareVersion(info) ? "Newer version needed: " + info.url : "Up to date.");
                if(VersionUtils.CompareVersion(info)){
                    Dictionary<string, string> strings = new Dictionary<string, string>();
                    strings.Add("settings_new_version_body_populated", 
                        string.Format(Localization.LocalizationManager.Instance.GetString("settings_new_version_body"), 
                        info.version, 
                        info.date, 
                        info.url));
                    Localization.LocalizationManager.Instance.AddStrings(strings, Localization.LocalizationManager.Instance.CurrentLanguage);
                    UIManager.Instance.ShowPopUp(
                        "settings_new_version_title",
                        "settings_new_version_body_populated",
                        new PopUp.PopUpOption(
                            "settings_new_version_button_download", 
                            ColorUtils.ColorPreset.GREEN, 
                            () => { Application.OpenURL(info.url); })
                        );
                }
            },
            null
        ));
    }

    #endregion
}
