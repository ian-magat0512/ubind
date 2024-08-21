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
import { QuoteDetailViewModel } from '@app/viewmodels/quote-detail.viewmodel';
import { QuoteVersionViewModel } from '@app/viewmodels/quote-version.viewmodel';
import { QuoteVersionApiService } from '@app/services/api/quote-version-api.service';
import { EntityViewModel } from '@app/viewmodels/entity.viewmodel';
import { EntityListComponent } from '../../../components/entity-list/entity-list.component';
import { QuoteVersionResourceModel } from '../../../resource-models/quote-version.resource-model';
import { UserTypePathHelper } from '../../../helpers/user-type-path.helper';
import { QuoteService } from '@app/services/quote.service';
import { Permission } from '@app/helpers/permissions.helper';
import { QuoteDetailResourceModel } from '@app/resource-models/quote.resource-model';
import { Subscription } from 'rxjs';
import { PermissionService } from '@app/services/permission.service';
import { IconLibrary } from '@app/models/icon-library.enum';
import { ActionButton } from '@app/models/action-button';

/**
 * Export list quote version page component class.
 * TODO: Write a better class header: displaying quote version on the list.
 */
@Component({
    selector: 'app-list-quote-version',
    templateUrl: './list-quote-version.page.html',
    animations: [contentAnimation],
    styleUrls: [
        '../../../../assets/css/scrollbar-segment.css',
    ],
    styles: [scrollbarStyle],
})
export class ListQuoteVersionPage implements OnInit {

    @ViewChild(EntityListComponent, { static: true })
    public listComponent: EntityListComponent<QuoteVersionViewModel, QuoteVersionResourceModel>;

    public quoteDetail: QuoteDetailViewModel;
    public title: string = 'Quote Versions';
    public permission: typeof Permission = Permission;
    private quoteId: string;
    public viewModelConstructor: any = QuoteVersionViewModel;
    public additionalActionButtonList: Array<ActionButton> = [];
    public iconLibrary: typeof IconLibrary = IconLibrary;

    public constructor(
        protected changeDetectorRef: ChangeDetectorRef,
        protected routeHelper: RouteHelper,
        public navProxy: NavProxyService,
        private authService: AuthenticationService,
        protected quoteApiService: QuoteApiService,
        public quoteVersionApiService: QuoteVersionApiService,
        private quoteService: QuoteService,
        protected broadcastService: BroadcastService,
        protected loadDataService: LoadDataService,
        public layoutManager: LayoutManagerService,
        protected userPath: UserTypePathHelper,
        private permissionService: PermissionService,
    ) {
    }

    public ngOnInit(): void {
        this.quoteId = this.routeHelper.getParam('quoteId');
        this.title = this.routeHelper.getParam('title') ||
            this.authService.isCustomer() ? 'My Quote Versions' : 'Quote Versions';
        let quoteDetailsSubscription: Subscription = this.quoteApiService.getQuoteDetails(this.quoteId).subscribe(
            (dt: QuoteDetailResourceModel) => {
                this.quoteDetail = new QuoteDetailViewModel(dt);
                this.title = dt.quoteNumber === null ? 'Quote Versions' : 'Quote Versions for ' + dt.quoteNumber;
                quoteDetailsSubscription.unsubscribe();
            },
        );
        this.initialiseAdditionalActionButtons();
    }

    public getDefaultHttpParams(): Map<string, string | Array<string>> {
        let params: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
        params.set('quoteId', this.quoteId);
        return params;
    }

    public itemSelected(item: EntityViewModel): void {
        this.navProxy.navigateForward([this.userPath.quote, this.quoteId, 'version', item.id], true);
    }

    public async createNewQuote(): Promise<void> {
        this.quoteService.createQuoteBySelectingProduct();
    }

    private initialiseAdditionalActionButtons(): void {
        let additionalActionButtonList: Array<ActionButton> = [];
        if (this.permissionService.hasPermission(Permission.ManageQuotes)) {
            additionalActionButtonList.push(ActionButton.createActionButton(
                "Create",
                "plus",
                IconLibrary.AngularMaterial,
                false,
                "Create Quote",
                true,
                (): Promise<void> => {
                    return this.createNewQuote();
                },
            ));
        }
        this.additionalActionButtonList = additionalActionButtonList;
    }
}
