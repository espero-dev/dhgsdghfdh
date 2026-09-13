using System;
using System.IO;

namespace DRBDBReader.DB;

public class SimpleBinaryReader
{
	public byte[] rawDB;

	public SimpleBinaryReader(FileStream dbFile)
	{
		rawDB = new byte[dbFile.Length];
		dbFile.Read(rawDB, 0, rawDB.Length);
	}

	public byte[] ReadBytes(ref int offset, int length)
	{
		byte[] array = new byte[length];
		Array.ConstrainedCopy(rawDB, offset, array, 0, array.Length);
		offset += length;
		return array;
	}

	public byte ReadUInt8(ref int offset)
	{
		byte result = rawDB[offset];
		offset++;
		return result;
	}

	public ushort ReadUInt16(ref int offset)
	{
		ushort result = BitConverter.ToUInt16(rawDB, offset);
		offset += 2;
		return result;
	}

	public uint ReadUInt32(ref int offset)
	{
		uint result = BitConverter.ToUInt32(rawDB, offset);
		offset += 4;
		return result;
	}

	public ulong ReadUInt64(ref int offset)
	{
		ulong result = BitConverter.ToUInt64(rawDB, offset);
		offset += 8;
		return result;
	}

	public sbyte ReadInt8(ref int offset)
	{
		sbyte result = Convert.ToSByte(rawDB[offset]);
		offset++;
		return result;
	}

	public short ReadInt16(ref int offset)
	{
		short result = BitConverter.ToInt16(rawDB, offset);
		offset += 2;
		return result;
	}

	public int ReadInt32(ref int offset)
	{
		int result = BitConverter.ToInt32(rawDB, offset);
		offset += 4;
		return result;
	}

	public long ReadInt64(ref int offset)
	{
		long result = BitConverter.ToInt64(rawDB, offset);
		offset += 8;
		return result;
	}

	public float ReadFloat(ref int offset)
	{
		float result = BitConverter.ToSingle(rawDB, offset);
		offset += 4;
		return result;
	}

	public double ReadDouble(ref int offset)
	{
		double result = BitConverter.ToDouble(rawDB, offset);
		offset += 8;
		return result;
	}
}
