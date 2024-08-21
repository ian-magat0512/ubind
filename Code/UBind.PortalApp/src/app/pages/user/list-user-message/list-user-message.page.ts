import { Component } from '@angular/core';
import { contentAnimation } from '@assets/animations';
import { NavProxyService } from '@app/services/nav-proxy.service';
import { LayoutManagerService } from '@app/services/layout-manager.service';
import { scrollbarStyle } from '@assets/scrollbar';
import { UserApiService } from '@app/services/api/user-api.service';
import { RouteHelper } from '@app/helpers/route.helper';
import { UserTypePathHelper } from '@app/helpers/user-type-path.helper';
import { EmailApiService } from '@app/services/api/email-api.service';
import { UserResourceModel } from '@app/resource-models/user/user.resource-model';
import { MessageService } from '@app/services/message.service';
import { BaseEntityListMessagePage } from '@app/components/message/base-entity-list-message.page';

/**
 * This class displays the user messages on the list.
 */
@Component({
    selector: 'app-list-user-message',
    templateUrl: './list-user-message.page.html',
    animations: [contentAnimation],
    styleUrls: [
        '../../../../assets/css/scrollbar-segment.css',
    ],
    styles: [scrollbarStyle],
})
export class ListUserMessagePage extends BaseEntityListMessagePage {
    public title: string = 'User Message';
    public constructor(
        routeHelper: RouteHelper,
        navProxy: NavProxyService,
        layoutManager: LayoutManagerService,
        emailApiService: EmailApiService,
        userPath: UserTypePathHelper,
        messageService: MessageService,
        private userApiService: UserApiService,
    ) {
        super(routeHelper, navProxy, layoutManager, emailApiService, userPath, messageService);
    }

    protected getEntityType(): string {
        return "user";
    }

    protected getEntityIdParamName(): string {
        return 'userId';
    }

    protected loadEntityName(userId: string): void {
        this.userApiService.getById(userId).subscribe((dt: UserResourceModel) => {
            this.title = 'Message for ' + dt.fullName;
        });
    }
}
