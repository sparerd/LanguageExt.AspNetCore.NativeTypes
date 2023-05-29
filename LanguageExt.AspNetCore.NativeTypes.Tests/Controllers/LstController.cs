using LanguageExt.AspNetCore.NativeTypes.Tests.Models;
using Microsoft.AspNetCore.Mvc;
using static LanguageExt.AspNetCore.NativeTypes.Tests.TestPrelude;

namespace LanguageExt.AspNetCore.NativeTypes.Tests.Controllers;

[Route($"{CONTROLLER_SOURCE}/[controller]")]
[ApiController]
public class LstController : ControllerBase
{
	[HttpPost("body")]
	public IActionResult FromBody([FromBody] Lst<int> num) => Ok(num.Sum());

	[HttpPost("body/complex")]
	public IActionResult FromBodyComplex([FromBody] RecordWithLsts value) => Ok(
		new RecordWithLsts(
			First: value.First.Map(i => Increment(i)),
			Second: value.Second.Map(i => Increment(i))));

	[HttpGet("query")]
	public IActionResult FromQuery([FromQuery] Lst<int> num) => Ok(num.Sum());

	[HttpGet("route/{num?}")]
	public IActionResult FromRoute([FromRoute] Lst<int> num) => Ok(num.Sum());

	[HttpGet("header")]
	public IActionResult FromHeader([FromHeader(Name = "X-OPT-NUM")] Lst<int> num) => Ok(num.Sum());

	[HttpPost("form")]
	public IActionResult FromForm([FromForm] Lst<int> num) => Ok(num.Sum());
}