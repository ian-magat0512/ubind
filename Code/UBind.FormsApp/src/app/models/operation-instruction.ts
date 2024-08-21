import { ActualiseOperation } from "@app/operations/actualise.operation";
import { CalculationOperation } from "@app/operations/calculation.operation";
import { ClaimVersionOperation } from "@app/operations/claim-version.operation";
import { CustomerUserOperation } from "@app/operations/customer-user.operation";
import { CustomerOperation } from "@app/operations/customer.operation";
import { FormUpdateOperation } from "@app/operations/form-update.operation";
import { GetCustomerOperation } from "@app/operations/get-customer.operation";
import { QuoteVersionOperation } from "@app/operations/quote-version.operation";
import { WorkflowStepOperation } from "@app/operations/workflow-step.operation";
import { Subject } from "rxjs";
import { OperationConfiguration } from "./configuration/operation-configuration";
import { OperationInstructionStatus } from "./operation-instruction-status.enum";
import _ from "lodash";
import { BindOperation } from "@app/operations/bind.operation";
import { SubmissionOperation } from "@app/operations/submission.operation";
import { InvoiceOperation } from "@app/operations/invoice.operation";
import { CreditCardPaymentOperation } from "@app/operations/credit-card-payment.operation";
import { AcknowledgeOperation } from "@app/operations/acknowledge.operation";
import { AssessmentApprovalOperation } from "@app/operations/assessment-approval.operation";
import { AssessmentReferralOperation } from "@app/operations/assessment-referral.operation";
import { AutoApprovalOperation } from "@app/operations/auto-approve.operation";
import { DeclineOperation } from "@app/operations/decline.operation";
import { EndorsementApprovalOperation } from "@app/operations/endorsement-approval.operation";
import { EndorsementReferralOperation } from "@app/operations/endorsement-referral.operation";
import { EnquiryOperation } from "@app/operations/enquiry.operation";
import { NotifyOperation } from "@app/operations/notify.operation";
import { PremiumFundingProposalAndAcceptanceOperation } from "@app/operations/premium-funding.operation";
import { ReturnOperation } from "@app/operations/return.operation";
import { ReviewApprovalOperation } from "@app/operations/review-approval.operation";
import { ReviewReferralOperation } from "@app/operations/review-referral.operation";
import { SaveOperation } from "@app/operations/save.operation";
import { SettleOperation } from "@app/operations/settle.operation";
import { StripePaymentOperation } from "@app/operations/stripe-payment.operation";
import { WithdrawOperation } from "@app/operations/withdraw.operation";
import { OperationArguments } from "@app/operations/operation";


/**
 * Represents an instruction for an operation to be executed in a given manner.
 */
export class OperationInstruction {

    public name: string;
    public destinationStepName: string;
    public backgroundExecution: boolean;
    public params: any;
    public args: OperationArguments;
    public status: OperationInstructionStatus = OperationInstructionStatus.NotStarted;
    public id: string;

    /** 
     * for debugging
     */
    public startTime: number;
    public endTime: number;

    /**
     * The string value in completed subject will hold the reason for completion, for
     * debugging purposes.
     */
    public completedSubject: Subject<string> = new Subject<string>();

    /**
     * This will be published to when the operation should be aborted.
     */
    public abortSubject: Subject<any> = new Subject<any>();

    public constructor(
        operationConfig: string | OperationConfiguration,
        destinationStepName?: string,
    ) {
        this.id = _.uniqueId();
        if (typeof(operationConfig) === 'string' || operationConfig instanceof String) {
            this.name = <string>operationConfig;
            this.backgroundExecution = OperationInstruction.getDefaultAsyncMode(this.name);
        } else {
            this.name = operationConfig.name;
            this.backgroundExecution = operationConfig.backgroundExecution === undefined
                ? OperationInstruction.getDefaultAsyncMode(this.name)
                : operationConfig.backgroundExecution;
            this.params = operationConfig.params;
        }
        this.destinationStepName = destinationStepName;
    }

    private static getDefaultAsyncMode(operationName: string): boolean {
        return OperationInstruction.getDefaultAsyncOperationNames().includes(operationName);
    }

    /**
     * 
     * @returns a list of operation names that should be async by default if not specified
     */
    public static getDefaultAsyncOperationNames(): Array<string> {
        return [
            CalculationOperation.opName,
            FormUpdateOperation.opName,
            CustomerOperation.opName,
            ActualiseOperation.opName,
            ClaimVersionOperation.opName,
            CustomerUserOperation.opName,
            GetCustomerOperation.opName,
            QuoteVersionOperation.opName,
            WorkflowStepOperation.opName,
        ];
    }

    public isCritical(): boolean {
        return OperationInstruction.getCriticalOperationNames().includes(this.name);
    }

    public shouldAbortExistingOperations(): boolean {
        return this.name == CalculationOperation.opName || this.name == FormUpdateOperation.opName;
    }

    /**
     * 
     * @returns a list of operation names that require any pending formupdate or calculation results 
     * to have been completed.
     */
    public static getCriticalOperationNames(): Array<string> {
        return [
            BindOperation.opName,
            SubmissionOperation.opName,
            InvoiceOperation.opName,
            CreditCardPaymentOperation.opName,
            AcknowledgeOperation.opName,
            AssessmentApprovalOperation.opName,
            AssessmentReferralOperation.opName,
            AutoApprovalOperation.opName,
            ClaimVersionOperation.opName,
            CustomerUserOperation.opName,
            DeclineOperation.opName,
            EndorsementApprovalOperation.opName,
            EndorsementReferralOperation.opName,
            EnquiryOperation.opName,
            NotifyOperation.opName,
            PremiumFundingProposalAndAcceptanceOperation.opName,
            QuoteVersionOperation.opName,
            ReturnOperation.opName,
            ReviewApprovalOperation.opName,
            ReviewReferralOperation.opName,
            SaveOperation.opName,
            SettleOperation.opName,
            StripePaymentOperation.opName,
            WithdrawOperation.opName,
        ];
    }

    /**
     * @returns true if this operation is a calculation, formUpdate or auto-approve operation,
     */
    public isBlockingOperation(): boolean {
        return OperationInstruction.getBlockingOperationNames().includes(this.name);
    }

    public static getBlockingOperationNames(): Array<string> {
        return [
            CalculationOperation.opName,
            FormUpdateOperation.opName,
            AutoApprovalOperation.opName,
        ];
    }

    public getDebugInfo(): string {
        let timeDiff: number = this.endTime - this.startTime;
        return `Operation: ${this.name}  Time: ${timeDiff} ms`;
    }
}
