import { Injectable } from '@angular/core';
import { ApiService } from '../services/api.service';
import { ApplicationService } from '../services/application.service';
import { ConfigService } from '../services/config.service';
import { FormService } from '../services/form.service';
import { OperationWithPayload } from './operation-with-payload';
import { MessageService } from '../services/message.service';
import { ErrorHandlerService } from '@app/services/error-handler.service';
import { FormTypeApplicationPropertiesResolver } from './operation-form-type-application-properties-resolver';
import { EventService } from '@app/services/event.service';
import { Observable } from 'rxjs';

/**
 * When a quote is being reviewed, e.g. by a broker, it can be approved
 * to accept the review triggers and allow the quote to be sent for endorsement,
 * or if there are no endorsement triggers, to be sent back to the customer
 * to complete and make payment.
 */
@Injectable()
export class ReviewApprovalOperation extends OperationWithPayload {
    public static opName: string = 'reviewApproval';
    public operationName: string = ReviewApprovalOperation.opName;

    protected includeFormModel: boolean = true;
    protected requiredApplicationProperties: Array<string>;

    public constructor(
        protected formService: FormService,
        protected config: ConfigService,
        protected applicationService: ApplicationService,
        protected apiService: ApiService,
        protected messageService: MessageService,
        protected errorHandlerService: ErrorHandlerService,
        private formTypeApplicationPropertiesResolver: FormTypeApplicationPropertiesResolver,
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

    protected createPayload(): Observable<any> {
        this.requiredApplicationProperties =
            this.formTypeApplicationPropertiesResolver.getApplicationPropertyNamesForFormType();
        return super.createPayload();
    }
}
