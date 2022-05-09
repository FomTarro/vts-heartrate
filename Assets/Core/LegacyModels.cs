using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class LegacyGlobalSaveData_v0_1_0 {
    public int minRate = 0;
    public int maxRate = 0;
    public List<ColorInputModule.SaveData> colors = new List<ColorInputModule.SaveData>();
    public List<HeartrateInputModule.SaveData> inputs = new List<HeartrateInputModule.SaveData>(); 

    public override string ToString()
    {
        return JsonUtility.ToJson(this, true);
    }
}

[System.Serializable]
public class LegacyModelSaveData_v1_0_0 {
    public string version;
    public string modelID;
    public string modelName;
    public List<ColorInputModule.SaveData> colors = new List<ColorInputModule.SaveData>();
    public List<LegacyExpressionSaveData_v1_0_0> expressions = new List<LegacyExpressionSaveData_v1_0_0>();

    public override string ToString()
    {
        return JsonUtility.ToJson(this, true);
    }
}

[System.Serializable]
public class LegacyExpressionSaveData_v1_0_0 {
    public string expressionFile;
    public int threshold;
    public bool shouldActivate = true;

    public override string ToString()
    {
        return JsonUtility.ToJson(this);
    }
}