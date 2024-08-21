import { Injectable } from '@angular/core';
import { ApiService } from '../services/api.service';
import { ApplicationService } from '../services/application.service';
import { ConfigService } from '../services/config.service';
import { FormService } from '../services/form.service';
import { OperationWithPayload } from './operation-with-payload';
import { MessageService } from '../services/message.service';
import { ErrorHandlerService } from '@app/services/error-handler.service';
import { of as observableOf, Observable, Subject } from 'rxjs';
import { CalculationService } from '@app/services/calculation.service';
import { UnifiedFormModelService } from '@app/services/unified-form-model.service';
import { EventService } from '@app/services/event.service';
import { OperationArguments } from './operation';

/**
 * Copies the data from an expired quote to create a new quote.
 */
@Injectable()
export class CopyExpiredQuoteOperation extends OperationWithPayload {
    public static opName: string = 'copyExpiredQuote';
    public operationName: string = CopyExpiredQuoteOperation.opName;
    private requestParams: any;
    protected includeFormModel: boolean = true;
    protected requiredApplicationProperties: Array<any> = [];
    protected includeWorkflowRoles: any = {};

    public constructor(
        protected unifiedFormModelService: UnifiedFormModelService,
        protected formService: FormService,
        protected config: ConfigService,
        protected applicationService: ApplicationService,
        protected apiService: ApiService,
        protected calculationService: CalculationService,
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

    public generateRequest(
        requestParams: any,
        args: OperationArguments,
        operationId: number,
        abortSubject: Subject<any>,
    ): Observable<any> {
        this.requestParams = requestParams;
        return super.generateRequest(requestParams, args, operationId, abortSubject);
    }

    protected createPayload(): Observable<any> {
        return observableOf(this.requestParams);
    }

    protected processResponse(data: any, operationId: any, requestPayload: any, args: OperationArguments): any {
        const response: any = super.processResponse(data, operationId, requestPayload, args);
        if (response['formModel'] != null) {
            this.unifiedFormModelService.apply(response['formModel']);
        }
        if (response['calculationResult']) {
            this.eventService.calculationResponseSubject.next(response);
        }
        if (response['currentUser']) {
            this.applicationService.currentUser = response['currentUser'];
        }
        return (response);
    }
}
