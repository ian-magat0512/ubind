/**
 * Model for notification of a change in the state of a claim
 */
export interface ClaimStateChangedModel {
    claimId: string;
    previousClaimState: string;
    newClaimState: string;
}
