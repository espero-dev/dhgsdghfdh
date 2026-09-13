using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using DRBDBReader.DB.Converters;
using DRBDBReader.DB.Records;

namespace DRBDBReader.DB;

public class Table
{
	public uint offset;

	private ushort id;

	public ushort rowCount;

	public ushort rowSize;

	public byte colCount;

	public byte[] colSizes;

	public Record[] records;

	public Database db;

	private ConcurrentDictionary<long, int> idToRecordCache;

	public Dictionary<ushort, Dictionary<ushort, BinaryStateConverter>> txBSCCache;

	public Dictionary<ushort, Dictionary<ushort, NumericConverter>> txNCCache;

	public Dictionary<ushort, Dictionary<ushort, StateConverter>> txSCCache;

	public Dictionary<ushort, Dictionary<ushort, UnknownConverter>> txUCCache;

	private byte[] scratch = new byte[8];

	public Table(Database db, ushort id, uint offset, ushort rowCount, ushort rowSize, byte colCount, byte[] colSizes)
	{
		this.db = db;
		this.id = id;
		this.offset = offset;
		this.rowCount = rowCount;
		this.rowSize = rowSize;
		this.colCount = colCount;
		this.colSizes = colSizes;
		records = new Record[rowCount];
		idToRecordCache = new ConcurrentDictionary<long, int>(4, this.rowCount / 2);
		if (this.id == 23)
		{
			txBSCCache = new Dictionary<ushort, Dictionary<ushort, BinaryStateConverter>>();
			txNCCache = new Dictionary<ushort, Dictionary<ushort, NumericConverter>>();
			txSCCache = new Dictionary<ushort, Dictionary<ushort, StateConverter>>();
			txUCCache = new Dictionary<ushort, Dictionary<ushort, UnknownConverter>>();
		}
	}

	public void readRecords()
	{
		int num = (int)offset;
		switch (id)
		{
		case 13:
		{
			for (ushort num3 = 0; num3 < rowCount; num3++)
			{
				records[num3] = new SDSRecord(this, db.dbReader.ReadBytes(ref num, rowSize));
			}
			break;
		}
		case 2:
		{
			for (ushort num11 = 0; num11 < rowCount; num11++)
			{
				records[num11] = new BDSRecord(this, db.dbReader.ReadBytes(ref num, rowSize));
			}
			break;
		}
		case 17:
		{
			for (ushort num15 = 0; num15 < rowCount; num15++)
			{
				records[num15] = new NDSRecord(this, db.dbReader.ReadBytes(ref num, rowSize));
			}
			break;
		}
		case 4:
		{
			for (ushort num7 = 0; num7 < rowCount; num7++)
			{
				records[num7] = new SCRecord(this, db.dbReader.ReadBytes(ref num, rowSize));
			}
			break;
		}
		case 5:
		{
			for (ushort num17 = 0; num17 < rowCount; num17++)
			{
				records[num17] = new NCRecord(this, db.dbReader.ReadBytes(ref num, rowSize));
			}
			break;
		}
		case 15:
		{
			for (ushort num13 = 0; num13 < rowCount; num13++)
			{
				records[num13] = new StateEntryRecord(this, db.dbReader.ReadBytes(ref num, rowSize));
			}
			break;
		}
		case 16:
		{
			for (ushort num9 = 0; num9 < rowCount; num9++)
			{
				records[num9] = new StringRecord(this, db.dbReader.ReadBytes(ref num, rowSize));
			}
			break;
		}
		case 8:
		{
			for (ushort num5 = 0; num5 < rowCount; num5++)
			{
				records[num5] = new DADRecord(this, db.dbReader.ReadBytes(ref num, rowSize));
			}
			break;
		}
		case 1:
		{
			for (ushort num18 = 0; num18 < rowCount; num18++)
			{
				records[num18] = new DESRecord(this, db.dbReader.ReadBytes(ref num, rowSize));
			}
			break;
		}
		case 6:
		{
			for (ushort num16 = 0; num16 < rowCount; num16++)
			{
				records[num16] = new ServiceCatRecord(this, db.dbReader.ReadBytes(ref num, rowSize));
			}
			break;
		}
		case 23:
		{
			for (ushort num14 = 0; num14 < rowCount; num14++)
			{
				records[num14] = new TXRecord(this, db.dbReader.ReadBytes(ref num, rowSize));
			}
			break;
		}
		case 10:
		{
			for (ushort num12 = 0; num12 < rowCount; num12++)
			{
				records[num12] = new ModuleDataElemRecord(this, db.dbReader.ReadBytes(ref num, rowSize));
			}
			break;
		}
		case 0:
		{
			for (ushort num10 = 0; num10 < rowCount; num10++)
			{
				records[num10] = new ModuleRecord(this, db.dbReader.ReadBytes(ref num, rowSize));
			}
			break;
		}
		case 9:
		{
			for (ushort num8 = 0; num8 < rowCount; num8++)
			{
				records[num8] = new MenuRecord(this, db.dbReader.ReadBytes(ref num, rowSize));
			}
			break;
		}
		case 3:
		{
			for (ushort num6 = 0; num6 < rowCount; num6++)
			{
				records[num6] = new RecordUnknownWithString(this, db.dbReader.ReadBytes(ref num, rowSize), 2);
			}
			break;
		}
		case 21:
		{
			for (ushort num4 = 0; num4 < rowCount; num4++)
			{
				records[num4] = new RecordUnknownWithString(this, db.dbReader.ReadBytes(ref num, rowSize), 3);
			}
			break;
		}
		default:
		{
			for (ushort num2 = 0; num2 < rowCount; num2++)
			{
				records[num2] = new Record(this, db.dbReader.ReadBytes(ref num, rowSize));
			}
			break;
		}
		}
	}

	public Record getRecord(long key, byte idcol = 0, bool sorted = true)
	{
		if (idcol == 0 && idToRecordCache.ContainsKey(key))
		{
			return records[idToRecordCache[key]];
		}
		long num = 0L;
		if (sorted)
		{
			int num2 = 0;
			int num3 = rowCount - 1;
			int num4 = (num2 + num3) / 2;
			do
			{
				num = readField(records[num4], idcol);
				if (num == key)
				{
					if (idcol == 0)
					{
						idToRecordCache[key] = num4;
					}
					return records[num4];
				}
				if (num < key)
				{
					num2 = num4 + 1;
				}
				else
				{
					num3 = num4 - 1;
				}
				num4 = (num2 + num3) / 2;
			}
			while (num2 <= num3);
		}
		else
		{
			for (int i = 0; i < records.Length; i++)
			{
				num = readField(records[i], idcol);
				if (num == key)
				{
					if (idcol == 0)
					{
						idToRecordCache[key] = i;
					}
					return records[i];
				}
			}
		}
		return null;
	}

	public ushort getColumnOffset(byte col)
	{
		ushort num = 0;
		for (byte b = 0; b < col; b++)
		{
			num += colSizes[b];
		}
		return num;
	}

	public long readField(Record rec, byte col)
	{
		return readInternal(rec.record, getColumnOffset(col), colSizes[col]);
	}

	public byte[] readFieldRaw(Record rec, byte col)
	{
		byte[] array = new byte[colSizes[col]];
		Array.Copy(rec.record, getColumnOffset(col), array, 0, array.Length);
		return array;
	}

	public long readInternal(byte[] record, ushort colOffset, byte colSize)
	{
		for (int i = colSize; i < scratch.Length; i++)
		{
			scratch[i] = 0;
		}
		if (!db.isStarScanDB)
		{
			int num = colOffset + colSize - 1;
			for (int j = 0; j < colSize; j++)
			{
				scratch[j] = record[num - j];
			}
		}
		else
		{
			Array.Copy(record, colOffset, scratch, 0, colSize);
		}
		return BitConverter.ToInt64(scratch, 0);
	}

	public List<Record> selectRecords(byte field, long key, bool sorted = false)
	{
		List<Record> list = new List<Record>();
		for (ushort num = 0; num < records.Length; num++)
		{
			long num2 = readField(records[num], field);
			if (num2 == key)
			{
				list.Add(records[num]);
			}
			else if (sorted && num2 > key)
			{
				break;
			}
		}
		return list;
	}

	public List<ushort> selectRecordsReturnIDs(byte field, long key, bool sorted = false)
	{
		List<ushort> list = new List<ushort>();
		for (ushort num = 0; num < rowCount; num++)
		{
			long num2 = readField(records[num], field);
			if (num2 == key)
			{
				list.Add(num);
			}
			else if (sorted && num2 > key)
			{
				break;
			}
		}
		return list;
	}
}
