namespace DRBDBReader.DB.Records;

public class ModuleDataElemRecord : Record
{
	private const byte FIELD_MODULE_FOR_ID = 0;

	private const byte FIELD_TXID = 1;

	public ushort moduleForId;

	public uint txid;

	public ModuleDataElemRecord(Table table, byte[] record)
		: base(table, record)
	{
		moduleForId = (ushort)base.table.readField(this, 0);
		txid = (uint)base.table.readField(this, 1);
		Table obj = base.table.db.tables[0];
		Table table2 = base.table.db.tables[23];
		ModuleRecord obj2 = (ModuleRecord)obj.getRecord(moduleForId, 0);
		TXRecord item = (TXRecord)table2.getRecord(txid, 0);
		obj2.dataelements.Add(item);
	}
}
