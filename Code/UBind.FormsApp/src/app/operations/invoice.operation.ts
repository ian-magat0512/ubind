import { Injectable } from '@angular/core';
import { ApiService } from '../services/api.service';
import { ApplicationService } from '../services/application.service';
import { ConfigService } from '../services/config.service';
import { FormService } from '../services/form.service';

import { OperationWithPayload } from './operation-with-payload';
import { MessageService } from '../services/message.service';
import { ErrorHandlerService } from '@app/services/error-handler.service';
import { EventService } from '@app/services/event.service';

/**
 * Creates an invoice for the current quote and sends it to the user (e.g. via email)
 * Note, it's best to instead use the bind operation as that will send the invoice
 * and handle payment and binding all in one atomic call.
 */
@Injectable()
export class InvoiceOperation extends OperationWithPayload {
    public static opName: string = 'invoice';
    public operationName: string = InvoiceOperation.opName;

    protected includeFormModel: boolean = false;
    protected requiredApplicationProperties: Array<string> = [
        'quoteId',
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
}
