import { Errors } from "@app/models/errors";
import { TextCase } from "@app/models/text-case.enum";
import * as ChangeCase from "change-case";
import { Options } from "change-case";

/**
 * Export string helper class.
 * TODO: Write a better class header: string helper functions.
 */
export class StringHelper {
    public static capitalizeFirstLetter(string: string): string {
        return string.charAt(0).toUpperCase() + string.slice(1);
    }

    public static equalsIgnoreCase(string1: string, string2: string): boolean {
        return string1.toUpperCase() == string2.toUpperCase();
    }

    public static isNullOrEmpty(string: string): boolean {
        return string == null || string === '';
    }

    public static isNullOrWhitespace(string: string): boolean {
        return string == null || (typeof string == 'string' && string.trim() == '');
    }

    public static containsIgnoreCase(haystack: string, needle: string): boolean {
        if (!haystack || !needle) {
            return false;
        }
        return haystack.toUpperCase().indexOf(needle.toUpperCase()) != -1;
    }

    /**
     * Until ES6 support is wide enough, this function can be used instead of the
     * string.replaceAll() in built javascript function
     * @param str the source string
     * @param substr the substring you want to replace
     * @param newSubstr the replacement string
     */
    public static replaceAll(str: string, substr: string, newSubstr: string): string {
        return str.split(substr).join(newSubstr);
    }

    /**
     * Converts text to the specified case
     * @param str the string to convert
     * @param textCase the case to convert to, see TextCase
     * @param splitRegexp a regular expression which can be passed to split the string into words. If not passed a
     * default regexp is used, which splits on capitalisation changes and before the start of numbers.
     */
    public static toCase(str: string, textCase: TextCase, splitRegexp?: RegExp): string {
        splitRegexp = splitRegexp ? splitRegexp : /([a-z])([A-Z0-9])/g;
        switch (textCase) {
            case TextCase.Sentence:
                return ChangeCase.sentenceCase(str, { splitRegexp: splitRegexp });
            case TextCase.Title:
                return ChangeCase.capitalCase(str, { splitRegexp: splitRegexp });
            case TextCase.Camel: {
                let result: string = ChangeCase.camelCase(str, { splitRegexp: splitRegexp });
                return result.replace(/_/g, '');
            }
            default:
                throw Errors.General.NotImplemented(
                    `Converting a string to ${textCase} case has not been implemented. `);
        }
    }

    public static toCamelCase(str: string): string {
        return this.toCase(str, TextCase.Camel);
    }

    public static toTitleCase(str: string): string {
        return this.toCase(str, TextCase.Title);
    }

    public static toInputCase(input: string, inputCase: string): string {
        switch (inputCase) {
            case TextCase.Camel: {
                let camelCase: string = input.replace(/([a-z])([A-Z])/g, '$1 $2')
                    .replace(/([A-Z])([A-Z][a-z])/g, '$1 $2')
                    .replace(/([a-z])([0-9])/gi, '$1 $2')
                    .replace(/([0-9])([a-z])/gi, '$1 $2');
                return camelCase;
            }
            case TextCase.Pascal: {
                let pascalCase: string = input.replace(/([a-z])([A-Z])/g, '$1 $2')
                    .replace(/([A-Z])([A-Z][a-z])/g, '$1 $2')
                    .replace(/([a-z])([0-9])/gi, '$1 $2')
                    .replace(/([0-9])([a-z])/gi, '$1 $2');
                return pascalCase;
            }
            case TextCase.Kebab: {
                let kebabCase: string = input.replace(/[\s]/g, '')
                    .replace(/-/g, ' ');
                return kebabCase;
            }
            case TextCase.Snake: {
                let snakeCase: string = input.replace(/[\s]/g, '')
                    .replace(/_/g, ' ');
                return snakeCase;
            }
            default: {
                let splitString: Array<string> = input.toLowerCase().split(/[\s]/g);
                return splitString.filter((str: string) => str).join(' ');
            }
        }
    }

    public static toInputCaseOptions(inputCase: string): Options {
        let stripRegexp: RegExp = /[^a-zA-Z0-9\-\_\.]/gi;

        switch (inputCase) {
            case TextCase.Camel: {
                return {
                    splitRegexp: /([a-z])([A-Z0-9])/g,
                    stripRegexp: stripRegexp,

                };
            }
            case TextCase.Pascal: {
                return {
                    splitRegexp: /([a-z])([A-Z0-9])/g,
                    stripRegexp: stripRegexp,
                };
            }
            case TextCase.Kebab: {
                return {
                    splitRegexp: /[-]/g,
                    stripRegexp: stripRegexp,
                };
            }
            case TextCase.Snake: {
                return {
                    splitRegexp: /[_]/g,
                    stripRegexp: stripRegexp,
                };
            }
            default:
                return {
                    stripRegexp: stripRegexp,
                };
        }
    }

    public static isSingleWord(input: string): boolean {
        return input.split(" ").length == 1;
    }
}
