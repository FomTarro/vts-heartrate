using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UI.InputTools;
using TMPro;

public class PluginAuthEntry : MonoBehaviour
{
    [SerializeField]
    private TMP_Text _title = null;
    [SerializeField]
    private ExtendedButton _revokeButton = null;
    [SerializeField]
    private ExtendedButton _tooltipButton = null;
    private string _token;

    

    public void Configure(APIManager.PluginSaveData data){
        this._title.text = data.pluginName;
        this._token = data.token;
        this._revokeButton.onPointerUp.AddListener(() => { 
            Dictionary<string, string> strings = new Dictionary<string, string>();
            strings.Add("settings_api_server_revoke_plugin_tooltip_populated", 
                string.Format(Localization.LocalizationManager.Instance.GetString("settings_api_server_revoke_plugin_tooltip"), 
                    data.pluginName, 
                    data.pluginAuthor));
            Localization.LocalizationManager.Instance.AddStrings(strings, Localization.LocalizationManager.Instance.CurrentLanguage);
            UIManager.Instance.ShowPopUp(
                "settings_api_server_revoke_plugin_title",
                "settings_api_server_revoke_plugin_tooltip_populated",
                new PopUp.PopUpOption(
                    "settings_api_server_button_keep",
                    ColorUtils.ColorPreset.WHITE,
                    () => {
                        UIManager.Instance.HidePopUp();
                    }),
                new PopUp.PopUpOption(
                    "settings_api_server_button_revoke",
                    ColorUtils.ColorPreset.RED,
                    () => {
                        APIManager.Instance.RevokeTokenData(this._token); 
                        Destroy(this.gameObject); 
                        UIManager.Instance.HidePopUp();
                    })
            );
        });
        this._tooltipButton.onPointerUp.AddListener(() => { 
            Dictionary<string, string> strings = new Dictionary<string, string>();
            strings.Add("current_plugin_title", 
                data.pluginName);
            strings.Add("current_plugin_tooltip",
                string.Format(Localization.LocalizationManager.Instance.GetString("settings_api_plugin_tooltip"),
                    data.pluginAuthor,
                    data.pluginAbout == null || data.pluginAbout.Length <= 0 ? "%settings_api_plugin_no_desc" : data.pluginAbout));
            Localization.LocalizationManager.Instance.AddStrings(strings, Localization.LocalizationManager.Instance.CurrentLanguage);
            UIManager.Instance.ShowPopUp(
                    "current_plugin_title",
                    "current_plugin_tooltip");
        });
    }
}
