using LanguageExt.AspNetCore.NativeTypes.Tests.Models;
using Microsoft.AspNetCore.Mvc;
using static LanguageExt.AspNetCore.NativeTypes.Tests.TestPrelude;
using static LanguageExt.Prelude;

namespace LanguageExt.AspNetCore.NativeTypes.Tests.Controllers;

[Route($"{CONTROLLER_SOURCE}/[controller]/[action]")]
[ApiController]
public class EffController
{
	[HttpGet]
	public Eff<JsonResult> ReturnsActionResultClass() => SuccessEff(new JsonResult(0));

	[HttpGet]
	public Eff<IActionResult> ReturnsActionResultInterface() => SuccessEff<IActionResult>(new JsonResult(0));

	[HttpGet]
	public Eff<int> ReturnsSimpleType() => SuccessEff(0);

	[HttpGet]
	public Eff<RecordWithOptions> ReturnsComplexType() => SuccessEff(new RecordWithOptions(5, None));

	[HttpGet]
	public Eff<TestRuntime, RecordWithOptions> NeedsSpecificRuntime() => 
		Eff<TestRuntime, RecordWithOptions>(rt => new RecordWithOptions(rt.Val1, None));

	[HttpGet]
	public Eff<RT, RecordWithOptions> NeedsRuntimeWithTrait<RT>()
		where RT : struct, HasSubsystem<RT> =>
		from sub in Subsystem<RT>.CallSubFunction()
		select new RecordWithOptions(sub, None);

	[HttpGet]
	public Eff<int> ReturnsDefaultEffect() => default;
}