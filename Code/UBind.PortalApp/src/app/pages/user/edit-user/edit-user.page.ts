import { Component, Injector, ElementRef } from '@angular/core';
import { FormBuilder } from '@angular/forms';
import { SharedLoaderService } from '@app/services/shared-loader.service';
import { SharedAlertService } from '@app/services/shared-alert.service';
import { AuthenticationService } from '@app/services/authentication.service';
import { UserApiService } from '@app/services/api/user-api.service';
import { InvitationApiService } from '@app/services/api/invitation-api.service';
import { NavProxyService } from '@app/services/nav-proxy.service';
import { AppConfigService } from '@app/services/app-config.service';
import { scrollbarStyle } from '@assets/scrollbar';
import { LayoutManagerService } from '@app/services/layout-manager.service';
import { UserResourceModel } from '@app/resource-models/user/user.resource-model';
import { RouteHelper } from '@app/helpers/route.helper';
import { EventService } from '@app/services/event.service';
import { FormHelper } from '@app/helpers/form.helper';
import { PersonCategory } from '@app/models';
import { CreateEditPersonComponent } from '@app/components/create-edit-person/create-edit-person.component';
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
import { PersonViewModel } from '@app/viewmodels/person.viewmodel';
import { FeatureSettingService } from '@app/services/feature-setting.service';
import { AdditionalPropertiesHelper } from '@app/helpers/additional-properties.helper';
import { PermissionService } from '@app/services/permission.service';

/**
 * Export edit user page component class.
 * This class manage of editing of user details.
 */
@Component({
    selector: 'app-edit-user',
    templateUrl: '../../../components/create-edit-person/create-edit-person.component.html',
    styleUrls: [
        '../../../../assets/css/scrollbar-form.css',
        '../../../../assets/css/form-toolbar.scss',
    ],
    styles: [scrollbarStyle],
})
export class EditUserPage extends CreateEditPersonComponent {
    public entityType: EntityType = EntityType.User;
    public isEdit: boolean = true;
    public routeParamName: string = 'userId';
    public personAdditionalPropertyValueFields: Array<AdditionalPropertyValue> = [];

    public constructor(
        protected roleApiService: RoleApiService,
        protected authService: AuthenticationService,
        protected permissionService: PermissionService,
        protected eventService: EventService,
        protected formBuilder: FormBuilder,
        public navProxy: NavProxyService,
        protected appConfigService: AppConfigService,
        protected userApiService: UserApiService,
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
        portalApiService: PortalApiService,
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
            userApiService,
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

    protected async loadEntity(): Promise<void> {
        let params: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
        params.set('tenant', this.routeHelper.getContextTenantAlias());
        try {
            const user: UserResourceModel = await this.userApiService
                .getById(this.routeHelper.getParam(this.routeParamName), params)
                .toPromise();
            this.configureUser(user);
        } catch (error) {
            this.errorMessage = "There was a problem loading the user's details";
            throw error;
        } finally {
            this.isLoading = false;
        }
    }

    protected configureUser(user: UserResourceModel): void {
        this.person = PersonViewModel.createPersonFromUser(user);

        this.personForm = this.generateFormBuilder(true);
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
            portal: user.portalId,
        });

        if (this.canEditAdditionalPropertyValues
            && this.person.additionalPropertyValues && this.person.additionalPropertyValues.length > 0
        ) {
            let additionalPropertyValues: any = [];
            this.person.additionalPropertyValues.forEach((app: AdditionalPropertyValue) => {
                let id: string = AdditionalPropertiesHelper.generateControlId(app.additionalPropertyDefinitionModel.id);
                additionalPropertyValues[id] = app.value;
            });
            this.personForm.patchValue(additionalPropertyValues);

            this.additionalPropertyValueService.registerUniqueAdditionalPropertyFieldsOnValueChanges(
                this.person.additionalPropertyValues,
                this.personForm,
                this.person.entityId,
                this.person.tenantId,
                this.detailsListFormItems,
            );
        }
    }

    public didSelectUpdate(value: any): void {
        const successCallback: (user: UserResourceModel, message: string) => Promise<void>
            = async (user: UserResourceModel, message: string): Promise<void> => {
                this.eventService.getEntityUpdatedSubject('User').next(user);
                // set delay so it popup doesnt show up immediately.
                setTimeout(() => {
                    this.checkAndHandleValueForPersonEmailChange(value.accountEmail);
                    if (this.authService.userId == user.id) {
                        this.authService.updateEmail(user.email);
                    }
                }, 750);
                this.returnToPrevious(message);
            };
        this.didSelectUpdateWithCallback(value, successCallback, () => this.returnToPrevious());
    }

    public returnToPrevious(message?: string): void {
        const pathSegments: Array<string> = this.routeHelper.getPathSegments();
        pathSegments.pop();
        this.navProxy.navigateBack(pathSegments);
        if (message) {
            this.sharedAlertService.showToast(message);
        }
        this.loader.dismiss();
    }
}
