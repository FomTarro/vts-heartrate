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
}
