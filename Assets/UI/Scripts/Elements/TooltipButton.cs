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

    private void DisplayTooltip(Tooltips tip){
        switch(tip){
            case Tooltips.HEARTRATE_RANGES:
                UIManager.Instance.ShowPopUp(
                    "Heartrate Ranges",
                    "Input your expected range of heartrates here. <b>Resting</b> refers to your low-stress heartrate, while <b>Maximum</b> refers to your high-stress heartrate.\n\n"+
                    "The values of the various <b>Outputs</b> will be scaled based on where your current heartrate lands along this range."
                );
                break;
            case Tooltips.HEARTRATE_SLIDER:
                UIManager.Instance.ShowPopUp(
                    "Test Slider",
                    "Use the slider for quick testing of different heartrate values.\nThe slider range is from 0 to 255."
                );
                break;
            case Tooltips.HEARTRATE_PULSOID_CONNECT:
                UIManager.Instance.ShowPopUp(
                    "Pulsoid App",
                    "<b>Pulsoid</b> is a free third-party app for Android/iOS which allows for easy, reliable connectivity to a wide set of heartrate monitors.\n"+
                    "Once you have a Pulsoid account, you can use this input method to collect heartrate data from the service.\n\n" +
                    "By clicking the <b>'Login' button</b>, you will be asked to grant this plugin permission to connect to your account. You will then be given an <b>'Authentication Token'</b> which you must paste in to the plugin.",
                    new PopUp.PopUpOption(
                        "Visit pulsoid.net",
                        Color.white,
                        () => { Application.OpenURL("https://www.pulsoid.net"); })
                );
                break;
            case Tooltips.HEARTRATE_PULSOID_FEED:
                UIManager.Instance.ShowPopUp(
                    "Pulsoid Feed Reference",
                    "<b>Pulsoid</b> is a free third-party app for Android/iOS which allows for easy, reliable connectivity to a wide set of heartrate monitors.\n"+
                    "Once you have a Pulsoid account, you can use this input method to collect heartrate data from the service.\n\n" +
                    "To use this input method, navigate to your <b>Pulsoid account page</b>, and then go to <b>Widgets -> Advanced</b>. From there, you should find a <b>'Feed URL'</b> which you must paste into the plugin.\n\n" +
                    "Please note that Pulsoid may be deprecating this input method in the future, in favor of direct app connectivity.",
                    new PopUp.PopUpOption(
                        "Visit pulsoid.net",
                        Color.white,
                        () => { Application.OpenURL("https://www.pulsoid.net"); })
                );
                break;
            case Tooltips.HEARTRATE_FILE:
                UIManager.Instance.ShowPopUp(
                    "Read from File",
                    "Use this input method to read heartrate data from an external file.\n"+
                    "The file must simply contain the numeric heartrate value in plain text. File path must be absolute.\n\n"+
                    "Useful if you have another program that can output heartrate data."
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
                        Color.white,
                        () => { Application.OpenURL("https://hawk.bar/VTubeStudioTagger/"); })
                );
                break;
            case Tooltips.OUTPUT_PARAMS:
                UIManager.Instance.ShowPopUp(
                    "Custom Parameters",
                    "This plugin outputs three custom tracking parameters for use. They are as follows:\n"+
                    "<b>VTS_Heartrate_Linear</b>: A value that scales from 0.0 to 1.0 as your heartrate moves across the expected range.\n"+
                    "<b>VTS_Heartrate_Pulse</b>: A value that oscillates between 0.0 and 1.0 with a frequency exactly matching your heartrate.\n"+
                    "<b>VTS_Heartrate_Breath</b>: A value that oscillates between 0.0 and 1.0 with a frequency slower than Pulse, suitable for controlling your model's <b>ParamBreath</b> output.\n"
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

        OUTPUT_PARAMS = 201,
        OUTPUT_COLOR = 202
    }
}
