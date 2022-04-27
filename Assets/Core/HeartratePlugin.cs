using VTS.Networking.Impl;
using VTS.Models.Impl;
using VTS.Models;
using VTS;

using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;
public class HeartratePlugin : VTSPlugin
{
    #region Member Variables
    private string GLOBAL_SAVE_PATH = "";
    private string MODEL_SAVE_PATH = "";
    public string SavePath { get { return Application.persistentDataPath; } }
    private VTSCurrentModelData _currentModel = new VTSCurrentModelData();

    [SerializeField]
    [Range(50, 120)]
    private int _heartRate = 70;
    public int HeartRate { get { return Math.Max(0, this._heartRate); } }
    private int _maxRate = 100;
    private int _minRate = 70;
    private ShiftingAverage _average = new ShiftingAverage(60);

    private const string PARAMETER_LINEAR = "VTS_Heartrate_Linear";
    private const string PARAMETER_SINE_PULSE = "VTS_Heartrate_Pulse";
    private const string PARAMETER_SINE_BREATH = "VTS_Heartrate_Breath";
    private const string PARAMETER_BPM = "VTS_Heartrate_BPM";
    private Dictionary<String, float> _parameterMap = new Dictionary<string, float>();
    public Dictionary<String, float> ParameterMap { get { return this._parameterMap; } }
    private List<VTSParameterInjectionValue> _paramValues = new List<VTSParameterInjectionValue>();
    private VTSParameterInjectionValue _linear = new VTSParameterInjectionValue();
    private VTSParameterInjectionValue _pulse = new VTSParameterInjectionValue();
    private VTSParameterInjectionValue _breath = new VTSParameterInjectionValue();
    private VTSParameterInjectionValue _bpm = new VTSParameterInjectionValue();
    private OscillatingValue _oscillatingPulse = new OscillatingValue();
    private OscillatingValue _oscillatingBreath = new OscillatingValue();
    private const int PARAMETER_MAX_VALUE = 1000000;

    [Header("Colors")]
    [SerializeField]
    private ColorInputModule _colorPrefab = null;
    [SerializeField]
    private RectTransform _colorListParent = null;
    [SerializeField]
    private List<ColorInputModule> _colors = new List<ColorInputModule>();

    [Header("Expressions")]
    [SerializeField]
    private ExpressionModule _expressionPrefab = null;
    private List<string> _expressions = new List<string>();
    public List<string> Expressions { get { return this._expressions; } }
    [SerializeField]
    private List<ExpressionModule> _expressionModules = new List<ExpressionModule>();

    [Header("Hotkeys")]
    [SerializeField]
    private HotkeyModule _hotkeyPrefab = null;
    private List<HotkeyListItem> _hotkeys = new List<HotkeyListItem>();
    public List<HotkeyListItem> Hotkeys { get { return this._hotkeys; } }
    [SerializeField]
    private List<HotkeyModule> _hotkeyModules = new List<HotkeyModule>();

    [Header("Input Modules")]
    [SerializeField]
    private List<HeartrateInputModule> _heartrateInputs = new List<HeartrateInputModule>();
    public List<HeartrateInputModule> HeartrateInputs { get { return new List<HeartrateInputModule>(this._heartrateInputs); }}
    private HeartrateInputModule _activeModule = null;
    public String ActiveInputModule { get { return this._activeModule != null? this._activeModule.ToString() : null; }}
    [SerializeField]
    private HeartrateRangesInputModule _heartrateRanges = null;

    [Header("Misc.")]
    [SerializeField]
    private StatusIndicator _connectionStatus = null;
    #endregion
    
    #region Lifecycle

    private void Start()
    {
        this.GLOBAL_SAVE_PATH = Path.Combine(Application.persistentDataPath, "save.json");
        this.MODEL_SAVE_PATH = Path.Combine(Application.persistentDataPath, "models");
        LoadGlobalData(); 
        UIManager.Instance.GoTo(UIManager.Tabs.HEARTRATE_INPUTS);
        Connect();
    }

    public void Connect(){
        Initialize(
            new WebSocketSharpImpl(),
            new JsonUtilityImpl(),
            new TokenStorageImpl(),
            () => {
                HttpUtils.ConnectionStatus status = new HttpUtils.ConnectionStatus();
                status.status = HttpUtils.ConnectionStatus.Status.CONNECTED;
                this._connectionStatus.SetStatus(status);
                // LoggingManager.Instance.Log("Connected to VTube Studio!");
                this._paramValues = new List<VTSParameterInjectionValue>();
                CreateNewParameter(PARAMETER_LINEAR, "", 1,
                (s) => {
                    _linear.id = PARAMETER_LINEAR;
                    _linear.value = 0;
                    _paramValues.Add(_linear);
                },
                (e) => {
                    Debug.LogError(e.ToString());
                });
                CreateNewParameter(PARAMETER_SINE_PULSE, "", 1,
                (s) => {
                    _pulse.id = PARAMETER_SINE_PULSE;
                    _pulse.value = 0;
                    _paramValues.Add(_pulse);
                },
                (e) => {
                    Debug.LogError(e.ToString());
                });
                CreateNewParameter(PARAMETER_SINE_BREATH, "", 1,
                (s) => {
                    _breath.id = PARAMETER_SINE_BREATH;
                    _breath.value = 0;
                    _paramValues.Add(_breath);
                },
                (e) => {
                    Debug.LogError(e.ToString());
                });
                CreateNewParameter(PARAMETER_BPM, "", 255,
                (s) => {
                    _bpm.id = PARAMETER_BPM;
                    _bpm.value = 0;
                    _paramValues.Add(_bpm);
                },
                (e) => {
                    Debug.LogError(e.ToString());
                });
            },
            () => {
                HttpUtils.ConnectionStatus status = new HttpUtils.ConnectionStatus();
                status.status = HttpUtils.ConnectionStatus.Status.DISCONNECTED;
                this._connectionStatus.SetStatus(status);
                //LoggingManager.Instance.Log("Disconnected from VTube Studio!");
            },
            () => {
                HttpUtils.ConnectionStatus status = new HttpUtils.ConnectionStatus();
                status.status = HttpUtils.ConnectionStatus.Status.ERROR;
                this._connectionStatus.SetStatus(status);
                // LoggingManager.Instance.Log("Error connecting to VTube Studio!");
            });
            HttpUtils.ConnectionStatus connect = new HttpUtils.ConnectionStatus();
            connect.status = HttpUtils.ConnectionStatus.Status.CONNECTING;
            this._connectionStatus.SetStatus(connect);
    }

    public void SetMinRate(int rate){
        this._minRate = Mathf.Clamp(0, rate, 255);
    }

    public void SetMaxRate(int rate){
        this._maxRate = Mathf.Clamp(0, rate, 255);
    }


    private void OnValidate(){
        this._heartrateInputs = new List<HeartrateInputModule>(FindObjectsOfType<HeartrateInputModule>());
        SortInputModules();
    }

    private void SortInputModules(){
        this._heartrateInputs.Sort((a, b) => { return a.Type - b.Type; });
    }

    private void OnApplicationQuit(){
        SaveGlobalData();
        SaveModelData(this._currentModel);
    }

    private void Update(){
        int priorHeartrate = this._heartRate;
        this._average.AddValue(this._activeModule != null ? this._activeModule.GetHeartrate() : 0);
        this._heartRate = Mathf.RoundToInt(this._average.Average);
        float interpolation = Mathf.Clamp01((float)(this._heartRate-this._minRate)/(float)(this._maxRate - this._minRate));

        if(this.IsAuthenticated){
            // see which model is currently loaded
            GetCurrentModel(
                (s) => {
                    if(!s.data.modelID.Equals(this._currentModel.data.modelID)){
                        // model has changed
                        SaveModelData(this._currentModel);
                        LoadModelData(s.data.modelID);
                    }
                    this._currentModel = s; 
                },
                (e) => {
                    this._currentModel = new VTSCurrentModelData();
                }
            );
            // get all expressions for currently loaded model
            GetExpressionStateList(
                (s) => {
                    this._expressions.Clear();
                    foreach(ExpressionData expression in s.data.expressions){
                        this._expressions.Add(expression.file);
                    }
                    foreach(ExpressionModule module in this._expressionModules){
                        module.RefreshExpressionList();
                    }
                },
                (e) => {
                    Debug.LogError(e.data.message);
                }
            );

            GetHotkeysInCurrentModel(
                this._currentModel.data.modelID,
                (s) => {
                    this._hotkeys.Clear();
                    foreach(HotkeyData hotkey in s.data.availableHotkeys){
                        this._hotkeys.Add(new HotkeyListItem(
                            String.Format("[{0}] {1} ({2})", hotkey.type, hotkey.name, hotkey.hotkeyID),
                            hotkey.hotkeyID));
                    }
                    foreach(HotkeyModule module in this._hotkeyModules){
                        module.RefreshHotkeyList();
                    }
                },
                (e) => {
                    Debug.LogError(e.data.message);
                }
            );

            // apply art mesh tints
            foreach(ColorInputModule module in this._colors){
                module.ApplyColor(interpolation);
            }

            SortExpressionModules();
            // apply expressions
            foreach(ExpressionModule module in this._expressionModules){
                module.CheckModuleCondition(priorHeartrate, this._heartRate);
            }

            foreach(HotkeyModule module in this._hotkeyModules){
                module.CheckModuleCondition(priorHeartrate, this._heartRate);
            }

            // calculate tracking parameters
            _linear.value = interpolation;
            _bpm.value = this._heartRate;
            _breath.value = _oscillatingBreath.GetValue(
                Mathf.Clamp(((float)this.HeartRate - this._minRate) / 60f, 0.35f, PARAMETER_MAX_VALUE));
            _pulse.value = _oscillatingPulse.GetValue(
                Mathf.Clamp(((float)this.HeartRate) / 60f, 0f, PARAMETER_MAX_VALUE));

            if(_paramValues.Count > 0){
                this.InjectParameterValues(_paramValues.ToArray(),
                (s) => {
                    InjectedParamValuesToDictionary(_paramValues.ToArray());
                },
                (e) => {
                    Debug.Log(JsonUtility.ToJson(e));
                });
            }
        }
    }

    #endregion

    #region Parameters
    public void SetActiveHeartrateInput(HeartrateInputModule module){
        Debug.Log("Activating Input module: " + module);
        foreach(HeartrateInputModule m in this._heartrateInputs){
            m.gameObject.SetActive(false);
            m.Deactivate();
        }
        module.gameObject.SetActive(true);
        this._activeModule = module;
    }

    private void InjectedParamValuesToDictionary(VTSParameterInjectionValue[] values){
        this._parameterMap = new Dictionary<string, float>();
        foreach(VTSParameterInjectionValue parameter in values){
            this._parameterMap.Add(parameter.id, parameter.value);
        }
    }

    private void CreateNewParameter(string paramName, string paramDescription, int paramMax, 
                                    System.Action<VTSParameterCreationData> onSuccess, System.Action<VTSErrorData> onError){
        VTSCustomParameter newParam = new VTSCustomParameter();
        newParam.defaultValue = 0;
        newParam.min = 0;
        newParam.max = paramMax;
        newParam.parameterName = paramName;
        newParam.explanation = paramDescription;
        this.AddCustomParameter(
            newParam,
            onSuccess,
            onError);
    }

    #endregion

    #region Module Creation

    public void CreateColorInputModule(ColorInputModule.SaveData module){
        ColorInputModule instance = Instantiate<ColorInputModule>(this._colorPrefab, Vector3.zero, Quaternion.identity, this._colorListParent);
        int index = Math.Max(1, TransformUtils.GetActiveChildCount(this._colorListParent) - 3);
        instance.transform.SetSiblingIndex(index);
        this._colors.Add(instance);
        if(module != null){
            instance.FromSaveData(module);
        }
    }

    public void DestroyColorInputModule(ColorInputModule module){
        if(this._colors.Contains(module)){
            this._colors.Remove(module);
            Destroy(module.gameObject);
        }
    }

    public void CreateExpressionModule(ExpressionModule.SaveData module){
        ExpressionModule instance = Instantiate<ExpressionModule>(this._expressionPrefab, Vector3.zero, Quaternion.identity, this._colorListParent);
        int index = Math.Max(1, TransformUtils.GetActiveChildCount(this._colorListParent) - 3);
        instance.transform.SetSiblingIndex(index);
        this._expressionModules.Add(instance);
        SortExpressionModules();
        if(module != null){
            instance.FromSaveData(module);
        }
    }

    public void DestroyExpressionModule(ExpressionModule module){
        if(this._expressionModules.Contains(module)){
            this._expressionModules.Remove(module);
            SortExpressionModules();
            Destroy(module.gameObject);
        }
    }

    private void SortExpressionModules(){
        this._expressionModules.Sort((a, b) => { return a.Threshold.CompareTo(b.Threshold); });
    }

    public void CreateHotkeyModule(HotkeyModule.SaveData module){
        HotkeyModule instance = Instantiate<HotkeyModule>(this._hotkeyPrefab, Vector3.zero, Quaternion.identity, this._colorListParent);
        int index = Math.Max(1, TransformUtils.GetActiveChildCount(this._colorListParent) - 3);
        instance.transform.SetSiblingIndex(index);
        this._hotkeyModules.Add(instance);
        SortExpressionModules();
        if(module != null){
            instance.FromSaveData(module);
        }
    }

    public void DestroyHotkeyModule(HotkeyModule module){
        if(this._hotkeyModules.Contains(module)){
            this._hotkeyModules.Remove(module);
            SortExpressionModules();
            Destroy(module.gameObject);
        }
    }

    #endregion

    #region Data Serialization

    public Dictionary<string, string> GetModelDataNameMap(){
        Dictionary<string, string> dict = new Dictionary<string, string>();
        foreach(string s in Directory.GetFiles(this.MODEL_SAVE_PATH)){
            string text = File.ReadAllText(s);
            ModelSaveData data = JsonUtility.FromJson<ModelSaveData>(text);
            dict.Add(String.Format("{0} ({1})", data.modelName, data.modelID), data.modelID);
        }
        return dict;
    }

    private void SaveGlobalData(){
        GlobalSaveData data = new GlobalSaveData();
        data.version = Application.version;
        data.maxRate = this._maxRate;
        data.minRate = this._minRate;
        data.activeInput = this._activeModule.Type;
        foreach(HeartrateInputModule module in this._heartrateInputs){
            data.inputs.Add(module.ToSaveData());
        }
        data.language = Localization.LocalizationManager.Instance.CurrentLanguage;
        File.WriteAllText(this.GLOBAL_SAVE_PATH, data.ToString());
    }

    private void SaveModelData(VTSCurrentModelData currentModel){
        ModelSaveData data = new ModelSaveData();
        data.version = Application.version;
        data.modelName = currentModel.data.modelName;
        data.modelID = currentModel.data.modelID;
        foreach(ColorInputModule module in this._colors){
            data.colors.Add(module.ToSaveData());
        }
        foreach(ExpressionModule module in this._expressionModules){
            data.expressions.Add(module.ToSaveData());
        }
        foreach(HotkeyModule module in this._hotkeyModules){
            data.hotkeys.Add(module.ToSaveData());
        }
        
        if(!Directory.Exists(this.MODEL_SAVE_PATH)){
            Directory.CreateDirectory(this.MODEL_SAVE_PATH);
        }

        if(data.modelID != null && data.modelID.Length > 0){
            Debug.Log("Saving Model: " + data.modelID);
            string filePath = Path.Combine(this.MODEL_SAVE_PATH, data.modelID+".json");
            File.WriteAllText(filePath, data.ToString());
        }
    }

    private void LoadGlobalData(){
        GlobalSaveData data;
        if(File.Exists(this.GLOBAL_SAVE_PATH)){
            string content = File.ReadAllText(this.GLOBAL_SAVE_PATH);
            data = JsonUtility.FromJson<GlobalSaveData>(content);
            ModernizeLegacyGlobalSaveData(data, content);
    
        }else{
            data = new GlobalSaveData();
            HeartrateInputModule.SaveData defaultData = new HeartrateInputModule.SaveData();
            defaultData.type = HeartrateInputModule.InputType.SLIDER;
            defaultData.values.value = 70f;
            data.inputs.Add(defaultData);
            data.activeInput = HeartrateInputModule.InputType.SLIDER;
        }
        this._maxRate = data.maxRate;
        this._heartrateRanges.SetMaxRate(this._maxRate.ToString());
        this._minRate = data.minRate;
        this._heartrateRanges.SetMinRate(this._minRate.ToString());
        this.SetActiveHeartrateInput(this._heartrateInputs.Find((x => x.Type == data.activeInput)));
    
        foreach(HeartrateInputModule.SaveData module in data.inputs){
            foreach(HeartrateInputModule m in this._heartrateInputs){
                if(m.Type == module.type){
                    m.FromSaveData(module);
                }
            }
        }
        if(data.language != 0){
            Localization.LocalizationManager.Instance.SwitchLanguage(data.language);
        }
    }

    private void ModernizeLegacyGlobalSaveData(GlobalSaveData data, string content){
        //TODO: this should be some kind of iterative function so that it migrates adjacent versions in order,
        // ie 0.1.0 to 0.2.0 to 1.0.0 to 1.1.0 etc
        // this should probably also return a modern save file?
        string version = data.version;
        switch(version){
            case null:
            case "":
            case "0.1.0":
            LegacyGlobalSaveData_v0_1_0 legacyData = JsonUtility.FromJson<LegacyGlobalSaveData_v0_1_0>(content);
            Debug.Log("Legacy Global Data detected: v"+version);
            if(legacyData.colors != null && legacyData.colors.Count > 0){
                // make a new ModelSaveData, apply it to the first loaded model.
                ModelSaveData modelData = new ModelSaveData();
                modelData.colors = legacyData.colors;
                LoadModelData(modelData);
            }
            data.activeInput = HeartrateInputModule.InputType.SLIDER;
            break;
            case "1.0.0":
            data.activeInput = HeartrateInputModule.InputType.SLIDER;
            break;
        }
    }

    private void ModernizeLegacyModelSaveData(ModelSaveData data, string content){
        string version = data.version;
        switch(version){
            case null:
            case "":
            case "1.0.0":
            LegacyModelSaveData_v1_0_0 legacyData = JsonUtility.FromJson<LegacyModelSaveData_v1_0_0>(content);
            for(int i = 0; i < legacyData.expressions.Count; i++){
                if(i < data.expressions.Count){
                    data.expressions[i].behavior = legacyData.expressions[i].shouldActivate ? 
                    ExpressionModule.TriggerBehavior.ACTIVATE_ABOVE_DEACTIVATE_BELOW : 
                    ExpressionModule.TriggerBehavior.DEACTIVATE_ABOVE_ACTIVATE_BELOW;
                }
            }
            Debug.Log("Legacy Model Data detected: v"+version);
            break;
        }
    }

    private void LoadModelData(string modelID){
        ModelSaveData data;
        if(!Directory.Exists(this.MODEL_SAVE_PATH)){
            Directory.CreateDirectory(this.MODEL_SAVE_PATH);
        }
        Debug.Log("Loading Model: " + modelID);
        string filePath = Path.Combine(this.MODEL_SAVE_PATH, modelID+".json");
        if(File.Exists(filePath)){
            string text = File.ReadAllText(filePath);
            data = JsonUtility.FromJson<ModelSaveData>(text);
            ModernizeLegacyModelSaveData(data, text);
            LoadModelData(data);
        }else{
            // if no data exists for this model, just wipe the slate clean
            ClearCurrentData();
        }
    }

    private void LoadModelData(ModelSaveData data){
        ClearCurrentData();
        foreach(ColorInputModule.SaveData module in data.colors){
            CreateColorInputModule(module);
        }
        foreach(ExpressionModule.SaveData module in data.expressions){
            CreateExpressionModule(module);
        }
        foreach(HotkeyModule.SaveData module in data.hotkeys){
            CreateHotkeyModule(module);
        }
    }

    private void ClearCurrentData(){
        List<ColorInputModule> tempColor = new List<ColorInputModule>(this._colors);
        foreach(ColorInputModule c in tempColor){
            DestroyColorInputModule(c);
        }
        List<ExpressionModule> tempEmotion = new List<ExpressionModule>(this._expressionModules);
        foreach(ExpressionModule e in tempEmotion){
            DestroyExpressionModule(e);
        }
        List<HotkeyModule> tempHotkey = new List<HotkeyModule>(this._hotkeyModules);
        foreach(HotkeyModule h in tempHotkey){
            DestroyHotkeyModule(h);
        }
    }

    public void CopyModelData(string modelID, string modelName){
        UIManager.Instance.ShowPopUp(
            "Confrim Data Copy",
            String.Format("Are you sure that you want to <b>copy model settings data</b> from <b>{0}</b> to your currently loaded model, <b>{1}</b>?\n"+
            "Doing so will <b>permanently erase</b> any settings your currently loaded model has configured.", modelName, this._currentModel.data.modelName),
            new PopUp.PopUpOption(
                "Proceed",
                true,
                () => {
                    LoadModelData(modelID);
                    UIManager.Instance.HidePopUp();
                }),
            new PopUp.PopUpOption(
                "Cancel",
                false,
                () => {
                    UIManager.Instance.HidePopUp();
                })
        );
    }

    [System.Serializable]
    public class GlobalSaveData {
        public string version;
        public int minRate = 0;
        public int maxRate = 0;
        public HeartrateInputModule.InputType activeInput;
        public List<HeartrateInputModule.SaveData> inputs = new List<HeartrateInputModule.SaveData>(); 
        public Localization.SupportedLanguage language;

        public override string ToString()
        {
            return JsonUtility.ToJson(this, true);
        }
    }

    [System.Serializable]
    public class ModelSaveData {
        public string version;
        public string modelID;
        public string modelName;
        public List<ColorInputModule.SaveData> colors = new List<ColorInputModule.SaveData>();
        public List<ExpressionModule.SaveData> expressions = new List<ExpressionModule.SaveData>();
        public List<HotkeyModule.SaveData> hotkeys = new List<HotkeyModule.SaveData>();

        public override string ToString()
        {
            return JsonUtility.ToJson(this, true);
        }
    }

    #endregion
}
