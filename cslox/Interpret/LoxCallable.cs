namespace Cslox.Interpret
{
	public interface LoxCallable
	{
		int Arity();
		object Call(Interpreter interpreter, List<object> arguments);
	}
}

