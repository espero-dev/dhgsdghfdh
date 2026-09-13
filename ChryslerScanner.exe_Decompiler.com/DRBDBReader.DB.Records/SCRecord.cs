using DRBDBReader.DB.Converters;

namespace DRBDBReader.DB.Records;

public class SCRecord : Record
{
	private const byte FIELD_ID = 0;

	private const byte FIELD_MASK = 1;

	private const byte FIELD_OP = 2;

	public ushort id;

	public ushort mask;

	public Operator op;

	public SCRecord(Table table, byte[] record)
		: base(table, record)
	{
		id = (ushort)base.table.readField(this, 0);
		mask = (ushort)base.table.readField(this, 1);
		op = (Operator)((ushort)base.table.readField(this, 2) >> 8);
	}
}
