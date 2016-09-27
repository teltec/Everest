/* 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;

namespace Teltec.Everest.Data.Models
{
	public enum BackupPlanPurgeTypeEnum
	{
		DEFAULT = 0,
		CUSTOM = 1,
	}

	public class BackupPlanPurgeOptions : BaseEntity<Int32?>
	{
		private Int32? _Id;
		public virtual Int32? Id
		{
			get { return _Id; }
			set { SetField(ref _Id, value); }
		}

		private BackupPlanPurgeTypeEnum _PurgeType = BackupPlanPurgeTypeEnum.DEFAULT;
		public virtual BackupPlanPurgeTypeEnum PurgeType
		{
			get { return _PurgeType; }
			set { SetField(ref _PurgeType, value); }
		}

		#region PurgeType == BackupPlanPurgeTypeEnum.CUSTOM

		private bool _EnabledKeepNumberOfVersions = false;
		public virtual bool EnabledKeepNumberOfVersions
		{
			get { return _EnabledKeepNumberOfVersions; }
			set { SetField(ref _EnabledKeepNumberOfVersions, value); }
		}

		private int _NumberOfVersionsToKeep = 1;
		public virtual int NumberOfVersionsToKeep
		{
			get { return _NumberOfVersionsToKeep; }
			set { SetField(ref _NumberOfVersionsToKeep, value); }
		}

		#endregion

		#region Auxiliary methods

		public virtual bool IsTypeDefault
		{
			get { return PurgeType == Models.BackupPlanPurgeTypeEnum.DEFAULT; }
		}

		public virtual bool IsTypeCustom
		{
			get { return PurgeType == Models.BackupPlanPurgeTypeEnum.CUSTOM; }
		}

		#endregion
	}
}
