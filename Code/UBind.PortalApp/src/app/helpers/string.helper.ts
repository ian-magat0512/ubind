import { Injectable } from '@angular/core';
import { Locale } from '@app/helpers/locale';
import * as ChangeCase from 'change-case';

/**
 * Common string utilities.
 */
@Injectable({
    providedIn: 'root',
})
export class StringHelper {
    private locale: Locale;

    public constructor() {
        this.locale = new Locale();
    }

    public static isNullOrWhitespace(string: string): boolean {
        return string == null || (typeof string == 'string' && string.trim() == '');
    }

    public static capitalizeFirstLetter(string: string): any {
        return string.charAt(0).toUpperCase() + string.slice(1);
    }

    public static toTitleCase(value: string): string {
        const splitValue: Array<string> = value.toLowerCase().split(' ');
        if (splitValue.length) {
            return splitValue.map((word: string) => this.capitalizeFirstLetter(word)).join(' ');
        }
        return this.capitalizeFirstLetter(value);
    }

    public static camelCase(str: any): string {
        if (!str) {
            return '';
        }
        const val: any = str.replace(/(?:^\w|[A-Z]|\b\w)/g, (word: string, index: number) => {
            return index === 0 ? word.toLowerCase() : word.toUpperCase();
        }).replace(/\s+/g, '');

        return val;
    }

    public static toKebabCase(valueToBeConverted: any): string {
        if (StringHelper.isNullOrEmpty(valueToBeConverted)) {
            return valueToBeConverted;
        }
        let sCase: string = ChangeCase.sentenceCase(valueToBeConverted);
        let kebabCase: string = sCase.split(" ").join("-").toLowerCase();
        return kebabCase;
    }

    public static toPascalCase(valueToBeConverted: string): string {
        if (StringHelper.isNullOrEmpty(valueToBeConverted)) {
            return valueToBeConverted;
        }
        let pCase: string = ChangeCase.pascalCase(valueToBeConverted);
        return pCase;
    }

    public static camelToSentenceCase(str: string): string {
        if (!str) {
            return '';
        }

        // adding space between strings
        const result: string = str.replace(/([A-Z])/g, ' $1');

        // converting first character to uppercase and join it to the final string
        let final: string = result.charAt(0).toUpperCase() + result.slice(1);
        final = final.replace(/ +(?= )/g, '').trim();
        return final;
    }

    public static beautify(key: string): string {
        if (key) {
            key = key.replace(/([A-Z]+)/g, ' $1').trim();
            key = key.replace(/([0-9]+)/g, ' $1').trim();
            key = key.charAt(0).toUpperCase() + key.slice(1);
            key = key.replace(/ +(?= )/g, '').trim();
        }

        return key;
    }

    public static removeDashFromKebabCase(kebabCase: string): string {
        const regex: RegExp = /-/g;
        return kebabCase.replace(regex, ' ');
    }

    public static isNullOrEmpty(val: string): boolean {
        return val == null || val == '';
    }

    public static withTrailing(input: string, trailing: string): string {
        if (!input.endsWith(trailing)) {
            return input + trailing;
        }
        return input;
    }

    public static withoutTrailing(input: string, trailing: string): string {
        if (input.endsWith(trailing)) {
            return input.substring(0, input.length - trailing.length);
        }
        return input;
    }

    public equalsIgnoreCase(string1: string, string2: string): boolean {
        return string1 && string2 ?
            string1.localeCompare(string2, this.locale.getLocale(), { sensitivity: 'accent' }) === 0 :
            string1 == string2;
    }
}
