import { Component, ElementRef, Injector, OnDestroy } from "@angular/core";
import { FormBuilder, FormGroup } from "@angular/forms";
import { FormValidatorHelper } from "@app/helpers/form-validator.helper";
import { FormHelper } from "@app/helpers/form.helper";
import { RouteHelper } from "@app/helpers/route.helper";
import { AppConfig } from "@app/models/app-config";
import { DetailsListFormItem } from "@app/models/details-list/details-list-form-item";
import { DetailsListFormTextAreaItem } from "@app/models/details-list/details-list-form-text-area-item";
import { DetailsListFormTextItem } from "@app/models/details-list/details-list-form-text-item";
import { DetailsListItemCard } from "@app/models/details-list/details-list-item-card";
import { EntityEditFieldOption } from "@app/models/entity-edit-field-option";
import { IconLibrary } from "@app/models/icon-library.enum";
import { DetailPage } from "@app/pages/master-detail/detail.page";
import { PortalStyleSettingsResourceModel } from "@app/resource-models/portal/portal-style-settings.resource-model";
import { PortalResourceModel } from "@app/resource-models/portal/portal.resource-model";
import { PortalApiService } from "@app/services/api/portal-api.service";
import { AppConfigService } from "@app/services/app-config.service";
import { EventService } from "@app/services/event.service";
import { NavProxyService } from "@app/services/nav-proxy.service";
import { SharedAlertService } from "@app/services/shared-alert.service";
import { SharedLoaderService } from "@app/services/shared-loader.service";
import { scrollbarStyle } from "@assets/scrollbar";
import { Subject, Subscription } from "rxjs";
import { finalize } from "rxjs/operators";

/**
 * Allows the setting of a portal location, which is the url which the portal will be
 * embedded. There is only one URL per environment, and this is so that links
 * generated can go to the correct portal URL.
 * Additionaly, the Portal URL is used for generating the CSP header to stop framejacking.
 */
@Component({
    selector: 'app-edit-portal-theme',
    templateUrl: './edit-portal-theme.page.html',
    styleUrls: [
        '../../../../assets/css/scrollbar-form.css',
        '../../../../assets/css/form-toolbar.scss',
    ],
    styles: [ scrollbarStyle ],
})
export class EditPortalThemePage extends DetailPage implements OnDestroy {
    public tenantAlias: string;
    private portalId: string;
    protected destroyed: Subject<void>;
    private model: PortalResourceModel;
    public form: FormGroup;
    public isLoading: boolean = true;
    public errorMessage: string = '';
    public detailsList: Array<DetailsListFormItem>;
    public fieldOptions: Array<EntityEditFieldOption> = [];
    public title: string = 'Portal Theme';
    public isEmbeddingExternally: boolean = false;

    public constructor(
        private routeHelper: RouteHelper,
        private portalApiService: PortalApiService,
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

    public ionViewWillEnter(): void {
        this.portalId = this.routeHelper.getParam('portalId');
        let tenantAliasFromUrl: string = this.routeHelper.getParam('tenantAlias');
        this.tenantAlias = tenantAliasFromUrl ? tenantAliasFromUrl : this.tenantAlias;
        this.destroyed = new Subject<void>();
        this.load().then(() => {
            this.title = this.model.name + ' - Portal Theme';
            this.prepareForm();
            this.form = this.buildForm();
            this.setFormValue(this.model);
        });
    }

    protected prepareForm(): void {
        const stylingCard: DetailsListItemCard = new DetailsListItemCard(
            'Styling',
            'Stying the portal');
        this.detailsList = new Array<DetailsListFormItem>();
        this.detailsList.push(DetailsListFormTextItem.create(
            stylingCard,
            'stylesheetUrl',
            'External Stylesheet Url')
            .withIcon('globe', IconLibrary.IonicV4)
            .withValidator(FormValidatorHelper.url()));
        this.detailsList.push(DetailsListFormTextAreaItem.create(
            stylingCard,
            'styles',
            'Custom Styles (CSS)')
            .withIcon('brush', IconLibrary.IonicV4));
    }

    protected buildForm(): FormGroup {
        let controls: any = [];
        this.detailsList.forEach((item: DetailsListFormItem) => {
            controls[item.Alias] = item.FormControl;
        });
        const form: FormGroup = this.formBuilder.group(controls);
        return form;
    }

    public async load(): Promise<void> {
        this.isLoading = true;
        const params: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
        params.set("tenant", this.tenantAlias);
        params.set("useCache", 'false');
        try {
            await this.portalApiService.getById(this.portalId, params)
                .pipe(finalize(() => this.isLoading = false))
                .toPromise()
                .then((data: PortalResourceModel) => {
                    this.model = data;
                });
        } catch (err) {
            this.errorMessage = 'There was a problem loading the portal details';
            throw err;
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
        this.update(value);
    }

    private returnToPrevious(): void {
        let pathSegments: Array<string> = this.routeHelper.getPathSegmentsUpUntil('portal');
        pathSegments.push(this.portalId);
        this.navProxy.navigateBack(pathSegments, true, { queryParams: { segment: "Settings" } });
    }

    private async update(value: any): Promise<void> {
        await this.sharedLoaderService.presentWithDelay();

        let updateModel: PortalStyleSettingsResourceModel = {
            tenant: this.tenantAlias,
            stylesheetUrl: value.stylesheetUrl,
            styles: value.styles,
        };
        const subscription: Subscription = this.portalApiService.updatePortalStyles(this.model.id, updateModel)
            .pipe(finalize(() => {
                this.sharedLoaderService.dismiss();
                subscription.unsubscribe();
            }))
            .subscribe((portal: PortalResourceModel) => {
                if (portal) { // will be null if we navigate away whilst loading
                    this.eventService.getEntityUpdatedSubject('Portal').next(portal);
                    this.sharedAlert.showToast(
                        `The portal styling settings for the ${portal.name} portal have been updated`);
                    this.returnToPrevious();
                }
            });
    }

    private setFormValue(model: PortalResourceModel): void {
        this.form.patchValue({
            stylesheetUrl: model.stylesheetUrl,
            styles: model.styles,
        });
    }
}
