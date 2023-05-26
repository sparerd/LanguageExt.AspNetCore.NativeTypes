using System.Reflection;

namespace LanguageExt.AspNetCore.NativeTypes;

public static class PreludeReflection
{
	/// <summary>
	/// Creates an untyped collection of elements from the given untyped elements collection.
	/// </summary>
	/// <param name="elements">
	/// Underlying type will be <see cref="IEnumerable{T}"/>
	/// </param>
	/// <returns></returns>
	public delegate object NewUntypedCollection(object elements);

	/// <summary>
	/// Converts the given value to an <see cref="Option{A}"/>
	/// with the provided <see cref="innerType"/>.
	/// </summary>
	/// <param name="innerType">
	/// The inner type of the <see cref="Option{A}"/>.
	/// </param>
	/// <param name="value">
	/// The optional value.
	/// </param>
	/// <returns>
	/// An untyped Some or None
	/// </returns>
	public static object Optional(Type innerType, object? value) =>
		typeof(Prelude)
			.GetMethods(BindingFlags.Public | BindingFlags.Static)
			.First(method => method.Name == nameof(Prelude.Optional))
			.MakeGenericMethod(innerType)
			.Invoke(null, new[] { value })!;

	/// <summary>
	/// Returns a <see cref="Option{A}"/> in the None state.
	/// </summary>
	/// <param name="innerType">
	/// The inner type of the <see cref="Option{A}"/>.
	/// </param>
	/// <returns></returns>
	public static object NoneUntyped(Type innerType) =>
		typeof(Option<>)
			.MakeGenericType(innerType)
			.GetField(nameof(Option<object>.None), BindingFlags.Public | BindingFlags.Static)!
			.GetValue(null)!;

	/// <summary>
	/// 
	/// </summary>
	/// <param name="elementType"></param>
	/// <returns></returns>
	public static NewUntypedCollection SeqNew(Type elementType) => 
		elements => typeof(Seq<>)
			.MakeGenericType(elementType)
			.GetConstructor(new[] { typeof(IEnumerable<>).MakeGenericType(elementType) })!
			.Invoke(new[] {elements});

	/// <summary>
	/// 
	/// </summary>
	/// <param name="elementType"></param>
	/// <returns></returns>
	public static NewUntypedCollection LstNew(Type elementType) =>
		elements => typeof(Lst<>)
			.MakeGenericType(elementType)
			.GetConstructor(new[] { typeof(IEnumerable<>).MakeGenericType(elementType) })!
			.Invoke(new[] { elements });
}
