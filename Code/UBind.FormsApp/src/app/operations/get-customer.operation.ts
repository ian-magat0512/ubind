import { Injectable } from '@angular/core';
import { ApiService } from '@app/services/api.service';
import { ApplicationService } from '@app/services/application.service';
import { ConfigService } from '@app/services/config.service';
import { FormService } from '@app/services/form.service';
import { MessageService } from '@app/services/message.service';
import { ErrorHandlerService } from '@app/services/error-handler.service';
import { Observable, Subject } from 'rxjs';
import { mergeMap } from 'rxjs/operators';
import { OperationWithPayload } from './operation-with-payload';
import { UserService } from '../services/user.service';
import { EventService } from '@app/services/event.service';
import { OperationArguments } from './operation';

/**
 * Gets the customer associated with the quote so it's customer ID is available in the web form,
 * and so it knows whether that customer has a user account or not.
 * If it does then we don't need to create a user account or ask them to create one.
 */
@Injectable()
export class GetCustomerOperation extends OperationWithPayload {
    public static opName: string = 'getCustomer';
    public operationName: string = GetCustomerOperation.opName;
    protected includeFormModel: boolean = true;
    protected requiredApplicationProperties: Array<string> = [
        'quoteId',
    ];

    public constructor(
        protected formService: FormService,
        protected config: ConfigService,
        protected applicationService: ApplicationService,
        protected apiService: ApiService,
        protected messageService: MessageService,
        protected userService: UserService,
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
        let customRequestSettings: any = {
            'retryAttempts': 0,
            'retryIntervalMillis': 15000,
            'followUpAttempts': 1,
        };
        return super.createPayload().pipe(
            mergeMap((payload: any) => this.get(
                `quote/${payload.quoteId}/customer`, null, args, operationId, customRequestSettings)),
        );
    }

    protected processResponse(data: any, operationId: any, requestPayload: any, args: OperationArguments): any {
        const response: any = super.processResponse(data, operationId, requestPayload, args);
        if (response) {
            this.applicationService.customerId = response.customerId;
            this.userService.isLoadedCustomerHasUser = response.customerHasAccount || false;
            this.applicationService.userHasAccount
                = this.userService.isLoadedCustomerHasUser ? true : this.applicationService.userHasAccount;
        }
        return response;
    }
}
