import { Component, Injector, ElementRef } from '@angular/core';
import { FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { AlertController, LoadingController } from '@ionic/angular';
import { UserResourceModel } from '@app/resource-models/user/user.resource-model';
import { AccountApiService } from '@app/services/api/account-api.service';
import { NavProxyService } from '@app/services/nav-proxy.service';
import { scrollbarStyle } from '@assets/scrollbar';
import { SharedAlertService } from '@app/services/shared-alert.service';
import { LayoutManagerService } from '@app/services/layout-manager.service';
import { EventService } from '@app/services/event.service';
import { AuthenticationService } from '@app/services/authentication.service';
import { AppConfigService } from '@app/services/app-config.service';
import { SharedLoaderService } from '@app/services/shared-loader.service';
import { InvitationApiService } from '@app/services/api/invitation-api.service';
import { RouteHelper } from '@app/helpers/route.helper';
import { FormHelper } from '@app/helpers/form.helper';
import { UserTypePathHelper } from '@app//helpers/user-type-path.helper';
import { PersonCategory } from '@app//models';
import { CreateEditPersonComponent } from '@app/components/create-edit-person/create-edit-person.component';
import { UserApiService } from '@app/services/api/user-api.service';
import { RoleApiService } from '@app/services/api/role-api.service';
import { ActivatedRoute } from '@angular/router';
import { EntityType } from '@app/models/entity-type.enum';
import { AdditionalPropertyValue } from '@app/models/additional-property-item-view.model';
import { UserService } from '@app/services/user.service';
import { AdditionalPropertyDefinitionApiService } from '@app/services/api/additional-property-definition-api.service';
import { TenantService } from '@app/services/tenant.service';
import { AdditionalPropertyValueService } from '@app/services/additional-property-value.service';
import { OrganisationApiService } from '@app/services/api/organisation-api.service';
import { PortalApiService } from '@app/services/api/portal-api.service';
import { FeatureSettingService } from '@app/services/feature-setting.service';
import { PermissionService } from '@app/services/permission.service';
import { PersonDetailsHelper } from '@app/helpers/person-details.helper';
import { AdditionalPropertiesHelper } from '@app/helpers/additional-properties.helper';
import { DetailsListFormItem } from '@app/models/details-list/details-list-form-item';

/**
 * Export edit account page component class
 * This class manage editing of account.
 */
@Component({
    selector: 'app-edit-account',
    templateUrl: '../../../components/create-edit-person/create-edit-person.component.html',
    styleUrls: [
        '../../../../assets/css/scrollbar-form.css',
        '../../../../assets/css/form-toolbar.scss',
    ],
    styles: [
        scrollbarStyle,
    ],
})

export class EditAccountPage extends CreateEditPersonComponent {
    public entityType: EntityType = EntityType.User;
    public isEdit: boolean = true;
    public routeParamName: string = 'account';
    public personAdditionalPropertyValueFields: Array<AdditionalPropertyValue> = [];
    public constructor(
        protected userApiService: UserApiService,
        protected roleApiService: RoleApiService,
        protected authService: AuthenticationService,
        protected permissionService: PermissionService,
        protected alertCtrl: AlertController,
        protected eventService: EventService,
        protected formBuilder: FormBuilder,
        protected loadCtrl: LoadingController,
        public navProxy: NavProxyService,
        protected appConfigService: AppConfigService,
        protected accountApiService: AccountApiService,
        public userPath: UserTypePathHelper,
        protected sharedAlertService: SharedAlertService,
        protected loader: SharedLoaderService,
        protected invitationService: InvitationApiService,
        public layoutManager: LayoutManagerService,
        route: ActivatedRoute,
        routeHelper: RouteHelper,
        formHelper: FormHelper,
        elementRef: ElementRef,
        injector: Injector,
        public userService: UserService,
        protected additionalPropertyDefinitionService: AdditionalPropertyDefinitionApiService,
        protected tenantService: TenantService,
        protected additionalPropertyValueService: AdditionalPropertyValueService,
        protected organisationApiService: OrganisationApiService,
        protected portalApiService: PortalApiService,
        featureSettingService: FeatureSettingService,
    ) {
        super(
            userApiService,
            roleApiService,
            authService,
            permissionService,
            sharedAlertService,
            eventService,
            formBuilder,
            navProxy,
            appConfigService,
            accountApiService,
            PersonCategory.User,
            loader,
            invitationService,
            layoutManager,
            route,
            routeHelper,
            formHelper,
            elementRef,
            injector,
            userService,
            additionalPropertyDefinitionService,
            tenantService,
            additionalPropertyValueService,
            organisationApiService,
            portalApiService,
            featureSettingService,
        );
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
        if ((this.personAdditionalPropertyValueFields && this.personAdditionalPropertyValueFields.length > 0)
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
        form.addControl('status', new FormControl('', Validators.required));
        if (defaultAdditionalPropertyValues) {
            form.patchValue(defaultAdditionalPropertyValues);
        }

        this.generateFormSelection();
        return form;
    }

    public didSelectUpdate(value: any): void {
        const successCallback: (user: UserResourceModel) => Promise<void>
            = async (user: UserResourceModel): Promise<void> => {
                this.eventService.getEntityUpdatedSubject('User').next(user);
                this.authenticationService.updateEmail(user.email);
                await this.checkAndHandleValueForPersonEmailChange(value.accountEmail);
                this.returnToPrevious();
            };
        this.didSelectUpdateWithCallback(value, successCallback, () => this.returnToPrevious());
    }

    public returnToPrevious(): void {
        this.navProxy.navigateBack([this.userPath.account]);
    }
}
