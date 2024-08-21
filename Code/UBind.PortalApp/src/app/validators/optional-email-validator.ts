import { Injectable } from '@angular/core';
import { AbstractControl, ValidationErrors, Validators } from "@angular/forms";

/**
 * Export optional email validator class.
 * TODO: Write a better class header: email validator option function.
 */
@Injectable()
export class OptionalEmailValidator {
    public static get(control: AbstractControl): ValidationErrors {
        return control.value ? Validators.email(control) : null;
    }
}
