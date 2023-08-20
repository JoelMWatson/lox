using Cslox.Interpret.Natives;
using Cslox.Scan;

namespace Cslox.Interpret
{
	public class Interpreter : Expr.Visitor<object>, Stmt.Visitor<object>
	{
        private readonly LoxEnvironment globals;
        private readonly Dictionary<Expr, int> locals;
        private LoxEnvironment environment;

        public Interpreter()
        {
            globals = new LoxEnvironment();
            environment = globals;
            locals = new Dictionary<Expr, int>();

            globals.Define("clock", new Clock());
        }

        public void Interpret(List<Stmt> statements)
        {
            try
            {
                foreach (Stmt statement in statements)
                {
                    Execute(statement);
                }
            }
            catch (RuntimeError error)
            {
                Lox.RuntimeError(error);
            }
        }

        public void Interpret(Expr expression)
        {
            try
            {
                object value = Evaluate(expression);
                Console.WriteLine(Stringify(value));
            }
            catch (RuntimeError error)
            {
                Lox.RuntimeError(error);
            }
        }

        private object Evaluate(Expr expr)
        {
            return expr.Accept(this);
        }

        private void Execute(Stmt stmt)
        {
            stmt.Accept(this);
        }

        private bool IsTruthy(object obj)
        {
            if (obj == null) return false;
            if (obj is bool) return (bool)obj;
            return true;
        }

        private bool IsEqual(object? left, object? right)
        {
            if (left == null && right == null) return true;
            if (left == null) return false;
            return left.Equals(right);
        }

        private string Stringify(object obj)
        {
            if (obj == null) return "nil";

            if (obj is double)
            {
                string text = obj.ToString() ?? "";
                if (text.EndsWith(".0"))
                {
                    text = text.Substring(0, text.Length - 2);
                }
                return text;
            }

            return obj.ToString() ?? "";
        }

        public void Resolve(Expr expr, int depth)
        {
            locals[expr] = depth;
        }

        public object VisitAssignExpr(Expr.Assign expr)
        {
            object value = Evaluate(expr.value);

            int distance = locals[expr];
            if (distance == null)
            {
                globals.Assign(expr.name, value);
            }
            else
            {
                environment.AssignAt(distance, expr.name, value);
            }

            return value;
        }

        public object VisitBinaryExpr(Expr.Binary expr)
        {
            object left = Evaluate(expr.left);
            object right = Evaluate(expr.right);

            switch (expr.op.type)
            {
                case TokenType.PLUS:
                    if (left is double)
                    {
                        if (right is double) return (double)left + (double)right;
                        if (right is string) return ((double)left).ToString() + (string)right;
                    }
                    else if (left is string)
                    {
                        if (right is string) return (string)left + (string)right;
                        if (right is double) return (string)left + ((double)right).ToString();
                    }
                    throw new RuntimeError(expr.op, "Operands must be two numbers or strings.");
                case TokenType.MINUS:
                    CheckNumberOperands(expr.op, left, right);
                    return (double)left - (double)right;
                case TokenType.STAR:
                    CheckNumberOperands(expr.op, left, right);
                    return (double)left * (double)right;
                case TokenType.SLASH:
                    CheckNumberOperands(expr.op, left, right);
                    if ((double)right != 0) return (double)left / (double)right;
                    throw new RuntimeError(expr.op, "Cannot divide by zero.");
                case TokenType.GREATER:
                    CheckNumberOperands(expr.op, left, right);
                    return (double)left > (double)right;
                case TokenType.GREATER_EQUAL:
                    CheckNumberOperands(expr.op, left, right);
                    return (double)left >= (double)right;
                case TokenType.LESS:
                    CheckNumberOperands(expr.op, left, right);
                    return (double)left < (double)right;
                case TokenType.LESS_EQUAL:
                    CheckNumberOperands(expr.op, left, right);
                    return (double)left <= (double)right;
                case TokenType.BANG_EQUAL:
                    return !IsEqual(left, right);
                case TokenType.EQUAL_EQUAL:
                    return IsEqual(left, right);
            }
            return null;
        }

        private void CheckNumberOperands(Token op, object left, object right)
        {
            if (left is double && right is double) return;
            throw new RuntimeError(op, "Operands must be numbers.");
        }

        public object VisitCallExpr(Expr.Call expr)
        {
            object callee = Evaluate(expr.callee);
            List<object> arguments = new List<object>();
            foreach (Expr argument in expr.arguments)
            {
                arguments.Add(Evaluate(argument));
            }

            if (!(callee is LoxCallable))
            {
                throw new RuntimeError(expr.paren, "Can only call functions or classes.");
            }

            LoxCallable function = (LoxCallable)callee;

            if (arguments.Count() != function.Arity())
            {
                throw new RuntimeError(expr.paren, $"Expected {function.Arity()} arguments but got {arguments.Count()}.");
            }
            return function.Call(this, arguments);
        }

        public object VisitConditionalExpr(Expr.Conditional expr)
        {
            if (IsTruthy(Evaluate(expr.cond)))
            {
                return Evaluate(expr.left);
            }
            else
            {
                return Evaluate(expr.right);
            }
        }

        public object VisitFunctionExpr(Expr.Function expr)
        {
            return new LoxFunction(null, expr, environment);
        }

        public object VisitGroupingExpr(Expr.Grouping expr)
        {
            return Evaluate(expr.expression);
        }

        public object VisitLiteralExpr(Expr.Literal expr)
        {
            return expr.value;
        }

        public object VisitLogicalExpr(Expr.Logical expr)
        {
            object left = Evaluate(expr.left);
            if (expr.op.type == TokenType.OR)
            {
                if (IsTruthy(left)) return left;
            }
            else
            {
                if (!IsTruthy(left)) return left;
            }
            return Evaluate(expr.right);
        }

        public object VisitUnaryExpr(Expr.Unary expr)
        {
            object right = Evaluate(expr.right);

            switch (expr.op.type)
            {
                case TokenType.BANG:
                    return !IsTruthy(right);
                case TokenType.MINUS:
                    CheckNumberOperand(expr.op, right);
                    return -(double)right;
            }

            return null;
        }

        private void CheckNumberOperand(Token op, object operand)
        {
            if (operand is double) return;
            throw new RuntimeError(op, "Operand must be a number.");
        }

        public object VisitVariableExpr(Expr.Variable expr)
        {
            return LookUpVariable(expr.name, expr);
        }

        private object LookUpVariable(Token name, Expr expr)
        {
            int distance = locals[expr];
            if (distance == null)
            {
                return globals.Get(name);
            }
            else
            {
                return environment.GetAt(distance, name.lexeme);
            }
        }

        public object VisitBlockStmt(Stmt.Block stmt)
        {
            ExecuteBlock(stmt.statements, new LoxEnvironment(environment));
            return null; // No void type ref in C#
        }

        public void ExecuteBlock(List<Stmt> stmts, LoxEnvironment env)
        {
            LoxEnvironment prev = this.environment;
            try
            {
                this.environment = env;
                foreach (Stmt stmt in stmts)
                {
                    Execute(stmt);
                }
            }
            finally
            {
                this.environment = prev;
            }
        }

        public object VisitBreakStmt(Stmt.Break stmt)
        {
            throw new BreakException();
        }

        public object VisitExpressionStmt(Stmt.Expression stmt)
        {
            Evaluate(stmt.expression);
            return null; // No void type ref in C#
        }

        public object VisitFunctionStmt(Stmt.Function stmt)
        {
            LoxFunction function = new LoxFunction(stmt.name.lexeme, stmt.function, environment);
            environment.Define(stmt.name.lexeme, function);
            return null; // No void type ref in C#
        }

        public object VisitIfStmt(Stmt.If stmt)
        {
            if (IsTruthy(Evaluate(stmt.condition)))
            {
                Execute(stmt.thenBranch);
            }
            else if (stmt.elseBranch != null)
            {
                Execute(stmt.elseBranch);
            }
            return null; // No void type ref in C#
        }

        public object VisitPrintStmt(Stmt.Print stmt)
        {
            object value = Evaluate(stmt.expression);
            Console.WriteLine(Stringify(value));
            return null; // No void type ref in C#
        }

        public object VisitReturnStmt(Stmt.Return stmt)
        {
            object value = null;
            if (stmt.value != null) value = Evaluate(stmt.value);

            throw new ReturnException(value);
        }

        public object VisitVarStmt(Stmt.Var stmt)
        {
            object value = null;
            if (stmt.initializer != null)
            {
                value = Evaluate(stmt.initializer);
            }
            environment.Define(stmt.name.lexeme, value);
            return null; // No void type ref in C#
        }

        public object VisitWhileStmt(Stmt.While stmt)
        {
            try
            {
                while (IsTruthy(Evaluate(stmt.condition)))
                {
                    Execute(stmt.body);
                }
            }
            catch (BreakException)
            {

            }
            return null; // No void type ref in C
        }
    }
}

