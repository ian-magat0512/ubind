import { Injectable } from '@angular/core';
import { ApiService } from '../services/api.service';
import { ApplicationService } from '../services/application.service';
import { ConfigService } from '../services/config.service';
import { FormService } from '../services/form.service';

import { OperationWithPayload } from './operation-with-payload';
import { MessageService } from '../services/message.service';
import { ErrorHandlerService } from '@app/services/error-handler.service';
import { FormType } from '../models/form-type.enum';
import { EventService } from '@app/services/event.service';

/**
 * Refers a quote for endorsment by and underwriter.
 */
@Injectable()
export class EndorsementReferralOperation extends OperationWithPayload {
    public static opName: string = 'endorsementReferral';
    public operationName: string = EndorsementReferralOperation.opName;
    protected applicableFormType: FormType = FormType.Quote;
    protected includeFormModel: boolean = true;
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
