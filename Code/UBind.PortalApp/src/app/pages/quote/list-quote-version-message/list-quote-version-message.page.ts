import { Component, OnInit, ViewChild, ChangeDetectorRef } from '@angular/core';
import { EntityListComponent } from '@app/components/entity-list/entity-list.component';
import { MessageViewModel } from '@app/viewmodels/message.viewmodel';
import { MessageResourceModel } from '@app/resource-models/message.resource-model';
import { Permission } from '@app/helpers';
import { RouteHelper } from '@app/helpers/route.helper';
import { NavProxyService } from '@app/services/nav-proxy.service';
import { AuthenticationService } from '@app/services/authentication.service';
import { QuoteApiService } from '@app/services/api/quote-api.service';
import { QuoteVersionApiService } from '@app/services/api/quote-version-api.service';
import { EmailApiService } from '@app/services/api/email-api.service';
import { BroadcastService } from '@app/services/broadcast.service';
import { LoadDataService } from '@app/services/load-data.service';
import { LayoutManagerService } from '@app/services/layout-manager.service';
import { UserTypePathHelper } from '@app/helpers/user-type-path.helper';
import { MessageService } from '@app/services/message.service';
import { QuoteVersionDetailResourceModel } from '@app/resource-models/quote-version.resource-model';
import { IconLibrary } from '@app/models/icon-library.enum';

/**
 * Component class for quote version messages list.
 */
@Component({
    selector: 'app-list-quote-version-message',
    templateUrl: 'app-list-quote-version-message.page.html',
})

export class ListQuoteVersionMessagePage implements OnInit {
    @ViewChild(EntityListComponent, { static: true })
    public listComponent: EntityListComponent<MessageViewModel, MessageResourceModel>;

    public title: string = 'Quote Version Messages';
    public permission: typeof Permission = Permission;
    private quoteId: string;
    private quoteVersionId: string;
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
        this.quoteVersionId = this.routeHelper.getParam('quoteVersionId');
        this.title = this.routeHelper.getParam('title') ||
            this.authService.isCustomer() ? 'My Quote Version Messages' : 'Quote Version Messages';
        this.quoteVersionApiService.getQuoteVersionDetail(this.quoteVersionId).subscribe(
            (dt: QuoteVersionDetailResourceModel) => {
                let referenceNumber: string = dt.quoteNumber
                    ? dt.quoteNumber + '-' + dt.quoteVersionNumber
                    : dt.quoteVersionNumber;
                this.title = 'Messages for ' + referenceNumber;
            },
        );
    }

    public getDefaultHttpParams(): Map<string, string | Array<string>> {
        let params: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
        let paramsArr: Array<string> = [];
        params.set('status', paramsArr);
        params.set('entityType', "quoteVersion");
        params.set('entityId', this.quoteVersionId);
        return params;
    }

    public itemSelected(item: MessageViewModel): void {
        this.quoteId = this.routeHelper.getParam('quoteId');
        this.quoteVersionId = this.routeHelper.getParam('quoteVersionId');
        let pathSegments: Array<string> = this.routeHelper.getModulePathSegments();
        pathSegments.push(this.quoteId);
        pathSegments.push("version");
        pathSegments.push(this.quoteVersionId);
        pathSegments.push("message");
        pathSegments.push(item.type);
        pathSegments.push(item.id);
        this.navProxy.navigateForward(pathSegments, true);
    }
}
