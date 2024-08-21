import { ValidatorFn } from "@angular/forms";
import { DetailsListItemActionIcon } from "./details-list-item-action-icon";
import { IconLibrary } from "../icon-library.enum";
import { DetailsListFormItem } from "./details-list-form-item";
import { DetailsListItemCard } from "./details-list-item-card";

/**
 * A model for a form input control for taking in a single line of text.
 */
export class DetailsListFormTextItem extends DetailsListFormItem {

    public isMultiLine: boolean;

    public constructor(
        card: DetailsListItemCard,
        groupName: string,
        alias: string,
        displayValue: string,
        description: string,
        icon: string = null,
        iconLibrary: string = IconLibrary.IonicV4,
        header: string = '',
        validator: Array<ValidatorFn> = [],
        actionIcon: DetailsListItemActionIcon = null,
        isRoundIcon: boolean = false,
        isInitVisible: boolean = true,
        isRepeating: boolean = false,
        subheader: string = "",
        includeSectionIcons: boolean = true,
        isMultiLine: boolean = false,
    ) {
        super(
            card,
            'text',
            groupName,
            alias,
            displayValue,
            description,
            icon,
            iconLibrary,
            header,
            validator,
            actionIcon,
            isRoundIcon,
            isInitVisible,
            isRepeating,
            subheader,
            includeSectionIcons);
        this.isMultiLine = isMultiLine;
    }

    public static create(
        card: DetailsListItemCard,
        alias: string,
        description: string,
    ): DetailsListFormTextItem {
        return new DetailsListFormTextItem(
            card,
            'default',
            alias,
            null,
            description);
    }

    public clone(): DetailsListFormItem {
        return new DetailsListFormTextItem(
            this.card,
            this.groupName,
            this.alias,
            this.displayValue,
            this.description,
            this.icon,
            this.iconLibrary,
            this.header,
            this.validator,
            this.actionIcon,
            this.isRoundIcon,
            this.visible,
            this.isRepeating,
            this.subHeader,
            this.includeSectionIcons,
            this.isMultiLine);
    }
}
