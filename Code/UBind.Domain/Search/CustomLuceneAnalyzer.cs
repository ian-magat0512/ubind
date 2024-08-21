// <copyright file="CustomLuceneAnalyzer.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Search;

using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Core;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Util;

/// <summary>
/// This was a custom analyzer to search the text with dash since in the standard analyzer it was not possible to search the text with dash.
/// It get ignored and the search was being multiple terms instead of single term.
/// </summary>
public class CustomLuceneAnalyzer : Analyzer
{
    protected override TokenStreamComponents CreateComponents(string fieldName, TextReader reader)
    {
        var tokenizer = new ClassicTokenizer(LuceneVersion.LUCENE_48, reader);
        var filter = new LowerCaseFilter(LuceneVersion.LUCENE_48, tokenizer);
        return new TokenStreamComponents(tokenizer, filter);
    }
}
