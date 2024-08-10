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
	private string _waitingOn = null;
	public string SelectedHotkey
	{
		get
		{
			return this._waitingOn == null ?
			(this._dropdown.value < HOTKEYS.Count ? HOTKEYS[this._dropdown.value].hotkeyID : null) :
			this._waitingOn;
		}
	}

	[SerializeField]
	private TMP_Dropdown _behavior = null;
	public TriggerBehavior Behavior { get { return (TriggerBehavior)this._behavior.value; } }
	private TriggerBehavior _priorBehavior = TriggerBehavior.ACTIVATE_ABOVE_ACTIVATE_BELOW;
	[SerializeField]
	private TMP_Text _minimizedSummary = null;

	private static List<HotkeyData> HOTKEYS = new List<HotkeyData>();

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
						Debug.LogError(string.Format("Error while triggering hotkey {0} in VTube Studio: {1} - {2}",
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

	private int HotkeyToIndex(string hotkeyID)
	{
		return hotkeyID == null
			? -1
			: this._dropdown.options.FindIndex((o) => { return o.text.Contains(hotkeyID); });
	}

	private void SetHotkey(string hotkeyID)
	{
		// index will only be -1 if the desired item is not in the list
		int index = HotkeyToIndex(hotkeyID);
		if (index < 0)
		{
			this._waitingOn = hotkeyID;
		}
		else if (this._dropdown.options.Count > 0 && this._dropdown.options.Count > index)
		{
			this._dropdown.SetValueWithoutNotify(index);
			// finally found what we were waiting for
			this._waitingOn = null;
		}
		this._minimizedSummary.text = string.Format("({0})", GetMinimizedText());
	}

	private string GetMinimizedText()
	{
		string name = this._dropdown.options.Count > 0 && HOTKEYS.Count >= this._dropdown.options.Count
			? string.Format("[{0}] {1}", HOTKEYS[this._dropdown.value].type, HOTKEYS[this._dropdown.value].name)
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

	// TODO: consolidate this behavior into RefreshableDropdown
	public void RefreshHotkeyList()
	{
		int currentIndex = this._dropdown.value;
		string hotkey = this._dropdown.options.Count > 0 ? this._dropdown.options[currentIndex].text : null;
		this._dropdown.ClearOptions();
		HOTKEYS = HeartrateManager.Instance.Plugin.GetHotkeysForModelID(ProfileManager.Instance.CurrentProfile.modelID);
		List<string> hotkeyNames = new List<string>();
		foreach (HotkeyData data in HOTKEYS)
		{
			hotkeyNames.Add(string.Format("[{0}] <size=0>{1}</size>{2}", data.type, data.hotkeyID, data.name));
		}
		this._dropdown.AddOptions(hotkeyNames);
		this._dropdown.RefreshShownValue();
		if (this._waitingOn != null)
		{
			SetHotkey(this._waitingOn);
		}
		else
		{
			SetHotkey(hotkey);
		}
	}

	private void OnEditThreshold(string value)
	{
		this._thresholdVal = Mathf.Clamp(MathUtils.StringToInt(value), 0, 255);
		this._threshold.text = this._thresholdVal.ToString();
	}

	[System.Serializable]
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
		data.hotkeyID = this.SelectedHotkey;
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
		SetHotkey(data.hotkeyID);
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
