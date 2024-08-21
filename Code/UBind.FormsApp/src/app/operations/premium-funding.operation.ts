import { Injectable } from '@angular/core';
import { ApiService } from '../services/api.service';
import { ConfigService } from '../services/config.service';
import { ApplicationService } from '../services/application.service';
import { FormService } from '../services/form.service';
import { CalculationService } from '../services/calculation.service';
import { OperationWithPayload } from './operation-with-payload';
import { MessageService } from '../services/message.service';
import { ErrorHandlerService } from '@app/services/error-handler.service';
import { WorkflowRole } from '@app/models/workflow-role.enum';
import { EventService } from '@app/services/event.service';
import { Observable } from 'rxjs';

/**
 * Accepts a premium funding proposal, which allows someone to finalise their contract
 * to pay monthly for their insurance using the configured premium funding provider.
 */
@Injectable()
export class PremiumFundingProposalAndAcceptanceOperation extends OperationWithPayload {
    public static opName: string = 'premiumFunding';
    public operationName: string = PremiumFundingProposalAndAcceptanceOperation.opName;

    protected includeFormModel: boolean = true;
    protected requiredApplicationProperties: Array<string> = [
        'quoteId',
        'calculationResultId',
        'premiumFundingProposalId',
    ];
    protected requiredWorkflowRoles: Array<string> = [
        'paymentMethod',
    ];

    public constructor(
        protected formService: FormService,
        protected config: ConfigService,
        protected calculation: CalculationService,
        protected application: ApplicationService,
        protected apiService: ApiService,
        protected messageService: MessageService,
        protected errorHandlerService: ErrorHandlerService,
        protected eventService: EventService,
    ) {
        super(
            formService,
            config,
            application,
            apiService,
            messageService,
            errorHandlerService,
            eventService);
    }

    protected createPayload(): Observable<any> {
        const valuesWithIgnoredWorkflowRoles: any = this.formService.getValues(true, true, true);
        let paymentMethod: any = this.formService.getValueForWorkflowRole(
            WorkflowRole.PaymentMethod,
            valuesWithIgnoredWorkflowRoles);
        if (paymentMethod == null || paymentMethod == '') {
            throw new Error(this.operationName + ' operation failed: The workflow role \'paymentMethod\' is required ' +
                'in order for the operation to work (the associated field does not contain ' +
                'a value - make it a required field)');
        }
        if (paymentMethod == 'Direct Debit') {
            this.requiredWorkflowRoles = [
                WorkflowRole.BankAccountName,
                WorkflowRole.BankAccountBsb,
                WorkflowRole.BankAccountNumber,
            ];
            this.includeWorkflowRoles = {
                'bankAccountDetails': [
                    WorkflowRole.BankAccountName,
                    WorkflowRole.BankAccountBsb,
                    WorkflowRole.BankAccountNumber,
                ],
            };
        } else {
            this.requiredWorkflowRoles = [
                WorkflowRole.CreditCardNumber,
                WorkflowRole.CreditCardName,
                WorkflowRole.CreditCardExpiry,
                WorkflowRole.CreditCardCcv,
            ];
            this.includeWorkflowRoles = {
                'creditCardDetails': [
                    WorkflowRole.CreditCardNumber,
                    WorkflowRole.CreditCardName,
                    WorkflowRole.CreditCardExpiry,
                    WorkflowRole.CreditCardCcv,
                ],
            };
        }

        return super.createPayload();
    }
}
