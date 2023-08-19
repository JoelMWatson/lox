namespace Cslox.Interpret
{
	public class LoxFunction : LoxCallable
	{
        private readonly Stmt.Function declaration;
        private readonly LoxEnvironment closure;

		public LoxFunction(Stmt.Function declaration, LoxEnvironment closure)
		{
            this.declaration = declaration;
            this.closure = closure;
		}

        public int Arity()
        {
            return declaration.parameters.Count();
        }

        public object Call(Interpreter interpreter, List<object> arguments)
        {
            LoxEnvironment environment = new LoxEnvironment(closure);
            for (int i=0; i < declaration.parameters.Count(); i++)
            {
                environment.Define(declaration.parameters[i].lexeme, arguments[i]);
            }
            try
            {
                interpreter.ExecuteBlock(declaration.body, environment);
            } catch (ReturnException value)
            {
                return value.value;
            }
            return null;
        }

        public override string ToString()
        {
            return $"<Fn {declaration.name.lexeme}>";
        }

    }
}

