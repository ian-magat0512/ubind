import { AppPage } from '@app/app.po';
import { QuotePageModel } from './quote-page.model';

export class QuoteAppPage extends AppPage {

    quotePageModel: QuotePageModel;

    public constructor() {
        super();
        this.quotePageModel = new QuotePageModel();
    }

    clickCompleteListSegment() {
        return this.clickSegment(this.quotePageModel.ListQuoteIonSegmentId, "COMPLETE");
    }

    checkListIfExists(fullName) {
        return this.clickIonList(this.quotePageModel.ListQuoteCompleteIonListId, fullName, true, false, false);
    }
}
