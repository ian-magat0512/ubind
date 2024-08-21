import { Component, Injector, ElementRef, OnInit } from '@angular/core';
import { FormBuilder } from '@angular/forms';
import { SharedLoaderService } from '@app/services/shared-loader.service';
import { SharedAlertService } from '@app/services/shared-alert.service';
import { AuthenticationService } from '@app/services/authentication.service';
import { InvitationApiService } from '@app/services/api/invitation-api.service';
import { NavProxyService } from '@app/services/nav-proxy.service';
import { AppConfigService } from '@app/services/app-config.service';
import { scrollbarStyle } from '@assets/scrollbar';
import { LayoutManagerService } from '@app/services/layout-manager.service';
import { UserResourceModel } from '@app/resource-models/user/user.resource-model';
import { RouteHelper } from '@app/helpers/route.helper';
import { EventService } from '@app/services/event.service';
import { FormHelper } from '@app/helpers/form.helper';
import { UserApiService } from '@app/services/api/user-api.service';
import { RoleApiService } from '@app/services/api/role-api.service';
import { ActivatedRoute } from '@angular/router';
import { AdditionalPropertyValueService } from '@app/services/additional-property-value.service';
import { AdditionalPropertyValue } from '@app/models/additional-property-item-view.model';
import { EntityType } from '@app/models/entity-type.enum';
import { UserService } from '@app/services/user.service';
import { AdditionalPropertyDefinitionApiService } from '@app/services/api/additional-property-definition-api.service';
import { TenantService } from '@app/services/tenant.service';
import { OrganisationApiService } from '@app/services/api/organisation-api.service';
import { PortalApiService } from '@app/services/api/portal-api.service';
import { FeatureSettingService } from '@app/services/feature-setting.service';
import { PermissionService } from '@app/services/permission.service';
import { EditUserPage } from '../edit-user/edit-user.page';

/**
 * Edit user page component class for organisations.
 */
@Component({
    selector: 'app-edit-organisation-user',
    templateUrl: '../../../components/create-edit-person/create-edit-person.component.html',
    styleUrls: [
        '../../../../assets/css/scrollbar-form.css',
        '../../../../assets/css/form-toolbar.scss',
    ],
    styles: [scrollbarStyle],
})
export class EditOrganisationUserPage extends EditUserPage implements OnInit {
    public personAdditionalPropertyValueFields: Array<AdditionalPropertyValue>;
    public entityType: EntityType = EntityType.Organisation;
    public isEdit: boolean = true;
    public routeParamName: string = 'userId';

    public constructor(
        protected userApiService: UserApiService,
        protected roleService: RoleApiService,
        protected authService: AuthenticationService,
        protected permissionService: PermissionService,
        protected eventService: EventService,
        protected formBuilder: FormBuilder,
        public navProxy: NavProxyService,
        protected appConfigService: AppConfigService,
        protected sharedAlertService: SharedAlertService,
        protected loader: SharedLoaderService,
        protected invitationService: InvitationApiService,
        public layoutManager: LayoutManagerService,
        route: ActivatedRoute,
        routeHelper: RouteHelper,
        formHelper: FormHelper,
        elementRef: ElementRef,
        injector: Injector,
        protected additionalPropertyValueService: AdditionalPropertyValueService,
        public userService: UserService,
        protected additionalPropertyDefinitionService: AdditionalPropertyDefinitionApiService,
        protected tenantService: TenantService,
        protected organisationApiService: OrganisationApiService,
        portalApiService: PortalApiService,
        featureSettingService: FeatureSettingService,
    ) {
        super(
            roleService,
            authService,
            permissionService,
            eventService,
            formBuilder,
            navProxy,
            appConfigService,
            userApiService,
            sharedAlertService,
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

    public async ngOnInit(): Promise<void> {
        this.ifTheUrlContainsOrganisationAndUserThenChangeTheEntityTypeToUser();
        super.ngOnInit();
    }

    private ifTheUrlContainsOrganisationAndUserThenChangeTheEntityTypeToUser(): void {
        let segments: Array<string> = this.routeHelper.getPathSegments().filter(
            (segment: string) => segment === "organisation" || segment === "user",
        );
        if (segments && segments.length == 2) {
            this.entityType = EntityType.User;
        }
    }

    public didSelectUpdate(value: any): void {
        const successCallback: any = (user: UserResourceModel): any => {
            this.eventService.getEntityUpdatedSubject('User').next(user);
            this.person.entityId = user.id;
            this.person.id = user.personId;
            this.checkAndHandleValueForPersonEmailChange(value.accountEmail);
        };
        this.didSelectUpdateWithCallback(value, successCallback, () => this.returnToPrevious());
    }

    public returnToPrevious(): void {
        const organisationId: string = this.routeHelper.getParam('organisationId');
        if (organisationId) {
            this.navProxy.navigateBack(['organisation', organisationId, 'user', this.routeHelper.getParam('userId')]);
        }
    }
}
