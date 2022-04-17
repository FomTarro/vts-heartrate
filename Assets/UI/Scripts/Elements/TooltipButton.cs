﻿using UnityEngine;
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
 
    private void DisplayTooltip(Tooltips tip){
        switch(tip){
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
            case Tooltips.HEARTRATE_PULSOID_CONNECT:
                UIManager.Instance.ShowPopUp(
                    "input_pulsoid_app_title",
                    "input_pulsoid_app_tooltip",
                    new PopUp.PopUpOption(
                        "input_pulsoid_button_visit",
                        true,
                        () => { Application.OpenURL("https://www.pulsoid.net"); })
                );
                break;
            case Tooltips.HEARTRATE_PULSOID_FEED:
                UIManager.Instance.ShowPopUp(
                    "Pulsoid (Widget Feed)",
                    "<b>Pulsoid</b> is a free third-party app for Android/iOS which allows for easy, reliable connectivity to a wide set of heartrate monitors.\n\n"+
                    "Once you have a Pulsoid account, you can use this input method to collect heartrate data from the service.\n\n" +
                    "To use this input method, navigate to your <b>Pulsoid account page</b>, and then go to <b>Widgets -> Advanced</b>. From there, you should find a <b>'Feed URL'</b> which you must paste into the plugin.\n\n" +
                    "Please note that Pulsoid may be deprecating this input method in the future, in favor of direct app connectivity.",
                    new PopUp.PopUpOption(
                        "Visit pulsoid.net",
                        true,
                        () => { Application.OpenURL("https://www.pulsoid.net"); })
                );
                break;
            case Tooltips.HEARTRATE_FILE:
                UIManager.Instance.ShowPopUp(
                    "Read from File",
                    "Use this input method to read heartrate data from an external file.\n"+
                    "The file must simply contain the numeric heartrate value in plain text. File path must be absolute.\n\n"+
                    "Useful if you have another program that can output heartrate data.\n"
                );
                break;
            case Tooltips.HEARTRATE_ANT_PLUS:
                UIManager.Instance.ShowPopUp(
                    "ANT+",
                    "<b>ANT+</b> is a low-power protocol supported by many sport and fitness sensors.\n\n"+
                    "This input method allows for direct connection to your ANT+ device, provided that you have a <b>USB receiver</b> plugged in.\n\n"+
                    "By clicking the <b>'Refresh' button</b>, this plugin will begin a continuous scan for devices that output heartrate data. "+
                    "Then, simply select one from the dropdown and click the <b>'Connect' button</b>.\n\n"+
                    "Please note that this plugin is not an officially licensed or certified affiliate of the ANT+ Brand.",
                    new PopUp.PopUpOption(
                        "Visit thisisant.com",
                        true,
                        () => { Application.OpenURL("https://www.thisisant.com/company/"); })
                );
                break;
            case Tooltips.OUTPUT_COLOR:
                UIManager.Instance.ShowPopUp(
                    "Art Mesh Tint",
                    "This output will gradually tint the matched <b>Art Meshes</b> the desired color, based on your current heartrate.\n\n"+
                    "<b>Art Meshes</b> are matched as long as their names or tags contain <b>any</b> of the provided list of text to match. For example, providing the text '<b>head,mouth,neck</b>' will match Art Meshes with names like '<b>forehead</b>', '<b>outermouth2</b>', and '<b>leftneckside1</b>'.\n\n"+
                    "Text to match must be <b>comma separated</b> and should <b>not contain spaces</b>.\n\n"+
                    "If you are unsure of what your Art Meshes are named, a great web-tool was developed by <b>Hawkbar</b> called <b>VTubeStudioTagger</b>, which offers an intuitive way to discover the names of your model's Art Meshes.",
                    new PopUp.PopUpOption(
                        "Visit VTubeStudioTagger",
                        true,
                        () => { Application.OpenURL("https://hawk.bar/VTubeStudioTagger/"); })
                );
                break;
            case Tooltips.OUTPUT_PARAMS:
                UIManager.Instance.ShowPopUp(
                    "Custom Parameters",
                    "This plugin outputs four custom tracking parameters for use. They are as follows:\n"+
                    "<b>VTS_Heartrate_BPM</b>: A value that represents the actual current BPM from 0 to 255, rather than an interpolated value.\n"+
                    "<b>VTS_Heartrate_Breath</b>: A value that oscillates between 0.0 and 1.0 with a frequency slower than Pulse, suitable for controlling your model's <b>ParamBreath</b> output.\n"+
                    "<b>VTS_Heartrate_Linear</b>: A value that scales from 0.0 to 1.0 as your heartrate moves across the expected range.\n"+
                    "<b>VTS_Heartrate_Pulse</b>: A value that oscillates between 0.0 and 1.0 with a frequency exactly matching your heartrate.\n",
                     new PopUp.PopUpOption(
                        "Learn how to use Custom Parameters",
                        true,
                        () => { Application.OpenURL("https://github.com/DenchiSoft/VTubeStudio/wiki/Plugins#what-are-custom-parameters"); })
                );
                break;
            case Tooltips.OUTPUT_EXPRESSION:
                UIManager.Instance.ShowPopUp(
                    "Expression Trigger",
                    "This output will cause an <b>Expression</b> to activate or deactivate when the current heartrate <b>exceeds a given threshold</b>. The Expression will then resume its original state once the heartrate dips back below the given threshold.\n"
                );
                break;
            case Tooltips.OUTPUT_COPY:
                UIManager.Instance.ShowPopUp(
                    "Copy Profile Settings",
                    "This feature allows you to <b>copy all of your output settings</b> (art mesh tints, expression triggers) from one model to your currently loaded model.\n\n"+
                    "As a result, any settings configured for your currently loaded model will be permanently erased.\n"
                );
                break;
            default:
            break;
        }
    }
 
    [System.Serializable]
    private enum Tooltips : int {
        HEARTRATE_RANGES = 101,
        HEARTRATE_SLIDER = 102,
        HEARTRATE_PULSOID_CONNECT = 103,
        HEARTRATE_PULSOID_FEED = 104,
        HEARTRATE_FILE = 105,
        HEARTRATE_BLUETOOTH = 106,
        HEARTRATE_ANT_PLUS = 107,
 
        OUTPUT_PARAMS = 201,
        OUTPUT_COLOR = 202,
        OUTPUT_EXPRESSION = 203,
        OUTPUT_COPY = 204,
    }
}
