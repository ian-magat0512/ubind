import { Component, OnInit, ViewChild } from '@angular/core';
import { contentAnimation } from '@assets/animations';
import { NavProxyService } from '@app/services/nav-proxy.service';
import { LayoutManagerService } from '@app/services/layout-manager.service';
import { scrollbarStyle } from '@assets/scrollbar';
import { ClaimApiService } from '@app/services/api/claim-api.service';
import { RouteHelper } from '@app/helpers/route.helper';
import { Permission } from '@app/helpers';
import { EntityListComponent } from '@app/components/entity-list/entity-list.component';
import { UserTypePathHelper } from '@app/helpers/user-type-path.helper';
import { EmailApiService } from '@app/services/api/email-api.service';
import { ClaimResourceModel } from '@app/resource-models/claim.resource-model';
import { MessageViewModel } from '@app/viewmodels/message.viewmodel';
import { MessageResourceModel } from '@app/resource-models/message.resource-model';
import { MessageService } from '@app/services/message.service';
import { IconLibrary } from '@app/models/icon-library.enum';

/**
 * This class displays the claim messages on the list.
 */
@Component({
    selector: 'app-list-claim-message',
    templateUrl: './list-claim-message.page.html',
    animations: [contentAnimation],
    styleUrls: [
        '../../../../assets/css/scrollbar-segment.css',
    ],
    styles: [
        scrollbarStyle,
    ],
})
export class ListClaimMessagePage implements OnInit {

    @ViewChild(EntityListComponent, { static: true })
    public listComponent: EntityListComponent<MessageViewModel, MessageResourceModel>;

    public title: string = 'Claim Messages';
    public permission: typeof Permission = Permission;
    private claimId: string;
    public viewModelConstructor: typeof MessageViewModel = MessageViewModel;
    public iconLibrary: typeof IconLibrary = IconLibrary;

    public constructor(
        protected routeHelper: RouteHelper,
        public navProxy: NavProxyService,
        public layoutManager: LayoutManagerService,
        public claimApiService: ClaimApiService,
        public emailApiService: EmailApiService,
        protected userPath: UserTypePathHelper,
        public messageService: MessageService,
    ) {
    }

    public ngOnInit(): void {
        this.claimId = this.routeHelper.getParam('claimId');
        this.claimApiService.getById(this.claimId)
            .subscribe((dt: ClaimResourceModel) => {
                this.title = 'Messages for ' + dt.claimReference;
            });
    }

    public getDefaultHttpParams(): Map<string, string | Array<string>> {
        let params: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
        let paramsArr: Array<string> = [];
        params.set('status', paramsArr);
        params.set('entityType', "claim");
        params.set('entityId', this.claimId);
        return params;
    }

    public itemSelected(item: MessageViewModel): void {
        this.claimId = this.routeHelper.getParam('claimId');
        let pathSegments: Array<string> = this.routeHelper.getModulePathSegments();
        pathSegments.push(this.claimId);
        pathSegments.push("message");
        pathSegments.push(item.type);
        pathSegments.push(item.id);
        this.navProxy.navigateForward(pathSegments, true);
    }
}
