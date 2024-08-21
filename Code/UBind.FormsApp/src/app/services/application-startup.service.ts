import { Injectable } from '@angular/core';
import { ApplicationService } from './application.service';
import { ApiService } from './api.service';
import { UserService } from './user.service';
import { StringHelper } from '@app/helpers/string.helper';
import { QuoteApiService } from './api/quote-api.service';
import { ConfigurationOperation } from '@app/operations/configuration.operation';
import { ConfigProcessorService } from './config-processor.service';
import { WorkflowService } from './workflow.service';
import { Subject } from 'rxjs';
import { ApplicationLoadService } from './application-load-service';
import { Errors } from '@app/models/errors';
import { UnifiedFormModelService } from './unified-form-model.service';
import { ConfigService } from './config.service';
import { filter, take } from 'rxjs/operators';
import { FormType } from '@app/models/form-type.enum';
import { QuoteResultProcessor } from './quote-result-processor';
import { ClaimResultProcessor } from './claim-result-processor';
import { QuoteType } from '@app/models/quote-type.enum';
import { AppContextApiService } from './api/app-context-api.service';
import { FormsAppContextModel } from '../models/forms-app-context.model';
import { ClaimApiService } from './api/claim-api-service';
import { ApplicationMode } from '@app/models/application-mode.enum';
import { DeploymentEnvironment } from '@app/models/deployment-environment';
import { EnumHelper } from '@app/helpers/enum.helper';
import { ContextEntityService } from './context-entity.service';
import { LocaleService } from './locale.service';
import { FormService } from './form.service';

/**
 * The parameters the can be set on the element which loads the application.
 */
interface AppParameters {
    apiOrigin: string;
    tenant: string;
    organisation: string;
    product: string;
    productId: string;
    environment: string;
    portal: string;
    quoteId: string;
    policyId: string;
    mode: any;
    quoteType: any;
    version: string;
    isTestData: boolean;
    debug: boolean;
    debugLevel: number;
    formType: string;
    claimId: string;
    isLoadedWithinPortal: boolean;
    sidebarOffsetConfiguration: string;
    productRelease: string;
    portalOrganisation: string;
}

/**
 * Export application startup service class
 * TODO: Test how it works when someone chooses to continue their quote using the quote id from 
 * their browser storage, by clicking on a button on the startscreen
 */
@Injectable()

export class ApplicationStartupService {

    public readySubject: Subject<boolean> = new Subject<boolean>();

    public constructor(
        private applicationService: ApplicationService,
        private userService: UserService,
        private apiService: ApiService,
        private quoteApiService: QuoteApiService,
        private claimApiService: ClaimApiService,
        private configurationOperation: ConfigurationOperation,
        private configProcessorService: ConfigProcessorService,
        private workflowService: WorkflowService,
        private applicationLoadService: ApplicationLoadService,
        private unifiedFormModelService: UnifiedFormModelService,
        private configService: ConfigService,
        private appContextApiService: AppContextApiService,
        private contextEntityService: ContextEntityService,
        private localeService: LocaleService,
        private formService: FormService,
    ) {
    }

    public async initialise(urlParams: URLSearchParams): Promise<void> {
        const appParams: AppParameters = this.parseAppParameters(urlParams);
        this.apiService.setApiOrigin(appParams.apiOrigin);
        this.throwIfParameterMissing('tenant', appParams.tenant);
        this.throwIfParameterMissing('product', appParams.product);
        this.throwIfPolicyModificationRequestedWithoutPolicyId(appParams);
        this.throwIfIntentIsToModifyQuoteOrClaimNotSpecified(appParams);
        this.throwIfQuoteTypeIsMissingForPolicyTransaction(appParams);
        const appContext: FormsAppContextModel = await this.getFormsAppContext(appParams);
        this.setApplicationConfiguration(appParams, appContext);
        this.createInitialEmptyCalculationResult();

        // Load user related information from browser storage
        this.userService.retrieveLoggedInUserData();

        const shouldLoadExistingQuote: boolean = this.shouldLoadExistingQuote(appParams);
        const shouldLoadExistingClaim: boolean = appParams.claimId != null;
        await this.fetchAndProcessConfiguration(
            this.applicationService.quoteId,
            appParams.policyId,
            appParams.productRelease,
            appParams.quoteType ?? QuoteType.NewBusiness);

        // prepare tasks to run in parallel
        let concurrentTasks: Array<Promise<void>> = new Array<Promise<void>>();

        const isQuoteOrClaimAlreadyAvailable: boolean =
            (this.applicationService.formType == FormType.Quote && this.applicationService.quoteId != null) ||
            (this.applicationService.formType == FormType.Claim && this.applicationService.claimId != null);
        if (isQuoteOrClaimAlreadyAvailable) {
            // Load context entities in parallel when quote/claim is available.
            concurrentTasks.push(await this.contextEntityService.loadContextEntities());
        }

        if (shouldLoadExistingQuote) {
            await this.loadExistingQuote(appParams, concurrentTasks);
        } else if (appParams.claimId) {
            await this.loadExistingClaim(appParams, concurrentTasks);
        } else if (this.applicationService.formType == FormType.Quote) {
            appParams.quoteType = appParams.quoteType ?? QuoteType.NewBusiness;
            this.createNewQuote(appParams, concurrentTasks);
        } else if (this.applicationService.formType == FormType.Claim) {
            this.createNewClaim(appParams, concurrentTasks);
        }

        // wait for concurrent tasks to complete
        await Promise.all(concurrentTasks).then(async () => {
            if (!isQuoteOrClaimAlreadyAvailable) {
                // Load context entities after the quote/claim has been loaded or created.
                await this.contextEntityService.loadContextEntities();
            }
        });

        // set the quote type after the creation of quote or loading of existing quote
        // from applicationService quoteType that is the correct quoteType of the quote.
        if (this.applicationService.formType == FormType.Quote) {
            appParams.quoteType = this.applicationService.quoteType;
        }

        await this.contextEntityService.setIntervalLoadingContextEntities(appParams.formType, appParams.quoteType);

        // WARNING: Loading of a form.model.json file does NOT work when using the Portal
        // becaue it creates the quote and so when it comes through to the webform app we are actually
        // editing it.
        if (!shouldLoadExistingQuote && !shouldLoadExistingClaim
            && (this.applicationService.formType == FormType.Claim || appParams.quoteType == QuoteType.NewBusiness)
        ) {
            this.unifiedFormModelService.applyInitialFormModelFromConfiguration();
        }

        // wait for the config to have been processed
        this.completeInitialisationWhenConfigurationProcessed();
    }

    private shouldLoadExistingQuote(appParams: AppParameters): boolean {
        return appParams.quoteId != null
            || (appParams.policyId != null && appParams.quoteType != null && appParams.mode != ApplicationMode.Create);
    }

    private completeInitialisationWhenConfigurationProcessed(): void {
        this.configService.configurationReadySubject.pipe(
            filter((ready: boolean) => ready),
            take(1),
        ).subscribe((ready: boolean) => {
            this.workflowService.initialise();
            const currencyCode: string = this.getCurrencyCode();
            this.localeService.initialiseBrowserLocaleAndCurrency(currencyCode).then(() => {
                // notify the rest of the webform app that startup has completed 
                this.readySubject.next(true);
            });
        });
    }

    private getCurrencyCode(): string {
        let currencyCode: string = 'AUD';
        let model: object = this.formService.getValues();
        if (model['currencyCode']) {
            currencyCode = model['currencyCode'];
        } else if (this.configService.settings?.financial?.defaultCurrency) {
            currencyCode = this.configService.settings.financial.defaultCurrency;
        }
        return currencyCode;
    }

    public async fetchAndProcessConfiguration(
        quoteId: string,
        policyId: string,
        productRelease: string,
        quoteType: QuoteType): Promise<void> {
        let requestParams: any = {
            quoteId,
            policyId,
            productRelease,
            quoteType,
        };

        if (quoteId) {
            requestParams.quoteId = quoteId;
        }
        if (productRelease) {
            requestParams.productRelease = productRelease;
        }
        if (quoteType) {
            requestParams.quoteType = quoteType;
        }
        if (policyId) {
            requestParams.policyId = policyId;
        }

        return this.configurationOperation.execute(requestParams).toPromise()
            .then((response: any) => {
                if (response) {
                    this.configProcessorService.onConfigurationResponse(response);
                }
            });
    }

    private throwIfParameterMissing(name: string, value: any): void {
        if (!value) {
            throw Errors.Injection.MissingParameter(name);
        }
    }

    /**
     * If someone intends to edit an existing quote or claim 
     * (either by way of endorse, review etc or just edit)
     * and they've not specified a quote or claim ID, then throw an exception.
     */
    private throwIfIntentIsToModifyQuoteOrClaimNotSpecified(appParams: AppParameters): void {
        const formType: FormType = EnumHelper.parseOrNull<FormType>(FormType, appParams.formType) ?? FormType.Quote;
        if (formType == FormType.Quote && appParams.mode != ApplicationMode.Create && !appParams.quoteId) {
            const hasPolicyIdAndQuoteType: boolean = appParams.policyId != null && appParams.quoteType != null;
            if (!hasPolicyIdAndQuoteType) {
                throw Errors.Quote.ApplicationModeModifyWithoutExistingQuoteOrPolicyAndQuoteType(appParams.mode);
            }
        } else if (formType == FormType.Claim
            && appParams.mode != ApplicationMode.Create && !appParams.claimId
        ) {
            throw Errors.Claim.ApplicationModeModifyWithoutExistingClaim(appParams.mode);
        }
    }

    private parseAppParameters(urlParams: URLSearchParams): AppParameters {
        const isTestDataRaw: string = urlParams.get('isTestData')
            || urlParams.get('testMode');
        const debugRaw: string = urlParams.get('debug');
        const debugLevelRaw: string = urlParams.get('debugLevel');
        const debugLevelParsed: number = parseInt(debugLevelRaw, 10);
        const isLoadedWithinPortalRaw: string = urlParams.get('isLoadedWithinPortal');
        const appParams: AppParameters = {
            apiOrigin: urlParams.get('apiOrigin'),
            tenant: urlParams.get('tenant'),
            organisation: urlParams.get('organisation'),
            productId: urlParams.get('productId'),
            product: urlParams.get('product'),
            environment: urlParams.get('environment'),
            portal: urlParams.get('portal'),
            quoteId: urlParams.get('quoteId'),
            policyId: urlParams.get('policyId'),
            mode: urlParams.get('mode'),
            quoteType: urlParams.get('quoteType'),
            version: urlParams.get('version'),
            isTestData: isTestDataRaw !== null ? StringHelper.equalsIgnoreCase(isTestDataRaw, 'true') : false,
            debug: debugRaw !== null ? StringHelper.equalsIgnoreCase(debugRaw, 'true') : false,
            debugLevel: isNaN(debugLevelParsed) ? 1 : debugLevelParsed,
            formType: urlParams.get('formType'),
            claimId: urlParams.get('claimId'),
            isLoadedWithinPortal: isLoadedWithinPortalRaw !== null
                ? StringHelper.equalsIgnoreCase(isLoadedWithinPortalRaw, 'true')
                : false,
            sidebarOffsetConfiguration: urlParams.get('sidebarOffset'),
            productRelease: urlParams.get('productRelease'),
            portalOrganisation: urlParams.get('portalOrganisation'),
        };

        // nullify empty strings
        for (let propertyName in appParams) {
            if (appParams[propertyName] === "") {
                appParams[propertyName] = null;
            }
        }

        // quote type could come in as a string or number
        this.parseQuoteType(appParams);

        // intelligently set the application mode if it's empty
        if (StringHelper.isNullOrEmpty(appParams.mode)) {
            if (appParams.quoteId || appParams.claimId
                || appParams.policyId != null && appParams.quoteType != null
            ) {
                appParams.mode = ApplicationMode.Edit;
            } else {
                appParams.mode = ApplicationMode.Create;
            }
        }

        return appParams;
    }

    /**
     * Quote type could come in as a string or a number, we support both.     
     */
    private parseQuoteType(appParams: AppParameters): void {
        if (StringHelper.isNullOrEmpty(appParams.quoteType)) {
            appParams.quoteType = null;
        }
        if (appParams.quoteType != null) {
            const quoteTypeValues: Array<QuoteType> = Object.values(QuoteType);
            if (quoteTypeValues.indexOf(appParams.quoteType) == -1) {
                let numberValue: number = Number.parseInt(appParams.quoteType, 10);
                if (!isNaN(numberValue)) {
                    if (numberValue >= 0 && numberValue < quoteTypeValues.length) {
                        appParams.quoteType = quoteTypeValues[numberValue];
                    } else {
                        throw Errors.General.InvalidEnumValue(
                            'quoteType', appParams.quoteType, QuoteType, 'application startup');
                    }
                } else {
                    throw Errors.General.InvalidEnumValue(
                        'quoteType', appParams.quoteType, QuoteType, 'application startup');
                }
            }

            // currently we only support manually setting testData = true 
            // via query params for new business quotes, then all subsequent quotes on that policy will inherit it
            if (appParams.quoteType != QuoteType.NewBusiness) {
                // once we load the quote we may find it has isTestData 
                // set on it which we will the update in ApplicationService separately
                appParams.isTestData = false;
            }
        }
    }

    private setApplicationConfiguration(appParams: AppParameters, formsAppContext: FormsAppContextModel): void {
        this.applicationService.setApplicationConfiguration(
            appParams.apiOrigin,
            formsAppContext.tenantId,
            formsAppContext.tenantAlias,
            formsAppContext.organisationId,
            formsAppContext.organisationAlias,
            formsAppContext.isDefaultOrganisation,
            formsAppContext.productId,
            formsAppContext.productAlias,
            appParams.environment,
            appParams.formType,
            appParams.quoteId,
            appParams.policyId,
            appParams.claimId,
            formsAppContext.portalId,
            formsAppContext.portalAlias,
            appParams.mode,
            appParams.quoteType,
            appParams.version,
            appParams.isTestData,
            appParams.debug,
            appParams.debugLevel,
            appParams.isLoadedWithinPortal,
            appParams.sidebarOffsetConfiguration,
            appParams.productRelease,
            appParams.portalOrganisation);
    }

    private async loadExistingQuote(appParams: AppParameters, concurrentTasks: Array<Promise<void>>): Promise<void> {
        if (!appParams.policyId && appParams.quoteId) {
            concurrentTasks.push(this.applicationLoadService.loadQuote(
                appParams.quoteId,
                appParams.version,
                appParams.quoteType));
        } else if (appParams.policyId && appParams.quoteType) {
            concurrentTasks.push(this.applicationLoadService.loadQuoteForPolicy(
                appParams.policyId,
                appParams.quoteType,
                appParams.version,
                appParams.productRelease));
        }
    }

    private async loadExistingClaim(appParams: AppParameters, concurrentTasks: Array<Promise<void>>): Promise<void> {
        concurrentTasks.push(this.applicationLoadService.loadClaim(appParams.claimId, appParams.version));
    }

    private createNewQuote(appParams: AppParameters, concurrentTasks: Array<Promise<void>>): void {
        if (appParams.quoteType == QuoteType.NewBusiness) {
            concurrentTasks.push(this.quoteApiService.createNewBusinessQuote(
                appParams.tenant,
                this.applicationService.organisationAlias,
                this.applicationService.portalId,
                appParams.product,
                <DeploymentEnvironment>appParams.environment,
                appParams.isTestData,
                appParams.productRelease));
        } else {
            switch (appParams.quoteType) {
                case QuoteType.Renewal:
                    concurrentTasks.push(this.quoteApiService.createRenewalQuote(
                        appParams.tenant, appParams.policyId, appParams.productRelease));
                    break;
                case QuoteType.Adjustment:
                    concurrentTasks.push(this.quoteApiService.createAdjustmentQuote(
                        appParams.tenant, appParams.policyId, appParams.productRelease));
                    break;
                case QuoteType.Cancellation:
                    concurrentTasks.push(this.quoteApiService.createCancellationQuote(
                        appParams.tenant, appParams.policyId, appParams.productRelease));
                    break;
                default:
                    throw Errors.General.InvalidEnumValue(
                        'quoteType', appParams.quoteType, QuoteType, 'Application Startup');
            }
        }
    }

    private createNewClaim(appParams: AppParameters, concurrentTasks: Array<Promise<void>>): void {
        concurrentTasks.push(this.claimApiService.createNewClaim(
            appParams.tenant,
            this.applicationService.organisationAlias,
            appParams.product,
            appParams.environment,
            appParams.isTestData));
    }

    private createInitialEmptyCalculationResult(): void {
        if (this.applicationService.formType == FormType.Quote) {
            this.applicationService.latestQuoteResult = QuoteResultProcessor.createEmpty();
        } else if (this.applicationService.formType == FormType.Claim) {
            this.applicationService.latestClaimResult = ClaimResultProcessor.createEmpty();
        }
    }

    private getFormsAppContext(appParams: AppParameters): Promise<FormsAppContextModel> {
        return this.appContextApiService.getFormsAppContext(
            appParams.tenant,
            appParams.product,
            appParams.organisation,
            appParams.portal,
            appParams.quoteId,
        ).toPromise();
    }

    private throwIfQuoteTypeIsMissingForPolicyTransaction(appParams: AppParameters): void {
        const formType: FormType = <FormType>appParams.formType ?? FormType.Quote;
        if (formType == FormType.Quote) {
            if (appParams.policyId && !appParams.quoteType) {
                throw Errors.Quote.PolicyModificationRequestedWithoutQuoteType(appParams.policyId);
            }
            if (appParams.policyId && appParams.quoteType == QuoteType.NewBusiness) {
                throw Errors.Quote.PolicyModificationRequestedInvalidQuoteType(appParams.policyId, "New Business");
            }
        }
    }

    private throwIfPolicyModificationRequestedWithoutPolicyId(appParams: AppParameters): void {
        const formType: FormType = <FormType>appParams.formType ?? FormType.Quote;
        if (formType == FormType.Quote && !appParams.quoteId && !appParams.policyId
            && appParams.quoteType != null && appParams.quoteType != QuoteType.NewBusiness
        ) {
            throw Errors.Quote.PolicyModificationRequestedWithoutPolicyId(appParams.quoteType);
        }
    }
}
