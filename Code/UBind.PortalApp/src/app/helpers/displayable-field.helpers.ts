import { DisplayableFieldsModel } from '@app/models';

/**
 * Export displayable field helper class
 * This class helps the displaying of fields.
 */
export class DisplayableFieldHelper {

    public static isDisplayableField(fieldInput: string, fieldList: DisplayableFieldsModel): boolean {
        if (fieldList.displayableFields) {
            return fieldList.displayableFields.filter((field: string) => field == fieldInput).length > 0;

        }
        return true;
    }

    public static isRepeatingDisplayableField(fieldInput: string, repeatingFieldList: DisplayableFieldsModel): boolean {
        if (repeatingFieldList.repeatingDisplayableFields) {
            return repeatingFieldList.repeatingDisplayableFields
                .filter((field: string) => field == fieldInput).length > 0;
        }
        return true;
    }

}
