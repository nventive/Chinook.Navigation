using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System.Collections.Generic
{
	internal static class ReadOnlyListExtensions
	{
		internal static IReadOnlyList<T> ImmutableRemoveAt<T>(this IReadOnlyList<T> readOnlyList, int index)
		{
			var list = readOnlyList.ToList();
			list.RemoveAt(index);
			return list;
		}

		internal static IReadOnlyList<T> ImmutableRemove<T>(this IReadOnlyList<T> readOnlyList, T itemToRemove)
		{
			var list = readOnlyList.ToList();
			list.Remove(itemToRemove);
			return list;
		}

		internal static IReadOnlyList<T> ImmutableAdd<T>(this IReadOnlyList<T> readOnlyList, T itemToAdd)
		{
			var list = readOnlyList.ToList();
			list.Add(itemToAdd);
			return list;
		}
	}
}
