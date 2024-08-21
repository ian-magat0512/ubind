import { Injectable } from '@angular/core';
import { ApplicationService } from './application.service';
import { CurrencyHelper } from '@app/helpers/currency.helper';
import { CallbackDataResult, PriceSummary } from '@app/models/callback-data-result.model';
import { Payment } from '@app/models/calculation-result';
import { WorkflowRole } from '@app/models/workflow-role.enum';
import { FormService } from './form.service';

/**
 * Export form event call back service class.
 * This contains methods related to form event call backs.
 */
@Injectable({
    providedIn: 'root',
})
export class FormEventCallbackService {

    public constructor(
        private applicationService: ApplicationService,
        private formService: FormService) {}

    public createDataObject(): CallbackDataResult {
        let data: CallbackDataResult = {
            state: this.applicationService.applicationState,
            quoteId: this.applicationService.quoteId,
            quoteReference: this.applicationService.quoteReference,
            // added the condition as this.applicationService?.policyId is returning null 
            policyId: this.applicationService?.policyId ?? "",
            policyNumber: this.applicationService.policyNumber,
            customerId: this.applicationService.customerId,
            customerAccountEmail: this.formService
                .getValueForWorkflowRole(WorkflowRole.CustomerEmail, this.formService.getValues()),
            productId: this.applicationService.productId,
            productAlias: this.applicationService.productAlias,
            organisationId: this.applicationService.organisationId,
            organisationAlias: this.applicationService.organisationAlias,
            tenantId: this.applicationService.tenantId,
            tenantAlias: this.applicationService.tenantAlias,
            environment: this.applicationService.environment,
            workflowStep: this.applicationService.currentWorkflowDestination.stepName,
            priceSummary: this.getPriceSummary(),
        };
        return this.filterUndefinedAndEmptyStringProperties(data);
    }

    private getPriceSummary(): PriceSummary | undefined {
        const quoteResultPayment: Payment = !this.applicationService.latestQuoteResult
            ? null
            : this.applicationService.latestQuoteResult.payment;
        const hasPriceSummary: boolean = !!(quoteResultPayment
            && (quoteResultPayment?.payableComponents && quoteResultPayment?.priceComponents));
        let serviceFees: number = hasPriceSummary ? this.getServiceFeesTotal() : 0;
        let paymentFees: number = hasPriceSummary ? this.getPaymentFeesTotal() : 0;

        if (!hasPriceSummary) {
            return undefined;
        }

        let priceSummary: PriceSummary = {
            currencyCode: quoteResultPayment.payableComponents.currencyCode,
            premium: this.returnParsedCurrencyOrUndefined(quoteResultPayment.priceComponents.basePremium),
            // TODO: As per John, this is not yet in production 
            // it will be introduced in the next version of PriceBreakdowns
            terrorismPremium: undefined,
            emergencyServicesLevy: this.returnParsedCurrencyOrUndefined(quoteResultPayment.priceComponents.ESL),
            stampDuty: this.returnParsedCurrencyOrUndefined(quoteResultPayment.priceComponents.stampDutyTotal),
            serviceFees: serviceFees != 0 ? serviceFees : undefined,
            paymentFees: paymentFees != 0 ? paymentFees : undefined,
            goodsAndServicesTax: this.returnParsedCurrencyOrUndefined(quoteResultPayment.priceComponents.totalGST),
            totalPayable: this.returnParsedCurrencyOrUndefined(quoteResultPayment.priceComponents.totalPayable),
        };

        return this.filterUndefinedAndEmptyStringProperties(priceSummary);
    }

    private filterUndefinedAndEmptyStringProperties<T>(obj: T): T {
        return Object.entries(obj).reduce((acc: T, [key, value]: [string, any]) => {
            if (value !== undefined && value !== '') {
                acc[key as keyof T] = value;
            }
            return acc;
        }, {} as T);
    }

    private returnParsedCurrencyOrUndefined(value?: string): number | undefined {
        let parsedValue: number = value ?  CurrencyHelper.parse(value) : 0;
        return parsedValue ? parsedValue : undefined;
    }

    private getServiceFeesTotal(): number {
        const latestQuoteResultPayment: Payment = this.applicationService.latestQuoteResult.payment;
        return this.totalFees([
            latestQuoteResultPayment?.payableComponents?.brokerFee,
            latestQuoteResultPayment?.payableComponents?.underwriterFee,
            latestQuoteResultPayment?.payableComponents?.roadsideAssistanceFee,
            latestQuoteResultPayment?.payableComponents?.policyFee,
            latestQuoteResultPayment?.payableComponents?.partnerFee,
            latestQuoteResultPayment?.payableComponents?.administrationFee,
            latestQuoteResultPayment?.payableComponents?.establishmentFee,
        ]);
    }

    private getPaymentFeesTotal(): number {
        const latestQuoteResultPayment: Payment = this.applicationService.latestQuoteResult.payment;
        return this.totalFees([
            latestQuoteResultPayment?.priceComponents?.merchantFees,
            latestQuoteResultPayment?.priceComponents?.interest,
            latestQuoteResultPayment?.priceComponents?.transactionCosts,
        ]);
    }

    private totalFees(fees: Array<string | undefined | null>): number {
        return fees.reduce((sum: number, fee: string) => {
            return fee ? CurrencyHelper.parse(fee) + sum : sum;
        }, 0);
    }
}
