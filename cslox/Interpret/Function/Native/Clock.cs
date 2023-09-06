namespace Cslox.Interpret.Function.Native
{
	public class Clock : LoxCallable
	{
		public Clock()
		{
		}

        public int Arity()
        {
            return 0;
        }

        public object Call(Interpreter interpreter, List<object> arguments)
        {
            return DateTimeOffset.Now.ToUnixTimeMilliseconds() / 1000.0;
        }

        public override string ToString()
        {
            return "<Native Fn>";
        }
    }
}

