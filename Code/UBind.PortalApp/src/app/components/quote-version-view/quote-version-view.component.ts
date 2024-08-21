import { Component, OnInit, Input, OnDestroy } from '@angular/core';
import { NavProxyService } from '@app/services/nav-proxy.service';
import { scrollbarStyle } from '@assets/scrollbar';
import { contentAnimation } from '@assets/animations';
import { UserTypePathHelper } from '@app/helpers/user-type-path.helper';
import { PermissionService } from '@app/services/permission.service';
import { QuoteVersionApiService } from '@app/services/api/quote-version-api.service';
import { finalize, takeUntil } from 'rxjs/operators';
import { QuoteVersionResourceModel } from '@app/resource-models/quote-version.resource-model';
import { QuoteVersionViewModel } from '@app/viewmodels/quote-version.viewmodel';
import { Subject } from 'rxjs';
import { QuoteDetailViewModel } from '@app/viewmodels';
import { FeatureSettingService } from '@app/services/feature-setting.service';

/**
 * Export quote version view component class
 * This is to load the quote version.
 */
@Component({
    selector: 'app-quote-version-view',
    templateUrl: './quote-version-view.component.html',
    styleUrls: [
        '../../../assets/css/scrollbar-segment.css',
        '../../../assets/css/scrollbar-div.css',
    ],
    styles: [
        scrollbarStyle,
    ],
    animations: [contentAnimation],
})
export class QuoteVersionViewComponent implements OnInit, OnDestroy {

    @Input() public quoteId: string;
    protected destroyed: Subject<void>;
    public versionsErrorMessage: string;
    public isLoadingVersions: boolean = true;
    public quoteVersionList: any = [];
    public detail: QuoteDetailViewModel;
    public constructor(
public navProxy: NavProxyService,
        protected userPath: UserTypePathHelper,
        private featureSettingService: FeatureSettingService,
        private permissionService: PermissionService,
        protected quoteVersionApiService: QuoteVersionApiService,
    ) {
    }

    public async ngOnInit(): Promise<void> {
        if (!this.canAccessQuote()) {
            this.versionsErrorMessage = 'You are not allowed to access quote version';
            return;
        }
        this.destroyed = new Subject<void>();
        await this.loadVersions();
    }

    public gotoVersionDetail(quoteVersion: any): void {
        this.navProxy.navigate([this.userPath.quote, this.quoteId, 'version', quoteVersion.quoteVersionId]);
    }

    public ngOnDestroy(): void {
        this.destroyed.next();
        this.destroyed.complete();
    }

    private async loadVersions(): Promise<void> {
        this.isLoadingVersions = true;
        return this.quoteVersionApiService.getQuoteVersions(this.quoteId)
            .pipe(
                takeUntil(this.destroyed),
                finalize(() => this.isLoadingVersions = false),
            )
            .toPromise().then(
                (quoteVersions: Array<QuoteVersionResourceModel>) => {
                    this.quoteVersionList = [];
                    quoteVersions?.forEach((quoteVersion: QuoteVersionResourceModel) => {
                        this.quoteVersionList.push(new QuoteVersionViewModel(quoteVersion));
                    });
                },
                (err: any) => {
                    this.versionsErrorMessage = 'There was a problem loading the quote versions';
                    throw err;
                },
            );
    }

    private canAccessQuote(): boolean {
        if (this.permissionService.hasViewQuotePermission()) {
            return true;
        }
        return false;
    }
}
