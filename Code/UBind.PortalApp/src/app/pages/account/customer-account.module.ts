import { NgModule } from '@angular/core';
import { RouterModule } from '@angular/router';
import { BackNavigationGuard } from '@app/providers/guard/back-navigation.guard';
import { DetailCustomerAccountPage } from './detail-customer-account/detail-customer-account.page';
import { EditAccountPage } from './edit-account/edit-account.page';
import { UploadPictureAccountPage } from './upload-picture-account/upload-picture-account.page';
import { PermissionGuard } from "@app/providers/guard/permission.guard";
import { Permission } from "@app/helpers/permissions.helper";
import {
    ShowMasterComponentWhenNotSplit,
} from '@app/components/show-master-when-not-split/show-master-when-not-split.component';
import { AccountCommonModule } from './account-common.module';
import { BlankPage } from '../blank/blank.page';
import { EnvironmentGuard } from '@app/providers/guard/environment.guard';
import { BlankSharedComponentModule } from '../blank/blank-shared-component.module';
import { TypedRoutes } from '@app/routing/typed-route';

const routes: TypedRoutes = [
    {
        path: "",
        component: ShowMasterComponentWhenNotSplit,
        canActivate: [EnvironmentGuard, PermissionGuard],
        data: {
            mustHavePermissions: [Permission.ViewMyAccount],
            masterComponent: DetailCustomerAccountPage,
            detailComponent: BlankPage,
        },
    },
    {
        path: "edit",
        component: EditAccountPage,
        canActivate: [EnvironmentGuard, PermissionGuard],
        data: {
            mustHavePermissions: [Permission.EditMyAccount],
            masterComponent: DetailCustomerAccountPage,
        },
        canDeactivate: [BackNavigationGuard],
    },
    {
        path: "picture/upload",
        component: UploadPictureAccountPage,
        canActivate: [EnvironmentGuard, PermissionGuard],
        data: {
            mustHavePermissions: [Permission.EditMyAccount],
            masterComponent: DetailCustomerAccountPage,
        },
        canDeactivate: [BackNavigationGuard],
    },
];

/**
 * Export customer account module class
 */
@NgModule({
    declarations: [
        DetailCustomerAccountPage,
    ],
    imports: [
        AccountCommonModule,
        BlankSharedComponentModule,
        RouterModule.forChild(routes),
    ],
})
export class CustomerAccountModule { }
