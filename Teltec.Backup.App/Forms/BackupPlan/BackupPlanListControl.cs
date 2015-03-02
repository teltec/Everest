using NLog;
using System;
using System.Windows.Forms;
using Teltec.Backup.App.DAO;

namespace Teltec.Backup.App.Forms.BackupPlan
{
	public partial class BackupPlanListControl : UserControl
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();
		private readonly BackupPlanRepository dao = new BackupPlanRepository();

		public BackupPlanListControl()
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
				BackupPlanViewControl viewControl = new BackupPlanViewControl();
				viewControl.Model = plan;
				viewControl.Deleted += (object sender, EventArgs e) =>
				{
					BackupPlanViewControl control = sender as BackupPlanViewControl;
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
