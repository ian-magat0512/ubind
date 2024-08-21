import { SegmentableEntityViewModel } from "./segmentable-entity.viewmodel";
import { EmailResourceModel } from "../resource-models/email.resource-model";
import { GroupedEntityViewModel } from "./grouped-entity.viewmodel";
import { LocalDateHelper } from "../helpers";
import { SortedEntityViewModel, SortDirection } from "./sorted-entity.viewmodel";
import { Tag } from "@app/resource-models/message.resource-model";

/**
 * Export email view model class.
 * TODO: Write a better class header: view model of email.
 */
export class EmailViewModel implements SegmentableEntityViewModel, GroupedEntityViewModel, SortedEntityViewModel {
    public constructor(email: EmailResourceModel) {
        this.id = email.id;
        this.subject = email.subject;
        this.hasAttachment = email.hasAttachment;
        this.recipient = email.recipient;
        this.createdDateTime = email.createdDateTime;
        let emailType: string;
        if (email.tags) {
            let tag: Tag = email.tags.find((x: Tag) => x.tagType == "EmailType");
            if (tag) {
                emailType = tag.value.toLowerCase();
            }
        }

        this.segment = emailType == 'admin' ? 'client' : emailType;
        this.groupByValue = LocalDateHelper.toLocalDate(email.createdDateTime);
        this.sortByValue = email.createdDateTime;
        this.sortDirection = SortDirection.Descending;
    }

    public id: string;
    public segment: string;
    public subject: string;
    public recipient: any;
    public hasAttachment: any;
    public createdDateTime: any;
    public groupByValue: string;
    public sortByValue: string;
    public sortDirection: SortDirection;
    public deleteFromList: boolean = false;

    public setGroupByValue(
        emailList: Array<EmailViewModel>,
        groupBy: string,
    ): Array<EmailViewModel> {
        // Email Entity only has one group by value - createdDateTime, 
        // since it was already set as a default value, we return the list for now
        return emailList;
    }

    public setSortOptions(
        emailList: Array<EmailViewModel>,
        sortBy: string,
        sortDirection: SortDirection,
    ): Array<EmailViewModel> {
        sortDirection = sortDirection == null ? SortDirection.Descending : sortDirection;

        // There is only one order by value for emails which is sent date
        emailList.forEach((item: EmailViewModel) => {
            item.sortByValue = item.createdDateTime;
            item.sortDirection = sortDirection;
        });

        return emailList;
    }
}
