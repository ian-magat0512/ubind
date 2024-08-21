import { Component, Injector, ElementRef } from '@angular/core';
import { FormBuilder } from '@angular/forms';
import { AlertController } from '@ionic/angular';
import { NavProxyService } from '@app/services/nav-proxy.service';
import { AuthenticationService } from '@app/services/authentication.service';
import { PersonCategory, PersonResourceModel } from '@app/models';
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
import { PersonApiService } from '@app/services/api/person-api.service';
import { SubscriptionLike } from 'rxjs';
import { UserResourceModel } from '@app/resource-models/user/user.resource-model';
import { ProblemDetails } from '@app/models/problem-details';
import { RoleApiService } from '@app/services/api/role-api.service';
import { ActivatedRoute } from '@angular/router';
import { PortalApiService } from '@app/services/api/portal-api.service';
import { FeatureSettingService } from '@app/services/feature-setting.service';
import { UserService } from '@app/services/user.service';
import { AdditionalPropertyDefinitionApiService } from '@app/services/api/additional-property-definition-api.service';
import { TenantService } from '@app/services/tenant.service';
import { AdditionalPropertyValueService } from '@app/services/additional-property-value.service';
import { OrganisationApiService } from '@app/services/api/organisation-api.service';
import { AdditionalPropertyValue } from '@app/models/additional-property-item-view.model';
import { EntityType } from '@app/models/entity-type.enum';
import { UserTypePathHelper } from '@app/helpers/user-type-path.helper';
import { PermissionService } from '@app/services/permission.service';

/**
 * Export create/edit person page class.
 * This class is for editing and creating of person.
 */
@Component({
    selector: 'app-create-customer',
    templateUrl: '../../../components/create-edit-person/create-edit-person.component.html',
    styleUrls: [
        '../../../../assets/css/scrollbar-form.css',
        '../../../../assets/css/form-toolbar.scss',
    ],
    styles: [scrollbarStyle],
})
export class CreateEditPersonPage extends CreateEditPersonComponent {
    public personAdditionalPropertyValueFields: Array<AdditionalPropertyValue>;
    public entityType: EntityType = EntityType.Person;
    public isEdit: boolean = this.routeHelper.isUrlEditPage();
    public routeParamName: string = 'personId';

    protected subscriptions: Array<SubscriptionLike> = new Array<SubscriptionLike>();

    public constructor(
        protected authenticationService: AuthenticationService,
        protected roleApiService: RoleApiService,
        protected alertCtrl: AlertController,
        protected eventService: EventService,
        protected formBuilder: FormBuilder,
        public navProxy: NavProxyService,
        protected appConfigService: AppConfigService,
        protected personApiService: PersonApiService,
        protected userApiService: UserApiService,
        protected sharedAlertService: SharedAlertService,
        protected loader: SharedLoaderService,
        protected invitationService: InvitationApiService,
        public layoutManager: LayoutManagerService,
        protected route: ActivatedRoute,
        protected portalApiService: PortalApiService,
        featureSettingService: FeatureSettingService,
        routeHelper: RouteHelper,
        formHelper: FormHelper,
        elementRef: ElementRef,
        injector: Injector,
        public userService: UserService,
        protected additionalPropertyDefinitionService: AdditionalPropertyDefinitionApiService,
        protected tenantService: TenantService,
        protected additionalPropertyValueService: AdditionalPropertyValueService,
        protected organisationApiService: OrganisationApiService,
        protected userPath: UserTypePathHelper,
        protected permissionService: PermissionService,
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
            personApiService,
            PersonCategory.Person,
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

    public didSelectUpdate(value: any): void {
        if (this.routeHelper.isUrlCreatePage()) {
            this.didSelectCreateWithCallback(value, (person: any) => {
                this.eventService.getEntityCreatedSubject('Person').next(person);
                this.redirectToDetails();
            });
        } else if (this.routeHelper.isUrlEditPage()) {
            const successCallback: (person: any, message?: string) => void
                 = (person: PersonResourceModel, message: string): void => {
                     this.eventService.getEntityUpdatedSubject('Person').next(person);
                     this.subscriptions.push(
                         this.userApiService.getUserByPersonId(person.id).subscribe(
                             async (userResourceModel: UserResourceModel): Promise<void> => {
                                 await this.checkAndHandleValueForPersonEmailChange(userResourceModel.email);
                                 this.returnToPrevious();
                             },
                             (err: any) => {
                                 if (err.headers.get('Content-Type').indexOf('application/problem+json') != -1) {
                                     let appError: ProblemDetails = ProblemDetails.fromJson(err.error);
                                     if (appError.HttpStatusCode == 404) {
                                         // the user account doesn't exist, so just ignore this.
                                         this.redirectToDetails(message);
                                     } else {
                                         throw err;
                                     }
                                 } else {
                                     throw err;
                                 }
                             }, () => {
                                 this.redirectToDetails(message);
                             }));
                 };
            this.didSelectUpdateWithCallback(value, successCallback, () => this.returnToPrevious());
        }
    }

    public returnToPrevious(): void {
        if (this.routeHelper.isUrlEditPage()) {
            this.redirectToDetails();
        } else {
            let customerId: string = this.routeHelper.getParam("customerId");
            if (this.layoutManager.splitPaneVisible) {
                let personId: string = this.routeHelper.PreviousActivatedRouteSnapShot?.paramMap.get("personId")
                    ?? this.routeHelper.getParam("personId");
                if (personId) {
                    this.navProxy.navigateBack([this.userPath.customer, customerId, this.userPath.person, personId]);
                } else {
                    this.navProxy.navigate(['customer', customerId], { queryParams: { segment: "People" } });
                }
            } else {
                this.navProxy.navigate(['customer', customerId], { queryParams: { segment: "People" } });
            }
        }
        this.loader.dismiss();
    }

    private redirectToDetails(message?: string): void {
        if (!this.newPersonId) {
            this.newPersonId = this.routeHelper.getParam("personId");
        }

        let customerId: string = this.routeHelper.getParam("customerId");
        this.navProxy.navigateBack([this.userPath.customer, customerId, this.userPath.person, this.newPersonId]);
        if (message) {
            this.sharedAlertService.showToast(message);
        }
        this.loader.dismiss();
    }
}
