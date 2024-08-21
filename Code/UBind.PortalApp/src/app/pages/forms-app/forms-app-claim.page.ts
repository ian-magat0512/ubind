import { FormsAppPage } from "@pages/forms-app/forms-app.page";
import { NavProxyService } from "@app/services/nav-proxy.service";
import { AppConfigService } from "@app/services/app-config.service";
import { LayoutManagerService } from "@app/services/layout-manager.service";
import { UserTypePathHelper } from "@app/helpers/user-type-path.helper";
import { EventService } from "@app/services/event.service";
import { ElementRef, Injector, Renderer2, ChangeDetectorRef, OnDestroy, Directive, OnInit } from "@angular/core";
import { DomSanitizer } from "@angular/platform-browser";
import { ErrorHandlerService } from "@app/services/error-handler.service";
import { ClaimApiService } from "@app/services/api/claim-api.service";
import { finalize, takeUntil } from "rxjs/operators";
import { PermissionDataModel, Permission } from "@app/helpers";
import { ClaimResourceModel } from "@app/resource-models/claim.resource-model";
import { ActivatedRoute } from "@angular/router";
import { RouteHelper } from "@app/helpers/route.helper";
import { PermissionService } from "@app/services/permission.service";
import { Errors } from "@app/models/errors";
import { BrowserDetectionService } from "@app/services/browser-detection.service";
import { PolicyApiService } from "@app/services/api/policy-api.service";
import { SharedLoaderService } from "@app/services/shared-loader.service";
import { AuthenticationService } from "@app/services/authentication.service";

/**
 * Export forms app claim page abstract class
 * TODO: Write a better class header: functions for the Form apps claims.
 */
@Directive({
    selector: '[formsAppClaimPage]',
})
export class FormsAppClaimPage extends FormsAppPage implements OnInit, OnDestroy {

    protected claimReference: string;
    protected productName: string;
    protected policyNumber: string;

    public constructor(
        public appConfigService: AppConfigService,
        public navProxy: NavProxyService,
        public layoutManager: LayoutManagerService,
        protected userPath: UserTypePathHelper,
        eventService: EventService,
        elementRef: ElementRef,
        injector: Injector,
        sanitizer: DomSanitizer,
        protected render: Renderer2,
        errorHandlerService: ErrorHandlerService,
        changeDetectorRef: ChangeDetectorRef,
        public claimApiService: ClaimApiService,
        public route: ActivatedRoute,
        routeHelper: RouteHelper,
        public permissionService: PermissionService,
        browserDetectionService: BrowserDetectionService,
        protected policyApiService: PolicyApiService,
        protected sharedLoaderService: SharedLoaderService,
        authenticationService: AuthenticationService,
    ) {
        super(
            appConfigService,
            layoutManager,
            sanitizer,
            errorHandlerService,
            eventService,
            elementRef,
            render,
            injector,
            changeDetectorRef,
            routeHelper,
            browserDetectionService,
            authenticationService,
        );
    }

    public ngOnInit(): void {
        super.ngOnInit();

        this.claimId = this.route.snapshot.paramMap.get('claimId') || null;
        this.productAlias = this.route.snapshot.queryParamMap.get('productAlias');
        this.formType = 'claim';
    }

    public goBackButtonClicked(): void {
        if (this.claimId) {
            this.navProxy.navigateBack([this.userPath.claim, this.claimId]);
            this.refreshClaim();
        } else {
            this.navProxy.navigateBack([this.userPath.claim, 'list']);
        }
    }

    public closeButtonClicked(): void {
        if (this.complete) {
            this.navProxy.navigateBack([this.userPath.claim, this.claimId]);
            this.refreshClaim();
        } else {
            return this.goBackButtonClicked();
        }
    }

    public loadClaimFormsApp(mode: string = null, version: string = null): void {
        this.sharedLoaderService.presentWithDelay('Please wait...')
            .then((result: void) => {
                this.claimApiService.getById(this.claimId)
                    .pipe(
                        finalize(() => this.sharedLoaderService.dismiss()),
                        takeUntil(this.destroyed),
                    )
                    .subscribe((claim: ClaimResourceModel) => {
                        let hasPermission: boolean = this.hasPermission(claim);
                        this.policyNumber = claim.policyNumber;
                        this.productName = claim.productName;
                        this.claimReference = claim.claimReference;
                        this.title = `${this.action}${this.policyNumber ? ` on Policy ${this.policyNumber}` : ''} `
                            + `for ${this.productName} ${this.claimReference ? `(${this.claimReference})` : ''}`;

                        if (!hasPermission) {
                            throw Errors.User.AccessDenied("claim");
                        }

                        this.loadFormsApp(mode, null, version);
                    });
            });
    }

    public hasPermission(claim: ClaimResourceModel): boolean {
        let permissionModel: PermissionDataModel = {
            organisationId: claim.organisationId,
            ownerUserId: claim.ownerUserId,
            customerId: claim.customerDetails ? claim.customerDetails.id : null,
        };

        return this.permissionService.hasElevatedPermissionsViaModel(
            Permission.ManageClaims,
            permissionModel,
        );
    }

    private refreshClaim(): void {
        this.claimApiService.getById(this.claimId)
            .pipe(takeUntil(this.destroyed))
            .subscribe((claim: ClaimResourceModel) => {
                this.eventService.getEntityUpdatedSubject('Claim').next(claim);
            });
    }
}
