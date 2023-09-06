namespace Cslox.Interpret.Function
{
	public class ReturnException : Exception
	{
		public readonly object value;

		public ReturnException(object value) : base(null, null)
        {
			this.value = value;
		}
	}
}

