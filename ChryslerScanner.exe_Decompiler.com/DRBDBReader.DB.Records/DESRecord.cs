namespace DRBDBReader.DB.Records;

public class DESRecord : Record
{
	private const byte FIELD_ID = 0;

	private const byte FIELD_NAME_ID = 1;

	public ushort id;

	public ushort nameid;

	public string name;

	public DESRecord(Table table, byte[] record)
		: base(table, record)
	{
		id = (ushort)base.table.readField(this, 0);
		nameid = (ushort)base.table.readField(this, 1);
		name = base.table.db.getString(nameid);
	}
}
