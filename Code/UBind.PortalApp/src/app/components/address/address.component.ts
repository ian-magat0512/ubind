import {
    Component, OnInit, OnDestroy, Input, Output, EventEmitter, AfterViewInit, ChangeDetectorRef,
} from "@angular/core";
import {
    FormGroup, FormControl, NG_VALUE_ACCESSOR, NG_VALIDATORS, ValidatorFn,
} from "@angular/forms";
import { Subscription } from "rxjs";
import { FormValidatorHelper } from "@app/helpers/form-validator.helper";
import { AddressPart } from "@app/models/address-enum";
import { FieldOption } from "@app/models/entity-edit-field-option";

/**
 * Interface for address form value.
 */
export interface AddressFormValue {
    address: string;
    suburb: string;
    state: string;
    postcode: string;
}

/**
 * Export address component class
 * This class is for address fields.
 */
@Component({
    selector: 'app-address',
    templateUrl: `./address.component.html`,
    providers: [
        {
            provide: NG_VALUE_ACCESSOR,
            useExisting: AddressComponent,
            multi: true,
        },
        {
            provide: NG_VALIDATORS,
            useExisting: AddressComponent,
            multi: true,
        },
    ],
    styleUrls: [
        '../detail-list-item-edit-form/detail-list-item-edit-form.component.scss',
        '../../../assets/css/form-toolbar.scss',
    ],
})
export class AddressComponent implements OnInit, AfterViewInit, OnDestroy {
    @Input() public name: string;
    @Input() public form: FormGroup;
    @Output() public addressKeyup: EventEmitter<any> = new EventEmitter();
    @Output() public addressBlur: EventEmitter<any> = new EventEmitter();
    @Output() public clearLabel: EventEmitter<any> = new EventEmitter();

    private subscriptions: Array<Subscription> = [];
    public previousRequiredValue: boolean = false;
    public hasAddressValue: boolean = false;

    public stateOptions: Array<FieldOption> = [
        { label: 'ACT', value: 'ACT' },
        { label: 'NSW', value: 'NSW' },
        { label: 'NT', value: 'NT' },
        { label: 'QLD', value: 'QLD' },
        { label: 'SA', value: 'SA' },
        { label: 'TAS', value: 'TAS' },
        { label: 'VIC', value: 'VIC' },
        { label: 'WA', value: 'WA' },
    ];

    public constructor(private changeDetectorRef: ChangeDetectorRef) {
    }

    public ngAfterViewInit(): void {
        this.form.updateValueAndValidity();
        this.checkAndFixValidator();
        this.checkFormFieldsValidity();
        this.checkAddressValues();

        this.changeDetectorRef.detectChanges();
    }

    public ngOnInit(): void {
        this.form.setValidators([this.validate.bind(this)]);
        this.subscriptions.push(
            this.form.valueChanges.subscribe((value: any) => {
                this.checkAddressValues();
            }),
        );
    }

    public ngOnDestroy(): void {
        this.subscriptions = null;
    }

    public onAddressKeyup(controlName: string, event?: any): void {
        let value: string;
        if (event.value) {
            value = event.value;
        } else {
            value = (document.getElementById(this.name + controlName) as HTMLInputElement).value;
        }
        this.checkAndFixValidator(value);
        this.addressKeyup.emit(value);
    }

    public onAddressBlur(controlName: string): void {
        if (this.form.get(AddressPart.StreetAddress).value) {
            this.addressBlur.emit(controlName);
        } else {
            let shouldRemarkControlsAsUntouched: boolean = true;
            Object
                .keys(this.form.controls)
                .filter((key: string) => key !== AddressPart.StreetAddress)
                .forEach((key: string) => {
                    if (this.form.get(key).value) {
                        shouldRemarkControlsAsUntouched = false;
                    }
                });

            if (shouldRemarkControlsAsUntouched) {
                this.markControlsAsUntouched();
            }
        }
    }

    private markControlsAsUntouched(): void {
        Object.keys(this.form.controls).forEach((key: string) => {
            this.form.get(key).markAsUntouched();
        });
        this.clearLabel.emit();
    }

    public checkAndFixValidator(value: string = ''): void {
        let streetValue: string = this.getElementValue(this.name + AddressPart.StreetAddress);
        let suburbValue: string = this.getElementValue(this.name + AddressPart.Suburb);
        let stateValue: string = this.form.get('state').value;
        let postcodeValue: string = this.getElementValue(this.name + AddressPart.Postcode);
        let isRequired: boolean = streetValue.length > 0 ||
            suburbValue.length > 0 ||
            stateValue.length > 0 ||
            postcodeValue.length > 0;

        this.form.get(AddressPart.StreetAddress).setValidators(FormValidatorHelper.streetAddress(isRequired));
        this.form.get(AddressPart.Suburb).setValidators(FormValidatorHelper.addressSuburb(isRequired));
        this.form.get(AddressPart.State).setValidators(isRequired ? FormValidatorHelper.required() : null);
        this.form.get(AddressPart.Postcode).setValidators(FormValidatorHelper.addressPostcode(isRequired));

        if (stateValue && postcodeValue) {
            let validators: Array<ValidatorFn> = FormValidatorHelper.addressPostcode(isRequired);
            let newValidator: ValidatorFn = FormValidatorHelper.addressStateAndPostcode(stateValue);
            validators.push(newValidator);
            this.form.get(AddressPart.Postcode).setValidators(validators);
        }
    }

    public checkFormFieldsValidity(): void {
        Object.keys(this.form.controls).forEach((key: string) => {
            this.form.get(key).updateValueAndValidity();
            if (this.form.get(key).invalid) {
                this.form.get(key).markAsTouched();
                this.form.get(key).markAsDirty();
                this.form.markAsTouched();
                this.form.markAsDirty();
            }
        });
    }

    public checkAddressValues(): void {
        this.hasAddressValue = this.form.get('address').value
            || this.form.get('suburb').value
            || this.form.get('state').value
            || this.form.get('postcode').value
            || this.form.get('address').dirty
            || this.form.get('suburb').dirty
            || this.form.get('state').dirty
            || this.form.get('postcode').dirty;
    }

    private getElementValue(elementName: string): string {
        let value: string = "";
        let element: any = document.getElementById(elementName);
        if (element) {
            value = element["value"];
        }

        return value;
    }

    public get value(): AddressFormValue {
        return this.form.value;
    }

    public set value(value: AddressFormValue) {
        this.form.setValue(value);
    }

    private validate({ value }: FormControl): void {
        let errors: any = [];

        if (this.form.get(AddressPart.StreetAddress).errors) {
            errors.push(this.form.get(AddressPart.StreetAddress).errors);
        }

        if (this.form.get(AddressPart.Suburb).errors) {
            errors.push(this.form.get(AddressPart.Suburb).errors);
        }

        if (this.form.get(AddressPart.State).errors) {
            errors.push(this.form.get(AddressPart.State).errors);
        }

        if (this.form.get(AddressPart.Postcode).errors) {
            errors.push(this.form.get(AddressPart.Postcode).errors);
        }
        return errors;
    }
}
