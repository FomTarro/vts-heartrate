using UnityEngine;

public static class TransformUtils {
	public static int GetActiveChildCount(Transform tf) {
		int count = 0;
		foreach (Transform child in tf) {
			if (child.gameObject.activeInHierarchy) {
				count += 1;
			}
		}
		return count;
	}
}
