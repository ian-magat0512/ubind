
import { Injectable } from '@angular/core';
import { LoadOperation } from './load.operation';
import { BindOperation } from './bind.operation';
import { CalculationOperation } from './calculation.operation';
import { ConfigurationOperation } from './configuration.operation';
import { EnquiryOperation } from './enquiry.operation';
import { FormUpdateOperation } from './form-update.operation';
import { CreditCardPaymentOperation } from './credit-card-payment.operation';
import { PremiumFundingProposalAndAcceptanceOperation } from './premium-funding.operation';
import { StripePaymentOperation } from './stripe-payment.operation';
import { SaveOperation } from './save.operation';
import { SubmissionOperation } from './submission.operation';
import { PolicyOperation } from './policy.operation';
import { CustomerOperation } from './customer.operation';
import { QuoteVersionOperation } from './quote-version.operation';
import { InvoiceOperation } from './invoice.operation';
import { AttachmentOperation } from './attachment.operation';
import { WorkflowStepOperation } from './workflow-step.operation';
import { CustomerUserOperation } from './customer-user.operation';
import { AutoApprovalOperation } from './auto-approve.operation';
import { ReviewApprovalOperation } from './review-approval.operation';
import { DeclineOperation } from './decline.operation';
import { ReviewReferralOperation } from './review-referral.operation';
import { ReturnOperation } from './return.operation';
import { EndorsementApprovalOperation } from './endorsement-approval.operation';
import { EndorsementReferralOperation } from './endorsement-referral.operation';
import { GetCustomerOperation } from '@app/operations/get-customer.operation';
import { NotifyOperation } from './notify.operation';
import { SettleOperation } from './settle.operation';
import { AcknowledgeOperation } from './acknowledge.operation';
import { AssessmentApprovalOperation } from './assessment-approval.operation';
import { AssessmentReferralOperation } from './assessment-referral.operation';
import { ActualiseOperation } from './actualise.operation';
import { WithdrawOperation } from './withdraw.operation';
import { ClaimVersionOperation } from "./claim-version.operation";
import { CopyExpiredQuoteOperation } from './copy-expired-quote.operation';
import { Operation } from '@app/operations/operation';
import { ApplicationService } from '../services/application.service';
import { Errors } from '@app/models/errors';

/**
 * A factory for instantiating operation instances from an operation name
 */
@Injectable()
export class OperationFactory {

    public operations: Map<string, Operation> = new Map<string, Operation>();

    public constructor(
        private applicationService: ApplicationService,
        loadOperation: LoadOperation,
        bindOperation: BindOperation,
        calculationOperation: CalculationOperation,
        configurationOperation: ConfigurationOperation,
        enquiryOperation: EnquiryOperation,
        formUpdateOperation: FormUpdateOperation,
        copyExpiredQuoteOperation: CopyExpiredQuoteOperation,
        creditCardPaymentOperation: CreditCardPaymentOperation,
        premiumFundingOperation: PremiumFundingProposalAndAcceptanceOperation,
        stripePaymentOperation: StripePaymentOperation,
        saveOperation: SaveOperation,
        submissionOperation: SubmissionOperation,
        customerOperation: CustomerOperation,
        quoteVersionOperation: QuoteVersionOperation,
        policyOperation: PolicyOperation,
        invoiceOperation: InvoiceOperation,
        attachmentOperation: AttachmentOperation,
        workflowStepOperation: WorkflowStepOperation,
        customerUserOperation: CustomerUserOperation,
        getCustomerOperation: GetCustomerOperation,
        autoApprovalOperation: AutoApprovalOperation,
        declineOperation: DeclineOperation,
        endorsementApprovalOperation: EndorsementApprovalOperation,
        endorsementReferralOperation: EndorsementReferralOperation,
        returnOperation: ReturnOperation,
        reviewApprovalOperation: ReviewApprovalOperation,
        reviewReferralOperation: ReviewReferralOperation,
        acknowledgeOperation: AcknowledgeOperation,
        assessmentApprovalOperation: AssessmentApprovalOperation,
        assessmentReferralOperation: AssessmentReferralOperation,
        actualiseOperation: ActualiseOperation,
        notifyOperation: NotifyOperation,
        settleOperation: SettleOperation,
        withdrawOperation: WithdrawOperation,
        claimVersionOperation: ClaimVersionOperation,
    ) {
        this.operations.set('load', loadOperation);
        this.operations.set('bind', bindOperation);
        this.operations.set('calculation', calculationOperation);
        this.operations.set('configuration', configurationOperation);
        this.operations.set('enquiry', enquiryOperation);
        this.operations.set('copyExpiredQuote', copyExpiredQuoteOperation);
        this.operations.set('formUpdate', formUpdateOperation);
        this.operations.set('creditCardPayment', creditCardPaymentOperation);
        this.operations.set('premiumFunding', premiumFundingOperation);
        this.operations.set('stripePayment', stripePaymentOperation);
        this.operations.set('save', saveOperation);
        this.operations.set('submission', submissionOperation);
        this.operations.set('customer', customerOperation);
        // Backward compatibility. Ensure 'quote' operation uses new actualise operation
        this.operations.set('quote', actualiseOperation);
        this.operations.set('actualise', actualiseOperation);
        this.operations.set('quoteVersion', quoteVersionOperation);
        this.operations.set('policy', policyOperation);
        this.operations.set('invoice', invoiceOperation);
        this.operations.set('attachment', attachmentOperation);
        this.operations.set('workflowStep', workflowStepOperation);
        this.operations.set('customerUser', customerUserOperation);
        this.operations.set('getCustomer', getCustomerOperation);
        this.operations.set('autoApproval', autoApprovalOperation);
        this.operations.set('decline', declineOperation);
        this.operations.set('endorsementApproval', endorsementApprovalOperation);
        this.operations.set('endorsementReferral', endorsementReferralOperation);
        this.operations.set('return', returnOperation);
        this.operations.set('reviewApproval', reviewApprovalOperation);
        this.operations.set('reviewReferral', reviewReferralOperation);
        this.operations.set('acknowledge', acknowledgeOperation);
        this.operations.set('assessmentApproval', assessmentApprovalOperation);
        this.operations.set('assessmentReferral', assessmentReferralOperation);
        this.operations.set('notify', notifyOperation);
        this.operations.set('withdraw', withdrawOperation);
        this.operations.set('settle', settleOperation);
        this.operations.set('claimVersion', claimVersionOperation);

        this.operations.forEach((operation: Operation) => {
            operation.nextResult.subscribe((data: any) => {
                this.applicationService.operationResultSubject.next(data);
            });
        });
    }

    public create(operationName: string): Operation {
        let operation: Operation = this.operations.get(operationName);
        if (operation == null) {
            throw Errors.Product.Configuration(
                `There is no "${operationName}" operation. Please check the documentation.`);
        }
        return operation;
    }
}
