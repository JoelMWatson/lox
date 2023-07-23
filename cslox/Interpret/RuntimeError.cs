using cslox.Scan;

namespace cslox.Interpret
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

