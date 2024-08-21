import { Injectable } from '@angular/core';
import { ApiService } from '../services/api.service';
import { ApplicationService } from '../services/application.service';
import { ConfigService } from '../services/config.service';
import { FormService } from '../services/form.service';
import { OperationWithPayload } from './operation-with-payload';
import { MessageService } from '../services/message.service';
import { ErrorHandlerService } from '@app/services/error-handler.service';
import { Observable, of, Subject } from 'rxjs';
import { FormTypeApplicationPropertiesResolver } from './operation-form-type-application-properties-resolver';
import { EventService } from '@app/services/event.service';
import { PaymentService } from '@app/services/payment.service';
import { map, mergeMap } from 'rxjs/operators';
import { OperationRequestSettings } from './operation-request-settings';
import { OperationArguments } from './operation';
import { WorkflowRole } from '@app/models/workflow-role.enum';

/**
 * Stores the form data that has recently been entered.
 */
@Injectable()
export class FormUpdateOperation extends OperationWithPayload {
    public static opName: string = 'formUpdate';
    public operationName: string = FormUpdateOperation.opName;
    protected includeFormModel: boolean = true;
    protected requiredApplicationProperties: Array<string>;

    public constructor(
        protected formService: FormService,
        protected config: ConfigService,
        protected applicationService: ApplicationService,
        protected apiService: ApiService,
        protected messageService: MessageService,
        protected errorHandlerService: ErrorHandlerService,
        private formTypeApplicationPropertiesResolver: FormTypeApplicationPropertiesResolver,
        protected eventService: EventService,
        private paymentService: PaymentService,
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

    protected processResponse(data: any, operationId: any, requestPayload: any, args: OperationArguments): any {
        return super.processResponse(data, operationId, requestPayload, args);
    }
}
