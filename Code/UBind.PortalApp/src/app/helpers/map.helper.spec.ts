import { StringHelper } from ".";
import { MapHelper } from "./map.helper";

let stringHelper: StringHelper = new StringHelper();

describe('MapHelper', () => {
    it('should convert the entry value to an array when I add a value and the key exists', () => {
    // Arrange        
        const key: string = 'theKey';
        const value1: string = 'aaaaaaaaa';
        const value2: string = 'bbbbbbbbb';
        let map: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
        map.set(key, value1);

        // Act
        MapHelper.add(map, key, value2);

        // Assert
        expect(map.get(key)).toEqual([value1, value2]);
    });

    it('should find a value in an entry which is an array', () => {
    // Arrange        
        const key: string = 'theKey';
        const value1: string = 'aaaaaaaaa';
        const value2: string = 'bbbbbbbbb';
        let map: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
        map.set(key, value1);
        MapHelper.add(map, key, value2);

        // Act
        const result: boolean = MapHelper.containsEntryWithValue(
            map,
            key,
            value2,
            stringHelper.equalsIgnoreCase.bind(stringHelper),
        );

        // Assert
        expect(result).toBeTruthy();
    });

    it('should not find a value in an entry which does not have it', () => {
    // Arrange        
        const key: string = 'theKey';
        const value1: string = 'aaaaaaaaa';
        const value2: string = 'bbbbbbbbb';
        let map: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
        map.set(key, value1);
        MapHelper.add(map, key, value2);

        // Act
        const result: boolean = MapHelper.containsEntryWithValue(
            map,
            key,
            'ccccccccc',
            stringHelper.equalsIgnoreCase.bind(stringHelper),
        );

        // Assert
        expect(result).toBeFalsy();
    });

    it('should replace a value in an entry which it finds', () => {
    // Arrange        
        const key: string = 'theKey';
        const value1: string = 'aaaaaaaaa';
        const value2: string = 'bbbbbbbbb';
        const value3: string = 'ccccccccc';
        let map: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
        map.set(key, value1);
        MapHelper.add(map, key, value2);

        // Act
        const result: boolean = MapHelper.replaceEntryValue(
            map,
            key,
            value1,
            value3,
            stringHelper.equalsIgnoreCase.bind(stringHelper),
        );

        // Assert
        expect(result).toBeTruthy();
        expect(map.get(key)).toEqual([value3, value2]);
    });

    it('should replace a value in an entry which it finds that has different casing', () => {
    // Arrange        
        const key: string = 'theKey';
        const value1: string = 'aaaaaaaaa';
        const value1DifferentCase: string = 'Aaaaaaaaa';
        const value2: string = 'bbbbbbbbb';
        const value3: string = 'ccccccccc';
        let map: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
        map.set(key, value1);
        MapHelper.add(map, key, value2);

        // Act
        const result: boolean = MapHelper.replaceEntryValue(
            map,
            key,
            value1DifferentCase,
            value3,
            stringHelper.equalsIgnoreCase.bind(stringHelper),
        );

        // Assert
        expect(result).toBeTruthy();
        expect(map.get(key)).toEqual([value3, value2]);
    });

});
