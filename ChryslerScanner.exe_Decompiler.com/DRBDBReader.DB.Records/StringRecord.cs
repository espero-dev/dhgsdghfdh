using System;
using System.Text;
using System.Threading;

namespace DRBDBReader.DB.Records;

public class StringRecord : Record
{
	private const byte FIELD_ID = 0;

	private const byte FIELD_LOCATION = 1;

	private const byte FIELD_OBD_CODE_STR = 3;

	public ushort id;

	private uint location;

	public byte textTableNumber;

	public uint textTableOffset;

	public string text;

	private byte[] obdCodeBytes;

	public string obdCodeString;

	public static ThreadLocal<StringBuilder> cachedSB = new ThreadLocal<StringBuilder>(() => new StringBuilder());

	public StringRecord(Table table, byte[] record)
		: base(table, record)
	{
		id = (ushort)base.table.readField(this, 0);
		location = (uint)base.table.readField(this, 1);
		textTableNumber = (byte)(location >> 24);
		textTableOffset = location & 0xFFFFFFu;
		text = readText();
		if (!base.table.db.isStarScanDB)
		{
			obdCodeBytes = base.table.readFieldRaw(this, 3);
			int num = Array.IndexOf(obdCodeBytes, (byte)0);
			if (num == -1)
			{
				num = obdCodeBytes.Length;
			}
			obdCodeString = Encoding.ASCII.GetString(obdCodeBytes, 0, num);
		}
		else
		{
			obdCodeBytes = null;
			obdCodeString = "";
		}
	}

	private string readText()
	{
		Table table = base.table.db.tables[26 + textTableNumber];
		uint num = (uint)Math.Floor((double)(textTableOffset / table.rowSize));
		uint num2 = textTableOffset % table.rowSize;
		Record record = table.records[num];
		StringBuilder value = cachedSB.Value;
		value.Clear();
		if (value.Capacity >= 256)
		{
			value.Capacity = 64;
		}
		byte value2;
		while ((value2 = record.record[num2++]) != 0)
		{
			value.Append(Convert.ToChar(value2));
			if (num2 >= table.rowSize)
			{
				num2 = 0u;
				record = table.records[++num];
			}
		}
		return value.ToString();
	}
}
