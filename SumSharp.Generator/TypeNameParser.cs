using System;
using System.Collections.Generic;
using System.Linq;

namespace SumSharp.Generator;
public static class TypeNameParser
{
    public static List<string> ExtractLeafTypes(string text)
    {
        var parser = new Parser(text);
        var result = new List<string>();

        parser.ParseType(result);

        return result;
    }

    private sealed class Parser
    {
        private readonly string _text;
        private int _pos;

        public Parser(string text)
        {
            _text = text;
        }

        public void ParseType(List<string> output)
        {
            SkipWhitespace();

            if (Peek() == '(')
            {
                ParseTuple(output);
                return;
            }

            string identifier = ParseIdentifier();

            SkipWhitespace();

            // Generic?
            if (Peek() == '<')
            {
                Consume('<');

                while (true)
                {
                    ParseType(output);

                    SkipWhitespace();

                    if (Peek() == ',')
                    {
                        Consume(',');
                        continue;
                    }

                    Consume('>');
                    break;
                }
            }
            else
            {
                output.Add(identifier);
            }

            // Ignore array suffixes
            while (true)
            {
                SkipWhitespace();

                if (Peek() != '[')
                    break;

                Consume('[');

                while (Peek() != ']')
                    _pos++;

                Consume(']');
            }

            // Optional nullable suffix
            if (Peek() == '?')
                Consume('?');
        }

        private void ParseTuple(List<string> output)
        {
            Consume('(');

            while (true)
            {
                ParseType(output);

                SkipWhitespace();

                // Skip tuple field name if present
                if (char.IsLetter(Peek()) || Peek() == '_')
                {
                    ParseIdentifier();
                }

                SkipWhitespace();

                if (Peek() == ',')
                {
                    Consume(',');
                    continue;
                }

                Consume(')');
                break;
            }
        }

        private string ParseIdentifier()
        {
            SkipWhitespace();

            int start = _pos;

            while (_pos < _text.Length)
            {
                char c = _text[_pos];

                if (char.IsLetterOrDigit(c) || c == '_' || c == '.')
                {
                    _pos++;
                }
                else
                {
                    break;
                }
            }

            return _text.Substring(start, _pos - start);
        }

        private void SkipWhitespace()
        {
            while (_pos < _text.Length && char.IsWhiteSpace(_text[_pos]))
                _pos++;
        }

        private char Peek()
        {
            return _pos < _text.Length ? _text[_pos] : '\0';
        }

        private void Consume(char c)
        {
            if (Peek() != c)
                throw new FormatException($"Expected '{c}'.");

            _pos++;
        }
    }
}