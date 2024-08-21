import { FieldConfiguration } from "./field.configuration";

/**
 * Represents the configuration for a field which stores data.
 */
export interface DataStoringFieldConfiguration extends FieldConfiguration {

    /**
     * Gets or sets a UBind Expression which is evaluated in order to set the default value of the field when it
     * first appears.
     */
    defaultValueExpression: string;

    /**
     * Gets or sets a UBind Expression which is evaluated to set the value of the field when one or more other
     * fields or properties within the system change.
     */
    calculatedValueExpression: string;

    /**
     * Gets or sets a UBind Expression which is evaluated when it's inputs change, and each time it
     * changes it causes the calculated value to be updated to the result of the CalculatedValueExpression.
     * If this is not set, the CalculatedValueExpression will cause the fields value to be updated each
     * time the calculated value updates. If this is set, it will only update the field when this expressions
     * value changes.
     */
    calculatedValueTriggerExpression: string;

    /**
     *  Gets or sets a value indicating whether when the field is first rendered, whether it should automatically
     *  trigger the calculated value. Defaults to true.
     */
    autoTriggerCalculatedValue?: boolean;

    /**
     * Gets or sets a value indicating whether AutoTriggerCalculatedValue is allowed if the field has previously
     * had a value. Defaults to false.
     * Normally, if a field has a value then it's hidden and unhidden, it will retain the value set, even if it
     * was a calculated value. It won't re-calculate the value. If you set this to true, then when the field is
     * unhidden, it will recalculate the value overwriting the previous value (regardless of whether it was set
     * by the user or by calculation).
     */
    allowCalculatedValueToAutoTriggerWhenFieldHadPreviousValue?: boolean;

    /**
     * Gets or sets a UBind Expression which must evaluate to true for the CalculatedValueExpression to
     * be able to update the fields value.
     */
    calculatedValueConditionExpression: string;

    /**
     * Gets or sets a value indicating whether this field's value affects the premium, and if changed
     * a re-calculation of the premium can be done.
     */
    affectsPremium?: boolean;

    /**
     * Gets or sets a value indicating whether this field's value affects triggers, and if changed
     * a re-evaluation of the referral triggers can be done.
     */
    affectsTriggers?: boolean;

    /**
     * Gets or sets a value indicating whether this field must be filled out and valid before
     * a calculation of the premium can be done.
     */
    requiredForCalculations?: boolean;

    /**
     * Gets or sets a value indicating whether this field should be stored due to the sensitivity of it's
     * data. Typically private fields include credit card numbers and similar.
     */
    private?: boolean;

    /**
     * Gets or sets a value indicating whether this field should be displayed to users in the portal.
     */
    displayable: boolean;

    /**
     * Gets or sets a value indicating whether a change to this field's value is allowed after the quote has been
     * approved. If set to false, a change to this fields value will stop the quote from being able to be bound.
     */
    canChangeWhenApproved?: boolean;

    /**
     * Gets or sets a value indicating whether the value of this field will be cleared when a new quote is
     * created based on a previous quote or policy. For example, in the case of a renewal, this would be
     * typically set on the duty of disclosure questions to ensure a user has to confirm again that they
     * have disclosed everything.
     */
    resetForNewQuotes?: boolean;

    /**
     * Gets or sets a value indicating whether the value of this field will be cleared when a new renewal quote is
     * created based on a previous quote or policy. For example, in the case of a renewal, this would be
     * typically set on the duty of disclosure questions to ensure a user has to confirm again that they
     * have disclosed everything.
     */
    resetForNewRenewalQuotes?: boolean;

    /**
     * Gets or sets a value indicating whether the value of this field will be cleared when a new adjustment quote is
     * created based on a previous quote or policy. For example, in the case of a adjustment, this would be
     * typically set on the duty of disclosure questions to ensure a user has to confirm again that they
     * have disclosed everything.
     */
    resetForNewAdjustmentQuotes?: boolean;

    /**
     * Gets or sets a value indicating whether the value of this field will be cleared when a new cancellation quote is
     * created based on a previous quote or policy. For example, in the case of a cancellation, this would be
     * typically set on the duty of disclosure questions to ensure a user has to confirm again that they
     * have disclosed everything.
     */
    resetForNewCancellationQuotes?: boolean;

    /**
     * Gets or sets a value indicating whether the value of this field will be cleared when a new purchase quote is
     * created based on a previous quote or policy. For example, in the case of a purchase, this would be
     * typically set on the duty of disclosure questions to ensure a user has to confirm again that they
     * have disclosed everything.
     */
    resetForNewPurchaseQuotes?: boolean;

    /**
     * Gets or sets the location of the cell where the field value(s) are written to as part of
     * calculating a premium or triggers. For fields which are repeated, the workbook cell location
     * represents the location of the first value.
     * This property can be null if there is no calculation workboook being used.
     */
    calculationWorkbookCellLocation: CalculationWorkbookCellLocation;

    /**
     * Gets or sets a value indicating whether the fields value should not be updated until the value
     * stops changing for more than a given period, defined by fieldValueUpdateDebounceMilliseconds.
     */
    debounceFieldValueUpdates?: boolean;

    /**
     * Gets or sets the number of millseconds of no changes after which the field value is updated.
     * Only applies if debounceFieldValueUpdates is true.
     * Defaults to 500.
     */
    fieldValueUpdateDebounceMilliseconds?: number;

}

/**
 * Represents a location of a cell in the calculation workbook.
 */
export interface CalculationWorkbookCellLocation {
    sheetIndex: number;
    rowIndex: number;
    colIndex: number;
}
