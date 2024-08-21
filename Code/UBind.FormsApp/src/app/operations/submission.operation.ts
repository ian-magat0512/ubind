import { Injectable } from '@angular/core';
import { ApiService } from '../services/api.service';
import { ApplicationService } from '../services/application.service';
import { ConfigService } from '../services/config.service';
import { FormService } from '../services/form.service';
import { OperationWithPayload } from './operation-with-payload';
import { MessageService } from '../services/message.service';
import { ErrorHandlerService } from '@app/services/error-handler.service';
import { UserService } from '../services/user.service';
import { ResumeApplicationService } from '@app/services/resume-application.service';
import { EventService } from '@app/services/event.service';

/**
 * Submits or completes a quote.
 * This is use for quote-only systems that do not bind policies.
 */
@Injectable()
export class SubmissionOperation extends OperationWithPayload {
    public static opName: string = 'submission';
    public operationName: string = SubmissionOperation.opName;

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
        private resumeApplicationService: ResumeApplicationService,
        private userService: UserService,
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

    protected processResponse(data: any, operationId: any, requestPayload: any, params: any): any {

        if (data.status == 200 && this.userService.isCustomer && !this.userService.isCustomerOrClientLoggedIn) {
            this.resumeApplicationService.deleteQuoteId();
        }

        return super.processResponse(data, operationId, requestPayload, params);
    }

}
