using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using VTS.Core;

public class SaveDataManager : Singleton<SaveDataManager>, IEventPublisher<SaveDataManager.SaveDataEventType>
{

	private string GLOBAL_SAVE_DIRECTORY = "";
	private string GLOBAL_SAVE_FILE_PATH = "";
	private string MODEL_SAVE_DIRECTORY = "";
	private string PLUGINS_SAVE_DIRECTORY = "";
	private string PLUGINS_SAVE_FILE_PATH = "";
	public string SaveDirectory { get { return this.GLOBAL_SAVE_DIRECTORY; } }

	private Dictionary<EventCallbackRegistration, Action> _onFileRead = new Dictionary<EventCallbackRegistration, Action>();
	private Dictionary<EventCallbackRegistration, Action> _onFileWrite = new Dictionary<EventCallbackRegistration, Action>();

	private IJsonUtility jsonUtility = new NewtonsoftJsonUtilityImpl();

	public override void Initialize()
	{
		this.GLOBAL_SAVE_DIRECTORY = Application.persistentDataPath;
		this.GLOBAL_SAVE_FILE_PATH = Path.Combine(this.GLOBAL_SAVE_DIRECTORY, "save.json");
		this.MODEL_SAVE_DIRECTORY = Path.Combine(this.GLOBAL_SAVE_DIRECTORY, "models");
		this.PLUGINS_SAVE_DIRECTORY = Path.Combine(this.GLOBAL_SAVE_DIRECTORY, "plugins");
		this.PLUGINS_SAVE_FILE_PATH = Path.Combine(this.PLUGINS_SAVE_DIRECTORY, "plugins.json");
		CreateDirectoryIfNotFound(this.GLOBAL_SAVE_DIRECTORY);
		CreateDirectoryIfNotFound(this.MODEL_SAVE_DIRECTORY);
		CreateDirectoryIfNotFound(this.PLUGINS_SAVE_DIRECTORY);
		Debug.Log(string.Format("Version: {0}", Application.version));
	}

	private void CreateDirectoryIfNotFound(string path)
	{
		if (!Directory.Exists(path))
		{
			Directory.CreateDirectory(path);
		}
	}

	#region Global Settings

	public HeartratePlugin.GlobalSaveData ReadGlobalSaveData()
	{
		HeartratePlugin.GlobalSaveData data = new HeartratePlugin.GlobalSaveData();
		if (File.Exists(this.GLOBAL_SAVE_FILE_PATH))
		{
			Debug.Log(string.Format("Reading from path: {0}", this.GLOBAL_SAVE_FILE_PATH));
			string content = File.ReadAllText(this.GLOBAL_SAVE_FILE_PATH);
			data = this.jsonUtility.FromJson<HeartratePlugin.GlobalSaveData>(content);
			data = ModernizeLegacyGlobalSaveData(data, content);
		}
		return data;
	}

	public void WriteGlobalSaveData(HeartratePlugin.GlobalSaveData data)
	{
		data.version = Application.version;
		Debug.Log(string.Format("Writing to path: {0}", this.GLOBAL_SAVE_FILE_PATH));
		File.WriteAllText(this.GLOBAL_SAVE_FILE_PATH, data.ToString());
	}

	private HeartratePlugin.GlobalSaveData ModernizeLegacyGlobalSaveData(HeartratePlugin.GlobalSaveData data, string content)
	{
		string version = data.version;
		if (VersionUtils.IsOlderThan(version, "1.0.0"))
		{
			LegacyGlobalSaveData_v0_1_0 legacyData = this.jsonUtility.FromJson<LegacyGlobalSaveData_v0_1_0>(content);
			return Modernize_v1_0_0_to_v1_1_0(Modernize_v0_1_0_to_v1_0_0(legacyData));
		}
		else if (VersionUtils.IsOlderThan(version, "1.1.0"))
		{
			return Modernize_v1_0_0_to_v1_1_0(data);
		}
		else
		{
			return data;
		}
	}

	private HeartratePlugin.GlobalSaveData Modernize_v0_1_0_to_v1_0_0(LegacyGlobalSaveData_v0_1_0 legacyData)
	{
		Debug.Log("Migrating global save file data from v0.1.0 to v1.0.0...");
		if (legacyData.colors != null && legacyData.colors.Count > 0)
		{
			// make a new ModelSaveData, apply it to the default, no-model profile.
			// TODO: is there a way to make this apply to the first LOADED model?
			LegacyModelSaveData_v1_0_0 modelData = new LegacyModelSaveData_v1_0_0();
			modelData.colors = legacyData.colors;
			HeartratePlugin.ModelSaveData modernData = ModernizeLegacyModelSaveData(new HeartratePlugin.ModelSaveData(), modelData.ToString());
			HeartrateManager.Instance.Plugin.FromModelSaveData(modernData);
			WriteModelSaveData(HeartrateManager.Instance.Plugin.ToModelSaveData());
		}
		HeartratePlugin.GlobalSaveData data = new HeartratePlugin.GlobalSaveData();
		data.inputs = legacyData.inputs;
		data.activeInput = HeartrateInputModule.InputType.SLIDER;
		data.maxRate = legacyData.maxRate;
		data.minRate = legacyData.minRate;
		return data;
	}

	private HeartratePlugin.GlobalSaveData Modernize_v1_0_0_to_v1_1_0(HeartratePlugin.GlobalSaveData legacyData)
	{
		Debug.Log("Migrating global save file data from v1.0.0 to v1.1.0...");
		HeartratePlugin.GlobalSaveData data = new HeartratePlugin.GlobalSaveData();
		data.version = legacyData.version;
		data.inputs = legacyData.inputs;
		data.activeInput = HeartrateInputModule.InputType.SLIDER;
		data.maxRate = legacyData.maxRate;
		data.minRate = legacyData.minRate;
		return data;
	}

	#endregion

	#region Model Settings

	public HeartratePlugin.ModelSaveData ReadModelData(ProfileManager.ProfileData profile)
	{
		HeartratePlugin.ModelSaveData data = new HeartratePlugin.ModelSaveData();
		Debug.Log("Loading Model: " + profile.FileName);
		string filePath = Path.Combine(this.MODEL_SAVE_DIRECTORY, profile.FileName + ".json");
		if (File.Exists(filePath))
		{
			Debug.Log(string.Format("Reading from path: {0}", profile.FileName));
			string text = File.ReadAllText(filePath);
			data = this.jsonUtility.FromJson<HeartratePlugin.ModelSaveData>(text);
			data = ModernizeLegacyModelSaveData(data, text);
		}
		ExecuteReadCallbacks();
		return data;
	}

	public void WriteModelSaveData(HeartratePlugin.ModelSaveData data)
	{
		data.version = Application.version;
		string filePath = Path.Combine(this.MODEL_SAVE_DIRECTORY, ProfileManager.Instance.CurrentProfile.FileName + ".json");
		Debug.Log(string.Format("Writing to path: {0}", ProfileManager.Instance.CurrentProfile.FileName));
		File.WriteAllText(filePath, data.ToString());
		ExecuteWriteCallbacks();
	}

	public void DeleteProfileData(ProfileManager.ProfileData data)
	{
		string filePath = Path.Combine(this.MODEL_SAVE_DIRECTORY, data.FileName + ".json");
		if (File.Exists(filePath))
		{
			Debug.Log(string.Format("Deleting from path: {0}", data.FileName));
			File.Delete(filePath);
			ExecuteWriteCallbacks();
		}
	}

	private HeartratePlugin.ModelSaveData ModernizeLegacyModelSaveData(HeartratePlugin.ModelSaveData data, string content)
	{
		string version = data.version;
		if (VersionUtils.IsOlderThan(version, "1.1.0"))
		{
			LegacyModelSaveData_v1_0_0 legacyData = this.jsonUtility.FromJson<LegacyModelSaveData_v1_0_0>(content);
			return Modernize_v1_1_0_to_v_1_2_0(Modernize_v1_0_0_to_v_1_1_0(legacyData));
		}
		else if (VersionUtils.IsOlderThan(version, "1.2.0"))
		{
			LegacyModelSaveData_v1_1_0 legacyData_v1_1_0 = this.jsonUtility.FromJson<LegacyModelSaveData_v1_1_0>(content);
			return Modernize_v1_1_0_to_v_1_2_0(legacyData_v1_1_0);
		}
		else
		{
			return data;
		}
	}

	private LegacyModelSaveData_v1_1_0 Modernize_v1_0_0_to_v_1_1_0(LegacyModelSaveData_v1_0_0 legacyData)
	{
		Debug.Log("Migrating model save file data from v1.0.0 to v 1.1.0...");
		LegacyModelSaveData_v1_1_0 data = new LegacyModelSaveData_v1_1_0();
		data.colors = legacyData.colors;
		data.hotkeys = new List<HotkeyModule.SaveData>();
		data.modelID = legacyData.modelID;
		data.modelName = legacyData.modelName;
		data.version = legacyData.version;
		data.expressions = new List<ExpressionModule.SaveData>();

		foreach (LegacyExpressionSaveData_v1_0_0 legacyExpression in legacyData.expressions)
		{
			ExpressionModule.SaveData expressionData = new ExpressionModule.SaveData();
			expressionData.threshold = legacyExpression.threshold;
			expressionData.expressionFile = legacyExpression.expressionFile;
			expressionData.behavior = legacyExpression.shouldActivate ?
				ExpressionModule.TriggerBehavior.ACTIVATE_ABOVE_DEACTIVATE_BELOW :
				ExpressionModule.TriggerBehavior.DEACTIVATE_ABOVE_ACTIVATE_BELOW;
			data.expressions.Add(expressionData);
		}
		return data;
	}

	private HeartratePlugin.ModelSaveData Modernize_v1_1_0_to_v_1_2_0(LegacyModelSaveData_v1_1_0 legacyData)
	{
		Debug.Log("Migrating model save file data from v1.1.0 to v 1.2.0...");
		HeartratePlugin.ModelSaveData data = new HeartratePlugin.ModelSaveData();
		data.colors = legacyData.colors;
		data.hotkeys = legacyData.hotkeys;
		data.modelID = legacyData.modelID;
		data.modelName = legacyData.modelName;
		data.version = legacyData.version;
		data.expressions = legacyData.expressions;
		data.profileName = ProfileManager.ProfileData.PROFILE_DEFAULT;
		data.profileID = legacyData.modelID;
		return data;
	}

	#endregion

	#region Profile Settings 

	public List<ProfileManager.ProfileData> GetProfileList()
	{
		List<ProfileManager.ProfileData> list = new List<ProfileManager.ProfileData>();
		foreach (string s in Directory.GetFiles(this.MODEL_SAVE_DIRECTORY))
		{
			string text = File.ReadAllText(s);
			HeartratePlugin.ModelSaveData data = this.jsonUtility.FromJson<HeartratePlugin.ModelSaveData>(text);
			data = ModernizeLegacyModelSaveData(data, text);
			ProfileManager.ProfileData info = new ProfileManager.ProfileData(data.modelName, data.modelID, data.profileName, data.profileID);
			// string key = string.Format("{0}<size=0>{1}</size> ({2})", info.modelName, info.profileID, info.profileName);
			list.Add(info);
		}
		return list;
	}

	#endregion

	#region Plugin Tokens

	public APIManager.TokenSaveData ReadTokenSaveData()
	{
		APIManager.TokenSaveData data = new APIManager.TokenSaveData();
		if (File.Exists(this.PLUGINS_SAVE_FILE_PATH))
		{
			string text = File.ReadAllText(this.PLUGINS_SAVE_FILE_PATH);
			data = this.jsonUtility.FromJson<APIManager.TokenSaveData>(text);
		}
		return data;
	}

	public void WriteTokenSaveData(APIManager.TokenSaveData data)
	{
		File.WriteAllText(this.PLUGINS_SAVE_FILE_PATH, data.ToString());
	}

	#endregion

	#region Events

	public EventCallbackRegistration RegisterEventCallback(SaveDataEventType eventType, Action callback)
	{
		EventCallbackRegistration registration = new EventCallbackRegistration(System.Guid.NewGuid().ToString());
		if (eventType == SaveDataEventType.FILE_READ)
		{
			this._onFileRead.Add(registration, callback);
		}
		else if (eventType == SaveDataEventType.FILE_WRITE)
		{
			this._onFileWrite.Add(registration, callback);
		}
		return registration;
	}

	public void UnregisterEventCallback(EventCallbackRegistration registration)
	{
		if (this._onFileRead.ContainsKey(registration))
		{
			this._onFileRead.Remove(registration);
		}
		else if (this._onFileWrite.ContainsKey(registration))
		{
			this._onFileWrite.Remove(registration);
		}
	}

	private void ExecuteReadCallbacks()
	{
		foreach (Action callback in this._onFileRead.Values)
		{
			callback();
		}
	}

	private void ExecuteWriteCallbacks()
	{
		foreach (Action callback in this._onFileWrite.Values)
		{
			callback();
		}
	}

	public enum SaveDataEventType : int
	{
		FILE_READ,
		FILE_WRITE,
	}

	#endregion
}
