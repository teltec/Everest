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
		protected internal List<Type> _RegisteredFormTypes = new List<Type>();
		protected internal List<WizardForm> _InstantiatedForms = new List<WizardForm>();
		protected internal int _CurrentFormIndex = 0;
		protected internal Form _Owner;

		public virtual void RegisterFormClass<T>(T wizardFormType) where T : Type
		{
			bool isCompatible = typeof(WizardForm).IsAssignableFrom(wizardFormType);
			if (!isCompatible)
			{
				var message = String.Format("Type must be compatible with {0}", typeof(WizardForm).FullName);
				throw new ArgumentException(message, "wizardFormType");
			}
			_RegisteredFormTypes.Add(wizardFormType);
		}

		public virtual void ShowDialog(Form owner)
		{
			_CurrentFormIndex = 0; 
			_Owner = owner;
			CurrentForm.ShowDialog(owner);
		}

		public virtual void Close()
		{
			CloseAndDiposeAllForms();
			// TODO: fire event?
		}

		public virtual void Cancel()
		{
			CloseAndDiposeAllForms();
			// TODO: fire event?
		}

		protected virtual WizardForm InstantiateForm(int index, Form owner)
		{
			// Instantiate it.
			Type type = _RegisteredFormTypes.ElementAt(index);
			object instance = Activator.CreateInstance(type);
			WizardForm form = (WizardForm)instance;
			form.Owner = owner;
			form.NextEnabled = index < _RegisteredFormTypes.Count - 1;
			form.PreviousEnabled = index > 0;
			form.IsLastForm = index == _RegisteredFormTypes.Count - 1;
			form.CancelEvent += form_CancelEvent;
			form.FinishEvent += form_FinishEvent;
			form.PreviousEvent += form_PreviousEvent;
			form.NextEvent += form_NextEvent;
			return form;
		}

		protected virtual void form_CancelEvent(WizardForm sender, EventArgs e)
		{
			CloseAndDiposeAllForms();
		}

		protected virtual void form_FinishEvent(WizardForm sender, EventArgs e)
		{
			CloseAndDiposeAllForms();
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
			foreach (WizardForm form in _InstantiatedForms)
			{
				form.Close();
				form.Dispose();
			}
			_InstantiatedForms.Clear();
		}

		public virtual void Dispose()
		{
			CloseAndDiposeAllForms();
			_RegisteredFormTypes.Clear();
		}
	}
}
