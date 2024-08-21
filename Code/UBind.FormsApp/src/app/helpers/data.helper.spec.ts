import { DataHelper } from "./data.helper";

describe('DataHelper', () => {
    it('should convert column orented data into row oriented data', () => {
        let data: Array<Array<any>> = [
            ['a', 'b', 'c', 'd', 'e'],
            [1, 2, 3, 4, 5],
            ['v', 'w', 'x', 'y', 'z'],
        ];
        expect(data[0][1]).toBe('b');
        expect(data[2][2]).toBe('x');
        let result: Array<Array<any>> = DataHelper.convertColumnsToRows(data);
        expect(result[0][1]).toBe(1);
        expect(result[2][2]).toBe('x');
        expect(result[4][0]).toBe('e');
        expect(result[4][1]).toBe(5);
        expect(result[4][2]).toBe('z');
    });
});
