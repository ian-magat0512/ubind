/**
 * Provides helper functions for any types.
 */
export class AnyHelper {
    public static hasValue(value: any): boolean {
        return value !== null && value !== undefined && value !== '';
    }

    public static hasNoValue(value: any): boolean {
        return value === null || value === undefined || value === '';
    }
}
