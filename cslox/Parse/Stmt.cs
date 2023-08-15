using Cslox.Scan;

namespace Cslox;
public abstract class Stmt
{

    public interface Visitor<T>
    {
        T visitBlockStmt(Block stmt);
        T visitExpressionStmt(Expression stmt);
        T visitPrintStmt(Print stmt);
        T visitVarStmt(Var stmt);
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

    abstract public T Accept<T>(Visitor<T> visitor);

}
