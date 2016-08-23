using System;
using System.Collections.Generic;
using Teltec.Common.Extensions;

namespace Teltec.Everest.Data.Models
{
	public class RestorePlan : SchedulablePlan<RestorePlan>
	{
		public override string GetConcretePlanTypeName()
		{
			return "Restore";
		}

		public override Type GetVirtualType()
		{
			return this.GetType();
		}

		#region Sources

		private IList<RestorePlanSourceEntry> _SelectedSources = new List<RestorePlanSourceEntry>();
		public virtual IList<RestorePlanSourceEntry> SelectedSources
		{
			get { return _SelectedSources; }
			protected set { SetField(ref _SelectedSources, value); InvalidateCachedSelectedSourcesAsDelimitedString(); }
		}

		private string _CachedSelectedSourcesAsDelimitedString;
		public virtual string SelectedSourcesAsDelimitedString(string delimiter, int maxLength, string trail)
		{
			if (_CachedSelectedSourcesAsDelimitedString == null)
				_CachedSelectedSourcesAsDelimitedString = SelectedSources.AsDelimitedString(p => p.Path,
					"No selected sources", delimiter, maxLength, trail);
			return _CachedSelectedSourcesAsDelimitedString;
		}

		private void InvalidateCachedSelectedSourcesAsDelimitedString()
		{
			_CachedSelectedSourcesAsDelimitedString = null;
		}

		#endregion

		#region Files

		private IList<RestorePlanFile> _Files = new List<RestorePlanFile>();
		public virtual IList<RestorePlanFile> Files
		{
			get { return _Files; }
			protected set { SetField(ref _Files, value); }
		}

		#endregion

		#region Restores

		private IList<Restore> _Restores = new List<Restore>();
		public virtual IList<Restore> Restores
		{
			get { return _Restores; }
			protected set { SetField(ref _Restores, value); }
		}

		#endregion
	}
}
