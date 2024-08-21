/**
 * Model representing which fields should be displayed vs hidden
 */
export interface DisplayableFieldsModel {
    displayableFields: Array<string>;
    repeatingDisplayableFields: Array<string>;
    displayableFieldsEnabled: boolean;
    repeatingDisplayableFieldsEnabled: boolean;
}
