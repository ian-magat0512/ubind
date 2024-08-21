import { NgModule } from '@angular/core';
import { SharedModule } from '@app/shared.module';
import { SharedComponentsModule } from '@app/components/shared-components.module';
import { EditAccountPage } from './edit-account/edit-account.page';
import { UploadPictureAccountPage } from './upload-picture-account/upload-picture-account.page';

/**
 * Export account common module class
 * TODO: Write a better class header: ng module for the account.
 */
@NgModule({
    declarations: [
        EditAccountPage,
        UploadPictureAccountPage,
    ],
    imports: [
        SharedModule,
        SharedComponentsModule,
    ],
    exports: [
        SharedModule,
        SharedComponentsModule,
    ],
})
export class AccountCommonModule { }
