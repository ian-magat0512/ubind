// <copyright file="HttpTrigger.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Triggers
{
    using System.Threading.Tasks;
    using ServiceStack;
    using UBind.Application.Automation.Data;
    using UBind.Application.Automation.Enums;
    using UBind.Application.Automation.Http;
    using UBind.Application.Automation.Providers;
    using UBind.Domain.Exceptions;
    using Path = UBind.Domain.ValueTypes.Path;

    /// <summary>
    /// Represents a trigger of type HttpTrigger.
    /// </summary>
    public class HttpTrigger : Trigger
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HttpTrigger"/> class.
        /// </summary>
        /// <param name="name">The name of the trigger.</param>
        /// <param name="alias">The alias of the trigger.</param>
        /// <param name="description">The trigger description.</param>
        /// <param name="runConditionProvider">The run condition, if any.</param>
        /// <param name="endpoint">The endpoint to be used.</param>
        /// <param name="context">The context to be used.</param>
        /// <param name="httpResponse">The response to be used.</param>
        public HttpTrigger(
            string name,
            string alias,
            string description,
            IProvider<Data<bool>> runConditionProvider,
            TriggerRequestEndpoint endpoint,
            IProvider<Data<string>> context,
            HttpResponse httpResponse)
            : base(name, alias, description)
        {
            this.Endpoint = endpoint;
            this.Context = context;
            this.HttpResponse = httpResponse;
        }

        /// <summary>
        /// Gets the request endpoint configuration for the trigger.
        /// </summary>
        public TriggerRequestEndpoint Endpoint { get; }

        /// <summary>
        /// Gets the score that was calculated for this trigger if it matched the endpoint.
        /// A higher score means a better match.
        /// </summary>
        public int MatchScore { get; private set; } = -1;

        /// <summary>
        /// Gets the contet of the trigger.
        /// </summary>
        public IProvider<Data<string>> Context { get; }

        /// <summary>
        /// Gets the http response to be returned by the trigger.
        /// </summary>
        public HttpResponse HttpResponse { get; }

        /// <inheritdoc/>
        public override async Task<bool> DoesMatch(AutomationData automationData)
        {
            var providerContext = new ProviderContext(automationData);
            if (providerContext.AutomationData.Trigger.Type != TriggerType.HttpTrigger)
            {
                return false;
            }

            // First check that the http verb matches
            var triggerData = automationData.Trigger as HttpTriggerData;
            var dataRequestVerb = triggerData.HttpRequest.HttpVerb.ToString();
            if (!this.Endpoint.HttpVerb.EqualsIgnoreCase(dataRequestVerb))
            {
                return false;
            }

            // if the path is not the same length, it can't be a match
            var endpointPath = new Path(this.Endpoint.Path);
            var endpointPathSegments = endpointPath.Segments;
            var fullPathSegments = triggerData.HttpRequest.PathSegments;
            var actionPathSegments = this.GetActionPathSegments(fullPathSegments);
            if (endpointPathSegments.Length != actionPathSegments.Length)
            {
                return false;
            }

            // the fixed path segments must match
            var matchScore = 0;
            for (int i = 0; i < endpointPathSegments.Length; i++)
            {
                if (!this.IsParameter(endpointPathSegments[i]))
                {
                    if (endpointPathSegments[i] != actionPathSegments[i])
                    {
                        return false;
                    }

                    // the fixed path segment matched, so increase the match score
                    matchScore++;
                }
            }

            // it's potentially a match, but there might be multiple matches so the caller will have to determine the
            // closest match using the match score
            this.MatchScore = matchScore;
            return await Task.FromResult(true);
        }

        public override async Task GenerateCompletionResponse(IProviderContext providerContext)
        {
            var automationData = providerContext.AutomationData;
            try
            {
                var response = await this.HttpResponse.GenerateResponseAsync(providerContext);
                ((HttpTriggerData)automationData.Trigger).HttpResponse = response;
            }
            catch (System.Exception ex) when (!(ex is ErrorException))
            {
                var errorData = await providerContext.GetDebugContext();
                throw new ErrorException(
                    UBind.Domain.Errors.Automation.HttpRequest.HttpResponseGenerationError(
                        ex.Message, errorData), ex);
            }
        }

        private bool IsParameter(string segment)
        {
            return segment.StartsWith("{") && segment.EndsWith("}");
        }

        /// <summary>
        /// Gets the path segments after the "automations" or "automation" segment.
        /// </summary>
        /// <returns>An array of the path segments.</returns>
        private string[] GetActionPathSegments(string[] fullPathSegments)
        {
            int startIndex = Array.FindIndex(fullPathSegments, v => v == "automations" || v == "automation") + 1;
            return fullPathSegments.Skip(startIndex).ToArray();
        }
    }
}
