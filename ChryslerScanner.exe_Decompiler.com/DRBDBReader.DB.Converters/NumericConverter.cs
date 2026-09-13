using System;
using DRBDBReader.DB.Records;

namespace DRBDBReader.DB.Converters;

public class NumericConverter : Converter
{
	public NCRecord ncRecord;

	public NDSRecord ndsRecord;

	public NumericConverter(Database db, byte[] record, ushort cfid, ushort dsid)
		: base(db, record, cfid, dsid)
	{
		Table table = base.db.tables[5];
		ncRecord = (NCRecord)table.getRecord(base.cfid, 0);
		Table table2 = base.db.tables[17];
		dsRecord = table2.getRecord(base.dsid, 0);
		ndsRecord = (NDSRecord)dsRecord;
	}

	public override string processData(long data, bool outputMetric = false)
	{
		decimal num = (decimal)data * (decimal)ncRecord.slope + (decimal)ncRecord.offset;
		string text = ndsRecord.imperialUnitString;
		if (outputMetric)
		{
			num = num * (decimal)ndsRecord.metricConvSlope + (decimal)ndsRecord.metricConvOffset;
			text = ndsRecord.metricUnitString;
		}
		return num + " " + text;
	}

	public override string ToString()
	{
		string text = base.ToString() + Environment.NewLine;
		if (ndsRecord.imperialUnitStrId == ndsRecord.metricUnitStrId)
		{
			text = text + Environment.NewLine + "UNIT: " + getUnitToStringOutput(ndsRecord.imperialUnitString);
		}
		else
		{
			text = text + Environment.NewLine + "UNIT (DFLT/MTRC): ";
			text += getUnitToStringOutput(ndsRecord.imperialUnitString);
			text = text + "/" + getUnitToStringOutput(ndsRecord.metricUnitString);
		}
		text = text + Environment.NewLine + "SLOPE:  " + ncRecord.slope;
		text = text + Environment.NewLine + "OFFSET: " + ncRecord.offset;
		text = text + Environment.NewLine + "SLCONV: " + ndsRecord.metricConvSlope;
		return text + Environment.NewLine + "OFCONV: " + ndsRecord.metricConvOffset;
	}

	private static string getUnitToStringOutput(string unit)
	{
		if (string.IsNullOrWhiteSpace(unit))
		{
			return "(null)";
		}
		return unit;
	}
}
