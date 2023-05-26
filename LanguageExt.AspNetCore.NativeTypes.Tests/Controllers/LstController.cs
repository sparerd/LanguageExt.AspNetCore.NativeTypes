using LanguageExt.AspNetCore.NativeTypes.Tests.Models;
using Microsoft.AspNetCore.Mvc;
using static LanguageExt.AspNetCore.NativeTypes.Tests.TestPrelude;

namespace LanguageExt.AspNetCore.NativeTypes.Tests.Controllers;

[Route($"{CONTROLLER_SOURCE}/[controller]")]
[ApiController]
public class LstController
{
	private readonly LanguageExtAspNetCoreOptions _options;

	public LstController(LanguageExtAspNetCoreOptions options)
	{
		_options = options;
	}

	[HttpPost("body")]
	public IActionResult FromBody([FromBody] Lst<int> num) => JsonResult(num.Sum());

	[HttpPost("body/complex")]
	public IActionResult FromBodyComplex([FromBody] RecordWithLsts value) => JsonResult(
		new RecordWithLsts(
			First: value.First.Map(i => Increment(i)),
			Second: value.Second.Map(i => Increment(i))));

	[HttpGet("query")]
	public IActionResult FromQuery([FromQuery] Lst<int> num) => JsonResult(num.Sum());

	[HttpGet("route/{num?}")]
	public IActionResult FromRoute([FromRoute] Lst<int> num) => JsonResult(num.Sum());

	[HttpGet("header")]
	public IActionResult FromHeader([FromHeader(Name = "X-OPT-NUM")] Lst<int> num) => JsonResult(num.Sum());

	[HttpPost("form")]
	public IActionResult FromForm([FromForm] Lst<int> num) => JsonResult(num.Sum());

	private IActionResult JsonResult(object value) => TestPrelude.JsonResult(_options, value);
}