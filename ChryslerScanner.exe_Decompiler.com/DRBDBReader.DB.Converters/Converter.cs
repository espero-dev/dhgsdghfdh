using System;
using DRBDBReader.DB.Records;

namespace DRBDBReader.DB.Converters;

public class Converter
{
	public enum Types : byte
	{
		BINARY_STATE = 0,
		NUMERIC = 17,
		STATE = 32,
		UNKNOWN_x2 = 2,
		UNKNOWN_x12 = 18,
		UNKNOWN_x22 = 34
	}

	public Database db;

	public byte[] record;

	public ushort cfid;

	public ushort dsid;

	public Record dsRecord;

	public Types type;

	public Converter(Database db, byte[] record, ushort cfid, ushort dsid)
	{
		this.db = db;
		this.record = record;
		this.cfid = cfid;
		this.dsid = dsid;
		type = (Types)this.record[0];
	}

	public virtual string processData(long data, bool outputMetric = false)
	{
		return "(null)";
	}

	public override string ToString()
	{
		string text = "TYPE:  " + type;
		text = text + Environment.NewLine + "REC:   " + BitConverter.ToString(record);
		if (dsRecord != null)
		{
			text = text + Environment.NewLine + "DSREC: " + BitConverter.ToString(dsRecord.record);
		}
		return text;
	}
}
