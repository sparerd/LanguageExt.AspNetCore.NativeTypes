using Microsoft.AspNetCore.Mvc;

namespace LanguageExt.AspNetCore.NativeTypes.Tests.Controllers;

[Route($"{TestPrelude.CONTROLLER_SOURCE}/[controller]/[action]")]
[ApiController]
public class OptionOutputController
{
	[HttpGet]
	public Option<int> Some() => 7;

	[HttpGet]
	public Option<int> OptionTNone() => Option<int>.None;

	[HttpGet]
	public Option<int> OptionNone() => Prelude.None;
}