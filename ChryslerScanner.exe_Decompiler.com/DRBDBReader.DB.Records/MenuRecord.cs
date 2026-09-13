namespace DRBDBReader.DB.Records;

public class MenuRecord : Record
{
	private const byte FIELD_ID = 0;

	private const byte FIELD_PARENT_ID = 1;

	private const byte FIELD_NAME_ID = 2;

	private const byte FIELD_THREE = 3;

	private const byte FIELD_SCREEN_POS = 4;

	private const byte FIELD_FIVE = 5;

	private const byte FIELD_EMPTY = 6;

	public ushort id;

	public ushort parentid;

	public string name;

	public ushort nameid;

	public ushort fieldThree;

	public byte screenpos;

	public ushort fieldFive;

	public MenuRecord(Table table, byte[] record)
		: base(table, record)
	{
		id = (ushort)base.table.readField(this, 0);
		parentid = (ushort)base.table.readField(this, 1);
		nameid = (ushort)base.table.readField(this, 2);
		name = base.table.db.getString(nameid);
		fieldThree = (ushort)base.table.readField(this, 3);
		screenpos = (byte)(base.table.readField(this, 4) & 0xFF);
		fieldFive = (ushort)base.table.readField(this, 5);
	}
}
