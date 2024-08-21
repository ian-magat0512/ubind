import { Component, OnInit, Input } from '@angular/core';
import { NavProxyService } from '@app/services/nav-proxy.service';
import { ClaimResourceModel } from '@app/resource-models/claim.resource-model';
import { UserTypePathHelper } from '@app/helpers/user-type-path.helper';
import { ClaimApiService } from '@app/services/api/claim-api.service';
import { RouteHelper } from '@app/helpers/route.helper';

/**
 * Renders a list of claims for a given policy.
 */
@Component({
    selector: 'app-claim-policy-view',
    templateUrl: './claim-policy-view.component.html',
})
export class ClaimPolicyViewComponent implements OnInit {

    @Input() public policyId: string;
    public claimsData: Array<ClaimResourceModel>;

    public constructor(
private navProxy: NavProxyService,
        private claimApiService: ClaimApiService,
        protected userPath: UserTypePathHelper,
        protected routeHelper: RouteHelper,
    ) { }

    public ngOnInit(): void {
        this.claimApiService.getByPolicyId(this.policyId).subscribe((claims: Array<ClaimResourceModel>) => {
            this.claimsData = claims;
        });
    }

    public goToClaimsPage(claim: ClaimResourceModel): void {
        if (this.policyId) {
            let pathSegments: Array<string> = this.routeHelper.getPathSegments();
            pathSegments.pop();
            pathSegments.pop();
            pathSegments.push(this.userPath.claim, claim.id);
            this.navProxy.navigateForward(pathSegments);
            return;
        }
        this.navProxy.navigate([this.userPath.claim, claim.id], null);
    }
}
