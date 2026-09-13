using System.CodeDom.Compiler;
using System.Configuration;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace ChryslerScanner.Properties;

[CompilerGenerated]
[GeneratedCode("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "17.6.0.0")]
internal sealed class Settings : ApplicationSettingsBase
{
	private static Settings defaultInstance = (Settings)(object)SettingsBase.Synchronized((SettingsBase)(object)new Settings());

	public static Settings Default => defaultInstance;

	[UserScopedSetting]
	[DebuggerNonUserCode]
	[DefaultSettingValue("metric")]
	public string Units
	{
		get
		{
			return (string)((SettingsBase)this)["Units"];
		}
		set
		{
			((SettingsBase)this)["Units"] = value;
		}
	}

	[UserScopedSetting]
	[DebuggerNonUserCode]
	[DefaultSettingValue("False")]
	public bool Timestamp
	{
		get
		{
			return (bool)((SettingsBase)this)["Timestamp"];
		}
		set
		{
			((SettingsBase)this)["Timestamp"] = value;
		}
	}

	[UserScopedSetting]
	[DebuggerNonUserCode]
	[DefaultSettingValue("False")]
	public bool CCDBusOnDemand
	{
		get
		{
			return (bool)((SettingsBase)this)["CCDBusOnDemand"];
		}
		set
		{
			((SettingsBase)this)["CCDBusOnDemand"] = value;
		}
	}

	[UserScopedSetting]
	[DebuggerNonUserCode]
	[DefaultSettingValue("False")]
	public bool PCIBusOnDemand
	{
		get
		{
			return (bool)((SettingsBase)this)["PCIBusOnDemand"];
		}
		set
		{
			((SettingsBase)this)["PCIBusOnDemand"] = value;
		}
	}

	[UserScopedSetting]
	[DebuggerNonUserCode]
	[DefaultSettingValue("True")]
	public bool SortByID
	{
		get
		{
			return (bool)((SettingsBase)this)["SortByID"];
		}
		set
		{
			((SettingsBase)this)["SortByID"] = value;
		}
	}

	[UserScopedSetting]
	[DebuggerNonUserCode]
	[DefaultSettingValue("English")]
	public string Language
	{
		get
		{
			return (string)((SettingsBase)this)["Language"];
		}
		set
		{
			((SettingsBase)this)["Language"] = value;
		}
	}

	[UserScopedSetting]
	[DebuggerNonUserCode]
	[DefaultSettingValue("True")]
	public bool DisplayRawBusPackets
	{
		get
		{
			return (bool)((SettingsBase)this)["DisplayRawBusPackets"];
		}
		set
		{
			((SettingsBase)this)["DisplayRawBusPackets"] = value;
		}
	}

	[UserScopedSetting]
	[DebuggerNonUserCode]
	[DefaultSettingValue("250000")]
	public int UART0Baudrate
	{
		get
		{
			return (int)((SettingsBase)this)["UART0Baudrate"];
		}
		set
		{
			((SettingsBase)this)["UART0Baudrate"] = value;
		}
	}
}
