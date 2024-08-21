import { FieldConfiguration } from "./field.configuration";

/**
 * Represents a field which is visble
 */
export interface VisibleFieldConfiguration extends FieldConfiguration {
    /**
     * Gets or sets validation rules for the field.
     */
    validationRules: string;

    startsNewRow?: boolean;

    /**
     * Gets or sets a value indicating whether this field should start a new reveal group.
     * If a question set contains reveal groups it will incrementally reveal them as each reveal
     * group becomes valid.
     */
    startsNewRevealGroup?: boolean;

    bootstrapColumnsExtraSmall: number;
    bootstrapColumnsSmall: number;
    bootstrapColumnsMedium: number;
    bootstrapColumnsLarge: number;
    widgetCssWidth: string;
    label: string;
    question: string;
    heading2: string;
    heading3: string;
    heading4: string;
    paragraph: string;
    html: string;
    htmlTermsAndConditions: string;
    helpMessage: string;
    tooltip: string;
    containerClass: string;
    containerCss: string;
    summaryLabel: string;
    summaryPositionExpression: string;
}
