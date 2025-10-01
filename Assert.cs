using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;

namespace TestSurface
{
	public static class Assert
	{
		/// <summary>
		/// Compares the two objects by value. 
		/// Selects all public fields and properties if no BindingFlags are provided.
		/// </summary>
		/// <returns>True if the objects are the same, compared according to the BindingFlags selection.</returns>
		/// <exception cref="ArgumentException">When the objects types mismatch.</exception>
		public static bool SameValues(object a, object b, BindingFlags? bf = null, HashSet<object> visited = null)
		{
			if (!bf.HasValue) bf = BindingFlags.Public | BindingFlags.Instance;
			if (visited == null) visited = new HashSet<object>();

			if (a == null) return b == null;
			if (b == null) return false;

			var t = a.GetType();
			if (t != b.GetType()) throw new ArgumentException("Type mismatch");

			if ((t.IsPrimitive || t.IsValueType)) return a.Equals(b);
			if (t == typeof(string)) return a.Equals(b);
			if (typeof(IComparable).IsAssignableFrom(t)) return ((IComparable)a).CompareTo(b) == 0;

			// Handle collections directly as sequences (except strings which we handled above)
			if (typeof(IEnumerable).IsAssignableFrom(t) && t != typeof(string))
			{
				return sameSeq(a, b, bf, visited);
			}

			var av = visited.Contains(a);
			var bv = visited.Contains(b);

			if (av != bv) return false;
			else if (av == true) return true;
			else
			{
				visited.Add(a);
				visited.Add(b);
			}

			if (!fields.ContainsKey(t)) fields.TryAdd(t, t.GetFields(bf.Value));
			if (!props.ContainsKey(t)) props.TryAdd(t, t.GetProperties(bf.Value));

			var F = fields[t];
			var P = props[t];

			foreach (var f in F)
			{
				var ao = f.GetValue(a);
				var bo = f.GetValue(b);

				if ((typeof(IEnumerable).IsAssignableFrom(f.FieldType)))
				{
					if (!sameSeq(ao, bo, bf, visited)) return false;
				}
				else if (!SameValues(ao, bo, bf, visited)) return false;
			}

			foreach (var p in P)
			{
				// Indexer
				if (p.GetIndexParameters().Length > 0) continue;

				var ao = p.GetValue(a);
				var bo = p.GetValue(b);

				if ((typeof(IEnumerable).IsAssignableFrom(p.PropertyType)))
				{
					if (!sameSeq(ao, bo, bf, visited)) return false;
				}
				else if (!SameValues(ao, bo, bf, visited)) return false;
			}

			return true;
		}

		/// <summary>
		/// Erases the cached type meta-data,
		/// </summary>
		public static void ClearTypeCache()
		{
			fields.Clear();
			props.Clear();
		}

		static bool sameSeq(object ao, object bo, BindingFlags? bf, HashSet<object> visited)
		{
			if (ao == null) return bo == null;
			if (bo == null) return false;

			var ae = ((IEnumerable)ao).GetEnumerator();
			var be = ((IEnumerable)bo).GetEnumerator();

			try
			{
				var ar = ae.MoveNext();
				var br = be.MoveNext();

				while (ar && br)
				{
					if (!SameValues(ae.Current, be.Current, bf, visited)) return false;
					ar = ae.MoveNext();
					br = be.MoveNext();
				}

				return (!ar && !br);
			}
			finally
			{
				(ae as IDisposable)?.Dispose();
				(be as IDisposable)?.Dispose();
			}
		}

		static readonly ConcurrentDictionary<Type, FieldInfo[]> fields = new ConcurrentDictionary<Type, FieldInfo[]>();
		static readonly ConcurrentDictionary<Type, PropertyInfo[]> props = new ConcurrentDictionary<Type, PropertyInfo[]>();
	}
}
