using System;
using System.Collections.Generic;
using System.Linq;

namespace SumSharp.Generator;
public static class TypeNameParser
{
    public static List<string> ExtractLeafTypes(string typeName)
    {
        var parser = new Parser(typeName);
        var result = new List<string>();

        parser.ParseType(result);

        return result;
    }

    private sealed class Parser(string typeName)
    {
        private int _pos = 0;

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

            while (_pos < typeName.Length)
            {
                char c = typeName[_pos];

                if (char.IsLetterOrDigit(c) || c == '_' || c == '.')
                {
                    _pos++;
                }
                else
                {
                    break;
                }
            }

            return typeName.Substring(start, _pos - start);
        }

        private void SkipWhitespace()
        {
            while (_pos < typeName.Length && char.IsWhiteSpace(typeName[_pos]))
                _pos++;
        }

        private char Peek()
        {
            return _pos < typeName.Length ? typeName[_pos] : '\0';
        }

        private void Consume(char c)
        {
            if (Peek() != c)
                throw new FormatException($"Expected '{c}'.");

            _pos++;
        }
    }
}