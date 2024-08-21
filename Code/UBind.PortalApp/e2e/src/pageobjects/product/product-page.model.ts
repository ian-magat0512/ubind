import { by, element } from "protractor";

export class productPageModel {
    public constructor() {
    }
    
    public AddReleaseIcon = element(by.id('detail-product-add-icon'));
    public CreateEditReleaseTypeField = element(by.css('ion-select[ng-reflect-name=type]'));
    public CreateEditDescriptionField = element(by.css('ion-textarea[ng-reflect-name=label]'));
    
    public CreateReleaseSaveButton = element(by.id('create-release-save-btn'));
}