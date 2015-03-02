using GlacialComponents.Controls;
using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Teltec.Storage.Monitor
{
	// IMPORTANT: Don't let the code generator trash the forms using this class
	//            with columns creation code.
	public partial class TransferListControl
	{
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public new Control ActivatedEmbeddedControl
		{ get { return base.ActivatedEmbeddedControl; } set { base.ActivatedEmbeddedControl = value; } }

		//[Browsable(true)]
		[Browsable(false)] // ADDED
		[Category("Header")]
		[Description("Allow resizing of columns")]
		[RefreshProperties(RefreshProperties.Repaint)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)] // ADDED
		public new bool AllowColumnResize
		{ get { return base.AllowColumnResize; } set { base.AllowColumnResize = value; } }

		//[Browsable(true)]
		[Browsable(false)] // ADDED
		[Category("Item")]
		[Description("Allow multiple selections.")]
		[RefreshProperties(RefreshProperties.Repaint)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)] // ADDED
		public new bool AllowMultiselect
		{ get { return base.AllowMultiselect; } set { base.AllowMultiselect = value; } }

		//[Browsable(true)]
		[Browsable(false)] // ADDED
		[Category("Item Alternating Colors")]
		[Description("Color for text in boxes that are selected.")]
		[RefreshProperties(RefreshProperties.Repaint)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)] // ADDED
		public new Color AlternateBackground
		{ get { return base.AlternateBackground; } set { base.AlternateBackground = value; } }

		//[Browsable(true)]
		[Browsable(false)] // ADDED
		[Category("Item Alternating Colors")]
		[Description("turn xp themes on or not")]
		[RefreshProperties(RefreshProperties.Repaint)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)] // ADDED
		public new bool AlternatingColors
		{ get { return base.AlternatingColors; } set { base.AlternatingColors = value; } }

		//[Browsable(true)]
		[Browsable(false)] // ADDED
		[Category("Item")]
		[Description("Do we want rows to automatically adjust height")]
		[RefreshProperties(RefreshProperties.Repaint)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)] // ADDED
		public new bool AutoHeight
		{ get { return base.AutoHeight; } set { base.AutoHeight = value; } }

		//[Browsable(true)]
		[Browsable(false)] // ADDED
		[Category("Behavior")]
		[Description("Whether or not to stretch background to fit inner list area.")]
		[RefreshProperties(RefreshProperties.Repaint)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)] // ADDED
		public new bool BackgroundStretchToFit
		{ get { return base.BackgroundStretchToFit; } set { base.BackgroundStretchToFit = value; } }

		[Browsable(false)]
		[Description("Cell padding area")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public new int CellPaddingSize
		{ get { return base.CellPaddingSize; } }

		//[Browsable(true)]
		[Browsable(false)] // ADDED
		[Category("Header")]
		[Description("Column Collection")]
		//[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)] // ADDED
		public new GLColumnCollection Columns
		{ get { return base.Columns; } }

		//[Browsable(true)]
		[Browsable(false)] // ADDED
		[Category("Behavior")]
		[Description("Overall look of control")]
		[RefreshProperties(RefreshProperties.Repaint)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)] // ADDED
		public new GLControlStyles ControlStyle
		{ get { return base.ControlStyle; } set { base.ControlStyle = value; } }

		[Browsable(false)]
		[Category("Behavior")]
		[DefaultValue(0)]
		[Description("Number of items/rows in the list.")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public new int Count
		{ get { return base.Count; } }

		[Browsable(false)]
		[Description("Currently Focused Item")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public new GLItem FocusedItem
		{ get { return base.FocusedItem; } set { base.FocusedItem = value; } }

		//[Browsable(true)]
		[Browsable(false)] // ADDED
		[Category("Item")]
		[Description("Allow full row select.")]
		[RefreshProperties(RefreshProperties.Repaint)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)] // ADDED
		public new bool FullRowSelect
		{ get { return base.FullRowSelect; } set { base.FullRowSelect = value; } }

		//[Browsable(true)]
		[Browsable(false)] // ADDED
		[Category("Grid")]
		[Description("Color of the grid if we draw it.")]
		[RefreshProperties(RefreshProperties.Repaint)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)] // ADDED
		public new Color GridColor
		{ get { return base.GridColor; } set { base.GridColor = value; } }

		//[Browsable(true)]
		[Browsable(false)] // ADDED
		[Category("Grid")]
		[Description("Whether or not to draw gridlines")]
		[RefreshProperties(RefreshProperties.Repaint)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)] // ADDED
		public new GLGridLines GridLines
		{ get { return base.GridLines; } set { base.GridLines = value; } }

		//[Browsable(true)]
		[Browsable(false)] // ADDED
		[Category("Grid")]
		[Description("Whether or not to draw gridlines")]
		[RefreshProperties(RefreshProperties.Repaint)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)] // ADDED
		public new GLGridLineStyles GridLineStyle
		{ get { return base.GridLineStyle; } set { base.GridLineStyle = value; } }

		//[Browsable(true)]
		[Browsable(false)] // ADDED
		[Category("Grid")]
		[Description("Whether or not to draw gridlines")]
		[RefreshProperties(RefreshProperties.Repaint)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)] // ADDED
		public new GLGridTypes GridTypes
		{ get { return base.GridTypes; } set { base.GridTypes = value; } }

		//[Browsable(true)]
		[Browsable(false)] // ADDED
		[Category("Header")]
		[Description("How high the columns are.")]
		[RefreshProperties(RefreshProperties.Repaint)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)] // ADDED
		public new int HeaderHeight
		{ get { return base.HeaderHeight; } set { base.HeaderHeight = value; } }

		[Browsable(false)]
		[Description("The rectangle of the header inside parent control")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		protected new Rectangle HeaderRect
		{ get { return base.HeaderRect; } }

		//[Browsable(true)]
		[Browsable(false)] // ADDED
		[Category("Header")]
		[Description("Column Headers Visible")]
		[RefreshProperties(RefreshProperties.Repaint)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)] // ADDED
		public new bool HeaderVisible
		{ get { return base.HeaderVisible; } set { base.HeaderVisible = value; } }

		//[Browsable(true)]
		[Browsable(false)] // ADDED
		[Category("Header")]
		[Description("Word wrap in header")]
		[RefreshProperties(RefreshProperties.Repaint)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)] // ADDED
		public new bool HeaderWordWrap
		{ get { return base.HeaderWordWrap; } set { base.HeaderWordWrap = value; } }

		[Browsable(false)]
		[Description("Currently Focused Column")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public new int HotColumnIndex
		{ get { return base.HotColumnIndex; } set { base.HotColumnIndex = value; } }

		//[Browsable(true)]
		[Browsable(false)] // ADDED
		[Category("Behavior")]
		[Description("Show hot tracking.")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)] // ADDED
		public new bool HotColumnTracking
		{ get { return base.HotColumnTracking; } set { base.HotColumnTracking = value; } }

		[Browsable(false)]
		[Description("Currently Focused Item")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public new int HotItemIndex
		{ get { return base.HotItemIndex; } set { base.HotItemIndex = value; } }

		//[Browsable(true)]
		[Browsable(false)] // ADDED
		[Category("Behavior")]
		[Description("Show hot tracking.")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)] // ADDED
		public new bool HotItemTracking
		{ get { return base.HotItemTracking; } set { base.HotItemTracking = value; } }

		//[Browsable(true)]
		[Browsable(false)] // ADDED
		[Category("Appearance")]
		[Description("Color for hot tracking.")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)] // ADDED
		public new Color HotTrackingColor
		{ get { return base.HotTrackingColor; } set { base.HotTrackingColor = value; } }

		//[Browsable(true)]
		[Browsable(false)] // ADDED
		[Category("Behavior")]
		[Description("Enabling hover events slows the control some but allows you to be informed when a user has hovered over an item.")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)] // ADDED
		public new bool HoverEvents
		{ get { return base.HoverEvents; } set { base.HoverEvents = value; } }

		//[Browsable(true)]
		[Browsable(false)] // ADDED
		[Category("Behavior")]
		[Description("Amount of time in seconds a user hovers before hover event is fired.  Can NOT be zero.")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)] // ADDED
		public new int HoverTime
		{ get { return base.HoverTime; } set { base.HoverTime = value; } }

		//[Browsable(true)]
		[Browsable(false)] // ADDED
		[Category("Behavior")]
		[Description("ImageList to be used in listview.")]
		[RefreshProperties(RefreshProperties.Repaint)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)] // ADDED
		public new ImageList ImageList
		{ get { return base.ImageList; } set { base.ImageList = value; } }

		//[Browsable(true)]
		[Browsable(false)] // ADDED
		[Category("Item")]
		[Description("How high each row is.")]
		[RefreshProperties(RefreshProperties.Repaint)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)] // ADDED
		public new int ItemHeight
		{ get { return base.ItemHeight; } set { base.ItemHeight = value; } }

		//[Browsable(true)]
		[Browsable(false)] // ADDED
		[Category("Item")]
		[Description("Items collection")]
		//[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)] // ADDED
		public new GLItemCollection Items
		{ get { return base.Items; } }

		//[Browsable(true)]
		[Browsable(false)] // ADDED
		[Category("Item")]
		[Description("Word wrap in cells")]
		[RefreshProperties(RefreshProperties.Repaint)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)] // ADDED
		public new bool ItemWordWrap
		{ get { return base.ItemWordWrap; } set { base.ItemWordWrap = value; } }

		[Browsable(false)]
		[Description("this will always reflect the most height any item line has needed")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		protected new int MaxHeight
		{ get { return base.MaxHeight; } set { base.MaxHeight = value; } }

		[Browsable(false)]
		[Description("The rectangle of the client inside parent control")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		protected new Rectangle RowsClientRect
		{ get { return base.RowsClientRect; } }

		[Browsable(false)]
		[Description("The inner rectangle of the client inside parent control taking scroll bars into account.")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public new Rectangle RowsInnerClientRect
		{ get { return base.RowsInnerClientRect; } }

		[Browsable(false)]
		[Description("Full Sized rectangle of all columns total width.")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public new Rectangle RowsRect
		{ get { return base.RowsRect; } }

		//[Browsable(true)]
		[Browsable(false)] // ADDED
		[Category("Behavior")]
		[Description("Items selectable.")]
		[RefreshProperties(RefreshProperties.Repaint)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)] // ADDED
		public new bool Selectable
		{ get { return base.Selectable; } set { base.Selectable = value; } }
		
		[Browsable(false)]
		[Description("Selected Items Array Of Indicies")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public new ArrayList SelectedIndicies
		{ get { return base.SelectedIndicies; } }
		
		[Browsable(false)]
		[Description("Selected Items Array")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public new ArrayList SelectedItems
		{ get { return base.SelectedItems; } }

		//[Browsable(true)]
		[Browsable(false)] // ADDED
		[Category("Item")]
		[Description("Color for text in boxes that are selected.")]
		[RefreshProperties(RefreshProperties.Repaint)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)] // ADDED
		public new Color SelectedTextColor
		{ get { return base.SelectedTextColor; } set { base.SelectedTextColor = value; } }

		//[Browsable(true)]
		[Browsable(false)] // ADDED
		[Category("Item")]
		[Description("Background color to mark selection.")]
		[RefreshProperties(RefreshProperties.Repaint)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)] // ADDED
		public new Color SelectionColor
		{ get { return base.SelectionColor; } set { base.SelectionColor = value; } }

		//[Browsable(true)]
		[Browsable(false)] // ADDED
		[Category("Appearance")]
		[Description("Whether or not to show a border.")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)] // ADDED
		public new bool ShowBorder
		{ get { return base.ShowBorder; } set { base.ShowBorder = value; } }

		//[Browsable(true)]
		[Browsable(false)] // ADDED
		[Category("Item")]
		[Description("Show Focus Rect on items.")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)] // ADDED
		public new bool ShowFocusRect
		{ get { return base.ShowFocusRect; } set { base.ShowFocusRect = value; } }

		//[Browsable(true)]
		[Browsable(false)] // ADDED
		[Category("Behavior")]
		[Description("Type of sorting algorithm used.")]
		[RefreshProperties(RefreshProperties.Repaint)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)] // ADDED
		public new SortTypes SortType
		{ get { return base.SortType; } set { base.SortType = value; } }

		//[Browsable(true)]
		[Browsable(false)] // ADDED
		[Category("Header")]
		[Description("Color for text in boxes that are selected.")]
		[RefreshProperties(RefreshProperties.Repaint)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)] // ADDED
		public new Color SuperFlatHeaderColor
		{ get { return base.SuperFlatHeaderColor; } set { base.SuperFlatHeaderColor = value; } }

		[Browsable(false)]
		[Description("Are Themes Available")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		protected new bool ThemesAvailable
		{ get { return base.ThemesAvailable; } }

		[Browsable(false)]
		[Description("All items together height.")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		protected new int TotalRowHeight
		{ get { return base.TotalRowHeight; } }

		[Browsable(false)]
		[Description("Number of rows currently visible in inner rect.")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		protected new int VisibleRowsCount
		{ get { return base.VisibleRowsCount; } }
	}
}
