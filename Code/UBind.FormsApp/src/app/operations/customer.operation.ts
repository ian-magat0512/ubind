import { Injectable } from '@angular/core';
import { Observable, Subject } from 'rxjs';

import { ApiService } from '../services/api.service';
import { ApplicationService } from '../services/application.service';
import { ConfigService } from '../services/config.service';
import { FormService } from '../services/form.service';

import { OperationWithPayload } from './operation-with-payload';
import { MessageService } from '../services/message.service';
import { ErrorHandlerService } from '@app/services/error-handler.service';
import { mergeMap } from 'rxjs/operators';
import { WorkflowRole } from '@app/models/workflow-role.enum';
import { FormType } from '@app/models/form-type.enum';
import { EventService } from '@app/services/event.service';
import { OperationArguments } from './operation';

/**
 * Creates a customer using the entered form data.
 */
@Injectable()
export class CustomerOperation extends OperationWithPayload {
    public static opName: string = 'customer';
    public operationName: string = CustomerOperation.opName;

    protected includeFormModel: boolean = false;
    protected requiredApplicationProperties: Array<string> = [
        /*
        'quoteId',
        'claimId',
        */
    ];

    protected includeApplicationProperties: Array<string> = [
        'portalId',
    ];

    protected includeWorkflowRoles: any = {
        'customerDetails': [
            WorkflowRole.CustomerName,
            WorkflowRole.CustomerEmail,
            WorkflowRole.CustomerPhone,
            WorkflowRole.CustomerMobile,
            WorkflowRole.CustomerWorkPhone,
        ],
    };
    protected requiredWorkflowRoles: Array<string> = [
        'customerName',
    ];

    public constructor(
        protected formService: FormService,
        protected config: ConfigService,
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

    public generateRequest(
        requestParams: any,
        args: OperationArguments,
        operationId: number,
        abortSubject: Subject<any>,
    ): Observable<any> {
        if (this.applicationService.formType == FormType.Quote) {
            this.requiredApplicationProperties.push('quoteId');
        } else if (this.applicationService.formType == FormType.Claim) {
            this.requiredApplicationProperties.push('claimId');
        }
        if (!this.applicationService.customerId) {
            return super.generateRequest(requestParams, args, operationId, abortSubject);
        } else {
            let customRequestSettings: any = {
                'retryAttempts': 0,
                'retryIntervalMillis': 15000,
                'followUpAttempts': 1,
            };
            return super.createPayload().pipe(
                mergeMap((payload: any) => this.patch(
                    `customer/${this.applicationService.customerId}`,
                    payload,
                    args,
                    operationId,
                    customRequestSettings)),
            );
        }
    }

    protected processResponse(data: any, operationId: any, requestPayload: any, args: OperationArguments): any {
        this.messageService.sendMessage('customerUpdated', {});
        const response: any = super.processResponse(data, operationId, requestPayload, args);
        if (response) {
            this.applicationService.customerId = response.customerId;
        }
        return response;
    }
}
