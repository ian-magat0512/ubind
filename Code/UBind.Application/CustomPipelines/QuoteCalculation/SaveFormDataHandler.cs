// <copyright file="SaveFormDataHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.CustomPipelines.QuoteCalculation
{
    using System.Threading;
    using System.Threading.Tasks;
    using MediatR;
    using Newtonsoft.Json.Linq;
    using NodaTime;
    using UBind.Application.Commands.QuoteCalculation;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Quote;
    using UBind.Domain.Extensions;
    using UBind.Domain.ReadWriteModel;

    public class SaveFormDataHandler<TRequest, TResponse>
        : IPipelineBehavior<QuoteCalculationCommand, CalculationResponseModel>
        where TRequest : QuoteCalculationCommand
    {
        private readonly IHttpContextPropertiesResolver httpContextPropertiesResolver;
        private readonly IClock clock;

        public SaveFormDataHandler(
            IHttpContextPropertiesResolver httpContextPropertiesResolver,
            IClock clock)
        {
            this.httpContextPropertiesResolver = httpContextPropertiesResolver;
            this.clock = clock;
        }

        public async Task<CalculationResponseModel> Handle(
            QuoteCalculationCommand request,
            RequestHandlerDelegate<CalculationResponseModel> next,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var formDataModel = new
            {
                formModel = request.CalculationDataModel.FormModel,
                additionalFormData = request.AdditionalFormData,
            };
            request.FinalFormData = new FormData(JObject.FromObject(formDataModel));

            if (request.Quote != null && request.PersistResults)
            {
                request.FormDataUpdateId = request.Quote.UpdateFormData(
                    request.FinalFormData, this.httpContextPropertiesResolver.PerformingUserId, this.clock.Now());
            }

            return await next();
        }
    }
}
