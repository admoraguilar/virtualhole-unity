using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Holoverse.Data
{
	public abstract class Filter<T>
	{
		public abstract bool IsValid(T item);
	}
}
