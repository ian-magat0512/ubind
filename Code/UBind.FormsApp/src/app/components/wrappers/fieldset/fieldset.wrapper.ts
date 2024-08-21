import { AfterViewInit, Component, HostBinding } from "@angular/core";
import { FieldType } from "@app/models/field-type.enum";
import { merge } from "rxjs";
import { takeUntil } from "rxjs/operators";
import { Wrapper } from "../wrapper";

/**
 * This wrapper was created when upgrading from formly v4 to v5.
 * They removed the deprecated fieldset wrapper, so we need to add our own
 * custom wrapper to replace it.
 */
@Component({
    selector: 'formly-wrapper-fieldset',
    templateUrl: './fieldset.wrapper.html',
})
export class FieldsetWrapper extends Wrapper implements AfterViewInit {

    @HostBinding('class.form-group')
    public formGroupClass: boolean = true;

    @HostBinding('class.has-error')
    public hasErrorClass: boolean = false;

    public ngAfterViewInit(): void {
        if (this.fieldInstance.fieldType == FieldType.Repeating) {
            // we don't put the 'has-error' css class on repeating fields, because it would cause all fields underneath
            // to to show as invalid. We only want the individual fields to show as invalid, not all.
            // The reason for this is that bootstrap css has a css selector: ".has-error .form-control" which sets
            // a red border on invalid fields. We can't change this css since it's standard bootstrap, so we have to be
            // careful where we let the has-error css class turn up.
            return;
        }
        if (this.formControl) {
            merge(
                this.formControl.statusChanges,
                this.formControl.valueChanges,
                this.fieldInstance.onTouchedSubject,
            )
                .pipe(takeUntil(this.destroyed))
                .subscribe(() => {
                    const isInvalid: boolean = this.formControl.invalid && this.formControl.touched;
                    this.hasErrorClass = (this.showError || isInvalid);
                });
        }
    }
}
