import { Injectable } from '@angular/core';
import { ApiService } from '../services/api.service';
import { ApplicationService } from '../services/application.service';
import { ConfigService } from '../services/config.service';
import { FormService } from '../services/form.service';
import { OperationWithPayload } from './operation-with-payload';
import { MessageService } from '../services/message.service';
import { ErrorHandlerService } from '@app/services/error-handler.service';
import { UserService } from '../services/user.service';
import { FormType } from '../models/form-type.enum';
import { ResumeApplicationService } from '@app/services/resume-application.service';
import { EventService } from '@app/services/event.service';

/**
 * Creates a policy from a quote.
 * @deprecated instead please use the bind operation.
 */
@Injectable()
export class PolicyOperation extends OperationWithPayload {
    public static opName: string = 'policy';
    public operationName: string = PolicyOperation.opName;
    protected applicableFormType: FormType = FormType.Quote;
    protected includeFormModel: boolean = true;
    protected requiredApplicationProperties: Array<string> = [
        'quoteId',
        'calculationResultId',
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
