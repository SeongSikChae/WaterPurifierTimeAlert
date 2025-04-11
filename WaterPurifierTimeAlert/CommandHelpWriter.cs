using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace WaterPurifierTimeAlert
{
	public sealed class CommandHelpWriter : TextWriter
	{
		public override Encoding Encoding => Encoding.UTF8;

		public override void WriteLine(string? value)
		{
			base.WriteLine(value);
		}

		public override Task WriteLineAsync(string? value)
		{
			return base.WriteLineAsync(value);
		}

		public override void WriteLine(StringBuilder? value)
		{
			base.WriteLine(value);
		}

		public override void WriteLine([StringSyntax("CompositeFormat")] string format, params object?[] arg)
		{
			base.WriteLine(format, arg);
		}

		public override Task WriteLineAsync(StringBuilder? value, CancellationToken cancellationToken = default)
		{
			return base.WriteLineAsync(value, cancellationToken);
		}
	}
}
