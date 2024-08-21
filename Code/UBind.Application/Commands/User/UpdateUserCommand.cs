// <copyright file="UpdateUserCommand.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.User
{
    using UBind.Application.User;
    using UBind.Domain.Aggregates.AdditionalPropertyValue;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.ReadModel.User;

    [CreateTransactionThatSavesChangesIfNoneExists]
    public class UpdateUserCommand : ICommand<UserReadModel>
    {
        public UpdateUserCommand(
            Guid tenantId,
            Guid userId,
            UserUpdateModel userUpdateModel,
            List<AdditionalPropertyValueUpsertModel> properties = null)
        {
            this.TenantId = tenantId;
            this.UserId = userId;
            this.UserUpdateModel = userUpdateModel;
            this.Properties = properties;
        }

        public Guid TenantId { get; }

        public Guid UserId { get; }

        public UserUpdateModel UserUpdateModel { get; }

        public List<AdditionalPropertyValueUpsertModel> Properties { get; }
    }
}
