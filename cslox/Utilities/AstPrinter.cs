using System.Text;

namespace cslox.Utilities
{
	public class AstPrinter : Expr.Visitor<string>
	{
		public AstPrinter()
        {
        }

		public string Print(Expr expr)
		{
			return expr.Accept(this);
		}

        private string visitBinaryExpr(Expr.Binary expr)
        {
            return Parenthesize(expr.op.lexeme, expr.left, expr.right);
        }

        private string visitConditionalExpr(Expr.Conditional expr)
        {
            return Parenthesize(expr.op.lexeme, expr.cond, expr.left, expr.right);
        }

        private string visitGroupingExpr(Expr.Grouping expr)
        {
            return Parenthesize("group", expr.expression);
        }

        private string visitLiteralExpr(Expr.Literal expr)
        {
            if (expr.value == null) return "nil";
            return expr.value.ToString()!;
        }

        private string visitUnaryExpr(Expr.Unary expr)
        {
            return Parenthesize(expr.op.lexeme, expr.right);
        }

        private string Parenthesize(string name, params Expr[] exprs)
        {
            StringBuilder builder = new StringBuilder();

            builder.Append("(").Append(name);
            foreach (Expr expr in exprs)
            {
                builder.Append(" ");
                builder.Append(expr.Accept(this));
            }
            builder.Append(")");

            return builder.ToString();
        }
    }
}

