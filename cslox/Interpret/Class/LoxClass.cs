using Cslox.Interpret.Function;

namespace Cslox.Interpret.Class
{
	public class LoxClass : LoxCallable
	{
		private readonly string name;
        private readonly LoxClass superclass;
        private readonly Dictionary<string, LoxFunction> methods;

        public LoxClass(string name, LoxClass superclass,  Dictionary<string, LoxFunction>  methods)
		{
			this.name = name;
            this.superclass = superclass;
            this.methods = methods;
		}

        public int Arity()
        {
            LoxFunction? initilizer = FindMethod("init");
            return initilizer != null ? initilizer.Arity() : 0;
        }

        public object Call(Interpreter interpreter, List<object> arguments)
        {
            LoxInstance instance = new LoxInstance(this);
            LoxFunction? initilizer = FindMethod("init");
            if (initilizer != null)
            {
                initilizer.Bind(instance).Call(interpreter, arguments);
            }
            return instance;
        }

        public LoxFunction? FindMethod(string name)
        {
            if (methods.ContainsKey(name)) return methods[name];
            if (superclass != null) return superclass.FindMethod(name);
            return null;
        }

        public override string ToString()
        {
			return name;
        }
    }
}

