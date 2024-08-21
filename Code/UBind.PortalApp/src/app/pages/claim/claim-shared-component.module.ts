import { NgModule } from '@angular/core';
import { SharedComponentsModule } from '@app/components/shared-components.module';
import { ListModule } from '@app/list.module';
import { SharedModule } from '@app/shared.module';
import { DetailClaimPage } from './detail-claim/detail-claim.page';

/**
 * Claim component that will be shared to other module.
 */
@NgModule({
    declarations: [
        DetailClaimPage,
    ], imports: [
        SharedModule,
        SharedComponentsModule,
        ListModule,
    ],
    exports: [
        DetailClaimPage,
    ],
})
export class ClaimSharedComponentModule { }
