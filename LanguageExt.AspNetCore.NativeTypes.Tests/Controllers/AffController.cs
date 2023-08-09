using LanguageExt.AspNetCore.NativeTypes.Tests.Models;
using LanguageExt.Effects.Traits;
using Microsoft.AspNetCore.Mvc;
using static LanguageExt.Prelude;

namespace LanguageExt.AspNetCore.NativeTypes.Tests.Controllers;

[Route($"{TestPrelude.CONTROLLER_SOURCE}/[controller]/[action]")]
[ApiController]
public class AffController
{
	[HttpGet]
	public Aff<JsonResult> ReturnsActionResultClass() => SuccessEff(new JsonResult(0));

	[HttpGet]
	public Aff<IActionResult> ReturnsActionResultInterface() => SuccessEff<IActionResult>(new JsonResult(0));

	[HttpGet]
	public Aff<int> ReturnsSimpleType() => SuccessEff(0);

	[HttpGet]
	public Aff<RecordWithOptions> ReturnsComplexType() => SuccessEff(new RecordWithOptions(5, None));

	[HttpGet]
	public Aff<TestRuntime, RecordWithOptions> NeedsSpecificRuntime() => 
		Aff<TestRuntime, RecordWithOptions>(rt => new RecordWithOptions(rt.Val1, None).AsValueTask());

	[HttpGet]
	public Aff<RT, RecordWithOptions> NeedsRuntimeWithTrait<RT>()
		where RT : struct, HasSubsystem<RT>, HasCancel<RT> =>
		from sub in Subsystem<RT>.CallSubFunction()
		select new RecordWithOptions(sub, None);

	[HttpGet]
	public Aff<int> ReturnsDefaultEffect() => default;
}