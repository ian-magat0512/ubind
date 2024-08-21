import { Component, OnInit, OnDestroy } from '@angular/core';
import { SubscriptionLike, Observable } from 'rxjs';
import { scrollbarStyle } from '@assets/scrollbar';
import { NavProxyService } from '@app/services/nav-proxy.service';
import { AppConfigService } from '@app/services/app-config.service';
import { PolicyApiService } from '@app/services/api/policy-api.service';
import { ClaimApiService } from '@app/services/api/claim-api.service';
import { QuoteApiService } from '@app/services/api/quote-api.service';
import { FeatureSettingService } from '@app/services/feature-setting.service';
import { ClaimHelper, StringHelper } from '@app/helpers';
import { QuoteViewModel } from '@app/viewmodels/quote.viewmodel';
import { ClaimViewModel } from '@app/viewmodels/claim.viewmodel';
import { PolicyViewModel } from '@app/viewmodels/policy.viewmodel';
import { Permission } from '@app/helpers/permissions.helper';
import { LayoutManagerService } from '@app/services/layout-manager.service';
import { AppConfig } from '@app/models/app-config';
import { UserTypePathHelper } from '@app/helpers/user-type-path.helper';
import { RouteHelper } from '@app/helpers/route.helper';
import { finalize } from 'rxjs/operators';
import { PolicyResourceModel } from '@app/resource-models/policy.resource-model';
import { ProductFeatureSettingService } from '@app/services/product-feature-service';
import { QuoteResourceModel } from '@app/resource-models/quote.resource-model';
import { ClaimResourceModel } from '@app/resource-models/claim.resource-model';
import { PermissionService } from '@app/services/permission.service';
import { PolicyStatus, QuoteState } from '@app/models';
import { EventService } from '@app/services/event.service';

/**
 * Export customer home page component class
 * This class manage the displaying of quotes, claims and policies
 * of the customer in the portal also checking if there a permissions
 * to view the quotes, claims and policies.
 */
@Component({
    selector: 'app-customer-home',
    templateUrl: './customer-home.page.html',
    styleUrls: [
        './customer-home.page.scss',
    ],
    styles: [scrollbarStyle],
})
export class CustomerHomePage implements OnInit, OnDestroy {
    public isClaimsLoading: boolean = true;
    public isQuoteLoading: boolean = true;
    public isPolicyLoading: boolean = true;
    public isRenewalPolicyLoading: boolean = true;
    public hasRenewProductFeature: boolean;
    public tenantName: string = '';
    public organisationName: string;
    public portalTitle: string;
    public hasPortalTitle: boolean = false;
    public policies: Array<PolicyViewModel> = [];
    public policiesForRenewal: Array<PolicyViewModel> = [];
    public quotes: Array<QuoteViewModel> = [];
    public claims: Array<ClaimViewModel> = [];
    private subscription: Array<SubscriptionLike> = [];
    public permission: typeof Permission = Permission;
    protected subscriptions: Array<SubscriptionLike> = new Array<SubscriptionLike>();
    private portalTenantAlias: string;
    public redirectUrlMap: Map<string, any> = new Map<string, any>();
    public hasPolicyFeature: boolean;
    public hasQuoteFeature: boolean;
    public hasClaimFeature: boolean;

    public constructor(
        public navProxy: NavProxyService,
        private policyApiService: PolicyApiService,
        private claimApiService: ClaimApiService,
        private quoteApiService: QuoteApiService,
        private appConfigService: AppConfigService,
        private featureSettingService: FeatureSettingService,
        public layoutManager: LayoutManagerService,
        protected userPath: UserTypePathHelper,
        protected routeHelper: RouteHelper,
        protected productFeatureService: ProductFeatureSettingService,
        protected permissionService: PermissionService,
        private eventService: EventService,
    ) {
        this.subscriptions.push(this.appConfigService.appConfigSubject.subscribe((appConfig: AppConfig) => {
            this.tenantName = appConfig.portal.tenantName;
            this.organisationName = appConfig.portal.organisationName;
            this.portalTitle = appConfig.portal.title;
            this.hasPortalTitle = !StringHelper.isNullOrWhitespace(this.portalTitle);
            this.hasPolicyFeature = this.featureSettingService.hasPolicyFeature();
            this.hasClaimFeature = this.featureSettingService.hasClaimFeature();
            this.hasQuoteFeature = this.featureSettingService.hasQuoteFeature();
        }));

        this.eventService.featureSettingChangedSubject$.subscribe(async () => {
            await this.load();
        });
    }

    public ngOnInit(): void {
        this.portalTenantAlias = this.routeHelper.getParam('portalTenantAlias');
        this.load();
    }

    public ngOnDestroy(): void {
        this.subscriptions.forEach((s: SubscriptionLike) => s.unsubscribe());
    }

    private async load(): Promise<void> {
        this.hasPolicyFeature = this.featureSettingService.hasPolicyFeature();
        this.hasClaimFeature = this.featureSettingService.hasClaimFeature();
        this.hasQuoteFeature = this.featureSettingService.hasQuoteFeature();
        await this.loadWidgetResources();
    }

    private loadPolicies(): void {
        let params: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
        params.set('status', [PolicyStatus.Issued, PolicyStatus.Active]);
        params.set('pageSize', '3');
        this.subscriptions.push(this.policyApiService.getList(params)
            .pipe(finalize(() => this.isPolicyLoading = false))
            .subscribe((data: Array<PolicyResourceModel>) => {
                if (data) {
                    this.policies = [];
                    data.forEach((policy: PolicyResourceModel) => {
                        this.policies.push(new PolicyViewModel(policy));
                    });
                }
            }));
    }

    private loadPoliciesForRenewal(): void {
        let policyObservable: Observable<Array<PolicyResourceModel>>;
        if (this.portalTenantAlias == 'demo' || this.portalTenantAlias == 'demos') {
            let params: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
            params.set('status', [
                PolicyStatus.Issued, PolicyStatus.Active, PolicyStatus.Renewed, PolicyStatus.Adjusted,
            ]);
            params.set('pageSize', '3');
            policyObservable = this.policyApiService.getList(params);
        } else {
            policyObservable = this.policyApiService.getForRenewalPolicyList();
        }
        this.subscriptions.push(policyObservable
            .pipe(finalize(() => this.isRenewalPolicyLoading = false))
            .subscribe((data: Array<PolicyResourceModel>) => {
                if (data) {
                    this.policiesForRenewal = [];
                    data.forEach(async (policy: PolicyResourceModel) => {
                        this.hasRenewProductFeature =
                            await this.productFeatureService.productHasRenewFeature(policy.productId);
                        if (this.hasRenewProductFeature) {
                            this.policiesForRenewal.push(new PolicyViewModel(policy));
                        }
                    });
                }
            }));
    }

    private loadClaims(): void {
        let params: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
        params.set('status', ClaimHelper.status.Active);
        params.set('pageSize', '3');

        this.subscriptions.push(this.claimApiService.getList(params)
            .pipe(finalize(() => this.isClaimsLoading = false))
            .subscribe((data: Array<ClaimResourceModel>) => {
                if (data) {
                    this.claims = [];
                    data.forEach((claim: ClaimResourceModel) => {
                        this.claims.push(new ClaimViewModel(claim));
                    });
                }
            }));
    }

    private loadQuotes(): void {
        let params: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
        params.set('status', [
            QuoteState.Incomplete, QuoteState.Review, QuoteState.Endorsement, QuoteState.Approved,
        ]);
        params.set('pageSize', '3');
        this.subscriptions.push(this.quoteApiService.getList(params)
            .pipe(finalize(() => this.isQuoteLoading = false))
            .subscribe((data: Array<QuoteResourceModel>) => {
                if (data) {
                    this.quotes = [];
                    data.forEach((quote: QuoteResourceModel) => {
                        this.quotes.push(new QuoteViewModel(quote));
                    });
                }
            }));
    }

    public isSubscriptionClosed(item: any): boolean {
        if (this.subscription) {
            return this.subscription[item] && this.subscription[item].closed;
        }
    }

    private async loadWidgetResources(): Promise<void> {
        if (this.hasPolicyFeature
            && this.permissionService.hasViewPolicyPermission()
        ) {
            this.loadPolicies();
            this.loadPoliciesForRenewal();
        }
        if (this.hasClaimFeature
            && this.permissionService.hasViewClaimPermission()
        ) {
            this.loadClaims();
        }
        if (this.hasQuoteFeature
            && this.permissionService.hasViewQuotePermission()
        ) {
            this.loadQuotes();
        }
    }
}
