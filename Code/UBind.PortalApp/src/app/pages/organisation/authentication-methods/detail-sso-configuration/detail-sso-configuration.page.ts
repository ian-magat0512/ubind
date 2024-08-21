import { Component, ElementRef, Injector, OnDestroy, OnInit } from "@angular/core";
import { ActionButtonHelper } from "@app/helpers/action-button.helper";
import { DetailListItemHelper } from "@app/helpers/detail-list-item.helper";
import { RouteHelper } from "@app/helpers/route.helper";
import { TypeHelper } from "@app/helpers/type.helper";
import { ActionButton } from "@app/models/action-button";
import { ActionButtonPopover } from "@app/models/action-button-popover";
import { DetailsListItem } from "@app/models/details-list/details-list-item";
import { DetailsListItemCardType } from "@app/models/details-list/details-list-item-card-type.enum";
import { DetailsListGroupItemModel } from "@app/models/details-list/details-list-item-model";
import { IconLibrary } from "@app/models/icon-library.enum";
import { PopoverCommand } from "@app/models/popover-command";
import { DetailPage } from "@app/pages/master-detail/detail.page";
import { AuthenticationMethodResourceModel } from "@app/resource-models/authentication-method.resource-model";
import { AuthenticationMethodApiService } from "@app/services/api/authentication-method-api.service";
import { EventService } from "@app/services/event.service";
import { LayoutManagerService } from "@app/services/layout-manager.service";
import { NavProxyService } from "@app/services/nav-proxy.service";
import { Subject } from "rxjs";
import { finalize, takeUntil } from "rxjs/operators";
import { PopoverSsoConfigurationPage } from "../popover-sso-configuration/popover-sso-configuration";
import { SharedPopoverService } from "@app/services/shared-popover.service";
import { SharedAlertService } from "@app/services/shared-alert.service";
import { contentAnimation } from '@assets/animations';
import { RoleResourceModel } from "@app/resource-models/role.resource-model";
import { RoleApiService } from "@app/services/api/role-api.service";
import { PermissionService } from "@app/services/permission.service";
import { Permission } from "@app/helpers";
import { OrganisationModel } from "@app/models/organisation.model";

/**
 * Allows a user to view the details of an SSO configuration and perform related actions with it.
 */
@Component({
    selector: 'app-detail-sso-configuration-page',
    templateUrl: './detail-sso-configuration.page.html',
    animations: [contentAnimation],
    styleUrls: [
        './detail-sso-configuration.page.scss',
        '../../../../../assets/css/scrollbar-form.css',
        '../../../../../assets/css/form-toolbar.scss',
    ],
})
export class DetailSsoConfigurationPage extends DetailPage implements OnInit, OnDestroy {
    private organisationId: string;
    private performingUserOrganisationId?: string;
    private authenticationMethodId: string;
    public ssoConfiguration: AuthenticationMethodResourceModel;
    protected hasActionsIncludedInMenu: boolean = true;
    protected actions: Array<ActionButtonPopover> = [];
    public actionButtonList: Array<ActionButton>;
    public canShowMore: boolean = true;
    public flipMoreIcon: boolean = false;
    public detailsListItems: Array<DetailsListItem>;
    public title: string = 'SSO Configuration';
    private roles: Array<RoleResourceModel>;

    public constructor(
        private routeHelper: RouteHelper,
        private navProxy: NavProxyService,
        private authenticationMethodApiService: AuthenticationMethodApiService,
        public layoutManager: LayoutManagerService,
        private sharedPopoverService: SharedPopoverService,
        private sharedAlertService: SharedAlertService,
        eventService: EventService,
        elementRef: ElementRef,
        injector: Injector,
        private roleApiService: RoleApiService,
        private permissionService: PermissionService,
    ) {
        super(eventService, elementRef, injector);
    }

    public ngOnInit(): void {
        this.destroyed = new Subject<void>();
        this.organisationId = this.routeHelper.getParam('organisationId');
        this.authenticationMethodId = this.routeHelper.getParam('authenticationMethodId');
        this.load().then(() => this.initializeActionButtonList());
        this.eventService.performingUserOrganisationSubject$
            .pipe(takeUntil(this.destroyed))
            .subscribe((organisation: OrganisationModel) => {
                this.performingUserOrganisationId = organisation.id;
            });
    }

    public ngOnDestroy(): void {
        if (this.destroyed) {
            this.destroyed.next();
            this.destroyed.complete();
        }
    }

    public goBack(): void {
        const urlSegments: Array<string> = this.routeHelper.getPathSegmentsUpUntil('sso-configurations');
        urlSegments.push('list');
        this.navProxy.navigateBack(urlSegments);
    }

    private async load(): Promise<void> {
        this.isLoading = true;
        return this.authenticationMethodApiService
            .getAuthenticationMethod(this.authenticationMethodId, this.routeHelper.getContextTenantAlias())
            .pipe(
                takeUntil(this.destroyed),
                finalize(() => this.isLoading = false),
            )
            .toPromise()
            .then((ssoConfiguration: AuthenticationMethodResourceModel) => {
                this.ssoConfiguration = ssoConfiguration;
                this.title = 'SSO Configuration: ' + this.ssoConfiguration.name;
                if (TypeHelper.isSamlAuthenticationMethodResourceModel(this.ssoConfiguration)) {
                    const roleMapObject: any = this.ssoConfiguration.roleMap;
                    const hasRoles: boolean = this.ssoConfiguration.defaultAgentRoleId != null
                        || (roleMapObject != null && Object.keys(roleMapObject).length > 0);
                    if (hasRoles) {
                        this.loadRoles().then(() => this.initializeDetailsListItems());
                    } else {
                        this.initializeDetailsListItems();
                    }
                } else {
                    this.initializeDetailsListItems();
                }
            },
            (err: any) => {
                this.errorMessage = 'There was a problem loading this SSO configuration';
                throw err;
            });
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

    private initializeDetailsListItems(): void {
        let items: Array<DetailsListItem> = [];

        const detailsItems: Array<DetailsListGroupItemModel> = [
            DetailsListGroupItemModel.create('name', this.ssoConfiguration.name),
            DetailsListGroupItemModel.create('status', this.ssoConfiguration.disabled ? 'Disabled' : 'Enabled'),
            DetailsListGroupItemModel.create('type', this.ssoConfiguration.typeName),
        ];

        items = items.concat(
            DetailListItemHelper.createDetailItemGroup(
                DetailsListItemCardType.Details, detailsItems, "card-account-details", IconLibrary.AngularMaterial));

        const signInOptions: Array<DetailsListGroupItemModel> = [];
        signInOptions.push(DetailsListGroupItemModel.create(
            'includeSignInButtonOnPortalLoginPage',
            this.ssoConfiguration.includeSignInButtonOnPortalLoginPage ? 'Yes' : 'No'));
        if (this.ssoConfiguration.includeSignInButtonOnPortalLoginPage) {
            signInOptions.push(DetailsListGroupItemModel.create(
                'signInButtonLabel',
                this.ssoConfiguration.signInButtonLabel));
            signInOptions.push(DetailsListGroupItemModel.create(
                'signInButtonBackgroundColor',
                this.ssoConfiguration.signInButtonBackgroundColor));
            signInOptions.push(DetailsListGroupItemModel.create(
                'signInButtonIconUrl',
                this.ssoConfiguration.signInButtonIconUrl));
        }
        signInOptions.push(DetailsListGroupItemModel.create(
            'canCustomersSignIn',
            this.ssoConfiguration.canCustomersSignIn ? 'Yes' : 'No'));
        if (this.ssoConfiguration.canCustomersSignIn) {
            if (TypeHelper.isLocalAccountAuthenticationMethodResourceModel(this.ssoConfiguration)) {
                signInOptions.push(DetailsListGroupItemModel.create(
                    'allowCustomerSelfRegistration',
                    this.ssoConfiguration.allowCustomerSelfRegistration ? 'Yes' : 'No'));
            } else if (TypeHelper.isSamlAuthenticationMethodResourceModel(this.ssoConfiguration)) {
                signInOptions.push(DetailsListGroupItemModel.create(
                    'linkExistingCustomerWithSameEmailAddress',
                    this.ssoConfiguration.shouldLinkExistingCustomerWithSameEmailAddress ? 'Yes' : 'No'));
            }
        }
        signInOptions.push(DetailsListGroupItemModel.create(
            'canAgentsSignIn',
            this.ssoConfiguration.canAgentsSignIn ? 'Yes' : 'No'));
        if (this.ssoConfiguration.canAgentsSignIn) {
            if (TypeHelper.isLocalAccountAuthenticationMethodResourceModel(this.ssoConfiguration)) {
                signInOptions.push(DetailsListGroupItemModel.create(
                    'allowAgentSelfRegistration',
                    this.ssoConfiguration.allowAgentSelfRegistration ? 'Yes' : 'No'));
            } else if (TypeHelper.isSamlAuthenticationMethodResourceModel(this.ssoConfiguration)) {
                signInOptions.push(DetailsListGroupItemModel.create(
                    'linkExistingAgentWithSameEmailAddress',
                    this.ssoConfiguration.shouldLinkExistingAgentWithSameEmailAddress ? 'Yes' : 'No'));
            }
        }

        if (TypeHelper.isSamlAuthenticationMethodResourceModel(this.ssoConfiguration)) {
            signInOptions.push(DetailsListGroupItemModel.create(
                'canUsersOfManagedOrganisationsSignIn',
                this.ssoConfiguration.canUsersOfManagedOrganisationsSignIn ? 'Yes' : 'No'));
        }

        items = items.concat(
            DetailListItemHelper.createDetailItemGroup(
                DetailsListItemCardType.SignInOptions,
                signInOptions,
                "form-textbox-password",
                IconLibrary.AngularMaterial));

        if (TypeHelper.isSamlAuthenticationMethodResourceModel(this.ssoConfiguration)) {
            const samlIdentityProviderDetails: Array<DetailsListGroupItemModel> = [];
            samlIdentityProviderDetails.push(DetailsListGroupItemModel.create(
                "entityIdentifier",
                this.ssoConfiguration.identityProviderEntityIdentifier));
            samlIdentityProviderDetails.push(DetailsListGroupItemModel.create(
                "singleSignOnServiceUrl",
                this.ssoConfiguration.identityProviderSingleSignOnServiceUrl));
            if (this.ssoConfiguration.identityProviderSingleLogoutServiceUrl) {
                samlIdentityProviderDetails.push(DetailsListGroupItemModel.create(
                    "singleLogoutServiceUrl",
                    this.ssoConfiguration.identityProviderSingleLogoutServiceUrl));
            }
            if (this.ssoConfiguration.identityProviderArtifactResolutionServiceUrl) {
                samlIdentityProviderDetails.push(DetailsListGroupItemModel.create(
                    "artefactResolutionServiceUrl",
                    this.ssoConfiguration.identityProviderArtifactResolutionServiceUrl));
            }
            samlIdentityProviderDetails.push(DetailsListGroupItemModel.create(
                "certificate",
                this.ssoConfiguration.identityProviderCertificate ? "Provided" : "Not Provided"));
            samlIdentityProviderDetails.push(DetailsListGroupItemModel.create(
                "signAuthenticationRequests",
                this.ssoConfiguration.mustSignAuthenticationRequests ? "Yes" : "No"));

            items = items.concat(DetailListItemHelper.createDetailItemGroup(
                DetailsListItemCardType.SamlIdentityProvider,
                samlIdentityProviderDetails,
                "filing",
                IconLibrary.IonicV4));

            const autoProvisioningDetails: Array<DetailsListGroupItemModel> = [];
            autoProvisioningDetails.push(DetailsListGroupItemModel.create(
                'canCustomerAccountsBeAutoProvisioned',
                this.ssoConfiguration.canCustomerAccountsBeAutoProvisioned ? 'Yes' : 'No'));
            if (this.ssoConfiguration.canCustomerAccountsBeAutoProvisioned) {
                autoProvisioningDetails.push(DetailsListGroupItemModel.create(
                    'canCustomerDetailsBeAutoUpdated',
                    this.ssoConfiguration.canCustomerDetailsBeAutoUpdated ? 'Yes' : 'No'));
            }
            autoProvisioningDetails.push(DetailsListGroupItemModel.create(
                'canAgentAccountsBeAutoProvisioned',
                this.ssoConfiguration.canCustomerAccountsBeAutoProvisioned ? 'Yes' : 'No'));
            if (this.ssoConfiguration.canAgentAccountsBeAutoProvisioned) {
                autoProvisioningDetails.push(DetailsListGroupItemModel.create(
                    'canAgentDetailsBeAutoUpdated',
                    this.ssoConfiguration.canAgentDetailsBeAutoUpdated ? 'Yes' : 'No'));
            }
            autoProvisioningDetails.push(DetailsListGroupItemModel.create(
                'canOrganisationsBeAutoProvisioned',
                this.ssoConfiguration.canOrganisationsBeAutoProvisioned ? 'Yes' : 'No'));
            if (this.ssoConfiguration.canOrganisationsBeAutoProvisioned) {
                autoProvisioningDetails.push(DetailsListGroupItemModel.create(
                    'canOrganisationDetailsBeAutoUpdated',
                    this.ssoConfiguration.canOrganisationDetailsBeAutoUpdated ? 'Yes' : 'No'));
            }
            if (this.ssoConfiguration.canOrganisationsBeAutoProvisioned) {
                autoProvisioningDetails.push(DetailsListGroupItemModel.create(
                    'shouldLinkExistingOrganisationWithSameAlias',
                    this.ssoConfiguration.shouldLinkExistingOrganisationWithSameAlias ? 'Yes' : 'No'));
            }

            items = items.concat(DetailListItemHelper.createDetailItemGroup(
                DetailsListItemCardType.AutoProvisioning,
                autoProvisioningDetails,
                "cog",
                IconLibrary.IonicV4));

            const samlAttributes: Array<DetailsListGroupItemModel> = [];
            samlAttributes.push(DetailsListGroupItemModel.create(
                'nameIdFormat',
                this.ssoConfiguration.nameIdFormat));
            samlAttributes.push(DetailsListGroupItemModel.create(
                'useNameIdAsUniqueIdentifier',
                this.ssoConfiguration.useNameIdAsUniqueIdentifier ? 'Yes' : 'No'));
            if (this.ssoConfiguration.useNameIdAsEmailAddress !== undefined) {
                samlAttributes.push(DetailsListGroupItemModel.create(
                    'useNameIdAsEmailAddress',
                    this.ssoConfiguration.useNameIdAsEmailAddress ? 'Yes' : 'No'));
            }
            if (this.ssoConfiguration.uniqueIdentifierAttributeName !== undefined) {
                samlAttributes.push(DetailsListGroupItemModel.create(
                    'uniqueIdentifierAttributeName',
                    this.ssoConfiguration.uniqueIdentifierAttributeName));
            }
            if (this.ssoConfiguration.firstNameAttributeName !== undefined) {
                samlAttributes.push(DetailsListGroupItemModel.create(
                    'userTypeAttributeName',
                    this.ssoConfiguration.userTypeAttributeName));
            }
            if (this.ssoConfiguration.firstNameAttributeName !== undefined) {
                samlAttributes.push(DetailsListGroupItemModel.create(
                    'firstNameAttributeName',
                    this.ssoConfiguration.firstNameAttributeName));
            }
            if (this.ssoConfiguration.lastNameAttributeName !== undefined) {
                samlAttributes.push(DetailsListGroupItemModel.create(
                    'lastNameAttributeName',
                    this.ssoConfiguration.lastNameAttributeName));
            }
            if (this.ssoConfiguration.emailAddressAttributeName !== undefined) {
                samlAttributes.push(DetailsListGroupItemModel.create(
                    'emailAddressAttributeName',
                    this.ssoConfiguration.emailAddressAttributeName));
            }
            if (this.ssoConfiguration.emailAddressAttributeName !== undefined) {
                samlAttributes.push(DetailsListGroupItemModel.create(
                    'phoneNumberAttributeName',
                    this.ssoConfiguration.phoneNumberAttributeName));
            }
            if (this.ssoConfiguration.emailAddressAttributeName !== undefined) {
                samlAttributes.push(DetailsListGroupItemModel.create(
                    'mobileNumberAttributeName',
                    this.ssoConfiguration.mobileNumberAttributeName));
            }
            if (this.ssoConfiguration.organisationUniqueIdentifierAttributeName !== undefined) {
                samlAttributes.push(DetailsListGroupItemModel.create(
                    'organisationUniqueIdentifierAttributeName',
                    this.ssoConfiguration.organisationUniqueIdentifierAttributeName));
            }
            if (this.ssoConfiguration.organisationNameAttributeName !== undefined) {
                samlAttributes.push(DetailsListGroupItemModel.create(
                    'organisationNameAttributeName',
                    this.ssoConfiguration.organisationNameAttributeName));
            }
            if (this.ssoConfiguration.organisationAliasAttributeName !== undefined) {
                samlAttributes.push(DetailsListGroupItemModel.create(
                    'organisationAliasAttributeName',
                    this.ssoConfiguration.organisationAliasAttributeName));
            }
            if (this.ssoConfiguration.roleAttributeName !== undefined) {
                samlAttributes.push(DetailsListGroupItemModel.create(
                    'roleAttributeName',
                    this.ssoConfiguration.roleAttributeName));
            }
            if (this.ssoConfiguration.roleAttributeValueDelimiter !== undefined) {
                samlAttributes.push(DetailsListGroupItemModel.create(
                    'roleAttributeValueDelimiter',
                    this.ssoConfiguration.roleAttributeValueDelimiter));
            }

            items = items.concat(DetailListItemHelper.createDetailItemGroup(
                DetailsListItemCardType.SamlAttributes,
                samlAttributes,
                "pricetags",
                IconLibrary.IonicV4));

            const roleMappings: Array<DetailsListGroupItemModel> = [];
            if (this.ssoConfiguration.defaultAgentRoleId) {
                const roleId: string = this.ssoConfiguration.defaultAgentRoleId;
                const role: RoleResourceModel = this.roles.find((role: RoleResourceModel) => {
                    return role.id === roleId;
                });
                if (role) {
                    roleMappings.push(DetailsListGroupItemModel.create("Default Agent Role", role.name));
                }
            }

            if (this.ssoConfiguration.roleMap !== undefined) {
                for (let key in this.ssoConfiguration.roleMap) {
                    if (Object.prototype.hasOwnProperty.call(this.ssoConfiguration.roleMap, key)) {
                        let ubindRoleId: string = this.ssoConfiguration.roleMap[key];
                        const role: RoleResourceModel = this.roles.find((role: RoleResourceModel) => {
                            return role.id === ubindRoleId;
                        });
                        if (role) {
                            roleMappings.push(DetailsListGroupItemModel.create(key, role.name));
                        }
                    }
                }
            }

            roleMappings.push(DetailsListGroupItemModel.create(
                'rolesManagedExclusivelyByThisIdentityProvider',
                this.ssoConfiguration.areRolesManagedExclusivelyByThisIdentityProvider ? 'Yes' : 'No'));

            items = items.concat(DetailListItemHelper.createDetailItemGroup(
                DetailsListItemCardType.RoleMappings,
                roleMappings,
                "shirt",
                IconLibrary.IonicV4));
        }

        this.detailsListItems = items;
    }

    private initializeActionButtonList(): void {
        const actionButtonList: Array<ActionButton> = [];
        if (this.canUserEdit()) {
            actionButtonList.push(ActionButton.createActionButton(
                "Edit",
                "pencil",
                IconLibrary.AngularMaterial,
                false,
                "Edit Config",
                true,
                (): void => {
                    return this.didSelectEdit();
                },
            ));
        }

        if (TypeHelper.isSamlAuthenticationMethodResourceModel(this.ssoConfiguration)) {
            actionButtonList.push(ActionButton.createActionButton(
                "Metadata",
                "code",
                IconLibrary.IonicV4,
                false,
                "Show SAML Metadata",
                true,
                (): void => {
                    return this.showSamlMetadata();
                },
            ));
        }
        this.actionButtonList = ActionButtonHelper.sortActionButtons(actionButtonList);
    }

    public async presentPopover(event: any): Promise<void> {
        this.flipMoreIcon = true;
        const popoverDismissAction = (command: PopoverCommand): void => {
            this.flipMoreIcon = false;
            if (command && command.data && command.data.action) {
                const commandAction: string = command.data.action.actionName;
                if (command.data.action.callback) {
                    command.data.action.callback();
                } else if (commandAction === 'edit') {
                    this.didSelectEdit();
                } else if (commandAction === 'enable') {
                    this.didSelectEnable();
                } else if (commandAction === 'disable') {
                    this.didSelectDisable();
                } else if (commandAction === 'delete') {
                    this.didSelectDelete();
                } else if (commandAction === 'showSamlMetadata') {
                    this.showSamlMetadata();
                }
            }
        };

        await this.sharedPopoverService.show(
            {
                component: PopoverSsoConfigurationPage,
                componentProps: {
                    isPortalDisabled: this.ssoConfiguration.disabled,
                    actions: this.actions,
                },
                cssClass: 'custom-popover more-button-top-popover-positioning',
                event: event,
            },
            'SSO configuration option popover',
            popoverDismissAction);
    }

    private canUserEdit(): boolean {
        const isUserAllowedToEdit: boolean =
            this.permissionService.hasPermission(Permission.ManageOrganisations)
            &&  this.performingUserOrganisationId == this.ssoConfiguration.organisationId;
        const doesUserHasManageAllOrganisations: boolean =
        this.permissionService.hasPermission(Permission.ManageAllOrganisations);
        return isUserAllowedToEdit || doesUserHasManageAllOrganisations;
    }

    private didSelectEdit(): void {
        let pathSegments: Array<string> = this.routeHelper.getPathSegments();
        pathSegments.pop();
        pathSegments.push(this.ssoConfiguration.id);
        pathSegments.push('edit');
        this.navProxy.navigateForward(pathSegments);
    }

    private didSelectDelete(): void {
        this.authenticationMethodApiService
            .deleteAuthenticationMethod(this.ssoConfiguration.id, this.routeHelper.getContextTenantAlias())
            .pipe(takeUntil(this.destroyed))
            .subscribe(() => {
                this.eventService.getEntityDeletedSubject('AuthenticationMethod').next(this.ssoConfiguration);
                this.sharedAlertService.showToast(
                    `The authentication method ${this.ssoConfiguration.name} has been deleted`);
                this.goBack();
            });
    }

    private didSelectEnable(): void {
        this.authenticationMethodApiService
            .enableAuthenticationMethod(this.ssoConfiguration.id, this.routeHelper.getContextTenantAlias())
            .pipe(takeUntil(this.destroyed))
            .subscribe((model: AuthenticationMethodResourceModel) => {
                if (model) { // will be null if we navigate away whilst loading
                    this.eventService.getEntityUpdatedSubject('AuthenticationMethod').next(model);
                    this.sharedAlertService.showToast(`The authentication method ${model.name} has been enabled`);
                }
            });
    }

    private didSelectDisable(): void {
        this.authenticationMethodApiService
            .disableAuthenticationMethod(this.ssoConfiguration.id, this.routeHelper.getContextTenantAlias())
            .pipe(takeUntil(this.destroyed))
            .subscribe((model: AuthenticationMethodResourceModel) => {
                if (model) { // will be null if we navigate away whilst loading
                    this.eventService.getEntityUpdatedSubject('AuthenticationMethod').next(model);
                    this.sharedAlertService.showToast(`The authentication method ${model.name} has been disabled`);
                }
            });
    }

    private showSamlMetadata(): void {
        const pathSegments: Array<string> = this.routeHelper.getPathSegments();
        pathSegments.push('metadata');
        this.navProxy.navigateForward(pathSegments);
    }
}
