using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;

namespace Teltec.Common.Controls
{
	public class RadioButtonGroup : Component
	{
		private HashSet<RadioButton> _group = new HashSet<RadioButton>();

		public RadioButtonGroup()
		{
		}

		public RadioButtonGroup(IContainer container) : this()
		{
		}

		public void AddRadioButton(RadioButton button)
		{
			button.CheckedChanged += radioButton_CheckedChanged;
			_group.Add(button);
		}

		public void RemoveRadioButton(RadioButton button)
		{
			button.CheckedChanged -= radioButton_CheckedChanged;
			_group.Remove(button);
		}

		protected internal void radioButton_CheckedChanged(object sender, EventArgs e)
		{
			RadioButton rb = (RadioButton)sender;
			if (!rb.Checked)
				return;

			foreach (RadioButton radio in _group)
			{
				if (radio == rb)
					continue;
				radio.Checked = false;
			}
		}
/*
		private Dictionary<int, HashSet<RadioButton>> _groups = new Dictionary<int, HashSet<RadioButton>>();

		private void AddRadioButtonToGroup(int groupId, RadioButton newRadio, object tag)
		{
			HashSet<RadioButton> group;
			bool contains = _groups.TryGetValue(groupId, out group);
			if (!contains)
			{
				// Add new radio group.
				group = new HashSet<RadioButton>();
				_groups.Add(groupId, group);
			}
			// Add new radio button.
			newRadio.Tag = tag;
			newRadio.CheckedChanged += radioButton_CheckedChanged;
			group.Add(newRadio);
			//Console.WriteLine("group {0} has {1} radios", groupId, group.Count);
		}

		private void radioButton_CheckedChanged(object sender, EventArgs e)
		{
			RadioButton rb = (RadioButton)sender;
			if (!rb.Checked)
				return;

			foreach (var group in _groups.Values)
			{
				//Console.WriteLine("group ... has {1} radios", group.Count);
				bool contains = group.Contains(rb);
				if (!contains)
					continue;

				foreach (RadioButton radio in group)
				{
					if (radio == rb)
						continue;
					radio.Checked = false;
				}
			}
		}
*/
	}
}
