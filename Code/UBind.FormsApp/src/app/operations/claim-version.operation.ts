import { Injectable } from '@angular/core';
import { OperationWithPayload } from "./operation-with-payload";
import { FormType } from "../models/form-type.enum";
import { FormService } from "../services/form.service";
import { ConfigService } from "../services/config.service";
import { ApiService } from "../services/api.service";
import { MessageService } from "../services/message.service";
import { ErrorHandlerService } from '@app/services/error-handler.service';
import { ApplicationService } from "../services/application.service";
import { EventService } from '@app/services/event.service';

/**
 * Creates a new version of a claim, so the claim has a history that can be reviewed.
 */
@Injectable()
export class ClaimVersionOperation extends OperationWithPayload {
    public static opName: string = 'claimVersion';
    public operationName: string = ClaimVersionOperation.opName;
    protected applicableFormType: FormType = FormType.Claim;

    protected includeFormModel: boolean = true;
    protected requiredApplicationProperties: Array<string> = [
        'claimId',
        'calculationResultId',
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
