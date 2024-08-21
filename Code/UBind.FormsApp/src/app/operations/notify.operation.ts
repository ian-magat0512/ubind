import { Injectable } from '@angular/core';
import { OperationWithPayload } from './operation-with-payload';
import { FormService } from '../services/form.service';
import { ConfigService } from '../services/config.service';
import { ApplicationService } from '../services/application.service';
import { ApiService } from '../services/api.service';
import { ErrorHandlerService } from '@app/services/error-handler.service';
import { MessageService } from '../services/message.service';
import { FormType } from '../models/form-type.enum';
import { EventService } from '@app/services/event.service';

/**
 * Marks a claim as notified. This is used when the insured identifies that a loss
 * has occurred and there might be a claim coming at some time in the future, e.g. FNOL.
 */
@Injectable()

export class NotifyOperation extends OperationWithPayload {
    public static opName: string = 'notify';
    public operationName: string = NotifyOperation.opName;
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
