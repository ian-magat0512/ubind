import { AppPage } from '../../app.po';
import { TenantPageModel } from './tenant-page.model';
import { browser } from 'protractor';

export class TenantAppPage extends AppPage {

    tenantPageModel: TenantPageModel;

    public constructor() {
        super();
        this.tenantPageModel = new TenantPageModel();
    }

    clickListTenantAddIcon() {
        return this.waitForElement(this.tenantPageModel.ListTenantAddIcon, 'list-tenant-add-icon').then(x => {
            return this.tenantPageModel.ListTenantAddIcon.isPresent().then(isPresent => {
                if (isPresent) {
                    return this.tenantPageModel.ListTenantAddIcon.click();
                }
            });
        });
    }

    async checkTenantListIfExisting(value) {
        return this.clickIonList('active-list-tenants', value, true, false);
    }

    async clickList(value) {
        return this.checkTenantListIfExisting(value).then(exists => {
            if (exists) {
                return this.clickIonList('active-list-tenants', value);
            }
        })
    }

    clickDetailSegment(value) {
        return this.clickSegment('detail-tenant-segment', value);
    }

    async checkProductListIfExisting(nameValue, abbreviationValue) {
        return this.clickIonList(this.tenantPageModel.DetailTenantProductIonListId, abbreviationValue, true, false);
    }

    clickAddProductIcon() {
        return this.tenantPageModel.AddProductIcon.click();
    }

    newProduct() {
        return this.tenantPageModel.AddProductSaveButton.click();
    }

    fillAddTenantModal(nameValue, abbreviationValue) {
        return this.setIonicInput(this.tenantPageModel.NameField, nameValue).then(x => {
            return this.setIonicInput(this.tenantPageModel.AbbreviationField, abbreviationValue);
        });
    }

    fillAddProductModal(nameValue, abbreviationValue) {
        return this.setIonicInput(this.tenantPageModel.NameField, nameValue).then(x => {
            return this.setIonicInput(this.tenantPageModel.AbbreviationField, abbreviationValue);
        });
    }

    saveNewProduct() {
        return this.waitForElement(this.tenantPageModel.AddProductSaveButton, 'create-product-save-btn').then(x => {
            return this.tenantPageModel.AddProductSaveButton.click();
        });
    }

    saveNewTenant() {
        return this.waitForElement(this.tenantPageModel.AddTenantSaveButton, 'create-tenant-save-btn').then(x => {
            return this.tenantPageModel.AddTenantSaveButton.click();
        });
    }

    clickBackButton() {
        return this.waitForElement(this.tenantPageModel.ArrowBackIonButton, 'back button', 3).then(x => {
            return this.tenantPageModel.ArrowBackIonButton.click();
        });
    }

    toggleAllSettings() {
        return this.ionListToggleAll('detail-tenant-settings-list');
    }

    //sometimes there is a chance an error happen on a previous action. like filling up a form and submitting. you want to continue.
    ignoreAlert() {
        return this.waitForElement(this.tenantPageModel.AlertBox, 'alert-wrapper', 3).then(x => {
            return this.tenantPageModel.AlertBox.isPresent().then(isPresent => {
                if (isPresent) {
                    return this.waitForElementClickable(this.tenantPageModel.AlertBoxOKButton, "alert button", 5).then(x => {
                        return this.tenantPageModel.AlertBoxOKButton.click();
                    });
                }
            });
        });
    }
}