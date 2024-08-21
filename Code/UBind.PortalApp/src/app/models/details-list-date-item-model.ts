import { ValidatorFn } from "@angular/forms";
import { DetailsListItemActionIcon } from "./details-list-item-action-icon";
import { DetailListItemModel } from "./details-list-item-model";

/**
 * This is a subclass of DetailListItemModel specifically for dates.
 */
export class DetailListDateItemModel extends DetailListItemModel {
    private static readonly dateItemControlType: string = "datepicker";
    private static readonly dateItemLineCount: number = 1;
    private static readonly dateItemMaxLines: number = 1;
    public minValue: string;
    public maxValue: string;
    public constructor(
        identifier: string,
        value: string,
        controlType: string,
        messageType: string,
        actions: DetailsListItemActionIcon | Array<DetailsListItemActionIcon>,
        maxLines: number,
        validators: Array<ValidatorFn>,
        lineCount: number,
        includeSectionIcons: boolean = true,
        isMultiLine: boolean = false,
    ) {
        super(
            identifier,
            value,
            controlType,
            messageType,
            actions,
            maxLines,
            validators,
            lineCount,
            includeSectionIcons,
            isMultiLine,
        );
    }

    public static createDateItem(
        identifier: string,
        value: string,
        messageType: string = null,
        actions: DetailsListItemActionIcon | Array<DetailsListItemActionIcon> = null,
        validators: Array<ValidatorFn> = null,
    ): DetailListDateItemModel {
        let model: DetailListDateItemModel = new DetailListDateItemModel(
            identifier,
            value,
            this.dateItemControlType,
            messageType,
            actions,
            this.dateItemMaxLines,
            validators,
            this.dateItemLineCount,
        );
        return model;
    }

    /** 
     * To be used to set a min/max date limit for the date picker.
     * @param minValue can be set to null to use the default ion-datepicker minValue.
     * @param maxValue can be set to null to use the default ion-datepicker maxValue.
     */
    public withDateRange(
        minValue: string,
        maxValue: string,
    ): DetailListItemModel {
        this.minValue = minValue;
        this.maxValue = maxValue;
        return this;
    }
}
