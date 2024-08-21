import Ajv from 'ajv';


interface JsonValidatorAssertSchemaOptions {
    removeAdditional?: boolean;
    allErrors?: boolean;
}

/**
 * This class provides utility methods for validating JSON objects
 * it has methods for asserting schema conformity
 * and checking the validity of JSON objects.
 */
export class JsonValidator {
    public static assertSchema(
        schemaString: string,
        jsonString: string,
        validationOptions?: JsonValidatorAssertSchemaOptions): string {
        try {
            const schema: any = JSON.parse(schemaString);
            const options: JsonValidatorAssertSchemaOptions = {
                // Default options
                allErrors: true,

                // Allow for custom options to be passed in
                ...(validationOptions ? validationOptions : {}),
            };

            // Ajv includes the $schema key in assertion, so we need to delete it
            if ('$schema' in schema) {
                delete schema.$schema;
            }
            const ajv: any = new Ajv(options);
            const validate: any = ajv.compile(schema);
            let jsonData: JSON;
            try {
                jsonData = JSON.parse(jsonString);
            } catch (error) {
                return 'Invalid JSON object';
            }
            const isValid: any = validate(jsonData);
            if (!isValid) {
                return 'JSON object does not pass schema assertion';
            }
            return null;
        } catch (error) {
            return 'Invalid JSON schema';
        }
    }

    public static isValidJson(jsonString: string): boolean {
        try {
            const obj: Record<string, any> = JSON.parse(jsonString);
            return obj !== null && typeof obj === 'object';
        } catch (error) {
            return false;
        }
    }
}
