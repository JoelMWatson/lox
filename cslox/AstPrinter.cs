﻿using System.Text;
using cslox.Scan;

namespace cslox
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

        public string visitBinaryExpr(Expr.Binary expr)
        {
            return Parenthesize(expr.op.lexeme, expr.left, expr.right);
        }

        public string visitGroupingExpr(Expr.Grouping expr)
        {
            return Parenthesize("group", expr.expression);
        }

        public string visitLiteralExpr(Expr.Literal expr)
        {
            if (expr.value == null) return "nil";
            return expr.value.ToString()!;
        }

        public string visitUnaryExpr(Expr.Unary expr)
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

        // Proof it is working
        //public static void Main(string[] args)
        //{
        //    Expr expression = new Expr.Binary(
        //        new Expr.Unary(
        //            new Token(TokenType.MINUS, "-", null, 1),
        //            new Expr.Literal(123)),
        //        new Token(TokenType.STAR, "*", null, 1),
        //        new Expr.Grouping(
        //            new Expr.Literal(45.67)));

        //    Console.WriteLine(new AstPrinter().Print(expression));
        //}
    }
}

