import { by, element } from "protractor";

export class TenantPageModel {

    public constructor() {
        this.DetailTenantProductIonList = element(by.id(this.DetailTenantProductIonListId));
    }

    public AddProductIcon = element(by.id('detail-tenant-add-icon'));
    public NameField = element(by.css('ion-input[ng-reflect-name=name]'));
    public AbbreviationField = element(by.css('ion-input[ng-reflect-name=abbreviation]'));
    public AddProductSaveButton = element(by.id('create-product-save-btn'));
    public AddTenantSaveButton = element(by.id('create-tenant-save-btn'));
    public AlertBox = element(by.className('alert-wrapper'));
    public AlertBoxOKButton = element(by.className('alert-button'));
    public ListTenantAddIcon = element(by.id('list-tenant-add-icon'));
    public DetailTenantProductIonListId = 'detail-tenant-product-ion-list';
    public DetailTenantProductIonList = element(by.id('detail-tenant-product-ion-list'));
    public ArrowBackIonButton = element(by.id('arrow-back-ion-button'));
}