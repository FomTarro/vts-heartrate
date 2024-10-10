using System;
using System.Collections.Generic;
using UnityEngine;
using VTS.Core;
using VTS.Unity;

public class HeartratePlugin : UnityVTSPlugin
{

	#region Member Variables

	private int _heartRate = 70;
	public int HeartRate { get { return Math.Max(0, this._heartRate); } }
	private int _maxRate = 100;
	private int _minRate = 70;
	private int _offset = 0;
	private ShiftingAverage _average = new ShiftingAverage(30);

	private PostProcessingEffect[] _vfxList = new PostProcessingEffect[0];
	public List<PostProcessingEffect> VFXList { get { return new List<PostProcessingEffect>(_vfxList); } }

	private const string PARAMETER_LINEAR = "VTS_Heartrate_Linear";
	private const string PARAMETER_SINE_PULSE = "VTS_Heartrate_Pulse";
	private const string PARAMETER_SINE_BREATH = "VTS_Heartrate_Breath";

	private const string PARAMETER_BPM = "VTS_Heartrate_BPM";
	private const string PARAMETER_BPM_ONES = "VTS_Heartrate_BPM_Ones";
	private const string PARAMETER_BPM_TENS = "VTS_Heartrate_BPM_Tens";
	private const string PARAMETER_BPM_HUNDREDS = "VTS_Heartrate_BPM_Hundreds";

	private const string PARAMETER_SAW_REPEAT_1 = "VTS_Heartrate_Repeat_1";
	private const string PARAMETER_SAW_REPEAT_5 = "VTS_Heartrate_Repeat_5";
	private const string PARAMETER_SAW_REPEAT_10 = "VTS_Heartrate_Repeat_10";
	private const string PARAMETER_SAW_REPEAT_20 = "VTS_Heartrate_Repeat_20";
	private const string PARAMETER_SAW_REPEAT_30 = "VTS_Heartrate_Repeat_30";
	private const string PARAMETER_SAW_REPEAT_60 = "VTS_Heartrate_Repeat_60";
	private const string PARAMETER_SAW_REPEAT_120 = "VTS_Heartrate_Repeat_120";
	private const string PARAMETER_SAW_REPEAT_BREATH = "VTS_Heartrate_Repeat_Breath";

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

	private VTSParameterInjectionValue _repeat1 = new VTSParameterInjectionValue();
	private VTSParameterInjectionValue _repeat5 = new VTSParameterInjectionValue();
	private VTSParameterInjectionValue _repeat10 = new VTSParameterInjectionValue();
	private VTSParameterInjectionValue _repeat20 = new VTSParameterInjectionValue();
	private VTSParameterInjectionValue _repeat30 = new VTSParameterInjectionValue();
	private VTSParameterInjectionValue _repeat60 = new VTSParameterInjectionValue();
	private VTSParameterInjectionValue _repeat120 = new VTSParameterInjectionValue();
	private VTSParameterInjectionValue _repeatBreath = new VTSParameterInjectionValue();

	private OscillatingValue _oscillatingPulse = new OscillatingValue();
	private OscillatingValue _oscillatingBreath = new OscillatingValue();

	private SawValue _saw1 = new SawValue(1);
	private SawValue _saw5 = new SawValue(5);
	private SawValue _saw10 = new SawValue(10);
	private SawValue _saw20 = new SawValue(20);
	private SawValue _saw30 = new SawValue(30);
	private SawValue _saw60 = new SawValue(60);
	private SawValue _saw120 = new SawValue(120);
	private SawValue _sawBreath = new SawValue(1);

	private const int PARAMETER_MAX_VALUE = 1000000;

	[Header("Outputs")]
	[SerializeField]
	private RectTransform _outputModulesParent = null;

	[Header("Colors")]
	[SerializeField]
	private ColorInputModule _colorPrefab = null;
	[SerializeField]
	private List<ColorInputModule> _colors = new List<ColorInputModule>();
	public List<ColorInputModule> ColorModules { get { return new List<ColorInputModule>(this._colors); } }

	[Header("Expressions")]
	[SerializeField]
	private ExpressionModule _expressionPrefab = null;

	[SerializeField]
	private List<ExpressionModule> _expressionModules = new List<ExpressionModule>();
	public List<ExpressionModule> ExpressionModules { get { return new List<ExpressionModule>(this._expressionModules); } }
	private Dictionary<string, List<ExpressionData>> _expressionsByModelID = new Dictionary<string, List<ExpressionData>>();

	[Header("Hotkeys")]
	[SerializeField]
	private HotkeyModule _hotkeyPrefab = null;
	// public List<HotkeyData> Hotkeys { get { return this._hotkeysByModelID; } }

	[SerializeField]
	private List<HotkeyModule> _hotkeyModules = new List<HotkeyModule>();
	public List<HotkeyModule> HotkeyModules { get { return new List<HotkeyModule>(this._hotkeyModules); } }
	private Dictionary<string, List<HotkeyData>> _hotkeysByModelID = new Dictionary<string, List<HotkeyData>>();

	[Header("VFX")]
	[SerializeField]
	private VFXModule _vfxPrefab = null;

	private List<VFXModule> _vfxModules = new List<VFXModule>();
	public List<VFXModule> VFXModules { get { return new List<VFXModule>(this._vfxModules); } }

	[Header("Input Modules")]
	[SerializeField]
	private List<HeartrateInputModule> _heartrateInputs = new List<HeartrateInputModule>();
	public List<HeartrateInputModule> HeartrateInputs { get { return new List<HeartrateInputModule>(this._heartrateInputs); } }
	private HeartrateInputModule _activeModule = null;
	public String ActiveInputModule { get { return this._activeModule != null ? this._activeModule.ToString() : null; } }
	[SerializeField]
	private HeartrateRangesInputModule _heartrateRanges = null;

	[Header("Misc.")]
	[SerializeField]
	private StatusIndicator _connectionStatus = null;
	#endregion

	#region Lifecycle

	private void OnValidate()
	{

	}

	public void OnLaunch()
	{
		this._heartrateInputs = new List<HeartrateInputModule>(FindObjectsOfType<HeartrateInputModule>());
		this._heartrateInputs.Sort((a, b) => { return a.Type - b.Type; });
		CreateAllParameters();
		FromGlobalSaveData(SaveDataManager.Instance.ReadGlobalSaveData());
		FromModelSaveData(SaveDataManager.Instance.ReadModelData(ProfileManager.Instance.CurrentProfile));
		Connect();
	}

	private void OnApplicationQuit()
	{
		Debug.Log("Quitting!");
		SaveDataManager.Instance.WriteGlobalSaveData(ToGlobalSaveData());
		SaveDataManager.Instance.WriteModelSaveData(ToModelSaveData());
		// we do this AFTER saving
		foreach (VFXModule vfx in this._vfxModules)
		{
			vfx.Reset(false);
		}
	}

	public void Connect()
	{
		Initialize(
			new WebSocketSharpImpl(this.Logger),
			new NewtonsoftJsonUtilityImpl(),
			new TokenStorageImpl(Application.persistentDataPath),
			() =>
			{
				HttpUtils.ConnectionStatus status = new HttpUtils.ConnectionStatus();
				status.status = HttpUtils.ConnectionStatus.Status.CONNECTED;
				this._connectionStatus.SetStatus(status);
				this.Logger.Log("Connected to VTube Studio!");
				CreateAllParameters();
				SubscribeToEvents();
				GetVFXData();
			},
			() =>
			{
				HttpUtils.ConnectionStatus status = new HttpUtils.ConnectionStatus();
				status.status = HttpUtils.ConnectionStatus.Status.DISCONNECTED;
				this._connectionStatus.SetStatus(status);
				this.Logger.Log("Disconnected from VTube Studio!");
			},
			(ex) =>
			{
				HttpUtils.ConnectionStatus status = new HttpUtils.ConnectionStatus();
				status.status = HttpUtils.ConnectionStatus.Status.ERROR;
				this._connectionStatus.SetStatus(status);
				this.Logger.LogError("Error connecting to VTube Studio!");
			});
		HttpUtils.ConnectionStatus connect = new HttpUtils.ConnectionStatus();
		connect.status = HttpUtils.ConnectionStatus.Status.CONNECTING;
		this._connectionStatus.SetStatus(connect);
	}

	private void SubscribeToEvents()
	{
		this.Logger.Log("Subscribing to VTube Studio Events...");
		SubscribeToModelLoadedEvent(new VTSModelLoadedEventConfigOptions(),
			(loadData) =>
			{
				GetModelData();
			},
			(loadSuccess) =>
			{
				GetModelData();
				this.Logger.Log("Subscribed to model load event.");
				SubscribeToModelConfigChangedEvent(
					(configData) =>
					{
						if (configData.data.hotkeyConfigChanged)
						{
							GetHotkeyData();
						}
					},
					(configSuccess) =>
					{
						this.Logger.Log("Subscribed to config change event.");
					},
					(configError) =>
					{
						OnSubscriptionError(configError);
					}
				);
			},
			(loadError) =>
			{
				OnSubscriptionError(loadError);
			}
		);
	}

	private void OnSubscriptionError(VTSErrorData data)
	{
		this.Logger.LogError(string.Format("Error subscribing to VTUbe Studio Events: {0}.", data.data.message));
		Dictionary<string, string> strings = new Dictionary<string, string>();
		strings.Add("error_cannot_subscribe_tooltip_populated",
			string.Format(Localization.LocalizationManager.Instance.GetString("error_cannot_subscribe_tooltip"),
			data.data.errorID,
			data.data.message));
		Localization.LocalizationManager.Instance.AddStrings(strings, Localization.LocalizationManager.Instance.CurrentLanguage);
		UIManager.Instance.ShowPopUp(
			"error_generic_title",
			"error_cannot_subscribe_tooltip_populated",
			new PopUp.PopUpOption(
				"settings_feedback_button_tweet",
				ColorUtils.ColorPreset.GREEN,
				() => { Application.OpenURL("https://twitter.com/intent/tweet?text=@FomTarro"); }),
			new PopUp.PopUpOption(
				"settings_feedback_button_email",
				ColorUtils.ColorPreset.GREEN,
				() => { Application.OpenURL("mailto:tom@skeletom.net"); })
		);
	}

	public void SetMinRate(int rate)
	{
		this._minRate = Mathf.Clamp(0, rate, 255);
	}

	public void SetMaxRate(int rate)
	{
		this._maxRate = Mathf.Clamp(0, rate, 255);
	}

	public void SetOffset(int rate)
	{
		this._offset = Mathf.Clamp(-255, rate, 255);
	}

	public void SetActiveHeartrateInput(HeartrateInputModule module)
	{
		this.Logger.Log("Activating Input module: " + module);
		foreach (HeartrateInputModule m in this._heartrateInputs)
		{
			if (m.Type != module.Type)
			{
				m.gameObject.SetActive(false);
				m.Deactivate();
			}
		}
		module.gameObject.SetActive(true);
		this._activeModule = module;
	}

	private void Update()
	{
		int priorHeartrate = this._heartRate;
		this._average.AddValue(this._activeModule != null ? this._activeModule.GetHeartrate() : 0);
		this._heartRate = Mathf.Max(0, Mathf.RoundToInt(this._average.Average) + this._offset);
		float numerator = Math.Max(0, (float)(this._heartRate - this._minRate));
		float denominator = Math.Max(1, (float)(this._maxRate - this._minRate));
		float interpolation = Mathf.Clamp01(numerator / denominator);
		// Data API message
		DataMessage dataMessage = new DataMessage(this.HeartRate);

		// apply art mesh tints
		foreach (ColorInputModule module in this._colors)
		{
			module.ApplyColor(interpolation);
			DataMessage.Tint colorModule = new DataMessage.Tint(module.ModuleColor, module.ModuleInterpolatedColor, module.ModuleMatchers);
			dataMessage.data.tints.Add(colorModule);
		}

		SortExpressionModules();
		// apply expressions
		foreach (ExpressionModule module in this._expressionModules)
		{
			module.CheckModuleCondition(priorHeartrate, this._heartRate);
		}

		SortHotkeyModules();
		// apply hotkeys
		foreach (HotkeyModule module in this._hotkeyModules)
		{
			module.CheckModuleCondition(priorHeartrate, this._heartRate);
		}

		// calculate tracking parameters
		float beatsPerSecond = ((float)this._heartRate) / 60f;
		float normalizedBeatsPerSecond = MathUtils.Normalize((float)this._heartRate, this._minRate, this._maxRate, 0, 1);
		this._linear.value = interpolation;
		this._bpm.value = this._heartRate;

		this._bpm_ones.value = this._heartRate < 1 ? -1 : this._heartRate % 10;
		this._bpm_tens.value = this._heartRate < 10 ? -1 : (this._heartRate % 100) / 10;
		this._bpm_hundreds.value = this._heartRate < 100 ? -1 : this._heartRate / 100;

		float breathingFrequency = Mathf.Clamp(normalizedBeatsPerSecond, 0.35f, PARAMETER_MAX_VALUE);
		this._breath.value = _oscillatingBreath.GetValue(breathingFrequency);
		float pulseFrequency = Mathf.Clamp(beatsPerSecond, 0f, PARAMETER_MAX_VALUE);
		this._pulse.value = _oscillatingPulse.GetValue(pulseFrequency);

		this._repeat1.value = this._saw1.GetValue(beatsPerSecond);
		this._repeat5.value = this._saw5.GetValue(beatsPerSecond);
		this._repeat10.value = this._saw10.GetValue(beatsPerSecond);
		this._repeat10.value = this._saw10.GetValue(beatsPerSecond);
		this._repeat20.value = this._saw20.GetValue(beatsPerSecond);
		this._repeat30.value = this._saw30.GetValue(beatsPerSecond);
		this._repeat60.value = this._saw60.GetValue(beatsPerSecond);
		this._repeat120.value = this._saw120.GetValue(beatsPerSecond);
		this._repeatBreath.value = this._sawBreath.GetValue(breathingFrequency);

		if (this._paramValues.Count > 0 && this.IsAuthenticated)
		{
			InjectParameterValues(this._paramValues.ToArray(),
			(s) =>
			{
				InjectedParamValuesToDictionary(this._paramValues.ToArray());
			},
			(e) =>
			{
				this.Logger.LogError(e.data.message);
			});
		}
		else if (!this.IsAuthenticated)
		{
			InjectedParamValuesToDictionary(this._paramValues.ToArray());
		}

		// apply VFX Configs (after we calculate tracking params)
		if (IsAuthenticated)
		{
			BatchVFXConfigs();
		}

		// set API data values
		dataMessage.data.parameters.vts_heartrate_linear = this._linear.value;
		dataMessage.data.parameters.vts_heartrate_pulse = this._pulse.value;
		dataMessage.data.parameters.vts_heartrate_breath = this._breath.value;

		dataMessage.data.parameters.vts_heartrate_bpm = this._bpm.value;
		dataMessage.data.parameters.vts_heartrate_bpm_ones = this._bpm_ones.value;
		dataMessage.data.parameters.vts_heartrate_bpm_tens = this._bpm_tens.value;
		dataMessage.data.parameters.vts_heartrate_bpm_hundreds = this._bpm_hundreds.value;

		dataMessage.data.parameters.vts_heartrate_repeat_1 = this._repeat1.value;
		dataMessage.data.parameters.vts_heartrate_repeat_5 = this._repeat5.value;
		dataMessage.data.parameters.vts_heartrate_repeat_10 = this._repeat10.value;
		dataMessage.data.parameters.vts_heartrate_repeat_20 = this._repeat20.value;
		dataMessage.data.parameters.vts_heartrate_repeat_30 = this._repeat30.value;
		dataMessage.data.parameters.vts_heartrate_repeat_60 = this._repeat60.value;
		dataMessage.data.parameters.vts_heartrate_repeat_120 = this._repeat120.value;
		dataMessage.data.parameters.vts_heartrate_repeat_breath = this._repeatBreath.value;

		APIManager.Instance.SendData(dataMessage);
	}

	#endregion

	#region Event Callbacks

	public void GetModelData()
	{
		this.Logger.Log("Querying for model data...");
		GetCurrentModel(
			(modelData) =>
			{
				// Model is loaded in VTS and it's not the model we have a loaded profile for
				if (modelData.data.modelLoaded && !modelData.data.modelID.Equals(ProfileManager.Instance.CurrentProfile.modelID))
				{
					// load Default profile for the new model
					ProfileManager.Instance.SetCurrentProfileInfo(
						modelData.data.modelName,
						modelData.data.modelID);
					FromModelSaveData(SaveDataManager.Instance.ReadModelData(ProfileManager.Instance.CurrentProfile));
					SaveDataManager.Instance.WriteModelSaveData(ToModelSaveData());
					// If no model is loaded in VTS but we do have a model profile loaded here, revert to the NO_MODEL default profile
				}
				else if (!modelData.data.modelLoaded && ProfileManager.Instance.IsModelLoaded())
				{
					ProfileManager.Instance.CreateDefaultNoModelProfile();
					FromModelSaveData(SaveDataManager.Instance.ReadModelData(ProfileManager.Instance.CurrentProfile));
					SaveDataManager.Instance.WriteModelSaveData(ToModelSaveData());
				}
				if (ProfileManager.Instance.IsModelLoaded())
				{
					GetHotkeyData();
					GetExpressionData();
				}
			},
			(modelError) =>
			{
				ProfileManager.Instance.CreateDefaultNoModelProfile();
				this.Logger.LogError(string.Format("Error while querying Model Data from VTube Studio: {0} - {1}",
					modelError.data.errorID, modelError.data.message));
			}
		);
	}

	private void GetHotkeyData()
	{
		this.Logger.Log("Querying for Hotkey Data...");
		if (ProfileManager.Instance.IsModelLoaded())
		{
			GetHotkeysInCurrentModel(
				ProfileManager.Instance.CurrentProfile.modelID,
				(s) =>
				{
					try
					{
						if (this._hotkeysByModelID.ContainsKey(s.data.modelID))
						{
							this._hotkeysByModelID[s.data.modelID] = new List<HotkeyData>(s.data.availableHotkeys);
						}
						else
						{
							this._hotkeysByModelID.Add(s.data.modelID, new List<HotkeyData>(s.data.availableHotkeys));
						}
						// repaint hotkey modules
						foreach (HotkeyModule module in this._hotkeyModules)
						{
							module.RefreshHotkeyList();
						}
					}
					catch (Exception e)
					{
						this.Logger.LogError(string.Format("Error updating hotkeys: {0}", e.StackTrace));
					}
				},
				(e) =>
				{
					this.Logger.LogError(string.Format("Error while querying Hotkey Data from VTube Studio: {0} - {1}",
						e.data.errorID, e.data.message));
				}
			);
		}
		else
		{
			this.Logger.Log("No model loaded for hotkeys...");
		}
	}

	private void GetExpressionData()
	{
		this.Logger.Log("Querying for expression data...");
		if (ProfileManager.Instance.IsModelLoaded())
		{
			GetExpressionStateList(
				(expressionData) =>
				{
					try
					{
						if (this._expressionsByModelID.ContainsKey(expressionData.data.modelID))
						{
							this._expressionsByModelID[expressionData.data.modelID] = new List<ExpressionData>(expressionData.data.expressions);
						}
						else
						{
							this._expressionsByModelID.Add(expressionData.data.modelID, new List<ExpressionData>(expressionData.data.expressions));
						}
						foreach (ExpressionModule module in this._expressionModules)
						{
							// TODO: can we pass the expression dictionary in instead of making it accessible publicly?
							module.RefreshExpressionList();
						}
					}
					catch (Exception e)
					{
						this.Logger.LogError(string.Format("Error updating expressions: {0}", e.StackTrace));
					}
				},
				(expressionError) =>
				{
					this.Logger.LogError(string.Format("Error while querying Expression Data from VTube Studio: {0} - {1}",
						expressionError.data.errorID, expressionError.data.message));
				}
			);
		}
		else
		{
			this.Logger.Log("No model loaded for expressions...");
		}
	}

	private void GetVFXData()
	{
		Effects[] effectFilter = new Effects[0];
		GetPostProcessingEffectStateList(false, true, effectFilter,
		(success) =>
		{
			this._vfxList = success.data.postProcessingEffects;
			foreach (VFXModule module in this._vfxModules)
			{
				module.RefreshEffectList();
			}
		},
		(error) =>
		{
			this.Logger.LogError(string.Format("Error while querying Expression Data from VTube Studio: {0} - {1}",
			error.data.errorID, error.data.message));
		});
	}

	private void BatchVFXConfigs()
	{
		// This is executed here instead of inside each module so that we can batch our request 
		// and set every config parameter at once
		VTSPostProcessingUpdateOptions options = new VTSPostProcessingUpdateOptions();
		options.setPostProcessingValues = true;
		options.postProcessingOn = true;
		Dictionary<EffectConfigs, PostProcessingValue> values = new Dictionary<EffectConfigs, PostProcessingValue>();
		foreach (VFXModule module in this._vfxModules)
		{
			foreach (PostProcessingValue value in module.EffectParameters)
			{
				if (!values.ContainsKey(value.configID))
				{
					values.Add(value.configID, value);
				}
				else if (float.Parse(values[value.configID].configValue) < float.Parse(value.configValue))
				{
					values[value.configID] = value;
				}
			}
		}
		PostProcessingValue[] array = new PostProcessingValue[values.Count];
		values.Values.CopyTo(array, 0);
		SetPostProcessingEffectValues(options, array,
		(s) => { },
		(e) =>
		{
			Debug.LogError(string.Format("Error while setting VFX Config in VTube Studio: {0} - {1}",
				e.data.errorID, e.data.message));
		});
	}

	#endregion

	#region Parameters

	private void CreateAllParameters()
	{
		this._paramValues = new List<VTSParameterInjectionValue>();
		CreateNewParameter(PARAMETER_LINEAR, "param_vts_heartrate_linear", 1, this._linear);
		CreateNewParameter(PARAMETER_SINE_PULSE, "param_vts_heartrate_pulse", 1, this._pulse);
		CreateNewParameter(PARAMETER_SINE_BREATH, "param_vts_heartrate_breath", 1, this._breath);
		CreateNewParameter(PARAMETER_BPM, "param_vts_heartrate_bpm_all", 255, this._bpm);
		CreateNewParameter(PARAMETER_BPM_ONES, "param_vts_heartrate_bpm_ones", 9, this._bpm_ones);
		CreateNewParameter(PARAMETER_BPM_TENS, "param_vts_heartrate_bpm_tens", 9, this._bpm_tens);
		CreateNewParameter(PARAMETER_BPM_HUNDREDS, "param_vts_heartrate_bpm_hundreds", 9, this._bpm_hundreds);
		CreateNewParameter(PARAMETER_SAW_REPEAT_1, "param_vts_heartrate_repeat_1", 1, this._repeat1);
		CreateNewParameter(PARAMETER_SAW_REPEAT_5, "param_vts_heartrate_repeat_5", 1, this._repeat5);
		CreateNewParameter(PARAMETER_SAW_REPEAT_10, "param_vts_heartrate_repeat_10", 1, this._repeat10);
		CreateNewParameter(PARAMETER_SAW_REPEAT_20, "param_vts_heartrate_repeat_20", 1, this._repeat20);
		CreateNewParameter(PARAMETER_SAW_REPEAT_30, "param_vts_heartrate_repeat_30", 1, this._repeat30);
		CreateNewParameter(PARAMETER_SAW_REPEAT_60, "param_vts_heartrate_repeat_60", 1, this._repeat60);
		CreateNewParameter(PARAMETER_SAW_REPEAT_120, "param_vts_heartrate_repeat_120", 1, this._repeat120);
		CreateNewParameter(PARAMETER_SAW_REPEAT_BREATH, "param_vts_heartrate_repeat_breath", 1, this._repeatBreath);
	}

	private void CreateNewParameter(string paramName, string paramDescriptionKey, int paramMax, VTSParameterInjectionValue value)
	{
		value.id = paramName;
		value.value = 0;
		value.weight = 1;
		this._paramValues.Add(value);
		if (this.IsAuthenticated)
		{
			VTSCustomParameter newParam = new VTSCustomParameter();
			newParam.defaultValue = 0;
			newParam.min = 0;
			newParam.max = paramMax;
			newParam.parameterName = paramName;
			newParam.explanation = Localization.LocalizationManager.Instance.GetString(paramDescriptionKey);
			this.Logger.Log(string.Format("Creating tracking parameter: {0}", paramName));
			this.AddCustomParameter(
				newParam,
				(s) =>
				{
					this.Logger.Log(string.Format("Successfully created parameter in VTube Studio: {0}", paramName));
				},
				(e) =>
				{
					this.Logger.LogError(string.Format("Error while injecting Parameter Data {0} into VTube Studio: {1} - {2}",
						paramName, e.data.errorID, e.data.message));
				});
		}
	}

	private void InjectedParamValuesToDictionary(VTSParameterInjectionValue[] values)
	{
		this._parameterMap = new Dictionary<string, float>();
		foreach (VTSParameterInjectionValue parameter in values)
		{
			this._parameterMap.Add(parameter.id, parameter.value);
		}
	}

	#endregion

	#region Module Creation

	public void CreateColorTintModule(ColorInputModule.SaveData module)
	{
		ColorInputModule instance = Instantiate<ColorInputModule>(this._colorPrefab, Vector3.zero, Quaternion.identity, this._outputModulesParent);
		int index = GetModuleNewChildIndex();
		instance.transform.SetSiblingIndex(index);
		this._colors.Add(instance);
		if (module != null)
		{
			instance.FromSaveData(module);
		}
	}

	public void DestroyColorTintModule(ColorInputModule module)
	{
		if (this._colors.Contains(module))
		{
			this._colors.Remove(module);
			module.ApplyColor(0);
			Destroy(module.gameObject);
		}
	}

	public void CreateExpressionModule(ExpressionModule.SaveData module)
	{
		ExpressionModule instance = Instantiate<ExpressionModule>(this._expressionPrefab, Vector3.zero, Quaternion.identity, this._outputModulesParent);
		int index = GetModuleNewChildIndex();
		instance.transform.SetSiblingIndex(index);
		this._expressionModules.Add(instance);
		SortExpressionModules();
		if (module != null)
		{
			instance.FromSaveData(module);
		}
	}

	public void DestroyExpressionModule(ExpressionModule module)
	{
		if (this._expressionModules.Contains(module))
		{
			this._expressionModules.Remove(module);
			SortExpressionModules();
			Destroy(module.gameObject);
		}
	}

	private void SortExpressionModules()
	{
		this._expressionModules.Sort((a, b)
			=>
		{ return a.Threshold.CompareTo(b.Threshold); });
	}

	public void CreateHotkeyModule(HotkeyModule.SaveData module)
	{
		HotkeyModule instance = Instantiate<HotkeyModule>(this._hotkeyPrefab, Vector3.zero, Quaternion.identity, this._outputModulesParent);
		int index = GetModuleNewChildIndex();
		instance.transform.SetSiblingIndex(index);
		this._hotkeyModules.Add(instance);
		SortHotkeyModules();
		if (module != null)
		{
			instance.FromSaveData(module);
		}
	}

	public void DestroyHotkeyModule(HotkeyModule module)
	{
		if (this._hotkeyModules.Contains(module))
		{
			this._hotkeyModules.Remove(module);
			SortHotkeyModules();
			Destroy(module.gameObject);
		}
	}

	private void SortHotkeyModules()
	{
		this._hotkeyModules.Sort((a, b)
			=>
		{ return a.Threshold.CompareTo(b.Threshold); });
	}

	private int GetModuleNewChildIndex()
	{
		return 1; //Math.Max(1, TransformUtils.GetActiveChildCount(this._outputModulesParent) - 3);
	}

	public List<HotkeyData> GetHotkeysForModelID(string modelID)
	{
		if (this._hotkeysByModelID.ContainsKey(modelID))
		{
			return new List<HotkeyData>(this._hotkeysByModelID[modelID]);
		}
		else
		{
			return new List<HotkeyData>();
		}
	}

	public List<ExpressionData> GetExpressionsForModelID(string modelID)
	{
		if (this._expressionsByModelID.ContainsKey(modelID))
		{
			return new List<ExpressionData>(this._expressionsByModelID[modelID]);
		}
		else
		{
			return new List<ExpressionData>();
		}
	}

	public void CreateVFXModule(VFXModule.SaveData module)
	{
		VFXModule instance = Instantiate<VFXModule>(this._vfxPrefab, Vector3.zero, Quaternion.identity, this._outputModulesParent);
		int index = GetModuleNewChildIndex();
		instance.transform.SetSiblingIndex(index);
		this._vfxModules.Add(instance);
		SortVFXModules();
		if (module != null)
		{
			instance.FromSaveData(module);
		}
	}

	public void DestroyVFXModule(VFXModule module)
	{
		if (this._vfxModules.Contains(module))
		{
			this._vfxModules.Remove(module);
			SortVFXModules();
			Destroy(module.gameObject);
		}
	}

	private void SortVFXModules()
	{
		// TODO
		// this._vfxModules.Sort((a, b)
		// 	=>
		// { return a.CompareTo(b); });
	}

	#endregion

	#region Data Saving

	public GlobalSaveData ToGlobalSaveData()
	{
		GlobalSaveData data = new GlobalSaveData();
		data.version = Application.version;
		data.maxRate = this._maxRate;
		data.minRate = this._minRate;
		data.offset = this._offset;
		data.activeInput = this._activeModule.Type;
		foreach (HeartrateInputModule module in this._heartrateInputs)
		{
			data.inputs.Add(module.ToSaveData());
		}
		data.language = Localization.LocalizationManager.Instance.CurrentLanguage;
		data.apiServerPort = APIManager.Instance.Port;
		return data;
	}

	public ModelSaveData ToModelSaveData()
	{
		ModelSaveData data = new ModelSaveData();
		ProfileManager.ProfileData currentModel = ProfileManager.Instance.CurrentProfile;
		data.version = Application.version;
		data.modelName = currentModel.modelName;
		data.modelID = currentModel.modelID;
		data.profileName = currentModel.profileName;
		data.profileID = currentModel.profileID;
		foreach (ColorInputModule module in this._colors)
		{
			data.colors.Add(module.ToSaveData());
		}
		foreach (ExpressionModule module in this._expressionModules)
		{
			data.expressions.Add(module.ToSaveData());
		}
		foreach (HotkeyModule module in this._hotkeyModules)
		{
			data.hotkeys.Add(module.ToSaveData());
		}
		foreach (VFXModule module in this._vfxModules)
		{
			data.vfx.Add(module.ToSaveData());
		}
		return data;
	}

	public void FromGlobalSaveData(GlobalSaveData data)
	{
		this._maxRate = Mathf.Clamp(data.maxRate, 0, 255);
		this._heartrateRanges.SetMaxRate(this._maxRate.ToString());
		this._minRate = Mathf.Clamp(data.minRate, 0, 255);
		this._heartrateRanges.SetMinRate(this._minRate.ToString());
		this._offset = Mathf.Clamp(data.offset, -255, 255);
		this._heartrateRanges.SetOffset(this._offset.ToString());
		// Default to SLIDER if we can't find the provided input type
		HeartrateInputModule activeModule = this._heartrateInputs.Find(x => x.Type == data.activeInput);
		activeModule = activeModule != null ? activeModule : this._heartrateInputs.Find((x => x.Type == HeartrateInputModule.InputType.SLIDER));
		this.SetActiveHeartrateInput(activeModule);

		// Load settings for all input modules
		foreach (HeartrateInputModule.SaveData input in data.inputs)
		{
			HeartrateInputModule match = this._heartrateInputs.Find(m => m.Type == input.type);
			if (match != null)
			{
				match.FromSaveData(input);
			}
		}

		if (!Application.version.Equals(data.version))
		{
			this.Logger.Log("Applying system language settings on new version");
			Localization.LocalizationManager.Instance.SwitchLanguage(Application.systemLanguage);
		}
		else if (data.language != 0)
		{
			this.Logger.Log(String.Format("Setting language to {0}", data.language));
			Localization.LocalizationManager.Instance.SwitchLanguage(data.language);
		}
		else
		{
			this.Logger.Log("Defaulting language to English as no settings were found");
			Localization.LocalizationManager.Instance.SwitchLanguage(Localization.SupportedLanguage.ENGLISH);
		}

		APIManager.Instance.SetPort(data.apiServerPort);
	}

	public void FromModelSaveData(ModelSaveData data)
	{
		// wipe current settings
		DestroyCurrentModules();
		if (data != null)
		{
			foreach (ColorInputModule.SaveData module in data.colors)
			{
				CreateColorTintModule(module);
			}
			foreach (ExpressionModule.SaveData module in data.expressions)
			{
				CreateExpressionModule(module);
			}
			foreach (HotkeyModule.SaveData module in data.hotkeys)
			{
				CreateHotkeyModule(module);
			}
			foreach (VFXModule.SaveData module in data.vfx)
			{
				CreateVFXModule(module);
			}
		}
	}

	private void DestroyCurrentModules()
	{
		List<ColorInputModule> tempColor = new List<ColorInputModule>(this._colors);
		foreach (ColorInputModule c in tempColor)
		{
			DestroyColorTintModule(c);
		}
		List<ExpressionModule> tempExpression = new List<ExpressionModule>(this._expressionModules);
		foreach (ExpressionModule e in tempExpression)
		{
			DestroyExpressionModule(e);
		}
		List<HotkeyModule> tempHotkey = new List<HotkeyModule>(this._hotkeyModules);
		foreach (HotkeyModule h in tempHotkey)
		{
			DestroyHotkeyModule(h);
		}
		List<VFXModule> tempVFX = new List<VFXModule>(this._vfxModules);
		foreach (VFXModule v in tempVFX)
		{
			DestroyVFXModule(v);
		}
	}

	[Serializable]
	public class GlobalSaveData
	{
		public string version;
		public int minRate = 0;
		public int maxRate = 0;
		public int offset = 0;
		public HeartrateInputModule.InputType activeInput = HeartrateInputModule.InputType.SLIDER;
		public List<HeartrateInputModule.SaveData> inputs = new List<HeartrateInputModule.SaveData>();
		public Localization.SupportedLanguage language;
		public int apiServerPort;

		public override string ToString()
		{
			return new NewtonsoftJsonUtilityImpl().ToJson(this);
		}
	}

	[Serializable]
	public class ModelSaveData
	{
		public string version;
		public string modelID;
		public string modelName;
		public string profileName = "DEFAULT";
		public string profileID;
		public List<ColorInputModule.SaveData> colors = new List<ColorInputModule.SaveData>();
		public List<ExpressionModule.SaveData> expressions = new List<ExpressionModule.SaveData>();
		public List<HotkeyModule.SaveData> hotkeys = new List<HotkeyModule.SaveData>();
		public List<VFXModule.SaveData> vfx = new List<VFXModule.SaveData>();

		public override string ToString()
		{
			return new NewtonsoftJsonUtilityImpl().ToJson(this);
		}
	}

	#endregion
}
