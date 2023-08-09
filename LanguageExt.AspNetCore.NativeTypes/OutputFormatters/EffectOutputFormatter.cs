using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using static LanguageExt.AspNetCore.NativeTypes.OutputFormatters.OutputFormat;
using static LanguageExt.Prelude;

namespace LanguageExt.AspNetCore.NativeTypes.OutputFormatters;

/// <summary>
/// Provides support for returning <see cref="Eff{A}"/> or <see cref="Aff{A}"/> types from
/// controller methods directly.
/// </summary>
public class EffectOutputFormatter : IOutputFormatter
{
	private readonly Func<OutputFormatterWriteContext, Aff<IActionResult>> _runEffect;
	private readonly Func<Type, bool> _typeSupported;

	/// <summary>
	/// 
	/// </summary>
	public EffectOutputFormatter(
		Func<OutputFormatterWriteContext, Aff<IActionResult>> runEffect, 
		Func<Type, bool> typeSupported)
	{
		_runEffect = runEffect ?? throw new ArgumentNullException(nameof(runEffect));
		_typeSupported = typeSupported;
	}

	public bool CanWriteResult(OutputFormatterCanWriteContext context) =>
		Optional(context.ObjectType)
			.Map(_typeSupported)
			.IfNone(false);

	public async Task WriteAsync(OutputFormatterWriteContext context) => 
		await Write(context).RunUnit();

	private Aff<Unit> Write(OutputFormatterWriteContext context) =>
		from result in _runEffect(context)
		from _ in ExecuteResult(result, context.HttpContext)
		select unit;
}