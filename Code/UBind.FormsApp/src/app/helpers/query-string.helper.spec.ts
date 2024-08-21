import { QueryStringHelper } from './query-string.helper';

describe('QueryStringHelper', () => {

    beforeEach(() => {
    });

    it('should convert a basic query string to json', () => {
        // Arrange
        let queryString: string = 'a=asdf&b=1234';
        let expected: any = {
            a: 'asdf',
            b: 1234,
        };

        // Act
        let actual: object = QueryStringHelper.queryStringToJson(queryString);

        // Assert
        expect(actual).toEqual(expected);
    });

    it('should convert a query string with a json object in it to json', () => {
        // Arrange
        let queryString: string = 'a=asdf&b=1234&c={\'d\': \'asdf\', \'e\': 1234 }';
        let expected: any = {
            a: 'asdf',
            b: 1234,
            c: {
                d: 'asdf',
                e: 1234,
            },
        };

        // Act
        let actual: object = QueryStringHelper.queryStringToJson(queryString);

        // Assert
        expect(actual).toEqual(expected);
    });

    it('should detect when something is not a query string because it\'s an object', () => {
        // Arrange
        let queryString: any = {
            a: 'asdf',
            b: 1234,
            c: {
                d: 'asdf',
                e: 1234,
            },
        };

        // Act
        let actual: boolean = QueryStringHelper.isQueryString(queryString);

        // Assert
        expect(actual).toBeFalsy();
    });
});
