// <copyright file="GetVehicleByVehicleKeyQueryHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Queries.ThirdPartyDataSets.RedBook
{
    using System.Threading;
    using System.Threading.Tasks;
    using UBind.Domain;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Repositories;
    using UBind.Domain.ThirdPartyDataSets.RedBook;

    public class GetVehicleByVehicleKeyQueryHandler : IQueryHandler<GetVehicleByVehicleKeyQuery, VehicleDetails>
    {
        private readonly IRedBookRepository redBookRepository;

        public GetVehicleByVehicleKeyQueryHandler(IRedBookRepository redBookRepository)
        {
            this.redBookRepository = redBookRepository;
        }

        /// <inheritdoc/>
        public async Task<VehicleDetails> Handle(GetVehicleByVehicleKeyQuery request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var vehicleResult = await this.redBookRepository.GetVehicleByVehicleKeyAsync(request.VehicleKey);

            if (vehicleResult == null)
            {
                throw new ErrorException(Errors.ThirdPartyDataSets.RedBook.VehicleKeyNotFound(request.VehicleKey));
            }

            return vehicleResult;
        }
    }
}
