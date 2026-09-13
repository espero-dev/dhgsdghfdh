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
using ChryslerScanner.Properties;
using ChryslerScanner.Services;

namespace ChryslerScanner;

public class ReadWriteMemoryForm : Form
{
	private enum Task
	{
		None,
		ReadSRIMileage,
		ReadSKIMVTSS,
		ReadVIN,
		ReadPartNumber,
		ReadEEPROM,
		ReadRAM,
		BackupEEPROM,
		BackupRAM,
		WriteSRIMileage,
		WriteSKIMVTSS,
		WriteVIN,
		WritePartNumber,
		WriteEEPROM,
		WriteRAM,
		RestoreEEPROM
	}

	private enum CCD_ID
	{
		BCMMileage = 206
	}

	private enum SCI_ID_OBD1
	{
		ReadMemory = 21,
		PCMInfo = 22,
		WriteEEPROM = 28,
		WriteRAM1 = 29,
		WriteRAM2 = 30,
		WriteRAM3 = 31
	}

	private enum SCI_ID_OBD2
	{
		ReadROMRAM = 38,
		WriteEEPROM,
		ReadEEPROM,
		WriteRAM,
		PCMInfo,
		GetSecuritySeed,
		SendSecurityKey
	}

	private readonly MainForm OriginalForm;

	private readonly SerialService SerialService;

	private readonly SynchronizationContext UIContext;

	private bool PCMUnlocked;

	private const ushort EEPROMSize = 512;

	private const ushort RAMSize = 6144;

	private const ushort SRIMileageOffsetStartOBD1 = 46592;

	private const ushort SRIMileageOffsetEndOBD1 = 46607;

	private const ushort SRIMileageOffsetStart = 0;

	private const ushort SRIMileageOffsetEnd = 7;

	private const ushort SKIMVTSSOffset = 8;

	private const ushort VINOffsetStart = 98;

	private const ushort VINOffsetEnd = 114;

	private const ushort VINLength = 17;

	private const ushort PartNumberOffset1Start = 482;

	private const ushort PartNumberOffset1End = 490;

	private const ushort PartNumberOffset2Start = 496;

	private const ushort PartNumberOffset2End = 499;

	private const ushort CCDBCMMileageMsgLength = 6;

	private byte[] CCDBCMMileage = new byte[6];

	private double CCDBCMMileageMi;

	private double CCDBCMMileageKm;

	private bool CCDBCMMileageReceived;

	private const ushort SRIMileageLength = 2;

	private byte[] SRIMileage = new byte[2];

	private uint SRIMileageRaw;

	private double SRIMileageMi;

	private double SRIMileageKm;

	private byte[] SRIMileageNew = new byte[8];

	private uint SRIMileageNewRaw;

	private double SRIMileageNewMi;

	private double SRIMileageNewKm;

	private byte SKIMVTSS;

	private byte SKIMVTSSNew;

	private byte[] VIN = new byte[17];

	private string VINString;

	private byte[] PartNumberBuffer = new byte[18];

	private byte[] PartNumber = new byte[6];

	private ushort PartNumberLocation;

	private byte[] EEPROMBuffer;

	private byte[] RAMBuffer;

	private string SCIBusPCMWriteMemoryLogFilename;

	private string SCIBusPCMMemoryEEPROMBackupFilename;

	private string SCIBusPCMMemoryRAMBackupFilename;

	private System.Timers.Timer SCIBusPCMNextRequestTimer = new System.Timers.Timer();

	private System.Timers.Timer SCIBusPCMRxTimeoutTimer = new System.Timers.Timer();

	private System.Timers.Timer SCIBusPCMTxTimeoutTimer = new System.Timers.Timer();

	private BackgroundWorker SCIBusPCMReadMemoryWorker = new BackgroundWorker();

	private BackgroundWorker SCIBusPCMWriteMemoryWorker = new BackgroundWorker();

	private Task CurrentTask;

	private bool SCIBusPCMResponse;

	private bool SCIBusPCMNextRequest;

	private byte SCIBusPCMRxRetryCount;

	private byte SCIBusPCMTxRetryCount;

	private bool SCIBusPCMReadMemoryFinished;

	private bool SCIBusPCMWriteMemoryFinished;

	private bool SCIBusPCMRxTimeout;

	private bool SCIBusPCMTxTimeout;

	private byte[] SCIBusPCMTxPayload = new byte[4];

	private byte[] SCIBusPCMMemoryOffsetStartBytes = new byte[2];

	private uint SCIBusPCMMemoryOffsetStart;

	private byte[] SCIBusPCMMemoryOffsetEndBytes = new byte[2];

	private uint SCIBusPCMMemoryOffsetEnd;

	private byte[] SCIBusPCMCurrentMemoryOffsetBytes = new byte[2];

	private uint SCIBusPCMCurrentMemoryOffset;

	private byte SCIBusPCMMemoryValue;

	private TimeSpan ElapsedMillis;

	private DateTime Timestamp;

	private string TimestampString;

	private IContainer components;

	private TabControl WriteMemoryTabControl;

	private TabPage CCDBusTabPage;

	private TabPage SCIBusPCMTabPage;

	private Label SCIBusPCMWriteMemorySRIMileageLabel;

	private TextBox SCIBusPCMWriteMemorySRIMileageTextBox;

	private Label SCIBusPCMWriteMemorySRIMileageUnitLabel;

	private Button SCIBusPCMWriteMemorySRIMileageReadButton;

	private Button SCIBusPCMWriteMemorySRIMileageWriteButton;

	private Label SCIBusPCMWriteMemorySKIMVTSSLabel;

	private ComboBox SCIBusPCMWriteMemorySKIMVTSSComboBox;

	private Button SCIBusPCMWriteMemorySKIMVTSSReadButton;

	private Button SCIBusPCMWriteMemorySKIMVTSSWriteButton;

	private Label SCIBusPCMWriteMemoryVINLabel;

	private TextBox SCIBusPCMWriteMemoryVINTextBox;

	private Button SCIBusPCMWriteMemoryVINReadButton;

	private Button SCIBusPCMWriteMemoryVINWriteButton;

	private Label SCIBusPCMWriteMemoryPartNumberLabel;

	private TextBox SCIBusPCMWriteMemoryPartNumberTextBox;

	private Button SCIBusPCMWriteMemoryPartNumberReadButton;

	private Button SCIBusPCMWriteMemoryPartNumberWriteButton;

	private Button SCIBusPCMWriteMemoryHelpButton;

	private TextBox SCIBusPCMWriteMemoryInfoTextBox;

	private TextBox SCIBusPCMWriteMemoryRAMOffsetTextBox;

	private Label SCIBusPCMWriteMemoryRAMOffsetLabel;

	private TextBox SCIBusPCMWriteMemoryRAMValueTextBox;

	private Label SCIBusPCMWriteMemoryRAMValueLabel;

	private GroupBox SCIBusPCMWriteMemoryEEPROMGroupBox;

	private GroupBox SCIBusPCMWriteMemoryRAMGroupBox;

	private Label SCIBusPCMWriteMemoryEEPROMValueLabel;

	private TextBox SCIBusPCMWriteMemoryEEPROMValueTextBox;

	private Label SCIBusPCMWriteMemoryEEPROMOffsetLabel;

	private TextBox SCIBusPCMWriteMemoryEEPROMOffsetTextBox;

	private Button SCIBusPCMWriteMemoryEEPROMWriteButton;

	private Button SCIBusPCMWriteMemoryEEPROMReadButton;

	private Label SCIBusPCMWriteMemoryEEPROMSeparatorLabel;

	private Button SCIBusPCMWriteMemoryRAMWriteButton;

	private Button SCIBusPCMWriteMemoryRAMReadButton;

	private Label SCIBusPCMWriteMemoryEEPROMValueCountLabel;

	private TextBox SCIBusPCMWriteMemoryEEPROMValueCountTextBox;

	private Label SCIBusPCMWriteMemoryRAMValueCountLabel;

	private TextBox SCIBusPCMWriteMemoryRAMValueCountTextBox;

	private Button SCIBusPCMWriteMemoryCopyBCMMileageButton;

	private Button SCIBusPCMWriteMemoryEEPROMBackupButton;

	private Button SCIBusPCMWriteMemoryRAMBackupButton;

	private Button SCIBusPCMWriteMemoryEEPROMRestoreButton;

	private Button SCIBusPCMWriteMemoryRAMRestoreButton;

	private GroupBox groupBox1;

	private Button button2;

	private Button button3;

	private Label label1;

	private TextBox textBox2;

	private Button button4;

	private Button button5;

	private Label label3;

	private TextBox textBox3;

	private Label label4;

	private TextBox textBox4;

	private Button button1;

	private TextBox textBox1;

	private GroupBox groupBox2;

	public ReadWriteMemoryForm(MainForm IncomingForm, SerialService service)
	{
		OriginalForm = IncomingForm;
		InitializeComponent();
		UIContext = SynchronizationContext.Current;
		((Form)this).Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
		SerialService = service;
		SerialService.PacketReceived += PacketReceivedHandler;
		OriginalForm.ChangeLanguage();
		WriteMemoryTabControl.SelectedTab = SCIBusPCMTabPage;
		UpdateMileageUnit();
		SCIBusPCMWriteMemoryLogFilename = "LOG/PCM/pcmlog_write_memory_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".txt";
		SCIBusPCMNextRequestTimer.Elapsed += SCIBusPCMNextRequestHandler;
		SCIBusPCMNextRequestTimer.Interval = 25.0;
		SCIBusPCMNextRequestTimer.AutoReset = false;
		SCIBusPCMNextRequestTimer.Enabled = true;
		SCIBusPCMNextRequestTimer.Start();
		SCIBusPCMRxTimeoutTimer.Elapsed += SCIBusPCMRxTimeoutHandler;
		SCIBusPCMRxTimeoutTimer.Interval = 2000.0;
		SCIBusPCMRxTimeoutTimer.AutoReset = false;
		SCIBusPCMRxTimeoutTimer.Enabled = true;
		SCIBusPCMRxTimeoutTimer.Stop();
		SCIBusPCMTxTimeoutTimer.Elapsed += SCIBusPCMTxTimeoutHandler;
		SCIBusPCMTxTimeoutTimer.Interval = 2000.0;
		SCIBusPCMTxTimeoutTimer.AutoReset = false;
		SCIBusPCMTxTimeoutTimer.Enabled = true;
		SCIBusPCMTxTimeoutTimer.Stop();
		SCIBusPCMReadMemoryWorker.WorkerReportsProgress = true;
		SCIBusPCMReadMemoryWorker.WorkerSupportsCancellation = true;
		SCIBusPCMReadMemoryWorker.DoWork += SCIBusPCMReadMemory_DoWork;
		SCIBusPCMReadMemoryWorker.ProgressChanged += SCIBusPCMReadMemory_ProgressChanged;
		SCIBusPCMReadMemoryWorker.RunWorkerCompleted += SCIBusPCMReadMemory_RunWorkerCompleted;
		SCIBusPCMWriteMemoryWorker.WorkerReportsProgress = true;
		SCIBusPCMWriteMemoryWorker.WorkerSupportsCancellation = true;
		SCIBusPCMWriteMemoryWorker.DoWork += SCIBusPCMWriteMemory_DoWork;
		SCIBusPCMWriteMemoryWorker.ProgressChanged += SCIBusPCMWriteMemory_ProgressChanged;
		SCIBusPCMWriteMemoryWorker.RunWorkerCompleted += SCIBusPCMWriteMemory_RunWorkerCompleted;
		((ContainerControl)this).ActiveControl = (Control)(object)SCIBusPCMWriteMemorySRIMileageReadButton;
	}

	private void ReadWriteMemoryForm_Load(object sender, EventArgs e)
	{
		UpdateTextBox(SCIBusPCMWriteMemoryInfoTextBox, "Before modifying memory content, make sure to backup first with the \"BAK\" button.");
		UpdateTextBox(SCIBusPCMWriteMemoryInfoTextBox, Environment.NewLine + Environment.NewLine + "Cycle ignition key for changes in EEPROM to take effect. RAM changes are immediate.");
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

	private void SCIBusPCMWriteMemorySRIMileageTextBox_TextChanged(object sender, EventArgs e)
	{
		if (((Control)SCIBusPCMWriteMemorySRIMileageTextBox).Text.Length > 0)
		{
			if (((Control)SCIBusPCMWriteMemorySRIMileageTextBox).Text.Length > 8)
			{
				string text = Util.TruncateString(((Control)SCIBusPCMWriteMemorySRIMileageTextBox).Text, 8);
				((Control)SCIBusPCMWriteMemorySRIMileageTextBox).Text = text;
				((TextBoxBase)SCIBusPCMWriteMemorySRIMileageTextBox).SelectionStart = ((Control)SCIBusPCMWriteMemorySRIMileageTextBox).Text.Length;
				((TextBoxBase)SCIBusPCMWriteMemorySRIMileageTextBox).ScrollToCaret();
			}
			if (!((Control)SCIBusPCMWriteMemorySRIMileageTextBox).Text.IsNumeric())
			{
				((TextBoxBase)SCIBusPCMWriteMemorySRIMileageTextBox).Clear();
			}
			else
			{
				((Control)SCIBusPCMWriteMemorySRIMileageWriteButton).Enabled = true;
			}
		}
		else
		{
			((Control)SCIBusPCMWriteMemorySRIMileageWriteButton).Enabled = false;
		}
	}

	private void SCIBusPCMWriteMemorySRIMileageReadButton_Click(object sender, EventArgs e)
	{
		if (!SCIBusPCMReadMemoryWorker.IsBusy && !SCIBusPCMWriteMemoryWorker.IsBusy)
		{
			UpdateTextBox(SCIBusPCMWriteMemoryInfoTextBox, Environment.NewLine + Environment.NewLine + "Read SRI Mileage.");
			if (OriginalForm.PCM.logic == "inverted")
			{
				SCIBusPCMMemoryOffsetStart = 46592u;
				SCIBusPCMMemoryOffsetStartBytes[0] = 182;
				SCIBusPCMMemoryOffsetStartBytes[1] = 0;
				SCIBusPCMMemoryOffsetEnd = SCIBusPCMMemoryOffsetStart + 1;
				SCIBusPCMMemoryOffsetEndBytes[0] = 182;
				SCIBusPCMMemoryOffsetEndBytes[1] = 15;
				SCIBusPCMCurrentMemoryOffset = SCIBusPCMMemoryOffsetStart;
				SCIBusPCMCurrentMemoryOffsetBytes[0] = SCIBusPCMMemoryOffsetStartBytes[0];
				SCIBusPCMCurrentMemoryOffsetBytes[1] = SCIBusPCMMemoryOffsetStartBytes[1];
				SCIBusPCMTxPayload = new byte[3]
				{
					21,
					SCIBusPCMMemoryOffsetStartBytes[0],
					SCIBusPCMMemoryOffsetStartBytes[1]
				};
			}
			else
			{
				SCIBusPCMMemoryOffsetStart = 0u;
				SCIBusPCMMemoryOffsetStartBytes[0] = 0;
				SCIBusPCMMemoryOffsetStartBytes[1] = 0;
				SCIBusPCMMemoryOffsetEnd = SCIBusPCMMemoryOffsetStart + 1;
				SCIBusPCMMemoryOffsetEndBytes[0] = 0;
				SCIBusPCMMemoryOffsetEndBytes[1] = 7;
				SCIBusPCMCurrentMemoryOffset = SCIBusPCMMemoryOffsetStart;
				SCIBusPCMCurrentMemoryOffsetBytes[0] = SCIBusPCMMemoryOffsetStartBytes[0];
				SCIBusPCMCurrentMemoryOffsetBytes[1] = SCIBusPCMMemoryOffsetStartBytes[1];
				SCIBusPCMTxPayload = new byte[3]
				{
					40,
					SCIBusPCMMemoryOffsetStartBytes[0],
					SCIBusPCMMemoryOffsetStartBytes[1]
				};
			}
			SCIBusPCMReadMemoryFinished = false;
			SCIBusPCMNextRequest = true;
			CurrentTask = Task.ReadSRIMileage;
			SCIBusPCMReadMemoryWorker.RunWorkerAsync();
			((Control)SCIBusPCMWriteMemorySRIMileageReadButton).Enabled = false;
		}
	}

	private void SCIBusPCMWriteMemorySRIMileageWriteButton_Click(object sender, EventArgs e)
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		if (OriginalForm.PCM.logic == "inverted")
		{
			MessageBox.Show("Not supported yet.", "Warning", (MessageBoxButtons)0, (MessageBoxIcon)48);
		}
		else if (!SCIBusPCMReadMemoryWorker.IsBusy && !SCIBusPCMWriteMemoryWorker.IsBusy)
		{
			if (Settings.Default.Units == "imperial")
			{
				double.TryParse(((Control)SCIBusPCMWriteMemorySRIMileageTextBox).Text, out SRIMileageNewMi);
				SRIMileageNewKm = SRIMileageNewMi * 1.609344;
			}
			else if (Settings.Default.Units == "metric")
			{
				double.TryParse(((Control)SCIBusPCMWriteMemorySRIMileageTextBox).Text, out SRIMileageNewKm);
				SRIMileageNewMi = SRIMileageNewKm / 1.609344;
			}
			SRIMileageNewRaw = (uint)Math.Round(SRIMileageNewMi / 8.192);
			SRIMileageNew[0] = (byte)(SRIMileageNewRaw >> 8);
			SRIMileageNew[1] = (byte)SRIMileageNewRaw;
			SRIMileageNewRaw++;
			SRIMileageNew[2] = (byte)(SRIMileageNewRaw >> 8);
			SRIMileageNew[3] = (byte)SRIMileageNewRaw;
			SRIMileageNewRaw--;
			SRIMileageNew[4] = byte.MaxValue;
			SRIMileageNew[5] = byte.MaxValue;
			SRIMileageNew[6] = byte.MaxValue;
			SRIMileageNew[7] = byte.MaxValue;
			SRIMileageMi = (double)SRIMileageNewRaw * 8.192;
			SRIMileageKm = SRIMileageMi * 1.609344;
			if (OriginalForm.PCM.logic == "inverted")
			{
				SCIBusPCMMemoryOffsetStart = 46592u;
				SCIBusPCMMemoryOffsetStartBytes[0] = (byte)(SCIBusPCMMemoryOffsetStart >> 8);
				SCIBusPCMMemoryOffsetStartBytes[1] = (byte)SCIBusPCMMemoryOffsetStart;
				SCIBusPCMMemoryOffsetEnd = SCIBusPCMMemoryOffsetStart + 7;
				SCIBusPCMMemoryOffsetEndBytes[0] = (byte)(SCIBusPCMMemoryOffsetEnd >> 8);
				SCIBusPCMMemoryOffsetEndBytes[1] = (byte)SCIBusPCMMemoryOffsetEnd;
				SCIBusPCMCurrentMemoryOffset = SCIBusPCMMemoryOffsetStart;
				SCIBusPCMCurrentMemoryOffsetBytes[0] = SCIBusPCMMemoryOffsetStartBytes[0];
				SCIBusPCMCurrentMemoryOffsetBytes[1] = SCIBusPCMMemoryOffsetStartBytes[1];
				SCIBusPCMTxPayload = new byte[3]
				{
					28,
					SCIBusPCMMemoryOffsetStartBytes[1],
					SRIMileageNew[0]
				};
			}
			else
			{
				SCIBusPCMMemoryOffsetStart = 0u;
				SCIBusPCMMemoryOffsetStartBytes[0] = (byte)(SCIBusPCMMemoryOffsetStart >> 8);
				SCIBusPCMMemoryOffsetStartBytes[1] = (byte)SCIBusPCMMemoryOffsetStart;
				SCIBusPCMMemoryOffsetEnd = 7u;
				SCIBusPCMMemoryOffsetEndBytes[0] = (byte)(SCIBusPCMMemoryOffsetEnd >> 8);
				SCIBusPCMMemoryOffsetEndBytes[1] = (byte)SCIBusPCMMemoryOffsetEnd;
				SCIBusPCMCurrentMemoryOffset = SCIBusPCMMemoryOffsetStart;
				SCIBusPCMCurrentMemoryOffsetBytes[0] = SCIBusPCMMemoryOffsetStartBytes[0];
				SCIBusPCMCurrentMemoryOffsetBytes[1] = SCIBusPCMMemoryOffsetStartBytes[1];
				SCIBusPCMTxPayload = new byte[4]
				{
					39,
					SCIBusPCMMemoryOffsetStartBytes[0],
					SCIBusPCMMemoryOffsetStartBytes[1],
					SRIMileageNew[0]
				};
			}
			if (Settings.Default.Units == "imperial")
			{
				((Control)SCIBusPCMWriteMemorySRIMileageTextBox).Text = Math.Round(SRIMileageMi, 1).ToString("0.0");
				((TextBoxBase)SCIBusPCMWriteMemorySRIMileageTextBox).SelectionStart = ((Control)SCIBusPCMWriteMemorySRIMileageTextBox).Text.Length;
				((TextBoxBase)SCIBusPCMWriteMemorySRIMileageTextBox).ScrollToCaret();
			}
			else if (Settings.Default.Units == "metric")
			{
				((Control)SCIBusPCMWriteMemorySRIMileageTextBox).Text = Math.Round(SRIMileageKm, 1).ToString("0.0");
				((TextBoxBase)SCIBusPCMWriteMemorySRIMileageTextBox).SelectionStart = ((Control)SCIBusPCMWriteMemorySRIMileageTextBox).Text.Length;
				((TextBoxBase)SCIBusPCMWriteMemorySRIMileageTextBox).ScrollToCaret();
			}
			SCIBusPCMWriteMemoryFinished = false;
			SCIBusPCMNextRequest = true;
			CurrentTask = Task.WriteSRIMileage;
			SCIBusPCMWriteMemoryWorker.RunWorkerAsync();
			((Control)SCIBusPCMWriteMemorySRIMileageWriteButton).Enabled = false;
		}
	}

	private void SCIBusPCMWriteMemorySRIMileageTextBox_KeyPress(object sender, KeyPressEventArgs e)
	{
		if (e.KeyChar == '\r')
		{
			e.Handled = true;
			if (((Control)SCIBusPCMWriteMemorySRIMileageWriteButton).Enabled)
			{
				SCIBusPCMWriteMemorySRIMileageWriteButton_Click(this, EventArgs.Empty);
			}
		}
	}

	private void SCIBusPCMWriteMemorySKIMVTSSComboBox_SelectedIndexChanged(object sender, EventArgs e)
	{
		if (((ListControl)SCIBusPCMWriteMemorySKIMVTSSComboBox).SelectedIndex == 2)
		{
			((Control)SCIBusPCMWriteMemorySKIMVTSSWriteButton).Enabled = false;
		}
		else
		{
			((Control)SCIBusPCMWriteMemorySKIMVTSSWriteButton).Enabled = true;
		}
	}

	private void SCIBusPCMWriteMemorySKIMVTSSReadButton_Click(object sender, EventArgs e)
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		if (OriginalForm.PCM.logic == "inverted")
		{
			MessageBox.Show("Not supported.", "Warning", (MessageBoxButtons)0, (MessageBoxIcon)48);
		}
		else if (!SCIBusPCMReadMemoryWorker.IsBusy && !SCIBusPCMWriteMemoryWorker.IsBusy)
		{
			UpdateTextBox(SCIBusPCMWriteMemoryInfoTextBox, Environment.NewLine + Environment.NewLine + "Read SKIM status.");
			SCIBusPCMMemoryOffsetStart = 8u;
			SCIBusPCMMemoryOffsetStartBytes[0] = 0;
			SCIBusPCMMemoryOffsetStartBytes[1] = 8;
			SCIBusPCMMemoryOffsetEnd = 8u;
			SCIBusPCMMemoryOffsetEndBytes[0] = 0;
			SCIBusPCMMemoryOffsetEndBytes[1] = 8;
			SCIBusPCMCurrentMemoryOffset = SCIBusPCMMemoryOffsetStart;
			SCIBusPCMCurrentMemoryOffsetBytes[0] = SCIBusPCMMemoryOffsetStartBytes[0];
			SCIBusPCMCurrentMemoryOffsetBytes[1] = SCIBusPCMMemoryOffsetStartBytes[1];
			SCIBusPCMTxPayload = new byte[3]
			{
				40,
				SCIBusPCMMemoryOffsetStartBytes[0],
				SCIBusPCMMemoryOffsetStartBytes[1]
			};
			SCIBusPCMReadMemoryFinished = false;
			SCIBusPCMNextRequest = true;
			CurrentTask = Task.ReadSKIMVTSS;
			SCIBusPCMReadMemoryWorker.RunWorkerAsync();
			((Control)SCIBusPCMWriteMemorySKIMVTSSReadButton).Enabled = false;
		}
	}

	private void SCIBusPCMWriteMemorySKIMVTSSWriteButton_Click(object sender, EventArgs e)
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		if (OriginalForm.PCM.logic == "inverted")
		{
			MessageBox.Show("Not supported.", "Warning", (MessageBoxButtons)0, (MessageBoxIcon)48);
		}
		else if (!SCIBusPCMReadMemoryWorker.IsBusy && !SCIBusPCMWriteMemoryWorker.IsBusy)
		{
			UpdateTextBox(SCIBusPCMWriteMemoryInfoTextBox, Environment.NewLine + Environment.NewLine + "Write SKIM status.");
			SCIBusPCMMemoryOffsetStart = 8u;
			SCIBusPCMMemoryOffsetStartBytes[0] = 0;
			SCIBusPCMMemoryOffsetStartBytes[1] = 8;
			SCIBusPCMMemoryOffsetEnd = 8u;
			SCIBusPCMMemoryOffsetEndBytes[0] = 0;
			SCIBusPCMMemoryOffsetEndBytes[1] = 8;
			SCIBusPCMCurrentMemoryOffset = SCIBusPCMMemoryOffsetStart;
			SCIBusPCMCurrentMemoryOffsetBytes[0] = SCIBusPCMMemoryOffsetStartBytes[0];
			SCIBusPCMCurrentMemoryOffsetBytes[1] = SCIBusPCMMemoryOffsetStartBytes[1];
			byte b = byte.MaxValue;
			switch (((ListControl)SCIBusPCMWriteMemorySKIMVTSSComboBox).SelectedIndex)
			{
			case 0:
			case 3:
				b = byte.MaxValue;
				break;
			case 1:
				b = 0;
				break;
			}
			SCIBusPCMTxPayload = new byte[4]
			{
				39,
				SCIBusPCMMemoryOffsetStartBytes[0],
				SCIBusPCMMemoryOffsetStartBytes[1],
				b
			};
			SCIBusPCMWriteMemoryFinished = false;
			SCIBusPCMNextRequest = true;
			CurrentTask = Task.WriteSKIMVTSS;
			SCIBusPCMWriteMemoryWorker.RunWorkerAsync();
			((Control)SCIBusPCMWriteMemorySKIMVTSSWriteButton).Enabled = false;
		}
	}

	private void SCIBusPCMWriteMemoryVINTextBox_TextChanged(object sender, EventArgs e)
	{
		if (((Control)SCIBusPCMWriteMemoryVINTextBox).Text.Length >= 17)
		{
			string text = Util.TruncateString(((Control)SCIBusPCMWriteMemoryVINTextBox).Text.ToUpper(), 17);
			((Control)SCIBusPCMWriteMemoryVINTextBox).Text = text;
			((TextBoxBase)SCIBusPCMWriteMemoryVINTextBox).SelectionStart = ((Control)SCIBusPCMWriteMemoryVINTextBox).Text.Length;
			((TextBoxBase)SCIBusPCMWriteMemoryVINTextBox).ScrollToCaret();
			((Control)SCIBusPCMWriteMemoryVINWriteButton).Enabled = true;
		}
		else
		{
			((Control)SCIBusPCMWriteMemoryVINWriteButton).Enabled = false;
		}
	}

	private void SCIBusPCMWriteMemoryVINReadButton_Click(object sender, EventArgs e)
	{
		if (!SCIBusPCMReadMemoryWorker.IsBusy && !SCIBusPCMWriteMemoryWorker.IsBusy)
		{
			UpdateTextBox(SCIBusPCMWriteMemoryInfoTextBox, Environment.NewLine + Environment.NewLine + "Read VIN.");
			SCIBusPCMMemoryOffsetStart = 98u;
			SCIBusPCMMemoryOffsetStartBytes[0] = 0;
			SCIBusPCMMemoryOffsetStartBytes[1] = 98;
			SCIBusPCMMemoryOffsetEnd = 114u;
			SCIBusPCMMemoryOffsetEndBytes[0] = 0;
			SCIBusPCMMemoryOffsetEndBytes[1] = 114;
			SCIBusPCMCurrentMemoryOffset = SCIBusPCMMemoryOffsetStart;
			SCIBusPCMCurrentMemoryOffsetBytes[0] = SCIBusPCMMemoryOffsetStartBytes[0];
			SCIBusPCMCurrentMemoryOffsetBytes[1] = SCIBusPCMMemoryOffsetStartBytes[1];
			SCIBusPCMTxPayload = new byte[3]
			{
				40,
				SCIBusPCMMemoryOffsetStartBytes[0],
				SCIBusPCMMemoryOffsetStartBytes[1]
			};
			SCIBusPCMReadMemoryFinished = false;
			SCIBusPCMNextRequest = true;
			CurrentTask = Task.ReadVIN;
			SCIBusPCMReadMemoryWorker.RunWorkerAsync();
			((Control)SCIBusPCMWriteMemoryVINReadButton).Enabled = false;
		}
	}

	private void SCIBusPCMWriteMemoryVINWriteButton_Click(object sender, EventArgs e)
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		if (OriginalForm.PCM.logic == "inverted")
		{
			MessageBox.Show("Not supported yet.", "Warning", (MessageBoxButtons)0, (MessageBoxIcon)48);
		}
		else if (!SCIBusPCMReadMemoryWorker.IsBusy && !SCIBusPCMWriteMemoryWorker.IsBusy && ((Control)SCIBusPCMWriteMemoryVINTextBox).Text.Length == 17)
		{
			VINString = ((Control)SCIBusPCMWriteMemoryVINTextBox).Text.ToUpper();
			VIN = Encoding.ASCII.GetBytes(VINString);
			SCIBusPCMMemoryOffsetStart = 98u;
			SCIBusPCMMemoryOffsetStartBytes[0] = (byte)(SCIBusPCMMemoryOffsetStart >> 8);
			SCIBusPCMMemoryOffsetStartBytes[1] = (byte)SCIBusPCMMemoryOffsetStart;
			SCIBusPCMMemoryOffsetEnd = 114u;
			SCIBusPCMMemoryOffsetEndBytes[0] = (byte)(SCIBusPCMMemoryOffsetEnd >> 8);
			SCIBusPCMMemoryOffsetEndBytes[1] = (byte)SCIBusPCMMemoryOffsetEnd;
			SCIBusPCMCurrentMemoryOffset = SCIBusPCMMemoryOffsetStart;
			SCIBusPCMCurrentMemoryOffsetBytes[0] = SCIBusPCMMemoryOffsetStartBytes[0];
			SCIBusPCMCurrentMemoryOffsetBytes[1] = SCIBusPCMMemoryOffsetStartBytes[1];
			SCIBusPCMTxPayload = new byte[4]
			{
				39,
				SCIBusPCMMemoryOffsetStartBytes[0],
				SCIBusPCMMemoryOffsetStartBytes[1],
				VIN[0]
			};
			SCIBusPCMWriteMemoryFinished = false;
			SCIBusPCMNextRequest = true;
			CurrentTask = Task.WriteVIN;
			SCIBusPCMWriteMemoryWorker.RunWorkerAsync();
			((Control)SCIBusPCMWriteMemoryVINWriteButton).Enabled = false;
		}
	}

	private void SCIBusPCMWriteMemoryVINTextBox_KeyPress(object sender, KeyPressEventArgs e)
	{
		if (e.KeyChar == '\r')
		{
			e.Handled = true;
			if (((Control)SCIBusPCMWriteMemoryVINWriteButton).Enabled)
			{
				SCIBusPCMWriteMemoryVINWriteButton_Click(this, EventArgs.Empty);
			}
		}
	}

	private void SCIBusPCMWriteMemoryPartNumberTextBox_TextChanged(object sender, EventArgs e)
	{
		if (!Util.TruncateString(((Control)SCIBusPCMWriteMemoryPartNumberTextBox).Text, 8).ToString().IsNumeric())
		{
			((TextBoxBase)SCIBusPCMWriteMemoryPartNumberTextBox).Clear();
		}
		if (((Control)SCIBusPCMWriteMemoryPartNumberTextBox).Text.Length >= 10)
		{
			string text = Util.TruncateString(((Control)SCIBusPCMWriteMemoryPartNumberTextBox).Text, 10).ToUpper();
			((Control)SCIBusPCMWriteMemoryPartNumberTextBox).Text = text;
			((TextBoxBase)SCIBusPCMWriteMemoryPartNumberTextBox).SelectionStart = ((Control)SCIBusPCMWriteMemoryPartNumberTextBox).Text.Length;
			((TextBoxBase)SCIBusPCMWriteMemoryPartNumberTextBox).ScrollToCaret();
		}
		if (((Control)SCIBusPCMWriteMemoryPartNumberTextBox).Text.Length == 8 || ((Control)SCIBusPCMWriteMemoryPartNumberTextBox).Text.Length == 10)
		{
			((Control)SCIBusPCMWriteMemoryPartNumberWriteButton).Enabled = true;
		}
		else
		{
			((Control)SCIBusPCMWriteMemoryPartNumberWriteButton).Enabled = false;
		}
	}

	private void SCIBusPCMWriteMemoryPartNumberReadButton_Click(object sender, EventArgs e)
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		if (OriginalForm.PCM.logic == "inverted")
		{
			MessageBox.Show("Not supported.", "Warning", (MessageBoxButtons)0, (MessageBoxIcon)48);
		}
		else if (!SCIBusPCMReadMemoryWorker.IsBusy && !SCIBusPCMWriteMemoryWorker.IsBusy)
		{
			UpdateTextBox(SCIBusPCMWriteMemoryInfoTextBox, Environment.NewLine + Environment.NewLine + "Read Part Number.");
			SCIBusPCMMemoryOffsetStart = 482u;
			SCIBusPCMMemoryOffsetStartBytes[0] = 1;
			SCIBusPCMMemoryOffsetStartBytes[1] = 226;
			SCIBusPCMMemoryOffsetEnd = 499u;
			SCIBusPCMMemoryOffsetEndBytes[0] = 1;
			SCIBusPCMMemoryOffsetEndBytes[1] = 243;
			SCIBusPCMCurrentMemoryOffset = SCIBusPCMMemoryOffsetStart;
			SCIBusPCMCurrentMemoryOffsetBytes[0] = SCIBusPCMMemoryOffsetStartBytes[0];
			SCIBusPCMCurrentMemoryOffsetBytes[1] = SCIBusPCMMemoryOffsetStartBytes[1];
			SCIBusPCMTxPayload = new byte[3]
			{
				40,
				SCIBusPCMMemoryOffsetStartBytes[0],
				SCIBusPCMMemoryOffsetStartBytes[1]
			};
			SCIBusPCMReadMemoryFinished = false;
			SCIBusPCMNextRequest = true;
			CurrentTask = Task.ReadPartNumber;
			SCIBusPCMReadMemoryWorker.RunWorkerAsync();
			((Control)SCIBusPCMWriteMemoryPartNumberReadButton).Enabled = false;
		}
	}

	private void SCIBusPCMWriteMemoryPartNumberWriteButton_Click(object sender, EventArgs e)
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		if (OriginalForm.PCM.logic == "inverted")
		{
			MessageBox.Show("Not supported.", "Warning", (MessageBoxButtons)0, (MessageBoxIcon)48);
		}
		else
		{
			if (SCIBusPCMReadMemoryWorker.IsBusy || SCIBusPCMWriteMemoryWorker.IsBusy)
			{
				return;
			}
			if (PartNumberLocation == 0)
			{
				UpdateTextBox(SCIBusPCMWriteMemoryInfoTextBox, Environment.NewLine + Environment.NewLine + "Part Number needs to be read first before writing a new one!");
				return;
			}
			switch (((Control)SCIBusPCMWriteMemoryPartNumberTextBox).Text.Length)
			{
			case 8:
			{
				byte[] array = Util.HexStringToByte(((Control)SCIBusPCMWriteMemoryPartNumberTextBox).Text);
				Array.Copy(array, PartNumberBuffer, array.Length);
				PartNumber[0] = PartNumberBuffer[0];
				PartNumber[1] = PartNumberBuffer[1];
				PartNumber[2] = PartNumberBuffer[2];
				PartNumber[3] = PartNumberBuffer[3];
				PartNumber[4] = 0;
				PartNumber[5] = 0;
				break;
			}
			case 10:
			{
				byte[] array = Util.HexStringToByte(Util.TruncateString(((Control)SCIBusPCMWriteMemoryPartNumberTextBox).Text, 8));
				Array.Copy(array, PartNumberBuffer, array.Length);
				PartNumber[0] = PartNumberBuffer[0];
				PartNumber[1] = PartNumberBuffer[1];
				PartNumber[2] = PartNumberBuffer[2];
				PartNumber[3] = PartNumberBuffer[3];
				PartNumber[4] = Encoding.ASCII.GetBytes(((Control)SCIBusPCMWriteMemoryPartNumberTextBox).Text)[8];
				PartNumber[5] = Encoding.ASCII.GetBytes(((Control)SCIBusPCMWriteMemoryPartNumberTextBox).Text)[9];
				break;
			}
			default:
				UpdateTextBox(SCIBusPCMWriteMemoryInfoTextBox, Environment.NewLine + Environment.NewLine + "Invalid Part Number length.");
				break;
			}
			switch (PartNumberLocation)
			{
			case 482:
				if (PartNumber[4] != 0 && PartNumber[5] != 0)
				{
					SCIBusPCMMemoryOffsetStart = 482u;
					SCIBusPCMMemoryOffsetStartBytes[0] = (byte)(SCIBusPCMMemoryOffsetStart >> 8);
					SCIBusPCMMemoryOffsetStartBytes[1] = (byte)SCIBusPCMMemoryOffsetStart;
					SCIBusPCMMemoryOffsetEnd = 490u;
					SCIBusPCMMemoryOffsetEndBytes[0] = (byte)(SCIBusPCMMemoryOffsetEnd >> 8);
					SCIBusPCMMemoryOffsetEndBytes[1] = (byte)SCIBusPCMMemoryOffsetEnd;
				}
				else
				{
					SCIBusPCMMemoryOffsetStart = 482u;
					SCIBusPCMMemoryOffsetStartBytes[0] = (byte)(SCIBusPCMMemoryOffsetStart >> 8);
					SCIBusPCMMemoryOffsetStartBytes[1] = (byte)SCIBusPCMMemoryOffsetStart;
					SCIBusPCMMemoryOffsetEnd = SCIBusPCMMemoryOffsetStart + 3;
					SCIBusPCMMemoryOffsetEndBytes[0] = (byte)(SCIBusPCMMemoryOffsetEnd >> 8);
					SCIBusPCMMemoryOffsetEndBytes[1] = (byte)SCIBusPCMMemoryOffsetEnd;
				}
				break;
			case 496:
				SCIBusPCMMemoryOffsetStart = 496u;
				SCIBusPCMMemoryOffsetStartBytes[0] = (byte)(SCIBusPCMMemoryOffsetStart >> 8);
				SCIBusPCMMemoryOffsetStartBytes[1] = (byte)SCIBusPCMMemoryOffsetStart;
				SCIBusPCMMemoryOffsetEnd = 499u;
				SCIBusPCMMemoryOffsetEndBytes[0] = (byte)(SCIBusPCMMemoryOffsetEnd >> 8);
				SCIBusPCMMemoryOffsetEndBytes[1] = (byte)SCIBusPCMMemoryOffsetEnd;
				break;
			}
			SCIBusPCMCurrentMemoryOffset = SCIBusPCMMemoryOffsetStart;
			SCIBusPCMCurrentMemoryOffsetBytes[0] = SCIBusPCMMemoryOffsetStartBytes[0];
			SCIBusPCMCurrentMemoryOffsetBytes[1] = SCIBusPCMMemoryOffsetStartBytes[1];
			SCIBusPCMTxPayload = new byte[4]
			{
				39,
				SCIBusPCMMemoryOffsetStartBytes[0],
				SCIBusPCMMemoryOffsetStartBytes[1],
				PartNumber[0]
			};
			SCIBusPCMWriteMemoryFinished = false;
			SCIBusPCMNextRequest = true;
			CurrentTask = Task.WritePartNumber;
			SCIBusPCMWriteMemoryWorker.RunWorkerAsync();
			((Control)SCIBusPCMWriteMemoryPartNumberWriteButton).Enabled = false;
		}
	}

	private void SCIBusPCMWriteMemoryPartNumberTextBox_KeyPress(object sender, KeyPressEventArgs e)
	{
		if (e.KeyChar == '\r')
		{
			e.Handled = true;
			if (((Control)SCIBusPCMWriteMemoryPartNumberWriteButton).Enabled)
			{
				SCIBusPCMWriteMemoryPartNumberWriteButton_Click(this, EventArgs.Empty);
			}
		}
	}

	private void SCIBusPCMWriteMemoryEEPROMOffsetAndCountAndValueTextBox_TextChanged(object sender, EventArgs e)
	{
		bool flag;
		if (((Control)SCIBusPCMWriteMemoryEEPROMOffsetTextBox).Text != string.Empty)
		{
			byte[] array = Util.HexStringToByte(((Control)SCIBusPCMWriteMemoryEEPROMOffsetTextBox).Text);
			flag = ((array != null && array.Length == 2) ? true : false);
		}
		else
		{
			flag = false;
		}
		uint.TryParse(((Control)SCIBusPCMWriteMemoryEEPROMValueCountTextBox).Text, out var result);
		bool flag2 = ((((Control)SCIBusPCMWriteMemoryEEPROMValueCountTextBox).Text.IsNumeric() && result != 0) ? true : false);
		bool flag3;
		if (((Control)SCIBusPCMWriteMemoryEEPROMValueTextBox).Text != string.Empty)
		{
			byte[] array2 = Util.HexStringToByte(((Control)SCIBusPCMWriteMemoryEEPROMValueTextBox).Text);
			flag3 = ((array2 != null && array2.Length != 0) ? true : false);
		}
		else
		{
			flag3 = false;
		}
		if (flag && flag2)
		{
			((Control)SCIBusPCMWriteMemoryEEPROMReadButton).Enabled = true;
		}
		else
		{
			((Control)SCIBusPCMWriteMemoryEEPROMReadButton).Enabled = false;
		}
		if (flag && flag3)
		{
			((Control)SCIBusPCMWriteMemoryEEPROMWriteButton).Enabled = true;
		}
		else
		{
			((Control)SCIBusPCMWriteMemoryEEPROMWriteButton).Enabled = false;
		}
	}

	private void SCIBusPCMWriteMemoryEEPROMReadButton_Click(object sender, EventArgs e)
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		if (OriginalForm.PCM.logic == "inverted")
		{
			MessageBox.Show("Not supported yet.", "Warning", (MessageBoxButtons)0, (MessageBoxIcon)48);
			return;
		}
		if (((Control)SCIBusPCMWriteMemoryEEPROMReadButton).Text == "Stop")
		{
			SCIBusPCMReadMemoryWorker.CancelAsync();
			((Control)SCIBusPCMWriteMemoryEEPROMReadButton).Text = "Read";
		}
		if (!SCIBusPCMReadMemoryWorker.IsBusy && !SCIBusPCMWriteMemoryWorker.IsBusy)
		{
			UpdateTextBox(SCIBusPCMWriteMemoryInfoTextBox, Environment.NewLine + Environment.NewLine + "Read EEPROM.");
			uint.TryParse(((Control)SCIBusPCMWriteMemoryEEPROMValueCountTextBox).Text, out var result);
			SCIBusPCMMemoryOffsetStartBytes = Util.HexStringToByte(((Control)SCIBusPCMWriteMemoryEEPROMOffsetTextBox).Text);
			SCIBusPCMMemoryOffsetStart = (uint)((SCIBusPCMMemoryOffsetStartBytes[0] << 8) | SCIBusPCMMemoryOffsetStartBytes[1]);
			SCIBusPCMMemoryOffsetEnd = SCIBusPCMMemoryOffsetStart + result - 1;
			SCIBusPCMMemoryOffsetEndBytes[0] = (byte)(SCIBusPCMMemoryOffsetEnd >> 8);
			SCIBusPCMMemoryOffsetEndBytes[1] = (byte)SCIBusPCMMemoryOffsetEnd;
			if (SCIBusPCMMemoryOffsetStart > 511)
			{
				SCIBusPCMMemoryOffsetStart = 511u;
				SCIBusPCMMemoryOffsetStartBytes[0] = (byte)(SCIBusPCMMemoryOffsetStart >> 8);
				SCIBusPCMMemoryOffsetStartBytes[1] = (byte)SCIBusPCMMemoryOffsetStart;
				((Control)SCIBusPCMWriteMemoryEEPROMOffsetTextBox).Text = Util.ByteToHexString(SCIBusPCMMemoryOffsetStartBytes, 0, SCIBusPCMMemoryOffsetStartBytes.Length);
				result = 1u;
				((Control)SCIBusPCMWriteMemoryEEPROMValueCountTextBox).Text = result.ToString();
				SCIBusPCMMemoryOffsetEnd = 511u;
				SCIBusPCMMemoryOffsetEndBytes[0] = (byte)(SCIBusPCMMemoryOffsetEnd >> 8);
				SCIBusPCMMemoryOffsetEndBytes[1] = (byte)SCIBusPCMMemoryOffsetEnd;
			}
			if (SCIBusPCMMemoryOffsetEnd > 511)
			{
				result = 511 - SCIBusPCMMemoryOffsetStart + 1;
				((Control)SCIBusPCMWriteMemoryEEPROMValueCountTextBox).Text = result.ToString();
				SCIBusPCMMemoryOffsetEnd = 511u;
				SCIBusPCMMemoryOffsetEndBytes[0] = (byte)(SCIBusPCMMemoryOffsetEnd >> 8);
				SCIBusPCMMemoryOffsetEndBytes[1] = (byte)SCIBusPCMMemoryOffsetEnd;
			}
			EEPROMBuffer = new byte[result];
			SCIBusPCMCurrentMemoryOffset = SCIBusPCMMemoryOffsetStart;
			SCIBusPCMCurrentMemoryOffsetBytes[0] = SCIBusPCMMemoryOffsetStartBytes[0];
			SCIBusPCMCurrentMemoryOffsetBytes[1] = SCIBusPCMMemoryOffsetStartBytes[1];
			SCIBusPCMTxPayload = new byte[3]
			{
				40,
				SCIBusPCMMemoryOffsetStartBytes[0],
				SCIBusPCMMemoryOffsetStartBytes[1]
			};
			((TextBoxBase)SCIBusPCMWriteMemoryEEPROMValueTextBox).Clear();
			((Control)SCIBusPCMWriteMemoryEEPROMReadButton).Text = "Stop";
			SCIBusPCMReadMemoryFinished = false;
			SCIBusPCMNextRequest = true;
			CurrentTask = Task.ReadEEPROM;
			SCIBusPCMReadMemoryWorker.RunWorkerAsync();
		}
	}

	private void SCIBusPCMWriteMemoryEEPROMWriteButton_Click(object sender, EventArgs e)
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		if (OriginalForm.PCM.logic == "inverted")
		{
			MessageBox.Show("Not supported yet.", "Warning", (MessageBoxButtons)0, (MessageBoxIcon)48);
			return;
		}
		if (((Control)SCIBusPCMWriteMemoryEEPROMWriteButton).Text == "Stop")
		{
			SCIBusPCMWriteMemoryWorker.CancelAsync();
			((Control)SCIBusPCMWriteMemoryEEPROMWriteButton).Text = "Write";
		}
		if (!SCIBusPCMReadMemoryWorker.IsBusy && !SCIBusPCMWriteMemoryWorker.IsBusy)
		{
			byte[] array = Util.HexStringToByte(((Control)SCIBusPCMWriteMemoryEEPROMValueTextBox).Text);
			uint num = (uint)array.Length;
			SCIBusPCMMemoryOffsetStartBytes = Util.HexStringToByte(((Control)SCIBusPCMWriteMemoryEEPROMOffsetTextBox).Text);
			SCIBusPCMMemoryOffsetStart = (uint)((SCIBusPCMMemoryOffsetStartBytes[0] << 8) | SCIBusPCMMemoryOffsetStartBytes[1]);
			SCIBusPCMMemoryOffsetEnd = SCIBusPCMMemoryOffsetStart + num - 1;
			SCIBusPCMMemoryOffsetEndBytes[0] = (byte)(SCIBusPCMMemoryOffsetEnd >> 8);
			SCIBusPCMMemoryOffsetEndBytes[1] = (byte)SCIBusPCMMemoryOffsetEnd;
			if (SCIBusPCMMemoryOffsetStart > 511)
			{
				SCIBusPCMMemoryOffsetStart = 511u;
				SCIBusPCMMemoryOffsetStartBytes[0] = (byte)(SCIBusPCMMemoryOffsetStart >> 8);
				SCIBusPCMMemoryOffsetStartBytes[1] = (byte)SCIBusPCMMemoryOffsetStart;
				((Control)SCIBusPCMWriteMemoryEEPROMOffsetTextBox).Text = Util.ByteToHexString(SCIBusPCMMemoryOffsetStartBytes, 0, SCIBusPCMMemoryOffsetStartBytes.Length);
				num = 1u;
				((Control)SCIBusPCMWriteMemoryEEPROMValueCountTextBox).Text = num.ToString();
				SCIBusPCMMemoryOffsetEnd = 511u;
				SCIBusPCMMemoryOffsetEndBytes[0] = (byte)(SCIBusPCMMemoryOffsetEnd >> 8);
				SCIBusPCMMemoryOffsetEndBytes[1] = (byte)SCIBusPCMMemoryOffsetEnd;
				((Control)SCIBusPCMWriteMemoryEEPROMValueTextBox).Text = Util.ByteToHexString(array, 0, (int)num);
			}
			if (SCIBusPCMMemoryOffsetEnd > 511)
			{
				num = 511 - SCIBusPCMMemoryOffsetStart + 1;
				((Control)SCIBusPCMWriteMemoryEEPROMValueCountTextBox).Text = num.ToString();
				SCIBusPCMMemoryOffsetEnd = 511u;
				SCIBusPCMMemoryOffsetEndBytes[0] = (byte)(SCIBusPCMMemoryOffsetEnd >> 8);
				SCIBusPCMMemoryOffsetEndBytes[1] = (byte)SCIBusPCMMemoryOffsetEnd;
				((Control)SCIBusPCMWriteMemoryEEPROMValueTextBox).Text = Util.ByteToHexString(array, 0, (int)num);
			}
			EEPROMBuffer = new byte[num];
			Array.Copy(array, EEPROMBuffer, num);
			SCIBusPCMCurrentMemoryOffset = SCIBusPCMMemoryOffsetStart;
			SCIBusPCMCurrentMemoryOffsetBytes[0] = SCIBusPCMMemoryOffsetStartBytes[0];
			SCIBusPCMCurrentMemoryOffsetBytes[1] = SCIBusPCMMemoryOffsetStartBytes[1];
			SCIBusPCMTxPayload = new byte[4]
			{
				39,
				SCIBusPCMMemoryOffsetStartBytes[0],
				SCIBusPCMMemoryOffsetStartBytes[1],
				EEPROMBuffer[0]
			};
			((Control)SCIBusPCMWriteMemoryEEPROMValueCountTextBox).Text = num.ToString();
			((Control)SCIBusPCMWriteMemoryEEPROMWriteButton).Text = "Stop";
			SCIBusPCMWriteMemoryFinished = false;
			SCIBusPCMNextRequest = true;
			CurrentTask = Task.WriteEEPROM;
			SCIBusPCMWriteMemoryWorker.RunWorkerAsync();
		}
	}

	private void SCIBusPCMWriteMemoryEEPROMBackupButton_Click(object sender, EventArgs e)
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		if (OriginalForm.PCM.logic == "inverted")
		{
			MessageBox.Show("Not supported yet.", "Warning", (MessageBoxButtons)0, (MessageBoxIcon)48);
			return;
		}
		if (((Control)SCIBusPCMWriteMemoryEEPROMBackupButton).Text == "Stop")
		{
			SCIBusPCMReadMemoryWorker.CancelAsync();
			((Control)SCIBusPCMWriteMemoryEEPROMBackupButton).Text = "BAK";
		}
		if (!SCIBusPCMReadMemoryWorker.IsBusy && !SCIBusPCMWriteMemoryWorker.IsBusy)
		{
			SCIBusPCMMemoryEEPROMBackupFilename = "ROMs/PCM/pcm_eeprom_backup_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".bin";
			UpdateTextBox(SCIBusPCMWriteMemoryInfoTextBox, Environment.NewLine + Environment.NewLine + "Backup EEPROM to \"" + SCIBusPCMMemoryEEPROMBackupFilename + "\" is in progress.");
			SCIBusPCMMemoryOffsetStart = 0u;
			SCIBusPCMMemoryOffsetStartBytes[0] = 0;
			SCIBusPCMMemoryOffsetStartBytes[1] = 0;
			SCIBusPCMMemoryOffsetEnd = 511u;
			SCIBusPCMMemoryOffsetEndBytes[0] = (byte)(SCIBusPCMMemoryOffsetEnd >> 8);
			SCIBusPCMMemoryOffsetEndBytes[1] = (byte)SCIBusPCMMemoryOffsetEnd;
			((Control)SCIBusPCMWriteMemoryEEPROMValueCountTextBox).Text = ((ushort)512).ToString();
			SCIBusPCMCurrentMemoryOffset = SCIBusPCMMemoryOffsetStart;
			SCIBusPCMCurrentMemoryOffsetBytes[0] = SCIBusPCMMemoryOffsetStartBytes[0];
			SCIBusPCMCurrentMemoryOffsetBytes[1] = SCIBusPCMMemoryOffsetStartBytes[1];
			SCIBusPCMTxPayload = new byte[3]
			{
				40,
				SCIBusPCMMemoryOffsetStartBytes[0],
				SCIBusPCMMemoryOffsetStartBytes[1]
			};
			((Control)SCIBusPCMWriteMemoryEEPROMBackupButton).Text = "Stop";
			SCIBusPCMReadMemoryFinished = false;
			SCIBusPCMNextRequest = true;
			CurrentTask = Task.BackupEEPROM;
			SCIBusPCMReadMemoryWorker.RunWorkerAsync();
		}
	}

	private void SCIBusPCMWriteMemoryEEPROMRestoreButton_Click(object sender, EventArgs e)
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Expected O, but got Unknown
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Invalid comparison between Unknown and I4
		//IL_0157: Unknown result type (might be due to invalid IL or missing references)
		if (OriginalForm.PCM.logic == "inverted")
		{
			MessageBox.Show("Not supported yet.", "Warning", (MessageBoxButtons)0, (MessageBoxIcon)48);
			return;
		}
		OpenFileDialog val = new OpenFileDialog();
		try
		{
			((FileDialog)val).InitialDirectory = Path.Combine(Application.StartupPath, "ROMs\\PCM");
			((FileDialog)val).Filter = "Binary files (*.bin)|*.bin|All files (*.*)|*.*";
			((FileDialog)val).FilterIndex = 1;
			((FileDialog)val).RestoreDirectory = true;
			if ((int)((CommonDialog)val).ShowDialog() != 1)
			{
				return;
			}
			using FileStream fileStream = File.Open(((FileDialog)val).FileName, FileMode.Open);
			using BinaryReader binaryReader = new BinaryReader(fileStream, Encoding.UTF8, leaveOpen: false);
			if (fileStream.Length == 512)
			{
				Packet packet = new Packet();
				packet.Bus = 0;
				packet.Command = 14;
				packet.Mode = 240;
				packet.Payload = binaryReader.ReadBytes(512);
				OriginalForm.TransmitUSBPacket("[<-TX] Restore PCM EEPROM from backup:", packet);
				SerialService.WritePacket(packet);
				UpdateTextBox(SCIBusPCMWriteMemoryInfoTextBox, Environment.NewLine + Environment.NewLine + "Restore EEPROM. In progress.");
				CurrentTask = Task.RestoreEEPROM;
			}
			else
			{
				MessageBox.Show("Incorrect file size (" + fileStream.Length + " bytes)!" + Environment.NewLine + "EEPROM backup file size should be 512 bytes.", "Warning", (MessageBoxButtons)0, (MessageBoxIcon)48);
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	private void SCIBusPCMWriteMemoryEEPROMOffsetTextBox_KeyPress(object sender, KeyPressEventArgs e)
	{
		if (e.KeyChar == '\r')
		{
			e.Handled = true;
			if (((Control)SCIBusPCMWriteMemoryEEPROMReadButton).Enabled)
			{
				SCIBusPCMWriteMemoryEEPROMReadButton_Click(this, EventArgs.Empty);
			}
		}
	}

	private void SCIBusPCMWriteMemoryEEPROMValueCountTextBox_KeyPress(object sender, KeyPressEventArgs e)
	{
		if (e.KeyChar == '\r')
		{
			e.Handled = true;
			if (((Control)SCIBusPCMWriteMemoryEEPROMReadButton).Enabled)
			{
				SCIBusPCMWriteMemoryEEPROMReadButton_Click(this, EventArgs.Empty);
			}
		}
	}

	private void SCIBusPCMWriteMemoryEEPROMValueTextBox_KeyPress(object sender, KeyPressEventArgs e)
	{
		if (e.KeyChar == '\r')
		{
			e.Handled = true;
			if (((Control)SCIBusPCMWriteMemoryEEPROMWriteButton).Enabled)
			{
				SCIBusPCMWriteMemoryEEPROMWriteButton_Click(this, EventArgs.Empty);
			}
		}
	}

	private void SCIBusPCMWriteMemoryRAMOffsetAndCountAndValueTextBox_TextChanged(object sender, EventArgs e)
	{
		bool flag;
		if (((Control)SCIBusPCMWriteMemoryRAMOffsetTextBox).Text != string.Empty)
		{
			byte[] array = Util.HexStringToByte(((Control)SCIBusPCMWriteMemoryRAMOffsetTextBox).Text);
			flag = ((array != null && array.Length == 2) ? true : false);
		}
		else
		{
			flag = false;
		}
		uint.TryParse(((Control)SCIBusPCMWriteMemoryRAMValueCountTextBox).Text, out var result);
		bool flag2 = ((((Control)SCIBusPCMWriteMemoryRAMValueCountTextBox).Text.IsNumeric() && result != 0) ? true : false);
		bool flag3;
		if (((Control)SCIBusPCMWriteMemoryRAMValueTextBox).Text != string.Empty)
		{
			byte[] array2 = Util.HexStringToByte(((Control)SCIBusPCMWriteMemoryRAMValueTextBox).Text);
			flag3 = ((array2 != null && array2.Length != 0) ? true : false);
		}
		else
		{
			flag3 = false;
		}
		if (flag && flag2)
		{
			((Control)SCIBusPCMWriteMemoryRAMReadButton).Enabled = true;
		}
		else
		{
			((Control)SCIBusPCMWriteMemoryRAMReadButton).Enabled = false;
		}
		if (flag && flag3)
		{
			((Control)SCIBusPCMWriteMemoryRAMWriteButton).Enabled = true;
		}
		else
		{
			((Control)SCIBusPCMWriteMemoryRAMWriteButton).Enabled = false;
		}
	}

	private void SCIBusPCMWriteMemoryRAMReadButton_Click(object sender, EventArgs e)
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		if (OriginalForm.PCM.logic == "inverted")
		{
			MessageBox.Show("Not supported yet.", "Warning", (MessageBoxButtons)0, (MessageBoxIcon)48);
			return;
		}
		if (((Control)SCIBusPCMWriteMemoryRAMReadButton).Text == "Stop")
		{
			SCIBusPCMReadMemoryWorker.CancelAsync();
			((Control)SCIBusPCMWriteMemoryRAMReadButton).Text = "Read";
		}
		if (!SCIBusPCMReadMemoryWorker.IsBusy && !SCIBusPCMWriteMemoryWorker.IsBusy)
		{
			uint.TryParse(((Control)SCIBusPCMWriteMemoryRAMValueCountTextBox).Text, out var result);
			UpdateTextBox(SCIBusPCMWriteMemoryInfoTextBox, Environment.NewLine + Environment.NewLine + "Read RAM.");
			SCIBusPCMMemoryOffsetStartBytes = Util.HexStringToByte(((Control)SCIBusPCMWriteMemoryRAMOffsetTextBox).Text);
			SCIBusPCMMemoryOffsetStart = (uint)((SCIBusPCMMemoryOffsetStartBytes[0] << 8) | SCIBusPCMMemoryOffsetStartBytes[1]);
			SCIBusPCMMemoryOffsetEnd = SCIBusPCMMemoryOffsetStart + result - 1;
			SCIBusPCMMemoryOffsetEndBytes[0] = (byte)(SCIBusPCMMemoryOffsetEnd >> 8);
			SCIBusPCMMemoryOffsetEndBytes[1] = (byte)SCIBusPCMMemoryOffsetEnd;
			if (SCIBusPCMMemoryOffsetStart > 6143)
			{
				SCIBusPCMMemoryOffsetStart = 6143u;
				SCIBusPCMMemoryOffsetStartBytes[0] = (byte)(SCIBusPCMMemoryOffsetStart >> 8);
				SCIBusPCMMemoryOffsetStartBytes[1] = (byte)SCIBusPCMMemoryOffsetStart;
				((Control)SCIBusPCMWriteMemoryRAMOffsetTextBox).Text = Util.ByteToHexString(SCIBusPCMMemoryOffsetStartBytes, 0, SCIBusPCMMemoryOffsetStartBytes.Length);
				result = 1u;
				((Control)SCIBusPCMWriteMemoryRAMValueCountTextBox).Text = result.ToString();
				SCIBusPCMMemoryOffsetEnd = 6143u;
				SCIBusPCMMemoryOffsetEndBytes[0] = (byte)(SCIBusPCMMemoryOffsetEnd >> 8);
				SCIBusPCMMemoryOffsetEndBytes[1] = (byte)SCIBusPCMMemoryOffsetEnd;
			}
			if (SCIBusPCMMemoryOffsetEnd > 6143)
			{
				result = 6143 - SCIBusPCMMemoryOffsetStart + 1;
				((Control)SCIBusPCMWriteMemoryRAMValueCountTextBox).Text = result.ToString();
				SCIBusPCMMemoryOffsetEnd = 6143u;
				SCIBusPCMMemoryOffsetEndBytes[0] = (byte)(SCIBusPCMMemoryOffsetEnd >> 8);
				SCIBusPCMMemoryOffsetEndBytes[1] = (byte)SCIBusPCMMemoryOffsetEnd;
			}
			RAMBuffer = new byte[result];
			SCIBusPCMCurrentMemoryOffset = SCIBusPCMMemoryOffsetStart;
			SCIBusPCMCurrentMemoryOffsetBytes[0] = SCIBusPCMMemoryOffsetStartBytes[0];
			SCIBusPCMCurrentMemoryOffsetBytes[1] = SCIBusPCMMemoryOffsetStartBytes[1];
			SCIBusPCMTxPayload = new byte[4]
			{
				38,
				15,
				(byte)(SCIBusPCMMemoryOffsetStartBytes[0] + 128),
				SCIBusPCMMemoryOffsetStartBytes[1]
			};
			((TextBoxBase)SCIBusPCMWriteMemoryRAMValueTextBox).Clear();
			((Control)SCIBusPCMWriteMemoryRAMReadButton).Text = "Stop";
			SCIBusPCMReadMemoryFinished = false;
			SCIBusPCMNextRequest = true;
			CurrentTask = Task.ReadRAM;
			SCIBusPCMReadMemoryWorker.RunWorkerAsync();
		}
	}

	private void SCIBusPCMWriteMemoryRAMWriteButton_Click(object sender, EventArgs e)
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		if (OriginalForm.PCM.logic == "inverted")
		{
			MessageBox.Show("Not supported yet.", "Warning", (MessageBoxButtons)0, (MessageBoxIcon)48);
			return;
		}
		if (((Control)SCIBusPCMWriteMemoryRAMWriteButton).Text == "Stop")
		{
			SCIBusPCMWriteMemoryWorker.CancelAsync();
			((Control)SCIBusPCMWriteMemoryRAMWriteButton).Text = "Write";
		}
		if (!SCIBusPCMReadMemoryWorker.IsBusy && !SCIBusPCMWriteMemoryWorker.IsBusy)
		{
			byte[] array = Util.HexStringToByte(((Control)SCIBusPCMWriteMemoryRAMValueTextBox).Text);
			uint num = (uint)array.Length;
			SCIBusPCMMemoryOffsetStartBytes = Util.HexStringToByte(((Control)SCIBusPCMWriteMemoryRAMOffsetTextBox).Text);
			SCIBusPCMMemoryOffsetStart = (uint)((SCIBusPCMMemoryOffsetStartBytes[0] << 8) | SCIBusPCMMemoryOffsetStartBytes[1]);
			SCIBusPCMMemoryOffsetEnd = SCIBusPCMMemoryOffsetStart + num - 1;
			SCIBusPCMMemoryOffsetEndBytes[0] = (byte)(SCIBusPCMMemoryOffsetEnd >> 8);
			SCIBusPCMMemoryOffsetEndBytes[1] = (byte)SCIBusPCMMemoryOffsetEnd;
			if (SCIBusPCMMemoryOffsetStart > 6143)
			{
				SCIBusPCMMemoryOffsetStart = 6143u;
				SCIBusPCMMemoryOffsetStartBytes[0] = (byte)(SCIBusPCMMemoryOffsetStart >> 8);
				SCIBusPCMMemoryOffsetStartBytes[1] = (byte)SCIBusPCMMemoryOffsetStart;
				((Control)SCIBusPCMWriteMemoryRAMOffsetTextBox).Text = Util.ByteToHexString(SCIBusPCMMemoryOffsetStartBytes, 0, SCIBusPCMMemoryOffsetStartBytes.Length);
				num = 1u;
				((Control)SCIBusPCMWriteMemoryRAMValueCountTextBox).Text = num.ToString();
				SCIBusPCMMemoryOffsetEnd = 6143u;
				SCIBusPCMMemoryOffsetEndBytes[0] = (byte)(SCIBusPCMMemoryOffsetEnd >> 8);
				SCIBusPCMMemoryOffsetEndBytes[1] = (byte)SCIBusPCMMemoryOffsetEnd;
				((Control)SCIBusPCMWriteMemoryRAMValueTextBox).Text = Util.ByteToHexString(array, 0, (int)num);
			}
			if (SCIBusPCMMemoryOffsetEnd > 6143)
			{
				num = 6143 - SCIBusPCMMemoryOffsetStart + 1;
				((Control)SCIBusPCMWriteMemoryRAMValueCountTextBox).Text = num.ToString();
				SCIBusPCMMemoryOffsetEnd = 6143u;
				SCIBusPCMMemoryOffsetEndBytes[0] = (byte)(SCIBusPCMMemoryOffsetEnd >> 8);
				SCIBusPCMMemoryOffsetEndBytes[1] = (byte)SCIBusPCMMemoryOffsetEnd;
				((Control)SCIBusPCMWriteMemoryRAMValueTextBox).Text = Util.ByteToHexString(array, 0, (int)num);
			}
			RAMBuffer = new byte[num];
			Array.Copy(array, RAMBuffer, num);
			SCIBusPCMCurrentMemoryOffset = SCIBusPCMMemoryOffsetStart;
			SCIBusPCMCurrentMemoryOffsetBytes[0] = SCIBusPCMMemoryOffsetStartBytes[0];
			SCIBusPCMCurrentMemoryOffsetBytes[1] = SCIBusPCMMemoryOffsetStartBytes[1];
			SCIBusPCMTxPayload = new byte[4]
			{
				41,
				SCIBusPCMMemoryOffsetStartBytes[0],
				SCIBusPCMMemoryOffsetStartBytes[1],
				RAMBuffer[0]
			};
			((Control)SCIBusPCMWriteMemoryRAMValueCountTextBox).Text = num.ToString();
			((Control)SCIBusPCMWriteMemoryRAMWriteButton).Text = "Stop";
			SCIBusPCMWriteMemoryFinished = false;
			SCIBusPCMNextRequest = true;
			CurrentTask = Task.WriteRAM;
			SCIBusPCMWriteMemoryWorker.RunWorkerAsync();
		}
	}

	private void SCIBusPCMWriteMemoryRAMBackupButton_Click(object sender, EventArgs e)
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		if (OriginalForm.PCM.logic == "inverted")
		{
			MessageBox.Show("Not supported yet.", "Warning", (MessageBoxButtons)0, (MessageBoxIcon)48);
			return;
		}
		if (((Control)SCIBusPCMWriteMemoryRAMBackupButton).Text == "Stop")
		{
			SCIBusPCMReadMemoryWorker.CancelAsync();
			((Control)SCIBusPCMWriteMemoryRAMBackupButton).Text = "BAK";
		}
		if (!SCIBusPCMReadMemoryWorker.IsBusy && !SCIBusPCMWriteMemoryWorker.IsBusy)
		{
			SCIBusPCMMemoryRAMBackupFilename = "ROMs/PCM/pcm_ram_backup_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".bin";
			UpdateTextBox(SCIBusPCMWriteMemoryInfoTextBox, Environment.NewLine + Environment.NewLine + "Backup RAM to \"" + SCIBusPCMMemoryRAMBackupFilename + "\" is in progress.");
			SCIBusPCMMemoryOffsetStart = 0u;
			SCIBusPCMMemoryOffsetStartBytes[0] = 0;
			SCIBusPCMMemoryOffsetStartBytes[1] = 0;
			SCIBusPCMMemoryOffsetEnd = 6143u;
			SCIBusPCMMemoryOffsetEndBytes[0] = (byte)(SCIBusPCMMemoryOffsetEnd >> 8);
			SCIBusPCMMemoryOffsetEndBytes[1] = (byte)SCIBusPCMMemoryOffsetEnd;
			((Control)SCIBusPCMWriteMemoryRAMValueCountTextBox).Text = ((ushort)6144).ToString();
			SCIBusPCMCurrentMemoryOffset = SCIBusPCMMemoryOffsetStart;
			SCIBusPCMCurrentMemoryOffsetBytes[0] = SCIBusPCMMemoryOffsetStartBytes[0];
			SCIBusPCMCurrentMemoryOffsetBytes[1] = SCIBusPCMMemoryOffsetStartBytes[1];
			SCIBusPCMTxPayload = new byte[4]
			{
				38,
				15,
				(byte)(SCIBusPCMMemoryOffsetStartBytes[0] + 128),
				SCIBusPCMMemoryOffsetStartBytes[1]
			};
			((Control)SCIBusPCMWriteMemoryRAMBackupButton).Text = "Stop";
			SCIBusPCMReadMemoryFinished = false;
			SCIBusPCMNextRequest = true;
			CurrentTask = Task.BackupRAM;
			SCIBusPCMReadMemoryWorker.RunWorkerAsync();
		}
	}

	private void SCIBusPCMWriteMemoryRAMRestoreButton_Click(object sender, EventArgs e)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		MessageBox.Show("Not supported.", "Warning", (MessageBoxButtons)0, (MessageBoxIcon)48);
	}

	private void SCIBusPCMWriteMemoryRAMOffsetTextBox_KeyPress(object sender, KeyPressEventArgs e)
	{
		if (e.KeyChar == '\r')
		{
			e.Handled = true;
			if (((Control)SCIBusPCMWriteMemoryRAMReadButton).Enabled)
			{
				SCIBusPCMWriteMemoryRAMReadButton_Click(this, EventArgs.Empty);
			}
		}
	}

	private void SCIBusPCMWriteMemoryRAMValueCountTextBox_KeyPress(object sender, KeyPressEventArgs e)
	{
		if (e.KeyChar == '\r')
		{
			e.Handled = true;
			if (((Control)SCIBusPCMWriteMemoryRAMReadButton).Enabled)
			{
				SCIBusPCMWriteMemoryRAMReadButton_Click(this, EventArgs.Empty);
			}
		}
	}

	private void SCIBusPCMWriteMemoryRAMValueTextBox_KeyPress(object sender, KeyPressEventArgs e)
	{
		if (e.KeyChar == '\r')
		{
			e.Handled = true;
			if (((Control)SCIBusPCMWriteMemoryRAMWriteButton).Enabled)
			{
				SCIBusPCMWriteMemoryRAMWriteButton_Click(this, EventArgs.Empty);
			}
		}
	}

	private void SCIBusPCMWriteMemoryCopyBCMMileageButton_Click(object sender, EventArgs e)
	{
		SRIMileageMi = CCDBCMMileageMi;
		SRIMileageKm = CCDBCMMileageKm;
		if (Settings.Default.Units == "imperial")
		{
			((Control)SCIBusPCMWriteMemorySRIMileageTextBox).Text = Math.Round(SRIMileageMi, 1).ToString("0.0");
			((TextBoxBase)SCIBusPCMWriteMemorySRIMileageTextBox).SelectionStart = ((Control)SCIBusPCMWriteMemorySRIMileageTextBox).Text.Length;
			((TextBoxBase)SCIBusPCMWriteMemorySRIMileageTextBox).ScrollToCaret();
		}
		else if (Settings.Default.Units == "metric")
		{
			((Control)SCIBusPCMWriteMemorySRIMileageTextBox).Text = Math.Round(SRIMileageKm, 1).ToString("0.0");
			((TextBoxBase)SCIBusPCMWriteMemorySRIMileageTextBox).SelectionStart = ((Control)SCIBusPCMWriteMemorySRIMileageTextBox).Text.Length;
			((TextBoxBase)SCIBusPCMWriteMemorySRIMileageTextBox).ScrollToCaret();
		}
		UpdateTextBox(SCIBusPCMWriteMemoryInfoTextBox, Environment.NewLine + Environment.NewLine + "Rounded BCM Mileage copied.");
		CCDBCMMileageReceived = false;
		((Control)SCIBusPCMWriteMemoryCopyBCMMileageButton).Enabled = false;
	}

	private void SCIBusPCMWriteMemoryHelpButton_Click(object sender, EventArgs e)
	{
		UpdateTextBox(SCIBusPCMWriteMemoryInfoTextBox, Environment.NewLine + Environment.NewLine + "------------HELP------------");
		UpdateTextBox(SCIBusPCMWriteMemoryInfoTextBox, Environment.NewLine + "Welcome to the SBEC3 PCM memory manipulator.");
		UpdateTextBox(SCIBusPCMWriteMemoryInfoTextBox, Environment.NewLine + Environment.NewLine + "SRI Mileage:" + Environment.NewLine + "Low-resolution mileage. When entered manually it is rounded to the nearest multiple of 8.192 miles. If the scanner receives mileage from BCM via CCD-bus then the GUI offers an option to copy it.");
		UpdateTextBox(SCIBusPCMWriteMemoryInfoTextBox, Environment.NewLine + Environment.NewLine + "SKIM:" + Environment.NewLine + "Fuel shutoff settings.");
		UpdateTextBox(SCIBusPCMWriteMemoryInfoTextBox, Environment.NewLine + Environment.NewLine + "VIN:" + Environment.NewLine + "Vehicle identification number.");
		UpdateTextBox(SCIBusPCMWriteMemoryInfoTextBox, Environment.NewLine + Environment.NewLine + "Part Number:" + Environment.NewLine + "Redundant flash part number. Its value does not seem to affect the PCM.");
		UpdateTextBox(SCIBusPCMWriteMemoryInfoTextBox, Environment.NewLine + Environment.NewLine + "EEPROM Offset/n/Value(s):" + Environment.NewLine + "Advanced option to read and write at arbitrary positions in the EEPROM. Be sure to backup EEPROM with the \"BAK\" button to avoid damage. The binary backup file is saved to the \"ROMs\" folder. Cycle ignition key for changes in EEPROM to take effect.");
		UpdateTextBox(SCIBusPCMWriteMemoryInfoTextBox, Environment.NewLine + Environment.NewLine + "RAM Offset/n/Value(s):" + Environment.NewLine + "Advanced option to read and write at arbitrary positions in the RAM. The \"BAK\" button dumps its content in a binary file to the \"ROMs\" folder. Be cautious changing RAM values because they are directly responsible for PCM behavior. Changes made in RAM are taking effect immediately. RAM reading may not be possible for all SBEC3 PCMs due to limitations (lack of backdoor in firmware).");
		UpdateTextBox(SCIBusPCMWriteMemoryInfoTextBox, Environment.NewLine + Environment.NewLine + "Pressing the Enter key at various textboxes causes the related read/write function to be executed. Multiple error-checking measures are implemented to avoid writing bogus data.");
		UpdateTextBox(SCIBusPCMWriteMemoryInfoTextBox, Environment.NewLine + "----------------------------");
	}

	private void SCIBusPCMReadMemory_DoWork(object sender, DoWorkEventArgs e)
	{
		while (!SCIBusPCMReadMemoryFinished)
		{
			Thread.Sleep(1);
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
			if (SCIBusPCMCurrentMemoryOffset > SCIBusPCMMemoryOffsetEnd)
			{
				SCIBusPCMReadMemoryFinished = true;
			}
		}
	}

	private void SCIBusPCMReadMemory_ProgressChanged(object sender, ProgressChangedEventArgs e)
	{
		if (e.ProgressPercentage == 0)
		{
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
			UpdateTextBox(SCIBusPCMWriteMemoryInfoTextBox, " Cancelled.");
			switch (CurrentTask)
			{
			case Task.ReadSRIMileage:
				((Control)SCIBusPCMWriteMemorySRIMileageReadButton).Enabled = true;
				break;
			case Task.ReadSKIMVTSS:
				((Control)SCIBusPCMWriteMemorySKIMVTSSReadButton).Enabled = true;
				break;
			case Task.ReadVIN:
				((Control)SCIBusPCMWriteMemoryVINReadButton).Enabled = true;
				break;
			case Task.ReadPartNumber:
				((Control)SCIBusPCMWriteMemoryPartNumberReadButton).Enabled = true;
				break;
			case Task.ReadEEPROM:
				((Control)SCIBusPCMWriteMemoryEEPROMReadButton).Enabled = true;
				break;
			case Task.ReadRAM:
				((Control)SCIBusPCMWriteMemoryRAMReadButton).Enabled = true;
				break;
			case Task.BackupEEPROM:
				((Control)SCIBusPCMWriteMemoryEEPROMBackupButton).Text = "BAK";
				break;
			case Task.BackupRAM:
				((Control)SCIBusPCMWriteMemoryRAMBackupButton).Text = "BAK";
				break;
			}
			if (SCIBusPCMRxTimeout)
			{
				UpdateTextBox(SCIBusPCMWriteMemoryInfoTextBox, Environment.NewLine + "Memory read timeout (RX).");
			}
			if (SCIBusPCMTxTimeout)
			{
				UpdateTextBox(SCIBusPCMWriteMemoryInfoTextBox, Environment.NewLine + "Memory read timeout (TX).");
			}
		}
		else
		{
			switch (CurrentTask)
			{
			case Task.ReadSRIMileage:
				SRIMileageRaw = (uint)((SRIMileage[0] << 8) | SRIMileage[1]);
				SRIMileageMi = (double)SRIMileageRaw * 8.192;
				SRIMileageKm = SRIMileageMi * 1.609344;
				if (Settings.Default.Units == "imperial")
				{
					UpdateTextBox(SCIBusPCMWriteMemoryInfoTextBox, " Done." + Environment.NewLine + "SRI Mileage: " + Math.Round(SRIMileageMi, 1).ToString("0.0") + " mi");
					((Control)SCIBusPCMWriteMemorySRIMileageTextBox).Text = Math.Round(SRIMileageMi, 1).ToString("0.0");
					((TextBoxBase)SCIBusPCMWriteMemorySRIMileageTextBox).SelectionStart = ((Control)SCIBusPCMWriteMemorySRIMileageTextBox).Text.Length;
					((TextBoxBase)SCIBusPCMWriteMemorySRIMileageTextBox).ScrollToCaret();
				}
				else if (Settings.Default.Units == "metric")
				{
					UpdateTextBox(SCIBusPCMWriteMemoryInfoTextBox, " Done." + Environment.NewLine + "SRI Mileage: " + Math.Round(SRIMileageKm, 1).ToString("0.0") + " km");
					((Control)SCIBusPCMWriteMemorySRIMileageTextBox).Text = Math.Round(SRIMileageKm, 1).ToString("0.0");
					((TextBoxBase)SCIBusPCMWriteMemorySRIMileageTextBox).SelectionStart = ((Control)SCIBusPCMWriteMemorySRIMileageTextBox).Text.Length;
					((TextBoxBase)SCIBusPCMWriteMemorySRIMileageTextBox).ScrollToCaret();
				}
				((Control)SCIBusPCMWriteMemorySRIMileageReadButton).Enabled = true;
				break;
			case Task.ReadSKIMVTSS:
				UpdateTextBox(SCIBusPCMWriteMemoryInfoTextBox, " Done." + Environment.NewLine + "SKIM status: ");
				switch (SKIMVTSS)
				{
				case byte.MaxValue:
					UpdateTextBox(SCIBusPCMWriteMemoryInfoTextBox, "Fuel enabled.");
					((ListControl)SCIBusPCMWriteMemorySKIMVTSSComboBox).SelectedIndex = 0;
					break;
				case 0:
					UpdateTextBox(SCIBusPCMWriteMemoryInfoTextBox, "Fuel disabled.");
					((ListControl)SCIBusPCMWriteMemorySKIMVTSSComboBox).SelectedIndex = 1;
					break;
				default:
					UpdateTextBox(SCIBusPCMWriteMemoryInfoTextBox, "Unknown.");
					((ListControl)SCIBusPCMWriteMemorySKIMVTSSComboBox).SelectedIndex = 2;
					break;
				}
				((Control)SCIBusPCMWriteMemorySKIMVTSSReadButton).Enabled = true;
				break;
			case Task.ReadVIN:
				UpdateTextBox(SCIBusPCMWriteMemoryInfoTextBox, " Done." + Environment.NewLine + "VIN: " + Encoding.ASCII.GetString(VIN, 0, VIN.Length));
				((Control)SCIBusPCMWriteMemoryVINTextBox).Text = Encoding.ASCII.GetString(VIN, 0, VIN.Length);
				((TextBoxBase)SCIBusPCMWriteMemoryVINTextBox).SelectionStart = ((Control)SCIBusPCMWriteMemoryVINTextBox).Text.Length;
				((TextBoxBase)SCIBusPCMWriteMemoryVINTextBox).ScrollToCaret();
				((Control)SCIBusPCMWriteMemoryVINReadButton).Enabled = true;
				break;
			case Task.ReadPartNumber:
				if (PartNumberBuffer[0] != byte.MaxValue)
				{
					PartNumberLocation = 482;
					PartNumber[0] = PartNumberBuffer[0];
					PartNumber[1] = PartNumberBuffer[1];
					PartNumber[2] = PartNumberBuffer[2];
					PartNumber[3] = PartNumberBuffer[3];
					PartNumber[4] = PartNumberBuffer[7];
					PartNumber[5] = PartNumberBuffer[8];
				}
				else if (PartNumberBuffer[14] != byte.MaxValue)
				{
					PartNumberLocation = 496;
					PartNumber[0] = PartNumberBuffer[14];
					PartNumber[1] = PartNumberBuffer[15];
					PartNumber[2] = PartNumberBuffer[16];
					PartNumber[3] = PartNumberBuffer[17];
					PartNumber[4] = 0;
					PartNumber[5] = 0;
				}
				else
				{
					PartNumberLocation = 0;
					PartNumber[0] = 0;
					PartNumber[1] = 0;
					PartNumber[2] = 0;
					PartNumber[3] = 0;
					PartNumber[4] = 0;
					PartNumber[5] = 0;
				}
				if (PartNumber[0] != 0)
				{
					string text = Util.ByteToHexString(PartNumber, 0, 4).Replace(" ", "");
					if (PartNumber[4] != 0 && PartNumber[5] != 0)
					{
						text += Encoding.ASCII.GetString(PartNumber, 4, 2);
					}
					UpdateTextBox(SCIBusPCMWriteMemoryInfoTextBox, " Done." + Environment.NewLine + "Part Number: " + text);
					((Control)SCIBusPCMWriteMemoryPartNumberTextBox).Text = text;
					((TextBoxBase)SCIBusPCMWriteMemoryPartNumberTextBox).SelectionStart = ((Control)SCIBusPCMWriteMemoryPartNumberTextBox).Text.Length;
					((TextBoxBase)SCIBusPCMWriteMemoryPartNumberTextBox).ScrollToCaret();
					((Control)SCIBusPCMWriteMemoryPartNumberReadButton).Enabled = true;
				}
				else
				{
					UpdateTextBox(SCIBusPCMWriteMemoryInfoTextBox, Environment.NewLine + "Cannot find Part Number.");
				}
				break;
			case Task.ReadEEPROM:
				UpdateTextBox(SCIBusPCMWriteMemoryInfoTextBox, " Done.");
				UpdateTextBox(SCIBusPCMWriteMemoryInfoTextBox, Environment.NewLine + "Offset: " + Util.ByteToHexStringSimple(SCIBusPCMMemoryOffsetStartBytes) + " | Count: " + EEPROMBuffer.Length);
				UpdateTextBox(SCIBusPCMWriteMemoryInfoTextBox, Environment.NewLine + "Result:");
				UpdateTextBox(SCIBusPCMWriteMemoryInfoTextBox, Environment.NewLine + Util.ByteToHexStringSimple(EEPROMBuffer, 8));
				((Control)SCIBusPCMWriteMemoryEEPROMValueTextBox).Text = Util.ByteToHexString(EEPROMBuffer, 0, EEPROMBuffer.Length, EEPROMBuffer.Length);
				((TextBoxBase)SCIBusPCMWriteMemoryEEPROMValueTextBox).SelectionStart = 0;
				((TextBoxBase)SCIBusPCMWriteMemoryEEPROMValueTextBox).ScrollToCaret();
				((Control)SCIBusPCMWriteMemoryEEPROMReadButton).Text = "Read";
				break;
			case Task.ReadRAM:
				UpdateTextBox(SCIBusPCMWriteMemoryInfoTextBox, " Done.");
				UpdateTextBox(SCIBusPCMWriteMemoryInfoTextBox, Environment.NewLine + "Offset: " + Util.ByteToHexStringSimple(SCIBusPCMMemoryOffsetStartBytes) + " | Count: " + RAMBuffer.Length);
				UpdateTextBox(SCIBusPCMWriteMemoryInfoTextBox, Environment.NewLine + "Result:");
				UpdateTextBox(SCIBusPCMWriteMemoryInfoTextBox, Environment.NewLine + Util.ByteToHexStringSimple(RAMBuffer, 8));
				((Control)SCIBusPCMWriteMemoryRAMValueTextBox).Text = Util.ByteToHexString(RAMBuffer, 0, RAMBuffer.Length, RAMBuffer.Length);
				((TextBoxBase)SCIBusPCMWriteMemoryRAMValueTextBox).SelectionStart = 0;
				((TextBoxBase)SCIBusPCMWriteMemoryRAMValueTextBox).ScrollToCaret();
				((Control)SCIBusPCMWriteMemoryRAMReadButton).Text = "Read";
				break;
			case Task.BackupEEPROM:
			{
				UpdateTextBox(SCIBusPCMWriteMemoryInfoTextBox, " Done.");
				((Control)SCIBusPCMWriteMemoryEEPROMBackupButton).Text = "BAK";
				using (BinaryWriter binaryWriter2 = new BinaryWriter(File.Open(SCIBusPCMMemoryEEPROMBackupFilename, FileMode.Open)))
				{
					if (binaryWriter2.BaseStream.Length != 512)
					{
						UpdateTextBox(SCIBusPCMWriteMemoryInfoTextBox, Environment.NewLine + "Incorrect file size!");
					}
				}
				break;
			}
			case Task.BackupRAM:
			{
				UpdateTextBox(SCIBusPCMWriteMemoryInfoTextBox, " Done.");
				((Control)SCIBusPCMWriteMemoryRAMBackupButton).Text = "BAK";
				using (BinaryWriter binaryWriter = new BinaryWriter(File.Open(SCIBusPCMMemoryRAMBackupFilename, FileMode.Open)))
				{
					if (binaryWriter.BaseStream.Length != 6144)
					{
						UpdateTextBox(SCIBusPCMWriteMemoryInfoTextBox, Environment.NewLine + "Incorrect file size!");
					}
				}
				break;
			}
			}
		}
		SCIBusPCMRxTimeout = false;
		SCIBusPCMRxTimeoutTimer.Stop();
		SCIBusPCMTxTimeout = false;
		SCIBusPCMTxTimeoutTimer.Stop();
		CurrentTask = Task.None;
		SCIBusPCMTxPayload = null;
	}

	private void SCIBusPCMWriteMemory_DoWork(object sender, DoWorkEventArgs e)
	{
		while (!SCIBusPCMWriteMemoryFinished)
		{
			Thread.Sleep(1);
			if (SCIBusPCMWriteMemoryWorker.CancellationPending)
			{
				e.Cancel = true;
				break;
			}
			while (!SCIBusPCMNextRequest)
			{
				Thread.Sleep(1);
				if (SCIBusPCMWriteMemoryWorker.CancellationPending)
				{
					e.Cancel = true;
					break;
				}
			}
			SCIBusPCMResponse = false;
			SCIBusPCMNextRequest = false;
			SCIBusPCMWriteMemoryWorker.ReportProgress(0);
			while ((!PCMUnlocked && !SCIBusPCMRxTimeout) || (!SCIBusPCMResponse && !SCIBusPCMRxTimeout))
			{
				Thread.Sleep(1);
				if (SCIBusPCMWriteMemoryWorker.CancellationPending)
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
			if (SCIBusPCMCurrentMemoryOffset > SCIBusPCMMemoryOffsetEnd)
			{
				SCIBusPCMWriteMemoryFinished = true;
			}
		}
	}

	private void SCIBusPCMWriteMemory_ProgressChanged(object sender, ProgressChangedEventArgs e)
	{
		if (e.ProgressPercentage == 0)
		{
			Packet packet = new Packet();
			if (!PCMUnlocked)
			{
				packet.Bus = 2;
				packet.Command = 6;
				packet.Mode = 2;
				packet.Payload = new byte[1] { 43 };
			}
			else
			{
				packet.Bus = 2;
				packet.Command = 6;
				packet.Mode = 2;
				packet.Payload = SCIBusPCMTxPayload;
			}
			OriginalForm.TransmitUSBPacket("[<-TX] Send an SCI-bus (PCM) message once:", packet);
			SerialService.WritePacket(packet);
			SCIBusPCMRxTimeout = false;
			SCIBusPCMRxTimeoutTimer.Stop();
			SCIBusPCMRxTimeoutTimer.Start();
		}
	}

	private void SCIBusPCMWriteMemory_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
	{
		if (e.Cancelled)
		{
			switch (CurrentTask)
			{
			case Task.WriteSRIMileage:
				UpdateTextBox(SCIBusPCMWriteMemoryInfoTextBox, Environment.NewLine + Environment.NewLine + "Write SRI Mileage. Cancelled.");
				SCIBusPCMWriteMemorySRIMileageTextBox_TextChanged(this, EventArgs.Empty);
				break;
			case Task.WriteSKIMVTSS:
				UpdateTextBox(SCIBusPCMWriteMemoryInfoTextBox, Environment.NewLine + Environment.NewLine + "Write SKIM status. Cancelled.");
				SCIBusPCMWriteMemorySKIMVTSSComboBox_SelectedIndexChanged(this, EventArgs.Empty);
				break;
			case Task.WriteVIN:
				UpdateTextBox(SCIBusPCMWriteMemoryInfoTextBox, Environment.NewLine + Environment.NewLine + "Write VIN. Cancelled.");
				SCIBusPCMWriteMemoryVINTextBox_TextChanged(this, EventArgs.Empty);
				break;
			case Task.WritePartNumber:
				UpdateTextBox(SCIBusPCMWriteMemoryInfoTextBox, Environment.NewLine + Environment.NewLine + "Write Part Number. Cancelled.");
				SCIBusPCMWriteMemoryPartNumberTextBox_TextChanged(this, EventArgs.Empty);
				break;
			case Task.WriteEEPROM:
				UpdateTextBox(SCIBusPCMWriteMemoryInfoTextBox, Environment.NewLine + Environment.NewLine + "Write EEPROM. Cancelled.");
				SCIBusPCMWriteMemoryEEPROMOffsetAndCountAndValueTextBox_TextChanged(this, EventArgs.Empty);
				break;
			case Task.WriteRAM:
				UpdateTextBox(SCIBusPCMWriteMemoryInfoTextBox, Environment.NewLine + Environment.NewLine + "Write RAM. Cancelled.");
				SCIBusPCMWriteMemoryRAMOffsetAndCountAndValueTextBox_TextChanged(this, EventArgs.Empty);
				break;
			}
			if (SCIBusPCMRxTimeout)
			{
				UpdateTextBox(SCIBusPCMWriteMemoryInfoTextBox, Environment.NewLine + "Memory write timeout (RX).");
			}
			if (SCIBusPCMTxTimeout)
			{
				UpdateTextBox(SCIBusPCMWriteMemoryInfoTextBox, Environment.NewLine + "Memory write timeout (TX).");
			}
		}
		else
		{
			switch (CurrentTask)
			{
			case Task.WriteSRIMileage:
				UpdateTextBox(SCIBusPCMWriteMemoryInfoTextBox, Environment.NewLine + Environment.NewLine + "Write SRI Mileage. Done.");
				if (Settings.Default.Units == "imperial")
				{
					UpdateTextBox(SCIBusPCMWriteMemoryInfoTextBox, Environment.NewLine + "New SRI Mileage: " + Math.Round(SRIMileageMi, 1).ToString("0.0") + " mi");
				}
				else if (Settings.Default.Units == "metric")
				{
					UpdateTextBox(SCIBusPCMWriteMemoryInfoTextBox, Environment.NewLine + "New SRI Mileage: " + Math.Round(SRIMileageKm, 1).ToString("0.0") + " km");
				}
				SCIBusPCMWriteMemorySRIMileageTextBox_TextChanged(this, EventArgs.Empty);
				break;
			case Task.WriteSKIMVTSS:
				UpdateTextBox(SCIBusPCMWriteMemoryInfoTextBox, Environment.NewLine + Environment.NewLine + "Write SKIM status. Done.");
				SCIBusPCMWriteMemorySKIMVTSSComboBox_SelectedIndexChanged(this, EventArgs.Empty);
				break;
			case Task.WriteVIN:
				UpdateTextBox(SCIBusPCMWriteMemoryInfoTextBox, Environment.NewLine + Environment.NewLine + "Write VIN. Done.");
				UpdateTextBox(SCIBusPCMWriteMemoryInfoTextBox, Environment.NewLine + "New VIN: " + VINString);
				SCIBusPCMWriteMemoryVINTextBox_TextChanged(this, EventArgs.Empty);
				break;
			case Task.WritePartNumber:
			{
				UpdateTextBox(SCIBusPCMWriteMemoryInfoTextBox, Environment.NewLine + Environment.NewLine + "Write Part Number. Done.");
				string text = Util.ByteToHexString(PartNumber, 0, 4).Replace(" ", "");
				if (PartNumberLocation == 482 && PartNumber[4] != 0 && PartNumber[5] != 0)
				{
					text += Encoding.ASCII.GetString(PartNumber, 4, 2);
				}
				UpdateTextBox(SCIBusPCMWriteMemoryInfoTextBox, Environment.NewLine + "New Part Number: " + text);
				((Control)SCIBusPCMWriteMemoryPartNumberTextBox).Text = text;
				((TextBoxBase)SCIBusPCMWriteMemoryPartNumberTextBox).SelectionStart = ((Control)SCIBusPCMWriteMemoryPartNumberTextBox).Text.Length;
				((TextBoxBase)SCIBusPCMWriteMemoryPartNumberTextBox).ScrollToCaret();
				SCIBusPCMWriteMemoryPartNumberTextBox_TextChanged(this, EventArgs.Empty);
				break;
			}
			case Task.WriteEEPROM:
				UpdateTextBox(SCIBusPCMWriteMemoryInfoTextBox, Environment.NewLine + Environment.NewLine + "Write EEPROM. Done.");
				UpdateTextBox(SCIBusPCMWriteMemoryInfoTextBox, Environment.NewLine + "Offset: " + Util.ByteToHexStringSimple(SCIBusPCMMemoryOffsetStartBytes) + " | Count: " + EEPROMBuffer.Length);
				UpdateTextBox(SCIBusPCMWriteMemoryInfoTextBox, Environment.NewLine + "Result:");
				UpdateTextBox(SCIBusPCMWriteMemoryInfoTextBox, Environment.NewLine + Util.ByteToHexStringSimple(EEPROMBuffer, 8));
				((Control)SCIBusPCMWriteMemoryEEPROMWriteButton).Text = "Write";
				SCIBusPCMWriteMemoryEEPROMOffsetAndCountAndValueTextBox_TextChanged(this, EventArgs.Empty);
				break;
			case Task.WriteRAM:
				UpdateTextBox(SCIBusPCMWriteMemoryInfoTextBox, Environment.NewLine + Environment.NewLine + "Write RAM. Done.");
				UpdateTextBox(SCIBusPCMWriteMemoryInfoTextBox, Environment.NewLine + "Offset: " + Util.ByteToHexStringSimple(SCIBusPCMMemoryOffsetStartBytes) + " | Count: " + RAMBuffer.Length);
				UpdateTextBox(SCIBusPCMWriteMemoryInfoTextBox, Environment.NewLine + "Result:");
				UpdateTextBox(SCIBusPCMWriteMemoryInfoTextBox, Environment.NewLine + Util.ByteToHexStringSimple(RAMBuffer, 8));
				((Control)SCIBusPCMWriteMemoryRAMWriteButton).Text = "Write";
				SCIBusPCMWriteMemoryRAMOffsetAndCountAndValueTextBox_TextChanged(this, EventArgs.Empty);
				break;
			}
		}
		SCIBusPCMRxTimeout = false;
		SCIBusPCMRxTimeoutTimer.Stop();
		SCIBusPCMTxTimeout = false;
		SCIBusPCMTxTimeoutTimer.Stop();
		CurrentTask = Task.None;
		SCIBusPCMTxPayload = null;
	}

	private void PacketReceivedHandler(object sender, Packet packet)
	{
		UIContext.Post(delegate
		{
			_ = packet.Payload.Length;
			_ = 4;
			if (packet.Bus == 0 && packet.Command == 14 && packet.Mode == 240)
			{
				if (packet.Payload[0] == 0)
				{
					UpdateTextBox(SCIBusPCMWriteMemoryInfoTextBox, Environment.NewLine + Environment.NewLine + "Restore EEPROM. Done.");
				}
				else
				{
					UpdateTextBox(SCIBusPCMWriteMemoryInfoTextBox, Environment.NewLine + Environment.NewLine + "Restore EEPROM. Error [" + Util.ByteToHexStringSimple(new byte[1] { packet.Payload[0] }) + "].");
				}
				CurrentTask = Task.None;
			}
			if (packet.Bus == 1)
			{
				byte[] array = packet.Payload.Skip(4).ToArray();
				if (array[0] == 206 && array.Length == 6 && !CCDBCMMileageReceived)
				{
					CCDBCMMileage = array.Skip(1).Take(4).ToArray();
					CCDBCMMileageMi = Math.Round((double)(uint)((array[1] << 24) | (array[2] << 16) | (array[3] << 8) | array[4]) * 0.000125, 1);
					CCDBCMMileageKm = Math.Round(CCDBCMMileageMi * 1.609344, 1);
					if (Settings.Default.Units == "imperial")
					{
						UpdateTextBox(SCIBusPCMWriteMemoryInfoTextBox, Environment.NewLine + Environment.NewLine + "BCM Mileage: " + CCDBCMMileageMi.ToString("0.0") + " mi");
					}
					else if (Settings.Default.Units == "metric")
					{
						UpdateTextBox(SCIBusPCMWriteMemoryInfoTextBox, Environment.NewLine + Environment.NewLine + "BCM Mileage: " + CCDBCMMileageKm.ToString("0.0") + " km");
					}
					CCDBCMMileageReceived = true;
					((Control)SCIBusPCMWriteMemoryCopyBCMMileageButton).Enabled = true;
				}
			}
			if (packet.Bus == 2)
			{
				byte[] array2 = packet.Payload.Skip(4).ToArray();
				switch (array2[0])
				{
				case 21:
					if (array2.Length >= 4)
					{
						uint num2 = SCIBusPCMCurrentMemoryOffset - SCIBusPCMMemoryOffsetStart;
						switch (CurrentTask)
						{
						case Task.ReadSRIMileage:
							SRIMileage[num2] = array2[3];
							break;
						case Task.ReadVIN:
							VIN[num2] = array2[3];
							break;
						case Task.ReadEEPROM:
							EEPROMBuffer[num2] = array2[3];
							break;
						case Task.BackupEEPROM:
						{
							using (BinaryWriter binaryWriter = new BinaryWriter(File.Open(SCIBusPCMMemoryEEPROMBackupFilename, FileMode.Append)))
							{
								binaryWriter.Write(array2[3]);
								binaryWriter.Close();
							}
							((Control)SCIBusPCMWriteMemoryEEPROMOffsetTextBox).Text = Util.ByteToHexString(array2, 1, 2);
							((Control)SCIBusPCMWriteMemoryEEPROMValueTextBox).Text = Util.ByteToHexString(array2, 3);
							break;
						}
						}
						SCIBusPCMResponse = true;
						SCIBusPCMCurrentMemoryOffset++;
						SCIBusPCMCurrentMemoryOffsetBytes[0] = (byte)(SCIBusPCMCurrentMemoryOffset >> 8);
						SCIBusPCMCurrentMemoryOffsetBytes[1] = (byte)SCIBusPCMCurrentMemoryOffset;
						SCIBusPCMTxPayload = new byte[3]
						{
							21,
							SCIBusPCMCurrentMemoryOffsetBytes[0],
							SCIBusPCMCurrentMemoryOffsetBytes[1]
						};
					}
					break;
				case 38:
					if (array2.Length > 4)
					{
						uint num3 = SCIBusPCMCurrentMemoryOffset - SCIBusPCMMemoryOffsetStart;
						switch (CurrentTask)
						{
						case Task.ReadRAM:
							RAMBuffer[num3] = array2[4];
							break;
						case Task.BackupRAM:
						{
							using (BinaryWriter binaryWriter2 = new BinaryWriter(File.Open(SCIBusPCMMemoryRAMBackupFilename, FileMode.Append)))
							{
								binaryWriter2.Write(array2[4]);
								binaryWriter2.Close();
							}
							((Control)SCIBusPCMWriteMemoryRAMOffsetTextBox).Text = Util.ByteToHexString(SCIBusPCMCurrentMemoryOffsetBytes, 0, 2);
							((Control)SCIBusPCMWriteMemoryRAMValueTextBox).Text = Util.ByteToHexString(array2, 4);
							break;
						}
						}
						SCIBusPCMResponse = true;
						SCIBusPCMCurrentMemoryOffset++;
						SCIBusPCMCurrentMemoryOffsetBytes[0] = (byte)(SCIBusPCMCurrentMemoryOffset >> 8);
						SCIBusPCMCurrentMemoryOffsetBytes[1] = (byte)SCIBusPCMCurrentMemoryOffset;
						SCIBusPCMTxPayload = new byte[4]
						{
							38,
							15,
							(byte)(SCIBusPCMCurrentMemoryOffsetBytes[0] + 128),
							SCIBusPCMCurrentMemoryOffsetBytes[1]
						};
					}
					break;
				case 39:
					if (array2.Length >= 5)
					{
						switch (array2[4])
						{
						case 226:
						{
							SCIBusPCMResponse = true;
							SCIBusPCMCurrentMemoryOffset++;
							SCIBusPCMCurrentMemoryOffsetBytes[0] = (byte)(SCIBusPCMCurrentMemoryOffset >> 8);
							SCIBusPCMCurrentMemoryOffsetBytes[1] = (byte)SCIBusPCMCurrentMemoryOffset;
							uint num4 = SCIBusPCMCurrentMemoryOffset - SCIBusPCMMemoryOffsetStart;
							byte b2 = 0;
							switch (CurrentTask)
							{
							case Task.WriteSRIMileage:
								if (num4 >= SRIMileageNew.Length)
								{
									num4 = (uint)(SRIMileageNew.Length - 1);
								}
								b2 = SRIMileageNew[num4];
								break;
							case Task.WriteVIN:
								if (num4 >= VIN.Length)
								{
									num4 = (uint)(VIN.Length - 1);
								}
								b2 = VIN[num4];
								break;
							case Task.WritePartNumber:
								if (num4 >= PartNumber.Length)
								{
									num4 = (uint)(PartNumber.Length - 1);
								}
								if (SCIBusPCMCurrentMemoryOffset == 486 && PartNumberLocation == 482 && PartNumber[4] != 0 && PartNumber[5] != 0)
								{
									SCIBusPCMMemoryOffsetStart += 3;
									SCIBusPCMMemoryOffsetStartBytes[0] = (byte)(SCIBusPCMMemoryOffsetStart >> 8);
									SCIBusPCMMemoryOffsetStartBytes[1] = (byte)SCIBusPCMMemoryOffsetStart;
									SCIBusPCMCurrentMemoryOffset += 3;
									SCIBusPCMCurrentMemoryOffsetBytes[0] = (byte)(SCIBusPCMCurrentMemoryOffset >> 8);
									SCIBusPCMCurrentMemoryOffsetBytes[1] = (byte)SCIBusPCMCurrentMemoryOffset;
									num4 = SCIBusPCMCurrentMemoryOffset - SCIBusPCMMemoryOffsetStart;
								}
								b2 = PartNumber[num4];
								break;
							case Task.WriteEEPROM:
								if (num4 >= EEPROMBuffer.Length)
								{
									num4 = (uint)(EEPROMBuffer.Length - 1);
								}
								b2 = EEPROMBuffer[num4];
								break;
							}
							SCIBusPCMTxPayload = new byte[4]
							{
								39,
								SCIBusPCMCurrentMemoryOffsetBytes[0],
								SCIBusPCMCurrentMemoryOffsetBytes[1],
								b2
							};
							break;
						}
						case 241:
							PCMUnlocked = false;
							break;
						}
					}
					break;
				case 40:
					if (array2.Length >= 4)
					{
						uint num5 = SCIBusPCMCurrentMemoryOffset - SCIBusPCMMemoryOffsetStart;
						switch (CurrentTask)
						{
						case Task.ReadSRIMileage:
							SRIMileage[num5] = array2[3];
							break;
						case Task.ReadSKIMVTSS:
							SKIMVTSS = array2[3];
							break;
						case Task.ReadVIN:
							VIN[num5] = array2[3];
							break;
						case Task.ReadPartNumber:
							PartNumberBuffer[num5] = array2[3];
							break;
						case Task.ReadEEPROM:
							EEPROMBuffer[num5] = array2[3];
							break;
						case Task.BackupEEPROM:
						{
							using (BinaryWriter binaryWriter3 = new BinaryWriter(File.Open(SCIBusPCMMemoryEEPROMBackupFilename, FileMode.Append)))
							{
								binaryWriter3.Write(array2[3]);
								binaryWriter3.Close();
							}
							((Control)SCIBusPCMWriteMemoryEEPROMOffsetTextBox).Text = Util.ByteToHexString(array2, 1, 2);
							((Control)SCIBusPCMWriteMemoryEEPROMValueTextBox).Text = Util.ByteToHexString(array2, 3);
							break;
						}
						}
						SCIBusPCMResponse = true;
						SCIBusPCMCurrentMemoryOffset++;
						SCIBusPCMCurrentMemoryOffsetBytes[0] = (byte)(SCIBusPCMCurrentMemoryOffset >> 8);
						SCIBusPCMCurrentMemoryOffsetBytes[1] = (byte)SCIBusPCMCurrentMemoryOffset;
						SCIBusPCMTxPayload = new byte[3]
						{
							40,
							SCIBusPCMCurrentMemoryOffsetBytes[0],
							SCIBusPCMCurrentMemoryOffsetBytes[1]
						};
					}
					break;
				case 41:
					if (array2.Length >= 5)
					{
						switch (array2[4])
						{
						case 229:
						{
							SCIBusPCMResponse = true;
							SCIBusPCMCurrentMemoryOffset++;
							SCIBusPCMCurrentMemoryOffsetBytes[0] = (byte)(SCIBusPCMCurrentMemoryOffset >> 8);
							SCIBusPCMCurrentMemoryOffsetBytes[1] = (byte)SCIBusPCMCurrentMemoryOffset;
							uint num = SCIBusPCMCurrentMemoryOffset - SCIBusPCMMemoryOffsetStart;
							byte b = 0;
							if (CurrentTask == Task.WriteRAM)
							{
								if (num >= RAMBuffer.Length)
								{
									num = (uint)(RAMBuffer.Length - 1);
								}
								b = RAMBuffer[num];
							}
							SCIBusPCMTxPayload = new byte[4]
							{
								41,
								SCIBusPCMCurrentMemoryOffsetBytes[0],
								SCIBusPCMCurrentMemoryOffsetBytes[1],
								b
							};
							break;
						}
						case 241:
							PCMUnlocked = false;
							break;
						}
					}
					break;
				case 43:
					if (array2.Length < 4)
					{
						UpdateTextBox(SCIBusPCMWriteMemoryInfoTextBox, Environment.NewLine + Environment.NewLine + "Check PCM status: invalid response.");
					}
					else if (array2[3] != Util.ChecksumCalculator(array2, 0, array2.Length - 1))
					{
						PCMUnlocked = false;
						UpdateTextBox(SCIBusPCMWriteMemoryInfoTextBox, Environment.NewLine + Environment.NewLine + "Check PCM status: checksum error.");
					}
					else
					{
						SCIBusPCMResponse = true;
						if (array2[1] == 0 && array2[2] == 0)
						{
							PCMUnlocked = true;
							UpdateTextBox(SCIBusPCMWriteMemoryInfoTextBox, Environment.NewLine + Environment.NewLine + "Check PCM status: unlocked.");
						}
						else
						{
							PCMUnlocked = false;
							UpdateTextBox(SCIBusPCMWriteMemoryInfoTextBox, Environment.NewLine + Environment.NewLine + "Check PCM status: locked.");
							UpdateTextBox(SCIBusPCMWriteMemoryInfoTextBox, Environment.NewLine + "Attempting to unlock PCM.");
							if (CurrentTask == Task.RestoreEEPROM)
							{
								return;
							}
							byte[] securityKey = UnlockAlgorithm.GetSecurityKey(UnlockAlgorithm.Controllers.SBEC, UnlockAlgorithm.SecurityLevels.Level1, array2.Skip(1).Take(2).ToArray());
							if (securityKey != null)
							{
								byte[] payload = new byte[4]
								{
									44,
									securityKey[0],
									securityKey[1],
									(byte)(44 + securityKey[0] + securityKey[1])
								};
								Packet packet2 = new Packet
								{
									Bus = 2,
									Command = 6,
									Mode = 2,
									Payload = payload
								};
								OriginalForm.TransmitUSBPacket("[<-TX] Send a SCI-bus (PCM) message once:", packet2);
								SerialService.WritePacket(packet2);
							}
						}
					}
					break;
				case 44:
					if (array2.Length < 5)
					{
						PCMUnlocked = false;
						UpdateTextBox(SCIBusPCMWriteMemoryInfoTextBox, Environment.NewLine + "PCM unlock error [1].");
					}
					else
					{
						SCIBusPCMResponse = true;
						if (array2[4] == 0)
						{
							PCMUnlocked = true;
							UpdateTextBox(SCIBusPCMWriteMemoryInfoTextBox, Environment.NewLine + "PCM unlocked.");
						}
						else
						{
							PCMUnlocked = false;
							UpdateTextBox(SCIBusPCMWriteMemoryInfoTextBox, Environment.NewLine + "PCM unlock failed [" + Util.ByteToHexString(array2, 4) + "].");
						}
					}
					break;
				}
				SCIBusPCMNextRequest = false;
				SCIBusPCMNextRequestTimer.Stop();
				SCIBusPCMNextRequestTimer.Start();
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
			if (((Control)TB).Name == "SCIBusPCMWriteMemoryInfoTextBox" && SCIBusPCMWriteMemoryLogFilename != null)
			{
				File.AppendAllText(SCIBusPCMWriteMemoryLogFilename, text);
			}
		});
	}

	public void UpdateMileageUnit()
	{
		if (Settings.Default.Units == "imperial")
		{
			((Control)SCIBusPCMWriteMemorySRIMileageUnitLabel).Text = "mi";
			if (SRIMileageMi != 0.0)
			{
				((Control)SCIBusPCMWriteMemorySRIMileageTextBox).Text = SRIMileageMi.ToString("0.0");
			}
		}
		else if (Settings.Default.Units == "metric")
		{
			((Control)SCIBusPCMWriteMemorySRIMileageUnitLabel).Text = "km";
			if (SRIMileageKm != 0.0)
			{
				((Control)SCIBusPCMWriteMemorySRIMileageTextBox).Text = SRIMileageKm.ToString("0.0");
			}
		}
	}

	private void WriteMemoryForm_FormClosing(object sender, FormClosingEventArgs e)
	{
		if (SCIBusPCMReadMemoryWorker.IsBusy)
		{
			SCIBusPCMReadMemoryWorker.CancelAsync();
		}
		if (SCIBusPCMWriteMemoryWorker.IsBusy)
		{
			SCIBusPCMWriteMemoryWorker.CancelAsync();
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
		//IL_01ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f4: Expected O, but got Unknown
		//IL_01f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ff: Expected O, but got Unknown
		//IL_0200: Unknown result type (might be due to invalid IL or missing references)
		//IL_020a: Expected O, but got Unknown
		//IL_020b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0215: Expected O, but got Unknown
		//IL_0216: Unknown result type (might be due to invalid IL or missing references)
		//IL_0220: Expected O, but got Unknown
		//IL_0221: Unknown result type (might be due to invalid IL or missing references)
		//IL_022b: Expected O, but got Unknown
		//IL_022c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0236: Expected O, but got Unknown
		//IL_0237: Unknown result type (might be due to invalid IL or missing references)
		//IL_0241: Expected O, but got Unknown
		//IL_0242: Unknown result type (might be due to invalid IL or missing references)
		//IL_024c: Expected O, but got Unknown
		//IL_024d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0257: Expected O, but got Unknown
		//IL_0258: Unknown result type (might be due to invalid IL or missing references)
		//IL_0262: Expected O, but got Unknown
		//IL_0263: Unknown result type (might be due to invalid IL or missing references)
		//IL_026d: Expected O, but got Unknown
		//IL_026e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0278: Expected O, but got Unknown
		//IL_0279: Unknown result type (might be due to invalid IL or missing references)
		//IL_0283: Expected O, but got Unknown
		//IL_0284: Unknown result type (might be due to invalid IL or missing references)
		//IL_028e: Expected O, but got Unknown
		//IL_028f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0299: Expected O, but got Unknown
		//IL_029a: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a4: Expected O, but got Unknown
		//IL_0b4e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b58: Expected O, but got Unknown
		//IL_0c73: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c7d: Expected O, but got Unknown
		//IL_0ce3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0ced: Expected O, but got Unknown
		//IL_0e1b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0e25: Expected O, but got Unknown
		//IL_0f90: Unknown result type (might be due to invalid IL or missing references)
		//IL_0f9a: Expected O, but got Unknown
		//IL_1033: Unknown result type (might be due to invalid IL or missing references)
		//IL_103d: Expected O, but got Unknown
		//IL_1388: Unknown result type (might be due to invalid IL or missing references)
		//IL_1392: Expected O, but got Unknown
		//IL_13d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_13e1: Expected O, but got Unknown
		//IL_146a: Unknown result type (might be due to invalid IL or missing references)
		//IL_1474: Expected O, but got Unknown
		//IL_156e: Unknown result type (might be due to invalid IL or missing references)
		//IL_1578: Expected O, but got Unknown
		ComponentResourceManager componentResourceManager = new ComponentResourceManager(typeof(ReadWriteMemoryForm));
		WriteMemoryTabControl = new TabControl();
		CCDBusTabPage = new TabPage();
		groupBox2 = new GroupBox();
		groupBox1 = new GroupBox();
		button2 = new Button();
		button3 = new Button();
		label1 = new Label();
		textBox2 = new TextBox();
		button4 = new Button();
		button5 = new Button();
		label3 = new Label();
		textBox3 = new TextBox();
		label4 = new Label();
		textBox4 = new TextBox();
		button1 = new Button();
		textBox1 = new TextBox();
		SCIBusPCMTabPage = new TabPage();
		SCIBusPCMWriteMemoryCopyBCMMileageButton = new Button();
		SCIBusPCMWriteMemoryEEPROMGroupBox = new GroupBox();
		SCIBusPCMWriteMemoryEEPROMRestoreButton = new Button();
		SCIBusPCMWriteMemoryEEPROMBackupButton = new Button();
		SCIBusPCMWriteMemoryEEPROMValueCountLabel = new Label();
		SCIBusPCMWriteMemoryEEPROMValueCountTextBox = new TextBox();
		SCIBusPCMWriteMemoryEEPROMSeparatorLabel = new Label();
		SCIBusPCMWriteMemoryEEPROMWriteButton = new Button();
		SCIBusPCMWriteMemoryEEPROMReadButton = new Button();
		SCIBusPCMWriteMemoryEEPROMValueLabel = new Label();
		SCIBusPCMWriteMemoryEEPROMValueTextBox = new TextBox();
		SCIBusPCMWriteMemoryEEPROMOffsetLabel = new Label();
		SCIBusPCMWriteMemoryEEPROMOffsetTextBox = new TextBox();
		SCIBusPCMWriteMemorySRIMileageLabel = new Label();
		SCIBusPCMWriteMemorySKIMVTSSLabel = new Label();
		SCIBusPCMWriteMemorySRIMileageUnitLabel = new Label();
		SCIBusPCMWriteMemoryVINLabel = new Label();
		SCIBusPCMWriteMemoryPartNumberLabel = new Label();
		SCIBusPCMWriteMemoryPartNumberWriteButton = new Button();
		SCIBusPCMWriteMemorySRIMileageTextBox = new TextBox();
		SCIBusPCMWriteMemoryPartNumberReadButton = new Button();
		SCIBusPCMWriteMemorySKIMVTSSComboBox = new ComboBox();
		SCIBusPCMWriteMemoryVINWriteButton = new Button();
		SCIBusPCMWriteMemoryVINTextBox = new TextBox();
		SCIBusPCMWriteMemoryVINReadButton = new Button();
		SCIBusPCMWriteMemoryPartNumberTextBox = new TextBox();
		SCIBusPCMWriteMemorySKIMVTSSWriteButton = new Button();
		SCIBusPCMWriteMemorySRIMileageReadButton = new Button();
		SCIBusPCMWriteMemorySKIMVTSSReadButton = new Button();
		SCIBusPCMWriteMemorySRIMileageWriteButton = new Button();
		SCIBusPCMWriteMemoryRAMGroupBox = new GroupBox();
		SCIBusPCMWriteMemoryRAMRestoreButton = new Button();
		SCIBusPCMWriteMemoryRAMBackupButton = new Button();
		SCIBusPCMWriteMemoryRAMValueCountLabel = new Label();
		SCIBusPCMWriteMemoryRAMWriteButton = new Button();
		SCIBusPCMWriteMemoryRAMValueCountTextBox = new TextBox();
		SCIBusPCMWriteMemoryRAMOffsetTextBox = new TextBox();
		SCIBusPCMWriteMemoryRAMReadButton = new Button();
		SCIBusPCMWriteMemoryRAMValueTextBox = new TextBox();
		SCIBusPCMWriteMemoryRAMOffsetLabel = new Label();
		SCIBusPCMWriteMemoryRAMValueLabel = new Label();
		SCIBusPCMWriteMemoryHelpButton = new Button();
		SCIBusPCMWriteMemoryInfoTextBox = new TextBox();
		((Control)WriteMemoryTabControl).SuspendLayout();
		((Control)CCDBusTabPage).SuspendLayout();
		((Control)groupBox1).SuspendLayout();
		((Control)SCIBusPCMTabPage).SuspendLayout();
		((Control)SCIBusPCMWriteMemoryEEPROMGroupBox).SuspendLayout();
		((Control)SCIBusPCMWriteMemoryRAMGroupBox).SuspendLayout();
		((Control)this).SuspendLayout();
		((Control)WriteMemoryTabControl).Controls.Add((Control)(object)CCDBusTabPage);
		((Control)WriteMemoryTabControl).Controls.Add((Control)(object)SCIBusPCMTabPage);
		componentResourceManager.ApplyResources(WriteMemoryTabControl, "WriteMemoryTabControl");
		((Control)WriteMemoryTabControl).Name = "WriteMemoryTabControl";
		WriteMemoryTabControl.SelectedIndex = 0;
		((Control)CCDBusTabPage).BackColor = Color.Transparent;
		((Control)CCDBusTabPage).Controls.Add((Control)(object)groupBox2);
		((Control)CCDBusTabPage).Controls.Add((Control)(object)groupBox1);
		((Control)CCDBusTabPage).Controls.Add((Control)(object)button1);
		((Control)CCDBusTabPage).Controls.Add((Control)(object)textBox1);
		componentResourceManager.ApplyResources(CCDBusTabPage, "CCDBusTabPage");
		((Control)CCDBusTabPage).Name = "CCDBusTabPage";
		componentResourceManager.ApplyResources(groupBox2, "groupBox2");
		((Control)groupBox2).Name = "groupBox2";
		groupBox2.TabStop = false;
		((Control)groupBox1).Controls.Add((Control)(object)button2);
		((Control)groupBox1).Controls.Add((Control)(object)button3);
		((Control)groupBox1).Controls.Add((Control)(object)label1);
		((Control)groupBox1).Controls.Add((Control)(object)textBox2);
		((Control)groupBox1).Controls.Add((Control)(object)button4);
		((Control)groupBox1).Controls.Add((Control)(object)button5);
		((Control)groupBox1).Controls.Add((Control)(object)label3);
		((Control)groupBox1).Controls.Add((Control)(object)textBox3);
		((Control)groupBox1).Controls.Add((Control)(object)label4);
		((Control)groupBox1).Controls.Add((Control)(object)textBox4);
		componentResourceManager.ApplyResources(groupBox1, "groupBox1");
		((Control)groupBox1).Name = "groupBox1";
		groupBox1.TabStop = false;
		componentResourceManager.ApplyResources(button2, "button2");
		((Control)button2).Name = "button2";
		((ButtonBase)button2).UseVisualStyleBackColor = true;
		componentResourceManager.ApplyResources(button3, "button3");
		((Control)button3).Name = "button3";
		((ButtonBase)button3).UseVisualStyleBackColor = true;
		componentResourceManager.ApplyResources(label1, "label1");
		((Control)label1).Name = "label1";
		componentResourceManager.ApplyResources(textBox2, "textBox2");
		((Control)textBox2).Name = "textBox2";
		componentResourceManager.ApplyResources(button4, "button4");
		((Control)button4).Name = "button4";
		((ButtonBase)button4).UseVisualStyleBackColor = true;
		componentResourceManager.ApplyResources(button5, "button5");
		((Control)button5).Name = "button5";
		((ButtonBase)button5).UseVisualStyleBackColor = true;
		componentResourceManager.ApplyResources(label3, "label3");
		((Control)label3).Name = "label3";
		componentResourceManager.ApplyResources(textBox3, "textBox3");
		((Control)textBox3).Name = "textBox3";
		componentResourceManager.ApplyResources(label4, "label4");
		((Control)label4).Name = "label4";
		componentResourceManager.ApplyResources(textBox4, "textBox4");
		((Control)textBox4).Name = "textBox4";
		componentResourceManager.ApplyResources(button1, "button1");
		((Control)button1).Name = "button1";
		((ButtonBase)button1).UseVisualStyleBackColor = true;
		((Control)textBox1).BackColor = SystemColors.Window;
		componentResourceManager.ApplyResources(textBox1, "textBox1");
		((Control)textBox1).Name = "textBox1";
		((TextBoxBase)textBox1).ReadOnly = true;
		((Control)SCIBusPCMTabPage).BackColor = Color.Transparent;
		((Control)SCIBusPCMTabPage).Controls.Add((Control)(object)SCIBusPCMWriteMemoryCopyBCMMileageButton);
		((Control)SCIBusPCMTabPage).Controls.Add((Control)(object)SCIBusPCMWriteMemoryEEPROMGroupBox);
		((Control)SCIBusPCMTabPage).Controls.Add((Control)(object)SCIBusPCMWriteMemoryRAMGroupBox);
		((Control)SCIBusPCMTabPage).Controls.Add((Control)(object)SCIBusPCMWriteMemoryHelpButton);
		((Control)SCIBusPCMTabPage).Controls.Add((Control)(object)SCIBusPCMWriteMemoryInfoTextBox);
		componentResourceManager.ApplyResources(SCIBusPCMTabPage, "SCIBusPCMTabPage");
		((Control)SCIBusPCMTabPage).Name = "SCIBusPCMTabPage";
		componentResourceManager.ApplyResources(SCIBusPCMWriteMemoryCopyBCMMileageButton, "SCIBusPCMWriteMemoryCopyBCMMileageButton");
		((Control)SCIBusPCMWriteMemoryCopyBCMMileageButton).Name = "SCIBusPCMWriteMemoryCopyBCMMileageButton";
		((ButtonBase)SCIBusPCMWriteMemoryCopyBCMMileageButton).UseVisualStyleBackColor = true;
		((Control)SCIBusPCMWriteMemoryCopyBCMMileageButton).Click += SCIBusPCMWriteMemoryCopyBCMMileageButton_Click;
		((Control)SCIBusPCMWriteMemoryEEPROMGroupBox).Controls.Add((Control)(object)SCIBusPCMWriteMemoryEEPROMRestoreButton);
		((Control)SCIBusPCMWriteMemoryEEPROMGroupBox).Controls.Add((Control)(object)SCIBusPCMWriteMemoryEEPROMBackupButton);
		((Control)SCIBusPCMWriteMemoryEEPROMGroupBox).Controls.Add((Control)(object)SCIBusPCMWriteMemoryEEPROMValueCountLabel);
		((Control)SCIBusPCMWriteMemoryEEPROMGroupBox).Controls.Add((Control)(object)SCIBusPCMWriteMemoryEEPROMValueCountTextBox);
		((Control)SCIBusPCMWriteMemoryEEPROMGroupBox).Controls.Add((Control)(object)SCIBusPCMWriteMemoryEEPROMSeparatorLabel);
		((Control)SCIBusPCMWriteMemoryEEPROMGroupBox).Controls.Add((Control)(object)SCIBusPCMWriteMemoryEEPROMWriteButton);
		((Control)SCIBusPCMWriteMemoryEEPROMGroupBox).Controls.Add((Control)(object)SCIBusPCMWriteMemoryEEPROMReadButton);
		((Control)SCIBusPCMWriteMemoryEEPROMGroupBox).Controls.Add((Control)(object)SCIBusPCMWriteMemoryEEPROMValueLabel);
		((Control)SCIBusPCMWriteMemoryEEPROMGroupBox).Controls.Add((Control)(object)SCIBusPCMWriteMemoryEEPROMValueTextBox);
		((Control)SCIBusPCMWriteMemoryEEPROMGroupBox).Controls.Add((Control)(object)SCIBusPCMWriteMemoryEEPROMOffsetLabel);
		((Control)SCIBusPCMWriteMemoryEEPROMGroupBox).Controls.Add((Control)(object)SCIBusPCMWriteMemoryEEPROMOffsetTextBox);
		((Control)SCIBusPCMWriteMemoryEEPROMGroupBox).Controls.Add((Control)(object)SCIBusPCMWriteMemorySRIMileageLabel);
		((Control)SCIBusPCMWriteMemoryEEPROMGroupBox).Controls.Add((Control)(object)SCIBusPCMWriteMemorySKIMVTSSLabel);
		((Control)SCIBusPCMWriteMemoryEEPROMGroupBox).Controls.Add((Control)(object)SCIBusPCMWriteMemorySRIMileageUnitLabel);
		((Control)SCIBusPCMWriteMemoryEEPROMGroupBox).Controls.Add((Control)(object)SCIBusPCMWriteMemoryVINLabel);
		((Control)SCIBusPCMWriteMemoryEEPROMGroupBox).Controls.Add((Control)(object)SCIBusPCMWriteMemoryPartNumberLabel);
		((Control)SCIBusPCMWriteMemoryEEPROMGroupBox).Controls.Add((Control)(object)SCIBusPCMWriteMemoryPartNumberWriteButton);
		((Control)SCIBusPCMWriteMemoryEEPROMGroupBox).Controls.Add((Control)(object)SCIBusPCMWriteMemorySRIMileageTextBox);
		((Control)SCIBusPCMWriteMemoryEEPROMGroupBox).Controls.Add((Control)(object)SCIBusPCMWriteMemoryPartNumberReadButton);
		((Control)SCIBusPCMWriteMemoryEEPROMGroupBox).Controls.Add((Control)(object)SCIBusPCMWriteMemorySKIMVTSSComboBox);
		((Control)SCIBusPCMWriteMemoryEEPROMGroupBox).Controls.Add((Control)(object)SCIBusPCMWriteMemoryVINWriteButton);
		((Control)SCIBusPCMWriteMemoryEEPROMGroupBox).Controls.Add((Control)(object)SCIBusPCMWriteMemoryVINTextBox);
		((Control)SCIBusPCMWriteMemoryEEPROMGroupBox).Controls.Add((Control)(object)SCIBusPCMWriteMemoryVINReadButton);
		((Control)SCIBusPCMWriteMemoryEEPROMGroupBox).Controls.Add((Control)(object)SCIBusPCMWriteMemoryPartNumberTextBox);
		((Control)SCIBusPCMWriteMemoryEEPROMGroupBox).Controls.Add((Control)(object)SCIBusPCMWriteMemorySKIMVTSSWriteButton);
		((Control)SCIBusPCMWriteMemoryEEPROMGroupBox).Controls.Add((Control)(object)SCIBusPCMWriteMemorySRIMileageReadButton);
		((Control)SCIBusPCMWriteMemoryEEPROMGroupBox).Controls.Add((Control)(object)SCIBusPCMWriteMemorySKIMVTSSReadButton);
		((Control)SCIBusPCMWriteMemoryEEPROMGroupBox).Controls.Add((Control)(object)SCIBusPCMWriteMemorySRIMileageWriteButton);
		componentResourceManager.ApplyResources(SCIBusPCMWriteMemoryEEPROMGroupBox, "SCIBusPCMWriteMemoryEEPROMGroupBox");
		((Control)SCIBusPCMWriteMemoryEEPROMGroupBox).Name = "SCIBusPCMWriteMemoryEEPROMGroupBox";
		SCIBusPCMWriteMemoryEEPROMGroupBox.TabStop = false;
		componentResourceManager.ApplyResources(SCIBusPCMWriteMemoryEEPROMRestoreButton, "SCIBusPCMWriteMemoryEEPROMRestoreButton");
		((Control)SCIBusPCMWriteMemoryEEPROMRestoreButton).Name = "SCIBusPCMWriteMemoryEEPROMRestoreButton";
		((ButtonBase)SCIBusPCMWriteMemoryEEPROMRestoreButton).UseVisualStyleBackColor = true;
		((Control)SCIBusPCMWriteMemoryEEPROMRestoreButton).Click += SCIBusPCMWriteMemoryEEPROMRestoreButton_Click;
		componentResourceManager.ApplyResources(SCIBusPCMWriteMemoryEEPROMBackupButton, "SCIBusPCMWriteMemoryEEPROMBackupButton");
		((Control)SCIBusPCMWriteMemoryEEPROMBackupButton).Name = "SCIBusPCMWriteMemoryEEPROMBackupButton";
		((ButtonBase)SCIBusPCMWriteMemoryEEPROMBackupButton).UseVisualStyleBackColor = true;
		((Control)SCIBusPCMWriteMemoryEEPROMBackupButton).Click += SCIBusPCMWriteMemoryEEPROMBackupButton_Click;
		componentResourceManager.ApplyResources(SCIBusPCMWriteMemoryEEPROMValueCountLabel, "SCIBusPCMWriteMemoryEEPROMValueCountLabel");
		((Control)SCIBusPCMWriteMemoryEEPROMValueCountLabel).Name = "SCIBusPCMWriteMemoryEEPROMValueCountLabel";
		componentResourceManager.ApplyResources(SCIBusPCMWriteMemoryEEPROMValueCountTextBox, "SCIBusPCMWriteMemoryEEPROMValueCountTextBox");
		((Control)SCIBusPCMWriteMemoryEEPROMValueCountTextBox).Name = "SCIBusPCMWriteMemoryEEPROMValueCountTextBox";
		((Control)SCIBusPCMWriteMemoryEEPROMValueCountTextBox).TextChanged += SCIBusPCMWriteMemoryEEPROMOffsetAndCountAndValueTextBox_TextChanged;
		((Control)SCIBusPCMWriteMemoryEEPROMValueCountTextBox).KeyPress += new KeyPressEventHandler(SCIBusPCMWriteMemoryEEPROMValueCountTextBox_KeyPress);
		SCIBusPCMWriteMemoryEEPROMSeparatorLabel.BorderStyle = (BorderStyle)2;
		componentResourceManager.ApplyResources(SCIBusPCMWriteMemoryEEPROMSeparatorLabel, "SCIBusPCMWriteMemoryEEPROMSeparatorLabel");
		((Control)SCIBusPCMWriteMemoryEEPROMSeparatorLabel).Name = "SCIBusPCMWriteMemoryEEPROMSeparatorLabel";
		componentResourceManager.ApplyResources(SCIBusPCMWriteMemoryEEPROMWriteButton, "SCIBusPCMWriteMemoryEEPROMWriteButton");
		((Control)SCIBusPCMWriteMemoryEEPROMWriteButton).Name = "SCIBusPCMWriteMemoryEEPROMWriteButton";
		((ButtonBase)SCIBusPCMWriteMemoryEEPROMWriteButton).UseVisualStyleBackColor = true;
		((Control)SCIBusPCMWriteMemoryEEPROMWriteButton).Click += SCIBusPCMWriteMemoryEEPROMWriteButton_Click;
		componentResourceManager.ApplyResources(SCIBusPCMWriteMemoryEEPROMReadButton, "SCIBusPCMWriteMemoryEEPROMReadButton");
		((Control)SCIBusPCMWriteMemoryEEPROMReadButton).Name = "SCIBusPCMWriteMemoryEEPROMReadButton";
		((ButtonBase)SCIBusPCMWriteMemoryEEPROMReadButton).UseVisualStyleBackColor = true;
		((Control)SCIBusPCMWriteMemoryEEPROMReadButton).Click += SCIBusPCMWriteMemoryEEPROMReadButton_Click;
		componentResourceManager.ApplyResources(SCIBusPCMWriteMemoryEEPROMValueLabel, "SCIBusPCMWriteMemoryEEPROMValueLabel");
		((Control)SCIBusPCMWriteMemoryEEPROMValueLabel).Name = "SCIBusPCMWriteMemoryEEPROMValueLabel";
		componentResourceManager.ApplyResources(SCIBusPCMWriteMemoryEEPROMValueTextBox, "SCIBusPCMWriteMemoryEEPROMValueTextBox");
		((Control)SCIBusPCMWriteMemoryEEPROMValueTextBox).Name = "SCIBusPCMWriteMemoryEEPROMValueTextBox";
		((Control)SCIBusPCMWriteMemoryEEPROMValueTextBox).TextChanged += SCIBusPCMWriteMemoryEEPROMOffsetAndCountAndValueTextBox_TextChanged;
		((Control)SCIBusPCMWriteMemoryEEPROMValueTextBox).KeyPress += new KeyPressEventHandler(SCIBusPCMWriteMemoryEEPROMValueTextBox_KeyPress);
		componentResourceManager.ApplyResources(SCIBusPCMWriteMemoryEEPROMOffsetLabel, "SCIBusPCMWriteMemoryEEPROMOffsetLabel");
		((Control)SCIBusPCMWriteMemoryEEPROMOffsetLabel).Name = "SCIBusPCMWriteMemoryEEPROMOffsetLabel";
		componentResourceManager.ApplyResources(SCIBusPCMWriteMemoryEEPROMOffsetTextBox, "SCIBusPCMWriteMemoryEEPROMOffsetTextBox");
		((Control)SCIBusPCMWriteMemoryEEPROMOffsetTextBox).Name = "SCIBusPCMWriteMemoryEEPROMOffsetTextBox";
		((Control)SCIBusPCMWriteMemoryEEPROMOffsetTextBox).TextChanged += SCIBusPCMWriteMemoryEEPROMOffsetAndCountAndValueTextBox_TextChanged;
		((Control)SCIBusPCMWriteMemoryEEPROMOffsetTextBox).KeyPress += new KeyPressEventHandler(SCIBusPCMWriteMemoryEEPROMOffsetTextBox_KeyPress);
		componentResourceManager.ApplyResources(SCIBusPCMWriteMemorySRIMileageLabel, "SCIBusPCMWriteMemorySRIMileageLabel");
		((Control)SCIBusPCMWriteMemorySRIMileageLabel).Name = "SCIBusPCMWriteMemorySRIMileageLabel";
		componentResourceManager.ApplyResources(SCIBusPCMWriteMemorySKIMVTSSLabel, "SCIBusPCMWriteMemorySKIMVTSSLabel");
		((Control)SCIBusPCMWriteMemorySKIMVTSSLabel).Name = "SCIBusPCMWriteMemorySKIMVTSSLabel";
		componentResourceManager.ApplyResources(SCIBusPCMWriteMemorySRIMileageUnitLabel, "SCIBusPCMWriteMemorySRIMileageUnitLabel");
		((Control)SCIBusPCMWriteMemorySRIMileageUnitLabel).Name = "SCIBusPCMWriteMemorySRIMileageUnitLabel";
		componentResourceManager.ApplyResources(SCIBusPCMWriteMemoryVINLabel, "SCIBusPCMWriteMemoryVINLabel");
		((Control)SCIBusPCMWriteMemoryVINLabel).Name = "SCIBusPCMWriteMemoryVINLabel";
		componentResourceManager.ApplyResources(SCIBusPCMWriteMemoryPartNumberLabel, "SCIBusPCMWriteMemoryPartNumberLabel");
		((Control)SCIBusPCMWriteMemoryPartNumberLabel).Name = "SCIBusPCMWriteMemoryPartNumberLabel";
		componentResourceManager.ApplyResources(SCIBusPCMWriteMemoryPartNumberWriteButton, "SCIBusPCMWriteMemoryPartNumberWriteButton");
		((Control)SCIBusPCMWriteMemoryPartNumberWriteButton).Name = "SCIBusPCMWriteMemoryPartNumberWriteButton";
		((ButtonBase)SCIBusPCMWriteMemoryPartNumberWriteButton).UseVisualStyleBackColor = true;
		((Control)SCIBusPCMWriteMemoryPartNumberWriteButton).Click += SCIBusPCMWriteMemoryPartNumberWriteButton_Click;
		componentResourceManager.ApplyResources(SCIBusPCMWriteMemorySRIMileageTextBox, "SCIBusPCMWriteMemorySRIMileageTextBox");
		((Control)SCIBusPCMWriteMemorySRIMileageTextBox).Name = "SCIBusPCMWriteMemorySRIMileageTextBox";
		((Control)SCIBusPCMWriteMemorySRIMileageTextBox).TextChanged += SCIBusPCMWriteMemorySRIMileageTextBox_TextChanged;
		((Control)SCIBusPCMWriteMemorySRIMileageTextBox).KeyPress += new KeyPressEventHandler(SCIBusPCMWriteMemorySRIMileageTextBox_KeyPress);
		componentResourceManager.ApplyResources(SCIBusPCMWriteMemoryPartNumberReadButton, "SCIBusPCMWriteMemoryPartNumberReadButton");
		((Control)SCIBusPCMWriteMemoryPartNumberReadButton).Name = "SCIBusPCMWriteMemoryPartNumberReadButton";
		((ButtonBase)SCIBusPCMWriteMemoryPartNumberReadButton).UseVisualStyleBackColor = true;
		((Control)SCIBusPCMWriteMemoryPartNumberReadButton).Click += SCIBusPCMWriteMemoryPartNumberReadButton_Click;
		SCIBusPCMWriteMemorySKIMVTSSComboBox.DropDownStyle = (ComboBoxStyle)2;
		((ListControl)SCIBusPCMWriteMemorySKIMVTSSComboBox).FormattingEnabled = true;
		SCIBusPCMWriteMemorySKIMVTSSComboBox.Items.AddRange(new object[4]
		{
			componentResourceManager.GetString("SCIBusPCMWriteMemorySKIMVTSSComboBox.Items"),
			componentResourceManager.GetString("SCIBusPCMWriteMemorySKIMVTSSComboBox.Items1"),
			componentResourceManager.GetString("SCIBusPCMWriteMemorySKIMVTSSComboBox.Items2"),
			componentResourceManager.GetString("SCIBusPCMWriteMemorySKIMVTSSComboBox.Items3")
		});
		componentResourceManager.ApplyResources(SCIBusPCMWriteMemorySKIMVTSSComboBox, "SCIBusPCMWriteMemorySKIMVTSSComboBox");
		((Control)SCIBusPCMWriteMemorySKIMVTSSComboBox).Name = "SCIBusPCMWriteMemorySKIMVTSSComboBox";
		SCIBusPCMWriteMemorySKIMVTSSComboBox.SelectedIndexChanged += SCIBusPCMWriteMemorySKIMVTSSComboBox_SelectedIndexChanged;
		componentResourceManager.ApplyResources(SCIBusPCMWriteMemoryVINWriteButton, "SCIBusPCMWriteMemoryVINWriteButton");
		((Control)SCIBusPCMWriteMemoryVINWriteButton).Name = "SCIBusPCMWriteMemoryVINWriteButton";
		((ButtonBase)SCIBusPCMWriteMemoryVINWriteButton).UseVisualStyleBackColor = true;
		((Control)SCIBusPCMWriteMemoryVINWriteButton).Click += SCIBusPCMWriteMemoryVINWriteButton_Click;
		componentResourceManager.ApplyResources(SCIBusPCMWriteMemoryVINTextBox, "SCIBusPCMWriteMemoryVINTextBox");
		((Control)SCIBusPCMWriteMemoryVINTextBox).Name = "SCIBusPCMWriteMemoryVINTextBox";
		((Control)SCIBusPCMWriteMemoryVINTextBox).TextChanged += SCIBusPCMWriteMemoryVINTextBox_TextChanged;
		((Control)SCIBusPCMWriteMemoryVINTextBox).KeyPress += new KeyPressEventHandler(SCIBusPCMWriteMemoryVINTextBox_KeyPress);
		componentResourceManager.ApplyResources(SCIBusPCMWriteMemoryVINReadButton, "SCIBusPCMWriteMemoryVINReadButton");
		((Control)SCIBusPCMWriteMemoryVINReadButton).Name = "SCIBusPCMWriteMemoryVINReadButton";
		((ButtonBase)SCIBusPCMWriteMemoryVINReadButton).UseVisualStyleBackColor = true;
		((Control)SCIBusPCMWriteMemoryVINReadButton).Click += SCIBusPCMWriteMemoryVINReadButton_Click;
		((Control)SCIBusPCMWriteMemoryPartNumberTextBox).BackColor = SystemColors.Window;
		componentResourceManager.ApplyResources(SCIBusPCMWriteMemoryPartNumberTextBox, "SCIBusPCMWriteMemoryPartNumberTextBox");
		((Control)SCIBusPCMWriteMemoryPartNumberTextBox).Name = "SCIBusPCMWriteMemoryPartNumberTextBox";
		((Control)SCIBusPCMWriteMemoryPartNumberTextBox).TextChanged += SCIBusPCMWriteMemoryPartNumberTextBox_TextChanged;
		((Control)SCIBusPCMWriteMemoryPartNumberTextBox).KeyPress += new KeyPressEventHandler(SCIBusPCMWriteMemoryPartNumberTextBox_KeyPress);
		componentResourceManager.ApplyResources(SCIBusPCMWriteMemorySKIMVTSSWriteButton, "SCIBusPCMWriteMemorySKIMVTSSWriteButton");
		((Control)SCIBusPCMWriteMemorySKIMVTSSWriteButton).Name = "SCIBusPCMWriteMemorySKIMVTSSWriteButton";
		((ButtonBase)SCIBusPCMWriteMemorySKIMVTSSWriteButton).UseVisualStyleBackColor = true;
		((Control)SCIBusPCMWriteMemorySKIMVTSSWriteButton).Click += SCIBusPCMWriteMemorySKIMVTSSWriteButton_Click;
		componentResourceManager.ApplyResources(SCIBusPCMWriteMemorySRIMileageReadButton, "SCIBusPCMWriteMemorySRIMileageReadButton");
		((Control)SCIBusPCMWriteMemorySRIMileageReadButton).Name = "SCIBusPCMWriteMemorySRIMileageReadButton";
		((ButtonBase)SCIBusPCMWriteMemorySRIMileageReadButton).UseVisualStyleBackColor = true;
		((Control)SCIBusPCMWriteMemorySRIMileageReadButton).Click += SCIBusPCMWriteMemorySRIMileageReadButton_Click;
		componentResourceManager.ApplyResources(SCIBusPCMWriteMemorySKIMVTSSReadButton, "SCIBusPCMWriteMemorySKIMVTSSReadButton");
		((Control)SCIBusPCMWriteMemorySKIMVTSSReadButton).Name = "SCIBusPCMWriteMemorySKIMVTSSReadButton";
		((ButtonBase)SCIBusPCMWriteMemorySKIMVTSSReadButton).UseVisualStyleBackColor = true;
		((Control)SCIBusPCMWriteMemorySKIMVTSSReadButton).Click += SCIBusPCMWriteMemorySKIMVTSSReadButton_Click;
		componentResourceManager.ApplyResources(SCIBusPCMWriteMemorySRIMileageWriteButton, "SCIBusPCMWriteMemorySRIMileageWriteButton");
		((Control)SCIBusPCMWriteMemorySRIMileageWriteButton).Name = "SCIBusPCMWriteMemorySRIMileageWriteButton";
		((ButtonBase)SCIBusPCMWriteMemorySRIMileageWriteButton).UseVisualStyleBackColor = true;
		((Control)SCIBusPCMWriteMemorySRIMileageWriteButton).Click += SCIBusPCMWriteMemorySRIMileageWriteButton_Click;
		((Control)SCIBusPCMWriteMemoryRAMGroupBox).Controls.Add((Control)(object)SCIBusPCMWriteMemoryRAMRestoreButton);
		((Control)SCIBusPCMWriteMemoryRAMGroupBox).Controls.Add((Control)(object)SCIBusPCMWriteMemoryRAMBackupButton);
		((Control)SCIBusPCMWriteMemoryRAMGroupBox).Controls.Add((Control)(object)SCIBusPCMWriteMemoryRAMValueCountLabel);
		((Control)SCIBusPCMWriteMemoryRAMGroupBox).Controls.Add((Control)(object)SCIBusPCMWriteMemoryRAMWriteButton);
		((Control)SCIBusPCMWriteMemoryRAMGroupBox).Controls.Add((Control)(object)SCIBusPCMWriteMemoryRAMValueCountTextBox);
		((Control)SCIBusPCMWriteMemoryRAMGroupBox).Controls.Add((Control)(object)SCIBusPCMWriteMemoryRAMOffsetTextBox);
		((Control)SCIBusPCMWriteMemoryRAMGroupBox).Controls.Add((Control)(object)SCIBusPCMWriteMemoryRAMReadButton);
		((Control)SCIBusPCMWriteMemoryRAMGroupBox).Controls.Add((Control)(object)SCIBusPCMWriteMemoryRAMValueTextBox);
		((Control)SCIBusPCMWriteMemoryRAMGroupBox).Controls.Add((Control)(object)SCIBusPCMWriteMemoryRAMOffsetLabel);
		((Control)SCIBusPCMWriteMemoryRAMGroupBox).Controls.Add((Control)(object)SCIBusPCMWriteMemoryRAMValueLabel);
		componentResourceManager.ApplyResources(SCIBusPCMWriteMemoryRAMGroupBox, "SCIBusPCMWriteMemoryRAMGroupBox");
		((Control)SCIBusPCMWriteMemoryRAMGroupBox).Name = "SCIBusPCMWriteMemoryRAMGroupBox";
		SCIBusPCMWriteMemoryRAMGroupBox.TabStop = false;
		componentResourceManager.ApplyResources(SCIBusPCMWriteMemoryRAMRestoreButton, "SCIBusPCMWriteMemoryRAMRestoreButton");
		((Control)SCIBusPCMWriteMemoryRAMRestoreButton).Name = "SCIBusPCMWriteMemoryRAMRestoreButton";
		((ButtonBase)SCIBusPCMWriteMemoryRAMRestoreButton).UseVisualStyleBackColor = true;
		((Control)SCIBusPCMWriteMemoryRAMRestoreButton).Click += SCIBusPCMWriteMemoryRAMRestoreButton_Click;
		componentResourceManager.ApplyResources(SCIBusPCMWriteMemoryRAMBackupButton, "SCIBusPCMWriteMemoryRAMBackupButton");
		((Control)SCIBusPCMWriteMemoryRAMBackupButton).Name = "SCIBusPCMWriteMemoryRAMBackupButton";
		((ButtonBase)SCIBusPCMWriteMemoryRAMBackupButton).UseVisualStyleBackColor = true;
		((Control)SCIBusPCMWriteMemoryRAMBackupButton).Click += SCIBusPCMWriteMemoryRAMBackupButton_Click;
		componentResourceManager.ApplyResources(SCIBusPCMWriteMemoryRAMValueCountLabel, "SCIBusPCMWriteMemoryRAMValueCountLabel");
		((Control)SCIBusPCMWriteMemoryRAMValueCountLabel).Name = "SCIBusPCMWriteMemoryRAMValueCountLabel";
		componentResourceManager.ApplyResources(SCIBusPCMWriteMemoryRAMWriteButton, "SCIBusPCMWriteMemoryRAMWriteButton");
		((Control)SCIBusPCMWriteMemoryRAMWriteButton).Name = "SCIBusPCMWriteMemoryRAMWriteButton";
		((ButtonBase)SCIBusPCMWriteMemoryRAMWriteButton).UseVisualStyleBackColor = true;
		((Control)SCIBusPCMWriteMemoryRAMWriteButton).Click += SCIBusPCMWriteMemoryRAMWriteButton_Click;
		componentResourceManager.ApplyResources(SCIBusPCMWriteMemoryRAMValueCountTextBox, "SCIBusPCMWriteMemoryRAMValueCountTextBox");
		((Control)SCIBusPCMWriteMemoryRAMValueCountTextBox).Name = "SCIBusPCMWriteMemoryRAMValueCountTextBox";
		((Control)SCIBusPCMWriteMemoryRAMValueCountTextBox).TextChanged += SCIBusPCMWriteMemoryRAMOffsetAndCountAndValueTextBox_TextChanged;
		((Control)SCIBusPCMWriteMemoryRAMValueCountTextBox).KeyPress += new KeyPressEventHandler(SCIBusPCMWriteMemoryRAMValueCountTextBox_KeyPress);
		componentResourceManager.ApplyResources(SCIBusPCMWriteMemoryRAMOffsetTextBox, "SCIBusPCMWriteMemoryRAMOffsetTextBox");
		((Control)SCIBusPCMWriteMemoryRAMOffsetTextBox).Name = "SCIBusPCMWriteMemoryRAMOffsetTextBox";
		((Control)SCIBusPCMWriteMemoryRAMOffsetTextBox).TextChanged += SCIBusPCMWriteMemoryRAMOffsetAndCountAndValueTextBox_TextChanged;
		((Control)SCIBusPCMWriteMemoryRAMOffsetTextBox).KeyPress += new KeyPressEventHandler(SCIBusPCMWriteMemoryRAMOffsetTextBox_KeyPress);
		componentResourceManager.ApplyResources(SCIBusPCMWriteMemoryRAMReadButton, "SCIBusPCMWriteMemoryRAMReadButton");
		((Control)SCIBusPCMWriteMemoryRAMReadButton).Name = "SCIBusPCMWriteMemoryRAMReadButton";
		((ButtonBase)SCIBusPCMWriteMemoryRAMReadButton).UseVisualStyleBackColor = true;
		((Control)SCIBusPCMWriteMemoryRAMReadButton).Click += SCIBusPCMWriteMemoryRAMReadButton_Click;
		componentResourceManager.ApplyResources(SCIBusPCMWriteMemoryRAMValueTextBox, "SCIBusPCMWriteMemoryRAMValueTextBox");
		((Control)SCIBusPCMWriteMemoryRAMValueTextBox).Name = "SCIBusPCMWriteMemoryRAMValueTextBox";
		((Control)SCIBusPCMWriteMemoryRAMValueTextBox).TextChanged += SCIBusPCMWriteMemoryRAMOffsetAndCountAndValueTextBox_TextChanged;
		((Control)SCIBusPCMWriteMemoryRAMValueTextBox).KeyPress += new KeyPressEventHandler(SCIBusPCMWriteMemoryRAMValueTextBox_KeyPress);
		componentResourceManager.ApplyResources(SCIBusPCMWriteMemoryRAMOffsetLabel, "SCIBusPCMWriteMemoryRAMOffsetLabel");
		((Control)SCIBusPCMWriteMemoryRAMOffsetLabel).Name = "SCIBusPCMWriteMemoryRAMOffsetLabel";
		componentResourceManager.ApplyResources(SCIBusPCMWriteMemoryRAMValueLabel, "SCIBusPCMWriteMemoryRAMValueLabel");
		((Control)SCIBusPCMWriteMemoryRAMValueLabel).Name = "SCIBusPCMWriteMemoryRAMValueLabel";
		componentResourceManager.ApplyResources(SCIBusPCMWriteMemoryHelpButton, "SCIBusPCMWriteMemoryHelpButton");
		((Control)SCIBusPCMWriteMemoryHelpButton).Name = "SCIBusPCMWriteMemoryHelpButton";
		((ButtonBase)SCIBusPCMWriteMemoryHelpButton).UseVisualStyleBackColor = true;
		((Control)SCIBusPCMWriteMemoryHelpButton).Click += SCIBusPCMWriteMemoryHelpButton_Click;
		((Control)SCIBusPCMWriteMemoryInfoTextBox).BackColor = SystemColors.Window;
		componentResourceManager.ApplyResources(SCIBusPCMWriteMemoryInfoTextBox, "SCIBusPCMWriteMemoryInfoTextBox");
		((Control)SCIBusPCMWriteMemoryInfoTextBox).Name = "SCIBusPCMWriteMemoryInfoTextBox";
		((TextBoxBase)SCIBusPCMWriteMemoryInfoTextBox).ReadOnly = true;
		componentResourceManager.ApplyResources(this, "$this");
		((ContainerControl)this).AutoScaleMode = (AutoScaleMode)2;
		((Control)this).Controls.Add((Control)(object)WriteMemoryTabControl);
		((Control)this).Name = "ReadWriteMemoryForm";
		((Form)this).FormClosing += new FormClosingEventHandler(WriteMemoryForm_FormClosing);
		((Form)this).Load += ReadWriteMemoryForm_Load;
		((Control)WriteMemoryTabControl).ResumeLayout(false);
		((Control)CCDBusTabPage).ResumeLayout(false);
		((Control)CCDBusTabPage).PerformLayout();
		((Control)groupBox1).ResumeLayout(false);
		((Control)groupBox1).PerformLayout();
		((Control)SCIBusPCMTabPage).ResumeLayout(false);
		((Control)SCIBusPCMTabPage).PerformLayout();
		((Control)SCIBusPCMWriteMemoryEEPROMGroupBox).ResumeLayout(false);
		((Control)SCIBusPCMWriteMemoryEEPROMGroupBox).PerformLayout();
		((Control)SCIBusPCMWriteMemoryRAMGroupBox).ResumeLayout(false);
		((Control)SCIBusPCMWriteMemoryRAMGroupBox).PerformLayout();
		((Control)this).ResumeLayout(false);
	}
}
