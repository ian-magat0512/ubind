import { Errors } from "@app/models/errors";

/**
 * Resolves a new absolute json path using a relative Json Pointer
 * https://tools.ietf.org/id/draft-handrews-relative-json-pointer-00.html
 * 
 * This is needed for referencing a sibling field in a repeating question set, 
 * and could be used for other things.
 */
export class RelativeJsonPointerResolver {

    /**
     * 
     * @param fieldPath An absolute fieldpath, in json path format, e.g. 'claims[0].amount;
     * @param jsonPointer A relative JSON Pointer, e.g. '1/date'. for further details
     * see https://tools.ietf.org/id/draft-handrews-relative-json-pointer-00.html
     */
    public static resolve(fieldPath: string, jsonPointer: string): string {
        let jpSegments: Array<string> = jsonPointer.split('/');
        let fpSegments: Array<string> = this.convertfieldPathToSegments(fieldPath);
        let jpPrefixSegment: string = jpSegments[0];
        if (jpPrefixSegment.endsWith('#')) {
            throw Errors.Product.JsonPointerHashSymbolNotSupportedWhenResolvingRelativeFieldPath(
                fieldPath, jsonPointer);
        }

        // go up
        let goUpAmount: number = parseInt(jpPrefixSegment, 10);
        if (isNaN(goUpAmount)) {
            throw Errors.Product.RelativeJsonPointerIntegerPrefixIsNotANumber(
                fieldPath, jsonPointer, jpPrefixSegment);
        }
        for (let i: number = 0; i < goUpAmount; i++) {
            fpSegments.pop();
        }

        // append
        for (let i: number = 1; i < jpSegments.length; i++) {
            let segmentAsNumber: number = parseInt(jpSegments[i], 10);
            if (isNaN(segmentAsNumber)) {
                fpSegments.push(jpSegments[i]);
            } else {
                fpSegments.push('[' + jpSegments[i] + ']');
            }
        }

        return fpSegments.join('.').replace(/\.\[/g, '[');
    }

    private static convertfieldPathToSegments(fieldPath: string): Array<string> {
        let dotSegments: Array<string> = fieldPath.split('.');
        let allSegments: Array<string> = new Array<string>();
        dotSegments.forEach((segment: string) => {
            let regex: RegExp = new RegExp(/\[\d*\]/);
            if (regex.test(segment)) {
                let splitIndex: number = segment.indexOf('[');
                allSegments.push(segment.substring(0, splitIndex));
                allSegments.push(segment.substring(splitIndex));
            } else {
                allSegments.push(segment);
            }
        });
        return allSegments;
    }
}
