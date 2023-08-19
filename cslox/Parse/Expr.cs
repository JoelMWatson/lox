using Cslox.Scan;

namespace Cslox;
public abstract class Expr
{

    public interface Visitor<T>
    {
        T visitAssignExpr(Assign expr);
        T visitBinaryExpr(Binary expr);
        T visitCallExpr(Call expr);
        T visitConditionalExpr(Conditional expr);
        T visitFunctionExpr(Function expr);
        T visitGroupingExpr(Grouping expr);
        T visitLiteralExpr(Literal expr);
        T visitLogicalExpr(Logical expr);
        T visitVariableExpr(Variable expr);
        T visitUnaryExpr(Unary expr);
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

    public class Call : Expr
    {
        public Call(Expr callee, Token paren, List<Expr> arguments)
        {
            this.callee = callee;
            this.paren = paren;
            this.arguments = arguments;
        }

        public override T Accept<T>(Visitor<T> visitor)
        {
            return visitor.visitCallExpr(this);
        }

        public readonly Expr callee;
        public readonly Token paren;
        public readonly List<Expr> arguments;
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

    public class Function : Expr
    {
        public Function(List<Token> parameters, List<Stmt> body)
        {
            this.parameters = parameters;
            this.body = body;
        }

        public override T Accept<T>(Visitor<T> visitor)
        {
            return visitor.visitFunctionExpr(this);
        }

        public readonly List<Token> parameters;
        public readonly List<Stmt> body;
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

    public class Logical : Expr
    {
        public Logical(Expr left, Token op, Expr right)
        {
            this.left = left;
            this.op = op;
            this.right = right;
        }

        public override T Accept<T>(Visitor<T> visitor)
        {
            return visitor.visitLogicalExpr(this);
        }

        public readonly Expr left;
        public readonly Token op;
        public readonly Expr right;
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

    abstract public T Accept<T>(Visitor<T> visitor);

}
