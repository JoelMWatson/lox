﻿using Cslox.Scan;

namespace Cslox.Parse
{
	public class Parser
	{
        private class ParseError : Exception { }
        private readonly List<Token> tokens;
		private int current = 0;
        private int loopDepth = 0;

		public Parser(List<Token> tokens)
		{
			this.tokens = tokens;
		}

        public List<Stmt> Parse()
        {
            List<Stmt> statements = new List<Stmt>();
            while(!IsAtEnd())
            {
                statements.Add(Declaration());
            }
            return statements;
        }

        private Expr Expression()
        {
            return Assignment();
        }

        private Stmt Statement()
        {
            if (Match(TokenType.FOR)) return ForStatement();
            if (Match(TokenType.IF)) return IfStatement();
            if (Match(TokenType.PRINT)) return PrintStatement();
            if (Match(TokenType.RETURN)) return ReturnStatement();
            if (Match(TokenType.WHILE)) return WhileStatement();
            if (Match(TokenType.LEFT_BRACE)) return new Stmt.Block(Block());
            if (Match(TokenType.BREAK)) return Break();
            return ExpressionStatement();
        }

        private Stmt IfStatement()
        {
            Consume(TokenType.LEFT_PAREN, "Expect '(' after 'if'.");
            Expr condition = Expression();
            Consume(TokenType.RIGHT_PAREN, "Expect ')' after if condition.");

            Stmt thenBranch = Statement();
            Stmt elseBranch = null;
            if (Match(TokenType.ELSE))
            {
                elseBranch = Statement();
            }

            return new Stmt.If(condition, thenBranch, elseBranch);
        }

        private Stmt Declaration()
        {
            try
            {
                if (Check(TokenType.FUN) && CheckNext(TokenType.IDENTIFIER))
                {
                    Consume(TokenType.FUN, null);
                    return Function("funciton");
                }
                if (Match(TokenType.VAR)) return VarDeclaration();
                return Statement();
            }
            catch (ParseError error)
            {
                Synchronize();
                return null;
            }
        }

        private Stmt VarDeclaration()
        {
            Token name = Consume(TokenType.IDENTIFIER, "Expect variable name.");
            Expr initializer = null;
            if (Match(TokenType.EQUAL))
            {
                initializer = Expression();
            }
            Consume(TokenType.SEMICOLON, "Expect ';' after variable declaration.");
            return new Stmt.Var(name, initializer);
        }

        private Stmt WhileStatement()
        {
            Consume(TokenType.LEFT_PAREN, "Expect '(' after while.");
            Expr condition = Expression();
            Consume(TokenType.RIGHT_PAREN, "Expect ')' after condition.");
            try
            {
                loopDepth++;
                Stmt body = Statement();
                return new Stmt.While(condition, body);
            }
            finally
            {
                loopDepth--;
            }
        }

        private Stmt ForStatement()
        {
            Consume(TokenType.LEFT_PAREN, "Expect '(' after if.");
            Stmt initializer;
            if (Match(TokenType.SEMICOLON))
            {
                initializer = null;
            }
            else if (Match(TokenType.VAR))
            {
                initializer = VarDeclaration();
            }
            else
            {
                initializer = ExpressionStatement();
            }

            Expr condition = null;
            if (!Check(TokenType.SEMICOLON))
            {
                condition = Expression();
            }
            Consume(TokenType.SEMICOLON, "Expect ';' after condition.");

            Expr increment = null;
            if (!Check(TokenType.RIGHT_PAREN))
            {
                increment = Expression();
            }
            Consume(TokenType.RIGHT_PAREN, "Expect ')' after for clauses.");

            try
            {
                loopDepth++;
                Stmt body = Statement();
                if (increment != null)
                {
                    body = new Stmt.Block(new List<Stmt>()
                    {
                        body,
                        new Stmt.Expression(increment)
                    });
                }

                if (condition == null)
                {
                    condition = new Expr.Literal(true);
                }
                body = new Stmt.While(condition, body);

                if (initializer != null)
                {
                    body = new Stmt.Block(new List<Stmt>() { initializer, body });
                }
                return body;
            }
            finally
            {
                loopDepth--;
            }
            
        }

        private Stmt PrintStatement()
        {
            Expr value = Expression();
            Consume(TokenType.SEMICOLON, "Expect ';' after value.");
            return new Stmt.Print(value);
        }

        private Stmt ReturnStatement()
        {
            Token keyword = Previous();
            Expr value = null;
            if (!Check(TokenType.SEMICOLON))
            {
                value = Expression();
            }
            Consume(TokenType.SEMICOLON, "Expect ';' after return");
            return new Stmt.Return(keyword, value);
        }

        private Stmt ExpressionStatement()
        {
            Expr expr = Expression();
            Consume(TokenType.SEMICOLON, "Expect ';' after expression.");
            return new Stmt.Expression(expr);
        }

        private List<Stmt> Block()
        {
            List<Stmt> statements = new List<Stmt>();
            while (!Check(TokenType.RIGHT_BRACE) && !IsAtEnd())
            {
                statements.Add(Declaration());
            }
            Consume(TokenType.RIGHT_BRACE, "Expect '}' after block.");
            return statements;
        }

        private Stmt Break()
        {
            if (loopDepth == 0)
            {
                Error(Previous(), "Must be inside a loop to call break");
            }
            Consume(TokenType.SEMICOLON, "Expect ';' after break.");
            return new Stmt.Break();
        }

        private Stmt.Function Function(string kind)
        {
            Token name = Consume(TokenType.IDENTIFIER, $"Expect {kind} name.");
            return new Stmt.Function(name, FunctionBody(kind));
        }

        private Expr.Function FunctionBody(string kind)
        {
            Consume(TokenType.LEFT_PAREN, $"Expect '(' after {kind} name.");
            List<Token> parameters = new List<Token>();
            if (!Check(TokenType.RIGHT_PAREN))
            {
                do
                {
                    if (parameters.Count() >= 255)
                    {
                        Error(Peek(), "Cannot have more than 255 parameters.");
                    }
                    parameters.Add(Consume(TokenType.IDENTIFIER, "Expect parameter name."));
                } while (Match(TokenType.COMMA));
            }
            Consume(TokenType.RIGHT_PAREN, "Expect ')' after parameters.");
            Consume(TokenType.LEFT_BRACE, $"Expect '}}' before {kind} body.");
            List<Stmt> body = Block();
            return new Expr.Function(parameters, body);
        }

        private Expr Assignment()
        {
            Expr expr = Or();
            if (Match(TokenType.EQUAL))
            {
                Token equals = Previous();
                Expr value = Assignment();
                if (expr.GetType() == typeof(Expr.Variable)) 
                {
                    Token name = ((Expr.Variable)expr).name;
                    return new Expr.Assign(name, value);
                }
                Error(equals, "Invalid assignment target.");
            }
            return expr;
        }

        private Expr Or()
        {
            Expr expr = And();
            while (Match(TokenType.OR))
            {
                Token op = Previous();
                Expr right = And();
                expr = new Expr.Logical(expr, op, right);
            }
            return expr;
        }

        private Expr And()
        {
            Expr expr = Comma();
            while (Match(TokenType.AND))
            {
                Token op = Previous();
                Expr right = Comma();
                expr = new Expr.Logical(expr, op, right);
            }
            return expr;
        }

        private Expr Comma()
        {
            Expr expr = Conditional();
            while (Match(TokenType.COMMA))
            {
                Token op = Previous();
                Expr right = Conditional();
                expr = new Expr.Binary(expr, op, right);
            }
            return expr;
        }

        private Expr Conditional()
        {
            Expr expr = Equality();
            while(Match(TokenType.Q_MARK))
            {
                Token op = Previous();
                Expr left = Expression();
                Consume(TokenType.COLON, "Expected ':' for ternary expression");
                Expr right = Conditional();
                expr = new Expr.Conditional(op, expr, left, right);
            }
            return expr;
        }

        private Expr Equality()
        {
            Expr expr = Comparison();
            while(Match(TokenType.BANG_EQUAL, TokenType.EQUAL_EQUAL))
            {
                Token op = Previous();
                Expr right = Comparison();
                expr = new Expr.Binary(expr, op, right);
            }
            return expr;
        }

        private Expr Comparison()
        {
            Expr expr = Term();

            while(Match(TokenType.GREATER, TokenType.GREATER_EQUAL, TokenType.LESS, TokenType.LESS_EQUAL))
            {
                Token op = Previous();
                Expr right = Term();
                expr = new Expr.Binary(expr, op, right);
            }
            return expr;
        }

        private Expr Term()
        {
            Expr expr = Factor();

            while (Match(TokenType.PLUS, TokenType.MINUS))
            {
                Token op = Previous();
                Expr right = Factor();
                expr = new Expr.Binary(expr, op, right);
            }
            return expr;
        }

        private Expr Factor()
        {
            Expr expr = Unary();

            while (Match(TokenType.STAR, TokenType.SLASH))
            {
                Token op = Previous();
                Expr right = Unary();
                expr = new Expr.Binary(expr, op, right);
            }
            return expr;
        }

        private Expr Unary()
        {
            while(Match(TokenType.BANG, TokenType.MINUS))
            {
                Token op = Previous();
                Expr right = Unary();
                return new Expr.Unary(op, right);
            }

            return Call();
        }

        private Expr Call()
        {
            Expr expr = Primary();
            while(true)
            {
                if (Match(TokenType.LEFT_PAREN))
                {
                    expr = FinishCall(expr);
                }
                else
                {
                    break;
                }
            }
            return expr;
        }

        private Expr FinishCall(Expr callee)
        {
            List<Expr> arguments = new List<Expr>();
            if (!Check(TokenType.RIGHT_PAREN))
            {
                do
                {
                    if (arguments.Count() >= 255) Error(Peek(), "Can't have more than 255 arguments");
                    arguments.Add(Expression());
                }
                while (Match(TokenType.COMMA));
            }
            Token paren = Consume(TokenType.RIGHT_PAREN, "Expect ')' after arguments.");
            return new Expr.Call(callee, paren, arguments);
        }

        private Expr Primary()
        {
            if (Match(TokenType.TRUE)) return new Expr.Literal(true);
            if (Match(TokenType.FALSE)) return new Expr.Literal(false);
            if (Match(TokenType.NIL)) return new Expr.Literal(null);

            if (Match(TokenType.NUMBER, TokenType.STRING))
            {
                return new Expr.Literal(Previous().literal);
            }

            if (Match(TokenType.IDENTIFIER))
            {
                return new Expr.Variable(Previous());
            }

            if (Match(TokenType.LEFT_PAREN))
            {
                Expr expr = Expression();
                Consume(TokenType.RIGHT_PAREN, "Expect ')' after expression.");
                return new Expr.Grouping(expr);
            }

            if (Match(TokenType.FUN)) return FunctionBody("function");

            throw Error(Peek(), "Expected expression.");
        }

        private bool Match(params TokenType[] types)
        {
            foreach (TokenType type in types)
            {
                if (Check(type))
                {
                    Advance();
                    return true;
                }
            }
            return false;
        }

        private bool Check(TokenType type)
        {
            if (IsAtEnd()) return false;
            return Peek().type == type;
        }

        private bool CheckNext(TokenType tokenType)
        {
            if (IsAtEnd()) return false;
            if (tokens[current + 1].type == TokenType.EOF) return false;
            return tokens[current + 1].type == tokenType;
        }

        private Token Advance()
        {
            if (!IsAtEnd()) current++;
            return Previous();
        }

        private bool IsAtEnd()
        {
            return Peek().type == TokenType.EOF;
        }

        private Token Peek()
        {
            return tokens[current];
        }

        private Token Previous()
        {
            return tokens[current - 1];
        }

        private Token Consume(TokenType type, string message)
        {
            if (Check(type)) return Advance();
            throw Error(Peek(), message);
        }

        private ParseError Error(Token token, string message)
        {
            Lox.Error(token, message);
            return new ParseError();
        }

        private void Synchronize()
        {
            Advance();
            while(!IsAtEnd())
            {
                if (Previous().type == TokenType.SEMICOLON) return;
                switch (Peek().type)
                {
                    case TokenType.CLASS:
                    case TokenType.FOR:
                    case TokenType.FUN:
                    case TokenType.IF:
                    case TokenType.PRINT:
                    case TokenType.RETURN:
                    case TokenType.VAR:
                    case TokenType.WHILE:
                        return;
                }
                Advance();
            }
        }
    }
}

