// <copyright file="DeletePolicyCommand.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.Policy;

using MediatR;
using System.Security.Claims;
using UBind.Domain.Enums;
using UBind.Domain.Patterns.Cqrs;

/// <summary>
/// The command to delete a policy as well as its associcated claims, quotes, customers, etc.
/// </summary>
public class DeletePolicyCommand : ICommand<Unit>
{
    public DeletePolicyCommand(
        ClaimsPrincipal performingUser,
        Guid policyId,
        bool deleteOrphanedCustomers,
        DeletedPolicyClaimsActionType associatedClaimAction,
        bool reusePolicyNumber,
        bool reuseClaimNumbers)
    {
        this.PerformingUser = performingUser;
        this.PolicyId = policyId;
        this.DeleteOrphanedCustomers = deleteOrphanedCustomers;
        this.AssociatedClaimAction = associatedClaimAction;
        this.ReusePolicyNumber = reusePolicyNumber;
        this.ReuseClaimNumbers = reuseClaimNumbers;
    }

    /// <summary>
    /// The performing user.
    /// </summary>
    public ClaimsPrincipal PerformingUser { get; }

    /// <summary>
    /// The ID of the policy that will be deleted.
    /// </summary>
    public Guid PolicyId { get; }

    /// <summary>
    /// Indicates whether to delete customers that will no longer have associated quote, policy, and claim.
    /// </summary>
    public bool DeleteOrphanedCustomers { get; }

    /// <summary>
    /// The action to be performed on claims associated with the deleted policy.
    /// </summary>
    public DeletedPolicyClaimsActionType AssociatedClaimAction { get; }

    /// <summary>
    /// Indicates whether the policy number associated with the deleted policy should be added to the policy number pool.
    /// </summary>
    public bool ReusePolicyNumber { get; }

    /// <summary>
    /// Indicates whether the claim number associated with deleted claims should be added to the claim number pool.
    /// </summary>
    public bool ReuseClaimNumbers { get; }
}
