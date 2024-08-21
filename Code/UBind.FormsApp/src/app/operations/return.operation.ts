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
 * Returns a quote to the customer (usually with a message asking for further information).
 * This is done by a broker or underwriter who is reviewing the quote.
 * It can also be done after a quote is approve and the customer wants to make changes,
 * typically by clicking an "edit" button.
 */
@Injectable()
export class ReturnOperation extends OperationWithPayload {
    public static opName: string = 'return';
    public operationName: string = ReturnOperation.opName;

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

    protected processResponse(data: any, operationId: any, requestPayload: any, params: any): any {
        const response: any = super.processResponse(data, operationId, requestPayload, params);
        if (response['quoteState']) {
            this.applicationService.latestQuoteResult.quoteState = response['quoteState'];
            this.applicationService.quoteState = this.applicationService.latestQuoteResult.quoteState;
            this.eventService.quoteResultSubject.next(this.applicationService.latestQuoteResult);
            this.applicationService.calculationResultSubject.next(this.applicationService.latestQuoteResult);
        }
        return (response);
    }
}
