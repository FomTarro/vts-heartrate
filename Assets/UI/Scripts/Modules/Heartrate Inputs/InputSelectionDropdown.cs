using System.Collections.Generic;

public class InputSelectionDropdown : RefreshableDropdown {
	private Dictionary<string, HeartrateInputModule> _modules = new Dictionary<string, HeartrateInputModule>();

	protected override void Initialize() {
		Refresh();
	}

	public override void Refresh() {
		this._modules.Clear();
		foreach (HeartrateInputModule module in HeartrateManager.Instance.Plugin.HeartrateInputs) {
			this._modules.Add(module.ToString(), module);
		}
		RefreshValues(this._modules.Keys);
	}

	protected override void SetValue(int index) {
		this._modules[this._dropdown.options[index].text].Activate();
	}

	private void Update() {
		//TODO: this is janked up, but polls to check what the active input module is so as to make the UI reflect that
		string activeModule = HeartrateManager.Instance.Plugin.ActiveInputModule;
		if (HeartrateManager.Instance.Plugin.ActiveInputModule != null && this._dropdown.value >= 0 &&
		(this._dropdown.options[this._dropdown.value].text != activeModule)) {
			int index = this.StringToIndex(activeModule);
			this._dropdown.SetValueWithoutNotify(index);
			this.SetValue(index);
		}
	}
}
