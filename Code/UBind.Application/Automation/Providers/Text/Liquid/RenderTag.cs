// <copyright file="RenderTag.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Providers.Liquid
{
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using DotLiquid;
    using UBind.Domain.Helpers;

    /// <summary>
    /// Creates a render tag that you can use in templates to render snippets from.
    /// The markup will then be overriden by a snippet.
    /// Expects a markup that is of snippetAlias value.
    /// </summary>
    public class RenderTag : Tag
    {
        private string alias;

        private Dictionary<string, string> localVariables = new Dictionary<string, string>();

        public override void Initialize(string tagName, string markup, List<string> tokens)
        {
            base.Initialize(tagName, markup, tokens);
            var markupSplit = markup.Split(',');
            if (markupSplit.Length > 0)
            {
                this.alias = markupSplit[0].Trim(' ', '\'', '\"');
            }

            foreach (var split in markupSplit.Skip(1))
            {
                var valueSplit = split.Split(':');
                this.localVariables.Add(valueSplit[0].Trim(), valueSplit[1].Trim());
            }
        }

        public override void Render(Context context, TextWriter result)
        {
            var registers = context.Registers.ToList();
            KeyValuePair<string, object> kvPairSnippet = registers.FirstOrDefault(x => x.Key == "snippet:" + this.alias);
            if (kvPairSnippet.Value == null)
            {
                result.Write(this.Markup);
            }

            Dictionary<string, object> localVariableDictionary = new Dictionary<string, object>();
            if (this.localVariables.Any())
            {
                foreach (var localVariable in this.localVariables)
                {
                    var scopeList = context.Scopes.ToList().SelectMany(x => x.ToDictionary(o => o.Key, o => o.Value));
                    var scope = scopeList.FirstOrDefault(x => x.Key == localVariable.Value?.ToString());
                    if (scope.Value != null)
                    {
                        localVariableDictionary.Add(localVariable.Key, scope.Value);
                    }
                    else
                    {
                        localVariableDictionary.Add(localVariable.Key, localVariable.Value);
                    }
                }

                context.Push(Hash.FromDictionary(localVariableDictionary));
            }

            var template = Template.Parse(kvPairSnippet.Value.ToString());
            var param = new RenderParameters(CultureInfo.GetCultureInfo(Locales.en_AU));
            param.Context = context;
            param.Registers = context.Registers;
            var render = template.Render(param);
            result.Write(render);
        }
    }
}
