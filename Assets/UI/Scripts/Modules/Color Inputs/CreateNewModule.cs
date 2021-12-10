using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateNewModule : MonoBehaviour
{
    public void CreateModule(){
        HeartrateManager.Instance.Plugin.CreateColorInputModule(new ColorInputModule.SaveData());
    }
}
