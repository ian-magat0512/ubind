import { DetailsListItemActionIcon } from "./details-list-item-action-icon";
import { ValidatorFn } from "@angular/forms";
import { RelatedEntityType } from "@app/models/related-entity-type.enum";

/**
 * Export detail list item model class.
 * TODO: This class is used to create detail list item.
 */
export class DetailListItemModel {
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
    ): DetailListItemModel {
        let model: DetailListItemModel = new DetailListItemModel(
            identifier,
            value,
            controltype,
            messageType,
            actions,
            maxLines,
            validators,
            lineCount,
            includeSectionIcons,
            isMultiLine,
        );

        return model;
    }

    public withRelatedEntity(
        relatedEntityType: RelatedEntityType,
        relatedEntityOrganisationId: string,
        relatedEntityOwnerId: string,
        relatedEntityCustomerId: string,
    ): DetailListItemModel {
        this.relatedEntityType = relatedEntityType;
        this.relatedEntityOrganisationId = relatedEntityOrganisationId;
        this.relatedEntityOwnerId = relatedEntityOwnerId;
        this.relatedEntityCustomerId = relatedEntityCustomerId;

        return this;
    }
}
