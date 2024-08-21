const fileStream = require('fs');

export class TestDataService {
    private path = "./e2e/src/testData.json";
    public data: any;
    private testCollection: string;

    public constructor(testCollection?: string) {
        this.retrieve();
    }

    retrieve() {
        var contents = fileStream.readFileSync(this.path);
        // Define to JSON type
        this.data = JSON.parse(contents);
        var index = this.testCollection || this.data.collectionToTest;
        this.data = this.data.collection[index];

        return this.data;
    }

    retrieveAll(search = null) {
        var contents = fileStream.readFileSync(this.path);
        // Define to JSON type
        this.data = JSON.parse(contents);
        var keys = Object.keys(this.data.collection);
        var array = [];

        keys.forEach(key => {
            if (search == null) {
                array.push(this.data.collection[key]);
            }
            else if (key.indexOf(search) > -1) {
                array.push(this.data.collection[key]);
            }
        });

        this.parseArray(array);
        return array;
    }

    originalData = null;

    parseArray(array: Array<any>) {
        for (var key in array) {
            var obj = array[key];
            // ...

            this.originalData = obj;

            this.parseData(obj);
            if (obj.checkResults == null)
                obj.checkResults = { email: true, customer: true, quote: true, policy: true };
        }
    }

    parseData(data: any) {
        for (var key in data) {
            var field = data[key];
            // ...
            if (typeof (field) == "object") {
                this.parseData(field);
            }
            else if (typeof (field) == "string") {
                //can be assigned a placeholder
                if (field.indexOf('{') >= 0) {
                    var placeHolder = field.substring(
                        field.lastIndexOf("{") + 1,
                        field.lastIndexOf("}") - 1
                    );
                    var placeholderReplacement = this.retrievePlaceholderValue(this.originalData, placeHolder);
                    data[key] = data[key].replace("{{" + placeHolder + "}}", placeholderReplacement);
                }

                //is calling a function
                if (field.indexOf('(') >= 0) {
                    var retrievedFunctionName = field.replace('(', "").replace(')', "");
                    data[key] = this[retrievedFunctionName]();
                }
            }
        }
    }

    retrievePlaceholderValue(data: any, fieldPathToSearch: string, currentFieldPath: string = null) {
        var returnValue = "";
        for (var key in data) {
            var field = data[key];
            var fieldPath = currentFieldPath == null ? key : currentFieldPath + "/" + key;
            if (typeof (field) == "object") {
                returnValue = this.retrievePlaceholderValue(field, fieldPathToSearch, fieldPath);
                if (returnValue)
                    break;
            }
            else if (typeof (field) == "string") {
                if (fieldPath == fieldPathToSearch) {
                    return field;
                }
            }
        }

        return returnValue;
    }

    randomName() {
        return this.randomEmail() + " Name Sample";
    }

    randomEmail() {
        var chars = 'abcdefghijklmnopqrstuvwxyz1234567890';
        var string = '';
        for (var ii = 0; ii < 15; ii++) {
            string += chars[Math.floor(Math.random() * chars.length)];
        }

        return string + '@test.com';
    }
}