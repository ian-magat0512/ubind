import { ObjectExpander } from "./object-expander";

describe('ObjectExpander', () => {

    it('should update the source object structure to hold "contactName"', () => {
        // Arrange
        let sourceObject: any = {};
        let path: string = 'contactName';

        // Act
        let result: object = ObjectExpander.expandObjectToHaveObjectForPropertyAtPath(sourceObject, path);

        // Assert
        let expected: any = {};
        expect(result).toEqual({});
        expect(sourceObject).toEqual(expected);
    });

    it('should update the source object structure to hold "fish.bait"', () => {
        // Arrange
        let sourceObject: any = {};
        let path: string = 'fish.bait';

        // Act
        let result: object = ObjectExpander.expandObjectToHaveObjectForPropertyAtPath(sourceObject, path);

        // Assert        
        let expected: any = {
            fish: {},
        };
        expect(result).toEqual({});
        expect(sourceObject).toEqual(expected);
    });

    it('should update the source object structure to hold "drivers[0].age"', () => {
        // Arrange
        let sourceObject: any = {};
        let path: string = 'drivers[0].age';

        // Act
        let result: object = ObjectExpander.expandObjectToHaveObjectForPropertyAtPath(sourceObject, path);

        // Assert        
        let expected: any = {
            drivers: [{}],
        };
        expect(result).toEqual({});
        expect(sourceObject).toEqual(expected);
    });

    it('should update the source object structure to hold "drivers[0].claims[0].amount"', () => {
        // Arrange
        let sourceObject: any = {};
        let path: string = 'drivers[0].claims[0].amount';

        // Act
        let result: object = ObjectExpander.expandObjectToHaveObjectForPropertyAtPath(sourceObject, path);

        // Assert        
        let expected: any = {
            drivers: [
                {
                    claims: [{}],
                },
            ],
        };
        expect(result).toEqual({});
        expect(sourceObject).toEqual(expected);
    });

    it('should update the source object structure to hold "drivers[0].claims[2].amount"', () => {
        // Arrange
        let sourceObject: any = {};
        let path: string = 'drivers[0].claims[2].amount';

        // Act
        let result: object = ObjectExpander.expandObjectToHaveObjectForPropertyAtPath(sourceObject, path);

        // Assert        
        let expected: any = {
            drivers: [{}],
        };
        expected.drivers[0]['claims'] = new Array<object>();
        expected.drivers[0]['claims'][2] = {};

        expect(result).toEqual({});
        expect(sourceObject).toEqual(expected);
    });

    it('should update the source object structure to hold "drivers[0]"', () => {
        // Arrange
        let sourceObject: any = {};
        let path: string = 'drivers[0]';

        // Act
        let result: object = ObjectExpander.expandObjectToHaveObjectForPropertyAtPath(sourceObject, path);

        // Assert        
        let expected: any = {
            drivers: [{}],
        };
        expect(result).toEqual({});
        expect(sourceObject).toEqual(expected);
    });

    it('should update the source object structure to hold "drivers[0].claims[2]"', () => {
        // Arrange
        let sourceObject: any = {};
        let path: string = 'drivers[0].claims[2]';

        // Act
        let result: object = ObjectExpander.expandObjectToHaveObjectForPropertyAtPath(sourceObject, path);

        // Assert        
        let expected: any = {
            drivers: [{}],
        };
        expected.drivers[0]['claims'] = new Array<object>();
        expected.drivers[0]['claims'][2] = {};

        expect(result).toEqual({});
        expect(sourceObject).toEqual(expected);
    });

});
