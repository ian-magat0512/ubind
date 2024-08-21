import { Component, OnInit, ViewChild } from '@angular/core';
import { contentAnimation } from '@assets/animations';
import { NavProxyService } from '@app/services/nav-proxy.service';
import { LayoutManagerService } from '@app/services/layout-manager.service';
import { scrollbarStyle } from '@assets/scrollbar';
import { PolicyApiService } from '@app/services/api/policy-api.service';
import { RouteHelper } from '@app/helpers/route.helper';
import { Permission } from '@app/helpers';
import { EntityListComponent } from '@app/components/entity-list/entity-list.component';
import { PolicyTransactionDetailResourceModel } from '@app/resource-models/policy.resource-model';
import { UserTypePathHelper } from '@app/helpers/user-type-path.helper';
import { EmailApiService } from '@app/services/api/email-api.service';
import { EmailResourceModel } from '@app/resource-models/email.resource-model';
import { ActivatedRoute } from '@angular/router';
import { MessageViewModel } from '@app/viewmodels/message.viewmodel';
import { MessageService } from '@app/services/message.service';
import { IconLibrary } from '@app/models/icon-library.enum';

/**
 * This class displays the policy transaction messages on the list.
 */
@Component({
    selector: 'app-list-policy-transaction-message',
    templateUrl: './list-policy-transaction-message.page.html',
    animations: [contentAnimation],
    styleUrls: [
        '../../../../assets/css/scrollbar-segment.css',
    ],
    styles: [scrollbarStyle],
})
export class ListPolicyTransactionMessagePage implements OnInit {

    @ViewChild(EntityListComponent, { static: true })
    public listComponent: EntityListComponent<MessageViewModel, EmailResourceModel>;

    public title: string = 'Policy Transaction Messages';
    public permission: typeof Permission = Permission;
    public viewModelConstructor: typeof MessageViewModel = MessageViewModel;

    public policyId: string;
    private policyTransactionId: string;
    public listItemNamePlural: string = 'policy transaction messages';
    public iconLibrary: typeof IconLibrary = IconLibrary;

    public constructor(
        protected routeHelper: RouteHelper,
        public navProxy: NavProxyService,
        public layoutManager: LayoutManagerService,
        public emailApiService: EmailApiService,
        public policyApiService: PolicyApiService,
        protected userPath: UserTypePathHelper,
        private route: ActivatedRoute,
        public messageService: MessageService,
    ) {
    }

    public ngOnInit(): void {
        this.policyId = this.route.snapshot.paramMap.get('policyId');
        this.policyTransactionId = this.route.snapshot.paramMap.get('policyTransactionId');
        this.policyApiService.getPolicyTransaction(this.policyId, this.policyTransactionId)
            .subscribe((dt: PolicyTransactionDetailResourceModel) => {
                this.title = 'Message for ' + dt.quoteReference;
            });
    }

    public getDefaultHttpParams(): Map<string, string | Array<string>> {
        let params: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
        let paramsArr: Array<string> = [];
        params.set('status', paramsArr);
        params.set('entityType', "policyTransaction");
        params.set('entityId', this.policyTransactionId);
        return params;
    }

    public itemSelected(item: MessageViewModel): void {
        this.policyId = this.routeHelper.getParam('policyId');
        this.policyTransactionId = this.routeHelper.getParam('policyTransactionId');
        let pathSegments: Array<string> = this.routeHelper.getModulePathSegments();
        pathSegments.push(this.policyId);
        pathSegments.push("transaction");
        pathSegments.push(this.policyTransactionId);
        pathSegments.push("message");
        pathSegments.push(item.type);
        pathSegments.push(item.id);
        this.navProxy.navigateForward(pathSegments, true);
    }
}
