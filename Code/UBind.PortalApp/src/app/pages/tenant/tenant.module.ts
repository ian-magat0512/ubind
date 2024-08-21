import { NgModule } from '@angular/core';
import { SharedModule } from '@app/shared.module';
import { SharedComponentsModule } from '@app/components/shared-components.module';
import { CreateEditTenantPage } from '@app/pages/tenant/create-edit-tenant/create-edit-tenant.page';
import { DetailTenantPage } from '@app/pages/tenant/detail-tenant/detail-tenant.page';
import { ListTenantPage } from '@app/pages/tenant/list-tenant/list-tenant.page';
import { TenantSessionSettingsPage } from '@app/pages/tenant/session-settings/session-settings.page';
import { ListModule } from '@app/list.module';
import { ListTenantProductPage } from '@app/pages/tenant/list-tenant-product/list-tenant-product.page';
import { TenantRoutingModule } from './tenant-routing.modules';
import { ProductSharedComponentModule } from '@pages/product/product-shared-component.module';
import { TenantPasswordExpirySettingsPage } from './password-expiry-settings/password-expiry-settings.page';
import { DataTableSharedComponentModule } from '@pages/data-table/data-table-shared-component.module';

/**
 * Module for tenant page.
 */
 @NgModule({
     declarations: [
         CreateEditTenantPage,
         TenantSessionSettingsPage,
         TenantPasswordExpirySettingsPage,
         DetailTenantPage,
         ListTenantProductPage,
         ListTenantPage,
     ],
     imports: [
         SharedModule,
         SharedComponentsModule,
         ListModule,
         ProductSharedComponentModule,
         DataTableSharedComponentModule,
         TenantRoutingModule,
     ],
 })
export class TenantModule { }
