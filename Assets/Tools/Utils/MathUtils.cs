using System;

public static class MathUtils {
	/// <summary>
	/// Attempts to convert a string into a byte. Returns 0 if unable to convert.
	/// </summary>
	/// <param name="value"></param>
	/// <returns></returns>
	public static byte StringToByte(string value) {
		try {
			return Convert.ToByte(value);
		}
		catch {
			return 0;
		}
	}

	/// <summary>
	/// Attempts to conver a string into an int. Returns 0 if unable to convert.
	/// </summary>
	/// <param name="value"></param>
	/// <returns></returns>
	public static int StringToInt(string value) {
		try {
			return Convert.ToInt32(value);
		}
		catch {
			return 0;
		}
	}

	/// <summary>
	/// Normalizes a value from one range to another.
	/// </summary>
	/// <param name="val">The input value</param>
	/// <param name="valmin">The minimum of the input range</param>
	/// <param name="valmax">The maximum of the input range</param>
	/// <param name="min">The minimum of the output range</param>
	/// <param name="max">The maximum of the output range</param>
	/// <returns></returns>
	public static float Normalize(float val, float valmin, float valmax, float min, float max) {
		float numerator = ((val - valmin) / (valmax - valmin));
		numerator = Double.IsNaN(numerator) ? 0f : numerator;
		return (numerator * (max - min)) + min;
	}
}
