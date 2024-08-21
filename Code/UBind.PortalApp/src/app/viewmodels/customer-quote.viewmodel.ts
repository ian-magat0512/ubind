import { SegmentableEntityViewModel } from "./segmentable-entity.viewmodel";
import { QuoteViewModel } from "./quote.viewmodel";
import { QuoteResourceModel } from '@app/resource-models/quote.resource-model';

/**
 * Export customer quote view model class.
 * TODO: Write a better class header: view model of customer quote.
 */
export class CustomerQuoteViewModel extends QuoteViewModel implements SegmentableEntityViewModel {
    public constructor(quote: QuoteResourceModel) {
        super(quote);
        const quoteStatus: string = quote.status.toLowerCase();
        switch (quoteStatus) {
            case 'incomplete':
            case 'approved':
                this.segment = 'incomplete';
                break;
            case 'review':
            case 'endorsement':
                this.segment = 'referred';
                break;
            case 'declined':
            case 'complete':
                this.segment = 'complete';
                break;
            case 'expired':
                this.segment = 'expired';
                break;
        }
    }
}
