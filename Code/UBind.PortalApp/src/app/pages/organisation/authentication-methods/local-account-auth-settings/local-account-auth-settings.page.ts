import { Component, ElementRef, Injector, OnDestroy, OnInit } from "@angular/core";
import { FormBuilder, FormControl, FormGroup } from "@angular/forms";
import { RouteHelper } from "@app/helpers/route.helper";
import { DetailsListFormCheckboxItem } from "@app/models/details-list/details-list-form-checkbox-item";
import { DetailsListFormContentItem } from "@app/models/details-list/details-list-form-content-item";
import { DetailsListFormItem } from "@app/models/details-list/details-list-form-item";
import { DetailsListItemCard } from "@app/models/details-list/details-list-item-card";
import { EntityEditFieldOption } from "@app/models/entity-edit-field-option";
import { DetailPage } from "@app/pages/master-detail/detail.page";
import { OrganisationApiService } from "@app/services/api/organisation-api.service";
import { EventService } from "@app/services/event.service";
import { NavProxyService } from "@app/services/nav-proxy.service";
import { SharedAlertService } from "@app/services/shared-alert.service";
import { SharedLoaderService } from "@app/services/shared-loader.service";
import { Subject } from "rxjs";
import { finalize, takeUntil } from "rxjs/operators";
import { AuthenticationMethodType } from "@app/models/authentication-method-type.enum";
import {
    AuthenticationMethodResourceModel,
    LocalAccountAuthenticationMethodResourceModel,
    LocalAccountAuthenticationUpsertModel,
} from "@app/resource-models/authentication-method.resource-model";

/**
 * Page for editing the local account auth settings for an organisation.
 */
@Component({
    selector: 'app-local-account-auth-settings-page',
    templateUrl: './local-account-auth-settings.page.html',
    styleUrls: [
        './local-account-auth-settings.page.scss',
    ],
})
export class LocalAccountAuthSettingsPage extends DetailPage implements OnInit, OnDestroy {
    public title: string = 'Local Account Authentication Settings';
    public detailsList: Array<DetailsListFormItem>;
    public fieldOptions: Array<EntityEditFieldOption> = [];
    public form: FormGroup;
    public isLoading: boolean = false;
    public authenticationMethodId: string;
    private model: LocalAccountAuthenticationMethodResourceModel;
    private organisationId: string;

    public constructor(
        private routeHelper: RouteHelper,
        private organisationApiService: OrganisationApiService,
        private formBuilder: FormBuilder,
        private navProxy: NavProxyService,
        private sharedLoaderService: SharedLoaderService,
        private sharedAlert: SharedAlertService,
        eventService: EventService,
        elementRef: ElementRef,
        injector: Injector,
    ) {
        super(eventService, elementRef, injector);
    }

    public ngOnInit(): void {
        this.destroyed = new Subject<void>();
        this.organisationId = this.routeHelper.getParam('organisationId');
        this.prepareForm();
        this.buildForm();
        this.load();
    }

    public ngOnDestroy(): void {
        if (this.destroyed) {
            this.destroyed.next();
            this.destroyed.complete();
        }
    }

    private load(): void {
        this.isLoading = true;
        this.organisationApiService.getLocalAccountAuthenticationMethod(
            this.organisationId, this.routeHelper.getContextTenantAlias())
            .pipe(
                takeUntil(this.destroyed),
                finalize(() => this.isLoading = false),
            )
            .toPromise()
            .then((model: LocalAccountAuthenticationMethodResourceModel) => {
                this.model = model;
                let formModel: any = {
                    canCustomersSignIn: model.canCustomersSignIn,
                    canAgentsSignIn: model.canAgentsSignIn,
                    allowCustomerSelfRegistration: model.allowCustomerSelfRegistration,
                    allowAgentSelfRegistration: model.allowAgentSelfRegistration,
                };
                this.form.setValue(formModel);
            });
    }

    protected prepareForm(): void {
        const detailsCard: DetailsListItemCard = new DetailsListItemCard(
            'Local Authentication',
            'On this page you can configure the local authentication settings for your organisation.');
        this.detailsList = new Array<DetailsListFormItem>();
        this.detailsList.push(DetailsListFormCheckboxItem.create(
            detailsCard,
            "canCustomersSignIn",
            "Allow Customers to sign-in")
            .withGroupName('details'));
        this.detailsList.push(DetailsListFormCheckboxItem.create(
            detailsCard,
            "canAgentsSignIn",
            "Allow Agents to sign-in")
            .withGroupName('details'));
        this.detailsList.push(DetailsListFormCheckboxItem.create(
            detailsCard,
            'allowCustomerSelfRegistration',
            'Allow Customer Self Registration')
            .withHeader('Account Registration')
            .withParagraph('You can allow users to register themselves for an account from the portal login page.')
            .withGroupName('details')
            .withoutSectionIcons<DetailsListFormContentItem>());
        this.detailsList.push(DetailsListFormCheckboxItem.create(
            detailsCard,
            'allowAgentSelfRegistration',
            'Allow Agent Self Registration')
            .withGroupName('details'));
    }

    protected buildForm(): void {
        let controls: Array<FormControl> = [];
        this.detailsList.forEach((item: DetailsListFormItem) => {
            controls[item.Alias] = item.FormControl;
        });
        this.form = this.formBuilder.group(controls);

        this.manageVisibilityOfCustomerFields();
        this.manageVisibilityOfAgentFields();
    }

    public async close(): Promise<void> {
        this.returnToPrevious();
    }

    public async save(value: any): Promise<void> {
        if (this.form.invalid) {
            return;
        }

        let upsertModel: LocalAccountAuthenticationUpsertModel = {
            tenant: this.routeHelper.getContextTenantAlias(),
            organisation: this.organisationId,
            allowCustomerSelfRegistration: value.allowCustomerSelfRegistration,
            allowAgentSelfRegistration: value.allowAgentSelfRegistration,
            name: this.model.name,
            typeName: AuthenticationMethodType.LocalAccount,
            includeSignInButtonOnPortalLoginPage: this.model.includeSignInButtonOnPortalLoginPage,
            disabled: this.model.disabled,
            tenantId: this.model.tenantId,
            id: this.model.id,
            organisationId: this.model.organisationId,
            canCustomersSignIn: this.model.canCustomersSignIn,
            canAgentsSignIn: this.model.canAgentsSignIn,
        };

        await this.sharedLoaderService.presentWithDelay();
        this.organisationApiService.updateLocalAccountAuthenticationMethod(this.organisationId, upsertModel)
            .pipe(
                takeUntil(this.destroyed),
                finalize(() => this.sharedLoaderService.dismiss()),
            ).subscribe((result: AuthenticationMethodResourceModel) => {
                if (result) { // will be null if we navigate way whilst loading
                    this.eventService.getEntityUpdatedSubject('AuthenticationMethod').next(result);
                    this.sharedAlert.showToast(
                        `The authentication method '${result.name}' has been updated successfully.`);
                    this.returnToPrevious();
                }
            });
    }

    private returnToPrevious(): void {
        this.navProxy.navigateBackTwo(true, { queryParams: { segment: "Settings" } });
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
        this.detailsList.find((item: DetailsListFormItem) => item.Alias == 'allowCustomerSelfRegistration')
            .Visible = canCustomersSignInValue;
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
        this.detailsList.find((item: DetailsListFormItem) => item.Alias == 'allowAgentSelfRegistration')
            .Visible = canAgentsSignInValue;
    }
}
