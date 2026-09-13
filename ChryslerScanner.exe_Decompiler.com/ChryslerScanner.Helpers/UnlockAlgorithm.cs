using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ChryslerScanner.Helpers;

public static class UnlockAlgorithm
{
	public enum Controllers
	{
		SBEC = 1,
		EATX,
		JTEC
	}

	public enum SecurityLevels
	{
		Level1 = 1,
		Level2,
		Level3
	}

	public static byte[] GetSecurityKey(Controllers controller, SecurityLevels level, byte[] seed)
	{
		if (seed.Length != 2)
		{
			return null;
		}
		if (seed.All((byte s) => s == 0))
		{
			return null;
		}
		ushort num = (ushort)((seed[0] << 8) + seed[1]);
		ushort value = 0;
		if ((uint)(controller - 1) <= 2u)
		{
			switch (level)
			{
			case SecurityLevels.Level1:
				value = (ushort)((num << 2) + 36888);
				break;
			case SecurityLevels.Level2:
			{
				value = (ushort)(num & 0xFF00u);
				value |= (ushort)(value >> 8);
				ushort num2 = (ushort)(num & 0xFFu);
				num2 |= (ushort)(num2 << 8);
				value = (ushort)(value ^ 0x9340u);
				value += 4112;
				value ^= num2;
				value += 6417;
				uint num3 = (uint)((value << 16) | value);
				value += (ushort)(num3 >> 3);
				break;
			}
			case SecurityLevels.Level3:
			{
				value = (ushort)((uint)(num + 9340) | 5u);
				byte b = (byte)(value & 0xFu);
				value = (ushort)((value >> (int)b) | (value << 16 - b));
				value = (ushort)(value | 0x247Cu);
				break;
			}
			}
		}
		byte[] bytes = BitConverter.GetBytes(value);
		if (bytes.Length != 2)
		{
			return null;
		}
		if (BitConverter.IsLittleEndian)
		{
			Array.Reverse((Array)bytes);
		}
		return bytes;
	}

	public static byte[] GetSKIMUnlockKey(byte[] seed, string VIN)
	{
		if (VIN.Length != 17)
		{
			return null;
		}
		if (seed.All((byte s) => s == 0))
		{
			return null;
		}
		byte[] array = new byte[3];
		switch (seed.Length)
		{
		case 2:
			array[0] = seed[1];
			array[1] = seed[1];
			array[2] = seed[0];
			break;
		case 4:
			array[0] = seed[1];
			array[1] = seed[2];
			array[2] = seed[3];
			break;
		default:
			return null;
		}
		byte[] bytes = Encoding.ASCII.GetBytes(VIN);
		foreach (byte item in new List<byte> { 16, 15, 13, 8 })
		{
			byte b = bytes[item];
			array[2] += b;
			array[2] ^= b;
			array[1] += array[2];
			array[0] += array[1];
			array[0] ^= b;
		}
		return array;
	}
}
