import { FieldDataType } from '@app/models/field-data-type.enum';
import { KeyboardInputMode } from '@app/models/keyboard-input-mode.enum';

/**
 * Export keyboard input mode Helper
 * This class is used to get the keyboard input mode based on data type.
 */
export class KeyboardInputModeHelper {
    public static getInputMode(dataType: FieldDataType): KeyboardInputMode {

        const keyboardInputModeMap: Map<string, KeyboardInputMode> =
        new Map([[FieldDataType.Text, KeyboardInputMode.Text],
            [FieldDataType.Currency, KeyboardInputMode.Decimal],
            [FieldDataType.Number, KeyboardInputMode.Decimal],
            [FieldDataType.Percent, KeyboardInputMode.Decimal],
            [FieldDataType.Name, KeyboardInputMode.Text],
            [FieldDataType.Date, KeyboardInputMode.Numeric],
            [FieldDataType.Time, KeyboardInputMode.Text],
            [FieldDataType.Email, KeyboardInputMode.Email],
            [FieldDataType.Phone, KeyboardInputMode.Telephone],
            [FieldDataType.Postcode, KeyboardInputMode.Numeric],
            [FieldDataType.Url, KeyboardInputMode.Url],
            [FieldDataType.Abn, KeyboardInputMode.Numeric],
            [FieldDataType.NumberPlate, KeyboardInputMode.Text],
            [FieldDataType.Password, KeyboardInputMode.Text],
        ]);

        return keyboardInputModeMap.get(dataType);
    }
}
