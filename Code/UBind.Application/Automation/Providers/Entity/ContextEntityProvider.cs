// <copyright file="ContextEntityProvider.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Automation.Providers.Entity
{
    using MorseCode.ITask;
    using UBind.Application.Automation.Extensions;
    using UBind.Domain;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Helpers;
    using UBind.Domain.SerialisedEntitySchemaObject;

    /// <summary>
    /// This class is needed because it reads an entity reference with the specified path from from the automation context and returns it.
    /// </summary>
    public class ContextEntityProvider : StaticEntityProvider
    {
        private readonly IProvider<Data<string>> pathProvider;

        public ContextEntityProvider(IProvider<Data<string>> pathProvider, ISerialisedEntityFactory serialisedEntityFactory)
            : base((string?)null, serialisedEntityFactory, "contextEntity")
        {
            this.pathProvider = pathProvider;
        }

        // <summary>
        // Method for retrieving the entity from the context.
        // </summary>
        // <param name="automationData">The automation data.</param>
        // <returns>The entity object.</returns>
        public override async ITask<IProviderResult<Data<IEntity>>> Resolve(IProviderContext providerContext)
        {
            string path = (await this.pathProvider.Resolve(providerContext)).GetValueOrThrowIfFailed().DataValue;
            if (!PathHelper.IsJsonPointer(path))
            {
                path = PathHelper.ToJsonPointer(path);
            }

            var contextPath = "/context" + path;
            var entity = await providerContext.AutomationData.GetValue<IEntity>(contextPath, providerContext);
            if (entity == null)
            {
                var errorData = await providerContext.GetDebugContextForProviders(this.SchemaReferenceKey);
                errorData.Add("path", contextPath);
                throw new ErrorException(Errors.Automation.Provider.Entity.EntityNotInContext(contextPath, errorData));
            }

            return ProviderResult<Data<IEntity>>.Success(new Data<IEntity>(entity));
        }
    }
}
