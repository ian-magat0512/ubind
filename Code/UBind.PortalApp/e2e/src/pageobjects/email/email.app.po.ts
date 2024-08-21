import { AppPage } from '@app/app.po';
import { EmailPageModel } from '../email/email-page.model';
import { element, by } from 'protractor';

export class EmailAppPage extends AppPage {

    emailPageModel: EmailPageModel;

    public constructor() {
        super();
        this.emailPageModel = new EmailPageModel();
    }

    clickAdminListSegment() {
        return this.clickSegment(this.emailPageModel.ListEmailIonSegmentId, "ADMIN");
    }

    checkListIfExists(fullName) {
        // return this.waitForElement(element(by.id(this.emailPageModel.ListEmailAdminIonListId)), this.emailPageModel.ListEmailAdminIonListId, 40).then(x => {
        return this.clickIonList(this.emailPageModel.ListEmailAdminIonListId, fullName, true, false, false);
        // });
    }
}
