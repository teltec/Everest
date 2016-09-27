/* 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

namespace Teltec.Common
{
	public class ObservableWrapper<T> : ObservableObject
	{
		private T _Value;
		public T Value
		{
			get { return _Value; }
			set { SetField(ref _Value, value); }
		}

		public ObservableWrapper(T initial)
		{
			_Value = initial;
		}

		public static implicit operator ObservableWrapper<T>(T v)
		{
			return new ObservableWrapper<T>(v);
		}
	}
}
