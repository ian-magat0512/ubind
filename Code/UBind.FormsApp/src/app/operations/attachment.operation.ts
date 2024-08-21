import { Injectable } from '@angular/core';
import { Observable, Subject } from 'rxjs';
import { ApiService } from '../services/api.service';
import { ApplicationService } from '../services/application.service';
import { ConfigService } from '../services/config.service';
import { FormService } from '../services/form.service';
import { AttachmentService } from '../services/attachment.service';
import { OperationWithPayload } from './operation-with-payload';
import { MessageService } from '../services/message.service';
import { ErrorHandlerService } from '@app/services/error-handler.service';
import { FormTypeApplicationPropertiesResolver } from './operation-form-type-application-properties-resolver';
import { EventService } from '@app/services/event.service';
import { OperationRequestSettings } from './operation-request-settings';
import { map, mergeMap } from 'rxjs/operators';
import { OperationArguments } from './operation';

/**
 * Used when a file is attached in a webform and needs to be uploaded to the back end.
 */
@Injectable()
export class AttachmentOperation extends OperationWithPayload {
    public static opName: string = 'attachment';
    public operationName: string = AttachmentOperation.opName;
    protected enableQueuedExecution: boolean = true;
    protected defaultRequestSettings: OperationRequestSettings = {
        retryAttempts: 0,
        retryIntervalMillis: 3000,
        retryIntervalMultiplier: 1,
        timeoutRequests: false,
        defaultTimeoutMillis: 60000,
    };

    protected requiredApplicationProperties: Array<string>;

    public constructor(
        protected attachments: AttachmentService,
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

    public generateRequest(
        params: any,
        args: OperationArguments,
        operationId: number,
        abortSubject: Subject<any>,
    ): Observable<any> {
        return super.createPayload().pipe(
            map((payload: any) => Object.assign(payload, params.fileProperties)),
            mergeMap((payload: any) => this.post(
                'attachment', payload, args, operationId, this.defaultRequestSettings, params.cancelEmitter)));
    }

    protected createPayload(): Observable<any> {
        this.requiredApplicationProperties =
            this.formTypeApplicationPropertiesResolver.getApplicationPropertyNamesForFormType();
        return super.createPayload();
    }

    public getRequestParam(property: string): any {
        return this.applicationService[property];
    }

    public decrementExecutionQueueLength(): void {
        super.decrementExecutionQueueLength();
    }

    protected processError(err: any, operationId: any, requestType?: string): boolean {
        super.decrementExecutionQueueLength();
        if (requestType == 'post') {
            this.formService.resetActions();
        }
        return false;
    }

    public execute(
        requestParams: any = {},
        args: OperationArguments,
        operationId: number = Date.now(),
        abortSubject: Subject<any> = new Subject<any>(),
    ): Observable<any> {
        this.executionQueueLength++;
        return super.execute(requestParams, args, operationId, abortSubject);
    }
}
