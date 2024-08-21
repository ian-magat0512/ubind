import { Injectable } from '@angular/core';
import { OperationWithPayload } from './operation-with-payload';
import { FormService } from '../services/form.service';
import { ConfigService } from '../services/config.service';
import { ApplicationService } from '../services/application.service';
import { ApiService } from '../services/api.service';
import { MessageService } from '../services/message.service';
import { ErrorHandlerService } from '@app/services/error-handler.service';
import { FormType } from '../models/form-type.enum';
import { EventService } from '@app/services/event.service';

/**
 * Marks a claim as settled, which means payment for the claim
 * has been processed.
 */
@Injectable()

export class SettleOperation extends OperationWithPayload {
    public static opName: string = 'settle';
    public operationName: string = SettleOperation.opName;
    protected applicableFormType: FormType = FormType.Claim;
    protected includeFormModel: boolean = true;
    protected requiredApplicationProperties: Array<string> = [
        'claimId',
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
