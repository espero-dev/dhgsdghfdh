using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

namespace ChryslerScanner;

public class AboutForm : Form
{
	public MainForm OriginalForm;

	private IContainer components;

	private Label GUIFWHWVersionLabel;

	private Label AboutDescriptionLabel01;

	private Label AboutTitleLabel;

	private Label AboutDescriptionLabel02;

	private Label AboutDescriptionLabel03;

	private LinkLabel BlogLinkLabel;

	public AboutForm(MainForm IncomingForm)
	{
		OriginalForm = IncomingForm;
		InitializeComponent();
		((Form)this).Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
		((Control)GUIFWHWVersionLabel).Text = "GUI v" + Assembly.GetExecutingAssembly().GetName().Version.ToString(3) + "   |   FW ";
		if (OriginalForm.FWVersion != string.Empty)
		{
			Label gUIFWHWVersionLabel = GUIFWHWVersionLabel;
			((Control)gUIFWHWVersionLabel).Text = ((Control)gUIFWHWVersionLabel).Text + OriginalForm.FWVersion;
		}
		else
		{
			Label gUIFWHWVersionLabel2 = GUIFWHWVersionLabel;
			((Control)gUIFWHWVersionLabel2).Text = ((Control)gUIFWHWVersionLabel2).Text + "N/A";
		}
		Label gUIFWHWVersionLabel3 = GUIFWHWVersionLabel;
		((Control)gUIFWHWVersionLabel3).Text = ((Control)gUIFWHWVersionLabel3).Text + "   |   HW ";
		if (OriginalForm.HWVersion != string.Empty)
		{
			Label gUIFWHWVersionLabel4 = GUIFWHWVersionLabel;
			((Control)gUIFWHWVersionLabel4).Text = ((Control)gUIFWHWVersionLabel4).Text + OriginalForm.HWVersion;
		}
		else
		{
			Label gUIFWHWVersionLabel5 = GUIFWHWVersionLabel;
			((Control)gUIFWHWVersionLabel5).Text = ((Control)gUIFWHWVersionLabel5).Text + "N/A";
		}
	}

	private void BlogLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
	{
		Process.Start("https://chryslerccdsci.wordpress.com/");
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
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Expected O, but got Unknown
		ComponentResourceManager componentResourceManager = new ComponentResourceManager(typeof(AboutForm));
		GUIFWHWVersionLabel = new Label();
		AboutDescriptionLabel01 = new Label();
		AboutTitleLabel = new Label();
		AboutDescriptionLabel02 = new Label();
		AboutDescriptionLabel03 = new Label();
		BlogLinkLabel = new LinkLabel();
		((Control)this).SuspendLayout();
		componentResourceManager.ApplyResources(GUIFWHWVersionLabel, "GUIFWHWVersionLabel");
		((Control)GUIFWHWVersionLabel).Name = "GUIFWHWVersionLabel";
		componentResourceManager.ApplyResources(AboutDescriptionLabel01, "AboutDescriptionLabel01");
		((Control)AboutDescriptionLabel01).Name = "AboutDescriptionLabel01";
		componentResourceManager.ApplyResources(AboutTitleLabel, "AboutTitleLabel");
		((Control)AboutTitleLabel).Name = "AboutTitleLabel";
		componentResourceManager.ApplyResources(AboutDescriptionLabel02, "AboutDescriptionLabel02");
		((Control)AboutDescriptionLabel02).Name = "AboutDescriptionLabel02";
		componentResourceManager.ApplyResources(AboutDescriptionLabel03, "AboutDescriptionLabel03");
		((Control)AboutDescriptionLabel03).Name = "AboutDescriptionLabel03";
		componentResourceManager.ApplyResources(BlogLinkLabel, "BlogLinkLabel");
		((Control)BlogLinkLabel).Name = "BlogLinkLabel";
		BlogLinkLabel.TabStop = true;
		BlogLinkLabel.LinkClicked += new LinkLabelLinkClickedEventHandler(BlogLinkLabel_LinkClicked);
		componentResourceManager.ApplyResources(this, "$this");
		((ContainerControl)this).AutoScaleMode = (AutoScaleMode)2;
		((Control)this).Controls.Add((Control)(object)BlogLinkLabel);
		((Control)this).Controls.Add((Control)(object)AboutDescriptionLabel03);
		((Control)this).Controls.Add((Control)(object)AboutDescriptionLabel02);
		((Control)this).Controls.Add((Control)(object)GUIFWHWVersionLabel);
		((Control)this).Controls.Add((Control)(object)AboutDescriptionLabel01);
		((Control)this).Controls.Add((Control)(object)AboutTitleLabel);
		((Form)this).FormBorderStyle = (FormBorderStyle)1;
		((Form)this).MaximizeBox = false;
		((Form)this).MinimizeBox = false;
		((Control)this).Name = "AboutForm";
		((Control)this).ResumeLayout(false);
		((Control)this).PerformLayout();
	}
}
