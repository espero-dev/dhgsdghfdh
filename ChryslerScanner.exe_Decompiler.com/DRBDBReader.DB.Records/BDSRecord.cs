namespace DRBDBReader.DB.Records;

public class BDSRecord : Record
{
	private const byte FIELD_ID = 0;

	private const byte FIELD_TRUE_STR_ID = 1;

	private const byte FIELD_FALSE_STR_ID = 2;

	public ushort id;

	public ushort trueStrId;

	public string trueString;

	public ushort falseStrId;

	public string falseString;

	public BDSRecord(Table table, byte[] record)
		: base(table, record)
	{
		id = (ushort)base.table.readField(this, 0);
		trueStrId = (ushort)base.table.readField(this, 1);
		trueString = base.table.db.getString(trueStrId);
		falseStrId = (ushort)base.table.readField(this, 2);
		falseString = base.table.db.getString(falseStrId);
	}
}
