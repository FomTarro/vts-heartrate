using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VTS.Models;

public class ColorInputModule : MonoBehaviour {

	[SerializeField]
	private Color32 _color = Color.white;
	public Color32 ModuleColor { get { return this._color; } }
	private Color32 _currentColor = Color.white;
	public Color32 ModuleInterpolatedColor { get { return this._currentColor; } }
	private string[] _matchers = new string[0];
	public String[] ModuleMatchers { get { return this._matchers; } }

	[SerializeField]
	private TMP_InputField _redField = null;
	[SerializeField]
	private TMP_InputField _greenField = null;
	[SerializeField]
	private TMP_InputField _blueField = null;
	[SerializeField]
	private TMP_InputField _alphaField = null;
	[SerializeField]
	private TMP_InputField _hexField = null;
	[SerializeField]
	private TMP_InputField _matchersField = null;
	[SerializeField]
	private TMP_Text _minimizedSummary = null;

	[SerializeField]
	private Image _background = null;

	private void Start() {
		this._redField.onEndEdit.AddListener(SetRed);
		this._greenField.onEndEdit.AddListener(SetGreen);
		this._blueField.onEndEdit.AddListener(SetBlue);
		this._alphaField.onEndEdit.AddListener(SetAlpha);
		this._hexField.onEndEdit.AddListener(SetHex);
		this._matchersField.onEndEdit.AddListener(SetMatchers);
	}

	public void Clone() {
		HeartrateManager.Instance.Plugin.CreateColorTintModule(this.ToSaveData());
	}

	public void Delete() {
		HeartrateManager.Instance.Plugin.DestroyColorTintModule(this);
		ApplyColor(0);
	}

	public void ApplyColor(float interpolation) {
		this._currentColor = Color32.Lerp(Color.white, this.ModuleColor, interpolation);
		if (HeartrateManager.Instance.Plugin.IsAuthenticated) {
			ArtMeshMatcher matcher = new ArtMeshMatcher();
			matcher.tintAll = false;
			matcher.nameContains = this.ModuleMatchers;
			HeartrateManager.Instance.Plugin.TintArtMesh(
				this._currentColor,
				0.5f,
				matcher,
				(success) => { },
				(error) => {
					Debug.LogError(string.Format("Error while applying color tint in VTube Studio: {0} - {1}",
						error.data.errorID, error.data.message));
				});
		}
	}

	public void SetRed(string value) {
		byte v = MathUtils.StringToByte(value);
		this._color = new Color32(
			v,
			this._color.g,
			this._color.b,
			this._color.a);
		this._redField.text = v.ToString();
		this._hexField.text = "#" + ColorUtility.ToHtmlStringRGBA(this._color);
		this._background.color = this._color;
	}

	public void SetGreen(string value) {
		byte v = MathUtils.StringToByte(value);
		this._color = new Color32(
			this._color.r,
			v,
			this._color.b,
			this._color.a);
		this._greenField.text = v.ToString();
		this._hexField.text = "#" + ColorUtility.ToHtmlStringRGBA(this._color);
		this._background.color = this._color;
	}

	public void SetBlue(string value) {
		byte v = MathUtils.StringToByte(value);
		this._color = new Color32(
			this._color.r,
			this._color.g,
			v,
			this._color.a);
		this._blueField.text = v.ToString();
		this._hexField.text = "#" + ColorUtility.ToHtmlStringRGBA(this._color);
		this._background.color = this._color;
	}

	public void SetAlpha(string value) {
		byte v = MathUtils.StringToByte(value);
		this._color = new Color32(
			this._color.r,
			this._color.g,
			this._color.b,
			v);
		this._alphaField.text = v.ToString();
		this._hexField.text = "#" + ColorUtility.ToHtmlStringRGBA(this._color);
		this._background.color = this._color;
	}

	public void SetHex(string hex) {
		Color color;
		if (ColorUtility.TryParseHtmlString(hex, out color)) {
			this._hexField.text = hex.ToLower();
			Color32 colorBytes = (Color32)color;
			SetRed(colorBytes.r.ToString());
			SetGreen(colorBytes.g.ToString());
			SetBlue(colorBytes.b.ToString());
			SetAlpha(colorBytes.a.ToString());
		}
		else {
			this._hexField.text = "#" + ColorUtility.ToHtmlStringRGBA(this._color);
		}
	}

	public void SetMatchers(string value) {
		string[] split = value.Trim().Split(' ', ',');
		List<string> sanitized = new List<string>();
		for (int i = 0; i < split.Length; i++) {
			split[i] = split[i].Trim();
			if (split[i].Length > 0) {
				sanitized.Add(split[i]);
			}
		}
		// wipes old colors in cases where matchers are removed
		ApplyColor(0);
		this._matchers = sanitized.ToArray();
		this._matchersField.text = string.Join(",", sanitized);
		this._minimizedSummary.text = string.Format("({0})",
			this._matchersField.text.Length > 48
			? string.Format("{0}...", this._matchersField.text.Substring(0, 45))
			: this._matchersField.text);
	}

	public void RequestArtMeshSelection() {
		Debug.Log("Requesting Art Mesh selection in VTube Studio...");
		HeartrateManager.Instance.Plugin.GetArtMeshList(
			(meshSuccess) => {
				// determine which art meshes are already soft-selected by the provided text
				List<string> fuzzyMatches = new List<string>();
				foreach (string matcher in this._matchers) {
					foreach (string mesh in meshSuccess.data.artMeshNames) {
						if (mesh.Contains(matcher)) {
							fuzzyMatches.Add(mesh);
						}
					}
				}
				HeartrateManager.Instance.Plugin.RequestArtMeshSelection(
					Localization.LocalizationManager.Instance.GetString("output_artmesh_selection_title"),
					Localization.LocalizationManager.Instance.GetString("output_artmesh_selection_tooltip"), -1, fuzzyMatches,
				(selectionSuccess) => {
					if (selectionSuccess.data.success) {
						Debug.Log("Art Mesh selection request completed.");
						// remove the existing fuzzy matches from the selection set
						List<string> selectedMeshes = new List<string>(selectionSuccess.data.activeArtMeshes);
						foreach (string mesh in fuzzyMatches) {
							selectedMeshes.Remove(mesh);
						}
						List<string> newMatchers = new List<string>();
						newMatchers.AddRange(this._matchers);
						newMatchers.AddRange(selectedMeshes);
						// remove specific meshes that we disabled from the selected set, too
						List<string> deselectedMeshes = new List<string>(selectionSuccess.data.inactiveArtMeshes);
						foreach (string mesh in deselectedMeshes) {
							newMatchers.Remove(mesh);
						}
						SetMatchers(string.Join(",", newMatchers));
					}
					else {
						Debug.Log("Art Mesh selection request was cancelled!");
					}
				},
				(selectionError) => {
					Debug.LogError(string.Format("Error while selecting Art Meshes in VTube Studio: {0} - {1}",
					selectionError.data.errorID, selectionError.data.message));
				});
			},
			(meshError) => {
				Debug.LogError(string.Format("Error while querying Mesh Data from VTube Studio: {0} - {1}",
					meshError.data.errorID, meshError.data.message));
			});
	}

	[System.Serializable]
	public class SaveData {
		public Color32 color = Color.white;
		public string[] matchers = new string[0];

		public override string ToString() {
			return JsonUtility.ToJson(this);
		}
	}

	public SaveData ToSaveData() {
		SaveData data = new SaveData();
		data.color = this._color;
		data.matchers = this._matchers;

		return data;
	}

	public void FromSaveData(SaveData data) {
		SetRed(data.color.r.ToString());
		SetGreen(data.color.g.ToString());
		SetBlue(data.color.b.ToString());
		SetAlpha(data.color.a.ToString());
		SetMatchers(string.Join(",", data.matchers));
	}
}
