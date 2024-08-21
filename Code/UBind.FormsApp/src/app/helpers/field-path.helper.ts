/**
 * Utilities for working with field paths
 */
export class FieldPathHelper {

    public static getRepeatingIndex(fieldPath: string): number {
        let openSquareBracketPos: number = fieldPath.indexOf('[');
        let closeSquareBracketPos: number = fieldPath.indexOf(']');
        let index: string = fieldPath.substring(openSquareBracketPos + 1, closeSquareBracketPos);
        return parseInt(index, 10);
    }

    public static getFieldKey(fieldPath: string): string {
        if (fieldPath.indexOf('.') > -1) {
            return fieldPath.substring(fieldPath.indexOf('.') + 1);
        } else {
            return fieldPath;
        }
    }
}
