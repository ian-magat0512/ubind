import { Injectable } from '@angular/core';
import { AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';
import { Papa, ParseError, ParseResult } from 'ngx-papaparse';

interface CsvValidatorOptions {
    size?: number;
}

/**
 * Validator service for csv text.
 */
@Injectable({
    providedIn: 'root',
})
export class CsvValidatorService {
    private defaultBytesSize: number = 20971520; // 20MB

    public constructor(private csvParser: Papa) { }

    public csvValidator(options?: CsvValidatorOptions): ValidatorFn {
        return (control: AbstractControl): ValidationErrors | null => {
            let value: string = control.value;
            const result: ParseResult = this.csvParser.parse(value);

            // Contains duplicate header
            if (!result.errors.length && result.data[0] && this.hasDuplicatesIgnoreCase(result.data[0])) {
                return { duplicateCsv: true };
            }

            if (this.isCachingEnabled(control)) {
                if (value.length > this.defaultBytesSize) {
                    return { exceedLimitCsv: true };
                }
            }

            if (!result.errors.length) {
                return null;
            }

            if (result.errors.length || !result.data.every((r: any) => r.length === result.data[0].length)) {
                if (result.data[0] && result.data[0].length == 1
                    && result.errors.filter((e: ParseError) => e.code == "UndetectableDelimiter").length) {
                    return null;
                }
                if (result.data[result.data.length - 1] && !result.data[result.data.length - 1][0]
                    && result.errors.filter((e: ParseError) => e.code == "UndetectableDelimiter").length
                    && result.data.filter((d: any) => d.length == 1).length == 1) {
                    return null;
                }
                return { invalidCsv: true };
            }

            return null;
        };
    }

    public isCachingEnabled(control: AbstractControl): boolean {
        if (control.parent) {
            const memoryCachingEnabledControl: AbstractControl = control.parent.get('memoryCachingEnabled');
            return memoryCachingEnabledControl.value;
        }
        return false;
    }

    private hasDuplicatesIgnoreCase(arr: Array<string>): boolean {
        const lowercaseMap: any = {};
        return arr.some((text: string) => {
            const lowerText: string = text.toLowerCase();
            if (lowercaseMap[lowerText]) {
                return true;
            }
            lowercaseMap[lowerText] = true;
            return false;
        });
    }
}
