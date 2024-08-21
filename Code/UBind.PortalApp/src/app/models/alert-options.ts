/**
 * Interface for alert dialog options
 */
export interface AlertOptions {
    header: string;
    subHeader: string;
    buttons?: Array<AlertButton>;
    inputs?: Array<AlertInput>;
}

/**
 * Represents an alert input
 */
export interface AlertInput {
    type: string;
    label: string;
    value: string;
    checked: boolean;
    handler?(): any;
}

/**
 * Represets a button on an alert dialog
 */
export interface AlertButton {
    text: string;
    role?: string;
    cssClass?: string;
    handler?(param?: any): any;
}
