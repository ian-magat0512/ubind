import { AppPage } from '@app/app.po';
import { CustomerPageModel } from './customer-page.model';

export class CustomerAppPage extends AppPage {

    customerPageModel: CustomerPageModel;

    public constructor() {
        super();
        this.customerPageModel = new CustomerPageModel();
    }

    clickNewListSegment() {
        return this.clickSegment(this.customerPageModel.ListPersonIonSegmentId, "NEW");
    }

    checkListIfExists(fullName) {
        return this.clickIonList(this.customerPageModel.ListPersonNewIonListId, fullName, true, false, false);
    }
}
