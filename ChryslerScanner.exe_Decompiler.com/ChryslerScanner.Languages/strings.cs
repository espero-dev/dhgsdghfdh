using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Resources;
using System.Runtime.CompilerServices;

namespace ChryslerScanner.Languages;

[GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "17.0.0.0")]
[DebuggerNonUserCode]
[CompilerGenerated]
public class strings
{
	private static ResourceManager resourceMan;

	private static CultureInfo resourceCulture;

	[EditorBrowsable(EditorBrowsableState.Advanced)]
	public static ResourceManager ResourceManager
	{
		get
		{
			if (resourceMan == null)
			{
				resourceMan = new ResourceManager("ChryslerScanner.Languages.strings", typeof(strings).Assembly);
			}
			return resourceMan;
		}
	}

	[EditorBrowsable(EditorBrowsableState.Advanced)]
	public static CultureInfo Culture
	{
		get
		{
			return resourceCulture;
		}
		set
		{
			resourceCulture = value;
		}
	}

	public static string About => ResourceManager.GetString("About", resourceCulture);

	public static string ABSTools => ResourceManager.GetString("ABSTools", resourceCulture);

	public static string Add => ResourceManager.GetString("Add", resourceCulture);

	public static string Apply => ResourceManager.GetString("Apply", resourceCulture);

	public static string BlinkDuration => ResourceManager.GetString("BlinkDuration", resourceCulture);

	public static string BootstrapTools => ResourceManager.GetString("BootstrapTools", resourceCulture);

	public static string CalculateChecksum => ResourceManager.GetString("CalculateChecksum", resourceCulture);

	public static string CalculateCRC => ResourceManager.GetString("CalculateCRC", resourceCulture);

	public static string CCDBusOnDemand => ResourceManager.GetString("CCDBusOnDemand", resourceCulture);

	public static string CCDBusTerminationBias => ResourceManager.GetString("CCDBusTerminationBias", resourceCulture);

	public static string CCDBusTransceiver => ResourceManager.GetString("CCDBusTransceiver", resourceCulture);

	public static string Clear => ResourceManager.GetString("Clear", resourceCulture);

	public static string Collapse => ResourceManager.GetString("Collapse", resourceCulture);

	public static string Connect => ResourceManager.GetString("Connect", resourceCulture);

	public static string ControlPanel => ResourceManager.GetString("ControlPanel", resourceCulture);

	public static string CopyTableToClipboard => ResourceManager.GetString("CopyTableToClipboard", resourceCulture);

	public static string Demo => ResourceManager.GetString("Demo", resourceCulture);

	public static string Diagnostics => ResourceManager.GetString("Diagnostics", resourceCulture);

	public static string Disconnect => ResourceManager.GetString("Disconnect", resourceCulture);

	public static string Engine => ResourceManager.GetString("Engine", resourceCulture);

	public static string EngineTools => ResourceManager.GetString("EngineTools", resourceCulture);

	public static string English => ResourceManager.GetString("English", resourceCulture);

	public static string Expand => ResourceManager.GetString("Expand", resourceCulture);

	public static string Handshake => ResourceManager.GetString("Handshake", resourceCulture);

	public static string HeartbeatInterval => ResourceManager.GetString("HeartbeatInterval", resourceCulture);

	public static string Imperial => ResourceManager.GetString("Imperial", resourceCulture);

	public static string IncludeTimestampInLogFiles => ResourceManager.GetString("IncludeTimestampInLogFiles", resourceCulture);

	public static string Interval => ResourceManager.GetString("Interval", resourceCulture);

	public static string Language => ResourceManager.GetString("Language", resourceCulture);

	public static string Main => ResourceManager.GetString("Main", resourceCulture);

	public static string max => ResourceManager.GetString("max", resourceCulture);

	public static string Metric => ResourceManager.GetString("Metric", resourceCulture);

	public static string min => ResourceManager.GetString("min", resourceCulture);

	public static string Module => ResourceManager.GetString("Module", resourceCulture);

	public static string NGCDebug => ResourceManager.GetString("NGCDebug", resourceCulture);

	public static string OBDConfig => ResourceManager.GetString("OBDConfig", resourceCulture);

	public static string OFF => ResourceManager.GetString("OFF", resourceCulture);

	public static string ON => ResourceManager.GetString("ON", resourceCulture);

	public static string OverwriteDuplicateID => ResourceManager.GetString("OverwriteDuplicateID", resourceCulture);

	public static string PCIBusOnDemand => ResourceManager.GetString("PCIBusOnDemand", resourceCulture);

	public static string PCIBusTransceiver => ResourceManager.GetString("PCIBusTransceiver", resourceCulture);

	public static string ReadMemory => ResourceManager.GetString("ReadMemory", resourceCulture);

	public static string ReadWriteMemory => ResourceManager.GetString("ReadWriteMemory", resourceCulture);

	public static string Refresh => ResourceManager.GetString("Refresh", resourceCulture);

	public static string Remove => ResourceManager.GetString("Remove", resourceCulture);

	public static string RepeatInterval => ResourceManager.GetString("RepeatInterval", resourceCulture);

	public static string Request => ResourceManager.GetString("Request", resourceCulture);

	public static string Reset => ResourceManager.GetString("Reset", resourceCulture);

	public static string ResetView => ResourceManager.GetString("ResetView", resourceCulture);

	public static string SBEC2Logic => ResourceManager.GetString("SBEC2Logic", resourceCulture);

	public static string SCIBusEngine => ResourceManager.GetString("SCIBusEngine", resourceCulture);

	public static string SCIBusTransmission => ResourceManager.GetString("SCIBusTransmission", resourceCulture);

	public static string SendMessages => ResourceManager.GetString("SendMessages", resourceCulture);

	public static string SendPacket => ResourceManager.GetString("SendPacket", resourceCulture);

	public static string SendRandomMessages => ResourceManager.GetString("SendRandomMessages", resourceCulture);

	public static string SetLEDs => ResourceManager.GetString("SetLEDs", resourceCulture);

	public static string Settings => ResourceManager.GetString("Settings", resourceCulture);

	public static string SettingsLabel => ResourceManager.GetString("SettingsLabel", resourceCulture);

	public static string Snapshot => ResourceManager.GetString("Snapshot", resourceCulture);

	public static string SortMessagesByIDByte => ResourceManager.GetString("SortMessagesByIDByte", resourceCulture);

	public static string Spanish => ResourceManager.GetString("Spanish", resourceCulture);

	public static string Speed => ResourceManager.GetString("Speed", resourceCulture);

	public static string Status => ResourceManager.GetString("Status", resourceCulture);

	public static string StopRepeatedMessages => ResourceManager.GetString("StopRepeatedMessages", resourceCulture);

	public static string Timestamp => ResourceManager.GetString("Timestamp", resourceCulture);

	public static string Tools => ResourceManager.GetString("Tools", resourceCulture);

	public static string Transmission => ResourceManager.GetString("Transmission", resourceCulture);

	public static string Unit => ResourceManager.GetString("Unit", resourceCulture);

	public static string Update => ResourceManager.GetString("Update", resourceCulture);

	public static string USBCommunication => ResourceManager.GetString("USBCommunication", resourceCulture);

	public static string VersionInfo => ResourceManager.GetString("VersionInfo", resourceCulture);

	public static string Voltages => ResourceManager.GetString("Voltages", resourceCulture);

	internal strings()
	{
	}
}
