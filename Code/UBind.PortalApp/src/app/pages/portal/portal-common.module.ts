import { NgModule } from '@angular/core';
import { ListPortalPage } from '@app/pages/portal/list-portal/list-portal.page';
import { DetailPortalPage } from '@app/pages/portal/detail-portal/detail-portal.page';
import { CreateEditPortalPage } from './create-edit-portal/create-edit-portal.page';
import { ListModule } from '@app/list.module';
import { SharedComponentsModule } from '@app/components/shared-components.module';
import { SharedModule } from '@app/shared.module';
import { EditPortalLocationPage } from './edit-portal-location/edit-portal-location.page';
import { EditPortalThemePage } from './edit-portal-theme/edit-portal-theme.page';
import { ManagePortalSignInMethodsPage } from './sign-in-methods/manage-portal-sign-in-methods.page';

/**
 * Common portal components, used by PortalModule and OrganisationModule
 */
@NgModule({
    declarations: [
        DetailPortalPage,
        CreateEditPortalPage,
        ListPortalPage,
        EditPortalLocationPage,
        EditPortalThemePage,
        ManagePortalSignInMethodsPage,
    ],
    imports: [
        SharedModule,
        SharedComponentsModule,
        ListModule,
    ],
})
export class PortalCommonModule { }
