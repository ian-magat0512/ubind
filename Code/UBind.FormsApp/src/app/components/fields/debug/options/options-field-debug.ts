import { Component, Input } from "@angular/core";
import { OptionsField } from "../../options/options.field";

/**
 * Export options field debug component class.
 * This class manage Option field debug function.
 */
@Component({
    selector: 'options-field-debug',
    templateUrl: './options-field-debug.html',
})
export class OptionsFieldDebugComponent {

    @Input('field')
    public field: OptionsField;
}
