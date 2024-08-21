import { Component } from '@angular/core';
import { contentAnimation } from '@assets/animations';
import { NavProxyService } from '@app/services/nav-proxy.service';
import { LayoutManagerService } from '@app/services/layout-manager.service';
import { scrollbarStyle } from '@assets/scrollbar';
import { CustomerApiService } from '@app/services/api/customer-api.service';
import { RouteHelper } from '@app/helpers/route.helper';
import { UserTypePathHelper } from '@app/helpers/user-type-path.helper';
import { EmailApiService } from '@app/services/api/email-api.service';
import { CustomerResourceModel } from '@app/resource-models/customer.resource-model';
import { MessageService } from '@app/services/message.service';
import { BaseEntityListMessagePage } from '@app/components/message/base-entity-list-message.page';

/**
 * This class displays the customer messages on the list.
 */
@Component({
    selector: 'app-list-customer-message',
    templateUrl: './list-customer-message.page.html',
    animations: [contentAnimation],
    styleUrls: [
        '../../../../assets/css/scrollbar-segment.css',
    ],
    styles: [scrollbarStyle],
})
export class ListCustomerMessagePage extends BaseEntityListMessagePage {
    public title: string = 'Customer Message';

    public constructor(
        routeHelper: RouteHelper,
        navProxy: NavProxyService,
        layoutManager: LayoutManagerService,
        emailApiService: EmailApiService,
        userPath: UserTypePathHelper,
        messageService: MessageService,
        private customerApiService: CustomerApiService,
    ) {
        super(routeHelper, navProxy, layoutManager, emailApiService, userPath, messageService);
    }

    protected getEntityType(): string {
        return "customer";
    }

    protected getEntityIdParamName(): string {
        return 'customerId';
    }

    protected loadEntityName(customerId: string): void {
        this.customerApiService.getById(customerId).subscribe((dt: CustomerResourceModel) => {
            this.title = 'Messages for ' + dt.fullName;
        });
    }
}
