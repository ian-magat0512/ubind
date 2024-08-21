import { NgModule } from '@angular/core';
import { RouterModule } from '@angular/router';
import { BackNavigationGuard } from '@app/providers/guard/back-navigation.guard';
import { DetailAccountPage } from './detail-account/detail-account.page';
import { EditAccountPage } from './edit-account/edit-account.page';
import { UploadPictureAccountPage } from './upload-picture-account/upload-picture-account.page';
import { PermissionGuard } from "@app/providers/guard/permission.guard";
import { Permission } from "@app/helpers/permissions.helper";
import {
    ShowMasterComponentWhenNotSplit,
} from '@app/components/show-master-when-not-split/show-master-when-not-split.component';
import { AccountCommonModule } from './account-common.module';
import { BlankPage } from '../blank/blank.page';
import { ListModule } from '@app/list.module';
import { BlankSharedComponentModule } from '../blank/blank-shared-component.module';
import { TypedRoutes } from '@app/routing/typed-route';

const routes: TypedRoutes = [
    {
        path: "",
        component: ShowMasterComponentWhenNotSplit,
        canActivate: [PermissionGuard],
        data: {
            mustHavePermissions: [Permission.ViewMyAccount],
            masterComponent: DetailAccountPage,
            detailComponent: BlankPage,
        },
    },
    {
        path: "edit",
        component: EditAccountPage,
        canActivate: [PermissionGuard],
        data: {
            mustHavePermissions: [Permission.EditMyAccount],
            masterComponent: DetailAccountPage,
        },
        canDeactivate: [BackNavigationGuard],
    },
    {
        path: "picture/upload",
        component: UploadPictureAccountPage,
        canActivate: [PermissionGuard],
        data: {
            mustHavePermissions: [Permission.EditMyAccount],
            masterComponent: DetailAccountPage,
        },
        canDeactivate: [BackNavigationGuard],
    },
];

/**
 * Export account module class
 */
@NgModule({
    declarations: [
        DetailAccountPage,
    ],
    imports: [
        ListModule,
        AccountCommonModule,
        BlankSharedComponentModule,
        RouterModule.forChild(routes),
    ],
})
export class AccountModule { }
