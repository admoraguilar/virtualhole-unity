using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Holoverse.Data
{
	public abstract class Filter<T>
	{
		public bool isOpposite = false;

		public bool IsValid(T item)
		{
			return IsValidImpl(item) && !isOpposite;
		}

		protected abstract bool IsValidImpl(T item);
	}
}
