import * as _ from 'lodash-es';

/**
 * Represents a data model for the form that includes the models for many question sets into on large object
 */
export class UnifiedFormModel {

    /**
     * The form model is an object which is kept up to date with the latest
     * values of the fields by formly and reactive forms.
     */
    public model: object = {};

    public constructor(
    ) {
    }

    /**
     * @param fieldPath The field path at which data is found for the given question set
     */
    public getOrCreateFormModelForQuestionSet(fieldPath: string): object {
        if (fieldPath == null) {
            throw new Error("ConfigService::getFormModelForQuestionSet was passed a null fieldPath. "
                + "Please ensure you pass the field path for the given question set. Typically if the "
                + "question set is not nested, the fieldPath will be an empty string.");
        }
        let objectAtCurrentLevel: object = this.getOrCreateObjectAtPathWithinUnifiedFormModel(fieldPath);
        // create a new object which only has the properties at the current level, no sub objects or arrays:
        return _.pickBy(objectAtCurrentLevel, (value: any) => {
            return !_.isObject(value);
        });
    }

    public getOrCreateObjectAtPathWithinUnifiedFormModel(fieldPath: string): object {
        let objectAtCurrentLevel: object = this.model;
        if (fieldPath != '') {
            let segments: Array<string> = fieldPath.split('.');
            for (let segment of segments) {
                if (segment.indexOf('[') != -1) {
                    let segmentArrayIndex: string = segment.match(/\[(.*)\]/)[1];
                    segment = segment.replace(/\[.*\]/g, '');
                    if (!Array.isArray(objectAtCurrentLevel[segment])) {
                        objectAtCurrentLevel[segment] = new Array<object>();
                    }
                    if (objectAtCurrentLevel[segment][segmentArrayIndex] == null) {
                        objectAtCurrentLevel[segment][segmentArrayIndex] = {};
                    }
                    objectAtCurrentLevel = objectAtCurrentLevel[segment][segmentArrayIndex];
                } else {
                    if (objectAtCurrentLevel[segment] == null) {
                        objectAtCurrentLevel[segment] = {};
                    }
                    objectAtCurrentLevel = objectAtCurrentLevel[segment];
                }
            }
        }
        return objectAtCurrentLevel;
    }

    public patchQuestionSetModelIntoUnifiedFormModel(fieldPath: string, model: object): void {
        if (fieldPath == null) {
            throw new Error("ConfigService::getFormModelForQuestionSet was passed a null fieldPath. "
                + "Please ensure you pass the field path for the given question set. Typically if the "
                + "question set is not nested, the fieldPath will be an empty string.");
        }
        let objectAtCurrentLevel: object = this.getOrCreateObjectAtPathWithinUnifiedFormModel(fieldPath);
        _.merge(objectAtCurrentLevel, model);
    }

    /**
     * Deletes an object at a given fieldpath within the unified form model.
     * This is typically used when a repeating question set is deleted.
     * Note that it also reindexes the items, so if you delete index 0, then what
     * was at index 1 will now be at index 0.
     * @param fieldPath 
     */
    public deleteQuestionSetModelFromUnifiedFormModel(fieldPath: string): void {
        let objectAtCurrentLevel: object = this.model;
        if (fieldPath != '') {
            let segments: Array<string> = fieldPath.split('.');
            for (let segmentIndex: number = 0; segmentIndex < segments.length; segmentIndex++) {
                let segment: string = segments[segmentIndex];
                if (segment.indexOf('[') != -1) {
                    let segmentArrayIndex: string = segment.match(/\[(.*)\]/)[1];
                    segment = segment.replace(/\[.*\]/g, '');
                    if (objectAtCurrentLevel[segment] == null) {
                        return;
                    }
                    if (objectAtCurrentLevel[segment][segmentArrayIndex] == null) {
                        return;
                    }
                    if (segmentIndex == segments.length - 1) {
                        if (Array.isArray(objectAtCurrentLevel[segment])) {
                            objectAtCurrentLevel[segment].splice(segmentArrayIndex, 1);
                        }
                        return;
                    }
                } else {
                    if (objectAtCurrentLevel[segment] == null) {
                        return;
                    }
                    if (segmentIndex == segments.length - 1) {
                        delete objectAtCurrentLevel[segment];
                        return;
                    }
                }
            }
        }
    }
}
