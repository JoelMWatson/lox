using Cslox.Scan;

namespace Cslox;
public abstract class Stmt
{

    public interface Visitor<T>
    {
        T visitBlockStmt(Block stmt);
        T visitBreakStmt(Break stmt);
        T visitExpressionStmt(Expression stmt);
        T visitFunctionStmt(Function stmt);
        T visitIfStmt(If stmt);
        T visitPrintStmt(Print stmt);
        T visitReturnStmt(Return stmt);
        T visitVarStmt(Var stmt);
        T visitWhileStmt(While stmt);
    }

    public class Block : Stmt
    {
        public Block(List<Stmt> statements)
        {
            this.statements = statements;
        }

        public override T Accept<T>(Visitor<T> visitor)
        {
            return visitor.visitBlockStmt(this);
        }

        public readonly List<Stmt> statements;
    }

    public class Break : Stmt
    {
        public Break()
        {
        }

        public override T Accept<T>(Visitor<T> visitor)
        {
            return visitor.visitBreakStmt(this);
        }

    }

    public class Expression : Stmt
    {
        public Expression(Expr expression)
        {
            this.expression = expression;
        }

        public override T Accept<T>(Visitor<T> visitor)
        {
            return visitor.visitExpressionStmt(this);
        }

        public readonly Expr expression;
    }

    public class Function : Stmt
    {
        public Function(Token name, Expr.Function function)
        {
            this.name = name;
            this.function = function;
        }

        public override T Accept<T>(Visitor<T> visitor)
        {
            return visitor.visitFunctionStmt(this);
        }

        public readonly Token name;
        public readonly Expr.Function function;
    }

    public class If : Stmt
    {
        public If(Expr condition, Stmt thenBranch, Stmt elseBranch)
        {
            this.condition = condition;
            this.thenBranch = thenBranch;
            this.elseBranch = elseBranch;
        }

        public override T Accept<T>(Visitor<T> visitor)
        {
            return visitor.visitIfStmt(this);
        }

        public readonly Expr condition;
        public readonly Stmt thenBranch;
        public readonly Stmt elseBranch;
    }

    public class Print : Stmt
    {
        public Print(Expr expression)
        {
            this.expression = expression;
        }

        public override T Accept<T>(Visitor<T> visitor)
        {
            return visitor.visitPrintStmt(this);
        }

        public readonly Expr expression;
    }

    public class Return : Stmt
    {
        public Return(Token keyword, Expr value)
        {
            this.keyword = keyword;
            this.value = value;
        }

        public override T Accept<T>(Visitor<T> visitor)
        {
            return visitor.visitReturnStmt(this);
        }

        public readonly Token keyword;
        public readonly Expr value;
    }

    public class Var : Stmt
    {
        public Var(Token name, Expr initializer)
        {
            this.name = name;
            this.initializer = initializer;
        }

        public override T Accept<T>(Visitor<T> visitor)
        {
            return visitor.visitVarStmt(this);
        }

        public readonly Token name;
        public readonly Expr initializer;
    }

    public class While : Stmt
    {
        public While(Expr condition, Stmt body)
        {
            this.condition = condition;
            this.body = body;
        }

        public override T Accept<T>(Visitor<T> visitor)
        {
            return visitor.visitWhileStmt(this);
        }

        public readonly Expr condition;
        public readonly Stmt body;
    }

    abstract public T Accept<T>(Visitor<T> visitor);

}
