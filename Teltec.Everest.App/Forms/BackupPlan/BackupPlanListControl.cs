using NLog;
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Teltec.Everest.Data.DAO;
using Models = Teltec.Everest.Data.Models;

namespace Teltec.Everest.App.Forms.BackupPlan
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

		public bool ControlsAlreadyContainControlForPlan(Models.BackupPlan plan)
		{
			foreach (Control ctrl in this.layoutPanel.Controls)
			{
				if (!(ctrl is BackupPlanViewControl))
					continue;

				BackupPlanViewControl obj = ctrl as BackupPlanViewControl;
				Models.BackupPlan objPlan = obj.Model as Models.BackupPlan;

				if (objPlan.Id.Equals(plan.Id))
					return true;
			}
			return false;
		}

		public void RemoveAllExceptRunning()
		{
			List<Control> toBeRemoved = new List<Control>();

			// Remove all that are not running.
			foreach (Control ctrl in this.layoutPanel.Controls)
			{
				if (!(ctrl is BackupPlanViewControl))
					continue;

				BackupPlanViewControl obj = ctrl as BackupPlanViewControl;
				if (!obj.OperationIsRunning)
					toBeRemoved.Add(ctrl);
			}

			// Remove them.
			foreach (Control ctrl in toBeRemoved)
				this.layoutPanel.Controls.Remove(ctrl);
		}

		public void LoadPlans()
		{
			RemoveAllExceptRunning();

			var plans = dao.GetAllActive();

			foreach (var plan in plans)
			{
				if (ControlsAlreadyContainControlForPlan(plan))
					continue;

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
