using VTS.Networking.Impl;
using VTS.Models.Impl;
using VTS.Models;
using VTS;

using UnityEngine;
using System;
using System.Collections.Generic;

public class HeartratePlugin : VTSPlugin
{
    #region Member Variables

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
    private const string PARAMETER_BPM_ONES = "VTS_Heartrate_BPM_Ones";
    private const string PARAMETER_BPM_TENS = "VTS_Heartrate_BPM_Tens";
    private const string PARAMETER_BPM_HUNDREDS = "VTS_Heartrate_BPM_Hundreds";

    private const string PARAMETER_SAW_1 = "VTS_Heartrate_Repeat_1";
    private const string PARAMETER_SAW_5 = "VTS_Heartrate_Repeat_5";
    private const string PARAMETER_SAW_10 = "VTS_Heartrate_Repeat_10";
    private const string PARAMETER_SAW_20 = "VTS_Heartrate_Repeat_20";
    private const string PARAMETER_SAW_30 = "VTS_Heartrate_Repeat_30";
    private const string PARAMETER_SAW_60 = "VTS_Heartrate_Repeat_60";
    private const string PARAMETER_SAW_120 = "VTS_Heartrate_Repeat_120";

    private Dictionary<string, float> _parameterMap = new Dictionary<string, float>();
    public Dictionary<string, float> ParameterMap { get { return this._parameterMap; } }
    private List<VTSParameterInjectionValue> _paramValues = new List<VTSParameterInjectionValue>();
    private VTSParameterInjectionValue _linear = new VTSParameterInjectionValue();
    private VTSParameterInjectionValue _pulse = new VTSParameterInjectionValue();
    private VTSParameterInjectionValue _breath = new VTSParameterInjectionValue();
    private VTSParameterInjectionValue _bpm = new VTSParameterInjectionValue();
    private VTSParameterInjectionValue _bpm_ones = new VTSParameterInjectionValue();
    private VTSParameterInjectionValue _bpm_tens = new VTSParameterInjectionValue();
    private VTSParameterInjectionValue _bpm_hundreds = new VTSParameterInjectionValue();
    private VTSParameterInjectionValue _saw_wave_1 = new VTSParameterInjectionValue();
    private VTSParameterInjectionValue _saw_wave_5 = new VTSParameterInjectionValue();
    private VTSParameterInjectionValue _saw_wave_10 = new VTSParameterInjectionValue();
    private VTSParameterInjectionValue _saw_wave_30 = new VTSParameterInjectionValue();
    private VTSParameterInjectionValue _saw_wave_60 = new VTSParameterInjectionValue();
    private VTSParameterInjectionValue _saw_wave_120 = new VTSParameterInjectionValue();

    private OscillatingValue _oscillatingPulse = new OscillatingValue();
    private OscillatingValue _oscillatingBreath = new OscillatingValue();
    private SawValue _saw1 = new SawValue(1);
    private const int PARAMETER_MAX_VALUE = 1000000;

    [Header("Colors")]
    [SerializeField]
    private ColorInputModule _colorPrefab = null;
    [SerializeField]
    private RectTransform _colorListParent = null;
    [SerializeField]
    private List<ColorInputModule> _colors = new List<ColorInputModule>();
    public List<ColorInputModule> ColorModules { get { return new List<ColorInputModule>(this._colors); } }

    [Header("Expressions")]
    [SerializeField]
    private ExpressionModule _expressionPrefab = null;
    private List<string> _expressions = new List<string>();
    public List<string> Expressions { get { return this._expressions; } }
    [SerializeField]
    private List<ExpressionModule> _expressionModules = new List<ExpressionModule>();
    public List<ExpressionModule> ExpressionModules { get { return new List<ExpressionModule>(this._expressionModules); } }

    [Header("Hotkeys")]
    [SerializeField]
    private HotkeyModule _hotkeyPrefab = null;
    private List<HotkeyListItem> _hotkeys = new List<HotkeyListItem>();
    public List<HotkeyListItem> Hotkeys { get { return this._hotkeys; } }
    [SerializeField]
    private List<HotkeyModule> _hotkeyModules = new List<HotkeyModule>();
    public List<HotkeyModule> HotkeyModules { get { return new List<HotkeyModule>(this._hotkeyModules); } }

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

    public void OnLaunch(){
        FromGlobalSaveData(SaveDataManager.Instance.ReadGlobalSaveData());
        FromModelSaveData(SaveDataManager.Instance.ReadModelData(SaveDataManager.Instance.CurrentProfile));
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
                    this._linear.id = PARAMETER_LINEAR;
                    this._linear.value = 0;
                    this._linear.weight = 1;
                    this._paramValues.Add(this._linear);
                },
                (e) => {
                    Debug.LogError(e.ToString());
                });
                CreateNewParameter(PARAMETER_SINE_PULSE, "", 1,
                (s) => {
                    this._pulse.id = PARAMETER_SINE_PULSE;
                    this._pulse.value = 0;
                    this._pulse.weight = 1;
                    this._paramValues.Add(this._pulse);
                },
                (e) => {
                    Debug.LogError(e.ToString());
                });
                CreateNewParameter(PARAMETER_SINE_BREATH, "", 1,
                (s) => {
                    this._breath.id = PARAMETER_SINE_BREATH;
                    this._breath.value = 0;
                    this._breath.weight = 1;
                    this._paramValues.Add(this._breath);
                },
                (e) => {
                    Debug.LogError(e.ToString());
                });
                CreateNewParameter(PARAMETER_BPM, "", 255,
                (s) => {
                    this._bpm.id = PARAMETER_BPM;
                    this._bpm.value = 0;
                    this._bpm.weight = 1;
                    this._paramValues.Add(this._bpm);
                },
                (e) => {
                    Debug.LogError(e.ToString());
                });
                CreateNewParameter(PARAMETER_BPM_ONES, "", 9,
                (s) => {
                    this._bpm_ones.id = PARAMETER_BPM_ONES;
                    this._bpm_ones.value = 0;
                    this._bpm_ones.weight = 1;
                    this._paramValues.Add(this._bpm_ones);
                },
                (e) => {
                    Debug.LogError(e.ToString());
                });
                CreateNewParameter(PARAMETER_BPM_TENS, "", 9,
                (s) => {
                    this._bpm_tens.id = PARAMETER_BPM_TENS;
                    this._bpm_tens.value = 0;
                    this._bpm_tens.weight = 1;
                    this._paramValues.Add(this._bpm_tens);
                },
                (e) => {
                    Debug.LogError(e.ToString());
                });
                CreateNewParameter(PARAMETER_BPM_HUNDREDS, "", 9,
                (s) => {
                    this._bpm_hundreds.id = PARAMETER_BPM_HUNDREDS;
                    this._bpm_hundreds.value = 0;
                    this._bpm_hundreds.weight = 1;
                    this._paramValues.Add(this._bpm_hundreds);
                },
                (e) => {
                    Debug.LogError(e.ToString());
                });
                CreateNewParameter(PARAMETER_SAW_1, "", 1,
                (s) => {
                    this._saw_wave_1.id = PARAMETER_SAW_1;
                    this._saw_wave_1.value = 0;
                    this._saw_wave_1.weight = 1;
                    this._paramValues.Add(this._saw_wave_1);
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
        SaveDataManager.Instance.WriteGlobalSaveData(ToGlobalSaveData());
        SaveDataManager.Instance.WriteModelSaveData(ToModelSaveData());
    }

    private void Update(){
        int priorHeartrate = this._heartRate;
        this._average.AddValue(this._activeModule != null ? this._activeModule.GetHeartrate() : 0);
        this._heartRate = Mathf.RoundToInt(this._average.Average);
        float numerator =  Math.Max(0, (float)(this._heartRate-this._minRate));
        float demominator = Math.Max(1, (float)(this._maxRate - this._minRate));
        float interpolation = Mathf.Clamp01(numerator/demominator);
        // Data API message
        DataMessage dataMessage = new DataMessage(this.HeartRate);
        // see which model is currently loaded
        if(this.IsAuthenticated){
            GetCurrentModel(
                (s) => {
                    if(s.data.modelLoaded && !s.data.modelID.Equals(SaveDataManager.Instance.CurrentProfile.modelID)){
                        SaveDataManager.Instance.SetCurrentProfileInfo(
                            s.data.modelName, 
                            s.data.modelID);
                        FromModelSaveData(SaveDataManager.Instance.ReadModelData(SaveDataManager.Instance.CurrentProfile));
                        SaveDataManager.Instance.WriteModelSaveData(ToModelSaveData());
                    }else if(!s.data.modelLoaded && SaveDataManager.Instance.IsModelLoaded()){
                        SaveDataManager.Instance.CreateDefaultUnloadedProfile();
                        FromModelSaveData(SaveDataManager.Instance.ReadModelData(SaveDataManager.Instance.CurrentProfile));
                        SaveDataManager.Instance.WriteModelSaveData(ToModelSaveData());
                    }
                },
                (e) => {
                    SaveDataManager.Instance.CreateDefaultUnloadedProfile();
                    Debug.LogError(e.data.message);
                }
            );
        }
        // get all expressions for currently loaded model
        if(this.IsAuthenticated){
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
        }
        // get all hotkeys in currently loaded model
        if(this.IsAuthenticated){
            if(SaveDataManager.Instance.IsModelLoaded()){
                GetHotkeysInCurrentModel(
                    SaveDataManager.Instance.CurrentProfile.modelID,
                    (s) => {
                        try{
                        this._hotkeys.Clear();
                        foreach(HotkeyData hotkey in s.data.availableHotkeys){
                            this._hotkeys.Add(new HotkeyListItem(
                                string.Format(
                                    "[{0}] {1} <size=0>{2}</size>", 
                                    hotkey.type, 
                                    hotkey.name, 
                                    hotkey.hotkeyID),
                                hotkey.hotkeyID));
                        }
                        foreach(HotkeyModule module in this._hotkeyModules){
                            module.RefreshHotkeyList();
                        }
                        }catch(System.Exception e){
                            Debug.LogError(e);
                        }
                    },
                    (e) => {
                        Debug.LogError(e.data.message);
                    }
                );
            }
        }

        // apply art mesh tints
        foreach(ColorInputModule module in this._colors){
            module.ApplyColor(interpolation);
            DataMessage.Tint colorModule = new DataMessage.Tint(module.ModuleColor, module.ModuleInterpolatedColor, module.ModuleMatchers);
            dataMessage.data.tints.Add(colorModule);
        }

        SortExpressionModules();
        // apply expressions
        foreach(ExpressionModule module in this._expressionModules){
            module.CheckModuleCondition(priorHeartrate, this._heartRate);
        }
        SortHotkeyModules();
        // apply hotkeys
        foreach(HotkeyModule module in this._hotkeyModules){
            module.CheckModuleCondition(priorHeartrate, this._heartRate);
        }

        // calculate tracking parameters
        this._linear.value = interpolation;
        this._bpm.value = this._heartRate;
        this._bpm_ones.value = this._heartRate < 1 ? -1 :this._heartRate % 10;
        this._bpm_tens.value = this._heartRate < 10 ? -1 : (this._heartRate % 100) / 10;
        this._bpm_hundreds.value = this._heartRate < 100 ? -1 :this._heartRate / 100;
        this._breath.value = _oscillatingBreath.GetValue(
            Mathf.Clamp(((float)this.HeartRate - this._minRate) / 60f, 0.35f, PARAMETER_MAX_VALUE));
        this._pulse.value = _oscillatingPulse.GetValue(
            Mathf.Clamp(((float)this.HeartRate) / 60f, 0f, PARAMETER_MAX_VALUE));

        this._saw_wave_1.value = this._saw1.GetValue(this.HeartRate);

        if(this._paramValues.Count > 0 && this.IsAuthenticated){
            this.InjectParameterValues(this._paramValues.ToArray(),
            (s) => {
                InjectedParamValuesToDictionary(this._paramValues.ToArray());
            },
            (e) => {
                Debug.LogError(e.data.message);
            });
        }

        dataMessage.data.parameters.vts_heartrate_bpm = this._bpm.value;
        dataMessage.data.parameters.vts_heartrate_pulse = this._pulse.value;
        dataMessage.data.parameters.vts_heartrate_breath = this._breath.value;
        dataMessage.data.parameters.vts_heartrate_linear = this._linear.value;

        APIManager.Instance.SendData(dataMessage);
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
        int index = GetChildIndex();
        instance.transform.SetSiblingIndex(index);
        this._colors.Add(instance);
        if(module != null){
            instance.FromSaveData(module);
        }
    }

    public void DestroyColorInputModule(ColorInputModule module){
        if(this._colors.Contains(module)){
            this._colors.Remove(module);
            module.ApplyColor(0);
            Destroy(module.gameObject);
        }
    }

    public void CreateExpressionModule(ExpressionModule.SaveData module){
        ExpressionModule instance = Instantiate<ExpressionModule>(this._expressionPrefab, Vector3.zero, Quaternion.identity, this._colorListParent);
        int index = GetChildIndex();
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
        int index = GetChildIndex();
        instance.transform.SetSiblingIndex(index);
        this._hotkeyModules.Add(instance);
        SortHotkeyModules();
        if(module != null){
            instance.FromSaveData(module);
        }
    }

    public void DestroyHotkeyModule(HotkeyModule module){
        if(this._hotkeyModules.Contains(module)){
            this._hotkeyModules.Remove(module);
            SortHotkeyModules();
            Destroy(module.gameObject);
        }
    }

    private void SortHotkeyModules(){
        this._hotkeyModules.Sort((a, b) => { return a.Threshold.CompareTo(b.Threshold); });
    }

    private int GetChildIndex(){
        return Math.Max(1, TransformUtils.GetActiveChildCount(this._colorListParent) - 3);
    }

    #endregion

    #region Data Saving

    public GlobalSaveData ToGlobalSaveData(){
        GlobalSaveData data = new GlobalSaveData();
        data.version = Application.version;
        data.maxRate = this._maxRate;
        data.minRate = this._minRate;
        data.activeInput = this._activeModule.Type;
        foreach(HeartrateInputModule module in this._heartrateInputs){
            data.inputs.Add(module.ToSaveData());
        }
        data.language = Localization.LocalizationManager.Instance.CurrentLanguage;
        data.apiServerPort = APIManager.Instance.Port;
        return data;
    }

    public ModelSaveData ToModelSaveData(){
        ModelSaveData data = new ModelSaveData();
        SaveDataManager.ModelProfileInfo currentModel = SaveDataManager.Instance.CurrentProfile;
        data.version = Application.version;
        data.modelName = currentModel.modelName;
        data.modelID = currentModel.modelID;
        data.profileName = currentModel.profileName;
        data.profileID = currentModel.profileID;
        foreach(ColorInputModule module in this._colors){
            data.colors.Add(module.ToSaveData());
        }
        foreach(ExpressionModule module in this._expressionModules){
            data.expressions.Add(module.ToSaveData());
        }
        foreach(HotkeyModule module in this._hotkeyModules){
            data.hotkeys.Add(module.ToSaveData());
        }
        return data;
    }

    public void FromGlobalSaveData(GlobalSaveData data){    
        this._maxRate = data.maxRate;
        this._heartrateRanges.SetMaxRate(this._maxRate.ToString());
        this._minRate = data.minRate;
        this._heartrateRanges.SetMinRate(this._minRate.ToString());
        // Default to SLIDER if we can't find the provided input type
        HeartrateInputModule activeModule = this._heartrateInputs.Find((x => x.Type == data.activeInput));
        activeModule = activeModule != null ? activeModule : this._heartrateInputs.Find((x => x.Type == HeartrateInputModule.InputType.SLIDER));
        this.SetActiveHeartrateInput(activeModule);
    
        // Load settings for all input modules
        foreach(HeartrateInputModule.SaveData module in data.inputs){
            foreach(HeartrateInputModule m in this._heartrateInputs){
                if(m.Type == module.type){
                    m.FromSaveData(module);
                }
            }
        }

        if(!Application.version.Equals(data.version)){
            Debug.Log("Applying system language settings on new version");
            Localization.LocalizationManager.Instance.SwitchLanguage(Application.systemLanguage);
        }
        else if(data.language != 0){
            Localization.LocalizationManager.Instance.SwitchLanguage(data.language);
        }else{
            Debug.Log("Defaulting language to English as no settings were found");
            Localization.LocalizationManager.Instance.SwitchLanguage(Localization.SupportedLanguage.ENGLISH);
        }

        APIManager.Instance.SetPort(data.apiServerPort);
    }

    public void FromModelSaveData(ModelSaveData data){
        // wipe current settings
        ClearCurrentData();
        if(data != null){
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

    [System.Serializable]
    public class GlobalSaveData {
        public string version;
        public int minRate = 0;
        public int maxRate = 0;
        public HeartrateInputModule.InputType activeInput = HeartrateInputModule.InputType.SLIDER;
        public List<HeartrateInputModule.SaveData> inputs = new List<HeartrateInputModule.SaveData>(); 
        public Localization.SupportedLanguage language;

        public int apiServerPort;

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
        public string profileName = "DEFAULT";
        public string profileID;

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
