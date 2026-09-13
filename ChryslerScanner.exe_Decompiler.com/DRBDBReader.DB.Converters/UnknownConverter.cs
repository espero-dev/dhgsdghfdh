namespace DRBDBReader.DB.Converters;

public class UnknownConverter : Converter
{
	public UnknownConverter(Database db, byte[] record, ushort cfid, ushort dsid)
		: base(db, record, cfid, dsid)
	{
		if (base.record[0] == 2)
		{
			Table table = base.db.tables[2];
			dsRecord = table.getRecord(base.dsid, 0);
		}
		else if (base.record[0] == 18)
		{
			Table table = base.db.tables[17];
			dsRecord = table.getRecord(base.dsid, 0);
		}
		else if (base.record[0] == 34)
		{
			Table table = base.db.tables[13];
			dsRecord = table.getRecord(base.dsid, 0);
		}
	}

	public override string processData(long data, bool outputMetric = false)
	{
		return ToString();
	}
}
