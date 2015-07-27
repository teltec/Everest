using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Teltec.Forms.Wizard
{
	public class WizardPresenter : IDisposable
	{
		public class WizardFormOptions
		{
			public Type Type { get; set; }
			public bool DoValidate { get; set; }
		}

		protected internal List<WizardFormOptions> _RegisteredForms = new List<WizardFormOptions>();
		protected internal List<WizardForm> _InstantiatedForms = new List<WizardForm>();
		protected internal int _CurrentFormIndex = 0;
		protected internal Form _Owner;

		protected object _Model;
		protected internal virtual object Model
		{
			get { return _Model; }
			set
			{
				_Model = value;
				foreach (var form in _InstantiatedForms)
				{
					form.Model = _Model;
				}
			}
		}

		protected bool _IsEditingModel = true;
		protected internal bool IsEditingModel
		{
			get { return _IsEditingModel; }
			set { _IsEditingModel = value; }
		}

		~WizardPresenter()
		{
			Dispose(false);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!disposing)
				return;
			_Owner = null; // Break cyclic reference.
			CloseAndDiposeAllForms();
			_RegisteredForms.Clear();
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		public virtual void RegisterFormClass(Type wizardFormType, WizardFormOptions options=null)
		{
			bool isCompatible = typeof(WizardForm).IsAssignableFrom(wizardFormType);
			if (!isCompatible)
			{
				var message = String.Format("Type must be compatible with {0}", typeof(WizardForm).FullName);
				throw new ArgumentException(message, "wizardFormType");
			}
			if (options != null)
				_RegisteredForms.Add(new WizardFormOptions { Type = wizardFormType, DoValidate = options.DoValidate });
			else
				_RegisteredForms.Add(new WizardFormOptions { Type = wizardFormType });
		}

		public virtual void ShowDialog(Form owner)
		{
			_Owner = owner;
			_CurrentFormIndex = 0;
			CurrentForm.ShowDialog(_Owner);
			GC.ReRegisterForFinalize(this);
		}

		protected virtual WizardForm InstantiateForm(int index, Form owner)
		{
			// Instantiate it.
			WizardFormOptions options = _RegisteredForms.ElementAt(index);
			object instance = Activator.CreateInstance(options.Type);
			WizardForm form = (WizardForm)instance;
			form.Owner = owner;
			form.IsLastForm = index == _RegisteredForms.Count - 1;
			form.FinishEnabled = form.IsLastForm;
			form.NextEnabled = !form.IsLastForm;
			form.PreviousEnabled = index > 0;
			form.FormClosedEvent += form_FormClosedEvent;
			form.CancelEvent += form_CancelEvent;
			form.FinishEvent += form_FinishEvent;
			form.PreviousEvent += form_PreviousEvent;
			form.NextEvent += form_NextEvent;
			form.Model = Model;
			form.DoValidate = options.DoValidate;
			return form;
		}

		protected virtual void form_FormClosedEvent(WizardForm sender, EventArgs e)
		{
			OnFormClosed();
		}

		protected virtual void form_CancelEvent(WizardForm sender, EventArgs e)
		{
			OnCancel();
		}

		protected virtual void form_FinishEvent(WizardForm sender, EventArgs e)
		{
			OnFinish();
		}

		protected virtual void form_PreviousEvent(WizardForm sender, EventArgs e)
		{
			MoveToPrevious();
		}

		protected virtual void form_NextEvent(WizardForm sender, EventArgs e)
		{
			MoveToNext();
		}

		public virtual WizardForm CurrentForm
		{
			get
			{
				if (_InstantiatedForms.Count > _CurrentFormIndex)
					return _InstantiatedForms.ElementAt(_CurrentFormIndex);

				WizardForm form = InstantiateForm(_CurrentFormIndex, _Owner);
				_InstantiatedForms.Add(form);
				return form;
			}
		}

		public virtual int CurrentFormIndex
		{
			get { return _CurrentFormIndex; }
			protected set { _CurrentFormIndex = value; }
		}

		public virtual void MoveToPrevious()
		{
			CurrentForm.Hide();
			CurrentFormIndex--;
			CurrentForm.Show();
		}

		public virtual void MoveToNext()
		{
			CurrentForm.Hide();
			CurrentFormIndex++;
			CurrentForm.ShowDialog();
		}

		protected virtual void CloseAndDiposeAllForms()
		{
			foreach (var form in _InstantiatedForms)
			{
				form.Close();
				form.Dispose();
			}
			_InstantiatedForms.Clear();
		}

		public virtual void OnFormClosed()
		{
			CloseAndDiposeAllForms();
		}

		public virtual void OnFinish()
		{
			CloseAndDiposeAllForms();
		}

		public virtual void OnCancel()
		{
			CloseAndDiposeAllForms();
		}
	}
}
