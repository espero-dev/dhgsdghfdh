using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Timers;
using System.Windows.Forms;
using ChryslerScanner.Helpers;
using ChryslerScanner.Models;
using ChryslerScanner.Services;

namespace ChryslerScanner;

public class ABSToolsForm : Form
{
	private enum Task
	{
		None,
		ReadID,
		ReadFaultCodes,
		EraseFaultCodes
	}

	private readonly MainForm OriginalForm;

	private readonly SerialService SerialService;

	private readonly SynchronizationContext UIContext;

	private bool CCDBusAlive;

	private bool CCDBusEcho;

	private bool CCDBusResponse;

	private bool CCDBusNextRequest;

	private byte CCDBusRxRetryCount;

	private byte CCDBusTxRetryCount;

	private bool ABSToolsFinished;

	private bool CCDBusRxTimeout;

	private bool CCDBusTxTimeout;

	private byte[] CCDBusTxPayload;

	private bool ABSEnterMK20DiagModeRequested;

	private bool ABSExitMK20DiagModeRequested;

	private bool ABSSoftwareIDRequested;

	private byte ABSSoftwareID;

	private bool ABSMK20Type;

	private byte ABSSoftwareVersion;

	private bool ABSHardwareIDRequested;

	private byte DrivenWheels;

	private string DrivenWheelsString = string.Empty;

	private byte PumpValveCount;

	private bool ABSSoftwareChecksumRequested;

	private byte ABSSoftwareChecksum;

	private bool ABSTattleTaleRequested;

	private string ABSTattleTaleStatus = string.Empty;

	private bool ABSPartNumberRequested;

	private byte[] ABSPartNumber;

	private byte ABSPartNumberStart;

	private byte ABSPartNumberLength;

	private byte ABSPartNumberPtr;

	private bool ABSFaultCodesRequested;

	private byte ABSFaultCodePage;

	private byte[] ABSFaultCodes = new byte[6];

	private bool ABSEraseFaultCodesRequested;

	private byte[] ABSEnterMK20DiagMode = new byte[6] { 178, 67, 104, 2, 0, 0 };

	private byte[] ABSExitMK20DiagMode = new byte[6] { 178, 67, 96, 0, 0, 0 };

	private Task CurrentTask;

	private string ABSToolsLogFilename;

	private System.Timers.Timer CCDBusAliveTimer = new System.Timers.Timer();

	private System.Timers.Timer CCDBusNextRequestTimer = new System.Timers.Timer();

	private System.Timers.Timer CCDBusRxTimeoutTimer = new System.Timers.Timer();

	private System.Timers.Timer CCDBusTxTimeoutTimer = new System.Timers.Timer();

	private BackgroundWorker ABSToolsWorker = new BackgroundWorker();

	private IContainer components;

	private GroupBox FaultCodeGroupBox;

	private Button EraseFaultCodesButton;

	private Button ReadFaultCodesButton;

	private GroupBox ABSModuleTypeGroupBox;

	private Button ABSModuleIDReadButton;

	private TextBox ABSToolsInfoTextBox;

	private Button ABSToolsHelpButton;

	public ABSToolsForm(MainForm IncomingForm, SerialService service)
	{
		OriginalForm = IncomingForm;
		InitializeComponent();
		UIContext = SynchronizationContext.Current;
		((Form)this).Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
		SerialService = service;
		SerialService.PacketReceived += PacketReceivedHandler;
		OriginalForm.ChangeLanguage();
		ABSToolsLogFilename = "LOG/ABS/abslog_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".txt";
		CCDBusAliveTimer.Elapsed += CCDBusAliveHandler;
		CCDBusAliveTimer.Interval = 1000.0;
		CCDBusAliveTimer.AutoReset = true;
		CCDBusAliveTimer.Enabled = true;
		CCDBusAliveTimer.Start();
		CCDBusNextRequestTimer.Elapsed += CCDBusNextRequestHandler;
		CCDBusNextRequestTimer.Interval = 25.0;
		CCDBusNextRequestTimer.AutoReset = false;
		CCDBusNextRequestTimer.Enabled = true;
		CCDBusNextRequestTimer.Start();
		CCDBusRxTimeoutTimer.Elapsed += CCDBusRxTimeoutHandler;
		CCDBusRxTimeoutTimer.Interval = 2000.0;
		CCDBusRxTimeoutTimer.AutoReset = false;
		CCDBusRxTimeoutTimer.Enabled = true;
		CCDBusRxTimeoutTimer.Stop();
		CCDBusTxTimeoutTimer.Elapsed += CCDBusTxTimeoutHandler;
		CCDBusTxTimeoutTimer.Interval = 2000.0;
		CCDBusTxTimeoutTimer.AutoReset = false;
		CCDBusTxTimeoutTimer.Enabled = true;
		CCDBusTxTimeoutTimer.Stop();
		ABSToolsWorker.WorkerReportsProgress = true;
		ABSToolsWorker.WorkerSupportsCancellation = true;
		ABSToolsWorker.DoWork += ABSTools_DoWork;
		ABSToolsWorker.ProgressChanged += ABSTools_ProgressChanged;
		ABSToolsWorker.RunWorkerCompleted += ABSTools_RunWorkerCompleted;
		((ContainerControl)this).ActiveControl = (Control)(object)ABSModuleIDReadButton;
	}

	private void ABSToolsForm_Load(object sender, EventArgs e)
	{
		UpdateTextBox(ABSToolsInfoTextBox, "Begin by identifying ABS module.");
	}

	private void CCDBusAliveHandler(object source, ElapsedEventArgs e)
	{
		CCDBusAlive = false;
	}

	private void CCDBusNextRequestHandler(object source, ElapsedEventArgs e)
	{
		CCDBusNextRequest = true;
	}

	private void CCDBusRxTimeoutHandler(object source, ElapsedEventArgs e)
	{
		CCDBusRxTimeout = true;
	}

	private void CCDBusTxTimeoutHandler(object source, ElapsedEventArgs e)
	{
		CCDBusTxTimeout = true;
	}

	private void ABSTools_DoWork(object sender, DoWorkEventArgs e)
	{
		while (!ABSToolsFinished)
		{
			Thread.Sleep(1);
			if (ABSToolsWorker.CancellationPending)
			{
				e.Cancel = true;
				break;
			}
			while (!CCDBusNextRequest)
			{
				Thread.Sleep(1);
				if (ABSToolsWorker.CancellationPending)
				{
					e.Cancel = true;
					break;
				}
			}
			CCDBusEcho = false;
			CCDBusResponse = false;
			CCDBusNextRequest = false;
			ABSToolsWorker.ReportProgress(0);
			while (!CCDBusEcho && !CCDBusTxTimeout)
			{
				Thread.Sleep(1);
				if (ABSToolsWorker.CancellationPending)
				{
					e.Cancel = true;
					break;
				}
			}
			if (CCDBusTxTimeout)
			{
				CCDBusTxRetryCount++;
				if (CCDBusTxRetryCount > 9)
				{
					CCDBusTxRetryCount = 0;
					e.Cancel = true;
					break;
				}
				CCDBusNextRequest = true;
				CCDBusTxTimeout = false;
				CCDBusTxTimeoutTimer.Stop();
				CCDBusTxTimeoutTimer.Start();
			}
			if (!CCDBusEcho)
			{
				continue;
			}
			CCDBusTxRetryCount = 0;
			while (!CCDBusResponse && !CCDBusRxTimeout)
			{
				Thread.Sleep(1);
				if (ABSToolsWorker.CancellationPending)
				{
					e.Cancel = true;
					break;
				}
			}
			if (CCDBusResponse)
			{
				CCDBusRxRetryCount = 0;
			}
			if (CCDBusRxTimeout)
			{
				CCDBusRxRetryCount++;
				if (CCDBusRxRetryCount > 9)
				{
					CCDBusRxRetryCount = 0;
					e.Cancel = true;
					break;
				}
				CCDBusNextRequest = true;
				CCDBusRxTimeout = false;
				CCDBusRxTimeoutTimer.Stop();
				CCDBusRxTimeoutTimer.Start();
			}
		}
	}

	private void ABSTools_ProgressChanged(object sender, ProgressChangedEventArgs e)
	{
		if (e.ProgressPercentage == 0)
		{
			Packet packet = new Packet();
			packet.Bus = 1;
			packet.Command = 6;
			packet.Mode = 2;
			packet.Payload = CCDBusTxPayload;
			OriginalForm.TransmitUSBPacket("[<-TX] Send a CCD-bus message once:", packet);
			SerialService.WritePacket(packet);
			CCDBusTxTimeout = false;
			CCDBusTxTimeoutTimer.Stop();
			CCDBusTxTimeoutTimer.Start();
		}
	}

	private void ABSTools_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
	{
		if (e.Cancelled)
		{
			if (CCDBusRxTimeout)
			{
				UpdateTextBox(ABSToolsInfoTextBox, Environment.NewLine + "Timeout (RX).");
			}
			if (CCDBusTxTimeout)
			{
				UpdateTextBox(ABSToolsInfoTextBox, Environment.NewLine + "Timeout (TX).");
			}
			UpdateTextBox(ABSToolsInfoTextBox, Environment.NewLine + Environment.NewLine + "Task cancelled.");
			if (!CCDBusAlive)
			{
				UpdateTextBox(ABSToolsInfoTextBox, Environment.NewLine + Environment.NewLine + "CCD-bus communication lost.");
			}
		}
		else
		{
			UpdateTextBox(ABSToolsInfoTextBox, Environment.NewLine + Environment.NewLine + "Task finished.");
		}
		CurrentTask = Task.None;
		CCDBusRxTimeout = false;
		CCDBusRxTimeoutTimer.Stop();
		CCDBusTxTimeout = false;
		CCDBusTxTimeoutTimer.Stop();
	}

	private void ABSModuleIDReadButton_Click(object sender, EventArgs e)
	{
		if (!ABSToolsWorker.IsBusy)
		{
			UpdateTextBox(ABSToolsInfoTextBox, Environment.NewLine + Environment.NewLine + "Read software and hardware ID.");
			CCDBusEcho = false;
			CCDBusResponse = false;
			ABSToolsFinished = false;
			CCDBusNextRequest = true;
			CCDBusRxRetryCount = 0;
			CCDBusTxRetryCount = 0;
			CCDBusTxPayload = ABSEnterMK20DiagMode;
			CurrentTask = Task.ReadID;
			ABSToolsWorker.RunWorkerAsync();
		}
	}

	private void ReadFaultCodesButton_Click(object sender, EventArgs e)
	{
		if (!ABSToolsWorker.IsBusy)
		{
			UpdateTextBox(ABSToolsInfoTextBox, Environment.NewLine + Environment.NewLine + "Read fault codes.");
			CCDBusEcho = false;
			CCDBusResponse = false;
			ABSToolsFinished = false;
			CCDBusNextRequest = true;
			CCDBusRxRetryCount = 0;
			CCDBusTxRetryCount = 0;
			ABSFaultCodePage = 0;
			CCDBusTxPayload = ABSEnterMK20DiagMode;
			CurrentTask = Task.ReadFaultCodes;
			ABSToolsWorker.RunWorkerAsync();
		}
	}

	private void EraseFaultCodesButton_Click(object sender, EventArgs e)
	{
		if (!ABSToolsWorker.IsBusy)
		{
			UpdateTextBox(ABSToolsInfoTextBox, Environment.NewLine + Environment.NewLine + "Erase fault codes.");
			CCDBusEcho = false;
			CCDBusResponse = false;
			ABSToolsFinished = false;
			CCDBusNextRequest = true;
			CCDBusRxRetryCount = 0;
			CCDBusTxRetryCount = 0;
			CCDBusTxPayload = ABSEnterMK20DiagMode;
			CurrentTask = Task.EraseFaultCodes;
			ABSToolsWorker.RunWorkerAsync();
		}
	}

	private void PacketReceivedHandler(object sender, Packet packet)
	{
		UIContext.Post(delegate
		{
			_ = packet.Payload.Length;
			_ = 4;
			if (packet.Bus == 0)
			{
				_ = packet.Command;
			}
			if (packet.Bus == 1)
			{
				CCDBusAliveTimer.Stop();
				CCDBusAliveTimer.Start();
				CCDBusAlive = true;
				byte[] array = packet.Payload.Skip(4).ToArray();
				switch (array[0])
				{
				case 178:
					if (array.Length >= 6)
					{
						CCDBusEcho = true;
						CCDBusTxTimeoutTimer.Stop();
						CCDBusRxTimeout = false;
						CCDBusRxTimeoutTimer.Start();
						if (array[1] == 67 && array[2] == 104 && array[3] == 2 && array[4] == 0)
						{
							UpdateTextBox(ABSToolsInfoTextBox, Environment.NewLine + Environment.NewLine + "Enter diagnostic mode.");
							switch (CurrentTask)
							{
							case Task.ReadID:
								CCDBusTxPayload = new byte[6] { 178, 67, 36, 0, 0, 25 };
								break;
							case Task.ReadFaultCodes:
							{
								ABSToolsForm aBSToolsForm4 = this;
								byte[] obj4 = new byte[6] { 178, 67, 22, 0, 0, 0 };
								obj4[3] = ABSFaultCodePage;
								aBSToolsForm4.CCDBusTxPayload = obj4;
								break;
							}
							case Task.EraseFaultCodes:
								CCDBusTxPayload = new byte[6] { 178, 67, 64, 0, 0, 0 };
								break;
							}
							ABSEnterMK20DiagModeRequested = true;
						}
						if (array[1] == 67 && array[2] == 96 && array[3] == 0 && array[4] == 0)
						{
							UpdateTextBox(ABSToolsInfoTextBox, Environment.NewLine + Environment.NewLine + "Exit diagnostic mode.");
							ABSExitMK20DiagModeRequested = true;
							CCDBusResponse = true;
							ABSToolsFinished = true;
						}
						if (array[1] == 67 && array[2] == 36 && array[3] == 0 && array[4] == 0)
						{
							ABSSoftwareIDRequested = true;
						}
						if (array[1] == 67 && array[2] == 32 && array[3] == (byte)(ABSPartNumberStart + ABSPartNumberPtr) && array[4] == 0)
						{
							ABSPartNumberRequested = true;
						}
						if (array[1] == 67 && array[2] == 36 && array[3] == 2 && array[4] == 0)
						{
							ABSSoftwareChecksumRequested = true;
						}
						if (array[1] == 67 && array[2] == 36 && array[3] == 1 && array[4] == 0)
						{
							ABSHardwareIDRequested = true;
						}
						if (array[1] == 67 && array[2] == 36 && array[3] == 3 && array[4] == 0)
						{
							ABSTattleTaleRequested = true;
						}
						if (array[1] == 67 && array[2] == 22 && array[3] == ABSFaultCodePage && array[4] == 0)
						{
							ABSFaultCodesRequested = true;
						}
						if (array[1] == 67 && array[2] == 64 && array[3] == 0 && array[4] == 0)
						{
							ABSEraseFaultCodesRequested = true;
						}
					}
					break;
				case 242:
					if (array.Length >= 6)
					{
						CCDBusRxTimeoutTimer.Stop();
						CCDBusNextRequest = false;
						CCDBusNextRequestTimer.Stop();
						CCDBusNextRequestTimer.Start();
						if (ABSEnterMK20DiagModeRequested && array[1] == 67 && array[2] == 104)
						{
							ABSEnterMK20DiagModeRequested = false;
							UpdateTextBox(ABSToolsInfoTextBox, " OK.");
						}
						if (ABSExitMK20DiagModeRequested && array[1] == 67 && array[2] == 96)
						{
							ABSExitMK20DiagModeRequested = false;
							UpdateTextBox(ABSToolsInfoTextBox, " OK.");
							ABSToolsFinished = true;
						}
						if (ABSSoftwareIDRequested && array[1] == 67 && array[2] == 36)
						{
							ABSSoftwareIDRequested = false;
							ABSSoftwareID = array[3];
							ABSSoftwareVersion = array[4];
							UpdateTextBox(ABSToolsInfoTextBox, Environment.NewLine + Environment.NewLine + "Type: ");
							switch (ABSSoftwareID)
							{
							case 128:
							case 133:
								UpdateTextBox(ABSToolsInfoTextBox, "Teves/ATE Jeep MK4-G ABS");
								break;
							case 25:
							case 132:
								UpdateTextBox(ABSToolsInfoTextBox, "Teves/ATE LH MK4-G ABS+LTCS");
								break;
							case 9:
							case 130:
								UpdateTextBox(ABSToolsInfoTextBox, "Teves/ATE LH MK4-G ABS");
								break;
							case 10:
							case 131:
								UpdateTextBox(ABSToolsInfoTextBox, "Teves/ATE NS MK4-G ABS");
								break;
							case 187:
								UpdateTextBox(ABSToolsInfoTextBox, "Teves/ATE Jeep MK20 ABS");
								break;
							case 190:
								UpdateTextBox(ABSToolsInfoTextBox, "Teves/ATE NS MK20 ABS+LTCS");
								break;
							case 188:
								UpdateTextBox(ABSToolsInfoTextBox, "Teves/ATE NS MK20 ABS");
								break;
							case 191:
								UpdateTextBox(ABSToolsInfoTextBox, "Teves/ATE JA/JX MK20 ABS+LTCS");
								break;
							case 189:
								UpdateTextBox(ABSToolsInfoTextBox, "Teves/ATE JA/JX MK20 ABS");
								break;
							case 177:
								UpdateTextBox(ABSToolsInfoTextBox, "Teves/ATE PL MK20 ABS");
								break;
							default:
								UpdateTextBox(ABSToolsInfoTextBox, "unknown");
								break;
							}
							UpdateTextBox(ABSToolsInfoTextBox, Environment.NewLine + "Version: " + Util.ByteToHexString(new byte[1] { ABSSoftwareVersion }).Insert(1, "."));
							switch (ABSSoftwareID)
							{
							case 9:
							case 10:
							case 25:
							case 128:
							case 130:
							case 131:
							case 132:
							case 133:
								ABSMK20Type = false;
								ABSPartNumber = new byte[8];
								ABSPartNumberStart = 1;
								ABSPartNumberLength = 8;
								ABSPartNumberPtr = 0;
								break;
							default:
								ABSMK20Type = true;
								ABSPartNumber = new byte[10];
								ABSPartNumberStart = 121;
								ABSPartNumberLength = 10;
								ABSPartNumberPtr = 0;
								break;
							}
							ABSToolsForm aBSToolsForm = this;
							byte[] obj = new byte[6] { 178, 67, 32, 0, 0, 0 };
							obj[3] = (byte)(ABSPartNumberStart + ABSPartNumberPtr);
							aBSToolsForm.CCDBusTxPayload = obj;
						}
						if (ABSPartNumberRequested && array[1] == 67 && array[2] == 32)
						{
							ABSPartNumberRequested = false;
							byte b = 0;
							b = ((!ABSMK20Type) ? array[4] : array[3]);
							ABSPartNumber[ABSPartNumberPtr] = b;
							ABSPartNumberPtr++;
							ABSToolsForm aBSToolsForm2 = this;
							byte[] obj2 = new byte[6] { 178, 67, 32, 0, 0, 0 };
							obj2[3] = (byte)(ABSPartNumberStart + ABSPartNumberPtr);
							aBSToolsForm2.CCDBusTxPayload = obj2;
							if (ABSPartNumberPtr >= ABSPartNumberLength)
							{
								UpdateTextBox(ABSToolsInfoTextBox, Environment.NewLine + "Part number: " + Encoding.ASCII.GetString(ABSPartNumber));
								CCDBusTxPayload = new byte[6] { 178, 67, 36, 2, 0, 27 };
							}
						}
						if (ABSSoftwareChecksumRequested && array[1] == 67 && array[2] == 36)
						{
							ABSSoftwareChecksumRequested = false;
							ABSSoftwareChecksum = array[3];
							UpdateTextBox(ABSToolsInfoTextBox, Environment.NewLine + "Checksum: " + Util.ByteToHexString(new byte[1] { ABSSoftwareChecksum }));
							CCDBusTxPayload = new byte[6] { 178, 67, 36, 1, 0, 26 };
						}
						if (ABSHardwareIDRequested && array[1] == 67 && array[2] == 36)
						{
							ABSHardwareIDRequested = false;
							DrivenWheels = (byte)((array[3] & 0xC0) >> 6);
							PumpValveCount = (byte)(array[4] & 0xFu);
							switch (DrivenWheels)
							{
							case 1:
								DrivenWheelsString = "RWD";
								break;
							case 2:
								DrivenWheelsString = "FWD";
								break;
							case 3:
								DrivenWheelsString = "4WD";
								break;
							default:
								DrivenWheelsString = "?WD (unknown)";
								break;
							}
							UpdateTextBox(ABSToolsInfoTextBox, Environment.NewLine + Environment.NewLine + "Driven wheels: " + DrivenWheelsString);
							UpdateTextBox(ABSToolsInfoTextBox, Environment.NewLine + "Pump valve count: " + PumpValveCount.ToString("0"));
							CCDBusTxPayload = new byte[6] { 178, 67, 36, 3, 0, 28 };
						}
						if (ABSTattleTaleRequested && array[1] == 67 && array[2] == 36)
						{
							ABSTattleTaleRequested = false;
							if ((array[3] & 1) == 1)
							{
								ABSTattleTaleStatus = "were read.";
							}
							else
							{
								ABSTattleTaleStatus = "were not read.";
							}
							UpdateTextBox(ABSToolsInfoTextBox, Environment.NewLine + "Fault codes " + ABSTattleTaleStatus);
							CCDBusTxPayload = ABSExitMK20DiagMode;
						}
						if (ABSFaultCodesRequested && array[1] == 67 && array[2] == 22 && array[3] == ABSFaultCodePage)
						{
							ABSFaultCodesRequested = false;
							ABSFaultCodes[ABSFaultCodePage] = array[4];
							ABSFaultCodePage++;
							ABSToolsForm aBSToolsForm3 = this;
							byte[] obj3 = new byte[6] { 178, 67, 22, 0, 0, 0 };
							obj3[3] = ABSFaultCodePage;
							aBSToolsForm3.CCDBusTxPayload = obj3;
							if ((ABSSoftwareID == 128 || ABSSoftwareID == 133) && ABSFaultCodePage >= 2)
							{
								if (ABSFaultCodes[0] == 0 && ABSFaultCodes[1] == 0)
								{
									UpdateTextBox(ABSToolsInfoTextBox, Environment.NewLine + Environment.NewLine + "No fault code.");
								}
								else
								{
									UpdateTextBox(ABSToolsInfoTextBox, Environment.NewLine + Environment.NewLine + "Fault codes:" + Environment.NewLine);
									if (Util.IsBitSet(ABSFaultCodes[0], 7))
									{
										UpdateTextBox(ABSToolsInfoTextBox, Environment.NewLine + "RR WHEEL SPD / SIGNAL");
									}
									if (Util.IsBitSet(ABSFaultCodes[0], 6))
									{
										UpdateTextBox(ABSToolsInfoTextBox, Environment.NewLine + "RR SENSOR / SIGNAL");
									}
									if (Util.IsBitSet(ABSFaultCodes[0], 5))
									{
										UpdateTextBox(ABSToolsInfoTextBox, Environment.NewLine + "RL WHEEL SPD / SIGNAL");
									}
									if (Util.IsBitSet(ABSFaultCodes[0], 4))
									{
										UpdateTextBox(ABSToolsInfoTextBox, Environment.NewLine + "RL SENSOR / SIGNAL");
									}
									if (Util.IsBitSet(ABSFaultCodes[0], 3))
									{
										UpdateTextBox(ABSToolsInfoTextBox, Environment.NewLine + "FR WHEEL SPD / SIGNAL");
									}
									if (Util.IsBitSet(ABSFaultCodes[0], 2))
									{
										UpdateTextBox(ABSToolsInfoTextBox, Environment.NewLine + "FR SENSOR / SIGNAL");
									}
									if (Util.IsBitSet(ABSFaultCodes[0], 1))
									{
										UpdateTextBox(ABSToolsInfoTextBox, Environment.NewLine + "FL WHEEL SPD / SIGNAL");
									}
									if (Util.IsBitSet(ABSFaultCodes[0], 0))
									{
										UpdateTextBox(ABSToolsInfoTextBox, Environment.NewLine + "FL SENSOR / SIGNAL");
									}
									if (Util.IsBitSet(ABSFaultCodes[1], 7))
									{
										UpdateTextBox(ABSToolsInfoTextBox, Environment.NewLine + "OVERVOLTAGE");
									}
									if (Util.IsBitSet(ABSFaultCodes[1], 6))
									{
										UpdateTextBox(ABSToolsInfoTextBox, Environment.NewLine + "BUS COMMUNICATION");
									}
									if (Util.IsBitSet(ABSFaultCodes[1], 5))
									{
										UpdateTextBox(ABSToolsInfoTextBox, Environment.NewLine + "DISTURBANCE DETECT");
									}
									if (Util.IsBitSet(ABSFaultCodes[1], 4))
									{
										UpdateTextBox(ABSToolsInfoTextBox, Environment.NewLine + "UNDERVOLTAGE");
									}
									if (Util.IsBitSet(ABSFaultCodes[1], 3))
									{
										UpdateTextBox(ABSToolsInfoTextBox, Environment.NewLine + "G-SWITCH INPUT");
									}
									if (Util.IsBitSet(ABSFaultCodes[1], 2))
									{
										UpdateTextBox(ABSToolsInfoTextBox, Environment.NewLine + "ECU INTERNAL");
									}
									if (Util.IsBitSet(ABSFaultCodes[1], 1))
									{
										UpdateTextBox(ABSToolsInfoTextBox, Environment.NewLine + "MAIN RELAY");
									}
									if (Util.IsBitSet(ABSFaultCodes[1], 0))
									{
										UpdateTextBox(ABSToolsInfoTextBox, Environment.NewLine + "HYDRO PUMP CIRCUIT");
									}
								}
								CCDBusTxPayload = ABSExitMK20DiagMode;
							}
							else if (ABSMK20Type && ABSFaultCodePage >= 2)
							{
								if (ABSFaultCodes[0] == 0 && ABSFaultCodes[1] == 0)
								{
									UpdateTextBox(ABSToolsInfoTextBox, Environment.NewLine + Environment.NewLine + "No fault code.");
								}
								else
								{
									UpdateTextBox(ABSToolsInfoTextBox, Environment.NewLine + Environment.NewLine + "Fault codes:" + Environment.NewLine);
									if (Util.IsBitSet(ABSFaultCodes[0], 7))
									{
										UpdateTextBox(ABSToolsInfoTextBox, Environment.NewLine + "RR WHEEL SPD / SIGNAL");
									}
									if (Util.IsBitSet(ABSFaultCodes[0], 6))
									{
										UpdateTextBox(ABSToolsInfoTextBox, Environment.NewLine + "RR SENSOR / SIGNAL");
									}
									if (Util.IsBitSet(ABSFaultCodes[0], 5))
									{
										UpdateTextBox(ABSToolsInfoTextBox, Environment.NewLine + "RL WHEEL SPD / SIGNAL");
									}
									if (Util.IsBitSet(ABSFaultCodes[0], 4))
									{
										UpdateTextBox(ABSToolsInfoTextBox, Environment.NewLine + "RL SENSOR / SIGNAL");
									}
									if (Util.IsBitSet(ABSFaultCodes[0], 3))
									{
										UpdateTextBox(ABSToolsInfoTextBox, Environment.NewLine + "FR WHEEL SPD / SIGNAL");
									}
									if (Util.IsBitSet(ABSFaultCodes[0], 2))
									{
										UpdateTextBox(ABSToolsInfoTextBox, Environment.NewLine + "FR SENSOR / SIGNAL");
									}
									if (Util.IsBitSet(ABSFaultCodes[0], 1))
									{
										UpdateTextBox(ABSToolsInfoTextBox, Environment.NewLine + "FL WHEEL SPD / SIGNAL");
									}
									if (Util.IsBitSet(ABSFaultCodes[0], 0))
									{
										UpdateTextBox(ABSToolsInfoTextBox, Environment.NewLine + "FL SENSOR / SIGNAL");
									}
									if (Util.IsBitSet(ABSFaultCodes[1], 7))
									{
										UpdateTextBox(ABSToolsInfoTextBox, Environment.NewLine + "OVERVOLTAGE");
									}
									if (Util.IsBitSet(ABSFaultCodes[1], 6))
									{
										UpdateTextBox(ABSToolsInfoTextBox, Environment.NewLine + "BUS COMMUNICATION");
									}
									if (Util.IsBitSet(ABSFaultCodes[1], 5))
									{
										UpdateTextBox(ABSToolsInfoTextBox, Environment.NewLine + "DISTURBANCE DETECT");
									}
									if (Util.IsBitSet(ABSFaultCodes[1], 4))
									{
										UpdateTextBox(ABSToolsInfoTextBox, Environment.NewLine + "UNDERVOLTAGE");
									}
									if (Util.IsBitSet(ABSFaultCodes[1], 3))
									{
										UpdateTextBox(ABSToolsInfoTextBox, Environment.NewLine + "WARNING LAMP CIRCUIT");
									}
									if (Util.IsBitSet(ABSFaultCodes[1], 2))
									{
										UpdateTextBox(ABSToolsInfoTextBox, Environment.NewLine + "ECU INTERNAL");
									}
									if (Util.IsBitSet(ABSFaultCodes[1], 1))
									{
										UpdateTextBox(ABSToolsInfoTextBox, Environment.NewLine + "MAIN RELAY");
									}
									if (Util.IsBitSet(ABSFaultCodes[1], 0))
									{
										UpdateTextBox(ABSToolsInfoTextBox, Environment.NewLine + "HYDRO PUMP CIRCUIT");
									}
								}
								CCDBusTxPayload = ABSExitMK20DiagMode;
							}
							else if (ABSFaultCodePage >= 6)
							{
								if (ABSFaultCodes[0] == 0 && ABSFaultCodes[1] == 0 && ABSFaultCodes[2] == 0 && ABSFaultCodes[3] == 0 && ABSFaultCodes[4] == 0)
								{
									UpdateTextBox(ABSToolsInfoTextBox, Environment.NewLine + Environment.NewLine + "No fault code.");
								}
								else
								{
									UpdateTextBox(ABSToolsInfoTextBox, Environment.NewLine + Environment.NewLine + "Fault codes:" + Environment.NewLine);
									if (Util.IsBitSet(ABSFaultCodes[0], 7))
									{
										UpdateTextBox(ABSToolsInfoTextBox, Environment.NewLine + "RR OUTLET VALVE");
									}
									if (Util.IsBitSet(ABSFaultCodes[0], 6))
									{
										UpdateTextBox(ABSToolsInfoTextBox, Environment.NewLine + "RR INLET VALVE");
									}
									if (Util.IsBitSet(ABSFaultCodes[0], 5))
									{
										UpdateTextBox(ABSToolsInfoTextBox, Environment.NewLine + "RL OUTLET VALVE");
									}
									if (Util.IsBitSet(ABSFaultCodes[0], 4))
									{
										UpdateTextBox(ABSToolsInfoTextBox, Environment.NewLine + "RL INLET VALVE");
									}
									if (Util.IsBitSet(ABSFaultCodes[0], 3))
									{
										UpdateTextBox(ABSToolsInfoTextBox, Environment.NewLine + "FR OUTLET VALVE");
									}
									if (Util.IsBitSet(ABSFaultCodes[0], 2))
									{
										UpdateTextBox(ABSToolsInfoTextBox, Environment.NewLine + "FR INLET VALVE");
									}
									if (Util.IsBitSet(ABSFaultCodes[0], 1))
									{
										UpdateTextBox(ABSToolsInfoTextBox, Environment.NewLine + "FL OUTLET VALVE");
									}
									if (Util.IsBitSet(ABSFaultCodes[0], 0))
									{
										UpdateTextBox(ABSToolsInfoTextBox, Environment.NewLine + "FL INLET VALVE");
									}
									if (Util.IsBitSet(ABSFaultCodes[1], 7))
									{
										UpdateTextBox(ABSToolsInfoTextBox, Environment.NewLine + "TRAC CNTL VALVE 1");
									}
									if (Util.IsBitSet(ABSFaultCodes[1], 6))
									{
										UpdateTextBox(ABSToolsInfoTextBox, Environment.NewLine + "TRAC CNTL VALVE 2");
									}
									if (Util.IsBitSet(ABSFaultCodes[1], 5))
									{
										UpdateTextBox(ABSToolsInfoTextBox, Environment.NewLine + "MAIN RELAY");
									}
									if (Util.IsBitSet(ABSFaultCodes[1], 4))
									{
										UpdateTextBox(ABSToolsInfoTextBox, Environment.NewLine + "DIAGNOSTIC COMPARE");
									}
									if (Util.IsBitSet(ABSFaultCodes[1], 3))
									{
										UpdateTextBox(ABSToolsInfoTextBox, Environment.NewLine + "VALVE BLOCK FEED");
									}
									if (Util.IsBitSet(ABSFaultCodes[1], 2))
									{
										UpdateTextBox(ABSToolsInfoTextBox, Environment.NewLine + "PUMP MOTOR CIRCUIT");
									}
									if (Util.IsBitSet(ABSFaultCodes[1], 1))
									{
										UpdateTextBox(ABSToolsInfoTextBox, Environment.NewLine + "CONTROLLER CONFIG");
									}
									if (Util.IsBitSet(ABSFaultCodes[1], 0))
									{
										UpdateTextBox(ABSToolsInfoTextBox, Environment.NewLine + "PUMP NOT RUNNING");
									}
									if (Util.IsBitSet(ABSFaultCodes[2], 7))
									{
										UpdateTextBox(ABSToolsInfoTextBox, Environment.NewLine + "RR CONTINUITY > 25");
									}
									if (Util.IsBitSet(ABSFaultCodes[2], 6))
									{
										UpdateTextBox(ABSToolsInfoTextBox, Environment.NewLine + "RL CONTINUITY > 25");
									}
									if (Util.IsBitSet(ABSFaultCodes[2], 5))
									{
										UpdateTextBox(ABSToolsInfoTextBox, Environment.NewLine + "FR CONTINUITY > 25");
									}
									if (Util.IsBitSet(ABSFaultCodes[2], 4))
									{
										UpdateTextBox(ABSToolsInfoTextBox, Environment.NewLine + "FL CONTINUITY > 25");
									}
									if (Util.IsBitSet(ABSFaultCodes[2], 3))
									{
										UpdateTextBox(ABSToolsInfoTextBox, Environment.NewLine + "RR TRIGGER MONITOR");
									}
									if (Util.IsBitSet(ABSFaultCodes[2], 2))
									{
										UpdateTextBox(ABSToolsInfoTextBox, Environment.NewLine + "RL TRIGGER MONITOR");
									}
									if (Util.IsBitSet(ABSFaultCodes[2], 1))
									{
										UpdateTextBox(ABSToolsInfoTextBox, Environment.NewLine + "FR TRIGGER MONITOR");
									}
									if (Util.IsBitSet(ABSFaultCodes[2], 0))
									{
										UpdateTextBox(ABSToolsInfoTextBox, Environment.NewLine + "FL TRIGGER MONITOR");
									}
									if (Util.IsBitSet(ABSFaultCodes[3], 7))
									{
										UpdateTextBox(ABSToolsInfoTextBox, Environment.NewLine + "OVERVOLTAGE");
									}
									if (Util.IsBitSet(ABSFaultCodes[3], 6))
									{
										UpdateTextBox(ABSToolsInfoTextBox, Environment.NewLine + "UNDERVOLTAGE");
									}
									if (Util.IsBitSet(ABSFaultCodes[3], 5))
									{
										UpdateTextBox(ABSToolsInfoTextBox, Environment.NewLine + "WARNING LAMP CIRCUIT");
									}
									if (Util.IsBitSet(ABSFaultCodes[3], 4))
									{
										UpdateTextBox(ABSToolsInfoTextBox, Environment.NewLine + "G-SWITCH INPUT");
									}
									if (Util.IsBitSet(ABSFaultCodes[3], 3))
									{
										UpdateTextBox(ABSToolsInfoTextBox, Environment.NewLine + "RR SPEED COMPARISON");
									}
									if (Util.IsBitSet(ABSFaultCodes[3], 2))
									{
										UpdateTextBox(ABSToolsInfoTextBox, Environment.NewLine + "RL SPEED COMPARISON");
									}
									if (Util.IsBitSet(ABSFaultCodes[3], 1))
									{
										UpdateTextBox(ABSToolsInfoTextBox, Environment.NewLine + "FR SPEED COMPARISON");
									}
									if (Util.IsBitSet(ABSFaultCodes[3], 0))
									{
										UpdateTextBox(ABSToolsInfoTextBox, Environment.NewLine + "FL SPEED COMPARISON");
									}
									if (Util.IsBitSet(ABSFaultCodes[4], 7))
									{
										UpdateTextBox(ABSToolsInfoTextBox, Environment.NewLine + "RR MISSING SIGNAL");
									}
									if (Util.IsBitSet(ABSFaultCodes[4], 6))
									{
										UpdateTextBox(ABSToolsInfoTextBox, Environment.NewLine + "RL MISSING SIGNAL");
									}
									if (Util.IsBitSet(ABSFaultCodes[4], 5))
									{
										UpdateTextBox(ABSToolsInfoTextBox, Environment.NewLine + "FR MISSING SIGNAL");
									}
									if (Util.IsBitSet(ABSFaultCodes[4], 4))
									{
										UpdateTextBox(ABSToolsInfoTextBox, Environment.NewLine + "FL MISSING SIGNAL");
									}
									if (Util.IsBitSet(ABSFaultCodes[4], 3))
									{
										UpdateTextBox(ABSToolsInfoTextBox, Environment.NewLine + "RR CONTINUITY < 25");
									}
									if (Util.IsBitSet(ABSFaultCodes[4], 2))
									{
										UpdateTextBox(ABSToolsInfoTextBox, Environment.NewLine + "RL CONTINUITY < 25");
									}
									if (Util.IsBitSet(ABSFaultCodes[4], 1))
									{
										UpdateTextBox(ABSToolsInfoTextBox, Environment.NewLine + "FR CONTINUITY < 25");
									}
									if (Util.IsBitSet(ABSFaultCodes[4], 0))
									{
										UpdateTextBox(ABSToolsInfoTextBox, Environment.NewLine + "FL CONTINUITY < 25");
									}
									byte b2 = (byte)(32 - ABSFaultCodes[5]);
									UpdateTextBox(ABSToolsInfoTextBox, Environment.NewLine + Environment.NewLine + b2 + " since last fault.");
								}
								CCDBusTxPayload = ABSExitMK20DiagMode;
							}
						}
						if (ABSEraseFaultCodesRequested && array[1] == 67 && array[2] == 64)
						{
							ABSEraseFaultCodesRequested = false;
							if (array[3] == 0 && array[4] == 0)
							{
								UpdateTextBox(ABSToolsInfoTextBox, Environment.NewLine + Environment.NewLine + "Fault codes erased successfully.");
							}
							else
							{
								UpdateTextBox(ABSToolsInfoTextBox, Environment.NewLine + Environment.NewLine + "Fault codes were not erased, try again.");
							}
							CCDBusTxPayload = ABSExitMK20DiagMode;
						}
						if (array[1] == 67 && array[2] == byte.MaxValue)
						{
							ABSEnterMK20DiagModeRequested = false;
							ABSExitMK20DiagModeRequested = false;
							ABSSoftwareIDRequested = false;
							ABSPartNumberRequested = false;
							ABSSoftwareChecksumRequested = false;
							ABSHardwareIDRequested = false;
							ABSTattleTaleRequested = false;
							ABSFaultCodesRequested = false;
							ABSEraseFaultCodesRequested = false;
						}
						CCDBusResponse = true;
					}
					break;
				}
			}
		}, null);
	}

	private void UpdateTextBox(TextBox TB, string text)
	{
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Expected O, but got Unknown
		if (((Control)TB).IsDisposed || !((Control)TB).IsHandleCreated)
		{
			return;
		}
		((Control)this).Invoke((Delegate)(MethodInvoker)delegate
		{
			if (((TextBoxBase)TB).TextLength + text.Length > ((TextBoxBase)TB).MaxLength)
			{
				((TextBoxBase)TB).Clear();
				GC.Collect();
			}
			((TextBoxBase)TB).AppendText(text);
			File.AppendAllText(ABSToolsLogFilename, text);
		});
	}

	private void ABSToolsForm_FormClosing(object sender, FormClosingEventArgs e)
	{
		if (ABSToolsWorker.IsBusy)
		{
			ABSToolsWorker.CancelAsync();
		}
		SerialService.PacketReceived -= PacketReceivedHandler;
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing && components != null)
		{
			components.Dispose();
		}
		((Form)this).Dispose(disposing);
	}

	private void InitializeComponent()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Expected O, but got Unknown
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Expected O, but got Unknown
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Expected O, but got Unknown
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Expected O, but got Unknown
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Expected O, but got Unknown
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Expected O, but got Unknown
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Expected O, but got Unknown
		//IL_02b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bd: Expected O, but got Unknown
		ComponentResourceManager componentResourceManager = new ComponentResourceManager(typeof(ABSToolsForm));
		FaultCodeGroupBox = new GroupBox();
		EraseFaultCodesButton = new Button();
		ReadFaultCodesButton = new Button();
		ABSModuleTypeGroupBox = new GroupBox();
		ABSModuleIDReadButton = new Button();
		ABSToolsInfoTextBox = new TextBox();
		ABSToolsHelpButton = new Button();
		((Control)FaultCodeGroupBox).SuspendLayout();
		((Control)ABSModuleTypeGroupBox).SuspendLayout();
		((Control)this).SuspendLayout();
		((Control)FaultCodeGroupBox).Controls.Add((Control)(object)EraseFaultCodesButton);
		((Control)FaultCodeGroupBox).Controls.Add((Control)(object)ReadFaultCodesButton);
		componentResourceManager.ApplyResources(FaultCodeGroupBox, "FaultCodeGroupBox");
		((Control)FaultCodeGroupBox).Name = "FaultCodeGroupBox";
		FaultCodeGroupBox.TabStop = false;
		componentResourceManager.ApplyResources(EraseFaultCodesButton, "EraseFaultCodesButton");
		((Control)EraseFaultCodesButton).Name = "EraseFaultCodesButton";
		((ButtonBase)EraseFaultCodesButton).UseVisualStyleBackColor = true;
		((Control)EraseFaultCodesButton).Click += EraseFaultCodesButton_Click;
		componentResourceManager.ApplyResources(ReadFaultCodesButton, "ReadFaultCodesButton");
		((Control)ReadFaultCodesButton).Name = "ReadFaultCodesButton";
		((ButtonBase)ReadFaultCodesButton).UseVisualStyleBackColor = true;
		((Control)ReadFaultCodesButton).Click += ReadFaultCodesButton_Click;
		((Control)ABSModuleTypeGroupBox).Controls.Add((Control)(object)ABSModuleIDReadButton);
		componentResourceManager.ApplyResources(ABSModuleTypeGroupBox, "ABSModuleTypeGroupBox");
		((Control)ABSModuleTypeGroupBox).Name = "ABSModuleTypeGroupBox";
		ABSModuleTypeGroupBox.TabStop = false;
		componentResourceManager.ApplyResources(ABSModuleIDReadButton, "ABSModuleIDReadButton");
		((Control)ABSModuleIDReadButton).Name = "ABSModuleIDReadButton";
		((ButtonBase)ABSModuleIDReadButton).UseVisualStyleBackColor = true;
		((Control)ABSModuleIDReadButton).Click += ABSModuleIDReadButton_Click;
		((Control)ABSToolsInfoTextBox).BackColor = SystemColors.Window;
		componentResourceManager.ApplyResources(ABSToolsInfoTextBox, "ABSToolsInfoTextBox");
		((Control)ABSToolsInfoTextBox).Name = "ABSToolsInfoTextBox";
		((TextBoxBase)ABSToolsInfoTextBox).ReadOnly = true;
		componentResourceManager.ApplyResources(ABSToolsHelpButton, "ABSToolsHelpButton");
		((Control)ABSToolsHelpButton).Name = "ABSToolsHelpButton";
		((ButtonBase)ABSToolsHelpButton).UseVisualStyleBackColor = true;
		componentResourceManager.ApplyResources(this, "$this");
		((ContainerControl)this).AutoScaleMode = (AutoScaleMode)2;
		((Control)this).Controls.Add((Control)(object)ABSToolsHelpButton);
		((Control)this).Controls.Add((Control)(object)ABSToolsInfoTextBox);
		((Control)this).Controls.Add((Control)(object)ABSModuleTypeGroupBox);
		((Control)this).Controls.Add((Control)(object)FaultCodeGroupBox);
		((Control)this).Name = "ABSToolsForm";
		((Form)this).FormClosing += new FormClosingEventHandler(ABSToolsForm_FormClosing);
		((Form)this).Load += ABSToolsForm_Load;
		((Control)FaultCodeGroupBox).ResumeLayout(false);
		((Control)ABSModuleTypeGroupBox).ResumeLayout(false);
		((Control)this).ResumeLayout(false);
		((Control)this).PerformLayout();
	}
}
