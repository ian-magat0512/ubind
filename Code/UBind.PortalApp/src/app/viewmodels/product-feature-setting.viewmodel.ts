import { ProductFeatureSettingItem } from "@app/models/product-feature-setting-item.enum";

/**
 * Export Product feature setting view model class.
 * This class manage the setting of the feature for product.
 */
export class ProductFeatureSettingViewModel {
    public constructor(
        public productFeatureSettingItem: ProductFeatureSettingItem,
        public name: string,
        public isEnabled: boolean,
        public iconName: string,
    ) { }
}
