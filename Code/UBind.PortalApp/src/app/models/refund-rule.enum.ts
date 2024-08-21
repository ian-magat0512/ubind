export enum RefundRule {
    RefundsAreAlwaysProvided = 0,
    RefundsAreNeverProvided = 1,
    RefundsAreProvidedIfNoClaimsWereMade = 2,
    RefundsCanOptionallyBeProvided = 3,
}
