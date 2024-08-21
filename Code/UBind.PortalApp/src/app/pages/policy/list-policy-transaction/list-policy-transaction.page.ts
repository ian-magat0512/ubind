import { Component, OnInit, ViewChild } from '@angular/core';
import { contentAnimation } from '@assets/animations';
import { NavProxyService } from '@app/services/nav-proxy.service';
import { LayoutManagerService } from '@app/services/layout-manager.service';
import { PolicyTransactionViewModel } from '@app/viewmodels/policy-transaction.viewmodel';
import { scrollbarStyle } from '@assets/scrollbar';
import { PolicyApiService } from '@app/services/api/policy-api.service';
import { PolicyTransactionApiService } from '@app/services/api/policy-transaction-api.service';
import { RouteHelper } from '../../../helpers/route.helper';
import { PolicyDetailViewModel } from '../../../viewmodels/policy-detail.viewmodel';
import { EntityViewModel } from '../../../viewmodels/entity.viewmodel';
import { Permission } from '../../../helpers';
import { EntityListComponent } from '../../../components/entity-list/entity-list.component';
import {
    PolicyDetailResourceModel, PolicyTransactionResourceModel,
} from '../../../resource-models/policy.resource-model';
import { AuthenticationService } from '../../../services/authentication.service';
import { UserTypePathHelper } from '../../../helpers/user-type-path.helper';
import { Subscription } from 'rxjs';
import { IconLibrary } from '@app/models/icon-library.enum';

/**
 * Export list policy transaction page component class.
 * This class displays the policy transaction details on the list.
 */
@Component({
    selector: 'app-list-policy-transaction',
    templateUrl: './list-policy-transaction.page.html',
    animations: [contentAnimation],
    styleUrls: [
        '../../../../assets/css/scrollbar-segment.css',
    ],
    styles: [scrollbarStyle],
})
export class ListPolicyTransactionPage implements OnInit {

    @ViewChild(EntityListComponent, { static: true })
    public listComponent: EntityListComponent<PolicyTransactionViewModel, PolicyTransactionResourceModel>;

    public policyDetail: PolicyDetailViewModel;
    public title: string = 'Policy History';
    public permission: typeof Permission = Permission;
    private policyId: string;
    public viewModelConstructor: typeof PolicyTransactionViewModel = PolicyTransactionViewModel;

    public policyNumber: string;
    public isMutual: boolean;
    public listItemNamePlural: string = 'policy history';
    public iconLibrary: typeof IconLibrary = IconLibrary;

    public constructor(
        protected routeHelper: RouteHelper,
        public navProxy: NavProxyService,
        public layoutManager: LayoutManagerService,
        protected policyApiService: PolicyApiService,
        public policyTransactionApiService: PolicyTransactionApiService,
        private authService: AuthenticationService,
        protected userPath: UserTypePathHelper,
    ) {
    }

    public ngOnInit(): void {
        this.isMutual = this.authService.isMutualTenant();
        this.listItemNamePlural = this.isMutual ? 'protection history' : this.listItemNamePlural;
        this.policyId = this.routeHelper.getParam('policyId');

        if (this.authService.isCustomer()) {
            if (this.isMutual) {
                this.title = 'My Protection History';
            } else {
                this.title = 'My Policy History';
            }
        } else {
            if (this.isMutual) {
                this.title = 'Protection History';
            } else {
                this.title = 'Policy History';
            }
        }

        let policyDetailsSubscription: Subscription = this.policyApiService.getPolicyBaseDetails(this.policyId)
            .subscribe((dt: PolicyDetailResourceModel) => {
                this.policyDetail = new PolicyDetailViewModel(dt);
                if (this.authService.isCustomer()) {
                    this.title = this.isMutual ? `My ${dt.productName} Protection History` :
                        `My ${dt.productName} Policy History`;
                } else {
                    this.title = this.isMutual ? 'Protection History for ' + dt.policyNumber :
                        'Policy History for ' + dt.policyNumber;
                }
                policyDetailsSubscription.unsubscribe();
            });
    }

    public getDefaultHttpParams(): Map<string, string | Array<string>> {
        let params: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
        params.set('policyId', this.policyId);
        return params;
    }

    public itemSelected(item: EntityViewModel): void {
        this.navProxy.navigateForward([this.userPath.policy, this.policyId, 'transaction', item.id], true);
    }
}
