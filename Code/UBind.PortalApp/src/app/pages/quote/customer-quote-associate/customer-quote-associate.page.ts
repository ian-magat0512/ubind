import { Component, OnInit } from "@angular/core";
import { RouteHelper } from "@app/helpers/route.helper";
import { ActionService } from "@app/services/action.service";
import { CustomerService } from "@app/services/customer.service";
import { NavProxyService } from "@app/services/nav-proxy.service";

/**
 * This page will never actually show, because it will redirect to the home page
 * before being shown. However it does register an action to be execute once the home page
 * loads.
 */
@Component({
    selector: 'app-customer-quote-associate',
    template: '',
})
export class CustomerQuoteAssociatePage implements OnInit {
    public constructor(
        protected actionService: ActionService,
        protected customerService: CustomerService,
        protected routeHelper: RouteHelper,
        protected navProxy: NavProxyService,
    ) {
    }

    public ngOnInit(): void {
        let quoteId: string = this.routeHelper.getParam('quoteId');
        let associationInvitationId: string = this.routeHelper.getParam('associateInvitationId');
        this.actionService.register(() =>
            this.customerService.associateQuoteWithCustomer(
                associationInvitationId,
                quoteId,
            ));
        this.navProxy.redirectToHome();
    }
}
