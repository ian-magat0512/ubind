import { map, mergeMap } from 'rxjs/operators';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from '../services/api.service';
import { ConfigService } from '../services/config.service';
import { ApplicationService } from '../services/application.service';
import { FormService } from '../services/form.service';
import { StripeTokenService } from '../services/stripe-token/stripe-token.service';
import { CreditCardDetails } from '../models/credit-card-details';
import { OperationWithPayload } from './operation-with-payload';
import { StripeTokenResponse } from '../services/stripe-token/stripe-token-response';
import { MessageService } from '../services/message.service';
import { ErrorHandlerService } from '@app/services/error-handler.service';
import { WorkflowRole } from '@app/models/workflow-role.enum';
import { Errors } from '@app/models/errors';
import { PaymentService } from '@app/services/payment.service';
import { EventService } from '@app/services/event.service';

/**
 * Performs a stripe card payment
 */
@Injectable()
export class StripePaymentOperation extends OperationWithPayload {
    public static opName: string = 'stripePayment';
    public operationName: string = StripePaymentOperation.opName;

    protected includeFormModel: boolean = true;
    protected requiredApplicationProperties: Array<string> = [
        'quoteId',
        'calculationResultId',
    ];

    public constructor(
        private tokenService: StripeTokenService,
        protected formService: FormService,
        protected config: ConfigService,
        protected applicationService: ApplicationService,
        protected apiService: ApiService,
        protected messageService: MessageService,
        protected errorHandlerService: ErrorHandlerService,
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

    protected createPayload(): Observable<any> {
        let environment: string = this.applicationService.environment;
        let publicApiKey: string = this.paymentService.getPublicApiKey();
        if (!publicApiKey) {
            throw Errors.Payment.EnvironmentConfigurationMissing(environment, 'stripe', 'publicApiKey');
        }
        const valuesWithIgnoredWorkflowRolesUnencrypted: any =
            this.formService.getValues(true, true, true, true, false, false);
        let creditCardDetails: CreditCardDetails = this.getCreditCardDetails(valuesWithIgnoredWorkflowRolesUnencrypted);
        return super
            .createPayload().pipe(
                mergeMap((payload: any) => this.addTokenToPayload(payload, publicApiKey, creditCardDetails)));
    }

    private getCreditCardDetails(valuesWithIgnoredWorkflowRolesUnencrypted: object): CreditCardDetails {
        let number: string = this.formService.getValueForWorkflowRole(
            WorkflowRole.CreditCardNumber,
            valuesWithIgnoredWorkflowRolesUnencrypted);
        let name: string = this.formService.getValueForWorkflowRole(
            WorkflowRole.CreditCardName,
            valuesWithIgnoredWorkflowRolesUnencrypted);
        let expiryMMyy: string = this.formService.getValueForWorkflowRole(
            WorkflowRole.CreditCardExpiry,
            valuesWithIgnoredWorkflowRolesUnencrypted);
        let ccv: string = this.formService.getValueForWorkflowRole(
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
}
