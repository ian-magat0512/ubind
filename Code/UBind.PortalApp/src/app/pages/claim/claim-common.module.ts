import { NgModule } from '@angular/core';
import { SharedModule } from '@app/shared.module';
import { SharedComponentsModule } from '@app/components/shared-components.module';
import { ListModule } from '@app/list.module';
import { ListClaimMessagePage } from './list-claim-message/list-claim-message.page';
import { ListCustomerClaimPage } from './list-customer-claim/list-customer-claim.page';
import { DetailCustomerClaimPage } from './detail-customer-claim/detail-customer-claim.page';
import { CreateClaimPage } from './create-claim/create-claim.page';
import { AssessClaimPage } from './assess-claim/assess-claim.page';
import { ListClaimPage } from './claim.index';
import { DetailClaimVersionPage } from './detail-claim-version/detail-claim-version.page';
import { DetailClaimPage } from './detail-claim/detail-claim.page';
import { NotificationAcknowledgeClaimPage } from './notification-acknowledge-claim/notification-acknowledge-claim.page';
import { NumberAssignClaimPage } from './number-assign-claim/number-assign-claim.page';
import { NumberUpdateClaimPage } from './number-update-claim/number-update-claim.page';
import { ReviewClaimPage } from './review-claim/review-claim.page';
import { SettleClaimPage } from './settle-claim/settle-claim.page';
import { UpdateClaimVersionPage } from './update-claim-version/update-claim-version.page';
import { UpdateClaimPage } from './update-claim/update-claim.page';
import { ListClaimVersionPage } from './list-claim-version/list-claim-version.page';
import { ClaimSharedComponentModule } from './claim-shared-component.module';

/**
 * Export Claim Common Module class.
 * This class manage Ng Module declarations claim common.
 */
@NgModule({
    declarations: [
        UpdateClaimPage,
        NotificationAcknowledgeClaimPage,
        ReviewClaimPage,
        AssessClaimPage,
        SettleClaimPage,
        NumberAssignClaimPage,
        NumberUpdateClaimPage,
        DetailClaimVersionPage,
        UpdateClaimVersionPage,
        ListClaimPage,
        ListClaimMessagePage,
        ListCustomerClaimPage,
        DetailCustomerClaimPage,
        CreateClaimPage,
        ListClaimVersionPage,
    ],
    imports: [
        SharedModule,
        SharedComponentsModule,
        ClaimSharedComponentModule,
        ListModule,
    ],
    exports: [
        SharedModule,
        SharedComponentsModule,
        ListModule,
        DetailClaimPage,
        UpdateClaimPage,
        NotificationAcknowledgeClaimPage,
        ReviewClaimPage,
        AssessClaimPage,
        SettleClaimPage,
        NumberAssignClaimPage,
        NumberUpdateClaimPage,
        DetailClaimVersionPage,
        UpdateClaimVersionPage,
        ListClaimPage,
        ListClaimMessagePage,
        ListCustomerClaimPage,
        DetailCustomerClaimPage,
        CreateClaimPage,
        ListClaimVersionPage,
    ],
})
export class ClaimCommonModule { }
