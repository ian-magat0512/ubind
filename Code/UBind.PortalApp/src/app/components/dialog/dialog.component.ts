import { Component, Inject } from '@angular/core';
import { FormControl, FormGroup, Validators } from '@angular/forms';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { DialogConfiguration, DialogData, DialogInput } from '@app/models/dialog-config';

/**
 * Component for dialog boxes.
 */
@Component({
    selector: 'app-dialog',
    templateUrl: './dialog.component.html',
    styleUrls: ["./dialog.component.scss"],
})
export class DialogComponent {

    public configuration: DialogConfiguration;
    public inputForm: FormGroup;

    public constructor(
        public dialogRef: MatDialogRef<DialogComponent>,
        @Inject(MAT_DIALOG_DATA) public data: DialogData,
    ) {
        let group: any = {};

        data.configuration.inputs.forEach((input: DialogInput) => {
            group[input.key] = input.required ? new FormControl(input.value ? input.value : '', Validators.required)
                : new FormControl(input.value ? input.value : '');
        });

        this.inputForm = new FormGroup(group);
        this.configuration = data.configuration;
    }

    public click(handler: (data: any) => void): void {
        if (handler) {
            handler(this.inputForm.value);
        } else {
            this.dialogRef.close();
        }
    }
}
