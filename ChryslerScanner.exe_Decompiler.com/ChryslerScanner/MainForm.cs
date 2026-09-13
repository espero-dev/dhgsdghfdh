using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Timers;
using System.Windows.Forms;
using ChryslerScanner.Helpers;
using ChryslerScanner.Languages;
using ChryslerScanner.Models;
using ChryslerScanner.Properties;
using ChryslerScanner.Services;
using DRBDBReader.DB;

namespace ChryslerScanner;

public class MainForm : Form
{
	private readonly SynchronizationContext UIContext;

	private bool GUIUpdateAvailable;

	private bool FWUpdateAvailabile;

	private bool ResetFromUpdate;

	public string GUIVersion = string.Empty;

	public string FWVersion = string.Empty;

	public string HWVersion = string.Empty;

	private ulong DeviceFirmwareTimestamp;

	private string SelectedPort = string.Empty;

	private bool timeout;

	private bool DeviceFound;

	private const uint IntEEPROMsize = 4096u;

	private const uint ExtEEPROMsize = 4096u;

	private static bool PCMSelected = true;

	private static bool TCMSelected = false;

	public static string USBTextLogFilename;

	public static string USBBinaryLogFilename;

	public static string CCDLogFilename;

	public static string CCDB2F2LogFilename;

	public static string CCDEPROMTextFilename;

	public static string CCDEPROMBinaryFilename;

	public static string CCDEEPROMTextFilename;

	public static string CCDEEPROMBinaryFilename;

	public static string PCILogFilename;

	public static string PCMLogFilename;

	public static string PCMFlashTextFilename;

	public static string PCMEPROMBinaryFilename;

	public static string PCMEEPROMTextFilename;

	public static string PCMEEPROMBinaryFilename;

	public static string TCMLogFilename;

	private int LastCCDScrollBarPosition;

	private int LastPCIScrollBarPosition;

	private int LastPCMScrollBarPosition;

	private int LastTCMScrollBarPosition;

	private static ushort TableRefreshRate = 0;

	private List<string> CCDTableBuffer = new List<string>();

	private List<int> CCDTableBufferLocation = new List<int>();

	private List<int> CCDTableRowCountHistory = new List<int>();

	private List<string> PCITableBuffer = new List<string>();

	private List<int> PCITableBufferLocation = new List<int>();

	private List<int> PCITableRowCountHistory = new List<int>();

	private List<string> PCMTableBuffer = new List<string>();

	private List<int> PCMTableBufferLocation = new List<int>();

	private List<int> PCMTableRowCountHistory = new List<int>();

	private List<string> TCMTableBuffer = new List<string>();

	private List<int> TCMTableBufferLocation = new List<int>();

	private List<int> TCMTableRowCountHistory = new List<int>();

	private readonly SerialService SerialService;

	private ReadMemoryForm ReadMemory;

	private ReadWriteMemoryForm ReadWriteMemory;

	private BootstrapToolsForm BootstrapTools;

	private EngineToolsForm EngineTools;

	private ABSToolsForm ABSTools;

	private AboutForm About;

	public CCD CCD = new CCD();

	public PCI PCI = new PCI();

	public SCIPCM PCM = new SCIPCM();

	public SCITCM TCM = new SCITCM();

	private System.Timers.Timer TimeoutTimer = new System.Timers.Timer();

	private System.Timers.Timer CCDTableRefreshTimer = new System.Timers.Timer();

	private System.Timers.Timer PCITableRefreshTimer = new System.Timers.Timer();

	private System.Timers.Timer PCMTableRefreshTimer = new System.Timers.Timer();

	private System.Timers.Timer TCMTableRefreshTimer = new System.Timers.Timer();

	private WebClient Downloader = new WebClient();

	private FileInfo fi = new FileInfo("DRBDBReader/database.mem");

	private Database db;

	private const string BaseGithubUrl = "https://raw.githubusercontent.com/laszlodaniel/ChryslerScanner/master";

	private IContainer components;

	private MenuStrip MenuStrip;

	private ToolStripMenuItem ToolsToolStripMenuItem;

	private ToolStripMenuItem UpdateToolStripMenuItem;

	private ToolStripMenuItem ReadMemoryToolStripMenuItem;

	private ToolStripMenuItem SettingsToolStripMenuItem;

	private ToolStripMenuItem UnitToolStripMenuItem;

	private ToolStripMenuItem MetricUnitsToolStripMenuItem;

	private ToolStripMenuItem ImperialUnitsToolStripMenuItem;

	private ToolStripMenuItem IncludeTimestampInLogFilesToolStripMenuItem;

	private ToolStripMenuItem AboutToolStripMenuItem;

	private GroupBox USBCommunicationGroupBox;

	private TextBox USBTextBox;

	private Button USBSendPacketButton;

	private Button ResetButton;

	private GroupBox ControlPanelGroupBox;

	private Button ConnectButton;

	private ComboBox COMPortsComboBox;

	private Button COMPortsRefreshButton;

	private Button ExpandButton;

	private TabControl ScannerTabControl;

	private TabPage ScannerControlTabPage;

	private Button HandshakeButton;

	private Button StatusButton;

	private Button DebugRandomCCDBusMessagesButton;

	private Button VersionInfoButton;

	private Button TimestampButton;

	private Button VoltagesButton;

	private Label MainLabel;

	private Label RequestLabel;

	private Button EEPROMChecksumButton;

	private Button ReadEEPROMButton;

	private Label DebugLabel;

	private TextBox EEPROMReadAddressTextBox;

	private Label EEPROMReadAddressLabel;

	private RadioButton ExternalEEPROMRadioButton;

	private RadioButton InternalEEPROMRadioButton;

	private Label EEPROMWriteAddressLabel;

	private TextBox EEPROMWriteAddressTextBox;

	private Button WriteEEPROMButton;

	private Label EEPROMWriteValuesLabel;

	private TextBox EEPROMWriteValuesTextBox;

	private Label EEPROMReadCountLabel;

	private TextBox EEPROMReadCountTextBox;

	private CheckBox EEPROMWriteEnableCheckBox;

	private Button SetLEDsButton;

	private Label SettingsLabel;

	private Label HeartbeatIntervalLabel;

	private Label MillisecondsLabel01;

	private TextBox HeartbeatIntervalTextBox;

	private Label MillisecondsLabel02;

	private TextBox LEDBlinkDurationTextBox;

	private Label LEDBlinkDurationLabel;

	private TabPage CCDBusControlTabPage;

	private ListBox CCDBusTxMessagesListBox;

	private ComboBox USBSendPacketComboBox;

	private Button CCDBusTxMessageAddButton;

	private ComboBox CCDBusTxMessageComboBox;

	private Button CCDBusTxMessageRemoveItemButton;

	private Button CCDBusTxMessageClearListButton;

	private CheckBox CCDBusOverwriteDuplicateIDCheckBox;

	private CheckBox CCDBusTxMessageChecksumCheckBox;

	private Button CCDBusSendMessagesButton;

	private CheckBox CCDBusTxMessageRepeatIntervalCheckBox;

	private Label MillisecondsLabel03;

	private TextBox CCDBusTxMessageRepeatIntervalTextBox;

	private Button CCDBusStopRepeatedMessagesButton;

	private Label CCDBusRandomMessageIntervalMinLabel;

	private TextBox CCDBusRandomMessageIntervalMinTextBox;

	private Label CCDBusRandomMessageIntervalMaxLabel;

	private TextBox CCDBusRandomMessageIntervalMaxTextBox;

	private Label MillisecondsLabel04;

	private CheckBox CCDBusTerminationBiasOnOffCheckBox;

	private Button MeasureCCDBusVoltagesButton;

	private CheckBox CCDBusTransceiverOnOffCheckBox;

	private TabPage SCIBusControlTabPage;

	private Label MillisecondsLabel05;

	private TextBox SCIBusTxMessageRepeatIntervalTextBox;

	private CheckBox SCIBusTxMessageRepeatIntervalCheckBox;

	private Button SCIBusSendMessagesButton;

	private CheckBox SCIBusTxMessageChecksumCheckBox;

	private CheckBox SCIBusOverwriteDuplicateIDCheckBox;

	private Button SCIBusTxMessageClearListButton;

	private Button SCIBusTxMessageRemoveItemButton;

	private Button SCIBusTxMessageAddButton;

	private ComboBox SCIBusTxMessageComboBox;

	private ListBox SCIBusTxMessagesListBox;

	private Button SCIBusStopRepeatedMessagesButton;

	private ComboBox SCIBusModuleComboBox;

	private Label SCIBusModuleLabel;

	private ComboBox SCIBusSpeedComboBox;

	private Label SCIBusSpeedLabel;

	private Button SCIBusModuleConfigSpeedApplyButton;

	private ComboBox SCIBusOBDConfigurationComboBox;

	private Label SCIBusOBDConfigurationLabel;

	private TabPage LCDControlTabPage;

	private Button LCDApplySettingsButton;

	private Label LCDRefreshRateLabel;

	private Label HzLabel01;

	private TextBox LCDRefreshRateTextBox;

	private Label LCDSizeLabel;

	private Label LCDRowLabel;

	private TextBox LCDHeightTextBox;

	private Label LCDColumnLabel;

	private TextBox LCDWidthTextBox;

	private ComboBox LCDStateComboBox;

	private Label LCDStateLabel;

	private TextBox LCDPreviewTextBox;

	private ComboBox LCDDataSourceComboBox;

	private Label LCDDataSourceLabel;

	private Label LCDPreviewLabel;

	private Label LCDI2CAddressHexLabel;

	private TextBox LCDI2CAddressTextBox;

	private Label LCDI2CAddressLabel;

	private GroupBox DiagnosticsGroupBox;

	private TabControl DiagnosticsTabControl;

	private TabPage CCDBusDiagnosticsTabPage;

	private FlickerFreeListBox CCDBusDiagnosticsListBox;

	private TabPage SCIBusPCMDiagnosticsTabPage;

	private FlickerFreeListBox SCIBusPCMDiagnosticsListBox;

	private TabPage SCIBusTCMDiagnosticsTabPage;

	private FlickerFreeListBox SCIBusTCMDiagnosticsListBox;

	private Button DiagnosticsRefreshButton;

	private Button DiagnosticsResetViewButton;

	private Button DiagnosticsCopyToClipboardButton;

	private Button DiagnosticsSnapshotButton;

	private ToolStripMenuItem CCDBusOnDemandToolStripMenuItem;

	private ToolStripMenuItem ReadWriteMemoryToolStripMenuItem;

	private ToolStripMenuItem BootstrapToolsToolStripMenuItem;

	private TabPage PCIBusControlTabPage;

	private TabPage PCIBusDiagnosticsTabPage;

	private CheckBox PCIBusTransceiverOnOffCheckBox;

	private Button PCIBusStopRepeatedMessagesButton;

	private Label MillisecondsLabel06;

	private TextBox PCIBusTxMessageRepeatIntervalTextBox;

	private CheckBox PCIBusTxMessageRepeatIntervalCheckBox;

	private Button PCIBusSendMessagesButton;

	private CheckBox PCIBusTxMessageCRCCheckBox;

	private CheckBox PCIBusOverwriteDuplicateIDCheckBox;

	private Button PCIBusTxMessageClearListButton;

	private Button PCIBusTxMessageRemoveItemButton;

	private Button PCIBusTxMessageAddButton;

	private ComboBox PCIBusTxMessageComboBox;

	private ListBox PCIBusTxMessagesListBox;

	private FlickerFreeListBox PCIBusDiagnosticsListBox;

	private ToolStripMenuItem EngineToolsToolStripMenuItem;

	private ToolStripMenuItem PCIBusOnDemandToolStripMenuItem;

	private ToolStripMenuItem SortMessagesByIDByteToolStripMenuItem;

	private ToolStripMenuItem ABSToolsToolStripMenuItem;

	private Button DemoButton;

	private ToolStripMenuItem LanguageToolStripMenuItem;

	private ToolStripMenuItem EnglishLangToolStripMenuItem;

	private ToolStripMenuItem SpanishLangToolStripMenuItem;

	private ToolStripMenuItem DisplayRawBusPacketsToolStripMenuItem;

	private ToolStripMenuItem UARTBaudrateToolStripMenuItem;

	private ToolStripMenuItem Baudrate250000ToolStripMenuItem;

	private ToolStripMenuItem Baudrate115200ToolStripMenuItem;

	private ComboBox SCIBusLogicComboBox;

	private Label SCIBusLogicLabel;

	public MainForm(SerialService service)
	{
		InitializeComponent();
		UIContext = SynchronizationContext.Current;
		((Form)this).Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
		((Control)DiagnosticsGroupBox).Visible = false;
		((Form)this).Size = new Size(405, 650);
		((Form)this).CenterToScreen();
		SerialService = service;
		Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US", useUserOverride: true);
		if (fi.Exists)
		{
			db = new Database(fi);
		}
		GUIVersion = "v" + Assembly.GetExecutingAssembly().GetName().Version.ToString(3);
		((Control)this).Text = ((Control)this).Text + "  |  GUI " + GUIVersion;
		if (!Directory.Exists("LOG"))
		{
			Directory.CreateDirectory("LOG");
		}
		if (!Directory.Exists("LOG/ABS"))
		{
			Directory.CreateDirectory("LOG/ABS");
		}
		if (!Directory.Exists("LOG/CCD"))
		{
			Directory.CreateDirectory("LOG/CCD");
		}
		if (!Directory.Exists("LOG/PCI"))
		{
			Directory.CreateDirectory("LOG/PCI");
		}
		if (!Directory.Exists("LOG/SCI"))
		{
			Directory.CreateDirectory("LOG/SCI");
		}
		if (!Directory.Exists("LOG/PCM"))
		{
			Directory.CreateDirectory("LOG/PCM");
		}
		if (!Directory.Exists("LOG/TCM"))
		{
			Directory.CreateDirectory("LOG/TCM");
		}
		if (!Directory.Exists("LOG/USB"))
		{
			Directory.CreateDirectory("LOG/USB");
		}
		if (!Directory.Exists("ROMs"))
		{
			Directory.CreateDirectory("ROMs");
		}
		if (!Directory.Exists("ROMs/CCD"))
		{
			Directory.CreateDirectory("ROMs/CCD");
		}
		if (!Directory.Exists("ROMs/PCM"))
		{
			Directory.CreateDirectory("ROMs/PCM");
		}
		string text = DateTime.Now.ToString("yyyyMMdd_HHmmss");
		USBTextLogFilename = "LOG/USB/usblog_" + text + ".txt";
		USBBinaryLogFilename = "LOG/USB/usblog_" + text + ".bin";
		CCDLogFilename = "LOG/CCD/ccdlog_" + text + ".txt";
		CCDB2F2LogFilename = "LOG/CCD/ccdb2f2log_" + text + ".txt";
		PCILogFilename = "LOG/PCI/pcilog_" + text + ".txt";
		PCMLogFilename = "LOG/PCM/pcmlog_" + text + ".txt";
		TCMLogFilename = "LOG/TCM/tcmlog_" + text + ".txt";
		TimeoutTimer.Elapsed += TimeoutHandler;
		TimeoutTimer.Interval = 2000.0;
		TimeoutTimer.AutoReset = false;
		TimeoutTimer.Enabled = true;
		TimeoutTimer.Stop();
		timeout = false;
		CCDTableRefreshTimer.Elapsed += CCDTableRefreshHandler;
		CCDTableRefreshTimer.Interval = 10.0;
		CCDTableRefreshTimer.AutoReset = true;
		CCDTableRefreshTimer.Enabled = true;
		PCITableRefreshTimer.Elapsed += PCITableRefreshHandler;
		PCITableRefreshTimer.Interval = 10.0;
		PCITableRefreshTimer.AutoReset = true;
		PCITableRefreshTimer.Enabled = true;
		PCMTableRefreshTimer.Elapsed += PCMTableRefreshHandler;
		PCMTableRefreshTimer.Interval = 10.0;
		PCMTableRefreshTimer.AutoReset = true;
		PCMTableRefreshTimer.Enabled = true;
		TCMTableRefreshTimer.Elapsed += TCMTableRefreshHandler;
		TCMTableRefreshTimer.Interval = 10.0;
		TCMTableRefreshTimer.AutoReset = true;
		TCMTableRefreshTimer.Enabled = true;
		UpdateCOMPortList();
		ObjectCollection items = ((ListBox)CCDBusDiagnosticsListBox).Items;
		object[] array = CCD.Diagnostics.Table.ToArray();
		items.AddRange(array);
		ObjectCollection items2 = ((ListBox)PCIBusDiagnosticsListBox).Items;
		array = PCI.Diagnostics.Table.ToArray();
		items2.AddRange(array);
		ObjectCollection items3 = ((ListBox)SCIBusPCMDiagnosticsListBox).Items;
		array = PCM.Diagnostics.Table.ToArray();
		items3.AddRange(array);
		ObjectCollection items4 = ((ListBox)SCIBusTCMDiagnosticsListBox).Items;
		array = TCM.Diagnostics.Table.ToArray();
		items4.AddRange(array);
		((ListControl)SCIBusModuleComboBox).SelectedIndex = 0;
		((ListControl)SCIBusOBDConfigurationComboBox).SelectedIndex = 0;
		((ListControl)SCIBusSpeedComboBox).SelectedIndex = 2;
		((ListControl)SCIBusLogicComboBox).SelectedIndex = 2;
		((ListControl)LCDStateComboBox).SelectedIndex = 0;
		((ListControl)LCDDataSourceComboBox).SelectedIndex = 0;
		if (Settings.Default.Units == "metric")
		{
			MetricUnitsToolStripMenuItem.Checked = true;
			ImperialUnitsToolStripMenuItem.Checked = false;
		}
		else if (Settings.Default.Units == "imperial")
		{
			MetricUnitsToolStripMenuItem.Checked = false;
			ImperialUnitsToolStripMenuItem.Checked = true;
		}
		if (Settings.Default.Language == "English")
		{
			EnglishLangToolStripMenuItem.Checked = true;
			SpanishLangToolStripMenuItem.Checked = false;
		}
		else if (Settings.Default.Language == "Spanish")
		{
			EnglishLangToolStripMenuItem.Checked = false;
			SpanishLangToolStripMenuItem.Checked = true;
		}
		ChangeLanguage();
		if (Settings.Default.UART0Baudrate == 250000)
		{
			Baudrate250000ToolStripMenuItem.Checked = true;
			Baudrate115200ToolStripMenuItem.Checked = false;
		}
		else if (Settings.Default.UART0Baudrate == 115200)
		{
			Baudrate250000ToolStripMenuItem.Checked = false;
			Baudrate115200ToolStripMenuItem.Checked = true;
		}
		if (Settings.Default.Timestamp)
		{
			IncludeTimestampInLogFilesToolStripMenuItem.Checked = true;
		}
		else
		{
			IncludeTimestampInLogFilesToolStripMenuItem.Checked = false;
		}
		if (Settings.Default.CCDBusOnDemand)
		{
			CCDBusOnDemandToolStripMenuItem.Checked = true;
		}
		else
		{
			CCDBusOnDemandToolStripMenuItem.Checked = false;
		}
		if (Settings.Default.PCIBusOnDemand)
		{
			PCIBusOnDemandToolStripMenuItem.Checked = true;
		}
		else
		{
			PCIBusOnDemandToolStripMenuItem.Checked = false;
		}
		if (Settings.Default.SortByID)
		{
			SortMessagesByIDByteToolStripMenuItem.Checked = true;
		}
		else
		{
			SortMessagesByIDByteToolStripMenuItem.Checked = false;
		}
		if (Settings.Default.DisplayRawBusPackets)
		{
			DisplayRawBusPacketsToolStripMenuItem.Checked = true;
		}
		else
		{
			DisplayRawBusPacketsToolStripMenuItem.Checked = false;
		}
		((ContainerControl)this).ActiveControl = (Control)(object)ConnectButton;
	}

	private void TimeoutHandler(object source, ElapsedEventArgs e)
	{
		timeout = true;
	}

	private void CCDTableRefreshHandler(object source, ElapsedEventArgs e)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Expected O, but got Unknown
		if (CCDTableBuffer.Count == 0)
		{
			return;
		}
		((Control)CCDBusDiagnosticsListBox).BeginInvoke((Delegate)(MethodInvoker)delegate
		{
			((ListBox)CCDBusDiagnosticsListBox).BeginUpdate();
			LastCCDScrollBarPosition = CCDBusDiagnosticsListBox.GetVerticalScrollPosition();
			((ListBox)CCDBusDiagnosticsListBox).Items.RemoveAt(1);
			((ListBox)CCDBusDiagnosticsListBox).Items.Insert(1, (object)CCD.Diagnostics.Table[1]);
			for (int i = 0; i < CCDTableBuffer.Count; i++)
			{
				if (((ListBox)CCDBusDiagnosticsListBox).Items.Count == CCDTableRowCountHistory[i] && CCDTableBufferLocation[i] < ((ListBox)CCDBusDiagnosticsListBox).Items.Count)
				{
					((ListBox)CCDBusDiagnosticsListBox).Items.RemoveAt(CCDTableBufferLocation[i]);
				}
				if (CCDTableBufferLocation[i] <= ((ListBox)CCDBusDiagnosticsListBox).Items.Count)
				{
					((ListBox)CCDBusDiagnosticsListBox).Items.Insert(CCDTableBufferLocation[i], (object)CCDTableBuffer[i]);
				}
			}
			CCDBusDiagnosticsListBox.SetVerticalScrollPosition(LastCCDScrollBarPosition);
			((ListBox)CCDBusDiagnosticsListBox).EndUpdate();
			CCDTableBuffer.Clear();
			CCDTableBufferLocation.Clear();
			CCDTableRowCountHistory.Clear();
		});
	}

	private void PCITableRefreshHandler(object source, ElapsedEventArgs e)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Expected O, but got Unknown
		if (PCITableBuffer.Count == 0)
		{
			return;
		}
		((Control)PCIBusDiagnosticsListBox).BeginInvoke((Delegate)(MethodInvoker)delegate
		{
			((ListBox)PCIBusDiagnosticsListBox).BeginUpdate();
			LastPCIScrollBarPosition = PCIBusDiagnosticsListBox.GetVerticalScrollPosition();
			((ListBox)PCIBusDiagnosticsListBox).Items.RemoveAt(1);
			((ListBox)PCIBusDiagnosticsListBox).Items.Insert(1, (object)PCI.Diagnostics.Table[1]);
			for (int i = 0; i < PCITableBuffer.Count; i++)
			{
				if (((ListBox)PCIBusDiagnosticsListBox).Items.Count == PCITableRowCountHistory[i] && PCITableBufferLocation[i] < ((ListBox)PCIBusDiagnosticsListBox).Items.Count)
				{
					((ListBox)PCIBusDiagnosticsListBox).Items.RemoveAt(PCITableBufferLocation[i]);
				}
				if (PCITableBufferLocation[i] <= ((ListBox)PCIBusDiagnosticsListBox).Items.Count)
				{
					((ListBox)PCIBusDiagnosticsListBox).Items.Insert(PCITableBufferLocation[i], (object)PCITableBuffer[i]);
				}
			}
			PCIBusDiagnosticsListBox.SetVerticalScrollPosition(LastPCIScrollBarPosition);
			((ListBox)PCIBusDiagnosticsListBox).EndUpdate();
			PCITableBuffer.Clear();
			PCITableBufferLocation.Clear();
			PCITableRowCountHistory.Clear();
		});
	}

	private void PCMTableRefreshHandler(object source, ElapsedEventArgs e)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Expected O, but got Unknown
		if (PCMTableBuffer.Count == 0)
		{
			return;
		}
		((Control)SCIBusPCMDiagnosticsListBox).BeginInvoke((Delegate)(MethodInvoker)delegate
		{
			((ListBox)SCIBusPCMDiagnosticsListBox).BeginUpdate();
			LastPCMScrollBarPosition = SCIBusPCMDiagnosticsListBox.GetVerticalScrollPosition();
			((ListBox)SCIBusPCMDiagnosticsListBox).Items.RemoveAt(1);
			((ListBox)SCIBusPCMDiagnosticsListBox).Items.Insert(1, (object)PCM.Diagnostics.Table[1]);
			for (int i = 0; i < PCMTableBuffer.Count; i++)
			{
				if (((ListBox)SCIBusPCMDiagnosticsListBox).Items.Count == PCMTableRowCountHistory[i] && PCMTableBufferLocation[i] < ((ListBox)SCIBusPCMDiagnosticsListBox).Items.Count)
				{
					((ListBox)SCIBusPCMDiagnosticsListBox).Items.RemoveAt(PCMTableBufferLocation[i]);
				}
				if (PCMTableBufferLocation[i] <= ((ListBox)SCIBusPCMDiagnosticsListBox).Items.Count)
				{
					((ListBox)SCIBusPCMDiagnosticsListBox).Items.Insert(PCMTableBufferLocation[i], (object)PCMTableBuffer[i]);
				}
			}
			SCIBusPCMDiagnosticsListBox.SetVerticalScrollPosition(LastPCMScrollBarPosition);
			((ListBox)SCIBusPCMDiagnosticsListBox).EndUpdate();
			PCMTableBuffer.Clear();
			PCMTableBufferLocation.Clear();
			PCMTableRowCountHistory.Clear();
		});
	}

	private void TCMTableRefreshHandler(object source, ElapsedEventArgs e)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Expected O, but got Unknown
		if (TCMTableBuffer.Count == 0)
		{
			return;
		}
		((Control)SCIBusTCMDiagnosticsListBox).BeginInvoke((Delegate)(MethodInvoker)delegate
		{
			((ListBox)SCIBusTCMDiagnosticsListBox).BeginUpdate();
			LastTCMScrollBarPosition = SCIBusTCMDiagnosticsListBox.GetVerticalScrollPosition();
			((ListBox)SCIBusTCMDiagnosticsListBox).Items.RemoveAt(1);
			((ListBox)SCIBusTCMDiagnosticsListBox).Items.Insert(1, (object)TCM.Diagnostics.Table[1]);
			for (int i = 0; i < TCMTableBuffer.Count; i++)
			{
				if (((ListBox)SCIBusTCMDiagnosticsListBox).Items.Count == TCMTableRowCountHistory[i])
				{
					((ListBox)SCIBusTCMDiagnosticsListBox).Items.RemoveAt(TCMTableBufferLocation[i]);
				}
				if (TCMTableBufferLocation[i] <= ((ListBox)SCIBusTCMDiagnosticsListBox).Items.Count)
				{
					((ListBox)SCIBusTCMDiagnosticsListBox).Items.Insert(TCMTableBufferLocation[i], (object)TCMTableBuffer[i]);
				}
			}
			SCIBusTCMDiagnosticsListBox.SetVerticalScrollPosition(LastTCMScrollBarPosition);
			((ListBox)SCIBusTCMDiagnosticsListBox).EndUpdate();
			TCMTableBuffer.Clear();
			TCMTableBufferLocation.Clear();
			TCMTableRowCountHistory.Clear();
		});
	}

	private void UpdateCOMPortList()
	{
		COMPortsComboBox.Items.Clear();
		string[] portNames = SerialPort.GetPortNames();
		if (portNames.Length != 0)
		{
			ObjectCollection items = COMPortsComboBox.Items;
			object[] array = portNames;
			items.AddRange(array);
			((Control)ConnectButton).Enabled = true;
			if (!(SelectedPort == string.Empty))
			{
				try
				{
					((ListControl)COMPortsComboBox).SelectedIndex = COMPortsComboBox.Items.IndexOf((object)SelectedPort);
					return;
				}
				catch
				{
					((ListControl)COMPortsComboBox).SelectedIndex = 0;
					return;
				}
			}
			((ListControl)COMPortsComboBox).SelectedIndex = 0;
			SelectedPort = ((Control)COMPortsComboBox).Text;
		}
		else
		{
			COMPortsComboBox.Items.Add((object)"N/A");
			((Control)ConnectButton).Enabled = false;
			((ListControl)COMPortsComboBox).SelectedIndex = 0;
			SelectedPort = string.Empty;
			Util.UpdateTextBox(USBTextBox, "[INFO] No device available.");
		}
	}

	private void PacketReceivedHandler(object sender, Packet packet)
	{
		MethodInvoker val21 = default(MethodInvoker);
		MethodInvoker val = default(MethodInvoker);
		MethodInvoker val2 = default(MethodInvoker);
		MethodInvoker val3 = default(MethodInvoker);
		MethodInvoker val4 = default(MethodInvoker);
		MethodInvoker val5 = default(MethodInvoker);
		MethodInvoker val6 = default(MethodInvoker);
		MethodInvoker val9 = default(MethodInvoker);
		MethodInvoker val8 = default(MethodInvoker);
		MethodInvoker val10 = default(MethodInvoker);
		MethodInvoker val7 = default(MethodInvoker);
		MethodInvoker val11 = default(MethodInvoker);
		MethodInvoker val12 = default(MethodInvoker);
		MethodInvoker val13 = default(MethodInvoker);
		MethodInvoker val14 = default(MethodInvoker);
		MethodInvoker val15 = default(MethodInvoker);
		MethodInvoker val16 = default(MethodInvoker);
		MethodInvoker val19 = default(MethodInvoker);
		MethodInvoker val18 = default(MethodInvoker);
		MethodInvoker val20 = default(MethodInvoker);
		MethodInvoker val17 = default(MethodInvoker);
		UIContext.Post(delegate
		{
			//IL_3c37: Unknown result type (might be due to invalid IL or missing references)
			//IL_3c41: Expected O, but got Unknown
			//IL_624e: Unknown result type (might be due to invalid IL or missing references)
			//IL_623a: Unknown result type (might be due to invalid IL or missing references)
			//IL_6409: Unknown result type (might be due to invalid IL or missing references)
			//IL_63f5: Unknown result type (might be due to invalid IL or missing references)
			//IL_6564: Unknown result type (might be due to invalid IL or missing references)
			//IL_6550: Unknown result type (might be due to invalid IL or missing references)
			//IL_671f: Unknown result type (might be due to invalid IL or missing references)
			//IL_670b: Unknown result type (might be due to invalid IL or missing references)
			//IL_2f76: Unknown result type (might be due to invalid IL or missing references)
			//IL_2f7b: Unknown result type (might be due to invalid IL or missing references)
			//IL_2f7e: Expected O, but got Unknown
			//IL_2f83: Expected O, but got Unknown
			//IL_2afb: Unknown result type (might be due to invalid IL or missing references)
			//IL_2b00: Unknown result type (might be due to invalid IL or missing references)
			//IL_2b03: Expected O, but got Unknown
			//IL_2b08: Expected O, but got Unknown
			//IL_2f13: Unknown result type (might be due to invalid IL or missing references)
			//IL_2f18: Unknown result type (might be due to invalid IL or missing references)
			//IL_2f1b: Expected O, but got Unknown
			//IL_2f20: Expected O, but got Unknown
			//IL_2a98: Unknown result type (might be due to invalid IL or missing references)
			//IL_2a9d: Unknown result type (might be due to invalid IL or missing references)
			//IL_2aa0: Expected O, but got Unknown
			//IL_2aa5: Expected O, but got Unknown
			//IL_2fcf: Unknown result type (might be due to invalid IL or missing references)
			//IL_2fd4: Unknown result type (might be due to invalid IL or missing references)
			//IL_2fd7: Expected O, but got Unknown
			//IL_2fdc: Expected O, but got Unknown
			//IL_2b54: Unknown result type (might be due to invalid IL or missing references)
			//IL_2b59: Unknown result type (might be due to invalid IL or missing references)
			//IL_2b5c: Expected O, but got Unknown
			//IL_2b61: Expected O, but got Unknown
			//IL_302f: Unknown result type (might be due to invalid IL or missing references)
			//IL_3034: Unknown result type (might be due to invalid IL or missing references)
			//IL_3037: Expected O, but got Unknown
			//IL_303c: Expected O, but got Unknown
			//IL_30ae: Unknown result type (might be due to invalid IL or missing references)
			//IL_30b3: Unknown result type (might be due to invalid IL or missing references)
			//IL_30b6: Expected O, but got Unknown
			//IL_30bb: Expected O, but got Unknown
			//IL_3078: Unknown result type (might be due to invalid IL or missing references)
			//IL_307d: Unknown result type (might be due to invalid IL or missing references)
			//IL_3080: Expected O, but got Unknown
			//IL_3085: Expected O, but got Unknown
			//IL_2bb4: Unknown result type (might be due to invalid IL or missing references)
			//IL_2bb9: Unknown result type (might be due to invalid IL or missing references)
			//IL_2bbc: Expected O, but got Unknown
			//IL_2bc1: Expected O, but got Unknown
			//IL_2c33: Unknown result type (might be due to invalid IL or missing references)
			//IL_2c38: Unknown result type (might be due to invalid IL or missing references)
			//IL_2c3b: Expected O, but got Unknown
			//IL_2c40: Expected O, but got Unknown
			//IL_2bfd: Unknown result type (might be due to invalid IL or missing references)
			//IL_2c02: Unknown result type (might be due to invalid IL or missing references)
			//IL_2c05: Expected O, but got Unknown
			//IL_2c0a: Expected O, but got Unknown
			//IL_41a4: Unknown result type (might be due to invalid IL or missing references)
			//IL_41ae: Expected O, but got Unknown
			//IL_310f: Unknown result type (might be due to invalid IL or missing references)
			//IL_3114: Unknown result type (might be due to invalid IL or missing references)
			//IL_3117: Expected O, but got Unknown
			//IL_311c: Expected O, but got Unknown
			//IL_3148: Unknown result type (might be due to invalid IL or missing references)
			//IL_314d: Unknown result type (might be due to invalid IL or missing references)
			//IL_3150: Expected O, but got Unknown
			//IL_3155: Expected O, but got Unknown
			//IL_317e: Unknown result type (might be due to invalid IL or missing references)
			//IL_3183: Unknown result type (might be due to invalid IL or missing references)
			//IL_3186: Expected O, but got Unknown
			//IL_318b: Expected O, but got Unknown
			//IL_31b4: Unknown result type (might be due to invalid IL or missing references)
			//IL_31b9: Unknown result type (might be due to invalid IL or missing references)
			//IL_31bc: Expected O, but got Unknown
			//IL_31c1: Expected O, but got Unknown
			//IL_2c94: Unknown result type (might be due to invalid IL or missing references)
			//IL_2c99: Unknown result type (might be due to invalid IL or missing references)
			//IL_2c9c: Expected O, but got Unknown
			//IL_2ca1: Expected O, but got Unknown
			//IL_2ccd: Unknown result type (might be due to invalid IL or missing references)
			//IL_2cd2: Unknown result type (might be due to invalid IL or missing references)
			//IL_2cd5: Expected O, but got Unknown
			//IL_2cda: Expected O, but got Unknown
			//IL_2d03: Unknown result type (might be due to invalid IL or missing references)
			//IL_2d08: Unknown result type (might be due to invalid IL or missing references)
			//IL_2d0b: Expected O, but got Unknown
			//IL_2d10: Expected O, but got Unknown
			//IL_2d39: Unknown result type (might be due to invalid IL or missing references)
			//IL_2d3e: Unknown result type (might be due to invalid IL or missing references)
			//IL_2d41: Expected O, but got Unknown
			//IL_2d46: Expected O, but got Unknown
			//IL_26a3: Unknown result type (might be due to invalid IL or missing references)
			//IL_26a8: Unknown result type (might be due to invalid IL or missing references)
			//IL_26ab: Expected O, but got Unknown
			//IL_26b0: Expected O, but got Unknown
			switch (packet.Bus)
			{
			case 0:
				switch (packet.Command)
				{
				case 0:
					if (packet.Payload == null || packet.Payload.Length == 0)
					{
						Util.UpdateTextBox(USBTextBox, "[RX->] Invalid reset packet:", PacketHelper.Serialize(packet));
					}
					else
					{
						switch (packet.Mode)
						{
						case 0:
							Util.UpdateTextBox(USBTextBox, "[RX->] Reset in progress:", PacketHelper.Serialize(packet));
							Util.UpdateTextBox(USBTextBox, "[INFO] Device is resetting, please wait.");
							break;
						case 1:
							Util.UpdateTextBox(USBTextBox, "[RX->] Reset done:", PacketHelper.Serialize(packet));
							switch (packet.Payload[0])
							{
							case 0:
								Util.UpdateTextBox(USBTextBox, "[INFO] Device is ready to accept instructions.");
								break;
							case 1:
								Util.UpdateTextBox(USBTextBox, "[INFO] Reset reason: ESP_RST_POWERON");
								break;
							case 2:
								Util.UpdateTextBox(USBTextBox, "[INFO] Reset reason: ESP_RST_EXT");
								break;
							case 3:
								Util.UpdateTextBox(USBTextBox, "[INFO] Reset reason: ESP_RST_SW");
								break;
							case 4:
								Util.UpdateTextBox(USBTextBox, "[INFO] Reset reason: ESP_RST_PANIC");
								break;
							case 5:
								Util.UpdateTextBox(USBTextBox, "[INFO] Reset reason: ESP_RST_INT_WDT");
								break;
							case 6:
								Util.UpdateTextBox(USBTextBox, "[INFO] Reset reason: ESP_RST_TASK_WDT");
								break;
							case 7:
								Util.UpdateTextBox(USBTextBox, "[INFO] Reset reason: ESP_RST_WDT");
								break;
							case 8:
								Util.UpdateTextBox(USBTextBox, "[INFO] Reset reason: ESP_RST_DEEPSLEEP");
								break;
							case 9:
								Util.UpdateTextBox(USBTextBox, "[INFO] Reset reason: ESP_RST_BROWNOUT");
								break;
							case 10:
								Util.UpdateTextBox(USBTextBox, "[INFO] Reset reason: ESP_RST_SDIO");
								break;
							default:
								Util.UpdateTextBox(USBTextBox, "[INFO] Reset reason: unknown.");
								break;
							}
							if (ResetFromUpdate)
							{
								ResetFromUpdate = false;
								VersionInfoButton_Click(this, EventArgs.Empty);
								Util.UpdateTextBox(USBTextBox, "[INFO] Device firmware updated.");
							}
							break;
						default:
							Util.UpdateTextBox(USBTextBox, "[INFO] Unknown reset packet.");
							break;
						}
					}
					break;
				case 1:
					if (packet.Payload == null)
					{
						Util.UpdateTextBox(USBTextBox, "[RX->] Invalid handshake packet:", PacketHelper.Serialize(packet));
					}
					else
					{
						byte[] array5 = PacketHelper.Serialize(packet);
						Util.UpdateTextBox(USBTextBox, "[RX->] Handshake response:", array5);
						if (Util.CompareArrays(array5, PacketHelper.ExpectedHandshake_V1, 0, PacketHelper.ExpectedHandshake_V1.Length) || Util.CompareArrays(array5, PacketHelper.ExpectedHandshake_V2, 0, PacketHelper.ExpectedHandshake_V2.Length))
						{
							Util.UpdateTextBox(USBTextBox, "[INFO] Handshake OK: " + Encoding.ASCII.GetString(packet.Payload, 0, packet.Payload.Length));
						}
						else
						{
							Util.UpdateTextBox(USBTextBox, "[INFO] Handshake ERROR: " + Encoding.ASCII.GetString(packet.Payload, 0, packet.Payload.Length));
						}
					}
					break;
				case 2:
					if (packet.Payload == null)
					{
						Util.UpdateTextBox(USBTextBox, "[RX->] Invalid status packet:", PacketHelper.Serialize(packet));
					}
					else if (HWVersion.Contains("v1.") && packet.Payload.Length >= 53)
					{
						string text65 = Util.ByteToHexString(packet.Payload, 0, 3);
						text65 = ((packet.Payload[0] != 30 || packet.Payload[1] != 152 || packet.Payload[2] != 1) ? (text65 + " (unknown)") : (text65 + " (ATmega2560)"));
						string empty12 = string.Empty;
						empty12 = ((packet.Payload[3] != 1) ? (empty12 + "no") : (empty12 + "yes"));
						string text66 = Util.ByteToHexString(packet.Payload, 4);
						text66 = ((packet.Payload[4] != packet.Payload[5]) ? (text66 + "!=" + Util.ByteToHexString(packet.Payload, 5) + ", ERROR") : (text66 + "=OK"));
						TimeSpan value2 = TimeSpan.FromMilliseconds((packet.Payload[6] << 24) + (packet.Payload[7] << 16) + (packet.Payload[8] << 8) + packet.Payload[9]);
						string text67 = DateTime.Today.Add(value2).ToString("HH:mm:ss.fff");
						int num14 = (packet.Payload[10] << 8) + packet.Payload[11];
						string text68 = (100.0 * ((8192.0 - (double)num14) / 8192.0)).ToString("0.0") + "% (" + (8192.0 - (double)num14).ToString("0") + "/8192 bytes)";
						string empty13 = string.Empty;
						empty13 = ((packet.Payload[12] != 1) ? "no" : "yes");
						string text69 = ((double)((packet.Payload[13] << 8) + packet.Payload[14]) / 1000.0).ToString("0.000") + " V";
						string empty14 = string.Empty;
						string empty15 = string.Empty;
						string empty16 = string.Empty;
						string empty17 = string.Empty;
						string empty18 = string.Empty;
						string empty19 = string.Empty;
						if (Util.IsBitClear(packet.Payload[15], 7))
						{
							CCDBusTransceiverOnOffCheckBox.CheckedChanged -= CCDBusSettingsCheckBox_CheckedChanged;
							CCDBusTransceiverOnOffCheckBox.Checked = false;
							((Control)CCDBusTransceiverOnOffCheckBox).Text = "CCD-bus transceiver OFF";
							CCDBusTransceiverOnOffCheckBox.CheckedChanged += CCDBusSettingsCheckBox_CheckedChanged;
							empty14 = "disabled";
						}
						else
						{
							CCDBusTransceiverOnOffCheckBox.CheckedChanged -= CCDBusSettingsCheckBox_CheckedChanged;
							CCDBusTransceiverOnOffCheckBox.Checked = true;
							((Control)CCDBusTransceiverOnOffCheckBox).Text = "CCD-bus transceiver ON";
							CCDBusTransceiverOnOffCheckBox.CheckedChanged += CCDBusSettingsCheckBox_CheckedChanged;
							empty14 = "enabled";
						}
						empty15 = ((!Util.IsBitClear(packet.Payload[15], 3)) ? "inverted" : "non-inverted");
						if (Util.IsBitClear(packet.Payload[15], 6))
						{
							CCDBusTerminationBiasOnOffCheckBox.CheckedChanged -= CCDBusSettingsCheckBox_CheckedChanged;
							CCDBusTerminationBiasOnOffCheckBox.Checked = false;
							((Control)CCDBusTerminationBiasOnOffCheckBox).Text = "CCD-bus termination / bias OFF";
							CCDBusTerminationBiasOnOffCheckBox.CheckedChanged += CCDBusSettingsCheckBox_CheckedChanged;
							empty16 = "disabled";
						}
						else
						{
							CCDBusTerminationBiasOnOffCheckBox.CheckedChanged -= CCDBusSettingsCheckBox_CheckedChanged;
							CCDBusTerminationBiasOnOffCheckBox.Checked = true;
							((Control)CCDBusTerminationBiasOnOffCheckBox).Text = "CCD-bus termination / bias ON";
							CCDBusTerminationBiasOnOffCheckBox.CheckedChanged += CCDBusSettingsCheckBox_CheckedChanged;
							empty16 = "enabled";
						}
						empty17 = (packet.Payload[15] & 3) switch
						{
							0 => "976.5 baud", 
							1 => "7812.5 baud", 
							2 => "62500 baud", 
							3 => "125000 baud", 
							_ => "unknown", 
						};
						CCD.UpdateHeader(empty14, empty17, empty15);
						empty18 = ((packet.Payload[16] << 24) + (packet.Payload[17] << 16) + (packet.Payload[18] << 8) + packet.Payload[19]).ToString();
						empty19 = ((packet.Payload[20] << 24) + (packet.Payload[21] << 16) + (packet.Payload[22] << 8) + packet.Payload[23]).ToString();
						string empty20 = string.Empty;
						string empty21 = string.Empty;
						string text70 = string.Empty;
						string text71 = string.Empty;
						string empty22 = string.Empty;
						string empty23 = string.Empty;
						string empty24 = string.Empty;
						string empty25 = string.Empty;
						if (Util.IsBitSet(packet.Payload[24], 7))
						{
							empty20 = "enabled";
							if (Util.IsBitClear(packet.Payload[24], 4) && Util.IsBitClear(packet.Payload[24], 3) && Util.IsBitClear(packet.Payload[24], 6))
							{
								((ListControl)SCIBusLogicComboBox).SelectedIndex = 2;
								text70 = "disabled";
								empty21 += "non-inverted";
							}
							else
							{
								if (Util.IsBitSet(packet.Payload[24], 4))
								{
									text70 = "enabled";
									((ListControl)SCIBusLogicComboBox).SelectedIndex = 3;
								}
								else
								{
									text70 = "disabled";
								}
								if (Util.IsBitSet(packet.Payload[24], 3))
								{
									empty21 += "inverted";
									((ListControl)SCIBusLogicComboBox).SelectedIndex = 1;
								}
								else
								{
									empty21 += "non-inverted";
								}
								if (Util.IsBitSet(packet.Payload[24], 6))
								{
									text71 += "(nibble swap)";
									((ListControl)SCIBusLogicComboBox).SelectedIndex = 0;
								}
							}
							if (Util.IsBitClear(packet.Payload[24], 2))
							{
								empty22 = "A";
								((ListControl)SCIBusOBDConfigurationComboBox).SelectedIndex = 0;
							}
							else
							{
								empty22 = "B";
								((ListControl)SCIBusOBDConfigurationComboBox).SelectedIndex = 1;
							}
							switch (packet.Payload[24] & 3)
							{
							case 0:
								empty23 = "976.5 baud";
								((ListControl)SCIBusSpeedComboBox).SelectedIndex = 1;
								break;
							case 1:
								empty23 = "7812.5 baud";
								((ListControl)SCIBusSpeedComboBox).SelectedIndex = 2;
								break;
							case 2:
								empty23 = "62500 baud";
								((ListControl)SCIBusSpeedComboBox).SelectedIndex = 3;
								break;
							case 3:
								empty23 = "125000 baud";
								((ListControl)SCIBusSpeedComboBox).SelectedIndex = 4;
								break;
							default:
								empty23 = "unknown";
								((ListControl)SCIBusSpeedComboBox).SelectedIndex = 0;
								break;
							}
						}
						else
						{
							empty20 = "disabled";
							empty21 = "-";
							empty22 = "-";
							empty23 = "-";
							((ListControl)SCIBusSpeedComboBox).SelectedIndex = 0;
						}
						PCM.UpdateHeader(empty20, empty23, empty21, empty22);
						empty24 = ((packet.Payload[25] << 24) + (packet.Payload[26] << 16) + (packet.Payload[27] << 8) + packet.Payload[28]).ToString();
						empty25 = ((packet.Payload[29] << 24) + (packet.Payload[30] << 16) + (packet.Payload[31] << 8) + packet.Payload[32]).ToString();
						string empty26 = string.Empty;
						string text72 = string.Empty;
						string text73 = string.Empty;
						string empty27 = string.Empty;
						string empty28 = string.Empty;
						string empty29 = string.Empty;
						string empty30 = string.Empty;
						string empty31 = string.Empty;
						if (Util.IsBitSet(packet.Payload[33], 7))
						{
							empty26 = "enabled";
							if (Util.IsBitClear(packet.Payload[33], 4) && Util.IsBitClear(packet.Payload[33], 3) && Util.IsBitClear(packet.Payload[33], 6))
							{
								((ListControl)SCIBusLogicComboBox).SelectedIndex = 2;
								text73 = "disabled";
								text72 += "non-inverted";
							}
							else
							{
								if (Util.IsBitSet(packet.Payload[33], 4))
								{
									text70 = "enabled";
									((ListControl)SCIBusLogicComboBox).SelectedIndex = 3;
								}
								else
								{
									text70 = "disabled";
								}
								if (Util.IsBitSet(packet.Payload[33], 3))
								{
									empty21 += "inverted";
									((ListControl)SCIBusLogicComboBox).SelectedIndex = 1;
								}
								else
								{
									empty21 += "non-inverted";
								}
								if (Util.IsBitSet(packet.Payload[33], 6))
								{
									text71 += "(nibble swap)";
									((ListControl)SCIBusLogicComboBox).SelectedIndex = 0;
								}
							}
							if (Util.IsBitClear(packet.Payload[33], 2))
							{
								empty28 = "A";
								((ListControl)SCIBusOBDConfigurationComboBox).SelectedIndex = 0;
							}
							else
							{
								empty28 = "B";
								((ListControl)SCIBusOBDConfigurationComboBox).SelectedIndex = 1;
							}
							switch (packet.Payload[33] & 3)
							{
							case 0:
								empty29 = "976.5 baud";
								((ListControl)SCIBusSpeedComboBox).SelectedIndex = 1;
								break;
							case 1:
								empty29 = "7812.5 baud";
								((ListControl)SCIBusSpeedComboBox).SelectedIndex = 2;
								break;
							case 2:
								empty29 = "62500 baud";
								((ListControl)SCIBusSpeedComboBox).SelectedIndex = 3;
								break;
							case 3:
								empty29 = "125000 baud";
								((ListControl)SCIBusSpeedComboBox).SelectedIndex = 4;
								break;
							default:
								empty29 = "unknown";
								((ListControl)SCIBusSpeedComboBox).SelectedIndex = 0;
								break;
							}
						}
						else
						{
							empty26 = "disabled";
							text72 = "-";
							empty28 = "-";
							empty29 = "-";
						}
						TCM.UpdateHeader(empty26, empty29, text72, empty28);
						empty30 = ((packet.Payload[34] << 24) + (packet.Payload[35] << 16) + (packet.Payload[36] << 8) + packet.Payload[37]).ToString();
						empty31 = ((packet.Payload[38] << 24) + (packet.Payload[39] << 16) + (packet.Payload[40] << 8) + packet.Payload[41]).ToString();
						string empty32 = string.Empty;
						string empty33 = string.Empty;
						string empty34 = string.Empty;
						string empty35 = string.Empty;
						string empty36 = string.Empty;
						string empty37 = string.Empty;
						if (packet.Payload[42] == 0)
						{
							((ListControl)LCDStateComboBox).SelectedIndex = 0;
							empty32 = "disabled";
						}
						else
						{
							((ListControl)LCDStateComboBox).SelectedIndex = 1;
							empty32 = "enabled";
						}
						((Control)LCDI2CAddressTextBox).Text = Util.ByteToHexString(packet.Payload, 43);
						empty33 = Util.ByteToHexString(packet.Payload, 43) + " (hex)";
						((Control)LCDWidthTextBox).Text = packet.Payload[44].ToString("0");
						((Control)LCDHeightTextBox).Text = packet.Payload[45].ToString("0");
						empty34 = packet.Payload[44].ToString("0") + "x" + packet.Payload[45].ToString("0") + " characters";
						((Control)LCDRefreshRateTextBox).Text = packet.Payload[46].ToString("0");
						empty35 = packet.Payload[46].ToString("0") + " Hz";
						empty36 = ((packet.Payload[47] != 0) ? "metric" : "imperial");
						switch (packet.Payload[48])
						{
						case 1:
							empty37 = "CCD-bus";
							((ListControl)LCDDataSourceComboBox).SelectedIndex = 0;
							break;
						case 2:
							empty37 = "SCI-bus (PCM)";
							((ListControl)LCDDataSourceComboBox).SelectedIndex = 1;
							break;
						case 3:
							empty37 = "SCI-bus (TCM)";
							((ListControl)LCDDataSourceComboBox).SelectedIndex = 2;
							break;
						default:
							empty37 = "CCD-bus";
							((ListControl)LCDDataSourceComboBox).SelectedIndex = 0;
							break;
						}
						ushort num15 = (ushort)((packet.Payload[49] << 8) + packet.Payload[50]);
						ushort num16 = (ushort)((packet.Payload[51] << 8) + packet.Payload[52]);
						string empty38 = string.Empty;
						empty38 = ((num15 <= 0) ? "disabled" : "enabled");
						string text74 = num16 + " ms";
						string text75 = num15 + " ms";
						((Control)HeartbeatIntervalTextBox).Text = num15.ToString();
						((Control)LEDBlinkDurationTextBox).Text = num16.ToString();
						Util.UpdateTextBox(USBTextBox, "[RX->] Status response:", PacketHelper.Serialize(packet));
						Util.UpdateTextBox(USBTextBox, "[INFO] -------------Device status--------------" + Environment.NewLine + "       AVR signature: " + text65 + Environment.NewLine + "       External EEPROM present: " + empty12 + Environment.NewLine + "       External EEPROM checksum: " + text66 + Environment.NewLine + "       Timestamp: " + text67 + Environment.NewLine + "       RAM usage: " + text68 + Environment.NewLine + "       Connected to vehicle: " + empty13 + Environment.NewLine + "       Battery voltage: " + text69 + Environment.NewLine + "       -------------CCD-bus status-------------" + Environment.NewLine + "       State: " + empty14 + Environment.NewLine + "       Logic: " + empty15 + Environment.NewLine + "       Termination and bias: " + empty16 + Environment.NewLine + "       Speed: " + empty17 + Environment.NewLine + "       Messages received: " + empty18 + Environment.NewLine + "       Messages sent: " + empty19 + Environment.NewLine + "       ----------SCI-bus (PCM) status----------" + Environment.NewLine + "       State: " + empty20 + Environment.NewLine + "       Logic: " + empty21 + " " + text71 + Environment.NewLine + "       NGC mode: " + text70 + Environment.NewLine + "       OBD config.: " + empty22 + Environment.NewLine + "       Speed: " + empty23 + Environment.NewLine + "       Messages received: " + empty24 + Environment.NewLine + "       Messages sent: " + empty25 + Environment.NewLine + "       ----------SCI-bus (TCM) status----------" + Environment.NewLine + "       State: " + empty26 + Environment.NewLine + "       Logic: " + text72 + " " + empty27 + Environment.NewLine + "       NGC mode: " + text73 + Environment.NewLine + "       OBD config.: " + empty28 + Environment.NewLine + "       Speed: " + empty29 + Environment.NewLine + "       Messages received: " + empty30 + Environment.NewLine + "       Messages sent: " + empty31 + Environment.NewLine + "       ---------------LCD status---------------" + Environment.NewLine + "       State: " + empty32 + Environment.NewLine + "       I2C address: " + empty33 + Environment.NewLine + "       Size: " + empty34 + Environment.NewLine + "       Refresh rate: " + empty35 + Environment.NewLine + "       Units: " + empty36 + Environment.NewLine + "       Data source: " + empty37 + Environment.NewLine + "       ---------------LED status---------------" + Environment.NewLine + "       Heartbeat state: " + empty38 + Environment.NewLine + "       Heartbeat interval: " + text75 + Environment.NewLine + "       Blink duration: " + text74);
					}
					else if (HWVersion.Contains("v2.") && packet.Payload.Length >= 45)
					{
						TimeSpan value3 = TimeSpan.FromMilliseconds((packet.Payload[0] << 24) + (packet.Payload[1] << 16) + (packet.Payload[2] << 8) + packet.Payload[3]);
						string text76 = DateTime.Today.Add(value3).ToString("HH:mm:ss.fff");
						int num17 = (packet.Payload[4] << 24) + (packet.Payload[5] << 16) + (packet.Payload[6] << 8) + packet.Payload[7];
						string text77 = (100.0 * ((double)num17 / 327680.0)).ToString("0.0") + "% (" + num17.ToString("0") + "/327680 bytes)";
						string text78 = ((double)((packet.Payload[8] << 8) + packet.Payload[9]) / 1000.0).ToString("0.000") + " V";
						string text79 = ((double)((packet.Payload[10] << 8) + packet.Payload[11]) / 1000.0).ToString("0.000") + " V";
						string text80 = ((double)((packet.Payload[12] << 8) + packet.Payload[13]) / 1000.0).ToString("0.000") + " V";
						string empty39 = string.Empty;
						string empty40 = string.Empty;
						string empty41 = string.Empty;
						string empty42 = string.Empty;
						string empty43 = string.Empty;
						string empty44 = string.Empty;
						if (Util.IsBitClear(packet.Payload[14], 7))
						{
							CCDBusTransceiverOnOffCheckBox.CheckedChanged -= CCDBusSettingsCheckBox_CheckedChanged;
							CCDBusTransceiverOnOffCheckBox.Checked = false;
							((Control)CCDBusTransceiverOnOffCheckBox).Text = "CCD-bus transceiver OFF";
							CCDBusTransceiverOnOffCheckBox.CheckedChanged += CCDBusSettingsCheckBox_CheckedChanged;
							empty39 = "disabled";
						}
						else
						{
							CCDBusTransceiverOnOffCheckBox.CheckedChanged -= CCDBusSettingsCheckBox_CheckedChanged;
							CCDBusTransceiverOnOffCheckBox.Checked = true;
							((Control)CCDBusTransceiverOnOffCheckBox).Text = "CCD-bus transceiver ON";
							CCDBusTransceiverOnOffCheckBox.CheckedChanged += CCDBusSettingsCheckBox_CheckedChanged;
							empty39 = "enabled";
						}
						empty40 = ((!Util.IsBitClear(packet.Payload[14], 3)) ? "inverted" : "non-inverted");
						if (Util.IsBitClear(packet.Payload[14], 6))
						{
							CCDBusTerminationBiasOnOffCheckBox.CheckedChanged -= CCDBusSettingsCheckBox_CheckedChanged;
							CCDBusTerminationBiasOnOffCheckBox.Checked = false;
							((Control)CCDBusTerminationBiasOnOffCheckBox).Text = "CCD-bus termination / bias OFF";
							CCDBusTerminationBiasOnOffCheckBox.CheckedChanged += CCDBusSettingsCheckBox_CheckedChanged;
							empty41 = "disabled";
						}
						else
						{
							CCDBusTerminationBiasOnOffCheckBox.CheckedChanged -= CCDBusSettingsCheckBox_CheckedChanged;
							CCDBusTerminationBiasOnOffCheckBox.Checked = true;
							((Control)CCDBusTerminationBiasOnOffCheckBox).Text = "CCD-bus termination / bias ON";
							CCDBusTerminationBiasOnOffCheckBox.CheckedChanged += CCDBusSettingsCheckBox_CheckedChanged;
							empty41 = "enabled";
						}
						empty42 = (packet.Payload[14] & 3) switch
						{
							0 => "976.5 baud", 
							1 => "7812.5 baud", 
							2 => "62500 baud", 
							3 => "125000 baud", 
							_ => "unknown", 
						};
						CCD.UpdateHeader(empty39, empty42, empty40);
						empty43 = ((packet.Payload[15] << 24) + (packet.Payload[16] << 16) + (packet.Payload[17] << 8) + packet.Payload[18]).ToString();
						empty44 = ((packet.Payload[19] << 24) + (packet.Payload[20] << 16) + (packet.Payload[21] << 8) + packet.Payload[22]).ToString();
						string empty45 = string.Empty;
						string empty46 = string.Empty;
						string empty47 = string.Empty;
						string empty48 = string.Empty;
						string empty49 = string.Empty;
						if (Util.IsBitClear(packet.Payload[23], 7))
						{
							PCIBusTransceiverOnOffCheckBox.CheckedChanged -= PCIBusSettingsCheckBox_CheckedChanged;
							PCIBusTransceiverOnOffCheckBox.Checked = false;
							((Control)PCIBusTransceiverOnOffCheckBox).Text = "PCI-bus transceiver OFF";
							PCIBusTransceiverOnOffCheckBox.CheckedChanged += PCIBusSettingsCheckBox_CheckedChanged;
							empty45 = "disabled";
						}
						else
						{
							PCIBusTransceiverOnOffCheckBox.CheckedChanged -= PCIBusSettingsCheckBox_CheckedChanged;
							PCIBusTransceiverOnOffCheckBox.Checked = true;
							((Control)PCIBusTransceiverOnOffCheckBox).Text = "PCI-bus transceiver ON";
							PCIBusTransceiverOnOffCheckBox.CheckedChanged += PCIBusSettingsCheckBox_CheckedChanged;
							empty45 = "enabled";
						}
						empty46 = ((!Util.IsBitClear(packet.Payload[23], 6)) ? "active-high" : "active-low");
						empty47 = "10416 baud";
						PCI.UpdateHeader(empty45, empty47, empty46);
						empty48 = ((packet.Payload[24] << 24) + (packet.Payload[25] << 16) + (packet.Payload[26] << 8) + packet.Payload[27]).ToString();
						empty49 = ((packet.Payload[28] << 24) + (packet.Payload[29] << 16) + (packet.Payload[30] << 8) + packet.Payload[31]).ToString();
						string empty50 = string.Empty;
						string empty51 = string.Empty;
						string empty52 = string.Empty;
						string text81 = string.Empty;
						string text82 = string.Empty;
						string empty53 = string.Empty;
						string empty54 = string.Empty;
						string empty55 = string.Empty;
						string empty56 = string.Empty;
						if (Util.IsBitSet(packet.Payload[32], 7))
						{
							empty50 = "enabled";
							if (Util.IsBitClear(packet.Payload[32], 4) && Util.IsBitClear(packet.Payload[32], 3) && Util.IsBitClear(packet.Payload[32], 6))
							{
								((ListControl)SCIBusLogicComboBox).SelectedIndex = 2;
								text81 = "disabled";
								empty52 += "non-inverted";
							}
							else
							{
								if (Util.IsBitSet(packet.Payload[32], 4))
								{
									text81 = "enabled";
									((ListControl)SCIBusLogicComboBox).SelectedIndex = 3;
								}
								else
								{
									text81 = "disabled";
								}
								if (Util.IsBitSet(packet.Payload[32], 3))
								{
									empty52 += "inverted";
									((ListControl)SCIBusLogicComboBox).SelectedIndex = 1;
								}
								else
								{
									empty52 += "non-inverted";
								}
								if (Util.IsBitSet(packet.Payload[32], 6))
								{
									text82 += "(nibble swap)";
									((ListControl)SCIBusLogicComboBox).SelectedIndex = 0;
								}
							}
							if (Util.IsBitClear(packet.Payload[32], 2))
							{
								empty53 = "A";
								((ListControl)SCIBusOBDConfigurationComboBox).SelectedIndex = 0;
							}
							else
							{
								empty53 = "B";
								((ListControl)SCIBusOBDConfigurationComboBox).SelectedIndex = 1;
							}
							switch (packet.Payload[32] & 3)
							{
							case 0:
								empty54 = "976.5 baud";
								((ListControl)SCIBusSpeedComboBox).SelectedIndex = 1;
								break;
							case 1:
								empty54 = "7812.5 baud";
								((ListControl)SCIBusSpeedComboBox).SelectedIndex = 2;
								break;
							case 2:
								empty54 = "62500 baud";
								((ListControl)SCIBusSpeedComboBox).SelectedIndex = 3;
								break;
							case 3:
								empty54 = "125000 baud";
								((ListControl)SCIBusSpeedComboBox).SelectedIndex = 4;
								break;
							default:
								empty54 = "unknown";
								((ListControl)SCIBusSpeedComboBox).SelectedIndex = 0;
								break;
							}
						}
						else
						{
							empty50 = "disabled";
							empty52 = "-";
							empty53 = "-";
							empty54 = "-";
							((ListControl)SCIBusSpeedComboBox).SelectedIndex = 0;
						}
						empty55 = ((packet.Payload[33] << 24) + (packet.Payload[34] << 16) + (packet.Payload[35] << 8) + packet.Payload[36]).ToString();
						empty56 = ((packet.Payload[37] << 24) + (packet.Payload[38] << 16) + (packet.Payload[39] << 8) + packet.Payload[40]).ToString();
						if (Util.IsBitClear(packet.Payload[32], 5))
						{
							empty51 = "PCM (Engine)";
							((ListControl)SCIBusModuleComboBox).SelectedIndex = 0;
							PCM.UpdateHeader(empty50, empty54, empty52, empty53);
							TCM.UpdateHeader("disabled", "-", "-", "-");
						}
						else
						{
							empty51 = "TCM (Transmission)";
							((ListControl)SCIBusModuleComboBox).SelectedIndex = 1;
							TCM.UpdateHeader(empty50, empty54, empty52, empty53);
							PCM.UpdateHeader("disabled", "-", "-", "-");
						}
						ushort num18 = (ushort)((packet.Payload[41] << 8) + packet.Payload[42]);
						ushort num19 = (ushort)((packet.Payload[43] << 8) + packet.Payload[44]);
						string empty57 = string.Empty;
						empty57 = ((num18 <= 0) ? "disabled" : "enabled");
						string text83 = num19 + " ms";
						string text84 = num18 + " ms";
						((Control)HeartbeatIntervalTextBox).Text = num18.ToString();
						((Control)LEDBlinkDurationTextBox).Text = num19.ToString();
						Util.UpdateTextBox(USBTextBox, "[RX->] Status response:", PacketHelper.Serialize(packet));
						Util.UpdateTextBox(USBTextBox, "[INFO] -------------Device status--------------" + Environment.NewLine + "       Timestamp: " + text76 + Environment.NewLine + "       RAM usage: " + text77 + Environment.NewLine + "       Battery voltage: " + text78 + Environment.NewLine + "       Bootstrap voltage: " + text79 + Environment.NewLine + "       Programming voltage: " + text80 + Environment.NewLine + "       -------------CCD-bus status-------------" + Environment.NewLine + "       State: " + empty39 + Environment.NewLine + "       Logic: " + empty40 + Environment.NewLine + "       Termination and bias: " + empty41 + Environment.NewLine + "       Speed: " + empty42 + Environment.NewLine + "       Messages received: " + empty43 + Environment.NewLine + "       Messages sent: " + empty44 + Environment.NewLine + "       -------------PCI-bus status-------------" + Environment.NewLine + "       State: " + empty45 + Environment.NewLine + "       Logic: " + empty46 + Environment.NewLine + "       Speed: " + empty47 + Environment.NewLine + "       Messages received: " + empty48 + Environment.NewLine + "       Messages sent: " + empty49 + Environment.NewLine + "       -------------SCI-bus status-------------" + Environment.NewLine + "       Module: " + empty51 + Environment.NewLine + "       State: " + empty50 + Environment.NewLine + "       Logic: " + empty52 + " " + text82 + Environment.NewLine + "       NGC mode: " + text81 + Environment.NewLine + "       OBD config.: " + empty53 + Environment.NewLine + "       Speed: " + empty54 + Environment.NewLine + "       Messages received: " + empty55 + Environment.NewLine + "       Messages sent: " + empty56 + Environment.NewLine + "       ---------------LED status---------------" + Environment.NewLine + "       Heartbeat state: " + empty57 + Environment.NewLine + "       Heartbeat interval: " + text84 + Environment.NewLine + "       Blink duration: " + text83);
						MainForm mainForm21 = this;
						MethodInvoker obj21 = val21;
						if (obj21 == null)
						{
							MethodInvoker val43 = delegate
							{
								((Control)EEPROMChecksumButton).Visible = false;
								((Control)InternalEEPROMRadioButton).Visible = false;
								((Control)ExternalEEPROMRadioButton).Visible = false;
								((Control)DebugLabel).Visible = false;
								((Control)ReadEEPROMButton).Visible = false;
								((Control)EEPROMReadAddressLabel).Visible = false;
								((Control)EEPROMReadAddressTextBox).Visible = false;
								((Control)EEPROMReadCountLabel).Visible = false;
								((Control)EEPROMReadCountTextBox).Visible = false;
								((Control)WriteEEPROMButton).Visible = false;
								((Control)EEPROMWriteAddressLabel).Visible = false;
								((Control)EEPROMWriteAddressTextBox).Visible = false;
								((Control)EEPROMWriteEnableCheckBox).Visible = false;
								((Control)EEPROMWriteValuesLabel).Visible = false;
								((Control)EEPROMWriteValuesTextBox).Visible = false;
								((Control)MeasureCCDBusVoltagesButton).Visible = false;
								((Control)CCDBusTerminationBiasOnOffCheckBox).Enabled = false;
								((Control)LCDApplySettingsButton).Enabled = false;
							};
							MethodInvoker val23 = val43;
							val21 = val43;
							obj21 = val23;
						}
						((Control)mainForm21).BeginInvoke((Delegate)(object)obj21);
					}
					break;
				case 3:
					switch (packet.Mode)
					{
					case 1:
						if (packet.Payload != null && packet.Payload.Length >= 4)
						{
							Util.UpdateTextBox(USBTextBox, "[RX->] LED settings changed:", PacketHelper.Serialize(packet));
							int num12 = (packet.Payload[0] << 8) + packet.Payload[1];
							int num13 = (packet.Payload[2] << 8) + packet.Payload[3];
							string text63 = num12 + " ms";
							string text64 = num13 + " ms";
							string empty11 = string.Empty;
							empty11 = ((num12 <= 0) ? "disabled" : "enabled");
							Util.UpdateTextBox(USBTextBox, "[INFO] LED settings:" + Environment.NewLine + "       Heartbeat state: " + empty11 + Environment.NewLine + "       Heartbeat interval: " + text63 + Environment.NewLine + "       LED blink duration: " + text64);
						}
						else
						{
							Util.UpdateTextBox(USBTextBox, "[RX->] Invalid settings packet:", PacketHelper.Serialize(packet));
						}
						break;
					case 2:
						if (packet.Payload != null && packet.Payload.Length != 0)
						{
							string empty7 = string.Empty;
							string empty8 = string.Empty;
							empty7 = ((!Util.IsBitClear(packet.Payload[0], 7)) ? "enabled" : "disabled");
							empty8 = ((!Util.IsBitClear(packet.Payload[0], 6)) ? "enabled" : "disabled");
							Util.UpdateTextBox(USBTextBox, "[RX->] CCD-bus settings changed:", PacketHelper.Serialize(packet));
							Util.UpdateTextBox(USBTextBox, "[INFO] CCD-bus settings: " + Environment.NewLine + "       - state: " + empty7 + Environment.NewLine + "       - termination and bias: " + empty8);
							CCD.UpdateHeader(empty7);
						}
						else
						{
							Util.UpdateTextBox(USBTextBox, "[RX->] Invalid settings packet:", PacketHelper.Serialize(packet));
						}
						break;
					case 3:
						if (packet.Payload != null && packet.Payload.Length != 0)
						{
							string state2 = string.Empty;
							string text53 = string.Empty;
							string text54 = string.Empty;
							string text55 = string.Empty;
							string text56 = string.Empty;
							string text57 = string.Empty;
							string state3 = string.Empty;
							string text58 = string.Empty;
							string text59 = string.Empty;
							string text60 = string.Empty;
							string text61 = string.Empty;
							string text62 = string.Empty;
							Util.UpdateTextBox(USBTextBox, "[RX->] SCI-bus settings changed:", PacketHelper.Serialize(packet));
							if (Util.IsBitSet(packet.Payload[0], 5))
							{
								if (Util.IsBitClear(packet.Payload[0], 7))
								{
									state3 = "disabled";
								}
								else
								{
									PCMSelected = false;
									TCMSelected = true;
									state3 = "enabled";
									state2 = "disabled";
									if (Util.IsBitClear(packet.Payload[0], 4) && Util.IsBitClear(packet.Payload[0], 3) && Util.IsBitClear(packet.Payload[0], 6))
									{
										MainForm mainForm = this;
										MethodInvoker obj = val;
										if (obj == null)
										{
											MethodInvoker val22 = delegate
											{
												((ListControl)SCIBusLogicComboBox).SelectedIndex = 2;
											};
											MethodInvoker val23 = val22;
											val = val22;
											obj = val23;
										}
										((Control)mainForm).BeginInvoke((Delegate)(object)obj);
										text59 = "disabled";
										text58 += "non-inverted";
									}
									else
									{
										if (Util.IsBitSet(packet.Payload[0], 4))
										{
											text59 = "enabled";
											MainForm mainForm2 = this;
											MethodInvoker obj2 = val2;
											if (obj2 == null)
											{
												MethodInvoker val24 = delegate
												{
													((ListControl)SCIBusLogicComboBox).SelectedIndex = 3;
												};
												MethodInvoker val23 = val24;
												val2 = val24;
												obj2 = val23;
											}
											((Control)mainForm2).BeginInvoke((Delegate)(object)obj2);
										}
										else
										{
											text59 = "disabled";
										}
										if (Util.IsBitSet(packet.Payload[0], 3))
										{
											text58 += "inverted";
											MainForm mainForm3 = this;
											MethodInvoker obj3 = val3;
											if (obj3 == null)
											{
												MethodInvoker val25 = delegate
												{
													((ListControl)SCIBusLogicComboBox).SelectedIndex = 1;
												};
												MethodInvoker val23 = val25;
												val3 = val25;
												obj3 = val23;
											}
											((Control)mainForm3).BeginInvoke((Delegate)(object)obj3);
										}
										else
										{
											text58 += "non-inverted";
										}
										if (Util.IsBitSet(packet.Payload[0], 6))
										{
											text60 += "(nibble swap)";
											MainForm mainForm4 = this;
											MethodInvoker obj4 = val4;
											if (obj4 == null)
											{
												MethodInvoker val26 = delegate
												{
													((ListControl)SCIBusLogicComboBox).SelectedIndex = 0;
												};
												MethodInvoker val23 = val26;
												val4 = val26;
												obj4 = val23;
											}
											((Control)mainForm4).BeginInvoke((Delegate)(object)obj4);
										}
									}
									if (Util.IsBitClear(packet.Payload[0], 2))
									{
										text61 = "A";
										MainForm mainForm5 = this;
										MethodInvoker obj5 = val5;
										if (obj5 == null)
										{
											MethodInvoker val27 = delegate
											{
												((ListControl)SCIBusOBDConfigurationComboBox).SelectedIndex = 0;
											};
											MethodInvoker val23 = val27;
											val5 = val27;
											obj5 = val23;
										}
										((Control)mainForm5).BeginInvoke((Delegate)(object)obj5);
									}
									else
									{
										text61 = "B";
										MainForm mainForm6 = this;
										MethodInvoker obj6 = val6;
										if (obj6 == null)
										{
											MethodInvoker val28 = delegate
											{
												((ListControl)SCIBusOBDConfigurationComboBox).SelectedIndex = 1;
											};
											MethodInvoker val23 = val28;
											val6 = val28;
											obj6 = val23;
										}
										((Control)mainForm6).BeginInvoke((Delegate)(object)obj6);
									}
									switch (packet.Payload[0] & 3)
									{
									case 0:
									{
										text62 = "976.5 baud";
										MainForm mainForm9 = this;
										MethodInvoker obj9 = val9;
										if (obj9 == null)
										{
											MethodInvoker val31 = delegate
											{
												((ListControl)SCIBusSpeedComboBox).SelectedIndex = 1;
											};
											MethodInvoker val23 = val31;
											val9 = val31;
											obj9 = val23;
										}
										((Control)mainForm9).BeginInvoke((Delegate)(object)obj9);
										break;
									}
									case 1:
									{
										text62 = "7812.5 baud";
										MainForm mainForm8 = this;
										MethodInvoker obj8 = val8;
										if (obj8 == null)
										{
											MethodInvoker val30 = delegate
											{
												((ListControl)SCIBusSpeedComboBox).SelectedIndex = 2;
											};
											MethodInvoker val23 = val30;
											val8 = val30;
											obj8 = val23;
										}
										((Control)mainForm8).BeginInvoke((Delegate)(object)obj8);
										break;
									}
									case 2:
									{
										text62 = "62500 baud";
										MainForm mainForm10 = this;
										MethodInvoker obj10 = val10;
										if (obj10 == null)
										{
											MethodInvoker val32 = delegate
											{
												((ListControl)SCIBusSpeedComboBox).SelectedIndex = 3;
											};
											MethodInvoker val23 = val32;
											val10 = val32;
											obj10 = val23;
										}
										((Control)mainForm10).BeginInvoke((Delegate)(object)obj10);
										break;
									}
									case 3:
									{
										text62 = "125000 baud";
										MainForm mainForm7 = this;
										MethodInvoker obj7 = val7;
										if (obj7 == null)
										{
											MethodInvoker val29 = delegate
											{
												((ListControl)SCIBusSpeedComboBox).SelectedIndex = 4;
											};
											MethodInvoker val23 = val29;
											val7 = val29;
											obj7 = val23;
										}
										((Control)mainForm7).BeginInvoke((Delegate)(object)obj7);
										break;
									}
									}
								}
								if (state3 == "enabled")
								{
									Util.UpdateTextBox(USBTextBox, "[INFO] TCM settings: " + Environment.NewLine + "       - state: " + state3 + Environment.NewLine + "       - logic: " + text58 + " " + text60 + Environment.NewLine + "       - ngc mode: " + text59 + Environment.NewLine + "       - obd config.: " + text61 + Environment.NewLine + "       - speed: " + text62 + Environment.NewLine + "       PCM settings: " + Environment.NewLine + "       - state: disabled");
								}
								else
								{
									Util.UpdateTextBox(USBTextBox, "[INFO] TCM settings: " + Environment.NewLine + "       - state: " + state3);
								}
								TCM.UpdateHeader(state3, text62, text58, text61);
								PCM.UpdateHeader(state2, text57, text53, text56);
							}
							else
							{
								if (Util.IsBitClear(packet.Payload[0], 7))
								{
									state2 = "disabled";
								}
								else
								{
									PCMSelected = true;
									TCMSelected = false;
									state2 = "enabled";
									state3 = "disabled";
									if (Util.IsBitClear(packet.Payload[0], 4) && Util.IsBitClear(packet.Payload[0], 3) && Util.IsBitClear(packet.Payload[0], 6))
									{
										MainForm mainForm11 = this;
										MethodInvoker obj11 = val11;
										if (obj11 == null)
										{
											MethodInvoker val33 = delegate
											{
												((ListControl)SCIBusLogicComboBox).SelectedIndex = 2;
											};
											MethodInvoker val23 = val33;
											val11 = val33;
											obj11 = val23;
										}
										((Control)mainForm11).BeginInvoke((Delegate)(object)obj11);
										text54 = "disabled";
										text53 += "non-inverted";
									}
									else
									{
										if (Util.IsBitSet(packet.Payload[0], 4))
										{
											text54 = "enabled";
											MainForm mainForm12 = this;
											MethodInvoker obj12 = val12;
											if (obj12 == null)
											{
												MethodInvoker val34 = delegate
												{
													((ListControl)SCIBusLogicComboBox).SelectedIndex = 3;
												};
												MethodInvoker val23 = val34;
												val12 = val34;
												obj12 = val23;
											}
											((Control)mainForm12).BeginInvoke((Delegate)(object)obj12);
										}
										else
										{
											text54 = "disabled";
										}
										if (Util.IsBitSet(packet.Payload[0], 3))
										{
											text53 += "inverted";
											MainForm mainForm13 = this;
											MethodInvoker obj13 = val13;
											if (obj13 == null)
											{
												MethodInvoker val35 = delegate
												{
													((ListControl)SCIBusLogicComboBox).SelectedIndex = 1;
												};
												MethodInvoker val23 = val35;
												val13 = val35;
												obj13 = val23;
											}
											((Control)mainForm13).BeginInvoke((Delegate)(object)obj13);
										}
										else
										{
											text53 += "non-inverted";
										}
										if (Util.IsBitSet(packet.Payload[0], 6))
										{
											text55 += "(nibble swap)";
											MainForm mainForm14 = this;
											MethodInvoker obj14 = val14;
											if (obj14 == null)
											{
												MethodInvoker val36 = delegate
												{
													((ListControl)SCIBusLogicComboBox).SelectedIndex = 0;
												};
												MethodInvoker val23 = val36;
												val14 = val36;
												obj14 = val23;
											}
											((Control)mainForm14).BeginInvoke((Delegate)(object)obj14);
										}
									}
									if (Util.IsBitClear(packet.Payload[0], 2))
									{
										text56 = "A";
										MainForm mainForm15 = this;
										MethodInvoker obj15 = val15;
										if (obj15 == null)
										{
											MethodInvoker val37 = delegate
											{
												((ListControl)SCIBusOBDConfigurationComboBox).SelectedIndex = 0;
											};
											MethodInvoker val23 = val37;
											val15 = val37;
											obj15 = val23;
										}
										((Control)mainForm15).BeginInvoke((Delegate)(object)obj15);
									}
									else
									{
										text56 = "B";
										MainForm mainForm16 = this;
										MethodInvoker obj16 = val16;
										if (obj16 == null)
										{
											MethodInvoker val38 = delegate
											{
												((ListControl)SCIBusOBDConfigurationComboBox).SelectedIndex = 1;
											};
											MethodInvoker val23 = val38;
											val16 = val38;
											obj16 = val23;
										}
										((Control)mainForm16).BeginInvoke((Delegate)(object)obj16);
									}
									switch (packet.Payload[0] & 3)
									{
									case 0:
									{
										text57 = "976.5 baud";
										MainForm mainForm19 = this;
										MethodInvoker obj19 = val19;
										if (obj19 == null)
										{
											MethodInvoker val41 = delegate
											{
												((ListControl)SCIBusSpeedComboBox).SelectedIndex = 1;
											};
											MethodInvoker val23 = val41;
											val19 = val41;
											obj19 = val23;
										}
										((Control)mainForm19).BeginInvoke((Delegate)(object)obj19);
										break;
									}
									case 1:
									{
										text57 = "7812.5 baud";
										MainForm mainForm18 = this;
										MethodInvoker obj18 = val18;
										if (obj18 == null)
										{
											MethodInvoker val40 = delegate
											{
												((ListControl)SCIBusSpeedComboBox).SelectedIndex = 2;
											};
											MethodInvoker val23 = val40;
											val18 = val40;
											obj18 = val23;
										}
										((Control)mainForm18).BeginInvoke((Delegate)(object)obj18);
										break;
									}
									case 2:
									{
										text57 = "62500 baud";
										MainForm mainForm20 = this;
										MethodInvoker obj20 = val20;
										if (obj20 == null)
										{
											MethodInvoker val42 = delegate
											{
												((ListControl)SCIBusSpeedComboBox).SelectedIndex = 3;
											};
											MethodInvoker val23 = val42;
											val20 = val42;
											obj20 = val23;
										}
										((Control)mainForm20).BeginInvoke((Delegate)(object)obj20);
										break;
									}
									case 3:
									{
										text57 = "125000 baud";
										MainForm mainForm17 = this;
										MethodInvoker obj17 = val17;
										if (obj17 == null)
										{
											MethodInvoker val39 = delegate
											{
												((ListControl)SCIBusSpeedComboBox).SelectedIndex = 4;
											};
											MethodInvoker val23 = val39;
											val17 = val39;
											obj17 = val23;
										}
										((Control)mainForm17).BeginInvoke((Delegate)(object)obj17);
										break;
									}
									}
								}
								if (state2 == "enabled")
								{
									Util.UpdateTextBox(USBTextBox, "[INFO] PCM settings: " + Environment.NewLine + "       - state: " + state2 + Environment.NewLine + "       - logic: " + text53 + " " + text55 + Environment.NewLine + "       - ngc mode: " + text54 + Environment.NewLine + "       - obd config.: " + text56 + Environment.NewLine + "       - speed: " + text57 + Environment.NewLine + "       TCM settings: " + Environment.NewLine + "       - state: disabled");
								}
								else
								{
									Util.UpdateTextBox(USBTextBox, "[INFO] PCM settings: " + Environment.NewLine + "       - state: " + state2);
								}
								PCM.UpdateHeader(state2, text57, text53, text56);
								TCM.UpdateHeader(state3, text62, text58, text61);
							}
						}
						else
						{
							Util.UpdateTextBox(USBTextBox, "[RX->] Invalid settings packet:", PacketHelper.Serialize(packet));
						}
						break;
					case 5:
						if (packet.Payload != null && packet.Payload.Length >= 7)
						{
							string empty4 = string.Empty;
							empty4 = packet.Payload[0] switch
							{
								0 => "disabled", 
								1 => "enabled", 
								_ => "unknown", 
							};
							string text50 = Util.ByteToHexString(packet.Payload, 1) + " (hex)";
							string text51 = packet.Payload[2].ToString("0") + "x" + packet.Payload[3].ToString("0") + " characters";
							string text52 = packet.Payload[4].ToString("0") + " Hz";
							string empty5 = string.Empty;
							empty5 = ((packet.Payload[5] == 0) ? "imperial" : ((packet.Payload[5] != 1) ? "imperial" : "metric"));
							string empty6 = string.Empty;
							empty6 = packet.Payload[6] switch
							{
								1 => "CCD-bus", 
								2 => "SCI-bus (PCM)", 
								3 => "SCI-bus (TCM)", 
								4 => "PCI-bus", 
								_ => "unknown", 
							};
							Util.UpdateTextBox(USBTextBox, "[RX->] LCD settings changed:", PacketHelper.Serialize(packet));
							Util.UpdateTextBox(USBTextBox, "[INFO] LCD information:" + Environment.NewLine + "       State: " + empty4 + Environment.NewLine + "       I2C address: " + text50 + Environment.NewLine + "       Size: " + text51 + Environment.NewLine + "       Refresh rate: " + text52 + Environment.NewLine + "       Units: " + empty5 + Environment.NewLine + "       Data source: " + empty6);
							UpdateLCDPreviewTextBox();
						}
						else
						{
							Util.UpdateTextBox(USBTextBox, "[RX->] Invalid settings packet:", PacketHelper.Serialize(packet));
						}
						break;
					case 6:
						if (packet.Payload != null && packet.Payload.Length != 0)
						{
							string empty9 = string.Empty;
							string empty10 = string.Empty;
							empty9 = ((!Util.IsBitClear(packet.Payload[0], 7)) ? "enabled" : "disabled");
							empty10 = ((!Util.IsBitClear(packet.Payload[0], 6)) ? "active-high" : "active-low");
							Util.UpdateTextBox(USBTextBox, "[RX->] PCI-bus settings changed:", PacketHelper.Serialize(packet));
							Util.UpdateTextBox(USBTextBox, "[INFO] PCI-bus settings: " + Environment.NewLine + "       - state: " + empty9 + Environment.NewLine + "       - logic: " + empty10);
							PCI.UpdateHeader(empty9, null, empty10);
						}
						else
						{
							Util.UpdateTextBox(USBTextBox, "[RX->] Invalid settings packet:", PacketHelper.Serialize(packet));
						}
						break;
					case 7:
						if (packet.Payload != null && packet.Payload.Length > 1)
						{
							Util.UpdateTextBox(USBTextBox, "[RX->] Programming voltage settings changed:", PacketHelper.Serialize(packet));
							switch ((ushort)((packet.Payload[0] << 8) + packet.Payload[1]))
							{
							case 12000:
								Util.UpdateTextBox(USBTextBox, "[INFO] VBB (12V) applied to SCI-TX pin.");
								break;
							case 20000:
								Util.UpdateTextBox(USBTextBox, "[INFO] VPP (20V) applied to SCI-TX pin.");
								break;
							case 0:
								Util.UpdateTextBox(USBTextBox, "[INFO] VBB/VPP removed from SCI-TX pin.");
								break;
							}
						}
						else
						{
							Util.UpdateTextBox(USBTextBox, "[RX->] Invalid programming voltage settings packet:", PacketHelper.Serialize(packet));
						}
						break;
					case 8:
						if (packet.Payload != null && packet.Payload.Length >= 4)
						{
							if (!SerialService.Connect(SelectedPort))
							{
								Util.UpdateTextBox(USBTextBox, "[INFO] Device not found on " + SelectedPort + ".");
							}
							else
							{
								uint num11 = (uint)((packet.Payload[0] << 24) | (packet.Payload[1] << 16) | (packet.Payload[2] << 8) | packet.Payload[3]);
								Util.UpdateTextBox(USBTextBox, "[INFO] Scanner baudrate = " + num11);
							}
						}
						else
						{
							Util.UpdateTextBox(USBTextBox, "[RX->] Invalid UART baudrate packet:", PacketHelper.Serialize(packet));
						}
						break;
					default:
						Util.UpdateTextBox(USBTextBox, "[RX->] Packet received:", PacketHelper.Serialize(packet));
						break;
					}
					break;
				case 5:
					switch (packet.Mode)
					{
					case 1:
						if (packet.Payload != null)
						{
							if (packet.Payload[0] == 0 && packet.Payload.Length >= 30)
							{
								double num10 = (double)((packet.Payload[0] << 8) + packet.Payload[1]) / 100.0;
								string HardwareVersionString2 = "v" + num10.ToString("0.00").Insert(3, ".");
								DateTime dateTime3 = Util.UnixTimeStampToDateTime((packet.Payload[6] << 24) + (packet.Payload[7] << 16) + (packet.Payload[8] << 8) + packet.Payload[9]);
								DateTime dateTime4 = Util.UnixTimeStampToDateTime((packet.Payload[14] << 24) + (packet.Payload[15] << 16) + (packet.Payload[16] << 8) + packet.Payload[17]);
								DateTime dateTime5 = Util.UnixTimeStampToDateTime((packet.Payload[22] << 24) + (packet.Payload[23] << 16) + (packet.Payload[24] << 8) + packet.Payload[25]);
								DeviceFirmwareTimestamp = (ulong)((packet.Payload[22] << 24) + (packet.Payload[23] << 16) + (packet.Payload[24] << 8) + packet.Payload[25]);
								string text36 = dateTime3.ToString("yyyy.MM.dd HH:mm:ss");
								string text37 = dateTime4.ToString("yyyy.MM.dd HH:mm:ss");
								string text38 = dateTime5.ToString("yyyy.MM.dd HH:mm:ss");
								string FirmwareVersionString2 = "v" + packet.Payload[26].ToString("0") + "." + packet.Payload[27].ToString("0") + "." + packet.Payload[28].ToString("0");
								HWVersion = HardwareVersionString2;
								FWVersion = FirmwareVersionString2;
								Util.UpdateTextBox(USBTextBox, "[RX->] Hardware/Firmware information response:", PacketHelper.Serialize(packet));
								Util.UpdateTextBox(USBTextBox, "[INFO] Hardware ver.: " + HardwareVersionString2 + Environment.NewLine + "       Firmware ver.: " + FirmwareVersionString2 + Environment.NewLine + "       Hardware date: " + text36 + Environment.NewLine + "       Assembly date: " + text37 + Environment.NewLine + "       Firmware date: " + text38);
								((Control)this).BeginInvoke((Delegate)(MethodInvoker)delegate
								{
									if (!((Control)this).Text.Contains("  |  FW v"))
									{
										((Control)this).Text = ((Control)this).Text + "  |  FW " + FirmwareVersionString2 + "  |  HW " + HardwareVersionString2;
									}
									else
									{
										((Control)this).Text = ((Control)this).Text.Remove(((Control)this).Text.Length - (HardwareVersionString2.Length + FirmwareVersionString2.Length + 8));
										((Control)this).Text = ((Control)this).Text + FirmwareVersionString2 + "  |  HW " + HardwareVersionString2;
									}
								});
								if (Math.Round(num10 * 100.0) < 144.0)
								{
									((Control)MeasureCCDBusVoltagesButton).Enabled = false;
									((Control)CCDBusTerminationBiasOnOffCheckBox).Enabled = false;
								}
								else
								{
									((Control)MeasureCCDBusVoltagesButton).Enabled = true;
									((Control)CCDBusTerminationBiasOnOffCheckBox).Enabled = true;
								}
							}
							else if (packet.Payload.Length >= 23)
							{
								string HardwareVersionString = "v" + packet.Payload[0] + "." + packet.Payload[1] + "." + packet.Payload[2];
								string FirmwareVersionString = "v" + packet.Payload[4] + "." + packet.Payload[5] + "." + packet.Payload[6];
								HWVersion = HardwareVersionString;
								FWVersion = FirmwareVersionString;
								string text39 = packet.Payload[8] switch
								{
									1 => "ESP32", 
									2 => "ESP32-S2", 
									5 => "ESP32-C3", 
									6 => "ESP32-H2", 
									9 => "ESP32-S3", 
									12 => "ESP32-C2", 
									_ => "unknown", 
								};
								string text40 = packet.Payload[16] + "MB";
								string text41 = string.Empty;
								List<string> list4 = new List<string>();
								list4.Clear();
								if (Util.IsBitSet(packet.Payload[15], 0))
								{
									list4.Add(text40 + " Flash");
								}
								if (Util.IsBitSet(packet.Payload[15], 1))
								{
									list4.Add("WiFi");
								}
								if (Util.IsBitSet(packet.Payload[15], 5))
								{
									list4.Add("BT");
								}
								if (Util.IsBitSet(packet.Payload[15], 4))
								{
									list4.Add("BLE");
								}
								if (Util.IsBitSet(packet.Payload[15], 6))
								{
									list4.Add("IEEE 802.15.4");
								}
								if (Util.IsBitSet(packet.Payload[15], 7))
								{
									list4.Add("Embedded PSRAM");
								}
								if (list4.Count > 0)
								{
									foreach (string item in list4)
									{
										text41 = text41 + item + "/";
									}
									if (text41.Length > 2)
									{
										text41 = text41.Remove(text41.Length - 1);
									}
								}
								Util.UpdateTextBox(USBTextBox, "[RX->] Hardware/Firmware information response:", PacketHelper.Serialize(packet));
								Util.UpdateTextBox(USBTextBox, "[INFO] Hardware ver.: " + HardwareVersionString + Environment.NewLine + "       Firmware ver.: " + FirmwareVersionString + Environment.NewLine + "       CPU Model    : " + text39 + Environment.NewLine + "         - Revision : " + ((packet.Payload[9] << 8) + packet.Payload[10]) + Environment.NewLine + "         - Cores    : " + packet.Payload[11] + Environment.NewLine + "         - Features : " + text41 + Environment.NewLine + "         - BT MAC   : " + Util.ByteToHexString(packet.Payload, 17) + ":" + Util.ByteToHexString(packet.Payload, 18) + ":" + Util.ByteToHexString(packet.Payload, 19) + ":" + Util.ByteToHexString(packet.Payload, 20) + ":" + Util.ByteToHexString(packet.Payload, 21) + ":" + Util.ByteToHexString(packet.Payload, 22));
								((Control)this).BeginInvoke((Delegate)(MethodInvoker)delegate
								{
									if (!((Control)this).Text.Contains("  |  FW v"))
									{
										((Control)this).Text = ((Control)this).Text + "  |  FW " + FirmwareVersionString + "  |  HW " + HardwareVersionString;
									}
									else
									{
										((Control)this).Text = ((Control)this).Text.Remove(((Control)this).Text.Length - (HardwareVersionString.Length + FirmwareVersionString.Length + 8));
										((Control)this).Text = ((Control)this).Text + FirmwareVersionString + "  |  HW " + HardwareVersionString;
									}
								});
							}
						}
						else
						{
							Util.UpdateTextBox(USBTextBox, "[RX->] Invalid HW/FW info packet:", PacketHelper.Serialize(packet));
						}
						break;
					case 2:
						if (packet.Payload != null && packet.Payload.Length > 3)
						{
							TimeSpan value = TimeSpan.FromMilliseconds((packet.Payload[0] << 24) + (packet.Payload[1] << 16) + (packet.Payload[2] << 8) + packet.Payload[3]);
							string text49 = DateTime.Today.Add(value).ToString("HH:mm:ss.fff");
							Util.UpdateTextBox(USBTextBox, "[RX->] Timestamp response:", PacketHelper.Serialize(packet));
							Util.UpdateTextBox(USBTextBox, "[INFO] Device timestamp: " + text49);
						}
						else
						{
							Util.UpdateTextBox(USBTextBox, "[RX->] Invalid timestamp packet:", PacketHelper.Serialize(packet));
						}
						break;
					case 3:
						if (packet.Payload != null && packet.Payload.Length > 1)
						{
							string text48 = ((double)((packet.Payload[0] << 8) + packet.Payload[1]) / 1000.0).ToString("0.000") + " V";
							Util.UpdateTextBox(USBTextBox, "[RX->] Battery voltage response:", PacketHelper.Serialize(packet));
							Util.UpdateTextBox(USBTextBox, "[INFO] Battery voltage: " + text48);
						}
						else
						{
							Util.UpdateTextBox(USBTextBox, "[RX->] Invalid battery voltage packet:", PacketHelper.Serialize(packet));
						}
						break;
					case 4:
						if (packet.Payload != null && packet.Payload.Length > 2)
						{
							Util.UpdateTextBox(USBTextBox, "[RX->] External EEPROM checksum response:", PacketHelper.Serialize(packet));
							if (packet.Payload[0] == 1)
							{
								string text42 = Util.ByteToHexString(packet.Payload, 1);
								string text43 = Util.ByteToHexString(packet.Payload, 2);
								if (packet.Payload[1] == packet.Payload[2])
								{
									Util.UpdateTextBox(USBTextBox, "[INFO] External EEPROM checksum: " + text42 + "=OK.");
								}
								else
								{
									Util.UpdateTextBox(USBTextBox, "[INFO] External EEPROM checksum ERROR: " + Environment.NewLine + "       - reads as: " + text42 + Environment.NewLine + "       - calculated: " + text43);
								}
							}
							else
							{
								Util.UpdateTextBox(USBTextBox, "[INFO] No external EEPROM found.");
							}
						}
						else
						{
							Util.UpdateTextBox(USBTextBox, "[RX->] Invalid extEEPROM checksum packet:", PacketHelper.Serialize(packet));
						}
						break;
					case 5:
						if (packet.Payload != null && packet.Payload.Length > 3)
						{
							string text46 = ((double)((packet.Payload[0] << 8) + packet.Payload[1]) / 1000.0).ToString("0.000") + " V";
							string text47 = ((double)((packet.Payload[2] << 8) + packet.Payload[3]) / 1000.0).ToString("0.000") + " V";
							Util.UpdateTextBox(USBTextBox, "[RX->] CCD-bus voltage measurements response:", PacketHelper.Serialize(packet));
							Util.UpdateTextBox(USBTextBox, "[INFO] CCD-bus wire voltages:" + Environment.NewLine + "       CCD+: " + text46 + Environment.NewLine + "       CCD-: " + text47);
						}
						else
						{
							Util.UpdateTextBox(USBTextBox, "[RX->] Invalid CCD-bus voltage measurements packet:", PacketHelper.Serialize(packet));
						}
						break;
					case 6:
						if (packet.Payload != null && packet.Payload.Length > 1)
						{
							string text44 = ((double)((packet.Payload[0] << 8) + packet.Payload[1]) / 1000.0).ToString("0.000") + " V";
							Util.UpdateTextBox(USBTextBox, "[RX->] Bootstrap voltage response:", PacketHelper.Serialize(packet));
							Util.UpdateTextBox(USBTextBox, "[INFO] Bootstrap voltage: " + text44);
						}
						else
						{
							Util.UpdateTextBox(USBTextBox, "[RX->] Invalid bootstrap voltage packet:", PacketHelper.Serialize(packet));
						}
						break;
					case 7:
						if (packet.Payload != null && packet.Payload.Length > 1)
						{
							string text45 = ((double)((packet.Payload[0] << 8) + packet.Payload[1]) / 1000.0).ToString("0.000") + " V";
							Util.UpdateTextBox(USBTextBox, "[RX->] Programming voltage response:", PacketHelper.Serialize(packet));
							Util.UpdateTextBox(USBTextBox, "[INFO] Programming voltage: " + text45);
						}
						else
						{
							Util.UpdateTextBox(USBTextBox, "[RX->] Invalid programming voltage packet:", PacketHelper.Serialize(packet));
						}
						break;
					case 8:
						if (packet.Payload != null && packet.Payload.Length > 5)
						{
							string text33 = ((double)((packet.Payload[0] << 8) + packet.Payload[1]) / 1000.0).ToString("0.000") + " V";
							string text34 = ((double)((packet.Payload[2] << 8) + packet.Payload[3]) / 1000.0).ToString("0.000") + " V";
							string text35 = ((double)((packet.Payload[4] << 8) + packet.Payload[5]) / 1000.0).ToString("0.000") + " V";
							Util.UpdateTextBox(USBTextBox, "[RX->] Voltages response:", PacketHelper.Serialize(packet));
							Util.UpdateTextBox(USBTextBox, "[INFO] Battery voltage: " + text33 + Environment.NewLine + "       Bootstrap voltage: " + text34 + Environment.NewLine + "       Programming voltage: " + text35);
						}
						else
						{
							Util.UpdateTextBox(USBTextBox, "[RX->] Invalid voltages packet:", PacketHelper.Serialize(packet));
						}
						break;
					default:
						Util.UpdateTextBox(USBTextBox, "[RX->] Packet received:", PacketHelper.Serialize(packet));
						break;
					}
					break;
				case 6:
					switch (packet.Mode)
					{
					case 1:
						if (packet.Payload != null && packet.Payload.Length != 0)
						{
							switch (packet.Payload[0])
							{
							case 1:
								Util.UpdateTextBox(USBTextBox, "[RX->] CCD-bus repeated Tx stopped:", PacketHelper.Serialize(packet));
								break;
							case 2:
								Util.UpdateTextBox(USBTextBox, "[RX->] SCI-bus (PCM) repeated Tx stopped:", PacketHelper.Serialize(packet));
								break;
							case 3:
								Util.UpdateTextBox(USBTextBox, "[RX->] SCI-bus (TCM) repeated Tx stopped:", PacketHelper.Serialize(packet));
								break;
							case 4:
								Util.UpdateTextBox(USBTextBox, "[RX->] PCI-bus repeated Tx stopped:", PacketHelper.Serialize(packet));
								break;
							default:
								Util.UpdateTextBox(USBTextBox, "[RX->] Unknown communication bus action:", PacketHelper.Serialize(packet));
								break;
							}
						}
						else
						{
							Util.UpdateTextBox(USBTextBox, "[RX->] Invalid communication bus action:", PacketHelper.Serialize(packet));
						}
						break;
					case 2:
					case 130:
						if (packet.Payload != null && packet.Payload.Length != 0)
						{
							switch (packet.Payload[0])
							{
							case 1:
								Util.UpdateTextBox(USBTextBox, "[RX->] CCD-bus message prepared for Tx:", PacketHelper.Serialize(packet));
								break;
							case 2:
								Util.UpdateTextBox(USBTextBox, "[RX->] SCI-bus (PCM) message prepared for Tx:", PacketHelper.Serialize(packet));
								break;
							case 3:
								Util.UpdateTextBox(USBTextBox, "[RX->] SCI-bus (TCM) message prepared for Tx:", PacketHelper.Serialize(packet));
								break;
							case 4:
								Util.UpdateTextBox(USBTextBox, "[RX->] PCI-bus message prepared for Tx:", PacketHelper.Serialize(packet));
								break;
							default:
								Util.UpdateTextBox(USBTextBox, "[RX->] Unknown communication bus action:", PacketHelper.Serialize(packet));
								break;
							}
						}
						else
						{
							Util.UpdateTextBox(USBTextBox, "[RX->] Invalid communication bus action:", PacketHelper.Serialize(packet));
						}
						break;
					case 3:
						if (packet.Payload != null && packet.Payload.Length != 0)
						{
							switch (packet.Payload[0])
							{
							case 1:
								Util.UpdateTextBox(USBTextBox, "[RX->] CCD-bus message list prepared for Tx:", PacketHelper.Serialize(packet));
								break;
							case 2:
								Util.UpdateTextBox(USBTextBox, "[RX->] SCI-bus (PCM) messages list prepared for Tx:", PacketHelper.Serialize(packet));
								break;
							case 3:
								Util.UpdateTextBox(USBTextBox, "[RX->] SCI-bus (TCM) messages list prepared for Tx:", PacketHelper.Serialize(packet));
								break;
							case 4:
								Util.UpdateTextBox(USBTextBox, "[RX->] PCI-bus message list prepared for Tx:", PacketHelper.Serialize(packet));
								break;
							default:
								Util.UpdateTextBox(USBTextBox, "[RX->] Unknown communication bus action:", PacketHelper.Serialize(packet));
								break;
							}
						}
						else
						{
							Util.UpdateTextBox(USBTextBox, "[RX->] Invalid communication bus action:", PacketHelper.Serialize(packet));
						}
						break;
					case 4:
						if (packet.Payload != null && packet.Payload.Length != 0)
						{
							switch (packet.Payload[0])
							{
							case 1:
								Util.UpdateTextBox(USBTextBox, "[RX->] CCD-bus repeated message list Tx started:", PacketHelper.Serialize(packet));
								break;
							case 2:
								Util.UpdateTextBox(USBTextBox, "[RX->] SCI-bus (PCM) repeated message list Tx started:", PacketHelper.Serialize(packet));
								break;
							case 3:
								Util.UpdateTextBox(USBTextBox, "[RX->] SCI-bus (TCM) repeated message list Tx started:", PacketHelper.Serialize(packet));
								break;
							case 4:
								Util.UpdateTextBox(USBTextBox, "[RX->] PCI-bus repeated message list Tx started:", PacketHelper.Serialize(packet));
								break;
							default:
								Util.UpdateTextBox(USBTextBox, "[RX->] Unknown communication bus action:", PacketHelper.Serialize(packet));
								break;
							}
						}
						else
						{
							Util.UpdateTextBox(USBTextBox, "[RX->] Invalid communication bus action:", PacketHelper.Serialize(packet));
						}
						break;
					default:
						Util.UpdateTextBox(USBTextBox, "[RX->] Packet received:", PacketHelper.Serialize(packet));
						break;
					}
					break;
				case 7:
				{
					byte mode = packet.Mode;
					if ((uint)(mode - 1) > 3u)
					{
						_ = 130;
					}
					Util.UpdateTextBox(USBTextBox, "[RX->] Packet received:", PacketHelper.Serialize(packet));
					break;
				}
				case 13:
					switch (packet.Mode)
					{
					case 1:
						Util.UpdateTextBox(USBTextBox, "[RX->] Flash write task update:", PacketHelper.Serialize(packet));
						break;
					case 2:
						Util.UpdateTextBox(USBTextBox, "[RX->] Flash read task update:", PacketHelper.Serialize(packet));
						break;
					case 3:
						Util.UpdateTextBox(USBTextBox, "[RX->] EEPROM write task update:", PacketHelper.Serialize(packet));
						break;
					case 4:
						Util.UpdateTextBox(USBTextBox, "[RX->] EEPROM read task update:", PacketHelper.Serialize(packet));
						break;
					case 5:
						if (packet.Payload != null)
						{
							byte[] payload4 = packet.Payload;
							if (payload4 == null || payload4.Length != 0)
							{
								Util.UpdateTextBox(USBTextBox, "[RX->] Enter bootstrap mode result:", PacketHelper.Serialize(packet));
								switch (packet.Payload[0])
								{
								case 0:
									Util.UpdateTextBox(USBTextBox, "[INFO] Bootstrap mode entered successfully.");
									break;
								case 1:
									Util.UpdateTextBox(USBTextBox, "[INFO] Error: set baudrate timeout.");
									break;
								case 2:
									Util.UpdateTextBox(USBTextBox, "[INFO] Error: set baudrate response.");
									break;
								case 3:
									Util.UpdateTextBox(USBTextBox, "[INFO] Error: seed timeout.");
									break;
								case 4:
									Util.UpdateTextBox(USBTextBox, "[INFO] Error: seed response.");
									break;
								case 5:
									Util.UpdateTextBox(USBTextBox, "[INFO] Error: seed checksum.");
									break;
								case 6:
									Util.UpdateTextBox(USBTextBox, "[INFO] Error: key timeout.");
									break;
								case 7:
									Util.UpdateTextBox(USBTextBox, "[INFO] Error: key response.");
									break;
								case 8:
									Util.UpdateTextBox(USBTextBox, "[INFO] Error: key checksum.");
									break;
								case 9:
									Util.UpdateTextBox(USBTextBox, "[INFO] Error: invalid key.");
									break;
								case 10:
									Util.UpdateTextBox(USBTextBox, "[INFO] Error: bootloader not supported.");
									break;
								case 11:
									Util.UpdateTextBox(USBTextBox, "[INFO] Error: bootloader error.");
									break;
								case 12:
									Util.UpdateTextBox(USBTextBox, "[INFO] Error: bootloader timeout.");
									break;
								case 13:
									Util.UpdateTextBox(USBTextBox, "[INFO] Error: bootloader start break.");
									break;
								case 14:
									Util.UpdateTextBox(USBTextBox, "[INFO] Error: bootloader start timeout.");
									break;
								case 15:
									Util.UpdateTextBox(USBTextBox, "[INFO] Error: bootloader start failed.");
									break;
								default:
									Util.UpdateTextBox(USBTextBox, "[INFO] Error: unknown result.");
									break;
								}
							}
						}
						break;
					case 6:
						if (packet.Payload != null)
						{
							byte[] payload2 = packet.Payload;
							if (payload2 == null || payload2.Length != 0)
							{
								Util.UpdateTextBox(USBTextBox, "[RX->] Upload worker result:", PacketHelper.Serialize(packet));
								switch (packet.Payload[0])
								{
								case 0:
									Util.UpdateTextBox(USBTextBox, "[INFO] Worker upload success.");
									break;
								case 1:
									Util.UpdateTextBox(USBTextBox, "[INFO] Error: invalid worker.");
									break;
								case 2:
									Util.UpdateTextBox(USBTextBox, "[INFO] Error: no response.");
									break;
								case 3:
									Util.UpdateTextBox(USBTextBox, "[INFO] Error: handshake.");
									break;
								case 4:
									Util.UpdateTextBox(USBTextBox, "[INFO] Error: upload error.");
									break;
								case 5:
									Util.UpdateTextBox(USBTextBox, "[INFO] Error: upload timeout.");
									break;
								case 6:
									Util.UpdateTextBox(USBTextBox, "[INFO] Error: upload interrupted.");
									break;
								case 7:
									Util.UpdateTextBox(USBTextBox, "[INFO] Error: upload failed.");
									break;
								default:
									Util.UpdateTextBox(USBTextBox, "[INFO] Error: unknown result.");
									break;
								}
							}
						}
						break;
					case 7:
						if (packet.Payload != null)
						{
							byte[] payload3 = packet.Payload;
							if (payload3 == null || payload3.Length != 0)
							{
								Util.UpdateTextBox(USBTextBox, "[RX->] Start worker result:", PacketHelper.Serialize(packet));
								switch (packet.Payload[0])
								{
								case 8:
									Util.UpdateTextBox(USBTextBox, "[INFO] Error: start timeout.");
									break;
								case 9:
									Util.UpdateTextBox(USBTextBox, "[INFO] Error: start error.");
									break;
								default:
									Util.UpdateTextBox(USBTextBox, "[INFO] Error: unknown result.");
									break;
								}
							}
						}
						break;
					case 8:
						if (packet.Payload != null)
						{
							byte[] payload = packet.Payload;
							if (payload == null || payload.Length != 0)
							{
								Util.UpdateTextBox(USBTextBox, "[RX->] Exit worker result:", PacketHelper.Serialize(packet));
								switch (packet.Payload[0])
								{
								case 10:
									Util.UpdateTextBox(USBTextBox, "[INFO] Error: exit timeout.");
									break;
								case 11:
									Util.UpdateTextBox(USBTextBox, "[INFO] Error: exit error.");
									break;
								default:
									Util.UpdateTextBox(USBTextBox, "[INFO] Error: unknown result.");
									break;
								}
							}
						}
						break;
					}
					break;
				case 14:
					switch (packet.Mode)
					{
					case 1:
						if (packet.Payload != null && packet.Payload.Length != 0)
						{
							switch (packet.Payload[0])
							{
							case 1:
								Util.UpdateTextBox(USBTextBox, "[RX->] Random CCD-bus messages started:", PacketHelper.Serialize(packet));
								break;
							case 0:
								Util.UpdateTextBox(USBTextBox, "[RX->] Random CCD-bus messages stopped:", PacketHelper.Serialize(packet));
								break;
							default:
								Util.UpdateTextBox(USBTextBox, "[RX->] Unknown debug packet:", PacketHelper.Serialize(packet));
								break;
							}
						}
						else
						{
							Util.UpdateTextBox(USBTextBox, "[RX->] Invalid debug packet:", PacketHelper.Serialize(packet));
						}
						break;
					case 2:
						if (packet.Payload != null && packet.Payload.Length > 3)
						{
							if (packet.Payload[0] == 0)
							{
								string text13 = Util.ByteToHexString(packet.Payload, 1, 2);
								string text14 = Util.ByteToHexString(packet.Payload, 3);
								Util.UpdateTextBox(USBTextBox, "[RX->] Internal EEPROM byte read response:", PacketHelper.Serialize(packet));
								Util.UpdateTextBox(USBTextBox, "[INFO] Internal EEPROM byte information:" + Environment.NewLine + "       Offset: " + text13 + " | Value: " + text14);
							}
							else
							{
								Util.UpdateTextBox(USBTextBox, "[RX->] Internal EEPROM byte read error:", PacketHelper.Serialize(packet));
							}
						}
						else
						{
							Util.UpdateTextBox(USBTextBox, "[RX->] Invalid debug packet:", PacketHelper.Serialize(packet));
						}
						break;
					case 3:
						if (packet.Payload != null && packet.Payload.Length > 4)
						{
							if (packet.Payload[0] == 0)
							{
								int num6 = (packet.Payload[1] << 8) + packet.Payload[2];
								int num7 = packet.Payload.Length - 3;
								string text15 = Util.ByteToHexString(packet.Payload, 1, 2);
								string text16 = Util.ByteToHexString(packet.Payload, 3, packet.Payload.Length - 3);
								string text17 = num7.ToString();
								Util.UpdateTextBox(USBTextBox, "[RX->] Internal EEPROM block read response:", PacketHelper.Serialize(packet));
								Util.UpdateTextBox(USBTextBox, "[INFO] Internal EEPROM block information:" + Environment.NewLine + "       Offset: " + text15 + " | Count: " + text17 + Environment.NewLine + text16);
								if (num6 == 0 && num7 == 256)
								{
									string text18 = "v" + ((double)((packet.Payload[3] << 8) + packet.Payload[4]) / 100.0).ToString("0.00").Insert(3, ".");
									DateTime dateTime = Util.UnixTimeStampToDateTime((packet.Payload[9] << 24) + (packet.Payload[10] << 16) + (packet.Payload[11] << 8) + packet.Payload[12]);
									DateTime dateTime2 = Util.UnixTimeStampToDateTime((packet.Payload[17] << 24) + (packet.Payload[18] << 16) + (packet.Payload[19] << 8) + packet.Payload[20]);
									string text19 = dateTime.ToString("yyyy.MM.dd HH:mm:ss");
									string text20 = dateTime2.ToString("yyyy.MM.dd HH:mm:ss");
									string text21 = ((double)((packet.Payload[21] << 8) + packet.Payload[22]) / 100.0).ToString("0.00") + " V";
									double num8 = (double)((packet.Payload[23] << 8) + packet.Payload[24]) / 1000.0;
									double num9 = (double)((packet.Payload[25] << 8) + packet.Payload[26]) / 1000.0;
									string text22 = num8.ToString("0.000") + " kΩ";
									string text23 = num9.ToString("0.000") + " kΩ";
									string empty = string.Empty;
									empty = ((!Util.IsBitSet(packet.Payload[27], 0)) ? "disabled" : "enabled");
									string text24 = Util.ByteToHexString(packet.Payload, 28) + " (hex)";
									string text25 = packet.Payload[29].ToString("0") + " characters";
									string text26 = packet.Payload[30].ToString("0") + " characters";
									string text27 = packet.Payload[31].ToString("0") + " Hz";
									string empty2 = string.Empty;
									string empty3 = string.Empty;
									empty2 = ((packet.Payload[32] == 0) ? "imperial" : ((packet.Payload[32] != 1) ? "imperial" : "metric"));
									empty3 = packet.Payload[33] switch
									{
										1 => "CCD-bus", 
										2 => "SCI-bus (PCM)", 
										3 => "SCI-bus (TCM)", 
										_ => "CCD-bus", 
									};
									string text28 = (packet.Payload[34] << 8) + packet.Payload[35] + " ms";
									string text29 = (packet.Payload[36] << 8) + packet.Payload[37] + " ms";
									Util.UpdateTextBox(USBTextBox, "[INFO] External EEPROM settings:" + Environment.NewLine + "       Hardware ver.: " + Util.ByteToHexString(packet.Payload, 3, 2) + " | " + text18 + Environment.NewLine + "       Hardware date: " + Util.ByteToHexString(packet.Payload, 5, 8) + " | " + Environment.NewLine + "                      " + text19 + Environment.NewLine + "       Assembly date: " + Util.ByteToHexString(packet.Payload, 13, 8) + " | " + Environment.NewLine + "                      " + text20 + Environment.NewLine + "       ADC supply:    " + Util.ByteToHexString(packet.Payload, 21, 2) + " | " + text21 + Environment.NewLine + "       RDH resistor:  " + Util.ByteToHexString(packet.Payload, 23, 2) + " | " + text22 + Environment.NewLine + "       RDL resistor:  " + Util.ByteToHexString(packet.Payload, 25, 2) + " | " + text23 + Environment.NewLine + "       LCD state:        " + Util.ByteToHexString(packet.Payload, 27) + " | " + empty + Environment.NewLine + "       LCD I2C addr.:    " + Util.ByteToHexString(packet.Payload, 28) + " | " + text24 + Environment.NewLine + "       LCD width:        " + Util.ByteToHexString(packet.Payload, 29) + " | " + text25 + Environment.NewLine + "       LCD height:       " + Util.ByteToHexString(packet.Payload, 30) + " | " + text26 + Environment.NewLine + "       LCD refresh:      " + Util.ByteToHexString(packet.Payload, 31) + " | " + text27 + Environment.NewLine + "       LCD units:        " + Util.ByteToHexString(packet.Payload, 32) + " | " + empty2 + Environment.NewLine + "       LCD data src:     " + Util.ByteToHexString(packet.Payload, 33) + " | " + empty3 + Environment.NewLine + "       LED heartbeat: " + Util.ByteToHexString(packet.Payload, 34, 2) + " | " + text28 + Environment.NewLine + "       LED blink:     " + Util.ByteToHexString(packet.Payload, 36, 2) + " | " + text29 + Environment.NewLine + "       CCD settings:     " + Util.ByteToHexString(packet.Payload, 38) + Environment.NewLine + "       PCM settings:     " + Util.ByteToHexString(packet.Payload, 39) + Environment.NewLine + "       TCM settings:     " + Util.ByteToHexString(packet.Payload, 40));
								}
							}
							else
							{
								Util.UpdateTextBox(USBTextBox, "[RX->] Internal EEPROM block read error:", PacketHelper.Serialize(packet));
							}
						}
						else
						{
							Util.UpdateTextBox(USBTextBox, "[RX->] Invalid debug packet:", PacketHelper.Serialize(packet));
						}
						break;
					case 4:
						if (packet.Payload != null && packet.Payload.Length > 3)
						{
							if (packet.Payload[0] == 0)
							{
								string text5 = Util.ByteToHexString(packet.Payload, 1, 2);
								string text6 = Util.ByteToHexString(packet.Payload, 3);
								Util.UpdateTextBox(USBTextBox, "[RX->] External EEPROM byte read response:", PacketHelper.Serialize(packet));
								Util.UpdateTextBox(USBTextBox, "[INFO] External EEPROM byte information:" + Environment.NewLine + "       Offset: " + text5 + " | Value: " + text6);
							}
							else
							{
								Util.UpdateTextBox(USBTextBox, "[RX->] External EEPROM byte read error:", PacketHelper.Serialize(packet));
							}
						}
						else
						{
							Util.UpdateTextBox(USBTextBox, "[RX->] Invalid debug packet:", PacketHelper.Serialize(packet));
						}
						break;
					case 5:
						if (packet.Payload != null && packet.Payload.Length > 4)
						{
							if (packet.Payload[0] == 0)
							{
								_ = packet.Payload[1];
								_ = packet.Payload[2];
								int num5 = packet.Payload.Length - 3;
								string text10 = Util.ByteToHexString(packet.Payload, 1, 2);
								string text11 = Util.ByteToHexString(packet.Payload, 3, packet.Payload.Length - 3);
								string text12 = num5.ToString();
								Util.UpdateTextBox(USBTextBox, "[RX->] External EEPROM block read response:", PacketHelper.Serialize(packet));
								Util.UpdateTextBox(USBTextBox, "[INFO] External EEPROM block information:" + Environment.NewLine + "       Offset: " + text10 + " | Count: " + text12 + Environment.NewLine + text11);
							}
							else
							{
								Util.UpdateTextBox(USBTextBox, "[RX->] External EEPROM block read error:", PacketHelper.Serialize(packet));
							}
						}
						else
						{
							Util.UpdateTextBox(USBTextBox, "[RX->] Invalid debug packet:", PacketHelper.Serialize(packet));
						}
						break;
					case 6:
						if (packet.Payload != null && packet.Payload.Length > 3)
						{
							if (packet.Payload[0] == 0)
							{
								string text = Util.ByteToHexString(packet.Payload, 1, 2);
								string text2 = Util.ByteToHexString(packet.Payload, 3);
								Util.UpdateTextBox(USBTextBox, "[RX->] Internal EEPROM byte write response:", PacketHelper.Serialize(packet));
								Util.UpdateTextBox(USBTextBox, "[INFO] Internal EEPROM byte information:" + Environment.NewLine + "       Offset: " + text + " | Value: " + text2);
								byte b = Util.HexStringToByte(((Control)EEPROMWriteValuesTextBox).Text)[0];
								if (packet.Payload[3] == b)
								{
									MessageBox.Show("Internal EEPROM byte write successful!", "Information", (MessageBoxButtons)0, (MessageBoxIcon)64);
								}
								else
								{
									MessageBox.Show("Internal EEPROM byte write failed!", "Warning", (MessageBoxButtons)0, (MessageBoxIcon)48);
								}
							}
							else
							{
								Util.UpdateTextBox(USBTextBox, "[RX->] Internal EEPROM byte write error:", PacketHelper.Serialize(packet));
							}
						}
						else
						{
							Util.UpdateTextBox(USBTextBox, "[RX->] Invalid debug packet:", PacketHelper.Serialize(packet));
						}
						break;
					case 7:
						if (packet.Payload != null && packet.Payload.Length > 4)
						{
							if (packet.Payload[0] == 0)
							{
								string text30 = Util.ByteToHexString(packet.Payload, 1, 2);
								string text31 = Util.ByteToHexString(packet.Payload, 3, packet.Payload.Length - 3);
								string text32 = (packet.Payload.Length - 3).ToString();
								Util.UpdateTextBox(USBTextBox, "[RX->] Internal EEPROM block write response:", PacketHelper.Serialize(packet));
								Util.UpdateTextBox(USBTextBox, "[INFO] Internal EEPROM block information:" + Environment.NewLine + "       Offset: " + text30 + " | Count: " + text32 + Environment.NewLine + text31);
								byte[] array3 = Util.HexStringToByte(((Control)EEPROMWriteValuesTextBox).Text);
								byte[] array4 = new byte[array3.Length];
								Array.Copy(packet.Payload, 3, array4, 0, packet.Payload.Length - 3);
								if (Util.CompareArrays(array3, array4, 0, array3.Length))
								{
									MessageBox.Show("Internal EEPROM block write successful!", "Information", (MessageBoxButtons)0, (MessageBoxIcon)64);
								}
								else
								{
									MessageBox.Show("Internal EEPROM block write failed!", "Warning", (MessageBoxButtons)0, (MessageBoxIcon)48);
								}
							}
							else
							{
								Util.UpdateTextBox(USBTextBox, "[RX->] Internal EEPROM block write error:", PacketHelper.Serialize(packet));
							}
						}
						else
						{
							Util.UpdateTextBox(USBTextBox, "[RX->] Invalid debug packet:", PacketHelper.Serialize(packet));
						}
						break;
					case 8:
						if (packet.Payload != null && packet.Payload.Length > 3)
						{
							if (packet.Payload[0] == 0)
							{
								string text3 = Util.ByteToHexString(packet.Payload, 1, 2);
								string text4 = Util.ByteToHexString(packet.Payload, 3);
								Util.UpdateTextBox(USBTextBox, "[RX->] External EEPROM byte write response:", PacketHelper.Serialize(packet));
								Util.UpdateTextBox(USBTextBox, "[INFO] External EEPROM byte information:" + Environment.NewLine + "       Offset: " + text3 + " | Value: " + text4);
								byte b2 = Util.HexStringToByte(((Control)EEPROMWriteValuesTextBox).Text)[0];
								if (packet.Payload[3] == b2)
								{
									MessageBox.Show("External EEPROM byte write successful!", "Information", (MessageBoxButtons)0, (MessageBoxIcon)64);
								}
								else
								{
									MessageBox.Show("External EEPROM byte write failed!", "Warning", (MessageBoxButtons)0, (MessageBoxIcon)48);
								}
							}
							else
							{
								Util.UpdateTextBox(USBTextBox, "[RX->] External EEPROM byte write error:", PacketHelper.Serialize(packet));
							}
						}
						else
						{
							Util.UpdateTextBox(USBTextBox, "[RX->] Invalid debug packet:", PacketHelper.Serialize(packet));
						}
						break;
					case 9:
						if (packet.Payload != null && packet.Payload.Length > 4)
						{
							if (packet.Payload[0] == 0)
							{
								string text7 = Util.ByteToHexString(packet.Payload, 1, 2);
								string text8 = Util.ByteToHexString(packet.Payload, 3, packet.Payload.Length - 3);
								string text9 = (packet.Payload.Length - 3).ToString();
								Util.UpdateTextBox(USBTextBox, "[RX->] External EEPROM block write response:", PacketHelper.Serialize(packet));
								Util.UpdateTextBox(USBTextBox, "[INFO] External EEPROM block information:" + Environment.NewLine + "       Offset: " + text7 + " | Count: " + text9 + Environment.NewLine + text8);
								byte[] array = Util.HexStringToByte(((Control)EEPROMWriteValuesTextBox).Text);
								byte[] array2 = new byte[array.Length];
								Array.Copy(packet.Payload, 3, array2, 0, packet.Payload.Length - 3);
								if (Util.CompareArrays(array, array2, 0, array.Length))
								{
									MessageBox.Show("External EEPROM block write successful!", "Information", (MessageBoxButtons)0, (MessageBoxIcon)64);
								}
								else
								{
									MessageBox.Show("External EEPROM block write failed!", "Warning", (MessageBoxButtons)0, (MessageBoxIcon)48);
								}
							}
							else
							{
								Util.UpdateTextBox(USBTextBox, "[RX->] External EEPROM block write error:", PacketHelper.Serialize(packet));
							}
						}
						else
						{
							Util.UpdateTextBox(USBTextBox, "[RX->] Invalid debug packet:", PacketHelper.Serialize(packet));
						}
						break;
					case 10:
						if (packet.Payload != null && packet.Payload.Length > 1)
						{
							switch (packet.Payload[0])
							{
							case 1:
								switch (packet.Payload[1])
								{
								case 1:
									Util.UpdateTextBox(USBTextBox, "[RX->] CCD-bus speed changed:", PacketHelper.Serialize(packet));
									Util.UpdateTextBox(USBTextBox, "[INFO] CCD-bus speed: 976.5 baud");
									break;
								case 2:
									Util.UpdateTextBox(USBTextBox, "[RX->] CCD-bus speed changed:", PacketHelper.Serialize(packet));
									Util.UpdateTextBox(USBTextBox, "[INFO] CCD-bus speed: 7812.5 baud");
									break;
								case 3:
									Util.UpdateTextBox(USBTextBox, "[RX->] CCD-bus speed changed:", PacketHelper.Serialize(packet));
									Util.UpdateTextBox(USBTextBox, "[INFO] CCD-bus speed: 62500 baud");
									break;
								case 4:
									Util.UpdateTextBox(USBTextBox, "[RX->] CCD-bus speed changed:", PacketHelper.Serialize(packet));
									Util.UpdateTextBox(USBTextBox, "[INFO] CCD-bus speed: 125000 baud");
									break;
								default:
									Util.UpdateTextBox(USBTextBox, "[RX->] CCD-bus speed unchanged:", PacketHelper.Serialize(packet));
									break;
								}
								break;
							case 2:
								switch (packet.Payload[1])
								{
								case 1:
									Util.UpdateTextBox(USBTextBox, "[RX->] SCI-bus (PCM) speed changed:", PacketHelper.Serialize(packet));
									Util.UpdateTextBox(USBTextBox, "[INFO] SCI-bus (PCM) speed: 976.5 baud");
									break;
								case 2:
									Util.UpdateTextBox(USBTextBox, "[RX->] SCI-bus (PCM) speed changed:", PacketHelper.Serialize(packet));
									Util.UpdateTextBox(USBTextBox, "[INFO] SCI-bus (PCM) speed: 7812.5 baud");
									break;
								case 3:
									Util.UpdateTextBox(USBTextBox, "[RX->] SCI-bus (PCM) speed changed:", PacketHelper.Serialize(packet));
									Util.UpdateTextBox(USBTextBox, "[INFO] SCI-bus (PCM) speed: 62500 baud");
									break;
								case 4:
									Util.UpdateTextBox(USBTextBox, "[RX->] SCI-bus (PCM) speed changed:", PacketHelper.Serialize(packet));
									Util.UpdateTextBox(USBTextBox, "[INFO] SCI-bus (PCM) speed: 125000 baud");
									break;
								default:
									Util.UpdateTextBox(USBTextBox, "[RX->] SCI-bus (PCM) speed unchanged:", PacketHelper.Serialize(packet));
									break;
								}
								break;
							case 3:
								switch (packet.Payload[1])
								{
								case 1:
									Util.UpdateTextBox(USBTextBox, "[RX->] SCI-bus (TCM) speed changed:", PacketHelper.Serialize(packet));
									Util.UpdateTextBox(USBTextBox, "[INFO] SCI-bus (TCM) speed: 976.5 baud");
									break;
								case 2:
									Util.UpdateTextBox(USBTextBox, "[RX->] SCI-bus (TCM) speed changed:", PacketHelper.Serialize(packet));
									Util.UpdateTextBox(USBTextBox, "[INFO] SCI-bus (TCM) speed: 7812.5 baud");
									break;
								case 3:
									Util.UpdateTextBox(USBTextBox, "[RX->] SCI-bus (TCM) speed changed:", PacketHelper.Serialize(packet));
									Util.UpdateTextBox(USBTextBox, "[INFO] SCI-bus (TCM) speed: 62500 baud");
									break;
								case 4:
									Util.UpdateTextBox(USBTextBox, "[RX->] SCI-bus (TCM) speed changed:", PacketHelper.Serialize(packet));
									Util.UpdateTextBox(USBTextBox, "[INFO] SCI-bus (TCM) speed: 125000 baud");
									break;
								default:
									Util.UpdateTextBox(USBTextBox, "[RX->] SCI-bus (TCM) speed unchanged:", PacketHelper.Serialize(packet));
									break;
								}
								break;
							default:
								Util.UpdateTextBox(USBTextBox, "[RX->] Unknown baudrate change request:", PacketHelper.Serialize(packet));
								break;
							}
						}
						else
						{
							Util.UpdateTextBox(USBTextBox, "[RX->] Invalid debug packet:", PacketHelper.Serialize(packet));
						}
						break;
					default:
						Util.UpdateTextBox(USBTextBox, "[RX->] Packet received:", PacketHelper.Serialize(packet));
						break;
					}
					break;
				case 15:
					switch (packet.Mode)
					{
					case 0:
						Util.UpdateTextBox(USBTextBox, "[RX->] OK:", PacketHelper.Serialize(packet));
						break;
					case 1:
						Util.UpdateTextBox(USBTextBox, "[RX->] Error, invalid packet length:", PacketHelper.Serialize(packet));
						break;
					case 2:
						Util.UpdateTextBox(USBTextBox, "[RX->] Error, invalid dc command:", PacketHelper.Serialize(packet));
						break;
					case 3:
						Util.UpdateTextBox(USBTextBox, "[RX->] Error, invalid sub-data code:", PacketHelper.Serialize(packet));
						break;
					case 4:
						Util.UpdateTextBox(USBTextBox, "[RX->] Error, invalid payload value(s):", PacketHelper.Serialize(packet));
						break;
					case 5:
						Util.UpdateTextBox(USBTextBox, "[RX->] Error, invalid checksum:", PacketHelper.Serialize(packet));
						break;
					case 6:
						Util.UpdateTextBox(USBTextBox, "[RX->] Error, packet timeout occured:", PacketHelper.Serialize(packet));
						break;
					case 7:
						Util.UpdateTextBox(USBTextBox, "[RX->] Error, buffer overflow:", PacketHelper.Serialize(packet));
						break;
					case 8:
						Util.UpdateTextBox(USBTextBox, "[RX->] Error, invalid bus:", PacketHelper.Serialize(packet));
						break;
					case 246:
						Util.UpdateTextBox(USBTextBox, "[RX->] Error, no response from SCI-bus:", PacketHelper.Serialize(packet));
						break;
					case 247:
						Util.UpdateTextBox(USBTextBox, "[RX->] Error, not enough MCU RAM:", PacketHelper.Serialize(packet));
						break;
					case 248:
						if (packet.Payload != null && packet.Payload.Length != 0)
						{
							Util.UpdateTextBox(USBTextBox, "[RX->] Error, SCI-bus RAM-table no response (" + Util.ByteToHexString(packet.Payload) + "):", PacketHelper.Serialize(packet));
						}
						else
						{
							Util.UpdateTextBox(USBTextBox, "[RX->] Error, SCI-bus RAM-table no response:", PacketHelper.Serialize(packet));
						}
						break;
					case 249:
						if (packet.Payload != null && packet.Payload.Length != 0)
						{
							Util.UpdateTextBox(USBTextBox, "[RX->] Error, SCI-bus RAM-table invalid (" + Util.ByteToHexString(packet.Payload) + "):", PacketHelper.Serialize(packet));
						}
						else
						{
							Util.UpdateTextBox(USBTextBox, "[RX->] Error, SCI-bus RAM-table invalid:", PacketHelper.Serialize(packet));
						}
						break;
					case 250:
						if (packet.Payload != null && packet.Payload.Length > 1)
						{
							Util.UpdateTextBox(USBTextBox, "[RX->] Error, no response from SCI-bus (" + Util.ByteToHexString(packet.Payload, 0, 2) + "):", PacketHelper.Serialize(packet));
						}
						else
						{
							Util.UpdateTextBox(USBTextBox, "[RX->] Error, no response from SCI-bus:", PacketHelper.Serialize(packet));
						}
						break;
					case 251:
						Util.UpdateTextBox(USBTextBox, "[RX->] Error, external EEPROM not found:", PacketHelper.Serialize(packet));
						break;
					case 252:
						Util.UpdateTextBox(USBTextBox, "[RX->] Error, external EEPROM read failure:", PacketHelper.Serialize(packet));
						break;
					case 253:
						Util.UpdateTextBox(USBTextBox, "[RX->] Error, external EEPROM write failure:", PacketHelper.Serialize(packet));
						break;
					case 254:
						Util.UpdateTextBox(USBTextBox, "[RX->] Error, internal error:", PacketHelper.Serialize(packet));
						break;
					case byte.MaxValue:
						Util.UpdateTextBox(USBTextBox, "[RX->] Error, fatal error:", PacketHelper.Serialize(packet));
						break;
					default:
						Util.UpdateTextBox(USBTextBox, "[RX->] Error packet received:", PacketHelper.Serialize(packet));
						break;
					}
					break;
				default:
					Util.UpdateTextBox(USBTextBox, "[RX->] Packet received:", PacketHelper.Serialize(packet));
					break;
				case 4:
					break;
				}
				break;
			case 1:
				if ((CCDBusOnDemandToolStripMenuItem.Checked && ((Control)ScannerTabControl.SelectedTab).Name == "CCDBusControlTabPage") || !CCDBusOnDemandToolStripMenuItem.Checked)
				{
					if (Settings.Default.DisplayRawBusPackets)
					{
						Util.UpdateTextBox(USBTextBox, "[RX->] CCD-bus message:", PacketHelper.Serialize(packet));
					}
					CCD.AddMessage(packet.Payload.ToArray());
				}
				break;
			case 4:
				if ((PCIBusOnDemandToolStripMenuItem.Checked && ((Control)ScannerTabControl.SelectedTab).Name == "PCIBusControlTabPage") || !PCIBusOnDemandToolStripMenuItem.Checked)
				{
					if (Settings.Default.DisplayRawBusPackets)
					{
						Util.UpdateTextBox(USBTextBox, "[RX->] PCI-bus message:", PacketHelper.Serialize(packet));
					}
					PCI.AddMessage(packet.Payload.ToArray());
				}
				break;
			case 2:
				switch (packet.Mode)
				{
				case 1:
					if (packet.Payload.Length > 4)
					{
						switch (packet.Payload[4])
						{
						case 16:
						case 50:
							if (packet.Payload.Length < 7)
							{
								Util.UpdateTextBox(USBTextBox, "[RX->] Invalid SCI-bus (PCM) stored fault code list:", PacketHelper.Serialize(packet));
							}
							else
							{
								int num3 = packet.Payload.Length - 1;
								if (packet.Payload[num3] != Util.ChecksumCalculator(packet.Payload, 4, num3))
								{
									Util.UpdateTextBox(USBTextBox, "[RX->] SCI-bus (PCM) fault code checksum error:", PacketHelper.Serialize(packet));
								}
								else
								{
									Util.UpdateTextBox(USBTextBox, "[RX->] SCI-bus (PCM) stored fault code list:", PacketHelper.Serialize(packet));
									List<byte> list3 = new List<byte>();
									list3.AddRange(packet.Payload.Skip(5).Take(packet.Payload.Length - 6));
									list3.Remove(253);
									list3.Remove(254);
									if (list3.Count > 0)
									{
										StringBuilder stringBuilder3 = new StringBuilder();
										foreach (byte item2 in list3)
										{
											int num4 = PCM.SBEC3EngineDTC.Rows.IndexOf(PCM.SBEC3EngineDTC.Rows.Find(item2));
											if (num4 > -1)
											{
												stringBuilder3.Append(Util.ByteToHexStringSimple(new byte[1] { item2 }) + ": " + PCM.SBEC3EngineDTC.Rows[num4]["description"]?.ToString() + Environment.NewLine);
											}
											else
											{
												stringBuilder3.Append(Util.ByteToHexStringSimple(new byte[1] { item2 }) + ": UNRECOGNIZED DTC" + Environment.NewLine);
											}
										}
										stringBuilder3.Remove(stringBuilder3.Length - 2, 2);
										Util.UpdateTextBox(USBTextBox, "[INFO] Stored PCM fault codes found:" + Environment.NewLine + stringBuilder3.ToString());
									}
									else
									{
										Util.UpdateTextBox(USBTextBox, "[INFO] No stored PCM fault code found.");
									}
								}
							}
							break;
						case 17:
							if (packet.Payload.Length < 7)
							{
								Util.UpdateTextBox(USBTextBox, "[RX->] Invalid SCI-bus (PCM) pending fault code list:", PacketHelper.Serialize(packet));
							}
							else
							{
								Util.UpdateTextBox(USBTextBox, "[RX->] SCI-bus (PCM) pending fault code list:", PacketHelper.Serialize(packet));
								List<byte> list = new List<byte>();
								list.AddRange(packet.Payload.Skip(5).Take(packet.Payload.Length - 2));
								if (list[0] == 0 && list[1] == 0)
								{
									Util.UpdateTextBox(USBTextBox, "[INFO] No pending PCM fault code found.");
								}
								else
								{
									StringBuilder stringBuilder = new StringBuilder();
									foreach (byte item3 in list)
									{
										if (item3 != 0)
										{
											int num = PCM.SBEC3EngineDTC.Rows.IndexOf(PCM.SBEC3EngineDTC.Rows.Find(item3));
											if (num > -1)
											{
												stringBuilder.Append(Util.ByteToHexStringSimple(new byte[1] { item3 }) + ": " + PCM.SBEC3EngineDTC.Rows[num]["description"]?.ToString() + Environment.NewLine);
											}
											else
											{
												stringBuilder.Append(Util.ByteToHexStringSimple(new byte[1] { item3 }) + ": EMPTY DTC SLOT" + Environment.NewLine);
											}
										}
									}
									stringBuilder.Remove(stringBuilder.Length - 2, 2);
									Util.UpdateTextBox(USBTextBox, "[INFO] Pending PCM fault codes found:" + Environment.NewLine + stringBuilder.ToString());
								}
							}
							break;
						case 46:
						case 51:
							if (packet.Payload.Length > 5)
							{
								Util.UpdateTextBox(USBTextBox, "[RX->] SCI-bus (PCM) one-trip fault code list:", PacketHelper.Serialize(packet));
								List<byte> list2 = new List<byte>();
								list2.AddRange(packet.Payload.Skip(5).Take(packet.Payload.Length - 6));
								list2.Remove(253);
								list2.Remove(254);
								if (list2.Count > 0)
								{
									StringBuilder stringBuilder2 = new StringBuilder();
									foreach (byte item4 in list2)
									{
										int num2 = PCM.SBEC3EngineDTC.Rows.IndexOf(PCM.SBEC3EngineDTC.Rows.Find(item4));
										if (num2 > -1)
										{
											stringBuilder2.Append(Util.ByteToHexStringSimple(new byte[1] { item4 }) + ": " + PCM.SBEC3EngineDTC.Rows[num2]["description"]?.ToString() + Environment.NewLine);
										}
										else
										{
											stringBuilder2.Append(Util.ByteToHexStringSimple(new byte[1] { item4 }) + ": UNRECOGNIZED DTC" + Environment.NewLine);
										}
									}
									stringBuilder2.Remove(stringBuilder2.Length - 2, 2);
									Util.UpdateTextBox(USBTextBox, "[INFO] One-trip PCM fault codes found:" + Environment.NewLine + stringBuilder2.ToString());
								}
								else
								{
									Util.UpdateTextBox(USBTextBox, "[INFO] No one-trip PCM fault code found.");
								}
							}
							else
							{
								Util.UpdateTextBox(USBTextBox, "[RX->] Invalid SCI-bus (PCM) one-trip fault code list:", PacketHelper.Serialize(packet));
							}
							break;
						case 23:
							if (packet.Payload.Length > 5)
							{
								Util.UpdateTextBox(USBTextBox, "[RX->] SCI-bus (PCM) erase fault code list:", PacketHelper.Serialize(packet));
								if (packet.Payload[5] == 224)
								{
									Util.UpdateTextBox(USBTextBox, "[INFO] SCI-bus (PCM) fault code list erased.");
								}
								else
								{
									Util.UpdateTextBox(USBTextBox, "[INFO] SCI-bus (PCM) erase fault code list error.");
								}
							}
							else
							{
								Util.UpdateTextBox(USBTextBox, "[RX->] Invalid SCI-bus (PCM) erase fault code list response:", PacketHelper.Serialize(packet));
							}
							break;
						default:
							if (Settings.Default.DisplayRawBusPackets)
							{
								Util.UpdateTextBox(USBTextBox, "[RX->] SCI-bus (PCM) low-speed message:", PacketHelper.Serialize(packet));
							}
							break;
						}
					}
					break;
				case 2:
					if (Settings.Default.DisplayRawBusPackets)
					{
						Util.UpdateTextBox(USBTextBox, "[RX->] SCI-bus (PCM) high-speed message:", PacketHelper.Serialize(packet));
					}
					break;
				default:
					if (Settings.Default.DisplayRawBusPackets)
					{
						Util.UpdateTextBox(USBTextBox, "[RX->] SCI-bus (PCM) message:", PacketHelper.Serialize(packet));
					}
					break;
				}
				PCM.AddMessage(packet.Payload.ToArray());
				break;
			case 3:
				if (Settings.Default.DisplayRawBusPackets)
				{
					Util.UpdateTextBox(USBTextBox, "[RX->] SCI-bus (TCM) message:", PacketHelper.Serialize(packet));
				}
				TCM.AddMessage(packet.Payload.ToArray());
				break;
			default:
				Util.UpdateTextBox(USBTextBox, "[RX->] Packet received:", PacketHelper.Serialize(packet));
				break;
			}
		}, null);
	}

	private void UpdateCCDTable(object sender, EventArgs e)
	{
		if (((ListBox)CCDBusDiagnosticsListBox).Items.Count == 0)
		{
			ObjectCollection items = ((ListBox)CCDBusDiagnosticsListBox).Items;
			object[] array = CCD.Diagnostics.Table.ToArray();
			items.AddRange(array);
		}
		else
		{
			CCDTableBuffer.Add(CCD.Diagnostics.Table[CCD.Diagnostics.LastUpdatedLine]);
			CCDTableBufferLocation.Add(CCD.Diagnostics.LastUpdatedLine);
			CCDTableRowCountHistory.Add(CCD.Diagnostics.Table.Count);
		}
	}

	private void UpdatePCITable(object sender, EventArgs e)
	{
		if (((ListBox)PCIBusDiagnosticsListBox).Items.Count == 0)
		{
			ObjectCollection items = ((ListBox)PCIBusDiagnosticsListBox).Items;
			object[] array = PCI.Diagnostics.Table.ToArray();
			items.AddRange(array);
		}
		else
		{
			PCITableBuffer.Add(PCI.Diagnostics.Table[PCI.Diagnostics.LastUpdatedLine]);
			PCITableBufferLocation.Add(PCI.Diagnostics.LastUpdatedLine);
			PCITableRowCountHistory.Add(PCI.Diagnostics.Table.Count);
		}
	}

	private void UpdateSCIPCMTable(object sender, EventArgs e)
	{
		if (((ListBox)SCIBusPCMDiagnosticsListBox).Items.Count == 0)
		{
			ObjectCollection items = ((ListBox)SCIBusPCMDiagnosticsListBox).Items;
			object[] array = PCM.Diagnostics.Table.ToArray();
			items.AddRange(array);
			return;
		}
		PCMTableBuffer.Add(PCM.Diagnostics.Table[PCM.Diagnostics.LastUpdatedLine]);
		PCMTableBufferLocation.Add(PCM.Diagnostics.LastUpdatedLine);
		PCMTableRowCountHistory.Add(PCM.Diagnostics.Table.Count);
		if (PCM.speed == "62500 baud" && PCM.Diagnostics.RAMDumpTableVisible)
		{
			for (int i = 0; i < 23; i++)
			{
				PCMTableBuffer.Add(PCM.Diagnostics.Table[PCM.Diagnostics.Table.Count - 23 + i]);
				PCMTableBufferLocation.Add(PCM.Diagnostics.Table.Count - 23 + i);
				PCMTableRowCountHistory.Add(PCM.Diagnostics.Table.Count);
			}
		}
	}

	private void UpdateSCITCMTable(object sender, EventArgs e)
	{
		if (((ListBox)SCIBusTCMDiagnosticsListBox).Items.Count == 0)
		{
			ObjectCollection items = ((ListBox)SCIBusTCMDiagnosticsListBox).Items;
			object[] array = TCM.Diagnostics.Table.ToArray();
			items.AddRange(array);
		}
		else
		{
			TCMTableBuffer.Add(TCM.Diagnostics.Table[TCM.Diagnostics.LastUpdatedLine]);
			TCMTableBufferLocation.Add(TCM.Diagnostics.LastUpdatedLine);
			TCMTableRowCountHistory.Add(TCM.Diagnostics.Table.Count);
		}
	}

	public void TransmitUSBPacket(string description, Packet packet)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Expected O, but got Unknown
		((Control)this).Invoke((Delegate)(MethodInvoker)delegate
		{
			if (!USBSendPacketComboBox.Items.Contains((object)((Control)USBSendPacketComboBox).Text))
			{
				USBSendPacketComboBox.Items.Add((object)((Control)USBSendPacketComboBox).Text);
			}
		});
		Util.UpdateTextBox(USBTextBox, description, PacketHelper.Serialize(packet));
	}

	public void UpdateUSBTextBox(string description)
	{
		Util.UpdateTextBox(USBTextBox, description);
	}

	public void SelectSCIBusHSMode()
	{
		((ListControl)SCIBusSpeedComboBox).SelectedIndex = 3;
		SCIBusModuleConfigSpeedApplyButton_Click(this, EventArgs.Empty);
	}

	public void SelectSCIBusLSMode()
	{
		((ListControl)SCIBusSpeedComboBox).SelectedIndex = 2;
		SCIBusModuleConfigSpeedApplyButton_Click(this, EventArgs.Empty);
	}

	private void USBSendPacketButton_Click(object sender, EventArgs e)
	{
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		if (((Control)USBSendPacketComboBox).Text == string.Empty)
		{
			return;
		}
		byte[] array = Util.HexStringToByte(((Control)USBSendPacketComboBox).Text);
		if (array == null || array.Length < 6)
		{
			MessageBox.Show("Minimum packet length = 6 bytes.", "Warning", (MessageBoxButtons)0, (MessageBoxIcon)48);
			return;
		}
		Packet packet = PacketHelper.Deserialize(array);
		if (packet == null)
		{
			Util.UpdateTextBox(USBTextBox, "[INFO] Invalid packet");
			return;
		}
		if (!USBSendPacketComboBox.Items.Contains((object)((Control)USBSendPacketComboBox).Text))
		{
			USBSendPacketComboBox.Items.Add((object)((Control)USBSendPacketComboBox).Text);
		}
		Util.UpdateTextBox(USBTextBox, "[<-TX] Data transmitted:", PacketHelper.Serialize(packet));
		SerialService.WritePacket(packet);
	}

	private void USBSendPacketComboBox_KeyPress(object sender, KeyPressEventArgs e)
	{
		if (e.KeyChar == '\r')
		{
			e.Handled = true;
			USBSendPacketButton_Click(this, EventArgs.Empty);
		}
	}

	private void COMPortsRefreshButton_Click(object sender, EventArgs e)
	{
		UpdateCOMPortList();
	}

	private void DemoButton_Click(object sender, EventArgs e)
	{
		Util.UpdateTextBox(USBTextBox, "[INFO] GUI is now running in demo mode." + Environment.NewLine + "       Explore features without scanner.");
		((Control)USBCommunicationGroupBox).Enabled = true;
		((Control)ScannerTabControl).Enabled = true;
		((Control)DiagnosticsGroupBox).Enabled = true;
		((ToolStripItem)ReadMemoryToolStripMenuItem).Enabled = true;
		((ToolStripItem)ReadWriteMemoryToolStripMenuItem).Enabled = true;
		((ToolStripItem)BootstrapToolsToolStripMenuItem).Enabled = true;
		((ToolStripItem)EngineToolsToolStripMenuItem).Enabled = true;
		((ToolStripItem)ABSToolsToolStripMenuItem).Enabled = true;
	}

	private void COMPortsComboBox_SelectedIndexChanged(object sender, EventArgs e)
	{
		SelectedPort = ((Control)COMPortsComboBox).Text;
	}

	private void ConnectButton_Click(object sender, EventArgs e)
	{
		UpdateCOMPortList();
		if (SelectedPort == "N/A" || SelectedPort == string.Empty)
		{
			return;
		}
		if (((Control)ConnectButton).Text == strings.Connect)
		{
			Util.UpdateTextBox(USBTextBox, "[INFO] Connecting to " + SelectedPort + ".");
			if (!SerialService.Connect(SelectedPort))
			{
				Util.UpdateTextBox(USBTextBox, "[INFO] Device not found on " + SelectedPort + ".");
				return;
			}
			Util.UpdateTextBox(USBTextBox, "[INFO] Device connected to " + SelectedPort + ".");
			DeviceFound = true;
			((Control)ConnectButton).Text = strings.Disconnect;
			((Control)COMPortsComboBox).Enabled = false;
			((Control)COMPortsRefreshButton).Enabled = false;
			((Control)USBCommunicationGroupBox).Enabled = true;
			((Control)ScannerTabControl).Enabled = true;
			((Control)DiagnosticsGroupBox).Enabled = true;
			((ToolStripItem)ReadMemoryToolStripMenuItem).Enabled = true;
			((ToolStripItem)ReadWriteMemoryToolStripMenuItem).Enabled = true;
			((ToolStripItem)BootstrapToolsToolStripMenuItem).Enabled = true;
			((ToolStripItem)EngineToolsToolStripMenuItem).Enabled = true;
			((ToolStripItem)ABSToolsToolStripMenuItem).Enabled = true;
			SerialService.PacketReceived += PacketReceivedHandler;
			CCD.Diagnostics.TableUpdated += UpdateCCDTable;
			PCI.Diagnostics.TableUpdated += UpdatePCITable;
			PCM.Diagnostics.TableUpdated += UpdateSCIPCMTable;
			TCM.Diagnostics.TableUpdated += UpdateSCITCMTable;
			((ContainerControl)this).ActiveControl = (Control)(object)ExpandButton;
			VersionInfoButton_Click(this, EventArgs.Empty);
			StatusButton_Click(this, EventArgs.Empty);
		}
		else if (((Control)ConnectButton).Text == strings.Disconnect)
		{
			SerialService.Disconnect();
			SerialService.PacketReceived -= PacketReceivedHandler;
			CCD.Diagnostics.TableUpdated -= UpdateCCDTable;
			PCI.Diagnostics.TableUpdated -= UpdatePCITable;
			PCM.Diagnostics.TableUpdated -= UpdateSCIPCMTable;
			TCM.Diagnostics.TableUpdated -= UpdateSCITCMTable;
			((Control)ConnectButton).Text = strings.Connect;
			((Control)COMPortsComboBox).Enabled = true;
			((Control)COMPortsRefreshButton).Enabled = true;
			((Control)USBCommunicationGroupBox).Enabled = false;
			((Control)ScannerTabControl).Enabled = false;
			((Control)DiagnosticsGroupBox).Enabled = false;
			((ToolStripItem)ReadMemoryToolStripMenuItem).Enabled = false;
			((ToolStripItem)ReadWriteMemoryToolStripMenuItem).Enabled = false;
			((ToolStripItem)BootstrapToolsToolStripMenuItem).Enabled = false;
			((ToolStripItem)EngineToolsToolStripMenuItem).Enabled = false;
			((ToolStripItem)ABSToolsToolStripMenuItem).Enabled = false;
			DeviceFound = false;
			timeout = false;
			Util.UpdateTextBox(USBTextBox, "[INFO] Device disconnected (" + SelectedPort + ").");
			((Control)this).Text = "Chrysler Scanner  |  GUI " + GUIVersion;
		}
	}

	private void ExpandButton_Click(object sender, EventArgs e)
	{
		if (((Control)ExpandButton).Text == strings.Expand)
		{
			((Control)DiagnosticsGroupBox).Visible = true;
			((Form)this).Size = new Size(1300, 650);
			((Form)this).CenterToScreen();
			((Control)ExpandButton).Text = strings.Collapse;
		}
		else if (((Control)ExpandButton).Text == strings.Collapse)
		{
			((Form)this).Size = new Size(405, 650);
			((Form)this).CenterToScreen();
			((Control)DiagnosticsGroupBox).Visible = false;
			((Control)ExpandButton).Text = strings.Expand;
		}
	}

	private void ResetButton_Click(object sender, EventArgs e)
	{
		Packet packet = new Packet();
		packet.Bus = 0;
		packet.Command = 0;
		packet.Mode = 0;
		packet.Payload = null;
		Util.UpdateTextBox(USBTextBox, "[<-TX] Reset device:", PacketHelper.Serialize(packet));
		SerialService.WritePacket(packet);
	}

	private void HandshakeButton_Click(object sender, EventArgs e)
	{
		Packet packet = new Packet();
		packet.Bus = 0;
		packet.Command = 1;
		packet.Mode = 0;
		packet.Payload = null;
		Util.UpdateTextBox(USBTextBox, "[<-TX] Handshake request:", PacketHelper.Serialize(packet));
		SerialService.WritePacket(packet);
	}

	private void StatusButton_Click(object sender, EventArgs e)
	{
		Packet packet = new Packet();
		packet.Bus = 0;
		packet.Command = 2;
		packet.Mode = 0;
		packet.Payload = null;
		Util.UpdateTextBox(USBTextBox, "[<-TX] Status request:", PacketHelper.Serialize(packet));
		SerialService.WritePacket(packet);
	}

	private void VersionInfoButton_Click(object sender, EventArgs e)
	{
		Packet packet = new Packet();
		packet.Bus = 0;
		packet.Command = 4;
		packet.Mode = 1;
		packet.Payload = null;
		Util.UpdateTextBox(USBTextBox, "[<-TX] Hardware/Firmware information request:", PacketHelper.Serialize(packet));
		SerialService.WritePacket(packet);
	}

	private void TimestampButton_Click(object sender, EventArgs e)
	{
		Packet packet = new Packet();
		packet.Bus = 0;
		packet.Command = 4;
		packet.Mode = 2;
		packet.Payload = null;
		Util.UpdateTextBox(USBTextBox, "[<-TX] Timestamp request:", PacketHelper.Serialize(packet));
		SerialService.WritePacket(packet);
	}

	private void VoltagesButton_Click(object sender, EventArgs e)
	{
		Packet packet = new Packet();
		packet.Bus = 0;
		packet.Command = 4;
		packet.Mode = 8;
		packet.Payload = null;
		Util.UpdateTextBox(USBTextBox, "[<-TX] Voltages request:", PacketHelper.Serialize(packet));
		SerialService.WritePacket(packet);
	}

	private void EEPROMChecksumButton_Click(object sender, EventArgs e)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		if (InternalEEPROMRadioButton.Checked)
		{
			MessageBox.Show("The internal EEPROM has no assigned checksum!", "Warning", (MessageBoxButtons)0, (MessageBoxIcon)48);
			return;
		}
		Packet packet = new Packet();
		packet.Bus = 0;
		packet.Command = 4;
		packet.Mode = 4;
		packet.Payload = null;
		Util.UpdateTextBox(USBTextBox, "[<-TX] External EEPROM checksum request:", PacketHelper.Serialize(packet));
		SerialService.WritePacket(packet);
	}

	private void ReadEEPROMButton_Click(object sender, EventArgs e)
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_019c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_021d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0188: Unknown result type (might be due to invalid IL or missing references)
		//IL_02af: Unknown result type (might be due to invalid IL or missing references)
		byte[] array = Util.HexStringToByte(((Control)EEPROMReadAddressTextBox).Text);
		if (array.Length != 2)
		{
			MessageBox.Show("Read address needs to be 2 bytes long!", "Warning", (MessageBoxButtons)0, (MessageBoxIcon)48);
		}
		else
		{
			if (array.Length == 0)
			{
				return;
			}
			int num = (array[0] << 8) + array[1];
			if (!int.TryParse(((Control)EEPROMReadCountTextBox).Text, out var result))
			{
				((Control)EEPROMReadCountTextBox).Text = "1";
				result = 1;
			}
			byte[] collection = new byte[2]
			{
				(byte)(result >> 8),
				(byte)((uint)result & 0xFFu)
			};
			if (InternalEEPROMRadioButton.Checked)
			{
				if (result == 1)
				{
					Packet packet = new Packet();
					packet.Bus = 0;
					packet.Command = 14;
					packet.Mode = 2;
					packet.Payload = array;
					Util.UpdateTextBox(USBTextBox, "[<-TX] Internal EEPROM byte read request:", PacketHelper.Serialize(packet));
					SerialService.WritePacket(packet);
					if ((long)num > 4095L)
					{
						MessageBox.Show("Internal EEPROM size exceeded (4096 bytes)!", "Warning", (MessageBoxButtons)0, (MessageBoxIcon)48);
					}
				}
				else if (result > 1)
				{
					List<byte> list = new List<byte>();
					list.AddRange(array);
					list.AddRange(collection);
					Packet packet2 = new Packet();
					packet2.Bus = 0;
					packet2.Command = 14;
					packet2.Mode = 3;
					packet2.Payload = list.ToArray();
					Util.UpdateTextBox(USBTextBox, "[<-TX] Internal EEPROM block read request:", PacketHelper.Serialize(packet2));
					SerialService.WritePacket(packet2);
					if ((long)(num + (result - 1)) > 4095L)
					{
						MessageBox.Show("Internal EEPROM size exceeded (4096 bytes)!", "Warning", (MessageBoxButtons)0, (MessageBoxIcon)48);
					}
				}
				else
				{
					MessageBox.Show("Count value cannot be 0!", "Warning", (MessageBoxButtons)0, (MessageBoxIcon)48);
				}
			}
			else
			{
				if (!ExternalEEPROMRadioButton.Checked)
				{
					return;
				}
				if (result == 1)
				{
					Packet packet3 = new Packet();
					packet3.Bus = 0;
					packet3.Command = 14;
					packet3.Mode = 4;
					packet3.Payload = array;
					Util.UpdateTextBox(USBTextBox, "[<-TX] External EEPROM byte read request:", PacketHelper.Serialize(packet3));
					SerialService.WritePacket(packet3);
					if ((long)num > 4095L)
					{
						MessageBox.Show("External EEPROM size exceeded (4096 bytes)!", "Warning", (MessageBoxButtons)0, (MessageBoxIcon)48);
					}
				}
				else if (result > 1)
				{
					List<byte> list2 = new List<byte>();
					list2.AddRange(array);
					list2.AddRange(collection);
					Packet packet4 = new Packet();
					packet4.Bus = 0;
					packet4.Command = 14;
					packet4.Mode = 5;
					packet4.Payload = list2.ToArray();
					Util.UpdateTextBox(USBTextBox, "[<-TX] External EEPROM block read request:", PacketHelper.Serialize(packet4));
					SerialService.WritePacket(packet4);
					if ((long)(num + (result - 1)) > 4095L)
					{
						MessageBox.Show("External EEPROM size exceeded (4096 bytes)!", "Warning", (MessageBoxButtons)0, (MessageBoxIcon)48);
					}
				}
				else
				{
					MessageBox.Show("Count value cannot be 0!", "Warning", (MessageBoxButtons)0, (MessageBoxIcon)48);
				}
			}
		}
	}

	private void WriteEEPROMButton_Click(object sender, EventArgs e)
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0199: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_023c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0185: Unknown result type (might be due to invalid IL or missing references)
		//IL_02cf: Unknown result type (might be due to invalid IL or missing references)
		byte[] array = Util.HexStringToByte(((Control)EEPROMWriteAddressTextBox).Text);
		if (array.Length != 2)
		{
			MessageBox.Show("Write address needs to be 2 bytes long!", "Warning", (MessageBoxButtons)0, (MessageBoxIcon)48);
		}
		else
		{
			if (array.Length == 0)
			{
				return;
			}
			int num = (array[0] << 8) + array[1];
			byte[] array2 = Util.HexStringToByte(((Control)EEPROMWriteValuesTextBox).Text);
			if (array2.Length == 0)
			{
				return;
			}
			int num2 = array2.Length;
			if (InternalEEPROMRadioButton.Checked)
			{
				if (num2 == 1)
				{
					List<byte> list = new List<byte>();
					list.AddRange(array);
					list.Add(array2[0]);
					Packet packet = new Packet();
					packet.Bus = 0;
					packet.Command = 14;
					packet.Mode = 6;
					packet.Payload = list.ToArray();
					Util.UpdateTextBox(USBTextBox, "[<-TX] Internal EEPROM byte write request:", PacketHelper.Serialize(packet));
					SerialService.WritePacket(packet);
					if ((long)num > 4095L)
					{
						MessageBox.Show("Internal EEPROM size exceeded (4096 bytes)!", "Warning", (MessageBoxButtons)0, (MessageBoxIcon)48);
					}
				}
				else if (num2 > 1)
				{
					List<byte> list2 = new List<byte>();
					list2.AddRange(array);
					list2.AddRange(array2);
					Packet packet2 = new Packet();
					packet2.Bus = 0;
					packet2.Command = 14;
					packet2.Mode = 7;
					packet2.Payload = list2.ToArray();
					Util.UpdateTextBox(USBTextBox, "[<-TX] Internal EEPROM block write request:", PacketHelper.Serialize(packet2));
					SerialService.WritePacket(packet2);
					if ((long)(num + (num2 - 1)) > 4095L)
					{
						MessageBox.Show("Internal EEPROM size exceeded (4096 bytes)!", "Warning", (MessageBoxButtons)0, (MessageBoxIcon)48);
					}
				}
				else
				{
					MessageBox.Show("Count value cannot be 0!", "Warning", (MessageBoxButtons)0, (MessageBoxIcon)48);
				}
			}
			else
			{
				if (!ExternalEEPROMRadioButton.Checked)
				{
					return;
				}
				if (num2 == 1)
				{
					List<byte> list3 = new List<byte>();
					list3.AddRange(array);
					list3.Add(array2[0]);
					Packet packet3 = new Packet();
					packet3.Bus = 0;
					packet3.Command = 14;
					packet3.Mode = 8;
					packet3.Payload = list3.ToArray();
					Util.UpdateTextBox(USBTextBox, "[<-TX] External EEPROM byte write request:", PacketHelper.Serialize(packet3));
					SerialService.WritePacket(packet3);
					if ((long)num > 4095L)
					{
						MessageBox.Show("External EEPROM size exceeded (4096 bytes)!", "Warning", (MessageBoxButtons)0, (MessageBoxIcon)48);
					}
				}
				else if (num2 > 1)
				{
					List<byte> list4 = new List<byte>();
					list4.AddRange(array);
					list4.AddRange(array2);
					Packet packet4 = new Packet();
					packet4.Bus = 0;
					packet4.Command = 14;
					packet4.Mode = 9;
					packet4.Payload = list4.ToArray();
					Util.UpdateTextBox(USBTextBox, "[<-TX] External EEPROM block write request:", PacketHelper.Serialize(packet4));
					SerialService.WritePacket(packet4);
					if ((long)(num + (num2 - 1)) > 4095L)
					{
						MessageBox.Show("External EEPROM size exceeded (4096 bytes)!", "Warning", (MessageBoxButtons)0, (MessageBoxIcon)48);
					}
				}
				else
				{
					MessageBox.Show("Count value cannot be 0!", "Warning", (MessageBoxButtons)0, (MessageBoxIcon)48);
				}
			}
		}
	}

	private void SetLEDsButton_Click(object sender, EventArgs e)
	{
		List<byte> list = new List<byte>();
		if (!int.TryParse(((Control)HeartbeatIntervalTextBox).Text, out var result))
		{
			((Control)HeartbeatIntervalTextBox).Text = "5000";
			result = 5000;
		}
		if (!int.TryParse(((Control)LEDBlinkDurationTextBox).Text, out var result2))
		{
			((Control)LEDBlinkDurationTextBox).Text = "50";
			result2 = 50;
		}
		byte b = (byte)((uint)(result >> 8) & 0xFFu);
		byte b2 = (byte)((uint)result & 0xFFu);
		byte b3 = (byte)((uint)(result2 >> 8) & 0xFFu);
		byte b4 = (byte)((uint)result2 & 0xFFu);
		list.AddRange(new byte[4] { b, b2, b3, b4 });
		Packet packet = new Packet();
		packet.Bus = 0;
		packet.Command = 3;
		packet.Mode = 1;
		packet.Payload = list.ToArray();
		Util.UpdateTextBox(USBTextBox, "[<-TX] Change LED settings:", PacketHelper.Serialize(packet));
		SerialService.WritePacket(packet);
	}

	private void EEPROMWriteEnableCheckBox_CheckedChanged(object sender, EventArgs e)
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Invalid comparison between Unknown and I4
		if (EEPROMWriteEnableCheckBox.Checked)
		{
			if ((int)MessageBox.Show("Modifying EEPROM values can cause the device to behave unpredictably!" + Environment.NewLine + "Are you sure you want to continue?", "Warning", (MessageBoxButtons)4, (MessageBoxIcon)48, (MessageBoxDefaultButton)256) == 6)
			{
				((Control)WriteEEPROMButton).Enabled = true;
				((Control)EEPROMWriteAddressLabel).Enabled = true;
				((Control)EEPROMWriteAddressTextBox).Enabled = true;
				((Control)EEPROMWriteValuesLabel).Enabled = true;
				((Control)EEPROMWriteValuesTextBox).Enabled = true;
			}
			else
			{
				EEPROMWriteEnableCheckBox.Checked = false;
			}
		}
		else
		{
			((Control)WriteEEPROMButton).Enabled = false;
			((Control)EEPROMWriteAddressLabel).Enabled = false;
			((Control)EEPROMWriteAddressTextBox).Enabled = false;
			((Control)EEPROMWriteValuesLabel).Enabled = false;
			((Control)EEPROMWriteValuesTextBox).Enabled = false;
		}
	}

	private void EEPROMReadAddressTextBox_KeyPress(object sender, KeyPressEventArgs e)
	{
		if (e.KeyChar == '\r')
		{
			e.Handled = true;
			ReadEEPROMButton_Click(this, EventArgs.Empty);
		}
	}

	private void EEPROMReadCountTextBox_KeyPress(object sender, KeyPressEventArgs e)
	{
		if (e.KeyChar == '\r')
		{
			e.Handled = true;
			ReadEEPROMButton_Click(this, EventArgs.Empty);
		}
	}

	private void EEPROMWriteAddressTextBox_KeyPress(object sender, KeyPressEventArgs e)
	{
		if (e.KeyChar == '\r')
		{
			e.Handled = true;
			WriteEEPROMButton_Click(this, EventArgs.Empty);
		}
	}

	private void EEPROMWriteValuesTextBox_KeyPress(object sender, KeyPressEventArgs e)
	{
		if (e.KeyChar == '\r')
		{
			e.Handled = true;
			WriteEEPROMButton_Click(this, EventArgs.Empty);
		}
	}

	private void HeartbeatIntervalTextBox_KeyPress(object sender, KeyPressEventArgs e)
	{
		if (e.KeyChar == '\r')
		{
			e.Handled = true;
			SetLEDsButton_Click(this, EventArgs.Empty);
		}
	}

	private void LEDBlinkDurationTextBox_KeyPress(object sender, KeyPressEventArgs e)
	{
		if (e.KeyChar == '\r')
		{
			e.Handled = true;
			SetLEDsButton_Click(this, EventArgs.Empty);
		}
	}

	private void CCDBusTxMessageAddButton_Click(object sender, EventArgs e)
	{
		string[] array = ((Control)CCDBusTxMessageComboBox).Text.Split(new char[1] { ',' });
		for (int i = 0; i < array.Length; i++)
		{
			byte[] array2 = Util.HexStringToByte(array[i]);
			if (array2.Length == 0)
			{
				continue;
			}
			byte b = array2[0];
			bool flag = false;
			int num = 0;
			for (int j = 0; j < CCDBusTxMessagesListBox.Items.Count; j++)
			{
				if (Util.HexStringToByte(CCDBusTxMessagesListBox.Items[j].ToString())[0] == b)
				{
					flag = true;
					num = j;
					break;
				}
			}
			string text = Util.ByteToHexString(array2, 0, array2.Length);
			if (!flag)
			{
				CCDBusTxMessagesListBox.Items.Add((object)text);
			}
			else if (CCDBusOverwriteDuplicateIDCheckBox.Checked && !CCDBusTxMessageRepeatIntervalCheckBox.Checked)
			{
				CCDBusTxMessagesListBox.Items.RemoveAt(num);
				CCDBusTxMessagesListBox.Items.Insert(num, (object)text);
			}
			else
			{
				CCDBusTxMessagesListBox.Items.Add((object)text);
			}
			if (CCDBusTxMessagesListBox.Items.Count > 0)
			{
				((Control)CCDBusTxMessageRemoveItemButton).Enabled = true;
				((Control)CCDBusTxMessageClearListButton).Enabled = true;
				((Control)CCDBusSendMessagesButton).Enabled = true;
				((Control)CCDBusStopRepeatedMessagesButton).Enabled = true;
			}
			else
			{
				((Control)CCDBusTxMessageRemoveItemButton).Enabled = false;
				((Control)CCDBusTxMessageClearListButton).Enabled = false;
				((Control)CCDBusSendMessagesButton).Enabled = false;
				((Control)CCDBusStopRepeatedMessagesButton).Enabled = false;
			}
			if (!CCDBusTxMessageComboBox.Items.Contains((object)((Control)CCDBusTxMessageComboBox).Text))
			{
				CCDBusTxMessageComboBox.Items.Add((object)((Control)CCDBusTxMessageComboBox).Text);
			}
			if (((Control)CCDBusTxMessageAddButton).Text == "Edit")
			{
				((Control)CCDBusTxMessageAddButton).Text = "Add";
			}
		}
		CCDBusTxMessagesListBox.TopIndex = CCDBusTxMessagesListBox.Items.Count - 1;
	}

	private void CCDBusTxMessageRemoveItemButton_Click(object sender, EventArgs e)
	{
		if (((ListControl)CCDBusTxMessagesListBox).SelectedIndex > -1)
		{
			for (int num = CCDBusTxMessagesListBox.SelectedIndices.Count - 1; num >= 0; num--)
			{
				int num2 = CCDBusTxMessagesListBox.SelectedIndices[num];
				CCDBusTxMessagesListBox.Items.RemoveAt(num2);
			}
			if (CCDBusTxMessagesListBox.Items.Count > 0)
			{
				((Control)CCDBusTxMessageRemoveItemButton).Enabled = true;
				((Control)CCDBusTxMessageClearListButton).Enabled = true;
				((Control)CCDBusSendMessagesButton).Enabled = true;
				((Control)CCDBusStopRepeatedMessagesButton).Enabled = true;
			}
			else
			{
				((Control)CCDBusTxMessageRemoveItemButton).Enabled = false;
				((Control)CCDBusTxMessageClearListButton).Enabled = false;
				((Control)CCDBusSendMessagesButton).Enabled = false;
				((Control)CCDBusStopRepeatedMessagesButton).Enabled = false;
			}
		}
	}

	private void CCDBusTxMessageClearListButton_Click(object sender, EventArgs e)
	{
		CCDBusTxMessagesListBox.Items.Clear();
		((Control)CCDBusTxMessageRemoveItemButton).Enabled = false;
		((Control)CCDBusTxMessageClearListButton).Enabled = false;
		((Control)CCDBusSendMessagesButton).Enabled = false;
		((Control)CCDBusStopRepeatedMessagesButton).Enabled = false;
	}

	private void CCDBusSendMessagesButton_Click(object sender, EventArgs e)
	{
		if (((Control)DebugRandomCCDBusMessagesButton).Text == "Stop random messages")
		{
			DebugRandomCCDBusMessagesButton_Click(this, EventArgs.Empty);
		}
		if (!CCDBusTxMessageRepeatIntervalCheckBox.Checked)
		{
			if (CCDBusTxMessagesListBox.Items.Count == 1)
			{
				byte[] array = Util.HexStringToByte(CCDBusTxMessagesListBox.Items[0].ToString());
				Packet packet = new Packet();
				packet.Bus = 1;
				packet.Command = 6;
				packet.Mode = 2;
				packet.Payload = array;
				Util.UpdateTextBox(USBTextBox, "[<-TX] Send a CCD-bus message once:", PacketHelper.Serialize(packet));
				if (array.Length != 0)
				{
					Util.UpdateTextBox(USBTextBox, "[INFO] CCD-bus message Tx list:" + Environment.NewLine + "       " + Util.ByteToHexStringSimple(array));
				}
				SerialService.WritePacket(packet);
				return;
			}
			if (!int.TryParse(((Control)CCDBusTxMessageRepeatIntervalTextBox).Text, out var result) || result == 0)
			{
				result = 50;
				((Control)CCDBusTxMessageRepeatIntervalTextBox).Text = "50";
			}
			byte[] collection = new byte[2]
			{
				(byte)((uint)(result >> 8) & 0xFFu),
				(byte)((uint)result & 0xFFu)
			};
			List<byte[]> list = new List<byte[]>();
			List<byte> list2 = new List<byte>();
			byte b = (byte)CCDBusTxMessagesListBox.Items.Count;
			for (int i = 0; i < b; i++)
			{
				list.Add(Util.HexStringToByte(CCDBusTxMessagesListBox.Items[i].ToString()));
			}
			list2.AddRange(collection);
			for (int j = 0; j < b; j++)
			{
				list2.Add((byte)list[j].Length);
				list2.AddRange(list[j]);
			}
			Packet packet2 = new Packet();
			packet2.Bus = 1;
			packet2.Command = 6;
			packet2.Mode = 3;
			packet2.Payload = list2.ToArray();
			Util.UpdateTextBox(USBTextBox, "[<-TX] Send a CCD-bus message list once:", PacketHelper.Serialize(packet2));
			StringBuilder stringBuilder = new StringBuilder();
			foreach (byte[] item in list)
			{
				stringBuilder.Append("       " + Util.ByteToHexStringSimple(item) + Environment.NewLine);
			}
			stringBuilder.Replace(Environment.NewLine, string.Empty, stringBuilder.Length - 2, 2);
			if (list.Count > 0)
			{
				Util.UpdateTextBox(USBTextBox, "[INFO] CCD-bus message Tx list:" + Environment.NewLine + stringBuilder.ToString());
			}
			SerialService.WritePacket(packet2);
			return;
		}
		if (!int.TryParse(((Control)CCDBusTxMessageRepeatIntervalTextBox).Text, out var result2) || result2 == 0)
		{
			result2 = 50;
			((Control)CCDBusTxMessageRepeatIntervalTextBox).Text = "50";
		}
		byte[] collection2 = new byte[2]
		{
			(byte)((uint)(result2 >> 8) & 0xFFu),
			(byte)((uint)result2 & 0xFFu)
		};
		List<byte> list3 = new List<byte>();
		list3.AddRange(collection2);
		List<byte[]> list4 = new List<byte[]>();
		byte b2 = (byte)CCDBusTxMessagesListBox.Items.Count;
		for (int k = 0; k < b2; k++)
		{
			list4.Add(Util.HexStringToByte(CCDBusTxMessagesListBox.Items[k].ToString()));
		}
		for (int l = 0; l < b2; l++)
		{
			list3.Add((byte)list4[l].Length);
			list3.AddRange(list4[l]);
		}
		Packet packet3 = new Packet();
		packet3.Bus = 1;
		packet3.Command = 6;
		packet3.Mode = 4;
		packet3.Payload = list3.ToArray();
		Util.UpdateTextBox(USBTextBox, "[<-TX] Send repeated CCD-bus message list:", PacketHelper.Serialize(packet3));
		StringBuilder stringBuilder2 = new StringBuilder();
		foreach (byte[] item2 in list4)
		{
			stringBuilder2.Append("       " + Util.ByteToHexStringSimple(item2) + Environment.NewLine);
		}
		stringBuilder2.Replace(Environment.NewLine, string.Empty, stringBuilder2.Length - 2, 2);
		if (list4.Count > 0)
		{
			Util.UpdateTextBox(USBTextBox, "[INFO] CCD-bus message Tx list:" + Environment.NewLine + stringBuilder2.ToString());
		}
		SerialService.WritePacket(packet3);
	}

	private void CCDBusStopRepeatedMessagesButton_Click(object sender, EventArgs e)
	{
		Packet packet = new Packet();
		packet.Bus = 1;
		packet.Command = 6;
		packet.Mode = 1;
		packet.Payload = null;
		Util.UpdateTextBox(USBTextBox, "[<-TX] Stop repeated Tx on CCD-bus:", PacketHelper.Serialize(packet));
		SerialService.WritePacket(packet);
	}

	private void DebugRandomCCDBusMessagesButton_Click(object sender, EventArgs e)
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Invalid comparison between Unknown and I4
		if (((Control)DebugRandomCCDBusMessagesButton).Text == "Send random messages")
		{
			if ((int)MessageBox.Show("Use this debug mode with caution!" + Environment.NewLine + "The random generated messages may confuse modules on a live CCD-bus network.", "Warning", (MessageBoxButtons)1, (MessageBoxIcon)48, (MessageBoxDefaultButton)256) == 1)
			{
				if (!int.TryParse(((Control)CCDBusRandomMessageIntervalMinTextBox).Text, out var result) || result == 0)
				{
					result = 200;
					((Control)CCDBusRandomMessageIntervalMinTextBox).Text = "200";
				}
				if (!int.TryParse(((Control)CCDBusRandomMessageIntervalMaxTextBox).Text, out var result2) || result2 == 0)
				{
					result2 = 1000;
					((Control)CCDBusRandomMessageIntervalMaxTextBox).Text = "1000";
				}
				byte b = (byte)((uint)(result >> 8) & 0xFFu);
				byte b2 = (byte)((uint)result & 0xFFu);
				byte b3 = (byte)((uint)(result2 >> 8) & 0xFFu);
				byte b4 = (byte)((uint)result2 & 0xFFu);
				Packet packet = new Packet();
				packet.Bus = 0;
				packet.Command = 14;
				packet.Mode = 1;
				packet.Payload = new byte[5] { 1, b, b2, b3, b4 };
				Util.UpdateTextBox(USBTextBox, "[<-TX] Send random CCD-bus messages:", PacketHelper.Serialize(packet));
				((Control)DebugRandomCCDBusMessagesButton).Text = "Stop random messages";
				SerialService.WritePacket(packet);
			}
		}
		else if (((Control)DebugRandomCCDBusMessagesButton).Text == "Stop random messages")
		{
			Packet packet2 = new Packet();
			packet2.Bus = 0;
			packet2.Command = 14;
			packet2.Mode = 1;
			packet2.Payload = new byte[5];
			Util.UpdateTextBox(USBTextBox, "[<-TX] Stop random CCD-bus messages:", PacketHelper.Serialize(packet2));
			((Control)DebugRandomCCDBusMessagesButton).Text = "Send random messages";
			SerialService.WritePacket(packet2);
		}
	}

	private void MeasureCCDBusVoltagesButton_Click(object sender, EventArgs e)
	{
		Packet packet = new Packet();
		packet.Bus = 0;
		packet.Command = 4;
		packet.Mode = 5;
		packet.Payload = null;
		Util.UpdateTextBox(USBTextBox, "[<-TX] Measure CCD-bus voltages request:", PacketHelper.Serialize(packet));
		SerialService.WritePacket(packet);
	}

	private void CCDBusTxMessagesListBox_DoubleClick(object sender, EventArgs e)
	{
		if (CCDBusTxMessagesListBox.Items.Count > 0 && ((ListControl)CCDBusTxMessagesListBox).SelectedIndex > -1)
		{
			string text = CCDBusTxMessagesListBox.SelectedItem.ToString();
			((Control)CCDBusTxMessageComboBox).Text = text;
			((Control)CCDBusTxMessageComboBox).Focus();
			CCDBusTxMessageComboBox.SelectionStart = ((Control)CCDBusTxMessageComboBox).Text.Length;
			((Control)CCDBusTxMessageAddButton).Text = "Edit";
		}
	}

	private void CCDBusTxMessageRepeatIntervalCheckBox_CheckedChanged(object sender, EventArgs e)
	{
		if (CCDBusTxMessageRepeatIntervalCheckBox.Checked)
		{
			((Control)CCDBusTxMessageRepeatIntervalTextBox).Enabled = true;
			((Control)MillisecondsLabel03).Enabled = true;
		}
		else
		{
			((Control)CCDBusTxMessageRepeatIntervalTextBox).Enabled = false;
			((Control)MillisecondsLabel03).Enabled = false;
		}
	}

	private void CCDBusSettingsCheckBox_CheckedChanged(object sender, EventArgs e)
	{
		byte value = 1;
		if (CCDBusTransceiverOnOffCheckBox.Checked)
		{
			value = Util.SetBit(value, 7);
			((Control)CCDBusTransceiverOnOffCheckBox).Text = "CCD-bus transceiver ON";
			CCD.UpdateHeader();
		}
		else
		{
			value = Util.ClearBit(value, 7);
			((Control)CCDBusTransceiverOnOffCheckBox).Text = "CCD-bus transceiver OFF";
			CCD.UpdateHeader("disabled");
		}
		if (CCDBusTerminationBiasOnOffCheckBox.Checked)
		{
			value = Util.SetBit(value, 6);
			((Control)CCDBusTerminationBiasOnOffCheckBox).Text = "CCD-bus termination / bias ON";
		}
		else
		{
			value = Util.ClearBit(value, 6);
			((Control)CCDBusTerminationBiasOnOffCheckBox).Text = "CCD-bus termination / bias OFF";
		}
		Packet packet = new Packet();
		packet.Bus = 0;
		packet.Command = 3;
		packet.Mode = 2;
		packet.Payload = new byte[1] { value };
		Util.UpdateTextBox(USBTextBox, "[<-TX] Change CCD-bus settings:", PacketHelper.Serialize(packet));
		SerialService.WritePacket(packet);
	}

	private void CCDBusRandomMessageIntervalMinTextBox_KeyPress(object sender, KeyPressEventArgs e)
	{
		if (e.KeyChar == '\r')
		{
			e.Handled = true;
			DebugRandomCCDBusMessagesButton_Click(this, EventArgs.Empty);
		}
	}

	private void CCDBusRandomMessageIntervalMaxTextBox_KeyPress(object sender, KeyPressEventArgs e)
	{
		if (e.KeyChar == '\r')
		{
			e.Handled = true;
			DebugRandomCCDBusMessagesButton_Click(this, EventArgs.Empty);
		}
	}

	private void CCDBusTxMessageComboBox_KeyPress(object sender, KeyPressEventArgs e)
	{
		if (e.KeyChar != '\r')
		{
			return;
		}
		e.Handled = true;
		if (((Control)CCDBusTxMessageComboBox).Text.Length < 2)
		{
			return;
		}
		if (((Control)CCDBusTxMessageComboBox).Text.Contains(","))
		{
			CCDBusTxMessageAddButton_Click(this, EventArgs.Empty);
			if (!CCDBusTxMessageComboBox.Items.Contains((object)((Control)CCDBusTxMessageComboBox).Text))
			{
				CCDBusTxMessageComboBox.Items.Add((object)((Control)CCDBusTxMessageComboBox).Text);
			}
			((Control)CCDBusTxMessageComboBox).Text = string.Empty;
			return;
		}
		byte[] array = Util.HexStringToByte(((Control)CCDBusTxMessageComboBox).Text);
		if (((Control)CCDBusTxMessageAddButton).Text != "Edit")
		{
			Packet packet = new Packet();
			packet.Bus = 1;
			packet.Command = 6;
			packet.Mode = 2;
			packet.Payload = array;
			Util.UpdateTextBox(USBTextBox, "[<-TX] Send a CCD-bus message once:", PacketHelper.Serialize(packet));
			if (array.Length != 0)
			{
				Util.UpdateTextBox(USBTextBox, "[INFO] CCD-bus message Tx list:" + Environment.NewLine + "       " + Util.ByteToHexStringSimple(array));
			}
			SerialService.WritePacket(packet);
		}
		else
		{
			CCDBusTxMessageAddButton_Click(this, EventArgs.Empty);
		}
		if (!CCDBusTxMessageComboBox.Items.Contains((object)((Control)CCDBusTxMessageComboBox).Text))
		{
			CCDBusTxMessageComboBox.Items.Add((object)((Control)CCDBusTxMessageComboBox).Text);
		}
	}

	private void SCIBusTxMessageAddButton_Click(object sender, EventArgs e)
	{
		string[] array = ((Control)SCIBusTxMessageComboBox).Text.Split(new char[1] { ',' });
		for (int i = 0; i < array.Length; i++)
		{
			byte[] array2 = Util.HexStringToByte(array[i]);
			if (array2.Length == 0)
			{
				continue;
			}
			byte b = array2[0];
			bool flag = false;
			int num = 0;
			for (int j = 0; j < SCIBusTxMessagesListBox.Items.Count; j++)
			{
				if (Util.HexStringToByte(SCIBusTxMessagesListBox.Items[j].ToString())[0] == b)
				{
					flag = true;
					num = j;
					break;
				}
			}
			string text = Util.ByteToHexString(array2, 0, array2.Length);
			if (!flag)
			{
				SCIBusTxMessagesListBox.Items.Add((object)text);
			}
			else if (SCIBusOverwriteDuplicateIDCheckBox.Checked && !SCIBusTxMessageRepeatIntervalCheckBox.Checked)
			{
				SCIBusTxMessagesListBox.Items.RemoveAt(num);
				SCIBusTxMessagesListBox.Items.Insert(num, (object)text);
			}
			else
			{
				SCIBusTxMessagesListBox.Items.Add((object)text);
			}
			if (SCIBusTxMessagesListBox.Items.Count > 0)
			{
				((Control)SCIBusTxMessageRemoveItemButton).Enabled = true;
				((Control)SCIBusTxMessageClearListButton).Enabled = true;
				((Control)SCIBusSendMessagesButton).Enabled = true;
				((Control)SCIBusStopRepeatedMessagesButton).Enabled = true;
			}
			else
			{
				((Control)SCIBusTxMessageRemoveItemButton).Enabled = false;
				((Control)SCIBusTxMessageClearListButton).Enabled = false;
				((Control)SCIBusSendMessagesButton).Enabled = false;
				((Control)SCIBusStopRepeatedMessagesButton).Enabled = false;
			}
			if (!SCIBusTxMessageComboBox.Items.Contains((object)((Control)SCIBusTxMessageComboBox).Text))
			{
				SCIBusTxMessageComboBox.Items.Add((object)((Control)SCIBusTxMessageComboBox).Text);
			}
			if (((Control)SCIBusTxMessageAddButton).Text == "Edit")
			{
				((Control)SCIBusTxMessageAddButton).Text = "Add";
			}
		}
		SCIBusTxMessagesListBox.TopIndex = SCIBusTxMessagesListBox.Items.Count - 1;
	}

	private void SCIBusTxMessageRemoveItemButton_Click(object sender, EventArgs e)
	{
		if (((ListControl)SCIBusTxMessagesListBox).SelectedIndex > -1)
		{
			for (int num = SCIBusTxMessagesListBox.SelectedIndices.Count - 1; num >= 0; num--)
			{
				int num2 = SCIBusTxMessagesListBox.SelectedIndices[num];
				SCIBusTxMessagesListBox.Items.RemoveAt(num2);
			}
			if (SCIBusTxMessagesListBox.Items.Count > 0)
			{
				((Control)SCIBusTxMessageRemoveItemButton).Enabled = true;
				((Control)SCIBusTxMessageClearListButton).Enabled = true;
				((Control)SCIBusSendMessagesButton).Enabled = true;
				((Control)SCIBusStopRepeatedMessagesButton).Enabled = true;
			}
			else
			{
				((Control)SCIBusTxMessageRemoveItemButton).Enabled = false;
				((Control)SCIBusTxMessageClearListButton).Enabled = false;
				((Control)SCIBusSendMessagesButton).Enabled = false;
				((Control)SCIBusStopRepeatedMessagesButton).Enabled = false;
			}
		}
	}

	private void SCIBusTxMessageClearListButton_Click(object sender, EventArgs e)
	{
		SCIBusTxMessagesListBox.Items.Clear();
		((Control)SCIBusTxMessageRemoveItemButton).Enabled = false;
		((Control)SCIBusTxMessageClearListButton).Enabled = false;
		((Control)SCIBusSendMessagesButton).Enabled = false;
		((Control)SCIBusStopRepeatedMessagesButton).Enabled = false;
	}

	private void SCIBusSendMessagesButton_Click(object sender, EventArgs e)
	{
		if (!SCIBusTxMessageRepeatIntervalCheckBox.Checked)
		{
			if (SCIBusTxMessagesListBox.Items.Count == 1)
			{
				byte[] array = Util.HexStringToByte(SCIBusTxMessagesListBox.Items[0].ToString());
				switch (((ListControl)SCIBusModuleComboBox).SelectedIndex)
				{
				case 0:
				{
					Packet packet2 = new Packet();
					packet2.Bus = 2;
					packet2.Command = 6;
					packet2.Mode = 2;
					packet2.Payload = array;
					Util.UpdateTextBox(USBTextBox, "[<-TX] Send an SCI-bus (PCM) message once::", PacketHelper.Serialize(packet2));
					if (array.Length != 0)
					{
						Util.UpdateTextBox(USBTextBox, "[INFO] SCI-bus (PCM) message Tx list:" + Environment.NewLine + "       " + Util.ByteToHexStringSimple(array));
					}
					SerialService.WritePacket(packet2);
					break;
				}
				case 1:
				{
					Packet packet = new Packet();
					packet.Bus = 3;
					packet.Command = 6;
					packet.Mode = 2;
					packet.Payload = array;
					Util.UpdateTextBox(USBTextBox, "[<-TX] Send an SCI-bus (TCM) message once:", PacketHelper.Serialize(packet));
					if (array.Length != 0)
					{
						Util.UpdateTextBox(USBTextBox, "[INFO] SCI-bus (TCM) message Tx list:" + Environment.NewLine + "       " + Util.ByteToHexStringSimple(array));
					}
					SerialService.WritePacket(packet);
					break;
				}
				}
				return;
			}
			if (!int.TryParse(((Control)SCIBusTxMessageRepeatIntervalTextBox).Text, out var result) || result == 0)
			{
				result = 50;
				((Control)SCIBusTxMessageRepeatIntervalTextBox).Text = "50";
			}
			byte[] collection = new byte[2]
			{
				(byte)((uint)(result >> 8) & 0xFFu),
				(byte)((uint)result & 0xFFu)
			};
			List<byte[]> list = new List<byte[]>();
			List<byte> list2 = new List<byte>();
			byte b = (byte)SCIBusTxMessagesListBox.Items.Count;
			for (int i = 0; i < b; i++)
			{
				list.Add(Util.HexStringToByte(SCIBusTxMessagesListBox.Items[i].ToString()));
			}
			list2.AddRange(collection);
			for (int j = 0; j < b; j++)
			{
				list2.Add((byte)list[j].Length);
				list2.AddRange(list[j]);
			}
			StringBuilder stringBuilder = new StringBuilder();
			switch (((ListControl)SCIBusModuleComboBox).SelectedIndex)
			{
			case 0:
			{
				Packet packet4 = new Packet();
				packet4.Bus = 2;
				packet4.Command = 6;
				packet4.Mode = 3;
				packet4.Payload = list2.ToArray();
				Util.UpdateTextBox(USBTextBox, "[<-TX] Send SCI-bus (PCM) message list once:", PacketHelper.Serialize(packet4));
				foreach (byte[] item in list)
				{
					stringBuilder.Append("       " + Util.ByteToHexStringSimple(item) + Environment.NewLine);
				}
				stringBuilder.Replace(Environment.NewLine, string.Empty, stringBuilder.Length - 2, 2);
				if (list.Count > 0)
				{
					Util.UpdateTextBox(USBTextBox, "[INFO] SCI-bus (PCM) message Tx list:" + Environment.NewLine + stringBuilder.ToString());
				}
				SerialService.WritePacket(packet4);
				break;
			}
			case 1:
			{
				Packet packet3 = new Packet();
				packet3.Bus = 3;
				packet3.Command = 6;
				packet3.Mode = 3;
				packet3.Payload = list2.ToArray();
				Util.UpdateTextBox(USBTextBox, "[<-TX] Send SCI-bus (TCM) message list once:", PacketHelper.Serialize(packet3));
				foreach (byte[] item2 in list)
				{
					stringBuilder.Append("       " + Util.ByteToHexStringSimple(item2) + Environment.NewLine);
				}
				stringBuilder.Replace(Environment.NewLine, string.Empty, stringBuilder.Length - 2, 2);
				if (list.Count > 0)
				{
					Util.UpdateTextBox(USBTextBox, "[INFO] SCI-bus (TCM) message Tx list:" + Environment.NewLine + stringBuilder.ToString());
				}
				SerialService.WritePacket(packet3);
				break;
			}
			}
			return;
		}
		if (!int.TryParse(((Control)SCIBusTxMessageRepeatIntervalTextBox).Text, out var result2) || result2 == 0)
		{
			result2 = 50;
			((Control)SCIBusTxMessageRepeatIntervalTextBox).Text = "50";
		}
		byte[] collection2 = new byte[2]
		{
			(byte)((uint)(result2 >> 8) & 0xFFu),
			(byte)((uint)result2 & 0xFFu)
		};
		List<byte[]> list3 = new List<byte[]>();
		List<byte> list4 = new List<byte>();
		byte b2 = (byte)SCIBusTxMessagesListBox.Items.Count;
		for (int k = 0; k < b2; k++)
		{
			list3.Add(Util.HexStringToByte(SCIBusTxMessagesListBox.Items[k].ToString()));
		}
		list4.AddRange(collection2);
		for (int l = 0; l < b2; l++)
		{
			list4.Add((byte)list3[l].Length);
			list4.AddRange(list3[l]);
		}
		StringBuilder stringBuilder2 = new StringBuilder();
		switch (((ListControl)SCIBusModuleComboBox).SelectedIndex)
		{
		case 0:
		{
			Packet packet6 = new Packet();
			packet6.Bus = 2;
			packet6.Command = 6;
			packet6.Mode = 4;
			packet6.Payload = list4.ToArray();
			Util.UpdateTextBox(USBTextBox, "[<-TX] Send repeated SCI-bus (PCM) message list:", PacketHelper.Serialize(packet6));
			foreach (byte[] item3 in list3)
			{
				stringBuilder2.Append("       " + Util.ByteToHexStringSimple(item3) + Environment.NewLine);
			}
			stringBuilder2.Replace(Environment.NewLine, string.Empty, stringBuilder2.Length - 2, 2);
			if (list3.Count > 0)
			{
				Util.UpdateTextBox(USBTextBox, "[INFO] SCI-bus (PCM) message Tx list:" + Environment.NewLine + stringBuilder2.ToString());
			}
			SerialService.WritePacket(packet6);
			break;
		}
		case 1:
		{
			Packet packet5 = new Packet();
			packet5.Bus = 3;
			packet5.Command = 6;
			packet5.Mode = 4;
			packet5.Payload = list4.ToArray();
			Util.UpdateTextBox(USBTextBox, "[<-TX] Send repeated SCI-bus (TCM) message list:", PacketHelper.Serialize(packet5));
			foreach (byte[] item4 in list3)
			{
				stringBuilder2.Append("       " + Util.ByteToHexStringSimple(item4) + Environment.NewLine);
			}
			stringBuilder2.Replace(Environment.NewLine, string.Empty, stringBuilder2.Length - 2, 2);
			if (list3.Count > 0)
			{
				Util.UpdateTextBox(USBTextBox, "[INFO] SCI-bus (TCM) message Tx list:" + Environment.NewLine + stringBuilder2.ToString());
			}
			SerialService.WritePacket(packet5);
			break;
		}
		}
	}

	private void SCIBusStopRepeatedMessagesButton_Click(object sender, EventArgs e)
	{
		Packet packet = new Packet();
		packet.Bus = 2;
		packet.Command = 6;
		packet.Mode = 1;
		packet.Payload = null;
		Util.UpdateTextBox(USBTextBox, "[<-TX] Stop repeated Tx on SCI-bus (PCM):", PacketHelper.Serialize(packet));
		SerialService.WritePacket(packet);
	}

	private void SCIBusModuleConfigSpeedApplyButton_Click(object sender, EventArgs e)
	{
		byte value = 0;
		switch (((ListControl)SCIBusModuleComboBox).SelectedIndex)
		{
		case 0:
			value = Util.ClearBit(value, 5);
			break;
		case 1:
			value = Util.SetBit(value, 5);
			break;
		}
		switch (((ListControl)SCIBusLogicComboBox).SelectedIndex)
		{
		case 0:
			value = Util.SetBit(value, 6);
			value = Util.SetBit(value, 3);
			break;
		case 1:
			value = Util.SetBit(value, 3);
			break;
		case 3:
			value = Util.SetBit(value, 4);
			break;
		}
		value = ((((ListControl)SCIBusOBDConfigurationComboBox).SelectedIndex != 0) ? Util.SetBit(value, 2) : Util.ClearBit(value, 2));
		switch (((ListControl)SCIBusSpeedComboBox).SelectedIndex)
		{
		case 0:
			value = Util.ClearBit(value, 7);
			value = Util.ClearBit(value, 1);
			value = Util.SetBit(value, 0);
			break;
		case 1:
			value = Util.SetBit(value, 7);
			value = Util.ClearBit(value, 1);
			value = Util.ClearBit(value, 0);
			break;
		case 2:
			value = Util.SetBit(value, 7);
			value = Util.ClearBit(value, 1);
			value = Util.SetBit(value, 0);
			break;
		case 3:
			value = Util.SetBit(value, 7);
			value = Util.SetBit(value, 1);
			value = Util.ClearBit(value, 0);
			break;
		case 4:
			value = Util.SetBit(value, 7);
			value = Util.SetBit(value, 1);
			value = Util.SetBit(value, 0);
			break;
		default:
			value = Util.SetBit(value, 7);
			value = Util.ClearBit(value, 1);
			value = Util.SetBit(value, 0);
			break;
		}
		Packet packet = new Packet();
		packet.Bus = 0;
		packet.Command = 3;
		packet.Mode = 3;
		packet.Payload = new byte[1] { value };
		Util.UpdateTextBox(USBTextBox, "[<-TX] Change SCI-bus settings:", PacketHelper.Serialize(packet));
		SerialService.WritePacket(packet);
	}

	private void SCIBusTxMessagesListBox_DoubleClick(object sender, EventArgs e)
	{
		if (SCIBusTxMessagesListBox.Items.Count > 0 && ((ListControl)SCIBusTxMessagesListBox).SelectedIndex > -1)
		{
			string text = SCIBusTxMessagesListBox.SelectedItem.ToString();
			((Control)SCIBusTxMessageComboBox).Text = text;
			((Control)SCIBusTxMessageComboBox).Focus();
			SCIBusTxMessageComboBox.SelectionStart = ((Control)SCIBusTxMessageComboBox).Text.Length;
			((Control)SCIBusTxMessageAddButton).Text = "Edit";
		}
	}

	private void SCIBusTxMessageRepeatIntervalCheckBox_CheckedChanged(object sender, EventArgs e)
	{
		if (SCIBusTxMessageRepeatIntervalCheckBox.Checked)
		{
			((Control)SCIBusTxMessageRepeatIntervalTextBox).Enabled = true;
			((Control)MillisecondsLabel05).Enabled = true;
		}
		else
		{
			((Control)SCIBusTxMessageRepeatIntervalTextBox).Enabled = false;
			((Control)MillisecondsLabel05).Enabled = false;
		}
	}

	private void SCIBusModuleComboBox_SelectedIndexChanged(object sender, EventArgs e)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Expected O, but got Unknown
		((Control)this).BeginInvoke((Delegate)(MethodInvoker)delegate
		{
			switch (((ListControl)SCIBusModuleComboBox).SelectedIndex)
			{
			case 0:
				if (PCM.state == "enabled")
				{
					if (PCM.speed == "976.5 baud")
					{
						((ListControl)SCIBusSpeedComboBox).SelectedIndex = 1;
					}
					else if (PCM.speed == "7812.5 baud")
					{
						((ListControl)SCIBusSpeedComboBox).SelectedIndex = 2;
					}
					else if (PCM.speed == "62500 baud")
					{
						((ListControl)SCIBusSpeedComboBox).SelectedIndex = 3;
					}
					else if (PCM.speed == "125000 baud")
					{
						((ListControl)SCIBusSpeedComboBox).SelectedIndex = 4;
					}
				}
				else if (PCM.state == "disabled")
				{
					((ListControl)SCIBusSpeedComboBox).SelectedIndex = 0;
				}
				if (PCM.logic == "non-inverted")
				{
					((ListControl)SCIBusLogicComboBox).SelectedIndex = 2;
				}
				else if (PCM.logic == "inverted")
				{
					((ListControl)SCIBusLogicComboBox).SelectedIndex = 1;
				}
				if (PCM.configuration == "A")
				{
					((ListControl)SCIBusOBDConfigurationComboBox).SelectedIndex = 0;
				}
				else if (PCM.configuration == "B")
				{
					((ListControl)SCIBusOBDConfigurationComboBox).SelectedIndex = 1;
				}
				PCMSelected = true;
				TCMSelected = false;
				break;
			case 1:
				if (TCM.state == "enabled")
				{
					if (TCM.speed == "976.5 baud")
					{
						((ListControl)SCIBusSpeedComboBox).SelectedIndex = 1;
					}
					else if (TCM.speed == "7812.5 baud")
					{
						((ListControl)SCIBusSpeedComboBox).SelectedIndex = 2;
					}
					else if (TCM.speed == "62500 baud")
					{
						((ListControl)SCIBusSpeedComboBox).SelectedIndex = 3;
					}
					else if (TCM.speed == "125000 baud")
					{
						((ListControl)SCIBusSpeedComboBox).SelectedIndex = 4;
					}
				}
				else if (TCM.state == "disabled")
				{
					((ListControl)SCIBusSpeedComboBox).SelectedIndex = 0;
				}
				if (TCM.logic == "non-inverted")
				{
					((ListControl)SCIBusLogicComboBox).SelectedIndex = 2;
				}
				else if (TCM.logic == "inverted")
				{
					((ListControl)SCIBusLogicComboBox).SelectedIndex = 1;
				}
				if (TCM.configuration == "A")
				{
					((ListControl)SCIBusOBDConfigurationComboBox).SelectedIndex = 0;
				}
				else if (TCM.configuration == "B")
				{
					((ListControl)SCIBusOBDConfigurationComboBox).SelectedIndex = 1;
				}
				PCMSelected = false;
				TCMSelected = true;
				break;
			}
		});
	}

	private void SCIBusTxMessageComboBox_KeyPress(object sender, KeyPressEventArgs e)
	{
		if (e.KeyChar != '\r')
		{
			return;
		}
		e.Handled = true;
		if (((Control)SCIBusTxMessageComboBox).Text.Length < 2)
		{
			return;
		}
		if (((Control)SCIBusTxMessageComboBox).Text.Contains(","))
		{
			SCIBusTxMessageAddButton_Click(this, EventArgs.Empty);
			if (!SCIBusTxMessageComboBox.Items.Contains((object)((Control)SCIBusTxMessageComboBox).Text))
			{
				SCIBusTxMessageComboBox.Items.Add((object)((Control)SCIBusTxMessageComboBox).Text);
			}
			((Control)SCIBusTxMessageComboBox).Text = string.Empty;
			return;
		}
		byte[] array = Util.HexStringToByte(((Control)SCIBusTxMessageComboBox).Text);
		if (((Control)SCIBusTxMessageAddButton).Text != "Edit")
		{
			switch (((ListControl)SCIBusModuleComboBox).SelectedIndex)
			{
			case 0:
			{
				Packet packet2 = new Packet();
				packet2.Bus = 2;
				packet2.Command = 6;
				packet2.Mode = 2;
				packet2.Payload = array;
				Util.UpdateTextBox(USBTextBox, "[<-TX] Send an SCI-bus (PCM) message once:", PacketHelper.Serialize(packet2));
				if (array.Length != 0)
				{
					Util.UpdateTextBox(USBTextBox, "[INFO] SCI-bus (PCM) message Tx list:" + Environment.NewLine + "       " + Util.ByteToHexStringSimple(array));
				}
				SerialService.WritePacket(packet2);
				break;
			}
			case 1:
			{
				Packet packet = new Packet();
				packet.Bus = 3;
				packet.Command = 6;
				packet.Mode = 2;
				packet.Payload = array;
				Util.UpdateTextBox(USBTextBox, "[<-TX] Send an SCI-bus (TCM) message once:", PacketHelper.Serialize(packet));
				if (array.Length != 0)
				{
					Util.UpdateTextBox(USBTextBox, "[INFO] SCI-bus (TCM) message Tx list:" + Environment.NewLine + "       " + Util.ByteToHexStringSimple(array));
				}
				SerialService.WritePacket(packet);
				break;
			}
			}
		}
		else
		{
			SCIBusTxMessageAddButton_Click(this, EventArgs.Empty);
		}
		if (!SCIBusTxMessageComboBox.Items.Contains((object)((Control)SCIBusTxMessageComboBox).Text))
		{
			SCIBusTxMessageComboBox.Items.Add((object)((Control)SCIBusTxMessageComboBox).Text);
		}
	}

	private void PCIBusTxMessageAddButton_Click(object sender, EventArgs e)
	{
		string[] array = ((Control)PCIBusTxMessageComboBox).Text.Split(new char[1] { ',' });
		for (int i = 0; i < array.Length; i++)
		{
			byte[] array2 = Util.HexStringToByte(array[i]);
			if (array2.Length == 0)
			{
				continue;
			}
			byte b = array2[0];
			bool flag = false;
			int num = 0;
			for (int j = 0; j < PCIBusTxMessagesListBox.Items.Count; j++)
			{
				if (Util.HexStringToByte(PCIBusTxMessagesListBox.Items[j].ToString())[0] == b)
				{
					flag = true;
					num = j;
					break;
				}
			}
			string text = Util.ByteToHexString(array2, 0, array2.Length);
			if (!flag)
			{
				PCIBusTxMessagesListBox.Items.Add((object)text);
			}
			else if (PCIBusOverwriteDuplicateIDCheckBox.Checked && !PCIBusTxMessageRepeatIntervalCheckBox.Checked)
			{
				PCIBusTxMessagesListBox.Items.RemoveAt(num);
				PCIBusTxMessagesListBox.Items.Insert(num, (object)text);
			}
			else
			{
				PCIBusTxMessagesListBox.Items.Add((object)text);
			}
			if (PCIBusTxMessagesListBox.Items.Count > 0)
			{
				((Control)PCIBusTxMessageRemoveItemButton).Enabled = true;
				((Control)PCIBusTxMessageClearListButton).Enabled = true;
				((Control)PCIBusSendMessagesButton).Enabled = true;
				((Control)PCIBusStopRepeatedMessagesButton).Enabled = true;
			}
			else
			{
				((Control)PCIBusTxMessageRemoveItemButton).Enabled = false;
				((Control)PCIBusTxMessageClearListButton).Enabled = false;
				((Control)PCIBusSendMessagesButton).Enabled = false;
				((Control)PCIBusStopRepeatedMessagesButton).Enabled = false;
			}
			if (!PCIBusTxMessageComboBox.Items.Contains((object)((Control)PCIBusTxMessageComboBox).Text))
			{
				PCIBusTxMessageComboBox.Items.Add((object)((Control)PCIBusTxMessageComboBox).Text);
			}
			if (((Control)PCIBusTxMessageAddButton).Text == "Edit")
			{
				((Control)PCIBusTxMessageAddButton).Text = "Add";
			}
		}
		CCDBusTxMessagesListBox.TopIndex = CCDBusTxMessagesListBox.Items.Count - 1;
	}

	private void PCIBusTxMessageRemoveItemButton_Click(object sender, EventArgs e)
	{
		if (((ListControl)PCIBusTxMessagesListBox).SelectedIndex > -1)
		{
			for (int num = PCIBusTxMessagesListBox.SelectedIndices.Count - 1; num >= 0; num--)
			{
				int num2 = PCIBusTxMessagesListBox.SelectedIndices[num];
				PCIBusTxMessagesListBox.Items.RemoveAt(num2);
			}
			if (PCIBusTxMessagesListBox.Items.Count > 0)
			{
				((Control)PCIBusTxMessageRemoveItemButton).Enabled = true;
				((Control)PCIBusTxMessageClearListButton).Enabled = true;
				((Control)PCIBusSendMessagesButton).Enabled = true;
				((Control)PCIBusStopRepeatedMessagesButton).Enabled = true;
			}
			else
			{
				((Control)PCIBusTxMessageRemoveItemButton).Enabled = false;
				((Control)PCIBusTxMessageClearListButton).Enabled = false;
				((Control)PCIBusSendMessagesButton).Enabled = false;
				((Control)PCIBusStopRepeatedMessagesButton).Enabled = false;
			}
		}
	}

	private void PCIBusTxMessageClearListButton_Click(object sender, EventArgs e)
	{
		PCIBusTxMessagesListBox.Items.Clear();
		((Control)PCIBusTxMessageRemoveItemButton).Enabled = false;
		((Control)PCIBusTxMessageClearListButton).Enabled = false;
		((Control)PCIBusSendMessagesButton).Enabled = false;
		((Control)PCIBusStopRepeatedMessagesButton).Enabled = false;
	}

	private void PCIBusSendMessagesButton_Click(object sender, EventArgs e)
	{
		if (!PCIBusTxMessageRepeatIntervalCheckBox.Checked)
		{
			if (PCIBusTxMessagesListBox.Items.Count == 1)
			{
				byte[] array = Util.HexStringToByte(PCIBusTxMessagesListBox.Items[0].ToString());
				Packet packet = new Packet();
				packet.Bus = 4;
				packet.Command = 6;
				packet.Mode = 2;
				packet.Payload = array;
				Util.UpdateTextBox(USBTextBox, "[<-TX] Send a PCI-bus message once:", PacketHelper.Serialize(packet));
				if (array.Length != 0)
				{
					Util.UpdateTextBox(USBTextBox, "[INFO] PCI-bus message Tx list:" + Environment.NewLine + "       " + Util.ByteToHexStringSimple(array));
				}
				SerialService.WritePacket(packet);
				return;
			}
			if (!int.TryParse(((Control)PCIBusTxMessageRepeatIntervalTextBox).Text, out var result) || result == 0)
			{
				result = 50;
				((Control)PCIBusTxMessageRepeatIntervalTextBox).Text = "50";
			}
			byte[] collection = new byte[2]
			{
				(byte)((uint)(result >> 8) & 0xFFu),
				(byte)((uint)result & 0xFFu)
			};
			List<byte[]> list = new List<byte[]>();
			List<byte> list2 = new List<byte>();
			byte b = (byte)PCIBusTxMessagesListBox.Items.Count;
			for (int i = 0; i < b; i++)
			{
				list.Add(Util.HexStringToByte(PCIBusTxMessagesListBox.Items[i].ToString()));
			}
			list2.AddRange(collection);
			for (int j = 0; j < b; j++)
			{
				list2.Add((byte)list[j].Length);
				list2.AddRange(list[j]);
			}
			Packet packet2 = new Packet();
			packet2.Bus = 4;
			packet2.Command = 6;
			packet2.Mode = 3;
			packet2.Payload = list2.ToArray();
			Util.UpdateTextBox(USBTextBox, "[<-TX] Send PCI-bus message list once:", PacketHelper.Serialize(packet2));
			StringBuilder stringBuilder = new StringBuilder();
			foreach (byte[] item in list)
			{
				stringBuilder.Append("       " + Util.ByteToHexStringSimple(item) + Environment.NewLine);
			}
			stringBuilder.Replace(Environment.NewLine, string.Empty, stringBuilder.Length - 2, 2);
			if (list.Count > 0)
			{
				Util.UpdateTextBox(USBTextBox, "[INFO] PCI-bus message Tx list:" + Environment.NewLine + stringBuilder.ToString());
			}
			SerialService.WritePacket(packet2);
			return;
		}
		if (!int.TryParse(((Control)PCIBusTxMessageRepeatIntervalTextBox).Text, out var result2) || result2 == 0)
		{
			result2 = 50;
			((Control)PCIBusTxMessageRepeatIntervalTextBox).Text = "50";
		}
		byte[] collection2 = new byte[2]
		{
			(byte)((uint)(result2 >> 8) & 0xFFu),
			(byte)((uint)result2 & 0xFFu)
		};
		List<byte[]> list3 = new List<byte[]>();
		List<byte> list4 = new List<byte>();
		byte b2 = (byte)PCIBusTxMessagesListBox.Items.Count;
		for (int k = 0; k < b2; k++)
		{
			list3.Add(Util.HexStringToByte(PCIBusTxMessagesListBox.Items[k].ToString()));
		}
		list4.AddRange(collection2);
		for (int l = 0; l < b2; l++)
		{
			list4.Add((byte)list3[l].Length);
			list4.AddRange(list3[l]);
		}
		Packet packet3 = new Packet();
		packet3.Bus = 4;
		packet3.Command = 6;
		packet3.Mode = 4;
		packet3.Payload = list4.ToArray();
		Util.UpdateTextBox(USBTextBox, "[<-TX] Send repeated PCI-bus message list:", PacketHelper.Serialize(packet3));
		StringBuilder stringBuilder2 = new StringBuilder();
		foreach (byte[] item2 in list3)
		{
			stringBuilder2.Append("       " + Util.ByteToHexStringSimple(item2) + Environment.NewLine);
		}
		stringBuilder2.Replace(Environment.NewLine, string.Empty, stringBuilder2.Length - 2, 2);
		if (list3.Count > 0)
		{
			Util.UpdateTextBox(USBTextBox, "[INFO] PCI-bus message Tx list:" + Environment.NewLine + stringBuilder2.ToString());
		}
		SerialService.WritePacket(packet3);
	}

	private void PCIBusStopRepeatedMessagesButton_Click(object sender, EventArgs e)
	{
		Packet packet = new Packet();
		packet.Bus = 4;
		packet.Command = 6;
		packet.Mode = 1;
		packet.Payload = null;
		Util.UpdateTextBox(USBTextBox, "[<-TX] Stop repeated Tx on PCI-bus:", PacketHelper.Serialize(packet));
		SerialService.WritePacket(packet);
	}

	private void PCIBusTxMessagesListBox_DoubleClick(object sender, EventArgs e)
	{
		if (PCIBusTxMessagesListBox.Items.Count > 0 && ((ListControl)PCIBusTxMessagesListBox).SelectedIndex > -1)
		{
			string text = PCIBusTxMessagesListBox.SelectedItem.ToString();
			((Control)PCIBusTxMessageComboBox).Text = text;
			((Control)PCIBusTxMessageComboBox).Focus();
			PCIBusTxMessageComboBox.SelectionStart = ((Control)PCIBusTxMessageComboBox).Text.Length;
			((Control)PCIBusTxMessageAddButton).Text = "Edit";
		}
	}

	private void PCIBusTxMessageRepeatIntervalCheckBox_CheckedChanged(object sender, EventArgs e)
	{
		if (PCIBusTxMessageRepeatIntervalCheckBox.Checked)
		{
			((Control)PCIBusTxMessageRepeatIntervalTextBox).Enabled = true;
			((Control)MillisecondsLabel06).Enabled = true;
		}
		else
		{
			((Control)PCIBusTxMessageRepeatIntervalTextBox).Enabled = false;
			((Control)MillisecondsLabel06).Enabled = false;
		}
	}

	private void PCIBusSettingsCheckBox_CheckedChanged(object sender, EventArgs e)
	{
		byte value = 64;
		if (PCIBusTransceiverOnOffCheckBox.Checked)
		{
			value = Util.SetBit(value, 7);
			((Control)PCIBusTransceiverOnOffCheckBox).Text = "PCI-bus transceiver ON";
			PCI.UpdateHeader();
		}
		else
		{
			value = Util.ClearBit(value, 7);
			((Control)PCIBusTransceiverOnOffCheckBox).Text = "PCI-bus transceiver OFF";
			PCI.UpdateHeader("disabled");
		}
		Packet packet = new Packet();
		packet.Bus = 0;
		packet.Command = 3;
		packet.Mode = 6;
		packet.Payload = new byte[1] { value };
		Util.UpdateTextBox(USBTextBox, "[<-TX] Change PCI-bus settings:", PacketHelper.Serialize(packet));
		SerialService.WritePacket(packet);
	}

	private void PCIBusTxMessageComboBox_KeyPress(object sender, KeyPressEventArgs e)
	{
		if (e.KeyChar != '\r')
		{
			return;
		}
		e.Handled = true;
		if (((Control)PCIBusTxMessageComboBox).Text.Length < 2)
		{
			return;
		}
		if (((Control)PCIBusTxMessageComboBox).Text.Contains(","))
		{
			PCIBusTxMessageAddButton_Click(this, EventArgs.Empty);
			if (!PCIBusTxMessageComboBox.Items.Contains((object)((Control)PCIBusTxMessageComboBox).Text))
			{
				PCIBusTxMessageComboBox.Items.Add((object)((Control)PCIBusTxMessageComboBox).Text);
			}
			((Control)PCIBusTxMessageComboBox).Text = string.Empty;
			return;
		}
		byte[] array = Util.HexStringToByte(((Control)PCIBusTxMessageComboBox).Text);
		if (((Control)PCIBusTxMessageAddButton).Text != "Edit")
		{
			Packet packet = new Packet();
			packet.Bus = 4;
			packet.Command = 6;
			packet.Mode = 2;
			packet.Payload = array;
			Util.UpdateTextBox(USBTextBox, "[<-TX] Send a PCI-bus message once:", PacketHelper.Serialize(packet));
			if (array.Length != 0)
			{
				Util.UpdateTextBox(USBTextBox, "[INFO] PCI-bus message Tx list:" + Environment.NewLine + "       " + Util.ByteToHexStringSimple(array));
			}
			SerialService.WritePacket(packet);
		}
		else
		{
			PCIBusTxMessageAddButton_Click(this, EventArgs.Empty);
		}
		if (!PCIBusTxMessageComboBox.Items.Contains((object)((Control)PCIBusTxMessageComboBox).Text))
		{
			PCIBusTxMessageComboBox.Items.Add((object)((Control)PCIBusTxMessageComboBox).Text);
		}
	}

	private void DiagnosticsRefreshButton_Click(object sender, EventArgs e)
	{
		switch (((Control)DiagnosticsTabControl.SelectedTab).Name)
		{
		case "CCDBusDiagnosticsTabPage":
		{
			((ListBox)CCDBusDiagnosticsListBox).Items.Clear();
			ObjectCollection items4 = ((ListBox)CCDBusDiagnosticsListBox).Items;
			object[] array = CCD.Diagnostics.Table.ToArray();
			items4.AddRange(array);
			break;
		}
		case "PCIBusDiagnosticsTabPage":
		{
			((ListBox)PCIBusDiagnosticsListBox).Items.Clear();
			ObjectCollection items3 = ((ListBox)PCIBusDiagnosticsListBox).Items;
			object[] array = PCI.Diagnostics.Table.ToArray();
			items3.AddRange(array);
			break;
		}
		case "SCIBusPCMDiagnosticsTabPage":
		{
			((ListBox)SCIBusPCMDiagnosticsListBox).Items.Clear();
			ObjectCollection items2 = ((ListBox)SCIBusPCMDiagnosticsListBox).Items;
			object[] array = PCM.Diagnostics.Table.ToArray();
			items2.AddRange(array);
			break;
		}
		case "SCIBusTCMDiagnosticsTabPage":
		{
			((ListBox)SCIBusTCMDiagnosticsListBox).Items.Clear();
			ObjectCollection items = ((ListBox)SCIBusTCMDiagnosticsListBox).Items;
			object[] array = TCM.Diagnostics.Table.ToArray();
			items.AddRange(array);
			break;
		}
		}
	}

	private void DiagnosticsResetViewButton_Click(object sender, EventArgs e)
	{
		switch (((Control)DiagnosticsTabControl.SelectedTab).Name)
		{
		case "CCDBusDiagnosticsTabPage":
		{
			CCD.Diagnostics.IDByteList.Clear();
			CCD.Diagnostics.UniqueIDByteList.Clear();
			CCD.Diagnostics.B2F2IDByteList.Clear();
			CCDTableBuffer.Clear();
			CCDTableBufferLocation.Clear();
			CCDTableRowCountHistory.Clear();
			CCD.Diagnostics.LastUpdatedLine = 1;
			CCD.Diagnostics.InitCCDTable();
			((ListBox)CCDBusDiagnosticsListBox).Items.Clear();
			ObjectCollection items4 = ((ListBox)CCDBusDiagnosticsListBox).Items;
			object[] array = CCD.Diagnostics.Table.ToArray();
			items4.AddRange(array);
			break;
		}
		case "PCIBusDiagnosticsTabPage":
		{
			PCI.Diagnostics.IDByteList.Clear();
			PCI.Diagnostics.UniqueIDByteList.Clear();
			PCI.Diagnostics.IDByte2426List.Clear();
			PCITableBuffer.Clear();
			PCITableBufferLocation.Clear();
			PCITableRowCountHistory.Clear();
			PCI.Diagnostics.LastUpdatedLine = 1;
			PCI.Diagnostics.InitPCITable();
			((ListBox)PCIBusDiagnosticsListBox).Items.Clear();
			ObjectCollection items3 = ((ListBox)PCIBusDiagnosticsListBox).Items;
			object[] array = PCI.Diagnostics.Table.ToArray();
			items3.AddRange(array);
			break;
		}
		case "SCIBusPCMDiagnosticsTabPage":
		{
			PCM.Diagnostics.IDByteList.Clear();
			PCM.Diagnostics.UniqueIDByteList.Clear();
			PCMTableBuffer.Clear();
			PCMTableBufferLocation.Clear();
			PCMTableRowCountHistory.Clear();
			PCM.Diagnostics.LastUpdatedLine = 1;
			PCM.Diagnostics.InitSCIPCMTable();
			PCM.Diagnostics.InitRAMDumpTable();
			PCM.Diagnostics.RAMDumpTableVisible = false;
			PCM.Diagnostics.RAMTableAddress = 0;
			((ListBox)SCIBusPCMDiagnosticsListBox).Items.Clear();
			ObjectCollection items2 = ((ListBox)SCIBusPCMDiagnosticsListBox).Items;
			object[] array = PCM.Diagnostics.Table.ToArray();
			items2.AddRange(array);
			break;
		}
		case "SCIBusTCMDiagnosticsTabPage":
		{
			TCM.Diagnostics.IDByteList.Clear();
			TCM.Diagnostics.UniqueIDByteList.Clear();
			TCMTableBuffer.Clear();
			TCMTableBufferLocation.Clear();
			TCMTableRowCountHistory.Clear();
			TCM.Diagnostics.LastUpdatedLine = 1;
			TCM.Diagnostics.InitSCITCMTable();
			((ListBox)SCIBusTCMDiagnosticsListBox).Items.Clear();
			ObjectCollection items = ((ListBox)SCIBusTCMDiagnosticsListBox).Items;
			object[] array = TCM.Diagnostics.Table.ToArray();
			items.AddRange(array);
			break;
		}
		}
	}

	private void DiagnosticsCopyToClipboardButton_Click(object sender, EventArgs e)
	{
		Clipboard.Clear();
		switch (((Control)DiagnosticsTabControl.SelectedTab).Name)
		{
		case "CCDBusDiagnosticsTabPage":
			Clipboard.SetText(string.Join(Environment.NewLine, CCD.Diagnostics.Table.ToArray()) + Environment.NewLine);
			break;
		case "PCIBusDiagnosticsTabPage":
			Clipboard.SetText(string.Join(Environment.NewLine, PCI.Diagnostics.Table.ToArray()) + Environment.NewLine);
			break;
		case "SCIBusPCMDiagnosticsTabPage":
			Clipboard.SetText(string.Join(Environment.NewLine, PCM.Diagnostics.Table.ToArray()) + Environment.NewLine);
			break;
		case "SCIBusTCMDiagnosticsTabPage":
			Clipboard.SetText(string.Join(Environment.NewLine, TCM.Diagnostics.Table.ToArray()) + Environment.NewLine);
			break;
		}
	}

	private void DiagnosticsSnapshotButton_Click(object sender, EventArgs e)
	{
		switch (((Control)DiagnosticsTabControl.SelectedTab).Name)
		{
		case "CCDBusDiagnosticsTabPage":
		{
			string text = DateTime.Now.ToString("yyyyMMdd_HHmmss");
			string text4 = "LOG/CCD/ccdsnapshot_" + text;
			int num = 1;
			while (File.Exists(text4 + ".txt"))
			{
				if (text4.Length > 35)
				{
					text4 = text4.Remove(text4.Length - 3, 3);
				}
				text4 += "_";
				if (num < 10)
				{
					text4 += "0";
				}
				text4 += num;
				num++;
			}
			text4 += ".txt";
			File.AppendAllText(text4, string.Join(Environment.NewLine, CCD.Diagnostics.Table.ToArray()) + Environment.NewLine);
			break;
		}
		case "PCIBusDiagnosticsTabPage":
		{
			string text = DateTime.Now.ToString("yyyyMMdd_HHmmss");
			string text3 = "LOG/PCI/pcisnapshot_" + text;
			int num = 1;
			while (File.Exists(text3 + ".txt"))
			{
				if (text3.Length > 35)
				{
					text3 = text3.Remove(text3.Length - 3, 3);
				}
				text3 += "_";
				if (num < 10)
				{
					text3 += "0";
				}
				text3 += num;
				num++;
			}
			text3 += ".txt";
			File.AppendAllText(text3, string.Join(Environment.NewLine, PCI.Diagnostics.Table.ToArray()) + Environment.NewLine);
			break;
		}
		case "SCIBusPCMDiagnosticsTabPage":
		{
			string text = DateTime.Now.ToString("yyyyMMdd_HHmmss");
			string text5 = "LOG/PCM/pcmsnapshot_" + text;
			int num = 1;
			while (File.Exists(text5 + ".txt"))
			{
				if (text5.Length > 35)
				{
					text5 = text5.Remove(text5.Length - 3, 3);
				}
				text5 += "_";
				if (num < 10)
				{
					text5 += "0";
				}
				text5 += num;
				num++;
			}
			text5 += ".txt";
			File.AppendAllText(text5, string.Join(Environment.NewLine, PCM.Diagnostics.Table.ToArray()) + Environment.NewLine);
			break;
		}
		case "SCIBusTCMDiagnosticsTabPage":
		{
			string text = DateTime.Now.ToString("yyyyMMdd_HHmmss");
			string text2 = "LOG/TCM/tcmsnapshot_" + text;
			int num = 1;
			while (File.Exists(text2 + ".txt"))
			{
				if (text2.Length > 35)
				{
					text2 = text2.Remove(text2.Length - 3, 3);
				}
				text2 += "_";
				if (num < 10)
				{
					text2 += "0";
				}
				text2 += num;
				num++;
			}
			text2 += ".txt";
			File.AppendAllText(text2, string.Join(Environment.NewLine, TCM.Diagnostics.Table.ToArray()) + Environment.NewLine);
			break;
		}
		}
	}

	private void LCDApplySettingsButton_Click(object sender, EventArgs e)
	{
		byte b = (byte)((ListControl)LCDStateComboBox).SelectedIndex;
		if (!byte.TryParse(Util.HexStringToByte(((Control)LCDI2CAddressTextBox).Text)[0].ToString(), out var result))
		{
			result = 39;
			((Control)LCDI2CAddressTextBox).Text = "27";
		}
		if (!byte.TryParse(((Control)LCDWidthTextBox).Text, out var result2))
		{
			result2 = 20;
			((Control)LCDWidthTextBox).Text = "20";
		}
		if (!byte.TryParse(((Control)LCDHeightTextBox).Text, out var result3))
		{
			result3 = 4;
			((Control)LCDHeightTextBox).Text = "4";
		}
		if (!byte.TryParse(((Control)LCDRefreshRateTextBox).Text, out var result4))
		{
			result4 = 20;
			((Control)LCDRefreshRateTextBox).Text = "20";
		}
		byte b2 = ((!(Settings.Default.Units == "imperial") && Settings.Default.Units == "metric") ? ((byte)1) : ((byte)0));
		byte b3 = (byte)(((ListControl)LCDDataSourceComboBox).SelectedIndex + 1);
		Packet packet = new Packet();
		packet.Bus = 0;
		packet.Command = 3;
		packet.Mode = 5;
		packet.Payload = new byte[7] { b, result, result2, result3, result4, b2, b3 };
		Util.UpdateTextBox(USBTextBox, "[<-TX] Change LCD settings:", PacketHelper.Serialize(packet));
		SerialService.WritePacket(packet);
		UpdateLCDPreviewTextBox();
	}

	private void LCDDataSourceComboBox_SelectedIndexChanged(object sender, EventArgs e)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		if (((ListControl)LCDDataSourceComboBox).SelectedIndex == 2)
		{
			MessageBox.Show("Currently the transmission controller cannot be selected as LCD data source.", "Information", (MessageBoxButtons)0, (MessageBoxIcon)64);
			((ListControl)LCDDataSourceComboBox).SelectedIndex = 1;
		}
	}

	private void UpdateLCDPreviewTextBox()
	{
		switch (((ListControl)LCDStateComboBox).SelectedIndex)
		{
		case 0:
			if (((Control)LCDWidthTextBox).Text == "20" && ((Control)LCDHeightTextBox).Text == "4")
			{
				((TextBoxBase)LCDPreviewTextBox).Clear();
				((TextBoxBase)LCDPreviewTextBox).AppendText("--------------------" + Environment.NewLine);
				((TextBoxBase)LCDPreviewTextBox).AppendText("  CHRYSLER CCD/SCI  " + Environment.NewLine);
				((TextBoxBase)LCDPreviewTextBox).AppendText("   SCANNER VX.XX    " + Environment.NewLine);
				((TextBoxBase)LCDPreviewTextBox).AppendText("--------------------");
			}
			else if (((Control)LCDWidthTextBox).Text == "16" && ((Control)LCDHeightTextBox).Text == "2")
			{
				((TextBoxBase)LCDPreviewTextBox).Clear();
				((TextBoxBase)LCDPreviewTextBox).AppendText("CHRYSLER CCD/SCI" + Environment.NewLine);
				((TextBoxBase)LCDPreviewTextBox).AppendText(" SCANNER VX.XX  ");
			}
			else
			{
				((TextBoxBase)LCDPreviewTextBox).Clear();
				((TextBoxBase)LCDPreviewTextBox).AppendText("CCD/SCI");
			}
			break;
		case 1:
			if (((Control)LCDWidthTextBox).Text == "20" && ((Control)LCDHeightTextBox).Text == "4")
			{
				if (Settings.Default.Units == "imperial")
				{
					((TextBoxBase)LCDPreviewTextBox).Clear();
					((TextBoxBase)LCDPreviewTextBox).AppendText("  0mph     0rpm   0%" + Environment.NewLine);
					((TextBoxBase)LCDPreviewTextBox).AppendText("  0/  0°F     0.0psi" + Environment.NewLine);
					((TextBoxBase)LCDPreviewTextBox).AppendText(" 0.0/ 0.0V          " + Environment.NewLine);
					((TextBoxBase)LCDPreviewTextBox).AppendText("     0.000mi        ");
				}
				else if (Settings.Default.Units == "metric")
				{
					((TextBoxBase)LCDPreviewTextBox).Clear();
					((TextBoxBase)LCDPreviewTextBox).AppendText("  0km/h    0rpm   0%" + Environment.NewLine);
					((TextBoxBase)LCDPreviewTextBox).AppendText("  0/  0°C     0.0kPa" + Environment.NewLine);
					((TextBoxBase)LCDPreviewTextBox).AppendText(" 0.0/ 0.0V          " + Environment.NewLine);
					((TextBoxBase)LCDPreviewTextBox).AppendText("     0.000km        ");
				}
			}
			else if (((Control)LCDWidthTextBox).Text == "16" && ((Control)LCDHeightTextBox).Text == "2")
			{
				if (Settings.Default.Units == "imperial")
				{
					((TextBoxBase)LCDPreviewTextBox).Clear();
					((TextBoxBase)LCDPreviewTextBox).AppendText("  0mph      0rpm" + Environment.NewLine);
					((TextBoxBase)LCDPreviewTextBox).AppendText("  0°F     0.0psi");
				}
				else if (Settings.Default.Units == "metric")
				{
					((TextBoxBase)LCDPreviewTextBox).Clear();
					((TextBoxBase)LCDPreviewTextBox).AppendText("  0km/h     0rpm" + Environment.NewLine);
					((TextBoxBase)LCDPreviewTextBox).AppendText("  0°C     0.0kPa");
				}
			}
			else
			{
				((TextBoxBase)LCDPreviewTextBox).Clear();
				((TextBoxBase)LCDPreviewTextBox).AppendText("    0rpm");
			}
			break;
		}
	}

	private void UpdateToolStripMenuItem_Click(object sender, EventArgs e)
	{
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Invalid comparison between Unknown and I4
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Invalid comparison between Unknown and I4
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Invalid comparison between Unknown and I4
		//IL_0163: Unknown result type (might be due to invalid IL or missing references)
		ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
		if (!Directory.Exists("Update"))
		{
			Directory.CreateDirectory("Update");
		}
		if (!HandleGuiUpdate())
		{
			return;
		}
		if (!DeviceFound)
		{
			MessageBox.Show("Connect to the device and try again!", "Warning", (MessageBoxButtons)0, (MessageBoxIcon)48);
			return;
		}
		string text = "";
		if (string.IsNullOrEmpty(HWVersion))
		{
			if ((int)MessageBox.Show("Force latest scanner firmware update?", "Confirm", (MessageBoxButtons)4, (MessageBoxIcon)32) == 7)
			{
				return;
			}
			text = (((int)MessageBox.Show("Is this a V2 scanner?\n(V2 has 2 push-buttons, V1 has 1)", "Device Type", (MessageBoxButtons)4, (MessageBoxIcon)32) != 6) ? "V1" : (((int)MessageBox.Show("Is the hardware version V2.4.0 or above?\n(Check the sticker on the device)", "V2 Hardware Version", (MessageBoxButtons)4, (MessageBoxIcon)32) == 6) ? "V2_NEW" : "V2_OLD"));
		}
		else if (HWVersion.Contains("v1."))
		{
			text = "V1";
		}
		else if (HWVersion.Contains("v2."))
		{
			text = ((new Version(HWVersion.Replace("v", "")) >= new Version(2, 4, 0)) ? "V2_NEW" : "V2_OLD");
		}
		switch (text)
		{
		case "V1":
			UpdateV1Scanner();
			break;
		case "V2_OLD":
			UpdateV2Scanner(isHardwareV240: false);
			break;
		case "V2_NEW":
			UpdateV2Scanner(isHardwareV240: true);
			break;
		default:
			MessageBox.Show("Firmware update availability cannot be checked.", "Warning", (MessageBoxButtons)0, (MessageBoxIcon)48);
			break;
		}
	}

	private bool HandleGuiUpdate()
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Invalid comparison between Unknown and I4
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		string text = "Update/AssemblyInfo.cs";
		string url = "https://raw.githubusercontent.com/laszlodaniel/ChryslerScanner/master/GUI/ChryslerScanner/bin/Release/ChryslerScanner_GUI.zip";
		if (!TryDownload("https://raw.githubusercontent.com/laszlodaniel/ChryslerScanner/master/GUI/ChryslerScanner/Properties/AssemblyInfo.cs", text))
		{
			MessageBox.Show("GUI update check failed.", "Warning", (MessageBoxButtons)0, (MessageBoxIcon)48);
			return false;
		}
		string? text2 = File.ReadLines(text).FirstOrDefault((string l) => l.StartsWith("[assembly: AssemblyVersion"));
		string text3 = "v" + ((text2 != null) ? text2.Split(new char[1] { '"' })[1] : null);
		File.Delete(text);
		if (text3 == GUIVersion)
		{
			MessageBox.Show("You are using the latest GUI version.", "Info", (MessageBoxButtons)0, (MessageBoxIcon)64);
			return true;
		}
		if ((int)MessageBox.Show("New GUI version " + text3 + " available. Download?", "Update", (MessageBoxButtons)4, (MessageBoxIcon)32) == 6)
		{
			if (TryDownload(url, "Update/ChryslerScanner_GUI.zip"))
			{
				MessageBox.Show("Download finished. Unpack the ZIP from the 'Update' folder.", "Done");
			}
			return false;
		}
		MessageBox.Show("Please run the latest GUI version to update scanner firmware.", "Information");
		return false;
	}

	private void UpdateV1Scanner()
	{
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		string text = "Update/temp.ino";
		string url = "https://raw.githubusercontent.com/laszlodaniel/ChryslerScanner/master/Arduino/ChryslerCCDSCIScanner/ChryslerCCDSCIScanner.ino";
		if (!TryDownload(url, text))
		{
			return;
		}
		string text2 = File.ReadLines(text).FirstOrDefault((string l) => l.Contains("#define FW_VERSION"));
		if (text2 == null)
		{
			return;
		}
		uint num = Convert.ToUInt32(text2.Substring(19, 10), 16);
		string text3 = $"v{num >> 24}.{(num >> 16) & 0xFFu}.{(num >> 8) & 0xFFu}";
		if (text3 == FWVersion)
		{
			MessageBox.Show("Scanner has latest firmware.", "No update", (MessageBoxButtons)0, (MessageBoxIcon)64);
		}
		else if (PromptUpdate(text3, FWVersion))
		{
			string url2 = "https://raw.githubusercontent.com/laszlodaniel/ChryslerScanner/master/Arduino/ChryslerCCDSCIScanner/ChryslerCCDSCIScanner.ino.mega.hex";
			if (TryDownload(url2, "Tools/firmware.hex"))
			{
				RunFlashTool("avrdude.exe", "-C avrdude.conf -p m2560 -c wiring -P " + SelectedPort + " -b 115200 -D -U flash:w:firmware.hex:i");
				File.Delete("Tools/firmware.hex");
				PostUpdateComplete(text3);
			}
		}
		File.Delete(text);
	}

	private void UpdateV2Scanner(bool isHardwareV240)
	{
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		string text = (isHardwareV240 ? "PlatformIO/ChryslerScanner/.pio/build/esp32-pico-v3-02" : "PlatformIO/ChryslerScanner/.pio/build/esp32-pico-d4");
		string text2 = "Update/CMakeLists.txt";
		if (TryDownload("https://raw.githubusercontent.com/laszlodaniel/ChryslerScanner/master/PlatformIO/ChryslerScanner/CMakeLists.txt", text2))
		{
			string text3 = File.ReadLines(text2).FirstOrDefault((string l) => l.Contains("set(PROJECT_VER"));
			string text4 = "v" + text3?.Substring(text3.IndexOf('"') + 1, 5);
			if (text4 == FWVersion && !string.IsNullOrEmpty(HWVersion))
			{
				MessageBox.Show("Scanner has latest firmware.", "No update", (MessageBoxButtons)0, (MessageBoxIcon)64);
			}
			else if (PromptUpdate(text4, FWVersion) && TryDownload("https://raw.githubusercontent.com/laszlodaniel/ChryslerScanner/master/" + text + "/bootloader.bin", "Tools/bootloader.bin") && TryDownload("https://raw.githubusercontent.com/laszlodaniel/ChryslerScanner/master/" + text + "/firmware.bin", "Tools/firmware.bin") && TryDownload("https://raw.githubusercontent.com/laszlodaniel/ChryslerScanner/master/" + text + "/ota_data_initial.bin", "Tools/ota_data_initial.bin") && TryDownload("https://raw.githubusercontent.com/laszlodaniel/ChryslerScanner/master/" + text + "/partitions.bin", "Tools/partitions.bin"))
			{
				string args = "--chip esp32 --port " + SelectedPort + " --baud 921600 --before default_reset --after hard_reset write_flash -z --flash_mode dio --flash_freq 40m --flash_size detect 0x1000 bootloader.bin 0x8000 partitions.bin 0xe000 ota_data_initial.bin 0x10000 firmware.bin";
				RunFlashTool("esptool.exe", args);
				File.Delete("Tools/bootloader.bin");
				File.Delete("Tools/firmware.bin");
				File.Delete("Tools/ota_data_initial.bin");
				File.Delete("Tools/partitions.bin");
				PostUpdateComplete(text4);
			}
			File.Delete(text2);
		}
	}

	private bool TryDownload(string url, string localPath)
	{
		try
		{
			Downloader.DownloadFile(new Uri(url), localPath);
			return File.Exists(localPath);
		}
		catch
		{
			return false;
		}
	}

	private bool PromptUpdate(string latest, string current)
	{
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Invalid comparison between Unknown and I4
		return (int)MessageBox.Show("Latest: " + latest + "\nCurrent: " + (current ?? "Unknown") + "\n\nUpdate device now?", "Firmware Update", (MessageBoxButtons)4, (MessageBoxIcon)32) == 6;
	}

	private void RunFlashTool(string exe, string args)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		if (!File.Exists("Tools/" + exe))
		{
			MessageBox.Show(exe + " missing in Tools folder.", "Error");
			return;
		}
		SerialService.Disconnect();
		((Control)this).Refresh();
		Process.Start(new ProcessStartInfo
		{
			WorkingDirectory = "Tools",
			FileName = exe,
			Arguments = args,
			UseShellExecute = true,
			CreateNoWindow = false
		})?.WaitForExit();
		((Control)this).Refresh();
	}

	private void PostUpdateComplete(string newVer)
	{
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		FWVersion = newVer;
		if (!SerialService.Connect(SelectedPort))
		{
			Util.UpdateTextBox(USBTextBox, "[INFO] Device not found on " + SelectedPort + ".");
		}
		ResetButton_Click(this, EventArgs.Empty);
		ResetFromUpdate = true;
		MessageBox.Show("Update complete!", "Success");
	}

	private void Downloader_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
	{
	}

	private void Downloader_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
	{
	}

	private void ReadMemoryToolStripMenuItem_Click(object sender, EventArgs e)
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Expected O, but got Unknown
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Invalid comparison between Unknown and I4
		if (ReadMemory == null)
		{
			ReadMemoryForm readMemoryForm = new ReadMemoryForm(this, ContainerManager.Instance.GetInstance<SerialService>());
			((Form)readMemoryForm).StartPosition = (FormStartPosition)4;
			ReadMemory = readMemoryForm;
			((Form)ReadMemory).FormClosed += (FormClosedEventHandler)delegate
			{
				ReadMemory = null;
			};
			((Form)ReadMemory).Show((IWin32Window)(object)this);
			if ((int)((Form)ReadMemory).StartPosition == 4)
			{
				int val = ((Form)this).Location.X + (((Control)this).Width - ((Control)ReadMemory).Width) / 2;
				int val2 = ((Form)this).Location.Y + (((Control)this).Height - ((Control)ReadMemory).Height) / 2;
				((Form)ReadMemory).Location = new Point(Math.Max(val, 0), Math.Max(val2, 0));
			}
		}
		else
		{
			((Form)ReadMemory).WindowState = (FormWindowState)0;
			((Control)ReadMemory).Focus();
		}
	}

	private void ReadWriteMemoryToolStripMenuItem_Click(object sender, EventArgs e)
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Expected O, but got Unknown
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Invalid comparison between Unknown and I4
		if (ReadWriteMemory == null)
		{
			ReadWriteMemoryForm readWriteMemoryForm = new ReadWriteMemoryForm(this, ContainerManager.Instance.GetInstance<SerialService>());
			((Form)readWriteMemoryForm).StartPosition = (FormStartPosition)4;
			ReadWriteMemory = readWriteMemoryForm;
			((Form)ReadWriteMemory).FormClosed += (FormClosedEventHandler)delegate
			{
				ReadWriteMemory = null;
			};
			((Form)ReadWriteMemory).Show((IWin32Window)(object)this);
			if ((int)((Form)ReadWriteMemory).StartPosition == 4)
			{
				int val = ((Form)this).Location.X + (((Control)this).Width - ((Control)ReadWriteMemory).Width) / 2;
				int val2 = ((Form)this).Location.Y + (((Control)this).Height - ((Control)ReadWriteMemory).Height) / 2;
				((Form)ReadWriteMemory).Location = new Point(Math.Max(val, 0), Math.Max(val2, 0));
			}
		}
		else
		{
			((Form)ReadWriteMemory).WindowState = (FormWindowState)0;
			((Control)ReadWriteMemory).Focus();
		}
	}

	private void BootstrapToolsToolStripMenuItem_Click(object sender, EventArgs e)
	{
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Expected O, but got Unknown
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Invalid comparison between Unknown and I4
		if (BootstrapTools == null)
		{
			ScannerTabControl.SelectedTab = SCIBusControlTabPage;
			if (((ListControl)SCIBusModuleComboBox).SelectedIndex == 0)
			{
				DiagnosticsTabControl.SelectedTab = SCIBusPCMDiagnosticsTabPage;
			}
			else if (((ListControl)SCIBusModuleComboBox).SelectedIndex == 1)
			{
				DiagnosticsTabControl.SelectedTab = SCIBusTCMDiagnosticsTabPage;
			}
			BootstrapToolsForm bootstrapToolsForm = new BootstrapToolsForm(this, ContainerManager.Instance.GetInstance<SerialService>());
			((Form)bootstrapToolsForm).StartPosition = (FormStartPosition)4;
			BootstrapTools = bootstrapToolsForm;
			((Form)BootstrapTools).FormClosed += (FormClosedEventHandler)delegate
			{
				BootstrapTools = null;
			};
			((Form)BootstrapTools).Show((IWin32Window)(object)this);
			if ((int)((Form)BootstrapTools).StartPosition == 4)
			{
				int val = ((Form)this).Location.X + (((Control)this).Width - ((Control)BootstrapTools).Width) / 2;
				int val2 = ((Form)this).Location.Y + (((Control)this).Height - ((Control)BootstrapTools).Height) / 2;
				((Form)BootstrapTools).Location = new Point(Math.Max(val, 0), Math.Max(val2, 0));
			}
		}
		else
		{
			((Form)BootstrapTools).WindowState = (FormWindowState)0;
			((Control)BootstrapTools).Focus();
		}
	}

	private void EngineToolsToolStripMenuItem_Click(object sender, EventArgs e)
	{
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Expected O, but got Unknown
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Invalid comparison between Unknown and I4
		if (EngineTools == null)
		{
			ScannerTabControl.SelectedTab = SCIBusControlTabPage;
			DiagnosticsTabControl.SelectedTab = SCIBusPCMDiagnosticsTabPage;
			EngineToolsForm engineToolsForm = new EngineToolsForm(this, ContainerManager.Instance.GetInstance<SerialService>());
			((Form)engineToolsForm).StartPosition = (FormStartPosition)4;
			EngineTools = engineToolsForm;
			((Form)EngineTools).FormClosed += (FormClosedEventHandler)delegate
			{
				EngineTools = null;
			};
			((Form)EngineTools).Show((IWin32Window)(object)this);
			if ((int)((Form)EngineTools).StartPosition == 4)
			{
				int val = ((Form)this).Location.X + (((Control)this).Width - ((Control)EngineTools).Width) / 2;
				int val2 = ((Form)this).Location.Y + (((Control)this).Height - ((Control)EngineTools).Height) / 2;
				((Form)EngineTools).Location = new Point(Math.Max(val, 0), Math.Max(val2, 0));
			}
		}
		else
		{
			((Form)EngineTools).WindowState = (FormWindowState)0;
			((Control)EngineTools).Focus();
		}
	}

	private void ABSToolsToolStripMenuItem_Click(object sender, EventArgs e)
	{
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Expected O, but got Unknown
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Invalid comparison between Unknown and I4
		if (ABSTools == null)
		{
			ScannerTabControl.SelectedTab = CCDBusControlTabPage;
			DiagnosticsTabControl.SelectedTab = CCDBusDiagnosticsTabPage;
			ABSToolsForm aBSToolsForm = new ABSToolsForm(this, ContainerManager.Instance.GetInstance<SerialService>());
			((Form)aBSToolsForm).StartPosition = (FormStartPosition)4;
			ABSTools = aBSToolsForm;
			((Form)ABSTools).FormClosed += (FormClosedEventHandler)delegate
			{
				ABSTools = null;
			};
			((Form)ABSTools).Show((IWin32Window)(object)this);
			if ((int)((Form)ABSTools).StartPosition == 4)
			{
				int val = ((Form)this).Location.X + (((Control)this).Width - ((Control)ABSTools).Width) / 2;
				int val2 = ((Form)this).Location.Y + (((Control)this).Height - ((Control)ABSTools).Height) / 2;
				((Form)ABSTools).Location = new Point(Math.Max(val, 0), Math.Max(val2, 0));
			}
		}
		else
		{
			((Form)ABSTools).WindowState = (FormWindowState)0;
			((Control)ABSTools).Focus();
		}
	}

	private void MetricUnitsToolStripMenuItem_Click(object sender, EventArgs e)
	{
		ImperialUnitsToolStripMenuItem.Checked = false;
		MetricUnitsToolStripMenuItem.Checked = true;
		Settings.Default.Units = "metric";
		((SettingsBase)Settings.Default).Save();
		ReadWriteMemory?.UpdateMileageUnit();
	}

	private void ImperialUnitsToolStripMenuItem_Click(object sender, EventArgs e)
	{
		ImperialUnitsToolStripMenuItem.Checked = true;
		MetricUnitsToolStripMenuItem.Checked = false;
		Settings.Default.Units = "imperial";
		((SettingsBase)Settings.Default).Save();
		ReadWriteMemory?.UpdateMileageUnit();
	}

	private void EnglishLangToolStripMenuItem_Click(object sender, EventArgs e)
	{
		EnglishLangToolStripMenuItem.Checked = true;
		SpanishLangToolStripMenuItem.Checked = false;
		Settings.Default.Language = "English";
		((SettingsBase)Settings.Default).Save();
		ChangeLanguage();
	}

	private void SpanishLangToolStripMenuItem_Click(object sender, EventArgs e)
	{
		EnglishLangToolStripMenuItem.Checked = false;
		SpanishLangToolStripMenuItem.Checked = true;
		Settings.Default.Language = "Spanish";
		((SettingsBase)Settings.Default).Save();
		ChangeLanguage();
	}

	public void ChangeLanguage()
	{
		if (Settings.Default.Language == "English")
		{
			CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.GetCultureInfo("en");
		}
		else if (Settings.Default.Language == "Spanish")
		{
			CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.GetCultureInfo("es");
		}
		((ToolStripItem)ToolsToolStripMenuItem).Text = strings.Tools;
		((ToolStripItem)SettingsToolStripMenuItem).Text = strings.Settings;
		((ToolStripItem)AboutToolStripMenuItem).Text = strings.About;
		((ToolStripItem)UpdateToolStripMenuItem).Text = strings.Update;
		((ToolStripItem)ReadMemoryToolStripMenuItem).Text = strings.ReadMemory;
		((ToolStripItem)ReadWriteMemoryToolStripMenuItem).Text = strings.ReadWriteMemory;
		((ToolStripItem)BootstrapToolsToolStripMenuItem).Text = strings.BootstrapTools;
		((ToolStripItem)EngineToolsToolStripMenuItem).Text = strings.EngineTools;
		((ToolStripItem)ABSToolsToolStripMenuItem).Text = strings.ABSTools;
		((ToolStripItem)UnitToolStripMenuItem).Text = strings.Unit;
		((ToolStripItem)LanguageToolStripMenuItem).Text = strings.Language;
		((ToolStripItem)IncludeTimestampInLogFilesToolStripMenuItem).Text = strings.IncludeTimestampInLogFiles;
		((ToolStripItem)CCDBusOnDemandToolStripMenuItem).Text = strings.CCDBusOnDemand;
		((ToolStripItem)PCIBusOnDemandToolStripMenuItem).Text = strings.PCIBusOnDemand;
		((ToolStripItem)SortMessagesByIDByteToolStripMenuItem).Text = strings.SortMessagesByIDByte;
		((ToolStripItem)MetricUnitsToolStripMenuItem).Text = strings.Metric;
		((ToolStripItem)ImperialUnitsToolStripMenuItem).Text = strings.Imperial;
		((ToolStripItem)EnglishLangToolStripMenuItem).Text = strings.English;
		((ToolStripItem)SpanishLangToolStripMenuItem).Text = strings.Spanish;
		((Control)USBCommunicationGroupBox).Text = strings.USBCommunication;
		((Control)ControlPanelGroupBox).Text = strings.ControlPanel;
		((Control)DiagnosticsGroupBox).Text = strings.Diagnostics;
		((Control)USBSendPacketButton).Text = strings.SendPacket;
		if (DeviceFound)
		{
			((Control)ConnectButton).Text = strings.Disconnect;
		}
		else
		{
			((Control)ConnectButton).Text = strings.Connect;
		}
		((Control)COMPortsRefreshButton).Text = strings.Refresh;
		((Control)DemoButton).Text = strings.Demo;
		if (((Form)this).Size == new Size(405, 650))
		{
			((Control)ExpandButton).Text = strings.Expand;
		}
		else if (((Form)this).Size == new Size(1300, 650))
		{
			((Control)ExpandButton).Text = strings.Collapse;
		}
		((Control)MainLabel).Text = strings.Main;
		((Control)ResetButton).Text = strings.Reset;
		((Control)HandshakeButton).Text = strings.Handshake;
		((Control)StatusButton).Text = strings.Status;
		((Control)RequestLabel).Text = strings.Request;
		((Control)VersionInfoButton).Text = strings.VersionInfo;
		((Control)TimestampButton).Text = strings.Timestamp;
		((Control)VoltagesButton).Text = strings.Voltages;
		((Control)SettingsLabel).Text = strings.SettingsLabel;
		((Control)SetLEDsButton).Text = strings.SetLEDs;
		((Control)HeartbeatIntervalLabel).Text = strings.HeartbeatInterval;
		((Control)LEDBlinkDurationLabel).Text = strings.BlinkDuration;
		((Control)SCIBusPCMDiagnosticsTabPage).Text = strings.SCIBusEngine;
		((Control)SCIBusTCMDiagnosticsTabPage).Text = strings.SCIBusTransmission;
		((Control)DiagnosticsRefreshButton).Text = strings.Refresh;
		((Control)DiagnosticsResetViewButton).Text = strings.ResetView;
		((Control)DiagnosticsCopyToClipboardButton).Text = strings.CopyTableToClipboard;
		((Control)DiagnosticsSnapshotButton).Text = strings.Snapshot;
		if (About != null)
		{
			((Control)About).Text = strings.About;
		}
		if (ReadMemory != null)
		{
			((Control)ReadMemory).Text = strings.ReadMemory;
		}
		if (ReadWriteMemory != null)
		{
			((Control)ReadWriteMemory).Text = strings.ReadWriteMemory;
		}
		if (BootstrapTools != null)
		{
			((Control)BootstrapTools).Text = strings.BootstrapTools;
		}
		if (EngineTools != null)
		{
			((Control)EngineTools).Text = strings.EngineTools;
		}
		if (ABSTools != null)
		{
			((Control)ABSTools).Text = strings.ABSTools;
		}
	}

	private void UpdateUARTBaudrate(int baudrate)
	{
		if (DeviceFound)
		{
			byte[] payload = new byte[4]
			{
				(byte)((uint)(baudrate >> 24) & 0xFFu),
				(byte)((uint)(baudrate >> 16) & 0xFFu),
				(byte)((uint)(baudrate >> 8) & 0xFFu),
				(byte)((uint)baudrate & 0xFFu)
			};
			Packet packet = new Packet();
			packet.Bus = 0;
			packet.Command = 3;
			packet.Mode = 8;
			packet.Payload = payload;
			Util.UpdateTextBox(USBTextBox, "[<-TX] Set UART baudrate:", PacketHelper.Serialize(packet));
			Util.UpdateTextBox(USBTextBox, "[INFO] GUI baudrate = " + baudrate);
			SerialService.WritePacket(packet);
		}
	}

	private void Baudrate250000ToolStripMenuItem_Click(object sender, EventArgs e)
	{
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		Baudrate250000ToolStripMenuItem.Checked = true;
		Baudrate115200ToolStripMenuItem.Checked = false;
		Settings.Default.UART0Baudrate = 250000;
		((SettingsBase)Settings.Default).Save();
		UpdateUARTBaudrate(Settings.Default.UART0Baudrate);
		MessageBox.Show("Restart GUI!", "Information", (MessageBoxButtons)0, (MessageBoxIcon)64);
		Application.Exit();
	}

	private void Baudrate115200ToolStripMenuItem_Click(object sender, EventArgs e)
	{
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		Baudrate250000ToolStripMenuItem.Checked = false;
		Baudrate115200ToolStripMenuItem.Checked = true;
		Settings.Default.UART0Baudrate = 115200;
		((SettingsBase)Settings.Default).Save();
		UpdateUARTBaudrate(Settings.Default.UART0Baudrate);
		MessageBox.Show("Restart GUI!", "Information", (MessageBoxButtons)0, (MessageBoxIcon)64);
		Application.Exit();
	}

	private void IncludeTimestampInLogFilesToolStripMenuItem_Click(object sender, EventArgs e)
	{
		if (IncludeTimestampInLogFilesToolStripMenuItem.Checked)
		{
			Settings.Default.Timestamp = true;
		}
		else
		{
			Settings.Default.Timestamp = false;
		}
		((SettingsBase)Settings.Default).Save();
	}

	private void CCDBusOnDemandToolStripMenuItem_Click(object sender, EventArgs e)
	{
		if (CCDBusOnDemandToolStripMenuItem.Checked)
		{
			Settings.Default.CCDBusOnDemand = true;
		}
		else
		{
			Settings.Default.CCDBusOnDemand = false;
		}
		((SettingsBase)Settings.Default).Save();
	}

	private void PCIBusOnDemandToolStripMenuItem_Click(object sender, EventArgs e)
	{
		if (PCIBusOnDemandToolStripMenuItem.Checked)
		{
			Settings.Default.PCIBusOnDemand = true;
		}
		else
		{
			Settings.Default.PCIBusOnDemand = false;
		}
		((SettingsBase)Settings.Default).Save();
	}

	private void SortMessagesByIDByteToolStripMenuItem_Click(object sender, EventArgs e)
	{
		if (SortMessagesByIDByteToolStripMenuItem.Checked)
		{
			Settings.Default.SortByID = true;
		}
		else
		{
			Settings.Default.SortByID = false;
		}
		((SettingsBase)Settings.Default).Save();
	}

	private void DisplayRawBusPacketsToolStripMenuItem_Click(object sender, EventArgs e)
	{
		if (DisplayRawBusPacketsToolStripMenuItem.Checked)
		{
			Settings.Default.DisplayRawBusPackets = true;
		}
		else
		{
			Settings.Default.DisplayRawBusPackets = false;
		}
		((SettingsBase)Settings.Default).Save();
	}

	private void AboutToolStripMenuItem_Click(object sender, EventArgs e)
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Expected O, but got Unknown
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		AboutForm aboutForm = new AboutForm(this);
		((Form)aboutForm).StartPosition = (FormStartPosition)4;
		About = aboutForm;
		((Form)About).FormClosed += (FormClosedEventHandler)delegate
		{
			About = null;
		};
		((Form)About).ShowDialog((IWin32Window)(object)this);
		ChangeLanguage();
	}

	private void MainForm_Load(object sender, EventArgs e)
	{
		Util.UpdateTextBox(USBTextBox, "[INFO] GUI started (" + GUIVersion + ")");
	}

	private void MainForm_KeyDown(object sender, KeyEventArgs e)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Invalid comparison between Unknown and I4
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Invalid comparison between Unknown and I4
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Invalid comparison between Unknown and I4
		if (DeviceFound)
		{
			if (e.Control && (int)e.KeyCode == 66)
			{
				BootstrapToolsToolStripMenuItem_Click(this, EventArgs.Empty);
			}
			if (e.Control && (int)e.KeyCode == 69)
			{
				EngineToolsToolStripMenuItem_Click(this, EventArgs.Empty);
			}
			if (e.Control && (int)e.KeyCode == 87)
			{
				ABSToolsToolStripMenuItem_Click(this, EventArgs.Empty);
			}
		}
	}

	private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
	{
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
		//IL_02a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02af: Expected O, but got Unknown
		//IL_02b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ba: Expected O, but got Unknown
		//IL_02bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c5: Expected O, but got Unknown
		//IL_02c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d0: Expected O, but got Unknown
		//IL_02d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02db: Expected O, but got Unknown
		//IL_02dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e6: Expected O, but got Unknown
		//IL_02e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f1: Expected O, but got Unknown
		//IL_02f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fc: Expected O, but got Unknown
		//IL_02fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0307: Expected O, but got Unknown
		//IL_0308: Unknown result type (might be due to invalid IL or missing references)
		//IL_0312: Expected O, but got Unknown
		//IL_0313: Unknown result type (might be due to invalid IL or missing references)
		//IL_031d: Expected O, but got Unknown
		//IL_031e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0328: Expected O, but got Unknown
		//IL_0329: Unknown result type (might be due to invalid IL or missing references)
		//IL_0333: Expected O, but got Unknown
		//IL_0334: Unknown result type (might be due to invalid IL or missing references)
		//IL_033e: Expected O, but got Unknown
		//IL_033f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0349: Expected O, but got Unknown
		//IL_034a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0354: Expected O, but got Unknown
		//IL_0355: Unknown result type (might be due to invalid IL or missing references)
		//IL_035f: Expected O, but got Unknown
		//IL_0360: Unknown result type (might be due to invalid IL or missing references)
		//IL_036a: Expected O, but got Unknown
		//IL_036b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0375: Expected O, but got Unknown
		//IL_0376: Unknown result type (might be due to invalid IL or missing references)
		//IL_0380: Expected O, but got Unknown
		//IL_0381: Unknown result type (might be due to invalid IL or missing references)
		//IL_038b: Expected O, but got Unknown
		//IL_038c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0396: Expected O, but got Unknown
		//IL_0397: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a1: Expected O, but got Unknown
		//IL_03a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ac: Expected O, but got Unknown
		//IL_03ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b7: Expected O, but got Unknown
		//IL_03b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c2: Expected O, but got Unknown
		//IL_03c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_03cd: Expected O, but got Unknown
		//IL_03ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d8: Expected O, but got Unknown
		//IL_03d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e3: Expected O, but got Unknown
		//IL_03e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ee: Expected O, but got Unknown
		//IL_03ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f9: Expected O, but got Unknown
		//IL_03fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0404: Expected O, but got Unknown
		//IL_0405: Unknown result type (might be due to invalid IL or missing references)
		//IL_040f: Expected O, but got Unknown
		//IL_0410: Unknown result type (might be due to invalid IL or missing references)
		//IL_041a: Expected O, but got Unknown
		//IL_041b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0425: Expected O, but got Unknown
		//IL_0426: Unknown result type (might be due to invalid IL or missing references)
		//IL_0430: Expected O, but got Unknown
		//IL_0431: Unknown result type (might be due to invalid IL or missing references)
		//IL_043b: Expected O, but got Unknown
		//IL_043c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0446: Expected O, but got Unknown
		//IL_0447: Unknown result type (might be due to invalid IL or missing references)
		//IL_0451: Expected O, but got Unknown
		//IL_0452: Unknown result type (might be due to invalid IL or missing references)
		//IL_045c: Expected O, but got Unknown
		//IL_045d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0467: Expected O, but got Unknown
		//IL_0468: Unknown result type (might be due to invalid IL or missing references)
		//IL_0472: Expected O, but got Unknown
		//IL_0473: Unknown result type (might be due to invalid IL or missing references)
		//IL_047d: Expected O, but got Unknown
		//IL_047e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0488: Expected O, but got Unknown
		//IL_0489: Unknown result type (might be due to invalid IL or missing references)
		//IL_0493: Expected O, but got Unknown
		//IL_0494: Unknown result type (might be due to invalid IL or missing references)
		//IL_049e: Expected O, but got Unknown
		//IL_049f: Unknown result type (might be due to invalid IL or missing references)
		//IL_04a9: Expected O, but got Unknown
		//IL_04aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_04b4: Expected O, but got Unknown
		//IL_04b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_04bf: Expected O, but got Unknown
		//IL_04c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_04ca: Expected O, but got Unknown
		//IL_04cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_04d5: Expected O, but got Unknown
		//IL_04d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_04e0: Expected O, but got Unknown
		//IL_04e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_04eb: Expected O, but got Unknown
		//IL_04ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_04f6: Expected O, but got Unknown
		//IL_04f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0501: Expected O, but got Unknown
		//IL_0502: Unknown result type (might be due to invalid IL or missing references)
		//IL_050c: Expected O, but got Unknown
		//IL_050d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0517: Expected O, but got Unknown
		//IL_0518: Unknown result type (might be due to invalid IL or missing references)
		//IL_0522: Expected O, but got Unknown
		//IL_0523: Unknown result type (might be due to invalid IL or missing references)
		//IL_052d: Expected O, but got Unknown
		//IL_052e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0538: Expected O, but got Unknown
		//IL_0539: Unknown result type (might be due to invalid IL or missing references)
		//IL_0543: Expected O, but got Unknown
		//IL_0544: Unknown result type (might be due to invalid IL or missing references)
		//IL_054e: Expected O, but got Unknown
		//IL_054f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0559: Expected O, but got Unknown
		//IL_055a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0564: Expected O, but got Unknown
		//IL_0565: Unknown result type (might be due to invalid IL or missing references)
		//IL_056f: Expected O, but got Unknown
		//IL_0570: Unknown result type (might be due to invalid IL or missing references)
		//IL_057a: Expected O, but got Unknown
		//IL_057b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0585: Expected O, but got Unknown
		//IL_0586: Unknown result type (might be due to invalid IL or missing references)
		//IL_0590: Expected O, but got Unknown
		//IL_0591: Unknown result type (might be due to invalid IL or missing references)
		//IL_059b: Expected O, but got Unknown
		//IL_059c: Unknown result type (might be due to invalid IL or missing references)
		//IL_05a6: Expected O, but got Unknown
		//IL_05a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_05b1: Expected O, but got Unknown
		//IL_05b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_05bc: Expected O, but got Unknown
		//IL_05bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_05c7: Expected O, but got Unknown
		//IL_05c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_05d2: Expected O, but got Unknown
		//IL_05d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_05dd: Expected O, but got Unknown
		//IL_05de: Unknown result type (might be due to invalid IL or missing references)
		//IL_05e8: Expected O, but got Unknown
		//IL_05e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_05f3: Expected O, but got Unknown
		//IL_05f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_05fe: Expected O, but got Unknown
		//IL_05ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0609: Expected O, but got Unknown
		//IL_060a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0614: Expected O, but got Unknown
		//IL_0615: Unknown result type (might be due to invalid IL or missing references)
		//IL_061f: Expected O, but got Unknown
		//IL_0620: Unknown result type (might be due to invalid IL or missing references)
		//IL_062a: Expected O, but got Unknown
		//IL_062b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0635: Expected O, but got Unknown
		//IL_0636: Unknown result type (might be due to invalid IL or missing references)
		//IL_0640: Expected O, but got Unknown
		//IL_0641: Unknown result type (might be due to invalid IL or missing references)
		//IL_064b: Expected O, but got Unknown
		//IL_064c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0656: Expected O, but got Unknown
		//IL_0657: Unknown result type (might be due to invalid IL or missing references)
		//IL_0661: Expected O, but got Unknown
		//IL_0662: Unknown result type (might be due to invalid IL or missing references)
		//IL_066c: Expected O, but got Unknown
		//IL_066d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0677: Expected O, but got Unknown
		//IL_0678: Unknown result type (might be due to invalid IL or missing references)
		//IL_0682: Expected O, but got Unknown
		//IL_0683: Unknown result type (might be due to invalid IL or missing references)
		//IL_068d: Expected O, but got Unknown
		//IL_0699: Unknown result type (might be due to invalid IL or missing references)
		//IL_06a3: Expected O, but got Unknown
		//IL_06af: Unknown result type (might be due to invalid IL or missing references)
		//IL_06b9: Expected O, but got Unknown
		//IL_06c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_06cf: Expected O, but got Unknown
		//IL_0f8b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0f95: Expected O, but got Unknown
		//IL_14d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_14da: Expected O, but got Unknown
		//IL_154a: Unknown result type (might be due to invalid IL or missing references)
		//IL_1554: Expected O, but got Unknown
		//IL_166d: Unknown result type (might be due to invalid IL or missing references)
		//IL_1677: Expected O, but got Unknown
		//IL_16c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_16d0: Expected O, but got Unknown
		//IL_171f: Unknown result type (might be due to invalid IL or missing references)
		//IL_1729: Expected O, but got Unknown
		//IL_182e: Unknown result type (might be due to invalid IL or missing references)
		//IL_1838: Expected O, but got Unknown
		//IL_1dd5: Unknown result type (might be due to invalid IL or missing references)
		//IL_1ddf: Expected O, but got Unknown
		//IL_1e4f: Unknown result type (might be due to invalid IL or missing references)
		//IL_1e59: Expected O, but got Unknown
		//IL_20f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_2101: Expected O, but got Unknown
		//IL_25c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_25d0: Expected O, but got Unknown
		//IL_2d76: Unknown result type (might be due to invalid IL or missing references)
		//IL_2d80: Expected O, but got Unknown
		//IL_384b: Unknown result type (might be due to invalid IL or missing references)
		//IL_3855: Expected O, but got Unknown
		//IL_386f: Unknown result type (might be due to invalid IL or missing references)
		//IL_3879: Expected O, but got Unknown
		ComponentResourceManager componentResourceManager = new ComponentResourceManager(typeof(MainForm));
		MenuStrip = new MenuStrip();
		ToolsToolStripMenuItem = new ToolStripMenuItem();
		UpdateToolStripMenuItem = new ToolStripMenuItem();
		ReadMemoryToolStripMenuItem = new ToolStripMenuItem();
		ReadWriteMemoryToolStripMenuItem = new ToolStripMenuItem();
		BootstrapToolsToolStripMenuItem = new ToolStripMenuItem();
		EngineToolsToolStripMenuItem = new ToolStripMenuItem();
		ABSToolsToolStripMenuItem = new ToolStripMenuItem();
		SettingsToolStripMenuItem = new ToolStripMenuItem();
		LanguageToolStripMenuItem = new ToolStripMenuItem();
		EnglishLangToolStripMenuItem = new ToolStripMenuItem();
		SpanishLangToolStripMenuItem = new ToolStripMenuItem();
		UnitToolStripMenuItem = new ToolStripMenuItem();
		MetricUnitsToolStripMenuItem = new ToolStripMenuItem();
		ImperialUnitsToolStripMenuItem = new ToolStripMenuItem();
		UARTBaudrateToolStripMenuItem = new ToolStripMenuItem();
		Baudrate250000ToolStripMenuItem = new ToolStripMenuItem();
		Baudrate115200ToolStripMenuItem = new ToolStripMenuItem();
		IncludeTimestampInLogFilesToolStripMenuItem = new ToolStripMenuItem();
		CCDBusOnDemandToolStripMenuItem = new ToolStripMenuItem();
		PCIBusOnDemandToolStripMenuItem = new ToolStripMenuItem();
		SortMessagesByIDByteToolStripMenuItem = new ToolStripMenuItem();
		DisplayRawBusPacketsToolStripMenuItem = new ToolStripMenuItem();
		AboutToolStripMenuItem = new ToolStripMenuItem();
		USBCommunicationGroupBox = new GroupBox();
		USBSendPacketButton = new Button();
		USBSendPacketComboBox = new ComboBox();
		USBTextBox = new TextBox();
		ControlPanelGroupBox = new GroupBox();
		DemoButton = new Button();
		ExpandButton = new Button();
		ScannerTabControl = new TabControl();
		ScannerControlTabPage = new TabPage();
		MillisecondsLabel02 = new Label();
		LEDBlinkDurationTextBox = new TextBox();
		LEDBlinkDurationLabel = new Label();
		MillisecondsLabel01 = new Label();
		HeartbeatIntervalTextBox = new TextBox();
		HeartbeatIntervalLabel = new Label();
		SetLEDsButton = new Button();
		SettingsLabel = new Label();
		EEPROMWriteEnableCheckBox = new CheckBox();
		EEPROMReadCountLabel = new Label();
		EEPROMReadCountTextBox = new TextBox();
		EEPROMWriteValuesLabel = new Label();
		EEPROMWriteValuesTextBox = new TextBox();
		EEPROMWriteAddressLabel = new Label();
		EEPROMWriteAddressTextBox = new TextBox();
		WriteEEPROMButton = new Button();
		ExternalEEPROMRadioButton = new RadioButton();
		InternalEEPROMRadioButton = new RadioButton();
		EEPROMReadAddressLabel = new Label();
		EEPROMReadAddressTextBox = new TextBox();
		ReadEEPROMButton = new Button();
		DebugLabel = new Label();
		EEPROMChecksumButton = new Button();
		MainLabel = new Label();
		RequestLabel = new Label();
		VoltagesButton = new Button();
		TimestampButton = new Button();
		VersionInfoButton = new Button();
		StatusButton = new Button();
		HandshakeButton = new Button();
		ResetButton = new Button();
		CCDBusControlTabPage = new TabPage();
		CCDBusTransceiverOnOffCheckBox = new CheckBox();
		MeasureCCDBusVoltagesButton = new Button();
		CCDBusTerminationBiasOnOffCheckBox = new CheckBox();
		MillisecondsLabel04 = new Label();
		CCDBusRandomMessageIntervalMaxTextBox = new TextBox();
		CCDBusRandomMessageIntervalMaxLabel = new Label();
		CCDBusRandomMessageIntervalMinLabel = new Label();
		CCDBusRandomMessageIntervalMinTextBox = new TextBox();
		CCDBusStopRepeatedMessagesButton = new Button();
		MillisecondsLabel03 = new Label();
		CCDBusTxMessageRepeatIntervalTextBox = new TextBox();
		CCDBusTxMessageRepeatIntervalCheckBox = new CheckBox();
		CCDBusSendMessagesButton = new Button();
		CCDBusTxMessageChecksumCheckBox = new CheckBox();
		CCDBusOverwriteDuplicateIDCheckBox = new CheckBox();
		CCDBusTxMessageClearListButton = new Button();
		CCDBusTxMessageRemoveItemButton = new Button();
		CCDBusTxMessageAddButton = new Button();
		CCDBusTxMessageComboBox = new ComboBox();
		CCDBusTxMessagesListBox = new ListBox();
		DebugRandomCCDBusMessagesButton = new Button();
		PCIBusControlTabPage = new TabPage();
		PCIBusTransceiverOnOffCheckBox = new CheckBox();
		PCIBusStopRepeatedMessagesButton = new Button();
		MillisecondsLabel06 = new Label();
		PCIBusTxMessageRepeatIntervalTextBox = new TextBox();
		PCIBusTxMessageRepeatIntervalCheckBox = new CheckBox();
		PCIBusSendMessagesButton = new Button();
		PCIBusTxMessageCRCCheckBox = new CheckBox();
		PCIBusOverwriteDuplicateIDCheckBox = new CheckBox();
		PCIBusTxMessageClearListButton = new Button();
		PCIBusTxMessageRemoveItemButton = new Button();
		PCIBusTxMessageAddButton = new Button();
		PCIBusTxMessageComboBox = new ComboBox();
		PCIBusTxMessagesListBox = new ListBox();
		SCIBusControlTabPage = new TabPage();
		SCIBusLogicComboBox = new ComboBox();
		SCIBusLogicLabel = new Label();
		SCIBusOBDConfigurationComboBox = new ComboBox();
		SCIBusOBDConfigurationLabel = new Label();
		SCIBusModuleConfigSpeedApplyButton = new Button();
		SCIBusSpeedComboBox = new ComboBox();
		SCIBusSpeedLabel = new Label();
		SCIBusModuleComboBox = new ComboBox();
		SCIBusModuleLabel = new Label();
		SCIBusStopRepeatedMessagesButton = new Button();
		MillisecondsLabel05 = new Label();
		SCIBusTxMessageRepeatIntervalTextBox = new TextBox();
		SCIBusTxMessageRepeatIntervalCheckBox = new CheckBox();
		SCIBusSendMessagesButton = new Button();
		SCIBusTxMessageChecksumCheckBox = new CheckBox();
		SCIBusOverwriteDuplicateIDCheckBox = new CheckBox();
		SCIBusTxMessageClearListButton = new Button();
		SCIBusTxMessageRemoveItemButton = new Button();
		SCIBusTxMessageAddButton = new Button();
		SCIBusTxMessageComboBox = new ComboBox();
		SCIBusTxMessagesListBox = new ListBox();
		LCDControlTabPage = new TabPage();
		LCDI2CAddressHexLabel = new Label();
		LCDI2CAddressTextBox = new TextBox();
		LCDI2CAddressLabel = new Label();
		LCDPreviewLabel = new Label();
		LCDDataSourceComboBox = new ComboBox();
		LCDDataSourceLabel = new Label();
		LCDStateComboBox = new ComboBox();
		LCDStateLabel = new Label();
		LCDRowLabel = new Label();
		LCDHeightTextBox = new TextBox();
		LCDColumnLabel = new Label();
		LCDWidthTextBox = new TextBox();
		LCDSizeLabel = new Label();
		LCDApplySettingsButton = new Button();
		LCDRefreshRateLabel = new Label();
		HzLabel01 = new Label();
		LCDRefreshRateTextBox = new TextBox();
		LCDPreviewTextBox = new TextBox();
		COMPortsRefreshButton = new Button();
		COMPortsComboBox = new ComboBox();
		ConnectButton = new Button();
		DiagnosticsGroupBox = new GroupBox();
		DiagnosticsSnapshotButton = new Button();
		DiagnosticsRefreshButton = new Button();
		DiagnosticsCopyToClipboardButton = new Button();
		DiagnosticsResetViewButton = new Button();
		DiagnosticsTabControl = new TabControl();
		CCDBusDiagnosticsTabPage = new TabPage();
		CCDBusDiagnosticsListBox = new FlickerFreeListBox();
		PCIBusDiagnosticsTabPage = new TabPage();
		PCIBusDiagnosticsListBox = new FlickerFreeListBox();
		SCIBusPCMDiagnosticsTabPage = new TabPage();
		SCIBusPCMDiagnosticsListBox = new FlickerFreeListBox();
		SCIBusTCMDiagnosticsTabPage = new TabPage();
		SCIBusTCMDiagnosticsListBox = new FlickerFreeListBox();
		((Control)MenuStrip).SuspendLayout();
		((Control)USBCommunicationGroupBox).SuspendLayout();
		((Control)ControlPanelGroupBox).SuspendLayout();
		((Control)ScannerTabControl).SuspendLayout();
		((Control)ScannerControlTabPage).SuspendLayout();
		((Control)CCDBusControlTabPage).SuspendLayout();
		((Control)PCIBusControlTabPage).SuspendLayout();
		((Control)SCIBusControlTabPage).SuspendLayout();
		((Control)LCDControlTabPage).SuspendLayout();
		((Control)DiagnosticsGroupBox).SuspendLayout();
		((Control)DiagnosticsTabControl).SuspendLayout();
		((Control)CCDBusDiagnosticsTabPage).SuspendLayout();
		((Control)PCIBusDiagnosticsTabPage).SuspendLayout();
		((Control)SCIBusPCMDiagnosticsTabPage).SuspendLayout();
		((Control)SCIBusTCMDiagnosticsTabPage).SuspendLayout();
		((Control)this).SuspendLayout();
		((ToolStrip)MenuStrip).ImageScalingSize = new Size(20, 20);
		((ToolStrip)MenuStrip).Items.AddRange((ToolStripItem[])(object)new ToolStripItem[3]
		{
			(ToolStripItem)ToolsToolStripMenuItem,
			(ToolStripItem)SettingsToolStripMenuItem,
			(ToolStripItem)AboutToolStripMenuItem
		});
		componentResourceManager.ApplyResources(MenuStrip, "MenuStrip");
		((Control)MenuStrip).Name = "MenuStrip";
		((ToolStripDropDownItem)ToolsToolStripMenuItem).DropDownItems.AddRange((ToolStripItem[])(object)new ToolStripItem[6]
		{
			(ToolStripItem)UpdateToolStripMenuItem,
			(ToolStripItem)ReadMemoryToolStripMenuItem,
			(ToolStripItem)ReadWriteMemoryToolStripMenuItem,
			(ToolStripItem)BootstrapToolsToolStripMenuItem,
			(ToolStripItem)EngineToolsToolStripMenuItem,
			(ToolStripItem)ABSToolsToolStripMenuItem
		});
		((ToolStripItem)ToolsToolStripMenuItem).Name = "ToolsToolStripMenuItem";
		componentResourceManager.ApplyResources(ToolsToolStripMenuItem, "ToolsToolStripMenuItem");
		((ToolStripItem)UpdateToolStripMenuItem).Name = "UpdateToolStripMenuItem";
		componentResourceManager.ApplyResources(UpdateToolStripMenuItem, "UpdateToolStripMenuItem");
		((ToolStripItem)UpdateToolStripMenuItem).Click += UpdateToolStripMenuItem_Click;
		componentResourceManager.ApplyResources(ReadMemoryToolStripMenuItem, "ReadMemoryToolStripMenuItem");
		((ToolStripItem)ReadMemoryToolStripMenuItem).Name = "ReadMemoryToolStripMenuItem";
		((ToolStripItem)ReadMemoryToolStripMenuItem).Click += ReadMemoryToolStripMenuItem_Click;
		componentResourceManager.ApplyResources(ReadWriteMemoryToolStripMenuItem, "ReadWriteMemoryToolStripMenuItem");
		((ToolStripItem)ReadWriteMemoryToolStripMenuItem).Name = "ReadWriteMemoryToolStripMenuItem";
		((ToolStripItem)ReadWriteMemoryToolStripMenuItem).Click += ReadWriteMemoryToolStripMenuItem_Click;
		componentResourceManager.ApplyResources(BootstrapToolsToolStripMenuItem, "BootstrapToolsToolStripMenuItem");
		((ToolStripItem)BootstrapToolsToolStripMenuItem).Name = "BootstrapToolsToolStripMenuItem";
		((ToolStripItem)BootstrapToolsToolStripMenuItem).Click += BootstrapToolsToolStripMenuItem_Click;
		componentResourceManager.ApplyResources(EngineToolsToolStripMenuItem, "EngineToolsToolStripMenuItem");
		((ToolStripItem)EngineToolsToolStripMenuItem).Name = "EngineToolsToolStripMenuItem";
		((ToolStripItem)EngineToolsToolStripMenuItem).Click += EngineToolsToolStripMenuItem_Click;
		componentResourceManager.ApplyResources(ABSToolsToolStripMenuItem, "ABSToolsToolStripMenuItem");
		((ToolStripItem)ABSToolsToolStripMenuItem).Name = "ABSToolsToolStripMenuItem";
		((ToolStripItem)ABSToolsToolStripMenuItem).Click += ABSToolsToolStripMenuItem_Click;
		((ToolStripDropDownItem)SettingsToolStripMenuItem).DropDownItems.AddRange((ToolStripItem[])(object)new ToolStripItem[8]
		{
			(ToolStripItem)LanguageToolStripMenuItem,
			(ToolStripItem)UnitToolStripMenuItem,
			(ToolStripItem)UARTBaudrateToolStripMenuItem,
			(ToolStripItem)IncludeTimestampInLogFilesToolStripMenuItem,
			(ToolStripItem)CCDBusOnDemandToolStripMenuItem,
			(ToolStripItem)PCIBusOnDemandToolStripMenuItem,
			(ToolStripItem)SortMessagesByIDByteToolStripMenuItem,
			(ToolStripItem)DisplayRawBusPacketsToolStripMenuItem
		});
		((ToolStripItem)SettingsToolStripMenuItem).Name = "SettingsToolStripMenuItem";
		componentResourceManager.ApplyResources(SettingsToolStripMenuItem, "SettingsToolStripMenuItem");
		((ToolStripDropDownItem)LanguageToolStripMenuItem).DropDownItems.AddRange((ToolStripItem[])(object)new ToolStripItem[2]
		{
			(ToolStripItem)EnglishLangToolStripMenuItem,
			(ToolStripItem)SpanishLangToolStripMenuItem
		});
		((ToolStripItem)LanguageToolStripMenuItem).Name = "LanguageToolStripMenuItem";
		componentResourceManager.ApplyResources(LanguageToolStripMenuItem, "LanguageToolStripMenuItem");
		EnglishLangToolStripMenuItem.Checked = true;
		EnglishLangToolStripMenuItem.CheckOnClick = true;
		EnglishLangToolStripMenuItem.CheckState = (CheckState)1;
		((ToolStripItem)EnglishLangToolStripMenuItem).Name = "EnglishLangToolStripMenuItem";
		componentResourceManager.ApplyResources(EnglishLangToolStripMenuItem, "EnglishLangToolStripMenuItem");
		((ToolStripItem)EnglishLangToolStripMenuItem).Click += EnglishLangToolStripMenuItem_Click;
		SpanishLangToolStripMenuItem.CheckOnClick = true;
		((ToolStripItem)SpanishLangToolStripMenuItem).Name = "SpanishLangToolStripMenuItem";
		componentResourceManager.ApplyResources(SpanishLangToolStripMenuItem, "SpanishLangToolStripMenuItem");
		((ToolStripItem)SpanishLangToolStripMenuItem).Click += SpanishLangToolStripMenuItem_Click;
		((ToolStripDropDownItem)UnitToolStripMenuItem).DropDownItems.AddRange((ToolStripItem[])(object)new ToolStripItem[2]
		{
			(ToolStripItem)MetricUnitsToolStripMenuItem,
			(ToolStripItem)ImperialUnitsToolStripMenuItem
		});
		((ToolStripItem)UnitToolStripMenuItem).Name = "UnitToolStripMenuItem";
		componentResourceManager.ApplyResources(UnitToolStripMenuItem, "UnitToolStripMenuItem");
		MetricUnitsToolStripMenuItem.Checked = true;
		MetricUnitsToolStripMenuItem.CheckOnClick = true;
		MetricUnitsToolStripMenuItem.CheckState = (CheckState)1;
		((ToolStripItem)MetricUnitsToolStripMenuItem).Name = "MetricUnitsToolStripMenuItem";
		componentResourceManager.ApplyResources(MetricUnitsToolStripMenuItem, "MetricUnitsToolStripMenuItem");
		((ToolStripItem)MetricUnitsToolStripMenuItem).Click += MetricUnitsToolStripMenuItem_Click;
		ImperialUnitsToolStripMenuItem.CheckOnClick = true;
		((ToolStripItem)ImperialUnitsToolStripMenuItem).Name = "ImperialUnitsToolStripMenuItem";
		componentResourceManager.ApplyResources(ImperialUnitsToolStripMenuItem, "ImperialUnitsToolStripMenuItem");
		((ToolStripItem)ImperialUnitsToolStripMenuItem).Click += ImperialUnitsToolStripMenuItem_Click;
		((ToolStripDropDownItem)UARTBaudrateToolStripMenuItem).DropDownItems.AddRange((ToolStripItem[])(object)new ToolStripItem[2]
		{
			(ToolStripItem)Baudrate250000ToolStripMenuItem,
			(ToolStripItem)Baudrate115200ToolStripMenuItem
		});
		((ToolStripItem)UARTBaudrateToolStripMenuItem).Name = "UARTBaudrateToolStripMenuItem";
		componentResourceManager.ApplyResources(UARTBaudrateToolStripMenuItem, "UARTBaudrateToolStripMenuItem");
		Baudrate250000ToolStripMenuItem.Checked = true;
		Baudrate250000ToolStripMenuItem.CheckOnClick = true;
		Baudrate250000ToolStripMenuItem.CheckState = (CheckState)1;
		((ToolStripItem)Baudrate250000ToolStripMenuItem).Name = "Baudrate250000ToolStripMenuItem";
		componentResourceManager.ApplyResources(Baudrate250000ToolStripMenuItem, "Baudrate250000ToolStripMenuItem");
		((ToolStripItem)Baudrate250000ToolStripMenuItem).Click += Baudrate250000ToolStripMenuItem_Click;
		Baudrate115200ToolStripMenuItem.CheckOnClick = true;
		((ToolStripItem)Baudrate115200ToolStripMenuItem).Name = "Baudrate115200ToolStripMenuItem";
		componentResourceManager.ApplyResources(Baudrate115200ToolStripMenuItem, "Baudrate115200ToolStripMenuItem");
		((ToolStripItem)Baudrate115200ToolStripMenuItem).Click += Baudrate115200ToolStripMenuItem_Click;
		IncludeTimestampInLogFilesToolStripMenuItem.CheckOnClick = true;
		((ToolStripItem)IncludeTimestampInLogFilesToolStripMenuItem).Name = "IncludeTimestampInLogFilesToolStripMenuItem";
		componentResourceManager.ApplyResources(IncludeTimestampInLogFilesToolStripMenuItem, "IncludeTimestampInLogFilesToolStripMenuItem");
		((ToolStripItem)IncludeTimestampInLogFilesToolStripMenuItem).Click += IncludeTimestampInLogFilesToolStripMenuItem_Click;
		CCDBusOnDemandToolStripMenuItem.CheckOnClick = true;
		((ToolStripItem)CCDBusOnDemandToolStripMenuItem).Name = "CCDBusOnDemandToolStripMenuItem";
		componentResourceManager.ApplyResources(CCDBusOnDemandToolStripMenuItem, "CCDBusOnDemandToolStripMenuItem");
		((ToolStripItem)CCDBusOnDemandToolStripMenuItem).Click += CCDBusOnDemandToolStripMenuItem_Click;
		PCIBusOnDemandToolStripMenuItem.CheckOnClick = true;
		((ToolStripItem)PCIBusOnDemandToolStripMenuItem).Name = "PCIBusOnDemandToolStripMenuItem";
		componentResourceManager.ApplyResources(PCIBusOnDemandToolStripMenuItem, "PCIBusOnDemandToolStripMenuItem");
		((ToolStripItem)PCIBusOnDemandToolStripMenuItem).Click += PCIBusOnDemandToolStripMenuItem_Click;
		SortMessagesByIDByteToolStripMenuItem.Checked = true;
		SortMessagesByIDByteToolStripMenuItem.CheckOnClick = true;
		SortMessagesByIDByteToolStripMenuItem.CheckState = (CheckState)1;
		((ToolStripItem)SortMessagesByIDByteToolStripMenuItem).Name = "SortMessagesByIDByteToolStripMenuItem";
		componentResourceManager.ApplyResources(SortMessagesByIDByteToolStripMenuItem, "SortMessagesByIDByteToolStripMenuItem");
		((ToolStripItem)SortMessagesByIDByteToolStripMenuItem).Click += SortMessagesByIDByteToolStripMenuItem_Click;
		DisplayRawBusPacketsToolStripMenuItem.Checked = true;
		DisplayRawBusPacketsToolStripMenuItem.CheckOnClick = true;
		DisplayRawBusPacketsToolStripMenuItem.CheckState = (CheckState)1;
		((ToolStripItem)DisplayRawBusPacketsToolStripMenuItem).Name = "DisplayRawBusPacketsToolStripMenuItem";
		componentResourceManager.ApplyResources(DisplayRawBusPacketsToolStripMenuItem, "DisplayRawBusPacketsToolStripMenuItem");
		((ToolStripItem)DisplayRawBusPacketsToolStripMenuItem).Click += DisplayRawBusPacketsToolStripMenuItem_Click;
		((ToolStripItem)AboutToolStripMenuItem).Name = "AboutToolStripMenuItem";
		componentResourceManager.ApplyResources(AboutToolStripMenuItem, "AboutToolStripMenuItem");
		((ToolStripItem)AboutToolStripMenuItem).Click += AboutToolStripMenuItem_Click;
		((Control)USBCommunicationGroupBox).Controls.Add((Control)(object)USBSendPacketButton);
		((Control)USBCommunicationGroupBox).Controls.Add((Control)(object)USBSendPacketComboBox);
		((Control)USBCommunicationGroupBox).Controls.Add((Control)(object)USBTextBox);
		componentResourceManager.ApplyResources(USBCommunicationGroupBox, "USBCommunicationGroupBox");
		((Control)USBCommunicationGroupBox).Name = "USBCommunicationGroupBox";
		USBCommunicationGroupBox.TabStop = false;
		componentResourceManager.ApplyResources(USBSendPacketButton, "USBSendPacketButton");
		((Control)USBSendPacketButton).Name = "USBSendPacketButton";
		((ButtonBase)USBSendPacketButton).UseVisualStyleBackColor = true;
		((Control)USBSendPacketButton).Click += USBSendPacketButton_Click;
		componentResourceManager.ApplyResources(USBSendPacketComboBox, "USBSendPacketComboBox");
		((ListControl)USBSendPacketComboBox).FormattingEnabled = true;
		((Control)USBSendPacketComboBox).Name = "USBSendPacketComboBox";
		((Control)USBSendPacketComboBox).KeyPress += new KeyPressEventHandler(USBSendPacketComboBox_KeyPress);
		((Control)USBTextBox).BackColor = SystemColors.Window;
		componentResourceManager.ApplyResources(USBTextBox, "USBTextBox");
		((Control)USBTextBox).Name = "USBTextBox";
		((TextBoxBase)USBTextBox).ReadOnly = true;
		((Control)ControlPanelGroupBox).Controls.Add((Control)(object)DemoButton);
		((Control)ControlPanelGroupBox).Controls.Add((Control)(object)ExpandButton);
		((Control)ControlPanelGroupBox).Controls.Add((Control)(object)ScannerTabControl);
		((Control)ControlPanelGroupBox).Controls.Add((Control)(object)COMPortsRefreshButton);
		((Control)ControlPanelGroupBox).Controls.Add((Control)(object)COMPortsComboBox);
		((Control)ControlPanelGroupBox).Controls.Add((Control)(object)ConnectButton);
		componentResourceManager.ApplyResources(ControlPanelGroupBox, "ControlPanelGroupBox");
		((Control)ControlPanelGroupBox).Name = "ControlPanelGroupBox";
		ControlPanelGroupBox.TabStop = false;
		componentResourceManager.ApplyResources(DemoButton, "DemoButton");
		((Control)DemoButton).Name = "DemoButton";
		((ButtonBase)DemoButton).UseVisualStyleBackColor = true;
		((Control)DemoButton).Click += DemoButton_Click;
		componentResourceManager.ApplyResources(ExpandButton, "ExpandButton");
		((Control)ExpandButton).Name = "ExpandButton";
		((ButtonBase)ExpandButton).UseVisualStyleBackColor = true;
		((Control)ExpandButton).Click += ExpandButton_Click;
		((Control)ScannerTabControl).Controls.Add((Control)(object)ScannerControlTabPage);
		((Control)ScannerTabControl).Controls.Add((Control)(object)CCDBusControlTabPage);
		((Control)ScannerTabControl).Controls.Add((Control)(object)PCIBusControlTabPage);
		((Control)ScannerTabControl).Controls.Add((Control)(object)SCIBusControlTabPage);
		((Control)ScannerTabControl).Controls.Add((Control)(object)LCDControlTabPage);
		componentResourceManager.ApplyResources(ScannerTabControl, "ScannerTabControl");
		((Control)ScannerTabControl).Name = "ScannerTabControl";
		ScannerTabControl.SelectedIndex = 0;
		((Control)ScannerControlTabPage).BackColor = Color.Transparent;
		((Control)ScannerControlTabPage).Controls.Add((Control)(object)MillisecondsLabel02);
		((Control)ScannerControlTabPage).Controls.Add((Control)(object)LEDBlinkDurationTextBox);
		((Control)ScannerControlTabPage).Controls.Add((Control)(object)LEDBlinkDurationLabel);
		((Control)ScannerControlTabPage).Controls.Add((Control)(object)MillisecondsLabel01);
		((Control)ScannerControlTabPage).Controls.Add((Control)(object)HeartbeatIntervalTextBox);
		((Control)ScannerControlTabPage).Controls.Add((Control)(object)HeartbeatIntervalLabel);
		((Control)ScannerControlTabPage).Controls.Add((Control)(object)SetLEDsButton);
		((Control)ScannerControlTabPage).Controls.Add((Control)(object)SettingsLabel);
		((Control)ScannerControlTabPage).Controls.Add((Control)(object)EEPROMWriteEnableCheckBox);
		((Control)ScannerControlTabPage).Controls.Add((Control)(object)EEPROMReadCountLabel);
		((Control)ScannerControlTabPage).Controls.Add((Control)(object)EEPROMReadCountTextBox);
		((Control)ScannerControlTabPage).Controls.Add((Control)(object)EEPROMWriteValuesLabel);
		((Control)ScannerControlTabPage).Controls.Add((Control)(object)EEPROMWriteValuesTextBox);
		((Control)ScannerControlTabPage).Controls.Add((Control)(object)EEPROMWriteAddressLabel);
		((Control)ScannerControlTabPage).Controls.Add((Control)(object)EEPROMWriteAddressTextBox);
		((Control)ScannerControlTabPage).Controls.Add((Control)(object)WriteEEPROMButton);
		((Control)ScannerControlTabPage).Controls.Add((Control)(object)ExternalEEPROMRadioButton);
		((Control)ScannerControlTabPage).Controls.Add((Control)(object)InternalEEPROMRadioButton);
		((Control)ScannerControlTabPage).Controls.Add((Control)(object)EEPROMReadAddressLabel);
		((Control)ScannerControlTabPage).Controls.Add((Control)(object)EEPROMReadAddressTextBox);
		((Control)ScannerControlTabPage).Controls.Add((Control)(object)ReadEEPROMButton);
		((Control)ScannerControlTabPage).Controls.Add((Control)(object)DebugLabel);
		((Control)ScannerControlTabPage).Controls.Add((Control)(object)EEPROMChecksumButton);
		((Control)ScannerControlTabPage).Controls.Add((Control)(object)MainLabel);
		((Control)ScannerControlTabPage).Controls.Add((Control)(object)RequestLabel);
		((Control)ScannerControlTabPage).Controls.Add((Control)(object)VoltagesButton);
		((Control)ScannerControlTabPage).Controls.Add((Control)(object)TimestampButton);
		((Control)ScannerControlTabPage).Controls.Add((Control)(object)VersionInfoButton);
		((Control)ScannerControlTabPage).Controls.Add((Control)(object)StatusButton);
		((Control)ScannerControlTabPage).Controls.Add((Control)(object)HandshakeButton);
		((Control)ScannerControlTabPage).Controls.Add((Control)(object)ResetButton);
		componentResourceManager.ApplyResources(ScannerControlTabPage, "ScannerControlTabPage");
		((Control)ScannerControlTabPage).Name = "ScannerControlTabPage";
		componentResourceManager.ApplyResources(MillisecondsLabel02, "MillisecondsLabel02");
		((Control)MillisecondsLabel02).Name = "MillisecondsLabel02";
		componentResourceManager.ApplyResources(LEDBlinkDurationTextBox, "LEDBlinkDurationTextBox");
		((Control)LEDBlinkDurationTextBox).Name = "LEDBlinkDurationTextBox";
		((Control)LEDBlinkDurationTextBox).KeyPress += new KeyPressEventHandler(LEDBlinkDurationTextBox_KeyPress);
		componentResourceManager.ApplyResources(LEDBlinkDurationLabel, "LEDBlinkDurationLabel");
		((Control)LEDBlinkDurationLabel).Name = "LEDBlinkDurationLabel";
		componentResourceManager.ApplyResources(MillisecondsLabel01, "MillisecondsLabel01");
		((Control)MillisecondsLabel01).Name = "MillisecondsLabel01";
		componentResourceManager.ApplyResources(HeartbeatIntervalTextBox, "HeartbeatIntervalTextBox");
		((Control)HeartbeatIntervalTextBox).Name = "HeartbeatIntervalTextBox";
		((Control)HeartbeatIntervalTextBox).KeyPress += new KeyPressEventHandler(HeartbeatIntervalTextBox_KeyPress);
		componentResourceManager.ApplyResources(HeartbeatIntervalLabel, "HeartbeatIntervalLabel");
		((Control)HeartbeatIntervalLabel).Name = "HeartbeatIntervalLabel";
		componentResourceManager.ApplyResources(SetLEDsButton, "SetLEDsButton");
		((Control)SetLEDsButton).Name = "SetLEDsButton";
		((ButtonBase)SetLEDsButton).UseVisualStyleBackColor = true;
		((Control)SetLEDsButton).Click += SetLEDsButton_Click;
		componentResourceManager.ApplyResources(SettingsLabel, "SettingsLabel");
		((Control)SettingsLabel).Name = "SettingsLabel";
		componentResourceManager.ApplyResources(EEPROMWriteEnableCheckBox, "EEPROMWriteEnableCheckBox");
		((Control)EEPROMWriteEnableCheckBox).Name = "EEPROMWriteEnableCheckBox";
		((ButtonBase)EEPROMWriteEnableCheckBox).UseVisualStyleBackColor = true;
		EEPROMWriteEnableCheckBox.CheckedChanged += EEPROMWriteEnableCheckBox_CheckedChanged;
		componentResourceManager.ApplyResources(EEPROMReadCountLabel, "EEPROMReadCountLabel");
		((Control)EEPROMReadCountLabel).Name = "EEPROMReadCountLabel";
		componentResourceManager.ApplyResources(EEPROMReadCountTextBox, "EEPROMReadCountTextBox");
		((Control)EEPROMReadCountTextBox).Name = "EEPROMReadCountTextBox";
		((Control)EEPROMReadCountTextBox).KeyPress += new KeyPressEventHandler(EEPROMReadCountTextBox_KeyPress);
		componentResourceManager.ApplyResources(EEPROMWriteValuesLabel, "EEPROMWriteValuesLabel");
		((Control)EEPROMWriteValuesLabel).Name = "EEPROMWriteValuesLabel";
		componentResourceManager.ApplyResources(EEPROMWriteValuesTextBox, "EEPROMWriteValuesTextBox");
		((Control)EEPROMWriteValuesTextBox).Name = "EEPROMWriteValuesTextBox";
		((Control)EEPROMWriteValuesTextBox).KeyPress += new KeyPressEventHandler(EEPROMWriteValuesTextBox_KeyPress);
		componentResourceManager.ApplyResources(EEPROMWriteAddressLabel, "EEPROMWriteAddressLabel");
		((Control)EEPROMWriteAddressLabel).Name = "EEPROMWriteAddressLabel";
		componentResourceManager.ApplyResources(EEPROMWriteAddressTextBox, "EEPROMWriteAddressTextBox");
		((Control)EEPROMWriteAddressTextBox).Name = "EEPROMWriteAddressTextBox";
		((Control)EEPROMWriteAddressTextBox).KeyPress += new KeyPressEventHandler(EEPROMWriteAddressTextBox_KeyPress);
		componentResourceManager.ApplyResources(WriteEEPROMButton, "WriteEEPROMButton");
		((Control)WriteEEPROMButton).Name = "WriteEEPROMButton";
		((ButtonBase)WriteEEPROMButton).UseVisualStyleBackColor = true;
		((Control)WriteEEPROMButton).Click += WriteEEPROMButton_Click;
		componentResourceManager.ApplyResources(ExternalEEPROMRadioButton, "ExternalEEPROMRadioButton");
		ExternalEEPROMRadioButton.Checked = true;
		((Control)ExternalEEPROMRadioButton).Name = "ExternalEEPROMRadioButton";
		ExternalEEPROMRadioButton.TabStop = true;
		((ButtonBase)ExternalEEPROMRadioButton).UseVisualStyleBackColor = true;
		componentResourceManager.ApplyResources(InternalEEPROMRadioButton, "InternalEEPROMRadioButton");
		((Control)InternalEEPROMRadioButton).Name = "InternalEEPROMRadioButton";
		((ButtonBase)InternalEEPROMRadioButton).UseVisualStyleBackColor = true;
		componentResourceManager.ApplyResources(EEPROMReadAddressLabel, "EEPROMReadAddressLabel");
		((Control)EEPROMReadAddressLabel).Name = "EEPROMReadAddressLabel";
		componentResourceManager.ApplyResources(EEPROMReadAddressTextBox, "EEPROMReadAddressTextBox");
		((Control)EEPROMReadAddressTextBox).Name = "EEPROMReadAddressTextBox";
		((Control)EEPROMReadAddressTextBox).KeyPress += new KeyPressEventHandler(EEPROMReadAddressTextBox_KeyPress);
		componentResourceManager.ApplyResources(ReadEEPROMButton, "ReadEEPROMButton");
		((Control)ReadEEPROMButton).Name = "ReadEEPROMButton";
		((ButtonBase)ReadEEPROMButton).UseVisualStyleBackColor = true;
		((Control)ReadEEPROMButton).Click += ReadEEPROMButton_Click;
		componentResourceManager.ApplyResources(DebugLabel, "DebugLabel");
		((Control)DebugLabel).Name = "DebugLabel";
		componentResourceManager.ApplyResources(EEPROMChecksumButton, "EEPROMChecksumButton");
		((Control)EEPROMChecksumButton).Name = "EEPROMChecksumButton";
		((ButtonBase)EEPROMChecksumButton).UseVisualStyleBackColor = true;
		((Control)EEPROMChecksumButton).Click += EEPROMChecksumButton_Click;
		componentResourceManager.ApplyResources(MainLabel, "MainLabel");
		((Control)MainLabel).Name = "MainLabel";
		componentResourceManager.ApplyResources(RequestLabel, "RequestLabel");
		((Control)RequestLabel).Name = "RequestLabel";
		componentResourceManager.ApplyResources(VoltagesButton, "VoltagesButton");
		((Control)VoltagesButton).Name = "VoltagesButton";
		((ButtonBase)VoltagesButton).UseVisualStyleBackColor = true;
		((Control)VoltagesButton).Click += VoltagesButton_Click;
		componentResourceManager.ApplyResources(TimestampButton, "TimestampButton");
		((Control)TimestampButton).Name = "TimestampButton";
		((ButtonBase)TimestampButton).UseVisualStyleBackColor = true;
		((Control)TimestampButton).Click += TimestampButton_Click;
		componentResourceManager.ApplyResources(VersionInfoButton, "VersionInfoButton");
		((Control)VersionInfoButton).Name = "VersionInfoButton";
		((ButtonBase)VersionInfoButton).UseVisualStyleBackColor = true;
		((Control)VersionInfoButton).Click += VersionInfoButton_Click;
		componentResourceManager.ApplyResources(StatusButton, "StatusButton");
		((Control)StatusButton).Name = "StatusButton";
		((ButtonBase)StatusButton).UseVisualStyleBackColor = true;
		((Control)StatusButton).Click += StatusButton_Click;
		componentResourceManager.ApplyResources(HandshakeButton, "HandshakeButton");
		((Control)HandshakeButton).Name = "HandshakeButton";
		((ButtonBase)HandshakeButton).UseVisualStyleBackColor = true;
		((Control)HandshakeButton).Click += HandshakeButton_Click;
		componentResourceManager.ApplyResources(ResetButton, "ResetButton");
		((Control)ResetButton).Name = "ResetButton";
		((ButtonBase)ResetButton).UseVisualStyleBackColor = true;
		((Control)ResetButton).Click += ResetButton_Click;
		((Control)CCDBusControlTabPage).BackColor = Color.Transparent;
		((Control)CCDBusControlTabPage).Controls.Add((Control)(object)CCDBusTransceiverOnOffCheckBox);
		((Control)CCDBusControlTabPage).Controls.Add((Control)(object)MeasureCCDBusVoltagesButton);
		((Control)CCDBusControlTabPage).Controls.Add((Control)(object)CCDBusTerminationBiasOnOffCheckBox);
		((Control)CCDBusControlTabPage).Controls.Add((Control)(object)MillisecondsLabel04);
		((Control)CCDBusControlTabPage).Controls.Add((Control)(object)CCDBusRandomMessageIntervalMaxTextBox);
		((Control)CCDBusControlTabPage).Controls.Add((Control)(object)CCDBusRandomMessageIntervalMaxLabel);
		((Control)CCDBusControlTabPage).Controls.Add((Control)(object)CCDBusRandomMessageIntervalMinLabel);
		((Control)CCDBusControlTabPage).Controls.Add((Control)(object)CCDBusRandomMessageIntervalMinTextBox);
		((Control)CCDBusControlTabPage).Controls.Add((Control)(object)CCDBusStopRepeatedMessagesButton);
		((Control)CCDBusControlTabPage).Controls.Add((Control)(object)MillisecondsLabel03);
		((Control)CCDBusControlTabPage).Controls.Add((Control)(object)CCDBusTxMessageRepeatIntervalTextBox);
		((Control)CCDBusControlTabPage).Controls.Add((Control)(object)CCDBusTxMessageRepeatIntervalCheckBox);
		((Control)CCDBusControlTabPage).Controls.Add((Control)(object)CCDBusSendMessagesButton);
		((Control)CCDBusControlTabPage).Controls.Add((Control)(object)CCDBusTxMessageChecksumCheckBox);
		((Control)CCDBusControlTabPage).Controls.Add((Control)(object)CCDBusOverwriteDuplicateIDCheckBox);
		((Control)CCDBusControlTabPage).Controls.Add((Control)(object)CCDBusTxMessageClearListButton);
		((Control)CCDBusControlTabPage).Controls.Add((Control)(object)CCDBusTxMessageRemoveItemButton);
		((Control)CCDBusControlTabPage).Controls.Add((Control)(object)CCDBusTxMessageAddButton);
		((Control)CCDBusControlTabPage).Controls.Add((Control)(object)CCDBusTxMessageComboBox);
		((Control)CCDBusControlTabPage).Controls.Add((Control)(object)CCDBusTxMessagesListBox);
		((Control)CCDBusControlTabPage).Controls.Add((Control)(object)DebugRandomCCDBusMessagesButton);
		componentResourceManager.ApplyResources(CCDBusControlTabPage, "CCDBusControlTabPage");
		((Control)CCDBusControlTabPage).Name = "CCDBusControlTabPage";
		componentResourceManager.ApplyResources(CCDBusTransceiverOnOffCheckBox, "CCDBusTransceiverOnOffCheckBox");
		((Control)CCDBusTransceiverOnOffCheckBox).Name = "CCDBusTransceiverOnOffCheckBox";
		((ButtonBase)CCDBusTransceiverOnOffCheckBox).UseVisualStyleBackColor = true;
		CCDBusTransceiverOnOffCheckBox.CheckedChanged += CCDBusSettingsCheckBox_CheckedChanged;
		componentResourceManager.ApplyResources(MeasureCCDBusVoltagesButton, "MeasureCCDBusVoltagesButton");
		((Control)MeasureCCDBusVoltagesButton).Name = "MeasureCCDBusVoltagesButton";
		((ButtonBase)MeasureCCDBusVoltagesButton).UseVisualStyleBackColor = true;
		((Control)MeasureCCDBusVoltagesButton).Click += MeasureCCDBusVoltagesButton_Click;
		componentResourceManager.ApplyResources(CCDBusTerminationBiasOnOffCheckBox, "CCDBusTerminationBiasOnOffCheckBox");
		((Control)CCDBusTerminationBiasOnOffCheckBox).Name = "CCDBusTerminationBiasOnOffCheckBox";
		((ButtonBase)CCDBusTerminationBiasOnOffCheckBox).UseVisualStyleBackColor = true;
		CCDBusTerminationBiasOnOffCheckBox.CheckedChanged += CCDBusSettingsCheckBox_CheckedChanged;
		componentResourceManager.ApplyResources(MillisecondsLabel04, "MillisecondsLabel04");
		((Control)MillisecondsLabel04).Name = "MillisecondsLabel04";
		componentResourceManager.ApplyResources(CCDBusRandomMessageIntervalMaxTextBox, "CCDBusRandomMessageIntervalMaxTextBox");
		((Control)CCDBusRandomMessageIntervalMaxTextBox).Name = "CCDBusRandomMessageIntervalMaxTextBox";
		((Control)CCDBusRandomMessageIntervalMaxTextBox).KeyPress += new KeyPressEventHandler(CCDBusRandomMessageIntervalMaxTextBox_KeyPress);
		componentResourceManager.ApplyResources(CCDBusRandomMessageIntervalMaxLabel, "CCDBusRandomMessageIntervalMaxLabel");
		((Control)CCDBusRandomMessageIntervalMaxLabel).Name = "CCDBusRandomMessageIntervalMaxLabel";
		componentResourceManager.ApplyResources(CCDBusRandomMessageIntervalMinLabel, "CCDBusRandomMessageIntervalMinLabel");
		((Control)CCDBusRandomMessageIntervalMinLabel).Name = "CCDBusRandomMessageIntervalMinLabel";
		componentResourceManager.ApplyResources(CCDBusRandomMessageIntervalMinTextBox, "CCDBusRandomMessageIntervalMinTextBox");
		((Control)CCDBusRandomMessageIntervalMinTextBox).Name = "CCDBusRandomMessageIntervalMinTextBox";
		((Control)CCDBusRandomMessageIntervalMinTextBox).KeyPress += new KeyPressEventHandler(CCDBusRandomMessageIntervalMinTextBox_KeyPress);
		componentResourceManager.ApplyResources(CCDBusStopRepeatedMessagesButton, "CCDBusStopRepeatedMessagesButton");
		((Control)CCDBusStopRepeatedMessagesButton).Name = "CCDBusStopRepeatedMessagesButton";
		((ButtonBase)CCDBusStopRepeatedMessagesButton).UseVisualStyleBackColor = true;
		((Control)CCDBusStopRepeatedMessagesButton).Click += CCDBusStopRepeatedMessagesButton_Click;
		componentResourceManager.ApplyResources(MillisecondsLabel03, "MillisecondsLabel03");
		((Control)MillisecondsLabel03).Name = "MillisecondsLabel03";
		componentResourceManager.ApplyResources(CCDBusTxMessageRepeatIntervalTextBox, "CCDBusTxMessageRepeatIntervalTextBox");
		((Control)CCDBusTxMessageRepeatIntervalTextBox).Name = "CCDBusTxMessageRepeatIntervalTextBox";
		componentResourceManager.ApplyResources(CCDBusTxMessageRepeatIntervalCheckBox, "CCDBusTxMessageRepeatIntervalCheckBox");
		((Control)CCDBusTxMessageRepeatIntervalCheckBox).Name = "CCDBusTxMessageRepeatIntervalCheckBox";
		((ButtonBase)CCDBusTxMessageRepeatIntervalCheckBox).UseVisualStyleBackColor = true;
		CCDBusTxMessageRepeatIntervalCheckBox.CheckedChanged += CCDBusTxMessageRepeatIntervalCheckBox_CheckedChanged;
		componentResourceManager.ApplyResources(CCDBusSendMessagesButton, "CCDBusSendMessagesButton");
		((Control)CCDBusSendMessagesButton).Name = "CCDBusSendMessagesButton";
		((ButtonBase)CCDBusSendMessagesButton).UseVisualStyleBackColor = true;
		((Control)CCDBusSendMessagesButton).Click += CCDBusSendMessagesButton_Click;
		componentResourceManager.ApplyResources(CCDBusTxMessageChecksumCheckBox, "CCDBusTxMessageChecksumCheckBox");
		CCDBusTxMessageChecksumCheckBox.Checked = true;
		CCDBusTxMessageChecksumCheckBox.CheckState = (CheckState)1;
		((Control)CCDBusTxMessageChecksumCheckBox).Name = "CCDBusTxMessageChecksumCheckBox";
		((ButtonBase)CCDBusTxMessageChecksumCheckBox).UseVisualStyleBackColor = true;
		componentResourceManager.ApplyResources(CCDBusOverwriteDuplicateIDCheckBox, "CCDBusOverwriteDuplicateIDCheckBox");
		CCDBusOverwriteDuplicateIDCheckBox.Checked = true;
		CCDBusOverwriteDuplicateIDCheckBox.CheckState = (CheckState)1;
		((Control)CCDBusOverwriteDuplicateIDCheckBox).Name = "CCDBusOverwriteDuplicateIDCheckBox";
		((ButtonBase)CCDBusOverwriteDuplicateIDCheckBox).UseVisualStyleBackColor = true;
		componentResourceManager.ApplyResources(CCDBusTxMessageClearListButton, "CCDBusTxMessageClearListButton");
		((Control)CCDBusTxMessageClearListButton).Name = "CCDBusTxMessageClearListButton";
		((ButtonBase)CCDBusTxMessageClearListButton).UseVisualStyleBackColor = true;
		((Control)CCDBusTxMessageClearListButton).Click += CCDBusTxMessageClearListButton_Click;
		componentResourceManager.ApplyResources(CCDBusTxMessageRemoveItemButton, "CCDBusTxMessageRemoveItemButton");
		((Control)CCDBusTxMessageRemoveItemButton).Name = "CCDBusTxMessageRemoveItemButton";
		((ButtonBase)CCDBusTxMessageRemoveItemButton).UseVisualStyleBackColor = true;
		((Control)CCDBusTxMessageRemoveItemButton).Click += CCDBusTxMessageRemoveItemButton_Click;
		componentResourceManager.ApplyResources(CCDBusTxMessageAddButton, "CCDBusTxMessageAddButton");
		((Control)CCDBusTxMessageAddButton).Name = "CCDBusTxMessageAddButton";
		((ButtonBase)CCDBusTxMessageAddButton).UseVisualStyleBackColor = true;
		((Control)CCDBusTxMessageAddButton).Click += CCDBusTxMessageAddButton_Click;
		componentResourceManager.ApplyResources(CCDBusTxMessageComboBox, "CCDBusTxMessageComboBox");
		((ListControl)CCDBusTxMessageComboBox).FormattingEnabled = true;
		((Control)CCDBusTxMessageComboBox).Name = "CCDBusTxMessageComboBox";
		((Control)CCDBusTxMessageComboBox).KeyPress += new KeyPressEventHandler(CCDBusTxMessageComboBox_KeyPress);
		componentResourceManager.ApplyResources(CCDBusTxMessagesListBox, "CCDBusTxMessagesListBox");
		((ListControl)CCDBusTxMessagesListBox).FormattingEnabled = true;
		((Control)CCDBusTxMessagesListBox).Name = "CCDBusTxMessagesListBox";
		CCDBusTxMessagesListBox.SelectionMode = (SelectionMode)3;
		((Control)CCDBusTxMessagesListBox).DoubleClick += CCDBusTxMessagesListBox_DoubleClick;
		componentResourceManager.ApplyResources(DebugRandomCCDBusMessagesButton, "DebugRandomCCDBusMessagesButton");
		((Control)DebugRandomCCDBusMessagesButton).Name = "DebugRandomCCDBusMessagesButton";
		((ButtonBase)DebugRandomCCDBusMessagesButton).UseVisualStyleBackColor = true;
		((Control)DebugRandomCCDBusMessagesButton).Click += DebugRandomCCDBusMessagesButton_Click;
		((Control)PCIBusControlTabPage).BackColor = Color.Transparent;
		((Control)PCIBusControlTabPage).Controls.Add((Control)(object)PCIBusTransceiverOnOffCheckBox);
		((Control)PCIBusControlTabPage).Controls.Add((Control)(object)PCIBusStopRepeatedMessagesButton);
		((Control)PCIBusControlTabPage).Controls.Add((Control)(object)MillisecondsLabel06);
		((Control)PCIBusControlTabPage).Controls.Add((Control)(object)PCIBusTxMessageRepeatIntervalTextBox);
		((Control)PCIBusControlTabPage).Controls.Add((Control)(object)PCIBusTxMessageRepeatIntervalCheckBox);
		((Control)PCIBusControlTabPage).Controls.Add((Control)(object)PCIBusSendMessagesButton);
		((Control)PCIBusControlTabPage).Controls.Add((Control)(object)PCIBusTxMessageCRCCheckBox);
		((Control)PCIBusControlTabPage).Controls.Add((Control)(object)PCIBusOverwriteDuplicateIDCheckBox);
		((Control)PCIBusControlTabPage).Controls.Add((Control)(object)PCIBusTxMessageClearListButton);
		((Control)PCIBusControlTabPage).Controls.Add((Control)(object)PCIBusTxMessageRemoveItemButton);
		((Control)PCIBusControlTabPage).Controls.Add((Control)(object)PCIBusTxMessageAddButton);
		((Control)PCIBusControlTabPage).Controls.Add((Control)(object)PCIBusTxMessageComboBox);
		((Control)PCIBusControlTabPage).Controls.Add((Control)(object)PCIBusTxMessagesListBox);
		componentResourceManager.ApplyResources(PCIBusControlTabPage, "PCIBusControlTabPage");
		((Control)PCIBusControlTabPage).Name = "PCIBusControlTabPage";
		componentResourceManager.ApplyResources(PCIBusTransceiverOnOffCheckBox, "PCIBusTransceiverOnOffCheckBox");
		((Control)PCIBusTransceiverOnOffCheckBox).Name = "PCIBusTransceiverOnOffCheckBox";
		((ButtonBase)PCIBusTransceiverOnOffCheckBox).UseVisualStyleBackColor = true;
		PCIBusTransceiverOnOffCheckBox.CheckedChanged += PCIBusSettingsCheckBox_CheckedChanged;
		componentResourceManager.ApplyResources(PCIBusStopRepeatedMessagesButton, "PCIBusStopRepeatedMessagesButton");
		((Control)PCIBusStopRepeatedMessagesButton).Name = "PCIBusStopRepeatedMessagesButton";
		((ButtonBase)PCIBusStopRepeatedMessagesButton).UseVisualStyleBackColor = true;
		((Control)PCIBusStopRepeatedMessagesButton).Click += PCIBusStopRepeatedMessagesButton_Click;
		componentResourceManager.ApplyResources(MillisecondsLabel06, "MillisecondsLabel06");
		((Control)MillisecondsLabel06).Name = "MillisecondsLabel06";
		componentResourceManager.ApplyResources(PCIBusTxMessageRepeatIntervalTextBox, "PCIBusTxMessageRepeatIntervalTextBox");
		((Control)PCIBusTxMessageRepeatIntervalTextBox).Name = "PCIBusTxMessageRepeatIntervalTextBox";
		componentResourceManager.ApplyResources(PCIBusTxMessageRepeatIntervalCheckBox, "PCIBusTxMessageRepeatIntervalCheckBox");
		((Control)PCIBusTxMessageRepeatIntervalCheckBox).Name = "PCIBusTxMessageRepeatIntervalCheckBox";
		((ButtonBase)PCIBusTxMessageRepeatIntervalCheckBox).UseVisualStyleBackColor = true;
		PCIBusTxMessageRepeatIntervalCheckBox.CheckedChanged += PCIBusTxMessageRepeatIntervalCheckBox_CheckedChanged;
		componentResourceManager.ApplyResources(PCIBusSendMessagesButton, "PCIBusSendMessagesButton");
		((Control)PCIBusSendMessagesButton).Name = "PCIBusSendMessagesButton";
		((ButtonBase)PCIBusSendMessagesButton).UseVisualStyleBackColor = true;
		((Control)PCIBusSendMessagesButton).Click += PCIBusSendMessagesButton_Click;
		componentResourceManager.ApplyResources(PCIBusTxMessageCRCCheckBox, "PCIBusTxMessageCRCCheckBox");
		PCIBusTxMessageCRCCheckBox.Checked = true;
		PCIBusTxMessageCRCCheckBox.CheckState = (CheckState)1;
		((Control)PCIBusTxMessageCRCCheckBox).Name = "PCIBusTxMessageCRCCheckBox";
		((ButtonBase)PCIBusTxMessageCRCCheckBox).UseVisualStyleBackColor = true;
		componentResourceManager.ApplyResources(PCIBusOverwriteDuplicateIDCheckBox, "PCIBusOverwriteDuplicateIDCheckBox");
		PCIBusOverwriteDuplicateIDCheckBox.Checked = true;
		PCIBusOverwriteDuplicateIDCheckBox.CheckState = (CheckState)1;
		((Control)PCIBusOverwriteDuplicateIDCheckBox).Name = "PCIBusOverwriteDuplicateIDCheckBox";
		((ButtonBase)PCIBusOverwriteDuplicateIDCheckBox).UseVisualStyleBackColor = true;
		componentResourceManager.ApplyResources(PCIBusTxMessageClearListButton, "PCIBusTxMessageClearListButton");
		((Control)PCIBusTxMessageClearListButton).Name = "PCIBusTxMessageClearListButton";
		((ButtonBase)PCIBusTxMessageClearListButton).UseVisualStyleBackColor = true;
		((Control)PCIBusTxMessageClearListButton).Click += PCIBusTxMessageClearListButton_Click;
		componentResourceManager.ApplyResources(PCIBusTxMessageRemoveItemButton, "PCIBusTxMessageRemoveItemButton");
		((Control)PCIBusTxMessageRemoveItemButton).Name = "PCIBusTxMessageRemoveItemButton";
		((ButtonBase)PCIBusTxMessageRemoveItemButton).UseVisualStyleBackColor = true;
		((Control)PCIBusTxMessageRemoveItemButton).Click += PCIBusTxMessageRemoveItemButton_Click;
		componentResourceManager.ApplyResources(PCIBusTxMessageAddButton, "PCIBusTxMessageAddButton");
		((Control)PCIBusTxMessageAddButton).Name = "PCIBusTxMessageAddButton";
		((ButtonBase)PCIBusTxMessageAddButton).UseVisualStyleBackColor = true;
		((Control)PCIBusTxMessageAddButton).Click += PCIBusTxMessageAddButton_Click;
		componentResourceManager.ApplyResources(PCIBusTxMessageComboBox, "PCIBusTxMessageComboBox");
		((ListControl)PCIBusTxMessageComboBox).FormattingEnabled = true;
		((Control)PCIBusTxMessageComboBox).Name = "PCIBusTxMessageComboBox";
		((Control)PCIBusTxMessageComboBox).KeyPress += new KeyPressEventHandler(PCIBusTxMessageComboBox_KeyPress);
		componentResourceManager.ApplyResources(PCIBusTxMessagesListBox, "PCIBusTxMessagesListBox");
		((ListControl)PCIBusTxMessagesListBox).FormattingEnabled = true;
		((Control)PCIBusTxMessagesListBox).Name = "PCIBusTxMessagesListBox";
		PCIBusTxMessagesListBox.SelectionMode = (SelectionMode)3;
		((Control)PCIBusTxMessagesListBox).DoubleClick += PCIBusTxMessagesListBox_DoubleClick;
		((Control)SCIBusControlTabPage).BackColor = Color.Transparent;
		((Control)SCIBusControlTabPage).Controls.Add((Control)(object)SCIBusLogicComboBox);
		((Control)SCIBusControlTabPage).Controls.Add((Control)(object)SCIBusLogicLabel);
		((Control)SCIBusControlTabPage).Controls.Add((Control)(object)SCIBusOBDConfigurationComboBox);
		((Control)SCIBusControlTabPage).Controls.Add((Control)(object)SCIBusOBDConfigurationLabel);
		((Control)SCIBusControlTabPage).Controls.Add((Control)(object)SCIBusModuleConfigSpeedApplyButton);
		((Control)SCIBusControlTabPage).Controls.Add((Control)(object)SCIBusSpeedComboBox);
		((Control)SCIBusControlTabPage).Controls.Add((Control)(object)SCIBusSpeedLabel);
		((Control)SCIBusControlTabPage).Controls.Add((Control)(object)SCIBusModuleComboBox);
		((Control)SCIBusControlTabPage).Controls.Add((Control)(object)SCIBusModuleLabel);
		((Control)SCIBusControlTabPage).Controls.Add((Control)(object)SCIBusStopRepeatedMessagesButton);
		((Control)SCIBusControlTabPage).Controls.Add((Control)(object)MillisecondsLabel05);
		((Control)SCIBusControlTabPage).Controls.Add((Control)(object)SCIBusTxMessageRepeatIntervalTextBox);
		((Control)SCIBusControlTabPage).Controls.Add((Control)(object)SCIBusTxMessageRepeatIntervalCheckBox);
		((Control)SCIBusControlTabPage).Controls.Add((Control)(object)SCIBusSendMessagesButton);
		((Control)SCIBusControlTabPage).Controls.Add((Control)(object)SCIBusTxMessageChecksumCheckBox);
		((Control)SCIBusControlTabPage).Controls.Add((Control)(object)SCIBusOverwriteDuplicateIDCheckBox);
		((Control)SCIBusControlTabPage).Controls.Add((Control)(object)SCIBusTxMessageClearListButton);
		((Control)SCIBusControlTabPage).Controls.Add((Control)(object)SCIBusTxMessageRemoveItemButton);
		((Control)SCIBusControlTabPage).Controls.Add((Control)(object)SCIBusTxMessageAddButton);
		((Control)SCIBusControlTabPage).Controls.Add((Control)(object)SCIBusTxMessageComboBox);
		((Control)SCIBusControlTabPage).Controls.Add((Control)(object)SCIBusTxMessagesListBox);
		componentResourceManager.ApplyResources(SCIBusControlTabPage, "SCIBusControlTabPage");
		((Control)SCIBusControlTabPage).Name = "SCIBusControlTabPage";
		SCIBusLogicComboBox.DropDownStyle = (ComboBoxStyle)2;
		((ListControl)SCIBusLogicComboBox).FormattingEnabled = true;
		SCIBusLogicComboBox.Items.AddRange(new object[4]
		{
			componentResourceManager.GetString("SCIBusLogicComboBox.Items"),
			componentResourceManager.GetString("SCIBusLogicComboBox.Items1"),
			componentResourceManager.GetString("SCIBusLogicComboBox.Items2"),
			componentResourceManager.GetString("SCIBusLogicComboBox.Items3")
		});
		componentResourceManager.ApplyResources(SCIBusLogicComboBox, "SCIBusLogicComboBox");
		((Control)SCIBusLogicComboBox).Name = "SCIBusLogicComboBox";
		componentResourceManager.ApplyResources(SCIBusLogicLabel, "SCIBusLogicLabel");
		((Control)SCIBusLogicLabel).Name = "SCIBusLogicLabel";
		SCIBusOBDConfigurationComboBox.DropDownStyle = (ComboBoxStyle)2;
		((ListControl)SCIBusOBDConfigurationComboBox).FormattingEnabled = true;
		SCIBusOBDConfigurationComboBox.Items.AddRange(new object[2]
		{
			componentResourceManager.GetString("SCIBusOBDConfigurationComboBox.Items"),
			componentResourceManager.GetString("SCIBusOBDConfigurationComboBox.Items1")
		});
		componentResourceManager.ApplyResources(SCIBusOBDConfigurationComboBox, "SCIBusOBDConfigurationComboBox");
		((Control)SCIBusOBDConfigurationComboBox).Name = "SCIBusOBDConfigurationComboBox";
		componentResourceManager.ApplyResources(SCIBusOBDConfigurationLabel, "SCIBusOBDConfigurationLabel");
		((Control)SCIBusOBDConfigurationLabel).Name = "SCIBusOBDConfigurationLabel";
		componentResourceManager.ApplyResources(SCIBusModuleConfigSpeedApplyButton, "SCIBusModuleConfigSpeedApplyButton");
		((Control)SCIBusModuleConfigSpeedApplyButton).Name = "SCIBusModuleConfigSpeedApplyButton";
		((ButtonBase)SCIBusModuleConfigSpeedApplyButton).UseVisualStyleBackColor = true;
		((Control)SCIBusModuleConfigSpeedApplyButton).Click += SCIBusModuleConfigSpeedApplyButton_Click;
		SCIBusSpeedComboBox.DropDownStyle = (ComboBoxStyle)2;
		((ListControl)SCIBusSpeedComboBox).FormattingEnabled = true;
		SCIBusSpeedComboBox.Items.AddRange(new object[5]
		{
			componentResourceManager.GetString("SCIBusSpeedComboBox.Items"),
			componentResourceManager.GetString("SCIBusSpeedComboBox.Items1"),
			componentResourceManager.GetString("SCIBusSpeedComboBox.Items2"),
			componentResourceManager.GetString("SCIBusSpeedComboBox.Items3"),
			componentResourceManager.GetString("SCIBusSpeedComboBox.Items4")
		});
		componentResourceManager.ApplyResources(SCIBusSpeedComboBox, "SCIBusSpeedComboBox");
		((Control)SCIBusSpeedComboBox).Name = "SCIBusSpeedComboBox";
		componentResourceManager.ApplyResources(SCIBusSpeedLabel, "SCIBusSpeedLabel");
		((Control)SCIBusSpeedLabel).Name = "SCIBusSpeedLabel";
		SCIBusModuleComboBox.DropDownStyle = (ComboBoxStyle)2;
		((ListControl)SCIBusModuleComboBox).FormattingEnabled = true;
		SCIBusModuleComboBox.Items.AddRange(new object[2]
		{
			componentResourceManager.GetString("SCIBusModuleComboBox.Items"),
			componentResourceManager.GetString("SCIBusModuleComboBox.Items1")
		});
		componentResourceManager.ApplyResources(SCIBusModuleComboBox, "SCIBusModuleComboBox");
		((Control)SCIBusModuleComboBox).Name = "SCIBusModuleComboBox";
		SCIBusModuleComboBox.SelectedIndexChanged += SCIBusModuleComboBox_SelectedIndexChanged;
		componentResourceManager.ApplyResources(SCIBusModuleLabel, "SCIBusModuleLabel");
		((Control)SCIBusModuleLabel).Name = "SCIBusModuleLabel";
		componentResourceManager.ApplyResources(SCIBusStopRepeatedMessagesButton, "SCIBusStopRepeatedMessagesButton");
		((Control)SCIBusStopRepeatedMessagesButton).Name = "SCIBusStopRepeatedMessagesButton";
		((ButtonBase)SCIBusStopRepeatedMessagesButton).UseVisualStyleBackColor = true;
		((Control)SCIBusStopRepeatedMessagesButton).Click += SCIBusStopRepeatedMessagesButton_Click;
		componentResourceManager.ApplyResources(MillisecondsLabel05, "MillisecondsLabel05");
		((Control)MillisecondsLabel05).Name = "MillisecondsLabel05";
		componentResourceManager.ApplyResources(SCIBusTxMessageRepeatIntervalTextBox, "SCIBusTxMessageRepeatIntervalTextBox");
		((Control)SCIBusTxMessageRepeatIntervalTextBox).Name = "SCIBusTxMessageRepeatIntervalTextBox";
		componentResourceManager.ApplyResources(SCIBusTxMessageRepeatIntervalCheckBox, "SCIBusTxMessageRepeatIntervalCheckBox");
		((Control)SCIBusTxMessageRepeatIntervalCheckBox).Name = "SCIBusTxMessageRepeatIntervalCheckBox";
		((ButtonBase)SCIBusTxMessageRepeatIntervalCheckBox).UseVisualStyleBackColor = true;
		SCIBusTxMessageRepeatIntervalCheckBox.CheckedChanged += SCIBusTxMessageRepeatIntervalCheckBox_CheckedChanged;
		componentResourceManager.ApplyResources(SCIBusSendMessagesButton, "SCIBusSendMessagesButton");
		((Control)SCIBusSendMessagesButton).Name = "SCIBusSendMessagesButton";
		((ButtonBase)SCIBusSendMessagesButton).UseVisualStyleBackColor = true;
		((Control)SCIBusSendMessagesButton).Click += SCIBusSendMessagesButton_Click;
		componentResourceManager.ApplyResources(SCIBusTxMessageChecksumCheckBox, "SCIBusTxMessageChecksumCheckBox");
		SCIBusTxMessageChecksumCheckBox.Checked = true;
		SCIBusTxMessageChecksumCheckBox.CheckState = (CheckState)1;
		((Control)SCIBusTxMessageChecksumCheckBox).Name = "SCIBusTxMessageChecksumCheckBox";
		((ButtonBase)SCIBusTxMessageChecksumCheckBox).UseVisualStyleBackColor = true;
		componentResourceManager.ApplyResources(SCIBusOverwriteDuplicateIDCheckBox, "SCIBusOverwriteDuplicateIDCheckBox");
		((Control)SCIBusOverwriteDuplicateIDCheckBox).Name = "SCIBusOverwriteDuplicateIDCheckBox";
		((ButtonBase)SCIBusOverwriteDuplicateIDCheckBox).UseVisualStyleBackColor = true;
		componentResourceManager.ApplyResources(SCIBusTxMessageClearListButton, "SCIBusTxMessageClearListButton");
		((Control)SCIBusTxMessageClearListButton).Name = "SCIBusTxMessageClearListButton";
		((ButtonBase)SCIBusTxMessageClearListButton).UseVisualStyleBackColor = true;
		((Control)SCIBusTxMessageClearListButton).Click += SCIBusTxMessageClearListButton_Click;
		componentResourceManager.ApplyResources(SCIBusTxMessageRemoveItemButton, "SCIBusTxMessageRemoveItemButton");
		((Control)SCIBusTxMessageRemoveItemButton).Name = "SCIBusTxMessageRemoveItemButton";
		((ButtonBase)SCIBusTxMessageRemoveItemButton).UseVisualStyleBackColor = true;
		((Control)SCIBusTxMessageRemoveItemButton).Click += SCIBusTxMessageRemoveItemButton_Click;
		componentResourceManager.ApplyResources(SCIBusTxMessageAddButton, "SCIBusTxMessageAddButton");
		((Control)SCIBusTxMessageAddButton).Name = "SCIBusTxMessageAddButton";
		((ButtonBase)SCIBusTxMessageAddButton).UseVisualStyleBackColor = true;
		((Control)SCIBusTxMessageAddButton).Click += SCIBusTxMessageAddButton_Click;
		componentResourceManager.ApplyResources(SCIBusTxMessageComboBox, "SCIBusTxMessageComboBox");
		((ListControl)SCIBusTxMessageComboBox).FormattingEnabled = true;
		((Control)SCIBusTxMessageComboBox).Name = "SCIBusTxMessageComboBox";
		((Control)SCIBusTxMessageComboBox).KeyPress += new KeyPressEventHandler(SCIBusTxMessageComboBox_KeyPress);
		componentResourceManager.ApplyResources(SCIBusTxMessagesListBox, "SCIBusTxMessagesListBox");
		((ListControl)SCIBusTxMessagesListBox).FormattingEnabled = true;
		((Control)SCIBusTxMessagesListBox).Name = "SCIBusTxMessagesListBox";
		SCIBusTxMessagesListBox.SelectionMode = (SelectionMode)3;
		((Control)SCIBusTxMessagesListBox).DoubleClick += SCIBusTxMessagesListBox_DoubleClick;
		((Control)LCDControlTabPage).BackColor = Color.Transparent;
		((Control)LCDControlTabPage).Controls.Add((Control)(object)LCDI2CAddressHexLabel);
		((Control)LCDControlTabPage).Controls.Add((Control)(object)LCDI2CAddressTextBox);
		((Control)LCDControlTabPage).Controls.Add((Control)(object)LCDI2CAddressLabel);
		((Control)LCDControlTabPage).Controls.Add((Control)(object)LCDPreviewLabel);
		((Control)LCDControlTabPage).Controls.Add((Control)(object)LCDDataSourceComboBox);
		((Control)LCDControlTabPage).Controls.Add((Control)(object)LCDDataSourceLabel);
		((Control)LCDControlTabPage).Controls.Add((Control)(object)LCDStateComboBox);
		((Control)LCDControlTabPage).Controls.Add((Control)(object)LCDStateLabel);
		((Control)LCDControlTabPage).Controls.Add((Control)(object)LCDRowLabel);
		((Control)LCDControlTabPage).Controls.Add((Control)(object)LCDHeightTextBox);
		((Control)LCDControlTabPage).Controls.Add((Control)(object)LCDColumnLabel);
		((Control)LCDControlTabPage).Controls.Add((Control)(object)LCDWidthTextBox);
		((Control)LCDControlTabPage).Controls.Add((Control)(object)LCDSizeLabel);
		((Control)LCDControlTabPage).Controls.Add((Control)(object)LCDApplySettingsButton);
		((Control)LCDControlTabPage).Controls.Add((Control)(object)LCDRefreshRateLabel);
		((Control)LCDControlTabPage).Controls.Add((Control)(object)HzLabel01);
		((Control)LCDControlTabPage).Controls.Add((Control)(object)LCDRefreshRateTextBox);
		((Control)LCDControlTabPage).Controls.Add((Control)(object)LCDPreviewTextBox);
		componentResourceManager.ApplyResources(LCDControlTabPage, "LCDControlTabPage");
		((Control)LCDControlTabPage).Name = "LCDControlTabPage";
		componentResourceManager.ApplyResources(LCDI2CAddressHexLabel, "LCDI2CAddressHexLabel");
		((Control)LCDI2CAddressHexLabel).Name = "LCDI2CAddressHexLabel";
		componentResourceManager.ApplyResources(LCDI2CAddressTextBox, "LCDI2CAddressTextBox");
		((Control)LCDI2CAddressTextBox).Name = "LCDI2CAddressTextBox";
		componentResourceManager.ApplyResources(LCDI2CAddressLabel, "LCDI2CAddressLabel");
		((Control)LCDI2CAddressLabel).Name = "LCDI2CAddressLabel";
		componentResourceManager.ApplyResources(LCDPreviewLabel, "LCDPreviewLabel");
		((Control)LCDPreviewLabel).Name = "LCDPreviewLabel";
		LCDDataSourceComboBox.DropDownStyle = (ComboBoxStyle)2;
		((ListControl)LCDDataSourceComboBox).FormattingEnabled = true;
		LCDDataSourceComboBox.Items.AddRange(new object[3]
		{
			componentResourceManager.GetString("LCDDataSourceComboBox.Items"),
			componentResourceManager.GetString("LCDDataSourceComboBox.Items1"),
			componentResourceManager.GetString("LCDDataSourceComboBox.Items2")
		});
		componentResourceManager.ApplyResources(LCDDataSourceComboBox, "LCDDataSourceComboBox");
		((Control)LCDDataSourceComboBox).Name = "LCDDataSourceComboBox";
		LCDDataSourceComboBox.SelectedIndexChanged += LCDDataSourceComboBox_SelectedIndexChanged;
		componentResourceManager.ApplyResources(LCDDataSourceLabel, "LCDDataSourceLabel");
		((Control)LCDDataSourceLabel).Name = "LCDDataSourceLabel";
		LCDStateComboBox.DropDownStyle = (ComboBoxStyle)2;
		((ListControl)LCDStateComboBox).FormattingEnabled = true;
		LCDStateComboBox.Items.AddRange(new object[2]
		{
			componentResourceManager.GetString("LCDStateComboBox.Items"),
			componentResourceManager.GetString("LCDStateComboBox.Items1")
		});
		componentResourceManager.ApplyResources(LCDStateComboBox, "LCDStateComboBox");
		((Control)LCDStateComboBox).Name = "LCDStateComboBox";
		componentResourceManager.ApplyResources(LCDStateLabel, "LCDStateLabel");
		((Control)LCDStateLabel).Name = "LCDStateLabel";
		componentResourceManager.ApplyResources(LCDRowLabel, "LCDRowLabel");
		((Control)LCDRowLabel).Name = "LCDRowLabel";
		componentResourceManager.ApplyResources(LCDHeightTextBox, "LCDHeightTextBox");
		((Control)LCDHeightTextBox).Name = "LCDHeightTextBox";
		componentResourceManager.ApplyResources(LCDColumnLabel, "LCDColumnLabel");
		((Control)LCDColumnLabel).Name = "LCDColumnLabel";
		componentResourceManager.ApplyResources(LCDWidthTextBox, "LCDWidthTextBox");
		((Control)LCDWidthTextBox).Name = "LCDWidthTextBox";
		componentResourceManager.ApplyResources(LCDSizeLabel, "LCDSizeLabel");
		((Control)LCDSizeLabel).Name = "LCDSizeLabel";
		componentResourceManager.ApplyResources(LCDApplySettingsButton, "LCDApplySettingsButton");
		((Control)LCDApplySettingsButton).Name = "LCDApplySettingsButton";
		((ButtonBase)LCDApplySettingsButton).UseVisualStyleBackColor = true;
		((Control)LCDApplySettingsButton).Click += LCDApplySettingsButton_Click;
		componentResourceManager.ApplyResources(LCDRefreshRateLabel, "LCDRefreshRateLabel");
		((Control)LCDRefreshRateLabel).Name = "LCDRefreshRateLabel";
		componentResourceManager.ApplyResources(HzLabel01, "HzLabel01");
		((Control)HzLabel01).Name = "HzLabel01";
		componentResourceManager.ApplyResources(LCDRefreshRateTextBox, "LCDRefreshRateTextBox");
		((Control)LCDRefreshRateTextBox).Name = "LCDRefreshRateTextBox";
		((Control)LCDPreviewTextBox).BackColor = Color.YellowGreen;
		componentResourceManager.ApplyResources(LCDPreviewTextBox, "LCDPreviewTextBox");
		((Control)LCDPreviewTextBox).Name = "LCDPreviewTextBox";
		((TextBoxBase)LCDPreviewTextBox).ReadOnly = true;
		componentResourceManager.ApplyResources(COMPortsRefreshButton, "COMPortsRefreshButton");
		((Control)COMPortsRefreshButton).Name = "COMPortsRefreshButton";
		((ButtonBase)COMPortsRefreshButton).UseVisualStyleBackColor = true;
		((Control)COMPortsRefreshButton).Click += COMPortsRefreshButton_Click;
		COMPortsComboBox.DropDownStyle = (ComboBoxStyle)2;
		componentResourceManager.ApplyResources(COMPortsComboBox, "COMPortsComboBox");
		((ListControl)COMPortsComboBox).FormattingEnabled = true;
		((Control)COMPortsComboBox).Name = "COMPortsComboBox";
		COMPortsComboBox.SelectedIndexChanged += COMPortsComboBox_SelectedIndexChanged;
		componentResourceManager.ApplyResources(ConnectButton, "ConnectButton");
		((Control)ConnectButton).Name = "ConnectButton";
		((ButtonBase)ConnectButton).UseVisualStyleBackColor = true;
		((Control)ConnectButton).Click += ConnectButton_Click;
		((Control)DiagnosticsGroupBox).Controls.Add((Control)(object)DiagnosticsSnapshotButton);
		((Control)DiagnosticsGroupBox).Controls.Add((Control)(object)DiagnosticsRefreshButton);
		((Control)DiagnosticsGroupBox).Controls.Add((Control)(object)DiagnosticsCopyToClipboardButton);
		((Control)DiagnosticsGroupBox).Controls.Add((Control)(object)DiagnosticsResetViewButton);
		((Control)DiagnosticsGroupBox).Controls.Add((Control)(object)DiagnosticsTabControl);
		componentResourceManager.ApplyResources(DiagnosticsGroupBox, "DiagnosticsGroupBox");
		((Control)DiagnosticsGroupBox).Name = "DiagnosticsGroupBox";
		DiagnosticsGroupBox.TabStop = false;
		componentResourceManager.ApplyResources(DiagnosticsSnapshotButton, "DiagnosticsSnapshotButton");
		((Control)DiagnosticsSnapshotButton).Name = "DiagnosticsSnapshotButton";
		((ButtonBase)DiagnosticsSnapshotButton).UseVisualStyleBackColor = true;
		((Control)DiagnosticsSnapshotButton).Click += DiagnosticsSnapshotButton_Click;
		componentResourceManager.ApplyResources(DiagnosticsRefreshButton, "DiagnosticsRefreshButton");
		((Control)DiagnosticsRefreshButton).Name = "DiagnosticsRefreshButton";
		((ButtonBase)DiagnosticsRefreshButton).UseVisualStyleBackColor = true;
		((Control)DiagnosticsRefreshButton).Click += DiagnosticsRefreshButton_Click;
		componentResourceManager.ApplyResources(DiagnosticsCopyToClipboardButton, "DiagnosticsCopyToClipboardButton");
		((Control)DiagnosticsCopyToClipboardButton).Name = "DiagnosticsCopyToClipboardButton";
		((ButtonBase)DiagnosticsCopyToClipboardButton).UseVisualStyleBackColor = true;
		((Control)DiagnosticsCopyToClipboardButton).Click += DiagnosticsCopyToClipboardButton_Click;
		componentResourceManager.ApplyResources(DiagnosticsResetViewButton, "DiagnosticsResetViewButton");
		((Control)DiagnosticsResetViewButton).Name = "DiagnosticsResetViewButton";
		((ButtonBase)DiagnosticsResetViewButton).UseVisualStyleBackColor = true;
		((Control)DiagnosticsResetViewButton).Click += DiagnosticsResetViewButton_Click;
		((Control)DiagnosticsTabControl).Controls.Add((Control)(object)CCDBusDiagnosticsTabPage);
		((Control)DiagnosticsTabControl).Controls.Add((Control)(object)PCIBusDiagnosticsTabPage);
		((Control)DiagnosticsTabControl).Controls.Add((Control)(object)SCIBusPCMDiagnosticsTabPage);
		((Control)DiagnosticsTabControl).Controls.Add((Control)(object)SCIBusTCMDiagnosticsTabPage);
		componentResourceManager.ApplyResources(DiagnosticsTabControl, "DiagnosticsTabControl");
		((Control)DiagnosticsTabControl).Name = "DiagnosticsTabControl";
		DiagnosticsTabControl.SelectedIndex = 0;
		((Control)CCDBusDiagnosticsTabPage).BackColor = Color.Transparent;
		((Control)CCDBusDiagnosticsTabPage).Controls.Add((Control)(object)CCDBusDiagnosticsListBox);
		componentResourceManager.ApplyResources(CCDBusDiagnosticsTabPage, "CCDBusDiagnosticsTabPage");
		((Control)CCDBusDiagnosticsTabPage).Name = "CCDBusDiagnosticsTabPage";
		((ListBox)CCDBusDiagnosticsListBox).DrawMode = (DrawMode)1;
		componentResourceManager.ApplyResources(CCDBusDiagnosticsListBox, "CCDBusDiagnosticsListBox");
		((Control)CCDBusDiagnosticsListBox).Name = "CCDBusDiagnosticsListBox";
		((Control)PCIBusDiagnosticsTabPage).BackColor = Color.Transparent;
		((Control)PCIBusDiagnosticsTabPage).Controls.Add((Control)(object)PCIBusDiagnosticsListBox);
		componentResourceManager.ApplyResources(PCIBusDiagnosticsTabPage, "PCIBusDiagnosticsTabPage");
		((Control)PCIBusDiagnosticsTabPage).Name = "PCIBusDiagnosticsTabPage";
		((ListBox)PCIBusDiagnosticsListBox).DrawMode = (DrawMode)1;
		componentResourceManager.ApplyResources(PCIBusDiagnosticsListBox, "PCIBusDiagnosticsListBox");
		((Control)PCIBusDiagnosticsListBox).Name = "PCIBusDiagnosticsListBox";
		((Control)SCIBusPCMDiagnosticsTabPage).BackColor = Color.Transparent;
		((Control)SCIBusPCMDiagnosticsTabPage).Controls.Add((Control)(object)SCIBusPCMDiagnosticsListBox);
		componentResourceManager.ApplyResources(SCIBusPCMDiagnosticsTabPage, "SCIBusPCMDiagnosticsTabPage");
		((Control)SCIBusPCMDiagnosticsTabPage).Name = "SCIBusPCMDiagnosticsTabPage";
		((ListBox)SCIBusPCMDiagnosticsListBox).DrawMode = (DrawMode)1;
		componentResourceManager.ApplyResources(SCIBusPCMDiagnosticsListBox, "SCIBusPCMDiagnosticsListBox");
		((Control)SCIBusPCMDiagnosticsListBox).Name = "SCIBusPCMDiagnosticsListBox";
		((ListBox)SCIBusPCMDiagnosticsListBox).SelectionMode = (SelectionMode)0;
		((Control)SCIBusTCMDiagnosticsTabPage).BackColor = Color.Transparent;
		((Control)SCIBusTCMDiagnosticsTabPage).Controls.Add((Control)(object)SCIBusTCMDiagnosticsListBox);
		componentResourceManager.ApplyResources(SCIBusTCMDiagnosticsTabPage, "SCIBusTCMDiagnosticsTabPage");
		((Control)SCIBusTCMDiagnosticsTabPage).Name = "SCIBusTCMDiagnosticsTabPage";
		((ListBox)SCIBusTCMDiagnosticsListBox).DrawMode = (DrawMode)1;
		componentResourceManager.ApplyResources(SCIBusTCMDiagnosticsListBox, "SCIBusTCMDiagnosticsListBox");
		((Control)SCIBusTCMDiagnosticsListBox).Name = "SCIBusTCMDiagnosticsListBox";
		((ListBox)SCIBusTCMDiagnosticsListBox).SelectionMode = (SelectionMode)0;
		componentResourceManager.ApplyResources(this, "$this");
		((ContainerControl)this).AutoScaleMode = (AutoScaleMode)2;
		((Control)this).Controls.Add((Control)(object)DiagnosticsGroupBox);
		((Control)this).Controls.Add((Control)(object)ControlPanelGroupBox);
		((Control)this).Controls.Add((Control)(object)USBCommunicationGroupBox);
		((Control)this).Controls.Add((Control)(object)MenuStrip);
		((Control)this).DoubleBuffered = true;
		((Form)this).KeyPreview = true;
		((Form)this).MainMenuStrip = MenuStrip;
		((Control)this).Name = "MainForm";
		((Form)this).FormClosing += new FormClosingEventHandler(MainForm_FormClosing);
		((Form)this).Load += MainForm_Load;
		((Control)this).KeyDown += new KeyEventHandler(MainForm_KeyDown);
		((Control)MenuStrip).ResumeLayout(false);
		((Control)MenuStrip).PerformLayout();
		((Control)USBCommunicationGroupBox).ResumeLayout(false);
		((Control)USBCommunicationGroupBox).PerformLayout();
		((Control)ControlPanelGroupBox).ResumeLayout(false);
		((Control)ScannerTabControl).ResumeLayout(false);
		((Control)ScannerControlTabPage).ResumeLayout(false);
		((Control)ScannerControlTabPage).PerformLayout();
		((Control)CCDBusControlTabPage).ResumeLayout(false);
		((Control)CCDBusControlTabPage).PerformLayout();
		((Control)PCIBusControlTabPage).ResumeLayout(false);
		((Control)PCIBusControlTabPage).PerformLayout();
		((Control)SCIBusControlTabPage).ResumeLayout(false);
		((Control)SCIBusControlTabPage).PerformLayout();
		((Control)LCDControlTabPage).ResumeLayout(false);
		((Control)LCDControlTabPage).PerformLayout();
		((Control)DiagnosticsGroupBox).ResumeLayout(false);
		((Control)DiagnosticsTabControl).ResumeLayout(false);
		((Control)CCDBusDiagnosticsTabPage).ResumeLayout(false);
		((Control)PCIBusDiagnosticsTabPage).ResumeLayout(false);
		((Control)SCIBusPCMDiagnosticsTabPage).ResumeLayout(false);
		((Control)SCIBusTCMDiagnosticsTabPage).ResumeLayout(false);
		((Control)this).ResumeLayout(false);
		((Control)this).PerformLayout();
	}
}
