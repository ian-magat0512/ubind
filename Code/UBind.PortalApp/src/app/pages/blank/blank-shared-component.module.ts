import { NgModule } from '@angular/core';
import { SharedComponentsModule } from '@app/components/shared-components.module';
import { SharedModule } from '@app/shared.module';
import { BlankPage } from './blank.page';

/**
 * Blank page components that will be used by other pages.
 */
@NgModule({
    declarations: [
        BlankPage,
    ],
    imports: [
        SharedModule,
        SharedComponentsModule,
    ],
    exports: [
        BlankPage,
    ],
})
export class BlankSharedComponentModule { }
