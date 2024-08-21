import { v4 as uuidv4 } from 'uuid';

/**
 * Helper class for UUID (Universally Unique Identifier) related functions.
 */
export class UuidHelper {
    public static readonly ValidUuidPattern: RegExp
        = /^[0-9a-f]{8}-[0-9a-f]{4}-[1-5][0-9a-f]{3}-[89ab][0-9a-f]{3}-[0-9a-f]{12}$/i;

    public static isUuid(value: string): boolean {
        return UuidHelper.ValidUuidPattern.test(value);
    }

    public static newGuid(): string {
        return uuidv4();
    }
}
