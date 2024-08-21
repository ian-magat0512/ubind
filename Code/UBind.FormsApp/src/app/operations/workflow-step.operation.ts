import { Injectable } from '@angular/core';
import { ApiService } from '../services/api.service';
import { ApplicationService } from '../services/application.service';
import { ConfigService } from '../services/config.service';
import { FormService } from '../services/form.service';
import { OperationWithPayload } from './operation-with-payload';
import { MessageService } from '../services/message.service';
import { ErrorHandlerService } from '@app/services/error-handler.service';
import { FormType } from '../models/form-type.enum';
import { BroadcastService } from '@app/services/broadcast.service';
import { ProductFeatureHelper } from '@app/helpers/product-feature.helper';
import { WorkflowStatusService } from '@app/services/workflow-status.service';
import { EventService } from '@app/services/event.service';
import { Observable, of } from 'rxjs';

/**
 * Let's the back end know the workflow step has changed so it can record it.
 */
@Injectable()
export class WorkflowStepOperation extends OperationWithPayload {
    public static opName: string = 'workflowStep';
    public operationName: string = WorkflowStepOperation.opName;

    protected includeFormModel: boolean = true;
    protected requiredApplicationProperties: Array<any> = [
        'quoteId',
        'claimId',
    ];

    public constructor(
        protected formService: FormService,
        protected config: ConfigService,
        protected applicationService: ApplicationService,
        protected apiService: ApiService,
        protected messageService: MessageService,
        protected errorHandlerService: ErrorHandlerService,
        protected broadcast: BroadcastService,
        protected workflowStatusService: WorkflowStatusService,
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
        const payload: any = {};
        payload['workflowStep'] = this.workflowStatusService.targetDestination.stepName;
        if (this.applicationService.formType == FormType.Quote) {
            if (this.applicationService.quoteId) {
                payload['quoteId'] = this.applicationService.quoteId;
            }
        } else if (this.applicationService.formType == FormType.Claim) {
            if (this.applicationService.claimId) {
                payload['claimId'] = this.applicationService.claimId;
            }
        } else {
            throw new Error("When creating a payload for the workflow step operation, " +
                "we could not deterimine whether this is for a quote or a claim.");
        }

        return of(payload);
    }

    protected processError(err: any, operationId: any, requestType?: string): boolean {
        if (ProductFeatureHelper.isProductFeatureDisabled(err.error.code)) {
            this.broadcast.broadcast('TransactionDisabledError', {});
        }
        return super.processError(err, operationId, requestType);
    }
}
