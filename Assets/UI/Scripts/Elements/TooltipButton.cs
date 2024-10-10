using UnityEngine;
using UI.InputTools;

[RequireComponent(typeof(ExtendedButton))]
public class TooltipButton : MonoBehaviour
{
    private ExtendedButton _button = null;
    [SerializeField]
    private Tooltips _tooltip;

    // Start is called before the first frame update
    void Start()
    {
        this._button = GetComponent<ExtendedButton>();
        this._button.onPointerUp.AddListener(() => { DisplayTooltip(this._tooltip); });
    }

    private void DisplayTooltip(Tooltips tip)
    {
        switch (tip)
        {
            case Tooltips.HEARTRATE_RANGES:
                UIManager.Instance.ShowPopUp(
                    "input_ranges_title",
                    "input_ranges_tooltip"
                );
                break;
            case Tooltips.HEARTRATE_SLIDER:
                UIManager.Instance.ShowPopUp(
                    "input_slider_title",
                    "input_slider_tooltip"
                );
                break;
            case Tooltips.HEARTRATE_WEBSOCKET:
                UIManager.Instance.ShowPopUp(
                    "input_websocket_title",
                    "input_websocket_tooltip",
                    new PopUp.PopUpOption(
                        "settings_api_server_button_open_readme",
                        ColorUtils.ColorPreset.BLUE,
                        () => { Application.OpenURL("https://github.com/FomTarro/vts-heartrate/#api"); })
                );
                break;
            case Tooltips.HEARTRATE_PULSOID_CONNECT:
                UIManager.Instance.ShowPopUp(
                    "input_pulsoid_app_title",
                    "input_pulsoid_app_tooltip",
                    new PopUp.PopUpOption(
                        "input_pulsoid_button_visit",
                        ColorUtils.ColorPreset.BLUE,
                        () => { Application.OpenURL("https://www.pulsoid.net"); })
                );
                break;
            case Tooltips.HEARTRATE_PULSOID_FEED:
                UIManager.Instance.ShowPopUp(
                    "input_pulsoid_feed",
                    "input_pulsoid_feed_tooltip",
                    new PopUp.PopUpOption(
                        "input_pulsoid_button_visit",
                        ColorUtils.ColorPreset.BLUE,
                        () => { Application.OpenURL("https://www.pulsoid.net"); })
                );
                break;
            case Tooltips.HEARTRATE_FILE:
                UIManager.Instance.ShowPopUp(
                    "input_file_title",
                    "input_file_tooltip"
                );
                break;
            case Tooltips.HEARTRATE_ANT_PLUS:
                UIManager.Instance.ShowPopUp(
                    "input_ant_plus_title",
                    "input_ant_plus_tooltip",
                    new PopUp.PopUpOption(
                        "input_ant_plus_button_visit",
                        ColorUtils.ColorPreset.BLUE,
                        () => { Application.OpenURL("https://www.thisisant.com/company/"); })
                );
                break;
            case Tooltips.HEARTRATE_HYPERATE:
                UIManager.Instance.ShowPopUp(
                    "input_hyperate_title",
                    "input_hyperate_tooltip",
                    new PopUp.PopUpOption(
                        "input_hyperate_button_visit",
                        ColorUtils.ColorPreset.BLUE,
                        () => { Application.OpenURL("https://www.hyperate.io"); })
                );
                break;
            case Tooltips.HEARTRATE_FITBIT:
                UIManager.Instance.ShowPopUp(
                    "input_fitbit_title",
                    "input_fitbit_tooltip",
                    new PopUp.PopUpOption(
                        "input_fitbit_button_visit",
                        ColorUtils.ColorPreset.BLUE,
                        () => { Application.OpenURL("https://www.fitbit.com/"); })
                );
                break;
            case Tooltips.OUTPUT_COLOR:
                UIManager.Instance.ShowPopUp(
                    "output_artmesh_title",
                    "output_artmesh_tooltip",
                    new PopUp.PopUpOption(
                        "output_artmesh_button",
                        ColorUtils.ColorPreset.BLUE,
                        () => { Application.OpenURL("https://hawk.bar/VTubeStudioTagger/"); })
                );
                break;
            case Tooltips.OUTPUT_PARAMS:
                UIManager.Instance.ShowPopUp(
                    "output_custom_params_title",
                    "output_custom_params_tooltip",
                    new PopUp.PopUpOption(
                        "output_custom_params_button",
                        ColorUtils.ColorPreset.BLUE,
                        () => { Application.OpenURL("https://github.com/DenchiSoft/VTubeStudio/wiki/Plugins#what-are-custom-parameters"); })
                );
                break;
            case Tooltips.OUTPUT_EXPRESSION:
                UIManager.Instance.ShowPopUp(
                    "output_expressions_title",
                    "output_expressions_tooltip"
                );
                break;
            case Tooltips.OUTPUT_PROFILE:
                UIManager.Instance.ShowPopUp(
                    "output_current_profile_title",
                    "output_current_profile_tooltip"
                );
                break;
            case Tooltips.OUTPUT_HOTKEY:
                UIManager.Instance.ShowPopUp(
                    "output_hotkey_title",
                    "output_hotkey_tooltip"
                );
                break;
            case Tooltips.OUTPUT_VFX:
                UIManager.Instance.ShowPopUp(
                    "output_vfx_title",
                    "output_vfx_tooltip",
                    new PopUp.PopUpOption(
                        "output_vfx_tooltip_button",
                        ColorUtils.ColorPreset.BLUE,
                        () => { Application.OpenURL("https://github.com/DenchiSoft/VTubeStudio/wiki/Visual-Effects"); })
                );
                break;
            case Tooltips.OUTPUT_WIDGET:
                UIManager.Instance.ShowPopUp(
                    "output_stream_widget_title",
                    "output_stream_widget_tooltip"
                );
                break;
            case Tooltips.SETTINGS_API:
                UIManager.Instance.ShowPopUp(
                    "settings_api_server_title",
                    "settings_api_server_tooltip",
                    new PopUp.PopUpOption(
                        "settings_api_server_button_open_readme",
                        ColorUtils.ColorPreset.BLUE,
                        () => { Application.OpenURL("https://github.com/FomTarro/vts-heartrate/#api"); })
                );
                break;
            default:
                break;
        }
    }

    [System.Serializable]
    private enum Tooltips : int
    {
        HEARTRATE_RANGES = 101,
        HEARTRATE_SLIDER = 102,
        HEARTRATE_PULSOID_CONNECT = 103,
        HEARTRATE_PULSOID_FEED = 104,
        HEARTRATE_FILE = 105,
        HEARTRATE_BLUETOOTH = 106,
        HEARTRATE_ANT_PLUS = 107,
        HEARTRATE_HYPERATE = 108,
        HEARTRATE_WEBSOCKET = 109,
        HEARTRATE_FITBIT = 110,

        OUTPUT_PARAMS = 201,
        OUTPUT_COLOR = 202,
        OUTPUT_EXPRESSION = 203,
        OUTPUT_COPY = 204,
        OUTPUT_HOTKEY = 205,
        OUTPUT_PROFILE = 206,
        OUTPUT_VFX = 207,
        OUTPUT_WIDGET = 208,

        SETTINGS_API = 301,
    }
}
