using System;
using System.Windows.Forms;
using ChryslerScanner.Services;

namespace ChryslerScanner;

internal static class Program
{
	[STAThread]
	private static void Main()
	{
		Application.EnableVisualStyles();
		Application.SetCompatibleTextRenderingDefault(false);
		Application.Run((Form)(object)ContainerManager.Instance.GetInstance<MainForm>());
	}
}
