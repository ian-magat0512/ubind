declare global {
    /**
     * A definition of the browser window object, so we can reference it in typescript.
     */
    // eslint-disable-next-line no-unused-vars
    interface Window {
        chrome: any;
        // eslint-disable-next-line @typescript-eslint/naming-convention
        Intl: any;
    }

    /**
     * A definition of the browser navigator object, so we can reference it in typescript.
     */
    // eslint-disable-next-line no-unused-vars
    interface Navigator {
        userLanguage: any;
    }
}

/**
 * Export locale class.
 * This class manage locale the string.
 */
export class Locale {

    private locale: any;
    private localeString: string;

    public constructor() {
        this.determineLocale();
        this.localeString = this.formatLocale(this.locale);
    }

    private determineLocale(): string {
        let locale: any;
        if (window.chrome && window.chrome.runtime && typeof window.chrome.runtime.getManifest == 'function') {
            locale = window.chrome.runtime.getManifest();
            if (locale && locale.current_locale) {
                this.locale = locale.current_locale;
                return;
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
                this.locale = locale[1];
                return;
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
                this.locale = (process.env.LANG || process.env.LANGUAGE || String())
                    .replace(/[.:].*/, '')
                    .replace('_', '-');
                return;
            }
        }

        this.locale = locale;
    }

    private formatLocale(locale: string): string {
        if (typeof locale != 'string') return locale;
        // 'en-US-u-VA-posix'.split('-').slice(0, 2)
        // ["en", "US"]
        return locale.split('-').slice(0, 2).map((chunk: any, index: number) => {
            // en[0]-US[1] <- chunk[1].toUpperCase()
            if (index != 0 && chunk.length == 2) return chunk.toUpperCase();
            return chunk;
        }).join('-');
    }

    public getLocale(): string {
        return this.localeString;
    }

    public getLanguageCode(locale: string): string {
        let localeString: string = locale ? locale : this.getLocale();
        const localeCode: string = localeString && localeString.length >= 2
            ? localeString.split('-')[0] : '';
        return localeCode.toLowerCase();
    }
}
