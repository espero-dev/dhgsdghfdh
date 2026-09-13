using System;

namespace DRBDBReader.DB.Records;

public class NCRecord : Record
{
	private const byte FIELD_ID = 0;

	private const byte FIELD_SLOPE = 1;

	private const byte FIELD_OFFSET = 2;

	private const byte FIELD_THREE = 3;

	private const byte FIELD_EMPTY = 4;

	public ushort id;

	public float slope;

	public float offset;

	public NCRecord(Table table, byte[] record)
		: base(table, record)
	{
		id = (ushort)base.table.readField(this, 0);
		slope = BitConverter.ToSingle(BitConverter.GetBytes((int)base.table.readField(this, 1)), 0);
		offset = BitConverter.ToSingle(BitConverter.GetBytes((int)base.table.readField(this, 2)), 0);
	}
}
