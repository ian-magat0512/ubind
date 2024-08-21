import { Errors } from '@app/models/errors';
import { RelativeJsonPointerResolver } from './relative-json-pointer-resolver';

describe('RelativeJsonPointerResolver', () => {

    it('should resolve an ancestor of the given depth: "a.b.c.d", "2" == "a.b"', () => {
        // Arrange
        let fieldPath: string = "a.b.c.d";
        let jsonPointer: string = "2";
        let expected: string = "a.b";

        // Act
        let result: string = RelativeJsonPointerResolver.resolve(fieldPath, jsonPointer);

        // Assert
        expect(result).toEqual(expected);
    });

    it('should resolve an ancestor where it\'s an array element object: "claims[0].amount", "1" == "claims[0]"', () => {
        // Arrange
        let fieldPath: string = "claims[0].amount";
        let jsonPointer: string = "1";
        let expected: string = "claims[0]";

        // Act
        let result: string = RelativeJsonPointerResolver.resolve(fieldPath, jsonPointer);

        // Assert
        expect(result).toEqual(expected);
    });

    it('should resolve an ancestor where it\'s array: "abc.claims[0].amount", "2" == "abc.claims"', () => {
        // Arrange
        let fieldPath: string = "abc.claims[0].amount";
        let jsonPointer: string = "2";
        let expected: string = "abc.claims";

        // Act
        let result: string = RelativeJsonPointerResolver.resolve(fieldPath, jsonPointer);

        // Assert
        expect(result).toEqual(expected);
    });

    it('should resolve an ancestor where it\'s an ancestor of array: "abc.claims[0].amount", "2" == "abc"', () => {
        // Arrange
        let fieldPath: string = "abc.claims[0].amount";
        let jsonPointer: string = "2";
        let expected: string = "abc.claims";

        // Act
        let result: string = RelativeJsonPointerResolver.resolve(fieldPath, jsonPointer);

        // Assert
        expect(result).toEqual(expected);
    });

    it('should add a new property to the fieldPath after resolving an ancestor '
        + ': "abc.claims[0].amount", "3/customerName" == "abc.customerName"', () => {
        // Arrange
        let fieldPath: string = "abc.claims[0].amount";
        let jsonPointer: string = "3/customerName";
        let expected: string = "abc.customerName";

        // Act
        let result: string = RelativeJsonPointerResolver.resolve(fieldPath, jsonPointer);

        // Assert
        expect(result).toEqual(expected);
    });

    it('should add a new array index to the fieldPath after resolving an ancestor '
        + ': "abc.claims[0].amount", "2/9/customerName" == "abc.claims[9].customerName"', () => {
        // Arrange
        let fieldPath: string = "abc.claims[0].amount";
        let jsonPointer: string = "2/9/customerName";
        let expected: string = "abc.claims[9].customerName";

        // Act
        let result: string = RelativeJsonPointerResolver.resolve(fieldPath, jsonPointer);

        // Assert
        expect(result).toEqual(expected);
    });

    it('should add a new array and index to the fieldPath after '
        + 'resolving an ancestor : "abc.claims[0].amount", "3/drivers/0/name" == "abc.drivers[0].name"', () => {
        // Arrange
        let fieldPath: string = "abc.claims[0].amount";
        let jsonPointer: string = "3/drivers/0/name";
        let expected: string = "abc.drivers[0].name";

        // Act
        let result: string = RelativeJsonPointerResolver.resolve(fieldPath, jsonPointer);

        // Assert
        expect(result).toEqual(expected);
    });

    it('should throw an error when a # symbol is used in the JSON Pointer', () => {
        // Arrange
        let fieldPath: string = "abc.claims[0].amount";
        let jsonPointer: string = "3#/drivers/0/name";

        // Act
        let act: () => string = (): string => RelativeJsonPointerResolver.resolve(fieldPath, jsonPointer);

        // Assert
        expect(act).toThrow(Errors.Product.JsonPointerHashSymbolNotSupportedWhenResolvingRelativeFieldPath(
            fieldPath, jsonPointer));
    });

    it('should throw an error when there is no integer prefix at the start of the JSON Pointer', () => {
        // Arrange
        let fieldPath: string = "abc.claims[0].amount";
        let jsonPointer: string = "abc/drivers/0/name";

        // Act
        let act: () => string = (): string => RelativeJsonPointerResolver.resolve(fieldPath, jsonPointer);

        // Assert
        expect(act).toThrow(Errors.Product.RelativeJsonPointerIntegerPrefixIsNotANumber(
            fieldPath, jsonPointer, 'abc'));
    });
});
