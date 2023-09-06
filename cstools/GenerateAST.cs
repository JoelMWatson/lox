namespace CsTools;

// Generates AST
class GenerateAST
{
    static void Main(string[] args)
    {
        if (args.Length != 1)
        {
            Console.Error.WriteLine("Usage: generate_ast <output director>");
            Environment.Exit(64);
        }
        String outputDir = args[0];
        DefineAST(outputDir, "Expr", new List<string>()
        {
            "Assign       : Token name, Expr value",
            "Binary       : Expr left, Token op, Expr right",
            "Call         : Expr callee, Token paren, List<Expr> arguments",
            "Conditional  : Token op, Expr cond, Expr left, Expr right",
            "Get          : Expr obj, Token name",
            "Function     : List<Token> parameters, List<Stmt> body",
            "Grouping     : Expr expression",
            "Literal      : Object value",
            "Logical      : Expr left, Token op, Expr right",
            "Set          : Expr obj, Token name, Expr value",
            "Super        : Token keyword, Token method",
            "This         : Token keyword",
            "Unary        : Token op, Expr right",
            "Variable     : Token name",
        }) ;

        DefineAST(outputDir, "Stmt", new List<string>()
        {
            "Block      : List<Stmt> statements",
            "Break      :  ",
            "Class      : Token name, Expr.Variable superclass, List<Stmt.Function> methods",
            "Expression : Expr expression",
            "Function   : Token name, Expr.Function function",
            "If         : Expr condition, Stmt thenBranch, Stmt elseBranch",
            "Print      : Expr expression",
            "Return     : Token keyword, Expr value",
            "Var        : Token name, Expr initializer",
            "While      : Expr condition, Stmt body"
        });
    }

    private static void DefineAST(String outputDir, String name, List<string> types)
    {
        String path = $"{outputDir}/{name}.cs";
        using (StreamWriter writer = new StreamWriter(path))
        {
            writer.WriteLine("using Cslox.Scan;");
            writer.WriteLine();
            writer.WriteLine("namespace Cslox;");
            writer.WriteLine($"public abstract class {name}");
            writer.WriteLine("{");
            writer.WriteLine();

            DefineVisitor(writer, name, types);
            writer.WriteLine();

            foreach (string type in types)
            {
                string className = type.Split(":")[0].Trim();
                string fields = type.Split(":")[1].Trim();
                DefineType(writer, name, className, fields);
            }

            // bace Accept() method
            writer.WriteLine("    abstract public T Accept<T>(Visitor<T> visitor);");
            writer.WriteLine();

            writer.WriteLine("}");
        }
    }

    private static void DefineVisitor(StreamWriter writer, string name, List<string> types)
    {
        writer.WriteLine("    public interface Visitor<T>");
        writer.WriteLine("    {");

        foreach (string type in types)
        {
            string typeName = type.Split(" ")[0].Trim();
            writer.WriteLine($"        T Visit{typeName}{name}({typeName} {name.ToLower()});");
        }
        writer.WriteLine("    }");
    }

    private static void DefineType(StreamWriter writer, string baseName, string className, string fieldsList)
    {
        writer.WriteLine($"    public class {className} : {baseName}");
        writer.WriteLine("    {");

        // constructor
        writer.WriteLine($"        public {className}({fieldsList})");
        writer.WriteLine("        {");

        // store params in fields
        string[] fields;
        if (fieldsList.Length == 0)
        {
            fields = new String[0];
        }
        else
        {
            fields = fieldsList.Split(", ");
        }
        foreach (string field in fields)
        {
            string name = field.Split(" ")[1];
            writer.WriteLine($"            this.{name} = {name};");
        }

        writer.WriteLine("        }");

        // visitor pattern
        writer.WriteLine();
        writer.WriteLine("        public override T Accept<T>(Visitor<T> visitor)");
        writer.WriteLine("        {");
        writer.WriteLine($"            return visitor.Visit{className}{baseName}(this);");
        writer.WriteLine("        }");
        
        // fields
        writer.WriteLine();
        foreach (string field in fields)
        {
            writer.WriteLine($"        public readonly {field};");
        }
        writer.WriteLine("    }");
        writer.WriteLine();
    }
}
