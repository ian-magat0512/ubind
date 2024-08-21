import { Injectable } from '@angular/core';
import { Observable, of, Subject } from 'rxjs';
import { ApiService } from '../services/api.service';
import { ConfigService } from '../services/config.service';
import { ApplicationService } from '../services/application.service';
import { FormService } from '../services/form.service';
import { OperationWithPayload } from './operation-with-payload';
import { ErrorHandlerService } from '@app/services/error-handler.service';
import { BroadcastService } from '../services/broadcast.service';
import { MessageService } from '../services/message.service';
import { FormType } from '../models/form-type.enum';
import { FormTypeApplicationPropertiesResolver } from './operation-form-type-application-properties-resolver';
import { PaymentService } from '@app/services/payment.service';
import { EventService } from '@app/services/event.service';
import {  CalculationResponseCache } from './calculation-response-cache';
import { OperationRequestSettings } from './operation-request-settings';
import { map, mergeMap } from 'rxjs/operators';
import { OperationArguments } from './operation';
import { WorkflowRole } from '@app/models/workflow-role.enum';

export interface CalculationOperationArguments {
    silent: boolean;
}

/**
 * Performs a calculation for the quote using all of the available form data.
 */
@Injectable()
export class CalculationOperation extends OperationWithPayload {
    public static opName: string = 'calculation';
    public operationName: string = CalculationOperation.opName;
    protected includeFormModel: boolean = true;
    protected requiredApplicationProperties: Array<string>;
    protected includeApplicationProperties: Array<string> = [
        'productReleaseId',
    ];
    protected includeWorkflowRoles: any = { };

    public constructor(
        protected formService: FormService,
        protected config: ConfigService,
        protected applicationService: ApplicationService,
        protected apiService: ApiService,
        protected messageService: MessageService,
        protected errorHandlerService: ErrorHandlerService,
        protected broadcast: BroadcastService,
        private formTypeApplicationPropertiesResolver: FormTypeApplicationPropertiesResolver,
        private paymentService: PaymentService,
        protected eventService: EventService,
        private operationManager:  CalculationResponseCache,
    ) {
        super(
            formService,
            config,
            applicationService,
            apiService,
            messageService,
            errorHandlerService,
            eventService);

        // we don't want to publish the calculation results to analytics, there are too many calculations.
        this.publishAppEvents = false;
    }

    public getNextPayload(): any {
        let result: any;
        this.createPayload().subscribe((payload: any) => result = payload);
        return result;
    }

    protected createPayload(): Observable<any> {
        this.requiredApplicationProperties =
            this.formTypeApplicationPropertiesResolver.getApplicationPropertyNamesForFormType();
        return super.createPayload().pipe(
            map((payload: any) => this.addQuoteTypeToPayload(payload)),
            map((payload: any) => {
                const formModel: any = this.getFormModelFromPayload(payload);
                const paymentMethod: any = this.formService.getValueForWorkflowRole(
                    WorkflowRole.PaymentMethod, formModel);
                const singlePayment: boolean = !this.paymentService.isFundingPayment(paymentMethod);
                return this.addPaymentDataToPayload(
                    payload,
                    this.paymentService.getPaymentCardBin(),
                    this.paymentService.getPaymentCardNumberLength(),
                    singlePayment);
            }));
    }

    public generateRequest(
        requestParams: any,
        args: OperationArguments,
        operationId: number,
        abortSubject: Subject<any>,
    ): Observable<any> {
        let customRequestSettings: OperationRequestSettings = {
            retryAttempts: 0,
        };
        return this.createPayload().pipe(
            mergeMap((payload: any) => {
                if (payload == null) {
                    return of(null);
                } else {
                    return this.post(
                        this.operationName, payload, args, operationId, customRequestSettings, abortSubject);
                }
            }));
    }

    protected processResponse(data: any, operationId: any, requestPayload: any, params: any): any {
        if (operationId != this.latestOperationId) {
            // a new operation has since be created so we no longer need to process this operation's response data
            if (this.applicationService.debug) {
                console.log(`Ignoring calculation response ${operationId} since a new operation has been `
                    + `created with a different ID ${this.latestOperationId}`);
                return null;
            }
        }

        // set that this payload succeeded so if the next request tries to send it we'll skip the request
        this.operationManager.addSuccessfulResponse(requestPayload, data);

        const response: any = super.processResponse(data, operationId, requestPayload, params);
        response['calculationResultId'] = response.body?.calculationResultId;
        if (response['calculationResultId']) {
            response['calculationResult'] = response.body?.calculationResult;
        }
        if (!response['calculationResult']['payment']) {
            response['calculationResult']['payment'] = {
                'amountPayable': 0.0,
            };
        }
        if (this.applicationService.formType == FormType.Quote) {
            response['calculationResult']['payment']['amountPayable'] = response.body.amountPayable;
            response['calculationResult']['payment']['refundComponents'] =
                JSON.parse(JSON.stringify(response.body!.refundBreakdown));
            response['calculationResult']['payment']['payableComponents'] =
                JSON.parse(JSON.stringify(response.body!.priceBreakdown));

            if (response.body.premiumFundingProposal != undefined) {
                response['calculationResult']['payment']['premiumFundingProposal'] =
                    JSON.parse(JSON.stringify(response.body.premiumFundingProposal));
            }
        }

        const typedParams: CalculationOperationArguments = <CalculationOperationArguments>params;
        if (!typedParams?.silent) {
            // publish the calculation response.
            this.eventService.calculationResponseSubject.next(data);
        } else {
            if (this.applicationService.debug) {
                console.log('not publishing calculation result because we used the cached version.');
            }
        }
        return response;
    }

    protected processError(err: any, operationId: any): boolean {
        // Ignore operation cancelled error, this occur on race condition when the client closes 
        // the request while the backend still processing the response.
        if(err.status == 499) {
            console.log(err.error?.detail);
            return true;
        }

        setTimeout(() => {
            this.broadcast.broadcast('ErrorPromptEvent', {});
        }, 500);
        return super.processError(err, operationId, 'post');
    }
}
