using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class LegacySaveData_v0_1_0 {
    public int minRate = 0;
    public int maxRate = 0;
    public List<ColorInputModule.SaveData> colors = new List<ColorInputModule.SaveData>();
    public List<HeartrateInputModule.SaveData> inputs = new List<HeartrateInputModule.SaveData>(); 

    public override string ToString()
    {
        return JsonUtility.ToJson(this, true);
    }
}