using Cslox.Interpret.Function;
using Cslox.Scan;

namespace Cslox.Interpret.Class
{
	public class LoxInstance
	{
		private LoxClass klass;
		private readonly Dictionary<string, object> fields = new Dictionary<string, object>();

		public LoxInstance(LoxClass klass)
		{
			this.klass = klass;
		}

		public object Get(Token name)
		{
			if (fields.ContainsKey(name.lexeme))
			{
				return fields[name.lexeme];
			}
			LoxFunction? method = klass.FindMethod(name.lexeme);
			if (method != null) return method.Bind(this);
			throw new RuntimeError(name, $"Undefined property {name.lexeme} on {this}.");
		}

		public void Set(Token name, object value)
		{
			fields[name.lexeme] = value;
		}

        public override string ToString()
        {
			return $"{klass} instance";
        }
    }
}

