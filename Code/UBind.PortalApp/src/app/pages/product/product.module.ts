import { NgModule } from '@angular/core';
import { ListProductPage } from '@app/pages/product/list-product/list-product.page';
import { ProductRoutingModule } from './product-routing.module';
import { ProductSharedComponentModule } from './product-shared-component.module';
import { DataTableSharedComponentModule } from '@pages/data-table/data-table-shared-component.module';

/**
 * Export product module class.
 * This class manage Ng Module declarations of product.
 */
@NgModule({
    declarations: [
        ListProductPage,
    ],
    imports: [
        ProductSharedComponentModule,
        DataTableSharedComponentModule,
        ProductRoutingModule,
    ],
})
export class ProductModule { }
