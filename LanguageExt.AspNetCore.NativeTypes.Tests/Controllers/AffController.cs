using LanguageExt.AspNetCore.NativeTypes.Tests.Models;
using Microsoft.AspNetCore.Mvc;

namespace LanguageExt.AspNetCore.NativeTypes.Tests.Controllers;

[Route($"{TestPrelude.CONTROLLER_SOURCE}/[controller]/[action]")]
[ApiController]
public class AffController
{
	[HttpGet]
	public Aff<JsonResult> ReturnsActionResultClass() => Prelude.SuccessEff(new JsonResult(0));

	[HttpGet]
	public Aff<IActionResult> ReturnsActionResultInterface() => Prelude.SuccessEff<IActionResult>(new JsonResult(0));

	[HttpGet]
	public Aff<int> ReturnsSimpleType() => Prelude.SuccessEff(0);

	[HttpGet]
	public Aff<RecordWithOptions> ReturnsComplexType() => Prelude.SuccessEff(new RecordWithOptions(5, Prelude.None));

	[HttpGet]
	public Aff<int> ReturnsDefaultEffect() => default;
}