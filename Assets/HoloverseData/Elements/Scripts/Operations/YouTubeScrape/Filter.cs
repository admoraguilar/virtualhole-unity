
namespace Holoverse.Data
{
	public abstract class Filter<T>
	{
		public bool isOpposite = false;

		public bool IsValid(T item) => isOpposite ? !IsValidImpl(item) : IsValidImpl(item);

		protected abstract bool IsValidImpl(T item);
	}
}
