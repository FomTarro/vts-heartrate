using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateNewModule : MonoBehaviour
{
    public void CreateColorModule(){
        HeartrateManager.Instance.Plugin.CreateColorInputModule(new ColorInputModule.SaveData());
    }

    public void CreateExpressionModule(){
        HeartrateManager.Instance.Plugin.CreateExpressionModule(new ExpressionModule.SaveData());
    }
}
