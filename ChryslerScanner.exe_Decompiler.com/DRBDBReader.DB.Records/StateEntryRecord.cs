namespace DRBDBReader.DB.Records;

public class StateEntryRecord : Record
{
	private const byte FIELD_STRING_ID = 0;

	private const byte FIELD_VALUE = 1;

	private const byte FIELD_DSID_THING = 3;

	public ushort nameStrId;

	public string nameString;

	public ushort value;

	public ushort dsidThing;

	public StateEntryRecord(Table table, byte[] record)
		: base(table, record)
	{
		nameStrId = (ushort)base.table.readField(this, 0);
		nameString = base.table.db.getString(nameStrId);
		value = (ushort)base.table.readField(this, 1);
		dsidThing = (ushort)base.table.readField(this, 3);
	}
}
