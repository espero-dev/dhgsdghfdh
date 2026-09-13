using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ChryslerScanner.Helpers;
using ChryslerScanner.Models;
using ChryslerScanner.Properties;

namespace ChryslerScanner.Services;

public class SerialService
{
	private readonly SerialPort SP;

	private readonly ConcurrentQueue<Packet> TxQueue;

	private Task RxPump;

	private Task TxPump;

	private CancellationTokenSource CTSource;

	public event EventHandler<Packet> PacketReceived;

	public SerialService()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Expected O, but got Unknown
		SP = new SerialPort();
		TxQueue = new ConcurrentQueue<Packet>();
	}

	public bool Connect(string port)
	{
		bool flag = false;
		if (SP.IsOpen)
		{
			SP.Close();
		}
		SP.PortName = port;
		SP.BaudRate = Settings.Default.UART0Baudrate;
		SP.DataBits = 8;
		SP.StopBits = (StopBits)1;
		SP.Parity = (Parity)0;
		SP.ReadTimeout = 500;
		SP.WriteTimeout = 500;
		try
		{
			SP.Open();
			SP.BaseStream.Flush();
			CTSource = new CancellationTokenSource();
			CancellationToken CT = CTSource.Token;
			RxPump = Task.Run(async delegate
			{
				await RxTask(CT);
			}, CT);
			TxPump = Task.Run(async delegate
			{
				await TxTask(CT);
			}, CT);
			return true;
		}
		catch
		{
			return false;
		}
	}

	public bool Disconnect()
	{
		CTSource.Cancel();
		if (SP.IsOpen)
		{
			SP.Close();
		}
		Task.WaitAll(new Task[2] { TxPump, RxPump }, 100);
		return true;
	}

	public void WritePacket(Packet packet)
	{
		if (packet != null)
		{
			TxQueue.Enqueue(packet);
		}
	}

	public void WritePacket(List<Packet> packets)
	{
		if (packets == null)
		{
			return;
		}
		foreach (Packet packet in packets)
		{
			if (packet != null)
			{
				TxQueue.Enqueue(packet);
			}
		}
	}

	private async Task RxTask(CancellationToken CT)
	{
		byte[] buffer = new byte[4096];
		int index = 0;
		try
		{
			while (SP.IsOpen && !CT.IsCancellationRequested)
			{
				if (SP.BytesToRead == 0)
				{
					await Task.Delay(1);
				}
				else
				{
					if (await SP.BaseStream.ReadAsync(buffer, index, 1) == 0 || buffer[0] != 61)
					{
						continue;
					}
					index++;
					if (index < 3)
					{
						continue;
					}
					int num = (buffer[1] << 8) + buffer[2];
					if (num > 4092)
					{
						await SP.BaseStream.FlushAsync();
						index = 0;
					}
					else if (index >= num + 4)
					{
						Packet packet = PacketHelper.Deserialize(buffer.Take(index).ToArray());
						index = 0;
						if (packet != null)
						{
							this.PacketReceived?.Invoke(this, packet);
						}
					}
				}
			}
		}
		catch (Exception)
		{
		}
	}

	private async Task TxTask(CancellationToken CT)
	{
		_ = 1;
		try
		{
			while (SP.IsOpen && !CT.IsCancellationRequested)
			{
				if (TxQueue.TryDequeue(out var result))
				{
					if (result == null)
					{
						continue;
					}
					byte[] array = PacketHelper.Serialize(result);
					if (array == null)
					{
						continue;
					}
					await SP.BaseStream.WriteAsync(array, 0, array.Length);
				}
				await Task.Delay(1);
			}
		}
		catch (Exception)
		{
		}
	}
}
