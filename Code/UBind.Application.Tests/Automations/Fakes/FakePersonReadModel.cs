// <copyright file="FakePersonReadModel.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace UBind.Application.Tests.Automations.Fakes
{
    using UBind.Domain.ReadModel;

    public class FakePersonReadModel : PersonReadModel
    {
        public FakePersonReadModel(Guid personId)
            : base(personId)
        {
        }
    }
}
