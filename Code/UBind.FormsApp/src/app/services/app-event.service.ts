import { Injectable } from '@angular/core';
import { FormService } from './form.service';
import { MessageService } from './message.service';
import { ApplicationService } from './application.service';
import { FormType } from '@app/models/form-type.enum';
import { QuoteResultProcessor } from '@app/services/quote-result-processor';
import { ClaimResultProcessor } from '@app/services/claim-result-processor';
import { CalculationResult } from '@app/models/calculation-result';
import { EventService } from './event.service';

/**
 * This class is used for sending messages through to the UBind injector, which then passes
 * them through to Google Tag Manager for analytics purposes.
 */
@Injectable()
export class AppEventService {

    public constructor(
        private formService: FormService,
        private applicationService: ApplicationService,
        private messageService: MessageService,
        eventService: EventService,
    ) {
        eventService.appEventSubject.subscribe((eventType: string) => this.createEvent(eventType, {}));
    }

    public createEvent(
        eventType: string,
        data: any,
        formModel: object = null,
        calculationResult: CalculationResult = null,
    ): void {
        if (!formModel) {
            formModel = this.formService.getValues();
        }
        if (!calculationResult) {
            if (this.applicationService.formType == FormType.Quote) {
                calculationResult = this.applicationService.latestQuoteResult || QuoteResultProcessor.createEmpty();
            } else if (this.applicationService.formType == FormType.Claim) {
                calculationResult = this.applicationService.latestClaimResult || ClaimResultProcessor.createEmpty();
            }
        }
        data = data || {};
        let payload: any = {
            'eventType': eventType,
            'eventData': data,
            'formModel': formModel,
            'calculation': calculationResult,
        };
        this.messageService.sendMessage('appEvent', payload);
    }
}
