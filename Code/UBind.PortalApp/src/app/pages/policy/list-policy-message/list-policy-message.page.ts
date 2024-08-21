import { Component, OnInit, ViewChild } from '@angular/core';
import { contentAnimation } from '@assets/animations';
import { NavProxyService } from '@app/services/nav-proxy.service';
import { LayoutManagerService } from '@app/services/layout-manager.service';
import { scrollbarStyle } from '@assets/scrollbar';
import { PolicyApiService } from '@app/services/api/policy-api.service';
import { PolicyTransactionApiService } from '@app/services/api/policy-transaction-api.service';
import { RouteHelper } from '@app/helpers/route.helper';
import { Permission } from '@app/helpers';
import { EntityListComponent } from '@app/components/entity-list/entity-list.component';
import { PolicyDetailResourceModel } from '@app/resource-models/policy.resource-model';
import { AuthenticationService } from '@app/services/authentication.service';
import { UserTypePathHelper } from '@app/helpers/user-type-path.helper';
import { EmailApiService } from '@app/services/api/email-api.service';
import { MessageService } from '@app/services/message.service';
import { MessageViewModel } from '@app/viewmodels/message.viewmodel';
import { MessageResourceModel } from '@app/resource-models/message.resource-model';
import { IconLibrary } from '@app/models/icon-library.enum';

/**
 * This class displays the policy messages on the list.
 */
@Component({
    selector: 'app-list-policy-message',
    templateUrl: './list-policy-message.page.html',
    animations: [contentAnimation],
    styleUrls: [
        '../../../../assets/css/scrollbar-segment.css',
    ],
    styles: [scrollbarStyle],
})
export class ListPolicyMessagePage implements OnInit {

    @ViewChild(EntityListComponent, { static: true })
    public listComponent: EntityListComponent<MessageViewModel, MessageResourceModel>;

    public title: string = 'Policy Message';
    public permission: typeof Permission = Permission;
    private policyId: string;
    public viewModelConstructor: typeof MessageViewModel = MessageViewModel;

    public policyNumber: string;
    public isMutual: boolean;
    public listItemNamePlural: string = 'policy messages';
    public iconLibrary: typeof IconLibrary = IconLibrary;

    public constructor(
        protected routeHelper: RouteHelper,
        public navProxy: NavProxyService,
        public layoutManager: LayoutManagerService,
        protected policyApiService: PolicyApiService,
        public emailApiService: EmailApiService,
        public policTransactionApiService: PolicyTransactionApiService,
        private authService: AuthenticationService,
        protected userPath: UserTypePathHelper,
        public messageService: MessageService,
    ) {
    }

    public ngOnInit(): void {
        this.isMutual = this.authService.isMutualTenant();
        this.listItemNamePlural = this.isMutual ? 'protection message' : this.listItemNamePlural;
        this.policyId = this.routeHelper.getParam('policyId');

        if (this.authService.isCustomer()) {
            if (this.isMutual) {
                this.title = 'My Protection Messages';
            } else {
                this.title = 'My Policy Messages';
            }
        } else {
            if (this.isMutual) {
                this.title = 'Protection Messages';
            } else {
                this.title = 'Policy Messages';
            }
        }

        this.policyApiService.getPolicyBaseDetails(this.policyId)
            .subscribe((dt: PolicyDetailResourceModel) => {
                this.title = 'Messages for ' + dt.policyNumber;
            });
    }

    public getDefaultHttpParams(): Map<string, string | Array<string>> {
        let params: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
        let paramsArr: Array<string> = [];
        params.set('status', paramsArr);
        params.set('entityType', "policy");
        params.set('entityId', this.policyId);
        return params;
    }

    public itemSelected(item: MessageViewModel): void {
        this.policyId = this.routeHelper.getParam('policyId');
        let pathSegments: Array<string> = this.routeHelper.getModulePathSegments();
        pathSegments.push(this.policyId);
        pathSegments.push("message");
        pathSegments.push(item.type);
        pathSegments.push(item.id);
        this.navProxy.navigateForward(pathSegments, true);
    }
}
