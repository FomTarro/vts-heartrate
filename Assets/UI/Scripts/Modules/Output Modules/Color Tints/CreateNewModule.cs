﻿using UnityEngine;

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

    public void CreateColorModule(){
        HeartrateManager.Instance.Plugin.CreateColorTintModule(new ColorInputModule.SaveData());
    }

    public void CreateExpressionModule(){
        HeartrateManager.Instance.Plugin.CreateExpressionModule(new ExpressionModule.SaveData());
    }

    public void CreateHotkeyModule(){
        HeartrateManager.Instance.Plugin.CreateHotkeyModule(new HotkeyModule.SaveData());
    }

    public void CreateNewProfile(){
        SaveDataManager.Instance.CreateNewProfileForCurrentModel();
        HeartrateManager.Instance.Plugin.FromModelSaveData(new HeartratePlugin.ModelSaveData());
        SaveDataManager.Instance.WriteModelSaveData(HeartrateManager.Instance.Plugin.ToModelSaveData());
    }
}
