﻿using System.Collections.Generic;

namespace LingoEngine.Lingo.Core.Tokenizer
{
  /// <summary>
    /// Represents a Lingo handler (method or event).
    /// </summary>
    public class LingoHandler
    {
        public LingoScript Script { get; set; } = null!;
        public string Name { get; set; } = string.Empty;
        public List<string> ArgumentNames { get; set; } = new();
        public List<string> GlobalNames { get; set; } = new();
        public bool IsGenericEvent { get; set; }
        public bool IsMethod { get; internal set; }

        /// <summary>
        /// The abstract syntax tree (AST) of this handler, if parsed.
        /// </summary>
        public HandlerAst? Ast { get; set; }

        public void Parse()
        {
            var parser = new LingoAstParser();
            Ast = new HandlerAst(parser.Parse(Script.Source));
        }

    }

    /// <summary>
    /// Wraps the root AST node for a handler.
    /// </summary>
    public class HandlerAst
    {
        public LingoNode? Root { get; set; }

        public HandlerAst(LingoNode? root)
        {
            Root = root;
        }
    }

}


