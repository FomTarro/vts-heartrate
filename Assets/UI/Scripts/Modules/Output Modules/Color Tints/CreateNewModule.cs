using UnityEngine;

public class CreateNewModule : MonoBehaviour
{

    public void CreateNewModulePrompt(){
        UIManager.Instance.ShowPopUp("output_create_title", "output_create_tooltip",
            new PopUp.PopUpOption("output_artmesh_title", ColorUtils.ColorPreset.GREEN, () => {
                CreateColorModule();
                UIManager.Instance.HidePopUp();
            }),
            new PopUp.PopUpOption("output_expressions_title", ColorUtils.ColorPreset.GREEN, () => {
                CreateExpressionModule();
                UIManager.Instance.HidePopUp();
            }),
            new PopUp.PopUpOption("output_hotkey_title", ColorUtils.ColorPreset.GREEN, () => {
                CreateHotkeyModule();
                UIManager.Instance.HidePopUp();
            }));
    }

    private void CreateColorModule(){
        HeartrateManager.Instance.Plugin.CreateColorTintModule(new ColorInputModule.SaveData());
    }

    private void CreateExpressionModule(){
        HeartrateManager.Instance.Plugin.CreateExpressionModule(new ExpressionModule.SaveData());
    }

    private void CreateHotkeyModule(){
        HeartrateManager.Instance.Plugin.CreateHotkeyModule(new HotkeyModule.SaveData());
    }

}
