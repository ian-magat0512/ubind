import { Component } from "@angular/core";
import { FormBuilder, FormGroup } from "@angular/forms";
import { FieldEventLogRegistry } from "@app/services/debug/field-event-log-registry";

/**
 * A component to configure which fields should have event logs output to the console.
 */
@Component({
    selector: 'field-event-log',
    templateUrl: './field-event-log.component.html',
    styleUrls: ['./field-event-log.component.scss'],
})
export class FieldEventLogComponent {

    public form: FormGroup = new FormGroup({});
    public fieldPaths: Array<string>;

    public constructor(
        formBuilder: FormBuilder,
        fieldEventLogRegistry: FieldEventLogRegistry,
    ) {
        this.form = formBuilder.group({
            fieldPath: [''],
        });
        this.fieldPaths = fieldEventLogRegistry.fieldPaths;
    }

    public addButtonClicked(): void {
        if (this.form.value['fieldPath'] == '') {
            return;
        }
        const fieldPath: string = this.form.value['fieldPath'].trim();
        if (this.fieldPaths.includes(fieldPath)) {
            return;
        }
        this.fieldPaths.push(fieldPath);
        this.form.reset();
    }

    public removeButtonClicked(index: number): void {
        this.fieldPaths.splice(index, 1);
    }
}
