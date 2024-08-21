import { Component, Injector, ElementRef } from '@angular/core';
import { FormBuilder } from '@angular/forms';
import { NavProxyService } from '@app/services/nav-proxy.service';
import { CustomerApiService } from '@app/services/api/customer-api.service';
import { AuthenticationService } from '@app/services/authentication.service';
import { PersonCategory } from '@app/models';
import { scrollbarStyle } from '@assets/scrollbar';
import { LayoutManagerService } from '@app/services/layout-manager.service';
import { EventService } from '@app/services/event.service';
import { SharedLoaderService } from '@app/services/shared-loader.service';
import { SharedAlertService } from '@app/services/shared-alert.service';
import { FormHelper } from '@app/helpers/form.helper';
import { AppConfigService } from '@app/services/app-config.service';
import { UserApiService } from '@app/services/api/user-api.service';
import { InvitationApiService } from '@app/services/api/invitation-api.service';
import { RouteHelper } from '@app/helpers/route.helper';
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
import { FeatureSettingService } from '@app/services/feature-setting.service';
import { PermissionService } from '@app/services/permission.service';

/**
 * Export create customer page component class
 * TODO: Write a better class header: creating of customer.
 */
@Component({
    selector: 'app-create-customer',
    templateUrl: '../../../components/create-edit-person/create-edit-person.component.html',
    styleUrls: [
        '../../../../assets/css/scrollbar-form.css',
        '../../../../assets/css/form-toolbar.scss',
    ],
    styles: [
        scrollbarStyle,
    ],
})
export class CreateCustomerPage extends CreateEditPersonComponent {
    public isEdit: boolean = false;
    public routeParamName: string = '';
    public constructor(
        protected roleApiService: RoleApiService,
        protected authenticationService: AuthenticationService,
        protected permissionService: PermissionService,
        protected eventService: EventService,
        protected formBuilder: FormBuilder,
        public navProxy: NavProxyService,
        protected appConfigService: AppConfigService,
        protected customerApiService: CustomerApiService,
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
            authenticationService,
            permissionService,
            sharedAlertService,
            eventService,
            formBuilder,
            navProxy,
            appConfigService,
            customerApiService,
            PersonCategory.Customer,
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

    public entityType: EntityType = EntityType.Customer;
    public personAdditionalPropertyValueFields: Array<AdditionalPropertyValue> = [];

    public didSelectUpdate(value: any): void {
        this.didSelectCreateWithCallback(value, (customer: any) => {
            this.eventService.getEntityCreatedSubject('Customer').next(customer);
            this.redirectToDetails();
        });
    }

    public returnToPrevious(): void {
        this.navProxy.navigateBack(['customer', 'list']);
    }

    private redirectToDetails(): void {
        this.navProxy.navigateBack(['customer', this.newPersonId]);
    }
}
