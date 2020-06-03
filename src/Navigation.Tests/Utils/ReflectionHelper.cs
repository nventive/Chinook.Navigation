using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace System.Reflection
{
	internal static class ReflectionHelper
	{
		/// <summary>
		/// Gets a enumerable of extension methods for the desired type.
		/// </summary>
		/// <param name="assemblies"></param>
		/// <param name="extendedType">The type extended by the extension methods.</param>
		internal static IEnumerable<MethodInfo> GetExtensionMethods(this Assembly[] assemblies, Type extendedType)
		{
			return assemblies
				.SelectMany(a => a
					.GetTypes()
					.Where(t => t.IsSealed && !t.IsGenericType && !t.IsNested)
					.SelectMany(t => t.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
					.Where(m => m.IsDefined(typeof(ExtensionAttribute), inherit: false) && m.GetParameters()[0].ParameterType == extendedType)
				)
				.ToArray();
		}

		/// <summary>
		/// Gets whether 2 <see cref="MethodInfo"/> have the same extension method signature.
		/// </summary>
		/// <param name="referenceMethod"></param>
		/// <param name="method"></param>
		/// <returns>True if both signature match. False otherwise.</returns>
		internal static bool MatchExtensionSignature(this MethodInfo referenceMethod, MethodInfo method)
		{
			if (referenceMethod.Name != method.Name)
			{
				return false;
			}

			if (referenceMethod.ReturnType.ToString() != method.ReturnType.ToString())
			{
				return false;
			}

			// Skip 1 because this is the extended type.
			var referenceParameters = referenceMethod.GetParameters().Skip(1);
			var parameters = method.GetParameters().Skip(1);

			if (referenceParameters.Count() != parameters.Count())
			{
				return false;
			}

			return referenceParameters
				.Zip(parameters, (p1, p2) => p1.Name == p2.Name
					&& p1.ParameterType.ToString() == p2.ParameterType.ToString()
					&& p1.IsOut == p2.IsOut
					&& p1.IsIn == p1.IsIn
					&& p1.HasDefaultValue == p1.HasDefaultValue)
				.All(x => x);
		}

		/// <summary>
		/// Checks that all extension methods on <paramref name="referenceType"/> also exist on <paramref name="extendedType"/>.
		/// </summary>
		/// <param name="assemblies">The assemblies in which the extension methods can be found.</param>
		/// <param name="referenceType"></param>
		/// <param name="extendedType"></param>
		/// <param name="exceptions">Extension method names from <paramref name="referenceType"/> to ignore.</param>
		internal static void MatchExtensions(Assembly[] assemblies, Type referenceType, Type extendedType, string[] exceptions = null)
		{
			exceptions = exceptions ?? new string[0];

			var extensionsOnReferenceType = assemblies.GetExtensionMethods(referenceType);
			var extensionsOnExtendedType = assemblies.GetExtensionMethods(extendedType);

			foreach (var extension in extensionsOnReferenceType)
			{
				if (!exceptions.Contains(extension.Name))
				{
					var match = extensionsOnExtendedType.Any(m => extension.MatchExtensionSignature(m));
					if (!match)
					{
						throw new MissingMethodException($"No extension method found on {extendedType.Name} matching {extension}.");
					}
				}
			}
		}
	}
}
