import { FormGroup, FormBuilder, Validators, FormControl, FormArray } from '@angular/forms';
import { UserStatus, PersonCategory, PersonCreateModel } from '@app/models';
import { NavProxyService } from '@app/services/nav-proxy.service';
import { AppConfigService } from '@app/services/app-config.service';
import { AuthenticationService } from '@app/services/authentication.service';
import { InvitationApiService } from '@app/services/api/invitation-api.service';
import { SharedAlertService } from '@app/services/shared-alert.service';
import { finalize, map, takeUntil } from 'rxjs/operators';
import { SharedLoaderService } from '@app/services/shared-loader.service';
import { LayoutManagerService } from '@app/services/layout-manager.service';
import { EntityLoaderSaverService } from '@app/services/entity-loader-saver.service';
import { RouteHelper } from '@app/helpers/route.helper';
import { PersonViewModel } from '@app/viewmodels';
import { EventService } from '@app/services/event.service';
import { PortalResourceModel } from '@app/resource-models/portal.resource-model';
import { FormHelper } from '@app/helpers/form.helper';
import { DetailPage } from '@app/pages/master-detail/detail.page';
import { Directive, ElementRef, Inject, Injector, OnDestroy, OnInit } from '@angular/core';
import { PersonDetailsHelper } from '@app/helpers/person-details.helper';
import { DetailsListFormItem } from '@app/models/details-list/details-list-form-item';
import {
    RepeatingFieldResourceModel, RepeatingAddressFieldResourceModel,
} from '@app/resource-models/repeating-field.resource-model';
import { EntityEditFieldOption, FieldOption } from '@app/models/entity-edit-field-option';
import { Subject, Subscription } from 'rxjs';
import { EmailAddressFieldResourceModel } from '@app/resource-models/person/email-address-field.resource-model';
import { PhoneNumberFieldResourceModel } from '@app/resource-models/person/phone-number-field.resource-model';
import { SocialMediaIdFieldResourceModel } from '@app/resource-models/person/social-field.resource-model';
import { StreetAddressFieldResourceModel } from '@app/resource-models/person/street-address-field.resource-model';
import { WebsiteAddressFieldResourceModel } from '@app/resource-models/person/website-address-field.resource-model';
import { MessengerIdFieldResourceModel } from '@app/resource-models/person/messenger-id-field.resource-model';
import { UserApiService } from '@app/services/api/user-api.service';
import { RoleApiService } from '@app/services/api/role-api.service';
import { RoleResourceModel } from '@app/resource-models/role.resource-model';
import { ActivatedRoute, Params } from '@angular/router';
import { AdditionalPropertyDefinitionApiService } from '@app/services/api/additional-property-definition-api.service';
import { TenantService } from '@app/services/tenant.service';
import { AdditionalPropertyValueService } from '@app/services/additional-property-value.service';
import {
    AdditionalPropertyDefinition, AdditionalPropertyValue,
} from '@app/models/additional-property-item-view.model';
import { AdditionalPropertyDefinitionContextType } from '@app/models/additional-property-context-type.enum';
import { EntityType } from '@app/models/entity-type.enum';
import { AdditionalPropertiesHelper } from '@app/helpers/additional-properties.helper';
import { UserService } from '@app/services/user.service';
import { Permission } from '@app/helpers';
import { AdditionalPropertyValueUpsertResourceModel } from '@app/resource-models/additional-property.resource-model';
import { OrganisationApiService } from '@app/services/api/organisation-api.service';
import { PortalApiService } from '@app/services/api/portal-api.service';
import { PermissionService } from '@app/services/permission.service';
import { FeatureSettingService } from '@app/services/feature-setting.service';
import { HttpErrorResponse } from '@angular/common/http';
import { entityLoaderSaverServiceToken } from '@app/injection-tokens/services-injection-tokens';
import { PortalUserType } from '@app/models/portal-user-type.enum';
import { OrganisationModel } from '@app/models/organisation.model';

/**
 * Interface for the repeating fields arrays.
 * For single and address repeating fields.
 */
interface RepeatingItems {
    single: Array<RepeatingFieldResourceModel>;
    address: Array<RepeatingAddressFieldResourceModel>;
    emailAddresses: Array<EmailAddressFieldResourceModel>;
    phoneNumbers: Array<PhoneNumberFieldResourceModel>;
    streetAddresses: Array<StreetAddressFieldResourceModel>;
    websiteAddresses: Array<WebsiteAddressFieldResourceModel>;
    messengerIds: Array<MessengerIdFieldResourceModel>;
    socialMediaIds: Array<SocialMediaIdFieldResourceModel>;
}

/**
 * Export create/edit person component class
 * This class is for creating and editing of the
 * person's components.
 */
@Directive({ selector: '[appCreateEditPerson]' })
export abstract class CreateEditPersonComponent extends DetailPage implements OnInit, OnDestroy {
    protected newPersonId: string;
    public personForm: FormGroup;
    public person: PersonViewModel;
    public applicableRoles: Array<RoleResourceModel>;
    public errorMessage: string;
    public detailsListFormItems: Array<DetailsListFormItem>;
    public itemGroups: Array<string>;
    public formHasError: boolean = false;
    public itemGroupActionIcons: Array<any>;
    public isFirstItemVisible: boolean;
    public isHasManageUsersPermission: boolean;
    public personFieldsOptions: Array<EntityEditFieldOption> = [];
    protected organisationId: any;
    protected tenantAlias: string;
    public canEditAdditionalPropertyValues: boolean;
    protected performingUserOrganisationId: string;
    private tenantId: string;
    private portals: Array<PortalResourceModel>;
    private displayAdditionalPropertiesToPage: boolean = false;
    private customerAndUserCategories: Array<PersonCategory> = [PersonCategory.User, PersonCategory.Customer];
    private userType: PortalUserType;

    /*
     * list of abstract classes
     */
    public abstract personAdditionalPropertyValueFields: Array<AdditionalPropertyValue>;
    public abstract entityType: EntityType;
    public abstract isEdit: boolean;
    public abstract routeParamName: string;
    public abstract didSelectUpdate(value: any): void;
    public abstract returnToPrevious(message?: string): void;

    /*
     * constructor
     */
    public constructor(
        protected userApiService: UserApiService,
        protected roleApiService: RoleApiService,
        protected authenticationService: AuthenticationService,
        protected permissionService: PermissionService,
        protected sharedAlertService: SharedAlertService,
        protected eventService: EventService,
        protected formBuilder: FormBuilder,
        public navProxy: NavProxyService,
        protected appConfigService: AppConfigService,
        @Inject(entityLoaderSaverServiceToken) protected entityLoaderService: EntityLoaderSaverService<any>,
        public personCategory: PersonCategory,
        protected loader: SharedLoaderService,
        protected invitationService: InvitationApiService,
        public layoutManager: LayoutManagerService,
        protected route: ActivatedRoute,
        public routeHelper: RouteHelper,
        protected formHelper: FormHelper,
        elementRef: ElementRef,
        injector: Injector,
        public userService: UserService,
        protected additionalPropertyDefinitionService: AdditionalPropertyDefinitionApiService,
        protected tenantService: TenantService,
        protected additionalPropertyValueService: AdditionalPropertyValueService,
        protected organisationApiService: OrganisationApiService,
        protected portalApiService: PortalApiService,
        private featureSettingService: FeatureSettingService,
    ) {
        super(eventService, elementRef, injector);
        this.canEditAdditionalPropertyValues
            = this.permissionService.hasPermission(Permission.EditAdditionalPropertyValues);
        this.isHasManageUsersPermission = permissionService.hasManageUserPermission();
        this.isLoading = true;
    }

    public ngOnInit(): void {
        this.destroyed = new Subject<void>();
        this.eventService.performingUserOrganisationSubject$.pipe(takeUntil(this.destroyed))
            .subscribe((organisation: OrganisationModel) => {
                this.performingUserOrganisationId = organisation.id;
            });
        this.route.params.pipe(takeUntil(this.destroyed)).subscribe((params: Params) => {
            this.organisationId = params['organisationId'];
        });
        this.displayAdditionalPropertiesToPage = this.entityType === EntityType.User;
        this.userType = this.routeHelper.pathContainsSegment('customer')
            ? PortalUserType.Customer
            : PortalUserType.Agent;
        this.initialise();
    }

    public ngOnDestroy(): void {
        if (this.destroyed) {
            this.destroyed.next();
            this.destroyed.complete();
        }
    }

    public async initialise(): Promise<void> {
        this.tenantAlias = this.routeHelper.getParam('portalTenantAlias');
        this.organisationId = this.routeHelper.getParam('organisationId');
        if (!this.tenantId) {
            this.tenantId = await this.tenantService.getTenantIdFromAlias(this.tenantAlias);
        }

        if (this.displayAdditionalPropertiesToPage && this.canEditAdditionalPropertyValues) {
            // Initialization of additional property fields must be done before form is generated
            await this.initializeAdditionalPropertyInputFields();
        }

        if (!this.isEdit) {
            // For Edit pages, form must be generated after the person object is loaded
            this.personForm = this.generateFormBuilder();
        }

        await this.load();

        if (!this.isEdit
            && this.canEditAdditionalPropertyValues
            && this.personAdditionalPropertyValueFields?.length > 0) {
            this.additionalPropertyValueService.registerUniqueAdditionalPropertyFieldsOnValueChanges(
                this.personAdditionalPropertyValueFields,
                this.personForm,
                "",
                this.tenantId,
                this.detailsListFormItems,
            );
            this.addApplicableRolesFormControls();
        }

        if (this.canChangePortal()) {
            this.applyOptionsToPortalField();
        }

        if (!this.isEdit && this.personCategory == PersonCategory.User) {
            this.addApplicableRolesFormControls();
        }
    }

    private async load(): Promise<void> {
        this.isLoading = true;
        let promises: Array<Promise<void>> = new Array<Promise<void>>();
        if (this.canChangePortal()) {
            promises.push(this.loadPortals());
        }
        if (!this.isEdit) {
            if (this.personCategory == PersonCategory.User) {
                promises.push(this.loadRoles());
            }
        } else {
            promises.push(this.loadEntity());
        }

        try {
            await Promise.all(promises);
        } catch (error) {
            this.errorMessage = 'There was a problem loading the data';
            throw error;
        } finally {
            this.isLoading = false;
        }
    }

    protected async loadEntity(): Promise<void> {
        return this.entityLoaderService.getById(this.routeHelper.getParam(this.routeParamName))
            .toPromise().then((data: any) => {
                if (this.displayAdditionalPropertiesToPage) {
                    data.additionalPropertyValues
                        = AdditionalPropertiesHelper.generateAdditionalPropertyInputFieldsForEdit(
                            this.personAdditionalPropertyValueFields,
                            data.additionalPropertyValues,
                        );
                }

                this.configure(new PersonViewModel(data));
            });
    }

    protected async initializeAdditionalPropertyInputFields(): Promise<void> {
        const organisationId: string = this.organisationId || this.performingUserOrganisationId;
        return this.additionalPropertyDefinitionService
            .getAdditionalPropertyDefinitionsByContextAndEntityAndParentContextId(
                this.tenantId,
                AdditionalPropertyDefinitionContextType.Organisation,
                organisationId,
                this.entityType,
                this.person ? this.person.entityId : null,
                this.tenantId,
                true,
            )
            .pipe(takeUntil(this.destroyed))
            .toPromise()
            .then(
                (result: Array<AdditionalPropertyDefinition>) => {
                    if (result) {
                        this.personAdditionalPropertyValueFields =
                            AdditionalPropertiesHelper.generateAdditionalPropertyInputFieldsForCreateForm(result);
                    }
                },
                (err: HttpErrorResponse) => {
                    this.errorMessage = 'Error loading additional properties for organisation';
                    throw err;
                },
            );
    }

    /*
     * list of helper methods
    */
    protected configure(person: PersonViewModel): void {
        this.person = person;

        let hasUserAccount: boolean = person.status ? person.status != UserStatus.New : false;
        this.personForm = this.generateFormBuilder(hasUserAccount);
        this.personForm.patchValue({
            preferredName: this.person.preferredName,
            namePrefix: this.person.namePrefix,
            firstName: this.person.firstName,
            middleNames: this.person.middleNames,
            lastName: this.person.lastName,
            nameSuffix: this.person.nameSuffix,
            company: this.person.company,
            title: this.person.title,
            status: this.person.status,
            accountEmail: this.person.email,
        });

        if (this.canEditAdditionalPropertyValues
            && person.additionalPropertyValues?.length > 0) {
            let additionalPropertyValues: any = [];
            person.additionalPropertyValues.forEach((app: AdditionalPropertyValue) => {
                let id: string = AdditionalPropertiesHelper.generateControlId(
                    app.additionalPropertyDefinitionModel.id,
                );
                additionalPropertyValues[id] = app.value;
            });
            this.personForm.patchValue(additionalPropertyValues);

            this.additionalPropertyValueService.registerUniqueAdditionalPropertyFieldsOnValueChanges(
                person.additionalPropertyValues,
                this.personForm,
                this.person.entityId || this.person.id,
                this.tenantId,
                this.detailsListFormItems,
            );
        }
    }

    public async didSelectClose(value: any): Promise<void> {
        if (this.personForm.dirty) {
            if (!await this.formHelper.confirmExitWithUnsavedChanges()) {
                return;
            }
        }
        this.returnToPrevious();
    }

    private getFullName(firstName: string, lastName: string): string {
        return `${firstName} ${lastName ? lastName : ''}`.trim();
    }

    public async didSelectUpdateWithCallback(
        value: any,
        successCallback: (person: any, message?: string) => void,
        failureCallback: (person: any, message?: string) => void,
    ): Promise<void> {
        let fromEntityId: string = this.person.entityId || this.person.id;
        if (this.canEditAdditionalPropertyValues
            && !await this.additionalPropertyValueService.validateAdditionalPropertyValues(
                this.personAdditionalPropertyValueFields,
                fromEntityId,
                this.personForm,
                value,
                this.tenantId,
            )) {
            return;
        }
        this.loader.presentWait(
            this.layoutManager.splitPaneEnabled ? 'detail-loader' : '',
            !this.layoutManager.splitPaneEnabled,
        ).then(() => {
            let repeatingFields: RepeatingItems = this.generateRepeatingFieldsFromObject(value);
            let environment: any = this.appConfigService.getEnvironment();

            let properties: Array<AdditionalPropertyValueUpsertResourceModel> = [];
            this.canEditAdditionalPropertyValues =
                this.permissionService.hasPermission(Permission.EditAdditionalPropertyValues);
            if (this.canEditAdditionalPropertyValues) {
                properties = AdditionalPropertiesHelper.buildProperties(
                    this.personAdditionalPropertyValueFields,
                    value,
                );
            }

            let updateModel: any = {
                id: fromEntityId,
                tenant: this.routeHelper.getContextTenantAlias(),
                fullName: this.getFullName(value.firstName, value.lastName),
                preferredName: value.preferredName,
                namePrefix: value.namePrefix,
                firstName: value.firstName,
                middleNames: value.middleNames,
                lastName: value.lastName,
                nameSuffix: value.nameSuffix,
                company: value.company,
                title: value.title,
                picture: value.picture,
                userType: this.person.userType,
                email: value.accountEmail,
                blocked: value.status === UserStatus.Deactivated || value.status === UserStatus.Disabled,
                organisationId: this.organisationId || this.performingUserOrganisationId,
                ownerId: null,
                pictureId: null,
                userStatus: null,
                default: false,
                emailAddresses: repeatingFields.emailAddresses,
                phoneNumbers: repeatingFields.phoneNumbers,
                websiteAddresses: repeatingFields.websiteAddresses,
                streetAddresses: repeatingFields.streetAddresses,
                messengerIds: repeatingFields.messengerIds,
                socialMediaIds: repeatingFields.socialMediaIds,
                createdDateTime: null,
                lastModifiedDateTime: null,
                environment: environment,
                portalId: value.portal == 'none' ? null : value.portal,
                customerId: this.person.customerId,
            };

            if (properties.length > 0) {
                updateModel = { ...updateModel, properties: properties };
            }

            this.entityLoaderService.update(fromEntityId, updateModel)
                .pipe(finalize((): void => this.loader.dismiss()))
                .subscribe((person: any) => {
                    let toastMessage: string = `${updateModel.fullName} was updated`;
                    if (this.customerAndUserCategories.includes(this.personCategory)) {
                        const category: string
                            = `${this.personCategory[0].toUpperCase()}${this.personCategory.substr(1)}`;
                        toastMessage = `${category} ${toastMessage}`;
                    }

                    this.sharedAlertService.showToast(toastMessage);
                    if (successCallback) {
                        successCallback(person);
                    }
                });
        });
    }

    private extractAccountEmailFromRepeatingFields(repeatingFields: Array<RepeatingFieldResourceModel>): string {
        let emails: Array<RepeatingFieldResourceModel> = repeatingFields
            .filter((f: RepeatingFieldResourceModel) => f.parentFieldName == "email");
        return emails.length == 0 ? "" : emails[0].value;
    }

    public async didSelectCreateWithCallback(value: any, successCallback: (response: any) => void): Promise<void> {
        let repeatingFields: RepeatingItems = this.generateRepeatingFieldsFromObject(value);
        let customerId: string = this.routeHelper.getParam("customerId");
        let environment: any = this.appConfigService.getEnvironment();
        let properties: Array<AdditionalPropertyValueUpsertResourceModel> = AdditionalPropertiesHelper.buildProperties(
            this.personAdditionalPropertyValueFields,
            value,
        );
        let signupModel: PersonCreateModel = {
            preferredName: value.preferredName,
            fullName: this.getFullName(value.firstName, value.lastName),
            displayName: value.displayName,
            namePrefix: value.namePrefix,
            firstName: value.firstName,
            middleNames: value.middleNames,
            lastName: value.lastName,
            nameSuffix: value.nameSuffix,
            company: value.company,
            title: value.title,
            email: value.accountEmail,
            userType: this.personCategory === PersonCategory.User ? value.userType : '',
            blocked: false,
            tenant: this.routeHelper.getContextTenantAlias(),
            tenantId: null,
            organisationId: this.organisationId || this.performingUserOrganisationId,
            emailAddresses: repeatingFields.emailAddresses,
            phoneNumbers: repeatingFields.phoneNumbers,
            websiteAddresses: repeatingFields.websiteAddresses,
            streetAddresses: repeatingFields.streetAddresses,
            messengerIds: repeatingFields.messengerIds,
            socialMediaIds: repeatingFields.socialMediaIds,
            environment: environment,
            portalId: value.portal == 'none' ? null : value.portal,
            customerId: customerId,
            hasActivePolicies: false,
            initialRoles: this.personCategory === PersonCategory.User ?
                value.applicableRoles.map((v: any, i: any) => v ?
                    this.applicableRoles[i].id : null)
                    .filter((v: any) => v !== null)
                : null,
        };
        if (properties.length > 0) {
            signupModel = { ...signupModel, properties: properties };
        }

        if (this.canEditAdditionalPropertyValues
            && !await this.additionalPropertyValueService.validateAdditionalPropertyValues(
                this.personAdditionalPropertyValueFields,
                "",
                this.personForm,
                value,
                this.tenantId,
            )) {
            return;
        }

        await this.loader.presentWithDelay();
        const subscription: Subscription = this.entityLoaderService.create(signupModel)
            .pipe(finalize(() => {
                this.loader.dismiss();
                subscription.unsubscribe();
            }))
            .subscribe(
                (res: any) => {
                    this.newPersonId = (<any>res).id;
                    if (successCallback) {
                        successCallback(res);
                    }

                    let toastMessage: string = `${signupModel.fullName} was created`;
                    if (this.customerAndUserCategories.includes(this.personCategory)) {
                        const category: string
                            = `${this.personCategory[0].toUpperCase()}${this.personCategory.substr(1)}`;
                        let userToastMessage: string = '';
                        if (this.personCategory == PersonCategory.User) {
                            userToastMessage = `. An email has been sent to their email address `
                                + `with a link to activate their account`;
                        }

                        toastMessage = `${category} ${toastMessage}${userToastMessage}`;
                    }
                    this.sharedAlertService.showToast(toastMessage);
                },
            );
    }

    public async checkAndHandleValueForPersonEmailChange(formEmail: string): Promise<void> {
        return new Promise<void>((resolve: () => void): void => {
            if (formEmail == null) {
                return;
            }

            if (this.person.email !== formEmail) {
                switch (this.person.status) {
                    case UserStatus.Invited: {
                        this.sharedAlertService.showWithActionHandler({
                            header: 'Resend activation email',
                            subHeader: `You have changed the email address associated with a user
	                            who did not yet activate their account. ` +
                                `Would you like to resend the account activation invitation email ` +
                                `to the new email address?`,
                            buttons: [{
                                text: 'Cancel',
                                handler: (): any => {
                                    resolve();
                                },
                            },
                            {
                                text: 'Resend',
                                handler: async (): Promise<any> => {
                                    await this.resendActivation(
                                        this.person.fullName, this.person.id, this.person.tenantId);
                                    resolve();
                                },
                            }],
                        });
                        break;
                    }
                    case UserStatus.Active: {
                        this.sharedAlertService.showWithActionHandler({
                            header: 'Reminder',
                            subHeader: `You have changed the email address associated with an activated user account. `
                                + `Please note that from now on the user will need to use the new email address to `
                                + `sign in.`,
                            buttons: [{
                                text: 'OK',
                                handler: (): any => {
                                    resolve();
                                },
                            }],
                        });
                        break;
                    }
                    case UserStatus.Disabled:
                    case UserStatus.Deactivated: {
                        this.sharedAlertService.showWithActionHandler({
                            header: 'Reminder',
                            subHeader: `You have changed the email address associated with a disabled user account. ` +
                                `Please note that if the account is enabled in the future, ` +
                                `the user will need to use the new email address to sign in.`,
                            buttons: [{
                                text: 'OK',
                                handler: (): any => {
                                    resolve();
                                },
                            }],
                        });
                        break;
                    }
                    default: {
                        resolve();
                        break;
                    }
                }
            } else {
                resolve();
            }
        });
    }

    protected generateFormBuilder(hasUserAccount: boolean = true): FormGroup {
        this.detailsListFormItems = PersonDetailsHelper
            .createPersonDetailsListForEdit(
                this.isEdit,
                this.personCategory,
                this.personAdditionalPropertyValueFields,
                this.canEditAdditionalPropertyValues,
                hasUserAccount,
            );
        let controls: any = [];
        this.detailsListFormItems.forEach((item: DetailsListFormItem) => {
            if (!item.IsRepeating) {
                controls[item.Alias] = item.FormControl;
            }
        });
        let defaultAdditionalPropertyValues: any = [];
        if ((this.personAdditionalPropertyValueFields?.length > 0)
            && this.canEditAdditionalPropertyValues) {
            this.personAdditionalPropertyValueFields.forEach((item: AdditionalPropertyValue) => {
                let id: string = AdditionalPropertiesHelper.generateControlId(
                    item.additionalPropertyDefinitionModel.id,
                );
                let formItem: DetailsListFormItem = this.detailsListFormItems.find(
                    (dlfi: DetailsListFormItem) => dlfi.Alias === id);
                if (formItem) {
                    controls[id] = formItem.FormControl;
                    defaultAdditionalPropertyValues[id] = item.value;
                }
            });
        }
        const form: FormGroup = this.formBuilder.group(controls);

        if (this.isEdit) {
            form.addControl('status', new FormControl('', Validators.required));
        }

        if (defaultAdditionalPropertyValues) {
            form.patchValue(defaultAdditionalPropertyValues);
        }

        this.generateFormSelection();
        return form;
    }

    private generateRepeatingFieldsFromObject(fields: any): RepeatingItems {
        let repeatingfields: Array<RepeatingFieldResourceModel> = [];
        let repeatingAddressfields: Array<RepeatingAddressFieldResourceModel> = [];
        let emails: Array<EmailAddressFieldResourceModel> = [];
        let phones: Array<PhoneNumberFieldResourceModel> = [];
        let addresses: Array<StreetAddressFieldResourceModel> = [];
        let websites: Array<WebsiteAddressFieldResourceModel> = [];
        let messengers: Array<MessengerIdFieldResourceModel> = [];
        let socials: Array<SocialMediaIdFieldResourceModel> = [];
        let defaultGuid: string = "00000000-0000-0000-0000-000000000000";

        let repeatingFieldNames: Array<DetailsListFormItem> =
            this.detailsListFormItems.filter((f: DetailsListFormItem) =>
                f.IsRepeating);
        repeatingFieldNames.forEach((item: DetailsListFormItem) => {
            let indexes: Array<number> = [];
            Object.keys(fields)
                .filter((key: string) => key.indexOf(item.Alias) > -1)
                .reduce((obj: any, key: string) => {
                    indexes.push(parseInt(key.replace(/\D/g, ""), 10));
                    return obj;
                }, {});

            let lastIndex: number = indexes.sort()[indexes.length - 1];
            for (let i: number = 0; i < lastIndex + 1; i++) {
                let value: any = fields[item.Alias + i];
                let label: string = fields[item.Alias + "_label" + i];
                let customLabel: string = fields[item.Alias + "_customLabel" + i];
                let isDefault: boolean = fields[item.Alias + "_default" + i] ?
                    fields[item.Alias + "_default" + i] : false;
                if (value) {
                    if (item.Alias == "address") {
                        if (value['address']) {
                            addresses.push(
                                {
                                    address: value['address'],
                                    suburb: value['suburb'],
                                    state: value['state'],
                                    postcode: value['postcode'],
                                    default: isDefault,
                                    label: label,
                                    customLabel: customLabel,
                                    sequenceNo: i,
                                    fieldId: defaultGuid,
                                },
                            );
                        }
                    } else {
                        if (value) {
                            repeatingfields.push(
                                {
                                    parentFieldName: item.Alias,
                                    name: item.Alias + i,
                                    sequenceNo: i,
                                    label: label,
                                    customLabel: customLabel,
                                    referenceId: "",
                                    value: value,
                                    default: false,
                                });
                            if (item.Alias == "email") {
                                emails.push(
                                    {
                                        emailAddress: value,
                                        label: label,
                                        customLabel: customLabel,
                                        sequenceNo: i,
                                        default: isDefault,
                                        fieldId: defaultGuid,
                                    },
                                );
                            }

                            if (item.Alias == "phone") {
                                phones.push(
                                    {
                                        phoneNumber: value,
                                        label: label,
                                        customLabel: customLabel,
                                        sequenceNo: i,
                                        default: isDefault,
                                        fieldId: defaultGuid,
                                    },
                                );
                            }

                            if (item.Alias == "website") {
                                websites.push(
                                    {
                                        websiteAddress: value,
                                        label: label,
                                        customLabel: customLabel,
                                        sequenceNo: i,
                                        default: isDefault,
                                        fieldId: defaultGuid,
                                    },
                                );
                            }

                            if (item.Alias == "messenger") {
                                messengers.push(
                                    {
                                        messengerId: value,
                                        label: label,
                                        customLabel: customLabel,
                                        sequenceNo: i,
                                        default: isDefault,
                                        fieldId: defaultGuid,
                                    },
                                );
                            }

                            if (item.Alias == "social") {
                                socials.push(
                                    {
                                        socialMediaId: value,
                                        label: label,
                                        customLabel: customLabel,
                                        sequenceNo: i,
                                        default: isDefault,
                                        fieldId: defaultGuid,
                                    },
                                );
                            }

                            if (item.Alias == "address") {
                                addresses.push(
                                    {
                                        address: value['address'],
                                        suburb: value['suburb'],
                                        state: value['state'],
                                        postcode: value['postcode'],
                                        label: label,
                                        customLabel: customLabel,
                                        sequenceNo: i,
                                        default: isDefault,
                                        fieldId: defaultGuid,
                                    },
                                );
                            }
                        }
                    }
                }
            }
        });
        return {
            single: repeatingfields,
            address: repeatingAddressfields,
            emailAddresses: emails,
            phoneNumbers: phones,
            streetAddresses: addresses,
            messengerIds: messengers,
            websiteAddresses: websites,
            socialMediaIds: socials,
        };
    }

    protected generateFormSelection(): void {
        const phoneLabelOptions: Array<FieldOption> = [
            { label: 'Mobile', value: 'mobile' },
            { label: 'Home', value: 'home' },
            { label: 'Work', value: 'work' },
            { label: 'Other', value: 'other' },
        ];

        const emailLabelOptions: Array<FieldOption> = [
            { label: 'Work', value: 'work' },
            { label: 'Personal', value: 'personal' },
            { label: 'Other', value: 'other' },
        ];

        const websiteLabelOptions: Array<FieldOption> = [
            { label: 'Personal', value: 'personal' },
            { label: 'Business', value: 'business' },
            { label: 'Other', value: 'other' },
        ];

        const messengerLabelOptions: Array<FieldOption> = [
            { label: 'Skype', value: 'skype' },
            { label: 'WhatsApp', value: 'whatsapp' },
            { label: 'Other', value: 'other' },
        ];

        const socialLabelOptions: Array<FieldOption> = [
            { label: 'LinkedIn', value: 'linkedin' },
            { label: 'Twitter', value: 'twitter' },
            { label: 'Facebook', value: 'facebook' },
            { label: 'Instagram', value: 'instagram' },
            { label: 'YouTube', value: 'youtube' },
            { label: 'Other', value: 'other' },
        ];

        const addressLabelOptions: Array<FieldOption> = [
            { label: 'Home', value: 'home' },
            { label: 'Work', value: 'work' },
            { label: 'Postal', value: 'postal' },
            { label: 'Other', value: 'other' },
        ];

        this.personFieldsOptions.push({ name: "phone", options: phoneLabelOptions, type: null });
        this.personFieldsOptions.push({ name: "email", options: emailLabelOptions, type: null });
        this.personFieldsOptions.push({ name: "website", options: websiteLabelOptions, type: null });
        this.personFieldsOptions.push({ name: "messenger", options: messengerLabelOptions, type: null });
        this.personFieldsOptions.push({ name: "social", options: socialLabelOptions, type: null });
        this.personFieldsOptions.push({ name: "address", options: addressLabelOptions, type: null });
    }

    private resendActivation(fullName: string, personId: string, tenant: string): void {
        const cssClass: string = this.layoutManager.splitPaneEnabled ? 'detail-loader' : '';
        const showBackdrop: boolean = !this.layoutManager.splitPaneEnabled;
        this.loader.presentWait(cssClass, showBackdrop).then(() => {
            this.invitationService.sendActivationForPerson(personId, tenant)
                .pipe(finalize(() => this.loader.dismiss()))
                .subscribe(() => {
                    this.sharedAlertService.showToast(
                        `An updated email has been sent to ${fullName} with a link to activate their user account`);
                });
        });
    }

    private addApplicableRolesFormControls(): void {
        if ((this.personCategory !== PersonCategory.User) || (!this.isHasManageUsersPermission) ||
            (this.isEdit)) {
            return;
        }
        let roleOptionsList: Array<any> = Array<any>();
        let isChecked: boolean = false;
        let controlName: string = "applicableRoles";
        this.personForm.addControl(controlName, this.formBuilder.array([]));
        if (this.applicableRoles) {
            this.applicableRoles.map((p: RoleResourceModel, i: number) => {
                (this.personForm.get(controlName) as FormArray)
                    .push(new FormControl(isChecked));
                roleOptionsList.push({ label: p.name, value: isChecked.toString() });
            });
        }
        this.personFieldsOptions.push({ name: "applicableRoles", options: roleOptionsList, type: "checkbox" });
    }

    private async loadRoles(): Promise<void> {
        let params: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
        params.set('assignable', 'true');
        params.set('tenant', this.routeHelper.getContextTenantAlias());
        const organisationId: string = this.organisationId || this.performingUserOrganisationId;
        if (organisationId) {
            params.set('organisation', organisationId);
        }
        return this.roleApiService.getList(params)
            .pipe(
                finalize(() => this.isLoading = false),
                map((roles: Array<RoleResourceModel>) => {
                    this.applicableRoles = roles.filter((role: RoleResourceModel) =>
                        role.type.toLocaleLowerCase() != PersonCategory.Customer.toLocaleLowerCase());

                }),
            ).toPromise();
    }

    private applyOptionsToPortalField(): void {
        let items: Array<any> = [];
        if (this.portals) {
            this.portals.forEach((portal: PortalResourceModel) => {
                items.push({
                    label: portal.name,
                    value: portal.id,
                });
            });
        }

        items.push({
            label: 'None',
            value: null,
        });

        this.personFieldsOptions.push({ name: "portal", options: items, type: "option" });
    }

    private async loadPortals(): Promise<void> {
        return this.portalApiService.getActivePortals(
            this.routeHelper.getContextTenantAlias(),
            this.getContextOrganisationid(),
            this.userType)
            .pipe(takeUntil(this.destroyed))
            .toPromise().then((portals: Array<PortalResourceModel>) => {
                this.portals = portals;
            });
    }

    private getContextOrganisationid(): string {
        return this.organisationId || this.performingUserOrganisationId;
    }

    private canChangePortal(): boolean {
        const contextTenantAlias: string = this.routeHelper.getContextTenantAlias();
        return this.personCategory == PersonCategory.User
            && !this.authenticationService.isCustomer()
            && this.featureSettingService.hasPortalManagementFeature()
            && contextTenantAlias != 'ubind' && contextTenantAlias != 'master';
    }
}
