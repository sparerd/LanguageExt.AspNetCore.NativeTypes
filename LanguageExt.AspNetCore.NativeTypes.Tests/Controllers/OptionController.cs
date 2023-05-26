using LanguageExt.AspNetCore.NativeTypes.Tests.Models;
using Microsoft.AspNetCore.Mvc;
using static LanguageExt.AspNetCore.NativeTypes.Tests.TestPrelude;

namespace LanguageExt.AspNetCore.NativeTypes.Tests.Controllers;

[Route($"{CONTROLLER_SOURCE}/[controller]")]
[ApiController]
public class OptionController
{
	private readonly LanguageExtAspNetCoreOptions _options;

	public OptionController(LanguageExtAspNetCoreOptions options)
	{
		_options = options;
	}

	[HttpPost("body")]
	public IActionResult FromBody([FromBody] Option<int> num) => JsonResult(Increment(num));

	[HttpPost("body/complex")]
	public IActionResult FromBodyComplex([FromBody] RecordWithOptions value) => JsonResult(
		new RecordWithOptions(
			Num1: Increment(value.Num1), 
			Num2: Increment(value.Num2)));

	[HttpGet("query")]
	public IActionResult FromQuery([FromQuery] Option<int> num) => JsonResult(Increment(num));

	[HttpGet("route/{num?}")]
	public IActionResult FromRoute([FromRoute] Option<int> num) => JsonResult(Increment(num));

	[HttpGet("header")]
	public IActionResult FromHeader([FromHeader(Name = "X-OPT-NUM")] Option<int> num) => JsonResult(Increment(num));

	[HttpPost("form")]
	public IActionResult FromForm([FromForm] Option<int> num) => JsonResult(Increment(num));
		
	private IActionResult JsonResult(object value) => TestPrelude.JsonResult(_options, value);
}