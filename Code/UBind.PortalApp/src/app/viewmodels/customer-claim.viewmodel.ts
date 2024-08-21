import { ClaimResourceModel } from "../resource-models/claim.resource-model";
import { ClaimViewModel } from "./claim.viewmodel";
import { ClaimHelper } from "../helpers";

/**
 * Export customer claim view model class.
 * TODO: Write a better class header: view model of customer claim.
 */
export class CustomerClaimViewModel extends ClaimViewModel {
    public constructor(claim: ClaimResourceModel) {
        super(claim);
        switch (claim.status.toLowerCase()) {
            case ClaimHelper.status.Incomplete.toLowerCase():
            case ClaimHelper.status.Notified.toLowerCase():
            case ClaimHelper.status.Acknowledged.toLowerCase():
            case ClaimHelper.status.Review.toLowerCase():
            case ClaimHelper.status.Assessment.toLowerCase():
            case ClaimHelper.status.Approved.toLowerCase():
                this.segment = 'Active';
                break;
            case ClaimHelper.status.Complete.toLowerCase():
            case ClaimHelper.status.Withdrawn.toLowerCase():
            case ClaimHelper.status.Declined.toLowerCase():
                this.segment = 'Inactive';
                break;
        }
    }
}
