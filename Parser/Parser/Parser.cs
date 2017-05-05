using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Parser
{
    public enum T // Tokens
    {
        Unknown, Statement, Semicolon, Assignment, Identifier, AssignmentOperator, StatementEnd,
        DataType, ObjectDefinition, AssignmentValue, Expression, Value, Literal, Integer,
        Operator, OpenParenthesis, CloseParenthesis, BitwiseOperator, LogicalOperator,
        ModuloOperator, ComparisonOperator, DivisionOperator, SubtractionOperator,
        MultplicationOperator, AddictionOperator, EqualToOperator, NotEqualToOperator,
        GreaterThanOperator, LessThanOperator, GreaterOrEqualToOperator, LessOrEqualToOperator,
        AndOperator, OrOperator, BitwiseAndOperator, BitwiseLeftShiftOperator, BitwiseRightShiftOperator,
        BitwiseOrOperator, BitwiseXorOperator, BooleanLiteral, BooleanFalse, BooleanTrue, PrefixOperator,
        PostfixOperator, MinusSign, PlusSign, IncrementOperator, DecrementOperator, Pol
    };

    public class Parser
    {
        public List<string> ErrorList = new List<string>();
        public List<ParseResult> ParsedCode = new List<ParseResult>();
        public string Code = "";
        public int Pos = 0;
        public int Line = 1;
        public int Col = 1;

        public string[] ReservedWords = { "bool", "int", "uint", "byte", "ubyte", "double", "udouble", "float",
            "string", "true", "false", "do", "while", "if", "then", "for", "else", "return" };

        public int Parse(string code)
        {
            Code = code;
            Pos = 0;
            Line = 1;
            Col = 1;

            while (ParseToken(T.Statement)) ;
            return Pos;
        }


        public bool ParseToken(T token, bool tryParse = false)
        {
            ParseWhiteSpace();

            switch (token)
            {
                case T.Statement:
                    if (Pos == Code.Length)
                    {
                        return false;
                    }
                    if (ParseToken(T.Identifier))
                    {
                        if (ParseToken(T.AssignmentOperator))
                        {
                            if (ParseToken(T.AssignmentValue))
                            {
                                if (ParseToken(T.Semicolon))
                                {
                                    return true;
                                }
                                ErrorExpected(token, T.Semicolon);
                                return false;
                            }
                            ErrorExpected(token, T.AssignmentValue);
                            return false;
                        }
                        ErrorExpected(token, T.AssignmentOperator);
                        return false;
                    }
                    ErrorExpected(token, T.Identifier);
                    return false;

                case T.AssignmentValue: return ParseToken(T.Expression);

                case T.Expression:
                    bool dataTypeDefined = false;
                    if (!ParseToken(T.Identifier, true))
                    {
                        dataTypeDefined = ParseToken(T.DataType);
                    }
                    while (ParseToken(T.PrefixOperator)) ;
                    if (ParseToken(T.Value))
                    {
                        while (ParseToken(T.PostfixOperator)) ;
                        if (ParseToken(T.Operator))
                        {
                            if (ParseToken(T.Expression))
                            {
                                return true;
                            }
                            ErrorExpected(token, T.Expression);
                            return false;
                        }
                        return true;
                    }
                    else if (ParseToken(T.OpenParenthesis))
                    {
                        if (ParseToken(T.Expression))
                        {
                            if (ParseToken(T.CloseParenthesis))
                            {
                                while (ParseToken(T.PostfixOperator)) ;
                                return true;
                            }
                            ErrorExpected(token, T.CloseParenthesis);
                            return false;
                        }
                        ErrorExpected(token, T.Expression);
                        return false;
                    }
                    return dataTypeDefined;

                case T.PrefixOperator:
                    return ParseToken(T.IncrementOperator) || ParseToken(T.DecrementOperator) ||
                        ParseToken(T.PlusSign) || ParseToken(T.MinusSign);

                case T.PostfixOperator: return ParseToken(T.IncrementOperator) || ParseToken(T.DecrementOperator);

                case T.Operator:
                    return ParseToken(T.AddictionOperator) || ParseToken(T.SubtractionOperator) ||
                        ParseToken(T.MultplicationOperator) || ParseToken(T.DivisionOperator) ||
                        ParseToken(T.ModuloOperator) || ParseToken(T.LogicalOperator) ||
                        ParseToken(T.BitwiseOperator) || ParseToken(T.ComparisonOperator);

                case T.Value: return ParseToken(T.Literal) || ParseToken(T.Identifier);

                case T.Literal: return ParseToken(T.Integer) || ParseToken(T.BooleanLiteral);

                case T.ComparisonOperator:
                    return ParseToken(T.EqualToOperator) || ParseToken(T.NotEqualToOperator) ||
                        ParseToken(T.GreaterOrEqualToOperator) || ParseToken(T.LessOrEqualToOperator) ||
                        ParseToken(T.GreaterThanOperator) || ParseToken(T.LessThanOperator);

                case T.LogicalOperator: return ParseToken(T.AndOperator) || ParseToken(T.OrOperator);

                case T.BitwiseOperator:
                    return ParseToken(T.BitwiseAndOperator) || ParseToken(T.BitwiseOrOperator) ||
                        ParseToken(T.BitwiseXorOperator) || ParseToken(T.BitwiseLeftShiftOperator) ||
                        ParseToken(T.BitwiseRightShiftOperator);

                case T.BooleanLiteral: return ParseToken(T.BooleanTrue) || ParseToken(T.BooleanFalse);

                case T.BooleanFalse: return ParseTokenText(token, @"false");

                case T.BooleanTrue: return ParseTokenText(token, @"true");

                case T.IncrementOperator: return ParseTokenText(token, @"\+\+");

                case T.DecrementOperator: return ParseTokenText(token, @"--");

                case T.PlusSign: return ParseTokenText(token, @"\+");

                case T.MinusSign: return ParseTokenText(token, @"-");

                case T.EqualToOperator: return ParseTokenText(token, @"==");

                case T.NotEqualToOperator: return ParseTokenText(token, @"!=");

                case T.GreaterThanOperator: return ParseTokenText(token, @">");

                case T.LessThanOperator: return ParseTokenText(token, @"<");

                case T.GreaterOrEqualToOperator: return ParseTokenText(token, @">=");

                case T.LessOrEqualToOperator: return ParseTokenText(token, @"<=");

                case T.AndOperator: return ParseTokenText(token, @"&&");

                case T.OrOperator: return ParseTokenText(token, @"\|\|");

                case T.BitwiseAndOperator: return ParseTokenText(token, @"&");

                case T.BitwiseOrOperator: return ParseTokenText(token, @"\|");

                case T.BitwiseXorOperator: return ParseTokenText(token, @"\^");

                case T.BitwiseLeftShiftOperator: return ParseTokenText(token, @"<<");

                case T.BitwiseRightShiftOperator: return ParseTokenText(token, @">>");

                case T.Integer: return ParseTokenText(token, @"[0-9]+");

                case T.Semicolon: return ParseTokenText(token, @";");

                case T.Identifier: return ParseTokenText(token, @"[_a-zA-Z][_a-zA-Z0-9]*", tryParse, ReservedWords);

                case T.AssignmentOperator: return ParseTokenText(token, @":");

                case T.DataType: return ParseTokenText(token, @"int|uint|byte|ubyte|double|udouble|float|bool|string");

                case T.AddictionOperator: return ParseTokenText(token, @"\+");

                case T.SubtractionOperator: return ParseTokenText(token, @"-");

                case T.MultplicationOperator: return ParseTokenText(token, @"\*");

                case T.DivisionOperator: return ParseTokenText(token, @"\/");

                case T.ModuloOperator: return ParseTokenText(token, @"%");

                case T.OpenParenthesis: return ParseTokenText(token, @"\(");

                case T.CloseParenthesis: return ParseTokenText(token, @"\)");

                case T.Pol:
                    Error(token, "Once you rationalise the first misstep, it's easy to fall into a pattern of behaviour.");
                    break;

                default:
                    Error(token, "Not yet implemented.");
                    break;
            }
            return false;
        }

        public bool ParseTokenText(T token, string regex, bool tryParse = false, string[] disallowedWords = null)
        {
            ParseResult pr = new ParseResult(token, Pos, Line, Col);
            pr.Text = ParseRegex(regex);
            if (pr.Text.Length > 0)
            {
                if (disallowedWords != null && disallowedWords.Contains(pr.Text))
                {
                    if (!tryParse) Error(token, $"'{pr.Text}' is a reserved word.");
                    return false;
                }
                if (!tryParse)
                {
                    Console.WriteLine("DEBUG: (Line: {0}, Col: {1}) Parsed token '{2}', with value '{3}'.", Line, Col, token.ToString(), pr.Text);
                    ParsedCode.Add(pr);
                    Pos += pr.Text.Length;
                    Col += pr.Text.Length;
                }
                return true;
            }
            //Console.WriteLine("DEBUG: Failed to parsed token '{0}'.", token.ToString());
            return false;
        }

        public void ParseWhiteSpace()
        {
            while (Pos < Code.Length && char.IsWhiteSpace(Code[Pos]))
            {
                if (Code[Pos] == '\n')
                {
                    Col = 1;
                    Line++;
                }
                else
                {
                    Col++;
                }
                Pos++;
            }
        }

        public string ParseRegex(string regex)
        {
            return Regex.Match(Code.Substring(Pos), @"^(" + regex + @")").Value;
        }

        public string ConvertCamelCase(string text)
        {
            string ret = "";
            foreach (char c in text)
            {
                if (char.IsUpper(c) && ret.Length > 0)
                {
                    ret += ' ';
                }
                ret += c;
            }
            return ret;
        }

        public string GetTokenName(T token)
        {
            return ConvertCamelCase(token.ToString());
        }

        public void ErrorExpected(T token, T expected = T.Unknown)
        {
            ErrorList.Add(string.Format("(Line {0}, Col {1}) Error while parsing '{2}': expected '{3}'.", Line, Col, GetTokenName(token), GetTokenName(expected)));
        }

        public void Error(T token, string message)
        {
            ErrorList.Add(string.Format("(Line {0}, Col {1}) Error while parsing '{2}': {3}", Line, Col, GetTokenName(token), message));
        }
    }

    public class ParseResult
    {
        public T Token;
        public string Text = "";
        public int Position;
        public int Line;
        public int Column;

        public ParseResult(T token, int pos, int line, int col)
        {
            Token = token;
            Position = pos;
            Line = line;
            Column = col;
        }
    }
}
