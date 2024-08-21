import { AppPage } from '../../app.po';
import { productPageModel } from './product-page.model';

export class ProductAppPage extends AppPage {

    productPageModel: productPageModel;

    public constructor() {
        super();
        this.productPageModel = new productPageModel();
    }

    clickList(value) {
        return this.clickIonList('active-list-products', value, true, true);
    }

    clickDetailSegment(value) {
        return this.clickSegment('detail-product-segment', value);
    }

    clickAddReleaseIcon() {
        return this.waitForElement(this.productPageModel.AddReleaseIcon, "detail-product-add-icon").then(x => {
            return this.productPageModel.AddReleaseIcon.click();
        });
    }

    fillCreateReleaseModal(type, description) {
        return this.setIonicInput(this.productPageModel.CreateEditReleaseTypeField, type).then(x => {
            return this.setIonicInput(this.productPageModel.CreateEditDescriptionField, description);
        });
    }

    clickCreateReleaseSaveButton() {
        return this.waitForElement(this.productPageModel.CreateReleaseSaveButton, "create-release-save-btn").then(x => {
            return this.productPageModel.CreateReleaseSaveButton.click();
        });
    }

    ignoreAlert() {
        // this.waitForElement(this.tenantPageModel.AlertBox);

        // return this.tenantPageModel.AlertBox.isPresent().then(isPresent => {
        //     if (isPresent)
        //         this.tenantPageModel.AlertBoxOKButton.click();
        // });
    }
}
