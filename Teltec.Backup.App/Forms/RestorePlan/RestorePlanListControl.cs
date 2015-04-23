using NLog;
using System;
using System.Windows.Forms;
using Teltec.Backup.App.DAO;

namespace Teltec.Backup.App.Forms.RestorePlan
{
	public partial class RestorePlanListControl : UserControl
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();
		private readonly RestorePlanRepository dao = new RestorePlanRepository();

		public RestorePlanListControl()
		{
			InitializeComponent();
			this.layoutPanel.ControlAdded += layoutPanel_ControlAdded;
		}

		void layoutPanel_ControlAdded(object sender, ControlEventArgs e)
		{
			this.layoutPanel.SetFlowBreak(e.Control, true);
		}

		public void LoadPlans()
		{
			this.layoutPanel.Controls.Clear();

			var plans = dao.GetAll();

			foreach (var plan in plans)
			{
				RestorePlanViewControl viewControl = new RestorePlanViewControl();
				viewControl.Model = plan;
				viewControl.Deleted += (object sender, EventArgs e) =>
				{
					RestorePlanViewControl control = sender as RestorePlanViewControl;
					layoutPanel.Controls.Remove(control);
				};
				this.layoutPanel.Controls.Add(viewControl);
			}
		}

		public void RefreshPlans()
		{
			LoadPlans();
		}
	}
}
