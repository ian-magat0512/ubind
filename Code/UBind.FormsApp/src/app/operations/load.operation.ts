import { Injectable, Directive } from '@angular/core';
import { ProblemDetails } from '../models/problem-details';
import { ApiService } from '../services/api.service';
import { ApplicationService } from '../services/application.service';
import { ErrorHandlerService } from '@app/services/error-handler.service';
import { Operation } from './operation';
import { MessageService } from '../services/message.service';
import { UnifiedFormModelService } from '@app/services/unified-form-model.service';
import { ConfigService } from '@app/services/config.service';
import { filter, take } from 'rxjs/operators';
import { ProductFeatureHelper } from '@app/helpers/product-feature.helper';
import { WorkflowRole } from '@app/models/workflow-role.enum';
import { QuoteState } from '@app/models/quote-state.enum';
import { EventService } from '@app/services/event.service';

/**
 * Export load operation class.
 * TODO: Write a better class header: loading operations functions.
 */
@Directive()
@Injectable()
export class LoadOperation extends Operation {
    public operationName: string = 'load';

    protected includeWorkflowRoles: any = {
        'loadDetails': [
            WorkflowRole.LoadQuoteId,
        ],
    };
    protected requiredWorkflowRoles: Array<string> = [
        WorkflowRole.LoadQuoteId,
    ];

    public constructor(
        protected unifiedFormModelService: UnifiedFormModelService,
        protected applicationService: ApplicationService,
        protected apiService: ApiService,
        protected messageService: MessageService,
        protected errorHandlerService: ErrorHandlerService,
        private configService: ConfigService,
        protected eventService: EventService,
    ) {
        super(applicationService, apiService, messageService, errorHandlerService, eventService);
    }

    protected processResponse(data: any, operationId: any, requestPayload: any, params: any): any {
        const response: any = super.processResponse(data, operationId, requestPayload, params);
        if (response['formModel'] != null) {
            this.unifiedFormModelService.apply(response['formModel']);
        }
        if (response['calculationResult']) {
            // don't try processing the calculation response until the config is ready.
            // This is needed since on slow connections, the configuration loading can take longer
            // than the form model loading, and both http requests fire sumultaneously
            this.configService.configurationReadySubject.pipe(
                filter((ready: boolean) => ready),
                take(1),
            ).subscribe((ready: boolean) => {
                this.eventService.calculationResponseSubject.next(response);
            });
        }
        if (response['currentUser']) {
            this.applicationService.currentUser = response['currentUser'];
        }
        return (response);
    }

    protected processError(err: any, operationId: any): boolean {
        if (ProblemDetails.isProblemDetailsResponse(err)) {
            const problemDetails: ProblemDetails = ProblemDetails.fromJson(err.error);
            if (problemDetails.code == 'quote.cannot.load.expired.quote') {
                this.applicationService.quoteState = QuoteState.Expired;
                return true;
            }
        }
        const response: any = super.processError(err, operationId);
        if (response.status === 400 || response.status === 401 || response.status === 403) {
            const errorDetail: any = err && err.error ? err.error.error : '';
            if (errorDetail) {
                this.errorHandlerService.handleError(ProblemDetails.fromJson(errorDetail));
            }
            const isNewBusinessDisabled: boolean = ProductFeatureHelper.isNewBusinessQuoteDisabled(err.error.code);
            const isAdjustmentDisabled: boolean = ProductFeatureHelper.isAdjustmentQuoteDisabled(err.error.code);
            const isRenewalDisabled: boolean = ProductFeatureHelper.isRenewalQuoteDisabled(err.error.code);
            const isCancellationDisabled: boolean = ProductFeatureHelper.isCancellationQuoteDisabled(err.error.code);

            if (ProductFeatureHelper.isProductFeatureDisabled(err.error.code)) {
                const transactionType: string = isNewBusinessDisabled ? 'Purchase' : isAdjustmentDisabled ?
                    'Adjustment' : isRenewalDisabled ? 'Renewal' : isCancellationDisabled ? 'Cancellation' : '';

                const operation: string = isNewBusinessDisabled ? 'edit' : isAdjustmentDisabled ? 'adjust'
                    : isRenewalDisabled ? 'renew' : isCancellationDisabled ? 'cancel' : '';
                const documentType: string = isNewBusinessDisabled ? 'quotes' : 'policy';
                const message: string = `Because ${transactionType} transactions have been disabled for this product, `
                    + `you cannot currently ${operation} your ${err.error.data.productName} ${documentType} online. `
                    + `We apologise for the inconvenience. If you believe this is a mistake, `
                    + `or you would like assistance, please contact customer support.`;
                let payload: any = {
                    'message': message,
                    'severity': 3,
                };

                this.messageService.sendMessage('displayMessage', payload);
                return true;
            }
        }
        return false;
    }
}
