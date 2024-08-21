/**
 * Tools for transforming and operating on data.
 */
export class DataHelper {

    /**
     * Converts a 2 dimensional array of data from columns to rows
     * @param data 
     */
    public static convertColumnsToRows(data: Array<Array<any>>): Array<Array<any>> {
        let result: Array<Array<string>> = new Array<Array<string>>();
        for (let rowNo: number = 0; rowNo < data[0].length; rowNo++) {
            let row: Array<string> = new Array<string>();
            // eslint-disable-next-line @typescript-eslint/prefer-for-of
            // eslint-disable-next-line @typescript-eslint/prefer-for-of
            for (let columnNo: number = 0; columnNo < data.length; columnNo++) {
                row.push(data[columnNo][rowNo]);
            }
            result.push(row);
        }
        return result;
    }
}
