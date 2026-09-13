using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using ChryslerScanner.Helpers;
using ChryslerScanner.Models;
using ChryslerScanner.Services;

namespace ChryslerScanner;

public class BootstrapToolsForm : Form
{
	private enum SCI_ID
	{
		BreakCharacter = 0,
		SetBootstrapBaudrate = 6,
		UploadWorkerResult = 17,
		StartWorker = 33,
		ExitWorker = 34,
		BootstrapSeedKeyRequest = 36,
		BootstrapSeedKeyResponse = 38,
		FlashBlockWrite = 49,
		FlashBlockRead = 52,
		EEPROMBlockWrite = 55,
		EEPROMBlockRead = 58,
		StartBootloader = 71,
		UploadBootloader = 76,
		BootstrapModeNotProtected = 219
	}

	private enum Bootloader
	{
		Empty,
		SBEC2,
		SBEC3_3PLUS,
		SBEC3A_3APLUS_3B,
		EATX3_3A,
		EATX3B,
		EATX4_4A,
		JTEC,
		JTECPLUS
	}

	private enum Bootworker
	{
		Empty,
		PartNumberRead,
		FlashID,
		FlashRead,
		FlashErase,
		FlashWrite,
		ReturnFlashChecksum,
		EEPROMRead,
		EEPROMWrite,
		MemoryRipper,
		Read68HC11K,
		EEPROMErase
	}

	private enum FlashMemoryManufacturer
	{
		AMD = 1,
		ATMEL = 31,
		ST = 32,
		CATALYST = 49,
		INTEL = 137,
		TI = 151,
		TOSHIBA = 152,
		SST = 191
	}

	private enum FlashMemoryChip
	{
		M28F512 = 2,
		AM28F512 = 37,
		TC97208 = 64,
		M28F102 = 80,
		CAT28F102 = 81,
		TC97209 = 112,
		M28F200T = 116,
		M28F200B = 117,
		_87C257 = 128,
		AM28F256 = 161,
		_27SF256 = 163,
		_27SF512 = 164,
		_27SF010 = 165,
		_27SF020 = 166,
		M28F256 = 168,
		N28F010 = 180,
		P28F512 = 184,
		P28F256 = 185,
		N28F020 = 189,
		_29C256 = 220,
		M28F210 = 224,
		TMS28F210 = 229,
		M28F220 = 230
	}

	private enum BootloaderResult
	{
		OK,
		SetBaudrateTimeout,
		SetBaudrateError,
		SeedTimeout,
		SeedError,
		SeedChecksum,
		KeyTimeout,
		KeyError,
		KeyChecksum,
		InvalidKey,
		BootloaderNotSupported,
		BootloaderUploadError,
		BootloaderUploadTimeout,
		BootloaderStartError,
		BootloaderStartTimeout,
		BootloaderStartFailed
	}

	private enum BootworkerResult
	{
		OK,
		InvalidWorker,
		NoResponse,
		HandshakeError,
		UploadError,
		UploadTimeout,
		UploadInterrupted,
		UploadStatusError
	}

	private enum AutoBootResult
	{
		OK,
		FlashFileMissing,
		FlashFileSizeMismatch,
		StorageFull,
		ScannerDisconnected,
		BatteryVoltsLow,
		BootstrapVoltsLow,
		ProgrammingVoltsLow,
		StorageFileReadError,
		FlashReadError,
		FlashEraseError,
		FlashWriteError,
		FlashChecksumMismatch,
		EepromFileMissing,
		EepromFileSizeMismatch,
		EepromReadError,
		EepromWriteError,
		InvalidBootloader,
		UploadTaskTimeout,
		StartTaskTimeout,
		ExitTaskTimeout,
		TaskQueueFull,
		TaskCancelled,
		EepromEraseError,
		FlashClearError,
		UnauthorizedHardware,
		ReadProtected,
		DecryptionError,
		AdminAuthFailed,
		AdminAuthLockedOut
	}

	private enum AutoBootStatus
	{
		VerifyVoltages = 128,
		VoltagesOk,
		UploadPNWorker,
		StartPNWorker,
		PartNumber,
		FlashBackupFilename,
		UploadFlashIDWorker,
		StartFlashIDWorker,
		FlashChipIDs,
		UploadFlashReadWorker,
		StartFlashReadWorker,
		ExitFlashReadWorker,
		UploadFlashEraseWorker,
		StartFlashEraseWorker,
		FlashEraseResult,
		UploadFlashWriteWorker,
		StartFlashWriteWorker,
		ExitFlashWriteWorker,
		EEPROMBackupFilename,
		UploadEEPROMWriteWorker,
		StartEEPROMWriteWorker,
		ExitEEPROMWriteWorker,
		UploadEEPROMReadWorker,
		StartEEPROMReadWorker,
		ExitEEPROMReadWorker,
		UploadReturnChecksumWorker,
		StartReturnChecksumWorker,
		ChecksumResult,
		UploadMemoryRipperWorker,
		StartMemoryRipperWorker,
		ExitMemoryRipperWorker,
		UploadRead68HC11KWorker,
		StartRead68HC11Worker,
		ExitRead68HC11Worker,
		UploadEepromEraseWorker,
		StartEepromEraseWorker,
		EepromEraseResult,
		UploadFlashClearWorker,
		StartFlashClearWorker,
		FlashClearResult,
		UploadBootloader,
		CheckReadProtection
	}

	private enum FileUploadType
	{
		UploadFlashFile = 1,
		UploadEEPROMFile
	}

	private readonly MainForm OriginalForm;

	private readonly SerialService SerialService;

	private readonly SynchronizationContext UIContext;

	private string SCIBusBootstrapLogFilename;

	private string SCIBusFlashReadFilePath;

	private string SCIBusEEPROMReadFilePath;

	private string FlashFileName = string.Empty;

	private byte[] FlashFileBuffer;

	private string EEPROMFileName = string.Empty;

	private byte[] EEPROMFileBuffer;

	private const double MinBattVolts = 11.5;

	private const double MinBootVolts = 11.5;

	private const double MinProgVolts = 19.0;

	private const int FlashReadBlockSize = 512;

	private const int FlashWriteBlockSize = 512;

	private const int EEPROMReadBlockSize = 128;

	private const int EEPROMWriteBlockSize = 128;

	private int FlashChipSize;

	private int EEPROMSize;

	private byte FlashChecksum;

	private bool SwitchBackToLSWhenExit;

	private int FileUploadPending;

	private bool SessionRunning;

	private BackgroundWorker FileUploadWorker = new BackgroundWorker();

	private TaskCompletionSource<Packet> PacketReceivedTask;

	private IContainer components;

	private GroupBox BootModeGroupBox;

	private ComboBox BootloaderComboBox;

	private Label BootloaderLabel;

	private Button BootstrapButton;

	private GroupBox WorkerGroupBox;

	private ComboBox WorkerComboBox;

	private Label TaskLabel;

	private Button UploadButton;

	private Label FlashChipManufacturerLabel;

	private Button FlashChipDetectButton;

	private TextBox SCIBusBootstrapInfoTextBox;

	private GroupBox FlashMemoryGroupBox;

	private Button ExitButton;

	private Button FlashStopButton;

	private Button FlashWriteButton;

	private Button FlashBrowseButton;

	private Label FlashFileNameLabel;

	private Label FlashFileLabel;

	private Button StartButton;

	private GroupBox EEPROMGroupBox;

	private Button EEPROMStopButton;

	private Button EEPROMWriteButton;

	private Button EEPROMBrowseButton;

	private Label EEPROMFileNameLabel;

	private Label EEPROMFileLabel;

	private Label SCIBusBootstrapToolsProgressLabel;

	private Button SCIBusBootstrapToolsHelpButton;

	private Button FlashReadButton;

	private Button EEPROMReadButton;

	private CheckBox FlashMemoryBackupCheckBox;

	private Label FlashChipIDLabel;

	private Label EEPROMSizeLabel;

	private Button FormatStorageButton;

	public BootstrapToolsForm(MainForm IncomingForm, SerialService service)
	{
		OriginalForm = IncomingForm;
		InitializeComponent();
		UIContext = SynchronizationContext.Current;
		((Form)this).Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
		SerialService = service;
		SerialService.PacketReceived += PacketReceivedHandler;
		OriginalForm.ChangeLanguage();
		((ListControl)BootloaderComboBox).SelectedIndex = 2;
		((ListControl)WorkerComboBox).SelectedIndex = 0;
		SCIBusBootstrapLogFilename = "LOG/SCI/scibootstraplog_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".txt";
		FileUploadWorker.WorkerReportsProgress = true;
		FileUploadWorker.WorkerSupportsCancellation = true;
		FileUploadWorker.DoWork += FileUpload_DoWork;
		FileUploadWorker.ProgressChanged += FileUpload_ProgressChanged;
		FileUploadWorker.RunWorkerCompleted += FileUpload_RunWorkerCompleted;
		((ContainerControl)this).ActiveControl = (Control)(object)BootloaderComboBox;
	}

	private void BootstrapToolsForm_Load(object sender, EventArgs e)
	{
		UpdateTextBox(SCIBusBootstrapInfoTextBox, "Turn off CCD/PCI-bus transceivers!" + Environment.NewLine + "Begin by bootstrapping ECU with selected bootloader.");
	}

	private void BootstrapButton_Click(object sender, EventArgs e)
	{
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Invalid comparison between Unknown and I4
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0191: Unknown result type (might be due to invalid IL or missing references)
		//IL_0197: Invalid comparison between Unknown and I4
		if (((ListControl)BootloaderComboBox).SelectedIndex < 2)
		{
			MessageBox.Show("OBD1 bootloaders are not supported yet.", "Information", (MessageBoxButtons)0, (MessageBoxIcon)64);
			return;
		}
		string speed = OriginalForm.PCM.speed;
		UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + Environment.NewLine + "Turn key to OFF/LOCKED position.");
		if ((int)MessageBox.Show("Turn key to OFF/LOCKED position." + Environment.NewLine + "Wait at least 10 seconds afterwards." + Environment.NewLine + "Click OK when done." + Environment.NewLine + Environment.NewLine + "Try again and wait more if bootstrap fails.", "Information", (MessageBoxButtons)1, (MessageBoxIcon)64, (MessageBoxDefaultButton)256) == 1)
		{
			if (OriginalForm.PCM.speed != "62500 baud")
			{
				UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + "Scanner SCI-bus speed is set to 62500 baud.");
				OriginalForm.SelectSCIBusHSMode();
			}
			Packet packet = new Packet();
			packet.Bus = 0;
			packet.Command = 3;
			packet.Mode = 7;
			packet.Payload = new byte[2] { 46, 224 };
			OriginalForm.TransmitUSBPacket("[<-TX] Apply VBB to SCI-TX pin:", packet);
			SerialService.WritePacket(packet);
			if ((int)MessageBox.Show("Turn key to RUN position." + Environment.NewLine + "Do not start the engine." + Environment.NewLine + Environment.NewLine + "Click OK when done.", "Information", (MessageBoxButtons)1, (MessageBoxIcon)64, (MessageBoxDefaultButton)256) == 1)
			{
				packet = new Packet();
				packet.Bus = 0;
				packet.Command = 3;
				packet.Mode = 7;
				packet.Payload = new byte[2];
				OriginalForm.TransmitUSBPacket("[<-TX] Remove VBB from SCI-TX pin:", packet);
				SerialService.WritePacket(packet);
				byte b = 0;
				packet = new Packet();
				packet.Bus = 0;
				packet.Command = 13;
				packet.Mode = 5;
				packet.Payload = new byte[2]
				{
					(byte)((ListControl)BootloaderComboBox).SelectedIndex,
					b
				};
				OriginalForm.TransmitUSBPacket("[<-TX] Enter bootstrap mode:", packet);
				SerialService.WritePacket(packet);
				switch ((byte)((ListControl)BootloaderComboBox).SelectedIndex)
				{
				case 2:
					OriginalForm.UpdateUSBTextBox("[INFO] Bootloader: SBEC3/SBEC3+.");
					break;
				case 3:
					OriginalForm.UpdateUSBTextBox("[INFO] Bootloader: SBEC3A/3A+/3B.");
					break;
				case 4:
					OriginalForm.UpdateUSBTextBox("[INFO] Bootloader: EATX3/3A.");
					break;
				case 5:
					OriginalForm.UpdateUSBTextBox("[INFO] Bootloader: EATX3B.");
					break;
				case 6:
					OriginalForm.UpdateUSBTextBox("[INFO] Bootloader: EATX4/4A.");
					break;
				case 7:
					OriginalForm.UpdateUSBTextBox("[INFO] Bootloader: JTEC.");
					break;
				case 8:
					OriginalForm.UpdateUSBTextBox("[INFO] Bootloader: JTEC+.");
					break;
				default:
					OriginalForm.UpdateUSBTextBox("[INFO] Bootloader: unknown.");
					break;
				}
				SwitchBackToLSWhenExit = true;
			}
			else
			{
				packet = new Packet();
				packet.Bus = 0;
				packet.Command = 3;
				packet.Mode = 7;
				packet.Payload = new byte[2];
				OriginalForm.TransmitUSBPacket("[<-TX] Remove VBB from SCI-TX pin:", packet);
				SerialService.WritePacket(packet);
				UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + "Enter bootstrap mode cancelled.");
				if (speed != "62500 baud")
				{
					UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + Environment.NewLine + "Scanner SCI-bus speed is set to 7812.5 baud.");
					OriginalForm.SelectSCIBusLSMode();
				}
			}
		}
		else
		{
			UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + "Enter bootstrap mode cancelled.");
			if (speed != "62500 baud")
			{
				UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + Environment.NewLine + "Scanner SCI-bus speed is set to 7812.5 baud.");
				OriginalForm.SelectSCIBusLSMode();
			}
		}
	}

	private void UploadButton_Click(object sender, EventArgs e)
	{
		byte b = 0;
		Packet packet = new Packet();
		packet.Bus = 0;
		packet.Command = 13;
		packet.Mode = 6;
		packet.Payload = new byte[3]
		{
			(byte)((ListControl)BootloaderComboBox).SelectedIndex,
			(byte)((ListControl)WorkerComboBox).SelectedIndex,
			b
		};
		OriginalForm.TransmitUSBPacket("[<-TX] Upload worker:", packet);
		SerialService.WritePacket(packet);
		switch (((ListControl)WorkerComboBox).SelectedIndex)
		{
		case 1:
			OriginalForm.UpdateUSBTextBox("[INFO] Worker: part number read.");
			break;
		case 2:
			OriginalForm.UpdateUSBTextBox("[INFO] Worker: flash ID.");
			break;
		case 3:
			OriginalForm.UpdateUSBTextBox("[INFO] Worker: flash read.");
			break;
		case 4:
			OriginalForm.UpdateUSBTextBox("[INFO] Worker: flash erase.");
			break;
		case 5:
			OriginalForm.UpdateUSBTextBox("[INFO] Worker: flash write.");
			break;
		case 6:
			OriginalForm.UpdateUSBTextBox("[INFO] Worker: return flash checksum.");
			break;
		case 7:
			OriginalForm.UpdateUSBTextBox("[INFO] Worker: EEPROM read.");
			break;
		case 8:
			OriginalForm.UpdateUSBTextBox("[INFO] Worker: EEPROM write.");
			break;
		case 9:
			OriginalForm.UpdateUSBTextBox("[INFO] Worker: memory ripper.");
			break;
		case 10:
			OriginalForm.UpdateUSBTextBox("[INFO] Worker: read 68HC11K.");
			break;
		default:
			OriginalForm.UpdateUSBTextBox("[INFO] Worker: empty.");
			break;
		}
	}

	private void StartButton_Click(object sender, EventArgs e)
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Invalid comparison between Unknown and I4
		if (((ListControl)WorkerComboBox).SelectedIndex != 4 || (int)MessageBox.Show("Are you sure you want to erase the flash memory?", "Flash erase confirmation", (MessageBoxButtons)4, (MessageBoxIcon)48, (MessageBoxDefaultButton)256) != 7)
		{
			Packet packet = new Packet();
			packet.Bus = 0;
			packet.Command = 13;
			packet.Mode = 7;
			packet.Payload = new byte[2]
			{
				(byte)((ListControl)BootloaderComboBox).SelectedIndex,
				(byte)((ListControl)WorkerComboBox).SelectedIndex
			};
			OriginalForm.TransmitUSBPacket("[<-TX] Start worker:", packet);
			SerialService.WritePacket(packet);
		}
	}

	private void ExitButton_Click(object sender, EventArgs e)
	{
		Packet packet = new Packet();
		packet.Bus = 0;
		packet.Command = 13;
		packet.Mode = 8;
		packet.Payload = new byte[2]
		{
			(byte)((ListControl)BootloaderComboBox).SelectedIndex,
			(byte)((ListControl)WorkerComboBox).SelectedIndex
		};
		OriginalForm.TransmitUSBPacket("[<-TX] Exit worker:", packet);
		SerialService.WritePacket(packet);
	}

	private void FlashChipDetectButton_Click(object sender, EventArgs e)
	{
		((ListControl)WorkerComboBox).SelectedIndex = 2;
		UploadButton_Click(this, EventArgs.Empty);
		StartButton_Click(this, EventArgs.Empty);
	}

	private void FlashBrowseButton_Click(object sender, EventArgs e)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Expected O, but got Unknown
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Invalid comparison between Unknown and I4
		OpenFileDialog val = new OpenFileDialog();
		try
		{
			((FileDialog)val).InitialDirectory = Path.Combine(Application.StartupPath, "ROMs\\PCM");
			((FileDialog)val).Filter = "Binary files (*.bin)|*.bin|All files (*.*)|*.*";
			((FileDialog)val).FilterIndex = 2;
			((FileDialog)val).RestoreDirectory = false;
			if ((int)((CommonDialog)val).ShowDialog() != 1)
			{
				return;
			}
			using FileStream fileStream = File.Open(((FileDialog)val).FileName, FileMode.Open);
			FlashFileName = Path.GetFileName(((FileDialog)val).FileName);
			using MemoryStream memoryStream = new MemoryStream();
			fileStream.CopyTo(memoryStream);
			FlashFileBuffer = memoryStream.ToArray();
			if (FlashFileName.Length > 29)
			{
				((Control)FlashFileNameLabel).Text = FlashFileName.Remove(29);
			}
			else
			{
				((Control)FlashFileNameLabel).Text = FlashFileName;
			}
			UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + Environment.NewLine + "Flash file is loaded.");
			UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + "Name: " + FlashFileName);
			UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + "Size: " + FlashFileBuffer.Length + " bytes = " + ((double)FlashFileBuffer.Length / 1024.0).ToString("0.00") + " kilobytes.");
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	private async Task<bool> FileUploadTask(string FileName, byte[] FileBuffer, int FileOffset, int BlockSize)
	{
		List<byte> list = new List<byte>();
		list.AddRange(Encoding.Default.GetBytes(FileName + "\0"));
		list.AddRange(FileBuffer.Skip(FileOffset).Take(BlockSize).ToArray());
		Packet packet = new Packet();
		packet.Bus = 0;
		packet.Command = 12;
		packet.Mode = 3;
		packet.Payload = list.ToArray();
		OriginalForm.TransmitUSBPacket("[<-TX] Upload file:", packet);
		SerialService.WritePacket(packet);
		Packet packet2 = await WaitForResponse(1000);
		if (packet2 != null && packet2?.Mode == 0)
		{
			return true;
		}
		return false;
	}

	private void FileUpload_DoWork(object sender, DoWorkEventArgs e)
	{
		int num = 4;
		int num2 = 0;
		int num3 = 0;
		int num4 = 0;
		int num5 = 0;
		switch (FileUploadPending)
		{
		case 1:
			num4 = 512;
			num5 = FlashFileBuffer.Length;
			break;
		case 2:
			num4 = 128;
			num5 = EEPROMFileBuffer.Length;
			break;
		}
		if (num5 == 0)
		{
			return;
		}
		while (true)
		{
			Task<bool> task = null;
			switch (FileUploadPending)
			{
			case 1:
				if (num5 - 1 - num3 < 512)
				{
					num4 = num5 - num3;
				}
				task = FileUploadTask(FlashFileName, FlashFileBuffer, num3, num4);
				break;
			case 2:
				if (num5 - 1 - num3 < 128)
				{
					num4 = num5 - num3;
				}
				task = FileUploadTask(EEPROMFileName, EEPROMFileBuffer, num3, num4);
				break;
			}
			if (task == null)
			{
				break;
			}
			task.Wait();
			if (task.Result)
			{
				num3 += num4;
				num2 = (int)Math.Round((double)num3 / (double)num5 * 100.0);
				FileUploadWorker.ReportProgress(num2, new Tuple<int, int>(num3, num5));
				if (num3 >= num5)
				{
					break;
				}
				num = 4;
			}
			else
			{
				num--;
				if (num == 0)
				{
					break;
				}
			}
		}
	}

	private void FileUpload_ProgressChanged(object sender, ProgressChangedEventArgs e)
	{
		Tuple<int, int> tuple = (Tuple<int, int>)e.UserState;
		((Control)SCIBusBootstrapToolsProgressLabel).Text = "Progress: " + e.ProgressPercentage + "% (" + tuple.Item1 + "/" + tuple.Item2 + " bytes)";
	}

	private async void FileUpload_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
	{
		if (e.Cancelled)
		{
			UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + Environment.NewLine + "File upload has been cancelled.");
			return;
		}
		switch (FileUploadPending)
		{
		case 1:
		{
			UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + Environment.NewLine + "Flash file uploaded successfully.");
			Packet packet3 = new Packet();
			packet3.Bus = 0;
			packet3.Command = 12;
			packet3.Mode = 2;
			packet3.Payload = Encoding.Default.GetBytes(FlashFileName + "\0");
			OriginalForm.TransmitUSBPacket("[<-TX] Verify upload:", packet3);
			SerialService.WritePacket(packet3);
			Packet packet4 = await WaitForResponse(1000);
			if (packet4 == null)
			{
				MessageBox.Show("No response from scanner.", "Warning", (MessageBoxButtons)0, (MessageBoxIcon)48);
				return;
			}
			if (packet4.Payload.Length < 7)
			{
				MessageBox.Show("Invalid response from scanner.", "Warning", (MessageBoxButtons)0, (MessageBoxIcon)48);
				return;
			}
			if (packet4.Payload[packet4.Payload.Length - 5] == 0)
			{
				MessageBox.Show("Flash file does not exist in internal storage." + Environment.NewLine + "Try uploading again.", "Warning", (MessageBoxButtons)0, (MessageBoxIcon)48);
				UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + "File upload error: flash file does not exist in internal storage.");
				return;
			}
			if ((packet4.Payload[packet4.Payload.Length - 4] << 24) + (packet4.Payload[packet4.Payload.Length - 3] << 16) + (packet4.Payload[packet4.Payload.Length - 2] << 8) + packet4.Payload[packet4.Payload.Length - 1] != FlashFileBuffer.Length)
			{
				MessageBox.Show("Flash file size error." + Environment.NewLine + "Try uploading again.", "Warning", (MessageBoxButtons)0, (MessageBoxIcon)48);
				UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + "File upload error: file size error.");
				return;
			}
			packet3 = new Packet();
			packet3.Bus = 0;
			packet3.Command = 13;
			packet3.Mode = 1;
			byte item2 = Convert.ToByte(FlashMemoryBackupCheckBox.Checked);
			List<byte> list2 = new List<byte>();
			list2.Add((byte)((ListControl)BootloaderComboBox).SelectedIndex);
			list2.Add(item2);
			list2.AddRange(Encoding.Default.GetBytes(FlashFileName + "\0"));
			packet3.Payload = list2.ToArray();
			OriginalForm.TransmitUSBPacket("[<-TX] Start flash memory writing:", packet3);
			SerialService.WritePacket(packet3);
			UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + Environment.NewLine + "Start flash memory writing session.");
			break;
		}
		case 2:
		{
			UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + Environment.NewLine + "EEPROM file uploaded successfully.");
			Packet packet = new Packet();
			packet.Bus = 0;
			packet.Command = 12;
			packet.Mode = 2;
			packet.Payload = Encoding.Default.GetBytes(EEPROMFileName + "\0");
			OriginalForm.TransmitUSBPacket("[<-TX] Verify upload:", packet);
			SerialService.WritePacket(packet);
			Packet packet2 = await WaitForResponse(1000);
			if (packet2 == null)
			{
				MessageBox.Show("No response from scanner.", "Warning", (MessageBoxButtons)0, (MessageBoxIcon)48);
				return;
			}
			if (packet2.Payload.Length < 7)
			{
				MessageBox.Show("Invalid response from scanner.", "Warning", (MessageBoxButtons)0, (MessageBoxIcon)48);
				return;
			}
			if (packet2.Payload[packet2.Payload.Length - 5] == 0)
			{
				MessageBox.Show("EEPROM file does not exist in internal storage." + Environment.NewLine + "Try uploading again.", "Warning", (MessageBoxButtons)0, (MessageBoxIcon)48);
				UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + "File upload error: EEPROM file does not exist in internal storage.");
				return;
			}
			if ((packet2.Payload[packet2.Payload.Length - 4] << 24) + (packet2.Payload[packet2.Payload.Length - 3] << 16) + (packet2.Payload[packet2.Payload.Length - 2] << 8) + packet2.Payload[packet2.Payload.Length - 1] != EEPROMFileBuffer.Length)
			{
				MessageBox.Show("EEPROM file size error." + Environment.NewLine + "Try uploading again.", "Warning", (MessageBoxButtons)0, (MessageBoxIcon)48);
				UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + "File upload error: file size error.");
				return;
			}
			packet = new Packet();
			packet.Bus = 0;
			packet.Command = 13;
			packet.Mode = 3;
			byte item = 0;
			List<byte> list = new List<byte>();
			list.Add((byte)((ListControl)BootloaderComboBox).SelectedIndex);
			list.Add(item);
			list.AddRange(Encoding.Default.GetBytes(EEPROMFileName + "\0"));
			packet.Payload = list.ToArray();
			OriginalForm.TransmitUSBPacket("[<-TX] Start EEPROM writing:", packet);
			SerialService.WritePacket(packet);
			UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + Environment.NewLine + "Start EEPROM writing session.");
			break;
		}
		}
		FileUploadPending = 0;
	}

	private async void FlashWriteButton_Click(object sender, EventArgs e)
	{
		if (FileUploadWorker.IsBusy)
		{
			MessageBox.Show("File upload is in progress!", "Warning", (MessageBoxButtons)0, (MessageBoxIcon)48);
		}
		else if (FlashFileBuffer == null)
		{
			MessageBox.Show("Browse flash file first!", "Warning", (MessageBoxButtons)0, (MessageBoxIcon)48);
		}
		else
		{
			if ((int)MessageBox.Show("Are you sure you want to continue?", "Confirm flash programming", (MessageBoxButtons)4, (MessageBoxIcon)32, (MessageBoxDefaultButton)256) == 7)
			{
				return;
			}
			Packet packet2 = new Packet
			{
				Bus = 0,
				Command = 12,
				Mode = 2,
				Payload = Encoding.Default.GetBytes(FlashFileName + "\0")
			};
			OriginalForm.TransmitUSBPacket("[<-TX] Prepare file upload:", packet2);
			SerialService.WritePacket(packet2);
			Packet packet3 = await WaitForResponse(1000);
			if (packet3 == null)
			{
				MessageBox.Show("No response from scanner.", "Warning", (MessageBoxButtons)0, (MessageBoxIcon)48);
				return;
			}
			if (packet3.Payload.Length < 7)
			{
				MessageBox.Show("Invalid response from scanner.", "Warning", (MessageBoxButtons)0, (MessageBoxIcon)48);
				return;
			}
			if (packet3.Payload[packet3.Payload.Length - 5] > 0)
			{
				if ((int)MessageBox.Show("Flash file already exists in the scanner's internal storage." + Environment.NewLine + "Do you want to reuse it?", "Reuse previous upload", (MessageBoxButtons)4, (MessageBoxIcon)32, (MessageBoxDefaultButton)256) == 6)
				{
					DisableControlsForFlashSession();
					SessionRunning = true;
					packet2 = new Packet
					{
						Bus = 0,
						Command = 13,
						Mode = 1
					};
					byte item = Convert.ToByte(FlashMemoryBackupCheckBox.Checked);
					List<byte> list = new List<byte>();
					list.Add((byte)((ListControl)BootloaderComboBox).SelectedIndex);
					list.Add(item);
					list.AddRange(Encoding.Default.GetBytes(FlashFileName + "\0"));
					packet2.Payload = list.ToArray();
					OriginalForm.TransmitUSBPacket("[<-TX] Start flash memory writing:", packet2);
					SerialService.WritePacket(packet2);
					UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + Environment.NewLine + "Start flash memory writing session.");
					return;
				}
				if ((int)MessageBox.Show("Do you want to overwrite flash file in scanner's internal storage?", "Overwrite", (MessageBoxButtons)4, (MessageBoxIcon)32, (MessageBoxDefaultButton)256) == 7)
				{
					FlashBrowseButton_Click(this, EventArgs.Empty);
					return;
				}
				packet2.Mode = 6;
				OriginalForm.TransmitUSBPacket("[<-TX] Delete file:", packet2);
				SerialService.WritePacket(packet2);
				packet3 = await WaitForResponse(1000);
				if (packet3 == null || packet3?.Mode != 0)
				{
					MessageBox.Show("File delete error.", "Warning", (MessageBoxButtons)0, (MessageBoxIcon)48);
					return;
				}
			}
			else
			{
				packet2.Bus = 0;
				packet2.Command = 12;
				packet2.Mode = 1;
				packet2.Payload = null;
				OriginalForm.TransmitUSBPacket("[<-TX] Check internal storage:", packet2);
				SerialService.WritePacket(packet2);
				packet3 = await WaitForResponse(1000);
				if (packet3 == null)
				{
					MessageBox.Show("An error occurred while checking storage information.", "Warning", (MessageBoxButtons)0, (MessageBoxIcon)48);
					return;
				}
				if (packet3.Payload.Length < 8)
				{
					MessageBox.Show("Invalid response from scanner.", "Warning", (MessageBoxButtons)0, (MessageBoxIcon)48);
					return;
				}
				int num = (packet3.Payload[0] << 24) + (packet3.Payload[1] << 16) + (packet3.Payload[2] << 8) + packet3.Payload[3];
				int num2 = (packet3.Payload[4] << 24) + (packet3.Payload[5] << 16) + (packet3.Payload[6] << 8) + packet3.Payload[7];
				if (num - num2 < FlashFileBuffer.Length + 33333)
				{
					if ((int)MessageBox.Show("There is not enough free space to save flash file." + Environment.NewLine + "Do you want to erase the internal storage?", "Confirm storage formatting", (MessageBoxButtons)4, (MessageBoxIcon)32, (MessageBoxDefaultButton)256) == 7)
					{
						return;
					}
					packet2.Bus = 0;
					packet2.Command = 12;
					packet2.Mode = 7;
					packet2.Payload = null;
					OriginalForm.TransmitUSBPacket("[<-TX] Format internal storage:", packet2);
					SerialService.WritePacket(packet2);
					packet3 = await WaitForResponse(1000);
					if (packet3 == null || packet3?.Mode != 0)
					{
						return;
					}
				}
			}
			DisableControlsForFlashSession();
			UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + Environment.NewLine + "Please wait while the selected flash file is transferred to the programmer.");
			FileUploadPending = 1;
			SessionRunning = true;
			FileUploadWorker.RunWorkerAsync();
		}
	}

	private void FlashReadButton_Click(object sender, EventArgs e)
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Expected O, but got Unknown
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Invalid comparison between Unknown and I4
		if (FileUploadWorker.IsBusy)
		{
			MessageBox.Show("Busy!", "Warning", (MessageBoxButtons)0, (MessageBoxIcon)48);
			return;
		}
		SaveFileDialog val = new SaveFileDialog();
		try
		{
			((FileDialog)val).InitialDirectory = Path.Combine(Application.StartupPath, "ROMs\\PCM");
			((FileDialog)val).Filter = "Binary files (*.bin)|*.bin|All files (*.*)|*.*";
			((FileDialog)val).FilterIndex = 2;
			((FileDialog)val).RestoreDirectory = false;
			if ((int)((CommonDialog)val).ShowDialog() != 1)
			{
				return;
			}
			SCIBusFlashReadFilePath = ((FileDialog)val).FileName;
			if (File.Exists(SCIBusFlashReadFilePath))
			{
				File.Delete(SCIBusFlashReadFilePath);
			}
			if (Path.GetFileName(SCIBusFlashReadFilePath).Length > 29)
			{
				((Control)FlashFileNameLabel).Text = Path.GetFileName(SCIBusFlashReadFilePath).Remove(29);
			}
			else
			{
				((Control)FlashFileNameLabel).Text = Path.GetFileName(SCIBusFlashReadFilePath);
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
		byte b = Convert.ToByte(FlashMemoryBackupCheckBox.Checked);
		Packet packet = new Packet();
		packet.Bus = 0;
		packet.Command = 13;
		packet.Mode = 2;
		packet.Payload = new byte[2]
		{
			(byte)((ListControl)BootloaderComboBox).SelectedIndex,
			b
		};
		if (((ListControl)WorkerComboBox).SelectedIndex == 10)
		{
			packet.Mode = 10;
		}
		OriginalForm.TransmitUSBPacket("[<-TX] Start flash reading:", packet);
		SerialService.WritePacket(packet);
		UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + Environment.NewLine + "Start flash memory reading session.");
		DisableControlsForFlashSession();
		SessionRunning = true;
	}

	private void FlashStopButton_Click(object sender, EventArgs e)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Invalid comparison between Unknown and I4
		if ((int)MessageBox.Show("Are you sure you want to stop flash operation?", "Warning", (MessageBoxButtons)4, (MessageBoxIcon)48, (MessageBoxDefaultButton)256) != 7)
		{
			Packet packet = new Packet();
			packet.Bus = 0;
			packet.Command = 13;
			packet.Mode = 9;
			packet.Payload = new byte[1] { (byte)((ListControl)BootloaderComboBox).SelectedIndex };
			OriginalForm.TransmitUSBPacket("[<-TX] Cancellation request:", packet);
			SerialService.WritePacket(packet);
		}
	}

	private void EEPROMBrowseButton_Click(object sender, EventArgs e)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Expected O, but got Unknown
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Invalid comparison between Unknown and I4
		OpenFileDialog val = new OpenFileDialog();
		try
		{
			((FileDialog)val).InitialDirectory = Path.Combine(Application.StartupPath, "ROMs\\PCM");
			((FileDialog)val).Filter = "Binary files (*.bin)|*.bin|All files (*.*)|*.*";
			((FileDialog)val).FilterIndex = 2;
			((FileDialog)val).RestoreDirectory = false;
			if ((int)((CommonDialog)val).ShowDialog() != 1)
			{
				return;
			}
			using FileStream fileStream = File.Open(((FileDialog)val).FileName, FileMode.Open);
			using MemoryStream memoryStream = new MemoryStream();
			fileStream.CopyTo(memoryStream);
			EEPROMFileBuffer = memoryStream.ToArray();
			EEPROMFileName = Path.GetFileName(((FileDialog)val).FileName);
			if (EEPROMFileName.Length > 29)
			{
				((Control)EEPROMFileNameLabel).Text = EEPROMFileName.Remove(29);
			}
			else
			{
				((Control)EEPROMFileNameLabel).Text = EEPROMFileName;
			}
			UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + Environment.NewLine + "EEPROM file is loaded.");
			UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + "Name: " + EEPROMFileName);
			UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + "Size: " + EEPROMFileBuffer.Length + " bytes = " + ((double)EEPROMFileBuffer.Length / 1024.0).ToString("0.00") + " kilobytes.");
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	private async void EEPROMWriteButton_Click(object sender, EventArgs e)
	{
		if (FileUploadWorker.IsBusy)
		{
			MessageBox.Show("Busy!", "Warning", (MessageBoxButtons)0, (MessageBoxIcon)48);
		}
		else if (EEPROMFileBuffer == null)
		{
			MessageBox.Show("Browse EEPROM file first!", "Warning", (MessageBoxButtons)0, (MessageBoxIcon)48);
		}
		else
		{
			if ((int)MessageBox.Show("Are you sure you want to continue?", "Confirm EEPROM programming", (MessageBoxButtons)4, (MessageBoxIcon)32, (MessageBoxDefaultButton)256) == 7)
			{
				return;
			}
			Packet packet2 = new Packet
			{
				Bus = 0,
				Command = 12,
				Mode = 2,
				Payload = Encoding.Default.GetBytes(EEPROMFileName + "\0")
			};
			OriginalForm.TransmitUSBPacket("[<-TX] Prepare file upload:", packet2);
			SerialService.WritePacket(packet2);
			Packet packet3 = await WaitForResponse(1000);
			if (packet3 == null)
			{
				MessageBox.Show("No response from scanner.", "Warning", (MessageBoxButtons)0, (MessageBoxIcon)48);
				return;
			}
			if (packet3.Payload.Length < 7)
			{
				MessageBox.Show("Invalid response from scanner.", "Warning", (MessageBoxButtons)0, (MessageBoxIcon)48);
				return;
			}
			if (packet3.Payload[packet3.Payload.Length - 5] > 0)
			{
				if ((int)MessageBox.Show("EEPROM file already exists in the scanner's internal storage." + Environment.NewLine + "Do you want to reuse it?", "Reuse previous upload", (MessageBoxButtons)4, (MessageBoxIcon)32, (MessageBoxDefaultButton)256) == 6)
				{
					DisableControlsForEEPROMSession();
					SessionRunning = true;
					packet2 = new Packet
					{
						Bus = 0,
						Command = 13,
						Mode = 3
					};
					byte item = 0;
					List<byte> list = new List<byte>();
					list.Add((byte)((ListControl)BootloaderComboBox).SelectedIndex);
					list.Add(item);
					list.AddRange(Encoding.Default.GetBytes(EEPROMFileName + "\0"));
					packet2.Payload = list.ToArray();
					OriginalForm.TransmitUSBPacket("[<-TX] Start EEPROM writing:", packet2);
					SerialService.WritePacket(packet2);
					UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + Environment.NewLine + "Start EEPROM writing session.");
					return;
				}
				if ((int)MessageBox.Show("Do you want to overwrite EEPROM file in scanner's internal storage?", "Overwrite", (MessageBoxButtons)4, (MessageBoxIcon)32, (MessageBoxDefaultButton)256) == 7)
				{
					EEPROMBrowseButton_Click(this, EventArgs.Empty);
					return;
				}
				packet2.Mode = 6;
				OriginalForm.TransmitUSBPacket("[<-TX] Delete file:", packet2);
				SerialService.WritePacket(packet2);
				packet3 = await WaitForResponse(1000);
				if (packet3 == null || packet3?.Mode != 0)
				{
					MessageBox.Show("File delete error.", "Warning", (MessageBoxButtons)0, (MessageBoxIcon)48);
					return;
				}
			}
			else
			{
				packet2.Bus = 0;
				packet2.Command = 12;
				packet2.Mode = 1;
				packet2.Payload = null;
				OriginalForm.TransmitUSBPacket("[<-TX] Check internal storage:", packet2);
				SerialService.WritePacket(packet2);
				packet3 = await WaitForResponse(1000);
				if (packet3 == null)
				{
					MessageBox.Show("An error occurred while checking storage information.", "Warning", (MessageBoxButtons)0, (MessageBoxIcon)48);
					return;
				}
				if (packet3.Payload.Length < 8)
				{
					MessageBox.Show("Invalid response from scanner.", "Warning", (MessageBoxButtons)0, (MessageBoxIcon)48);
					return;
				}
				int num = (packet3.Payload[0] << 24) + (packet3.Payload[1] << 16) + (packet3.Payload[2] << 8) + packet3.Payload[3];
				int num2 = (packet3.Payload[4] << 24) + (packet3.Payload[5] << 16) + (packet3.Payload[6] << 8) + packet3.Payload[7];
				if (num - num2 < EEPROMFileBuffer.Length + 33333)
				{
					if ((int)MessageBox.Show("There is not enough free space to save EEPROM file." + Environment.NewLine + "Do you want to erase the scanner's internal storage?", "Confirm storage formatting", (MessageBoxButtons)4, (MessageBoxIcon)32, (MessageBoxDefaultButton)256) == 7)
					{
						return;
					}
					packet2.Bus = 0;
					packet2.Command = 12;
					packet2.Mode = 7;
					packet2.Payload = null;
					OriginalForm.TransmitUSBPacket("[<-TX] Format internal storage:", packet2);
					SerialService.WritePacket(packet2);
					packet3 = await WaitForResponse(1000);
					if (packet3 == null || packet3?.Mode != 0)
					{
						return;
					}
				}
			}
			DisableControlsForEEPROMSession();
			UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + Environment.NewLine + "Please wait while the selected EEPROM file is transferred to the programmer.");
			FileUploadPending = 2;
			SessionRunning = true;
			FileUploadWorker.RunWorkerAsync();
		}
	}

	private void EEPROMReadButton_Click(object sender, EventArgs e)
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Expected O, but got Unknown
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Invalid comparison between Unknown and I4
		if (FileUploadWorker.IsBusy)
		{
			MessageBox.Show("Busy!", "Warning", (MessageBoxButtons)0, (MessageBoxIcon)48);
			return;
		}
		SaveFileDialog val = new SaveFileDialog();
		try
		{
			((FileDialog)val).InitialDirectory = Path.Combine(Application.StartupPath, "ROMs\\PCM");
			((FileDialog)val).Filter = "Binary files (*.bin)|*.bin|All files (*.*)|*.*";
			((FileDialog)val).FilterIndex = 2;
			((FileDialog)val).RestoreDirectory = false;
			if ((int)((CommonDialog)val).ShowDialog() != 1)
			{
				return;
			}
			SCIBusEEPROMReadFilePath = ((FileDialog)val).FileName;
			if (File.Exists(SCIBusEEPROMReadFilePath))
			{
				File.Delete(SCIBusEEPROMReadFilePath);
			}
			if (Path.GetFileName(SCIBusEEPROMReadFilePath).Length > 29)
			{
				((Control)EEPROMFileNameLabel).Text = Path.GetFileName(SCIBusEEPROMReadFilePath).Remove(29);
			}
			else
			{
				((Control)EEPROMFileNameLabel).Text = Path.GetFileName(SCIBusEEPROMReadFilePath);
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
		byte b = 0;
		Packet packet = new Packet();
		packet.Bus = 0;
		packet.Command = 13;
		packet.Mode = 4;
		packet.Payload = new byte[2]
		{
			(byte)((ListControl)BootloaderComboBox).SelectedIndex,
			b
		};
		OriginalForm.TransmitUSBPacket("[<-TX] Start EEPROM reading:", packet);
		SerialService.WritePacket(packet);
		UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + Environment.NewLine + "Start EEPROM reading session.");
		DisableControlsForEEPROMSession();
		SessionRunning = true;
	}

	private void EEPROMStopButton_Click(object sender, EventArgs e)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Invalid comparison between Unknown and I4
		if ((int)MessageBox.Show("Are you sure you want to stop EEPROM operation?", "Warning", (MessageBoxButtons)4, (MessageBoxIcon)48, (MessageBoxDefaultButton)256) != 7)
		{
			Packet packet = new Packet();
			packet.Bus = 0;
			packet.Command = 13;
			packet.Mode = 9;
			packet.Payload = new byte[1] { (byte)((ListControl)BootloaderComboBox).SelectedIndex };
			OriginalForm.TransmitUSBPacket("[<-TX] Cancellation request:", packet);
			SerialService.WritePacket(packet);
		}
	}

	private void BootloaderComboBox_SelectedIndexChanged(object sender, EventArgs e)
	{
		if (((ListControl)BootloaderComboBox).SelectedIndex == 0)
		{
			((Control)EEPROMSizeLabel).Text = "Size:";
			EEPROMSize = 0;
		}
		else if (((ListControl)BootloaderComboBox).SelectedIndex < 7)
		{
			((Control)EEPROMSizeLabel).Text = "Size: 512 b";
			EEPROMSize = 512;
		}
		else
		{
			((Control)EEPROMSizeLabel).Text = "Size: 640 b";
			EEPROMSize = 640;
		}
		switch (((ListControl)BootloaderComboBox).SelectedIndex)
		{
		case 2:
		case 4:
			FlashChipSize = 131072;
			break;
		case 3:
		case 5:
		case 6:
		case 7:
		case 8:
			FlashChipSize = 262144;
			break;
		}
	}

	private void WorkerComboBox_SelectedIndexChanged(object sender, EventArgs e)
	{
		switch (((ListControl)WorkerComboBox).SelectedIndex)
		{
		case 0:
		case 1:
		case 2:
		case 4:
		case 6:
			((Control)ExitButton).Enabled = false;
			break;
		case 3:
		case 5:
		case 7:
		case 8:
		case 9:
		case 10:
			if (!SessionRunning)
			{
				((Control)ExitButton).Enabled = true;
			}
			break;
		}
	}

	private async void FormatStorageButton_Click(object sender, EventArgs e)
	{
		if ((int)MessageBox.Show("The scanner's internal storage will be erased." + Environment.NewLine + "Proceed?", "Confirm storage formatting", (MessageBoxButtons)4, (MessageBoxIcon)32, (MessageBoxDefaultButton)256) != 7)
		{
			Packet packet = new Packet();
			packet.Bus = 0;
			packet.Command = 12;
			packet.Mode = 7;
			packet.Payload = null;
			OriginalForm.TransmitUSBPacket("[<-TX] Format internal storage:", packet);
			SerialService.WritePacket(packet);
			Packet packet2 = await WaitForResponse(1000);
			if (packet2 == null || packet2?.Mode != 0)
			{
				UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + Environment.NewLine + "Internal storage format error.");
				MessageBox.Show("Error.", "Warning", (MessageBoxButtons)0, (MessageBoxIcon)48);
			}
			else
			{
				UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + Environment.NewLine + "Internal storage format success.");
				MessageBox.Show("Success.", "Information", (MessageBoxButtons)0, (MessageBoxIcon)64);
			}
		}
	}

	private async Task<Packet> WaitForResponse(int TimeoutMillis)
	{
		PacketReceivedTask = new TaskCompletionSource<Packet>();
		try
		{
			if (await Task.WhenAny(new Task[2]
			{
				PacketReceivedTask.Task,
				Task.Delay(TimeoutMillis)
			}) == PacketReceivedTask.Task)
			{
				return PacketReceivedTask.Task.Result;
			}
			return null;
		}
		catch (TimeoutException)
		{
			return null;
		}
		catch (Exception)
		{
			return null;
		}
	}

	private void PacketReceivedHandler(object sender, Packet packet)
	{
		if (packet.Bus == 1 || packet.Bus == 4)
		{
			return;
		}
		UIContext.Post(delegate
		{
			//IL_06c5: Unknown result type (might be due to invalid IL or missing references)
			//IL_04f6: Unknown result type (might be due to invalid IL or missing references)
			switch (packet.Bus)
			{
			case 0:
				switch (packet.Command)
				{
				case 3:
					if (packet.Mode == 7 && packet.Payload != null)
					{
						byte[] payload3 = packet.Payload;
						if (payload3 == null || payload3.Length >= 2)
						{
							switch ((ushort)((packet.Payload[0] << 8) + packet.Payload[1]))
							{
							case 12000:
								UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + "VBB (12V) applied to SCI-TX pin.");
								UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + "Turn key to RUN position.");
								break;
							case 20000:
								UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + "VPP (20V) applied to SCI-TX pin.");
								break;
							case 0:
								UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + "VBB/VPP removed from SCI-TX pin.");
								break;
							}
						}
					}
					break;
				case 5:
					if (packet.Mode == 8 && packet.Payload != null)
					{
						byte[] payload2 = packet.Payload;
						if (payload2 == null || payload2.Length >= 6)
						{
							double num20 = (double)((packet.Payload[0] << 8) + packet.Payload[1]) / 1000.0;
							double num21 = (double)((packet.Payload[2] << 8) + packet.Payload[3]) / 1000.0;
							double num22 = (double)((packet.Payload[4] << 8) + packet.Payload[5]) / 1000.0;
							string text6 = num20.ToString("0.000") + " V";
							string text7 = num21.ToString("0.000") + " V";
							string text8 = num22.ToString("0.000") + " V";
							UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + Environment.NewLine + "Battery voltage: " + text6 + Environment.NewLine + "Bootstrap voltage: " + text7 + Environment.NewLine + "Programming voltage: " + text8);
							if (num20 < 11.5)
							{
								UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + Environment.NewLine + "Connect battery charger. Battery voltage must be above " + 11.5.ToString("0.0") + "V.");
							}
							if (num21 < 11.5)
							{
								UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + Environment.NewLine + "Bootstrap voltage must be above " + 11.5.ToString("0.0") + "V.");
							}
							if (num22 < 19.0)
							{
								UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + Environment.NewLine + "Programming voltage must be above " + 19.0.ToString("0.0") + "V.");
							}
						}
					}
					break;
				case 12:
					switch (packet.Mode)
					{
					case 1:
					case 2:
					case 3:
					case 4:
					case 5:
					case 6:
					case 7:
						break;
					}
					break;
				case 13:
					switch (packet.Mode)
					{
					case 1:
					case 2:
					case 3:
					case 4:
						if (packet.Payload[0] < 128)
						{
							switch (packet.Payload[0])
							{
							case 0:
								UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + Environment.NewLine + "Task succeeded.");
								MessageBox.Show("Success! Turn key to OFF/LOCKED position." + Environment.NewLine + "Wait for a few seconds before starting the engine.", "Information", (MessageBoxButtons)0, (MessageBoxIcon)64);
								break;
							case 2:
							{
								int num17 = 0;
								int num18 = 0;
								int num19 = packet.Payload.Length - 8;
								if (packet.Payload.Length > 9)
								{
									num17 = (packet.Payload[num19] << 24) + (packet.Payload[num19 + 1] << 16) + (packet.Payload[num19 + 2] << 8) + packet.Payload[num19 + 3];
									num18 = (packet.Payload[num19 + 4] << 24) + (packet.Payload[num19 + 5] << 16) + (packet.Payload[num19 + 6] << 8) + packet.Payload[num19 + 7];
								}
								UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + Environment.NewLine + "Flash file size mismatch:");
								UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + "Expected: " + num18 + " bytes");
								UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + "Given: " + num17 + " bytes");
								UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + Environment.NewLine + "Task failed.");
								break;
							}
							case 12:
								UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + Environment.NewLine + "Task succeeded with checksum error.");
								MessageBox.Show("Task finished with flash checksum error! Turn key to OFF/LOCKED position." + Environment.NewLine + "Wait for a few seconds before starting the engine.", "Information", (MessageBoxButtons)0, (MessageBoxIcon)64);
								break;
							case 14:
							{
								int num14 = 0;
								int num15 = 0;
								int num16 = packet.Payload.Length - 8;
								if (packet.Payload.Length > 8)
								{
									num14 = (packet.Payload[num16] << 24) + (packet.Payload[num16 + 1] << 16) + (packet.Payload[num16 + 2] << 8) + packet.Payload[num16 + 3];
									num15 = (packet.Payload[num16 + 4] << 24) + (packet.Payload[num16 + 5] << 16) + (packet.Payload[num16 + 6] << 8) + packet.Payload[num16 + 7];
								}
								UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + Environment.NewLine + "EEPROM file size mismatch:");
								UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + "Expected: " + num15 + " bytes");
								UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + "Given: " + num14 + " bytes");
								UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + Environment.NewLine + "Task failed.");
								break;
							}
							default:
								UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + Environment.NewLine + $"Task failed: {(AutoBootResult)packet.Payload[0]}");
								break;
							}
							SessionRunning = false;
							((ListControl)WorkerComboBox).SelectedIndex = 0;
							EnableControls();
						}
						else
						{
							switch (packet.Payload[0])
							{
							case 128:
								UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + Environment.NewLine + "Verify voltages.");
								break;
							case 129:
								UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + Environment.NewLine + "Voltages OK.");
								break;
							case 130:
								((ListControl)WorkerComboBox).SelectedIndex = 1;
								break;
							case 132:
								UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + "Part number: ");
								if (packet.Payload.Length < 3)
								{
									UpdateTextBox(SCIBusBootstrapInfoTextBox, "error.");
								}
								else
								{
									UpdateTextBox(SCIBusBootstrapInfoTextBox, Encoding.Default.GetString(packet.Payload.Skip(1).ToArray()).ToUpper());
								}
								break;
							case 134:
								((ListControl)WorkerComboBox).SelectedIndex = 2;
								break;
							case 136:
								UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + "FIDs: ");
								if (packet.Payload.Length < 3)
								{
									UpdateTextBox(SCIBusBootstrapInfoTextBox, "error.");
								}
								else
								{
									UpdateTextBox(SCIBusBootstrapInfoTextBox, Util.ByteToHexString(packet.Payload, 1, 2));
									UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + "Mfr.: ");
									string text4 = packet.Payload[1] switch
									{
										32 => "ST (SGS)", 
										49 => "Catalyst", 
										137 => "Intel", 
										151 => "TI", 
										152 => "Toshiba", 
										_ => Util.ByteToHexString(packet.Payload, 1), 
									};
									UpdateTextBox(SCIBusBootstrapInfoTextBox, text4);
									((Control)FlashChipManufacturerLabel).Text = "Mfr.: " + text4;
									UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + "Chip: ");
									string text5;
									switch (packet.Payload[2])
									{
									case 80:
										text5 = "M28F102 (128k)";
										FlashChipSize = 131072;
										break;
									case 81:
										text5 = "CAT28F102 (128k)";
										FlashChipSize = 131072;
										break;
									case 116:
										text5 = "M28F200T (256k)";
										FlashChipSize = 262144;
										break;
									case 117:
										text5 = "M28F200B (256k)";
										FlashChipSize = 262144;
										break;
									case 180:
										if (((ListControl)BootloaderComboBox).SelectedIndex == 7)
										{
											text5 = "N28F010A (2x 128k)";
											FlashChipSize = 262144;
										}
										else
										{
											text5 = "N28F010A (128k)";
											FlashChipSize = 131072;
										}
										break;
									case 189:
										text5 = "N28F020 (256k)";
										FlashChipSize = 262144;
										break;
									case 224:
										text5 = "M28F210 (256k)";
										FlashChipSize = 262144;
										break;
									case 229:
										text5 = "TMS28F210 (256k)";
										FlashChipSize = 262144;
										break;
									case 230:
										text5 = "M28F220 (256k)";
										FlashChipSize = 262144;
										break;
									default:
										text5 = Util.ByteToHexString(packet.Payload, 2);
										FlashChipSize = 0;
										break;
									}
									UpdateTextBox(SCIBusBootstrapInfoTextBox, text5);
									((Control)FlashChipIDLabel).Text = "Chip: " + text5;
								}
								break;
							case 133:
								UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + Environment.NewLine + "Flash backup filename: ");
								if (packet.Payload.Length < 3)
								{
									UpdateTextBox(SCIBusBootstrapInfoTextBox, "error.");
								}
								else
								{
									UpdateTextBox(SCIBusBootstrapInfoTextBox, Encoding.Default.GetString(packet.Payload.Skip(1).ToArray()));
								}
								break;
							case 146:
								UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + Environment.NewLine + "EEPROM backup filename: ");
								if (packet.Payload.Length < 3)
								{
									UpdateTextBox(SCIBusBootstrapInfoTextBox, "error.");
								}
								else
								{
									UpdateTextBox(SCIBusBootstrapInfoTextBox, Encoding.Default.GetString(packet.Payload.Skip(1).ToArray()));
								}
								break;
							case 137:
								((ListControl)WorkerComboBox).SelectedIndex = 3;
								((Control)SCIBusBootstrapToolsProgressLabel).Text = "Progress: 0%";
								break;
							case 140:
								((ListControl)WorkerComboBox).SelectedIndex = 4;
								break;
							case 141:
								UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + "Start flash erasing.");
								break;
							case 142:
								if (packet.Payload.Length < 2)
								{
									UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + "Flash erase error.");
								}
								else if (packet.Payload[1] == 34)
								{
									UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + "Flash erased successfully.");
								}
								else
								{
									UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + "Flash erase failed.");
								}
								break;
							case 143:
								((ListControl)WorkerComboBox).SelectedIndex = 5;
								((Control)SCIBusBootstrapToolsProgressLabel).Text = "Progress: 0%";
								break;
							case 153:
								((ListControl)WorkerComboBox).SelectedIndex = 6;
								break;
							case 155:
								UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + "Checksum result: ");
								if (packet.Payload.Length < 3)
								{
									UpdateTextBox(SCIBusBootstrapInfoTextBox, "error.");
								}
								else
								{
									UpdateTextBox(SCIBusBootstrapInfoTextBox, Util.ByteToHexString(packet.Payload, 1, 2));
									UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + "Expected checksum: " + Util.ByteToHexString(packet.Payload, 1));
									UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + "Flash checksum: " + Util.ByteToHexString(packet.Payload, 2));
									if (packet.Payload[1] == packet.Payload[2])
									{
										UpdateTextBox(SCIBusBootstrapInfoTextBox, " <- verified.");
									}
									else
									{
										UpdateTextBox(SCIBusBootstrapInfoTextBox, " <- error, values must be equal.");
									}
								}
								break;
							case 147:
								((ListControl)WorkerComboBox).SelectedIndex = 8;
								((Control)SCIBusBootstrapToolsProgressLabel).Text = "Progress: 0%";
								break;
							case 150:
								((ListControl)WorkerComboBox).SelectedIndex = 7;
								((Control)SCIBusBootstrapToolsProgressLabel).Text = "Progress: 0%";
								break;
							case 131:
							case 135:
							case 138:
							case 139:
							case 144:
							case 145:
							case 148:
							case 149:
							case 151:
							case 152:
							case 154:
								break;
							}
						}
						break;
					case 5:
						switch (packet.Payload[0])
						{
						case 0:
							UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + "Bootstrap mode entered successfully.");
							break;
						case 1:
							UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + "Bootstrap status: set baudrate timeout.");
							break;
						case 2:
							UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + "Bootstrap status: set baudrate error.");
							break;
						case 3:
							UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + "Bootstrap status: seed timeout.");
							break;
						case 4:
							UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + "Bootstrap status: seed error.");
							break;
						case 5:
							UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + "Bootstrap status: seed checksum error.");
							break;
						case 6:
							UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + "Bootstrap status: key timeout.");
							break;
						case 7:
							UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + "Bootstrap status: key error.");
							break;
						case 8:
							UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + "Bootstrap status: key checksum error.");
							break;
						case 9:
							UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + "Bootstrap status: invalid key error.");
							break;
						case 10:
							UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + "Bootstrap status: bootloader not supported.");
							break;
						case 12:
							UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + "Bootstrap status: bootloader upload timeout.");
							break;
						case 13:
							UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + "Bootstrap status: bootloader start error.");
							break;
						case 14:
							UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + "Bootstrap status: bootloader start timeout.");
							break;
						case 15:
							UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + "Bootstrap status: bootloader start failed.");
							break;
						default:
							UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + "Bootstrap status: unknown.");
							break;
						}
						break;
					case 6:
						switch (packet.Payload[0])
						{
						case 0:
							UpdateTextBox(SCIBusBootstrapInfoTextBox, " OK.");
							break;
						case 1:
							UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + "Worker status: invalid worker.");
							break;
						case 2:
							UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + "Worker status: no response.");
							break;
						case 3:
							UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + "Worker status: handshake error.");
							break;
						case 4:
							UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + "Worker status: upload error.");
							break;
						case 5:
							UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + "Worker status: upload timeout.");
							break;
						case 6:
							UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + "Worker status: upload interrupted.");
							break;
						case 7:
							UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + "Worker status: upload status error.");
							break;
						}
						break;
					case 7:
						UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + "Start worker.");
						break;
					case 8:
						UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + "Exit worker.");
						break;
					}
					break;
				}
				break;
			case 2:
			case 3:
				if (packet.Payload != null)
				{
					byte[] payload = packet.Payload;
					if (payload == null || payload.Length >= 5)
					{
						byte[] array = packet.Payload.Skip(4).ToArray();
						switch (array[0])
						{
						case 0:
							UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + "Break character received over SCI-bus.");
							break;
						case 6:
							UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + "Set bootstrap baudrate to 62500 baud. OK.");
							break;
						case 17:
							if (array.Length >= 2)
							{
								UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + Environment.NewLine + "Upload worker: " + WorkerComboBox.Items[((ListControl)WorkerComboBox).SelectedIndex].ToString() + ".");
							}
							break;
						case 33:
							if (array.Length < 2)
							{
								UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + "Start worker.");
							}
							else if (!SessionRunning)
							{
								switch (((ListControl)WorkerComboBox).SelectedIndex)
								{
								case 1:
									if (array.Length < 30)
									{
										UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + "Part number: unknown.");
									}
									else
									{
										string text3 = string.Empty;
										if (array[1] != byte.MaxValue)
										{
											FlashChecksum = array[4];
											text3 = Util.ByteToHexString(array, 1, 4).Replace(" ", "");
											text3 = ((array[5] < 65 || array[5] > 90 || array[6] < 65 || array[6] > 90) ? (text3 + "99") : (text3 + Encoding.ASCII.GetString(array, 5, 2)));
										}
										else if (array[21] != byte.MaxValue)
										{
											text3 = Util.ByteToHexString(array, 21, 4).Replace(" ", "");
											text3 = ((array[25] < 65 || array[25] > 90 || array[26] < 65 || array[26] > 90) ? (text3 + "99") : (text3 + Encoding.ASCII.GetString(array, 25, 2)));
											text3 += " (previous)";
										}
										UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + "Part number: ");
										if (text3 != string.Empty)
										{
											UpdateTextBox(SCIBusBootstrapInfoTextBox, text3);
										}
										else
										{
											UpdateTextBox(SCIBusBootstrapInfoTextBox, "unknown.");
										}
									}
									break;
								case 2:
									UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + "FIDs: ");
									if (array.Length < 3)
									{
										UpdateTextBox(SCIBusBootstrapInfoTextBox, "error.");
									}
									else
									{
										UpdateTextBox(SCIBusBootstrapInfoTextBox, Util.ByteToHexString(array, 1, 2));
										UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + "Mfr.: ");
										string text = array[1] switch
										{
											32 => "ST (SGS)", 
											49 => "Catalyst", 
											137 => "Intel", 
											151 => "TI", 
											152 => "Toshiba", 
											_ => Util.ByteToHexString(array, 1), 
										};
										UpdateTextBox(SCIBusBootstrapInfoTextBox, text);
										((Control)FlashChipManufacturerLabel).Text = "Mfr.: " + text;
										UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + "Chip: ");
										string text2;
										switch (array[2])
										{
										case 80:
											text2 = "M28F102 (128k)";
											FlashChipSize = 131072;
											break;
										case 81:
											text2 = "CAT28F102 (128k)";
											FlashChipSize = 131072;
											break;
										case 116:
											text2 = "M28F200T (256k)";
											FlashChipSize = 262144;
											break;
										case 117:
											text2 = "M28F200B (256k)";
											FlashChipSize = 262144;
											break;
										case 180:
											if (((ListControl)BootloaderComboBox).SelectedIndex == 7)
											{
												text2 = "N28F010A (2x 128k)";
												FlashChipSize = 262144;
											}
											else
											{
												text2 = "N28F010A (128k)";
												FlashChipSize = 131072;
											}
											break;
										case 189:
											text2 = "N28F020 (256k)";
											FlashChipSize = 262144;
											break;
										case 224:
											text2 = "M28F210 (256k)";
											FlashChipSize = 262144;
											break;
										case 229:
											text2 = "TMS28F210 (256k)";
											FlashChipSize = 262144;
											break;
										case 230:
											text2 = "M28F220 (256k)";
											FlashChipSize = 262144;
											break;
										default:
											text2 = Util.ByteToHexString(array, 2);
											FlashChipSize = 0;
											break;
										}
										UpdateTextBox(SCIBusBootstrapInfoTextBox, text2);
										((Control)FlashChipIDLabel).Text = "Chip: " + text2;
									}
									break;
								case 4:
									if (array.Length < 2)
									{
										UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + "Flash erase error.");
									}
									else if (array[1] == 34)
									{
										UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + "Flash erased successfully.");
									}
									else
									{
										UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + "Flash erase failed.");
									}
									break;
								case 6:
									if (FlashChecksum == 0)
									{
										UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + "Read part number first for checksum comparison.");
									}
									else
									{
										UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + "Expected checksum: " + Util.ByteToHexStringSimple(new byte[1] { FlashChecksum }));
										UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + "Flash checksum: ");
										if (array.Length < 2)
										{
											UpdateTextBox(SCIBusBootstrapInfoTextBox, "error.");
										}
										else
										{
											UpdateTextBox(SCIBusBootstrapInfoTextBox, Util.ByteToHexString(array, 1));
											if (array[1] == FlashChecksum)
											{
												UpdateTextBox(SCIBusBootstrapInfoTextBox, " <- verified.");
											}
											else
											{
												UpdateTextBox(SCIBusBootstrapInfoTextBox, " <- error, values must be equal.");
											}
										}
									}
									break;
								case 3:
								case 5:
									break;
								}
							}
							break;
						case 34:
							UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + "Exit worker.");
							break;
						case 38:
							if (array.Length == 5)
							{
								if (array[1] == 208 && array[2] == 103 && array[3] == 194 && array[4] == 31)
								{
									UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + "Unlock bootstrap mode security. OK.");
								}
								else
								{
									UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + "Unlock bootstrap mode security. Error.");
								}
							}
							else
							{
								_ = array.Length;
								_ = 7;
							}
							break;
						case 49:
							if (array.Length >= 7)
							{
								List<byte> list4 = new List<byte>();
								List<byte> list5 = new List<byte>();
								List<byte> list6 = new List<byte>();
								list4.AddRange(array.Skip(1).Take(3));
								list5.AddRange(array.Skip(4).Take(2));
								list6.AddRange(array.Skip(6));
								ushort num4 = (ushort)((array[4] << 8) + array[5]);
								ushort num5 = (ushort)(array.Length - 6);
								UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + "Write offset: " + Util.ByteToHexStringSimple(list4.ToArray()) + ". Size: " + Util.ByteToHexStringSimple(list5.ToArray()) + ". ");
								if (num5 == num4)
								{
									UpdateTextBox(SCIBusBootstrapInfoTextBox, "OK.");
									if (FlashChipSize != 0)
									{
										int num6 = (list4[0] << 16) + (list4[1] << 8) + list4[2] + num4;
										((Control)SCIBusBootstrapToolsProgressLabel).Text = "Progress: " + (byte)Math.Round((double)num6 / (double)FlashChipSize * 100.0) + "% (" + num6 + "/" + FlashChipSize + " bytes)";
									}
								}
								else
								{
									switch (array[^1])
									{
									case 1:
										UpdateTextBox(SCIBusBootstrapInfoTextBox, "Write error.");
										break;
									case 128:
										UpdateTextBox(SCIBusBootstrapInfoTextBox, "Invalid block size.");
										break;
									default:
										UpdateTextBox(SCIBusBootstrapInfoTextBox, "Unknown error.");
										break;
									}
								}
							}
							break;
						case 52:
							if (array.Length >= 7)
							{
								List<byte> list7 = new List<byte>();
								List<byte> list8 = new List<byte>();
								List<byte> list9 = new List<byte>();
								list7.AddRange(array.Skip(1).Take(3));
								list8.AddRange(array.Skip(4).Take(2));
								list9.AddRange(array.Skip(6));
								ushort num8 = (ushort)((array[4] << 8) + array[5]);
								ushort num9 = (ushort)(array.Length - 6);
								UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + "Read offset: " + Util.ByteToHexStringSimple(list7.ToArray()) + ". Size: " + Util.ByteToHexStringSimple(list8.ToArray()) + ". ");
								if (num9 == num8)
								{
									UpdateTextBox(SCIBusBootstrapInfoTextBox, "OK.");
									if (SCIBusFlashReadFilePath != null)
									{
										using BinaryWriter binaryWriter2 = new BinaryWriter(File.Open(SCIBusFlashReadFilePath, FileMode.Append));
										binaryWriter2.Write(list9.ToArray());
										binaryWriter2.Close();
									}
									if (FlashChipSize != 0)
									{
										int num10 = (list7[0] << 16) + (list7[1] << 8) + list7[2] + num8;
										((Control)SCIBusBootstrapToolsProgressLabel).Text = "Progress: " + (byte)Math.Round((double)num10 / (double)FlashChipSize * 100.0) + "% (" + num10 + "/" + FlashChipSize + " bytes)";
									}
								}
								else if (array[^1] == 128)
								{
									UpdateTextBox(SCIBusBootstrapInfoTextBox, "Invalid block size.");
								}
								else
								{
									UpdateTextBox(SCIBusBootstrapInfoTextBox, "Unknown error.");
								}
							}
							break;
						case 55:
							if (array.Length >= 6)
							{
								List<byte> list10 = new List<byte>();
								List<byte> list11 = new List<byte>();
								List<byte> list12 = new List<byte>();
								list10.AddRange(array.Skip(1).Take(2));
								list11.AddRange(array.Skip(3).Take(2));
								list12.AddRange(array.Skip(5));
								ushort num11 = (ushort)((array[3] << 8) + array[4]);
								ushort num12 = (ushort)(array.Length - 5);
								UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + "Write offset: " + Util.ByteToHexStringSimple(list10.ToArray()) + ". Size: " + Util.ByteToHexStringSimple(list11.ToArray()) + ". ");
								if (num12 == num11)
								{
									UpdateTextBox(SCIBusBootstrapInfoTextBox, "OK.");
									if (EEPROMSize != 0)
									{
										int num13 = (list10[0] << 8) + list10[1] + num11;
										((Control)SCIBusBootstrapToolsProgressLabel).Text = "Progress: " + (byte)Math.Round((double)num13 / (double)EEPROMSize * 100.0) + "% (" + num13 + "/" + EEPROMSize + " bytes)";
									}
								}
								else
								{
									switch (array[^1])
									{
									case 1:
										UpdateTextBox(SCIBusBootstrapInfoTextBox, "Write error.");
										break;
									case 128:
										UpdateTextBox(SCIBusBootstrapInfoTextBox, "Invalid block size.");
										break;
									case 132:
										UpdateTextBox(SCIBusBootstrapInfoTextBox, "Invalid offset.");
										break;
									default:
										UpdateTextBox(SCIBusBootstrapInfoTextBox, "Unknown error.");
										break;
									}
								}
							}
							break;
						case 58:
							if (array.Length >= 6)
							{
								List<byte> list = new List<byte>();
								List<byte> list2 = new List<byte>();
								List<byte> list3 = new List<byte>();
								list.AddRange(array.Skip(1).Take(2));
								list2.AddRange(array.Skip(3).Take(2));
								list3.AddRange(array.Skip(5));
								ushort num = (ushort)((array[3] << 8) + array[4]);
								ushort num2 = (ushort)(array.Length - 5);
								UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + "Read offset: " + Util.ByteToHexStringSimple(list.ToArray()) + ". Size: " + Util.ByteToHexStringSimple(list2.ToArray()) + ". ");
								if (num2 == num)
								{
									UpdateTextBox(SCIBusBootstrapInfoTextBox, "OK.");
									if (SCIBusEEPROMReadFilePath != null)
									{
										using BinaryWriter binaryWriter = new BinaryWriter(File.Open(SCIBusEEPROMReadFilePath, FileMode.Append));
										binaryWriter.Write(list3.ToArray());
										binaryWriter.Close();
									}
									if (EEPROMSize != 0)
									{
										int num3 = (list[0] << 8) + list[1] + num;
										((Control)SCIBusBootstrapToolsProgressLabel).Text = "Progress: " + (byte)Math.Round((double)num3 / (double)EEPROMSize * 100.0) + "% (" + num3 + "/" + EEPROMSize + " bytes)";
									}
								}
								else
								{
									switch (array[^1])
									{
									case 128:
										UpdateTextBox(SCIBusBootstrapInfoTextBox, "Invalid block size.");
										break;
									case 132:
										UpdateTextBox(SCIBusBootstrapInfoTextBox, "Invalid offset.");
										break;
									default:
										UpdateTextBox(SCIBusBootstrapInfoTextBox, "Unknown error.");
										break;
									}
								}
							}
							break;
						case 71:
							if (array.Length == 4 && array[3] == 34)
							{
								UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + "Start bootloader. OK.");
							}
							else
							{
								UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + "Start bootloader. Error.");
							}
							break;
						case 76:
						{
							UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + "Upload bootloader: " + BootloaderComboBox.Items[((ListControl)BootloaderComboBox).SelectedIndex].ToString() + ". ");
							ushort num7 = (ushort)((array[1] << 8) + array[2]);
							if ((ushort)((array[3] << 8) + array[4]) - num7 + 1 == array.Length - 5)
							{
								UpdateTextBox(SCIBusBootstrapInfoTextBox, "OK.");
							}
							else
							{
								UpdateTextBox(SCIBusBootstrapInfoTextBox, "Error.");
							}
							break;
						}
						case 219:
							if (array.Length == 5 && array[1] == 47 && array[2] == 216 && array[3] == 62 && array[4] == 35)
							{
								UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + "Bootstrap mode is not protected.");
							}
							break;
						}
					}
				}
				break;
			}
		}, null);
		if (PacketReceivedTask == null)
		{
			return;
		}
		TaskCompletionSource<Packet> packetReceivedTask = PacketReceivedTask;
		if (packetReceivedTask == null || !packetReceivedTask.Task.IsCompleted)
		{
			UIContext.Post(delegate
			{
				PacketReceivedTask.SetResult(packet);
			}, null);
		}
	}

	private void DisableControlsForFlashSession()
	{
		((Control)BootloaderComboBox).Enabled = false;
		((Control)BootstrapButton).Enabled = false;
		((Control)WorkerComboBox).Enabled = false;
		((Control)UploadButton).Enabled = false;
		((Control)StartButton).Enabled = false;
		((Control)ExitButton).Enabled = false;
		((Control)FlashChipDetectButton).Enabled = false;
		((Control)FlashBrowseButton).Enabled = false;
		((Control)FlashWriteButton).Enabled = false;
		((Control)FlashReadButton).Enabled = false;
		((Control)FlashMemoryBackupCheckBox).Enabled = false;
		((Control)EEPROMBrowseButton).Enabled = false;
		((Control)EEPROMWriteButton).Enabled = false;
		((Control)EEPROMReadButton).Enabled = false;
		((Control)EEPROMStopButton).Enabled = false;
		((Control)FormatStorageButton).Enabled = false;
	}

	private void DisableControlsForEEPROMSession()
	{
		((Control)BootloaderComboBox).Enabled = false;
		((Control)BootstrapButton).Enabled = false;
		((Control)WorkerComboBox).Enabled = false;
		((Control)UploadButton).Enabled = false;
		((Control)StartButton).Enabled = false;
		((Control)ExitButton).Enabled = false;
		((Control)FlashChipDetectButton).Enabled = false;
		((Control)FlashBrowseButton).Enabled = false;
		((Control)FlashWriteButton).Enabled = false;
		((Control)FlashReadButton).Enabled = false;
		((Control)FlashStopButton).Enabled = false;
		((Control)FlashMemoryBackupCheckBox).Enabled = false;
		((Control)EEPROMBrowseButton).Enabled = false;
		((Control)EEPROMWriteButton).Enabled = false;
		((Control)EEPROMReadButton).Enabled = false;
	}

	private void EnableControls()
	{
		((Control)BootloaderComboBox).Enabled = true;
		((Control)BootstrapButton).Enabled = true;
		((Control)WorkerComboBox).Enabled = true;
		((Control)UploadButton).Enabled = true;
		((Control)StartButton).Enabled = true;
		((Control)ExitButton).Enabled = true;
		((Control)FlashChipDetectButton).Enabled = true;
		((Control)FlashBrowseButton).Enabled = true;
		((Control)FlashWriteButton).Enabled = true;
		((Control)FlashReadButton).Enabled = true;
		((Control)FlashStopButton).Enabled = true;
		((Control)FlashMemoryBackupCheckBox).Enabled = true;
		((Control)EEPROMBrowseButton).Enabled = true;
		((Control)EEPROMWriteButton).Enabled = true;
		((Control)EEPROMReadButton).Enabled = true;
		((Control)EEPROMStopButton).Enabled = true;
		((Control)FormatStorageButton).Enabled = true;
		WorkerComboBox_SelectedIndexChanged(this, EventArgs.Empty);
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
			if (((Control)TB).Name == "SCIBusBootstrapInfoTextBox" && SCIBusBootstrapInfoTextBox != null)
			{
				File.AppendAllText(SCIBusBootstrapLogFilename, text);
			}
		});
	}

	private void BootstrapToolsForm_FormClosing(object sender, FormClosingEventArgs e)
	{
		if (FileUploadWorker.IsBusy)
		{
			FileUploadWorker.CancelAsync();
		}
		if (SwitchBackToLSWhenExit && OriginalForm.PCM.speed == "62500 baud")
		{
			UpdateTextBox(SCIBusBootstrapInfoTextBox, Environment.NewLine + Environment.NewLine + "Scanner SCI-bus speed is set to 7812.5 baud.");
			OriginalForm.SelectSCIBusLSMode();
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
		//IL_0cce: Unknown result type (might be due to invalid IL or missing references)
		//IL_0cd8: Expected O, but got Unknown
		ComponentResourceManager componentResourceManager = new ComponentResourceManager(typeof(BootstrapToolsForm));
		BootModeGroupBox = new GroupBox();
		BootloaderComboBox = new ComboBox();
		BootloaderLabel = new Label();
		BootstrapButton = new Button();
		WorkerGroupBox = new GroupBox();
		StartButton = new Button();
		ExitButton = new Button();
		WorkerComboBox = new ComboBox();
		TaskLabel = new Label();
		UploadButton = new Button();
		FlashChipDetectButton = new Button();
		FlashChipManufacturerLabel = new Label();
		SCIBusBootstrapInfoTextBox = new TextBox();
		FlashMemoryGroupBox = new GroupBox();
		FlashChipIDLabel = new Label();
		FlashMemoryBackupCheckBox = new CheckBox();
		FlashReadButton = new Button();
		FlashStopButton = new Button();
		FlashWriteButton = new Button();
		FlashBrowseButton = new Button();
		FlashFileNameLabel = new Label();
		FlashFileLabel = new Label();
		EEPROMGroupBox = new GroupBox();
		EEPROMSizeLabel = new Label();
		EEPROMReadButton = new Button();
		EEPROMStopButton = new Button();
		EEPROMWriteButton = new Button();
		EEPROMBrowseButton = new Button();
		EEPROMFileNameLabel = new Label();
		EEPROMFileLabel = new Label();
		SCIBusBootstrapToolsProgressLabel = new Label();
		SCIBusBootstrapToolsHelpButton = new Button();
		FormatStorageButton = new Button();
		((Control)BootModeGroupBox).SuspendLayout();
		((Control)WorkerGroupBox).SuspendLayout();
		((Control)FlashMemoryGroupBox).SuspendLayout();
		((Control)EEPROMGroupBox).SuspendLayout();
		((Control)this).SuspendLayout();
		((Control)BootModeGroupBox).Controls.Add((Control)(object)BootloaderComboBox);
		((Control)BootModeGroupBox).Controls.Add((Control)(object)BootloaderLabel);
		((Control)BootModeGroupBox).Controls.Add((Control)(object)BootstrapButton);
		componentResourceManager.ApplyResources(BootModeGroupBox, "BootModeGroupBox");
		((Control)BootModeGroupBox).Name = "BootModeGroupBox";
		BootModeGroupBox.TabStop = false;
		BootloaderComboBox.DropDownStyle = (ComboBoxStyle)2;
		((ListControl)BootloaderComboBox).FormattingEnabled = true;
		BootloaderComboBox.Items.AddRange(new object[9]
		{
			componentResourceManager.GetString("BootloaderComboBox.Items"),
			componentResourceManager.GetString("BootloaderComboBox.Items1"),
			componentResourceManager.GetString("BootloaderComboBox.Items2"),
			componentResourceManager.GetString("BootloaderComboBox.Items3"),
			componentResourceManager.GetString("BootloaderComboBox.Items4"),
			componentResourceManager.GetString("BootloaderComboBox.Items5"),
			componentResourceManager.GetString("BootloaderComboBox.Items6"),
			componentResourceManager.GetString("BootloaderComboBox.Items7"),
			componentResourceManager.GetString("BootloaderComboBox.Items8")
		});
		componentResourceManager.ApplyResources(BootloaderComboBox, "BootloaderComboBox");
		((Control)BootloaderComboBox).Name = "BootloaderComboBox";
		BootloaderComboBox.SelectedIndexChanged += BootloaderComboBox_SelectedIndexChanged;
		componentResourceManager.ApplyResources(BootloaderLabel, "BootloaderLabel");
		((Control)BootloaderLabel).Name = "BootloaderLabel";
		componentResourceManager.ApplyResources(BootstrapButton, "BootstrapButton");
		((Control)BootstrapButton).Name = "BootstrapButton";
		((ButtonBase)BootstrapButton).UseVisualStyleBackColor = true;
		((Control)BootstrapButton).Click += BootstrapButton_Click;
		((Control)WorkerGroupBox).Controls.Add((Control)(object)StartButton);
		((Control)WorkerGroupBox).Controls.Add((Control)(object)ExitButton);
		((Control)WorkerGroupBox).Controls.Add((Control)(object)WorkerComboBox);
		((Control)WorkerGroupBox).Controls.Add((Control)(object)TaskLabel);
		((Control)WorkerGroupBox).Controls.Add((Control)(object)UploadButton);
		componentResourceManager.ApplyResources(WorkerGroupBox, "WorkerGroupBox");
		((Control)WorkerGroupBox).Name = "WorkerGroupBox";
		WorkerGroupBox.TabStop = false;
		componentResourceManager.ApplyResources(StartButton, "StartButton");
		((Control)StartButton).Name = "StartButton";
		((ButtonBase)StartButton).UseVisualStyleBackColor = true;
		((Control)StartButton).Click += StartButton_Click;
		componentResourceManager.ApplyResources(ExitButton, "ExitButton");
		((Control)ExitButton).Name = "ExitButton";
		((ButtonBase)ExitButton).UseVisualStyleBackColor = true;
		((Control)ExitButton).Click += ExitButton_Click;
		WorkerComboBox.DropDownStyle = (ComboBoxStyle)2;
		((ListControl)WorkerComboBox).FormattingEnabled = true;
		WorkerComboBox.Items.AddRange(new object[11]
		{
			componentResourceManager.GetString("WorkerComboBox.Items"),
			componentResourceManager.GetString("WorkerComboBox.Items1"),
			componentResourceManager.GetString("WorkerComboBox.Items2"),
			componentResourceManager.GetString("WorkerComboBox.Items3"),
			componentResourceManager.GetString("WorkerComboBox.Items4"),
			componentResourceManager.GetString("WorkerComboBox.Items5"),
			componentResourceManager.GetString("WorkerComboBox.Items6"),
			componentResourceManager.GetString("WorkerComboBox.Items7"),
			componentResourceManager.GetString("WorkerComboBox.Items8"),
			componentResourceManager.GetString("WorkerComboBox.Items9"),
			componentResourceManager.GetString("WorkerComboBox.Items10")
		});
		componentResourceManager.ApplyResources(WorkerComboBox, "WorkerComboBox");
		((Control)WorkerComboBox).Name = "WorkerComboBox";
		WorkerComboBox.SelectedIndexChanged += WorkerComboBox_SelectedIndexChanged;
		componentResourceManager.ApplyResources(TaskLabel, "TaskLabel");
		((Control)TaskLabel).Name = "TaskLabel";
		componentResourceManager.ApplyResources(UploadButton, "UploadButton");
		((Control)UploadButton).Name = "UploadButton";
		((ButtonBase)UploadButton).UseVisualStyleBackColor = true;
		((Control)UploadButton).Click += UploadButton_Click;
		componentResourceManager.ApplyResources(FlashChipDetectButton, "FlashChipDetectButton");
		((Control)FlashChipDetectButton).Name = "FlashChipDetectButton";
		((ButtonBase)FlashChipDetectButton).UseVisualStyleBackColor = true;
		((Control)FlashChipDetectButton).Click += FlashChipDetectButton_Click;
		componentResourceManager.ApplyResources(FlashChipManufacturerLabel, "FlashChipManufacturerLabel");
		((Control)FlashChipManufacturerLabel).Name = "FlashChipManufacturerLabel";
		((Control)SCIBusBootstrapInfoTextBox).BackColor = SystemColors.Window;
		componentResourceManager.ApplyResources(SCIBusBootstrapInfoTextBox, "SCIBusBootstrapInfoTextBox");
		((Control)SCIBusBootstrapInfoTextBox).Name = "SCIBusBootstrapInfoTextBox";
		((TextBoxBase)SCIBusBootstrapInfoTextBox).ReadOnly = true;
		((Control)FlashMemoryGroupBox).Controls.Add((Control)(object)FlashChipIDLabel);
		((Control)FlashMemoryGroupBox).Controls.Add((Control)(object)FlashMemoryBackupCheckBox);
		((Control)FlashMemoryGroupBox).Controls.Add((Control)(object)FlashReadButton);
		((Control)FlashMemoryGroupBox).Controls.Add((Control)(object)FlashStopButton);
		((Control)FlashMemoryGroupBox).Controls.Add((Control)(object)FlashWriteButton);
		((Control)FlashMemoryGroupBox).Controls.Add((Control)(object)FlashBrowseButton);
		((Control)FlashMemoryGroupBox).Controls.Add((Control)(object)FlashFileNameLabel);
		((Control)FlashMemoryGroupBox).Controls.Add((Control)(object)FlashFileLabel);
		((Control)FlashMemoryGroupBox).Controls.Add((Control)(object)FlashChipDetectButton);
		((Control)FlashMemoryGroupBox).Controls.Add((Control)(object)FlashChipManufacturerLabel);
		componentResourceManager.ApplyResources(FlashMemoryGroupBox, "FlashMemoryGroupBox");
		((Control)FlashMemoryGroupBox).Name = "FlashMemoryGroupBox";
		FlashMemoryGroupBox.TabStop = false;
		componentResourceManager.ApplyResources(FlashChipIDLabel, "FlashChipIDLabel");
		((Control)FlashChipIDLabel).Name = "FlashChipIDLabel";
		componentResourceManager.ApplyResources(FlashMemoryBackupCheckBox, "FlashMemoryBackupCheckBox");
		FlashMemoryBackupCheckBox.Checked = true;
		FlashMemoryBackupCheckBox.CheckState = (CheckState)1;
		((Control)FlashMemoryBackupCheckBox).Name = "FlashMemoryBackupCheckBox";
		((ButtonBase)FlashMemoryBackupCheckBox).UseVisualStyleBackColor = true;
		componentResourceManager.ApplyResources(FlashReadButton, "FlashReadButton");
		((Control)FlashReadButton).Name = "FlashReadButton";
		((ButtonBase)FlashReadButton).UseVisualStyleBackColor = true;
		((Control)FlashReadButton).Click += FlashReadButton_Click;
		componentResourceManager.ApplyResources(FlashStopButton, "FlashStopButton");
		((Control)FlashStopButton).Name = "FlashStopButton";
		((ButtonBase)FlashStopButton).UseVisualStyleBackColor = true;
		((Control)FlashStopButton).Click += FlashStopButton_Click;
		componentResourceManager.ApplyResources(FlashWriteButton, "FlashWriteButton");
		((Control)FlashWriteButton).Name = "FlashWriteButton";
		((ButtonBase)FlashWriteButton).UseVisualStyleBackColor = true;
		((Control)FlashWriteButton).Click += FlashWriteButton_Click;
		componentResourceManager.ApplyResources(FlashBrowseButton, "FlashBrowseButton");
		((Control)FlashBrowseButton).Name = "FlashBrowseButton";
		((ButtonBase)FlashBrowseButton).UseVisualStyleBackColor = true;
		((Control)FlashBrowseButton).Click += FlashBrowseButton_Click;
		componentResourceManager.ApplyResources(FlashFileNameLabel, "FlashFileNameLabel");
		((Control)FlashFileNameLabel).Name = "FlashFileNameLabel";
		componentResourceManager.ApplyResources(FlashFileLabel, "FlashFileLabel");
		((Control)FlashFileLabel).Name = "FlashFileLabel";
		((Control)EEPROMGroupBox).Controls.Add((Control)(object)EEPROMSizeLabel);
		((Control)EEPROMGroupBox).Controls.Add((Control)(object)EEPROMReadButton);
		((Control)EEPROMGroupBox).Controls.Add((Control)(object)EEPROMStopButton);
		((Control)EEPROMGroupBox).Controls.Add((Control)(object)EEPROMWriteButton);
		((Control)EEPROMGroupBox).Controls.Add((Control)(object)EEPROMBrowseButton);
		((Control)EEPROMGroupBox).Controls.Add((Control)(object)EEPROMFileNameLabel);
		((Control)EEPROMGroupBox).Controls.Add((Control)(object)EEPROMFileLabel);
		componentResourceManager.ApplyResources(EEPROMGroupBox, "EEPROMGroupBox");
		((Control)EEPROMGroupBox).Name = "EEPROMGroupBox";
		EEPROMGroupBox.TabStop = false;
		componentResourceManager.ApplyResources(EEPROMSizeLabel, "EEPROMSizeLabel");
		((Control)EEPROMSizeLabel).Name = "EEPROMSizeLabel";
		componentResourceManager.ApplyResources(EEPROMReadButton, "EEPROMReadButton");
		((Control)EEPROMReadButton).Name = "EEPROMReadButton";
		((ButtonBase)EEPROMReadButton).UseVisualStyleBackColor = true;
		((Control)EEPROMReadButton).Click += EEPROMReadButton_Click;
		componentResourceManager.ApplyResources(EEPROMStopButton, "EEPROMStopButton");
		((Control)EEPROMStopButton).Name = "EEPROMStopButton";
		((ButtonBase)EEPROMStopButton).UseVisualStyleBackColor = true;
		((Control)EEPROMStopButton).Click += EEPROMStopButton_Click;
		componentResourceManager.ApplyResources(EEPROMWriteButton, "EEPROMWriteButton");
		((Control)EEPROMWriteButton).Name = "EEPROMWriteButton";
		((ButtonBase)EEPROMWriteButton).UseVisualStyleBackColor = true;
		((Control)EEPROMWriteButton).Click += EEPROMWriteButton_Click;
		componentResourceManager.ApplyResources(EEPROMBrowseButton, "EEPROMBrowseButton");
		((Control)EEPROMBrowseButton).Name = "EEPROMBrowseButton";
		((ButtonBase)EEPROMBrowseButton).UseVisualStyleBackColor = true;
		((Control)EEPROMBrowseButton).Click += EEPROMBrowseButton_Click;
		componentResourceManager.ApplyResources(EEPROMFileNameLabel, "EEPROMFileNameLabel");
		((Control)EEPROMFileNameLabel).Name = "EEPROMFileNameLabel";
		componentResourceManager.ApplyResources(EEPROMFileLabel, "EEPROMFileLabel");
		((Control)EEPROMFileLabel).Name = "EEPROMFileLabel";
		componentResourceManager.ApplyResources(SCIBusBootstrapToolsProgressLabel, "SCIBusBootstrapToolsProgressLabel");
		((Control)SCIBusBootstrapToolsProgressLabel).Name = "SCIBusBootstrapToolsProgressLabel";
		componentResourceManager.ApplyResources(SCIBusBootstrapToolsHelpButton, "SCIBusBootstrapToolsHelpButton");
		((Control)SCIBusBootstrapToolsHelpButton).Name = "SCIBusBootstrapToolsHelpButton";
		((ButtonBase)SCIBusBootstrapToolsHelpButton).UseVisualStyleBackColor = true;
		componentResourceManager.ApplyResources(FormatStorageButton, "FormatStorageButton");
		((Control)FormatStorageButton).Name = "FormatStorageButton";
		((ButtonBase)FormatStorageButton).UseVisualStyleBackColor = true;
		((Control)FormatStorageButton).Click += FormatStorageButton_Click;
		componentResourceManager.ApplyResources(this, "$this");
		((ContainerControl)this).AutoScaleMode = (AutoScaleMode)2;
		((Control)this).Controls.Add((Control)(object)FormatStorageButton);
		((Control)this).Controls.Add((Control)(object)SCIBusBootstrapToolsHelpButton);
		((Control)this).Controls.Add((Control)(object)SCIBusBootstrapToolsProgressLabel);
		((Control)this).Controls.Add((Control)(object)EEPROMGroupBox);
		((Control)this).Controls.Add((Control)(object)FlashMemoryGroupBox);
		((Control)this).Controls.Add((Control)(object)SCIBusBootstrapInfoTextBox);
		((Control)this).Controls.Add((Control)(object)WorkerGroupBox);
		((Control)this).Controls.Add((Control)(object)BootModeGroupBox);
		((Control)this).Name = "BootstrapToolsForm";
		((Form)this).FormClosing += new FormClosingEventHandler(BootstrapToolsForm_FormClosing);
		((Form)this).Load += BootstrapToolsForm_Load;
		((Control)BootModeGroupBox).ResumeLayout(false);
		((Control)BootModeGroupBox).PerformLayout();
		((Control)WorkerGroupBox).ResumeLayout(false);
		((Control)WorkerGroupBox).PerformLayout();
		((Control)FlashMemoryGroupBox).ResumeLayout(false);
		((Control)FlashMemoryGroupBox).PerformLayout();
		((Control)EEPROMGroupBox).ResumeLayout(false);
		((Control)EEPROMGroupBox).PerformLayout();
		((Control)this).ResumeLayout(false);
		((Control)this).PerformLayout();
	}
}
