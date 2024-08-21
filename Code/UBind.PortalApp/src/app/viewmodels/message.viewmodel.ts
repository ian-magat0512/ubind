import { LocalDateHelper } from "@app/helpers";
import { MessageResourceModel, Tag } from "@app/resource-models/message.resource-model";
import { GroupedEntityViewModel } from "./grouped-entity.viewmodel";
import { SegmentableEntityViewModel } from "./segmentable-entity.viewmodel";
import { SortDirection, SortedEntityViewModel } from "./sorted-entity.viewmodel";

/**
 * Message view model class.
 */
export class MessageViewModel implements SegmentableEntityViewModel, GroupedEntityViewModel, SortedEntityViewModel {

    public segment: string;
    public id: string;
    public deleteFromList: boolean = false;

    public groupByValue: string;

    public sortByValue: string;
    public sortDirection: SortDirection;

    public recipient: string;
    public subject: string;
    public message: string;
    public createdDateTime: string;
    public type: string;

    public constructor(message: MessageResourceModel) {
        this.id = message.id;
        this.recipient = message.recipient;
        this.subject = message.subject;
        this.message = message.message;
        this.createdDateTime = message.createdDateTime;
        this.type = message.type;

        let emailType: string;
        if (message.tags) {
            let tag: Tag = message.tags.find((x: Tag) => x.tagType == "EmailType");
            if (tag) {
                emailType = tag.value.toLowerCase();
            }
        }

        if (message.type == 'email') {
            this.segment = emailType == 'admin' ? 'client' : emailType;
        }

        this.groupByValue = LocalDateHelper.toLocalDate(message.createdDateTime);
        this.sortByValue = message.createdDateTime;
        this.sortDirection = SortDirection.Descending;
    }

    public setGroupByValue(
        messageList: Array<MessageViewModel>,
        groupBy: string,
    ): Array<MessageViewModel> {
        return messageList;
    }

    public setSortOptions(
        messageList: Array<MessageViewModel>,
        sortBy: string,
        sortDirection: SortDirection,
    ): Array<MessageViewModel> {

        sortDirection = sortDirection == null ? SortDirection.Descending : sortDirection;

        messageList.forEach((item: MessageViewModel) => {
            item.sortByValue = item.createdDateTime;
            item.sortDirection = sortDirection;
        });

        return messageList;
    }

}
