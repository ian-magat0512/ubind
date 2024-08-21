import { AppConfigService } from "@app/services/app-config.service";
import { Subject, SubscriptionLike } from "rxjs";
import { AppConfig } from "@app/models/app-config";
import { Directive, OnDestroy, OnInit } from "@angular/core";
import { PortalApiService } from "@app/services/api/portal-api.service";
import { StringHelper } from "@app/helpers";
import { Title } from "@angular/platform-browser";

/**
 * An entry page is a page that a users enters the portal through.
 * E.g. login page, password reset page etc
 */
@Directive({
    selector: '[appEntryPage]',
})
export abstract class EntryPage implements OnDestroy, OnInit {
    protected portalTenantId: string;
    protected portalOrganisationId: string;
    public tenantName: string;
    public organisationName: string;
    public portalTitle: string;
    public portalId: string;
    public hasPortalTitle: boolean = false;
    public isLoading: boolean = false;
    public environment: string = '';
    protected subscriptions: Array<SubscriptionLike> = new Array<SubscriptionLike>();
    public isDefaultOrganisation: boolean = false;
    public destroyed: Subject<void>;

    public constructor(
        protected appConfigService: AppConfigService,
        protected portalApiService: PortalApiService,
        protected titleService: Title,
    ) {

        this.subscriptions.push(this.appConfigService.appConfigSubject.subscribe(
            (appConfig: AppConfig) => {
                this.portalTenantId = appConfig.portal.tenantId;
                this.portalOrganisationId = appConfig.portal.organisationId;
                this.tenantName = appConfig.portal.tenantName;
                this.organisationName = appConfig.portal.organisationName;
                this.isDefaultOrganisation = appConfig.portal.isDefaultOrganisation;
                this.portalTitle = appConfig.portal.title;
                this.portalId = appConfig.portal.portalId;
                this.hasPortalTitle = !StringHelper.isNullOrWhitespace(this.portalTitle);
                this.environment = appConfig.portal.environment;
                this.titleService.setTitle(appConfig.portal.title || this.organisationName || this.tenantName);
            },
        ));
    }

    public ngOnInit(): void {
        this.destroyed = new Subject<void>();
    }

    public ngOnDestroy(): void {
        this.subscriptions.forEach((s: SubscriptionLike) => s.unsubscribe());
        if (this.destroyed) {
            this.destroyed.next();
            this.destroyed.complete();
        }
    }

}
