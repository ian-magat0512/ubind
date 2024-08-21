
/**
 * What comes back when you load a claim
 */
export interface ClaimApplicationModel {
    claimId: string;
    calculationResultId?: string;
    claimState: string;
    claimVersion?: number;
    formModel: any;
    calculationResult: any;
    workflowStep: string;
    currentUser: any;
}
