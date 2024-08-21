import { Component, Injector, ElementRef, Renderer2, ChangeDetectorRef, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { finalize, takeUntil } from 'rxjs/operators';
import { ClaimApiService } from '@app/services/api/claim-api.service';
import { NavProxyService } from '@app/services/nav-proxy.service';
import { scrollbarStyle } from '@assets/scrollbar';
import { LayoutManagerService } from '@app/services/layout-manager.service';
import { UserTypePathHelper } from '@app/helpers/user-type-path.helper';
import { EventService } from '@app/services/event.service';
import { ClaimResourceModel } from '@app/resource-models/claim.resource-model';
import { SplitLayoutManager } from '@app/models/split-layout-manager';
import { AppConfigService } from '@app/services/app-config.service';
import { DomSanitizer } from '@angular/platform-browser';
import { ErrorHandlerService } from '@app/services/error-handler.service';
import { FormsAppClaimPage } from '@pages/forms-app/forms-app-claim.page';
import { WebFormHelper } from '@app/helpers/webform.helper';
import { RouteHelper } from '@app/helpers/route.helper';
import { HttpErrorResponse } from '@angular/common/http';
import { PermissionService } from '@app/services/permission.service';
import { BrowserDetectionService } from '@app/services/browser-detection.service';
import { PolicyApiService } from '@app/services/api/policy-api.service';
import { fromEvent } from 'rxjs';
import { SharedLoaderService } from '@app/services/shared-loader.service';
import { AuthenticationService } from '@app/services/authentication.service';

/**
 * Export create claim page component class
 * TODO: Write a better class header: creation of cliams.
 */
@Component({
    selector: 'app-create-claim',
    templateUrl: '../../forms-app/forms-app.page.html',
    styleUrls: [
        '../../../../assets/css/scrollbar-segment.css',
        '../../forms-app/forms-app.page.scss',
    ],
    styles: [
        scrollbarStyle,
    ],
})
export class CreateClaimPage extends FormsAppClaimPage implements OnInit, SplitLayoutManager {
    private customerId: string;

    public constructor(
        public appConfigService: AppConfigService,
        public claimApiService: ClaimApiService,
        public navProxy: NavProxyService,
        public route: ActivatedRoute,
        public layoutManager: LayoutManagerService,
        protected userPath: UserTypePathHelper,
        eventService: EventService,
        elementRef: ElementRef,
        injector: Injector,
        sanitizer: DomSanitizer,
        protected render: Renderer2,
        errorHandlerService: ErrorHandlerService,
        changeDetectorRef: ChangeDetectorRef,
        routeHelper: RouteHelper,
        public permissionService: PermissionService,
        browserDetectionService: BrowserDetectionService,
        protected policyApiService: PolicyApiService,
        protected sharedLoaderService: SharedLoaderService,
        authenticationService: AuthenticationService,
    ) {
        super(
            appConfigService,
            navProxy,
            layoutManager,
            userPath,
            eventService,
            elementRef,
            injector,
            sanitizer,
            render,
            errorHandlerService,
            changeDetectorRef,
            claimApiService,
            route,
            routeHelper,
            permissionService,
            browserDetectionService,
            policyApiService,
            sharedLoaderService,
            authenticationService,
        );
    }

    public ngOnInit(): void {
        this.policyId = this.routeHelper.getParam('policyId');
        this.customerId = this.routeHelper.getParam('customerId');
        this.title = this.action = 'Create Claim';
        super.ngOnInit();
        this.createClaim();
        fromEvent(window, "message")
            .pipe(takeUntil(this.destroyed))
            .subscribe((e: MessageEvent) => {
                if (e.data.messageType === 'claimReferenceAllocated') {
                    this.claimReference = e.data.payload;
                    this.title = `${this.action}${this.policyNumber ? ` on Policy ${this.policyNumber}` : ''} `
                        + `for ${this.productName} ${this.claimReference ? `(${this.claimReference})` : ''}`;
                }
            });
    }

    private createClaim(): void {
        if (this.policyId) {
            this.createClaimForPolicy();
        } else if (this.productAlias) {
            this.createClaimForProduct();
        } else {
            throw new Error('Unable to create a claim because we don\'t have a policyId or a productAlias.');
        }
    }

    private createClaimForPolicy(): void {
        this.title = `${this.action} on Policy`;
        this.sharedLoaderService.presentWithDelay('Please wait...')
            .then((result: void) => {
                this.claimApiService.createClaimForPolicy(this.policyId)
                    .pipe(
                        finalize(() => this.sharedLoaderService.dismiss()),
                        takeUntil(this.destroyed),
                    )
                    .subscribe((data: ClaimResourceModel) => {
                        this.claimId = data.id;
                        this.loadClaimFormsApp(WebFormHelper.modes.Create);
                    }, (error: HttpErrorResponse) => {
                        // navigate back so we're not left on a blank page.
                        this.closeButtonClicked();
                        throw error;
                    });
            });
    }

    private createClaimForProduct(): void {
        this.sharedLoaderService.presentWithDelay('Please wait...')
            .then((result: void) => {
                this.claimApiService.createClaimForProduct(this.productAlias, this.customerId)
                    .pipe(
                        finalize(() => this.sharedLoaderService.dismiss()),
                        takeUntil(this.destroyed),
                    )
                    .subscribe((data: ClaimResourceModel) => {
                        this.claimId = data.id;
                        this.loadClaimFormsApp(WebFormHelper.modes.Create);
                    }, (error: HttpErrorResponse) => {
                        // navigate back so we're not left on a blank page.
                        this.closeButtonClicked();
                        throw error;
                    });
            });
    }
}
