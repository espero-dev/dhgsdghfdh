namespace DRBDBReader.DB.Records;

public class ServiceCatRecord : Record
{
	private const byte FIELD_NAMEID = 1;

	private const byte FIELD_SCID = 3;

	public ushort nameid;

	public string name;

	public ushort scid;

	public ServiceCatRecord(Table table, byte[] record)
		: base(table, record)
	{
		scid = (ushort)base.table.readField(this, 3);
		nameid = (ushort)base.table.readField(this, 1);
		name = base.table.db.getString(nameid);
	}
}
