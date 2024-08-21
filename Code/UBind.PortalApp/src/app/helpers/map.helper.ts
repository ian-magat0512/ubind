/**
 * Export map helper class
 * Mapping of value keys helper.
 */
export class MapHelper {
    public static merge<TMapKey, TMapValue>(
        map1: Map<TMapKey, TMapValue>,
        map2: Map<TMapKey, TMapValue>,
        map3?: Map<TMapKey, TMapValue>,
    ): Map<TMapKey, TMapValue> {
        let newMap: Map<TMapKey, TMapValue> = new Map<TMapKey, TMapValue>(map1);
        map2.forEach((value: TMapValue, key: TMapKey) => newMap.set(key, value));
        if (map3) {
            map3.forEach((value: TMapValue, key: TMapKey) => newMap.set(key, value));
        }
        return newMap;
    }

    /**
     * This is to add a key and a value into the map. if there is an existing value
     * then it will convert an existing value into an array and add to it.
     */
    public static add<TMapKey, TMapValue>(
        map: Map<TMapKey, TMapValue | Array<TMapValue>>,
        key: TMapKey,
        value: TMapValue | Array<TMapValue>,
    ): void {
        if (Array.isArray(value) && value.length == 0) {
            return;
        }
        const existingValue: any = map.get(key);
        if (existingValue) {
            if (Array.isArray(existingValue)) {
                if (Array.isArray(value)) {
                    existingValue.push(...value);
                } else {
                    existingValue.push(value);
                }
            } else {
                if (Array.isArray(value)) {
                    map.set(key, [existingValue, ...value]);
                } else {
                    map.set(key, [existingValue, value]);
                }
            }
        } else {
            map.set(key, value);
        }
    }

    /**
     * This is to replace a single string value within the map.
     */
    public static replaceEntryValue<TMapKey, TMapValue>(
        map: Map<TMapKey, TMapValue | Array<TMapValue>>,
        key: TMapKey,
        matchValue: TMapValue,
        newValue: TMapValue,
        doesMatchCallback: (left: TMapValue, right: TMapValue) =>
            boolean = (left: TMapValue, right: TMapValue): boolean => left == right,
    ): boolean {
        const existingValue: any = map.get(key);
        if (existingValue) {
            if (Array.isArray(existingValue)) {
                for (let i: number = 0; i < existingValue.length; i++) {
                    let val: TMapValue = existingValue[i];
                    if (doesMatchCallback(val, matchValue)) {
                        existingValue[i] = newValue;
                        return true;
                    }
                }
            } else {
                if (doesMatchCallback(existingValue, matchValue)) {
                    map.set(key, newValue);
                    return true;
                }
            }
        }
        return false;
    }

    /**
     * Checks whether a map contains an entry with a value that matches.
     * If doesMatchCallback is passed, that function is called to check if the two values match.
     */
    public static containsEntryWithValue<TMapKey, TMapValue>(
        map: Map<TMapKey, TMapValue | Array<TMapValue>>,
        key: TMapKey,
        matchValue: TMapValue,
        doesMatchCallback: (left: TMapValue, right: TMapValue) =>
            boolean = (left: TMapValue, right: TMapValue): boolean => left == right,
    ): boolean {
        let value: any = map.get(key);
        if (Array.isArray(value)) {
            return value.filter((item: TMapValue) => doesMatchCallback(item, matchValue)).length > 0;
        } else {
            return doesMatchCallback(value, matchValue);
        }
    }
}
