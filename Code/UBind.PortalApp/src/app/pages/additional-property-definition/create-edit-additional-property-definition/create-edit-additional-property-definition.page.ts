/* eslint-disable max-classes-per-file */
import {
    Component, Injector, ElementRef, OnInit, OnDestroy,
} from '@angular/core';
import { contentAnimation } from '@assets/animations';
import { scrollbarStyle } from '@assets/scrollbar';
import { LayoutManagerService } from '@app/services/layout-manager.service';
import {
    AbstractControl, AsyncValidatorFn, FormArray, FormBuilder, FormControl,
    FormGroup, ValidationErrors, ValidatorFn, Validators,
} from '@angular/forms';
import { AdditionalPropertyDefinitionTypeEnum } from '@app/models/additional-property-definition-types.enum';
import {
    AdditionalPropertyDefinitionResourceModel,
} from '@app/resource-models/additional-property-definition.resource-model';
import { AdditionalPropertyDefinitionApiService } from '@app/services/api/additional-property-definition-api.service';
import { finalize, map, takeUntil } from 'rxjs/operators';
import { ToastController } from '@ionic/angular';
import { AdditionalPropertyDefinition } from '@app/models/additional-property-item-view.model';
import { SharedAlertService } from '@app/services/shared-alert.service';
import { EventService } from '@app/services/event.service';
import { RouteHelper } from '@app/helpers/route.helper';
import { NavProxyService } from '@app/services/nav-proxy.service';
import {
    AdditionalPropertyDefinitionBasePage,
} from '@app/pages/additional-property-definition/additional-property-definition-base.page';
import { EntityEditFieldOption, FieldShowHideRule } from '@app/models/entity-edit-field-option';
import { AdditionalPropertyDefinitionViewModel } from '@app/viewmodels/additional-property-definition.viewmodel';
import { Observable, Subject } from 'rxjs';
import { TenantService } from '@app/services/tenant.service';
import { ProductService } from '@app/services/product.service';
import { OrganisationApiService } from '@app/services/api/organisation-api.service';
import { SharedLoaderService } from '@app/services/shared-loader.service';
import { DetailsListFormItem } from '@app/models/details-list/details-list-form-item';
import { AppConfigService } from '@app/services/app-config.service';
import { ActivatedRoute } from '@angular/router';
import { AdditionalPropertyDefinitionSchemaTypeEnum } from '@app/models/additional-property-schema-type.enum';
import { OtherSetting } from '@app/viewmodels/additional-property-definition-other-setting.viewmodel';
import { JsonValidator } from '@app/helpers/json-validator';
import { AdditionalPropertyValueService } from '@app/services/additional-property-value.service';
import { FormValidatorHelper } from '@app/helpers/form-validator.helper';

/**
 * Additional property definition schema type class
 * It contains the schema type of the additional property definitions
 */
interface AdditionalPropertyDefinitionSchemaType {
    id: string;
    description: string;
}

/**
 * Additional property definition type class
 * It contains the property type of the additional property definitions
 */
interface AdditionalPropertyDefinitionType {
    id: string;
    description: string;
}

/**
 * Class for creating or editing additional properties page.
 * This class contains all the functionalities in creating or editing additional properties.
 */
@Component({
    selector: 'app-create-edit-additional-property-definition',
    templateUrl: './create-edit-additional-property-definition.page.html',
    styleUrls: [
        '../../../../assets/css/scrollbar-form.css',
        '../../../../assets/css/form-toolbar.scss',
        '../../../components/detail-list-item-edit-form/detail-list-item-edit-form.component.scss',
    ],
    animations: [contentAnimation],
    styles: [
        scrollbarStyle,
    ],
})

export class CreateEditAdditionalPropertyDefinitionPage
    extends AdditionalPropertyDefinitionBasePage
    implements OnInit, OnDestroy {
    public fieldOptions: Array<EntityEditFieldOption> = [];
    public fieldShowHideRules: Array<FieldShowHideRule> = [];
    public additionalPropertyForm: FormGroup;
    private additionalPropertyTypeModels: Array<AdditionalPropertyDefinitionType> = [];
    private additionalPropertySchemaTypeModels: Array<AdditionalPropertyDefinitionSchemaType> = [];
    private mode: string;
    private additionalPropertyId: string;
    public additionalProperty: AdditionalPropertyDefinition;
    public detailList: Array<DetailsListFormItem>;
    public title: string;
    private viewModel: AdditionalPropertyDefinitionViewModel;
    private readonly nameKey: string = "name";
    private readonly aliasKey: string = "alias";
    private readonly defaultValuekey: string = "defaultValue";
    private defaultValueToggleLabel: string;

    public constructor(
        public eventService: EventService,
        elementRef: ElementRef,
        injector: Injector,
        public layoutManager: LayoutManagerService,
        private formBuilder: FormBuilder,
        private additionalPropertyDefinitionService: AdditionalPropertyDefinitionApiService,
        private toastCtrl: ToastController,
        private sharedAlertService: SharedAlertService,
        public routeHelper: RouteHelper,
        public navProxy: NavProxyService,
        protected tenantService: TenantService,
        protected productService: ProductService,
        protected organisationApiService: OrganisationApiService,
        private sharedLoaderService: SharedLoaderService,
        appConfigService: AppConfigService,
        route: ActivatedRoute,
        protected additionalPropertyValueService: AdditionalPropertyValueService,
    ) {
        super(
            eventService,
            elementRef,
            injector,
            routeHelper,
            tenantService,
            productService,
            organisationApiService,
            navProxy,
            appConfigService,
            route,
        );
    }

    public get otherSettingsFormControl(): FormArray {
        return this.additionalPropertyForm.get('otherSettings') as FormArray;
    }

    public ngOnInit(): void {
        super.ngOnInit();
        this.destroyed = new Subject<void>();
        this.loadAllBaseParametersBeforeLoadingOtherData(() => {
            this.defaultValueToggleLabel = 'Use a default value if no value has been set';
            this.additionalPropertyId = this.routeHelper.getParam('additionalPropertyId');
            this.buildInputTypes();
            this.setFieldShowHideRules();
            this.mode = !this.additionalPropertyId ? "Create" : "Edit";
            this.title = this.mode + " Property";
            this.isLoading = !this.additionalPropertyId ? false : true;

            if (!this.additionalPropertyId) {
                this.buildFormOnCreate();
            } else {
                this.loadAdditionalPropertyOnEdit();
            }
        });
    }

    public ngOnDestroy(): void {
        if (this.destroyed) {
            this.destroyed.next();
            this.destroyed.complete();
        }
    }

    private setFieldShowHideRules(): void {
        this.fieldShowHideRules.push(
            {
                fieldToHideOrShow: "defaultValue",
                triggerField: this.defaultValueToggleLabel,
                showWhenValueIs: "true",
            },
            {
                fieldToHideOrShow: "defaultValue",
                triggerField: "type",
                showWhenValueIs: "Text",
            },
            {
                fieldToHideOrShow: "schemaType",
                triggerField: "type",
                showWhenValueIs: "StructuredData",
            },
            {
                fieldToHideOrShow: "customSchema",
                triggerField: "schemaType",
                showWhenValueIs: "Custom",
            },
            {
                fieldToHideOrShow: "customSchema",
                triggerField: "type",
                showWhenValueIs: "StructuredData",
            },
            {
                fieldToHideOrShow: "defaultValueTextArea",
                triggerField: this.defaultValueToggleLabel,
                showWhenValueIs: "true",
            },
            {
                fieldToHideOrShow: "defaultValueTextArea",
                triggerField: "type",
                showWhenValueIs: "StructuredData",
            },
        );
    }

    private loadAdditionalPropertyOnEdit(): void {
        if (this.additionalPropertyId) {
            this.additionalPropertyDefinitionService
                .getAdditionalPropertyDefinitionById(this.parentContextId, this.additionalPropertyId)
                .pipe(
                    finalize(() => this.isLoading = false),
                    takeUntil(this.destroyed),
                )
                .subscribe(
                    (additionalPropertyResult: AdditionalPropertyDefinition) => {
                        this.additionalProperty = additionalPropertyResult;
                        this.viewModel = new AdditionalPropertyDefinitionViewModel(this.additionalProperty);
                        this.loadAdditionalPropertyIfExisting();
                    },
                    (err: any) => {
                        this.errorMessage = 'Error in loading details for additional property definition';
                        throw err;
                    },
                );
        }
    }

    private loadAdditionalPropertyIfExisting(): void {
        this.additionalPropertyForm = this.formBuilder.group({
            name: [this.viewModel.name,
                {
                    validators: FormValidatorHelper.nameValidator(true),
                    asyncValidators: [this.uniqueNameValidator()], updateOn: 'blur',
                }],
            alias: [this.viewModel.alias,
                {
                    validators: FormValidatorHelper.aliasValidator(true),
                    asyncValidators: [this.uniqueAliasValidator()], updateOn: 'blur',
                }],
            otherSettings: this.formBuilder.array([], { validators: [this.otherSettingsValidator()] }),
            defaultValue: [this.viewModel.defaultValue],
            defaultValueTextArea: [this.viewModel.defaultValue],
            disabled: false,
            type: [this.viewModel.type],
            schemaType: [this.viewModel.schemaType],
            customSchema: [this.viewModel.customSchema],
        });
        this.detailList = AdditionalPropertyDefinitionViewModel.createDetailListForEdit(this.additionalProperty);
        let typeItems: Array<any> = [];
        this.additionalPropertyTypeModels
            .forEach((model: AdditionalPropertyDefinitionType) => {
                typeItems.push({ value: model.id, label: model.description });
            });
        this.fieldOptions.push({ name: "type", options: typeItems, type: "option" });
        let schemaTypeItems: Array<any> = [];
        this.additionalPropertySchemaTypeModels
            .forEach((model: AdditionalPropertyDefinitionSchemaType) => {
                schemaTypeItems.push({ value: model.id, label: model.description });
            });
        this.fieldOptions.push({ name: "schemaType", options: schemaTypeItems, type: "option" });
        let otherOptions: Array<OtherSetting> =
            this.viewModel.generateOtherSettingOptionsForEdit(this.defaultValueToggleLabel);
        this.populateCheckboxOptionInFieldOptions(otherOptions);
        this.addDefaultValueValidation();
        this.addCustomSchemaValidation();
        this.watchForOtherSettingAndUpdateDefaultValueValidation();
        this.watchForSchemaTypeAndUpdateCustomSchemaValidation();
    }

    protected buildFormOnCreate(): void {
        let defaultType: string = this.additionalPropertyTypeModels[0].id;
        let defaultSchemaType: string = this.additionalPropertySchemaTypeModels[0].id;
        this.additionalPropertyForm = this.formBuilder.group({
            name: ['',
                {
                    validators: FormValidatorHelper.nameValidator(true),
                    asyncValidators: [this.uniqueNameValidator()], updateOn: 'blur',
                }],
            alias: ['',
                {
                    validators: FormValidatorHelper.aliasValidator(true),
                    asyncValidators: [this.uniqueAliasValidator()], updateOn: 'blur',
                }],
            type: [defaultType],
            otherSettings: this.formBuilder.array([], { validators: [this.otherSettingsValidator()] }),
            defaultValue: [''],
            defaultValueTextArea: [''],
            disabled: false,
            schemaType: [defaultSchemaType],
            customSchema: [''],
        });

        this.detailList = AdditionalPropertyDefinitionViewModel.createDetailListForCreate();
        let typeItems: Array<any> = [];
        this.additionalPropertyTypeModels
            .forEach((model: AdditionalPropertyDefinitionType) => {
                typeItems.push({ value: model.id, label: model.description });
            });
        this.fieldOptions.push({ name: "type", options: typeItems, type: "option" });
        let schemaTypeItems: Array<any> = [];
        this.additionalPropertySchemaTypeModels
            .forEach((model: AdditionalPropertyDefinitionSchemaType) => {
                schemaTypeItems.push({ value: model.id, label: model.description });
            });
        this.fieldOptions.push({ name: "schemaType", options: schemaTypeItems, type: "option" });
        let optionSettings: Array<OtherSetting> =
            AdditionalPropertyDefinitionViewModel.generateOtherSettingOptions(this.defaultValueToggleLabel);
        this.populateCheckboxOptionInFieldOptions(optionSettings);
        this.watchForOtherSettingAndUpdateDefaultValueValidation();
        this.watchForSchemaTypeAndUpdateCustomSchemaValidation();
    }

    private populateCheckboxOptionInFieldOptions(optionSettings: Array<OtherSetting>): void {
        const checkboxOptions: Array<any> = [];
        optionSettings.forEach((os: OtherSetting) => {
            checkboxOptions.push({ value: os.defaultValue.toString(), label: os.description });
            this.otherSettingsFormControl.push(new FormControl(os.defaultValue));
        });

        this.fieldOptions.push({ name: "otherSettings", options: checkboxOptions, type: "checkbox" });
    }

    protected buildInputTypes(): void {
        this.additionalPropertyTypeModels = Object.keys(AdditionalPropertyDefinitionTypeEnum)
            .map((key: string) => {
                let model: AdditionalPropertyDefinitionType = {
                    id: key,
                    description: this.formatEnumValue(AdditionalPropertyDefinitionTypeEnum[key]),
                };
                return model;
            });
        this.additionalPropertySchemaTypeModels = Object.keys(AdditionalPropertyDefinitionSchemaTypeEnum)
            .map((key: string) => {
                let model: AdditionalPropertyDefinitionSchemaType = {
                    id: key,
                    description: this.formatEnumValue(AdditionalPropertyDefinitionSchemaTypeEnum[key]),
                };
                return model;
            });
    }

    private formatEnumValue(inputString: string): string {
        return inputString.replace(/([a-z])([A-Z])/g, '$1 $2');
    }

    private navigateEitherFromCreateOrEdit(mode: string): void {
        let pathSegments: Array<string> = this.routeHelper.getPathSegments();
        pathSegments = pathSegments.filter((segment: string) => segment !== mode);
        this.navProxy.navigate(pathSegments);
    }

    private watchForOtherSettingAndUpdateDefaultValueValidation(): void {
        this.additionalPropertyForm.get('otherSettings').valueChanges
            .subscribe(async (value: any) => {
                const typeControl: AbstractControl = this.additionalPropertyForm.get('type');
                const defaultValueControl: AbstractControl = this.additionalPropertyForm.get('defaultValue');
                const defaultValueTextAreaControl: AbstractControl
                    = this.additionalPropertyForm.get('defaultValueTextArea');
                if (value[2] === true && typeControl.value == 'Text') {
                    defaultValueTextAreaControl.clearValidators();
                    defaultValueTextAreaControl.clearAsyncValidators();
                    defaultValueControl.setValidators([Validators.required]);
                } else if (value[2] === true && typeControl.value == 'StructuredData') {
                    defaultValueControl.clearValidators();
                    const schemaType: AbstractControl = this.additionalPropertyForm.get('schemaType');
                    if (schemaType.value != AdditionalPropertyDefinitionSchemaTypeEnum.None) {
                        defaultValueTextAreaControl.setValidators([
                            Validators.required,
                            this.jsonValidator(),
                            await this.assertSchemaValidator()]);
                    } else {
                        defaultValueTextAreaControl.setValidators([Validators.required, this.jsonValidator()]);
                    }
                } else {
                    defaultValueControl.clearValidators();
                    defaultValueTextAreaControl.clearValidators();
                    defaultValueTextAreaControl.clearAsyncValidators();
                }
                defaultValueControl.updateValueAndValidity();
                defaultValueTextAreaControl.updateValueAndValidity();
            });
    }

    private async addDefaultValueValidation(): Promise<void> {
        const defaultValueControl: AbstractControl = this.additionalPropertyForm.get('defaultValue');
        if (defaultValueControl.enabled) {
            defaultValueControl.setValidators([Validators.required]);
        } else {
            defaultValueControl.clearValidators();
        }
        defaultValueControl.updateValueAndValidity();

        const defaultValueTextAreaControl: AbstractControl
            = this.additionalPropertyForm.get('defaultValueTextArea');
        if (defaultValueTextAreaControl.enabled) {
            const schemaType: AbstractControl = this.additionalPropertyForm.get('schemaType');
            if (schemaType.value != AdditionalPropertyDefinitionSchemaTypeEnum.None) {
                defaultValueTextAreaControl.setValidators([
                    Validators.required,
                    this.jsonValidator(),
                    await this.assertSchemaValidator()]);
            } else {
                defaultValueTextAreaControl.setValidators([
                    Validators.required,
                    this.jsonValidator()]);
            }
        } else {
            defaultValueTextAreaControl.clearValidators();
            defaultValueTextAreaControl.clearAsyncValidators();
            defaultValueTextAreaControl.updateValueAndValidity();
        }
    }

    private watchForSchemaTypeAndUpdateCustomSchemaValidation(): void {
        this.additionalPropertyForm.get('schemaType').valueChanges
            .subscribe((value: any) => {
                const typeControl: AbstractControl = this.additionalPropertyForm.get('type');
                const customSchemaControl: AbstractControl = this.additionalPropertyForm.get('customSchema');
                if (value == 'Custom' && typeControl.value == 'StructuredData') {
                    customSchemaControl.setValidators([Validators.required, this.jsonValidator()]);
                } else {
                    customSchemaControl.setValue('');
                    customSchemaControl.clearValidators();
                    customSchemaControl.clearAsyncValidators();
                }
                customSchemaControl.updateValueAndValidity();
            });
    }

    private addCustomSchemaValidation(): void {
        const customSchemaControl: AbstractControl = this.additionalPropertyForm.get('customSchema');
        if (customSchemaControl.enabled) {
            customSchemaControl.setValidators([Validators.required, this.jsonValidator()]);
            customSchemaControl.updateValueAndValidity();
        } else {
            customSchemaControl.setValue('');
            customSchemaControl.clearValidators();
            customSchemaControl.clearAsyncValidators();
            customSchemaControl.updateValueAndValidity();
        }
    }

    private navigateBackToList(): void {
        this.navigateEitherFromCreateOrEdit("create");
    }

    private navigateBackToDetails(): void {
        this.navigateEitherFromCreateOrEdit("edit");
    }

    public async userDidTapSaveButton(): Promise<void> {
        await this.sharedLoaderService.presentWait();
        let vmModel: AdditionalPropertyDefinitionViewModel = this.additionalPropertyForm.value;
        let model: AdditionalPropertyDefinitionResourceModel
            = AdditionalPropertyDefinitionViewModel.createResourceModel(
                this.parentContextId,
                this.contextType,
                this.entityType,
                this.parentContextId,
                this.contextId,
                vmModel,
            );
        if (!this.additionalPropertyId) {
            this.additionalPropertyDefinitionService.create(model)
                .pipe(
                    finalize(() => this.sharedLoaderService.dismiss()),
                    takeUntil(this.destroyed),
                )
                .subscribe((additionProperty: AdditionalPropertyDefinitionResourceModel) => {
                    this.showSnackbarOnSuccessfulSaved(additionProperty, 'added to').then(() => {
                        this.navigateBackToList();
                    });
                });
        } else {
            model.id = this.additionalPropertyId;
            this.additionalPropertyDefinitionService.update(model)
                .pipe(
                    finalize(() => this.sharedLoaderService.dismiss()),
                    takeUntil(this.destroyed),
                )
                .subscribe((additionProperty: AdditionalPropertyDefinitionResourceModel) => {
                    this.showSnackbarOnSuccessfulSaved(additionProperty, 'updated for')
                        .then(() => {
                            this.navigateBackToDetails();
                        });
                });
        }
    }

    private anyOfTheRequiredFieldsAreInvalid(): boolean {
        return this.additionalPropertyForm.get(this.nameKey).dirty
            || this.additionalPropertyForm.get(this.aliasKey).dirty
            || this.additionalPropertyForm.get(this.defaultValuekey).dirty;
    }

    public async userDidTapCloseButton(): Promise<void> {
        if (this.anyOfTheRequiredFieldsAreInvalid()) {
            await this.sharedAlertService.showWithActionHandler({
                header: 'Unsaved Changes',
                subHeader: 'You have unsaved changes, are you sure you wish to close the current view without saving '
                    + 'them?',
                buttons: [
                    {
                        text: 'No',
                        handler: (): any => {
                            return;
                        },
                    },
                    {
                        text: 'Yes',
                        handler: (): any => {
                            this.navigateBasedOnTheMode();
                        },
                    },
                ],
            });
        } else {
            this.navigateBasedOnTheMode();
        }
    }

    private navigateBasedOnTheMode(): void {
        if (!this.additionalPropertyId) {
            this.navigateBackToList();
        } else {
            this.navigateBackToDetails();
        }
    }

    private async showSnackbarOnSuccessfulSaved(
        additionalProperty: AdditionalPropertyDefinitionResourceModel,
        descriptionOfTransaction: string,
    ): Promise<void> {

        let inPluralForm: string
            = this.additionalPropertyDefinitionService.getEntityDescriptionInPluralForm(this.entityType);
        const snackbar: HTMLIonToastElement = await this.toastCtrl.create({
            id: additionalProperty.id,
            message: `${additionalProperty.name} property ${descriptionOfTransaction} ${inPluralForm} associated with `
                + `${this.contextName}`,
            duration: 3000,
        });
        return await snackbar.present();
    }

    private otherSettingsValidator(): ValidatorFn {
        return (control: AbstractControl): ValidationErrors | null => {
            if (this.additionalPropertyForm) {
                if (this.otherSettingsFormControl &&
                    this.otherSettingsFormControl.controls &&
                    this.otherSettingsFormControl.controls.length == 3) {
                    const isUniqueSelected: boolean = this.otherSettingsFormControl.controls[1].value;
                    const hasDefaultSelected: boolean = this.otherSettingsFormControl.controls[2].value;
                    if (isUniqueSelected && hasDefaultSelected) {
                        return { uniqueFieldNoDefaultValue: true };
                    }
                }
                return null;
            }
        };
    }
    private async getSchema(schemaType: string): Promise<string> {
        if (schemaType == AdditionalPropertyDefinitionSchemaTypeEnum.None) {
            return null;
        }
        return schemaType == AdditionalPropertyDefinitionSchemaTypeEnum.Custom
            ? this.additionalPropertyForm.get('customSchema').value
            : await this.additionalPropertyValueService
                .getDefaultSchema(AdditionalPropertyDefinitionSchemaTypeEnum[schemaType]);
    }

    private jsonValidator(): ValidatorFn {
        return (control: AbstractControl): ValidationErrors | null => {
            if (!JsonValidator.isValidJson(control.value)) {
                return { invalidJson: true };
            }
            control.markAsUntouched();
            return null;
        };
    }

    private validateSchema(
        schema: string,
        control: AbstractControl,
    ): ValidationErrors | null {
        const result: string = JsonValidator.assertSchema(schema, control.value);
        if (result != null) {
            if (result.includes('Invalid JSON object')) {
                return { invalidJson: true };
            } else if (result.includes('JSON object does not pass schema assertion')) {
                return { jsonAssertionFailed: true };
            }
            return { invalidSchema: true };
        }
        return null;
    }

    private async assertSchemaValidator(): Promise<ValidatorFn> {
        const schemaType: string = this.additionalPropertyForm.get('schemaType').value;
        const schema: string = await this.getSchema(schemaType);
        return (control: AbstractControl): ValidationErrors | null => {
            if (schemaType != AdditionalPropertyDefinitionSchemaTypeEnum.None) {
                const validationResult: ValidationErrors = this.validateSchema(schema, control);
                if (validationResult != null) {
                    return validationResult;
                }
            }
            control.markAsUntouched();
            return null;
        };
    }

    private uniqueNameValidator(): AsyncValidatorFn {
        return (control: AbstractControl): Observable<ValidationErrors | null> => {
            return this.additionalPropertyDefinitionService.isNameAvailable(
                this.pathTenantAlias || this.portalTenantAlias,
                this.contextType,
                this.entityType,
                this.contextId,
                this.parentContextId,
                control.value,
                this.additionalPropertyId,
            )
                .pipe(
                    map((res: boolean) => {
                        if (!res) {
                            return { uniqueness: true };
                        }
                        // needs to mark it as untouch so it will not be triggered on Save button.
                        control.markAsUntouched({ onlySelf: true });
                        return null;
                    }),
                );
        };
    }

    private uniqueAliasValidator(): AsyncValidatorFn {
        return (control: AbstractControl): Observable<ValidationErrors | null> => {
            return this.additionalPropertyDefinitionService.isAliasAvailable(
                this.pathTenantAlias || this.portalTenantAlias,
                this.contextType,
                this.entityType,
                this.contextId,
                this.parentContextId,
                control.value,
                this.additionalPropertyId,
            )
                .pipe(
                    map((res: boolean) => {
                        if (!res) {
                            return { uniqueness: true };
                        }
                        // needs to mark it as untouch so it will not be triggered on Save button.
                        control.markAsUntouched({ onlySelf: true });
                        return null;
                    }),
                );
        };
    }
}
