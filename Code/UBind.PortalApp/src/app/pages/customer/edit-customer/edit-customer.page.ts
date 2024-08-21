import { Component, Injector, ElementRef } from '@angular/core';
import { AlertController } from '@ionic/angular';
import { FormBuilder } from '@angular/forms';
import { PersonViewModel } from '@app/viewmodels';
import { AuthenticationService } from '@app/services/authentication.service';
import { CustomerApiService } from '@app/services/api/customer-api.service';
import { InvitationApiService } from '@app/services/api/invitation-api.service';
import { NavProxyService } from '@app/services/nav-proxy.service';
import { AppConfigService } from '@app/services/app-config.service';
import { PersonCategory, UserStatus } from '@app/models';
import { scrollbarStyle } from '@assets/scrollbar';
import { SharedAlertService } from '@app/services/shared-alert.service';
import { SharedLoaderService } from '@app/services/shared-loader.service';
import { LayoutManagerService } from '@app/services/layout-manager.service';
import { RouteHelper } from '@app/helpers/route.helper';
import { EventService } from '@app/services/event.service';
import { CustomerDetailsResourceModel, CustomerResourceModel } from '@app/resource-models/customer.resource-model';
import { UserApiService } from '@app/services/api/user-api.service';
import { UserResourceModel } from '@app/resource-models/user/user.resource-model';
import { ProblemDetails } from '@app/models/problem-details';
import { SubscriptionLike } from 'rxjs';
import { FormHelper } from '@app/helpers/form.helper';
import { CreateEditPersonComponent } from '@app/components/create-edit-person/create-edit-person.component';
import { HttpErrorResponse } from '@angular/common/http';
import { RoleApiService } from '@app/services/api/role-api.service';
import { ActivatedRoute } from '@angular/router';
import { UserService } from '@app/services/user.service';
import { AdditionalPropertyDefinitionApiService } from '@app/services/api/additional-property-definition-api.service';
import { TenantService } from '@app/services/tenant.service';
import { AdditionalPropertyValueService } from '@app/services/additional-property-value.service';
import { EntityType } from '@app/models/entity-type.enum';
import { AdditionalPropertyValue } from '@app/models/additional-property-item-view.model';
import { OrganisationApiService } from '@app/services/api/organisation-api.service';
import { PortalApiService } from '@app/services/api/portal-api.service';
import { FeatureSettingService } from '@app/services/feature-setting.service';
import { AdditionalPropertiesHelper } from '@app/helpers/additional-properties.helper';
import { PermissionService } from '@app/services/permission.service';

/**
 * Export edit customer page component class
 * This class manage editing of customer.
 */
@Component({
    selector: 'app-edit-customer',
    templateUrl: '../../../components/create-edit-person/create-edit-person.component.html',
    styleUrls: [
        '../../../../assets/css/scrollbar-form.css',
        '../../../../assets/css/form-toolbar.scss',
    ],
    styles: [scrollbarStyle],
})
export class EditCustomerPage extends CreateEditPersonComponent {

    public entityType: EntityType = EntityType.Customer;
    public isEdit: boolean = true;
    public routeParamName: string = 'customerId';
    public personAdditionalPropertyValueFields: Array<AdditionalPropertyValue> = [];
    protected subscriptions: Array<SubscriptionLike> = new Array<SubscriptionLike>();

    public constructor(
        protected roleApiService: RoleApiService,
        protected authenticationService: AuthenticationService,
        protected permissionService: PermissionService,
        protected alertCtrl: AlertController,
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
        public additionalPropertyDefinitionService: AdditionalPropertyDefinitionApiService,
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

    protected async loadEntity(): Promise<void> {
        try {
            const customerDetail: CustomerDetailsResourceModel = await this.customerApiService
                .getCustomerDetails(this.routeHelper.getParam(this.routeParamName))
                .toPromise();
            this.configureCustomer(customerDetail);
        } catch (error) {
            this.errorMessage = "There was a problem loading the customer's details";
            throw error;
        } finally {
            this.isLoading = false;
        }
    }

    protected configureCustomer(customer: CustomerDetailsResourceModel): void {
        this.person = PersonViewModel.createPersonFromCustomer(customer);

        let hasUserAccount: boolean = this.person.status ? this.person.status != UserStatus.New : false;
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
            portal: customer.portalId,
        });

        if (this.canEditAdditionalPropertyValues
            && this.person.additionalPropertyValues?.length > 0) {
            let additionalPropertyValues: any = [];
            this.person.additionalPropertyValues.forEach((app: AdditionalPropertyValue) => {
                let id: string = AdditionalPropertiesHelper.generateControlId(
                    app.additionalPropertyDefinitionModel.id,
                );
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
        const successCallback: (person: any, message?: string) => void
            = (customer: CustomerResourceModel, message: string): void => {
                this.eventService.getEntityUpdatedSubject('Customer').next(customer);
                if (customer.status != UserStatus.New) {
                    this.subscriptions.push(
                        this.userApiService.getUserByPersonId(customer.primaryPersonId).subscribe(
                            async (userResourceModel: UserResourceModel): Promise<void> => {
                                await this.checkAndHandleValueForPersonEmailChange(userResourceModel.email);
                                this.returnToPrevious();
                            },
                            (err: HttpErrorResponse) => {
                                if (err.headers.get('Content-Type').indexOf('application/problem+json') != -1) {
                                    let appError: ProblemDetails = ProblemDetails.fromJson(err.error);
                                    if (appError.HttpStatusCode == 404) {
                                    // the user account doesn't exist, so just ignore this.
                                        this.returnToPrevious(message);
                                    } else {
                                        throw err;
                                    }
                                } else {
                                    throw err;
                                }
                            }, () => {
                                this.returnToPrevious(message);
                            }));
                } else {
                    this.returnToPrevious(message);
                }
            };
        this.didSelectUpdateWithCallback(value, successCallback, () => this.returnToPrevious());
    }

    public returnToPrevious(message?: string): void {
        this.navProxy.navigateBack(['customer', this.routeHelper.getParam('customerId')]);
        if (message) {
            this.sharedAlertService.showToast(message);
        }
        this.loader.dismiss();
    }
}
