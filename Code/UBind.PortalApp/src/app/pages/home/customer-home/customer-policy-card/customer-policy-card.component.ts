import { Component, OnInit, Input } from '@angular/core';
import { CardBaseComponent } from '@pages/home/customer-home/card-base.component';
import { PolicyViewModel } from '@app/viewmodels/policy.viewmodel';
import { NavProxyService } from '@app/services/nav-proxy.service';
import { UserTypePathHelper } from '@app/helpers/user-type-path.helper';
import { AuthenticationService } from '@app/services/authentication.service';
import { IconLibrary } from '@app/models/icon-library.enum';

/**
 * Export customer policy card component class
 * TODO: Write a better class header: displaying of customer policy cards.
 */
@Component({
    selector: 'app-customer-policy-card',
    templateUrl: './customer-policy-card.component.html',
    styleUrls: [
        '../customer-home.page.scss',
        '../card-base-component.scss',
    ],
})
export class CustomerPolicyCardComponent extends CardBaseComponent implements OnInit {

    @Input() public policies: Array<PolicyViewModel> = [];

    public isMutual: boolean;
    public iconLibrary: typeof IconLibrary = IconLibrary;

    public constructor(
        protected navProxy: NavProxyService,
        protected userPath: UserTypePathHelper,
        protected authService: AuthenticationService,
    ) {
        super();
    }

    public ngOnInit(): void {
        this.isMutual = this.authService.isMutualTenant();
    }

    public userDidTapPolicy(policy: PolicyViewModel): void {
        this.navProxy.navigateForward([this.userPath.policy, policy.id]);
    }

    public userDidTapInactivePolicies(): void {
        this.navProxy.navigateForward([this.userPath.policy, 'list'], true, { queryParams: { segment: 'Inactive' } });
    }
}
