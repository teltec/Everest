/* 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

namespace Teltec.Everest.PlanExecutor.Actions
{
	public abstract class TriggerableAction
	{
		public void BeforeExecute()
		{
		}

		public abstract void Execute();

		public void AfterExecute()
		{
		}
	}
}
