import { Component, Input } from "@angular/core";
import { ApplicationService } from "@app/services/application.service";
import { Field } from "../field";

/**
 * Export field debug component class.
 * TODO: Write a better class header: field debug function.
 */
@Component({
    selector: 'field-debug',
    templateUrl: './field-debug.html',
})
export class FieldDebugComponent {

    public constructor(public applicationService: ApplicationService) {
    }

    @Input('field')
    public field: Field;
}
