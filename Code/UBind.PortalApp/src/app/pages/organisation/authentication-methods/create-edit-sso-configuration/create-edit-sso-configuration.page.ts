import { Component, ElementRef, Injector, OnDestroy, OnInit } from "@angular/core";
import { FormBuilder, FormGroup } from "@angular/forms";
import { FormValidatorHelper } from "@app/helpers/form-validator.helper";
import { RouteHelper } from "@app/helpers/route.helper";
import { AuthenticationMethodType } from "@app/models/authentication-method-type.enum";
import { DetailsListFormCheckboxItem } from "@app/models/details-list/details-list-form-checkbox-item";
import { DetailsListFormItem } from "@app/models/details-list/details-list-form-item";
import { DetailsListFormSelectItem } from "@app/models/details-list/details-list-form-select-item";
import { DetailsListFormTextAreaItem } from "@app/models/details-list/details-list-form-text-area-item";
import { DetailsListFormTextItem } from "@app/models/details-list/details-list-form-text-item";
import { DetailsListItemCard } from "@app/models/details-list/details-list-item-card";
import { EntityEditFieldOption } from "@app/models/entity-edit-field-option";
import { IconLibrary } from "@app/models/icon-library.enum";
import { DetailPage } from "@app/pages/master-detail/detail.page";
import { AuthenticationMethodApiService } from "@app/services/api/authentication-method-api.service";
import { EventService } from "@app/services/event.service";
import { NavProxyService } from "@app/services/nav-proxy.service";
import { SharedAlertService } from "@app/services/shared-alert.service";
import { SharedLoaderService } from "@app/services/shared-loader.service";
import { Subject } from "rxjs";
import { finalize, takeUntil } from "rxjs/operators";
import * as _ from 'lodash';
import {
    AuthenticationMethodResourceModel,
    SamlAuthenticationMethodUpsertModel,
} from "@app/resource-models/authentication-method.resource-model";
import { DetailsListFormItemGroup } from "@app/models/details-list/details-list-form-item-group";
import { RoleApiService } from "@app/services/api/role-api.service";
import { RoleResourceModel } from "@app/resource-models/role.resource-model";
import { DetailsListFormContentItem } from "@app/models/details-list/details-list-form-content-item";
import { DetailsListFormItemRepeating } from "@app/models/details-list/details-list-form-item-repeating";

/**
 * Page for creating/editing SSO configurations.
 */
@Component({
    selector: 'app-create-edit-sso-configuration-page',
    templateUrl: './create-edit-sso-configuration.page.html',
    styleUrls: [
        '../../../../../assets/css/scrollbar-form.css',
        '../../../../../assets/css/form-toolbar.scss',
    ],
})
export class CreateEditSsoConfigurationPage extends DetailPage implements OnInit, OnDestroy {

    public detailsList: Array<DetailsListFormItem>;
    public samlFields: Array<DetailsListFormItem>;
    public signInFields: Array<DetailsListFormItem>;
    public fieldOptions: Array<EntityEditFieldOption> = [];
    public form: FormGroup;
    public title: string = 'SSO Configuration';
    public authenticationMethodId: string;
    private model: AuthenticationMethodResourceModel;
    private organisationId: string;
    public isEdit: boolean;
    private roles: Array<RoleResourceModel>;

    public constructor(
        private routeHelper: RouteHelper,
        private authenticationMethodApiService: AuthenticationMethodApiService,
        private formBuilder: FormBuilder,
        private navProxy: NavProxyService,
        private sharedLoaderService: SharedLoaderService,
        protected eventService: EventService,
        private sharedAlert: SharedAlertService,
        elementRef: ElementRef,
        public injector: Injector,
        private roleApiService: RoleApiService,
    ) {
        super(eventService, elementRef, injector);
    }

    public ngOnInit(): void {
        this.destroyed = new Subject<void>();
        this.organisationId = this.routeHelper.getParam('organisationId');
        this.authenticationMethodId = this.routeHelper.getParam('authenticationMethodId');
        this.isEdit = this.authenticationMethodId != null;
        this.loadRoles().then(() => {
            this.prepareForm();
            this.buildForm();
            if (this.isEdit) {
                this.load();
            } else {
                this.isLoading = false;
            }
        });
    }

    public ngOnDestroy(): void {
        if (this.destroyed) {
            this.destroyed.next();
            this.destroyed.complete();
        }
    }

    private load(): void {
        this.isLoading = true;
        this.authenticationMethodApiService.getAuthenticationMethod(
            this.authenticationMethodId, this.routeHelper.getContextTenantAlias())
            .pipe(
                takeUntil(this.destroyed),
                finalize(() => this.isLoading = false),
            )
            .toPromise()
            .then((model: AuthenticationMethodResourceModel) => {
                this.model = model;
                let formModel: any = _.clone(this.model);
                formModel.standardNameIdFormat = '';
                delete formModel.roleMapJson;
                delete formModel.id;
                delete formModel.organisationId;
                delete formModel.tenantId;
                delete formModel.disabled;
                delete formModel.createdDateTime;
                delete formModel.lastModifiedDateTime;
                formModel.roleMap = this.getRoleMapEntries(formModel.roleMap);

                // make sure there are enough role map form controls
                let roleMapItemArray: DetailsListFormItemRepeating
                    = <DetailsListFormItemRepeating> this.detailsList
                        .find((item: DetailsListFormItemRepeating) => item.Alias == 'roleMap');
                roleMapItemArray.setNumberOfInstances(Math.max(formModel.roleMap.length, 1));

                this.form.setValue(formModel);
            });
    }

    private getRoleMapEntries(
        roleMapObject: {[key: string]: string},
    ): Array<{idpRoleName: string; ubindRoleId: string}> {
        if (roleMapObject == null || Object.keys(roleMapObject).length == 0) {
            return [{ idpRoleName: '', ubindRoleId: '' }];
        }
        const roleMapEntries: Array<{idpRoleName: string; ubindRoleId: string}>
            = Object.entries(roleMapObject).map(([idpRoleName, ubindRoleId]: [string, string]) => {
                return { idpRoleName, ubindRoleId };
            });
        return roleMapEntries;
    }

    private async loadRoles(): Promise<void> {
        this.isLoading = true;
        let params: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
        params.set('tenant', this.routeHelper.getContextTenantAlias());
        params.set('assignable', 'true');
        if (this.organisationId) {
            params.set('organisationId', this.organisationId);
        }
        return this.roleApiService.getList(params)
            .pipe(
                takeUntil(this.destroyed),
                finalize(() => this.isLoading = false),
            ).toPromise().then((roles: Array<RoleResourceModel>) => {
                this.roles = roles;
            });
    }

    protected prepareForm(): void {
        this.detailsList = new Array<DetailsListFormItem>();
        this.samlFields = new Array<DetailsListFormItem>();
        this.signInFields = new Array<DetailsListFormItem>();

        const typeCard: DetailsListItemCard = new DetailsListItemCard(
            'Details',
            'Initial details to set the type of the authentication method.');
        this.detailsList.push(DetailsListFormTextItem.create(
            typeCard,
            'name',
            'Name')
            .withGroupName('initial')
            .withIcon('card-account-details', IconLibrary.AngularMaterial)
            .withValidator(FormValidatorHelper.entityNameValidator()));
        this.detailsList.push(DetailsListFormSelectItem.create(
            typeCard,
            'typeName',
            'Type')
            .withGroupName<DetailsListFormSelectItem>('initial')
            .withOption({ label: AuthenticationMethodType.Saml, value: AuthenticationMethodType.Saml })
            .withValidator(FormValidatorHelper.required()));

        const signInOptionsCard: DetailsListItemCard = new DetailsListItemCard(
            'Sign-in Options',
            'Options for who can sign in and what happens when they do.');
        this.signInFields.push(DetailsListFormCheckboxItem.create(
            signInOptionsCard,
            "includeSignInButtonOnPortalLoginPage",
            "Include sign-in button on portal login page",
            true)
            .withGroupName('signInOptions')
            .withParagraph('Options for who can sign in and what happens when they do.')
            .withHeader('Sign-in Options')
            .withIcon('form-textbox-password', IconLibrary.AngularMaterial));
        this.signInFields.push(DetailsListFormTextItem.create(
            signInOptionsCard,
            "signInButtonLabel",
            "Sign-in Button Label")
            .withGroupName('signInOptions')
            .withHint("e.g. Sign in with XXXX"));
        this.signInFields.push(DetailsListFormTextItem.create(
            signInOptionsCard,
            "signInButtonBackgroundColor",
            "Sign-in Button Background Color")
            .withGroupName('signInOptions')
            .withHint("A valid CSS color value, e.g. #ff0000"));
        this.signInFields.push(DetailsListFormTextItem.create(
            signInOptionsCard,
            "signInButtonIconUrl",
            "Sign-in Button Icon URL")
            .withGroupName('signInOptions')
            .withHint("The URL of an icon to display on the sign-in button. SVG format is recommended.")
            .withValidator(FormValidatorHelper.webUrl(false)));
        this.signInFields.push(DetailsListFormCheckboxItem.create(
            signInOptionsCard,
            "canCustomersSignIn",
            "Allow Customers to sign-in")
            .withGroupName('signInOptions'));
        this.signInFields.push(DetailsListFormCheckboxItem.create(
            signInOptionsCard,
            "shouldLinkExistingCustomerWithSameEmailAddress",
            "Link existing customer with the same email address")
            .withGroupName('signInOptions')
            .withHint("If unchecked and an existing user account exists with the same email address, an error will be "
                + "generated and the user won't be able to log in. If checked, during first time login, the user will "
                + "be linked to the existing user account with the same email address."));
        this.signInFields.push(DetailsListFormCheckboxItem.create(
            signInOptionsCard,
            "canAgentsSignIn",
            "Allow Agents to sign-in")
            .withGroupName('signInOptions'));
        this.signInFields.push(DetailsListFormCheckboxItem.create(
            signInOptionsCard,
            "shouldLinkExistingAgentWithSameEmailAddress",
            "Link existing agent with the same email address")
            .withGroupName('signInOptions')
            .withHint("If unchecked and an existing user account exists with the same email address, an error will be "
                + "generated and the user won't be able to log in. If checked, during first time login, the user will "
                + "be linked to the existing user account with the same email address."));
        this.signInFields.push(DetailsListFormCheckboxItem.create(
            signInOptionsCard,
            "canUsersOfManagedOrganisationsSignIn",
            "Allow sign-in for users of managed organisations")
            .withGroupName('signInOptions')
            .withHint("Checking this option allows this authentication method to be used by orgnanisations that this "
                + "organisation manages. Combined with auto-provisioning (below), it allows the identity provider to "
                + "have partner organisations automatically created for users when they sign in."));
        this.signInFields.push(DetailsListFormCheckboxItem.create(
            signInOptionsCard,
            "shouldLinkExistingOrganisationWithSameAlias",
            "Link existing organisations with the same alias")
            .withGroupName('signInOptions')
            .withHint("If unchecked and an existing organisation exists with the same alias during sign in, "
                + "and that organisation is not already linked to the Identity Provider organisaiton, then "
                + "an error will be generated, and the user will not be able to log in. "
                + "If checked, during sign-in the organisation will be linked to the Identity Provider organisation."));
        this.samlFields.push(...this.signInFields);

        const identityProviderCard: DetailsListItemCard = new DetailsListItemCard(
            'Identity Provider',
            'Details about the identity provider including URLs and certificates.');
        this.samlFields.push(DetailsListFormTextItem.create(
            identityProviderCard,
            'identityProviderEntityIdentifier',
            'Identity Provider Entity Identifier')
            .withGroupName('identityProvider')
            .withParagraph('Details about the identity provider including URLs and certificates.')
            .withHeader('Identity Provider')
            .withHint("A unique identifier for the service provider or identity provider. "
                + "It's usually a URI, but it doesn't have to be a reachable location.")
            .withIcon('filing', IconLibrary.IonicV4)
            .withValidator(FormValidatorHelper.required()));
        this.samlFields.push(DetailsListFormTextItem.create(
            identityProviderCard,
            'identityProviderSingleSignOnServiceUrl',
            'Single Sign-On Service URL')
            .withGroupName('identityProvider')
            .withHint("The URL where uBind sends SAML authentication request messages to when a user needs to be "
                + "authenticated")
            .withValidator(FormValidatorHelper.webUrl(true)));
        this.samlFields.push(DetailsListFormTextItem.create(
            identityProviderCard,
            'identityProviderArtifactResolutionServiceUrl',
            'Artefact Resolution Service URL')
            .withGroupName('identityProvider')
            .withHint("Instead of sending the entire SAML assertion to the Service Provider (SP) directly (via the "
                + "user's browser), the IdP sends a smaller reference called a SAML Artifact. This approach avoids "
                + "potentially sensitive data being exposed at the user's browser, and also keeps payload sizes down.")
            .withValidator(FormValidatorHelper.webUrl(false)));
        this.samlFields.push(DetailsListFormTextItem.create(
            identityProviderCard,
            'identityProviderSingleLogoutServiceUrl',
            'Single Logout Service URL')
            .withGroupName('identityProvider')
            .withHint("The URL which uBind will send a logout request to when the user logs out of uBind. "
                + "This \"SLO\" URL is used to inform the identity provider that the user has logged out of uBind, "
                + "so that the identity provider can invalidate the user's session and also notify other service "
                + "providers to log them out. The uBind service provider "
                + "SLO URL below should also be entered into the identity providers configuration so that uBind can "
                + "receive and respond to logout requests from the identity provider.")
            .withValidator(FormValidatorHelper.webUrl(false)));
        this.samlFields.push(DetailsListFormTextAreaItem.create(
            identityProviderCard,
            "identityProviderCertificate",
            "Certificate")
            .withGroupName('identityProvider')
            .withHint("The digital certificate used by the identity provider to sign SAML responses/assertions. "
                + "This certificate must contain identity provider's public key (not the private key). "
                + "uBind will use the public key to verify the signature of the SAML response/assertion."
                + "The certificate must be in PEM format. "
                + "Please open the PEM file in a text editor and paste the contents in here."));
        this.samlFields.push(DetailsListFormCheckboxItem.create(
            identityProviderCard,
            "mustSignAuthenticationRequests",
            "Sign authentication requests")
            .withGroupName('identityProvider'));

        const autoProvisioningCard: DetailsListItemCard = new DetailsListItemCard(
            'Auto Provisioning',
            'Determines whether to automatically create or update objects when someone signs in.');
        this.samlFields.push(DetailsListFormContentItem.create(
            autoProvisioningCard,
            "autoProvisioningContent",
            null)
            .withHeader('Auto Provisioning')
            .withParagraph('Determines whether to automatically create or update objects when someone signs in.')
            .withIcon('cog', IconLibrary.IonicV4)
            .withGroupName('autoProvisioning'));
        this.samlFields.push(DetailsListFormCheckboxItem.create(
            autoProvisioningCard,
            "canCustomerAccountsBeAutoProvisioned",
            "Allow automatic customer account creation")
            .withGroupName('autoProvisioning'));
        this.samlFields.push(DetailsListFormCheckboxItem.create(
            autoProvisioningCard,
            "canCustomerDetailsBeAutoUpdated",
            "Update customer details on sign-in")
            .withGroupName('autoProvisioning'));
        this.samlFields.push(DetailsListFormCheckboxItem.create(
            autoProvisioningCard,
            "canAgentAccountsBeAutoProvisioned",
            "Allow automatic agent account creation")
            .withGroupName('autoProvisioning'));
        this.samlFields.push(DetailsListFormCheckboxItem.create(
            autoProvisioningCard,
            "canAgentDetailsBeAutoUpdated",
            "Update agent details on sign-in")
            .withGroupName('autoProvisioning'));
        this.samlFields.push(DetailsListFormCheckboxItem.create(
            autoProvisioningCard,
            "canOrganisationsBeAutoProvisioned",
            "Allow automatic creation of organisations")
            .withGroupName('autoProvisioning'));
        this.samlFields.push(DetailsListFormCheckboxItem.create(
            autoProvisioningCard,
            "canOrganisationDetailsBeAutoUpdated",
            "Update organisation details on sign-in")
            .withGroupName('autoProvisioning'));

        const attributeMappingCard: DetailsListItemCard = new DetailsListItemCard(
            'Attribute Mapping',
            'How uBind interprets data and attributes included in SAML responses.');
        this.samlFields.push(DetailsListFormSelectItem.create(
            attributeMappingCard,
            'standardNameIdFormat',
            'Select a standard name ID format')
            .withIcon('pricetags', IconLibrary.IonicV4)
            .withGroupName<DetailsListFormSelectItem>('attributeMapping')
            .withOption({
                label: 'Email address (recommended)',
                value: 'urn:oasis:names:tc:SAML:1.1:nameid-format:emailAddress',
            })
            .withOption({ label: 'Unspecified', value: 'urn:oasis:names:tc:SAML:1.1:nameid-format:unspecified' })
            .withOption({ label: 'Persistent', value: 'urn:oasis:names:tc:SAML:2.0:nameid-format:persistent' })
            .withOption({ label: 'Transient', value: 'urn:oasis:names:tc:SAML:2.0:nameid-format:transient' })
            .withOption({ label: 'Other', value: '' })
            .withHeader('Attribute Mapping')
            .withParagraph("Select from one of the standard formats for the name ID. If you don't see the format you "
                + "need, you can enter a custom format below."));
        this.samlFields.push(DetailsListFormTextItem.create(
            attributeMappingCard,
            'nameIdFormat',
            'Name ID Format')
            .withGroupName('attributeMapping')
            .withValidator(FormValidatorHelper.required()));
        this.samlFields.push(DetailsListFormCheckboxItem.create(
            attributeMappingCard,
            "useNameIdAsUniqueIdentifier",
            "Use name ID as the unique identifier")
            .withGroupName('attributeMapping'));
        this.samlFields.push(DetailsListFormCheckboxItem.create(
            attributeMappingCard,
            "useNameIdAsEmailAddress",
            "Use name ID as the user's email address")
            .withGroupName('attributeMapping'));
        this.samlFields.push(DetailsListFormTextItem.create(
            attributeMappingCard,
            'uniqueIdentifierAttributeName',
            'Unique Identifier Attribute Name')
            .withGroupName('attributeMapping')
            .withValidator(FormValidatorHelper.required()));
        this.samlFields.push(DetailsListFormTextItem.create(
            attributeMappingCard,
            'userTypeAttributeName',
            'User Type Attribute Name')
            .withGroupName('attributeMapping')
            .withValidator(FormValidatorHelper.required()));
        this.samlFields.push(DetailsListFormTextItem.create(
            attributeMappingCard,
            'firstNameAttributeName',
            'First Name Attribute Name')
            .withGroupName('attributeMapping')
            .withValidator(FormValidatorHelper.required()));
        this.samlFields.push(DetailsListFormTextItem.create(
            attributeMappingCard,
            'lastNameAttributeName',
            'Last Name Attribute Name')
            .withGroupName('attributeMapping')
            .withValidator(FormValidatorHelper.required()));
        this.samlFields.push(DetailsListFormTextItem.create(
            attributeMappingCard,
            'emailAddressAttributeName',
            'Email Address Attribute Name')
            .withGroupName('attributeMapping')
            .withValidator(FormValidatorHelper.required()));
        this.samlFields.push(DetailsListFormTextItem.create(
            attributeMappingCard,
            'phoneNumberAttributeName',
            'Phone Number Attribute Name')
            .withGroupName('attributeMapping'));
        this.samlFields.push(DetailsListFormTextItem.create(
            attributeMappingCard,
            'mobileNumberAttributeName',
            'Mobile Number Attribute Name')
            .withGroupName('attributeMapping'));
        this.samlFields.push(DetailsListFormTextItem.create(
            attributeMappingCard,
            'organisationUniqueIdentifierAttributeName',
            'Organisation Unique Identifier Attribute Name')
            .withGroupName('attributeMapping'));
        this.samlFields.push(DetailsListFormTextItem.create(
            attributeMappingCard,
            'organisationNameAttributeName',
            'Organisation Name Attribute Name')
            .withGroupName('attributeMapping'));
        this.samlFields.push(DetailsListFormTextItem.create(
            attributeMappingCard,
            'organisationAliasAttributeName',
            'Organisation Alias Attribute Name')
            .withGroupName('attributeMapping')
            .withHint("Leave this blank to have a unique organisation alias generated automatically."));
        this.samlFields.push(DetailsListFormTextItem.create(
            attributeMappingCard,
            'roleAttributeName',
            'Role Attribute Name')
            .withGroupName('attributeMapping')
            .withHint("The name of the attribute that contains a user's role or roles. "
                + "The role attribute name allows us to know what roles the user has assigned by the identity "
                + "provider. The way these multi-valued attributes are represented in the SAML assertion can vary "
                + "For example, they might be included as multiple <saml:AttributeValue> elements within a "
                + "single <saml:Attribute> element, or they might be included as a single string with some form "
                + "of delimiter (such as commas or semicolons) between the individual values. "
                + "Once we have the role value(s) for a particular user, we'll still need to map the roles in the "
                + "service provider to roles in the organisation in uBind (below)."));
        this.samlFields.push(DetailsListFormTextItem.create(
            attributeMappingCard,
            'roleAttributeValueDelimiter',
            'Role Attribute Value Delimiter')
            .withGroupName('attributeMapping')
            .withHint("The delimiter used to separate multiple values in the role <saml:AttributeValue> "
                + "element. Leave empty if each role <saml:AttributeValue> element only contains a single value, "
                + "and instead there are multiple instances of the <saml:AttributeValue> element for the role "
                + "<saml:Attribute> element."));

        const roleMappingCard: DetailsListItemCard = new DetailsListItemCard(
            'Role Mapping',
            'How uBind maps a role in the identity providers system to a role in uBind.');
        const defaultAgentRoleIdDropList: DetailsListFormSelectItem = DetailsListFormSelectItem.create(
            roleMappingCard,
            'defaultAgentRoleId',
            'Default Agent Role')
            .withHint<DetailsListFormSelectItem>(
                "A role to assign to all agent users provisioned by this authentication method.")
            .withOption({ label: 'Select a role', value: '' });
        for (let role of this.roles) {
            defaultAgentRoleIdDropList.withOption({ label: role.name, value: role.id });
        }
        this.samlFields.push(defaultAgentRoleIdDropList);
        const roleMapRoleIdDropList: DetailsListFormSelectItem = DetailsListFormSelectItem.create(
            roleMappingCard,
            'ubindRoleId',
            'uBind Role')
            .withOption({ label: 'Select a role', value: '' });
        for (let role of this.roles) {
            roleMapRoleIdDropList.withOption({ label: role.name, value: role.id });
        }
        const repeatedItem: DetailsListFormItemGroup = DetailsListFormItemGroup.create(
            roleMappingCard,
            "roleMapEntry",
            "Role Mapping")
            .withItem(DetailsListFormTextItem.create(
                roleMappingCard,
                'idpRoleName',
                'IdP Role Name'))
            .withItem(roleMapRoleIdDropList);
        const roleMappingArray: DetailsListFormItemRepeating = DetailsListFormItemRepeating.create(
            roleMappingCard,
            "roleMap",
            "Role Mapping",
            repeatedItem)
            .withGroupName('roleMapping')
            .withHeader('Role Mapping')
            .withParagraph("Enter the names of roles in the identity provider's system and select corresponding roles "
                + "in uBind.")
            .withIcon<DetailsListFormItemRepeating>('shirt', IconLibrary.IonicV4);
        const exclusiveRolesItem: DetailsListFormCheckboxItem = DetailsListFormCheckboxItem.create(
            roleMappingCard,
            "areRolesManagedExclusivelyByThisIdentityProvider",
            "Roles are managed exclusively by this identity provider")
            .withGroupName('roleMapping')
            .withHint("If checked, roles cannot be manually assigned to this user wtihin uBind, and "
                + "each time the user signs in, their roles will be updated to match the roles specified in the "
                + "identity provider.");

        this.samlFields.push(roleMappingArray);
        this.samlFields.push(exclusiveRolesItem);
        this.detailsList.push(...this.samlFields);
    }

    private buildForm(): void {
        let controls: any = [];
        this.detailsList.forEach((item: DetailsListFormItem) => {
            if (item.FormControlType != 'content') {
                controls[item.Alias] = item.FormControl;
            }
        });
        this.form = this.formBuilder.group(controls);

        this.manageVisibilityOfUniqueIdentifierAttributeName();
        this.manageVisibilityOfUseNameIdAsEmailAddress();
        this.manageVisibilityOfEmailAddressAttributeName();
        this.manageVisibilityOfOrganisationAttributeNames();
        this.manageVisibilityOfCanOrganisationDetailsBeAutoUpdated();
        this.manageVisibilityOfCustomerFields();
        this.manageVisibilityOfCanCustomerDetailsBeAutoUpdated();
        this.manageVisibilityOfAgentFields();
        this.manageVisibilityOfCanAgentDetailsBeAutoUpdated();
        this.manageVisibilityOfSignInButtonFields();
        this.manageVisibilityOfSamlFields();
        this.manageVisibilityOfUserTypeAttributeNameField();

        // grab the value from the standard name ID format field and put it into the Name ID Format field
        this.form.get('standardNameIdFormat').valueChanges
            .pipe(takeUntil(this.destroyed))
            .subscribe((value: string) => {
                if (value) {
                    this.form.get('nameIdFormat').setValue(value);
                }
            });
    }

    public async close(): Promise<void> {
        this.returnToPrevious();
    }

    public async save(value: any): Promise<void> {
        if (this.form.invalid) {
            return;
        }

        let upsertModel: SamlAuthenticationMethodUpsertModel = this.model
            ? _.merge(this.model, this.form.value)
            : this.form.value;
        upsertModel.tenant = this.routeHelper.getContextTenantAlias();
        upsertModel.organisation = this.organisationId;
        upsertModel.roleMap = this.convertRoleMapEntriesToObject(this.form.value.roleMap);

        await this.sharedLoaderService.presentWithDelay();
        if (this.isEdit) {
            upsertModel.organisation = this.model.organisationId;
            this.authenticationMethodApiService.updateAuthenticationMethod(upsertModel.id, upsertModel)
                .pipe(
                    takeUntil(this.destroyed),
                    finalize(() => this.sharedLoaderService.dismiss()),
                ).subscribe((result: AuthenticationMethodResourceModel) => {
                    if (result) { // will be null if we navigate away whilst loading
                        this.eventService.getEntityUpdatedSubject('AuthenticationMethod').next(result);
                        this.sharedAlert.showToast(
                            `The authentication method '${result.name}' has been updated successfully.`);
                        this.goToDetailPage(result.id);
                    }
                });
        } else {
            this.authenticationMethodApiService.createAuthenticationMethod(upsertModel)
                .pipe(
                    takeUntil(this.destroyed),
                    finalize(() => this.sharedLoaderService.dismiss()),
                ).subscribe((result: AuthenticationMethodResourceModel) => {
                    if (result) { // will be null if we navigate away whilst loading
                        this.eventService.getEntityCreatedSubject('AuthenticationMethod').next(result);
                        this.sharedAlert.showToast(
                            `The authentication method '${result.name}' has been created successfully.`);
                        this.goToDetailPage(result.id);
                    }
                });
        }
    }

    /**
     * Converts from an array of entries to a plain JSON object, but removes any incomplete entries
     * so that the final object only contains complete entries.
     */
    private convertRoleMapEntriesToObject(
        roleMapEntries: Array<{idpRoleName: string; ubindRoleId: string}>,
    ): {[key: string]: string} {
        let roleMapObject: {[key: string]: string} = {};
        for (let roleMapEntry of roleMapEntries) {
            if (roleMapEntry.idpRoleName && roleMapEntry.ubindRoleId) {
                roleMapObject[roleMapEntry.idpRoleName] = roleMapEntry.ubindRoleId;
            }
        }
        return roleMapObject;
    }

    private returnToPrevious(): void {
        const urlSegments: Array<string> = this.routeHelper.getPathSegmentsUpUntil('sso-configurations');
        urlSegments.push('list');
        this.navProxy.navigateBack(urlSegments);
    }

    private goToDetailPage(authenticationMethodId: string): void {
        const urlSegments: Array<string> = this.routeHelper.getPathSegmentsUpUntil('sso-configurations');
        urlSegments.push(authenticationMethodId);
        this.navProxy.navigateBack(urlSegments);
    }

    private manageVisibilityOfUniqueIdentifierAttributeName(): void {
        // perform initial check
        this.updateUniqueIdentifierAttributeNameVisibility(this.form.get('useNameIdAsUniqueIdentifier').value);

        // subscribe to value changes
        this.form.get('useNameIdAsUniqueIdentifier').valueChanges
            .pipe(takeUntil(this.destroyed))
            .subscribe((value: boolean) => {
                this.updateUniqueIdentifierAttributeNameVisibility(value);
            });
    }

    private updateUniqueIdentifierAttributeNameVisibility(useNameIdAsUniqueIdentifierValue: boolean): void {
        this.detailsList.find((item: DetailsListFormItem) => item.Alias == 'uniqueIdentifierAttributeName')
            .Visible = !useNameIdAsUniqueIdentifierValue;
    }

    private manageVisibilityOfUseNameIdAsEmailAddress(): void {
        // perform initial check
        this.updateUseNameIdAsEmailAddressVisibility(this.form.get('nameIdFormat').value);

        // subscribe to value changes
        this.form.get('nameIdFormat').valueChanges.pipe(takeUntil(this.destroyed)).subscribe((value: string) => {
            this.updateUseNameIdAsEmailAddressVisibility(value);
        });
    }

    private updateUseNameIdAsEmailAddressVisibility(nameIdFormatValue: string): void {
        const visible: boolean = nameIdFormatValue.indexOf('emailAddress') !== -1;
        this.detailsList.find((item: DetailsListFormItem) => item.Alias == 'useNameIdAsEmailAddress').Visible = visible;
    }

    private manageVisibilityOfEmailAddressAttributeName(): void {
        // perform initial check
        this.updateEmailAddressAttributeNameVisibility(this.form.get('useNameIdAsEmailAddress').value);

        // subscribe to value changes
        this.form.get('useNameIdAsEmailAddress').valueChanges.pipe(takeUntil(this.destroyed))
            .subscribe((value: boolean) => {
                this.updateEmailAddressAttributeNameVisibility(value);
            });
    }

    private updateEmailAddressAttributeNameVisibility(useNameIdAsEmailAddressValue: boolean): void {
        this.detailsList.find((item: DetailsListFormItem) => item.Alias == 'emailAddressAttributeName')
            .Visible = !useNameIdAsEmailAddressValue;
    }

    private manageVisibilityOfOrganisationAttributeNames(): void {
        // perform initial check
        this.updateOrganisationAttributeNamesVisibility(this.form.get('canUsersOfManagedOrganisationsSignIn').value);

        // subscribe to value changes
        this.form.get('canUsersOfManagedOrganisationsSignIn').valueChanges.pipe(takeUntil(this.destroyed))
            .subscribe((value: boolean) => {
                this.updateOrganisationAttributeNamesVisibility(value);
            });
    }

    private updateOrganisationAttributeNamesVisibility(canUsersOfManagedOrganisationsSignInValue: boolean): void {
        this.detailsList.find((item: DetailsListFormItem) => item.Alias == 'canOrganisationsBeAutoProvisioned')
            .Visible = canUsersOfManagedOrganisationsSignInValue;
        this.detailsList.find((item: DetailsListFormItem) => item.Alias == 'canOrganisationDetailsBeAutoUpdated')
            .Visible = canUsersOfManagedOrganisationsSignInValue && this.form.get('canOrganisationsBeAutoProvisioned')
                .value;
        this.detailsList.find((item: DetailsListFormItem) => item.Alias == 'organisationUniqueIdentifierAttributeName')
            .Visible = canUsersOfManagedOrganisationsSignInValue;
        this.detailsList.find((item: DetailsListFormItem) => item.Alias == 'organisationNameAttributeName')
            .Visible = canUsersOfManagedOrganisationsSignInValue;
        this.detailsList.find((item: DetailsListFormItem) => item.Alias == 'organisationAliasAttributeName')
            .Visible = canUsersOfManagedOrganisationsSignInValue;
    }

    private manageVisibilityOfCanOrganisationDetailsBeAutoUpdated(): void {
        // perform initial check
        this.updateOrganisationDetailsAutoUpdateVisibility(this.form.get('canOrganisationsBeAutoProvisioned').value);

        // subscribe to value changes
        this.form.get('canOrganisationsBeAutoProvisioned').valueChanges.pipe(takeUntil(this.destroyed))
            .subscribe((value: boolean) => {
                this.updateOrganisationDetailsAutoUpdateVisibility(value);
            });
    }

    private updateOrganisationDetailsAutoUpdateVisibility(canOrganisationsBeAutoProvisionedValue: boolean): void {
        this.detailsList.find((item: DetailsListFormItem) => item.Alias == 'canOrganisationDetailsBeAutoUpdated')
            .Visible = canOrganisationsBeAutoProvisionedValue
                && this.form.get('canUsersOfManagedOrganisationsSignIn').value;
        this.detailsList.find((item: DetailsListFormItem) =>
            item.Alias == 'shouldLinkExistingOrganisationWithSameAlias')
            .Visible = canOrganisationsBeAutoProvisionedValue
                    && this.form.get('canUsersOfManagedOrganisationsSignIn').value;
    }

    private manageVisibilityOfCustomerFields(): void {
        // perform initial check
        this.updateCustomerFieldsVisibility(this.form.get('canCustomersSignIn').value);

        // subscribe to value changes
        this.form.get('canCustomersSignIn').valueChanges.pipe(takeUntil(this.destroyed))
            .subscribe((value: boolean) => {
                this.updateCustomerFieldsVisibility(value);
            });
    }

    private updateCustomerFieldsVisibility(canCustomersSignInValue: boolean): void {
        this.detailsList.find((item: DetailsListFormItem) => item.Alias == 'canCustomerAccountsBeAutoProvisioned')
            .Visible = canCustomersSignInValue;
        this.detailsList.find((item: DetailsListFormItem) => item.Alias == 'canCustomerDetailsBeAutoUpdated')
            .Visible = canCustomersSignInValue && this.form.get('canCustomerAccountsBeAutoProvisioned').value;
        this.detailsList.find((item: DetailsListFormItem) =>
            item.Alias == 'shouldLinkExistingCustomerWithSameEmailAddress')
            .Visible = canCustomersSignInValue;
    }

    private manageVisibilityOfCanCustomerDetailsBeAutoUpdated(): void {
        // perform initial check
        this.updateCustomerDetailsAutoUpdateVisibility(this.form.get('canCustomerAccountsBeAutoProvisioned').value);

        // subscribe to value changes
        this.form.get('canCustomerAccountsBeAutoProvisioned').valueChanges.pipe(takeUntil(this.destroyed))
            .subscribe((value: boolean) => {
                this.updateCustomerDetailsAutoUpdateVisibility(value);
            });
    }

    private updateCustomerDetailsAutoUpdateVisibility(canCustomerAccountsBeAutoProvisionedValue: boolean): void {
        this.detailsList.find((item: DetailsListFormItem) => item.Alias == 'canCustomerDetailsBeAutoUpdated')
            .Visible = canCustomerAccountsBeAutoProvisionedValue && this.form.get('canCustomersSignIn').value;
    }

    private manageVisibilityOfAgentFields(): void {
        // perform initial check
        this.updateAgentFieldsVisibility(this.form.get('canAgentsSignIn').value);

        // subscribe to value changes
        this.form.get('canAgentsSignIn').valueChanges.pipe(takeUntil(this.destroyed))
            .subscribe((value: boolean) => {
                this.updateAgentFieldsVisibility(value);
            });
    }

    private updateAgentFieldsVisibility(canAgentsSignInValue: boolean): void {
        this.detailsList.find((item: DetailsListFormItem) => item.Alias == 'canAgentAccountsBeAutoProvisioned')
            .Visible = canAgentsSignInValue;
        this.detailsList.find((item: DetailsListFormItem) => item.Alias == 'canAgentDetailsBeAutoUpdated')
            .Visible = canAgentsSignInValue && this.form.get('canAgentAccountsBeAutoProvisioned').value;
        this.detailsList.find((item: DetailsListFormItem) =>
            item.Alias == 'shouldLinkExistingAgentWithSameEmailAddress')
            .Visible = canAgentsSignInValue;
    }

    private manageVisibilityOfUserTypeAttributeNameField(): void {
        // perform initial check
        this.updateUserTypeAttributeNameFieldVisibility(
            this.form.get('canAgentsSignIn').value, this.form.get('canCustomersSignIn').value);

        // subscribe to value changes
        this.form.get('canAgentsSignIn').valueChanges.pipe(takeUntil(this.destroyed))
            .subscribe((value: boolean) => {
                this.updateUserTypeAttributeNameFieldVisibility(value, this.form.get('canCustomersSignIn').value);
            });
        this.form.get('canCustomersSignIn').valueChanges.pipe(takeUntil(this.destroyed))
            .subscribe((value: boolean) => {
                this.updateUserTypeAttributeNameFieldVisibility(this.form.get('canAgentsSignIn').value, value);
            });
    }

    private updateUserTypeAttributeNameFieldVisibility(
        canAgentsSignInValue: boolean,
        canCustomersSignInValue: boolean,
    ): void {
        this.detailsList.find((item: DetailsListFormItem) => item.Alias == 'userTypeAttributeName')
            .Visible = canAgentsSignInValue && canCustomersSignInValue;
    }

    private manageVisibilityOfCanAgentDetailsBeAutoUpdated(): void {
        // perform initial check
        this.updateAgentDetailsAutoUpdateVisibility(this.form.get('canAgentAccountsBeAutoProvisioned').value);

        // subscribe to value changes
        this.form.get('canAgentAccountsBeAutoProvisioned').valueChanges.pipe(takeUntil(this.destroyed))
            .subscribe((value: boolean) => {
                this.updateAgentDetailsAutoUpdateVisibility(value);
            });
    }

    private updateAgentDetailsAutoUpdateVisibility(canAgentAccountsBeAutoProvisionedValue: boolean): void {
        this.detailsList.find((item: DetailsListFormItem) => item.Alias == 'canAgentDetailsBeAutoUpdated')
            .Visible = canAgentAccountsBeAutoProvisionedValue && this.form.get('canAgentsSignIn').value;
    }

    private manageVisibilityOfSamlFields(): void {
        // perform initial check
        this.updateSamlFieldsVisibility(this.form.get('typeName').value);

        // subscribe to value changes
        this.form.get('typeName').valueChanges.pipe(takeUntil(this.destroyed))
            .subscribe((value: string) => {
                this.updateSamlFieldsVisibility(value);
            });
    }

    private updateSamlFieldsVisibility(typeNameValue: string): void {
        if (typeNameValue == AuthenticationMethodType.Saml) {
            this.samlFields.forEach((item: DetailsListFormItem) => item.Visible = true);
            this.updateUniqueIdentifierAttributeNameVisibility(this.form.get('useNameIdAsUniqueIdentifier').value);
            this.updateUseNameIdAsEmailAddressVisibility(this.form.get('nameIdFormat').value);
            this.updateEmailAddressAttributeNameVisibility(this.form.get('useNameIdAsEmailAddress').value);
            this.updateOrganisationAttributeNamesVisibility(
                this.form.get('canUsersOfManagedOrganisationsSignIn').value);
            this.updateOrganisationDetailsAutoUpdateVisibility(
                this.form.get('canOrganisationsBeAutoProvisioned').value);
            this.updateCustomerFieldsVisibility(this.form.get('canCustomersSignIn').value);
            this.updateCustomerDetailsAutoUpdateVisibility(
                this.form.get('canCustomerAccountsBeAutoProvisioned').value);
            this.updateAgentFieldsVisibility(this.form.get('canAgentsSignIn').value);
            this.updateAgentDetailsAutoUpdateVisibility(this.form.get('canAgentAccountsBeAutoProvisioned').value);
            this.updateSignInButtonFieldsVisibility(this.form.get('includeSignInButtonOnPortalLoginPage').value);
        } else {
            this.samlFields.forEach((item: DetailsListFormItem) => item.Visible = false);
        }
    }

    private manageVisibilityOfSignInButtonFields(): void {
        // perform initial check
        this.updateSignInButtonFieldsVisibility(this.form.get('includeSignInButtonOnPortalLoginPage').value);

        // subscribe to value changes
        this.form.get('includeSignInButtonOnPortalLoginPage').valueChanges.pipe(takeUntil(this.destroyed))
            .subscribe((value: boolean) => {
                this.updateSignInButtonFieldsVisibility(value);
            });
    }

    private updateSignInButtonFieldsVisibility(includeSignInButtonOnPortalLoginPageValue: boolean): void {
        this.detailsList.find((item: DetailsListFormItem) => item.Alias == 'signInButtonLabel')
            .Visible = includeSignInButtonOnPortalLoginPageValue;
        this.detailsList.find((item: DetailsListFormItem) => item.Alias == 'signInButtonBackgroundColor')
            .Visible = includeSignInButtonOnPortalLoginPageValue;
        this.detailsList.find((item: DetailsListFormItem) => item.Alias == 'signInButtonIconUrl')
            .Visible = includeSignInButtonOnPortalLoginPageValue;
    }
}
