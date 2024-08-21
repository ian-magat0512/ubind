import { ReportFileResourceModel } from "@app/resource-models/report.resource-model";
import { EntityViewModel } from "./entity.viewmodel";
import { LocalDateHelper } from "@app/helpers";
import { GroupedEntityViewModel } from "./grouped-entity.viewmodel";

/**
 * This was the view model for Report file, which is the file produced from generating the report.
 * It was used for displayed list on history tab.
 */
export class ReportFileViewModel implements EntityViewModel, GroupedEntityViewModel {
    public constructor(reportFile: ReportFileResourceModel) {
        this.id = reportFile.reportFileId;
        this.filename = reportFile.filename;
        this.size = reportFile.size;
        this.mimeType = reportFile.mimeType;
        this.deleteFromList = false;
        this.createdDate = reportFile.createdDateTime && LocalDateHelper.toLocalDate(reportFile.createdDateTime);
        this.createdDateTime = reportFile.createdDateTime;
        this.groupByValue = this.createdDate;
    }

    public id: string;
    public filename: string;
    public size: number;
    public mimeType: string;
    public createdDate: string;
    public createdDateTime: string;
    public deleteFromList: boolean;
    public groupByValue: string;

    public setGroupByValue(
        reportList: Array<ReportFileViewModel>,
        groupBy: string,
    ): Array<ReportFileViewModel> {
        reportList.forEach((item: ReportFileViewModel) => {
            item.groupByValue = item.createdDate;
        });

        return reportList;
    }
}
