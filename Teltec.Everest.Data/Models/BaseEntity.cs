/* 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using Teltec.Everest.Data.DAO.NH;
using Teltec.Common;

namespace Teltec.Everest.Data.Models
{
	public abstract class BaseEntity<ID> : ObservableObject, IPersistentEntity, IAuditable //where ID : class
	{
		private bool _saved = false;

		public virtual void OnSave()
		{
			_saved = true;
		}

		public virtual void OnLoad()
		{
			_saved = true;
		}

		public virtual bool IsSaved
		{
			get { return _saved; }
		}
	}
}
