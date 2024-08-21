import {  } from "jasmine";
import { FormControl } from '@angular/forms';
import { CustomValidators } from "./custom-validators";

describe('CustomValidators.optionalEmail', () => {

    it('returns null when control has no value', () => {
        expect(CustomValidators.optionalEmail(new FormControl(''))).toBe(null);
    });

    it('returns null when control has valid email address', () => {
        expect(CustomValidators.optionalEmail(new FormControl('foo@example.com'))).toBe(null);
    });

    it('returns email error when control has invalid email address', () => {
        expect(CustomValidators.optionalEmail(new FormControl('foo'))).toEqual({ email: true });
    });
});

describe('CustomValidators.emailWithNamePattern', () => {

    // valid entries should return null 
    it('returns null when control has no value', () => {
        let result: any = CustomValidators.emailWithNamePattern(new FormControl(''));
        expect(result).toBe(null);
    });

    it('returns null when control has valid email address', () => {
        let result: any = CustomValidators.emailWithNamePattern(new FormControl('admin@ubind.io'));
        expect(result).toBe(null);
    });

    it('returns null when control has valid name and email address', () => {
        let result: any = CustomValidators.emailWithNamePattern(new FormControl('Administrator <admin@ubind.io>'));
        expect(result).toBe(null);
    });

    it('returns null when control has valid first and last name and email address', () => {
        let result: any = CustomValidators.emailWithNamePattern(new FormControl('Admin User <admin@ubind.io>'));
        expect(result).toBe(null);
    });

    it('returns null when control has valid name with dash and email address', () => {
        let result: any = CustomValidators.emailWithNamePattern(new FormControl('no-reply <admin@ubind.io>'));
        expect(result).toBe(null);
    });

    it('returns null when control has valid name with dash and email address with dash', () => {
        let result: any = CustomValidators.emailWithNamePattern(new FormControl('no-reply <admin-sales@ubind.io>'));
        expect(result).toBe(null);
    });

    // invalid entries should return false
    it('returns false when control has invalid email address', () => {
        let result: any = CustomValidators.emailWithNamePattern(new FormControl('user'));
        expect(result.invalidEmailWithNamePattern.valid).toEqual(false);
    });

    it('returns false when control has " at name', () => {
        let result: any = CustomValidators.emailWithNamePattern(new FormControl('"user" <admin@ubind.io>'));
        expect(result.invalidEmailWithNamePattern.valid).toEqual(false);
    });

    it('returns false when control has improper name', () => {
        let result: any = CustomValidators.emailWithNamePattern(new FormControl('"test user"again<admin@ubind.io'));
        expect(result.invalidEmailWithNamePattern.valid).toEqual(false);
    });

    it('returns false when control has missing < at end of email address', () => {
        let result: any = CustomValidators.emailWithNamePattern(new FormControl('test user admin@ubind.io>'));
        expect(result.invalidEmailWithNamePattern.valid).toEqual(false);
    });

    it('returns false when control has missing > at end of email address', () => {
        let result: any = CustomValidators.emailWithNamePattern(new FormControl('test user <admin@ubind.io'));
        expect(result.invalidEmailWithNamePattern.valid).toEqual(false);
    });

    it('returns false when control has missing " at end of name', () => {
        let result: any = CustomValidators.emailWithNamePattern(new FormControl('"test user <admin@ubind.io>'));
        expect(result.invalidEmailWithNamePattern.valid).toEqual(false);
    });

    it('returns false when control has missing " at start of name', () => {
        let result: any = CustomValidators.emailWithNamePattern(new FormControl('test user" <admin@ubind.io>'));
        expect(result.invalidEmailWithNamePattern.valid).toEqual(false);
    });

    it('returns false when control has improper " at name', () => {
        let result: any = CustomValidators.emailWithNamePattern(new FormControl('u"se"r <admin@ubind.io>'));
        expect(result.invalidEmailWithNamePattern.valid).toEqual(false);
    });
});
