/**
 * Configuration settings for the component, which is part of the WorkingConfiguration.
 */
export interface Settings {
    financial: {
        defaultCurrency: string;
    };
    paymentForm: object;
}

/**
 * A google font definition
 */
export interface GoogleFont {
    usage: string; // typically one of "bodyText", "headings", "labels"
    family: string;
    weight: string; // e.g. 400
}
