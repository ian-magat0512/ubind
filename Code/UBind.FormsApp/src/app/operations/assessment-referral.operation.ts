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
 * Used when a claim is sent for assessment, or referred to an assesor.
 */
@Injectable()
export class AssessmentReferralOperation extends OperationWithPayload {
    public static opName: string = 'assessmentReferral';
    public operationName: string = AssessmentReferralOperation.opName;
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
