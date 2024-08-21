import { Component, OnInit, OnDestroy, ViewChild } from '@angular/core';
import { contentAnimation } from '@assets/animations';
import { NavProxyService } from '@app/services/nav-proxy.service';
import { ClaimApiService } from '@app/services/api/claim-api.service';
import { scrollbarStyle } from '@assets/scrollbar';
import { LayoutManagerService } from '@app/services/layout-manager.service';
import { ClaimViewModel } from '@app/viewmodels/claim.viewmodel';
import { EntityListComponent } from '@app/components/entity-list/entity-list.component';
import { ClaimResourceModel } from '@app/resource-models/claim.resource-model';
import { RouteHelper } from '@app/helpers/route.helper';
import { EntityViewModel } from '@app/viewmodels/entity.viewmodel';
import { UserTypePathHelper } from '@app/helpers/user-type-path.helper';
import { PolicyDetailResourceModel } from '@app/resource-models/policy.resource-model';
import { PolicyApiService } from '@app/services/api/policy-api.service';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { IconLibrary } from '@app/models/icon-library.enum';

/**
 * This class displays the policy claim on the list.
 */
@Component({
    selector: 'app-list-policy-claim',
    templateUrl: './list-policy-claim.page.html',
    animations: [contentAnimation],
    styleUrls: [
        '../../../../assets/css/scrollbar-segment.css',
    ],
    styles: [scrollbarStyle],
})
export class ListPolicyClaimPage implements OnInit, OnDestroy {

    @ViewChild(EntityListComponent, { static: true })
    public listComponent: EntityListComponent<ClaimViewModel, ClaimResourceModel>;
    public title: string = 'Policy Claims';
    public viewModelConstructor: typeof ClaimViewModel = ClaimViewModel;
    private policyId: string;
    private destroyed: Subject<void> = new Subject<void>();
    public iconLibrary: typeof IconLibrary = IconLibrary;

    public constructor(
        private routeHelper: RouteHelper,
        public navProxy: NavProxyService,
        public claimApiService: ClaimApiService,
        public layoutManager: LayoutManagerService,
        private userPath: UserTypePathHelper,
        private policyApiService: PolicyApiService,
    ) {
    }

    public ngOnInit(): void {
        this.destroyed = new Subject<void>();
        this.policyId = this.routeHelper.getParam('policyId');
        this.policyApiService.getPolicyBaseDetails(this.policyId)
            .pipe(takeUntil(this.destroyed))
            .subscribe((dt: PolicyDetailResourceModel) => {
                this.title = 'Claims for ' + dt.policyNumber;
            });
    }

    public ngOnDestroy(): void {
        if (this.destroyed) {
            this.destroyed.next();
            this.destroyed.complete();
        }
    }

    public getDefaultHttpParams(): Map<string, string | Array<string>> {
        let params: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
        let paramsArr: Array<string> = [];
        params.set('status', paramsArr);
        params.set('entityType', "policy");
        params.set('entityId', this.policyId);
        return params;
    }

    public itemSelected(item: EntityViewModel): void {
        this.navProxy.navigateForward([this.userPath.policy, this.policyId, 'claim', item.id], true);
    }
}
