namespace DRBDBReader.DB.Records;

public class RecordUnknownWithString : Record
{
	private const byte FIELD_ID = 0;

	public byte stringidcol;

	public ushort id;

	public string str;

	public ushort strid;

	public RecordUnknownWithString(Table table, byte[] record, byte stringidcol)
		: base(table, record)
	{
		this.stringidcol = stringidcol;
		id = (ushort)base.table.readField(this, 0);
		strid = (ushort)base.table.readField(this, this.stringidcol);
		str = base.table.db.getString(strid);
	}
}
