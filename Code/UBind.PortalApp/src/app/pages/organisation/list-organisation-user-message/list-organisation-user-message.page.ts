import { Component, OnInit, ViewChild } from '@angular/core';
import { contentAnimation } from '@assets/animations';
import { NavProxyService } from '@app/services/nav-proxy.service';
import { LayoutManagerService } from '@app/services/layout-manager.service';
import { scrollbarStyle } from '@assets/scrollbar';
import { UserApiService } from '@app/services/api/user-api.service';
import { RouteHelper } from '@app/helpers/route.helper';
import { Permission } from '@app/helpers';
import { EntityListComponent } from '@app/components/entity-list/entity-list.component';
import { UserTypePathHelper } from '@app/helpers/user-type-path.helper';
import { EmailApiService } from '@app/services/api/email-api.service';
import { UserResourceModel } from '@app/resource-models/user/user.resource-model';
import { MessageResourceModel } from '@app/resource-models/message.resource-model';
import { MessageViewModel } from '@app/viewmodels/message.viewmodel';
import { MessageService } from '@app/services/message.service';
import { IconLibrary } from '@app/models/icon-library.enum';

/**
 * This class displays the user messages on the list.
 */
@Component({
    selector: 'app-list-organisation-user-message',
    templateUrl: './list-organisation-user-message.page.html',
    animations: [contentAnimation],
    styleUrls: [
        '../../../../assets/css/scrollbar-segment.css',
    ],
    styles: [scrollbarStyle],
})
export class ListOrganisationUserMessagePage implements OnInit {

    @ViewChild(EntityListComponent, { static: true })
    public listComponent: EntityListComponent<MessageViewModel, MessageResourceModel>;

    public title: string = 'Organisation User Message';
    public permission: typeof Permission = Permission;
    private userId: string;
    public viewModelConstructor: typeof MessageViewModel = MessageViewModel;
    private organisationId: string;
    public iconLibrary: typeof IconLibrary = IconLibrary;

    public constructor(
        protected routeHelper: RouteHelper,
        public navProxy: NavProxyService,
        public layoutManager: LayoutManagerService,
        public userApiService: UserApiService,
        public emailApiService: EmailApiService,
        protected userPath: UserTypePathHelper,
        public messageService: MessageService,
    ) {
    }

    public ngOnInit(): void {
        this.userId = this.routeHelper.getParam('userId');
        this.userApiService.getById(this.userId)
            .subscribe((dt: UserResourceModel) => {
                this.title = 'Message for ' + dt.fullName;
            });
    }

    public getDefaultHttpParams(): Map<string, string | Array<string>> {
        let params: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
        let paramsArr: Array<string> = [];
        params.set('status', paramsArr);
        params.set('entityType', "user");
        params.set('entityId', this.userId);
        return params;
    }

    public itemSelected(item: MessageViewModel): void {
        this.organisationId = this.routeHelper.getParam('organisationId');
        this.userId = this.routeHelper.getParam('userId');
        let pathSegments: Array<string> = this.routeHelper.getModulePathSegments();
        pathSegments.push(this.organisationId);
        pathSegments.push("user");
        pathSegments.push(this.userId);
        pathSegments.push("message");
        pathSegments.push(item.type);
        pathSegments.push(item.id);
        this.navProxy.navigateForward(pathSegments, true);
    }
}
