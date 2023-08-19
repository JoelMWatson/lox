namespace Cslox.Scan
{
	public class Scanner
	{
		private readonly string source;
		private readonly List<Token> tokens = new List<Token>();
		private int start = 0;
		private int current = 0;
		private int line = 1;

		private static readonly Dictionary<string, TokenType> keywords;

		static Scanner()
		{
            keywords = new Dictionary<string, TokenType>
            {
                { "and", TokenType.AND },
				{ "class", TokenType.CLASS },
				{ "else", TokenType.ELSE },
				{ "false", TokenType.FALSE },
				{ "for", TokenType.FOR },
				{ "fun", TokenType.FUN },
				{ "if", TokenType.IF },
				{ "nil", TokenType.NIL },
				{ "or", TokenType.OR },
				{ "print", TokenType.PRINT },
				{ "return", TokenType.RETURN },
				{ "super", TokenType.SUPER },
				{ "this", TokenType.THIS },
				{ "true", TokenType.TRUE },
				{ "var", TokenType.VAR },
				{ "while", TokenType.WHILE },
				{ "break", TokenType.BREAK }
            };
        }

		public Scanner(string source)
		{
			this.source = source;
		}

		public List<Token> ScanTokens()
		{
			while (!IsAtEnd())
			{
                start = current;
				ScanToken();
			}

			tokens.Add(new Token(TokenType.EOF, "", null, line));
			return tokens;
		}

		private void ScanToken()
		{
			char c = Advance();
			switch (c)
			{
                // Single char
                case '(': AddToken(TokenType.LEFT_PAREN); break;
                case ')': AddToken(TokenType.RIGHT_PAREN); break;
                case '{': AddToken(TokenType.LEFT_BRACE); break;
                case '}': AddToken(TokenType.RIGHT_BRACE); break;
                case ',': AddToken(TokenType.COMMA); break;
                case '.': AddToken(TokenType.DOT); break;
                case '-': AddToken(TokenType.MINUS); break;
                case '+': AddToken(TokenType.PLUS); break;
                case ';': AddToken(TokenType.SEMICOLON); break;
                case '*': AddToken(TokenType.STAR); break;
                case '?': AddToken(TokenType.Q_MARK); break;
                case ':': AddToken(TokenType.COLON); break;

                // One or two char
                case '!':
                    AddToken(Match('=') ? TokenType.BANG_EQUAL : TokenType.BANG);
                    break;
                case '=':
                    AddToken(Match('=') ? TokenType.EQUAL_EQUAL : TokenType.EQUAL);
                    break;
                case '<':
                    AddToken(Match('=') ? TokenType.LESS_EQUAL : TokenType.LESS);
                    break;
                case '>':
                    AddToken(Match('=') ? TokenType.GREATER_EQUAL : TokenType.GREATER);
                    break;
                case '/':
					if (Match('/'))
					{
						while (Peek() != '\n' && !IsAtEnd()) Advance();
					}
					else if (Match('*'))
					{						
                        while (!(Peek() == '*' && PeekNext() == '/') && !IsAtEnd()) Advance();
						current += 2;
					}
					else
					{
						AddToken(TokenType.SLASH);
					}
                    break;

				// Whitespace
				case ' ':
				case '\r':
				case '\t':
					break; // ignore
				case '\n': line++; break;

				// Literals
				case '"': String(); break;
                default:
					if (IsDigit(c))
					{
						Number();
					}
					else if (IsAlpha(c))
					{
						Identifier();
					}
					else
					{
						Lox.Error(new Token(TokenType.NIL, c.ToString(), "", line), "Unexpected character.");
					}
					break;
            }
        }

        private bool IsAtEnd()
        {
            return current >= source.Length;
        }

        private char Advance()
		{
			return source[current++];
		}

		private void AddToken(TokenType type)
		{
			AddToken(type, null);
		}

		private void AddToken(TokenType type, Object? literal)
		{
			string text = source.Substring(start, current - start);
			tokens.Add(new Token(type, text, literal, line));
		}

		private bool Match(char expected)
		{
			if (IsAtEnd() || source[current] != expected) return false;

			current++;
			return true;
		}

        private char Peek()
        {
            if (IsAtEnd()) return '\0';
            return source[current];
        }

        private char PeekNext()
        {
            if (current + 1 >= source.Length) return '\0';
            return source[current+1];
        }

        private bool IsDigit(char c)
        {
            return c >= '0' && c <= '9';
        }

		private bool IsAlpha(char c)
		{
			return (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || c == '_';
		}

		private bool IsAlphaNumeric(char c)
		{
			return IsAlpha(c) || IsDigit(c);
		}

        private void String()
		{
			while(Peek() != '"' && !IsAtEnd())
			{
				if (Peek() == '\n') line++;
				Advance();
			}

			if (IsAtEnd())
			{
				Lox.Error(new Token(TokenType.EOF, Peek().ToString(), "", line), "Unterminated string.");
				return;
			}

			Advance();

			string value = source.Substring(start + 1, current - start - 2);
			AddToken(TokenType.STRING, value);
		}

		private void Number()
		{
			while (IsDigit(Peek())) Advance();

			if (Peek() == '.' && IsDigit(PeekNext()))
			{
				Advance();
				while (IsDigit(Peek())) Advance();
			}

			AddToken(TokenType.NUMBER, double.Parse(source.Substring(start, current-start)));
		}

		private void Identifier()
		{
			while (IsAlphaNumeric(Peek())) Advance();

			TokenType type;
			string text = source.Substring(start, current - start);
			bool found = keywords.TryGetValue(text, out type);
			if (!found) type = TokenType.IDENTIFIER;
			AddToken(type);
		}
	} 
}

