using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using VTS.Core;

public class HotkeyModule : MonoBehaviour
{
	[SerializeField]
	private TMP_InputField _threshold = null;
	private int _thresholdVal = 0;
	public int Threshold { get { return this._thresholdVal; } }
	private int _priorThreshold = 0;
	[SerializeField]
	private TMP_Dropdown _dropdown = null;
	// Because the dropdown is populated by an async method, 
	// we load the expression that should be selected from a profile load into this buffer
	// until the async method resolves.
	private string _waitingOnID = null;
	private string SelectedHotkey
	{
		get
		{
			return this._waitingOnID != null ?
			this._waitingOnID :
			GetDataFromDropdownSelection().hotkeyID;
		}
	}

	[SerializeField]
	private TMP_Dropdown _behavior = null;
	public TriggerBehavior Behavior { get { return (TriggerBehavior)this._behavior.value; } }
	private TriggerBehavior _priorBehavior = TriggerBehavior.ACTIVATE_ABOVE_ACTIVATE_BELOW;
	[SerializeField]
	private TMP_Text _minimizedSummary = null;

	public void Clone()
	{
		HeartrateManager.Instance.Plugin.CreateHotkeyModule(this.ToSaveData());
	}

	public void Delete()
	{
		HeartrateManager.Instance.Plugin.DestroyHotkeyModule(this);
	}

	public void CheckModuleCondition(int priorHeartrate, int currentHeartrate)
	{
		if (this._priorBehavior != this.Behavior)
		{
			// forces a re-assessment of the module status
			// TODO: if the behavior changes from ABOVE AND BELOW to just ABOVE, 
			// it will try to trigger AGAIN which just turns it off
			if (
				!(((this._priorBehavior == TriggerBehavior.ACTIVATE_ABOVE || this._priorBehavior == TriggerBehavior.ACTIVATE_ABOVE_ACTIVATE_BELOW) &&
				(this.Behavior == TriggerBehavior.ACTIVATE_ABOVE || this.Behavior == TriggerBehavior.ACTIVATE_ABOVE_ACTIVATE_BELOW)) ||
				((this._priorBehavior == TriggerBehavior.ACTIVATE_BELOW || this._priorBehavior == TriggerBehavior.ACTIVATE_ABOVE_ACTIVATE_BELOW) &&
				(this.Behavior == TriggerBehavior.ACTIVATE_BELOW || this.Behavior == TriggerBehavior.ACTIVATE_ABOVE_ACTIVATE_BELOW)))
			)
			{
				this._priorThreshold = -1;
			}
		}
		// rising edge
		if (
		(this._priorThreshold != this.Threshold && currentHeartrate >= this.Threshold) ||
		(priorHeartrate < this.Threshold && currentHeartrate >= this.Threshold))
		{
			if (
				this.Behavior == TriggerBehavior.ACTIVATE_ABOVE_ACTIVATE_BELOW ||
				this.Behavior == TriggerBehavior.ACTIVATE_ABOVE)
			{
				if (HeartrateManager.Instance.Plugin.IsAuthenticated)
				{
					HeartrateManager.Instance.Plugin.TriggerHotkey(this.SelectedHotkey,
					(s) => { },
					(e) =>
					{
						Debug.LogError(string.Format("Error while triggering hotkey [{0}] in VTube Studio: {1} - {2}",
							this.SelectedHotkey, e.data.errorID, e.data.message));
					});
				}
				HotkeyEventMessage message =
					new HotkeyEventMessage(this.Threshold, currentHeartrate, this.SelectedHotkey, this.Behavior);
				APIManager.Instance.SendEvent(message);
			}
			// falling edge
		}
		else if (
		(this._priorThreshold != this.Threshold && currentHeartrate < this.Threshold) ||
		(priorHeartrate >= this.Threshold && currentHeartrate < this.Threshold))
		{
			if (
				this.Behavior == TriggerBehavior.ACTIVATE_ABOVE_ACTIVATE_BELOW ||
				this.Behavior == TriggerBehavior.ACTIVATE_BELOW)
			{
				if (HeartrateManager.Instance.Plugin.IsAuthenticated)
				{
					HeartrateManager.Instance.Plugin.TriggerHotkey(this.SelectedHotkey,
					(s) => { },
					(e) =>
					{
						Debug.LogError(string.Format("Error while triggering hotkey {0} in VTube Studio: {1} - {2}",
							this.SelectedHotkey, e.data.errorID, e.data.message));
					});
				}
			}
			HotkeyEventMessage message =
				new HotkeyEventMessage(this.Threshold, currentHeartrate, this.SelectedHotkey, this.Behavior);
			APIManager.Instance.SendEvent(message);
		}
		this._priorThreshold = this.Threshold;
		this._priorBehavior = this.Behavior;
	}

	private void OnHotkeySelectionChanged(string hotkeyID)
	{
		// try to find the index in the list of the hotkey with the given UUID
		int index =
			hotkeyID == null
			? -1
			: this._dropdown.options.FindIndex((o) => { return ((HotkeyDropdownOption)o).Data.hotkeyID.Equals(hotkeyID); });
		if (index < 0)
		{
			// if we can't find that UUID, we store it in a "pending" variable
			// Which we will check every time new options get loaded in
			this._waitingOnID = hotkeyID;
		}
		else if (this._dropdown.options.Count > 0 && this._dropdown.options.Count > index)
		{
			// if we can find the UUID, we clear our "pending" variable
			// and just set the correct option
			this._dropdown.SetValueWithoutNotify(index);
			this._waitingOnID = null;
		}
		this._minimizedSummary.text = string.Format("({0})", GetMinimizedText());
	}

	private string GetMinimizedText()
	{
		string name = GetDataFromDropdownSelection().hotkeyID != null
			? string.Format("[{0}] {1}", GetDataFromDropdownSelection().type, GetDataFromDropdownSelection().name)
			: "NO HOTKEY SET";
		if (name.Length > 48)
		{
			return string.Format("{0}...", name.Substring(0, 45));
		}
		else
		{
			return name;
		}
	}

	// TODO: this might actually be better than RefreshableDropdown, 
	public void RefreshHotkeyList()
	{
		HotkeyData currentSelection = GetDataFromDropdownSelection();
		this._dropdown.ClearOptions();
		List<HotkeyData> hotkeys = HeartrateManager.Instance.Plugin.GetHotkeysForModelID(ProfileManager.Instance.CurrentProfile.modelID);
		List<TMP_Dropdown.OptionData> hotkeyNames = new List<TMP_Dropdown.OptionData>();
		foreach (HotkeyData data in hotkeys)
		{
			HotkeyDropdownOption opt = new HotkeyDropdownOption();
			opt.text = string.Format("[{0}] {1}", data.type, data.name);
			opt.Data = data;
			hotkeyNames.Add(opt);
		}
		this._dropdown.AddOptions(hotkeyNames);
		this._dropdown.RefreshShownValue();
		if (this._waitingOnID != null)
		{
			OnHotkeySelectionChanged(this._waitingOnID);
		}
		else
		{
			OnHotkeySelectionChanged(currentSelection.hotkeyID);
		}
	}

	private void OnEditThreshold(string value)
	{
		this._thresholdVal = Mathf.Clamp(MathUtils.StringToInt(value), 0, 255);
		this._threshold.text = this._thresholdVal.ToString();
	}

	private HotkeyData GetDataFromDropdownSelection()
	{
		return this._dropdown.options.Count > 0 && this._dropdown.value >= 0
		? ((HotkeyDropdownOption)this._dropdown.options[this._dropdown.value]).Data
		: new HotkeyData();
	}

	// Extension class to strap arbitrary data onto a Unity Dropdown option
	public class HotkeyDropdownOption : TMP_Dropdown.OptionData
	{
		public HotkeyData Data { get; set; }
	}

	[Serializable]
	public class SaveData
	{
		public string hotkeyID;
		public int threshold;
		public TriggerBehavior behavior;

		public override string ToString()
		{
			return JsonUtility.ToJson(this);
		}
	}

	public SaveData ToSaveData()
	{
		SaveData data = new SaveData();
		data.threshold = this.Threshold;
		data.behavior = this.Behavior;
		data.hotkeyID = SelectedHotkey;
		return data;
	}

	public void FromSaveData(SaveData data)
	{
		this._behavior.ClearOptions();
		this._behavior.AddOptions(Names());
		this._threshold.text = data.threshold.ToString();
		this._thresholdVal = Mathf.Clamp(data.threshold, 0, 255);
		this._threshold.onEndEdit.AddListener(OnEditThreshold);
		this._behavior.SetValueWithoutNotify((int)data.behavior);
		this._dropdown.onValueChanged.AddListener((i) =>
		{
			OnHotkeySelectionChanged(((HotkeyDropdownOption)this._dropdown.options[i]).Data.hotkeyID);
		});
		RefreshHotkeyList();
	}

	public enum TriggerBehavior : int
	{
		ACTIVATE_ABOVE_ACTIVATE_BELOW = 0,
		ACTIVATE_ABOVE = 1,
		ACTIVATE_BELOW = 2,
	}

	private static List<string> Names()
	{
		return new List<String>(new String[] {
			"output_hotkey_behavior_ab",
			"output_hotkey_behavior_a",
			"output_hotkey_behavior_b"
            // "Activate above, Activate below",
            // "Activate above",
            // "Activate below",
        });
	}
}
