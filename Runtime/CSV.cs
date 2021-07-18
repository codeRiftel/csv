using System.Collections.Generic;
using System.Text;
using System;

public static class CSV {
    public enum ErrorType {
        None,
        HangingValue,
        QuoteInUnquoted
    }

    public struct ParseRes {
        public ErrorType error;
        public List<List<string>> rows;
    }

    private enum TokenType {
        None,
        Value,
        QuotedValue,
        Sep,
        NewLine
    }

    private struct Token {
        public TokenType type;
        public int start;
        public int length;
    }

    private struct LexRes {
        public ErrorType error;
        public Token token;

        public static LexRes Token(Token token) {
            LexRes res = new LexRes();
            res.token = token;
            return res;
        }

        public static LexRes Error(ErrorType type) {
            LexRes res = new LexRes();
            res.error = type;
            return res;
        }
    }

    public static ParseRes Parse(string data) {
        ParseRes res = new ParseRes();
        List<List<string>> rows = new List<List<string>>();
        List<string> row = new List<string>();

        int pos = 0;
        TokenType prevTokenType = TokenType.None;
        while (pos < data.Length) {
            LexRes lexRes = Lex(data, pos);
            if (lexRes.error != ErrorType.None) {
                res.error = lexRes.error;
                return res;
            } else {
                Token token = lexRes.token;
                pos = token.start + token.length;
                if (token.type == TokenType.QuotedValue) {
                    StringBuilder builder = new StringBuilder();
                    int end = pos;
                    for (int i = token.start + 1; i < end; i++) {
                        if (data[i] == '"') {
                            if (i + 1 < end && data[i] == '"') {
                                builder.Append('"');
                                i++;
                            }
                        } else {
                            builder.Append(data[i]);
                        }
                    }
                    row.Add(builder.ToString());
                } else if (token.type == TokenType.Value) {
                    string tokenVal = data.Substring(token.start, token.length);
                    row.Add(tokenVal);
                } else if (token.type == TokenType.NewLine) {
                    if (prevTokenType == TokenType.Sep) {
                        row.Add(string.Empty);
                    }

                    rows.Add(row);
                    row = new List<string>();
                } else if (token.type == TokenType.Sep) {
                    bool isPrevNotVal = prevTokenType != TokenType.Value;
                    isPrevNotVal = isPrevNotVal && prevTokenType != TokenType.QuotedValue;
                    if (isPrevNotVal) {
                        row.Add(string.Empty);
                    }
                }

                if (pos >= data.Length && row.Count > 0) {
                    rows.Add(row);
                }

                prevTokenType = token.type;
            }
        }

        if (prevTokenType == TokenType.Sep) {
            rows[rows.Count - 1].Add(string.Empty);
        }

        res.rows = rows;
        return res;
    }

    public static string Generate(List<List<string>> rows) {
        StringBuilder builder = new StringBuilder();
        foreach (List<string> row in rows) {
            for (int i = 0; i < row.Count; i++) {
                string val = row[i];

                bool quote = val.Contains(",") || val.Contains("\"") || val.Contains("\r\n");
                if (quote) {
                    builder.Append('"');
                }

                for (int j = 0; j < val.Length; j++) {
                    builder.Append(val[j]);

                    if (val[j] == '"') {
                        builder.Append('"');
                    }
                }

                if (quote) {
                    builder.Append('"');
                }

                if (i != row.Count - 1) {
                    builder.Append(',');
                }
            }

            builder.Append("\r\n");
        }

        return builder.ToString();
    }

    private static LexRes Lex(string data, int pos) {
        switch (data[pos]) {
            case ',':
                Token comma = new Token();
                comma.type = TokenType.Sep;
                comma.start = pos;
                comma.length = 1;
                return LexRes.Token(comma);
            case '\r':
                if (pos + 1 == data.Length) {
                    Token val = new Token();
                    val.type = TokenType.Value;
                    val.start = pos;
                    val.length = 1;
                    return LexRes.Token(val);
                } else if (data[pos + 1] == '\n') {
                    Token val = new Token();
                    val.type = TokenType.NewLine;
                    val.start = pos;
                    val.length = 2;
                    return LexRes.Token(val);
                } else {
                    return LexValue(data, pos);
                }
            case '"':
                int startQuoted = pos;
                pos++;
                while (true) {
                    if (pos == data.Length) {
                        return LexRes.Error(ErrorType.HangingValue);
                    }

                    if (data[pos] == '"') {
                        pos++;
                        if (pos == data.Length || data[pos] != '"') {
                            break;
                        }
                    }

                    pos++;
                }

                Token quoted = new Token();
                quoted.type = TokenType.QuotedValue;
                quoted.start = startQuoted;
                quoted.length = pos - startQuoted;
                return LexRes.Token(quoted);
            default:
                return LexValue(data, pos);
        }
    }

    private static LexRes LexValue(string data, int pos) {
        int start = pos;
        while (pos < data.Length && data[pos] != ',' && data[pos] != '\r') {
            if (data[pos] == '"') {
                return LexRes.Error(ErrorType.QuoteInUnquoted);
            }
            pos++;
        }

        Token val = new Token();
        val.type = TokenType.Value;
        val.start = start;
        val.length = pos - start;
        return LexRes.Token(val);
    }
}
