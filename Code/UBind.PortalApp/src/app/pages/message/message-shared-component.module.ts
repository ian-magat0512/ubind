import { NgModule } from '@angular/core';
import { ListModule } from '@app/list.module';
import { SharedModule } from '@app/shared.module';
import { SharedComponentsModule } from '@app/components/shared-components.module';
import { DetailEmailPage } from './detail-email/detail-email.page';
import { DetailSmsPage } from './detail-sms/detail-sms.page';

/**
 * Message pages components that will be shared to other page.
 */
@NgModule({
    declarations: [
        DetailEmailPage,
        DetailSmsPage,
    ],
    imports: [
        SharedModule,
        SharedComponentsModule,
        ListModule,
    ],
    exports: [
        DetailEmailPage,
        DetailSmsPage,
    ],
})
export class MessageSharedComponentModule { }
