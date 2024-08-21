import { Injectable, Directive } from '@angular/core';
import { Observable, Subject } from 'rxjs';

import { ApiService } from '../services/api.service';
import { ApplicationService } from '../services/application.service';
import { ErrorHandlerService } from '@app/services/error-handler.service';

import { Operation, OperationArguments } from './operation';
import { MessageService } from '../services/message.service';
import { ProblemDetails } from '../models/problem-details';
import { EventService } from '@app/services/event.service';
import { HttpResponse } from '@angular/common/http';

/**
 * Export configuration operation class.
 * TODO: Write a better class header: configuration operations functions.
 */
@Directive()
@Injectable()

export class ConfigurationOperation extends Operation {
    public operationName: string = 'configuration';

    public constructor(
        protected applicationService: ApplicationService,
        protected apiService: ApiService,
        protected messageService: MessageService,
        protected errorHandlerService: ErrorHandlerService,
        protected eventService: EventService,
    ) {
        super(applicationService, apiService, messageService, errorHandlerService, eventService);
        this.publishAppEvents = false;
    }

    public generateRequest(
        requestParams: any,
        args: OperationArguments,
        operationId: number,
        abortSubject: Subject<any>,
    ): Observable<any> {

        const environment: string = this.applicationService.environment;
        const customRequestSettings: any = (environment != 'development')
            ? {
                'simultaneousRedundantRequests': 1,
                'retryAttempts': 0,
                'retryIntervalMillis': 5000,
                'retryIntervalMultiplier': 1.5,
            }
            : {
                'simultaneousRedundantRequests': 1,
                'retryAttempts': 0,
                'retryIntervalMillis': 5000,
                'retryIntervalMultiplier': 0,
            };

        let path: string = this.operationName;
        return this.get(path, requestParams, args, operationId, customRequestSettings);
    }

    protected processResponse(data: any, operationId: any, requestPayload: any, params: any): any {
        if (data instanceof HttpResponse) {
            const productReleaseId: string = data.headers.get('X-Product-Release-Id');
            if (productReleaseId != null) {
                this.applicationService.productReleaseId = productReleaseId;
            }
        }
        let response: any = super.processResponse(data, operationId, requestPayload, params);
        return (response);
    }

    protected processError(err: any, operationId: any): boolean {
        let message: string = "There was a problem loading the form configuration. " +
            "Please get in touch with customer support.";
        if (ProblemDetails.isProblemDetailsResponse(err)) {
            const errorBody: any = err.error;
            if (errorBody) {
                let problemDetails: ProblemDetails = ProblemDetails.fromJson(errorBody);
                message = problemDetails.detail;
            }
        } else if (err.error && err.error.error && err.error.error.message) {
            message = message + ' ' + err.error.error.message;
        } else if (err.status >= 500 && err.status <= 599) {
            message = `The server reported an error during the handling of your request. ` +
                `The issue has been logged and a notification has been sent to our support team. ` +
                `In the mean time, please try again, and if you're still having issues, ` +
                `please don't hesitate to contact us for assistance.`;
        }

        let payload: any = {
            'message': message,
            'severity': 3,
        };
        this.messageService.sendMessage('displayMessage', payload);
        return false;
    }
}
