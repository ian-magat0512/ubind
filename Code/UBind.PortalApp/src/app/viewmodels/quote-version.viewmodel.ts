import { QuoteVersionResourceModel } from "@app/resource-models/quote-version.resource-model";
import { LocalDateHelper } from "@app/helpers";
import { EntityViewModel } from "./entity.viewmodel";
import { GroupedEntityViewModel } from "./grouped-entity.viewmodel";
import { SortedEntityViewModel, SortDirection } from "./sorted-entity.viewmodel";

/**
 * Export quote version view model class.
 * TODO: Write a better class header: view model of qoute version.
 */
export class QuoteVersionViewModel implements EntityViewModel, GroupedEntityViewModel, SortedEntityViewModel {
    public constructor(quoteVersion: QuoteVersionResourceModel) {
        this.id = this.quoteVersionId = quoteVersion.id;
        this.quoteId = quoteVersion.quoteId;
        this.quoteNumber = quoteVersion.quoteNumber;
        this.quoteVersionNumber = quoteVersion.quoteVersionNumber;
        if (quoteVersion.lastModifiedDateTime) {
            this.lastModifiedDate = LocalDateHelper.toLocalDate(quoteVersion.lastModifiedDateTime);
            this.lastModifiedTime = LocalDateHelper.convertToLocalAndGetTimeOnly(quoteVersion.lastModifiedDateTime);
        }
        this.createdDate = LocalDateHelper.toLocalDate(quoteVersion.createdDateTime);
        this.createdTime = LocalDateHelper.convertToLocalAndGetTimeOnly(quoteVersion.createdDateTime);
        this.referenceNumber = quoteVersion.quoteNumber
            ? quoteVersion.quoteNumber + '-' + quoteVersion.quoteVersionNumber
            : quoteVersion.quoteVersionNumber;
        this.groupByValue = this.createdDate;
        this.sortByValue = this.quoteNumber;
        this.sortDirection = SortDirection.Descending;
    }

    public id: string;
    public quoteVersionId: string;
    public quoteId: string;
    public quoteNumber: string;
    public quoteVersionNumber: string;
    public createdDate: string;
    public createdTime: string;
    public lastModifiedDate: string;
    public lastModifiedTime: string;
    public groupByValue: string;
    public sortByValue: string;
    public sortDirection: SortDirection;
    public deleteFromList: boolean = false;
    public referenceNumber: string;

    public setGroupByValue(
        quoteVersionList: Array<QuoteVersionViewModel>,
        groupBy: string,
    ): Array<QuoteVersionViewModel> {
        // Quote Version Entity only has one group by value - createdDate, 
        // since it was already set as a default value, we return the list for now
        return quoteVersionList;
    }

    public setSortOptions(
        quoteVersionList: Array<QuoteVersionViewModel>,
        sortBy: string,
        sortDirection: SortDirection,
    ): Array<QuoteVersionViewModel> {
        sortDirection = sortDirection == null ? SortDirection.Descending : sortDirection;

        if (sortBy === "Quote Number") {
            quoteVersionList.forEach((item: QuoteVersionViewModel) => {
                item.sortByValue = item.quoteNumber;
                item.sortDirection = sortDirection;
            });
        } else {
            quoteVersionList.forEach((item: QuoteVersionViewModel) => {
                item.sortByValue = item.createdDate;
                item.sortDirection = sortDirection;
            });
        }
        return quoteVersionList;
    }
}
