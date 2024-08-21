import { Injectable } from '@angular/core';
import { ApiService } from '../services/api.service';
import { ApplicationService } from '../services/application.service';
import { ConfigService } from '../services/config.service';
import { FormService } from '../services/form.service';
import { OperationWithPayload } from './operation-with-payload';
import { MessageService } from '../services/message.service';
import { ErrorHandlerService } from '@app/services/error-handler.service';
import { FormTypeApplicationPropertiesResolver } from './operation-form-type-application-properties-resolver';
import { WorkflowRole } from '@app/models/workflow-role.enum';
import { HttpErrorResponse, HttpResponse } from '@angular/common/http';
import { EventService } from '@app/services/event.service';
import { Observable } from 'rxjs';

/**
 * Creates a user account for a customer
 */
@Injectable()
export class CustomerUserOperation extends OperationWithPayload {
    public static opName: string = 'customerUser';
    public operationName: string = CustomerUserOperation.opName;

    protected includeFormModel: boolean = false;
    protected requiredApplicationProperties: Array<string> = [
        'customerId',
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
        private formTypeApplicationPropertiesResolver: FormTypeApplicationPropertiesResolver,
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
        this.requiredApplicationProperties.push(
            ...this.formTypeApplicationPropertiesResolver.getApplicationPropertyNamesForFormType());
        return super.createPayload();
    }

    protected processResponse(data: any, operationId: any, requestPayload: any, params: any): any {
        super.processResponse(data, operationId, requestPayload, params);
        if (data instanceof HttpResponse) { // if successful
            this.applicationService.userHasAccount = true;
        } else if (data instanceof HttpErrorResponse) {
            this.applicationService.userHasAccount = false;
        }
    }
}
