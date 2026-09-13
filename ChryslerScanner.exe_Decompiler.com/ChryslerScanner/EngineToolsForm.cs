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

public class EngineToolsForm : Form
{
	private enum SCI_ID
	{
		SCIHiSpeed = 18,
		ActuatorTest = 19,
		DiagnosticData = 20,
		ConfigDataSBEC2 = 22,
		ResetMemory = 35,
		ConfigData = 42,
		GetSecuritySeedLegacy = 43,
		GetSecuritySeed = 53,
		SendSecurityKey = 44,
		SCILoSpeed = 254
	}

	private readonly MainForm OriginalForm;

	private readonly SerialService SerialService;

	private readonly SynchronizationContext UIContext;

	private string CSVFilename = string.Empty;

	private string CSVHeader = string.Empty;

	private string CSVLine = string.Empty;

	private byte DiagnosticItemCount;

	private List<byte[]> DiagnosticItems = new List<byte[]>();

	private byte FirstDiagnosticItemID;

	private List<byte[]> WordRequestFilter = new List<byte[]>();

	private byte WordRequestCount;

	private List<byte[]> DWordRequestFilter = new List<byte[]>();

	private byte DWordRequestCount;

	private TaskCompletionSource<Packet> PacketReceivedTask;

	private IContainer components;

	private GroupBox FaultCodeGroupBox;

	private GroupBox BaudrateGroupBox;

	private GroupBox ActuatorTestGroupBox;

	private GroupBox DiagnosticDataGroupBox;

	private Button EraseFaultCodesButton;

	private Button ReadFaultCodesButton;

	private Button Baud62500Button;

	private Button Baud7812Button;

	private ListBox DiagnosticDataListBox;

	private Button ActuatorTestStopButton;

	private Button ActuatorTestStartButton;

	private Button DiagnosticDataStopButton;

	private Button DiagnosticDataReadButton;

	private Button DiagnosticDataClearButton;

	private Label MillisecondsLabel01;

	private TextBox DiagnosticDataRepeatIntervalTextBox;

	private CheckBox DiagnosticDataRepeatIntervalCheckBox;

	private GroupBox SetIdleSpeedGroupBox;

	private TextBox SetIdleSpeedTextBox;

	private TrackBar SetIdleSpeedTrackBar;

	private Button SetIdleSpeedStopButton;

	private Button SetIdleSpeedSetButton;

	private Label RPMLabel;

	private Label IdleSpeedNoteLabel;

	private ComboBox ActuatorTestComboBox;

	private GroupBox ResetMemoryGroupBox;

	private ComboBox ResetMemoryComboBox;

	private Button ResetMemoryOKButton;

	private GroupBox SecurityGroupBox;

	private ComboBox SecurityLevelComboBox;

	private Button SecurityUnlockButton;

	private CheckBox LegacySecurityCheckBox;

	private CheckBox DiagnosticDataCSVCheckBox;

	private GroupBox ConfigurationGroupBox;

	private Button ConfigurationGetAllButton;

	private ComboBox ConfigurationComboBox;

	private Button ConfigurationGetButton;

	private Button ConfigurationGetPartNumberButton;

	private GroupBox RAMTableGroupBox;

	private Button RAMTableSelectButton;

	private ComboBox RAMTableComboBox;

	private Button ReadFaultCodeFreezeFrameButton;

	private GroupBox CHTGroupBox;

	private StatusStrip EngineToolsStatusStrip;

	private ToolStripStatusLabel EnginePropertiesLabel;

	private ComboBox CHTComboBox;

	private Button CHTDetectButton;

	private Label ActuatorTestStatusLabel;

	private Label ResetMemoryStatusLabel;

	private GroupBox SetFuelSyncGroupBox;

	private Button SetFuelSyncStartButton;

	private Label label2;

	private TextBox DistributorSettingTextBox;

	private Label label1;

	private Button SetFuelSyncStopButton;

	private Label FuelSyncInRangeLabel;

	public EngineToolsForm(MainForm IncomingForm, SerialService service)
	{
		OriginalForm = IncomingForm;
		InitializeComponent();
		UIContext = SynchronizationContext.Current;
		((Form)this).Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
		SerialService = service;
		SerialService.PacketReceived += PacketReceivedHandler;
		OriginalForm.ChangeLanguage();
		((ListControl)CHTComboBox).SelectedIndex = OriginalForm.PCM.ControllerHardwareType;
		((ListControl)RAMTableComboBox).SelectedIndex = 4;
		((ListControl)ActuatorTestComboBox).SelectedIndex = 1;
		((ListControl)ResetMemoryComboBox).SelectedIndex = 1;
		((ListControl)SecurityLevelComboBox).SelectedIndex = 0;
		((ListControl)ConfigurationComboBox).SelectedIndex = 1;
		if (OriginalForm.PCM.speed == "7812.5 baud")
		{
			LowSpeedLayout();
			((Control)ReadFaultCodeFreezeFrameButton).Enabled = false;
		}
		else if (OriginalForm.PCM.speed == "62500 baud")
		{
			HighSpeedLayout();
			((Control)ReadFaultCodeFreezeFrameButton).Enabled = true;
			AddHighSpeedDiagnosticData((byte)(240 + ((ListControl)RAMTableComboBox).SelectedIndex));
		}
		WordRequestFilter.Add(new byte[1]);
		WordRequestFilter.Add(new byte[1]);
		WordRequestFilter.Add(new byte[1]);
		WordRequestFilter.Add(new byte[1]);
		WordRequestFilter.Add(new byte[8] { 10, 12, 39, 41, 53, 60, 75, 122 });
		WordRequestFilter.Add(new byte[6] { 30, 71, 73, 75, 77, 203 });
		WordRequestFilter.Add(new byte[1]);
		WordRequestFilter.Add(new byte[1]);
		WordRequestFilter.Add(new byte[28]
		{
			6, 8, 12, 14, 16, 18, 20, 31, 33, 35,
			37, 41, 45, 49, 56, 58, 62, 64, 66, 68,
			70, 81, 83, 85, 87, 91, 95, 99
		});
		WordRequestFilter.Add(new byte[1]);
		WordRequestFilter.Add(new byte[1]);
		WordRequestFilter.Add(new byte[11]
		{
			1, 3, 5, 7, 11, 15, 17, 19, 21, 23,
			25
		});
		WordRequestFilter.Add(new byte[1] { 41 });
		WordRequestFilter.Add(new byte[19]
		{
			33, 35, 39, 41, 128, 130, 132, 134, 136, 138,
			140, 142, 144, 146, 148, 150, 160, 164, 166
		});
		DWordRequestFilter.Add(new byte[1]);
		DWordRequestFilter.Add(new byte[1]);
		DWordRequestFilter.Add(new byte[1]);
		DWordRequestFilter.Add(new byte[1]);
		DWordRequestFilter.Add(new byte[1]);
		DWordRequestFilter.Add(new byte[1]);
		DWordRequestFilter.Add(new byte[1]);
		DWordRequestFilter.Add(new byte[1]);
		DWordRequestFilter.Add(new byte[1]);
		DWordRequestFilter.Add(new byte[1]);
		DWordRequestFilter.Add(new byte[1]);
		DWordRequestFilter.Add(new byte[1]);
		DWordRequestFilter.Add(new byte[12]
		{
			1, 5, 9, 13, 17, 21, 25, 29, 33, 37,
			43, 47
		});
		DWordRequestFilter.Add(new byte[1] { 124 });
		((ContainerControl)this).ActiveControl = (Control)(object)CHTDetectButton;
	}

	private void EngineToolsForm_Load(object sender, EventArgs e)
	{
		UpdateCHTGroup();
		UpdateStatusBar();
	}

	private void CHTDetectButton_Click(object sender, EventArgs e)
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		if (OriginalForm.PCM.speed != "7812.5 baud")
		{
			MessageBox.Show("Detector works in low-speed mode only.", "Warning", (MessageBoxButtons)0, (MessageBoxIcon)48);
			return;
		}
		int num = 50;
		byte[] array = new byte[2]
		{
			(byte)((uint)(num >> 8) & 0xFFu),
			(byte)((uint)num & 0xFFu)
		};
		Packet packet = new Packet();
		if (OriginalForm.PCM.logic == "inverted")
		{
			packet.Bus = 2;
			packet.Command = 6;
			packet.Mode = 3;
			packet.Payload = new byte[11];
			packet.Payload[0] = array[0];
			packet.Payload[1] = array[1];
			for (int i = 0; i < 3; i++)
			{
				packet.Payload[2 + i * 3] = 2;
				packet.Payload[2 + i * 3 + 1] = 22;
				packet.Payload[2 + i * 3 + 1 + 1] = (byte)(128 + i);
			}
		}
		else
		{
			byte[] array2 = new byte[31]
			{
				15, 1, 2, 3, 4, 5, 6, 7, 8, 9,
				10, 11, 12, 13, 14, 16, 17, 18, 19, 20,
				21, 22, 23, 24, 25, 26, 27, 28, 29, 30,
				31
			};
			packet.Bus = 2;
			packet.Command = 6;
			packet.Mode = 3;
			packet.Payload = new byte[95];
			packet.Payload[0] = array[0];
			packet.Payload[1] = array[1];
			for (int j = 0; j < 31; j++)
			{
				packet.Payload[2 + j * 3] = 2;
				packet.Payload[2 + j * 3 + 1] = 42;
				packet.Payload[2 + j * 3 + 1 + 1] = array2[j];
			}
		}
		OriginalForm.TransmitUSBPacket("[<-TX] Information request:", packet);
		SerialService.WritePacket(packet);
	}

	private void CHTComboBox_SelectedIndexChanged(object sender, EventArgs e)
	{
		int selectedIndex = ((ListControl)CHTComboBox).SelectedIndex;
		if (selectedIndex == 9 || (uint)(selectedIndex - 21) <= 3u || selectedIndex == 29)
		{
			((ListControl)RAMTableComboBox).SelectedIndex = 11;
			OriginalForm.PCM.CumminsSelected = true;
		}
		else
		{
			((ListControl)RAMTableComboBox).SelectedIndex = 4;
			OriginalForm.PCM.CumminsSelected = false;
		}
		OriginalForm.PCM.ControllerHardwareType = (byte)((ListControl)CHTComboBox).SelectedIndex;
	}

	private void ReadFaultCodesButton_Click(object sender, EventArgs e)
	{
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		Packet packet = new Packet();
		if (OriginalForm.PCM.speed == "7812.5 baud")
		{
			packet.Bus = 2;
			packet.Command = 6;
			packet.Mode = 3;
			if (!int.TryParse(((Control)DiagnosticDataRepeatIntervalTextBox).Text, out var result) || result == 0)
			{
				result = 50;
				((Control)DiagnosticDataRepeatIntervalTextBox).Text = "50";
			}
			byte[] array = new byte[2]
			{
				(byte)((uint)(result >> 8) & 0xFFu),
				(byte)((uint)result & 0xFFu)
			};
			if (OriginalForm.PCM.CumminsSelected)
			{
				byte[] obj = new byte[6] { 0, 0, 1, 50, 1, 51 };
				obj[0] = array[0];
				obj[1] = array[1];
				packet.Payload = obj;
			}
			else
			{
				byte[] obj2 = new byte[8] { 0, 0, 1, 16, 1, 17, 1, 46 };
				obj2[0] = array[0];
				obj2[1] = array[1];
				packet.Payload = obj2;
			}
		}
		else if (OriginalForm.PCM.speed == "62500 baud")
		{
			if (OriginalForm.PCM.logic == "inverted")
			{
				MessageBox.Show("Select 7812.5 baud to read fault codes on OBD1 vehicles.", "Warning", (MessageBoxButtons)0, (MessageBoxIcon)48);
				return;
			}
			if (!int.TryParse(((Control)DiagnosticDataRepeatIntervalTextBox).Text, out var result2) || result2 == 0)
			{
				result2 = 50;
				((Control)DiagnosticDataRepeatIntervalTextBox).Text = "50";
			}
			byte[] array2 = new byte[2]
			{
				(byte)((uint)(result2 >> 8) & 0xFFu),
				(byte)((uint)result2 & 0xFFu)
			};
			packet.Bus = 2;
			packet.Command = 6;
			packet.Mode = 3;
			if (OriginalForm.PCM.CumminsSelected)
			{
				byte[] obj3 = new byte[26]
				{
					0, 0, 2, 251, 187, 2, 251, 188, 2, 251,
					189, 2, 251, 190, 2, 251, 191, 2, 251, 192,
					2, 251, 193, 2, 251, 194
				};
				obj3[0] = array2[0];
				obj3[1] = array2[1];
				packet.Payload = obj3;
			}
			else
			{
				byte[] obj4 = new byte[26]
				{
					0, 0, 2, 244, 1, 2, 244, 2, 2, 244,
					116, 2, 244, 117, 2, 244, 118, 2, 244, 119,
					2, 244, 120, 2, 244, 121
				};
				obj4[0] = array2[0];
				obj4[1] = array2[1];
				packet.Payload = obj4;
			}
		}
		else
		{
			packet.Bus = 2;
			packet.Command = 6;
			packet.Mode = 2;
			packet.Payload = new byte[1] { 16 };
		}
		OriginalForm.TransmitUSBPacket("[<-TX] PCM fault code list request:", packet);
		SerialService.WritePacket(packet);
	}

	private void EraseFaultCodesButton_Click(object sender, EventArgs e)
	{
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		Packet packet = new Packet();
		if (OriginalForm.PCM.speed == "7812.5 baud")
		{
			packet.Bus = 2;
			packet.Command = 6;
			packet.Mode = 2;
			if (OriginalForm.PCM.CumminsSelected)
			{
				packet.Payload = new byte[2] { 37, 1 };
			}
			else
			{
				packet.Payload = new byte[1] { 23 };
			}
			OriginalForm.TransmitUSBPacket("[<-TX] Erase PCM fault code(s) request:", packet);
			SerialService.WritePacket(packet);
		}
		else
		{
			MessageBox.Show("Fault codes can only be erased in low-speed mode.", "Warning", (MessageBoxButtons)0, (MessageBoxIcon)48);
		}
	}

	private void ReadFaultCodeFreezeFrameButton_Click(object sender, EventArgs e)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		if (OriginalForm.PCM.CumminsSelected)
		{
			MessageBox.Show("Freeze frames are not supported yet.", "Information", (MessageBoxButtons)0, (MessageBoxIcon)64);
			return;
		}
		if (OriginalForm.PCM.logic == "inverted")
		{
			MessageBox.Show("Freeze frames are not supported on OBD1 vehicles.", "Warning", (MessageBoxButtons)0, (MessageBoxIcon)48);
			return;
		}
		if (!int.TryParse(((Control)DiagnosticDataRepeatIntervalTextBox).Text, out var result) || result == 0)
		{
			result = 50;
			((Control)DiagnosticDataRepeatIntervalTextBox).Text = "50";
		}
		byte[] array = new byte[2]
		{
			(byte)((uint)(result >> 8) & 0xFFu),
			(byte)((uint)result & 0xFFu)
		};
		Packet packet = new Packet();
		packet.Bus = 2;
		packet.Command = 6;
		packet.Mode = 3;
		byte[] obj = new byte[48]
		{
			0, 0, 3, 245, 203, 204, 2, 244, 167, 2,
			244, 168, 2, 244, 169, 2, 244, 170, 2, 244,
			171, 2, 244, 172, 2, 244, 173, 2, 244, 174,
			2, 244, 175, 2, 244, 176, 2, 244, 177, 2,
			244, 178, 2, 244, 179, 2, 244, 180
		};
		obj[0] = array[0];
		obj[1] = array[1];
		packet.Payload = obj;
		OriginalForm.TransmitUSBPacket("[<-TX] PCM freeze frame request:", packet);
		SerialService.WritePacket(packet);
	}

	private void Baud7812Button_Click(object sender, EventArgs e)
	{
		Packet packet = new Packet();
		packet.Bus = 2;
		packet.Command = 6;
		packet.Mode = 2;
		packet.Payload = new byte[1] { 254 };
		OriginalForm.TransmitUSBPacket("[<-TX] Select SCI-bus low-speed mode:", packet);
		SerialService.WritePacket(packet);
	}

	private void Baud62500Button_Click(object sender, EventArgs e)
	{
		if (!(OriginalForm.PCM.speed != "7812.5 baud"))
		{
			Packet packet = new Packet();
			packet.Bus = 2;
			packet.Command = 6;
			packet.Mode = 2;
			packet.Payload = new byte[1] { 18 };
			OriginalForm.TransmitUSBPacket("[<-TX] Select SCI-bus high-speed mode:", packet);
			SerialService.WritePacket(packet);
		}
	}

	private void RAMTableSelectButton_Click(object sender, EventArgs e)
	{
		AddHighSpeedDiagnosticData((byte)(240 + ((ListControl)RAMTableComboBox).SelectedIndex));
	}

	private void ActuatorTestStartButton_Click(object sender, EventArgs e)
	{
		Packet packet = new Packet();
		packet.Bus = 2;
		packet.Command = 6;
		packet.Mode = 2;
		packet.Payload = new byte[2]
		{
			19,
			(byte)((ListControl)ActuatorTestComboBox).SelectedIndex
		};
		OriginalForm.TransmitUSBPacket("[<-TX] Start PCM actuator test:", packet);
		SerialService.WritePacket(packet);
	}

	private void ActuatorTestStopButton_Click(object sender, EventArgs e)
	{
		Packet packet = new Packet();
		packet.Bus = 2;
		packet.Command = 6;
		packet.Mode = 2;
		packet.Payload = new byte[2] { 19, 0 };
		OriginalForm.TransmitUSBPacket("[<-TX] Stop PCM actuator test:", packet);
		SerialService.WritePacket(packet);
	}

	private void DiagnosticDataReadButton_Click(object sender, EventArgs e)
	{
		byte b = (byte)DiagnosticDataListBox.SelectedIndices.Count;
		if (b == 0)
		{
			return;
		}
		Packet packet = new Packet();
		if (b == 1 && !DiagnosticDataRepeatIntervalCheckBox.Checked)
		{
			packet.Bus = 2;
			packet.Command = 6;
			packet.Mode = 2;
			if (OriginalForm.PCM.speed == "7812.5 baud")
			{
				packet.Payload = new byte[2]
				{
					20,
					(byte)((ListControl)DiagnosticDataListBox).SelectedIndex
				};
				OriginalForm.TransmitUSBPacket("[<-TX] Request diagnostic data:", packet);
				SerialService.WritePacket(packet);
			}
			else if (OriginalForm.PCM.speed == "62500 baud")
			{
				if (WordRequestFilter[((ListControl)RAMTableComboBox).SelectedIndex].Contains((byte)((ListControl)DiagnosticDataListBox).SelectedIndex))
				{
					packet.Payload = new byte[3]
					{
						(byte)(240 + ((ListControl)RAMTableComboBox).SelectedIndex),
						(byte)((ListControl)DiagnosticDataListBox).SelectedIndex,
						(byte)(((ListControl)DiagnosticDataListBox).SelectedIndex + 1)
					};
				}
				else if (DWordRequestFilter[((ListControl)RAMTableComboBox).SelectedIndex].Contains((byte)((ListControl)DiagnosticDataListBox).SelectedIndex))
				{
					packet.Payload = new byte[5]
					{
						(byte)(240 + ((ListControl)RAMTableComboBox).SelectedIndex),
						(byte)((ListControl)DiagnosticDataListBox).SelectedIndex,
						(byte)(((ListControl)DiagnosticDataListBox).SelectedIndex + 1),
						(byte)(((ListControl)DiagnosticDataListBox).SelectedIndex + 2),
						(byte)(((ListControl)DiagnosticDataListBox).SelectedIndex + 3)
					};
				}
				else
				{
					packet.Payload = new byte[2]
					{
						(byte)(240 + ((ListControl)RAMTableComboBox).SelectedIndex),
						(byte)((ListControl)DiagnosticDataListBox).SelectedIndex
					};
				}
				OriginalForm.TransmitUSBPacket("[<-TX] Request diagnostic data:", packet);
				SerialService.WritePacket(packet);
			}
			return;
		}
		if (!int.TryParse(((Control)DiagnosticDataRepeatIntervalTextBox).Text, out var result) || result == 0)
		{
			result = 50;
			((Control)DiagnosticDataRepeatIntervalTextBox).Text = "50";
		}
		byte[] array = new byte[2]
		{
			(byte)((uint)(result >> 8) & 0xFFu),
			(byte)((uint)result & 0xFFu)
		};
		if (DiagnosticDataRepeatIntervalCheckBox.Checked)
		{
			packet.Mode = 4;
		}
		else
		{
			packet.Mode = 3;
		}
		if (OriginalForm.PCM.speed == "7812.5 baud")
		{
			packet.Payload = new byte[3 * b + 2];
			packet.Payload[0] = array[0];
			packet.Payload[1] = array[1];
			ushort num = 2;
			for (int i = 0; i < b; i++)
			{
				packet.Payload[num] = 2;
				packet.Payload[num + 1] = 20;
				packet.Payload[num + 2] = (byte)DiagnosticDataListBox.SelectedIndices[i];
				num += 3;
			}
			packet.Bus = 2;
			packet.Command = 6;
			OriginalForm.TransmitUSBPacket("[<-TX] Request diagnostic data:", packet);
			SerialService.WritePacket(packet);
		}
		else if (OriginalForm.PCM.speed == "62500 baud")
		{
			WordRequestCount = 0;
			DWordRequestCount = 0;
			for (int j = 0; j < b; j++)
			{
				if (WordRequestFilter[((ListControl)RAMTableComboBox).SelectedIndex].Contains((byte)DiagnosticDataListBox.SelectedIndices[j]))
				{
					WordRequestCount++;
				}
				else if (DWordRequestFilter[((ListControl)RAMTableComboBox).SelectedIndex].Contains((byte)DiagnosticDataListBox.SelectedIndices[j]))
				{
					DWordRequestCount++;
				}
			}
			int num2 = (b - WordRequestCount - DWordRequestCount) * 3 + WordRequestCount * 4 + DWordRequestCount * 6 + 2;
			packet.Payload = new byte[num2];
			packet.Payload[0] = array[0];
			packet.Payload[1] = array[1];
			ushort num3 = 2;
			for (int k = 0; k < b; k++)
			{
				if (WordRequestFilter[((ListControl)RAMTableComboBox).SelectedIndex].Contains((byte)DiagnosticDataListBox.SelectedIndices[k]))
				{
					packet.Payload[num3] = 3;
					packet.Payload[num3 + 1] = (byte)(240 + ((ListControl)RAMTableComboBox).SelectedIndex);
					packet.Payload[num3 + 2] = (byte)DiagnosticDataListBox.SelectedIndices[k];
					packet.Payload[num3 + 3] = (byte)(DiagnosticDataListBox.SelectedIndices[k] + 1);
					num3 += 4;
				}
				else if (DWordRequestFilter[((ListControl)RAMTableComboBox).SelectedIndex].Contains((byte)DiagnosticDataListBox.SelectedIndices[k]))
				{
					packet.Payload[num3] = 5;
					packet.Payload[num3 + 1] = (byte)(240 + ((ListControl)RAMTableComboBox).SelectedIndex);
					packet.Payload[num3 + 2] = (byte)DiagnosticDataListBox.SelectedIndices[k];
					packet.Payload[num3 + 3] = (byte)(DiagnosticDataListBox.SelectedIndices[k] + 1);
					packet.Payload[num3 + 4] = (byte)(DiagnosticDataListBox.SelectedIndices[k] + 2);
					packet.Payload[num3 + 5] = (byte)(DiagnosticDataListBox.SelectedIndices[k] + 3);
					num3 += 6;
				}
				else
				{
					packet.Payload[num3] = 2;
					packet.Payload[num3 + 1] = (byte)(240 + ((ListControl)RAMTableComboBox).SelectedIndex);
					packet.Payload[num3 + 2] = (byte)DiagnosticDataListBox.SelectedIndices[k];
					num3 += 3;
				}
			}
			packet.Bus = 2;
			packet.Command = 6;
			OriginalForm.TransmitUSBPacket("[<-TX] Request diagnostic data:", packet);
			SerialService.WritePacket(packet);
		}
		if (!DiagnosticDataCSVCheckBox.Checked || b <= 0)
		{
			return;
		}
		DiagnosticItemCount = b;
		DiagnosticItems.Clear();
		if (OriginalForm.PCM.speed == "7812.5 baud")
		{
			for (int l = 0; l < b; l++)
			{
				DiagnosticItems.Add(new byte[2]
				{
					20,
					(byte)DiagnosticDataListBox.SelectedIndices[l]
				});
			}
		}
		else if (OriginalForm.PCM.speed == "62500 baud")
		{
			for (int m = 0; m < b; m++)
			{
				if (WordRequestFilter[((ListControl)RAMTableComboBox).SelectedIndex].Contains((byte)DiagnosticDataListBox.SelectedIndices[m]))
				{
					DiagnosticItems.Add(new byte[3]
					{
						(byte)(240 + ((ListControl)RAMTableComboBox).SelectedIndex),
						(byte)DiagnosticDataListBox.SelectedIndices[m],
						(byte)(DiagnosticDataListBox.SelectedIndices[m] + 1)
					});
				}
				else if (DWordRequestFilter[((ListControl)RAMTableComboBox).SelectedIndex].Contains((byte)DiagnosticDataListBox.SelectedIndices[m]))
				{
					DiagnosticItems.Add(new byte[5]
					{
						(byte)(240 + ((ListControl)RAMTableComboBox).SelectedIndex),
						(byte)DiagnosticDataListBox.SelectedIndices[m],
						(byte)(DiagnosticDataListBox.SelectedIndices[m] + 1),
						(byte)(DiagnosticDataListBox.SelectedIndices[m] + 2),
						(byte)(DiagnosticDataListBox.SelectedIndices[m] + 3)
					});
				}
				else
				{
					DiagnosticItems.Add(new byte[2]
					{
						(byte)(240 + ((ListControl)RAMTableComboBox).SelectedIndex),
						(byte)DiagnosticDataListBox.SelectedIndices[m]
					});
				}
			}
		}
		string text = "Milliseconds";
		if (DiagnosticItems.Count == 0)
		{
			return;
		}
		foreach (byte[] diagnosticItem in DiagnosticItems)
		{
			text = text + "," + Util.ByteToHexStringSimple(diagnosticItem);
		}
		FirstDiagnosticItemID = DiagnosticItems[0][1];
		DiagnosticItems.Clear();
		text = text.Replace(" ", "");
		if (text != CSVHeader)
		{
			string text2 = DateTime.Now.ToString("yyyyMMdd_HHmmss");
			CSVFilename = "LOG/PCM/pcmlog_" + text2 + ".csv";
			CSVHeader = text;
			File.AppendAllText(CSVFilename, CSVHeader);
		}
	}

	private void DiagnosticDataStopButton_Click(object sender, EventArgs e)
	{
		DiagnosticItemCount = 0;
		FirstDiagnosticItemID = 0;
		DiagnosticItems.Clear();
		Packet packet = new Packet();
		packet.Bus = 2;
		packet.Command = 6;
		packet.Mode = 1;
		packet.Payload = null;
		OriginalForm.TransmitUSBPacket("[<-TX] Stop diagnostic data request:", packet);
		SerialService.WritePacket(packet);
	}

	private void DiagnosticDataClearButton_Click(object sender, EventArgs e)
	{
		DiagnosticDataListBox.ClearSelected();
	}

	private void SetIdleSpeedSetButton_Click(object sender, EventArgs e)
	{
		Packet packet = new Packet();
		if (!int.TryParse(((Control)SetIdleSpeedTextBox).Text, out var result) || result == 0)
		{
			result = 1500;
			((Control)SetIdleSpeedTextBox).Text = "1500";
			SetIdleSpeedTrackBar.Value = 1500;
		}
		if (result > 2040)
		{
			result = 2040;
			((Control)SetIdleSpeedTextBox).Text = "2040";
			SetIdleSpeedTrackBar.Value = 2040;
		}
		packet = new Packet();
		packet.Bus = 2;
		packet.Command = 6;
		packet.Mode = 4;
		Packet packet2 = packet;
		byte[] obj = new byte[5] { 1, 244, 2, 25, 0 };
		obj[4] = (byte)(result >> 3);
		packet2.Payload = obj;
		OriginalForm.TransmitUSBPacket("[<-TX] Set idle speed:", packet);
		SerialService.WritePacket(packet);
		((Control)SetIdleSpeedStopButton).Focus();
	}

	private void SetIdleSpeedStopButton_Click(object sender, EventArgs e)
	{
		Packet packet = new Packet();
		packet.Bus = 2;
		packet.Command = 6;
		packet.Mode = 1;
		packet.Payload = null;
		OriginalForm.TransmitUSBPacket("[<-TX] Restore default idle speed:", packet);
		SerialService.WritePacket(packet);
		((Control)SetIdleSpeedSetButton).Focus();
	}

	private void SetIdleSpeedTextBox_KeyPress(object sender, KeyPressEventArgs e)
	{
		if (e.KeyChar == '\r')
		{
			e.Handled = true;
			SetIdleSpeedSetButton_Click(this, EventArgs.Empty);
			if (int.TryParse(((Control)SetIdleSpeedTextBox).Text, out var result))
			{
				SetIdleSpeedTrackBar.Value = result;
			}
		}
	}

	private void SetIdleSpeedTrackBar_Scroll(object sender, EventArgs e)
	{
		((Control)SetIdleSpeedTextBox).Text = SetIdleSpeedTrackBar.Value.ToString("0");
	}

	private void ResetMemoryOKButton_Click(object sender, EventArgs e)
	{
		Packet packet = new Packet();
		packet.Bus = 2;
		packet.Command = 6;
		packet.Mode = 2;
		packet.Payload = new byte[2]
		{
			35,
			(byte)((ListControl)ResetMemoryComboBox).SelectedIndex
		};
		OriginalForm.TransmitUSBPacket("[<-TX] Reset memory value:", packet);
		SerialService.WritePacket(packet);
	}

	private void SecurityUnlockButton_Click(object sender, EventArgs e)
	{
		Packet packet = new Packet();
		switch (((ListControl)SecurityLevelComboBox).SelectedIndex)
		{
		case 0:
			packet.Bus = 2;
			packet.Command = 6;
			packet.Mode = 2;
			if (LegacySecurityCheckBox.Checked)
			{
				packet.Payload = new byte[1] { 43 };
			}
			else
			{
				packet.Payload = new byte[2] { 53, 1 };
			}
			OriginalForm.TransmitUSBPacket("[<-TX] Request level 1 security seed:", packet);
			SerialService.WritePacket(packet);
			break;
		case 1:
			packet.Bus = 2;
			packet.Command = 6;
			packet.Mode = 2;
			packet.Payload = new byte[2] { 53, 2 };
			OriginalForm.TransmitUSBPacket("[<-TX] Request level 2 security seed:", packet);
			SerialService.WritePacket(packet);
			break;
		}
	}

	private void LegacySecurityCheckBox_CheckedChanged(object sender, EventArgs e)
	{
		if (LegacySecurityCheckBox.Checked)
		{
			SecurityLevelComboBox.Items[0] = "2B | Lvl 1";
		}
		else
		{
			SecurityLevelComboBox.Items[0] = "3501 | Lvl 1";
		}
	}

	private void ConfigurationGetButton_Click(object sender, EventArgs e)
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		if (OriginalForm.PCM.logic == "inverted")
		{
			MessageBox.Show("This feature is not available on OBD1 vehicles.", "Warning", (MessageBoxButtons)0, (MessageBoxIcon)48);
			return;
		}
		Packet packet = new Packet();
		packet.Bus = 2;
		packet.Command = 6;
		packet.Mode = 2;
		packet.Payload = new byte[2]
		{
			42,
			(byte)((ListControl)ConfigurationComboBox).SelectedIndex
		};
		OriginalForm.TransmitUSBPacket("[<-TX] Information request:", packet);
		SerialService.WritePacket(packet);
	}

	private void ConfigurationGetPartNumberButton_Click(object sender, EventArgs e)
	{
		Packet packet = new Packet();
		if (OriginalForm.PCM.logic == "inverted")
		{
			packet.Bus = 2;
			packet.Command = 6;
			packet.Mode = 2;
			packet.Payload = new byte[2] { 22, 128 };
		}
		else
		{
			int num = 50;
			byte[] array = new byte[2]
			{
				(byte)((uint)(num >> 8) & 0xFFu),
				(byte)((uint)num & 0xFFu)
			};
			packet.Bus = 2;
			packet.Command = 6;
			packet.Mode = 3;
			byte[] obj = new byte[20]
			{
				0, 0, 2, 42, 1, 2, 42, 2, 2, 42,
				3, 2, 42, 4, 2, 42, 23, 2, 42, 24
			};
			obj[0] = array[0];
			obj[1] = array[1];
			packet.Payload = obj;
		}
		OriginalForm.TransmitUSBPacket("[<-TX] PCM part number request:", packet);
		SerialService.WritePacket(packet);
	}

	private void ConfigurationGetAllButton_Click(object sender, EventArgs e)
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		if (OriginalForm.PCM.logic == "inverted")
		{
			MessageBox.Show("This feature is not available on OBD1 vehicles.", "Warning", (MessageBoxButtons)0, (MessageBoxIcon)48);
			return;
		}
		int num = 50;
		byte[] array = new byte[2]
		{
			(byte)((uint)(num >> 8) & 0xFFu),
			(byte)((uint)num & 0xFFu)
		};
		Packet packet = new Packet();
		packet.Bus = 2;
		packet.Command = 6;
		packet.Mode = 3;
		packet.Payload = new byte[95];
		packet.Payload[0] = array[0];
		packet.Payload[1] = array[1];
		for (int i = 0; i < 31; i++)
		{
			packet.Payload[2 + i * 3] = 2;
			packet.Payload[2 + i * 3 + 1] = 42;
			packet.Payload[2 + i * 3 + 1 + 1] = (byte)(i + 1);
		}
		OriginalForm.TransmitUSBPacket("[<-TX] Information request:", packet);
		SerialService.WritePacket(packet);
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
			byte bus = packet.Bus;
			if ((uint)(bus - 2) <= 1u)
			{
				byte[] array = packet.Payload.Skip(4).ToArray();
				if (array.Length != 0)
				{
					if (OriginalForm.PCM.speed == "7812.5 baud")
					{
						switch (array[0])
						{
						case 18:
							HighSpeedLayout();
							AddHighSpeedDiagnosticData((byte)(240 + ((ListControl)RAMTableComboBox).SelectedIndex));
							break;
						case 20:
							if (array.Length < 3)
							{
								if (DiagnosticDataCSVCheckBox.Checked)
								{
									DiagnosticItems.Add(new byte[3] { 20, 0, 0 });
								}
							}
							else
							{
								if (array[0] == 20 && array[1] == 42 && !((Control)SetFuelSyncStartButton).Enabled)
								{
									int num = array[2];
									((Control)DistributorSettingTextBox).Text = num.ToString();
									if (num >= -8 || num <= 8)
									{
										((Control)FuelSyncInRangeLabel).Text = "is in range.";
									}
									else
									{
										((Control)FuelSyncInRangeLabel).Text = "is out of range.";
									}
								}
								if (DiagnosticDataCSVCheckBox.Checked && DiagnosticItemCount > 0)
								{
									DiagnosticItems.Add(array);
									if (DiagnosticItems.Count == 1 && array[1] != FirstDiagnosticItemID)
									{
										DiagnosticItems.Clear();
									}
								}
								if (DiagnosticItems.Count == DiagnosticItemCount && DiagnosticItemCount > 0)
								{
									uint num2 = (uint)((packet.Payload[0] << 24) + (packet.Payload[1] << 16) + (packet.Payload[2] << 8) + packet.Payload[3]);
									CSVLine = num2.ToString();
									foreach (byte[] diagnosticItem in DiagnosticItems)
									{
										CSVLine = CSVLine + "," + diagnosticItem[2];
									}
									File.AppendAllText(CSVFilename, Environment.NewLine + CSVLine);
									DiagnosticItems.Clear();
								}
							}
							break;
						case 19:
							if (array.Length >= 3)
							{
								if (array.Length >= 3 && array[1] != 0 && array[2] == 0)
								{
									((Control)ActuatorTestStatusLabel).Text = "Status: mode not available";
								}
								else if (array[1] == 0 && array[2] == 0)
								{
									((Control)ActuatorTestStatusLabel).Text = "Status: stopped";
								}
								else
								{
									((Control)ActuatorTestStatusLabel).Text = "Status: running";
								}
							}
							break;
						case 22:
							if (array.Length >= 7)
							{
								UpdateCHTGroup();
								UpdateStatusBar();
							}
							break;
						case 35:
							if (array.Length >= 3)
							{
								switch (array[2])
								{
								case 0:
									((Control)ResetMemoryStatusLabel).Text = "Status: stop engine";
									break;
								case 1:
									((Control)ResetMemoryStatusLabel).Text = "Status: mode not available";
									break;
								case 2:
									((Control)ResetMemoryStatusLabel).Text = "Status: denied (module busy)";
									break;
								case 3:
									((Control)ResetMemoryStatusLabel).Text = "Status: denied (security level)";
									break;
								case 240:
									((Control)ResetMemoryStatusLabel).Text = "Status: ok";
									break;
								default:
									((Control)ResetMemoryStatusLabel).Text = "Status: unknown";
									break;
								}
							}
							break;
						case 42:
							if (array.Length >= 3)
							{
								UpdateCHTGroup();
								UpdateStatusBar();
							}
							break;
						case 43:
							if (array.Length >= 4 && array[3] == Util.ChecksumCalculator(array, 0, array.Length - 1) && (array[1] != 0 || array[2] != 0))
							{
								byte[] securityKey = UnlockAlgorithm.GetSecurityKey(UnlockAlgorithm.Controllers.SBEC, UnlockAlgorithm.SecurityLevels.Level1, array.Skip(1).Take(2).ToArray());
								if (securityKey != null)
								{
									byte[] payload2 = new byte[4]
									{
										44,
										securityKey[0],
										securityKey[1],
										(byte)(44 + securityKey[0] + securityKey[1])
									};
									Packet packet3 = new Packet
									{
										Bus = 2,
										Command = 6,
										Mode = 2,
										Payload = payload2
									};
									OriginalForm.TransmitUSBPacket("[<-TX] Send level 1 security key:", packet3);
									SerialService.WritePacket(packet3);
								}
							}
							break;
						case 53:
							if (array.Length >= 5 && array[4] == Util.ChecksumCalculator(array, 0, array.Length - 1) && (array[2] != 0 || array[3] != 0))
							{
								byte[] array2 = null;
								switch (array[1])
								{
								case 1:
									array2 = UnlockAlgorithm.GetSecurityKey(UnlockAlgorithm.Controllers.SBEC, UnlockAlgorithm.SecurityLevels.Level1, array.Skip(2).Take(2).ToArray());
									break;
								case 2:
									array2 = UnlockAlgorithm.GetSecurityKey(UnlockAlgorithm.Controllers.SBEC, UnlockAlgorithm.SecurityLevels.Level2, array.Skip(2).Take(2).ToArray());
									break;
								}
								if (array2 != null)
								{
									byte[] payload = new byte[4]
									{
										44,
										array2[0],
										array2[1],
										(byte)(44 + array2[0] + array2[1])
									};
									Packet packet2 = new Packet
									{
										Bus = 2,
										Command = 6,
										Mode = 2,
										Payload = payload
									};
									switch (array[1])
									{
									case 1:
										OriginalForm.TransmitUSBPacket("[<-TX] Send level 1 security key:", packet2);
										break;
									case 2:
										OriginalForm.TransmitUSBPacket("[<-TX] Send level 2 security key:", packet2);
										break;
									}
									SerialService.WritePacket(packet2);
								}
							}
							break;
						}
					}
					else if (OriginalForm.PCM.speed == "62500 baud")
					{
						if (array[0] >= 240 && array[0] <= 253)
						{
							if (array.Length >= 3)
							{
								if (DiagnosticDataCSVCheckBox.Checked && DiagnosticItemCount > 0)
								{
									DiagnosticItems.Add(array);
									if (DiagnosticItems.Count == 1 && array[1] != FirstDiagnosticItemID)
									{
										DiagnosticItems.Clear();
									}
								}
								if (DiagnosticItems.Count == DiagnosticItemCount && DiagnosticItemCount > 0)
								{
									uint num3 = (uint)((packet.Payload[0] << 24) + (packet.Payload[1] << 16) + (packet.Payload[2] << 8) + packet.Payload[3]);
									CSVLine = num3.ToString();
									foreach (byte[] diagnosticItem2 in DiagnosticItems)
									{
										CSVLine += ",";
										if (diagnosticItem2.Length >= 5 && WordRequestFilter[diagnosticItem2[0] - 240].Contains(diagnosticItem2[1]))
										{
											CSVLine += (diagnosticItem2[2] << 8) + diagnosticItem2[4];
										}
										else if (diagnosticItem2.Length >= 9 && DWordRequestFilter[diagnosticItem2[0] - 240].Contains(diagnosticItem2[1]))
										{
											CSVLine += (uint)((diagnosticItem2[2] << 24) | (diagnosticItem2[4] << 16) | (diagnosticItem2[6] << 8) | diagnosticItem2[8]);
										}
										else
										{
											CSVLine += diagnosticItem2[2];
										}
									}
									File.AppendAllText(CSVFilename, Environment.NewLine + CSVLine);
									DiagnosticItems.Clear();
								}
							}
							else if (array.Length < 3 && DiagnosticDataCSVCheckBox.Checked)
							{
								DiagnosticItems.Add(new byte[3]
								{
									(byte)(240 + ((ListControl)RAMTableComboBox).SelectedIndex),
									0,
									0
								});
							}
						}
						else if (array[0] == 254)
						{
							LowSpeedLayout();
							AddLowSpeedDiagnosticData();
						}
					}
					if (PacketReceivedTask != null)
					{
						TaskCompletionSource<Packet> packetReceivedTask = PacketReceivedTask;
						if (packetReceivedTask == null || !packetReceivedTask.Task.IsCompleted)
						{
							PacketReceivedTask.SetResult(packet);
						}
					}
				}
			}
		}, null);
	}

	private async void SetFuelSyncStartButton_Click(object sender, EventArgs e)
	{
		Packet packet = new Packet
		{
			Bus = 2,
			Command = 6,
			Mode = 2,
			Payload = new byte[2] { 20, 17 }
		};
		OriginalForm.TransmitUSBPacket("[<-TX] Check engine speed:", packet);
		SerialService.WritePacket(packet);
		Packet packet2 = await WaitForResponse(1000);
		if (packet2 == null)
		{
			MessageBox.Show("No response from scanner.", "Warning", (MessageBoxButtons)0, (MessageBoxIcon)48);
			return;
		}
		if (packet2.Payload[6] < 16)
		{
			MessageBox.Show("Engine must be running.", "Information", (MessageBoxButtons)0, (MessageBoxIcon)64);
			return;
		}
		if (packet2.Payload[6] >= 32)
		{
			MessageBox.Show("Engine must be idling.", "Information", (MessageBoxButtons)0, (MessageBoxIcon)64);
			return;
		}
		packet.Payload = new byte[2] { 33, 16 };
		OriginalForm.TransmitUSBPacket("[<-TX] Change ignition timing:", packet);
		SerialService.WritePacket(packet);
		packet2 = await WaitForResponse(1000);
		if (packet2 == null)
		{
			MessageBox.Show("No response from scanner.", "Warning", (MessageBoxButtons)0, (MessageBoxIcon)48);
			return;
		}
		switch (packet2.Payload[6])
		{
		case 2:
			MessageBox.Show("Throttle is open." + Environment.NewLine + "Release throttle pedal and try again.", "Warning", (MessageBoxButtons)0, (MessageBoxIcon)48);
			return;
		case 3:
			MessageBox.Show("Shifter is in drive." + Environment.NewLine + "Put shifter in park or neutral and try again.", "Warning", (MessageBoxButtons)0, (MessageBoxIcon)48);
			return;
		}
		if (packet2.Payload[6] != 1)
		{
			MessageBox.Show("Basic ignition timing could not be enabled.", "Warning", (MessageBoxButtons)0, (MessageBoxIcon)48);
			return;
		}
		packet.Mode = 4;
		packet.Payload = new byte[11]
		{
			0, 50, 2, 25, 125, 2, 20, 17, 2, 20,
			42
		};
		OriginalForm.TransmitUSBPacket("[<-TX] Read set fuel sync:", packet);
		SerialService.WritePacket(packet);
		((Control)SetFuelSyncStartButton).Enabled = false;
		((Control)SetFuelSyncStopButton).Enabled = true;
		((Control)SetFuelSyncStopButton).Focus();
	}

	private async void SetFuelSyncStopButton_Click(object sender, EventArgs e)
	{
		Packet packet = new Packet
		{
			Bus = 2,
			Command = 6,
			Mode = 1
		};
		OriginalForm.TransmitUSBPacket("[<-TX] Stop set fuel sync:", packet);
		SerialService.WritePacket(packet);
		await Task.Delay(300);
		packet.Mode = 2;
		packet.Payload = new byte[2] { 33, 0 };
		OriginalForm.TransmitUSBPacket("[<-TX] Restore ignition timing:", packet);
		SerialService.WritePacket(packet);
		Packet packet2 = await WaitForResponse(1000);
		if (packet2 == null)
		{
			MessageBox.Show("No response from scanner.", "Warning", (MessageBoxButtons)0, (MessageBoxIcon)48);
			return;
		}
		if (packet2.Payload[6] != 0)
		{
			MessageBox.Show("Invalid response from controller.", "Warning", (MessageBoxButtons)0, (MessageBoxIcon)48);
			return;
		}
		((Control)SetFuelSyncStartButton).Enabled = true;
		((Control)SetFuelSyncStopButton).Enabled = false;
		((Control)SetFuelSyncStartButton).Focus();
	}

	private void LowSpeedLayout()
	{
		((Control)CHTGroupBox).Enabled = true;
		((Control)EraseFaultCodesButton).Enabled = true;
		((Control)ReadFaultCodeFreezeFrameButton).Enabled = false;
		((Control)Baud7812Button).BackColor = Color.RoyalBlue;
		((Control)Baud7812Button).ForeColor = Color.White;
		((ButtonBase)Baud7812Button).FlatAppearance.BorderColor = Color.RoyalBlue;
		((ButtonBase)Baud7812Button).FlatAppearance.BorderSize = 1;
		((Control)Baud62500Button).BackColor = SystemColors.ControlLight;
		((Control)Baud62500Button).ForeColor = SystemColors.ControlText;
		((ButtonBase)Baud62500Button).FlatAppearance.BorderColor = SystemColors.ActiveBorder;
		((ButtonBase)Baud62500Button).FlatAppearance.BorderSize = 1;
		((Control)RAMTableGroupBox).Enabled = false;
		((Control)ActuatorTestGroupBox).Enabled = true;
		((Control)DiagnosticDataGroupBox).Enabled = true;
		((Control)SetIdleSpeedGroupBox).Enabled = true;
		((Control)ResetMemoryGroupBox).Enabled = true;
		((Control)ConfigurationGroupBox).Enabled = true;
		((Control)SecurityGroupBox).Enabled = true;
		((Control)SetFuelSyncGroupBox).Enabled = true;
	}

	private void AddLowSpeedDiagnosticData()
	{
		DiagnosticDataListBox.Items.Clear();
		DiagnosticDataListBox.Items.AddRange(new object[128]
		{
			"1400 |", "1401 | Battery temperature sensor voltage", "1402 | Upstream O2 1/1 sensor voltage", "1403 |", "1404 |", "1405 | Engine coolant temperature", "1406 | Engine coolant temperature sensor voltage", "1407 | Throttle position sensor voltage", "1408 | Minimum throttle position sensor voltage", "1409 | Knock sensor 1 voltage",
			"140A | Battery voltage", "140B | Intake manifold absolute pressure (MAP)", "140C | Target IAC stepper motor position", "140D |", "140E | Long term fuel trim 1", "140F | Barometric pressure", "1410 | Minimum air flow test", "1411 | Engine speed", "1412 | Cam/Crank sync sense", "1413 | Key-on cycles error 1",
			"1414 |", "1415 | Spark advance", "1416 | Cylinder 1 retard", "1417 | Cylinder 2 retard", "1418 | Cylinder 3 retard", "1419 | Cylinder 4 retard", "141A | Target boost", "141B | Intake air temperature", "141C | Intake air temperature sensor voltage", "141D | Cruise set speed",
			"141E | Key-on cycles error 2", "141F | Key-on cycles error 3", "1420 | Cruise control status 1", "1421 | Cylinder 1 retard", "1422 |", "1423 |", "1424 | Battery charging voltage", "1425 | Over 5 psi boost timer", "1426 |", "1427 | Vehicle theft alarm status",
			"1428 | Wastegate duty cycle", "1429 | Read fuel setting", "142A | Read set fuel sync", "142B |", "142C | Cruise switch voltage sense", "142D | Ambient/Battery temperature", "142E | Fuel factor (not LH)", "142F | Upstream O2 2/1 sensor voltage", "1430 | Knock sensor 2 voltage", "1431 | Long term fuel trim 2",
			"1432 | A/C high-side pressure sensor voltage", "1433 | A/C high-side pressure", "1434 | Flex fuel sensor voltage", "1435 | Flex fuel info 1", "1436 |", "1437 |", "1438 |", "1439 |", "143A |", "143B | Fuel system status 1",
			"143C |", "143D |", "143E | Calpot voltage", "143F | Downstream O2 1/2 sensor voltage", "1440 | MAP sensor voltage", "1441 | Vehicle speed", "1442 | Upstream O2 1/1 sensor level", "1443 |", "1444 |", "1445 | MAP vacuum",
			"1446 | Throttle position relative", "1447 | Spark advance", "1448 | Upstream O2 2/1 sensor level", "1449 | Downstream O2 2/2 sensor voltage", "144A | Downstream O2 1/2 sensor level", "144B | Downstream O2 2/2 sensor level", "144C |", "144D |", "144E | Fuel level sensor voltage", "144F | Fuel level",
			"1450 |", "1451 |", "1452 |", "1453 |", "1454 |", "1455 |", "1456 |", "1457 | Fuel system status 2", "1458 | Cruise control status 1", "1459 | Cruise control status 2",
			"145A | Output shaft speed", "145B | Governor pressure duty cycle", "145C | Engine load", "145D |", "145E |", "145F | EGR position sensor voltage", "1460 | EGR Zref update D.C.", "1461 |", "1462 |", "1463 |",
			"1464 | Actual purge current", "1465 | Catalyst temperature sensor voltage", "1466 | Catalyst temperature", "1467 |", "1468 |", "1469 | Ambient temperature sensor voltage", "146A |", "146B |", "146C |", "146D | T-case switch voltage",
			"146E |", "146F |", "1470 |", "1471 |", "1472 |", "1473 |", "1474 |", "1475 |", "1476 |", "1477 |",
			"1478 |", "1479 |", "147A | FCA current", "147B |", "147C | Oil temperature sensor voltage", "147D | Oil temperature", "147E |", "147F |"
		});
	}

	private void HighSpeedLayout()
	{
		((Control)CHTGroupBox).Enabled = false;
		((Control)EraseFaultCodesButton).Enabled = false;
		((Control)ReadFaultCodeFreezeFrameButton).Enabled = true;
		((Control)Baud7812Button).BackColor = SystemColors.ControlLight;
		((Control)Baud7812Button).ForeColor = SystemColors.ControlText;
		((ButtonBase)Baud7812Button).FlatAppearance.BorderColor = SystemColors.ActiveBorder;
		((ButtonBase)Baud7812Button).FlatAppearance.BorderSize = 1;
		((Control)Baud62500Button).BackColor = Color.IndianRed;
		((Control)Baud62500Button).ForeColor = Color.White;
		((ButtonBase)Baud62500Button).FlatAppearance.BorderColor = Color.IndianRed;
		((ButtonBase)Baud62500Button).FlatAppearance.BorderSize = 1;
		((Control)RAMTableGroupBox).Enabled = true;
		((Control)ActuatorTestGroupBox).Enabled = false;
		((Control)DiagnosticDataGroupBox).Enabled = true;
		((Control)SetIdleSpeedGroupBox).Enabled = false;
		((Control)ResetMemoryGroupBox).Enabled = false;
		((Control)ConfigurationGroupBox).Enabled = false;
		((Control)SecurityGroupBox).Enabled = false;
		((Control)SetFuelSyncGroupBox).Enabled = false;
	}

	private void AddHighSpeedDiagnosticData(byte RAMTable)
	{
		DiagnosticDataListBox.Items.Clear();
		switch (RAMTable)
		{
		case 240:
			DiagnosticDataListBox.Items.AddRange(new object[1] { "F000 |" });
			break;
		case 241:
			DiagnosticDataListBox.Items.AddRange(new object[1] { "F100 |" });
			break;
		case 242:
			DiagnosticDataListBox.Items.AddRange(new object[1] { "F200 |" });
			break;
		case 243:
			DiagnosticDataListBox.Items.AddRange(new object[1] { "F300 |" });
			break;
		case 244:
			DiagnosticDataListBox.Items.AddRange(new object[240]
			{
				"F400 |", "F401 | DTC 1", "F402 | DTC 8", "F403 | Key-on cycles error 1", "F404 | Key-on cycles error 2", "F405 | Key-on cycles error 3", "F406 | DTC counter 1", "F407 | DTC counter 2", "F408 | DTC counter 3", "F409 | DTC counter 4",
				"F40A0B | Engine speed", "F40B |", "F40C0D | Vehicle speed", "F40D |", "F40E | Cruise button pressed", "F40F | Battery voltage", "F410 | Ambient temperature sensor voltage", "F411 | Ambient temperature", "F412 | Thorttle position sensor (TPS) voltage", "F413 | Minimum TPS voltage",
				"F414 | Calculated TPS voltage", "F415 | Engine coolant temperature sensor voltage", "F416 | Engine coolant temperature", "F417 | Intake MAP sensor voltage", "F418 | Intake manifold absolute pressure", "F419 | Barometric pressure", "F41A | MAP vacuum", "F41B | Upstream O2 1/1 sensor voltage", "F41C | Upstream O2 2/1 sensor voltage", "F41D | Intake air temperature sensor voltage",
				"F41E | Intake air temperature", "F41F | Knock sensor 1 voltage", "F420 | Knock sensor 2 voltage", "F421 | Cruise switch voltage", "F422 | Battery temperature sensor voltage", "F423 | Flex fuel sensor voltage", "F424 | Flex fuel ethanol percentage", "F425 | A/C high-side pressure sensor voltage", "F426 | A/C high-side pressure", "F42728 | Injector pulse width 1",
				"F428 |", "F4292A | Injector pulse width 2", "F42A |", "F42B | Long term fuel trim 1", "F42C | Long term fuel trim 2", "F42D | Engine coolant temperature 2", "F42E | Engine coolant temperature 3", "F42F | Spark advance", "F430 | Total knock retard", "F431 | Cylinder 1 retard",
				"F432 | Cylinder 2 retard", "F433 | Cylinder 3 retard", "F434 | Cylinder 4 retard", "F43536 | Target idle speed", "F436 |", "F437 | Target idle air control motor steps", "F438 |", "F439 |", "F43A | Charging voltage", "F43B | Cruise set speed",
				"F43C3D | Bit state 5", "F43D | ", "F43E | Idle air control motor steps", "F43F | Cruise control status 1", "F440 | Vehicle theft alarm status", "F441 | Crankshaft/Camshaft sync state", "F442 | Fuel system status 1", "F443 | Current adaptive cell ID", "F444 | Short term fuel trim 1", "F445 | Short term fuel trim 2",
				"F446 | Emission settings 1", "F447 | Emission settings 2", "F448 | Downstream O2 1/2 sensor voltage", "F449 | Downstream O2 2/2 sensor voltage", "F44A | Closed loop timer", "F44B4C | Time from start/run", "F44C |", "F44D | Runtime at stall", "F44E | Current fuel shutoff", "F44F | History of fuel shutoff",
				"F450 |", "F451 | Adaptive numerator 1", "F452 |", "F453 |", "F454 |", "F455 |", "F456 |", "F457 | RPM/VSS ratio", "F458 | Transmission selected gear 2", "F459 |",
				"F45A | Dwell coil 1 (cylinders 1 & 4)", "F45B | Dwell coil 2 (cylinders 2 & 3)", "F45C | Dwell coil 3 (cylinders 3 & 6)", "F45D | Fan duty cycle", "F45E |", "F45F |", "F460 | A/C relay state", "F461 | Distance traveled up to 4.2 miles", "F462 |", "F463 |",
				"F464 |", "F465 |", "F466 |", "F467 |", "F468 |", "F469 |", "F46A |", "F46B |", "F46C |", "F46D |",
				"F46E |", "F46F |", "F470 |", "F471 |", "F472 |", "F473 | Limp-in status", "F474 | DTC 2", "F475 | DTC 3", "F476 | DTC 4", "F477 | DTC 5",
				"F478 | DTC 6", "F479 | DTC 7", "F47A7B | SPI transfer result", "F47B |", "F47C |", "F47D |", "F47E |", "F47F |", "F480 |", "F481 |",
				"F482 |", "F483 |", "F484 |", "F485 |", "F486 |", "F487 |", "F488 |", "F489 |", "F48A |", "F48B |",
				"F48C |", "F48D |", "F48E | Bit states 7", "F48F |", "F490 |", "F491 |", "F492 |", "F493 |", "F494 | EGR Zref update D.C.", "F495 | EGR position sensor voltage",
				"F496 | Actual purge current", "F497 |", "F498 | TPS intermittent counter", "F499 |", "F49A |", "F49B | Transmission temperature", "F49C |", "F49D |", "F49E |", "F49F |",
				"F4A0 |", "F4A1 |", "F4A2 |", "F4A3 | Cam timing position", "F4A4 | Engine good trip counter", "F4A5 | Engine warm-up cycle counter", "F4A6 | OBD2 monitor test results 1", "F4A7 | Freeze frame priority level", "F4A8 | Freeze frame DTC", "F4A9 | Fuel system status 1",
				"F4AA | Fuel system status 2", "F4AB | Engine load", "F4AC | Engine coolant temperature", "F4AD | Short term fuel trim 1", "F4AE | Long term fuel trim 1", "F4AF | Short term fuel trim 2", "F4B0 | Long term fuel trum 2", "F4B1 | Intake manifold absolute pressure", "F4B2 | Engine speed", "F4B3 | Vehicle speed",
				"F4B4 | MAP vacuum", "F4B5 | Last emission related DTC stored", "F4B6 | Catalyst temperature sensor voltage", "F4B7 | Purge duty cycle", "F4B8 |", "F4B9 |", "F4BA |", "F4BB |", "F4BC |", "F4BD | Brake switch monitor result 0",
				"F4BE | Brake switch monitor result 1", "F4BF | Brake switch monitor result 2", "F4C0 | Catalyst temperature", "F4C1 | Fuel level status 1", "F4C2 | Fuel level sensor voltage 3", "F4C3 |", "F4C4 |", "F4C5 |", "F4C6 |", "F4C7 |",
				"F4C8 |", "F4C9 |", "F4CA |", "F4CB |", "F4CC |", "F4CD |", "F4CE |", "F4CF |", "F4D0 |", "F4D1 |",
				"F4D2 | Sensor rationality result 0", "F4D3 | Sensor rationality result 1", "F4D4 | O2 sensor rationality result 0", "F4D5 | O2 sensor rationality result 1", "F4D6 | Torque converter clutch monitor", "F4D7 | Sensor rationality result 2", "F4D8 | Sensor rationality result 3", "F4D9 |", "F4DA | P_PCM_NOC_STRDCAMTIME", "F4DB | Fuel level sensor voltage 2",
				"F4DC |", "F4DD | Configuration 1", "F4DE | Fuel level sensor voltage 1", "F4DF | Fuel level", "F4E0 | Fuel used", "F4E1 |", "F4E2 |", "F4E3 | Engine load", "F4E4 | OBD2 monitor test results 2", "F4E5 | Configuration 2",
				"F4E6 |", "F4E7 | Cell #1 - idle cell", "F4E8 | Cell #2 - 1st off idle cell", "F4E9 | Cell #3 - 2nd off idle cell", "F4EA |", "F4EB |", "F4EC | Fuel system status 2", "F4ED | Cruise control status 1", "F4EE | Cruise control satuts 2", "F4EF | Calculated TPS voltage"
			});
			break;
		case 245:
			DiagnosticDataListBox.Items.AddRange(new object[1] { "F500 |" });
			break;
		case 246:
			DiagnosticDataListBox.Items.AddRange(new object[1] { "F600 |" });
			break;
		case 247:
			DiagnosticDataListBox.Items.AddRange(new object[1] { "F700 |" });
			break;
		case 248:
			if (OriginalForm.PCM.Year < 2003 && OriginalForm.PCM.CumminsSelected)
			{
				DiagnosticDataListBox.Items.AddRange(new object[240]
				{
					"F800 | CUMMINS DTC FREEZE FRAMES", "F801 |", "F802 |", "F803 |", "F804 |", "F805 |", "F806 |", "F80708 | Vehicle speed", "F808 |", "F8090A | Engine speed",
					"F80A |", "F80B | Switch state 1", "F80C |", "F80D0E | Engine load", "F80E |", "F80F10 | APP sensor percent", "F810 |", "F81112 | Boost pressure", "F812 |", "F81314 | Engine coolant temperature",
					"F814 |", "F81516 | Intake air temperature", "F816 |", "F81718 | Oil pressure", "F818 |", "F819 | Switch state 2", "F81A |", "F81B |", "F81C |", "F81D |",
					"F81E |", "F81F |", "F820 | Final fuel state", "F82122 | Battery voltage", "F822 |", "F82324 | Calculated fuel", "F824 |", "F82526 | Calculated timing", "F826 |", "F82728 | Regulator valve current",
					"F828 |", "F8292A | Injector pump fuel temperature", "F82A |", "F82B2C | Injector pump engine speed", "F82C |", "F82D |", "F82E |", "F82F30 | Freeze frame DTC 1", "F830 |", "F831 |",
					"F832 |", "F833 |", "F834 |", "F835 |", "F836 |", "F83738 | Vehicle speed", "F838 |", "F8393A | Engine speed", "F83A |", "F83B | Switch state 1",
					"F83C |", "F83D3E | Engine load", "F83E |", "F83F40 | APP sensor percent", "F840 |", "F84142 | Boost pressure", "F842 |", "F84344 | Engine coolant temperature", "F844 |", "F84546 | Intake air temperature",
					"F846 |", "F84748 | Oil pressure", "F808 |", "F849 | Switch state 2", "F84A |", "F84B |", "F84C |", "F84D |", "F84E |", "F84F |",
					"F850 | Final fuel state", "F85152 | Battery voltage", "F852 |", "F85354 | Calculated fuel", "F854 |", "F85556 | Calculated timing", "F856 |", "F85758 | Regulator valve current", "F858 |", "F8595A | Injector pump fuel temperature",
					"F85A |", "F85B5C | Injector pump engine speed", "F85C |", "F85D |", "F85E |", "F85F60 | Freeze frame DTC 2", "F860 |", "F861 |", "F862 |", "F863 |",
					"F864 |", "F865 |", "F866 |", "F867 |", "F868 |", "F869 |", "F86A |", "F86B |", "F86C |", "F86D |",
					"F86E |", "F86F |", "F870 |", "F871 |", "F872 |", "F873 |", "F874 |", "F875 |", "F876 |", "F877 |",
					"F878 |", "F879 |", "F87A |", "F87B |", "F87C |", "F87D |", "F87E |", "F87F |", "F880 |", "F881 |",
					"F882 |", "F883 |", "F884 |", "F885 |", "F886 |", "F887 |", "F888 |", "F889 |", "F88A |", "F88B |",
					"F88C |", "F88D |", "F88E |", "F88F |", "F890 |", "F891 |", "F892 |", "F893 |", "F894 |", "F895 |",
					"F896 |", "F897 |", "F898 |", "F899 |", "F89A |", "F89B |", "F89C |", "F89D |", "F89E |", "F89F |",
					"F8A0 |", "F8A1 |", "F8A2 |", "F8A3 |", "F8A4 |", "F8A5 |", "F8A6 |", "F8A7 |", "F8A8 |", "F8A9 |",
					"F8AA |", "F8AB |", "F8AC |", "F8AD |", "F8AE |", "F8AF |", "F8B0 |", "F8B1 |", "F8B2 |", "F8B3 |",
					"F8B4 |", "F8B5 |", "F8B6 |", "F8B7 |", "F8B8 |", "F8B9 |", "F8BA |", "F8BB |", "F8BC |", "F8BD |",
					"F8BE |", "F8BF |", "F8C0 |", "F8C1 |", "F8C2 |", "F8C3 |", "F8C4 |", "F8C5 |", "F8C6 |", "F8C7 |",
					"F8C8 |", "F8C9 |", "F8CA |", "F8CB |", "F8CC |", "F8CD |", "F8CE |", "F8CF |", "F8D0 |", "F8D1 |",
					"F8D2 |", "F8D3 |", "F8D4 |", "F8D5 |", "F8D6 |", "F8D7 |", "F8D8 |", "F8D9 |", "F8DA |", "F8DB |",
					"F8DC |", "F8DD |", "F8DE |", "F8DF |", "F8E0 |", "F8E1 |", "F8E2 |", "F8E3 |", "F8E4 |", "F8E5 |",
					"F8E6 |", "F8E7 |", "F8E8 |", "F8E9 |", "F8EA |", "F8EB |", "F8EC |", "F8ED |", "F8EE |", "F8EF |"
				});
			}
			else if (OriginalForm.PCM.Year >= 2003 && OriginalForm.PCM.CumminsSelected)
			{
				DiagnosticDataListBox.Items.AddRange(new object[240]
				{
					"F800 | CUMMINS DTC FREEZE FRAMES", "F801 |", "F802 |", "F803 |", "F804 |", "F805 |", "F80607 | Vehicle speed", "F807 |", "F80809 | Engine speed", "F809 |",
					"F80A | Switch state 1", "F80B |", "F80C0D | Engine load", "F80D |", "F80E0F | APP sensor percent", "F80F |", "F81011 | Boost pressure", "F811 |", "F81213 | Engine coolant temperature", "F813 |",
					"F81415 | Intake air temperature", "F815 |", "F816 |", "F817 |", "F818 |", "F819 | Switch state 2", "F81A |", "F81B |", "F81C |", "F81D |",
					"F81E | Final fuel state", "F81F20 | Battery voltage", "F820 |", "F82122 | Calculated fuel", "F822 |", "F82324 | Calculated timing", "F824 |", "F82526 | Regulator valve current", "F826 |", "F827 | Defect status",
					"F828 | Fuel pressure status", "F8292A | Fuel pressure volts", "F82A |", "F82B |", "F82C |", "F82D2E | Fuel level percent", "F82E |", "F82F |", "F830 |", "F83132 | Freeze frame DTC 1",
					"F832 |", "F833 |", "F834 |", "F835 |", "F836 |", "F837 |", "F83839 | Vehicle speed", "F839 |", "F83A3B | Engine speed", "F83B |",
					"F83C | Switch state 1", "F83D |", "F83E3F | Engine load", "F83F |", "F84041 | APP sensor percent", "F841 |", "F84243 | Boost pressure", "F843 |", "F84445 | Engine coolant temperature", "F845 |",
					"F84647 | Intake air temperature", "F847 |", "F848 |", "F849 | Switch state 2", "F84A |", "F84B |", "F84C |", "F84D |", "F84E |", "F84F |",
					"F850 | Final fuel state", "F85152 | Battery voltage", "F852 |", "F85354 | Calculated fuel", "F854 |", "F85556 | Calculated timing", "F856 |", "F85758 | Regulator valve current", "F858 |", "F8595A | Injector pump fuel temperature",
					"F85A |", "F85B5C | Injector pump engine speed", "F85C |", "F85D |", "F85E |", "F85F60 | Freeze frame DTC 2", "F860 |", "F861 |", "F862 |", "F863 |",
					"F864 |", "F865 |", "F866 |", "F867 |", "F868 |", "F869 |", "F86A |", "F86B |", "F86C |", "F86D |",
					"F86E |", "F86F |", "F870 |", "F871 |", "F872 |", "F873 |", "F874 |", "F875 |", "F876 |", "F877 |",
					"F878 |", "F879 |", "F87A |", "F87B |", "F87C |", "F87D |", "F87E |", "F87F |", "F880 |", "F881 |",
					"F882 |", "F883 |", "F884 |", "F885 |", "F886 |", "F887 |", "F888 |", "F889 |", "F88A |", "F88B |",
					"F88C |", "F88D |", "F88E |", "F88F |", "F890 |", "F891 |", "F892 |", "F893 |", "F894 |", "F895 |",
					"F896 |", "F897 |", "F898 |", "F899 |", "F89A |", "F89B |", "F89C |", "F89D |", "F89E |", "F89F |",
					"F8A0 |", "F8A1 |", "F8A2 |", "F8A3 |", "F8A4 |", "F8A5 |", "F8A6 |", "F8A7 |", "F8A8 |", "F8A9 |",
					"F8AA |", "F8AB |", "F8AC |", "F8AD |", "F8AE |", "F8AF |", "F8B0 |", "F8B1 |", "F8B2 |", "F8B3 |",
					"F8B4 |", "F8B5 |", "F8B6 |", "F8B7 |", "F8B8 |", "F8B9 |", "F8BA |", "F8BB |", "F8BC |", "F8BD |",
					"F8BE |", "F8BF |", "F8C0 |", "F8C1 |", "F8C2 |", "F8C3 |", "F8C4 |", "F8C5 |", "F8C6 |", "F8C7 |",
					"F8C8 |", "F8C9 |", "F8CA |", "F8CB |", "F8CC |", "F8CD |", "F8CE |", "F8CF |", "F8D0 |", "F8D1 |",
					"F8D2 |", "F8D3 |", "F8D4 |", "F8D5 |", "F8D6 |", "F8D7 |", "F8D8 |", "F8D9 |", "F8DA |", "F8DB |",
					"F8DC |", "F8DD |", "F8DE |", "F8DF |", "F8E0 |", "F8E1 |", "F8E2 |", "F8E3 |", "F8E4 |", "F8E5 |",
					"F8E6 |", "F8E7 |", "F8E8 |", "F8E9 |", "F8EA |", "F8EB |", "F8EC |", "F8ED |", "F8EE |", "F8EF |"
				});
			}
			else
			{
				DiagnosticDataListBox.Items.AddRange(new object[1] { "F800 |" });
			}
			break;
		case 249:
			DiagnosticDataListBox.Items.AddRange(new object[1] { "F900 |" });
			break;
		case 250:
			DiagnosticDataListBox.Items.AddRange(new object[1] { "FA00 |" });
			break;
		case 251:
			if (OriginalForm.PCM.Year < 2003 && OriginalForm.PCM.CumminsSelected)
			{
				DiagnosticDataListBox.Items.AddRange(new object[240]
				{
					"FB00 | CUMMINS SENSORS", "FB0102 | Engine speed", "FB02 |", "FB0304 | Transmission temperature", "FB04 |", "FB0506 | Vehicle speed", "FB06 | ", "FB0708 | APP sensor percent", "FB08 |", "FB09 |",
					"FB0A |", "FB0B0C | Trans temp sensor volts", "FB0C |", "FB0D |", "FB0E |", "FB0F10 | Engine coolant temperature", "FB10 |", "FB1112 | Engine clnt tmp sensor volts", "FB12 |", "FB1314 | Boost pressure",
					"FB14 |", "FB1516 | Intake air temperature", "FB16 |", "FB1718 | Intake air temp sensor volts", "FB18 |", "FB191A | Battery voltage", "FB1A |", "FB1B1C | Injector pump battery voltage", "FB1C |", "FB1D |",
					"FB1E |", "FB1F20 | Injector pump fuel temperature", "FB20 |", "FB21 |", "FB22 |", "FB23 |", "FB24 |", "FB25 |", "FB26 |", "FB27 |",
					"FB28 |", "FB29 |", "FB2A |", "FB2B |", "FB2C |", "FB2D |", "FB2E |", "FB2F |", "FB30 |", "FB31 |",
					"FB32 |", "FB33 |", "FB34 |", "FB35 |", "FB36 |", "FB37 |", "FB38 |", "FB39 |", "FB3A |", "FB3B |",
					"FB3C |", "FB3D |", "FB3E |", "FB3F |", "FB40 |", "FB41 |", "FB42 |", "FB43 |", "FB44 |", "FB45 |",
					"FB46 |", "FB47 | Switch status", "FB48 |", "FB49 |", "FB4A | Final fuel state", "FB4B |", "FB4C |", "FB4D |", "FB4E |", "FB4F |",
					"FB50 |", "FB5152 | Boost volts", "FB52 |", "FB53 |", "FB54 |", "FB5556 | Water in fuel volts", "FB56 |", "FB5758 | Engine load", "FB58 |", "FB59 |",
					"FB5A |", "FB5B |", "FB5C |", "FB5D |", "FB5E |", "FB5F |", "FB60 |", "FB61 |", "FB62 |", "FB63 |",
					"FB64 |", "FB65 |", "FB66 |", "FB67 |", "FB68 |", "FB69 |", "FB6A |", "FB6B |", "FB6C |", "FB6D |",
					"FB6E |", "FB6F |", "FB70 |", "FB71 |", "FB72 |", "FB73 |", "FB74 |", "FB75 |", "FB76 |", "FB77 |",
					"FB78 |", "FB79 |", "FB7A |", "FB7B |", "FB7C |", "FB7D |", "FB7E |", "FB7F |", "FB80 |", "FB81 |",
					"FB82 |", "FB83 |", "FB84 |", "FB85 |", "FB86 |", "FB87 |", "FB88 |", "FB89 |", "FB8A |", "FB8B |",
					"FB8C |", "FB8D |", "FB8E |", "FB8F |", "FB90 |", "FB91 |", "FB92 |", "FB93 |", "FB94 |", "FB95 |",
					"FB96 |", "FB97 |", "FB98 |", "FB99 |", "FB9A |", "FB9B |", "FB9C |", "FB9D |", "FB9E |", "FB9F |",
					"FBA0 |", "FBA1 |", "FBA2 |", "FBA3 |", "FBA4 |", "FBA5 |", "FBA6 |", "FBA7 |", "FBA8 |", "FBA9 |",
					"FBAA |", "FBAB |", "FBAC |", "FBAD |", "FBAE |", "FBAF |", "FBB0 |", "FBB1 |", "FBB2 |", "FBB3 |",
					"FBB4 |", "FBB5 |", "FBB6 |", "FBB7 |", "FBB8 |", "FBB9 | Battery temperature", "FBBA |", "FBBB |", "FBBC |", "FBBD |",
					"FBBE |", "FBBF |", "FBC0 |", "FBC1 |", "FBC2 |", "FBC3 |", "FBC4 |", "FBC5 |", "FBC6 |", "FBC7 |",
					"FBC8 |", "FBC9 |", "FBCA |", "FBCBCC | Key-on counter", "FBCC |", "FBCDCE | Engine speed CKD sensor", "FBCE |", "FBCFD0 | Engine speed CMP sensor", "FBD0 |", "FBD1 |",
					"FBD2 |", "FBD3 |", "FBD4 |", "FBD5 |", "FBD6 |", "FBD7D8 | APP sensor volts", "FBD8 |", "FBD9 |", "FBDA |", "FBDB |",
					"FBDC | Cruise control denied reason", "FBDD |", "FBDE | Cruise control last cutout reason", "FBDF |", "FBE0 |", "FBE1 |", "FBE2 | Cruise indicator lamp", "FBE3 |", "FBE4 | Cruise button pressed", "FBE5E6 | Cruise set speed",
					"FBE6 |", "FBE7E8 | Cruise switch volts", "FBE8 |", "FBE9 |", "FBEA |", "FBEB |", "FBEC |", "FBED |", "FBEE |", "FBEF |"
				});
			}
			else if (OriginalForm.PCM.Year >= 2003 && OriginalForm.PCM.CumminsSelected)
			{
				DiagnosticDataListBox.Items.AddRange(new object[240]
				{
					"FB00 | CUMMINS SENSORS", "FB0102 | Engine speed", "FB02 |", "FB0304 | Transmission temperature", "FB04 |", "FB0506 | Vehicle speed", "FB06 |", "FB0708 | APP sensor percent", "FB08 |", "FB09 |",
					"FB0A |", "FB0B0C | Trans temp sensor volts", "FB0C |", "FB0D |", "FB0E |", "FB0F10 | Engine coolant temperature", "FB10 |", "FB1112 | Engine clnt tmp sensor volts", "FB12 |", "FB1314 | Boost pressure",
					"FB14 |", "FB1516 | Intake air temperature", "FB16 |", "FB1718 | Intake air temp sensor volts", "FB18 |", "FB191A | Battery voltage", "FB1A |", "FB1B1C | Output shaft speed", "FB1C |", "FB1D1E | Water in fuel counter",
					"FB1E |", "FB1F20 | Transmission PWM duty cycle", "FB20 |", "FB21 |", "FB22 |", "FB23 | Present drive gear", "FB24 |", "FB25 |", "FB26 |", "FB27 |",
					"FB28 |", "FB29 |", "FB2A |", "FB2B |", "FB2C |", "FB2D |", "FB2E |", "FB2F30 | Target governor pressure", "FB30 |", "FB3132 | PPS 1 sensor percent",
					"FB32 |", "FB3334 | PPS 1 sensor volts", "FB34 |", "FB35 |", "FB36 |", "FB37 |", "FB38 |", "FB39 |", "FB3A |", "FB3B |",
					"FB3C |", "FB3D |", "FB3E |", "FB3F40 | PPS 2 sensor percent", "FB40 |", "FB4142 | PPS 2 sensor volts", "FB42 |", "FB43 |", "FB44 |", "FB45 | Idle switch status",
					"FB46 | Brake switch pressed", "FB47 | Switch status", "FB48 | Desired TC clutch status", "FB49 |", "FB4A | Final fuel state", "FB4B |", "FB4C |", "FB4D |", "FB4E |", "FB4F50 | Wastegate duty cycle",
					"FB50 |", "FB5152 | Boots volts", "FB52 |", "FB53 |", "FB54 |", "FB5556 | Water in fuel volts", "FB56 |", "FB5758 | Engine load", "FB58 |", "FB59 |",
					"FB5A |", "FB5B |", "FB5C |", "FB5D |", "FB5E |", "FB5F |", "FB60 |", "FB61 |", "FB62 |", "FB63 |",
					"FB64 |", "FB65 |", "FB66 |", "FB67 |", "FB68 |", "FB69 |", "FB6A |", "FB6B |", "FB6C |", "FB6D |",
					"FB6E |", "FB6F |", "FB70 |", "FB71 |", "FB72 |", "FB73 |", "FB74 |", "FB75 |", "FB76 |", "FB77 |",
					"FB78 |", "FB79 |", "FB7A |", "FB7B |", "FB7C |", "FB7D |", "FB7E |", "FB7F |", "FB80 |", "FB81 |",
					"FB82 |", "FB83 |", "FB84 |", "FB85 |", "FB86 |", "FB87 |", "FB88 |", "FB89 |", "FB8A |", "FB8B |",
					"FB8C |", "FB8D |", "FB8E |", "FB8F |", "FB90 |", "FB91 |", "FB92 |", "FB93 |", "FB94 |", "FB95 |",
					"FB96 |", "FB97 |", "FB98 |", "FB99 |", "FB9A |", "FB9B |", "FB9C |", "FB9D |", "FB9E |", "FB9F |",
					"FBA0 |", "FBA1 |", "FBA2 |", "FBA3 |", "FBA4 |", "FBA5 |", "FBA6 |", "FBA7 |", "FBA8 |", "FBA9 |",
					"FBAA |", "FBAB |", "FBAC |", "FBAD |", "FBAE |", "FBAF |", "FBB0 |", "FBB1 |", "FBB2 |", "FBB3 |",
					"FBB4 |", "FBB5 |", "FBB6 |", "FBB7 |", "FBB8 |", "FBB9BA | Battery temperature", "FBBA |", "FBBB |", "FBBC |", "FBBD |",
					"FBBE |", "FBBF |", "FBC0 |", "FBC1 |", "FBC2 |", "FBC3 |", "FBC4 |", "FBC5 |", "FBC6 |", "FBC7 |",
					"FBC8 |", "FBC9 |", "FBCA |", "FBCBCC | Key-on counter", "FBCC |", "FBCDCE | Engine speed CKD sensor", "FBCE |", "FBCFD0 | Engine speed CMP sensor", "FBD0 |", "FBD1 |",
					"FBD2 | Relay status", "FBD3 |", "FBD4 |", "FBD5 |", "FBD6 |", "FBD7D8 | APP sensor volts", "FBD8 |", "FBD9 |", "FBDA |", "FBDB |",
					"FBDC | Cruise control denied reason", "FBDD |", "FBDE | Cruise control last cutout reason", "FBDF |", "FBE0 |", "FBE1 |", "FBE2 | Cruise indicator lamp", "FBE3 |", "FBE4 | Cruise button pressed", "FBE5E6 | Cruise set speed",
					"FBE6 |", "FBE7E8 | Cruise switch volts", "FBE8 |", "FBE9 |", "FBEA |", "FBEBEC | Injectors disabled vehicle speed", "FBEC |", "FBED |", "FBEE |", "FBEF |"
				});
			}
			else
			{
				DiagnosticDataListBox.Items.AddRange(new object[1] { "FB00 |" });
			}
			break;
		case 252:
			DiagnosticDataListBox.Items.AddRange(new object[240]
			{
				"FC00 | CUMMINS STATISTICS 1", "FC0104 | Total fuel used", "FC02 |", "FC03 |", "FC04 |", "FC0508 | Trip fuel used", "FC06 |", "FC07 |", "FC08 |", "FC090C | Total time",
				"FC0A |", "FC0B |", "FC0C |", "FC0D10 | Trip time", "FC0E |", "FC0F |", "FC10 |", "FC1114 | Total idle fuel", "FC12 |", "FC13 |",
				"FC14 |", "FC1518 | Trip idle fuel", "FC16 |", "FC17 |", "FC18 |", "FC191C | Total idle time", "FC1A |", "FC1B |", "FC1C |", "FC1D20 | Trip idle time",
				"FC1E |", "FC1F |", "FC20 |", "FC2124 | Total distance", "FC22 |", "FC23 |", "FC24 |", "FC2528 | Trip distance", "FC26 |", "FC27 |",
				"FC28 |", "FC292A | Trip average fuel", "FC2A |", "FC2B2E | ECM run time", "FC2C |", "FC2D |", "FC2E |", "FC2F32 | Engine run time", "FC30 |", "FC31 |",
				"FC32 |", "FC33 |", "FC34 |", "FC35 |", "FC36 |", "FC37 |", "FC38 |", "FC39 |", "FC3A |", "FC3B |",
				"FC3C |", "FC3D |", "FC3E |", "FC3F |", "FC40 |", "FC41 |", "FC42 |", "FC43 |", "FC44 |", "FC45 |",
				"FC46 |", "FC47 |", "FC48 |", "FC49 |", "FC4A |", "FC4B |", "FC4C |", "FC4D |", "FC4E |", "FC4F |",
				"FC50 |", "FC51 |", "FC52 |", "FC53 |", "FC54 |", "FC55 |", "FC56 |", "FC57 |", "FC58 |", "FC59 |",
				"FC5A |", "FC5B |", "FC5C |", "FC5D |", "FC5E |", "FC5F |", "FC60 |", "FC61 |", "FC62 |", "FC63 |",
				"FC64 |", "FC65 |", "FC66 |", "FC67 |", "FC68 |", "FC69 |", "FC6A |", "FC6B |", "FC6C |", "FC6D |",
				"FC6E |", "FC6F |", "FC70 |", "FC71 |", "FC72 |", "FC73 |", "FC74 |", "FC75 |", "FC76 |", "FC77 |",
				"FC78 |", "FC79 |", "FC7A |", "FC7B |", "FC7C |", "FC7D |", "FC7E |", "FC7F |", "FC80 |", "FC81 |",
				"FC82 |", "FC83 |", "FC84 |", "FC85 |", "FC86 |", "FC87 |", "FC88 |", "FC89 |", "FC8A |", "FC8B |",
				"FC8C |", "FC8D |", "FC8E |", "FC8F |", "FC90 |", "FC91 |", "FC92 |", "FC93 |", "FC94 |", "FC95 |",
				"FC96 |", "FC97 |", "FC98 |", "FC99 |", "FC9A |", "FC9B |", "FC9C |", "FC9D |", "FC9E |", "FC9F |",
				"FCA0 |", "FCA1 |", "FCA2 |", "FCA3 |", "FCA4 |", "FCA5 |", "FCA6 |", "FCA7 |", "FCA8 |", "FCA9 |",
				"FCAA |", "FCAB |", "FCAC |", "FCAD |", "FCAE |", "FCAF |", "FCB0 |", "FCB1 |", "FCB2 |", "FCB3 |",
				"FCB4 |", "FCB5 |", "FCB6 |", "FCB7 |", "FCB8 |", "FCB9 |", "FCBA |", "FCBB |", "FCBC |", "FCBD |",
				"FCBE |", "FCBF |", "FCC0 |", "FCC1 |", "FCC2 |", "FCC3 |", "FCC4 |", "FCC5 |", "FCC6 |", "FCC7 |",
				"FCC8 |", "FCC9 |", "FCCA |", "FCCB |", "FCCC |", "FCCD |", "FCCE |", "FCCF |", "FCD0 |", "FCD1 |",
				"FCD2 |", "FCD3 |", "FCD4 |", "FCD5 |", "FCD6 |", "FCD7 |", "FCD8 |", "FCD9 |", "FCDA |", "FCDB |",
				"FCDC |", "FCDD |", "FCDE |", "FCDF |", "FCE0 |", "FCE1 |", "FCE2 |", "FCE3 |", "FCE4 |", "FCE5 |",
				"FCE6 |", "FCE7 |", "FCE8 |", "FCE9 |", "FCEA |", "FCEB |", "FCEC |", "FCED |", "FCEE |", "FCEF |"
			});
			break;
		case 253:
			DiagnosticDataListBox.Items.AddRange(new object[240]
			{
				"FD00 | CUMMINS STATISTICS 2", "FD01 |", "FD02 |", "FD03 |", "FD04 |", "FD05 |", "FD06 |", "FD07 |", "FD08 |", "FD09 |",
				"FD0A |", "FD0B |", "FD0C |", "FD0D |", "FD0E |", "FD0F |", "FD10 |", "FD11 |", "FD12 |", "FD13 |",
				"FD14 |", "FD15 |", "FD16 |", "FD17 |", "FD18 |", "FD19 |", "FD1A |", "FD1B |", "FD1C |", "FD1D |",
				"FD1E |", "FD1F |", "FD20 |", "FD2122 | Fuel pressure regulator output", "FD22 |", "FD2324 | Fuel pressure", "FD24 |", "FD25 |", "FD26 |", "FD278 | Fuel pressure setpoint",
				"FD28 |", "FD292A | Fuel pressure sensor volts", "FD2A |", "FD2B |", "FD2C |", "FD2D |", "FD2E |", "FD2F |", "FD30 |", "FD31 |",
				"FD32 |", "FD33 |", "FD34 |", "FD35 |", "FD36 |", "FD37 |", "FD38 |", "FD39 |", "FD3A |", "FD3B |",
				"FD3C |", "FD3D |", "FD3E |", "FD3F |", "FD40 |", "FD41 |", "FD42 |", "FD43 |", "FD44 |", "FD45 |",
				"FD46 |", "FD47 |", "FD48 |", "FD49 |", "FD4A |", "FD4B |", "FD4C |", "FD4D |", "FD4E |", "FD4F |",
				"FD50 |", "FD51 |", "FD52 |", "FD53 |", "FD54 |", "FD55 |", "FD56 |", "FD57 |", "FD58 |", "FD59 |",
				"FD5A |", "FD5B |", "FD5C |", "FD5D |", "FD5E |", "FD5F |", "FD60 |", "FD61 |", "FD62 |", "FD63 |",
				"FD64 |", "FD65 |", "FD66 |", "FD67 |", "FD68 |", "FD69 |", "FD6A |", "FD6B |", "FD6C |", "FD6D |",
				"FD6E |", "FD6F |", "FD70 |", "FD71 |", "FD72 |", "FD73 |", "FD74 |", "FD75 |", "FD76 |", "FD77 |",
				"FD78 |", "FD79 |", "FD7A |", "FD7B |", "FD7C7F | CVN", "FD7D |", "FD7E |", "FD7F |", "FD8081 | Radiator fan speed", "FD81 |",
				"FD8283 | Desired radiator fan PWM", "FD83 |", "FD8485 | % of time @  0-10% LOAD", "FD85 |", "FD8687 | % of time @ 11-20% LOAD", "FD87 |", "FD8889 | % of time @ 21-30% LOAD", "FD89 |", "FD8A8B | % of time @ 31-40% LOAD", "FD8B |",
				"FD8C8D | % of time @ 41-50% LOAD", "FD8D |", "FD8E8F | % of time @ 51-60% LOAD", "FD8F |", "FD9091 | % of time @ 61-70% LOAD", "FD91 |", "FD9293 | % of time @ 71-80% LOAD", "FD93 |", "FD9495 | % of time @ 81-90% LOAD", "FD95 |",
				"FD9697 | % of time @ 91-100% LOAD", "FD97 |", "FD98 |", "FD99 |", "FD9A |", "FD9B |", "FD9C |", "FD9D |", "FD9E |", "FD9F |",
				"FDA0A1 | Barometric pressure", "FDA1 |", "FDA2 |", "FDA3 |", "FDA4A5 | Ambient air temperature", "FDA5 |", "FDA6A7 | Ambient air temp sensor volts", "FDA7 |", "FDA8 |", "FDA9 |",
				"FDAA |", "FDAB |", "FDAC |", "FDAD |", "FDAE |", "FDAF |", "FDB0 |", "FDB1 |", "FDB2 |", "FDB3 |",
				"FDB4 |", "FDB5 |", "FDB6 |", "FDB7 |", "FDB8 |", "FDB9 |", "FDBA |", "FDBB |", "FDBC |", "FDBD |",
				"FDBE |", "FDBF |", "FDC0 |", "FDC1 |", "FDC2 |", "FDC3 |", "FDC4 |", "FDC5 |", "FDC6 |", "FDC7 |",
				"FDC8 |", "FDC9 |", "FDCA |", "FDCB |", "FDCC |", "FDCD |", "FDCE |", "FDCF |", "FDD0 |", "FDD1 |",
				"FDD2 |", "FDD3 |", "FDD4 |", "FDD5 |", "FDD6 |", "FDD7 |", "FDD8 |", "FDD9 |", "FDDA |", "FDDB |",
				"FDDC |", "FDDD |", "FDDE |", "FDDF |", "FDE0 |", "FDE1 | Cylinder 1 contribution", "FDE2 | Cylinder 5 contribution", "FDE3 | Cylinder 3 contribution", "FDE4 | Cylinder 6 contribution", "FDE5 | Cylinder 2 contribution",
				"FDE6 | Cylinder 4 contribution", "FDE7 | Cylinder 1-3 contribution", "FDE8 | Cylinder 4-6 contribution", "FDE9 |", "FDEA | Engine speed", "FDEB | Cylinder test status", "FDEC | FPO test status", "FDED |", "FDEE |", "FDEF |"
			});
			break;
		}
	}

	private void UpdateCHTGroup()
	{
		((ListControl)CHTComboBox).SelectedIndex = OriginalForm.PCM.ControllerHardwareType;
	}

	private void UpdateStatusBar()
	{
		if (OriginalForm.PCM.PartNumberChars[0] == 0)
		{
			((ToolStripItem)EnginePropertiesLabel).Text = "P/N | Year | Body | Manufacturer | Engine | Fuel | Injection" + Environment.NewLine + "Emission | Aspiration | Fans | Chassis ";
			return;
		}
		string text = string.Empty;
		byte value = 0;
		if (Array.IndexOf(OriginalForm.PCM.PartNumberChars.Take(4).ToArray(), value) == -1)
		{
			text = text + " P/N " + Util.ByteToHexString(OriginalForm.PCM.PartNumberChars, 0, 4).Replace(" ", "");
			if (OriginalForm.PCM.PartNumberChars[4] >= 65 && OriginalForm.PCM.PartNumberChars[4] <= 90 && OriginalForm.PCM.PartNumberChars[5] >= 65 && OriginalForm.PCM.PartNumberChars[5] <= 90)
			{
				text += Encoding.ASCII.GetString(OriginalForm.PCM.PartNumberChars.Skip(4).ToArray());
			}
		}
		text += " | ";
		for (int i = 0; i < 6; i++)
		{
			text = text + OriginalForm.PCM.EngineToolsStatusBarTextItems[i] + " | ";
		}
		text = text.Remove(text.Length - 3).TrimEnd(Array.Empty<char>());
		text = text + Environment.NewLine + " ";
		for (int j = 6; j < 11; j++)
		{
			text = text + OriginalForm.PCM.EngineToolsStatusBarTextItems[j] + " | ";
		}
		text = text.Remove(text.Length - 3).TrimEnd(Array.Empty<char>());
		((ToolStripItem)EnginePropertiesLabel).Text = text;
	}

	private void EngineToolsForm_FormClosing(object sender, FormClosingEventArgs e)
	{
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
		//IL_1991: Unknown result type (might be due to invalid IL or missing references)
		//IL_199b: Expected O, but got Unknown
		//IL_2b12: Unknown result type (might be due to invalid IL or missing references)
		//IL_2b1c: Expected O, but got Unknown
		ComponentResourceManager componentResourceManager = new ComponentResourceManager(typeof(EngineToolsForm));
		FaultCodeGroupBox = new GroupBox();
		ReadFaultCodeFreezeFrameButton = new Button();
		EraseFaultCodesButton = new Button();
		ReadFaultCodesButton = new Button();
		BaudrateGroupBox = new GroupBox();
		Baud62500Button = new Button();
		Baud7812Button = new Button();
		ActuatorTestGroupBox = new GroupBox();
		ActuatorTestStatusLabel = new Label();
		ActuatorTestComboBox = new ComboBox();
		ActuatorTestStopButton = new Button();
		ActuatorTestStartButton = new Button();
		DiagnosticDataGroupBox = new GroupBox();
		DiagnosticDataCSVCheckBox = new CheckBox();
		MillisecondsLabel01 = new Label();
		DiagnosticDataRepeatIntervalTextBox = new TextBox();
		DiagnosticDataRepeatIntervalCheckBox = new CheckBox();
		DiagnosticDataClearButton = new Button();
		DiagnosticDataStopButton = new Button();
		DiagnosticDataListBox = new ListBox();
		DiagnosticDataReadButton = new Button();
		SetIdleSpeedGroupBox = new GroupBox();
		IdleSpeedNoteLabel = new Label();
		RPMLabel = new Label();
		SetIdleSpeedTextBox = new TextBox();
		SetIdleSpeedTrackBar = new TrackBar();
		SetIdleSpeedStopButton = new Button();
		SetIdleSpeedSetButton = new Button();
		ResetMemoryGroupBox = new GroupBox();
		ResetMemoryStatusLabel = new Label();
		ResetMemoryComboBox = new ComboBox();
		ResetMemoryOKButton = new Button();
		SecurityGroupBox = new GroupBox();
		LegacySecurityCheckBox = new CheckBox();
		SecurityLevelComboBox = new ComboBox();
		SecurityUnlockButton = new Button();
		ConfigurationGroupBox = new GroupBox();
		ConfigurationGetPartNumberButton = new Button();
		ConfigurationGetAllButton = new Button();
		ConfigurationComboBox = new ComboBox();
		ConfigurationGetButton = new Button();
		RAMTableGroupBox = new GroupBox();
		RAMTableComboBox = new ComboBox();
		RAMTableSelectButton = new Button();
		CHTGroupBox = new GroupBox();
		CHTDetectButton = new Button();
		CHTComboBox = new ComboBox();
		EngineToolsStatusStrip = new StatusStrip();
		EnginePropertiesLabel = new ToolStripStatusLabel();
		SetFuelSyncGroupBox = new GroupBox();
		FuelSyncInRangeLabel = new Label();
		label2 = new Label();
		DistributorSettingTextBox = new TextBox();
		label1 = new Label();
		SetFuelSyncStopButton = new Button();
		SetFuelSyncStartButton = new Button();
		((Control)FaultCodeGroupBox).SuspendLayout();
		((Control)BaudrateGroupBox).SuspendLayout();
		((Control)ActuatorTestGroupBox).SuspendLayout();
		((Control)DiagnosticDataGroupBox).SuspendLayout();
		((Control)SetIdleSpeedGroupBox).SuspendLayout();
		((ISupportInitialize)SetIdleSpeedTrackBar).BeginInit();
		((Control)ResetMemoryGroupBox).SuspendLayout();
		((Control)SecurityGroupBox).SuspendLayout();
		((Control)ConfigurationGroupBox).SuspendLayout();
		((Control)RAMTableGroupBox).SuspendLayout();
		((Control)CHTGroupBox).SuspendLayout();
		((Control)EngineToolsStatusStrip).SuspendLayout();
		((Control)SetFuelSyncGroupBox).SuspendLayout();
		((Control)this).SuspendLayout();
		((Control)FaultCodeGroupBox).Controls.Add((Control)(object)ReadFaultCodeFreezeFrameButton);
		((Control)FaultCodeGroupBox).Controls.Add((Control)(object)EraseFaultCodesButton);
		((Control)FaultCodeGroupBox).Controls.Add((Control)(object)ReadFaultCodesButton);
		componentResourceManager.ApplyResources(FaultCodeGroupBox, "FaultCodeGroupBox");
		((Control)FaultCodeGroupBox).Name = "FaultCodeGroupBox";
		FaultCodeGroupBox.TabStop = false;
		componentResourceManager.ApplyResources(ReadFaultCodeFreezeFrameButton, "ReadFaultCodeFreezeFrameButton");
		((Control)ReadFaultCodeFreezeFrameButton).Name = "ReadFaultCodeFreezeFrameButton";
		((ButtonBase)ReadFaultCodeFreezeFrameButton).UseVisualStyleBackColor = true;
		((Control)ReadFaultCodeFreezeFrameButton).Click += ReadFaultCodeFreezeFrameButton_Click;
		componentResourceManager.ApplyResources(EraseFaultCodesButton, "EraseFaultCodesButton");
		((Control)EraseFaultCodesButton).Name = "EraseFaultCodesButton";
		((ButtonBase)EraseFaultCodesButton).UseVisualStyleBackColor = true;
		((Control)EraseFaultCodesButton).Click += EraseFaultCodesButton_Click;
		componentResourceManager.ApplyResources(ReadFaultCodesButton, "ReadFaultCodesButton");
		((Control)ReadFaultCodesButton).Name = "ReadFaultCodesButton";
		((ButtonBase)ReadFaultCodesButton).UseVisualStyleBackColor = true;
		((Control)ReadFaultCodesButton).Click += ReadFaultCodesButton_Click;
		((Control)BaudrateGroupBox).Controls.Add((Control)(object)Baud62500Button);
		((Control)BaudrateGroupBox).Controls.Add((Control)(object)Baud7812Button);
		componentResourceManager.ApplyResources(BaudrateGroupBox, "BaudrateGroupBox");
		((Control)BaudrateGroupBox).Name = "BaudrateGroupBox";
		BaudrateGroupBox.TabStop = false;
		componentResourceManager.ApplyResources(Baud62500Button, "Baud62500Button");
		((Control)Baud62500Button).Name = "Baud62500Button";
		((ButtonBase)Baud62500Button).UseVisualStyleBackColor = true;
		((Control)Baud62500Button).Click += Baud62500Button_Click;
		componentResourceManager.ApplyResources(Baud7812Button, "Baud7812Button");
		((Control)Baud7812Button).Name = "Baud7812Button";
		((ButtonBase)Baud7812Button).UseVisualStyleBackColor = true;
		((Control)Baud7812Button).Click += Baud7812Button_Click;
		((Control)ActuatorTestGroupBox).Controls.Add((Control)(object)ActuatorTestStatusLabel);
		((Control)ActuatorTestGroupBox).Controls.Add((Control)(object)ActuatorTestComboBox);
		((Control)ActuatorTestGroupBox).Controls.Add((Control)(object)ActuatorTestStopButton);
		((Control)ActuatorTestGroupBox).Controls.Add((Control)(object)ActuatorTestStartButton);
		componentResourceManager.ApplyResources(ActuatorTestGroupBox, "ActuatorTestGroupBox");
		((Control)ActuatorTestGroupBox).Name = "ActuatorTestGroupBox";
		ActuatorTestGroupBox.TabStop = false;
		componentResourceManager.ApplyResources(ActuatorTestStatusLabel, "ActuatorTestStatusLabel");
		((Control)ActuatorTestStatusLabel).Name = "ActuatorTestStatusLabel";
		ActuatorTestComboBox.DropDownHeight = 226;
		ActuatorTestComboBox.DropDownStyle = (ComboBoxStyle)2;
		componentResourceManager.ApplyResources(ActuatorTestComboBox, "ActuatorTestComboBox");
		((ListControl)ActuatorTestComboBox).FormattingEnabled = true;
		ActuatorTestComboBox.Items.AddRange(new object[128]
		{
			componentResourceManager.GetString("ActuatorTestComboBox.Items"),
			componentResourceManager.GetString("ActuatorTestComboBox.Items1"),
			componentResourceManager.GetString("ActuatorTestComboBox.Items2"),
			componentResourceManager.GetString("ActuatorTestComboBox.Items3"),
			componentResourceManager.GetString("ActuatorTestComboBox.Items4"),
			componentResourceManager.GetString("ActuatorTestComboBox.Items5"),
			componentResourceManager.GetString("ActuatorTestComboBox.Items6"),
			componentResourceManager.GetString("ActuatorTestComboBox.Items7"),
			componentResourceManager.GetString("ActuatorTestComboBox.Items8"),
			componentResourceManager.GetString("ActuatorTestComboBox.Items9"),
			componentResourceManager.GetString("ActuatorTestComboBox.Items10"),
			componentResourceManager.GetString("ActuatorTestComboBox.Items11"),
			componentResourceManager.GetString("ActuatorTestComboBox.Items12"),
			componentResourceManager.GetString("ActuatorTestComboBox.Items13"),
			componentResourceManager.GetString("ActuatorTestComboBox.Items14"),
			componentResourceManager.GetString("ActuatorTestComboBox.Items15"),
			componentResourceManager.GetString("ActuatorTestComboBox.Items16"),
			componentResourceManager.GetString("ActuatorTestComboBox.Items17"),
			componentResourceManager.GetString("ActuatorTestComboBox.Items18"),
			componentResourceManager.GetString("ActuatorTestComboBox.Items19"),
			componentResourceManager.GetString("ActuatorTestComboBox.Items20"),
			componentResourceManager.GetString("ActuatorTestComboBox.Items21"),
			componentResourceManager.GetString("ActuatorTestComboBox.Items22"),
			componentResourceManager.GetString("ActuatorTestComboBox.Items23"),
			componentResourceManager.GetString("ActuatorTestComboBox.Items24"),
			componentResourceManager.GetString("ActuatorTestComboBox.Items25"),
			componentResourceManager.GetString("ActuatorTestComboBox.Items26"),
			componentResourceManager.GetString("ActuatorTestComboBox.Items27"),
			componentResourceManager.GetString("ActuatorTestComboBox.Items28"),
			componentResourceManager.GetString("ActuatorTestComboBox.Items29"),
			componentResourceManager.GetString("ActuatorTestComboBox.Items30"),
			componentResourceManager.GetString("ActuatorTestComboBox.Items31"),
			componentResourceManager.GetString("ActuatorTestComboBox.Items32"),
			componentResourceManager.GetString("ActuatorTestComboBox.Items33"),
			componentResourceManager.GetString("ActuatorTestComboBox.Items34"),
			componentResourceManager.GetString("ActuatorTestComboBox.Items35"),
			componentResourceManager.GetString("ActuatorTestComboBox.Items36"),
			componentResourceManager.GetString("ActuatorTestComboBox.Items37"),
			componentResourceManager.GetString("ActuatorTestComboBox.Items38"),
			componentResourceManager.GetString("ActuatorTestComboBox.Items39"),
			componentResourceManager.GetString("ActuatorTestComboBox.Items40"),
			componentResourceManager.GetString("ActuatorTestComboBox.Items41"),
			componentResourceManager.GetString("ActuatorTestComboBox.Items42"),
			componentResourceManager.GetString("ActuatorTestComboBox.Items43"),
			componentResourceManager.GetString("ActuatorTestComboBox.Items44"),
			componentResourceManager.GetString("ActuatorTestComboBox.Items45"),
			componentResourceManager.GetString("ActuatorTestComboBox.Items46"),
			componentResourceManager.GetString("ActuatorTestComboBox.Items47"),
			componentResourceManager.GetString("ActuatorTestComboBox.Items48"),
			componentResourceManager.GetString("ActuatorTestComboBox.Items49"),
			componentResourceManager.GetString("ActuatorTestComboBox.Items50"),
			componentResourceManager.GetString("ActuatorTestComboBox.Items51"),
			componentResourceManager.GetString("ActuatorTestComboBox.Items52"),
			componentResourceManager.GetString("ActuatorTestComboBox.Items53"),
			componentResourceManager.GetString("ActuatorTestComboBox.Items54"),
			componentResourceManager.GetString("ActuatorTestComboBox.Items55"),
			componentResourceManager.GetString("ActuatorTestComboBox.Items56"),
			componentResourceManager.GetString("ActuatorTestComboBox.Items57"),
			componentResourceManager.GetString("ActuatorTestComboBox.Items58"),
			componentResourceManager.GetString("ActuatorTestComboBox.Items59"),
			componentResourceManager.GetString("ActuatorTestComboBox.Items60"),
			componentResourceManager.GetString("ActuatorTestComboBox.Items61"),
			componentResourceManager.GetString("ActuatorTestComboBox.Items62"),
			componentResourceManager.GetString("ActuatorTestComboBox.Items63"),
			componentResourceManager.GetString("ActuatorTestComboBox.Items64"),
			componentResourceManager.GetString("ActuatorTestComboBox.Items65"),
			componentResourceManager.GetString("ActuatorTestComboBox.Items66"),
			componentResourceManager.GetString("ActuatorTestComboBox.Items67"),
			componentResourceManager.GetString("ActuatorTestComboBox.Items68"),
			componentResourceManager.GetString("ActuatorTestComboBox.Items69"),
			componentResourceManager.GetString("ActuatorTestComboBox.Items70"),
			componentResourceManager.GetString("ActuatorTestComboBox.Items71"),
			componentResourceManager.GetString("ActuatorTestComboBox.Items72"),
			componentResourceManager.GetString("ActuatorTestComboBox.Items73"),
			componentResourceManager.GetString("ActuatorTestComboBox.Items74"),
			componentResourceManager.GetString("ActuatorTestComboBox.Items75"),
			componentResourceManager.GetString("ActuatorTestComboBox.Items76"),
			componentResourceManager.GetString("ActuatorTestComboBox.Items77"),
			componentResourceManager.GetString("ActuatorTestComboBox.Items78"),
			componentResourceManager.GetString("ActuatorTestComboBox.Items79"),
			componentResourceManager.GetString("ActuatorTestComboBox.Items80"),
			componentResourceManager.GetString("ActuatorTestComboBox.Items81"),
			componentResourceManager.GetString("ActuatorTestComboBox.Items82"),
			componentResourceManager.GetString("ActuatorTestComboBox.Items83"),
			componentResourceManager.GetString("ActuatorTestComboBox.Items84"),
			componentResourceManager.GetString("ActuatorTestComboBox.Items85"),
			componentResourceManager.GetString("ActuatorTestComboBox.Items86"),
			componentResourceManager.GetString("ActuatorTestComboBox.Items87"),
			componentResourceManager.GetString("ActuatorTestComboBox.Items88"),
			componentResourceManager.GetString("ActuatorTestComboBox.Items89"),
			componentResourceManager.GetString("ActuatorTestComboBox.Items90"),
			componentResourceManager.GetString("ActuatorTestComboBox.Items91"),
			componentResourceManager.GetString("ActuatorTestComboBox.Items92"),
			componentResourceManager.GetString("ActuatorTestComboBox.Items93"),
			componentResourceManager.GetString("ActuatorTestComboBox.Items94"),
			componentResourceManager.GetString("ActuatorTestComboBox.Items95"),
			componentResourceManager.GetString("ActuatorTestComboBox.Items96"),
			componentResourceManager.GetString("ActuatorTestComboBox.Items97"),
			componentResourceManager.GetString("ActuatorTestComboBox.Items98"),
			componentResourceManager.GetString("ActuatorTestComboBox.Items99"),
			componentResourceManager.GetString("ActuatorTestComboBox.Items100"),
			componentResourceManager.GetString("ActuatorTestComboBox.Items101"),
			componentResourceManager.GetString("ActuatorTestComboBox.Items102"),
			componentResourceManager.GetString("ActuatorTestComboBox.Items103"),
			componentResourceManager.GetString("ActuatorTestComboBox.Items104"),
			componentResourceManager.GetString("ActuatorTestComboBox.Items105"),
			componentResourceManager.GetString("ActuatorTestComboBox.Items106"),
			componentResourceManager.GetString("ActuatorTestComboBox.Items107"),
			componentResourceManager.GetString("ActuatorTestComboBox.Items108"),
			componentResourceManager.GetString("ActuatorTestComboBox.Items109"),
			componentResourceManager.GetString("ActuatorTestComboBox.Items110"),
			componentResourceManager.GetString("ActuatorTestComboBox.Items111"),
			componentResourceManager.GetString("ActuatorTestComboBox.Items112"),
			componentResourceManager.GetString("ActuatorTestComboBox.Items113"),
			componentResourceManager.GetString("ActuatorTestComboBox.Items114"),
			componentResourceManager.GetString("ActuatorTestComboBox.Items115"),
			componentResourceManager.GetString("ActuatorTestComboBox.Items116"),
			componentResourceManager.GetString("ActuatorTestComboBox.Items117"),
			componentResourceManager.GetString("ActuatorTestComboBox.Items118"),
			componentResourceManager.GetString("ActuatorTestComboBox.Items119"),
			componentResourceManager.GetString("ActuatorTestComboBox.Items120"),
			componentResourceManager.GetString("ActuatorTestComboBox.Items121"),
			componentResourceManager.GetString("ActuatorTestComboBox.Items122"),
			componentResourceManager.GetString("ActuatorTestComboBox.Items123"),
			componentResourceManager.GetString("ActuatorTestComboBox.Items124"),
			componentResourceManager.GetString("ActuatorTestComboBox.Items125"),
			componentResourceManager.GetString("ActuatorTestComboBox.Items126"),
			componentResourceManager.GetString("ActuatorTestComboBox.Items127")
		});
		((Control)ActuatorTestComboBox).Name = "ActuatorTestComboBox";
		componentResourceManager.ApplyResources(ActuatorTestStopButton, "ActuatorTestStopButton");
		((Control)ActuatorTestStopButton).Name = "ActuatorTestStopButton";
		((ButtonBase)ActuatorTestStopButton).UseVisualStyleBackColor = true;
		((Control)ActuatorTestStopButton).Click += ActuatorTestStopButton_Click;
		componentResourceManager.ApplyResources(ActuatorTestStartButton, "ActuatorTestStartButton");
		((Control)ActuatorTestStartButton).Name = "ActuatorTestStartButton";
		((ButtonBase)ActuatorTestStartButton).UseVisualStyleBackColor = true;
		((Control)ActuatorTestStartButton).Click += ActuatorTestStartButton_Click;
		((Control)DiagnosticDataGroupBox).Controls.Add((Control)(object)DiagnosticDataCSVCheckBox);
		((Control)DiagnosticDataGroupBox).Controls.Add((Control)(object)MillisecondsLabel01);
		((Control)DiagnosticDataGroupBox).Controls.Add((Control)(object)DiagnosticDataRepeatIntervalTextBox);
		((Control)DiagnosticDataGroupBox).Controls.Add((Control)(object)DiagnosticDataRepeatIntervalCheckBox);
		((Control)DiagnosticDataGroupBox).Controls.Add((Control)(object)DiagnosticDataClearButton);
		((Control)DiagnosticDataGroupBox).Controls.Add((Control)(object)DiagnosticDataStopButton);
		((Control)DiagnosticDataGroupBox).Controls.Add((Control)(object)DiagnosticDataListBox);
		((Control)DiagnosticDataGroupBox).Controls.Add((Control)(object)DiagnosticDataReadButton);
		componentResourceManager.ApplyResources(DiagnosticDataGroupBox, "DiagnosticDataGroupBox");
		((Control)DiagnosticDataGroupBox).Name = "DiagnosticDataGroupBox";
		DiagnosticDataGroupBox.TabStop = false;
		componentResourceManager.ApplyResources(DiagnosticDataCSVCheckBox, "DiagnosticDataCSVCheckBox");
		DiagnosticDataCSVCheckBox.Checked = true;
		DiagnosticDataCSVCheckBox.CheckState = (CheckState)1;
		((Control)DiagnosticDataCSVCheckBox).Name = "DiagnosticDataCSVCheckBox";
		((ButtonBase)DiagnosticDataCSVCheckBox).UseVisualStyleBackColor = true;
		componentResourceManager.ApplyResources(MillisecondsLabel01, "MillisecondsLabel01");
		((Control)MillisecondsLabel01).Name = "MillisecondsLabel01";
		componentResourceManager.ApplyResources(DiagnosticDataRepeatIntervalTextBox, "DiagnosticDataRepeatIntervalTextBox");
		((Control)DiagnosticDataRepeatIntervalTextBox).Name = "DiagnosticDataRepeatIntervalTextBox";
		componentResourceManager.ApplyResources(DiagnosticDataRepeatIntervalCheckBox, "DiagnosticDataRepeatIntervalCheckBox");
		DiagnosticDataRepeatIntervalCheckBox.Checked = true;
		DiagnosticDataRepeatIntervalCheckBox.CheckState = (CheckState)1;
		((Control)DiagnosticDataRepeatIntervalCheckBox).Name = "DiagnosticDataRepeatIntervalCheckBox";
		((ButtonBase)DiagnosticDataRepeatIntervalCheckBox).UseVisualStyleBackColor = true;
		componentResourceManager.ApplyResources(DiagnosticDataClearButton, "DiagnosticDataClearButton");
		((Control)DiagnosticDataClearButton).Name = "DiagnosticDataClearButton";
		((ButtonBase)DiagnosticDataClearButton).UseVisualStyleBackColor = true;
		((Control)DiagnosticDataClearButton).Click += DiagnosticDataClearButton_Click;
		componentResourceManager.ApplyResources(DiagnosticDataStopButton, "DiagnosticDataStopButton");
		((Control)DiagnosticDataStopButton).Name = "DiagnosticDataStopButton";
		((ButtonBase)DiagnosticDataStopButton).UseVisualStyleBackColor = true;
		((Control)DiagnosticDataStopButton).Click += DiagnosticDataStopButton_Click;
		componentResourceManager.ApplyResources(DiagnosticDataListBox, "DiagnosticDataListBox");
		((ListControl)DiagnosticDataListBox).FormattingEnabled = true;
		DiagnosticDataListBox.Items.AddRange(new object[128]
		{
			componentResourceManager.GetString("DiagnosticDataListBox.Items"),
			componentResourceManager.GetString("DiagnosticDataListBox.Items1"),
			componentResourceManager.GetString("DiagnosticDataListBox.Items2"),
			componentResourceManager.GetString("DiagnosticDataListBox.Items3"),
			componentResourceManager.GetString("DiagnosticDataListBox.Items4"),
			componentResourceManager.GetString("DiagnosticDataListBox.Items5"),
			componentResourceManager.GetString("DiagnosticDataListBox.Items6"),
			componentResourceManager.GetString("DiagnosticDataListBox.Items7"),
			componentResourceManager.GetString("DiagnosticDataListBox.Items8"),
			componentResourceManager.GetString("DiagnosticDataListBox.Items9"),
			componentResourceManager.GetString("DiagnosticDataListBox.Items10"),
			componentResourceManager.GetString("DiagnosticDataListBox.Items11"),
			componentResourceManager.GetString("DiagnosticDataListBox.Items12"),
			componentResourceManager.GetString("DiagnosticDataListBox.Items13"),
			componentResourceManager.GetString("DiagnosticDataListBox.Items14"),
			componentResourceManager.GetString("DiagnosticDataListBox.Items15"),
			componentResourceManager.GetString("DiagnosticDataListBox.Items16"),
			componentResourceManager.GetString("DiagnosticDataListBox.Items17"),
			componentResourceManager.GetString("DiagnosticDataListBox.Items18"),
			componentResourceManager.GetString("DiagnosticDataListBox.Items19"),
			componentResourceManager.GetString("DiagnosticDataListBox.Items20"),
			componentResourceManager.GetString("DiagnosticDataListBox.Items21"),
			componentResourceManager.GetString("DiagnosticDataListBox.Items22"),
			componentResourceManager.GetString("DiagnosticDataListBox.Items23"),
			componentResourceManager.GetString("DiagnosticDataListBox.Items24"),
			componentResourceManager.GetString("DiagnosticDataListBox.Items25"),
			componentResourceManager.GetString("DiagnosticDataListBox.Items26"),
			componentResourceManager.GetString("DiagnosticDataListBox.Items27"),
			componentResourceManager.GetString("DiagnosticDataListBox.Items28"),
			componentResourceManager.GetString("DiagnosticDataListBox.Items29"),
			componentResourceManager.GetString("DiagnosticDataListBox.Items30"),
			componentResourceManager.GetString("DiagnosticDataListBox.Items31"),
			componentResourceManager.GetString("DiagnosticDataListBox.Items32"),
			componentResourceManager.GetString("DiagnosticDataListBox.Items33"),
			componentResourceManager.GetString("DiagnosticDataListBox.Items34"),
			componentResourceManager.GetString("DiagnosticDataListBox.Items35"),
			componentResourceManager.GetString("DiagnosticDataListBox.Items36"),
			componentResourceManager.GetString("DiagnosticDataListBox.Items37"),
			componentResourceManager.GetString("DiagnosticDataListBox.Items38"),
			componentResourceManager.GetString("DiagnosticDataListBox.Items39"),
			componentResourceManager.GetString("DiagnosticDataListBox.Items40"),
			componentResourceManager.GetString("DiagnosticDataListBox.Items41"),
			componentResourceManager.GetString("DiagnosticDataListBox.Items42"),
			componentResourceManager.GetString("DiagnosticDataListBox.Items43"),
			componentResourceManager.GetString("DiagnosticDataListBox.Items44"),
			componentResourceManager.GetString("DiagnosticDataListBox.Items45"),
			componentResourceManager.GetString("DiagnosticDataListBox.Items46"),
			componentResourceManager.GetString("DiagnosticDataListBox.Items47"),
			componentResourceManager.GetString("DiagnosticDataListBox.Items48"),
			componentResourceManager.GetString("DiagnosticDataListBox.Items49"),
			componentResourceManager.GetString("DiagnosticDataListBox.Items50"),
			componentResourceManager.GetString("DiagnosticDataListBox.Items51"),
			componentResourceManager.GetString("DiagnosticDataListBox.Items52"),
			componentResourceManager.GetString("DiagnosticDataListBox.Items53"),
			componentResourceManager.GetString("DiagnosticDataListBox.Items54"),
			componentResourceManager.GetString("DiagnosticDataListBox.Items55"),
			componentResourceManager.GetString("DiagnosticDataListBox.Items56"),
			componentResourceManager.GetString("DiagnosticDataListBox.Items57"),
			componentResourceManager.GetString("DiagnosticDataListBox.Items58"),
			componentResourceManager.GetString("DiagnosticDataListBox.Items59"),
			componentResourceManager.GetString("DiagnosticDataListBox.Items60"),
			componentResourceManager.GetString("DiagnosticDataListBox.Items61"),
			componentResourceManager.GetString("DiagnosticDataListBox.Items62"),
			componentResourceManager.GetString("DiagnosticDataListBox.Items63"),
			componentResourceManager.GetString("DiagnosticDataListBox.Items64"),
			componentResourceManager.GetString("DiagnosticDataListBox.Items65"),
			componentResourceManager.GetString("DiagnosticDataListBox.Items66"),
			componentResourceManager.GetString("DiagnosticDataListBox.Items67"),
			componentResourceManager.GetString("DiagnosticDataListBox.Items68"),
			componentResourceManager.GetString("DiagnosticDataListBox.Items69"),
			componentResourceManager.GetString("DiagnosticDataListBox.Items70"),
			componentResourceManager.GetString("DiagnosticDataListBox.Items71"),
			componentResourceManager.GetString("DiagnosticDataListBox.Items72"),
			componentResourceManager.GetString("DiagnosticDataListBox.Items73"),
			componentResourceManager.GetString("DiagnosticDataListBox.Items74"),
			componentResourceManager.GetString("DiagnosticDataListBox.Items75"),
			componentResourceManager.GetString("DiagnosticDataListBox.Items76"),
			componentResourceManager.GetString("DiagnosticDataListBox.Items77"),
			componentResourceManager.GetString("DiagnosticDataListBox.Items78"),
			componentResourceManager.GetString("DiagnosticDataListBox.Items79"),
			componentResourceManager.GetString("DiagnosticDataListBox.Items80"),
			componentResourceManager.GetString("DiagnosticDataListBox.Items81"),
			componentResourceManager.GetString("DiagnosticDataListBox.Items82"),
			componentResourceManager.GetString("DiagnosticDataListBox.Items83"),
			componentResourceManager.GetString("DiagnosticDataListBox.Items84"),
			componentResourceManager.GetString("DiagnosticDataListBox.Items85"),
			componentResourceManager.GetString("DiagnosticDataListBox.Items86"),
			componentResourceManager.GetString("DiagnosticDataListBox.Items87"),
			componentResourceManager.GetString("DiagnosticDataListBox.Items88"),
			componentResourceManager.GetString("DiagnosticDataListBox.Items89"),
			componentResourceManager.GetString("DiagnosticDataListBox.Items90"),
			componentResourceManager.GetString("DiagnosticDataListBox.Items91"),
			componentResourceManager.GetString("DiagnosticDataListBox.Items92"),
			componentResourceManager.GetString("DiagnosticDataListBox.Items93"),
			componentResourceManager.GetString("DiagnosticDataListBox.Items94"),
			componentResourceManager.GetString("DiagnosticDataListBox.Items95"),
			componentResourceManager.GetString("DiagnosticDataListBox.Items96"),
			componentResourceManager.GetString("DiagnosticDataListBox.Items97"),
			componentResourceManager.GetString("DiagnosticDataListBox.Items98"),
			componentResourceManager.GetString("DiagnosticDataListBox.Items99"),
			componentResourceManager.GetString("DiagnosticDataListBox.Items100"),
			componentResourceManager.GetString("DiagnosticDataListBox.Items101"),
			componentResourceManager.GetString("DiagnosticDataListBox.Items102"),
			componentResourceManager.GetString("DiagnosticDataListBox.Items103"),
			componentResourceManager.GetString("DiagnosticDataListBox.Items104"),
			componentResourceManager.GetString("DiagnosticDataListBox.Items105"),
			componentResourceManager.GetString("DiagnosticDataListBox.Items106"),
			componentResourceManager.GetString("DiagnosticDataListBox.Items107"),
			componentResourceManager.GetString("DiagnosticDataListBox.Items108"),
			componentResourceManager.GetString("DiagnosticDataListBox.Items109"),
			componentResourceManager.GetString("DiagnosticDataListBox.Items110"),
			componentResourceManager.GetString("DiagnosticDataListBox.Items111"),
			componentResourceManager.GetString("DiagnosticDataListBox.Items112"),
			componentResourceManager.GetString("DiagnosticDataListBox.Items113"),
			componentResourceManager.GetString("DiagnosticDataListBox.Items114"),
			componentResourceManager.GetString("DiagnosticDataListBox.Items115"),
			componentResourceManager.GetString("DiagnosticDataListBox.Items116"),
			componentResourceManager.GetString("DiagnosticDataListBox.Items117"),
			componentResourceManager.GetString("DiagnosticDataListBox.Items118"),
			componentResourceManager.GetString("DiagnosticDataListBox.Items119"),
			componentResourceManager.GetString("DiagnosticDataListBox.Items120"),
			componentResourceManager.GetString("DiagnosticDataListBox.Items121"),
			componentResourceManager.GetString("DiagnosticDataListBox.Items122"),
			componentResourceManager.GetString("DiagnosticDataListBox.Items123"),
			componentResourceManager.GetString("DiagnosticDataListBox.Items124"),
			componentResourceManager.GetString("DiagnosticDataListBox.Items125"),
			componentResourceManager.GetString("DiagnosticDataListBox.Items126"),
			componentResourceManager.GetString("DiagnosticDataListBox.Items127")
		});
		((Control)DiagnosticDataListBox).Name = "DiagnosticDataListBox";
		DiagnosticDataListBox.SelectionMode = (SelectionMode)3;
		componentResourceManager.ApplyResources(DiagnosticDataReadButton, "DiagnosticDataReadButton");
		((Control)DiagnosticDataReadButton).Name = "DiagnosticDataReadButton";
		((ButtonBase)DiagnosticDataReadButton).UseVisualStyleBackColor = true;
		((Control)DiagnosticDataReadButton).Click += DiagnosticDataReadButton_Click;
		((Control)SetIdleSpeedGroupBox).Controls.Add((Control)(object)IdleSpeedNoteLabel);
		((Control)SetIdleSpeedGroupBox).Controls.Add((Control)(object)RPMLabel);
		((Control)SetIdleSpeedGroupBox).Controls.Add((Control)(object)SetIdleSpeedTextBox);
		((Control)SetIdleSpeedGroupBox).Controls.Add((Control)(object)SetIdleSpeedTrackBar);
		((Control)SetIdleSpeedGroupBox).Controls.Add((Control)(object)SetIdleSpeedStopButton);
		((Control)SetIdleSpeedGroupBox).Controls.Add((Control)(object)SetIdleSpeedSetButton);
		componentResourceManager.ApplyResources(SetIdleSpeedGroupBox, "SetIdleSpeedGroupBox");
		((Control)SetIdleSpeedGroupBox).Name = "SetIdleSpeedGroupBox";
		SetIdleSpeedGroupBox.TabStop = false;
		componentResourceManager.ApplyResources(IdleSpeedNoteLabel, "IdleSpeedNoteLabel");
		((Control)IdleSpeedNoteLabel).Name = "IdleSpeedNoteLabel";
		componentResourceManager.ApplyResources(RPMLabel, "RPMLabel");
		((Control)RPMLabel).Name = "RPMLabel";
		componentResourceManager.ApplyResources(SetIdleSpeedTextBox, "SetIdleSpeedTextBox");
		((Control)SetIdleSpeedTextBox).Name = "SetIdleSpeedTextBox";
		((Control)SetIdleSpeedTextBox).KeyPress += new KeyPressEventHandler(SetIdleSpeedTextBox_KeyPress);
		SetIdleSpeedTrackBar.LargeChange = 32;
		componentResourceManager.ApplyResources(SetIdleSpeedTrackBar, "SetIdleSpeedTrackBar");
		SetIdleSpeedTrackBar.Maximum = 2040;
		((Control)SetIdleSpeedTrackBar).Name = "SetIdleSpeedTrackBar";
		SetIdleSpeedTrackBar.SmallChange = 16;
		SetIdleSpeedTrackBar.TickFrequency = 64;
		SetIdleSpeedTrackBar.TickStyle = (TickStyle)3;
		SetIdleSpeedTrackBar.Value = 1500;
		SetIdleSpeedTrackBar.Scroll += SetIdleSpeedTrackBar_Scroll;
		componentResourceManager.ApplyResources(SetIdleSpeedStopButton, "SetIdleSpeedStopButton");
		((Control)SetIdleSpeedStopButton).Name = "SetIdleSpeedStopButton";
		((ButtonBase)SetIdleSpeedStopButton).UseVisualStyleBackColor = true;
		((Control)SetIdleSpeedStopButton).Click += SetIdleSpeedStopButton_Click;
		componentResourceManager.ApplyResources(SetIdleSpeedSetButton, "SetIdleSpeedSetButton");
		((Control)SetIdleSpeedSetButton).Name = "SetIdleSpeedSetButton";
		((ButtonBase)SetIdleSpeedSetButton).UseVisualStyleBackColor = true;
		((Control)SetIdleSpeedSetButton).Click += SetIdleSpeedSetButton_Click;
		((Control)ResetMemoryGroupBox).Controls.Add((Control)(object)ResetMemoryStatusLabel);
		((Control)ResetMemoryGroupBox).Controls.Add((Control)(object)ResetMemoryComboBox);
		((Control)ResetMemoryGroupBox).Controls.Add((Control)(object)ResetMemoryOKButton);
		componentResourceManager.ApplyResources(ResetMemoryGroupBox, "ResetMemoryGroupBox");
		((Control)ResetMemoryGroupBox).Name = "ResetMemoryGroupBox";
		ResetMemoryGroupBox.TabStop = false;
		componentResourceManager.ApplyResources(ResetMemoryStatusLabel, "ResetMemoryStatusLabel");
		((Control)ResetMemoryStatusLabel).Name = "ResetMemoryStatusLabel";
		ResetMemoryComboBox.DropDownHeight = 226;
		ResetMemoryComboBox.DropDownStyle = (ComboBoxStyle)2;
		componentResourceManager.ApplyResources(ResetMemoryComboBox, "ResetMemoryComboBox");
		((ListControl)ResetMemoryComboBox).FormattingEnabled = true;
		ResetMemoryComboBox.Items.AddRange(new object[40]
		{
			componentResourceManager.GetString("ResetMemoryComboBox.Items"),
			componentResourceManager.GetString("ResetMemoryComboBox.Items1"),
			componentResourceManager.GetString("ResetMemoryComboBox.Items2"),
			componentResourceManager.GetString("ResetMemoryComboBox.Items3"),
			componentResourceManager.GetString("ResetMemoryComboBox.Items4"),
			componentResourceManager.GetString("ResetMemoryComboBox.Items5"),
			componentResourceManager.GetString("ResetMemoryComboBox.Items6"),
			componentResourceManager.GetString("ResetMemoryComboBox.Items7"),
			componentResourceManager.GetString("ResetMemoryComboBox.Items8"),
			componentResourceManager.GetString("ResetMemoryComboBox.Items9"),
			componentResourceManager.GetString("ResetMemoryComboBox.Items10"),
			componentResourceManager.GetString("ResetMemoryComboBox.Items11"),
			componentResourceManager.GetString("ResetMemoryComboBox.Items12"),
			componentResourceManager.GetString("ResetMemoryComboBox.Items13"),
			componentResourceManager.GetString("ResetMemoryComboBox.Items14"),
			componentResourceManager.GetString("ResetMemoryComboBox.Items15"),
			componentResourceManager.GetString("ResetMemoryComboBox.Items16"),
			componentResourceManager.GetString("ResetMemoryComboBox.Items17"),
			componentResourceManager.GetString("ResetMemoryComboBox.Items18"),
			componentResourceManager.GetString("ResetMemoryComboBox.Items19"),
			componentResourceManager.GetString("ResetMemoryComboBox.Items20"),
			componentResourceManager.GetString("ResetMemoryComboBox.Items21"),
			componentResourceManager.GetString("ResetMemoryComboBox.Items22"),
			componentResourceManager.GetString("ResetMemoryComboBox.Items23"),
			componentResourceManager.GetString("ResetMemoryComboBox.Items24"),
			componentResourceManager.GetString("ResetMemoryComboBox.Items25"),
			componentResourceManager.GetString("ResetMemoryComboBox.Items26"),
			componentResourceManager.GetString("ResetMemoryComboBox.Items27"),
			componentResourceManager.GetString("ResetMemoryComboBox.Items28"),
			componentResourceManager.GetString("ResetMemoryComboBox.Items29"),
			componentResourceManager.GetString("ResetMemoryComboBox.Items30"),
			componentResourceManager.GetString("ResetMemoryComboBox.Items31"),
			componentResourceManager.GetString("ResetMemoryComboBox.Items32"),
			componentResourceManager.GetString("ResetMemoryComboBox.Items33"),
			componentResourceManager.GetString("ResetMemoryComboBox.Items34"),
			componentResourceManager.GetString("ResetMemoryComboBox.Items35"),
			componentResourceManager.GetString("ResetMemoryComboBox.Items36"),
			componentResourceManager.GetString("ResetMemoryComboBox.Items37"),
			componentResourceManager.GetString("ResetMemoryComboBox.Items38"),
			componentResourceManager.GetString("ResetMemoryComboBox.Items39")
		});
		((Control)ResetMemoryComboBox).Name = "ResetMemoryComboBox";
		componentResourceManager.ApplyResources(ResetMemoryOKButton, "ResetMemoryOKButton");
		((Control)ResetMemoryOKButton).Name = "ResetMemoryOKButton";
		((ButtonBase)ResetMemoryOKButton).UseVisualStyleBackColor = true;
		((Control)ResetMemoryOKButton).Click += ResetMemoryOKButton_Click;
		((Control)SecurityGroupBox).Controls.Add((Control)(object)LegacySecurityCheckBox);
		((Control)SecurityGroupBox).Controls.Add((Control)(object)SecurityLevelComboBox);
		((Control)SecurityGroupBox).Controls.Add((Control)(object)SecurityUnlockButton);
		componentResourceManager.ApplyResources(SecurityGroupBox, "SecurityGroupBox");
		((Control)SecurityGroupBox).Name = "SecurityGroupBox";
		SecurityGroupBox.TabStop = false;
		componentResourceManager.ApplyResources(LegacySecurityCheckBox, "LegacySecurityCheckBox");
		LegacySecurityCheckBox.Checked = true;
		LegacySecurityCheckBox.CheckState = (CheckState)1;
		((Control)LegacySecurityCheckBox).Name = "LegacySecurityCheckBox";
		((ButtonBase)LegacySecurityCheckBox).UseVisualStyleBackColor = true;
		LegacySecurityCheckBox.CheckedChanged += LegacySecurityCheckBox_CheckedChanged;
		SecurityLevelComboBox.DropDownHeight = 226;
		SecurityLevelComboBox.DropDownStyle = (ComboBoxStyle)2;
		componentResourceManager.ApplyResources(SecurityLevelComboBox, "SecurityLevelComboBox");
		((ListControl)SecurityLevelComboBox).FormattingEnabled = true;
		SecurityLevelComboBox.Items.AddRange(new object[2]
		{
			componentResourceManager.GetString("SecurityLevelComboBox.Items"),
			componentResourceManager.GetString("SecurityLevelComboBox.Items1")
		});
		((Control)SecurityLevelComboBox).Name = "SecurityLevelComboBox";
		componentResourceManager.ApplyResources(SecurityUnlockButton, "SecurityUnlockButton");
		((Control)SecurityUnlockButton).Name = "SecurityUnlockButton";
		((ButtonBase)SecurityUnlockButton).UseVisualStyleBackColor = true;
		((Control)SecurityUnlockButton).Click += SecurityUnlockButton_Click;
		((Control)ConfigurationGroupBox).Controls.Add((Control)(object)ConfigurationGetPartNumberButton);
		((Control)ConfigurationGroupBox).Controls.Add((Control)(object)ConfigurationGetAllButton);
		((Control)ConfigurationGroupBox).Controls.Add((Control)(object)ConfigurationComboBox);
		((Control)ConfigurationGroupBox).Controls.Add((Control)(object)ConfigurationGetButton);
		componentResourceManager.ApplyResources(ConfigurationGroupBox, "ConfigurationGroupBox");
		((Control)ConfigurationGroupBox).Name = "ConfigurationGroupBox";
		ConfigurationGroupBox.TabStop = false;
		componentResourceManager.ApplyResources(ConfigurationGetPartNumberButton, "ConfigurationGetPartNumberButton");
		((Control)ConfigurationGetPartNumberButton).Name = "ConfigurationGetPartNumberButton";
		((ButtonBase)ConfigurationGetPartNumberButton).UseVisualStyleBackColor = true;
		((Control)ConfigurationGetPartNumberButton).Click += ConfigurationGetPartNumberButton_Click;
		componentResourceManager.ApplyResources(ConfigurationGetAllButton, "ConfigurationGetAllButton");
		((Control)ConfigurationGetAllButton).Name = "ConfigurationGetAllButton";
		((ButtonBase)ConfigurationGetAllButton).UseVisualStyleBackColor = true;
		((Control)ConfigurationGetAllButton).Click += ConfigurationGetAllButton_Click;
		ConfigurationComboBox.DropDownHeight = 226;
		ConfigurationComboBox.DropDownStyle = (ComboBoxStyle)2;
		componentResourceManager.ApplyResources(ConfigurationComboBox, "ConfigurationComboBox");
		((ListControl)ConfigurationComboBox).FormattingEnabled = true;
		ConfigurationComboBox.Items.AddRange(new object[32]
		{
			componentResourceManager.GetString("ConfigurationComboBox.Items"),
			componentResourceManager.GetString("ConfigurationComboBox.Items1"),
			componentResourceManager.GetString("ConfigurationComboBox.Items2"),
			componentResourceManager.GetString("ConfigurationComboBox.Items3"),
			componentResourceManager.GetString("ConfigurationComboBox.Items4"),
			componentResourceManager.GetString("ConfigurationComboBox.Items5"),
			componentResourceManager.GetString("ConfigurationComboBox.Items6"),
			componentResourceManager.GetString("ConfigurationComboBox.Items7"),
			componentResourceManager.GetString("ConfigurationComboBox.Items8"),
			componentResourceManager.GetString("ConfigurationComboBox.Items9"),
			componentResourceManager.GetString("ConfigurationComboBox.Items10"),
			componentResourceManager.GetString("ConfigurationComboBox.Items11"),
			componentResourceManager.GetString("ConfigurationComboBox.Items12"),
			componentResourceManager.GetString("ConfigurationComboBox.Items13"),
			componentResourceManager.GetString("ConfigurationComboBox.Items14"),
			componentResourceManager.GetString("ConfigurationComboBox.Items15"),
			componentResourceManager.GetString("ConfigurationComboBox.Items16"),
			componentResourceManager.GetString("ConfigurationComboBox.Items17"),
			componentResourceManager.GetString("ConfigurationComboBox.Items18"),
			componentResourceManager.GetString("ConfigurationComboBox.Items19"),
			componentResourceManager.GetString("ConfigurationComboBox.Items20"),
			componentResourceManager.GetString("ConfigurationComboBox.Items21"),
			componentResourceManager.GetString("ConfigurationComboBox.Items22"),
			componentResourceManager.GetString("ConfigurationComboBox.Items23"),
			componentResourceManager.GetString("ConfigurationComboBox.Items24"),
			componentResourceManager.GetString("ConfigurationComboBox.Items25"),
			componentResourceManager.GetString("ConfigurationComboBox.Items26"),
			componentResourceManager.GetString("ConfigurationComboBox.Items27"),
			componentResourceManager.GetString("ConfigurationComboBox.Items28"),
			componentResourceManager.GetString("ConfigurationComboBox.Items29"),
			componentResourceManager.GetString("ConfigurationComboBox.Items30"),
			componentResourceManager.GetString("ConfigurationComboBox.Items31")
		});
		((Control)ConfigurationComboBox).Name = "ConfigurationComboBox";
		componentResourceManager.ApplyResources(ConfigurationGetButton, "ConfigurationGetButton");
		((Control)ConfigurationGetButton).Name = "ConfigurationGetButton";
		((ButtonBase)ConfigurationGetButton).UseVisualStyleBackColor = true;
		((Control)ConfigurationGetButton).Click += ConfigurationGetButton_Click;
		((Control)RAMTableGroupBox).Controls.Add((Control)(object)RAMTableComboBox);
		((Control)RAMTableGroupBox).Controls.Add((Control)(object)RAMTableSelectButton);
		componentResourceManager.ApplyResources(RAMTableGroupBox, "RAMTableGroupBox");
		((Control)RAMTableGroupBox).Name = "RAMTableGroupBox";
		RAMTableGroupBox.TabStop = false;
		RAMTableComboBox.DropDownHeight = 226;
		RAMTableComboBox.DropDownStyle = (ComboBoxStyle)2;
		componentResourceManager.ApplyResources(RAMTableComboBox, "RAMTableComboBox");
		((ListControl)RAMTableComboBox).FormattingEnabled = true;
		RAMTableComboBox.Items.AddRange(new object[14]
		{
			componentResourceManager.GetString("RAMTableComboBox.Items"),
			componentResourceManager.GetString("RAMTableComboBox.Items1"),
			componentResourceManager.GetString("RAMTableComboBox.Items2"),
			componentResourceManager.GetString("RAMTableComboBox.Items3"),
			componentResourceManager.GetString("RAMTableComboBox.Items4"),
			componentResourceManager.GetString("RAMTableComboBox.Items5"),
			componentResourceManager.GetString("RAMTableComboBox.Items6"),
			componentResourceManager.GetString("RAMTableComboBox.Items7"),
			componentResourceManager.GetString("RAMTableComboBox.Items8"),
			componentResourceManager.GetString("RAMTableComboBox.Items9"),
			componentResourceManager.GetString("RAMTableComboBox.Items10"),
			componentResourceManager.GetString("RAMTableComboBox.Items11"),
			componentResourceManager.GetString("RAMTableComboBox.Items12"),
			componentResourceManager.GetString("RAMTableComboBox.Items13")
		});
		((Control)RAMTableComboBox).Name = "RAMTableComboBox";
		componentResourceManager.ApplyResources(RAMTableSelectButton, "RAMTableSelectButton");
		((Control)RAMTableSelectButton).Name = "RAMTableSelectButton";
		((ButtonBase)RAMTableSelectButton).UseVisualStyleBackColor = true;
		((Control)RAMTableSelectButton).Click += RAMTableSelectButton_Click;
		((Control)CHTGroupBox).Controls.Add((Control)(object)CHTDetectButton);
		((Control)CHTGroupBox).Controls.Add((Control)(object)CHTComboBox);
		componentResourceManager.ApplyResources(CHTGroupBox, "CHTGroupBox");
		((Control)CHTGroupBox).Name = "CHTGroupBox";
		CHTGroupBox.TabStop = false;
		componentResourceManager.ApplyResources(CHTDetectButton, "CHTDetectButton");
		((Control)CHTDetectButton).Name = "CHTDetectButton";
		((ButtonBase)CHTDetectButton).UseVisualStyleBackColor = true;
		((Control)CHTDetectButton).Click += CHTDetectButton_Click;
		CHTComboBox.DropDownHeight = 226;
		CHTComboBox.DropDownStyle = (ComboBoxStyle)2;
		componentResourceManager.ApplyResources(CHTComboBox, "CHTComboBox");
		((ListControl)CHTComboBox).FormattingEnabled = true;
		CHTComboBox.Items.AddRange(new object[30]
		{
			componentResourceManager.GetString("CHTComboBox.Items"),
			componentResourceManager.GetString("CHTComboBox.Items1"),
			componentResourceManager.GetString("CHTComboBox.Items2"),
			componentResourceManager.GetString("CHTComboBox.Items3"),
			componentResourceManager.GetString("CHTComboBox.Items4"),
			componentResourceManager.GetString("CHTComboBox.Items5"),
			componentResourceManager.GetString("CHTComboBox.Items6"),
			componentResourceManager.GetString("CHTComboBox.Items7"),
			componentResourceManager.GetString("CHTComboBox.Items8"),
			componentResourceManager.GetString("CHTComboBox.Items9"),
			componentResourceManager.GetString("CHTComboBox.Items10"),
			componentResourceManager.GetString("CHTComboBox.Items11"),
			componentResourceManager.GetString("CHTComboBox.Items12"),
			componentResourceManager.GetString("CHTComboBox.Items13"),
			componentResourceManager.GetString("CHTComboBox.Items14"),
			componentResourceManager.GetString("CHTComboBox.Items15"),
			componentResourceManager.GetString("CHTComboBox.Items16"),
			componentResourceManager.GetString("CHTComboBox.Items17"),
			componentResourceManager.GetString("CHTComboBox.Items18"),
			componentResourceManager.GetString("CHTComboBox.Items19"),
			componentResourceManager.GetString("CHTComboBox.Items20"),
			componentResourceManager.GetString("CHTComboBox.Items21"),
			componentResourceManager.GetString("CHTComboBox.Items22"),
			componentResourceManager.GetString("CHTComboBox.Items23"),
			componentResourceManager.GetString("CHTComboBox.Items24"),
			componentResourceManager.GetString("CHTComboBox.Items25"),
			componentResourceManager.GetString("CHTComboBox.Items26"),
			componentResourceManager.GetString("CHTComboBox.Items27"),
			componentResourceManager.GetString("CHTComboBox.Items28"),
			componentResourceManager.GetString("CHTComboBox.Items29")
		});
		((Control)CHTComboBox).Name = "CHTComboBox";
		CHTComboBox.SelectedIndexChanged += CHTComboBox_SelectedIndexChanged;
		componentResourceManager.ApplyResources(EngineToolsStatusStrip, "EngineToolsStatusStrip");
		((ToolStrip)EngineToolsStatusStrip).ImageScalingSize = new Size(20, 20);
		((ToolStrip)EngineToolsStatusStrip).Items.AddRange((ToolStripItem[])(object)new ToolStripItem[1] { (ToolStripItem)EnginePropertiesLabel });
		EngineToolsStatusStrip.LayoutStyle = (ToolStripLayoutStyle)3;
		((Control)EngineToolsStatusStrip).Name = "EngineToolsStatusStrip";
		((ToolStripItem)EnginePropertiesLabel).DisplayStyle = (ToolStripItemDisplayStyle)1;
		componentResourceManager.ApplyResources(EnginePropertiesLabel, "EnginePropertiesLabel");
		((ToolStripItem)EnginePropertiesLabel).Name = "EnginePropertiesLabel";
		((Control)SetFuelSyncGroupBox).Controls.Add((Control)(object)FuelSyncInRangeLabel);
		((Control)SetFuelSyncGroupBox).Controls.Add((Control)(object)label2);
		((Control)SetFuelSyncGroupBox).Controls.Add((Control)(object)DistributorSettingTextBox);
		((Control)SetFuelSyncGroupBox).Controls.Add((Control)(object)label1);
		((Control)SetFuelSyncGroupBox).Controls.Add((Control)(object)SetFuelSyncStopButton);
		((Control)SetFuelSyncGroupBox).Controls.Add((Control)(object)SetFuelSyncStartButton);
		componentResourceManager.ApplyResources(SetFuelSyncGroupBox, "SetFuelSyncGroupBox");
		((Control)SetFuelSyncGroupBox).Name = "SetFuelSyncGroupBox";
		SetFuelSyncGroupBox.TabStop = false;
		componentResourceManager.ApplyResources(FuelSyncInRangeLabel, "FuelSyncInRangeLabel");
		((Control)FuelSyncInRangeLabel).Name = "FuelSyncInRangeLabel";
		componentResourceManager.ApplyResources(label2, "label2");
		((Control)label2).Name = "label2";
		componentResourceManager.ApplyResources(DistributorSettingTextBox, "DistributorSettingTextBox");
		((Control)DistributorSettingTextBox).Name = "DistributorSettingTextBox";
		componentResourceManager.ApplyResources(label1, "label1");
		((Control)label1).Name = "label1";
		componentResourceManager.ApplyResources(SetFuelSyncStopButton, "SetFuelSyncStopButton");
		((Control)SetFuelSyncStopButton).Name = "SetFuelSyncStopButton";
		((ButtonBase)SetFuelSyncStopButton).UseVisualStyleBackColor = true;
		((Control)SetFuelSyncStopButton).Click += SetFuelSyncStopButton_Click;
		componentResourceManager.ApplyResources(SetFuelSyncStartButton, "SetFuelSyncStartButton");
		((Control)SetFuelSyncStartButton).Name = "SetFuelSyncStartButton";
		((ButtonBase)SetFuelSyncStartButton).UseVisualStyleBackColor = true;
		((Control)SetFuelSyncStartButton).Click += SetFuelSyncStartButton_Click;
		componentResourceManager.ApplyResources(this, "$this");
		((ContainerControl)this).AutoScaleMode = (AutoScaleMode)2;
		((Control)this).Controls.Add((Control)(object)SetFuelSyncGroupBox);
		((Control)this).Controls.Add((Control)(object)EngineToolsStatusStrip);
		((Control)this).Controls.Add((Control)(object)CHTGroupBox);
		((Control)this).Controls.Add((Control)(object)RAMTableGroupBox);
		((Control)this).Controls.Add((Control)(object)ConfigurationGroupBox);
		((Control)this).Controls.Add((Control)(object)SecurityGroupBox);
		((Control)this).Controls.Add((Control)(object)ResetMemoryGroupBox);
		((Control)this).Controls.Add((Control)(object)SetIdleSpeedGroupBox);
		((Control)this).Controls.Add((Control)(object)DiagnosticDataGroupBox);
		((Control)this).Controls.Add((Control)(object)ActuatorTestGroupBox);
		((Control)this).Controls.Add((Control)(object)BaudrateGroupBox);
		((Control)this).Controls.Add((Control)(object)FaultCodeGroupBox);
		((Control)this).Name = "EngineToolsForm";
		((Form)this).FormClosing += new FormClosingEventHandler(EngineToolsForm_FormClosing);
		((Form)this).Load += EngineToolsForm_Load;
		((Control)FaultCodeGroupBox).ResumeLayout(false);
		((Control)BaudrateGroupBox).ResumeLayout(false);
		((Control)ActuatorTestGroupBox).ResumeLayout(false);
		((Control)ActuatorTestGroupBox).PerformLayout();
		((Control)DiagnosticDataGroupBox).ResumeLayout(false);
		((Control)DiagnosticDataGroupBox).PerformLayout();
		((Control)SetIdleSpeedGroupBox).ResumeLayout(false);
		((Control)SetIdleSpeedGroupBox).PerformLayout();
		((ISupportInitialize)SetIdleSpeedTrackBar).EndInit();
		((Control)ResetMemoryGroupBox).ResumeLayout(false);
		((Control)ResetMemoryGroupBox).PerformLayout();
		((Control)SecurityGroupBox).ResumeLayout(false);
		((Control)SecurityGroupBox).PerformLayout();
		((Control)ConfigurationGroupBox).ResumeLayout(false);
		((Control)RAMTableGroupBox).ResumeLayout(false);
		((Control)CHTGroupBox).ResumeLayout(false);
		((Control)EngineToolsStatusStrip).ResumeLayout(false);
		((Control)EngineToolsStatusStrip).PerformLayout();
		((Control)SetFuelSyncGroupBox).ResumeLayout(false);
		((Control)SetFuelSyncGroupBox).PerformLayout();
		((Control)this).ResumeLayout(false);
	}
}
