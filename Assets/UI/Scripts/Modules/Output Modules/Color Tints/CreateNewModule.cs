using UnityEngine;

public class CreateNewModule : MonoBehaviour
{
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
