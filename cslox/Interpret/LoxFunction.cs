namespace Cslox.Interpret
{
	public class LoxFunction : LoxCallable
	{
        private readonly string name;
        private readonly Expr.Function function;
        private readonly LoxEnvironment closure;

		public LoxFunction(string name, Expr.Function function, LoxEnvironment closure)
		{
            this.name = name;
            this.function = function;
            this.closure = closure;
		}

        public int Arity()
        {
            return function.parameters.Count();
        }

        public object Call(Interpreter interpreter, List<object> arguments)
        {
            LoxEnvironment environment = new LoxEnvironment(closure);
            for (int i=0; i < function.parameters.Count(); i++)
            {
                environment.Define(function.parameters[i].lexeme, arguments[i]);
            }
            try
            {
                interpreter.ExecuteBlock(function.body, environment);
            } catch (ReturnException value)
            {
                return value.value;
            }
            return null;
        }

        public override string ToString()
        {
            if (name == null) return "<Fn>";
            return $"<Fn {name}>";
        }

    }
}

