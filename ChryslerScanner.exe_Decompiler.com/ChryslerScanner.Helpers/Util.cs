using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ChryslerScanner.Helpers;

public static class Util
{
	public static bool IsWindows
	{
		get
		{
			PlatformID platform = Environment.OSVersion.Platform;
			if (platform != PlatformID.Win32Windows)
			{
				return platform == PlatformID.Win32NT;
			}
			return true;
		}
	}

	public static string ByteToHexString(byte[] data, int offset = 0, int length = 1, int maxNumberCount = 16)
	{
		if (data.Length != 0)
		{
			StringBuilder stringBuilder = new StringBuilder();
			for (int i = offset; i < offset + length; i++)
			{
				if (maxNumberCount > 0 && stringBuilder.Length != 0 && i % maxNumberCount == offset)
				{
					stringBuilder.Append(Environment.NewLine);
				}
				stringBuilder.Append(Convert.ToString(data[i], 16).PadLeft(2, '0').PadRight(3, ' ')
					.ToUpper());
			}
			stringBuilder.Remove(stringBuilder.Length - 1, 1);
			return stringBuilder.ToString();
		}
		return string.Empty;
	}

	public static string ByteToHexStringSimple(byte[] data, int maxNumberCount = 16)
	{
		return ByteToHexString(data, 0, data.Length, maxNumberCount);
	}

	public static byte[] HexStringToByte(string str)
	{
		string ret = str.Trim().Replace(" ", string.Empty).Replace(",", string.Empty)
			.Replace(";", string.Empty)
			.Replace("$", string.Empty)
			.Replace("0x", string.Empty);
		try
		{
			return (from x in Enumerable.Range(0, ret.Length)
				where x % 2 == 0
				select Convert.ToByte(ret.Substring(x, 2), 16)).ToArray();
		}
		catch
		{
			return new byte[0];
		}
	}

	public static void UpdateTextBox(TextBox TB, string text, byte[] bytes = null)
	{
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Expected O, but got Unknown
		if (((Control)TB).IsDisposed)
		{
			return;
		}
		StringBuilder ret = new StringBuilder();
		((Control)TB).Invoke((Delegate)(MethodInvoker)delegate
		{
			if (((Control)TB).Text != "")
			{
				ret.Append(Environment.NewLine + Environment.NewLine);
			}
			ret.Append(text);
			if (bytes != null)
			{
				ret.Append(Environment.NewLine + ByteToHexString(bytes, 0, bytes.Length));
			}
			if (((TextBoxBase)TB).TextLength + ret.Length > ((TextBoxBase)TB).MaxLength)
			{
				((TextBoxBase)TB).Clear();
				GC.Collect();
			}
			((TextBoxBase)TB).AppendText(ret.ToString());
			if (((Control)TB).Name == "USBTextBox")
			{
				File.AppendAllText(MainForm.USBTextLogFilename, ret.ToString());
			}
			using BinaryWriter binaryWriter = new BinaryWriter(File.Open(MainForm.USBBinaryLogFilename, FileMode.Append));
			if (bytes != null)
			{
				binaryWriter.Write(bytes);
				binaryWriter.Close();
			}
		});
	}

	public static bool CompareArrays(byte[] first, byte[] second, int index, int length)
	{
		bool result = false;
		if (first.Length < index + length || second.Length < index + length)
		{
			return false;
		}
		for (int i = index; i < index + length; i++)
		{
			result = ((first[i] == second[i]) ? true : false);
		}
		return result;
	}

	public static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
	{
		return new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddSeconds(unixTimeStamp).ToLocalTime();
	}

	public static string TruncateString(string value, int maxLength)
	{
		if (string.IsNullOrEmpty(value))
		{
			return value;
		}
		if (value.Length > maxLength)
		{
			return value.Substring(0, maxLength);
		}
		return value;
	}

	public static byte SetBit(byte value, byte position)
	{
		return value |= (byte)(1 << (int)position);
	}

	public static byte ClearBit(byte value, byte position)
	{
		return value &= (byte)(~(1 << (int)position));
	}

	public static byte InvertBit(byte value, byte position)
	{
		return value ^= (byte)(1 << (int)position);
	}

	public static bool IsBitSet(byte value, byte position)
	{
		return (value & (1 << (int)position)) != 0;
	}

	public static bool IsBitClear(byte value, byte position)
	{
		return !IsBitSet(value, position);
	}

	public static byte ChecksumCalculator(byte[] data, int index, int length)
	{
		byte b = 0;
		for (int i = index; i < length; i++)
		{
			b += data[i];
		}
		return b;
	}

	public static byte CRCCalculator(byte[] data, int index, int length)
	{
		byte b = byte.MaxValue;
		int num = 0;
		int num2 = index;
		while (num2 < length)
		{
			byte b2 = 0;
			byte b3 = 128;
			while (b2 < 8)
			{
				if ((b3 & data[num]) > 0)
				{
					byte b4 = (byte)(IsBitSet(b, 7) ? 1u : 28u);
					b = (byte)(((uint)(b << 1) | 1u) ^ b4);
				}
				else
				{
					byte b4 = (byte)(IsBitSet(b, 7) ? 29u : 0u);
					b = (byte)((b << 1) ^ b4);
				}
				b2++;
				b3 >>= 1;
			}
			num2++;
			num++;
		}
		return (byte)(~b);
	}
}
