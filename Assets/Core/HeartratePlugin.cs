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
    private string SAVE_PATH = "";

    [SerializeField]
    [Range(50, 120)]
    private int _heartRate = 70;
    public int HeartRate { get { return this._heartRate; } }
    private int _maxRate = 100;
    private int _minRate = 70;

    private const string PARAMETER_LINEAR = "vts_heartrate_linear";
    private const string PARAMETER_SINE = "vts_heartrate_bounce";
    private const string PARAMETER_SINE_REALTIME = "vts_heartrate_bounce_realtime";
    private List<VTSParameterInjectionValue> _paramValues = new List<VTSParameterInjectionValue>();
    private VTSParameterInjectionValue _linear = new VTSParameterInjectionValue();
    private VTSParameterInjectionValue _sine = new VTSParameterInjectionValue();
    private VTSParameterInjectionValue _realtime = new VTSParameterInjectionValue();

    [Header("Colors")]
    [SerializeField]
    private ColorInputModule _colorPrefab = null;
    [SerializeField]
    private RectTransform _colorListParent = null;
    [SerializeField]
    private List<ColorInputModule> _colors = new List<ColorInputModule>();

    [Header("Input Modules")]
    [SerializeField]
    private List<HeartrateInputModule> _heartrateInputs = new List<HeartrateInputModule>();
    [SerializeField]
    private HeartrateRangesInputModule _heartrateRanges = null;

    [Header("Misc.")]
    [SerializeField]
    private StatusIndicator _connectionStatus = null;
    
    // Start is called before the first frame update
    private void Start()
    {
        this.SAVE_PATH = Path.Combine(Application.persistentDataPath, "save.json");
        Application.OpenURL(Application.persistentDataPath);
        Load(); 
        // Everything you need to get started!
        Initialize(
            new WebSocketSharpImpl(),
            new JsonUtilityImpl(),
            new TokenStorageImpl(),
            () => {
                HttpUtils.ConnectionStatus status = new HttpUtils.ConnectionStatus();
                status.status = HttpUtils.ConnectionStatus.Status.CONNECTED;
                this._connectionStatus.SetStatus(status);
                // LoggingManager.Instance.Log("Connected to VTube Studio!");
                CreateNewParameter(PARAMETER_LINEAR, 
                (s) => {
                    // confirm param created with bool
                    _linear.id = PARAMETER_LINEAR;
                    _linear.value = 0;
                    _paramValues.Add(_linear);
                },
                (e) => {
                    Debug.LogError(e.ToString());
                });
                CreateNewParameter(PARAMETER_SINE, 
                (s) => {
                    // confirm param created with bool
                    _sine.id = PARAMETER_SINE;
                    _sine.value = 0;
                    _paramValues.Add(_sine);
                },
                (e) => {
                    Debug.LogError(e.ToString());
                });
                CreateNewParameter(PARAMETER_SINE_REALTIME, 
                (s) => {
                    // confirm param created with bool
                    _realtime.id = PARAMETER_SINE_REALTIME;
                    _realtime.value = 0;
                    _paramValues.Add(_realtime);
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
    }

    private void OnValidate(){
        this._heartrateInputs = new List<HeartrateInputModule>(FindObjectsOfType<HeartrateInputModule>());
    }

    private void OnApplicationQuit(){
        Save();
    }

    public void SetMinRate(int rate){
        this._minRate = rate;
    }

    public void SetMaxRate(int rate){
        this._maxRate = rate;
    }

    private void Update(){

        foreach(HeartrateInputModule module in this._heartrateInputs){
            if(module.IsActive){
                this._heartRate = module.GetHeartrate();
                break;
            }
        }
        float interpolation = Mathf.Clamp01((float)(this._heartRate-this._minRate)/(float)(this._maxRate - this._minRate));
        if(this.IsAuthenticated){
            foreach(ColorInputModule module in this._colors){
                ArtMeshMatcher matcher = new ArtMeshMatcher();
                matcher.tintAll = false;
                matcher.nameContains = module.ModuleMatchers;
                this.TintArtMesh(
                    Color32.Lerp(Color.white, module.ModuleColor, interpolation),  
                    0.5f, 
                    matcher,
                    (success) => {

                    },
                    (error) => {

                    });
            }

            _linear.value = interpolation;
            _sine.value = 0.5f * (1 + Mathf.Sin((1+interpolation) * Time.time));
            float freq = ((float)this.HeartRate) / 60f;
            _realtime.value = 0.5f * (1 + Mathf.Sin(2 * Mathf.PI * freq * Time.time));
            if(_paramValues.Count > 0){
                this.InjectParameterValues(_paramValues.ToArray(),
                (s) => {

                },
                (e) => {
                    Debug.Log(JsonUtility.ToJson(e));
                }
                );
            }
        }
    }

    public void CreateColorInputModule(ColorInputModule.SaveData module){
        ColorInputModule instance = Instantiate<ColorInputModule>(this._colorPrefab, Vector3.zero, Quaternion.identity, this._colorListParent);
        instance.transform.SetSiblingIndex(1);
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

    public void SetActiveHeartrateInput(HeartrateInputModule module){
        foreach(HeartrateInputModule m in this._heartrateInputs){
            if(!m.name.Equals(module.name)){
                m.SetStatus(false);
            }
        }
    }

    private void CreateNewParameter(string paramName, System.Action<VTSParameterCreationData> onSuccess, System.Action<VTSErrorData> onError){
        VTSCustomParameter newParam = new VTSCustomParameter();
        newParam.defaultValue = 0;
        newParam.min = 0;
        newParam.max = 1;
        newParam.parameterName = paramName;
        this.AddCustomParameter(
            newParam,
            onSuccess,
            onError);
    }

    private void Save(){
        SaveData data = new SaveData();
        data.maxRate = this._maxRate;
        data.minRate = this._minRate;
        foreach(ColorInputModule module in this._colors){
            data.colors.Add(module.ToSaveData());
        }
        foreach(HeartrateInputModule module in this._heartrateInputs){
            data.inputs.Add(module.ToSaveData());
        }
        File.WriteAllText(this.SAVE_PATH, data.ToString());
    }

    private void Load(){
        if(File.Exists(this.SAVE_PATH)){
            string text = File.ReadAllText(this.SAVE_PATH);
            SaveData data = JsonUtility.FromJson<SaveData>(text);

            this._maxRate = data.maxRate;
            this._heartrateRanges.SetMaxRate(this._maxRate.ToString());
            this._minRate = data.minRate;
            this._heartrateRanges.SetMinRate(this._minRate.ToString());

            foreach(ColorInputModule.SaveData module in data.colors){
                CreateColorInputModule(module);
            }

            foreach(HeartrateInputModule.SaveData module in data.inputs){
                foreach(HeartrateInputModule m in this._heartrateInputs){
                    if(m.Type == module.type){
                        m.FromSaveData(module);
                    }
                }
            }
        }
    }

    [System.Serializable]
    public class SaveData {
        public int minRate = 0;
        public int maxRate = 0;
        public List<ColorInputModule.SaveData> colors = new List<ColorInputModule.SaveData>();
        public List<HeartrateInputModule.SaveData> inputs = new List<HeartrateInputModule.SaveData>(); 

        public override string ToString()
        {
            return JsonUtility.ToJson(this);
        }
    }
}
