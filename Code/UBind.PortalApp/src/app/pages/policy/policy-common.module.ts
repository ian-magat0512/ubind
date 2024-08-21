import { NgModule } from '@angular/core';
import { ListModule } from '@app/list.module';
import { SharedModule } from '@app/shared.module';
import { SharedComponentsModule } from '@app/components/shared-components.module';
import { ListPolicyTransactionPage } from './list-policy-transaction/list-policy-transaction.page';
import { DetailPolicyTransactionPage } from './detail-policy-transaction/detail-policy-transaction.page';
import { DetailPolicyPage } from './detail-policy/detail-policy.page';
import { UpdatePolicyPage } from './update-policy/update-policy.page';
import { ListPolicyMessagePage } from './list-policy-message/list-policy-message.page';
import { ListPolicyTransactionMessagePage }
    from './list-policy-transaction-message/list-policy-transaction-message.page';
import { ListPolicyClaimPage } from './list-policy-claim/list-policy-claim.page';
import { IssuePolicyPage } from './issue-policy/issue-policy.page';
import { UpdatePolicyNumberPage } from './update-policy-number/update-policy-number.page';

/**
 * Export policy common module class.
 * This class manage Ng Module declarations of
 * policy common.
 */
@NgModule({
    declarations: [
        ListPolicyTransactionPage,
        DetailPolicyTransactionPage,
        DetailPolicyPage,
        UpdatePolicyPage,
        ListPolicyMessagePage,
        ListPolicyTransactionMessagePage,
        ListPolicyClaimPage,
        IssuePolicyPage,
        UpdatePolicyNumberPage,
    ],
    imports: [
        SharedModule,
        SharedComponentsModule,
        ListModule,
    ],
    exports: [
        SharedModule,
        SharedComponentsModule,
        ListModule,
        ListPolicyTransactionPage,
        DetailPolicyTransactionPage,
        DetailPolicyPage,
        UpdatePolicyPage,
        ListPolicyMessagePage,
        ListPolicyTransactionMessagePage,
        IssuePolicyPage,
        UpdatePolicyNumberPage,
    ],
})
export class PolicyCommonModule { }
