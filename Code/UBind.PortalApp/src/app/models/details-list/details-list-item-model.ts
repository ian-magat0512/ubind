import { DetailsListItemActionIcon } from "./details-list-item-action-icon";
import { ValidatorFn } from "@angular/forms";
import { RelatedEntityType } from "@app/models/related-entity-type.enum";

/**
 * Represents a item to be rendered within a group of items.
 * This class is used to define each of the items int the group without needing to specify the group specific details,
 * such as the category name and icon.
 * It allows for easier creation of all of the items in the group. Once all of the items have been created, they can be
 * converted into DetailListItem instances by passing them to DetailListItemHelper.createDetailItemGroup(...).
 */
export class DetailsListGroupItemModel {
    public identifier: string;
    public value: string;
    public actions: DetailsListItemActionIcon | Array<DetailsListItemActionIcon> = null;
    public messageType: string;
    public controlType: string;
    public maxLines: number;
    public validators: Array<ValidatorFn>;
    public lineCount: number;
    public isMultiLine: boolean;
    public includeSectionIcons: boolean;

    // if its a related entity.
    public relatedEntityType: RelatedEntityType;
    public relatedEntityOwnerId: string;
    public relatedEntityCustomerId: string;
    public relatedEntityOrganisationId: string;

    protected constructor(
        identifier: string,
        value: string,
        controltype: string,
        messageType: string,
        actions: DetailsListItemActionIcon | Array<DetailsListItemActionIcon>,
        maxLines: number,
        validators: Array<ValidatorFn>,
        lineCount: number,
        includeSectionIcons: boolean = true,
        isMultiLine: boolean = false,
    ) {
        this.identifier = identifier;
        this.actions = actions;
        this.value = value;
        this.messageType = messageType;
        this.controlType = controltype;
        this.maxLines = maxLines;
        this.validators = validators;
        this.lineCount = lineCount;
        this.isMultiLine = isMultiLine;
        this.includeSectionIcons = includeSectionIcons;
    }

    public static create(
        identifier: string,
        value: string,
        controltype: string = null,
        messageType: string = null,
        actions: DetailsListItemActionIcon | Array<DetailsListItemActionIcon> = null,
        maxLines: number = 2,
        validators: Array<ValidatorFn> = null,
        lineCount: number = 1,
        includeSectionIcons: boolean = true,
        isMultiLine: boolean = false,
    ): DetailsListGroupItemModel {
        let model: DetailsListGroupItemModel = new DetailsListGroupItemModel(
            identifier,
            value,
            controltype,
            messageType,
            actions,
            maxLines,
            validators,
            lineCount,
            includeSectionIcons,
            isMultiLine);

        return model;
    }

    public withRelatedEntity(
        relatedEntityType: RelatedEntityType,
        relatedEntityOrganisationId: string,
        relatedEntityOwnerId: string,
        relatedEntityCustomerId: string): DetailsListGroupItemModel {
        this.relatedEntityType = relatedEntityType;
        this.relatedEntityOrganisationId = relatedEntityOrganisationId;
        this.relatedEntityOwnerId = relatedEntityOwnerId;
        this.relatedEntityCustomerId = relatedEntityCustomerId;

        return this;
    }
}
