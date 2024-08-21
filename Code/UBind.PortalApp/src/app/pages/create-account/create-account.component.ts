import { AfterViewChecked, Component, ElementRef, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Title } from '@angular/platform-browser';
import { FormValidatorHelper } from '@app/helpers/form-validator.helper';
import { PersonCategory, PersonCreateModel } from '@app/models';
import { EmailAddressFieldResourceModel } from '@app/resource-models/person/email-address-field.resource-model';
import { CustomerApiService } from '@app/services/api/customer-api.service';
import { PortalApiService } from '@app/services/api/portal-api.service';
import { AppConfigService } from '@app/services/app-config.service';
import { LayoutManagerService } from '@app/services/layout-manager.service';
import { NavProxyService } from '@app/services/nav-proxy.service';
import { SharedAlertService } from '@app/services/shared-alert.service';
import { SharedLoaderService } from '@app/services/shared-loader.service';
import { EntryPage } from '../login/entry.page';

/**
 * Create account component class.
 */
@Component({
    selector: 'app-create-account',
    templateUrl: './create-account.component.html',
    styleUrls: ['./create-account.component.scss'],
})

export class CreateAccountComponent extends EntryPage implements OnInit, AfterViewChecked, OnDestroy {

    @ViewChild('focusElement', { read: ElementRef, static: true }) public focusElement: any;

    public createAccountForm: FormGroup;
    public errorDisplay: string = '';
    public formHasError: boolean = false;
    public hasSubmitButtonBeenClicked: boolean = false;

    public constructor(
        public appConfigService: AppConfigService,
        public portalApiService: PortalApiService,
        public titleService: Title,
        public layoutManager: LayoutManagerService,
        public formBuilder: FormBuilder,
        private navProxy: NavProxyService,
        private customerService: CustomerApiService,
        private sharedLoaderService: SharedLoaderService,
        private sharedAlertService: SharedAlertService,
    ) {
        super(appConfigService, portalApiService, titleService);
    }

    public ngOnInit(): void {
        super.ngOnInit();
        this.createAccountForm = this.formBuilder.group({
            name: ['', FormValidatorHelper.nameValidator(true)],
            email: ['', [Validators.required, Validators.email]],
        });
    }

    public ngAfterViewChecked(): void {
        if (this.focusElement && this.createAccountForm.controls["name"].untouched) {
            let inputFocusElement: HTMLIonInputElement = (<HTMLIonInputElement> this.focusElement.nativeElement);
            if (!inputFocusElement.classList.toString().includes("has-focus")) {
                inputFocusElement.setFocus();
            }
        }
    }

    public ngOnDestroy(): void {
        super.ngOnDestroy();
    }

    public async userDidTapSendActivationLink(): Promise<void> {
        this.hasSubmitButtonBeenClicked = true;
        this.createAccountForm.markAsTouched();
        this.createAccountForm.markAsDirty();
        if (this.createAccountForm.invalid) {
            this.formHasError = true;
            return;
        }

        const emailAddress: EmailAddressFieldResourceModel = {
            emailAddress: this.createAccountForm.value.email,
            label: "personal",
            customLabel: "",
            sequenceNo: 1,
            default: true,
            fieldId: "00000000-0000-0000-0000-000000000000",
        };

        const createCustomerModel: PersonCreateModel = {
            tenant: this.portalTenantId,
            organisation: this.portalOrganisationId,
            portal: this.portalId,
            preferredName: null,
            fullName: this.createAccountForm.value.name,
            displayName: null,
            namePrefix: null,
            firstName: null,
            middleNames: null,
            lastName: null,
            nameSuffix: null,
            company: null,
            title: null,
            email: this.createAccountForm.value.email,
            userType: PersonCategory.Customer,
            blocked: false,
            tenantId: this.portalTenantId,
            organisationId: this.portalOrganisationId,
            emailAddresses: [emailAddress],
            phoneNumbers: [],
            websiteAddresses: [],
            streetAddresses: [],
            messengerIds: [],
            socialMediaIds: [],
            environment: this.environment,
            portalId: this.portalId,
            customerId: null,
            hasActivePolicies: false,
        };

        try {
            await this.sharedLoaderService.presentWithDelay("Creating new user account...");
            await this.customerService.createCustomerAccount(createCustomerModel)
                .toPromise();
            this.sharedLoaderService.dismiss();
            const alert: HTMLIonAlertElement = await this.sharedAlertService.showWithOk(
                "Activation Link Sent",
                "An email has been sent to you with a link to activate your new account.",
            );
            await alert.onDidDismiss();
            this.goBack();
        } catch (error) {
            this.sharedLoaderService.dismiss();
            throw error;
        }

        this.hasSubmitButtonBeenClicked = false;
    }

    public goBack(): void {
        this.navProxy.navigate(['login']);
    }
}
