/* 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using GlacialComponents.Controls;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using Teltec.Common.Extensions;
using Teltec.Common.Utils;

namespace Teltec.Storage.Monitor
{
	public partial class TransferListControl : GlacialList, ITransferMonitor
	{
		GLColumn PathColumn = new GLColumn();
		GLColumn RemainingColumn = new GLColumn();
		GLColumn ProgressColumn = new GLColumn();

		public TransferListControl()
		{
			InitializeComponent();
			Configure();
		}

		public void Configure()
		{
			this.AllowColumnResize = true;
			this.AllowMultiselect = false;
			this.AlternateBackground = Color.AliceBlue;
			this.AlternatingColors = true;
			this.AutoHeight = true;
			this.BackColor = SystemColors.ControlLightLight;
			this.BackgroundStretchToFit = true;
			this.CausesValidation = false;
			this.ControlStyle = GLControlStyles.XP;
			this.FullRowSelect = true;
			this.GridColor = Color.LightGray;
			this.GridLines = GLGridLines.gridHorizontal;
			this.GridLineStyle = GLGridLineStyles.gridSolid;
			this.GridTypes = GLGridTypes.gridOnExists;
			this.HeaderHeight = 22;
			this.HeaderVisible = true;
			this.HeaderWordWrap = false;
			this.HotColumnTracking = false;
			this.HotItemTracking = false;
			this.HotTrackingColor = Color.LightGray;
			this.HoverEvents = true;
			this.HoverTime = 1;
			this.ImageList = null;
			this.ItemHeight = 17;
			this.ItemWordWrap = false;
			this.Location = new Point(12, 12);
			this.Name = "transferListControl1";
			this.Selectable = true;
			this.SelectedTextColor = Color.White;
			this.SelectionColor = Color.LightSteelBlue;
			this.ShowBorder = true;
			this.ShowFocusRect = false;
			this.Size = new Size(307, 125);
			this.SortType = SortTypes.InsertionSort;
			this.SuperFlatHeaderColor = Color.White;
			this.TabIndex = 8;

			PathColumn.ActivatedEmbeddedType = GLActivatedEmbeddedTypes.UserType;
			PathColumn.CheckBoxes = false;
			PathColumn.ImageIndex = -1;
			PathColumn.Name = "PathColumn";
			PathColumn.NumericSort = false;
			PathColumn.Text = "Path";
			PathColumn.TextAlignment = ContentAlignment.MiddleLeft;
			PathColumn.Width = 170;

			RemainingColumn.ActivatedEmbeddedType = GLActivatedEmbeddedTypes.UserType;
			RemainingColumn.CheckBoxes = false;
			RemainingColumn.ImageIndex = -1;
			RemainingColumn.Name = "RemainingColumn";
			RemainingColumn.NumericSort = false;
			RemainingColumn.Text = "Remaining";
			RemainingColumn.TextAlignment = ContentAlignment.MiddleLeft;
			RemainingColumn.Width = 65;

			ProgressColumn.ActivatedEmbeddedType = GLActivatedEmbeddedTypes.UserType;
			ProgressColumn.CheckBoxes = false;
			ProgressColumn.ImageIndex = -1;
			ProgressColumn.Name = "ProgressColumn";
			ProgressColumn.NumericSort = false;
			ProgressColumn.Text = "Progress";
			ProgressColumn.TextAlignment = ContentAlignment.MiddleLeft;

			this.Columns.AddRange(new GLColumn[] { PathColumn, RemainingColumn, ProgressColumn });
		}

		protected override void OnHandleCreated(EventArgs e)
		{
			base.OnHandleCreated(e);
			ProgressColumn.Width = this.Width - PathColumn.Width - RemainingColumn.Width - 4;
		}

		internal enum ColumnIndex
		{
			Path = 0,
			Remaining,
			Progress,
		}

		internal class TransferEntry : IDisposable
		{
			internal TransferFileProgressArgs Data;
			internal Label Path; // IDisposable
			internal Label Remaining; // IDisposable
			internal Label Message; // IDisposable
			internal ProgressBar Progress; // IDisposable
			internal GLItem Item; // ATTENTION: Circular-reference!
			internal ToolTip Tooltip;
			internal Exception Exception;

			private void ControlDefaults(Control control)
			{
				control.Click += BubbleUpClickEvent; // Propagate to parent.
				if (!(control is ProgressBar))
					control.BackColor = Color.Transparent;
			}

			public TransferEntry(TransferFileProgressArgs other)
			{
				Data = other;

				Path = new Label();
				ControlDefaults(Path);
				Path.Name = "Path";
				Path.Text = Data.FilePath;
				// NOTE: Cannot use DataBindings because the change event is raised on another thread.
				Path.DataBindings.Add(new Binding("Text", Data,
					this.GetPropertyName((TransferFileProgressArgs x) => x.FilePath)));

				Remaining = new Label();
				ControlDefaults(Remaining);
				Remaining.Name = "Remaining";
				Remaining.Text = "";
				Binding RemainingBinding = new Binding("Text", Data,
					this.GetPropertyName((TransferFileProgressArgs x) => x.RemainingBytes));
				RemainingBinding.Format += FileSizeUtils.FileSizeToString;
				Remaining.DataBindings.Add(RemainingBinding);

				Message = new Label();
				ControlDefaults(Message);
				Message.Name = "Message";
				Message.Text = "";
				Message.ForeColor = Color.Red;

				Progress = new ExtendedProgressBar();
				ControlDefaults(Progress);
				Progress.Name = "Progress";
				Progress.Minimum = 0;
				Progress.Maximum = 100;
				// NOTE: Cannot use DataBindings because the change event is raised on another thread.
				Progress.DataBindings.Add(new Binding("Value", Data,
					this.GetPropertyName((TransferFileProgressArgs x) => x.PercentDone)));
			}

			void BubbleUpClickEvent(object sender, EventArgs e)
			{
				TransferEntry entry = this as TransferEntry;
				//TransferListControl parent = entry.Item.Parent as TransferListControl;
				entry.Item.Selected = true;
			}

			#region Dispose Pattern Implementation

			bool _shouldDispose = true;
			bool _isDisposed;

			/// <summary>
			/// Implements the Dispose pattern
			/// </summary>
			/// <param name="disposing">Whether this object is being disposed via a call to Dispose
			/// or garbage collected.</param>
			protected virtual void Dispose(bool disposing)
			{
				if (!this._isDisposed)
				{
					if (disposing && _shouldDispose)
					{
						if (Path != null)
						{
							Path.Dispose();
							Path = null;
						}
						if (Remaining != null)
						{
							Remaining.Dispose();
							Remaining = null;
						}
						if (Message != null)
						{
							Message.Dispose();
							Message = null;
						}
						if (Progress != null)
						{
							Progress.Dispose();
							Progress = null;
						}
					}
					this._isDisposed = true;
				}
			}

			/// <summary>
			/// Disposes of all managed and unmanaged resources.
			/// </summary>
			public void Dispose()
			{
				this.Dispose(true);
				GC.SuppressFinalize(this);
			}

			#endregion
		}

		// List of transfers indexed by their source paths.
		FastLookupBindingList<string, TransferEntry> Transfers =
			new FastLookupBindingList<string, TransferEntry>(p => p.Data.FilePath);

		public void ClearTransfers()
		{
			this.Items.Clear();
			this.Transfers.Clear();
		}

		public void TransferStarted(object sender, TransferFileProgressArgs args)
		{
			string key = args.FilePath;
			TransferEntry entry = new TransferEntry(args);
			Debug.Assert(entry.Data.State == TransferState.STARTED);
			AddItemToList(entry);
			OnChangedTransferState(entry);
		}

		public void TransferProgress(object sender, TransferFileProgressArgs args)
		{
			string key = args.FilePath;
			TransferEntry entry = Transfers[key];
			if (entry == null)
				return;
			Debug.Assert(entry.Data.State == TransferState.TRANSFERRING);
			//entry.Remaining.Text = FileSizeToString(entry.Data.TotalBytes - entry.Data.TransferredBytes);
			OnChangedTransferState(entry);
		}

		public void TransferFailed(object sender, TransferFileProgressArgs args)
		{
			string key = args.FilePath;
			TransferEntry entry = Transfers[key];
			if (entry == null)
				return;
			Debug.Assert(entry.Data.State == TransferState.FAILED);
			entry.Exception = args.Exception;
			OnChangedTransferState(entry);
		}

		public void TransferCanceled(object sender, TransferFileProgressArgs args)
		{
			string key = args.FilePath;
			TransferEntry entry = Transfers[key];
			if (entry == null)
				return;
			Debug.Assert(entry.Data.State == TransferState.CANCELED);
			entry.Exception = args.Exception;
			OnChangedTransferState(entry);
		}

		public void TransferCompleted(object sender, TransferFileProgressArgs args)
		{
			string key = args.FilePath;
			TransferEntry entry = Transfers[key];
			if (entry == null)
				return;
			Debug.Assert(entry.Data.State == TransferState.COMPLETED);
			OnChangedTransferState(entry);
		}

		private void OnChangedTransferState(TransferEntry entry)
		{
			switch (entry.Data.State)
			{
				case TransferState.CANCELED:
					CanceledItemFromList(entry);
					break;
				case TransferState.FAILED:
					FailedItemFromList(entry);
					break;
				case TransferState.COMPLETED:
					RemoveItemFromList(entry);
					break;
			}
		}

		private void RemoveItemFromList(TransferEntry entry)
		{
			this.Items.Remove(entry.Item);
			this.Invalidate();
		}

		private void CanceledItemFromList(TransferEntry entry)
		{
			entry.Remaining.Text = "";
			entry.Message.Text = "Canceled";
			//entry.Item.BackColor = Color.SeaShell;
			entry.Item.SubItems[(int)ColumnIndex.Progress].Control = entry.Message;
			this.Invalidate();
		}

		private void FailedItemFromList(TransferEntry entry)
		{
			entry.Remaining.Text = "";
			entry.Message.Text = "Failed";
			entry.Item.BackColor = Color.SeaShell;
			entry.Item.SubItems[(int)ColumnIndex.Progress].Control = entry.Message;
			this.Invalidate();

			if (entry.Tooltip == null)
			{
				entry.Tooltip = new ToolTip();
				entry.Tooltip.AutoPopDelay = 5000;
				entry.Tooltip.InitialDelay = 100;
				entry.Tooltip.ReshowDelay = 100;
				entry.Tooltip.ToolTipIcon = ToolTipIcon.Error;
				entry.Tooltip.ToolTipTitle = "Transfer failed";
			}
			entry.Tooltip.SetToolTip(entry.Message, entry.Exception.Message);
		}

		private void AddItemToList(TransferEntry entry)
		{
			// Add to our fast-lookup collection.
			Transfers.Add(entry);

			// Add to UI.
			GLItem item = this.Items.Add("");
			item.Tag = entry;
			item.SubItems[(int)ColumnIndex.Path].Control = entry.Path;
			item.SubItems[(int)ColumnIndex.Remaining].Control = entry.Remaining;
			item.SubItems[(int)ColumnIndex.Progress].Control = entry.Progress;
			entry.Item = item;

			// Invalidation is required in order to immediately display the control.
			this.Invalidate();
		}
	}
}
