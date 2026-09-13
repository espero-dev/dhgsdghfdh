using System;
using System.Collections.Generic;
using DRBDBReader.DB.Converters;

namespace DRBDBReader.DB.Records;

public class TXRecord : Record
{
	private const byte FIELD_ID = 0;

	private const byte FIELD_CONVERSION = 1;

	private const byte FIELD_DATA_AQU_DESC_ID = 2;

	private const byte FIELD_DATA_ELEM_SET_ID = 4;

	private const byte FIELD_TXBYTES = 6;

	private const byte FIELD_STRING_ID = 8;

	private const byte FIELD_SVCCAT_ID = 14;

	public uint id;

	public ushort dadid;

	public DADRecord dadRecord;

	public ushort dataelemsetid;

	public byte[] xmitbytes;

	public string xmitstring;

	public ushort nameid;

	public string name;

	public ushort scid;

	public string scname;

	public Converter converter;

	public TXRecord(Table table, byte[] record)
		: base(table, record)
	{
		id = (uint)base.table.readField(this, 0);
		byte[] array = base.table.readFieldRaw(this, 1);
		ushort dsid = (ushort)base.table.readInternal(array, 2, 2);
		ushort cfid = (ushort)base.table.readInternal(array, 4, 2);
		switch ((Converter.Types)array[0])
		{
		case Converter.Types.BINARY_STATE:
			converter = getBSC(array, cfid, dsid);
			break;
		case Converter.Types.NUMERIC:
			converter = getNC(array, cfid, dsid);
			break;
		case Converter.Types.STATE:
			converter = getSC(array, cfid, dsid);
			break;
		case Converter.Types.UNKNOWN_x2:
		case Converter.Types.UNKNOWN_x12:
		case Converter.Types.UNKNOWN_x22:
			converter = getUC(array, cfid, dsid);
			break;
		default:
			converter = new Converter(base.table.db, array, cfid, dsid);
			break;
		}
		dadid = (ushort)base.table.readField(this, 2);
		Table table2 = base.table.db.tables[8];
		dadRecord = (DADRecord)table2.getRecord(dadid, 0);
		dataelemsetid = (ushort)base.table.readField(this, 4);
		int columnOffset = base.table.getColumnOffset(6);
		xmitbytes = new byte[base.record[columnOffset]];
		Array.Copy(base.record, columnOffset + 1, xmitbytes, 0, xmitbytes.Length);
		xmitstring = BitConverter.ToString(xmitbytes);
		nameid = (ushort)base.table.readField(this, 8);
		name = base.table.db.getString(nameid);
		scid = (ushort)(base.table.readField(this, 14) >> 8);
		scname = base.table.db.getServiceCatString(scid);
	}

	private BinaryStateConverter getBSC(byte[] convertfield, ushort cfid, ushort dsid)
	{
		if (table.txBSCCache.ContainsKey(cfid))
		{
			if (table.txBSCCache[cfid].ContainsKey(dsid))
			{
				return table.txBSCCache[cfid][dsid];
			}
		}
		else
		{
			table.txBSCCache[cfid] = new Dictionary<ushort, BinaryStateConverter>();
		}
		table.txBSCCache[cfid][dsid] = new BinaryStateConverter(table.db, convertfield, cfid, dsid);
		return table.txBSCCache[cfid][dsid];
	}

	private StateConverter getSC(byte[] convertfield, ushort cfid, ushort dsid)
	{
		if (table.txSCCache.ContainsKey(cfid))
		{
			if (table.txSCCache[cfid].ContainsKey(dsid))
			{
				return table.txSCCache[cfid][dsid];
			}
		}
		else
		{
			table.txSCCache[cfid] = new Dictionary<ushort, StateConverter>();
		}
		table.txSCCache[cfid][dsid] = new StateConverter(table.db, convertfield, cfid, dsid);
		return table.txSCCache[cfid][dsid];
	}

	private NumericConverter getNC(byte[] convertfield, ushort cfid, ushort dsid)
	{
		if (table.txNCCache.ContainsKey(cfid))
		{
			if (table.txNCCache[cfid].ContainsKey(dsid))
			{
				return table.txNCCache[cfid][dsid];
			}
		}
		else
		{
			table.txNCCache[cfid] = new Dictionary<ushort, NumericConverter>();
		}
		table.txNCCache[cfid][dsid] = new NumericConverter(table.db, convertfield, cfid, dsid);
		return table.txNCCache[cfid][dsid];
	}

	private UnknownConverter getUC(byte[] convertfield, ushort cfid, ushort dsid)
	{
		if (table.txUCCache.ContainsKey(cfid))
		{
			if (table.txUCCache[cfid].ContainsKey(dsid))
			{
				return table.txUCCache[cfid][dsid];
			}
		}
		else
		{
			table.txUCCache[cfid] = new Dictionary<ushort, UnknownConverter>();
		}
		table.txUCCache[cfid][dsid] = new UnknownConverter(table.db, convertfield, cfid, dsid);
		return table.txUCCache[cfid][dsid];
	}
}
