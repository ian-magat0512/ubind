import { Injectable } from "@angular/core";
import { EnumHelper } from "@app/helpers/enum.helper";
import { Errors } from "@app/models/errors";
import { LoadOperationResultProperty } from "@app/models/quote-property";
import { QuoteState } from "@app/models/quote-state.enum";
import { QuoteType } from "@app/models/quote-type.enum";
import { ClaimApplicationModel } from "@app/resource-models/claim-application-model";
import { QuoteApplicationModel } from "@app/resource-models/quote-application-model";
import { CustomerApiService } from "./api/customer-api.service";
import { ApplicationService } from "./application.service";
import { OperationFactory } from "../operations/operation.factory";
import { UserService } from "./user.service";
import { WorkflowStatusService } from "./workflow-status.service";
import { ClaimState } from '@app/models/claim-state.enum';
import { StringHelper } from "@app/helpers/string.helper";
import { Operation } from "@app/operations/operation";

/**
 * This service loads an application (quote or claim) from the back.
 */
@Injectable({
    providedIn: 'root',
})
export class ApplicationLoadService {

    public constructor(
        private operationFactory: OperationFactory,
        private workflowStatusService: WorkflowStatusService,
        private customerApiService: CustomerApiService,
        private applicationService: ApplicationService,
        private userService: UserService,
    ) { }

    public async loadQuote(quoteId: string, version: string = null, quoteType: QuoteType = null): Promise<void> {
        const params: any = {
            quoteId: quoteId,
        };
        if (version) {
            params['version'] = version;
        }
        const data: any = await this.loadApplication(params);
        if (data[LoadOperationResultProperty.QuoteType]) {
            const loadedQuoteType: QuoteType = EnumHelper.parseOrThrow(
                QuoteType,
                data[LoadOperationResultProperty.QuoteType],
                LoadOperationResultProperty.QuoteType,
                'loadQuote');
            if (quoteType != null && quoteType != loadedQuoteType) {
                throw Errors.Quote.QuoteTypeMismatch(quoteId, quoteType, loadedQuoteType);
            }
            this.applicationService.quoteType = loadedQuoteType;
        }
        if (data[LoadOperationResultProperty.QuoteState]) {
            let quoteState: QuoteState = EnumHelper.parseOrThrow(
                QuoteState,
                data[LoadOperationResultProperty.QuoteState],
                LoadOperationResultProperty.QuoteState,
                'loadQuote');
            this.applicationService.quoteState = quoteState;
        }
    }

    public async loadQuoteForPolicy(policyId: string,
        quoteType: QuoteType,
        version: string = null,
        productRelease: string = null,
    ): Promise<void> {
        const params: any = {
            policyId: policyId,
            quoteType: quoteType,
            createIfNotExists: true,
            discardExistingQuoteOnCreate: true,
        };
        if (version) {
            params['version'] = version;
        }
        if (productRelease) {
            params['productRelease'] = productRelease;
        }
        const data: any = await this.loadApplication(params);
        this.applicationService.quoteType = quoteType;
        if (data[LoadOperationResultProperty.QuoteState]) {
            let quoteState: QuoteState = EnumHelper.parseOrThrow(
                QuoteState,
                data[LoadOperationResultProperty.QuoteState],
                LoadOperationResultProperty.QuoteState,
                'loadQuoteForPolicy');
            this.applicationService.quoteState = quoteState;
        }
    }

    public async loadClaim(claimId: string, version: string = null): Promise<void> {
        const params: any = {
            claimId: claimId,
        };
        if (version) {
            params['version'] = version;
        }
        const data: any = await this.loadApplication(params);
        if (data[LoadOperationResultProperty.ClaimState]) {
            let claimState: ClaimState = EnumHelper.parseOrThrow(
                ClaimState,
                data[LoadOperationResultProperty.ClaimState],
                LoadOperationResultProperty.ClaimState,
                'loadClaim');
            this.applicationService.claimState = claimState;
        }
    }

    public async handleLoadResponse(data: QuoteApplicationModel | ClaimApplicationModel): Promise<void> {
        if (data[LoadOperationResultProperty.QuoteState]) {
            let quoteState: QuoteState = EnumHelper.parseOrThrow(
                QuoteState,
                data[LoadOperationResultProperty.QuoteState],
                LoadOperationResultProperty.QuoteState,
                'handleLoadResponse');
            this.applicationService.previousQuoteState = quoteState;
            this.applicationService.quoteState = quoteState;
        }
        if (data[LoadOperationResultProperty.IsTestData]) {
            this.applicationService.isTestData = data[LoadOperationResultProperty.IsTestData];
        }
        if (data[LoadOperationResultProperty.HadCustomerOnCreation]) {
            this.applicationService.hadCustomerOnCreation
                = data[LoadOperationResultProperty.HadCustomerOnCreation];
        }
        if (data[LoadOperationResultProperty.ProductReleaseId]) {
            this.applicationService.productReleaseId
                = data[LoadOperationResultProperty.ProductReleaseId];
        }
        this.workflowStatusService.isApplicationLoaded = true;
        if (data[LoadOperationResultProperty.CustomerId]) {
            this.applicationService.customerId = data[LoadOperationResultProperty.CustomerId];
            if (this.userService.isClientLoggedIn) {
                await this.customerApiService.hasAccount(
                    this.applicationService.tenantId,
                    data[LoadOperationResultProperty.CustomerId])
                    .toPromise()
                    .then((response: boolean) => {
                        this.userService.isLoadedCustomerHasUser = response;
                        this.workflowStatusService.loadedCustomerHasUserSubject.next(
                            this.userService.isLoadedCustomerHasUser);
                    });
            } else if (this.userService.isCustomerLoggedIn) {
                this.userService.isLoadedCustomerHasUser = true;
                this.workflowStatusService.loadedCustomerHasUserSubject.next(
                    this.userService.isLoadedCustomerHasUser);
            }
        }
        if (data && data.workflowStep) {
            // set the current workflow step so it can be used within start screen expressions
            if (!this.applicationService.currentWorkflowDestination
                || StringHelper.isNullOrEmpty(this.applicationService.currentWorkflowDestination.stepName)) {
                // we only set this if it doesn't have a current destination,
                // because otherwise quote resume stops working.
                this.applicationService.currentWorkflowDestination = { stepName: data.workflowStep };
            }

            // add the current workflow step to the history so it's available for the 
            // expression method getPreviousWorkflowStep()
            this.workflowStatusService.workflowStepHistory.push(data.workflowStep);
        }
    }

    private async loadApplication(requestParams: object): Promise<any> {
        return new Promise((resolve: any, reject: any): any => {
            let operation: Operation = this.operationFactory.create('load');
            operation.execute(requestParams)
                .subscribe(async (data: any) => {
                    await this.handleLoadResponse(data);
                    resolve(data);
                });
        });
    }
}
