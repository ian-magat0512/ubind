import { Component, OnInit, Input } from '@angular/core';
import { PolicyViewModel } from '@app/viewmodels/policy.viewmodel';
import { CardBaseComponent } from '@pages/home/customer-home/card-base.component';
import { PolicyService } from '@app/services/policy.service';
import { AuthenticationService } from '@app/services/authentication.service';

/**
 * Export customer policy for renewal card component class
 * TODO: Write a better class header: Renewal of customer policy card.
 */
@Component({
    selector: 'app-customer-policy-for-renewal-card',
    templateUrl: './customer-policy-for-renewal-card.component.html',
    styleUrls: [
        './customer-policy-for-renewal-card.component.scss',
        '../card-base-component.scss',
    ],
})
export class CustomerPolicyForRenewalCardComponent extends CardBaseComponent implements OnInit {
    @Input() public policiesForRenewal: Array<PolicyViewModel> = [];
    public policyBeingRenewed: PolicyViewModel;

    public isMutual: boolean;

    public constructor(
        protected policyService: PolicyService,
        protected authService: AuthenticationService,
    ) {
        super();
    }

    public ngOnInit(): void {
        this.isMutual = this.authService.isMutualTenant();
    }

    public userDidTapRenewPolicy(policyViewModel: PolicyViewModel): void {
        this.policyService.renewPolicy(policyViewModel.id);
    }
}
