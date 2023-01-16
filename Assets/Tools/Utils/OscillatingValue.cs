using UnityEngine;

public class OscillatingValue {
	private float _time = 0;
	private float _pos = 0;

	public float GetValue(float hertz) {
		float delta = Time.deltaTime * (hertz);
		this._time = this._pos >= 1 ? 0 : this._time + delta;
		this._pos = Mathf.Lerp(0, 1, this._time);
		return 0.5f * (1 + Mathf.Sin((2 * Mathf.PI) * this._pos));
	}
}
