import { NgModule } from '@angular/core';
import { SharedComponentsModule } from '@app/components/shared-components.module';
import { ListModule } from '@app/list.module';
import { SharedModule } from '@app/shared.module';
import { CreateEditProductPage } from './create-edit-product/create-edit-product.page';
import { ProductDeploymentSettingsPage } from './deployment-setting/deployment-setting.page';
import { DetailProductPage } from './detail-product/detail-product.page';
import { EditNumberPoolPage } from './edit-number-pool/edit-number-pool.page';
import { ProductQuoteExpirySettingsPage } from './quote-expiry-setting/quote-expiry-setting.page';
import { RefundSettingsPage } from './refund-settings/refund-settings.page';
import { ReleaseSelectionSettingsPage } from './release-selection-setting/release-selection-settings.page';
import { TransactionTypePage } from './transaction-type/renewal-transaction-type.page';

/**
 * Common product page component.
 */
@NgModule({
    declarations: [
        DetailProductPage,
        CreateEditProductPage,
        ProductDeploymentSettingsPage,
        TransactionTypePage,
        ProductQuoteExpirySettingsPage,
        RefundSettingsPage,
        EditNumberPoolPage,
        ReleaseSelectionSettingsPage,
    ],
    imports: [
        SharedModule,
        SharedComponentsModule,
        ListModule,
    ],
    exports: [
        SharedModule,
        SharedComponentsModule,
        ListModule,
        CreateEditProductPage,
        DetailProductPage,
        ProductDeploymentSettingsPage,
        TransactionTypePage,
        ProductQuoteExpirySettingsPage,
        EditNumberPoolPage,
        ReleaseSelectionSettingsPage,
    ],
})
export class ProductSharedComponentModule { }
