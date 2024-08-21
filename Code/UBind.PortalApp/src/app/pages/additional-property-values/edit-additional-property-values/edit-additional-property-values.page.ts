import { ElementRef, Component, Injector, OnDestroy, OnInit } from "@angular/core";
import { FormBuilder, FormGroup } from "@angular/forms";
import { AdditionalPropertyValueUpsertResourceModel } from "@app/resource-models/additional-property.resource-model";
import { PolicyTransactionDetailResourceModel } from "@app/resource-models/policy.resource-model";
import { PolicyHelper, StringHelper } from "@app/helpers";
import { AdditionalPropertiesHelper } from "@app/helpers/additional-properties.helper";
import { FormHelper } from "@app/helpers/form.helper";
import { RouteHelper } from "@app/helpers/route.helper";
import { AdditionalPropertyValue } from "@app/models/additional-property-item-view.model";
import { DetailsListFormItem } from "@app/models/details-list/details-list-form-item";
import { EntityType } from "@app/models/entity-type.enum";
import { PolicyTransactionEventNamePastTense } from "@app/models/policy-transaction-event-name-past-tense.enum";
import { DetailPage } from "@app/pages/master-detail/detail.page";
import { AdditionalPropertyValueService } from "@app/services/additional-property-value.service";
import { AdditionalPropertyValueApiService } from "@app/services/api/additional-property-value-api.service";
import { PolicyApiService } from "@app/services/api/policy-api.service";
import { AuthenticationService } from "@app/services/authentication.service";
import { EventService } from "@app/services/event.service";
import { LayoutManagerService } from "@app/services/layout-manager.service";
import { NavProxyService } from "@app/services/nav-proxy.service";
import { SharedLoaderService } from "@app/services/shared-loader.service";
import { contentAnimation } from "@assets/animations";
import { scrollbarStyle } from "@assets/scrollbar";
import { ToastController } from "@ionic/angular";
import { Subject } from "rxjs";
import { finalize, takeUntil } from "rxjs/operators";

/**
 * Class for editing additional property values page. The creation is being done in the backend as part of aggregate
 * This class contains all the functionalities in editing additional properties.
 */
@Component({
    selector: 'app-edit-additional-property-value',
    templateUrl: './edit-additional-property-values.page.html',
    styleUrls: [
        '../../../../assets/css/scrollbar-form.css',
        '../../../../assets/css/form-toolbar.scss',
    ],
    animations: [contentAnimation],
    styles: [
        scrollbarStyle,
    ],
})

export class AdditionalPropertyValueEdit extends DetailPage implements OnInit, OnDestroy {
    public detailList: Array<DetailsListFormItem> = [];
    public title: string;
    public form: FormGroup;
    private entityType: EntityType;
    private entityId: string;
    private additionalPropertyValues: Array<AdditionalPropertyValue>;

    public constructor(
        public eventService: EventService,
        elementRef: ElementRef,
        injector: Injector,
        public layoutManager: LayoutManagerService,
        private formBuilder: FormBuilder,
        private toastCtrl: ToastController,
        public routeHelper: RouteHelper,
        public navProxy: NavProxyService,
        private sharedLoaderService: SharedLoaderService,
        private formHelper: FormHelper,
        private additionalPropertyValueApiService: AdditionalPropertyValueApiService,
        private additionalPropertyValueService: AdditionalPropertyValueService,
        private authService: AuthenticationService,
        private policyApiService: PolicyApiService,
    ) {
        super(eventService, elementRef, injector);
    }

    public ngOnInit(): void {
        this.initializeProperties();
        this.loadAdditionalPropertyValues();
    }

    private initializeProperties(): void {
        this.destroyed = new Subject<void>();
        const entityTypeAsKebabCase: string = this.routeHelper.getParam('entityType') as string;
        const pascalCaseEntityType: string = StringHelper.toPascalCase(entityTypeAsKebabCase);
        this.entityId = this.routeHelper.getParam('entityId') as string;
        // This happens when the edit additional property values page is active in the right pane and a new entity
        // is clicked in the left pane.
        if (!EntityType[pascalCaseEntityType]) {
            this.redirectToEntityMainRoute(entityTypeAsKebabCase);
            return;
        }
        this.entityType = pascalCaseEntityType as EntityType;
        const titleCase: string = StringHelper.toTitleCase(
            StringHelper.removeDashFromKebabCase(entityTypeAsKebabCase));
        this.setTitle(titleCase);
    }

    private setTitle(titleCase: string): void {
        this.title = `Edit ${titleCase} Properties`;
        if (this.entityType == EntityType.PolicyTransaction) {
            const policyId: string = this.routeHelper.getParam('policyId') as string;
            this.policyApiService.getPolicyTransaction(policyId, this.entityId)
                .pipe(takeUntil(this.destroyed))
                .subscribe((dt: PolicyTransactionDetailResourceModel) => {
                    const eventSummary: PolicyTransactionEventNamePastTense = dt.eventTypeSummary;
                    this.title = PolicyHelper.getEditAdditionalPropertiesPopOverTitle(eventSummary);
                });
        }
    }

    public ngOnDestroy(): void {
        if (this.destroyed) {
            this.destroyed.next();
            this.destroyed.complete();
        }
    }

    private redirectToEntityMainRoute(id: string): void {
        let segments: Array<string> = this.routeHelper.getPathSegmentsUpUntil('additional-property-values');
        segments.pop();
        segments[segments.length - 1] = id;
        this.navProxy.navigateForward(segments);
    }

    private loadAdditionalPropertyValues(): void {
        this.additionalPropertyValueApiService.getValuesByEntityType(
            this.entityType,
            this.entityId,
            this.authService.tenantId,
        )
            .pipe(
                takeUntil(this.destroyed),
                finalize(() => this.isLoading = false),
            )
            .subscribe(
                (result: Array<AdditionalPropertyValue>) => {
                    if (result?.length > 0) {
                        this.additionalPropertyValues = result;
                        let additionalPropertValues: Array<DetailsListFormItem> = AdditionalPropertiesHelper
                            .getDetailListForEdit(this.additionalPropertyValues);
                        this.detailList = this.detailList.concat(additionalPropertValues);
                        this.buildForm();
                        this.setValue();
                        this.additionalPropertyValueService.registerUniqueAdditionalPropertyFieldsOnValueChanges(
                            this.additionalPropertyValues,
                            this.form,
                            this.entityId,
                            this.authService.tenantId,
                            this.detailList,
                        );
                    }
                },
                (err: any) => {
                    this.errorMessage = "There was an error loading additional property values";
                    this.isLoading = false;
                    throw err;
                },
            );
    }

    private buildForm(): void {
        let formConfig: any = [];
        this.detailList.forEach((item: DetailsListFormItem) => {
            formConfig[item.Alias] = item.FormControl;
        });
        this.form = this.formBuilder.group(formConfig);
    }

    private setValue(): void {
        let formWithValue: any = AdditionalPropertiesHelper.setFormValue(
            this.form,
            this.additionalPropertyValues,
            this.additionalPropertyValues,
        );
        this.form.patchValue(formWithValue);
    }

    public async userDidTapSaveButton(value: any): Promise<void> {
        if (!await this.validateAdditionalPropertyValues(value)) {
            return;
        }
        await this.sharedLoaderService.presentWait();
        let properties: Array<AdditionalPropertyValueUpsertResourceModel> = AdditionalPropertiesHelper.buildProperties(
            this.additionalPropertyValues,
            value,
        );
        this.additionalPropertyValueApiService.updatePropertValues(properties, this.entityId, this.entityType)
            .pipe(finalize(() => this.sharedLoaderService.dismiss()))
            .subscribe(
                (result: string) => {
                    this.showSnackbarOnSuccessfulSaved(result);
                    this.returnToPrevious();
                });
    }

    private async validateAdditionalPropertyValues(value: any): Promise<boolean> {
        return this.additionalPropertyValueService.validateAdditionalPropertyValues(
            this.additionalPropertyValues,
            this.entityId,
            this.form,
            value,
            this.authService.tenantId,
        );
    }

    public async userDidTapCloseButton(): Promise<void> {
        if (this.form.dirty) {
            if (!await this.formHelper.confirmExitWithUnsavedChanges()) {
                return;
            }
        }
        this.returnToPrevious();
    }

    public returnToPrevious(): void {
        this.navProxy.navigateBackN(2, true);
    }

    private async showSnackbarOnSuccessfulSaved(referenceNumber: string): Promise<void> {

        let message: string = `Additional property values for ${referenceNumber} were saved`;
        const snackbar: HTMLIonToastElement = await this.toastCtrl.create({
            id: this.entityId,
            message: message,
            duration: 3000,
        });
        return await snackbar.present();
    }
}
