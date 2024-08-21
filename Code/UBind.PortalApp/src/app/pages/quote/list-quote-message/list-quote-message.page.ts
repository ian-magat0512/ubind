import { Component, ChangeDetectorRef, ViewChild, OnInit } from '@angular/core';
import { NavProxyService } from '@app/services/nav-proxy.service';
import { AuthenticationService } from '@app/services/authentication.service';
import { QuoteApiService } from '@app/services/api/quote-api.service';
import { BroadcastService } from '@app/services/broadcast.service';
import { LoadDataService } from '@app/services/load-data.service';
import { contentAnimation } from '@assets/animations';
import { scrollbarStyle } from '@assets/scrollbar';
import { LayoutManagerService } from '@app/services/layout-manager.service';
import { RouteHelper } from '@app/helpers/route.helper';
import { QuoteVersionApiService } from '@app/services/api/quote-version-api.service';
import { EntityListComponent } from '@app/components/entity-list/entity-list.component';
import { UserTypePathHelper } from '@app/helpers/user-type-path.helper';
import { Permission } from '@app/helpers/permissions.helper';
import { QuoteDetailResourceModel } from '@app/resource-models/quote.resource-model';
import { EmailApiService } from '@app/services/api/email-api.service';
import { MessageResourceModel } from '@app/resource-models/message.resource-model';
import { MessageViewModel } from '@app/viewmodels/message.viewmodel';
import { MessageService } from '@app/services/message.service';
import { IconLibrary } from '@app/models/icon-library.enum';

/**
 * This class displays the quote messages on the list.
 */
@Component({
    selector: 'app-list-quote-message',
    templateUrl: './list-quote-message.page.html',
    animations: [contentAnimation],
    styleUrls: [
        '../../../../assets/css/scrollbar-segment.css',
    ],
    styles: [scrollbarStyle],
})
export class ListQuoteMessagePage implements OnInit {

    @ViewChild(EntityListComponent, { static: true })
    public listComponent: EntityListComponent<MessageViewModel, MessageResourceModel>;

    public title: string = 'Quote Messages';
    public permission: typeof Permission = Permission;
    private quoteId: string;
    public viewModelConstructor: typeof MessageViewModel = MessageViewModel;
    public iconLibrary: typeof IconLibrary = IconLibrary;

    public constructor(
        protected changeDetectorRef: ChangeDetectorRef,
        protected routeHelper: RouteHelper,
        public navProxy: NavProxyService,
        private authService: AuthenticationService,
        protected quoteApiService: QuoteApiService,
        public quoteVersionApiService: QuoteVersionApiService,
        public emailApiService: EmailApiService,
        protected broadcastService: BroadcastService,
        protected loadDataService: LoadDataService,
        public layoutManager: LayoutManagerService,
        protected userPath: UserTypePathHelper,
        public messageService: MessageService,
    ) {
    }

    public ngOnInit(): void {
        this.quoteId = this.routeHelper.getParam('quoteId');
        this.title = this.routeHelper.getParam('title') ||
            this.authService.isCustomer() ? 'My Quote Messages' : 'Quote Messages';
        this.quoteApiService.getQuoteDetails(this.quoteId).subscribe(
            (dt: QuoteDetailResourceModel) => {
                this.title = 'Messages for ' + dt.quoteNumber;
            },
        );
    }

    public getDefaultHttpParams(): Map<string, string | Array<string>> {
        let params: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
        let paramsArr: Array<string> = [];
        params.set('status', paramsArr);
        params.set('entityType', "quote");
        params.set('entityId', this.quoteId);
        return params;
    }

    public itemSelected(item: MessageViewModel): void {
        this.quoteId = this.routeHelper.getParam('quoteId');
        let pathSegments: Array<string> = this.routeHelper.getModulePathSegments();
        pathSegments.push(this.quoteId);
        pathSegments.push("message");
        pathSegments.push(item.type);
        pathSegments.push(item.id);
        this.navProxy.navigateForward(pathSegments, true);
    }
}
