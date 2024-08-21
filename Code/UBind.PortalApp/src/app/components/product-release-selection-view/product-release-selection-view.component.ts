import { Component, EventEmitter, Input, Output } from "@angular/core";
import { IconLibrary } from "@app/models/icon-library.enum";
import { ProductReleaseSettingsModel } from "@app/models/product-release-settings.model";
import { ProductReleaseSettingsResourceModel } from "@app/resource-models/product-release-settings.resource-model";

/**
 * The view component for product release selection inside under settings tab.
 */
@Component({
    selector: 'app-product-release-selection-view',
    templateUrl: './product-release-selection-view.component.html',
})
export class ProductReleaseSelectionComponent {

    @Input() public productReleaseSettings: ProductReleaseSettingsResourceModel;
    @Output() public handleClick: EventEmitter<any> = new EventEmitter();

    public iconLibrary: typeof IconLibrary = IconLibrary;
    public constructor() { }

    public onClick(quoteType: string): void {
        let doesUseDefaultProductRelease: boolean = false;
        if (quoteType == 'adjustment') {
            doesUseDefaultProductRelease = this.productReleaseSettings.doesAdjustmentUseDefaultProductRelease;
        } else if (quoteType == 'cancellation') {
            doesUseDefaultProductRelease = this.productReleaseSettings.doesAdjustmentUseDefaultProductRelease;
        }

        const productReleaseSettingsModel: ProductReleaseSettingsModel = {
            quoteType: quoteType,
            doesUseDefaultProductRelease: doesUseDefaultProductRelease,
        };
        this.handleClick.emit(productReleaseSettingsModel);
    }
}
