import { OnInit } from "@angular/core";
import { takeUntil } from 'rxjs/operators';
import { KeyboardInputMode } from "@app/models/keyboard-input-mode.enum";
import { FieldConfiguration } from "@app/resource-models/configuration/fields/field.configuration";
import { Field } from "./field";
import { KeyboardInputModeHelper } from '@app/helpers/keyboard-input-mode.helper';
import { FieldDataType } from "@app/models/field-data-type.enum";
import { StringHelper } from "@app/helpers/string.helper";

/**
 * Export keyboard input field
 * This class is used to identify which keyboard to popup based on data type.
 */
export abstract class KeyboardInputField extends Field implements OnInit {

    public keyboardInputMode: KeyboardInputMode;

    protected onConfigurationUpdated(): void {
        super.onConfigurationUpdated();
        this.eventService.getFieldConfigUpdatedObservable(this.fieldKey).pipe(takeUntil(this.destroyed))
            .subscribe((configs: { old: FieldConfiguration; new: FieldConfiguration}) => {
                this.setKeyboardInput(configs.new.keyboardInputMode);
            });
    }

    protected initialiseField(): void {
        super.initialiseField();
        this.setKeyboardInput(this.field.templateOptions.fieldConfiguration.keyboardInputMode);
    }

    private setKeyboardInput(customKeyboardInputMode: KeyboardInputMode) {
        const dataType: FieldDataType =
            StringHelper.capitalizeFirstLetter(this.interactiveFieldConfiguration.dataType) as FieldDataType;
        this.keyboardInputMode = customKeyboardInputMode
            ?? KeyboardInputModeHelper.getInputMode(FieldDataType[dataType]);
    }
}
