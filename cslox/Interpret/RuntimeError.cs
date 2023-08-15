using Cslox.Scan;

namespace Cslox.Interpret
{
	public class RuntimeError : Exception
	{
		public Token token;

		public RuntimeError(Token token, string message) : base(message)
		{
			this.token = token;
		}
	}
}

