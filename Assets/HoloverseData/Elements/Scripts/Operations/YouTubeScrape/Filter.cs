
namespace Holoverse.Data
{
	public abstract class Filter<T>
	{
		public bool isOpposite = false;

		public bool IsValid(T item) => IsValidImpl(item) && !isOpposite;

		protected abstract bool IsValidImpl(T item);
	}
}
