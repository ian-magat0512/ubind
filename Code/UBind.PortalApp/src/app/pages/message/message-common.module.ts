import { NgModule } from '@angular/core';
import { ListModule } from '@app/list.module';
import { SharedModule } from '@app/shared.module';
import { SharedComponentsModule } from '@app/components/shared-components.module';
import { MessageSharedComponentModule } from './message-shared-component.module';

/**
 * Export email common module class.
 * TODO: Write a better class header: Ng module declaration of email common.
 */
@NgModule({
    declarations: [],
    imports: [
        SharedModule,
        SharedComponentsModule,
        ListModule,
        MessageSharedComponentModule,
    ],
    exports: [
        SharedModule,
        SharedComponentsModule,
        ListModule,
        MessageSharedComponentModule,
    ],
})
export class MessageCommonModule { }
