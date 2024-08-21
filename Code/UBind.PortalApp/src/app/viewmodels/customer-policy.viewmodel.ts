import { CustomerPolicyStatus, PolicyStatus } from '@app/models/policy-status.enum';
import { PolicyResourceModel } from '@app/resource-models/policy.resource-model';
import { PolicyViewModel } from '@app/viewmodels/policy.viewmodel';

/**
 * Export customer policy view model class.
 * TODO: Write a better class header: view model of customer policy.
 */
export class CustomerPolicyViewModel extends PolicyViewModel {
    public constructor(policy: PolicyResourceModel) {
        super(policy);
        switch (policy.status) {
            case PolicyStatus.Issued:
            case PolicyStatus.Active:
                this.segment = CustomerPolicyStatus.Current;
                break;
            case PolicyStatus.Expired:
            case PolicyStatus.Cancelled:
                this.segment = CustomerPolicyStatus.Inactive;
                break;
        }
    }
}
