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
}
