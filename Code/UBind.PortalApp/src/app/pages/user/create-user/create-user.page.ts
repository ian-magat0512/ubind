import { Component, Injector, ElementRef, OnInit } from '@angular/core';
import { FormBuilder } from '@angular/forms';
import { NavProxyService } from '@app/services/nav-proxy.service';
import { UserApiService } from '@app/services/api/user-api.service';
import { AuthenticationService } from '@app/services/authentication.service';
import { PersonCategory } from '@app/models';
import { scrollbarStyle } from '@assets/scrollbar';
import { LayoutManagerService } from '@app/services/layout-manager.service';
import { EventService } from '@app/services/event.service';
import { SharedLoaderService } from '@app/services/shared-loader.service';
import { SharedAlertService } from '@app/services/shared-alert.service';
import { UserResourceModel } from '@app/resource-models/user/user.resource-model';
import { FormHelper } from '@app/helpers/form.helper';
import { AppConfigService } from '@app/services/app-config.service';
import { InvitationApiService } from '@app/services/api/invitation-api.service';
import { RouteHelper } from '@app/helpers/route.helper';
import { CreateEditPersonComponent } from '@app/components/create-edit-person/create-edit-person.component';
import { EntityType } from '@app/models/entity-type.enum';
import { AdditionalPropertyValue } from '@app/models/additional-property-item-view.model';
import { UserService } from '@app/services/user.service';
import { AdditionalPropertyDefinitionApiService } from '@app/services/api/additional-property-definition-api.service';
import { TenantService } from '@app/services/tenant.service';
import { AdditionalPropertyValueService } from '@app/services/additional-property-value.service';
import { RoleApiService } from '@app/services/api/role-api.service';
import { ActivatedRoute } from '@angular/router';
import { OrganisationApiService } from '@app/services/api/organisation-api.service';
import { PortalApiService } from '@app/services/api/portal-api.service';
import { FeatureSettingService } from '@app/services/feature-setting.service';
import { PermissionService } from '@app/services/permission.service';

/**
 * Export create user page component class.
 * This class manage creation of users.
 */
@Component({
    selector: 'app-create-user',
    templateUrl: '../../../components/create-edit-person/create-edit-person.component.html',
    styleUrls: [
        '../../../../assets/css/scrollbar-form.css',
        '../../../../assets/css/form-toolbar.scss',
    ],
    styles: [scrollbarStyle],
})
export class CreateUserPage extends CreateEditPersonComponent implements OnInit {
    public entityType: EntityType = EntityType.User;
    public isEdit: boolean = false;
    public routeParamName: string = '';
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

    public async ngOnInit(): Promise<void> {
        this.organisationId = this.routeHelper.getParam('organisationId');
        await super.ngOnInit();
    }

    /*
    protected async loadPerson(): Promise<void> {
        return this.userApiService.getById(this.routeHelper.getParam(this.routeParamName))
            .toPromise().then((data: UserResourceModel) => {
                this.configureUser(data);
            });
    }

    protected configureUser(user: UserResourceModel): void {
        this.person = new PersonViewModel(user);
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

        if (this.isEdit && this.personCategory == PersonCategory.Customer && this.person.status == UserStatus.new) {
            let accountEmailKey: string = "accountEmail";

            let deleteAccountEmailControl: boolean = true;
            Object.keys(this.personForm.controls).forEach((key: string) => {
                if (key == accountEmailKey) {
                    deleteAccountEmailControl = false;
                }
            });

            if (deleteAccountEmailControl) {
                this.personForm.removeControl(accountEmailKey);
            }

            this.detailsListFormItems
                = this.detailsListFormItems.filter((f: DetailsListFormItem) => f.Title != "accountEmail");
        }
    }
*/

    public didSelectUpdate(value: any): void {
        this.didSelectCreateWithCallback(value, (user: UserResourceModel) => {
            this.eventService.getEntityCreatedSubject('User').next(user);
            this.redirectToDetails();
        });
    }

    public returnToPrevious(): void {
        let pathSegments: Array<string> = this.routeHelper.getPathSegmentsUpUntil('user');
        if (this.organisationId) {
            pathSegments.pop();
            this.navProxy.navigateBack(pathSegments, true, { queryParams: { segment: 'Users' } });
        } else {
            this.navProxy.navigateBack(pathSegments);
        }
    }

    private redirectToDetails(): void {
        if (this.routeHelper.hasPathSegment('organisation')) {
            let pathSegments: Array<string> = this.routeHelper.getPathSegments();
            pathSegments.pop();
            pathSegments.push(this.newPersonId);
            this.navProxy.navigateForward(pathSegments);
        } else {
            this.navProxy.navigate(['user', this.newPersonId]);
        }
    }
}
