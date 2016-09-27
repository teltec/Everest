/* 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Specialized;
using Teltec.Common.Extensions;

namespace Teltec.Everest.Data.Models
{
	public class PlanNotification : BaseEntity<Int32?>
	{
		private Int32? _Id;
		public virtual Int32? Id
		{
			get { return _Id; }
			set { SetField(ref _Id, value); }
		}

		public enum TriggerCondition
		{
			FAILED = 0,
			ALWAYS = 1,
		};

		private TriggerCondition _WhenToNotify = TriggerCondition.ALWAYS;
		public virtual TriggerCondition WhenToNotify
		{
			get { return _WhenToNotify; }
			set { SetField(ref _WhenToNotify, value); }
		}

		private bool _IsNotificationEnabled = false;
		public virtual bool IsNotificationEnabled
		{
			get { return _IsNotificationEnabled; }
			set { SetField(ref _IsNotificationEnabled, value); }
		}

		public const int EmailAddressMaxLen = 254; // REFERENCE: https://en.wikipedia.org/wiki/Email_address
		private string _EmailAddress;
		public virtual string EmailAddress
		{
			get { return _EmailAddress; }
			set { SetField(ref _EmailAddress, value); }
		}

		public const int FullNameMaxLen = 64;
		private string _FullName;
		public virtual string FullName
		{
			get { return _FullName; }
			set { SetField(ref _FullName, value); }
		}

		public static readonly string VAR_NAME = "name";
		public static readonly string VAR_TYPE = "type";
		public static readonly string VAR_STATUS = "status";
		public static readonly string DEFAULT_SUBJECT = "[{{name}}] {{type}} {{status}}";

		public const int SubjectMaxLen = 128;
		private string _Subject = DEFAULT_SUBJECT;
		public virtual string Subject
		{
			get { return _Subject; }
			set { SetField(ref _Subject, value); }
		}

		public virtual string GetFormattedSubject(string planName, string planType, string status)
		{
			StringDictionary vars = new StringDictionary();
			vars.Add(Models.PlanNotification.VAR_NAME, planName);
			vars.Add(Models.PlanNotification.VAR_TYPE, planType);
			vars.Add(Models.PlanNotification.VAR_STATUS, status);
			return this.Subject.ExpandVariables(vars);
		}
	}
}
