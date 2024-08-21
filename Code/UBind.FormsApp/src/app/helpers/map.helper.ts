
/**
 * Helper functions for doing things with javascript Maps.
 */
export class MapHelper {

    public static remove<TKey, TValue>(
        map: Map<TKey, TValue>,
        predicate: (key: TKey, value: TValue) => boolean,
        beforeRemoveFunction: (key: TKey, value: TValue) => void): void {
        for (let [key, value] of map) {
            if (predicate(key, value)) {
                if (beforeRemoveFunction) {
                    beforeRemoveFunction(key, value);
                }
                map.delete(key);
            }
        }
    }
}
