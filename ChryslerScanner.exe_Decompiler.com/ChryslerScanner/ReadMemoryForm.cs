using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Timers;
using System.Windows.Forms;
using ChryslerScanner.Helpers;
using ChryslerScanner.Models;
using ChryslerScanner.Services;

namespace ChryslerScanner;

public class ReadMemoryForm : Form
{
	private readonly MainForm OriginalForm;

	private readonly SerialService SerialService;

	private readonly SynchronizationContext UIContext;

	private bool CCDBusAlive;

	private bool CCDBusEcho;

	private bool CCDBusResponse;

	private bool CCDBusNextRequest;

	private byte CCDBusRxRetryCount;

	private byte CCDBusTxRetryCount;

	private bool CCDBusReadMemoryFinished;

	private bool CCDBusRxTimeout;

	private bool CCDBusTxTimeout;

	private byte CCDBusModule;

	private byte CCDBusReadMemoryCommand;

	private byte[] CCDBusMemoryOffsetStartBytes = new byte[2];

	private uint CCDBusMemoryOffsetStart;

	private byte[] CCDBusMemoryOffsetEndBytes = new byte[2];

	private uint CCDBusMemoryOffsetEnd;

	private byte[] CCDBusIncrementBytes = new byte[2];

	private uint CCDBusIncrement;

	private byte[] CCDBusCurrentMemoryOffsetBytes = new byte[2];

	private uint CCDBusCurrentMemoryOffset;

	private byte[] CCDBusMemoryValueBytes = new byte[2];

	private uint CCDBusBytesReadCount;

	private uint CCDBusTotalBytes;

	private byte[] CCDBusTxPayload = new byte[6] { 178, 32, 34, 0, 0, 244 };

	private string CCDBusMemoryBinaryFilename;

	private string CCDBusMemoryTextFilename;

	private System.Timers.Timer CCDBusAliveTimer = new System.Timers.Timer();

	private System.Timers.Timer CCDBusNextRequestTimer = new System.Timers.Timer();

	private System.Timers.Timer CCDBusRxTimeoutTimer = new System.Timers.Timer();

	private System.Timers.Timer CCDBusTxTimeoutTimer = new System.Timers.Timer();

	private BackgroundWorker CCDBusReadMemoryWorker = new BackgroundWorker();

	private bool SCIBusPCMResponse;

	private bool SCIBusPCMNextRequest;

	private byte SCIBusPCMRxRetryCount;

	private byte SCIBusPCMTxRetryCount;

	private bool SCIBusPCMReadMemoryFinished;

	private bool SCIBusPCMRxTimeout;

	private bool SCIBusPCMTxTimeout;

	private byte SCIBusPCMReadMemoryCommand;

	private byte SCIBusPCMMemoryOffsetWidth = 24;

	private byte[] SCIBusPCMMemoryOffsetStartBytes = new byte[3];

	private uint SCIBusPCMMemoryOffsetStart;

	private byte[] SCIBusPCMMemoryOffsetEndBytes = new byte[3];

	private uint SCIBusPCMMemoryOffsetEnd;

	private byte[] SCIBusPCMIncrementBytes = new byte[3];

	private uint SCIBusPCMIncrement;

	private byte[] SCIBusPCMCurrentMemoryOffsetBytes = new byte[3];

	private uint SCIBusPCMCurrentMemoryOffset;

	private byte SCIBusPCMMemoryValue;

	private byte[] SCIBusPCMMemoryArray;

	private uint SCIBusPCMBytesReadCount;

	private uint SCIBusPCMTotalBytes;

	private byte[] SCIBusPCMTxPayload = new byte[4] { 38, 0, 0, 0 };

	private string SCIBusPCMMemoryBinaryFilename;

	private string SCIBusPCMMemoryTextFilename;

	private System.Timers.Timer SCIBusPCMNextRequestTimer = new System.Timers.Timer();

	private System.Timers.Timer SCIBusPCMRxTimeoutTimer = new System.Timers.Timer();

	private System.Timers.Timer SCIBusPCMTxTimeoutTimer = new System.Timers.Timer();

	private BackgroundWorker SCIBusPCMReadMemoryWorker = new BackgroundWorker();

	private TimeSpan ElapsedMillis;

	private DateTime Timestamp;

	private string TimestampString;

	private IContainer components;

	private TabControl ReadMemoryTabControl;

	private TabPage CCDBusTabPage;

	private TabPage SCIBusPCMTabPage;

	private Button CCDBusReadMemoryInitializeSessionButton;

	private Label CCDBusReadMemoryModuleLabel;

	private TextBox CCDBusReadMemoryModuleTextBox;

	private Label CCDBusReadMemoryCommandLabel;

	private TextBox CCDBusReadMemoryCommandTextBox;

	private Label CCDBusReadMemoryIncrementLabel;

	private TextBox CCDBusReadMemoryIncrementTextBox;

	private Label CCDBusReadMemoryEndOffsetLabel;

	private TextBox CCDBusReadMemoryEndOffsetTextBox;

	private Label CCDBusReadMemoryStartOffsetLabel;

	private TextBox CCDBusReadMemoryStartOffsetTextBox;

	private Button CCDBusReadMemoryStopButton;

	private Button CCDBusReadMemoryStartButton;

	private Label CCDBusReadMemoryValueLabel;

	private TextBox CCDBusReadMemoryValuesTextBox;

	private Label CCDBusReadMemoryCurrentOffsetLabel;

	private TextBox CCDBusReadMemoryCurrentOffsetTextBox;

	private TextBox CCDBusReadMemoryInfoTextBox;

	private Label CCDBusReadMemoryProgressLabel;

	private Label SCIBusPCMReadMemoryProgressLabel;

	private TextBox SCIBusPCMReadMemoryInfoTextBox;

	private Label SCIBusPCMReadMemoryValueLabel;

	private TextBox SCIBusPCMReadMemoryValueTextBox;

	private Label SCIBusPCMReadMemoryCurrentOffsetLabel;

	private TextBox SCIBusPCMReadMemoryCurrentOffsetTextBox;

	private Button SCIBusPCMReadMemoryStopButton;

	private Button SCIBusPCMReadMemoryStartButton;

	private Label SCIBusPCMReadMemoryIncrementLabel;

	private TextBox SCIBusPCMReadMemoryIncrementTextBox;

	private Label SCIBusPCMReadMemoryEndOffsetLabel;

	private TextBox SCIBusPCMReadMemoryEndOffsetTextBox;

	private Label SCIBusPCMReadMemoryStartOffsetLabel;

	private TextBox SCIBusPCMReadMemoryStartOffsetTextBox;

	private Label SCIBusPCMReadMemoryCommandLabel;

	private TextBox SCIBusPCMReadMemoryCommandTextBox;

	private Label SCIBusPCMReadMemoryPresetLabel;

	private Button SCIBusPCMReadMemoryInitializeSessionButton;

	private Button CCDBusReadMemoryHelpButton;

	private Button SCIBusPCMReadMemoryHelpButton;

	private ComboBox SCIBusPCMReadMemoryPresetComboBox;

	public ReadMemoryForm(MainForm IncomingForm, SerialService service)
	{
		OriginalForm = IncomingForm;
		InitializeComponent();
		UIContext = SynchronizationContext.Current;
		((Form)this).Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
		SerialService = service;
		SerialService.PacketReceived += PacketReceivedHandler;
		OriginalForm.ChangeLanguage();
		CCDBusAliveTimer.Elapsed += CCDBusAliveHandler;
		CCDBusAliveTimer.Interval = 1000.0;
		CCDBusAliveTimer.AutoReset = true;
		CCDBusAliveTimer.Enabled = true;
		CCDBusAliveTimer.Start();
		CCDBusNextRequestTimer.Elapsed += CCDBusNextRequestHandler;
		CCDBusNextRequestTimer.Interval = 150.0;
		CCDBusNextRequestTimer.AutoReset = false;
		CCDBusRxTimeoutTimer.Elapsed += CCDBusRxTimeoutHandler;
		CCDBusRxTimeoutTimer.Interval = 2000.0;
		CCDBusRxTimeoutTimer.AutoReset = false;
		CCDBusTxTimeoutTimer.Elapsed += CCDBusTxTimeoutHandler;
		CCDBusTxTimeoutTimer.Interval = 2000.0;
		CCDBusTxTimeoutTimer.AutoReset = false;
		CCDBusReadMemoryWorker.WorkerReportsProgress = true;
		CCDBusReadMemoryWorker.WorkerSupportsCancellation = true;
		CCDBusReadMemoryWorker.DoWork += CCDBusReadMemory_DoWork;
		CCDBusReadMemoryWorker.ProgressChanged += CCDBusReadMemory_ProgressChanged;
		CCDBusReadMemoryWorker.RunWorkerCompleted += CCDBusReadMemory_RunWorkerCompleted;
		SCIBusPCMNextRequestTimer.Elapsed += SCIBusPCMNextRequestHandler;
		SCIBusPCMNextRequestTimer.Interval = 25.0;
		SCIBusPCMNextRequestTimer.AutoReset = false;
		SCIBusPCMRxTimeoutTimer.Elapsed += SCIBusPCMRxTimeoutHandler;
		SCIBusPCMRxTimeoutTimer.Interval = 2000.0;
		SCIBusPCMRxTimeoutTimer.AutoReset = false;
		SCIBusPCMTxTimeoutTimer.Elapsed += SCIBusPCMTxTimeoutHandler;
		SCIBusPCMTxTimeoutTimer.Interval = 2000.0;
		SCIBusPCMTxTimeoutTimer.AutoReset = false;
		SCIBusPCMReadMemoryWorker.WorkerReportsProgress = true;
		SCIBusPCMReadMemoryWorker.WorkerSupportsCancellation = true;
		SCIBusPCMReadMemoryWorker.DoWork += SCIBusPCMReadMemory_DoWork;
		SCIBusPCMReadMemoryWorker.ProgressChanged += SCIBusPCMReadMemory_ProgressChanged;
		SCIBusPCMReadMemoryWorker.RunWorkerCompleted += SCIBusPCMReadMemory_RunWorkerCompleted;
		((ListControl)SCIBusPCMReadMemoryPresetComboBox).SelectedIndex = 2;
		((ContainerControl)this).ActiveControl = (Control)(object)CCDBusReadMemoryInitializeSessionButton;
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

	private void CCDBusReadMemoryInitializeSessionButton_Click(object sender, EventArgs e)
	{
		byte[] array = Util.HexStringToByte(((Control)CCDBusReadMemoryModuleTextBox).Text);
		if (array.Length == 1)
		{
			if (!byte.TryParse(array[0].ToString(), out CCDBusModule))
			{
				CCDBusModule = 32;
			}
		}
		else
		{
			CCDBusModule = 32;
		}
		((Control)CCDBusReadMemoryModuleTextBox).Text = Util.ByteToHexStringSimple(new byte[1] { CCDBusModule });
		((TextBoxBase)CCDBusReadMemoryModuleTextBox).SelectionLength = 0;
		array = Util.HexStringToByte(((Control)CCDBusReadMemoryCommandTextBox).Text);
		if (array.Length == 1)
		{
			if (!byte.TryParse(array[0].ToString(), out CCDBusReadMemoryCommand))
			{
				CCDBusReadMemoryCommand = 34;
			}
		}
		else
		{
			CCDBusReadMemoryCommand = 34;
		}
		((Control)CCDBusReadMemoryCommandTextBox).Text = Util.ByteToHexStringSimple(new byte[1] { CCDBusReadMemoryCommand });
		((TextBoxBase)CCDBusReadMemoryCommandTextBox).SelectionLength = 0;
		CCDBusMemoryOffsetStartBytes = Util.HexStringToByte(((Control)CCDBusReadMemoryStartOffsetTextBox).Text);
		if (CCDBusMemoryOffsetStartBytes.Length == 2)
		{
			if (!uint.TryParse(((CCDBusMemoryOffsetStartBytes[0] << 8) | CCDBusMemoryOffsetStartBytes[1]).ToString(), out CCDBusMemoryOffsetStart))
			{
				CCDBusMemoryOffsetStart = 0u;
				CCDBusMemoryOffsetStartBytes = new byte[2];
				CCDBusMemoryOffsetStartBytes[0] = 0;
				CCDBusMemoryOffsetStartBytes[1] = 0;
			}
		}
		else
		{
			CCDBusMemoryOffsetStart = 0u;
			CCDBusMemoryOffsetStartBytes = new byte[2];
			CCDBusMemoryOffsetStartBytes[0] = 0;
			CCDBusMemoryOffsetStartBytes[1] = 0;
		}
		((Control)CCDBusReadMemoryStartOffsetTextBox).Text = Util.ByteToHexStringSimple(CCDBusMemoryOffsetStartBytes);
		((TextBoxBase)CCDBusReadMemoryStartOffsetTextBox).SelectionLength = 0;
		CCDBusMemoryOffsetEndBytes = Util.HexStringToByte(((Control)CCDBusReadMemoryEndOffsetTextBox).Text);
		if (CCDBusMemoryOffsetEndBytes.Length == 2)
		{
			if (!uint.TryParse(((CCDBusMemoryOffsetEndBytes[0] << 8) | CCDBusMemoryOffsetEndBytes[1]).ToString(), out CCDBusMemoryOffsetEnd))
			{
				CCDBusMemoryOffsetEnd = 65534u;
				CCDBusMemoryOffsetEndBytes = new byte[2];
				CCDBusMemoryOffsetEndBytes[0] = byte.MaxValue;
				CCDBusMemoryOffsetEndBytes[1] = 254;
			}
		}
		else
		{
			CCDBusMemoryOffsetEnd = 65534u;
			CCDBusMemoryOffsetEndBytes = new byte[2];
			CCDBusMemoryOffsetEndBytes[0] = byte.MaxValue;
			CCDBusMemoryOffsetEndBytes[1] = 254;
		}
		((Control)CCDBusReadMemoryEndOffsetTextBox).Text = Util.ByteToHexStringSimple(CCDBusMemoryOffsetEndBytes);
		((TextBoxBase)CCDBusReadMemoryEndOffsetTextBox).SelectionLength = 0;
		CCDBusIncrementBytes = Util.HexStringToByte(((Control)CCDBusReadMemoryIncrementTextBox).Text);
		if (CCDBusIncrementBytes.Length == 2)
		{
			if (!uint.TryParse(((CCDBusIncrementBytes[0] << 8) | CCDBusIncrementBytes[1]).ToString(), out CCDBusIncrement))
			{
				CCDBusIncrement = 2u;
				CCDBusIncrementBytes = new byte[2];
				CCDBusIncrementBytes[0] = 0;
				CCDBusIncrementBytes[1] = 2;
			}
		}
		else
		{
			CCDBusIncrement = 2u;
			CCDBusIncrementBytes = new byte[2];
			CCDBusIncrementBytes[0] = 0;
			CCDBusIncrementBytes[1] = 2;
		}
		((Control)CCDBusReadMemoryIncrementTextBox).Text = Util.ByteToHexStringSimple(CCDBusIncrementBytes);
		((TextBoxBase)CCDBusReadMemoryIncrementTextBox).SelectionLength = 0;
		CCDBusCurrentMemoryOffsetBytes = CCDBusMemoryOffsetStartBytes.ToArray();
		CCDBusCurrentMemoryOffset = (ushort)((CCDBusCurrentMemoryOffsetBytes[0] << 8) | CCDBusCurrentMemoryOffsetBytes[1]);
		((Control)CCDBusReadMemoryCurrentOffsetTextBox).Text = Util.ByteToHexStringSimple(CCDBusCurrentMemoryOffsetBytes);
		((TextBoxBase)CCDBusReadMemoryCurrentOffsetTextBox).SelectionLength = 0;
		if (CCDBusIncrement == 1)
		{
			((Control)CCDBusReadMemoryValuesTextBox).Text = "00";
		}
		else
		{
			((Control)CCDBusReadMemoryValuesTextBox).Text = "00 00";
		}
		string text;
		switch (CCDBusModule)
		{
		case 16:
			text = "VIC – Vehicle Info Center";
			break;
		case 24:
		case 27:
			text = "VTS – Vehicle Theft Security";
			break;
		case 25:
			text = "CMT – Compass Mini-Trip";
			break;
		case 30:
			text = "ACM – Airbag Control Module";
			break;
		case 32:
			text = "BCM – Body Control Module";
			break;
		case 34:
		case 96:
			text = "MIC – Mechanical Instrument Cluster";
			break;
		case 65:
		case 66:
			text = "TCM – Transmission Control Module";
			break;
		case 67:
			text = "ABS – Antilock Brake System";
			break;
		case 80:
			text = "HVAC - Heat Vent Air Conditioning";
			break;
		case 128:
			text = "DDM - Driver Door Module";
			break;
		case 129:
			text = "PDM - Passenger Door Module";
			break;
		case 130:
			text = "MSM - Memory Seat Module";
			break;
		case 150:
			text = "ASM - Audio System Module";
			break;
		case 192:
			text = "SKIM - Sentry Key Immobilizer Module";
			break;
		default:
			text = "unknown";
			break;
		}
		string text2 = DateTime.Now.ToString("yyyyMMdd_HHmmss");
		CCDBusMemoryBinaryFilename = "ROMs/CCD/ccd_eprom_" + text2 + ".bin";
		CCDBusMemoryTextFilename = "ROMs/CCD/ccd_eprom_" + text2 + ".txt";
		if (CCDBusIncrement == 2)
		{
			CCDBusTotalBytes = CCDBusMemoryOffsetEnd - CCDBusMemoryOffsetStart + CCDBusIncrement;
		}
		else
		{
			CCDBusTotalBytes = (CCDBusMemoryOffsetEnd - CCDBusMemoryOffsetStart + CCDBusIncrement) / CCDBusIncrement;
		}
		CCDBusBytesReadCount = 0u;
		((Control)CCDBusReadMemoryProgressLabel).Text = "Progress: " + (byte)Math.Round((double)CCDBusBytesReadCount / (double)CCDBusTotalBytes * 100.0) + "% (" + CCDBusBytesReadCount + "/" + CCDBusTotalBytes + " bytes)";
		((TextBoxBase)CCDBusReadMemoryInfoTextBox).Clear();
		UpdateTextBox(CCDBusReadMemoryInfoTextBox, "Initialize memory reading session.");
		UpdateTextBox(CCDBusReadMemoryInfoTextBox, Environment.NewLine + "Module: " + Util.ByteToHexStringSimple(new byte[1] { CCDBusModule }) + " (" + text + ")");
		UpdateTextBox(CCDBusReadMemoryInfoTextBox, Environment.NewLine + "Command: " + Util.ByteToHexStringSimple(new byte[1] { CCDBusReadMemoryCommand }));
		UpdateTextBox(CCDBusReadMemoryInfoTextBox, Environment.NewLine + "Start offset: " + Util.ByteToHexStringSimple(CCDBusMemoryOffsetStartBytes));
		UpdateTextBox(CCDBusReadMemoryInfoTextBox, Environment.NewLine + "End offset: " + Util.ByteToHexStringSimple(CCDBusMemoryOffsetEndBytes));
		UpdateTextBox(CCDBusReadMemoryInfoTextBox, Environment.NewLine + "Increment: " + Util.ByteToHexStringSimple(CCDBusIncrementBytes));
		UpdateTextBox(CCDBusReadMemoryInfoTextBox, Environment.NewLine + "Output: " + CCDBusMemoryBinaryFilename);
		UpdateTextBox(CCDBusReadMemoryInfoTextBox, Environment.NewLine + "Binary size: " + CCDBusTotalBytes + " bytes = " + ((double)CCDBusTotalBytes / 1024.0).ToString("0.00") + " kilobytes.");
		UpdateTextBox(CCDBusReadMemoryInfoTextBox, Environment.NewLine + "Check CCD-bus status: ");
		if (CCDBusAlive)
		{
			UpdateTextBox(CCDBusReadMemoryInfoTextBox, "alive");
			UpdateTextBox(CCDBusReadMemoryInfoTextBox, Environment.NewLine + "Memory reading session is ready to start.");
		}
		else
		{
			UpdateTextBox(CCDBusReadMemoryInfoTextBox, "no activity");
		}
		if (!CCDBusReadMemoryWorker.IsBusy && CCDBusAlive)
		{
			((Control)CCDBusReadMemoryInitializeSessionButton).Enabled = true;
			((Control)CCDBusReadMemoryStartButton).Enabled = true;
			((Control)CCDBusReadMemoryStopButton).Enabled = false;
			CCDBusEcho = false;
			CCDBusResponse = false;
			CCDBusRxTimeout = false;
			CCDBusTxTimeout = false;
			CCDBusRxRetryCount = 0;
			CCDBusTxRetryCount = 0;
			CCDBusTxPayload = new byte[5]
			{
				178,
				CCDBusModule,
				CCDBusReadMemoryCommand,
				CCDBusCurrentMemoryOffsetBytes[0],
				CCDBusCurrentMemoryOffsetBytes[1]
			};
		}
		else
		{
			((Control)CCDBusReadMemoryInitializeSessionButton).Enabled = true;
			((Control)CCDBusReadMemoryStartButton).Enabled = false;
			((Control)CCDBusReadMemoryStopButton).Enabled = false;
			UpdateTextBox(CCDBusReadMemoryInfoTextBox, Environment.NewLine + "Failed to initialize session.");
		}
		((Control)CCDBusReadMemoryHelpButton).Enabled = true;
	}

	private void CCDBusReadMemoryStartButton_Click(object sender, EventArgs e)
	{
		if (CCDBusAlive)
		{
			((Control)CCDBusReadMemoryCurrentOffsetTextBox).Text = Util.ByteToHexStringSimple(CCDBusCurrentMemoryOffsetBytes);
			((TextBoxBase)CCDBusReadMemoryCurrentOffsetTextBox).SelectionLength = 0;
			CCDBusTxPayload = new byte[5]
			{
				178,
				CCDBusModule,
				CCDBusReadMemoryCommand,
				CCDBusCurrentMemoryOffsetBytes[0],
				CCDBusCurrentMemoryOffsetBytes[1]
			};
			((Control)CCDBusReadMemoryInitializeSessionButton).Enabled = false;
			((Control)CCDBusReadMemoryStartButton).Enabled = false;
			((Control)CCDBusReadMemoryStopButton).Enabled = true;
			CCDBusEcho = false;
			CCDBusResponse = false;
			CCDBusReadMemoryFinished = false;
			CCDBusNextRequest = true;
			CCDBusRxRetryCount = 0;
			CCDBusTxRetryCount = 0;
			UpdateTextBox(CCDBusReadMemoryInfoTextBox, Environment.NewLine + "Start memory reading session.");
			CCDBusReadMemoryWorker.RunWorkerAsync();
		}
		else
		{
			((Control)CCDBusReadMemoryStartButton).Enabled = false;
			((Control)CCDBusReadMemoryStopButton).Enabled = false;
			UpdateTextBox(CCDBusReadMemoryInfoTextBox, Environment.NewLine + "No CCD-bus activity detected.");
		}
	}

	private void CCDBusReadMemoryStopButton_Click(object sender, EventArgs e)
	{
		if (CCDBusReadMemoryWorker.IsBusy && CCDBusReadMemoryWorker.WorkerSupportsCancellation)
		{
			CCDBusReadMemoryWorker.CancelAsync();
		}
		CCDBusEcho = false;
		CCDBusResponse = false;
		CCDBusReadMemoryFinished = true;
	}

	private void CCDBusReadMemoryHelpButton_Click(object sender, EventArgs e)
	{
		if (((Control)CCDBusReadMemoryInfoTextBox).Text != "")
		{
			UpdateTextBox(CCDBusReadMemoryInfoTextBox, Environment.NewLine + Environment.NewLine);
		}
		UpdateTextBox(CCDBusReadMemoryInfoTextBox, "----------------------HELP---------------------");
		UpdateTextBox(CCDBusReadMemoryInfoTextBox, Environment.NewLine + "Module: hex address of the target module to be interrogated.");
		UpdateTextBox(CCDBusReadMemoryInfoTextBox, Environment.NewLine + "Known module addresses:");
		UpdateTextBox(CCDBusReadMemoryInfoTextBox, Environment.NewLine + "10:    VIC - Vehicle Info Center");
		UpdateTextBox(CCDBusReadMemoryInfoTextBox, Environment.NewLine + "18,1B: VTS - Vehicle Theft Security");
		UpdateTextBox(CCDBusReadMemoryInfoTextBox, Environment.NewLine + "19:    CMT - Compass Mini-Trip");
		UpdateTextBox(CCDBusReadMemoryInfoTextBox, Environment.NewLine + "1E:    ACM - Airbag Control Module");
		UpdateTextBox(CCDBusReadMemoryInfoTextBox, Environment.NewLine + "20:    BCM - Body Control Module");
		UpdateTextBox(CCDBusReadMemoryInfoTextBox, Environment.NewLine + "22,60: MIC - Mechanical Instrument Cluster");
		UpdateTextBox(CCDBusReadMemoryInfoTextBox, Environment.NewLine + "41,42: TCM - Transmission Control Module");
		UpdateTextBox(CCDBusReadMemoryInfoTextBox, Environment.NewLine + "43:    ABS - Antilock Brake System");
		UpdateTextBox(CCDBusReadMemoryInfoTextBox, Environment.NewLine + "50:   HVAC - Heat Vent Air Conditioning");
		UpdateTextBox(CCDBusReadMemoryInfoTextBox, Environment.NewLine + "80:    DDM - Driver Door Module");
		UpdateTextBox(CCDBusReadMemoryInfoTextBox, Environment.NewLine + "81:    PDM - Passenger Door Module");
		UpdateTextBox(CCDBusReadMemoryInfoTextBox, Environment.NewLine + "82:    MSM - Memory Seat Module");
		UpdateTextBox(CCDBusReadMemoryInfoTextBox, Environment.NewLine + "96:    ASM - Audio System Module");
		UpdateTextBox(CCDBusReadMemoryInfoTextBox, Environment.NewLine + "C0:   SKIM - Sentry Key Immobilizer Module");
		UpdateTextBox(CCDBusReadMemoryInfoTextBox, Environment.NewLine + Environment.NewLine + "Command: hex value of the \"read memory\" command recognized by the target module. 22 is known to be working with BCM. Not all modules may support memory reading.");
		UpdateTextBox(CCDBusReadMemoryInfoTextBox, Environment.NewLine + Environment.NewLine + "Start offset: this 16-bit value signifies the starting address of the memory space. Zero is a good place to start.");
		UpdateTextBox(CCDBusReadMemoryInfoTextBox, Environment.NewLine + Environment.NewLine + "End offset: this 16-bit value signifies the ending address of the memory space. With a usual 64 kB of program space FF FE is a good place to end the session.");
		UpdateTextBox(CCDBusReadMemoryInfoTextBox, Environment.NewLine + Environment.NewLine + "Increment: memory values are read using multiple request messages and this value gets added to the requested memory offset. Due to the format of the B2/F2 diagnostic messages 2 payload bytes are returned. During memory reading the first payload byte of the response message contains the requested memory value at the given offset, the second payload byte contains the next memory value. Therefore it's enough to request every other memory offset. When in doubt use an increment value of 1 and change the end offset value to FF FF.");
		UpdateTextBox(CCDBusReadMemoryInfoTextBox, Environment.NewLine + Environment.NewLine + "Current offset/Value(s): these show the current status of the session.");
		UpdateTextBox(CCDBusReadMemoryInfoTextBox, Environment.NewLine + Environment.NewLine + "Initialize session: prepares session, assigns output filename and checks if CCD-bus is alive.");
		UpdateTextBox(CCDBusReadMemoryInfoTextBox, Environment.NewLine + Environment.NewLine + "Start: begins to read the target module's memory using the given instructions. Multiple timeout measures are in place to ensure data integrity and to avoid hiccups. The reading session ends automatically once the ending offset is reached.");
		UpdateTextBox(CCDBusReadMemoryInfoTextBox, Environment.NewLine + Environment.NewLine + "Stop: cancels the current session with the possibility of resuming where the session was left hanging.");
		UpdateTextBox(CCDBusReadMemoryInfoTextBox, Environment.NewLine + Environment.NewLine + "Progress: shows current session status in percentage done and how much memory bytes have been read out of the total bytes count.");
		UpdateTextBox(CCDBusReadMemoryInfoTextBox, Environment.NewLine + Environment.NewLine + "Help: shows this text.");
		UpdateTextBox(CCDBusReadMemoryInfoTextBox, Environment.NewLine + Environment.NewLine + "Remarks: the binary output file and text log file are being updated during the session in real time. Modules with program space bigger than 64 kB may have multiple \"read memory\" commands. ROM/RAM/EEPROM areas are usually reachable between a single offset interval. Check the target module's microcontroller datasheet to discover where different memory areas are located.");
		UpdateTextBox(CCDBusReadMemoryInfoTextBox, Environment.NewLine + "-----------------------------------------------" + Environment.NewLine);
	}

	private void CCDBusReadMemory_DoWork(object sender, DoWorkEventArgs e)
	{
		//IL_0155: Unknown result type (might be due to invalid IL or missing references)
		//IL_015f: Expected O, but got Unknown
		while (!CCDBusReadMemoryFinished)
		{
			Thread.Sleep(1);
			if (CCDBusReadMemoryWorker.CancellationPending)
			{
				e.Cancel = true;
				break;
			}
			while (!CCDBusNextRequest)
			{
				Thread.Sleep(1);
				if (CCDBusReadMemoryWorker.CancellationPending)
				{
					e.Cancel = true;
					break;
				}
			}
			CCDBusEcho = false;
			CCDBusResponse = false;
			CCDBusNextRequest = false;
			CCDBusReadMemoryWorker.ReportProgress(0);
			while (!CCDBusEcho && !CCDBusTxTimeout)
			{
				Thread.Sleep(1);
				if (CCDBusReadMemoryWorker.CancellationPending)
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
				if (CCDBusReadMemoryWorker.CancellationPending)
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
				((Control)this).Invoke((Delegate)(MethodInvoker)delegate
				{
					UpdateTextBox(CCDBusReadMemoryInfoTextBox, " | no response");
				});
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

	private void CCDBusReadMemory_ProgressChanged(object sender, ProgressChangedEventArgs e)
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

	private void CCDBusReadMemory_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
	{
		if (e.Cancelled)
		{
			if (CCDBusRxTimeout)
			{
				UpdateTextBox(CCDBusReadMemoryInfoTextBox, Environment.NewLine + "Memory reading session timeout (RX).");
			}
			if (CCDBusTxTimeout)
			{
				UpdateTextBox(CCDBusReadMemoryInfoTextBox, Environment.NewLine + "Memory reading session timeout (TX).");
			}
			UpdateTextBox(CCDBusReadMemoryInfoTextBox, Environment.NewLine + "Memory reading session cancelled.");
			((Control)CCDBusReadMemoryInitializeSessionButton).Enabled = true;
			((Control)CCDBusReadMemoryStartButton).Enabled = true;
			((Control)CCDBusReadMemoryStopButton).Enabled = false;
		}
		else
		{
			UpdateTextBox(CCDBusReadMemoryInfoTextBox, Environment.NewLine + "Output: " + CCDBusMemoryBinaryFilename);
			UpdateTextBox(CCDBusReadMemoryInfoTextBox, Environment.NewLine + "Memory reading session finished.");
			((Control)CCDBusReadMemoryInitializeSessionButton).Enabled = true;
			((Control)CCDBusReadMemoryStartButton).Enabled = false;
			((Control)CCDBusReadMemoryStopButton).Enabled = false;
		}
		CCDBusRxTimeout = false;
		CCDBusRxTimeoutTimer.Stop();
		CCDBusTxTimeout = false;
		CCDBusTxTimeoutTimer.Stop();
	}

	private void SCIBusPCMNextRequestHandler(object source, ElapsedEventArgs e)
	{
		SCIBusPCMNextRequest = true;
	}

	private void SCIBusPCMRxTimeoutHandler(object source, ElapsedEventArgs e)
	{
		SCIBusPCMRxTimeout = true;
	}

	private void SCIBusPCMTxTimeoutHandler(object source, ElapsedEventArgs e)
	{
		SCIBusPCMTxTimeout = true;
	}

	private void SCIBusPCMReadMemoryInitializeSessionButton_Click(object sender, EventArgs e)
	{
		byte[] array = Util.HexStringToByte(((Control)SCIBusPCMReadMemoryCommandTextBox).Text);
		if (array.Length == 1)
		{
			if (!byte.TryParse(array[0].ToString(), out SCIBusPCMReadMemoryCommand))
			{
				SCIBusPCMReadMemoryCommand = 38;
			}
		}
		else
		{
			SCIBusPCMReadMemoryCommand = 38;
		}
		((Control)SCIBusPCMReadMemoryCommandTextBox).Text = Util.ByteToHexStringSimple(new byte[1] { SCIBusPCMReadMemoryCommand });
		((TextBoxBase)SCIBusPCMReadMemoryCommandTextBox).SelectionLength = 0;
		SCIBusPCMMemoryOffsetStartBytes = Util.HexStringToByte(((Control)SCIBusPCMReadMemoryStartOffsetTextBox).Text);
		if (SCIBusPCMMemoryOffsetStartBytes.Length == 2)
		{
			SCIBusPCMMemoryOffsetWidth = 16;
			if (!uint.TryParse(((SCIBusPCMMemoryOffsetStartBytes[0] << 8) | SCIBusPCMMemoryOffsetStartBytes[1]).ToString(), out SCIBusPCMMemoryOffsetStart))
			{
				SCIBusPCMMemoryOffsetStart = 0u;
				SCIBusPCMMemoryOffsetStartBytes = new byte[2];
				SCIBusPCMMemoryOffsetStartBytes[0] = 0;
				SCIBusPCMMemoryOffsetStartBytes[1] = 0;
			}
		}
		else if (SCIBusPCMMemoryOffsetStartBytes.Length == 3)
		{
			SCIBusPCMMemoryOffsetWidth = 24;
			if (!uint.TryParse(((SCIBusPCMMemoryOffsetStartBytes[0] << 16) | (SCIBusPCMMemoryOffsetStartBytes[1] << 8) | SCIBusPCMMemoryOffsetStartBytes[2]).ToString(), out SCIBusPCMMemoryOffsetStart))
			{
				SCIBusPCMMemoryOffsetStart = 0u;
				SCIBusPCMMemoryOffsetStartBytes = new byte[3];
				SCIBusPCMMemoryOffsetStartBytes[0] = 0;
				SCIBusPCMMemoryOffsetStartBytes[1] = 0;
				SCIBusPCMMemoryOffsetStartBytes[2] = 0;
			}
		}
		else
		{
			SCIBusPCMMemoryOffsetWidth = 24;
			SCIBusPCMMemoryOffsetStart = 0u;
			SCIBusPCMMemoryOffsetStartBytes = new byte[3];
			SCIBusPCMMemoryOffsetStartBytes[0] = 0;
			SCIBusPCMMemoryOffsetStartBytes[1] = 0;
			SCIBusPCMMemoryOffsetStartBytes[2] = 0;
		}
		((Control)SCIBusPCMReadMemoryStartOffsetTextBox).Text = Util.ByteToHexStringSimple(SCIBusPCMMemoryOffsetStartBytes);
		((TextBoxBase)SCIBusPCMReadMemoryStartOffsetTextBox).SelectionLength = 0;
		SCIBusPCMMemoryOffsetEndBytes = Util.HexStringToByte(((Control)SCIBusPCMReadMemoryEndOffsetTextBox).Text);
		if (SCIBusPCMMemoryOffsetWidth == 16)
		{
			if (SCIBusPCMMemoryOffsetEndBytes.Length == 2)
			{
				if (!uint.TryParse(((SCIBusPCMMemoryOffsetEndBytes[0] << 8) | SCIBusPCMMemoryOffsetEndBytes[1]).ToString(), out SCIBusPCMMemoryOffsetEnd))
				{
					SCIBusPCMMemoryOffsetEnd = 65535u;
					SCIBusPCMMemoryOffsetEndBytes = new byte[2];
					SCIBusPCMMemoryOffsetEndBytes[0] = byte.MaxValue;
					SCIBusPCMMemoryOffsetEndBytes[1] = byte.MaxValue;
				}
			}
			else
			{
				SCIBusPCMMemoryOffsetEnd = 65535u;
				SCIBusPCMMemoryOffsetEndBytes = new byte[2];
				SCIBusPCMMemoryOffsetEndBytes[0] = byte.MaxValue;
				SCIBusPCMMemoryOffsetEndBytes[1] = byte.MaxValue;
			}
		}
		if (SCIBusPCMMemoryOffsetWidth == 24)
		{
			if (SCIBusPCMMemoryOffsetEndBytes.Length == 3)
			{
				if (!uint.TryParse(((SCIBusPCMMemoryOffsetEndBytes[0] << 16) | (SCIBusPCMMemoryOffsetEndBytes[1] << 8) | SCIBusPCMMemoryOffsetEndBytes[2]).ToString(), out SCIBusPCMMemoryOffsetEnd))
				{
					SCIBusPCMMemoryOffsetEnd = 131071u;
					SCIBusPCMMemoryOffsetEndBytes = new byte[3];
					SCIBusPCMMemoryOffsetEndBytes[0] = 1;
					SCIBusPCMMemoryOffsetEndBytes[1] = byte.MaxValue;
					SCIBusPCMMemoryOffsetEndBytes[2] = byte.MaxValue;
				}
			}
			else
			{
				SCIBusPCMMemoryOffsetEnd = 131071u;
				SCIBusPCMMemoryOffsetEndBytes = new byte[3];
				SCIBusPCMMemoryOffsetEndBytes[0] = 1;
				SCIBusPCMMemoryOffsetEndBytes[1] = byte.MaxValue;
				SCIBusPCMMemoryOffsetEndBytes[2] = byte.MaxValue;
			}
		}
		((Control)SCIBusPCMReadMemoryEndOffsetTextBox).Text = Util.ByteToHexStringSimple(SCIBusPCMMemoryOffsetEndBytes);
		((TextBoxBase)SCIBusPCMReadMemoryEndOffsetTextBox).SelectionLength = 0;
		SCIBusPCMIncrementBytes = Util.HexStringToByte(((Control)SCIBusPCMReadMemoryIncrementTextBox).Text);
		if (SCIBusPCMMemoryOffsetWidth == 16)
		{
			if (SCIBusPCMIncrementBytes.Length == 2)
			{
				if (!uint.TryParse(((SCIBusPCMIncrementBytes[0] << 8) | SCIBusPCMIncrementBytes[1]).ToString(), out SCIBusPCMIncrement))
				{
					SCIBusPCMIncrement = 1u;
					SCIBusPCMIncrementBytes = new byte[2];
					SCIBusPCMIncrementBytes[0] = 0;
					SCIBusPCMIncrementBytes[1] = 1;
				}
			}
			else
			{
				SCIBusPCMIncrement = 1u;
				SCIBusPCMIncrementBytes = new byte[2];
				SCIBusPCMIncrementBytes[0] = 0;
				SCIBusPCMIncrementBytes[1] = 1;
			}
		}
		if (SCIBusPCMMemoryOffsetWidth == 24)
		{
			if (SCIBusPCMIncrementBytes.Length == 3)
			{
				if (!uint.TryParse(((SCIBusPCMIncrementBytes[0] << 16) | (SCIBusPCMIncrementBytes[1] << 8) | SCIBusPCMIncrementBytes[2]).ToString(), out SCIBusPCMIncrement))
				{
					SCIBusPCMIncrement = 1u;
					SCIBusPCMIncrementBytes = new byte[3];
					SCIBusPCMIncrementBytes[0] = 0;
					SCIBusPCMIncrementBytes[1] = 0;
					SCIBusPCMIncrementBytes[2] = 1;
				}
			}
			else
			{
				SCIBusPCMIncrement = 1u;
				SCIBusPCMIncrementBytes = new byte[3];
				SCIBusPCMIncrementBytes[0] = 0;
				SCIBusPCMIncrementBytes[1] = 0;
				SCIBusPCMIncrementBytes[2] = 1;
			}
		}
		((Control)SCIBusPCMReadMemoryIncrementTextBox).Text = Util.ByteToHexStringSimple(SCIBusPCMIncrementBytes);
		((TextBoxBase)SCIBusPCMReadMemoryIncrementTextBox).SelectionLength = 0;
		SCIBusPCMCurrentMemoryOffsetBytes = SCIBusPCMMemoryOffsetStartBytes.ToArray();
		if (SCIBusPCMMemoryOffsetWidth == 16)
		{
			SCIBusPCMCurrentMemoryOffset = (uint)((SCIBusPCMCurrentMemoryOffsetBytes[0] << 8) | SCIBusPCMCurrentMemoryOffsetBytes[1]);
		}
		if (SCIBusPCMMemoryOffsetWidth == 24)
		{
			SCIBusPCMCurrentMemoryOffset = (uint)((SCIBusPCMCurrentMemoryOffsetBytes[0] << 16) | (SCIBusPCMCurrentMemoryOffsetBytes[1] << 8) | SCIBusPCMCurrentMemoryOffsetBytes[2]);
		}
		((Control)SCIBusPCMReadMemoryCurrentOffsetTextBox).Text = Util.ByteToHexStringSimple(SCIBusPCMCurrentMemoryOffsetBytes);
		((TextBoxBase)SCIBusPCMReadMemoryCurrentOffsetTextBox).SelectionLength = 0;
		((Control)SCIBusPCMReadMemoryValueTextBox).Text = "00";
		SCIBusPCMTotalBytes = SCIBusPCMMemoryOffsetEnd - SCIBusPCMMemoryOffsetStart + 1;
		SCIBusPCMBytesReadCount = 0u;
		((Control)SCIBusPCMReadMemoryProgressLabel).Text = "Progress: " + (byte)Math.Round((double)SCIBusPCMBytesReadCount / (double)SCIBusPCMTotalBytes * 100.0) + "% (" + SCIBusPCMBytesReadCount + "/" + SCIBusPCMTotalBytes + " bytes)";
		((TextBoxBase)SCIBusPCMReadMemoryInfoTextBox).Clear();
		string text = DateTime.Now.ToString("yyyyMMdd_HHmmss");
		switch (((ListControl)SCIBusPCMReadMemoryPresetComboBox).SelectedIndex)
		{
		case 0:
			SCIBusPCMMemoryBinaryFilename = "ROMs/PCM/pcm_flash_" + text + ".bin";
			SCIBusPCMMemoryTextFilename = "ROMs/PCM/pcm_flash_" + text + ".txt";
			UpdateTextBox(SCIBusPCMReadMemoryInfoTextBox, "Initialize memory reading session.");
			UpdateTextBox(SCIBusPCMReadMemoryInfoTextBox, Environment.NewLine + "Preset: ");
			UpdateTextBox(SCIBusPCMReadMemoryInfoTextBox, "ROM/32kB");
			break;
		case 1:
			SCIBusPCMMemoryBinaryFilename = "ROMs/PCM/pcm_flash_" + text + ".bin";
			SCIBusPCMMemoryTextFilename = "ROMs/PCM/pcm_flash_" + text + ".txt";
			UpdateTextBox(SCIBusPCMReadMemoryInfoTextBox, "Initialize memory reading session.");
			UpdateTextBox(SCIBusPCMReadMemoryInfoTextBox, Environment.NewLine + "Preset: ");
			UpdateTextBox(SCIBusPCMReadMemoryInfoTextBox, "ROM/64kB");
			break;
		case 2:
			SCIBusPCMMemoryBinaryFilename = "ROMs/PCM/pcm_flash_" + text + ".bin";
			SCIBusPCMMemoryTextFilename = "ROMs/PCM/pcm_flash_" + text + ".txt";
			UpdateTextBox(SCIBusPCMReadMemoryInfoTextBox, "Initialize memory reading session.");
			UpdateTextBox(SCIBusPCMReadMemoryInfoTextBox, Environment.NewLine + "Preset: ");
			UpdateTextBox(SCIBusPCMReadMemoryInfoTextBox, "ROM/128kB");
			break;
		case 3:
			SCIBusPCMMemoryBinaryFilename = "ROMs/PCM/pcm_flash_" + text + ".bin";
			SCIBusPCMMemoryTextFilename = "ROMs/PCM/pcm_flash_" + text + ".txt";
			UpdateTextBox(SCIBusPCMReadMemoryInfoTextBox, "Initialize memory reading session.");
			UpdateTextBox(SCIBusPCMReadMemoryInfoTextBox, Environment.NewLine + "Preset: ");
			UpdateTextBox(SCIBusPCMReadMemoryInfoTextBox, "ROM/256kB");
			break;
		case 4:
			SCIBusPCMMemoryBinaryFilename = "ROMs/PCM/pcm_eeprom_" + text + ".bin";
			SCIBusPCMMemoryTextFilename = "ROMs/PCM/pcm_eeprom_" + text + ".txt";
			UpdateTextBox(SCIBusPCMReadMemoryInfoTextBox, "Initialize memory reading session.");
			UpdateTextBox(SCIBusPCMReadMemoryInfoTextBox, Environment.NewLine + "Preset: ");
			UpdateTextBox(SCIBusPCMReadMemoryInfoTextBox, "EEPROM");
			break;
		case 5:
			SCIBusPCMMemoryBinaryFilename = "ROMs/PCM/pcm_ram_" + text + ".bin";
			SCIBusPCMMemoryTextFilename = "ROMs/PCM/pcm_ram_" + text + ".txt";
			UpdateTextBox(SCIBusPCMReadMemoryInfoTextBox, "Initialize memory reading session.");
			UpdateTextBox(SCIBusPCMReadMemoryInfoTextBox, Environment.NewLine + "Preset: ");
			UpdateTextBox(SCIBusPCMReadMemoryInfoTextBox, "RAM");
			break;
		}
		UpdateTextBox(SCIBusPCMReadMemoryInfoTextBox, Environment.NewLine + "Command: " + Util.ByteToHexStringSimple(new byte[1] { SCIBusPCMReadMemoryCommand }));
		UpdateTextBox(SCIBusPCMReadMemoryInfoTextBox, Environment.NewLine + "Start offset: " + Util.ByteToHexStringSimple(SCIBusPCMMemoryOffsetStartBytes));
		UpdateTextBox(SCIBusPCMReadMemoryInfoTextBox, Environment.NewLine + "End offset: " + Util.ByteToHexStringSimple(SCIBusPCMMemoryOffsetEndBytes));
		UpdateTextBox(SCIBusPCMReadMemoryInfoTextBox, Environment.NewLine + "Increment: " + Util.ByteToHexStringSimple(SCIBusPCMIncrementBytes));
		UpdateTextBox(SCIBusPCMReadMemoryInfoTextBox, Environment.NewLine + "Output: " + SCIBusPCMMemoryBinaryFilename);
		UpdateTextBox(SCIBusPCMReadMemoryInfoTextBox, Environment.NewLine + "Binary size: " + SCIBusPCMTotalBytes + " bytes = " + ((double)SCIBusPCMTotalBytes / 1024.0).ToString("0.00") + " kilobytes.");
		UpdateTextBox(SCIBusPCMReadMemoryInfoTextBox, Environment.NewLine + "Memory reading session is ready to start.");
		if (!SCIBusPCMReadMemoryWorker.IsBusy)
		{
			((Control)SCIBusPCMReadMemoryInitializeSessionButton).Enabled = true;
			((Control)SCIBusPCMReadMemoryStartButton).Enabled = true;
			((Control)SCIBusPCMReadMemoryStopButton).Enabled = false;
			SCIBusPCMResponse = false;
			SCIBusPCMRxTimeout = false;
			SCIBusPCMTxTimeout = false;
			SCIBusPCMRxRetryCount = 0;
			SCIBusPCMTxRetryCount = 0;
			if (SCIBusPCMMemoryOffsetWidth == 16)
			{
				SCIBusPCMTxPayload = new byte[3]
				{
					SCIBusPCMReadMemoryCommand,
					SCIBusPCMCurrentMemoryOffsetBytes[0],
					SCIBusPCMCurrentMemoryOffsetBytes[1]
				};
			}
			else
			{
				SCIBusPCMTxPayload = new byte[4]
				{
					SCIBusPCMReadMemoryCommand,
					SCIBusPCMCurrentMemoryOffsetBytes[0],
					SCIBusPCMCurrentMemoryOffsetBytes[1],
					SCIBusPCMCurrentMemoryOffsetBytes[2]
				};
			}
			if (SCIBusPCMMemoryOffsetWidth == 24 && (SCIBusPCMReadMemoryCommand == 51 || SCIBusPCMReadMemoryCommand == 69))
			{
				SCIBusPCMTxPayload = new byte[6]
				{
					SCIBusPCMReadMemoryCommand,
					SCIBusPCMCurrentMemoryOffsetBytes[0],
					SCIBusPCMCurrentMemoryOffsetBytes[1],
					SCIBusPCMCurrentMemoryOffsetBytes[2],
					SCIBusPCMIncrementBytes[1],
					SCIBusPCMIncrementBytes[2]
				};
			}
			if (SCIBusPCMMemoryOffsetWidth == 16 && SCIBusPCMReadMemoryCommand == 57)
			{
				SCIBusPCMTxPayload = new byte[5]
				{
					SCIBusPCMReadMemoryCommand,
					SCIBusPCMCurrentMemoryOffsetBytes[0],
					SCIBusPCMCurrentMemoryOffsetBytes[1],
					SCIBusPCMIncrementBytes[0],
					SCIBusPCMIncrementBytes[1]
				};
			}
		}
		else
		{
			((Control)SCIBusPCMReadMemoryInitializeSessionButton).Enabled = true;
			((Control)SCIBusPCMReadMemoryStartButton).Enabled = false;
			((Control)SCIBusPCMReadMemoryStopButton).Enabled = false;
			UpdateTextBox(SCIBusPCMReadMemoryInfoTextBox, Environment.NewLine + "Failed to initialize session.");
		}
		if (SCIBusPCMReadMemoryCommand == 51 || SCIBusPCMReadMemoryCommand == 57 || SCIBusPCMReadMemoryCommand == 69)
		{
			SCIBusPCMNextRequestTimer.Interval = 10.0;
		}
		else
		{
			SCIBusPCMNextRequestTimer.Interval = 25.0;
		}
		((Control)SCIBusPCMReadMemoryHelpButton).Enabled = true;
	}

	private void SCIBusPCMReadMemoryStartButton_Click(object sender, EventArgs e)
	{
		((Control)SCIBusPCMReadMemoryCurrentOffsetTextBox).Text = Util.ByteToHexStringSimple(SCIBusPCMCurrentMemoryOffsetBytes);
		((TextBoxBase)SCIBusPCMReadMemoryCurrentOffsetTextBox).SelectionLength = 0;
		if (SCIBusPCMMemoryOffsetWidth == 16)
		{
			SCIBusPCMTxPayload = new byte[3]
			{
				SCIBusPCMReadMemoryCommand,
				SCIBusPCMCurrentMemoryOffsetBytes[0],
				SCIBusPCMCurrentMemoryOffsetBytes[1]
			};
		}
		else
		{
			SCIBusPCMTxPayload = new byte[4]
			{
				SCIBusPCMReadMemoryCommand,
				SCIBusPCMCurrentMemoryOffsetBytes[0],
				SCIBusPCMCurrentMemoryOffsetBytes[1],
				SCIBusPCMCurrentMemoryOffsetBytes[2]
			};
		}
		if (SCIBusPCMMemoryOffsetWidth == 24 && (SCIBusPCMReadMemoryCommand == 51 || SCIBusPCMReadMemoryCommand == 69))
		{
			SCIBusPCMTxPayload = new byte[6]
			{
				SCIBusPCMReadMemoryCommand,
				SCIBusPCMCurrentMemoryOffsetBytes[0],
				SCIBusPCMCurrentMemoryOffsetBytes[1],
				SCIBusPCMCurrentMemoryOffsetBytes[2],
				SCIBusPCMIncrementBytes[1],
				SCIBusPCMIncrementBytes[2]
			};
		}
		if (SCIBusPCMMemoryOffsetWidth == 16 && SCIBusPCMReadMemoryCommand == 57)
		{
			SCIBusPCMTxPayload = new byte[5]
			{
				SCIBusPCMReadMemoryCommand,
				SCIBusPCMCurrentMemoryOffsetBytes[0],
				SCIBusPCMCurrentMemoryOffsetBytes[1],
				SCIBusPCMIncrementBytes[0],
				SCIBusPCMIncrementBytes[1]
			};
		}
		((Control)SCIBusPCMReadMemoryInitializeSessionButton).Enabled = false;
		((Control)SCIBusPCMReadMemoryStartButton).Enabled = false;
		((Control)SCIBusPCMReadMemoryStopButton).Enabled = true;
		SCIBusPCMResponse = false;
		SCIBusPCMReadMemoryFinished = false;
		SCIBusPCMNextRequest = true;
		SCIBusPCMRxRetryCount = 0;
		SCIBusPCMTxRetryCount = 0;
		UpdateTextBox(SCIBusPCMReadMemoryInfoTextBox, Environment.NewLine + "Start memory reading session.");
		SCIBusPCMReadMemoryWorker.RunWorkerAsync();
	}

	private void SCIBusPCMReadMemoryStopButton_Click(object sender, EventArgs e)
	{
		if (SCIBusPCMReadMemoryWorker.IsBusy && SCIBusPCMReadMemoryWorker.WorkerSupportsCancellation)
		{
			SCIBusPCMReadMemoryWorker.CancelAsync();
		}
		SCIBusPCMResponse = false;
		SCIBusPCMReadMemoryFinished = true;
	}

	private void SCIBusPCMReadMemoryHelpButton_Click(object sender, EventArgs e)
	{
		if (((Control)SCIBusPCMReadMemoryInfoTextBox).Text != "")
		{
			UpdateTextBox(SCIBusPCMReadMemoryInfoTextBox, Environment.NewLine + Environment.NewLine);
		}
		UpdateTextBox(SCIBusPCMReadMemoryInfoTextBox, "----------------------HELP---------------------");
		UpdateTextBox(SCIBusPCMReadMemoryInfoTextBox, Environment.NewLine + "Command: hex value of the \"read memory\" command recognized by the PCM. For 16-bit address space 15, for 24-bit address space 26 is known to be working. Not all modules may support memory reading.");
		UpdateTextBox(SCIBusPCMReadMemoryInfoTextBox, Environment.NewLine + Environment.NewLine + "Start offset: this value (16/24-bit) signifies the starting address of the memory space. Zero is a good place to start.");
		UpdateTextBox(SCIBusPCMReadMemoryInfoTextBox, Environment.NewLine + Environment.NewLine + "End offset: this value (16/24-bit) signifies the ending address of the memory space.");
		UpdateTextBox(SCIBusPCMReadMemoryInfoTextBox, Environment.NewLine + Environment.NewLine + "Increment: memory values are read using multiple request messages and this value gets added to the requested memory offset. The default increment value of 1 is appropriate in 99.9% of cases.");
		UpdateTextBox(SCIBusPCMReadMemoryInfoTextBox, Environment.NewLine + Environment.NewLine + "Current offset/Value: these show the current status of the session.");
		UpdateTextBox(SCIBusPCMReadMemoryInfoTextBox, Environment.NewLine + Environment.NewLine + "Initialize session: prepares session, assigns output filename and makes the SCI-bus return to low-speed mode.");
		UpdateTextBox(SCIBusPCMReadMemoryInfoTextBox, Environment.NewLine + Environment.NewLine + "Start: begins to read the PCM's memory using the given instructions. Multiple timeout measures are in place to ensure data integrity and to avoid hiccups. The reading session ends automatically once the ending offset is reached.");
		UpdateTextBox(SCIBusPCMReadMemoryInfoTextBox, Environment.NewLine + Environment.NewLine + "Stop: cancels the current session with the possibility of resuming where the session was left hanging.");
		UpdateTextBox(SCIBusPCMReadMemoryInfoTextBox, Environment.NewLine + Environment.NewLine + "Progress: shows current session status in percentage done and how much memory bytes have been read out of the total bytes count.");
		UpdateTextBox(SCIBusPCMReadMemoryInfoTextBox, Environment.NewLine + Environment.NewLine + "Help: shows this text.");
		UpdateTextBox(SCIBusPCMReadMemoryInfoTextBox, Environment.NewLine + Environment.NewLine + "Remarks: the binary output file and text log file are being updated during the session in real time. PCMs may have different \"read memory\" commands. Memory reading works in low-speed mode only. ROM/RAM/EEPROM areas are usually reachable between a single offset interval. Check the target module's microcontroller datasheet to discover where different memory areas are located. The reader does not check the actual size of the program space, it has to be manually determined using the main GUI window. Check typical file size boundaries:" + Environment.NewLine + "- 7FFF: 32 kB" + Environment.NewLine + "- FFFF: 64 kB" + Environment.NewLine + "- 01FFFF: 128 kB" + Environment.NewLine + "- 03FFFF: 256 kB." + Environment.NewLine + "For a 128 kB program space the \"26 01 FF FF\" request should return a 5-byte response like \"26 01 FF FF AA\", where AA is the last readable memory value, while \"26 02 00 00\" will not return a memory value, just the 4-byte echo only. The command 28 seems to be used specifically to read EEPROM with 16-bit address space (28 00 00 - 28 01 FF).");
		UpdateTextBox(SCIBusPCMReadMemoryInfoTextBox, Environment.NewLine + "-----------------------------------------------" + Environment.NewLine);
	}

	private void SCIBusPCMReadMemory_DoWork(object sender, DoWorkEventArgs e)
	{
		while (!SCIBusPCMReadMemoryFinished)
		{
			Thread.Sleep(1);
			if (SCIBusPCMCurrentMemoryOffset > SCIBusPCMMemoryOffsetEnd - SCIBusPCMIncrement)
			{
				SCIBusPCMReadMemoryFinished = true;
			}
			if (SCIBusPCMReadMemoryWorker.CancellationPending)
			{
				e.Cancel = true;
				break;
			}
			while (!SCIBusPCMNextRequest)
			{
				Thread.Sleep(1);
				if (SCIBusPCMReadMemoryWorker.CancellationPending)
				{
					e.Cancel = true;
					break;
				}
			}
			SCIBusPCMResponse = false;
			SCIBusPCMNextRequest = false;
			SCIBusPCMReadMemoryWorker.ReportProgress(0);
			while (!SCIBusPCMResponse && !SCIBusPCMRxTimeout)
			{
				Thread.Sleep(1);
				if (SCIBusPCMReadMemoryWorker.CancellationPending)
				{
					e.Cancel = true;
					break;
				}
			}
			if (SCIBusPCMRxTimeout)
			{
				SCIBusPCMRxRetryCount++;
				if (SCIBusPCMRxRetryCount > 9)
				{
					SCIBusPCMRxRetryCount = 0;
					e.Cancel = true;
					break;
				}
				SCIBusPCMNextRequest = true;
				SCIBusPCMRxTimeout = false;
				SCIBusPCMRxTimeoutTimer.Stop();
				SCIBusPCMRxTimeoutTimer.Start();
			}
			if (SCIBusPCMResponse)
			{
				SCIBusPCMRxRetryCount = 0;
			}
		}
	}

	private void SCIBusPCMReadMemory_ProgressChanged(object sender, ProgressChangedEventArgs e)
	{
		if (e.ProgressPercentage == 0)
		{
			UpdateTextBox(SCIBusPCMReadMemoryInfoTextBox, Environment.NewLine + "TX: " + Util.ByteToHexStringSimple(SCIBusPCMTxPayload));
			Packet packet = new Packet();
			packet.Bus = 2;
			packet.Command = 6;
			packet.Mode = 2;
			packet.Payload = SCIBusPCMTxPayload;
			OriginalForm.TransmitUSBPacket("[<-TX] Send an SCI-bus (PCM) message once:", packet);
			SerialService.WritePacket(packet);
			SCIBusPCMRxTimeout = false;
			SCIBusPCMRxTimeoutTimer.Stop();
			SCIBusPCMRxTimeoutTimer.Start();
		}
	}

	private void SCIBusPCMReadMemory_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
	{
		if (e.Cancelled)
		{
			if (SCIBusPCMRxTimeout)
			{
				UpdateTextBox(SCIBusPCMReadMemoryInfoTextBox, Environment.NewLine + "Memory reading session timeout (RX).");
			}
			if (SCIBusPCMTxTimeout)
			{
				UpdateTextBox(SCIBusPCMReadMemoryInfoTextBox, Environment.NewLine + "Memory reading session timeout (TX).");
			}
			UpdateTextBox(SCIBusPCMReadMemoryInfoTextBox, Environment.NewLine + "Memory reading session cancelled.");
			((Control)SCIBusPCMReadMemoryInitializeSessionButton).Enabled = true;
			((Control)SCIBusPCMReadMemoryStartButton).Enabled = true;
			((Control)SCIBusPCMReadMemoryStopButton).Enabled = false;
		}
		else
		{
			UpdateTextBox(SCIBusPCMReadMemoryInfoTextBox, Environment.NewLine + "Output: " + SCIBusPCMMemoryBinaryFilename);
			UpdateTextBox(SCIBusPCMReadMemoryInfoTextBox, Environment.NewLine + "Memory reading session finished.");
			((Control)SCIBusPCMReadMemoryInitializeSessionButton).Enabled = true;
			((Control)SCIBusPCMReadMemoryStartButton).Enabled = false;
			((Control)SCIBusPCMReadMemoryStopButton).Enabled = false;
		}
		SCIBusPCMRxTimeout = false;
		SCIBusPCMRxTimeoutTimer.Stop();
		SCIBusPCMTxTimeout = false;
		SCIBusPCMTxTimeoutTimer.Stop();
	}

	private void SCIBusPCMReadMemoryPresetComboBox_SelectedIndexChanged(object sender, EventArgs e)
	{
		switch (((ListControl)SCIBusPCMReadMemoryPresetComboBox).SelectedIndex)
		{
		case 0:
			((Control)SCIBusPCMReadMemoryCommandTextBox).Text = "15";
			((Control)SCIBusPCMReadMemoryStartOffsetTextBox).Text = "80 00";
			((Control)SCIBusPCMReadMemoryEndOffsetTextBox).Text = "FF FF";
			((Control)SCIBusPCMReadMemoryIncrementTextBox).Text = "00 01";
			((Control)SCIBusPCMReadMemoryCurrentOffsetTextBox).Text = "00 00";
			((Control)SCIBusPCMReadMemoryValueTextBox).Text = "00";
			break;
		case 1:
			((Control)SCIBusPCMReadMemoryCommandTextBox).Text = "15";
			((Control)SCIBusPCMReadMemoryStartOffsetTextBox).Text = "00 00";
			((Control)SCIBusPCMReadMemoryEndOffsetTextBox).Text = "FF FF";
			((Control)SCIBusPCMReadMemoryIncrementTextBox).Text = "00 01";
			((Control)SCIBusPCMReadMemoryCurrentOffsetTextBox).Text = "00 00";
			((Control)SCIBusPCMReadMemoryValueTextBox).Text = "00";
			break;
		case 2:
			((Control)SCIBusPCMReadMemoryCommandTextBox).Text = "26";
			((Control)SCIBusPCMReadMemoryStartOffsetTextBox).Text = "00 00 00";
			((Control)SCIBusPCMReadMemoryEndOffsetTextBox).Text = "01 FF FF";
			((Control)SCIBusPCMReadMemoryIncrementTextBox).Text = "00 00 01";
			((Control)SCIBusPCMReadMemoryCurrentOffsetTextBox).Text = "00 00 00";
			((Control)SCIBusPCMReadMemoryValueTextBox).Text = "00";
			break;
		case 3:
			((Control)SCIBusPCMReadMemoryCommandTextBox).Text = "26";
			((Control)SCIBusPCMReadMemoryStartOffsetTextBox).Text = "00 00 00";
			((Control)SCIBusPCMReadMemoryEndOffsetTextBox).Text = "03 FF FF";
			((Control)SCIBusPCMReadMemoryIncrementTextBox).Text = "00 00 01";
			((Control)SCIBusPCMReadMemoryCurrentOffsetTextBox).Text = "00 00 00";
			((Control)SCIBusPCMReadMemoryValueTextBox).Text = "00";
			break;
		case 4:
			((Control)SCIBusPCMReadMemoryCommandTextBox).Text = "28";
			((Control)SCIBusPCMReadMemoryStartOffsetTextBox).Text = "00 00";
			((Control)SCIBusPCMReadMemoryEndOffsetTextBox).Text = "01 FF";
			((Control)SCIBusPCMReadMemoryIncrementTextBox).Text = "00 01";
			((Control)SCIBusPCMReadMemoryCurrentOffsetTextBox).Text = "00 00";
			((Control)SCIBusPCMReadMemoryValueTextBox).Text = "00";
			break;
		case 5:
			((Control)SCIBusPCMReadMemoryCommandTextBox).Text = "26";
			((Control)SCIBusPCMReadMemoryStartOffsetTextBox).Text = "0F 80 00";
			((Control)SCIBusPCMReadMemoryEndOffsetTextBox).Text = "0F 97 FF";
			((Control)SCIBusPCMReadMemoryIncrementTextBox).Text = "00 00 01";
			((Control)SCIBusPCMReadMemoryCurrentOffsetTextBox).Text = "0F 80 00";
			((Control)SCIBusPCMReadMemoryValueTextBox).Text = "00";
			break;
		}
	}

	private void PacketReceivedHandler(object sender, Packet packet)
	{
		UIContext.Post(delegate
		{
			_ = packet.Payload.Length;
			_ = 4;
			if (packet.Bus == 0 && packet.Command == 15 && packet.Mode == 254)
			{
				UpdateTextBox(SCIBusPCMReadMemoryInfoTextBox, " | error: internal error");
			}
			if (packet.Bus == 1)
			{
				CCDBusAliveTimer.Stop();
				CCDBusAliveTimer.Start();
				CCDBusAlive = true;
				byte[] array = packet.Payload.Skip(4).ToArray();
				if (array[0] == 178)
				{
					UpdateTextBox(CCDBusReadMemoryInfoTextBox, Environment.NewLine + "TX: " + Util.ByteToHexStringSimple(array));
					if (array.Take(5).ToArray().SequenceEqual(CCDBusTxPayload))
					{
						CCDBusEcho = true;
						CCDBusResponse = false;
						UpdateTextBox(CCDBusReadMemoryInfoTextBox, " | echo ok");
						CCDBusTxTimeoutTimer.Stop();
						CCDBusRxTimeout = false;
						CCDBusRxTimeoutTimer.Start();
					}
					else
					{
						CCDBusEcho = false;
						CCDBusResponse = false;
						UpdateTextBox(CCDBusReadMemoryInfoTextBox, " | error: invalid echo");
					}
				}
				if (array[0] == 242)
				{
					UpdateTextBox(CCDBusReadMemoryInfoTextBox, Environment.NewLine + "RX: " + Util.ByteToHexStringSimple(array));
					if (array.Length == 6)
					{
						if (array[1] == CCDBusModule && array[2] == CCDBusReadMemoryCommand && CCDBusEcho)
						{
							CCDBusRxTimeoutTimer.Stop();
							CCDBusNextRequest = false;
							CCDBusNextRequestTimer.Stop();
							CCDBusNextRequestTimer.Start();
							UpdateTextBox(CCDBusReadMemoryInfoTextBox, " | response ok");
							CCDBusEcho = false;
							CCDBusResponse = true;
							if (CCDBusIncrement == 2)
							{
								CCDBusMemoryValueBytes = array.Skip(3).Take(2).ToArray();
							}
							else
							{
								CCDBusMemoryValueBytes = array.Skip(3).Take(1).ToArray();
							}
							((Control)CCDBusReadMemoryCurrentOffsetTextBox).Text = Util.ByteToHexStringSimple(CCDBusCurrentMemoryOffsetBytes);
							((TextBoxBase)CCDBusReadMemoryCurrentOffsetTextBox).SelectionLength = 0;
							((Control)CCDBusReadMemoryValuesTextBox).Text = Util.ByteToHexStringSimple(CCDBusMemoryValueBytes);
							((TextBoxBase)CCDBusReadMemoryValuesTextBox).SelectionLength = 0;
							if (CCDBusIncrement == 2)
							{
								CCDBusBytesReadCount += 2;
							}
							else
							{
								CCDBusBytesReadCount++;
							}
							((Control)CCDBusReadMemoryProgressLabel).Text = "Progress: " + (byte)((double)CCDBusBytesReadCount / (double)CCDBusTotalBytes * 100.0) + "% (" + CCDBusBytesReadCount + "/" + CCDBusTotalBytes + " bytes)";
							using (BinaryWriter binaryWriter = new BinaryWriter(File.Open(CCDBusMemoryBinaryFilename, FileMode.Append)))
							{
								binaryWriter.Write(CCDBusMemoryValueBytes);
								binaryWriter.Close();
							}
							if (CCDBusCurrentMemoryOffset >= CCDBusMemoryOffsetEnd)
							{
								CCDBusReadMemoryFinished = true;
							}
							else
							{
								CCDBusCurrentMemoryOffset += CCDBusIncrement;
								CCDBusCurrentMemoryOffsetBytes[0] = (byte)(CCDBusCurrentMemoryOffset >> 8);
								CCDBusCurrentMemoryOffsetBytes[1] = (byte)CCDBusCurrentMemoryOffset;
								CCDBusTxPayload = new byte[5]
								{
									178,
									CCDBusModule,
									CCDBusReadMemoryCommand,
									CCDBusCurrentMemoryOffsetBytes[0],
									CCDBusCurrentMemoryOffsetBytes[1]
								};
							}
						}
						else if (array[2] == byte.MaxValue)
						{
							UpdateTextBox(CCDBusReadMemoryInfoTextBox, " | error: unknown command");
							CCDBusReadMemoryFinished = true;
							if (CCDBusReadMemoryWorker.IsBusy && CCDBusReadMemoryWorker.WorkerSupportsCancellation)
							{
								CCDBusReadMemoryWorker.CancelAsync();
							}
						}
						else
						{
							CCDBusEcho = false;
							CCDBusResponse = false;
							UpdateTextBox(CCDBusReadMemoryInfoTextBox, " | error: skip response");
						}
					}
					else
					{
						UpdateTextBox(CCDBusReadMemoryInfoTextBox, " | error: invalid length");
					}
				}
			}
			if (packet.Bus == 2)
			{
				byte[] array2 = packet.Payload.Skip(4).ToArray();
				if (SCIBusPCMReadMemoryCommand == 51 || SCIBusPCMReadMemoryCommand == 57 || SCIBusPCMReadMemoryCommand == 69)
				{
					UpdateTextBox(SCIBusPCMReadMemoryInfoTextBox, Environment.NewLine + "RX: " + Environment.NewLine + Util.ByteToHexStringSimple(array2));
				}
				else
				{
					UpdateTextBox(SCIBusPCMReadMemoryInfoTextBox, Environment.NewLine + "RX: " + Util.ByteToHexStringSimple(array2));
				}
				if (array2[0] == 254)
				{
					SCIBusPCMRxTimeoutTimer.Stop();
				}
				if ((array2[0] == 52 || array2[0] == 70) && array2.Length >= 6 + SCIBusPCMIncrement)
				{
					if (array2[1] == SCIBusPCMCurrentMemoryOffsetBytes[0] && array2[2] == SCIBusPCMCurrentMemoryOffsetBytes[1] && array2[3] == SCIBusPCMCurrentMemoryOffsetBytes[2])
					{
						SCIBusPCMResponse = true;
						SCIBusPCMRxTimeoutTimer.Stop();
						UpdateTextBox(SCIBusPCMReadMemoryInfoTextBox, " | ok");
						ushort num = (ushort)((array2[4] << 8) | array2[5]);
						SCIBusPCMMemoryArray = new byte[num];
						Array.Copy(array2, 6, SCIBusPCMMemoryArray, 0, num);
						((Control)SCIBusPCMReadMemoryCurrentOffsetTextBox).Text = Util.ByteToHexStringSimple(SCIBusPCMCurrentMemoryOffsetBytes);
						((TextBoxBase)SCIBusPCMReadMemoryCurrentOffsetTextBox).SelectionLength = 0;
						((Control)SCIBusPCMReadMemoryValueTextBox).Text = Util.ByteToHexStringSimple(SCIBusPCMMemoryArray);
						((TextBoxBase)SCIBusPCMReadMemoryValueTextBox).SelectionLength = 0;
						SCIBusPCMBytesReadCount += SCIBusPCMIncrement;
						((Control)SCIBusPCMReadMemoryProgressLabel).Text = "Progress: " + (byte)((double)SCIBusPCMBytesReadCount / (double)SCIBusPCMTotalBytes * 100.0) + "% (" + SCIBusPCMBytesReadCount + "/" + SCIBusPCMTotalBytes + " bytes)";
						using (BinaryWriter binaryWriter2 = new BinaryWriter(File.Open(SCIBusPCMMemoryBinaryFilename, FileMode.Append)))
						{
							binaryWriter2.Write(SCIBusPCMMemoryArray);
							binaryWriter2.Close();
						}
						SCIBusPCMCurrentMemoryOffset += SCIBusPCMIncrement;
						SCIBusPCMCurrentMemoryOffsetBytes[0] = (byte)(SCIBusPCMCurrentMemoryOffset >> 16);
						SCIBusPCMCurrentMemoryOffsetBytes[1] = (byte)(SCIBusPCMCurrentMemoryOffset >> 8);
						SCIBusPCMCurrentMemoryOffsetBytes[2] = (byte)SCIBusPCMCurrentMemoryOffset;
						SCIBusPCMTxPayload = new byte[6]
						{
							SCIBusPCMReadMemoryCommand,
							SCIBusPCMCurrentMemoryOffsetBytes[0],
							SCIBusPCMCurrentMemoryOffsetBytes[1],
							SCIBusPCMCurrentMemoryOffsetBytes[2],
							SCIBusPCMIncrementBytes[1],
							SCIBusPCMIncrementBytes[2]
						};
						SCIBusPCMNextRequest = false;
						SCIBusPCMNextRequestTimer.Stop();
						SCIBusPCMNextRequestTimer.Start();
					}
					else
					{
						UpdateTextBox(SCIBusPCMReadMemoryInfoTextBox, " | error: invalid offset");
					}
				}
				else if (array2[0] == 58 && array2.Length >= 5 + SCIBusPCMIncrement)
				{
					if (array2[1] == SCIBusPCMCurrentMemoryOffsetBytes[0] && array2[2] == SCIBusPCMCurrentMemoryOffsetBytes[1])
					{
						SCIBusPCMResponse = true;
						SCIBusPCMRxTimeoutTimer.Stop();
						UpdateTextBox(SCIBusPCMReadMemoryInfoTextBox, " | ok");
						ushort num2 = (ushort)((array2[3] << 8) | array2[4]);
						SCIBusPCMMemoryArray = new byte[num2];
						Array.Copy(array2, 5, SCIBusPCMMemoryArray, 0, num2);
						((Control)SCIBusPCMReadMemoryCurrentOffsetTextBox).Text = Util.ByteToHexStringSimple(SCIBusPCMCurrentMemoryOffsetBytes);
						((TextBoxBase)SCIBusPCMReadMemoryCurrentOffsetTextBox).SelectionLength = 0;
						((Control)SCIBusPCMReadMemoryValueTextBox).Text = Util.ByteToHexStringSimple(SCIBusPCMMemoryArray);
						((TextBoxBase)SCIBusPCMReadMemoryValueTextBox).SelectionLength = 0;
						SCIBusPCMBytesReadCount += SCIBusPCMIncrement;
						((Control)SCIBusPCMReadMemoryProgressLabel).Text = "Progress: " + (byte)((double)SCIBusPCMBytesReadCount / (double)SCIBusPCMTotalBytes * 100.0) + "% (" + SCIBusPCMBytesReadCount + "/" + SCIBusPCMTotalBytes + " bytes)";
						using (BinaryWriter binaryWriter3 = new BinaryWriter(File.Open(SCIBusPCMMemoryBinaryFilename, FileMode.Append)))
						{
							binaryWriter3.Write(SCIBusPCMMemoryArray);
							binaryWriter3.Close();
						}
						SCIBusPCMCurrentMemoryOffset += SCIBusPCMIncrement;
						SCIBusPCMCurrentMemoryOffsetBytes[0] = (byte)(SCIBusPCMCurrentMemoryOffset >> 8);
						SCIBusPCMCurrentMemoryOffsetBytes[1] = (byte)SCIBusPCMCurrentMemoryOffset;
						SCIBusPCMTxPayload = new byte[5]
						{
							SCIBusPCMReadMemoryCommand,
							SCIBusPCMCurrentMemoryOffsetBytes[0],
							SCIBusPCMCurrentMemoryOffsetBytes[1],
							SCIBusPCMIncrementBytes[0],
							SCIBusPCMIncrementBytes[1]
						};
						SCIBusPCMNextRequest = false;
						SCIBusPCMNextRequestTimer.Stop();
						SCIBusPCMNextRequestTimer.Start();
					}
					else
					{
						UpdateTextBox(SCIBusPCMReadMemoryInfoTextBox, " | error: invalid offset");
					}
				}
				else if (array2[0] == SCIBusPCMReadMemoryCommand)
				{
					if (array2.Length == SCIBusPCMTxPayload.Length + 1)
					{
						if (array2[1] == SCIBusPCMCurrentMemoryOffsetBytes[0] && array2[2] == SCIBusPCMCurrentMemoryOffsetBytes[1])
						{
							if (SCIBusPCMMemoryOffsetWidth == 16 || (SCIBusPCMMemoryOffsetWidth == 24 && array2[3] == SCIBusPCMCurrentMemoryOffsetBytes[2]))
							{
								SCIBusPCMResponse = true;
								SCIBusPCMRxTimeoutTimer.Stop();
								UpdateTextBox(SCIBusPCMReadMemoryInfoTextBox, " | ok");
								if (SCIBusPCMMemoryOffsetWidth == 16)
								{
									SCIBusPCMMemoryValue = array2[3];
								}
								if (SCIBusPCMMemoryOffsetWidth == 24)
								{
									SCIBusPCMMemoryValue = array2[4];
								}
								((Control)SCIBusPCMReadMemoryCurrentOffsetTextBox).Text = Util.ByteToHexStringSimple(SCIBusPCMCurrentMemoryOffsetBytes);
								((TextBoxBase)SCIBusPCMReadMemoryCurrentOffsetTextBox).SelectionLength = 0;
								((Control)SCIBusPCMReadMemoryValueTextBox).Text = Util.ByteToHexStringSimple(new byte[1] { SCIBusPCMMemoryValue });
								((TextBoxBase)SCIBusPCMReadMemoryValueTextBox).SelectionLength = 0;
								SCIBusPCMBytesReadCount++;
								((Control)SCIBusPCMReadMemoryProgressLabel).Text = "Progress: " + (byte)((double)SCIBusPCMBytesReadCount / (double)SCIBusPCMTotalBytes * 100.0) + "% (" + SCIBusPCMBytesReadCount + "/" + SCIBusPCMTotalBytes + " bytes)";
								using (BinaryWriter binaryWriter4 = new BinaryWriter(File.Open(SCIBusPCMMemoryBinaryFilename, FileMode.Append)))
								{
									binaryWriter4.Write(SCIBusPCMMemoryValue);
									binaryWriter4.Close();
								}
								SCIBusPCMCurrentMemoryOffset += SCIBusPCMIncrement;
								if (SCIBusPCMMemoryOffsetWidth == 16)
								{
									SCIBusPCMCurrentMemoryOffsetBytes[0] = (byte)(SCIBusPCMCurrentMemoryOffset >> 8);
									SCIBusPCMCurrentMemoryOffsetBytes[1] = (byte)SCIBusPCMCurrentMemoryOffset;
									SCIBusPCMTxPayload = new byte[3]
									{
										SCIBusPCMReadMemoryCommand,
										SCIBusPCMCurrentMemoryOffsetBytes[0],
										SCIBusPCMCurrentMemoryOffsetBytes[1]
									};
								}
								if (SCIBusPCMMemoryOffsetWidth == 24)
								{
									SCIBusPCMCurrentMemoryOffsetBytes[0] = (byte)(SCIBusPCMCurrentMemoryOffset >> 16);
									SCIBusPCMCurrentMemoryOffsetBytes[1] = (byte)(SCIBusPCMCurrentMemoryOffset >> 8);
									SCIBusPCMCurrentMemoryOffsetBytes[2] = (byte)SCIBusPCMCurrentMemoryOffset;
									SCIBusPCMTxPayload = new byte[4]
									{
										SCIBusPCMReadMemoryCommand,
										SCIBusPCMCurrentMemoryOffsetBytes[0],
										SCIBusPCMCurrentMemoryOffsetBytes[1],
										SCIBusPCMCurrentMemoryOffsetBytes[2]
									};
								}
								SCIBusPCMNextRequest = false;
								SCIBusPCMNextRequestTimer.Stop();
								SCIBusPCMNextRequestTimer.Start();
							}
							else
							{
								UpdateTextBox(SCIBusPCMReadMemoryInfoTextBox, " | error: invalid offset");
							}
						}
						else
						{
							UpdateTextBox(SCIBusPCMReadMemoryInfoTextBox, " | error: invalid offset");
						}
					}
					else
					{
						UpdateTextBox(SCIBusPCMReadMemoryInfoTextBox, " | error: no response");
					}
				}
				else
				{
					UpdateTextBox(SCIBusPCMReadMemoryInfoTextBox, " | error: invalid command");
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
			if (((Control)TB).Name == "CCDBusReadMemoryInfoTextBox" && CCDBusMemoryTextFilename != null)
			{
				File.AppendAllText(CCDBusMemoryTextFilename, text);
			}
			if (((Control)TB).Name == "SCIBusPCMReadMemoryInfoTextBox" && SCIBusPCMMemoryTextFilename != null)
			{
				File.AppendAllText(SCIBusPCMMemoryTextFilename, text);
			}
		});
	}

	private void ReadMemoryForm_FormClosing(object sender, FormClosingEventArgs e)
	{
		if (CCDBusReadMemoryWorker.IsBusy)
		{
			CCDBusReadMemoryStopButton_Click(this, EventArgs.Empty);
		}
		if (SCIBusPCMReadMemoryWorker.IsBusy)
		{
			SCIBusPCMReadMemoryStopButton_Click(this, EventArgs.Empty);
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
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Expected O, but got Unknown
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Expected O, but got Unknown
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Expected O, but got Unknown
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Expected O, but got Unknown
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Expected O, but got Unknown
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Expected O, but got Unknown
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Expected O, but got Unknown
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Expected O, but got Unknown
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Expected O, but got Unknown
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Expected O, but got Unknown
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Expected O, but got Unknown
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Expected O, but got Unknown
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Expected O, but got Unknown
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Expected O, but got Unknown
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Expected O, but got Unknown
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Expected O, but got Unknown
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Expected O, but got Unknown
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Expected O, but got Unknown
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Expected O, but got Unknown
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Expected O, but got Unknown
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Expected O, but got Unknown
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Expected O, but got Unknown
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Expected O, but got Unknown
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Expected O, but got Unknown
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Expected O, but got Unknown
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Expected O, but got Unknown
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Expected O, but got Unknown
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Expected O, but got Unknown
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_014f: Expected O, but got Unknown
		//IL_0150: Unknown result type (might be due to invalid IL or missing references)
		//IL_015a: Expected O, but got Unknown
		//IL_015b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0165: Expected O, but got Unknown
		//IL_0166: Unknown result type (might be due to invalid IL or missing references)
		//IL_0170: Expected O, but got Unknown
		//IL_0171: Unknown result type (might be due to invalid IL or missing references)
		//IL_017b: Expected O, but got Unknown
		//IL_017c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0186: Expected O, but got Unknown
		//IL_0187: Unknown result type (might be due to invalid IL or missing references)
		//IL_0191: Expected O, but got Unknown
		//IL_0192: Unknown result type (might be due to invalid IL or missing references)
		//IL_019c: Expected O, but got Unknown
		//IL_019d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a7: Expected O, but got Unknown
		//IL_01a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b2: Expected O, but got Unknown
		//IL_01b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bd: Expected O, but got Unknown
		//IL_01be: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c8: Expected O, but got Unknown
		//IL_01c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d3: Expected O, but got Unknown
		//IL_01d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01de: Expected O, but got Unknown
		//IL_01df: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e9: Expected O, but got Unknown
		//IL_0de7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0df1: Expected O, but got Unknown
		ComponentResourceManager componentResourceManager = new ComponentResourceManager(typeof(ReadMemoryForm));
		ReadMemoryTabControl = new TabControl();
		CCDBusTabPage = new TabPage();
		CCDBusReadMemoryHelpButton = new Button();
		CCDBusReadMemoryProgressLabel = new Label();
		CCDBusReadMemoryInfoTextBox = new TextBox();
		CCDBusReadMemoryValueLabel = new Label();
		CCDBusReadMemoryValuesTextBox = new TextBox();
		CCDBusReadMemoryCurrentOffsetLabel = new Label();
		CCDBusReadMemoryCurrentOffsetTextBox = new TextBox();
		CCDBusReadMemoryStopButton = new Button();
		CCDBusReadMemoryStartButton = new Button();
		CCDBusReadMemoryIncrementLabel = new Label();
		CCDBusReadMemoryIncrementTextBox = new TextBox();
		CCDBusReadMemoryEndOffsetLabel = new Label();
		CCDBusReadMemoryEndOffsetTextBox = new TextBox();
		CCDBusReadMemoryStartOffsetLabel = new Label();
		CCDBusReadMemoryStartOffsetTextBox = new TextBox();
		CCDBusReadMemoryCommandLabel = new Label();
		CCDBusReadMemoryCommandTextBox = new TextBox();
		CCDBusReadMemoryModuleLabel = new Label();
		CCDBusReadMemoryModuleTextBox = new TextBox();
		CCDBusReadMemoryInitializeSessionButton = new Button();
		SCIBusPCMTabPage = new TabPage();
		SCIBusPCMReadMemoryPresetComboBox = new ComboBox();
		SCIBusPCMReadMemoryHelpButton = new Button();
		SCIBusPCMReadMemoryProgressLabel = new Label();
		SCIBusPCMReadMemoryInfoTextBox = new TextBox();
		SCIBusPCMReadMemoryValueLabel = new Label();
		SCIBusPCMReadMemoryValueTextBox = new TextBox();
		SCIBusPCMReadMemoryCurrentOffsetLabel = new Label();
		SCIBusPCMReadMemoryCurrentOffsetTextBox = new TextBox();
		SCIBusPCMReadMemoryStopButton = new Button();
		SCIBusPCMReadMemoryStartButton = new Button();
		SCIBusPCMReadMemoryIncrementLabel = new Label();
		SCIBusPCMReadMemoryIncrementTextBox = new TextBox();
		SCIBusPCMReadMemoryEndOffsetLabel = new Label();
		SCIBusPCMReadMemoryEndOffsetTextBox = new TextBox();
		SCIBusPCMReadMemoryStartOffsetLabel = new Label();
		SCIBusPCMReadMemoryStartOffsetTextBox = new TextBox();
		SCIBusPCMReadMemoryCommandLabel = new Label();
		SCIBusPCMReadMemoryCommandTextBox = new TextBox();
		SCIBusPCMReadMemoryPresetLabel = new Label();
		SCIBusPCMReadMemoryInitializeSessionButton = new Button();
		((Control)ReadMemoryTabControl).SuspendLayout();
		((Control)CCDBusTabPage).SuspendLayout();
		((Control)SCIBusPCMTabPage).SuspendLayout();
		((Control)this).SuspendLayout();
		((Control)ReadMemoryTabControl).Controls.Add((Control)(object)CCDBusTabPage);
		((Control)ReadMemoryTabControl).Controls.Add((Control)(object)SCIBusPCMTabPage);
		componentResourceManager.ApplyResources(ReadMemoryTabControl, "ReadMemoryTabControl");
		((Control)ReadMemoryTabControl).Name = "ReadMemoryTabControl";
		ReadMemoryTabControl.SelectedIndex = 0;
		((Control)CCDBusTabPage).BackColor = Color.Transparent;
		((Control)CCDBusTabPage).Controls.Add((Control)(object)CCDBusReadMemoryHelpButton);
		((Control)CCDBusTabPage).Controls.Add((Control)(object)CCDBusReadMemoryProgressLabel);
		((Control)CCDBusTabPage).Controls.Add((Control)(object)CCDBusReadMemoryInfoTextBox);
		((Control)CCDBusTabPage).Controls.Add((Control)(object)CCDBusReadMemoryValueLabel);
		((Control)CCDBusTabPage).Controls.Add((Control)(object)CCDBusReadMemoryValuesTextBox);
		((Control)CCDBusTabPage).Controls.Add((Control)(object)CCDBusReadMemoryCurrentOffsetLabel);
		((Control)CCDBusTabPage).Controls.Add((Control)(object)CCDBusReadMemoryCurrentOffsetTextBox);
		((Control)CCDBusTabPage).Controls.Add((Control)(object)CCDBusReadMemoryStopButton);
		((Control)CCDBusTabPage).Controls.Add((Control)(object)CCDBusReadMemoryStartButton);
		((Control)CCDBusTabPage).Controls.Add((Control)(object)CCDBusReadMemoryIncrementLabel);
		((Control)CCDBusTabPage).Controls.Add((Control)(object)CCDBusReadMemoryIncrementTextBox);
		((Control)CCDBusTabPage).Controls.Add((Control)(object)CCDBusReadMemoryEndOffsetLabel);
		((Control)CCDBusTabPage).Controls.Add((Control)(object)CCDBusReadMemoryEndOffsetTextBox);
		((Control)CCDBusTabPage).Controls.Add((Control)(object)CCDBusReadMemoryStartOffsetLabel);
		((Control)CCDBusTabPage).Controls.Add((Control)(object)CCDBusReadMemoryStartOffsetTextBox);
		((Control)CCDBusTabPage).Controls.Add((Control)(object)CCDBusReadMemoryCommandLabel);
		((Control)CCDBusTabPage).Controls.Add((Control)(object)CCDBusReadMemoryCommandTextBox);
		((Control)CCDBusTabPage).Controls.Add((Control)(object)CCDBusReadMemoryModuleLabel);
		((Control)CCDBusTabPage).Controls.Add((Control)(object)CCDBusReadMemoryModuleTextBox);
		((Control)CCDBusTabPage).Controls.Add((Control)(object)CCDBusReadMemoryInitializeSessionButton);
		componentResourceManager.ApplyResources(CCDBusTabPage, "CCDBusTabPage");
		((Control)CCDBusTabPage).Name = "CCDBusTabPage";
		componentResourceManager.ApplyResources(CCDBusReadMemoryHelpButton, "CCDBusReadMemoryHelpButton");
		((Control)CCDBusReadMemoryHelpButton).Name = "CCDBusReadMemoryHelpButton";
		((ButtonBase)CCDBusReadMemoryHelpButton).UseVisualStyleBackColor = true;
		((Control)CCDBusReadMemoryHelpButton).Click += CCDBusReadMemoryHelpButton_Click;
		componentResourceManager.ApplyResources(CCDBusReadMemoryProgressLabel, "CCDBusReadMemoryProgressLabel");
		((Control)CCDBusReadMemoryProgressLabel).Name = "CCDBusReadMemoryProgressLabel";
		((Control)CCDBusReadMemoryInfoTextBox).BackColor = SystemColors.Window;
		componentResourceManager.ApplyResources(CCDBusReadMemoryInfoTextBox, "CCDBusReadMemoryInfoTextBox");
		((Control)CCDBusReadMemoryInfoTextBox).Name = "CCDBusReadMemoryInfoTextBox";
		componentResourceManager.ApplyResources(CCDBusReadMemoryValueLabel, "CCDBusReadMemoryValueLabel");
		((Control)CCDBusReadMemoryValueLabel).Name = "CCDBusReadMemoryValueLabel";
		((Control)CCDBusReadMemoryValuesTextBox).BackColor = SystemColors.Window;
		componentResourceManager.ApplyResources(CCDBusReadMemoryValuesTextBox, "CCDBusReadMemoryValuesTextBox");
		((Control)CCDBusReadMemoryValuesTextBox).Name = "CCDBusReadMemoryValuesTextBox";
		((TextBoxBase)CCDBusReadMemoryValuesTextBox).ReadOnly = true;
		componentResourceManager.ApplyResources(CCDBusReadMemoryCurrentOffsetLabel, "CCDBusReadMemoryCurrentOffsetLabel");
		((Control)CCDBusReadMemoryCurrentOffsetLabel).Name = "CCDBusReadMemoryCurrentOffsetLabel";
		((Control)CCDBusReadMemoryCurrentOffsetTextBox).BackColor = SystemColors.Window;
		componentResourceManager.ApplyResources(CCDBusReadMemoryCurrentOffsetTextBox, "CCDBusReadMemoryCurrentOffsetTextBox");
		((Control)CCDBusReadMemoryCurrentOffsetTextBox).Name = "CCDBusReadMemoryCurrentOffsetTextBox";
		((TextBoxBase)CCDBusReadMemoryCurrentOffsetTextBox).ReadOnly = true;
		componentResourceManager.ApplyResources(CCDBusReadMemoryStopButton, "CCDBusReadMemoryStopButton");
		((Control)CCDBusReadMemoryStopButton).Name = "CCDBusReadMemoryStopButton";
		((ButtonBase)CCDBusReadMemoryStopButton).UseVisualStyleBackColor = true;
		((Control)CCDBusReadMemoryStopButton).Click += CCDBusReadMemoryStopButton_Click;
		componentResourceManager.ApplyResources(CCDBusReadMemoryStartButton, "CCDBusReadMemoryStartButton");
		((Control)CCDBusReadMemoryStartButton).Name = "CCDBusReadMemoryStartButton";
		((ButtonBase)CCDBusReadMemoryStartButton).UseVisualStyleBackColor = true;
		((Control)CCDBusReadMemoryStartButton).Click += CCDBusReadMemoryStartButton_Click;
		componentResourceManager.ApplyResources(CCDBusReadMemoryIncrementLabel, "CCDBusReadMemoryIncrementLabel");
		((Control)CCDBusReadMemoryIncrementLabel).Name = "CCDBusReadMemoryIncrementLabel";
		componentResourceManager.ApplyResources(CCDBusReadMemoryIncrementTextBox, "CCDBusReadMemoryIncrementTextBox");
		((Control)CCDBusReadMemoryIncrementTextBox).Name = "CCDBusReadMemoryIncrementTextBox";
		componentResourceManager.ApplyResources(CCDBusReadMemoryEndOffsetLabel, "CCDBusReadMemoryEndOffsetLabel");
		((Control)CCDBusReadMemoryEndOffsetLabel).Name = "CCDBusReadMemoryEndOffsetLabel";
		componentResourceManager.ApplyResources(CCDBusReadMemoryEndOffsetTextBox, "CCDBusReadMemoryEndOffsetTextBox");
		((Control)CCDBusReadMemoryEndOffsetTextBox).Name = "CCDBusReadMemoryEndOffsetTextBox";
		componentResourceManager.ApplyResources(CCDBusReadMemoryStartOffsetLabel, "CCDBusReadMemoryStartOffsetLabel");
		((Control)CCDBusReadMemoryStartOffsetLabel).Name = "CCDBusReadMemoryStartOffsetLabel";
		componentResourceManager.ApplyResources(CCDBusReadMemoryStartOffsetTextBox, "CCDBusReadMemoryStartOffsetTextBox");
		((Control)CCDBusReadMemoryStartOffsetTextBox).Name = "CCDBusReadMemoryStartOffsetTextBox";
		componentResourceManager.ApplyResources(CCDBusReadMemoryCommandLabel, "CCDBusReadMemoryCommandLabel");
		((Control)CCDBusReadMemoryCommandLabel).Name = "CCDBusReadMemoryCommandLabel";
		componentResourceManager.ApplyResources(CCDBusReadMemoryCommandTextBox, "CCDBusReadMemoryCommandTextBox");
		((Control)CCDBusReadMemoryCommandTextBox).Name = "CCDBusReadMemoryCommandTextBox";
		componentResourceManager.ApplyResources(CCDBusReadMemoryModuleLabel, "CCDBusReadMemoryModuleLabel");
		((Control)CCDBusReadMemoryModuleLabel).Name = "CCDBusReadMemoryModuleLabel";
		componentResourceManager.ApplyResources(CCDBusReadMemoryModuleTextBox, "CCDBusReadMemoryModuleTextBox");
		((Control)CCDBusReadMemoryModuleTextBox).Name = "CCDBusReadMemoryModuleTextBox";
		componentResourceManager.ApplyResources(CCDBusReadMemoryInitializeSessionButton, "CCDBusReadMemoryInitializeSessionButton");
		((Control)CCDBusReadMemoryInitializeSessionButton).Name = "CCDBusReadMemoryInitializeSessionButton";
		((ButtonBase)CCDBusReadMemoryInitializeSessionButton).UseVisualStyleBackColor = true;
		((Control)CCDBusReadMemoryInitializeSessionButton).Click += CCDBusReadMemoryInitializeSessionButton_Click;
		((Control)SCIBusPCMTabPage).BackColor = Color.Transparent;
		((Control)SCIBusPCMTabPage).Controls.Add((Control)(object)SCIBusPCMReadMemoryPresetComboBox);
		((Control)SCIBusPCMTabPage).Controls.Add((Control)(object)SCIBusPCMReadMemoryHelpButton);
		((Control)SCIBusPCMTabPage).Controls.Add((Control)(object)SCIBusPCMReadMemoryProgressLabel);
		((Control)SCIBusPCMTabPage).Controls.Add((Control)(object)SCIBusPCMReadMemoryInfoTextBox);
		((Control)SCIBusPCMTabPage).Controls.Add((Control)(object)SCIBusPCMReadMemoryValueLabel);
		((Control)SCIBusPCMTabPage).Controls.Add((Control)(object)SCIBusPCMReadMemoryValueTextBox);
		((Control)SCIBusPCMTabPage).Controls.Add((Control)(object)SCIBusPCMReadMemoryCurrentOffsetLabel);
		((Control)SCIBusPCMTabPage).Controls.Add((Control)(object)SCIBusPCMReadMemoryCurrentOffsetTextBox);
		((Control)SCIBusPCMTabPage).Controls.Add((Control)(object)SCIBusPCMReadMemoryStopButton);
		((Control)SCIBusPCMTabPage).Controls.Add((Control)(object)SCIBusPCMReadMemoryStartButton);
		((Control)SCIBusPCMTabPage).Controls.Add((Control)(object)SCIBusPCMReadMemoryIncrementLabel);
		((Control)SCIBusPCMTabPage).Controls.Add((Control)(object)SCIBusPCMReadMemoryIncrementTextBox);
		((Control)SCIBusPCMTabPage).Controls.Add((Control)(object)SCIBusPCMReadMemoryEndOffsetLabel);
		((Control)SCIBusPCMTabPage).Controls.Add((Control)(object)SCIBusPCMReadMemoryEndOffsetTextBox);
		((Control)SCIBusPCMTabPage).Controls.Add((Control)(object)SCIBusPCMReadMemoryStartOffsetLabel);
		((Control)SCIBusPCMTabPage).Controls.Add((Control)(object)SCIBusPCMReadMemoryStartOffsetTextBox);
		((Control)SCIBusPCMTabPage).Controls.Add((Control)(object)SCIBusPCMReadMemoryCommandLabel);
		((Control)SCIBusPCMTabPage).Controls.Add((Control)(object)SCIBusPCMReadMemoryCommandTextBox);
		((Control)SCIBusPCMTabPage).Controls.Add((Control)(object)SCIBusPCMReadMemoryPresetLabel);
		((Control)SCIBusPCMTabPage).Controls.Add((Control)(object)SCIBusPCMReadMemoryInitializeSessionButton);
		componentResourceManager.ApplyResources(SCIBusPCMTabPage, "SCIBusPCMTabPage");
		((Control)SCIBusPCMTabPage).Name = "SCIBusPCMTabPage";
		SCIBusPCMReadMemoryPresetComboBox.DropDownStyle = (ComboBoxStyle)2;
		((ListControl)SCIBusPCMReadMemoryPresetComboBox).FormattingEnabled = true;
		SCIBusPCMReadMemoryPresetComboBox.Items.AddRange(new object[6]
		{
			componentResourceManager.GetString("SCIBusPCMReadMemoryPresetComboBox.Items"),
			componentResourceManager.GetString("SCIBusPCMReadMemoryPresetComboBox.Items1"),
			componentResourceManager.GetString("SCIBusPCMReadMemoryPresetComboBox.Items2"),
			componentResourceManager.GetString("SCIBusPCMReadMemoryPresetComboBox.Items3"),
			componentResourceManager.GetString("SCIBusPCMReadMemoryPresetComboBox.Items4"),
			componentResourceManager.GetString("SCIBusPCMReadMemoryPresetComboBox.Items5")
		});
		componentResourceManager.ApplyResources(SCIBusPCMReadMemoryPresetComboBox, "SCIBusPCMReadMemoryPresetComboBox");
		((Control)SCIBusPCMReadMemoryPresetComboBox).Name = "SCIBusPCMReadMemoryPresetComboBox";
		SCIBusPCMReadMemoryPresetComboBox.SelectedIndexChanged += SCIBusPCMReadMemoryPresetComboBox_SelectedIndexChanged;
		componentResourceManager.ApplyResources(SCIBusPCMReadMemoryHelpButton, "SCIBusPCMReadMemoryHelpButton");
		((Control)SCIBusPCMReadMemoryHelpButton).Name = "SCIBusPCMReadMemoryHelpButton";
		((ButtonBase)SCIBusPCMReadMemoryHelpButton).UseVisualStyleBackColor = true;
		((Control)SCIBusPCMReadMemoryHelpButton).Click += SCIBusPCMReadMemoryHelpButton_Click;
		componentResourceManager.ApplyResources(SCIBusPCMReadMemoryProgressLabel, "SCIBusPCMReadMemoryProgressLabel");
		((Control)SCIBusPCMReadMemoryProgressLabel).Name = "SCIBusPCMReadMemoryProgressLabel";
		((Control)SCIBusPCMReadMemoryInfoTextBox).BackColor = SystemColors.Window;
		componentResourceManager.ApplyResources(SCIBusPCMReadMemoryInfoTextBox, "SCIBusPCMReadMemoryInfoTextBox");
		((Control)SCIBusPCMReadMemoryInfoTextBox).Name = "SCIBusPCMReadMemoryInfoTextBox";
		((TextBoxBase)SCIBusPCMReadMemoryInfoTextBox).ReadOnly = true;
		componentResourceManager.ApplyResources(SCIBusPCMReadMemoryValueLabel, "SCIBusPCMReadMemoryValueLabel");
		((Control)SCIBusPCMReadMemoryValueLabel).Name = "SCIBusPCMReadMemoryValueLabel";
		((Control)SCIBusPCMReadMemoryValueTextBox).BackColor = SystemColors.Window;
		componentResourceManager.ApplyResources(SCIBusPCMReadMemoryValueTextBox, "SCIBusPCMReadMemoryValueTextBox");
		((Control)SCIBusPCMReadMemoryValueTextBox).Name = "SCIBusPCMReadMemoryValueTextBox";
		((TextBoxBase)SCIBusPCMReadMemoryValueTextBox).ReadOnly = true;
		componentResourceManager.ApplyResources(SCIBusPCMReadMemoryCurrentOffsetLabel, "SCIBusPCMReadMemoryCurrentOffsetLabel");
		((Control)SCIBusPCMReadMemoryCurrentOffsetLabel).Name = "SCIBusPCMReadMemoryCurrentOffsetLabel";
		((Control)SCIBusPCMReadMemoryCurrentOffsetTextBox).BackColor = SystemColors.Window;
		componentResourceManager.ApplyResources(SCIBusPCMReadMemoryCurrentOffsetTextBox, "SCIBusPCMReadMemoryCurrentOffsetTextBox");
		((Control)SCIBusPCMReadMemoryCurrentOffsetTextBox).Name = "SCIBusPCMReadMemoryCurrentOffsetTextBox";
		((TextBoxBase)SCIBusPCMReadMemoryCurrentOffsetTextBox).ReadOnly = true;
		componentResourceManager.ApplyResources(SCIBusPCMReadMemoryStopButton, "SCIBusPCMReadMemoryStopButton");
		((Control)SCIBusPCMReadMemoryStopButton).Name = "SCIBusPCMReadMemoryStopButton";
		((ButtonBase)SCIBusPCMReadMemoryStopButton).UseVisualStyleBackColor = true;
		((Control)SCIBusPCMReadMemoryStopButton).Click += SCIBusPCMReadMemoryStopButton_Click;
		componentResourceManager.ApplyResources(SCIBusPCMReadMemoryStartButton, "SCIBusPCMReadMemoryStartButton");
		((Control)SCIBusPCMReadMemoryStartButton).Name = "SCIBusPCMReadMemoryStartButton";
		((ButtonBase)SCIBusPCMReadMemoryStartButton).UseVisualStyleBackColor = true;
		((Control)SCIBusPCMReadMemoryStartButton).Click += SCIBusPCMReadMemoryStartButton_Click;
		componentResourceManager.ApplyResources(SCIBusPCMReadMemoryIncrementLabel, "SCIBusPCMReadMemoryIncrementLabel");
		((Control)SCIBusPCMReadMemoryIncrementLabel).Name = "SCIBusPCMReadMemoryIncrementLabel";
		componentResourceManager.ApplyResources(SCIBusPCMReadMemoryIncrementTextBox, "SCIBusPCMReadMemoryIncrementTextBox");
		((Control)SCIBusPCMReadMemoryIncrementTextBox).Name = "SCIBusPCMReadMemoryIncrementTextBox";
		componentResourceManager.ApplyResources(SCIBusPCMReadMemoryEndOffsetLabel, "SCIBusPCMReadMemoryEndOffsetLabel");
		((Control)SCIBusPCMReadMemoryEndOffsetLabel).Name = "SCIBusPCMReadMemoryEndOffsetLabel";
		componentResourceManager.ApplyResources(SCIBusPCMReadMemoryEndOffsetTextBox, "SCIBusPCMReadMemoryEndOffsetTextBox");
		((Control)SCIBusPCMReadMemoryEndOffsetTextBox).Name = "SCIBusPCMReadMemoryEndOffsetTextBox";
		componentResourceManager.ApplyResources(SCIBusPCMReadMemoryStartOffsetLabel, "SCIBusPCMReadMemoryStartOffsetLabel");
		((Control)SCIBusPCMReadMemoryStartOffsetLabel).Name = "SCIBusPCMReadMemoryStartOffsetLabel";
		componentResourceManager.ApplyResources(SCIBusPCMReadMemoryStartOffsetTextBox, "SCIBusPCMReadMemoryStartOffsetTextBox");
		((Control)SCIBusPCMReadMemoryStartOffsetTextBox).Name = "SCIBusPCMReadMemoryStartOffsetTextBox";
		componentResourceManager.ApplyResources(SCIBusPCMReadMemoryCommandLabel, "SCIBusPCMReadMemoryCommandLabel");
		((Control)SCIBusPCMReadMemoryCommandLabel).Name = "SCIBusPCMReadMemoryCommandLabel";
		componentResourceManager.ApplyResources(SCIBusPCMReadMemoryCommandTextBox, "SCIBusPCMReadMemoryCommandTextBox");
		((Control)SCIBusPCMReadMemoryCommandTextBox).Name = "SCIBusPCMReadMemoryCommandTextBox";
		componentResourceManager.ApplyResources(SCIBusPCMReadMemoryPresetLabel, "SCIBusPCMReadMemoryPresetLabel");
		((Control)SCIBusPCMReadMemoryPresetLabel).Name = "SCIBusPCMReadMemoryPresetLabel";
		componentResourceManager.ApplyResources(SCIBusPCMReadMemoryInitializeSessionButton, "SCIBusPCMReadMemoryInitializeSessionButton");
		((Control)SCIBusPCMReadMemoryInitializeSessionButton).Name = "SCIBusPCMReadMemoryInitializeSessionButton";
		((ButtonBase)SCIBusPCMReadMemoryInitializeSessionButton).UseVisualStyleBackColor = true;
		((Control)SCIBusPCMReadMemoryInitializeSessionButton).Click += SCIBusPCMReadMemoryInitializeSessionButton_Click;
		componentResourceManager.ApplyResources(this, "$this");
		((ContainerControl)this).AutoScaleMode = (AutoScaleMode)2;
		((Control)this).Controls.Add((Control)(object)ReadMemoryTabControl);
		((Control)this).Name = "ReadMemoryForm";
		((Form)this).FormClosing += new FormClosingEventHandler(ReadMemoryForm_FormClosing);
		((Control)ReadMemoryTabControl).ResumeLayout(false);
		((Control)CCDBusTabPage).ResumeLayout(false);
		((Control)CCDBusTabPage).PerformLayout();
		((Control)SCIBusPCMTabPage).ResumeLayout(false);
		((Control)SCIBusPCMTabPage).PerformLayout();
		((Control)this).ResumeLayout(false);
	}
}
