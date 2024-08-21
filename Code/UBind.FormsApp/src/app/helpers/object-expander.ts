
/**
 * This helper class expands an object with additional properties that match a given path
 */
export class ObjectExpander {

    /**
     * Expands the sourceObject with properties that match the given path (if they don't already exist), 
     * and returns the last property's object.
     * e.g. if the path is drivers[0].claims[0].amount, it will return an object containing the amount property.
     * 
     * @param path A path in the format {propertyName}.{arrayName}[{arrayIndex}] e.g. drivers[0].claims[0].amount
     * @param sourceObject The source object structure to expand
     * @returns an empty object which can then have the property added to it, 
     * which is a nested object within the sourceObject at the correct path.
     */
    public static expandObjectToHaveObjectForPropertyAtPath(sourceObject: object, path: string): object {
        let segments: Array<string> = path.split('.');
        let subObject: object = sourceObject;
        for (let i: number = 0; i < segments.length; i++) {
            let segment: string = segments[i];
            if (segment.indexOf('[') != -1) {
                let segmentArrayIndex: string = segment.match(/\[(.*)\]/)[1];
                segment = segment.replace(/\[.*\]/g, '');
                if (!subObject[segment] || !Array.isArray(subObject[segment])) {
                    subObject[segment] = new Array<object>();
                }
                if (!subObject[segment][segmentArrayIndex]) {
                    subObject[segment][segmentArrayIndex] = {};
                }
                subObject = subObject[segment][segmentArrayIndex];
            } else {
                if (i < segments.length - 1) {
                    if (!subObject[segment]) {
                        subObject[segment] = {};
                    }
                    subObject = subObject[segment];
                }
            }
        }
        return subObject;
    }
}
