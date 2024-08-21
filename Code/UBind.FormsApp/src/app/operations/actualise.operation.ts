import { Injectable } from '@angular/core';
import { OperationWithPayload } from './operation-with-payload';

import { ApiService } from '../services/api.service';
import { ApplicationService } from '../services/application.service';
import { FormService } from '../services/form.service';
import { ConfigService } from '../services/config.service';
import { MessageService } from '../services/message.service';
import { ErrorHandlerService } from '@app/services/error-handler.service';
import { ResumeApplicationService } from '../services/resume-application.service';
import { FormType } from '../models/form-type.enum';
import { FormTypeApplicationPropertiesResolver } from './operation-form-type-application-properties-resolver';
import { EventService } from '@app/services/event.service';
import { Observable } from 'rxjs';

/**
 * The actualise operation moves a quote from nascent status to incomplete status,
 * which then makes it visible in the list of incomplete quotes in the portal.
 * Nascent quotes are not normally visible.
 */
@Injectable()
export class ActualiseOperation extends OperationWithPayload {
    public static opName: string = 'actualise';
    public operationName: string = ActualiseOperation.opName;
    protected includeFormModel: boolean = true;
    protected requiredApplicationProperties: Array<string>;

    public constructor(
        protected form: FormService,
        protected config: ConfigService,
        protected application: ApplicationService,
        protected apiService: ApiService,
        protected message: MessageService,
        protected errorHandlerService: ErrorHandlerService,
        private resumeApplicationService: ResumeApplicationService,
        private formTypeApplicationPropertiesResolver: FormTypeApplicationPropertiesResolver,
        protected eventService: EventService,
    ) {
        super(form, config, application, apiService, message, errorHandlerService, eventService);
    }

    protected createPayload(): Observable<any> {
        this.requiredApplicationProperties =
            this.formTypeApplicationPropertiesResolver.getApplicationPropertyNamesForFormType();
        return super.createPayload();
    }

    protected processResponse(data: any, operationId: any, requestPayload: any, params: any): any {
        if (this.application.formType == FormType.Quote) {
            this.resumeApplicationService.saveQuoteIdForLater(data.body.quoteId, 30);
        }

        if (data.body?.quoteReference) {
            this.messageService.sendMessage(
                "quoteReferenceAllocated", data.body.quoteReference);
        }

        if(data.body?.claimReference) {
            this.messageService.sendMessage(
                "claimReferenceAllocated", data.body.claimReference);
        }

        const response: any = super.processResponse(data, operationId, requestPayload, params);
        if (response['quoteState']) {
            this.applicationService.quoteState = response['quoteState'];
            if (this.applicationService.latestQuoteResult) {
                this.applicationService.latestQuoteResult.quoteState = response['quoteState'];
                this.eventService.quoteResultSubject.next(this.applicationService.latestQuoteResult);
                this.applicationService.calculationResultSubject.next(this.applicationService.latestQuoteResult);
            }
        }
        return response;
    }
}
