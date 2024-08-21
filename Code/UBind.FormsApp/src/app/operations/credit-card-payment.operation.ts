import { Injectable } from '@angular/core';
import { ApiService } from '../services/api.service';
import { ConfigService } from '../services/config.service';
import { ApplicationService } from '../services/application.service';
import { FormService } from '../services/form.service';
import { CalculationService } from '../services/calculation.service';

import { OperationWithPayload } from './operation-with-payload';
import { MessageService } from '../services/message.service';
import { ErrorHandlerService } from '@app/services/error-handler.service';
import { WorkflowRole } from '@app/models/workflow-role.enum';
import { map } from 'rxjs/operators';
import { EventService } from '@app/services/event.service';
import { Observable } from 'rxjs';

/**
 * Performs a credit card payment.
 */
@Injectable()
export class CreditCardPaymentOperation extends OperationWithPayload {
    public static opName: string = 'creditCardPayment';
    public operationName: string = CreditCardPaymentOperation.opName;

    protected includeFormModel: boolean = true;
    protected requiredApplicationProperties: Array<string> = [
        'quoteId',
        'calculationResultId',
    ];
    protected includeWorkflowRoles: any = {
        'creditCardDetails': [
            WorkflowRole.CreditCardNumber,
            WorkflowRole.CreditCardName,
            WorkflowRole.CreditCardExpiry,
            WorkflowRole.CreditCardCcv,
        ],
    };
    protected requiredWorkflowRoles: Array<string> = [
        WorkflowRole.CreditCardNumber,
        WorkflowRole.CreditCardName,
        WorkflowRole.CreditCardExpiry,
        WorkflowRole.CreditCardCcv,
    ];

    public constructor(
        protected formService: FormService,
        protected config: ConfigService,
        protected calculation: CalculationService,
        protected applicationService: ApplicationService,
        protected apiService: ApiService,
        protected messageService: MessageService,
        protected errorHandlerService: ErrorHandlerService,
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
        const valuesWithIgnoredWorkflowRoles: any = this.formService.getValues(true, true, true);
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
        } else {
            return super.createPayload();
        }
    }
}
