/**
 * Extension methods for HTMLElement
 */
declare global {
    export interface HTMLElement {
        getStringAttributeValue(attributeName: string, valueIfNotFound?: string): string;
        getNumberAttributeValue(attributeName: string, valueIfNotFound?: number): number;
        getBooleanAttributeValue(attributeName: string, valueIfNotFound?: boolean): boolean;
    }
}

HTMLElement.prototype.getStringAttributeValue
    = function(attributeName: string, valueIfNotFound: string = undefined): string {
        const stringValue: string = this.getAttribute(attributeName);
        if (stringValue != null && stringValue != undefined && stringValue.toLowerCase() == "null") {
            return null;
        }
        return stringValue ?? valueIfNotFound;
    };

HTMLElement.prototype.getNumberAttributeValue
    = function(attributeName: string, valueIfNotFound: number = undefined): number {
        const stringValue: string = this.getAttribute(attributeName);
        if (stringValue != null && stringValue != undefined && stringValue.toLowerCase() == "null") {
            return null;
        }
        return stringValue !== null && stringValue !== undefined
            ? Number(stringValue).valueOf()
            : valueIfNotFound;
    };

HTMLElement.prototype.getBooleanAttributeValue
    = function(attributeName: string, valueIfNotFound: boolean = undefined): boolean {
        const stringValue: string = this.getAttribute(attributeName);
        if (stringValue == null) {
            return valueIfNotFound;
        }
        return stringValue.toLowerCase() == 'true';
    };

export {};
