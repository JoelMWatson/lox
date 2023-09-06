namespace Cslox.Interpret.Function
{
	public interface LoxCallable
	{
		int Arity();
		object Call(Interpreter interpreter, List<object> arguments);
	}
}

