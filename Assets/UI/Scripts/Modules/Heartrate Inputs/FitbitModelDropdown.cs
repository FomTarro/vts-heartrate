using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FitbitModelDropdown : RefreshableDropdown {

	private Dictionary<string, FitbitManager.FitBitModelSDKMap> _models = new Dictionary<string, FitbitManager.FitBitModelSDKMap>();

	public override void Refresh() {
		this._models.Clear();
		foreach (FitbitManager.FitBitModelSDKMap model in FitbitManager.Instance.ModelsToSDKMap) {
			this._models.Add(model.key, model);
		}
		RefreshValues(this._models.Keys);
	}

	protected override void Initialize() {

	}

	protected override void SetValue(int index) {
        FitbitManager.Instance.SetSDK(this._models[this._dropdown.options[index].text].sdk);
	}
}
