using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace ChryslerScanner;

public class FlickerFreeListBox : ListBox
{
	public delegate void ListBoxScrolledEventHandler(object sender, EventArgs e);

	private struct ScrollInfoStruct
	{
		public int cbSize;

		public int fMask;

		public int nMin;

		public int nMax;

		public int nPage;

		public int nPos;

		public int nTrackPos;
	}

	private const int WM_HSCROLL = 276;

	private const int WM_VSCROLL = 277;

	private const int SB_HORIZONTAL = 0;

	private const int SB_VERTICAL = 1;

	private const int SB_PAGELEFT = 2;

	private const int SB_PAGERIGHT = 3;

	private const int SB_THUMBPOSITION = 4;

	private const int SB_THUMBTRACK = 5;

	private const int SB_TOP = 6;

	private const int SB_LEFT = 6;

	private const int SB_BOTTOM = 7;

	private const int SB_RIGHT = 7;

	private const int SB_ENDSCROLL = 8;

	private const int SIF_TRACKPOS = 16;

	private const int SIF_RANGE = 1;

	private const int SIF_POS = 4;

	private const int SIF_PAGE = 2;

	private const int SIF_ALL = 23;

	public event ListBoxScrolledEventHandler Scrolled;

	[DllImport("user32.dll")]
	private static extern int GetScrollInfo(IntPtr hWnd, int n, ref ScrollInfoStruct lpScrollInfo);

	[DllImport("user32.dll")]
	private static extern int SetScrollInfo(IntPtr hwnd, int fnBar, [In] ref ScrollInfoStruct lpsi, bool fRedraw);

	[DllImport("user32.dll")]
	private static extern int GetScrollPos(IntPtr hWnd, int nBar);

	[DllImport("user32.dll")]
	private static extern IntPtr SendMessage(HandleRef hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

	public FlickerFreeListBox()
	{
		((Control)this).SetStyle((ControlStyles)139266, true);
		((ListBox)this).DrawMode = (DrawMode)1;
	}

	protected override void OnDrawItem(DrawItemEventArgs e)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Invalid comparison between Unknown and I4
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Expected O, but got Unknown
		if (((ListBox)this).Items.Count > 0)
		{
			e.DrawBackground();
			if ((e.State & 1) == 1)
			{
				e.Graphics.FillRectangle((Brush)new SolidBrush(((Control)this).BackColor), e.Bounds);
			}
			if (((Control)this).Enabled)
			{
				TextRenderer.DrawText((IDeviceContext)(object)e.Graphics, ((ListControl)this).GetItemText(((ListBox)this).Items[e.Index]), ((Control)this).Font, e.Bounds, ((Control)this).ForeColor, (TextFormatFlags)0);
			}
			else
			{
				TextRenderer.DrawText((IDeviceContext)(object)e.Graphics, ((ListControl)this).GetItemText(((ListBox)this).Items[e.Index]), ((Control)this).Font, e.Bounds, Color.Gray, (TextFormatFlags)0);
			}
		}
		((ListBox)this).OnDrawItem(e);
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Expected O, but got Unknown
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Expected O, but got Unknown
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Invalid comparison between Unknown and I4
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Invalid comparison between Unknown and I4
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Invalid comparison between Unknown and I4
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Expected O, but got Unknown
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Expected O, but got Unknown
		Region val = new Region(e.ClipRectangle);
		e.Graphics.FillRegion((Brush)new SolidBrush(((Control)this).BackColor), val);
		if (((ListBox)this).Items.Count > 0)
		{
			for (int i = 0; i < ((ListBox)this).Items.Count; i++)
			{
				Rectangle itemRectangle = ((ListBox)this).GetItemRectangle(i);
				if (e.ClipRectangle.IntersectsWith(itemRectangle))
				{
					if (((int)((ListBox)this).SelectionMode == 1 && ((ListControl)this).SelectedIndex == i) || ((int)((ListBox)this).SelectionMode == 2 && ((ListBox)this).SelectedIndices.Contains(i)) || ((int)((ListBox)this).SelectionMode == 3 && ((ListBox)this).SelectedIndices.Contains(i)))
					{
						((ListBox)this).OnDrawItem(new DrawItemEventArgs(e.Graphics, ((Control)this).Font, itemRectangle, i, (DrawItemState)1, ((Control)this).ForeColor, ((Control)this).BackColor));
					}
					else
					{
						((ListBox)this).OnDrawItem(new DrawItemEventArgs(e.Graphics, ((Control)this).Font, itemRectangle, i, (DrawItemState)32, ((Control)this).ForeColor, ((Control)this).BackColor));
					}
					val.Complement(itemRectangle);
				}
			}
		}
		((Control)this).OnPaint(e);
	}

	protected override void WndProc(ref Message m)
	{
		if (((((Message)(ref m)).Msg == 276) | (((Message)(ref m)).Msg == 277)) && this.Scrolled != null)
		{
			ScrollInfoStruct lpScrollInfo = default(ScrollInfoStruct);
			lpScrollInfo.fMask = 23;
			lpScrollInfo.cbSize = Marshal.SizeOf(lpScrollInfo);
			GetScrollInfo(((Message)(ref m)).HWnd, 0, ref lpScrollInfo);
			((Message)(ref m)).WParam.ToInt32();
			this.Scrolled?.Invoke(this, EventArgs.Empty);
		}
		((ListBox)this).WndProc(ref m);
	}

	public int GetVerticalScrollPosition()
	{
		return GetScrollPos(((Control)this).Handle, 1);
	}

	public void SetVerticalScrollPosition(int vertScrollPos)
	{
		ScrollInfoStruct structure = default(ScrollInfoStruct);
		int fMask = 16;
		structure.cbSize = Marshal.SizeOf(structure);
		structure.nTrackPos = vertScrollPos;
		structure.fMask = fMask;
		SendMessage(wParam: new IntPtr((vertScrollPos << 16) | 4), hWnd: new HandleRef(null, ((Control)this).Handle), Msg: 277u, lParam: IntPtr.Zero);
		SendMessage(wParam: new IntPtr(8), hWnd: new HandleRef(null, ((Control)this).Handle), Msg: 277u, lParam: IntPtr.Zero);
	}
}
