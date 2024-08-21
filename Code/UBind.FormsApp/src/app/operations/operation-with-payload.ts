import { Observable, of, Subject } from 'rxjs';
import { Directive, EventEmitter, Output } from '@angular/core';
import { ApiService } from '@app/services/api.service';
import { ApplicationService } from '@app/services/application.service';
import { ConfigService } from '@app/services/config.service';
import { FormService } from '@app/services/form.service';
import { Operation, OperationArguments } from '@app/operations/operation';
import { MessageService } from '@app/services/message.service';
import { ErrorHandlerService } from '@app/services/error-handler.service';
import { WorkflowOperation } from '@app/models/workflow-operation.constant';
import { EventService } from '@app/services/event.service';
import { FormType } from '@app/models/form-type.enum';
import { QuoteType } from '@app/models/quote-type.enum';
import { PaymentData } from '@app/resource-models/payment-data';
import { mergeMap } from 'rxjs/operators';
import { OperationError } from './operation-error';

/**
 * Export operation with payload abstract class.
 * TODO: Write a better class header: operations with payload functions.
 */
@Directive()
export abstract class OperationWithPayload extends Operation {

    @Output() public nextResult: EventEmitter<any> = new EventEmitter<any>();

    protected includeFormModel: boolean = false;
    protected requiredApplicationProperties: Array<any> = [];
    protected includeApplicationProperties: Array<string> = [];
    protected includeWorkflowRoles: any = {};
    protected requiredWorkflowRoles: Array<any> = [];
    protected valuesWithIgnoredWorkflowRoles: any;

    private workflowCalculationOperations: Array<WorkflowOperation> = [
        WorkflowOperation.Policy,
        WorkflowOperation.Calculation,
        WorkflowOperation.Quote];
    private roleMap: any = {
        'customerName': 'fullName',
        'customerEmail': 'email',
        'customerPhone': 'homePhone',
        'customerMobile': 'mobilePhone',
        'customerWorkPhone': 'workPhone',
    };

    public constructor(
        protected formService: FormService,
        protected config: ConfigService,
        protected applicationService: ApplicationService,
        protected apiService: ApiService,
        protected messageService: MessageService,
        protected errorHandlerService: ErrorHandlerService,
        protected eventService: EventService,
    ) {
        super(
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
        return this.createPayload().pipe(mergeMap((payload: any) =>
            this.request('post', this.operationName, payload, args, operationId, abortSubject)));
    }

    // Gets the form model object from the payload that's been created previously.
    public getFormModelFromPayload(payload: object) {
        return payload['formDataJson']['formModel'];
    }

    protected processError(err: any, operationId: any, requestType?: string): boolean {
        if (requestType == 'post') {
            this.formService.resetActions();
        }
        return super.processError(err, operationId, requestType);
    }

    protected createPayload(): Observable<any> {
        let payload: any = {};
        for (let propertyName of this.requiredApplicationProperties) {
            if (this.applicationService[propertyName] == null || this.applicationService[propertyName] == '') {
                throw new Error(this.operationName
                    + ' operation failed: The application property \''
                    + propertyName + '\' is required in order for the operation to work '
                    + '(the property does not contain a value)');
            }
            if (this.applicationService[propertyName]) {
                payload[propertyName] = this.applicationService[propertyName];
            }
        }

        for (let propertyName of this.includeApplicationProperties) {
            if (this.applicationService[propertyName]) {
                payload[propertyName] = this.applicationService[propertyName];
            }
        }

        let formData: any = {};
        if (this.includeFormModel) {
            let formModel: any = this.formService.getValues(true, true, false, false);
            formData['formModel'] = formModel;
            payload['formDataJson'] = formData;
        }

        this.valuesWithIgnoredWorkflowRoles = this.formService.getValues(true, true, true);
        for (let role of this.requiredWorkflowRoles) {
            if (!this.config.dataLocators[role] && !this.config.workflowRoles[role]) {
                throw new OperationError('Missing formDataLocator', 'In order to work correctly, the \''
                    + this.operationName
                    + '\' operation requires a formDataLocator called \''
                    + role + '\'. Please add this formDataLocator entry to the product.json '
                    + 'file associated with the product configuration, '
                    + 'and ensure that it maps to a usable value in the form model.');
            }
        }

        for (let roleCategory in this.includeWorkflowRoles) {
            for (let role of this.includeWorkflowRoles[roleCategory]) {
                if (this.config.dataLocators[role] || this.config.workflowRoles[role]) {
                    let value: any =
                        this.formService.getValueForWorkflowRole(role, this.valuesWithIgnoredWorkflowRoles);
                    if (value != null && value != '') {
                        if (payload[roleCategory] == null) {
                            payload[roleCategory] = {};
                        }
                        let propertyName: any = this.roleMap[role] || role;
                        payload[roleCategory][propertyName] = value;
                    }
                }

                const isOperationIncluded: boolean = WorkflowOperation[this.operationName] &&
                    this.workflowCalculationOperations.includes(WorkflowOperation[this.operationName]);

                if (isOperationIncluded) {
                    const value: any =
                        this.formService.getValueForWorkflowRole(role, this.valuesWithIgnoredWorkflowRoles);
                    this.includePolicyAdjustmentFieldInFormData(formData,
                        roleCategory, role, value);
                    if (this.operationName == WorkflowOperation.Policy) {
                        payload['calculationResultId'] = this.applicationService.calculationResultId;
                    }
                }
            }
        }

        return of(payload);
    }

    private includePolicyAdjustmentFieldInFormData(
        formData: any,
        category: string,
        role: string,
        value: string): any {
        if (category == 'policyDetails') {
            if (value != '') {
                formData['formModel'][role] = value;
            }
        }
    }

    protected addQuoteTypeToPayload(payload: any): any {
        if (this.applicationService.formType == FormType.Quote) {
            payload['quoteType'] = Object.values(QuoteType).indexOf(this.applicationService.quoteType);
        }
        return payload;
    }

    /**
     * DEFT & Arteva's Odyssey API requires some info about the card to calculate surcharges.
     * This function adds this to the payload.
     * UPDATE: UB-11900 On arteva, we need the length of the card number. 
     * This is to determine the credit card type in the backend
     * @param payload 
     */
    protected addPaymentDataToPayload(
        payload: any,
        paymentCardBin: string,
        paymentCardNumberLength: number,
        singlePayment: boolean): any {
        if (paymentCardBin) {
            let paymentData: PaymentData = {
                cardBin: paymentCardBin,
                cardNumberLength: paymentCardNumberLength,
                singlePayment,
            };
            payload.paymentData = paymentData;
        }
        return payload;
    }
}
