using Cslox.Scan;

namespace Cslox;
public abstract class Expr
{

    public interface Visitor<T>
    {
        T visitAssignExpr(Assign expr);
        T visitBinaryExpr(Binary expr);
        T visitGroupingExpr(Grouping expr);
        T visitLiteralExpr(Literal expr);
        T visitVariableExpr(Variable expr);
        T visitUnaryExpr(Unary expr);
        T visitConditionalExpr(Conditional expr);
    }

    public class Assign : Expr
    {
        public Assign(Token name, Expr value)
        {
            this.name = name;
            this.value = value;
        }

        public override T Accept<T>(Visitor<T> visitor)
        {
            return visitor.visitAssignExpr(this);
        }

        public readonly Token name;
        public readonly Expr value;
    }

    public class Binary : Expr
    {
        public Binary(Expr left, Token op, Expr right)
        {
            this.left = left;
            this.op = op;
            this.right = right;
        }

        public override T Accept<T>(Visitor<T> visitor)
        {
            return visitor.visitBinaryExpr(this);
        }

        public readonly Expr left;
        public readonly Token op;
        public readonly Expr right;
    }

    public class Grouping : Expr
    {
        public Grouping(Expr expression)
        {
            this.expression = expression;
        }

        public override T Accept<T>(Visitor<T> visitor)
        {
            return visitor.visitGroupingExpr(this);
        }

        public readonly Expr expression;
    }

    public class Literal : Expr
    {
        public Literal(Object value)
        {
            this.value = value;
        }

        public override T Accept<T>(Visitor<T> visitor)
        {
            return visitor.visitLiteralExpr(this);
        }

        public readonly Object value;
    }

    public class Variable : Expr
    {
        public Variable(Token name)
        {
            this.name = name;
        }

        public override T Accept<T>(Visitor<T> visitor)
        {
            return visitor.visitVariableExpr(this);
        }

        public readonly Token name;
    }

    public class Unary : Expr
    {
        public Unary(Token op, Expr right)
        {
            this.op = op;
            this.right = right;
        }

        public override T Accept<T>(Visitor<T> visitor)
        {
            return visitor.visitUnaryExpr(this);
        }

        public readonly Token op;
        public readonly Expr right;
    }

    public class Conditional : Expr
    {
        public Conditional(Token op, Expr cond, Expr left, Expr right)
        {
            this.op = op;
            this.cond = cond;
            this.left = left;
            this.right = right;
        }

        public override T Accept<T>(Visitor<T> visitor)
        {
            return visitor.visitConditionalExpr(this);
        }

        public readonly Token op;
        public readonly Expr cond;
        public readonly Expr left;
        public readonly Expr right;
    }

    abstract public T Accept<T>(Visitor<T> visitor);

}
