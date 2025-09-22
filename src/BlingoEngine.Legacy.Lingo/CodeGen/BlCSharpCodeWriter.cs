using System;
using System.Collections.Generic;
using System.Text;

namespace BlingoEngine.Legacy.Lingo.CodeGen;

/// <summary>
/// Provides helpers to generate C# source text with deterministic indentation.
/// </summary>
public sealed class BlCSharpCodeWriter
{
    private readonly StringBuilder _builder = new();
    private readonly string _lineEnding;
    private readonly string _indentation;
    private int _indentationLevel;
    private bool _requiresIndentation = true;
    private int _lineLength;

    /// <summary>
    /// Gets the number of indentation levels currently applied.
    /// </summary>
    public int IndentationLevel => _indentationLevel;

    /// <summary>
    /// Gets the number of characters written on the current line.
    /// </summary>
    public int LineLength => _lineLength;

    /// <summary>
    /// Gets a value indicating whether the writer is positioned at the beginning of a line.
    /// </summary>
    public bool IsAtLineStart => _requiresIndentation;

    /// <summary>
    /// Gets the total number of characters written to the buffer.
    /// </summary>
    public int Length => _builder.Length;

    /// <summary>
    /// Initializes a new <see cref="BlCSharpCodeWriter"/> instance.
    /// </summary>
    /// <param name="lineEnding">The line ending to use when emitting new lines. Defaults to <see cref="Environment.NewLine"/>.</param>
    /// <param name="indentation">The indentation string used for each indentation level. Defaults to four spaces.</param>
    public BlCSharpCodeWriter(string? lineEnding = null, string? indentation = null)
    {
        _lineEnding = lineEnding ?? Environment.NewLine;
        _indentation = string.IsNullOrEmpty(indentation) ? "    " : indentation;
    }

    /// <summary>
    /// Clears the underlying buffer and resets indentation state.
    /// </summary>
    public void Clear()
    {
        _builder.Clear();
        _indentationLevel = 0;
        _requiresIndentation = true;
        _lineLength = 0;
    }

    /// <summary>
    /// Writes the specified text without appending a trailing newline.
    /// </summary>
    public void Write(string? text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return;
        }

        EnsureIndentation();
        _builder.Append(text);
        _lineLength += text.Length;
    }

    /// <summary>
    /// Writes a single character without appending a trailing newline.
    /// </summary>
    public void Write(char ch)
    {
        EnsureIndentation();
        _builder.Append(ch);
        _lineLength++;
    }

    /// <summary>
    /// Writes the specified text followed by the configured line ending.
    /// </summary>
    public void WriteLine(string? text)
    {
        if (!string.IsNullOrEmpty(text))
        {
            Write(text);
        }

        _builder.Append(_lineEnding);
        _requiresIndentation = true;
        _lineLength = 0;
    }

    /// <summary>
    /// Writes an empty line.
    /// </summary>
    public void WriteLine() => WriteLine(null);

    /// <summary>
    /// Writes a delimited list of values using the provided callback for each item.
    /// </summary>
    /// <typeparam name="T">The type of the elements to emit.</typeparam>
    /// <param name="values">The elements to write.</param>
    /// <param name="writer">Callback invoked for each element.</param>
    /// <param name="separator">The separator inserted between elements. Defaults to ", ".</param>
    public void WriteSeparated<T>(IEnumerable<T> values, Action<BlCSharpCodeWriter, T> writer, string separator = ", ")
    {
        if (values is null)
        {
            throw new ArgumentNullException(nameof(values));
        }

        if (writer is null)
        {
            throw new ArgumentNullException(nameof(writer));
        }

        using var enumerator = values.GetEnumerator();
        if (!enumerator.MoveNext())
        {
            return;
        }

        writer(this, enumerator.Current);

        while (enumerator.MoveNext())
        {
            Write(separator);
            writer(this, enumerator.Current);
        }
    }

    /// <summary>
    /// Increases the indentation level for subsequent writes.
    /// </summary>
    public void Indent() => _indentationLevel++;

    /// <summary>
    /// Decreases the indentation level for subsequent writes.
    /// </summary>
    public void Unindent()
    {
        if (_indentationLevel == 0)
        {
            return;
        }

        _indentationLevel--;
    }

    /// <summary>
    /// Creates a scope that increases indentation while the returned object is alive.
    /// </summary>
    public IDisposable IndentScope()
    {
        Indent();
        return new IndentationScope(this);
    }

    /// <summary>
    /// Writes a code block using braces, indenting the body produced by the provided callback.
    /// </summary>
    /// <param name="header">Optional header written before the opening brace.</param>
    /// <param name="body">Callback responsible for writing the content inside the block.</param>
    public void WriteBlock(string? header, Action<BlCSharpCodeWriter> body)
    {
        if (body is null)
        {
            throw new ArgumentNullException(nameof(body));
        }

        if (!string.IsNullOrEmpty(header))
        {
            WriteLine(header);
        }

        WriteLine("{");
        using (IndentScope())
        {
            body(this);
        }

        WriteLine("}");
    }

    /// <inheritdoc />
    public override string ToString() => _builder.ToString();

    private void EnsureIndentation()
    {
        if (!_requiresIndentation)
        {
            return;
        }

        for (var i = 0; i < _indentationLevel; i++)
        {
            _builder.Append(_indentation);
        }

        _requiresIndentation = false;
        _lineLength = _indentationLevel * _indentation.Length;
    }

    private sealed class IndentationScope : IDisposable
    {
        private readonly BlCSharpCodeWriter _writer;
        private bool _disposed;

        public IndentationScope(BlCSharpCodeWriter writer)
        {
            _writer = writer;
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _writer.Unindent();
            _disposed = true;
        }
    }
}
