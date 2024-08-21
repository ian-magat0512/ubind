import { map, mergeMap } from 'rxjs/operators';
import { Injectable } from '@angular/core';
import { Observable, Subject } from 'rxjs';
import { ApiService } from '../services/api.service';
import { ApplicationService } from '../services/application.service';
import { ConfigService } from '../services/config.service';
import { FormService } from '../services/form.service';
import { OperationWithPayload } from './operation-with-payload';
import { CreditCardDetails } from '../models/credit-card-details';
import { StripeTokenResponse } from '../services/stripe-token/stripe-token-response';
import { StripeTokenService } from '../services/stripe-token/stripe-token.service';
import { MessageService } from '../services/message.service';
import { ErrorHandlerService } from '@app/services/error-handler.service';
import { CalculationService } from '../services/calculation.service';
import { UserService } from '../services/user.service';
import { FormType } from '../models/form-type.enum';
import { ResumeApplicationService } from '@app/services/resume-application.service';
import { CurrencyHelper } from '@app/helpers/currency.helper';
import { QuoteResult } from '@app/models/quote-result';
import { WorkflowRole } from '@app/models/workflow-role.enum';
import { PaymentService } from '@app/services/payment.service';
import { Errors } from '@app/models/errors';
import { EventService } from '@app/services/event.service';
import { OperationArguments } from './operation';

/**
 * Binds a policy, which effectively creates a policy out of a quote.
 */
@Injectable()
export class BindOperation extends OperationWithPayload {
    public static opName: string = 'bind';
    public operationName: string = BindOperation.opName;
    protected applicableFormType: FormType = FormType.Quote;
    protected includeFormModel: boolean = false;
    protected requiredApplicationProperties: Array<string> = [
        'quoteId',
        'calculationResultId',
    ];
    protected requiredWorkflowRoles: Array<any> = [];
    private cardPaymentOptions: Array<string> = ['VISA', 'MASTERCARD', 'AMEX', 'DINERS CLUB', 'DIRECT DEBIT'];
    private fundingPaymentOptions: Array<string> = ['PREMIUM FUNDING'];

    public constructor(
        private tokenService: StripeTokenService,
        protected formService: FormService,
        protected config: ConfigService,
        protected applicationService: ApplicationService,
        protected apiService: ApiService,
        protected messageService: MessageService,
        protected errorHandlerService: ErrorHandlerService,
        protected calculationService: CalculationService,
        private resumeApplicationService: ResumeApplicationService,
        private userService: UserService,
        private paymentService: PaymentService,
        protected eventService: EventService,
    ) {
        super(
            formService,
            config,
            applicationService,
            apiService,
            messageService,
            errorHandlerService,
            eventService);
    }

    public generateRequest(
        params: any,
        args: OperationArguments,
        operationId: number,
        abortSubject: Subject<any>,
    ): Observable<any> {
        const customRequestSettings: any = {
            'retryAttempts': 0,
            'followUpAttempts': 1,
            'timeoutRequests': true,
            'retryIntervalMillis': 100000,
        };
        return this.createPayload().pipe(
            mergeMap((payload: any) => this.post(
                this.operationName, payload, args, operationId, customRequestSettings, abortSubject)));
    }

    protected createPayload(): Observable<any> {
        // if there is nothing to pay, create a payload without the credit card or premium funding details.
        const latestQuoteResult: QuoteResult = this.applicationService.latestQuoteResult;
        if (latestQuoteResult && latestQuoteResult.amountPayable) {
            let payableAmount: number = CurrencyHelper.parse(latestQuoteResult.amountPayable);
            if (payableAmount <= 0) {
                return super.createPayload();
            }
        }

        const valuesWithIgnoredWorkflowRoles: any = this.formService.getValues(true, true, true);
        const paymentMethod: any = this.formService.getValueForWorkflowRole(
            WorkflowRole.PaymentMethod,
            valuesWithIgnoredWorkflowRoles);
        const savedPaymentMethodId: any = this.formService.getValueForWorkflowRole(
            WorkflowRole.SavedPaymentMethodId,
            valuesWithIgnoredWorkflowRoles);

        if (savedPaymentMethodId) {
            return super
                .createPayload().pipe(
                    map((payload: any) => {
                        payload['savedPaymentMethodId'] = savedPaymentMethodId;
                        return payload;
                    }));
        }
        if (paymentMethod) {
            const isCardPayment: boolean = this.paymentService.isCardPayment(paymentMethod);
            const isFundingPayment: boolean = this.paymentService.isFundingPayment(paymentMethod);
            if (isCardPayment)   {
                const publicApiKey: string = this.paymentService.hasPaymentFormConfiguration()
                    ? this.paymentService.getPublicApiKey()
                    : null;
                if (publicApiKey) {
                    const valuesWithIgnoredWorkflowRolesUnencrypted: any = this.formService.getValues(
                        true, true, true, true, false, false);
                    const creditCardDetails: CreditCardDetails = this.getCreditCardDetails(
                        valuesWithIgnoredWorkflowRolesUnencrypted);
                    return super
                        .createPayload().pipe(
                            mergeMap((payload: any) => this.addTokenToPayload(
                                payload,
                                publicApiKey,
                                creditCardDetails)));
                }
            }

            if (isFundingPayment || isCardPayment) {
                this.includeWorkflowRolesByPayment(paymentMethod);
                const premiumFundingId: string = this.applicationService.premiumFundingProposalId;
                if (isFundingPayment) {
                    if (premiumFundingId) {
                        this.requiredApplicationProperties.push('premiumFundingProposalId');
                    } else {
                        throw Errors.Payment.PremiumFundingProposalMissing(paymentMethod);
                    }
                }
            }
        }

        return super.createPayload();
    }

    protected processResponse(data: any, operationId: any, requestPayload: any, params: any): any {

        if (data.status == 200 && this.userService.isCustomer && !this.userService.isCustomerOrClientLoggedIn) {
            this.resumeApplicationService.deleteQuoteId();
        }

        return super.processResponse(data, operationId, requestPayload, params);
    }

    private getCreditCardDetails(valuesWithIgnoredWorkflowRolesUnencrypted: object): CreditCardDetails {
        const number: string = this.formService.getValueForWorkflowRole(
            WorkflowRole.CreditCardNumber,
            valuesWithIgnoredWorkflowRolesUnencrypted);
        const name: string = this.formService.getValueForWorkflowRole(
            WorkflowRole.CreditCardName,
            valuesWithIgnoredWorkflowRolesUnencrypted);
        const expiryMMyy: string = this.formService.getValueForWorkflowRole(
            WorkflowRole.CreditCardExpiry,
            valuesWithIgnoredWorkflowRolesUnencrypted);
        const ccv: string = this.formService.getValueForWorkflowRole(
            WorkflowRole.CreditCardCcv,
            valuesWithIgnoredWorkflowRolesUnencrypted);

        return new CreditCardDetails(
            name,
            number,
            expiryMMyy,
            ccv);
    }

    private addTokenToPayload(payload: string, publicApiKey: string,
        creditCardDetails: CreditCardDetails): Observable<any> {
        return this.tokenService
            .getToken(publicApiKey, creditCardDetails).pipe(
                map((tokenResponse: StripeTokenResponse) => this.setTokenIdOnPayload(payload, tokenResponse)));
    }

    private setTokenIdOnPayload(payload: any, tokenResponse: StripeTokenResponse): any {
        payload['tokenId'] = tokenResponse.id;
        return payload;
    }

    private includeWorkflowRolesByPayment(paymentMethod: string): void {
        if (this.cardPaymentOptions.indexOf(paymentMethod.toUpperCase()) > -1) {
            this.requiredWorkflowRoles = [
                WorkflowRole.CreditCardNumber,
                WorkflowRole.CreditCardName,
                WorkflowRole.CreditCardExpiry,
                WorkflowRole.CreditCardCcv,
            ];
            this.includeWorkflowRoles = {
                'creditCardDetails': [
                    WorkflowRole.CreditCardNumber,
                    WorkflowRole.CreditCardName,
                    WorkflowRole.CreditCardExpiry,
                    WorkflowRole.CreditCardCcv,
                ],
            };
        } else if (this.fundingPaymentOptions.indexOf(paymentMethod.toUpperCase()) > -1) {
            this.includeWorkflowRoles = {
                'bankAccountDetails': [
                    WorkflowRole.BankAccountName,
                    WorkflowRole.BankAccountBsb,
                    WorkflowRole.BankAccountNumber,
                ],
                'creditCardDetails': [
                    WorkflowRole.CreditCardNumber,
                    WorkflowRole.CreditCardName,
                    WorkflowRole.CreditCardExpiry,
                    WorkflowRole.CreditCardCcv,
                ],
            };
        }
    }
}
