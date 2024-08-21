import { Component, Input, OnInit } from '@angular/core';
import { CardBaseComponent } from '../card-base.component';
import { ClaimViewModel } from '@app/viewmodels/claim.viewmodel';
import { NavProxyService } from '../../../../services/nav-proxy.service';
import { UserTypePathHelper } from '../../../../helpers/user-type-path.helper';
import { ClaimService } from '@app/services/claim.service';
import { PolicyViewModel } from '@app/viewmodels/policy.viewmodel';
import { AuthenticationService } from '@app/services/authentication.service';
import { ProductFeatureSettingService } from '@app/services/product-feature-service';
import { IconLibrary } from '@app/models/icon-library.enum';

/**
 * Export customer claim card component class
 * TODO: Write a better class header: displaying of customer claim cards.
 */
@Component({
    selector: 'app-customer-claim-card',
    templateUrl: './customer-claim-card.component.html',
    styleUrls: [
        '../customer-home.page.scss',
        '../card-base-component.scss',
    ],
})
export class CustomerClaimCardComponent extends CardBaseComponent implements OnInit {

    @Input() public claims: Array<ClaimViewModel> = [];
    @Input() public policies: Array<PolicyViewModel> = [];
    public anyProductCanCreateStandaloneClaim: boolean;
    public iconLibrary: typeof IconLibrary = IconLibrary;

    public constructor(
        protected navProxy: NavProxyService,
        protected userPath: UserTypePathHelper,
        protected claimService: ClaimService,
        protected authenticationService: AuthenticationService,
        protected productFeatureSettings: ProductFeatureSettingService,
    ) {
        super();
    }

    public async ngOnInit(): Promise<void> {
        this.anyProductCanCreateStandaloneClaim
        = await this.productFeatureSettings.anyProductCanCreateStandaloneClaim();
    }

    public userDidTapClaim(claim: ClaimViewModel): void {
        this.navProxy.navigate([this.userPath.claim, claim.id]);
    }

    public userDidTapInactiveClaims(): void {
        this.navProxy.navigate([this.userPath.claim, 'list'], { queryParams: { segment: 'inactive' } });
    }

    public userDidTapNewClaim(): void {
        this.claimService.createClaimBySelectingPolicy(this.policies, this.authenticationService.customerId);
    }
}
