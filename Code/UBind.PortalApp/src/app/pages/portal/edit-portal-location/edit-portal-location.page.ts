import { Component, ElementRef, Injector, OnDestroy, OnInit } from "@angular/core";
import { FormBuilder, FormGroup } from "@angular/forms";
import { StringHelper } from "@app/helpers";
import { FormValidatorHelper } from "@app/helpers/form-validator.helper";
import { FormHelper } from "@app/helpers/form.helper";
import { RouteHelper } from "@app/helpers/route.helper";
import { AppConfig } from "@app/models/app-config";
import { DeploymentEnvironment } from "@app/models/deployment-environment.enum";
import { DetailsListFormContentItem } from "@app/models/details-list/details-list-form-content-item";
import { DetailsListFormItem } from "@app/models/details-list/details-list-form-item";
import { DetailsListFormRadioItem } from "@app/models/details-list/details-list-form-radio-item";
import { DetailsListFormTextItem } from "@app/models/details-list/details-list-form-text-item";
import { DetailsListItemCard } from "@app/models/details-list/details-list-item-card";
import { EntityEditFieldOption } from "@app/models/entity-edit-field-option";
import { PortalAppContextModel } from "@app/models/portal-app-context.model";
import { DetailPage } from "@app/pages/master-detail/detail.page";
import { PortalResourceModel } from "@app/resource-models/portal.resource-model";
import { AppContextApiService } from "@app/services/api/app-context-api.service";
import { PortalApiService } from "@app/services/api/portal-api.service";
import { AppConfigService } from "@app/services/app-config.service";
import { EventService } from "@app/services/event.service";
import { NavProxyService } from "@app/services/nav-proxy.service";
import { SharedAlertService } from "@app/services/shared-alert.service";
import { SharedLoaderService } from "@app/services/shared-loader.service";
import { scrollbarStyle } from "@assets/scrollbar";
import { Subject, Subscription } from "rxjs";
import { finalize, takeUntil } from "rxjs/operators";

/**
 * Allows the setting of a portal location, which is the url which the portal will be
 * embedded. There is only one URL per environment, and this is so that links
 * generated can go to the correct portal URL.
 * Additionaly, the Portal URL is used for generating the CSP header to stop framejacking.
 */
@Component({
    selector: 'app-edit-portal-location',
    templateUrl: './edit-portal-location.page.html',
    styleUrls: [
        '../../../../assets/css/scrollbar-form.css',
        '../../../../assets/css/form-toolbar.scss',
    ],
    styles: [ scrollbarStyle ],
})
export class EditPortalLocationPage extends DetailPage implements OnInit, OnDestroy {
    public tenantAlias: string;
    private portalAlias: string;
    private portalId: string;
    protected destroyed: Subject<void>;
    private portalModel: PortalResourceModel;
    private portalAppContextModel: PortalAppContextModel;
    public form: FormGroup;
    public isLoading: boolean = true;
    public errorMessage: string = '';
    public detailsList: Array<DetailsListFormItem>;
    public fieldOptions: Array<EntityEditFieldOption> = [];
    public title: string = 'Portal Location';
    private environment: string;
    public isEmbeddingExternally: boolean = false;

    public constructor(
        private routeHelper: RouteHelper,
        private portalApiService: PortalApiService,
        private appContextApiService: AppContextApiService,
        private formBuilder: FormBuilder,
        private navProxy: NavProxyService,
        private formHelper: FormHelper,
        private sharedLoaderService: SharedLoaderService,
        protected eventService: EventService,
        private sharedAlert: SharedAlertService,
        private appConfigService: AppConfigService,
        elementRef: ElementRef,
        public injector: Injector,
    ) {
        super(eventService, elementRef, injector);
        this.appConfigService.appConfigSubject.subscribe((appConfig: AppConfig) => {
            if (!this.appConfigService.isMasterPortal()) {
                this.tenantAlias = appConfig.portal.tenantAlias;
            }
        });
    }

    public ngOnDestroy(): void {
        this.destroyed.next();
        this.destroyed.complete();
    }

    public ngOnInit(): void {
        this.portalId = this.routeHelper.getParam('portalId');
        let tenantAliasFromUrl: string = this.routeHelper.getParam('tenantAlias');
        this.environment = this.routeHelper.getParam('environment');
        this.title = StringHelper.toTitleCase(this.environment) + ' Portal Location';
        this.tenantAlias = tenantAliasFromUrl ? tenantAliasFromUrl : this.tenantAlias;
        this.destroyed = new Subject<void>();
        this.load().then(() => {
            this.prepareForm();
            this.form = this.buildForm();
            this.setFormValue(this.portalModel);
        });
    }

    protected prepareForm(): void {
        const detailsCard: DetailsListItemCard = new DetailsListItemCard(
            'Portal Location',
            'The URL of the portal. This is used to generate the CSP header to stop framejacking. There is only '
            + 'one URL per environment, and this is so that links generated can go to the correct portal URL.');
        this.detailsList = new Array<DetailsListFormItem>();
        this.detailsList.push(DetailsListFormRadioItem.create(
            detailsCard,
            'locationType',
            'Select Portal Location')
            .withoutSectionIcons()
            .withHeader('Setting Portal Location')
            .withParagraph("When you choose to embed this portal on an external web page URL, that web page must "
                + "include a uBind JavaScript library and a portal embed tag.")
            .withParagraph<DetailsListFormRadioItem>("If you don't have access to an external web page yet, or if you "
                + "do not need to embed it on an external web page, then you can simply choose to access it using a "
                + "uBind application URL.")
            .withOption({ label: 'Access this portal from a uBind application URL', value: 'applicationUrl' })
            .withOption({ label: 'Embed this portal on an external web page', value: 'embedExternal' }));
        this.detailsList.push(DetailsListFormTextItem.create(
            detailsCard,
            'url',
            'External Web Page Url')
            .withValidator(FormValidatorHelper.webUrl(true)));

        const instructionsCard: DetailsListItemCard = new DetailsListItemCard(
            'Instructions',
            'Instructions on how to access or embed this portal.');

        let baseUrl: string = this.portalAppContextModel.appBaseUrl;
        baseUrl = baseUrl.endsWith('/') ? baseUrl : baseUrl + '/';
        this.detailsList.push(DetailsListFormContentItem.create(
            instructionsCard,
            'embedInstructions',
            null)
            .withoutSectionIcons()
            .withGroupName('embedInstructions')
            .withHeader<DetailsListFormContentItem>('Embedding This Portal')
            .withHtmlContent(`<p>To embed this portal on the external web page, please ensure that:</p>`
                + `<ol><li>`
                + `The &lt;head&gt; tag of the html document includes the following uBind Javascript library:`
                + `<div class="code-html"><code>&lt;script type="text/javascript" `
                + ` src="${baseUrl}assets/ubind.js"&gt; &lt;/script&gt; `
                + `</code></div>`
                + `</li>`
                + `<li>`
                + `The &lt;body&gt; tag of the html document includes the following portal embed tag:`
                + `<div class="code-html"><code>&lt;div class="ubind-portal" `
                + `data-tenant="${this.tenantAlias}" `
                + `data-portal="${this.portalAppContextModel.portalAlias}" `
                + `data-environment="${this.environment}"&gt;&lt;/div&gt;`
                + `</code></div>`
                + `</li></ol>`));
        this.detailsList.push(DetailsListFormContentItem.create(
            instructionsCard,
            'accessibleFrom',
            null)
            .withoutSectionIcons()
            .withGroupName('accessInstructions')
            .withHeader<DetailsListFormContentItem>('Portal Location')
            .withHtmlContent(`<p>Based upon your selection, this portal will be accessible from the following `
                + `uBind application URL:</p>`
                + `<div class="code-html"><code>${this.getDefaultPortalLocation()}</code></div>`));
    }

    private getDefaultPortalLocation(): string {
        let url: string = this.portalModel.defaultUrl;
        if (this.environment.toLowerCase() != DeploymentEnvironment.Production.toLowerCase()) {
            url += '?environment=' + this.environment.toLowerCase();
        }
        return url;
    }

    protected buildForm(): FormGroup {
        let controls: any = [];
        this.detailsList.forEach((item: DetailsListFormItem) => {
            controls[item.Alias] = item.FormControl;
        });
        const form: FormGroup = this.formBuilder.group(controls);

        // Hide the URL field if the user selects the application URL option.
        // Also hide/show any associated instructions
        form.get('locationType').valueChanges.pipe(takeUntil(this.destroyed)).subscribe((value: string) => {
            this.isEmbeddingExternally = (value == 'embedExternal');
            if (!this.isEmbeddingExternally) {
                if (this.form.controls['url']) {
                    this.form.removeControl('url');
                }
            } else {
                if (!this.form.controls['url']) {
                    this.form.addControl(
                        'url',
                        this.detailsList.find((item: DetailsListFormItem) => item.Alias == 'url').FormControl);
                }
            }
            this.detailsList.find((item: DetailsListFormItem) => item.Alias == 'url').Visible
                = this.isEmbeddingExternally;
            this.detailsList.find((item: DetailsListFormItem) => item.Alias == 'embedInstructions').Visible
                = this.isEmbeddingExternally;
            this.detailsList.find((item: DetailsListFormItem) => item.Alias == 'accessibleFrom').Visible
                = !this.isEmbeddingExternally;
        });
        return form;
    }

    public async load(): Promise<void> {
        this.isLoading = true;
        const params: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
        params.set("tenant", this.tenantAlias);
        params.set("useCache", 'false');
        try {
            await this.portalApiService.getById(this.portalId, params)
                .toPromise()
                .then((data: PortalResourceModel) => {
                    this.portalAlias = data.alias;
                    this.portalModel = data;
                })
                .then(async () => {
                    await this.appContextApiService
                        .getPortalAppContext(this.tenantAlias, this.portalModel.organisationId)
                        .toPromise()
                        .then((data: PortalAppContextModel) => {
                            this.portalAppContextModel = data;
                        });
                });
        } catch (err) {
            this.errorMessage = 'There was a problem loading the portal details';
            throw err;
        } finally {
            this.isLoading = false;
        }
    }

    // closes the create-edit page.
    public async close(): Promise<void> {
        if (this.form.dirty) {
            if (!await this.formHelper.confirmExitWithUnsavedChanges()) {
                return;
            }
        }
        this.returnToPrevious();
    }

    public async save(value: any): Promise<void> {
        if (this.form.invalid) {
            return;
        }
        const url: string = !value
            ? ''
            : value.locationType != 'embedExternal'
                ? ''
                : value.url;
        this.update(url);
    }

    private returnToPrevious(): void {
        let pathSegments: Array<string> = this.routeHelper.getPathSegmentsUpUntil('portal');
        pathSegments.push(this.portalId);
        this.navProxy.navigateBack(pathSegments, true, { queryParams: { segment: "Settings" } });
    }

    private async update(url: string): Promise<void> {
        await this.sharedLoaderService.presentWithDelay();
        url = this.addSchemeToUrlIfItDoesntHaveOne(url);
        const subscription: Subscription = this.portalApiService.updatePortalLocation(
            this.portalModel.id, this.environment, url, this.tenantAlias)
            .pipe(finalize(() => {
                this.sharedLoaderService.dismiss();
                subscription.unsubscribe();
            }))
            .subscribe((portal: PortalResourceModel) => {
                if (portal) { // will be null if we navigate away from the page whilst loading
                    this.eventService.getEntityUpdatedSubject('Portal').next(portal);
                    this.sharedAlert.showToast(
                        `The ${this.environment} portal location for the ${portal.name} portal has been updated`);
                    this.returnToPrevious();
                }
            });
    }

    private addSchemeToUrlIfItDoesntHaveOne(url: string): string {
        if (!url) {
            return url;
        }
        if (!url.startsWith('http://') && !url.startsWith('https://')) {
            url = 'https://' + url;
        }
        return url;
    }

    private setFormValue(model: PortalResourceModel): void {
        let url: string = null;
        switch (this.environment.toLowerCase()) {
            case DeploymentEnvironment.Production.toLowerCase():
                url = model.productionUrl;
                break;
            case DeploymentEnvironment.Staging.toLowerCase():
                url = model.stagingUrl;
                break;
            case DeploymentEnvironment.Development.toLowerCase():
                url = model.developmentUrl;
                break;
        }
        this.form.patchValue({
            locationType: url ? 'embedExternal' : 'applicationUrl',
            url: url,
        });
    }
}
