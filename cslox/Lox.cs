using cslox.Scan;
using cslox.Parse;
using cslox.Utilities;

namespace cslox
{
    public class Lox
    {
        private static bool hadError = false;

        static void Main(string[] args)
        {
            if (args.Length > 1)
            {
                Console.WriteLine("Usage: cslox: [script]");
                Environment.Exit(64);
            }
            else if (args.Length == 1)
            {
                RunFile(args[0]);
            }
            else
            {
                RunPrompt();
            }

        }

        // File reader
        private static void RunFile(string path)
        {
            byte[] bytes = File.ReadAllBytes(path);
            Run(System.Text.Encoding.Default.GetString(bytes));

            if (hadError) Environment.Exit(65);
        }

        // Prompt initiator
        private static void RunPrompt()
        {
            for (; ; )
            {
                Console.Write("> ");
                string? line = Console.ReadLine();
                if (line == null) break;
                Run(line);
                hadError = false;
            }
        }

        // Actual runner of code
        private static void Run(string source)
        {
            Scanner scanner = new Scanner(source);
            List<Token> tokens = scanner.ScanTokens();
            Parser parser = new Parser(tokens);
            Expr expression = parser.Parse();

            if (hadError) return;

            Console.WriteLine(new AstPrinter().Print(expression));
        }

        // Bare bones error handling
        public static void Error(Token token, string message)
        {
            if (token.type == TokenType.EOF)
            {
                Report(token.line, " at end", message);
            }
            else
            {
                Report(token.line, $" at '{token.lexeme}'", message);
            }
        }

        // Bare bones error reporting output
        private static void Report(int line, string where, string message)
        {
            Console.Error.WriteLine($"[line {0}] Error {1}: {2}", line, where, message);
            hadError = true;
        }
    }
}



