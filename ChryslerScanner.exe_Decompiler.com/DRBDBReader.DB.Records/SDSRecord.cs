namespace DRBDBReader.DB.Records;

public class SDSRecord : Record
{
	private const byte FIELD_ID = 0;

	private const byte FIELD_DEFAULT_STR_ID = 1;

	public ushort id;

	public ushort defaultStrId;

	public string defaultString;

	public SDSRecord(Table table, byte[] record)
		: base(table, record)
	{
		id = (ushort)base.table.readField(this, 0);
		defaultStrId = (ushort)base.table.readField(this, 1);
		defaultString = ((defaultStrId != 0) ? base.table.db.getString(defaultStrId) : "N/A");
	}
}
