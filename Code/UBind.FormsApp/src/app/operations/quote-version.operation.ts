import { Injectable } from '@angular/core';

import { ApiService } from '../services/api.service';
import { ApplicationService } from '../services/application.service';
import { ConfigService } from '../services/config.service';
import { FormService } from '../services/form.service';

import { OperationWithPayload } from './operation-with-payload';
import { MessageService } from '../services/message.service';
import { ErrorHandlerService } from '@app/services/error-handler.service';
import { FormType } from '../models/form-type.enum';
import { WorkflowRole } from '@app/models/workflow-role.enum';
import { EventService } from '@app/services/event.service';

/**
 * Creates a new quote version, so there's a history of changes
 * associated with a quote that can be reviewed.
 */
@Injectable()
export class QuoteVersionOperation extends OperationWithPayload {
    public static opName: string = 'quoteVersion';
    public operationName: string = QuoteVersionOperation.opName;
    protected applicableFormType: FormType = FormType.Quote;

    protected includeFormModel: boolean = true;
    protected requiredApplicationProperties: Array<string> = [
        'quoteId',
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
}
