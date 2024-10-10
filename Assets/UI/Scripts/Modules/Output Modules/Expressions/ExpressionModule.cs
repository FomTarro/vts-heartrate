using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using VTS.Core;

public class ExpressionModule : MonoBehaviour
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
	private string _waitingOnFile = null;
	private string SelectedExpression
	{
		get
		{
			return this._waitingOnFile != null ?
			this._waitingOnFile :
			GetDataFromDropdownSelection().file;
		}
	}

	[SerializeField]
	private TMP_Dropdown _behavior = null;
	public TriggerBehavior Behavior { get { return (TriggerBehavior)this._behavior.value; } }
	private TriggerBehavior _priorBehavior = TriggerBehavior.ACTIVATE_ABOVE_DEACTIVATE_BELOW;
	[SerializeField]
	private TMP_Text _minimizedSummary = null;

	public void Clone()
	{
		HeartrateManager.Instance.Plugin.CreateExpressionModule(this.ToSaveData());
	}

	public void Delete()
	{
		HeartrateManager.Instance.Plugin.DestroyExpressionModule(this);
	}

	public void CheckModuleCondition(int priorHeartrate, int currentHeartrate)
	{
		if (this._priorBehavior != this.Behavior)
		{
			// forces a re-assessment of the module status
			this._priorThreshold = -1;
		}
		// rising edge
		if (
		(this._priorThreshold != this.Threshold && currentHeartrate >= this.Threshold) ||
		(priorHeartrate < this.Threshold && currentHeartrate >= this.Threshold))
		{
			if (
				this.Behavior == ExpressionModule.TriggerBehavior.ACTIVATE_ABOVE_DEACTIVATE_BELOW ||
				this.Behavior == ExpressionModule.TriggerBehavior.ACTIVATE_ABOVE)
			{
				if (HeartrateManager.Instance.Plugin.IsAuthenticated)
				{
					HeartrateManager.Instance.Plugin.SetExpressionState(this.SelectedExpression, true,
					(s) => { },
					(e) =>
					{
						Debug.LogError(string.Format("Error while setting expression {0} in VTube Studio: {1} - {2}",
							this.SelectedExpression, e.data.errorID, e.data.message));
					});
				}
				ExpressionEventMessage message =
					new ExpressionEventMessage(this.Threshold, currentHeartrate, this.SelectedExpression, this.Behavior, true);
				APIManager.Instance.SendEvent(message);
			}
			else if (
				this.Behavior == ExpressionModule.TriggerBehavior.DEACTIVATE_ABOVE_ACTIVATE_BELOW ||
				this.Behavior == ExpressionModule.TriggerBehavior.DEACTIVATE_ABOVE)
			{
				if (HeartrateManager.Instance.Plugin.IsAuthenticated)
				{
					HeartrateManager.Instance.Plugin.SetExpressionState(this.SelectedExpression, false,
					(s) => { },
					(e) =>
					{
						Debug.LogError(string.Format("Error while setting expression {0} in VTube Studio: {1} - {2}",
							this.SelectedExpression, e.data.errorID, e.data.message));
					});
				}
				ExpressionEventMessage message =
					new ExpressionEventMessage(this.Threshold, currentHeartrate, this.SelectedExpression, this.Behavior, false);
				APIManager.Instance.SendEvent(message);
			}
			// falling edge
		}
		else if (
		(this._priorThreshold != this.Threshold && currentHeartrate < this.Threshold) ||
		(priorHeartrate >= this.Threshold && currentHeartrate < this.Threshold))
		{
			if (
				this.Behavior == ExpressionModule.TriggerBehavior.DEACTIVATE_ABOVE_ACTIVATE_BELOW ||
				this.Behavior == ExpressionModule.TriggerBehavior.ACTIVATE_BELOW)
			{
				if (HeartrateManager.Instance.Plugin.IsAuthenticated)
				{
					HeartrateManager.Instance.Plugin.SetExpressionState(this.SelectedExpression, true,
					(s) => { },
					(e) =>
					{
						Debug.LogError(string.Format("Error while setting expression {0} in VTube Studio: {1} - {2}",
							this.SelectedExpression, e.data.errorID, e.data.message));
					});
				}
				ExpressionEventMessage message =
					new ExpressionEventMessage(this.Threshold, currentHeartrate, this.SelectedExpression, this.Behavior, true);
				APIManager.Instance.SendEvent(message);
			}
			else if (
				this.Behavior == ExpressionModule.TriggerBehavior.ACTIVATE_ABOVE_DEACTIVATE_BELOW ||
				this.Behavior == ExpressionModule.TriggerBehavior.DEACTIVATE_BELOW)
			{
				if (HeartrateManager.Instance.Plugin.IsAuthenticated)
				{
					HeartrateManager.Instance.Plugin.SetExpressionState(this.SelectedExpression, false,
					(s) => { },
					(e) =>
					{
						Debug.LogError(string.Format("Error while setting expression [{0}] in VTube Studio: {1} - {2}",
							this.SelectedExpression, e.data.errorID, e.data.message));
					});
				}
				ExpressionEventMessage message =
					new ExpressionEventMessage(this.Threshold, currentHeartrate, this.SelectedExpression, this.Behavior, false);
				APIManager.Instance.SendEvent(message);
			}
		}
		this._priorThreshold = this.Threshold;
		this._priorBehavior = this.Behavior;
	}

	private void OnExpressionSelectionChanged(string expressionFile)
	{
		// try to find the index in the list of the expression with the given file name
		int index =
			expressionFile == null
			? -1
			: this._dropdown.options.FindIndex((o) => { return ((ExpressionDropdownOption)o).Data.file.Equals(expressionFile); });
		if (index < 0)
		{
			// if we can't find that file, we store it in a "pending" variable
			// Which we will check every time new options get loaded in
			this._waitingOnFile = expressionFile;
		}
		else if (this._dropdown.options.Count > 0 && this._dropdown.options.Count > index)
		{
			// if we can find the file, we clear our "pending" variable
			// and just set the correct option
			this._dropdown.SetValueWithoutNotify(index);
			this._waitingOnFile = null;
		}
		this._minimizedSummary.text = string.Format("({0})", GetMinimizedText());
	}

	private string GetMinimizedText()
	{
		string name = GetDataFromDropdownSelection().file != null
		? string.Format("{0}", GetDataFromDropdownSelection().name)
		: "NO EXPRESSION SET";
		if (name.Length > 48)
		{
			return string.Format("{0}...", name.Substring(0, 45));
		}
		else
		{
			return name;
		}
	}

	// TODO: this might actually be better than RefreshableDropdown
	// no longer as brittle with string-matching
	public void RefreshExpressionList()
	{
		ExpressionData currentSelection = GetDataFromDropdownSelection();
		this._dropdown.ClearOptions();
		List<ExpressionData> expressions = HeartrateManager.Instance.Plugin.GetExpressionsForModelID(ProfileManager.Instance.CurrentProfile.modelID);
		List<TMP_Dropdown.OptionData> expressionNames = new List<TMP_Dropdown.OptionData>();
		foreach (ExpressionData data in expressions)
		{
			ExpressionDropdownOption opt = new ExpressionDropdownOption();
			opt.text = string.Format("{0}", data.name);
			opt.Data = data;
			expressionNames.Add(opt);
		}
		this._dropdown.AddOptions(expressionNames);
		this._dropdown.RefreshShownValue();
		if (this._waitingOnFile != null)
		{
			OnExpressionSelectionChanged(this._waitingOnFile);
		}
		else
		{
			OnExpressionSelectionChanged(currentSelection.file);
		}
	}

	private void OnEditThreshold(string value)
	{
		this._thresholdVal = Mathf.Clamp(MathUtils.StringToInt(value), 0, 255);
		this._threshold.text = this._thresholdVal.ToString();
	}

	private ExpressionData GetDataFromDropdownSelection()
	{
		return this._dropdown.options.Count > 0 && this._dropdown.value >= 0
		? ((ExpressionDropdownOption)this._dropdown.options[this._dropdown.value]).Data
		: new ExpressionData();
	}

	// Extension class to strap arbitrary data onto a Unity Dropdown option
	public class ExpressionDropdownOption : ExtendedDropdownOption<ExpressionData> { }


	[System.Serializable]
	public class SaveData
	{
		public string expressionFile;
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
		data.expressionFile = this.SelectedExpression;
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
			OnExpressionSelectionChanged(((ExpressionDropdownOption)this._dropdown.options[i]).Data.file);
		});
		OnExpressionSelectionChanged(data.expressionFile);
		RefreshExpressionList();
	}

	public enum TriggerBehavior : int
	{
		ACTIVATE_ABOVE_DEACTIVATE_BELOW = 0,
		DEACTIVATE_ABOVE_ACTIVATE_BELOW = 1,
		ACTIVATE_ABOVE = 2,
		DEACTIVATE_ABOVE = 3,
		ACTIVATE_BELOW = 4,
		DEACTIVATE_BELOW = 5,
	}

	private static List<string> Names()
	{
		return new List<String>(new String[] {
			"output_expressions_behavior_aadb",
			"output_expressions_behavior_daab",
			"output_expressions_behavior_aa",
			"output_expressions_behavior_da",
			"output_expressions_behavior_db",
			"output_expressions_behavior_ab"
            // "Activate above, Deactivate below",
            // "Deactivate above, Activate below",
            // "Activate above",
            // "Deactivate above",
            // "Activate below",
            // "Deactivate below",
        });
	}
}
