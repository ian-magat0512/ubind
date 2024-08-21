import { Component, OnInit, Input, OnDestroy } from '@angular/core';
import { CardBaseComponent } from '../card-base.component';
import { QuoteViewModel } from '@app/viewmodels/quote.viewmodel';
import { NavProxyService } from '@app/services/nav-proxy.service';
import { UserTypePathHelper } from '@app/helpers/user-type-path.helper';
import { QuoteService } from '@app/services/quote.service';
import { AuthenticationService } from '@app/services/authentication.service';
import { SubscriptionLike } from 'rxjs/internal/types';
import { IconLibrary } from '@app/models/icon-library.enum';

/**
 * Export customer quote card component class
 * TODO: Write a better class header: displaying of customer quote cards.
 */
@Component({
    selector: 'app-customer-quote-card',
    templateUrl: './customer-quote-card.component.html',
    styleUrls: [
        '../customer-home.page.scss',
        '../card-base-component.scss',
    ],
})
export class CustomerQuoteCardComponent extends CardBaseComponent implements OnInit, OnDestroy {
    @Input() public quotes: Array<QuoteViewModel> = [];
    public isCreateNewQuoteEnabled: boolean;
    public iconLibrary: typeof IconLibrary = IconLibrary;
    protected subscriptions: Array<SubscriptionLike> = new Array<SubscriptionLike>();
    public constructor(
        protected navProxy: NavProxyService,
        protected userPath: UserTypePathHelper,
        protected quoteService: QuoteService,
        private authService: AuthenticationService,
    ) {
        super();
    }

    public async ngOnInit(): Promise<void> {
        this.isCreateNewQuoteEnabled = await this.quoteService.canCreateNewQuote();
    }

    public ngOnDestroy(): void {
        this.subscriptions.forEach((s: SubscriptionLike) => s.unsubscribe());
    }

    public userDidTapQuote(quote: QuoteViewModel): void {
        this.navProxy.navigateForward([this.userPath.quote, quote.id]);
    }

    public userDidTapInactiveQuotes(): void {
        this.navProxy.navigateForward([this.userPath.quote, 'list'], true, { queryParams: { segment: 'Inactive' } });
    }

    public userDidTapNewQuote(): void {
        this.quoteService.createQuoteBySelectingProduct(this.authService.customerId);
    }
}
