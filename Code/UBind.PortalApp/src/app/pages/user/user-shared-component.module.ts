import { NgModule } from '@angular/core';
import { SharedComponentsModule } from '@app/components/shared-components.module';
import { ListModule } from '@app/list.module';
import { SharedModule } from '@app/shared.module';
import { AssignRolePage } from './assign-role/assign-role.page';
import { CreateUserPage } from './create-user/create-user.page';
import { DetailUserPage } from './detail-user/detail-user.page';
import { EditUserPage } from './edit-user/edit-user.page';
import { ListUserMessagePage } from './list-user-message/list-user-message.page';
import { ListUserPage } from './list-user/list-user.page';
import { UploadPictureUserPage } from './upload-picture-user/upload-picture-user.page';

/**
 * User page components that was used other by other page.
 */
@NgModule({
    declarations: [
        CreateUserPage,
        EditUserPage,
        DetailUserPage,
        ListUserPage,
        UploadPictureUserPage,
        ListUserMessagePage,
        AssignRolePage,
    ],
    imports: [
        SharedModule,
        SharedComponentsModule,
        ListModule,
    ],
    exports: [
        CreateUserPage,
        EditUserPage,
        DetailUserPage,
        ListUserPage,
        UploadPictureUserPage,
        ListUserMessagePage,
        AssignRolePage,
    ],
})
export class UserSharedComponentsModule { }
