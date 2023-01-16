using UnityEngine;
using UnityEngine.UI;

public static class UIUtils {
	public static int GetTextBestFitSize(Text text) {
		text.cachedTextGenerator.Invalidate();
		Vector2 size = (text.transform as RectTransform).rect.size;
		TextGenerationSettings tempSettings = text.GetGenerationSettings(size);
		tempSettings.scaleFactor = 1;
		if (!text.cachedTextGenerator.Populate(text.text, tempSettings)) {
			Debug.LogWarningFormat("Text object with value: '{0}' cannot be rebuilt!", text.text);
		}
		return text.cachedTextGenerator.fontSizeUsedForBestFit;
	}
}