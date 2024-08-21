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
 * Export acknowledge operation class.
 * TODO: Write a better class header: acknowledge operations functions.
 */
@Injectable()
export class AcknowledgeOperation extends OperationWithPayload {
    public static opName: string = 'acknowledge';
    public operationName: string = AcknowledgeOperation.opName;
    protected applicableFormType: FormType = FormType.Claim;
    protected includeFormModel: boolean = true;
    protected requiredApplicationProperties: Array<string> = [
        'claimId',
    ];

    public constructor(
        protected form: FormService,
        protected config: ConfigService,
        protected application: ApplicationService,
        protected apiService: ApiService,
        protected message: MessageService,
        protected errorHandlerService: ErrorHandlerService,
        protected eventService: EventService,
    ) {
        super(form, config, application, apiService, message, errorHandlerService, eventService);
    }
}
