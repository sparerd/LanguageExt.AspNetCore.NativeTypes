namespace LanguageExt.AspNetCore.NativeTypes.Tests.Models;

public record RecordWithOptions(Option<int> Num1, Option<int> Num2);

public class ClassWithOptions
{
	public int? Num1 { get; set; }
	public int? Num2 { get; set; }
}