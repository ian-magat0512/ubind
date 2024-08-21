import { Component, OnDestroy, OnInit } from "@angular/core";
import { Permission } from "@app/helpers";
import { UserTypePathHelper } from "@app/helpers/user-type-path.helper";
import { QuoteApiService } from "@app/services/api/quote-api.service";
import { AuthenticationService } from "@app/services/authentication.service";
import { EventService, UserId } from "@app/services/event.service";
import { NavProxyService } from "@app/services/nav-proxy.service";
import { QuoteService } from "@app/services/quote.service";
import { interval, Subject } from "rxjs";
import { takeUntil } from "rxjs/operators";
import { PermissionService } from "@app/services/permission.service";
import { DateHelper } from "@app/helpers/date.helper";

enum PartOfDay {
    Morning = 'morning',
    Afternoon = 'afternoon',
    Evening = 'evening'
}

/**
 * Shows a hello and start new quote button
 */
@Component({
    selector: 'app-welcome',
    templateUrl: './welcome.component.html',
    styleUrls: [ './welcome.component.scss' ],
})
export class WelcomeComponent implements OnInit, OnDestroy {

    public partOfDay: PartOfDay = PartOfDay.Morning;
    public preferredName: string;
    public numberOfIncompleteQuotes: number = 0;
    public canViewAllQuotes: boolean = false;
    public canViewQuotes: boolean = false;
    public canManageQuotes: boolean = false;
    private destroyed: Subject<void>;
    public permission: typeof Permission = Permission;

    public constructor(
        private quoteApiService: QuoteApiService,
        private quoteService: QuoteService,
        private authService: AuthenticationService,
        private navProxyService: NavProxyService,
        private userPath: UserTypePathHelper,
        private eventService: EventService,
        private authenticationService: AuthenticationService,
        private permissionService: PermissionService,
    ) {
        this.updatePartOfDay();
    }

    public ngOnInit(): void {
        this.destroyed = new Subject<void>();
        this.load();

        // when we login, the permissions change so we need re-draw
        this.eventService.userLoginSubject$.pipe(takeUntil(this.destroyed)).subscribe((userId: UserId) => {
            if (this.authenticationService.isAuthenticated()) {
                this.load();
            }
        });
        this.updatePartOfTheDayEveryMinute();
    }

    private load(): void {
        this.preferredName = this.authService.userPreferredName;
        this.canViewQuotes = this.permissionService.hasViewQuotePermission();
        this.canViewAllQuotes = this.permissionService.hasPermission(Permission.ViewAllQuotes) ||
            this.permissionService.hasPermission(Permission.ViewAllQuotesFromAllOrganisations);
        this.canManageQuotes = this.permissionService.hasManageQuotePermission();
        if (this.canViewQuotes) {
            this.loadNumberOfIncompleteQuotes();
        }
    }

    public ngOnDestroy(): void {
        this.destroyed.next();
        this.destroyed.complete();
    }

    public updatePartOfTheDayEveryMinute(): void {
        interval(60000).pipe(takeUntil(this.destroyed)).subscribe(() => {
            this.updatePartOfDay();
        });
    }

    public updatePartOfDay(): void {
        let date: Date = new Date();
        let hour: number = date.getHours();
        if (hour < 12) {
            this.partOfDay = PartOfDay.Morning;
        } else if (hour < 18) {
            this.partOfDay = PartOfDay.Afternoon;
        } else {
            this.partOfDay = PartOfDay.Evening;
        }
    }

    private loadNumberOfIncompleteQuotes(): void {
        let params: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
        params.set('ownerUserId', this.authService.userId);
        let date: Date = new Date();
        date.setMonth(date.getMonth() - 3);
        params.set('dateFilteringPropertyName', 'CreatedTicksSinceEpoch');
        params.set('afterDateTime', DateHelper.formatYYYYMMDD(date));
        params.set('status', ['incomplete', 'approved', 'review', 'endorsement']);
        this.quoteApiService.getCount(params).pipe(takeUntil(this.destroyed)).subscribe((count: number) => {
            this.numberOfIncompleteQuotes = count;
        });
    }

    public userDidTapStartNewQuote(): void {
        this.quoteService.createQuoteBySelectingProduct(this.authService.customerId);
    }

    public userDidTapViewQuotes(): void {
        this.navProxyService.navigateForward([this.userPath.quote, 'list']);
    }
}
