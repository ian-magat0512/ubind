/**
 * Represents a field for selection options for an entity's edit view
 */
export interface EntityEditFieldOption {
    type: string;
    name: string;
    options: Array<FieldOption>;
}

/**
 * Represents an option that can be selected
 */
export interface FieldOption {
    label: string;
    value: string;
}

/**
 * Represents the showing/hiding of fields.
 * Mostly this is being used for additional property definition components
 * to show/hide fields based on the value of another field.
 * Ideally this will be refactored to be a part of the DetailsListFormItem in the future.
 */
export interface FieldShowHideRule {
    triggerField: string;
    fieldToHideOrShow: string;
    showWhenValueIs: string;
}
