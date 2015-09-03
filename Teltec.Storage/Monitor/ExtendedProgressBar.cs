using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Teltec.Storage.Monitor
{
	public enum ProgressBarDisplayText
	{
		Percentage,
		CustomText
	}

	//
	// "How do I put text on ProgressBar?" by "Barry" is licensed under CC BY-SA 3.0
	//
	// Title?   How do I put text on ProgressBar?
	// Author?  Barry - http://stackoverflow.com/users/300863/barry
	// Source?  http://stackoverflow.com/a/3529945/298054
	// License? CC BY-SA 3.0 - https://creativecommons.org/licenses/by-sa/3.0/legalcode
	//
	public class ExtendedProgressBar: ProgressBar
	{
		//Property to set to decide whether to print a % or Text
		public ProgressBarDisplayText DisplayStyle { get; set; }

		public Brush TextColor { get; set; }

		//Property to hold the custom text
		public String CustomText { get; set; }

		public ExtendedProgressBar()
		{
			// Modify the ControlStyles flags
			//http://msdn.microsoft.com/en-us/library/system.windows.forms.controlstyles.aspx
			SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint, true);
			this.Style = ProgressBarStyle.Continuous;
			TextColor = Brushes.Black;
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			Rectangle rect = ClientRectangle;
			Graphics g = e.Graphics;

			ProgressBarRenderer.DrawHorizontalBar(g, rect);
			rect.Inflate(-1, -1); // Padding

			if (Value > 0)
			{
				// As we doing this ourselves we need to draw the chunks on the progress bar
				Rectangle clip = new Rectangle(rect.X, rect.Y,
					(int)Math.Round(((float)Value / Maximum) * rect.Width),
					rect.Height);
				ProgressBarRenderer.DrawHorizontalChunks(g, clip);
			}

			// Set the Display text (Either a % amount or our custom text
			string text = DisplayStyle == ProgressBarDisplayText.Percentage ? Value.ToString() + '%' : CustomText;

			Font font = SystemFonts.DefaultFont;
			//using (Font font = new Font(FontFamily.GenericSansSerif, 7, FontStyle.Bold))
			//{
				SizeF len = g.MeasureString(text, font);
				// Calculate the location of the text (the middle of progress bar)
				// Point location = new Point(Convert.ToInt32((rect.Width / 2) - (len.Width / 2)), Convert.ToInt32((rect.Height / 2) - (len.Height / 2)));
				Point location = new Point(Convert.ToInt32((Width / 2) - len.Width / 2), Convert.ToInt32((Height / 2) - len.Height / 2));
				// The commented-out code will centre the text into the highlighted area only. This will centre the text regardless of the highlighted area.
				// Draw the custom text
				g.DrawString(text, font, TextColor, location);
			//}
		}

		//[DllImportAttribute("uxtheme.dll")]
		//private static extern int SetWindowTheme(IntPtr hWnd, string appname, string idlist);
		//
		//protected override void OnHandleCreated(EventArgs e)
		//{
		//	SetWindowTheme(this.Handle, "", "");
		//	base.OnHandleCreated(e);
		//}
	}
}
