import { Injectable } from '@angular/core';
import { BehaviorSubject, Subject } from 'rxjs';
import { QuoteType } from '../models/quote-type.enum';
import { FormType } from '../models/form-type.enum';
import { QuoteState } from '@app/models/quote-state.enum';
import { ClaimState } from '@app/models/claim-state.enum';
import { ClaimResult } from '@app/models/claim-result';
import { QuoteResult } from '@app/models/quote-result';
import { ApplicationMode } from '@app/models/application-mode.enum';
import { StringHelper } from '@app/helpers/string.helper';
import { PersonDetailModel } from '@app/models/person-detail.model';
import { DeploymentEnvironment } from '@app/models/deployment-environment';
import { WorkflowDestination } from '@app/models/workflow-destination';
import { NavigationDirection } from '@app/models/navigation-direction.enum';
import { SectionWidgetStatus } from '@app/components/widgets/section/section-widget-status';
import { ApplicationStatus } from '@app/models/application-status.enum';
import { UuidHelper } from '@app/helpers/uuid.helper';
import { WebFormEmbedOptions } from '@app/models/web-form-embed-options';

/**
 * Export application service class.
 * TODO: Write a better class header: application service functions.
 */
@Injectable()
export class ApplicationService {
    private _tenantId: string = '';
    private _tenantAlias: string = '';
    private _organisationId: string = '';
    private _organisationAlias: string = '';
    private _isDefaultOrganisation: boolean = false;
    private _productId: string = '';
    private _productAlias: string = '';
    private _environment: DeploymentEnvironment;
    private _quoteId: string;
    private _policyId: string;
    private _claimId: string;
    private _portalId: string;
    private _portalAlias: string;
    private _isTestData: boolean = false;
    private _debug: boolean = false;
    private _debugLevel: number = 0;
    private _calculationResultId: string = '';
    private _premiumFundingProposalId: string = '';
    private _principalFinanceAcceptanceUrl: string = '';
    private _policyNumber: string = '';
    private _invoiceNumber: string = '';
    private _quoteType: QuoteType = QuoteType.NewBusiness; // default value if not changed  on load is New Business
    private _currentWorkflowDestination: WorkflowDestination = { stepName: '' };
    private _previousWorkflowDestination: WorkflowDestination;
    private _mode: ApplicationMode = ApplicationMode.Create;
    private _quoteState: QuoteState = QuoteState.Nascent;
    private _claimState: ClaimState = ClaimState.Nascent;
    private _version: string = '';
    private _applicationState: ApplicationStatus = ApplicationStatus.Nascent;
    private _customerId: string = '';
    private _userHasAccount: boolean = false;
    private _quoteReference: string = '';
    private _formType: string = '';
    private _previousQuoteState: string = '';
    private _currentUser: any = {};
    private _isLoadedWithinPortal: boolean = false;
    public hadCustomerOnCreation: boolean = false;
    private _sidebarOffsetConfiguration: string = '';
    private _embedOptions: WebFormEmbedOptions;
    private _productReleaseId: string;
    private _productReleaseNumber: string = '';
    private _portalOrganisationAlias: string = '';

    public debugSubject: BehaviorSubject<boolean> = new BehaviorSubject<boolean>(false);
    public debugLevelSubject: BehaviorSubject<number> = new BehaviorSubject<number>(0);
    public workflowStepSubject: Subject<string> = new Subject<string>();
    public articleIndexSubject: Subject<number> = new Subject<number>();
    public articleElementIndexSubject: Subject<number> = new Subject<number>();
    public quoteStateSubject: Subject<string> = new Subject<string>();
    public claimStateSubject: Subject<string> = new Subject<string>();
    public applicationPropertyChangedSubject: Subject<{
        name: string;
        value: any;
    }> = new Subject<{ name: string; value: any }>();
    public operationStatuses: Map<string, string> = new Map<string, string>();
    public operationResultSubject: Subject<any> = new Subject<any>();
    public calculationResultSubject: Subject<any> = new Subject<any>();
    public calculationInProgressSubject: Subject<any> = new Subject<any>();
    public backgroundCalculationInProgressSubject: Subject<any> = new Subject<any>();
    public configurationAvailableSubject: BehaviorSubject<boolean> = new BehaviorSubject<boolean>(false);
    public latestQuoteResult: QuoteResult;
    public latestClaimResult: ClaimResult;
    public calculationInProgress: boolean;
    public apiOrigin: string = location.origin;
    public lastNavigationDirection: NavigationDirection;
    public sectionWidgetStatusSubject: Subject<SectionWidgetStatus> = new Subject<SectionWidgetStatus>();
    public operationInProgressSubject: Subject<string> = new Subject<string>();
    // this will tell if a background calculation is in progress.
    public backgroundCalculationInProgress: boolean = false;

    public constructor() {
    }

    public setApplicationConfiguration(
        apiOrigin: string,
        tenantId: string,
        tenantAlias: string,
        organisationId: string,
        organisationAlias: string,
        isDefaultOrganisation: boolean,
        productId: string,
        productAlias: string,
        environment: string,
        formType: string,
        quoteId: string,
        policyId: string,
        claimId: string,
        portalId: string,
        portalAlias: string,
        mode: ApplicationMode,
        quoteType: QuoteType,
        version: string,
        isTestData: boolean,
        debug: boolean = false,
        debugLevel: number = 0,
        isLoadedWithinPortal: boolean = false,
        sidebarOffsetConfiguration: string = '',
        productRelease: string = '',
        portalOrganisationAlias: string = '',
    ): void {
        this.apiOrigin = apiOrigin ?? location.origin;
        this._tenantId = tenantId;
        this._tenantAlias = tenantAlias;
        this._organisationAlias = organisationAlias;
        this._organisationId = organisationId;
        this._isDefaultOrganisation = isDefaultOrganisation;
        this._productId = productId;
        this._productAlias = productAlias;
        this._environment = <DeploymentEnvironment>environment.toLowerCase();
        this._formType = formType && formType != 'null' ? formType : FormType.Quote;
        this._quoteId = UuidHelper.isUuid(quoteId) ? quoteId : null;
        this._policyId = UuidHelper.isUuid(policyId) ? policyId : null;
        this._claimId = UuidHelper.isUuid(claimId) ? claimId : null;
        this._portalId = UuidHelper.isUuid(portalId) ? portalId : null;
        this._portalAlias = portalAlias;
        if (StringHelper.isNullOrEmpty(mode)) {
            if (this.quoteId || this.claimId) {
                this._mode = ApplicationMode.Edit;
            } else {
                this._mode = ApplicationMode.Create;
            }
        } else {
            this._mode = mode;
        }
        this._quoteType = quoteType ?? QuoteType.NewBusiness;
        this._version = version;
        this._isTestData = isTestData;
        this.debug = debug;
        this.debugLevel = debugLevel;
        this._isLoadedWithinPortal = isLoadedWithinPortal;
        this._productReleaseNumber = UuidHelper.isUuid(productRelease) ? null : productRelease;
        this._productReleaseId = UuidHelper.isUuid(productRelease) ? productRelease : null;
        this._portalOrganisationAlias = portalOrganisationAlias || organisationAlias;
        this.configurationAvailableSubject.next(true);
        this._sidebarOffsetConfiguration = sidebarOffsetConfiguration;
    }

    public get currentUser(): PersonDetailModel {
        return this._currentUser;
    }

    public set currentUser(currentUser: PersonDetailModel) {
        this._currentUser = currentUser;
        this.applicationPropertyChangedSubject.next({ name: 'currentUser', value: currentUser });
    }

    public get tenantId(): string {
        return this._tenantId;
    }

    public get tenantAlias(): string {
        return this._tenantAlias;
    }

    public get organisationAlias(): string {
        return this._organisationAlias;
    }

    public get organisationId(): string {
        return this._organisationId;
    }

    public get isDefaultOrganisation(): boolean {
        return this._isDefaultOrganisation;
    }

    public get productId(): string {
        return this._productId;
    }

    public get productAlias(): string {
        return this._productAlias;
    }

    public get environment(): DeploymentEnvironment {
        return this._environment;
    }

    public get formType(): string {
        return this._formType;
    }

    public get policyId(): string {
        return this._policyId;
    }

    public set policyId(policyId: string) {
        this._policyId = policyId;
        this.applicationPropertyChangedSubject.next({ name: 'policyId', value: policyId });
    }

    public get claimId(): string {
        return this._claimId;
    }

    public set claimId(claimId: string) {
        this._claimId = claimId;
        this.applicationPropertyChangedSubject.next({ name: 'claimId', value: claimId });
    }

    public get quoteId(): string {
        return this._quoteId;
    }

    public set quoteId(quoteId: string) {
        this._quoteId = quoteId;
        this.applicationPropertyChangedSubject.next({ name: 'quoteId', value: quoteId });
    }

    public get portalId(): string {
        return this._portalId;
    }

    public set portalId(portalId: string) {
        this._portalId = portalId;
        this.applicationPropertyChangedSubject.next({ name: 'portalId', value: portalId });
    }

    public get portalAlias(): string {
        return this._portalAlias;
    }

    public set portalAlias(portalAlias: string) {
        this._portalAlias = portalAlias;
        this.applicationPropertyChangedSubject.next({ name: 'portalAlias', value: portalAlias });
    }

    /**
     * The "previousQuoteState" is the state of the quote at the time it was loaded.
     */
    public get previousQuoteState(): string {
        return this._previousQuoteState;
    }

    /**
     * The "previousQuoteState" is the state of the quote at the time it was loaded.
     */
    public set previousQuoteState(quoteState: string) {
        this._previousQuoteState = quoteState;
    }

    public get isTestData(): boolean {
        return this._isTestData;
    }

    public set isTestData(isTestData: boolean) {
        this._isTestData = isTestData;
        this.applicationPropertyChangedSubject.next({ name: 'isTestData', value: isTestData });
    }

    public get debug(): boolean {
        return this._debug;
    }

    public set debug(debug: boolean) {
        this._debug = debug;
        if (debug && this._debugLevel < 1) {
            this.debugLevel = 1;
        }
        this.debugSubject.next(debug);
    }

    public set debugLevel(debugLevel: number) {
        this._debugLevel = debugLevel;
        this.debugLevelSubject.next(debugLevel);
    }

    public get debugLevel(): number {
        return this._debugLevel;
    }

    public get isLoadedWithinPortal(): boolean {
        return this._isLoadedWithinPortal;
    }

    public get calculationResultId(): string {
        return this._calculationResultId;
    }

    public set calculationResultId(calculationResultId: string) {
        this._calculationResultId = calculationResultId;
        this.applicationPropertyChangedSubject.next(
            { name: 'calculationResultId', value: calculationResultId });
    }

    public get premiumFundingProposalId(): string {
        return this._premiumFundingProposalId;
    }

    public set premiumFundingProposalId(premiumFundingProposalId: string) {
        this._premiumFundingProposalId = premiumFundingProposalId;
        this.applicationPropertyChangedSubject.next(
            { name: 'premiumFundingProposalId', value: premiumFundingProposalId });
    }

    public get principalFinanceAcceptanceUrl(): string {
        return this._principalFinanceAcceptanceUrl;
    }

    public set principalFinanceAcceptanceUrl(principalFinanceAcceptanceUrl: string) {
        this._principalFinanceAcceptanceUrl = principalFinanceAcceptanceUrl;
        this.applicationPropertyChangedSubject.next(
            { name: 'principalFinanceAcceptanceUrl', value: principalFinanceAcceptanceUrl });
    }

    public get policyNumber(): string {
        return this._policyNumber;
    }

    public set policyNumber(policyNumber: string) {
        this._policyNumber = policyNumber;
        this.applicationPropertyChangedSubject.next(
            { name: 'policyNumber', value: policyNumber });
    }

    public get invoiceNumber(): string {
        return this._invoiceNumber;
    }

    public set invoiceNumber(invoiceNumber: string) {
        this._invoiceNumber = invoiceNumber;
        this.applicationPropertyChangedSubject.next(
            { name: 'invoiceNumber', value: invoiceNumber });
    }

    public set quoteType(quoteType: QuoteType) {
        this._quoteType = quoteType;
        this.applicationPropertyChangedSubject.next(
            { name: 'quoteType', value: quoteType });
    }

    public get quoteType(): QuoteType {
        return this._quoteType;
    }

    public get quoteState(): QuoteState {
        return this._quoteState;
    }

    public set quoteState(quoteState: QuoteState) {
        this._quoteState = quoteState;
        this.quoteStateSubject.next(quoteState);
    }

    public get claimState(): ClaimState {
        return this._claimState;
    }

    public set claimState(claimState: ClaimState) {
        this._claimState = claimState;
        this.claimStateSubject.next(claimState);
    }

    public get currentWorkflowDestination(): WorkflowDestination {
        return this._currentWorkflowDestination;
    }

    public set currentWorkflowDestination(destination: WorkflowDestination) {
        this._currentWorkflowDestination = destination;
        const workflowStepChanged: boolean = !this._previousWorkflowDestination
            || this._previousWorkflowDestination.stepName != destination.stepName;
        if (workflowStepChanged) {
            this.workflowStepSubject.next(destination.stepName);
            this.applicationPropertyChangedSubject.next(
                { name: 'currentWorkflowStep', value: destination.stepName });
        }
        const articleIndexChanged: boolean = workflowStepChanged
            || !this._previousWorkflowDestination
            || this._previousWorkflowDestination.articleIndex != destination.articleIndex;
        if (articleIndexChanged && destination.articleIndex != undefined) {
            this.articleIndexSubject.next(destination.articleIndex);
            this.applicationPropertyChangedSubject.next(
                { name: 'currentArticleIndex', value: destination.articleIndex });
        }
        const articleElementIndexChanged: boolean = workflowStepChanged
            || !this._previousWorkflowDestination
            || this._previousWorkflowDestination.articleElementIndex != destination.articleElementIndex;
        if (articleElementIndexChanged && destination.articleElementIndex != undefined) {
            this.articleElementIndexSubject.next(destination.articleElementIndex);
            this.applicationPropertyChangedSubject.next(
                { name: 'currentArticleElementIndex', value: destination.articleElementIndex });
        }
    }

    public get previousWorkflowDestination(): WorkflowDestination {
        return this._previousWorkflowDestination;
    }

    public set previousWorkflowDestination(destination: WorkflowDestination) {
        this._previousWorkflowDestination = destination;
        this.applicationPropertyChangedSubject.next(
            { name: 'previousWorkflowStep', value: destination.stepName });
    }

    public get mode(): ApplicationMode {
        return this._mode;
    }

    public set mode(mode: ApplicationMode) {
        this._mode = mode;
        this.applicationPropertyChangedSubject.next(
            { name: 'mode', value: mode });
    }

    public get version(): string {
        return this._version;
    }

    public set version(version: string) {
        this._version = version;
        this.applicationPropertyChangedSubject.next(
            { name: 'version', value: version });
    }

    public get applicationState(): ApplicationStatus {
        return this._applicationState;
    }

    public set applicationState(applicationState: ApplicationStatus) {
        this._applicationState = applicationState;
        this.quoteStateSubject.next(applicationState);
        this.applicationPropertyChangedSubject.next(
            { name: 'applicationState', value: applicationState });
    }

    public get customerId(): string {
        return this._customerId;
    }

    public set customerId(customerId: string) {
        this._customerId = customerId;
        this.applicationPropertyChangedSubject.next(
            { name: 'customerId', value: customerId });
    }

    public get userHasAccount(): boolean {
        return this._userHasAccount;
    }

    public set userHasAccount(userHasAccount: boolean) {
        this._userHasAccount = userHasAccount;
        this.applicationPropertyChangedSubject.next(
            { name: 'userHasAccount', value: userHasAccount });
    }

    public get quoteReference(): string {
        return this._quoteReference;
    }

    public set quoteReference(quoteReference: string) {
        this._quoteReference = quoteReference;
        this.applicationPropertyChangedSubject.next(
            { name: 'quoteReference', value: quoteReference });
    }

    /**
     * Is the alias of the user that loaded the quote. 
     * This property is from the passed organisation alias of the portal
     * when the forms-app is being loaded.
     */
    public get portalOrganisationAlias(): string {
        return this._portalOrganisationAlias;
    }

    public get sidebarOffsetConfiguration(): string {
        return this._sidebarOffsetConfiguration;
    }

    public set embedOptions(options: WebFormEmbedOptions) {
        this._embedOptions = options;
    }

    public get embedOptions(): WebFormEmbedOptions {
        return this._embedOptions;
    }

    public get productReleaseNumber(): string {
        return this._productReleaseNumber;
    }

    public get productReleaseId(): string {
        return this._productReleaseId;
    }

    public set productReleaseId(productReleaseId: string) {
        this._productReleaseId = productReleaseId;
        this.applicationPropertyChangedSubject.next({ name: 'productReleaseId', value: productReleaseId });
    }
}
