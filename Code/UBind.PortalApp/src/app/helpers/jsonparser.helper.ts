import _ from 'lodash';

/**
 * Export json parser class
 * To parse the JSON files formats.
 */
export class JsonParser {
    public static cleanData(obj: any): any {
        const holder: any = obj;
        const properties: Array<string> = Object.getOwnPropertyNames(holder);
        for (let name of properties) {
            if (holder[name] == null || _.isEmpty(holder[name])) {
                delete obj[name];
            }
        }
        return obj;
    }

    public static retrieveCategory(obj: any, formData: any): any {
        const categories: Array<string> = Object.keys(obj);
        let repeatingData: any = null;
        for (let name in categories) {
            const key: string = categories[name];
            const properties: Array<string> = Object.getOwnPropertyNames(obj[key]);
            for (let name of properties) {
                if (name.indexOf('String') !== -1) {
                    delete obj[key][name];
                }
                if (typeof obj[key][name] === 'object') {
                    if (!repeatingData) {
                        repeatingData = {};
                    }

                    repeatingData[name] = obj[key][name];
                    delete obj[key][name];
                }
            }
        }

        if (!repeatingData) {
            repeatingData = JsonParser.getRepeatingDataFromFormData(formData);
        }

        return { header: categories, obj: obj, repeatingData: repeatingData };
    }

    public static getRepeatingDataFromFormData(formData: any): any {
        const obj: any = {};
        for (const formDataKey in formData) {
            // continue loop if null to avoid the evaluations below
            if (!formData[formDataKey]) {
                continue;
            }

            let formDataValue: any = formData[formDataKey];
            if ((typeof formDataValue) === "string") {
                // trim first
                formDataValue = formDataValue.trim();

                // repeating questions is expected as an array,
                // so just directly check the first and last character if '[' and ']'
                if (formDataValue.charAt(0) === "["
                    && formDataValue.charAt(formDataValue.length - 1) === "]") {
                    formData[formDataKey] = JSON.parse(formData[formDataKey]);
                }
            }

            if (formData[formDataKey] instanceof Array) {
                const returnArrayObjects: Array<any> = [];
                const arrObject: any = formData[formDataKey];
                for (const objItem of arrObject) {
                    const subObj: any = {};

                    // remove blank spaced content.
                    for (const objectItemKey in objItem) {
                        if (objItem[objectItemKey]) {
                            subObj[objectItemKey] = objItem[objectItemKey];
                        }
                    }

                    if (!_.isEmpty(subObj)) {
                        returnArrayObjects.push(subObj);
                    }
                }
                obj[formDataKey] = returnArrayObjects;
            }
        }

        return obj;
    }

    public static getRepeatingDataFromQuestionSet(questionSet: any): any {
        const obj: any = {};
        for (const formDataHeader in questionSet) {
            if (questionSet[formDataHeader] instanceof Array) {
                const returnArrayObjects: Array<any> = [];
                const arrObject: any = questionSet[formDataHeader];
                for (const objItem of arrObject) {
                    const subObj: any = {};

                    // remove blank spaced content.
                    for (const objectItemKey in objItem) {
                        if (objItem[objectItemKey]) {
                            subObj[objectItemKey] = objItem[objectItemKey];
                        }
                    }

                    if (!_.isEmpty(subObj)) {
                        returnArrayObjects.push(subObj);
                    }
                }

                obj[formDataHeader] = returnArrayObjects;
            } else {
                for (const formDataKey in questionSet[formDataHeader]) {
                    if (questionSet[formDataHeader][formDataKey] instanceof Array) {
                        const returnArrayObjects: Array<any> = [];
                        const arrObject: any = questionSet[formDataHeader][formDataKey];
                        for (const objItem of arrObject) {
                            const subObj: any = {};

                            // remove blank spaced content.
                            for (const objectItemKey in objItem) {
                                if (objItem[objectItemKey]) {
                                    subObj[objectItemKey] = objItem[objectItemKey];
                                }
                            }

                            if (!_.isEmpty(subObj)) {
                                returnArrayObjects.push(subObj);
                            }
                        }

                        obj[formDataKey] = returnArrayObjects;
                    }
                }
            }
        }
        return obj;
    }

    public static getFormDataWithoutRepeatingData(formData: any): any {
        const returnObject: any = {};
        for (const key in formData) {
            if (!(formData[key] instanceof Array)) {
                returnObject[key] = formData[key];
            }
        }
        return returnObject;
    }

    public static getQuestionSetWithoutRepeatingData(questionSet: any): any {
        const returnObject: any = {};
        for (const header in questionSet) {
            if (!(questionSet[header] instanceof Array)) {
                returnObject[header] = {};
                for (const key in questionSet[header]) {
                    if (!(questionSet[header][key] instanceof Array)) {
                        returnObject[header][key] = questionSet[header][key];
                    }
                }
            }
        }

        return returnObject;
    }

    public static groupArray(collection: Array<any>, groupBy: any): Array<any> {
    // prevents the application from breaking if the array of objects doesn't exist yet
        if (!collection) {
            return null;
        }

        const groupedCollection: any = collection.reduce((previous: any, current: any) => {
            if (!previous[current[groupBy]]) {
                previous[current[groupBy]] = [current];
            } else {
                previous[current[groupBy]].push(current);
            }

            return previous;
        }, {});

        // this will return an array of objects, each object containing a group of objects
        return Object.keys(groupedCollection).map((key: string) => ({ key, value: groupedCollection[key] }));
    }
}
