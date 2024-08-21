import { getLocaleCurrencyCode, registerLocaleData } from "@angular/common";
import { Injectable } from "@angular/core";
import { localeCurrencyCode } from "@app/models/locale-currency-codes";

/**
 * This class was created to centralize the management of locales in the application, making it easier
 * to maintain and extend as new locales are added or existing ones are updated.
 *
 * This class is responsible for handling locale information, derived from the user's browser.
 * It provides an easy-to-use interface for accessing and manipulating locales for formatting various types of data,
 * such as currencies and numbers according to the user's locale preferences.
 *
 * The LocaleService class is used by various parts of the code and any other components that need to access,
 * or manipulate locale-specific data such as currency, number, and date formats.
 *
 * How to use:
 * - To use this class, first initialise by calling initialiseBrowserLocaleAndCurrency with a currency code that is
 *   either provided by a config file, the FormService, or a default value specified by a developer.
 *   Then you should be able to access the various properties such as:
 *
 * @property {string} locale - The browser's locale.
 * @property {string} currencyCode - The currency code set in a config file, FormService, or as a default value.
 * @property {string} currencyLocale - The locale of the currency code that was provided during initialisation.
 * @property {Array<string>} registeredLocales - An array of locales that were successfully loaded and registered.
 *
 *
 * - After initialisation, you should also be able to call the different methods that would allow you to use
 *   different locales based on preference.
 *
 * @method initialiseOrGetCurrencyLocale - Registers and/or returns a locale based on a currency code synchronously.
 * @method initialiseOrGetCurrencyLocaleAsync - Registers and/or returns a locale based on a currency code
 *         asynchronously.
 * @method getLocaleCodeFromBrowser - Retrieves the user's browser locale.
 * @method getLanguageCode - Returns the language code either from a provided locale string or from the service's
 *         initialised locale.
 *
 *
 * When using the LocaleService, it is important to be aware of the following complexities:
 * - Proper handling of synchronous and asynchronous registration of locales
 * - Proper handling of character encoding and special characters in different locales
 * - Efficient memory management, as loading multiple large locales into memory can be resource-intensive
 *
 * Additional Notes:
 * - The LocaleService class is designed to be flexible and extensible, allowing developers to easily add
 *   support for new locales or modify existing ones as needed.
 * - Be aware of the dependencies between the LocaleService and other modules, as changes to the
 *   LocaleService might affect the behavior of other parts of the application specifically those
 *   that require locale-specific formats for various types of data.
 * - If a currency code was deemed unknown or unofficial but is necessary, add it to the
 *   locale-currency-code.ts file and provide the correct defaultLocale based on
 *   Angular's Internalization or i18n https://angular.io/guide/i18n-common-locale-id
 */
@Injectable({
    providedIn: 'root',
})
export class LocaleService {

    private currencyCode: string;
    private browserLocale: string;
    private localeString: string;
    private currencyLocale: string;
    private registeredLocales: Array<string> = [];

    /**
     * Initialises the user's browser locale and provided currency code.
     * This method should only be used upon application startup.
     * It is not recommended to use this method solely to register a currency's locale into memory.
     * @param currencyCode (optional)
     * @returns a Promise that provides the locale of the currency code if it was provided.
     */
    public async initialiseBrowserLocaleAndCurrency(currencyCode: string): Promise<void> {
        this.currencyCode = currencyCode;
        this.browserLocale = this.getLocaleCodeFromBrowser();
        return this.loadAndAssignLocaleProperties(this.browserLocale).then((resolve: string) => {
            if (resolve) {
                this.browserLocale = resolve;
            }
            if (currencyCode) {
                return this.determineCurrencyCodeLocale(this.currencyCode);
            }
        });
    }

    /**
     * Mainly used by the initialiseBrowserLocaleAndCurrency method.
     * This method determines whether the provided param matches the currency code of the user's browser locale.
     * If not, it will provide a locale based on a predetermined list.
     * @param currencyCode
     * @returns a void Promise
     */
    private async determineCurrencyCodeLocale(currencyCode: string): Promise<void> {
        if (getLocaleCurrencyCode(this.browserLocale) == currencyCode) {
            this.currencyLocale = this.browserLocale;
        } else {
            let defaultLocale: string = localeCurrencyCode[currencyCode]?.defaultLocale;

            if (defaultLocale) {
                this.currencyLocale = defaultLocale;
                return this.loadAndAssignLocaleProperties(this.currencyLocale).then((resolve: string) => {
                    if (resolve) {
                        this.currencyLocale = resolve;
                    }
                });
            } else {
                throw new Error(`The currency code '${currencyCode}' is unknown or an unofficial code.`);
            }
        }
    }

    /**
     * The async version of the initialiseOrGetCurrencyLocale function.
     * This method asynchronously loads and registers the locale of the provided currency code.
     * If the param does not correspond with this service's currency code,
     * it will provide a locale based on a predetermined list.
     * This should not be used on non asynchronous components.
     * e.g. Angular slider translate option.
     * @param currencyCode
     * @returns a Promise that provides a locale.
     */
    public async initialiseOrGetCurrencyLocaleAsync(currencyCode: string): Promise<string> {
        if (currencyCode !== this.currencyCode) {
            let defaultLocale: string = localeCurrencyCode[currencyCode]?.defaultLocale;

            if (defaultLocale) {
                return this.loadLocaleFileIntoMemory(defaultLocale).then((resolve: string) => {
                    if (resolve) {
                        this.addToRegisteredLocales(resolve);
                    }
                    return defaultLocale;
                });
            } else {
                throw new Error(`The currency code '${currencyCode}' is unknown or an unofficial code.`);
            }
        } else {
            return Promise.resolve(this.currencyLocale);
        }
    }

    /**
     * This method loads and registers the locale of the provided currency code.
     * If locale-specific formatting is not immediately necessary, utilize this method.
     * If the param does not correspond with this service's currency code,
     * it will provide a locale based on a predetermined list.
     * @param currencyCode 
     * @returns the locale of the currency code.
     */
    public initialiseOrGetCurrencyLocale(currencyCode: string): string {
        if (currencyCode !== this.currencyCode) {
            let defaultLocale: string = localeCurrencyCode[currencyCode]?.defaultLocale;

            if (defaultLocale) {
                this.loadLocaleFileIntoMemory(defaultLocale).then((resolve: string) => {
                    if (resolve) {
                        this.addToRegisteredLocales(resolve);
                    }
                    return defaultLocale;
                });
            } else {
                throw new Error(`The currency code '${currencyCode}' is unknown or an unofficial code.`);
            }
        } else {
            return this.currencyLocale;
        }

    }

    public getLocaleCodeFromBrowser(): string {
        let locale: any;
        let chromeWindow: any = window.chrome;
        if (chromeWindow && chromeWindow.runtime && typeof chromeWindow.runtime.getManifest == 'function') {
            locale = chromeWindow.runtime.getManifest();
            if (locale && locale.current_locale) {
                return locale.current_locale;
            }
        }

        locale = (window.navigator && (
            (window.navigator.languages && window.navigator.languages[0]) ||
            window.navigator.language ||
            window.navigator.userLanguage
        ));

        if (!locale && window.navigator && window.navigator.userAgent) {
            locale = window.navigator.userAgent.match(/;.(\w+-\w+)/i);
            if (locale) {
                return locale[1];
            }
        }

        if (!locale) {
            locale = (window.clientInformation || Object.create(null)).language;
        }

        if (!locale) {
            if (window.Intl && typeof window.Intl.DateTimeFormat == 'function') {
                locale = window.Intl.DateTimeFormat().resolvedOptions &&
                    window.Intl.DateTimeFormat().resolvedOptions().locale;
            }
            if (!locale && ['LANG', 'LANGUAGE'].some(Object.hasOwnProperty, process.env)) {
                return (process.env.LANG || process.env.LANGUAGE || String())
                    .replace(/[.:].*/, '')
                    .replace('_', '-');
            }
        }

        return locale;
    }

    private async loadAndAssignLocaleProperties(locale: string): Promise<any> {
        return this.loadLocaleFileIntoMemory(locale).then(
            (resolve: any) => {
                if (resolve) {
                    this.localeString = this.capitaliseLocaleCountryCode(resolve);
                    this.addToRegisteredLocales(resolve);
                    return resolve;
                }
            },
        );
    }

    private loadLocaleFileIntoMemory(locale: string): Promise<any> {
        if (!this.getRegisteredLocales().includes(locale)) {
            return import(`@/../@angular/common/locales/${locale}.mjs`).then(
                (localeModule: any) => {
                    registerLocaleData(localeModule.default, locale);
                    return locale;
                },
                () => {
                    /**
                     * Regional locale is not listed because it is the parent locale.
                     * The reason why we have "en" but not "en-US".
                     * The same for french, "fr" but not "fr-FR".
                     * This applies to "pt-BR" being "pt" only.
                     */
                    const fallbackLocale: string = locale.split('-')[0];
                    return import(`@/../@angular/common/locales/${fallbackLocale}.mjs`).then(
                        (localModule: any) => {
                            registerLocaleData(localModule.default, fallbackLocale);
                            return fallbackLocale;
                        });
                },
            );
        } else {
            return Promise.resolve();
        }
    }

    private addToRegisteredLocales(locale: string): void {
        if (!this.registeredLocales.includes(locale)) {
            this.registeredLocales.push(locale);
        }
    }

    private capitaliseLocaleCountryCode(locale: string): string {
        if (typeof locale != 'string') return locale;
        return locale.split('-').map((chunk: any, index: number) => {
            if (index != 0 && chunk.length == 2) return chunk.toUpperCase();
            return chunk;
        }).join('-');
    }

    public getLanguageCode(locale: string = null): string {
        let localeString: string = locale ? locale : this.getLocale();
        const localeCode: string = localeString && localeString.length >= 2
            ? localeString.split('-')[0] : '';
        return localeCode.toLowerCase();
    }

    public getLocale(): string {
        return this.localeString;
    }

    public getCurrencyLocale(): string {
        return this.currencyLocale;
    }

    public getCurrencyCode(): string {
        return this.currencyCode;
    }

    public getRegisteredLocales(): Array<string> {
        return this.registeredLocales;
    }
}
