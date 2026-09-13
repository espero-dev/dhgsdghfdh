namespace DRBDBReader.DB.Records;

public class Record
{
	public byte[] record;

	protected Table table;

	public Record(Table table, byte[] record)
	{
		this.table = table;
		this.record = record;
	}
}
