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
 * The assessment approval operation is used when a claim is assessed, and found
 * to be a valid claim - it can then be approved wtih this operation.
 */
@Injectable()
export class AssessmentApprovalOperation extends OperationWithPayload {
    public static opName: string = 'assessmentApproval';
    public operationName: string = AssessmentApprovalOperation.opName;
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
