import { Expression } from "@app/expressions/expression";
import { TextWithExpressions } from "@app/expressions/text-with-expressions";
import { OptionConfiguration } from "@app/resource-models/configuration/option.configuration";
import { Subject } from "rxjs";

/**
 * A select option, which can be used in a select list, radio list, search select, or buttons set.
 */
export interface SelectOption {
    label: string;
    value: string;
    searchableText: string;
    disabled: boolean;
    icon: string;
    cssClass: string;

    /**
     * if set to false, this option will not be rendered at all.
     */
    render: boolean;

    /**
     * if set to true, the option will not be shown as a possible selection
     */
    filtered: boolean;

    /**
     * additional properties associated with the option which can be accessed via an expression method.
     */
    properties: object;

    /**
     * Hidden expression set on the option
     */
    hiddenExpressionSetOnTheOption: Expression;
    labelExpression: TextWithExpressions;
    disabledExpression: Expression;

    /**
     * Hidden expression for the options, set on the field
     */
    hiddenExpressionSetOnTheField: Expression;

    /**
     * A unique id for this option which will be rendered in the id attribute of the html input element.
     */
    id: string;

    /**
     * The original configuration for this option.
     */
    definition: OptionConfiguration;

    /**
     * Triggered when the option is destroyed
     */
    destroyed: Subject<void>;
}
