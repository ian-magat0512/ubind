import { Errors } from "@app/models/errors";

/**
 * Tools to work with enums.
 */
export class EnumHelper {
    /**
     * Helper method for converting enum to list
     * @param enumInput the enum
     */
    public static enumToList(enumInput: any): Array<any> {
        const list: Array<any> = [];
        let index: number = 1;
        for (const enumMember in enumInput) {
            list.push({ id: index, name: enumInput[enumMember] });
            index++;
        }
        return list;
    }

    public static toEnumArray(enumInput: any): Array<any> {
        const result: Array<any> = Object.keys(enumInput)
            .map((key: string) => enumInput[key]);
        return result;
    }

    public static parseOrThrow<TEnum>(
        enumObject: { [s: string]: TEnum },
        valueToParse: string,
        propertyName: string,
        context: string,
        ignoreCase: boolean = true,
    ): TEnum {
        let parsed: TEnum = EnumHelper.parseOrNull<TEnum>(enumObject, valueToParse, ignoreCase);
        if (!parsed) {
            throw Errors.General.InvalidEnumValue(propertyName, valueToParse, enumObject, context);
        }

        return parsed;
    }

    public static parseOrNull<TEnum>(
        enumObject: { [s: string]: TEnum },
        valueToParse: string | number,
        ignoreCase: boolean = true,
    ): TEnum {
        if (valueToParse == null) {
            return null;
        }

        let values: Array<string> = Object.values(enumObject) as unknown as Array<string>;
        if(typeof valueToParse === 'number' && values[valueToParse] !== undefined) {
            valueToParse = values[valueToParse];
        }

        let value: string = valueToParse.toString();
        let matchingIndex: number;
        if (ignoreCase) {
            let lowerCaseValues: Array<string> = values.map((s: string) => s.toLowerCase());
            let lowerCaseValueToParse: string = value.toLowerCase();
            matchingIndex = lowerCaseValues.indexOf(lowerCaseValueToParse);
        } else {
            matchingIndex = values.indexOf(value);
        }

        if (matchingIndex != -1) {
            return values[matchingIndex] as unknown as TEnum;
        }
        return null;
    }
}
