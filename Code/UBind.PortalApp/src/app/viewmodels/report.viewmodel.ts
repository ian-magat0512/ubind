import { ReportResourceModel } from '@app/resource-models/report.resource-model';
import { EntityViewModel } from "./entity.viewmodel";
import { GroupedEntityViewModel } from './grouped-entity.viewmodel';
import { SortDirection, SortedEntityViewModel } from './sorted-entity.viewmodel';
import { LocalDateHelper } from '@app/helpers';
import { SortAndFilterBy } from '@app/models/sort-filter-by.enum';

/**
 * Export report view model class.
 * TODO: Write a better class header: view model of report.
 */
export class ReportViewModel implements EntityViewModel, GroupedEntityViewModel, SortedEntityViewModel {
    public constructor(report: ReportResourceModel) {
        this.id = report.id;
        this.name = report.name;
        this.description = report.description;
        this.isDeleted = report.isDeleted;
        this.deleteFromList = this.isDeleted;
        this.createdDate = report.createdDateTime && LocalDateHelper.toLocalDate(report.createdDateTime);
        this.createdDateTime = report.createdDateTime;
        this.lastModifiedDateTime = report.lastModifiedDateTime;
        this.lastModifiedDate = LocalDateHelper.toLocalDate(report.lastModifiedDateTime);
        this.lastModifiedTime = LocalDateHelper.convertToLocalAndGetTimeOnly(report.lastModifiedDateTime);
        this.groupByValue = this.createdDate;
        this.sortByValue = report.createdDateTime;
        this.sortDirection = SortDirection.Descending;
    }

    public id: string;
    public name: string;
    public description: string;
    public isDeleted: boolean;
    public deleteFromList: boolean;
    public createdDate: string;
    public createdDateTime: string;
    public lastModifiedDateTime: string;
    public lastModifiedDate: string;
    public lastModifiedTime: string;
    public groupByValue: string;
    public sortByValue: string;
    public sortDirection: SortDirection;

    public setGroupByValue(
        reportList: Array<ReportViewModel>,
        groupBy: string,
    ): Array<ReportViewModel> {
        if (groupBy === SortAndFilterBy.ReportName) {
            reportList.forEach((item: ReportViewModel) => {
                item.groupByValue = item.name;
            });
        } else if (groupBy === SortAndFilterBy.LastModifiedDate) {
            reportList.forEach((item: ReportViewModel) => {
                item.groupByValue = item.lastModifiedDate;
            });
        } else {
            reportList.forEach((item: ReportViewModel) => {
                item.groupByValue = item.createdDate;
            });
        }

        return reportList;
    }

    public setSortOptions(
        reportList: Array<ReportViewModel>,
        orderBy: string,
        sortDirection: SortDirection,
    ): Array<ReportViewModel> {
        sortDirection = sortDirection == null ? SortDirection.Descending : sortDirection;

        if (orderBy === SortAndFilterBy.ReportName) {
            reportList.forEach((item: ReportViewModel) => {
                item.sortByValue = item.name;
                item.sortDirection = sortDirection;
            });
        } else if (orderBy === SortAndFilterBy.LastModifiedDate) {
            reportList.forEach((item: ReportViewModel) => {
                item.sortByValue = item.lastModifiedDateTime;
                item.sortDirection = sortDirection;
            });
        } else {
            reportList.forEach((item: ReportViewModel) => {
                item.sortByValue = item.createdDateTime;
                item.sortDirection = sortDirection;
            });
        }

        return reportList;
    }
}
