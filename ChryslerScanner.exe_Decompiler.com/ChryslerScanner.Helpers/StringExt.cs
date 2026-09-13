namespace ChryslerScanner.Helpers;

public static class StringExt
{
	public static bool IsNumeric(this string text)
	{
		double result;
		return double.TryParse(text, out result);
	}
}
