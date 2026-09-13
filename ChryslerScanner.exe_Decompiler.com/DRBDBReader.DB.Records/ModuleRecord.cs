using System.Collections.Generic;

namespace DRBDBReader.DB.Records;

public class ModuleRecord : Record
{
	private const byte FIELD_ID = 0;

	private const byte FIELD_SCID = 1;

	private const byte FIELD_NAMEID = 3;

	public ushort id;

	public ushort scid;

	public string scname;

	public ushort nameid;

	public string name;

	public List<TXRecord> dataelements;

	public ModuleRecord(Table table, byte[] record)
		: base(table, record)
	{
		id = (ushort)base.table.readField(this, 0);
		scid = (ushort)base.table.readField(this, 1);
		scname = base.table.db.getServiceCatString(scid);
		nameid = (ushort)base.table.readField(this, 3);
		name = base.table.db.getString(nameid);
		dataelements = new List<TXRecord>();
	}
}
