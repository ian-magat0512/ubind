import { FormControl } from '@angular/forms';
import { FormValidatorHelper } from './form-validator.helper';

describe('StylesheetUrlValidator', () => {
    let validator: typeof FormValidatorHelper = FormValidatorHelper;
    it('should have no error when url is valid', () => {
        let control: FormControl = new FormControl('', validator.stylesheetUrlValidator());
        control.setValue('https://app.ubind.com.au/abc/123/d.css');
        expect(control.errors).toBeNull();

        control.setValue('https://app.ubind.com.au/assets/mercurien/ride-protect/development/portal.css');
        expect(control.errors).toBeNull();

        control.setValue('/assets/carl/dev/development/portal.css');
        expect(control.errors).toBeNull();
    });

    it('should have error when url is invalid', () => {
        let control: FormControl = new FormControl('', validator.stylesheetUrlValidator());
        control.setValue('http://example.w3.org/%a');
        expect(control.errors.invalidStylesheetUrl).toBeTruthy();

        control.setValue('https://app.ubind.com.au/assets/mercurien/ride-protect/develop ment/portal.css');
        expect(control.errors.invalidStylesheetUrl).toBeTruthy();

        control.setValue('assets/carl/dev/development/portal.css');
        expect(control.errors.invalidStylesheetUrl).toBeTruthy();
    });
});
