import { JsonValidator } from './json-validator';

describe('JsonValidatorIsValidJson', () => {

    it('should return true when json is valid', () => {
        const json: string = '{"name":"John", "age":30, "car":null}';
        expect(JsonValidator.isValidJson(json)).toBeTruthy();
    });

    it('should return false when json is invalid', () => {
        const jsonInvalidFormat: string = '{"name":"John", "age":30, "car":null';
        expect(JsonValidator.isValidJson(jsonInvalidFormat)).toBeFalsy();

        const jsonUnexpectedDataTypeString: string = 'This is not a json string';
        expect(JsonValidator.isValidJson(jsonUnexpectedDataTypeString)).toBeFalsy();

        const jsonUnexpectedDataTypeStringNumber: string = '12345.6';
        expect(JsonValidator.isValidJson(jsonUnexpectedDataTypeStringNumber)).toBeFalsy();
    });

});
