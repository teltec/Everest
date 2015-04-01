namespace Teltec.Common.Controls
{
	partial class AdvancedTreeView
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Component Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.stateImageList = new System.Windows.Forms.ImageList(this.components);
			this.SuspendLayout();
			// 
			// stateImageList
			// 
			this.stateImageList.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
			this.stateImageList.ImageSize = new System.Drawing.Size(16, 16);
			this.stateImageList.TransparentColor = System.Drawing.Color.Transparent;
			// 
			// FileSystemTreeView
			// 
			this.StateImageList = this.stateImageList;
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.ImageList stateImageList;

	}
}
