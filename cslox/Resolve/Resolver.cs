using System.Xml.Linq;
using Cslox.Interpret;
using Cslox.Scan;

namespace Cslox.Resolve
{
	public class Resolver : Expr.Visitor<object>, Stmt.Visitor<object>
	{
        private readonly Interpreter interpreter;
        private readonly Stack<Dictionary<string, bool>> scopes = new Stack<Dictionary<string, bool>>();
        private FunctionType currentFunction = FunctionType.NONE;
        private ClassType currentClass = ClassType.NONE;

		public Resolver(Interpreter interpreter)
		{
            this.interpreter = interpreter;
		}

        public void Resolve(List<Stmt> statements)
        {
            foreach (Stmt statement in statements)
            {
                Resolve(statement);
            }
        }

        private void Resolve(Stmt statement)
        {
            statement.Accept(this);
        }

        private void Resolve(Expr expression)
        {
            expression.Accept(this);
        }

        private void BeginScope()
        {
            scopes.Push(new Dictionary<string, bool>());
        }

        private void EndScope()
        {
            scopes.Pop();
        }

        private void Declare(Token name)
        {
            if (scopes.Count() == 0) return;
            Dictionary<string, bool> scope = scopes.Peek();
            if (scope.ContainsKey(name.lexeme))
            {
                Lox.Error(name, $"A variable with name {name.lexeme} is already defined in this scope");
            }
            scope[name.lexeme] = false;
        }

        private void Define(Token name)
        {
            if (scopes.Count() == 0) return;
            scopes.Peek()[name.lexeme] = true;
        }

        private void ResolveLocal(Expr expr, Token name)
        {
            for (int i = scopes.Count() - 1; i >= 0; i--)
            {
                if (scopes.ElementAt(i).ContainsKey(name.lexeme))
                {
                    interpreter.Resolve(expr, scopes.Count() - 1 - i);
                    return;
                }
            }
        }

        public object VisitAssignExpr(Expr.Assign expr)
        {
            Resolve(expr.value);
            ResolveLocal(expr, expr.name);
            return null;
        }

        public object VisitBinaryExpr(Expr.Binary expr)
        {
            Resolve(expr.left);
            Resolve(expr.right);
            return null;
        }

        public object VisitCallExpr(Expr.Call expr)
        {
            Resolve(expr.callee);
            foreach (Expr argument in expr.arguments)
            {
                Resolve(argument);
            }
            return null;
        }

        public object VisitConditionalExpr(Expr.Conditional expr)
        {
            Resolve(expr.cond);
            Resolve(expr.left);
            Resolve(expr.right);
            return null;
        }

        public object VisitFunctionExpr(Expr.Function expr)
        {
            if (currentClass == ClassType.NONE)
            {
                ResolveFunction(expr, FunctionType.FUNCTION);
            }
            else
            {
                ResolveFunction(expr, FunctionType.METHOD);
            }
            return null;
        }

        private void ResolveFunction(Expr.Function expr, FunctionType type)
        {
            FunctionType enclosingFunction = currentFunction;
            currentFunction = type;
            BeginScope();
            foreach (Token param in expr.parameters)
            {
                Declare(param);
                Define(param);
            }
            Resolve(expr.body);
            EndScope();
            currentFunction = enclosingFunction;
        }

        public object VisitGetExpr(Expr.Get expr)
        {
            Resolve(expr.obj);
            return null;
        }

        public object VisitGroupingExpr(Expr.Grouping expr)
        {
            Resolve(expr.expression);
            return null;
        }

        public object VisitLiteralExpr(Expr.Literal expr)
        {
            return null;
        }

        public object VisitLogicalExpr(Expr.Logical expr)
        {
            Resolve(expr.left);
            Resolve(expr.right);
            return null;
        }

        public object VisitSetExpr(Expr.Set expr)
        {
            Resolve(expr.value);
            Resolve(expr.obj);
            return null;
        }

        public object VisitSuperExpr(Expr.Super expr)
        {
            if (currentClass == ClassType.NONE)
            {
                Lox.Error(expr.keyword, "Can't use 'super' outside of a class.");
            }
            else if (currentClass == ClassType.CLASS)
            {
                Lox.Error(expr.keyword, "Can't use 'super' in a class with no superclass.");
            }
            ResolveLocal(expr, expr.keyword);
            return null;
        }

        public object VisitThisExpr(Expr.This expr)
        {
            if (currentClass == ClassType.NONE)
            {
                Lox.Error(expr.keyword, "Cannot call 'this' outside of a class");
            }
            ResolveLocal(expr, expr.keyword);
            return null;
        }

        public object VisitUnaryExpr(Expr.Unary expr)
        {
            Resolve(expr.right);
            return null;
        }

        public object VisitVariableExpr(Expr.Variable expr)
        {
            if (!(scopes.Count() == 0) && scopes.Peek()[expr.name.lexeme] == false)
            {
                Lox.Error(expr.name, "Can't read local variable in its own initializer.");
            }

            ResolveLocal(expr, expr.name);
            return null;
        }

        public object VisitBlockStmt(Stmt.Block stmt)
        {
            BeginScope();
            Resolve(stmt.statements);
            EndScope();
            return null;
        }

        public object VisitBreakStmt(Stmt.Break stmt)
        {
            return null;
        }

        public object VisitClassStmt(Stmt.Class stmt)
        {
            ClassType enclosing = currentClass;
            currentClass = ClassType.CLASS;
            Declare(stmt.name);
            Define(stmt.name);
            if (stmt.superclass != null && stmt.name.lexeme.Equals(stmt.superclass.name.lexeme))
            {
                Lox.Error(stmt.superclass.name, "A class can't inherit from itself.");
            }
            if (stmt.superclass != null)
            {
                BeginScope();
                scopes.Peek()["super"] = true;
                currentClass = ClassType.SUBCLASS;
                Resolve(stmt.superclass);
            }
            BeginScope();
            scopes.Peek()["this"] = true;
            foreach (Stmt.Function method in stmt.methods)
            {
                FunctionType declaration = FunctionType.METHOD;
                if (method.name.lexeme.Equals("init"))
                {
                    declaration = FunctionType.INITIALIZER;
                }
                ResolveFunction(method.function, declaration);
            }
            EndScope();
            if (stmt.superclass != null) EndScope();
            currentClass = enclosing;
            return null;
        }

        public object VisitExpressionStmt(Stmt.Expression stmt)
        {
            Resolve(stmt.expression);
            return null;
        }

        public object VisitFunctionStmt(Stmt.Function stmt)
        {
            Declare(stmt.name);
            Define(stmt.name);
            Resolve(stmt.function);
            return null;
        }

        public object VisitIfStmt(Stmt.If stmt)
        {
            Resolve(stmt.condition);
            Resolve(stmt.thenBranch);
            if (stmt.elseBranch != null) Resolve(stmt.elseBranch);
            return null;
        }

        public object VisitPrintStmt(Stmt.Print stmt)
        {
            Resolve(stmt.expression);
            return null;
        }

        public object VisitReturnStmt(Stmt.Return stmt)
        {
            if (currentFunction == FunctionType.NONE)
            {
                Lox.Error(stmt.keyword, "Cannot return from top-level code.");
            }
            if (stmt.value != null)
            {
                if (currentFunction == FunctionType.INITIALIZER)
                {
                    Lox.Error(stmt.keyword, "Can't return a value from an initializer.");
                }
                Resolve(stmt.value);
            }
            return null;
        }

        public object VisitVarStmt(Stmt.Var stmt)
        {
            Declare(stmt.name);
            if (stmt.initializer != null)
            {
                Resolve(stmt.initializer);
            }
            Define(stmt.name);
            return null;
        }

        public object VisitWhileStmt(Stmt.While stmt)
        {
            Resolve(stmt.condition);
            Resolve(stmt.body);
            return null;
        }
    }
}

