using System;

namespace DRBDBReader.DB.Records;

public class NDSRecord : Record
{
	private const byte FIELD_ID = 0;

	private const byte FIELD_METRIC_CONV_SLOPE = 2;

	private const byte FIELD_UNIT_METRIC_STR_ID = 3;

	private const byte FIELD_METRIC_CONV_OFFSET = 4;

	private const byte FIELD_UNIT_IMPERIAL_STR_ID = 6;

	public ushort id;

	public ushort metricUnitStrId;

	public string metricUnitString;

	public ushort imperialUnitStrId;

	public string imperialUnitString;

	public float metricConvSlope;

	public float metricConvOffset;

	public NDSRecord(Table table, byte[] record)
		: base(table, record)
	{
		id = (ushort)base.table.readField(this, 0);
		metricUnitStrId = (ushort)base.table.readField(this, 3);
		metricUnitString = ((metricUnitStrId != 0) ? base.table.db.getString(metricUnitStrId) : "");
		imperialUnitStrId = (ushort)base.table.readField(this, 6);
		imperialUnitString = ((imperialUnitStrId != 0) ? base.table.db.getString(imperialUnitStrId) : "");
		metricConvSlope = BitConverter.ToSingle(BitConverter.GetBytes((int)base.table.readField(this, 2)), 0);
		metricConvOffset = BitConverter.ToSingle(BitConverter.GetBytes((int)base.table.readField(this, 4)), 0);
	}
}
