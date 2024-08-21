import { QuoteResourceModel } from '@app/resource-models/quote.resource-model';
import { QuoteType } from '@app/models/quote-type.enum';
import { LocalDateHelper } from '@app/helpers';
import { SegmentableEntityViewModel } from './segmentable-entity.viewmodel';
import { GroupedEntityViewModel } from './grouped-entity.viewmodel';
import { SortDirection, SortedEntityViewModel } from './sorted-entity.viewmodel';
import { SortAndFilterBy } from '@app/models/sort-filter-by.enum';
import { Errors } from '@app/models/errors';

/**
 * Export quote view model class.
 * TODO: Write a better class header: view model of quote.
 */
export class QuoteViewModel implements SegmentableEntityViewModel, GroupedEntityViewModel, SortedEntityViewModel {
    public constructor(quote: QuoteResourceModel) {
        this.id = quote.id;
        this.quoteTitle = quote.quoteTitle;
        this.quoteNumber = quote.quoteNumber;
        this.productId = quote.productId;
        this.productName = quote.productName;
        this.deploymentEnvironment = quote.deploymentEnvironment;
        this.customerName = quote.customerDetails ? quote.customerDetails.displayName : '';
        this.totalAmount = quote.totalAmount;
        this.lastModifiedDate = LocalDateHelper.toLocalDate(quote.lastModifiedDateTime);
        this.lastModifiedTime = LocalDateHelper.convertToLocalAndGetTimeOnly(quote.lastModifiedDateTime);
        this.lastModifiedDateTime = quote.lastModifiedDateTime;
        this.status = quote.status;
        this.segment = quote.status.toLowerCase();
        this.createdTime = LocalDateHelper.convertToLocalAndGetTimeOnly(quote.createdDateTime);
        this.createdDate = LocalDateHelper.toLocalDate(quote.createdDateTime);
        this.createdDateTime = quote.createdDateTime;
        this.expiryDate = quote.expiryDateTime ?
            LocalDateHelper.toLocalDate(quote.expiryDateTime) : quote.expiryDateTime;
        this.expiryTime = quote.expiryDateTime ?
            LocalDateHelper.convertToLocalAndGetTimeOnly(quote.expiryDateTime) : quote.expiryDateTime;
        this.expiryDateTime = quote.expiryDateTime;
        this.isTestData = quote.isTestData;
        this.quoteType = quote.quoteType;
        this.iconName = QuoteViewModel.getIconNameForQuoteType(this.quoteType);
        this.groupByValue = this.lastModifiedDate || this.createdDate;
        this.sortByValue = quote.lastModifiedDateTime;
        this.sortDirection = SortDirection.Descending;
    }

    public id: string;
    public segment: string;
    public quoteTitle: string;
    public quoteNumber: string;
    public productId: string;
    public productName: string;
    public deploymentEnvironment: string;
    public customerName: string;
    public totalAmount: string;
    public lastModifiedDate: string;
    public lastModifiedTime: string;
    public lastModifiedDateTime: string;
    public status: string;
    public createdDate: string;
    public createdTime: string;
    public createdDateTime: string;
    public expiryDate: string;
    public expiryTime: string;
    public expiryDateTime: string;
    public isTestData: boolean;
    public quoteType: number;
    public iconName: string;
    public groupByValue: string;
    public sortByValue: string;
    public sortDirection: SortDirection;
    public deleteFromList: boolean = false;

    public static getIconNameForQuoteType(quoteType: any): string {
        switch (quoteType) {
            case QuoteType.NewBusiness:
                return 'calculator-add';
            case QuoteType.Renewal:
                return 'calculator-refresh';
            case QuoteType.Adjustment:
                return 'calculator-pen';
            case QuoteType.Cancellation:
                return 'calculator-ban';
        }
    }

    public setGroupByValue(quoteList: Array<QuoteViewModel>, groupBy: string): Array<QuoteViewModel> {
        switch (groupBy) {
            case SortAndFilterBy.LastModifiedDate:
                quoteList.forEach((item: QuoteViewModel) => {
                    item.groupByValue = item.lastModifiedDate;
                });
                break;
            case SortAndFilterBy.ExpiryDate:
                quoteList.forEach((item: QuoteViewModel) => {
                    if (item.expiryDate) {
                        item.groupByValue = item.expiryDate;
                    } else {
                        // If expiry date is null or empty, remove it from the list
                        quoteList = quoteList.filter((obj: QuoteViewModel) =>
                            obj.quoteNumber !== item.quoteNumber);
                    }
                });
                break;
            default:
                quoteList.forEach((item: QuoteViewModel) => {
                    item.groupByValue = item.createdDate;
                });
                break;
        }
        return quoteList;
    }

    public setSortOptions(
        quoteList: Array<QuoteViewModel>,
        sortBy: string,
        sortDirection: SortDirection,
    ): Array<QuoteViewModel> {
        sortDirection = sortDirection == null ? SortDirection.Descending : sortDirection;
        sortBy = sortBy || SortAndFilterBy.LastModifiedDate;

        switch (sortBy) {
            case SortAndFilterBy.LastModifiedDate:
                quoteList.forEach((item: QuoteViewModel) => {
                    item.sortByValue = item.lastModifiedDateTime;
                    item.sortDirection = sortDirection;
                });
                break;
            case SortAndFilterBy.CreatedDate:
                quoteList.forEach((item: QuoteViewModel) => {
                    item.sortByValue = item.createdDateTime;
                    item.sortDirection = sortDirection;
                });
                break;
            case SortAndFilterBy.ExpiryDate:
                quoteList.forEach((item: QuoteViewModel) => {
                    item.sortByValue = item.expiryDateTime;
                    item.sortDirection = sortDirection;
                });
                break;
            case SortAndFilterBy.CustomerName:
                quoteList.forEach((item: QuoteViewModel) => {
                    item.sortByValue = item.customerName;
                    item.sortDirection = sortDirection;
                });
                break;
            default:
                throw Errors.General.UnexpectedEnumValue(
                    'SortAndFilterBy',
                    sortBy,
                    "apply the sort options on a quote list",
                );
        }

        return quoteList;
    }
}
