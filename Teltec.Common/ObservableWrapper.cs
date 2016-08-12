
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
