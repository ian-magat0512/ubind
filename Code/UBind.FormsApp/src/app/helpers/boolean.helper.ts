/**
 * Export boolean helper class.
 * TODO: Write a better class header: boolean functions for helper.
 */
export class BooleanHelper {
    public static fromAny(value: any): boolean {
        let type: any = typeof value;
        switch (type) {
            case 'boolean':
                return value;
            case 'string':
                return BooleanHelper.fromString(value);
            case 'number':
                return BooleanHelper.fromNumber(value);
            default:
                return false;
        }
    }

    public static fromString(value: string): boolean {
        let lowerCaseValue: string = value.toLowerCase();
        let alternateTrueWords: Array<string> = ['true', 'ok', 'yes'];
        return alternateTrueWords.includes(lowerCaseValue);
    }

    public static fromNumber(value: number): boolean {
        return value > 0;
    }
}
