import { NgModule } from '@angular/core';
import { RouterModule } from '@angular/router';
import { EditEmailTemplatePage } from '@app/pages/edit-email-template/edit-email-template.page';
import { SharedModule } from '@app/shared.module';
import { SharedComponentsModule } from '@app/components/shared-components.module';
import { BackNavigationGuard } from '@app/providers/guard/back-navigation.guard';
import { PermissionGuard } from '@app/providers/guard/permission.guard';
import { Permission } from '@app/helpers/permissions.helper';
import { TypedRoutes } from '@app/routing/typed-route';

const routes: TypedRoutes = [
    {
        path: 'create',
        component: EditEmailTemplatePage,
        canActivate: [PermissionGuard],
        data: {
            mustHaveOneOfPermissions: [Permission.ManageTenants, Permission.ManagePortals, Permission.ManageProducts],
        },
        canDeactivate: [BackNavigationGuard],
    },
    {
        path: ':emailTemplateId/edit',
        component: EditEmailTemplatePage,
        canActivate: [PermissionGuard],
        data: {
            mustHaveOneOfPermissions: [Permission.ManageTenants, Permission.ManagePortals, Permission.ManageProducts],
        },
        canDeactivate: [BackNavigationGuard],
    },
];

/**
 * Export email template module class.
 * TODO: Write a better class header: Ng Module declaration of
 * email template.
 */
@NgModule({
    declarations: [EditEmailTemplatePage],
    imports: [
        SharedModule,
        SharedComponentsModule,
        RouterModule.forChild(routes),
    ],
})
export class EditEmailTemplateModule { }
