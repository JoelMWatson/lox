using Cslox.Interpret.Class;

namespace Cslox.Interpret.Function
{
	public class LoxFunction : LoxCallable
	{
        private readonly string name;
        private readonly Expr.Function function;
        private readonly LoxEnvironment closure;
        private readonly bool isInitializer;

		public LoxFunction(string name, Expr.Function function, LoxEnvironment closure, bool isInitializer)
		{
            this.name = name;
            this.function = function;
            this.closure = closure;
            this.isInitializer = isInitializer;
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
                if (isInitializer) return closure.GetAt(0, "this");
                return value.value;
            }

            return null;
        }

        public LoxFunction Bind(LoxInstance instance)
        {
            LoxEnvironment environment = new LoxEnvironment(closure);
            environment.Define("this", instance);
            return new LoxFunction(name, function, environment, isInitializer);
        }

        public override string ToString()
        {
            if (name == null) return "<Fn>";
            return $"<Fn {name}>";
        }

    }
}

