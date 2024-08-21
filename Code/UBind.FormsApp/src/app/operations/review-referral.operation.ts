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
 * Sends a quote for review, which happens when there are review triggers
 */
@Injectable()
export class ReviewReferralOperation extends OperationWithPayload {
    public static opName: string = 'reviewReferral';
    public operationName: string = ReviewReferralOperation.opName;

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
