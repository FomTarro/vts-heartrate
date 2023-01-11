using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FitbitModelDropdown : RefreshableDropdown {
	private Dictionary<string, FitbitManager.FitbitModelSDKMap> _models = new Dictionary<string, FitbitManager.FitbitModelSDKMap>();
	private Dictionary<FitbitManager.FitbitModel, string> _keys = new Dictionary<FitbitManager.FitbitModel, string>();

	[SerializeField]
	private RawImage _qrCode = null;

	protected override void Initialize() {
		Refresh();
	}

	public override void Refresh() {
		this._models.Clear();
		this._keys.Clear();
		foreach (FitbitManager.FitbitModelSDKMap model in FitbitManager.Instance.ModelsToSDKMap) {
			this._models.Add(model.key, model);
			this._keys.Add(model.model, model.key);
		}
		RefreshValues(this._models.Keys);
		FitbitManager.FitbitModel selectedModel = FitbitManager.Instance.SelectedModel;
		int index = this.StringToIndex(this._keys[selectedModel]);
		this._dropdown.SetValueWithoutNotify(index);
		this.SetValue(index);
	}

	protected override void SetValue(int index) {
		FitbitManager.Instance.SetSelectedModel(this._models[this._dropdown.options[index].text].model);
		Destroy(this._qrCode.texture);
        this._qrCode.texture = FitbitManager.Instance.GetGalleryLinkForSDK(this._models[this._dropdown.options[index].text].sdk);
	}

	private void Update() {
		//TODO: this is janked up, but polls to check what the active input module is so as to make the UI reflect that
		FitbitManager.FitbitModel selectedModel = FitbitManager.Instance.SelectedModel;
		if (this._models.Count > 0 && this._dropdown.value >= 0 && (this._models[this._dropdown.options[this._dropdown.value].text].model != selectedModel)) {
			int index = this.StringToIndex(this._keys[selectedModel]);
			this._dropdown.SetValueWithoutNotify(index);
			this.SetValue(index);
		}
	}
}
