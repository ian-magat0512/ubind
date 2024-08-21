import { UnifiedFormModel } from './unified-form-model';

describe('UnifiedFormModel', () => {

    it('getFormModelForQuestionSet should return the base properties for an empty fieldPath', () => {
        // Arrange
        let sut: UnifiedFormModel = new UnifiedFormModel();
        sut.model = getSampleFormModel();
        let path: string = '';

        // Act
        let result: object = sut.getOrCreateFormModelForQuestionSet(path);

        // Assert
        let expected: any = {
            propertyOne: "propertyOneValue",
            propertyTwo: "propertyOneValue",
        };
        expect(result).toEqual(expected);
    });

    it('getFormModelForQuestionSet should return the properties for fieldPath "objectOne"', () => {
        // Arrange
        let sut: UnifiedFormModel = new UnifiedFormModel();
        sut.model = getSampleFormModel();
        let path: string = 'objectOne';

        // Act
        let result: object = sut.getOrCreateFormModelForQuestionSet(path);

        // Assert
        let expected: any = {
            objectOnePropertyOne: "objectOnePropertyOneValue",
            objectOnePropertyTwo: "objectOnePropertyTwoValue",
        };
        expect(result).toEqual(expected);
    });

    it('getFormModelForQuestionSet should return the properties for fieldPath "objectOne.objectOneObjectOne"', () => {
        // Arrange
        let sut: UnifiedFormModel = new UnifiedFormModel();
        sut.model = getSampleFormModel();
        let path: string = 'objectOne.objectOneObjectOne';

        // Act
        let result: object = sut.getOrCreateFormModelForQuestionSet(path);

        // Assert
        let expected: any = {
            objectOneObjectOnePropertyOne: "objectOneObjectOnePropertyOneValue",
        };
        expect(result).toEqual(expected);
    });

    it('getFormModelForQuestionSet should return the properties for fieldPath "objectOne.objectOneArrayOne[0]"', () => {
        // Arrange
        let sut: UnifiedFormModel = new UnifiedFormModel();
        sut.model = getSampleFormModel();
        let path: string = 'objectOne.objectOneArrayOne[0]';

        // Act
        let result: object = sut.getOrCreateFormModelForQuestionSet(path);

        // Assert
        let expected: any = {
            objectOneArrayOneIndexOnePropertyOne: "objectOneArrayOneIndexOnePropertyOneValue",
        };
        expect(result).toEqual(expected);
    });

    it('getFormModelForQuestionSet should return the properties for fieldPath "arrayOne[1]"', () => {
        // Arrange
        let sut: UnifiedFormModel = new UnifiedFormModel();
        sut.model = getSampleFormModel();
        let path: string = 'arrayOne[1]';

        // Act
        let result: object = sut.getOrCreateFormModelForQuestionSet(path);

        // Assert
        let expected: any = {
            arrayOneIndexTwoPropertyOne: "arrayOneIndexTwoPropertyOneValue",
            arrayOneIndexTwoPropertyTwo: "arrayOneIndexTwoPropertyOneValue",
        };
        expect(result).toEqual(expected);
    });

    it('getFormModelForQuestionSet should return the properties for fieldPath "arrayOne[0].arrayOneIndexOneObjectOne"',
        () => {
            // Arrange
            let sut: UnifiedFormModel = new UnifiedFormModel();
            sut.model = getSampleFormModel();
            let path: string = 'arrayOne[0].arrayOneIndexOneObjectOne';

            // Act
            let result: object = sut.getOrCreateFormModelForQuestionSet(path);

            // Assert
            let expected: any = {
                arrayOneIndexOneObjectOnePropertyOne: "arrayOneIndexOneObjectOnePropertyOneValue",
            };
            expect(result).toEqual(expected);
        });

    it('getFormModelForQuestionSet should return the properties for fieldPath ' +
        '"arrayOne[1].arrayOneIndexTwoArrayOne[0]"',
    () => {
        // Arrange
        let sut: UnifiedFormModel = new UnifiedFormModel();
        sut.model = getSampleFormModel();
        let path: string = 'arrayOne[1].arrayOneIndexTwoArrayOne[0]';

        // Act
        let result: object = sut.getOrCreateFormModelForQuestionSet(path);

        // Assert
        let expected: any = {
            arrayOneIndexTwoArrayOneIndexOnePropertyOne: "arrayOneIndexTwoArrayOneIndexOnePropertyOne",
        };
        expect(result).toEqual(expected);
    });

    it('getFormModelForQuestionSet should return an empty object for fieldPath "asdf"', () => {
        // Arrange
        let sut: UnifiedFormModel = new UnifiedFormModel();
        sut.model = getSampleFormModel();
        let path: string = 'asdf';

        // Act
        let result: object = sut.getOrCreateFormModelForQuestionSet(path);

        // Assert
        let expected: any = {};
        expect(result).toEqual(expected);
    });

    it('getFormModelForQuestionSet should return an empty object for fieldPath "asdf.asdf"', () => {
        // Arrange
        let sut: UnifiedFormModel = new UnifiedFormModel();
        sut.model = getSampleFormModel();
        let path: string = 'asdf.asdf';

        // Act
        let result: object = sut.getOrCreateFormModelForQuestionSet(path);

        // Assert
        let expected: any = {};
        expect(result).toEqual(expected);
    });

    it('getFormModelForQuestionSet should return an empty object for fieldPath "objectOne.asdf"', () => {
        // Arrange
        let sut: UnifiedFormModel = new UnifiedFormModel();
        sut.model = getSampleFormModel();
        let path: string = 'objectOne.asdf';

        // Act
        let result: object = sut.getOrCreateFormModelForQuestionSet(path);

        // Assert
        let expected: any = {};
        expect(result).toEqual(expected);
    });

    it('getFormModelForQuestionSet should return an empty object for fieldPath "arrayOne[0].asdf"', () => {
        // Arrange
        let sut: UnifiedFormModel = new UnifiedFormModel();
        sut.model = getSampleFormModel();
        let path: string = 'arrayOne[0].asdf';

        // Act
        let result: object = sut.getOrCreateFormModelForQuestionSet(path);

        // Assert
        let expected: any = {};
        expect(result).toEqual(expected);
    });

    it('patchQuestionSetModelIntoUnifiedFormModel should patch in an object at the base level', () => {
        // Arrange
        let sut: UnifiedFormModel = new UnifiedFormModel();
        sut.model = getSampleFormModel();
        let path: string = '';
        let model: any = {
            modelPropertyOne: "modelPropertyOneValue",
            modelPropertyTwo: "modelPropertyTwoValue",
        };

        // Act
        sut.patchQuestionSetModelIntoUnifiedFormModel(path, model);

        // Assert
        expect(sut.model['modelPropertyOne']).toBeDefined();
    });

    it('patchQuestionSetModelIntoUnifiedFormModel should patch in an object within a repeating question set', () => {
        // Arrange
        let sut: UnifiedFormModel = new UnifiedFormModel();
        sut.model = getSampleFormModel();
        let path: string = 'arrayOne[0]';
        let model: any = {
            modelPropertyOne: "modelPropertyOneValue",
            modelPropertyTwo: "modelPropertyTwoValue",
        };

        // Act
        sut.patchQuestionSetModelIntoUnifiedFormModel(path, model);

        // Assert
        expect(sut.model['arrayOne'][0]['modelPropertyOne']).toBeDefined();
    });

    it('deleteQuestionSetModelFromUnifiedFormModel should delete an object which is a repeating question set', () => {
        // Arrange
        let sut: UnifiedFormModel = new UnifiedFormModel();
        sut.model = getSampleFormModel();
        let path: string = 'arrayOne[1]';

        // Act
        sut.deleteQuestionSetModelFromUnifiedFormModel(path);

        // Assert
        expect(sut.model['arrayOne'][1]).not.toBeDefined();
        expect(sut.model['arrayOne'][0]).toBeDefined();
    });

    it('deleteQuestionSetModelFromUnifiedFormModel should delete an object ' +
        'which is a repeating question set and re-order the indexes', () => {
        // Arrange
        let sut: UnifiedFormModel = new UnifiedFormModel();
        sut.model = getSampleFormModel();
        let path: string = 'arrayOne[0]';

        // Act
        sut.deleteQuestionSetModelFromUnifiedFormModel(path);

        // Assert
        expect(sut.model['arrayOne'][1]).not.toBeDefined();
        expect(sut.model['arrayOne'][0]).toBeDefined();
    });

    // eslint-disable-next-line prefer-arrow/prefer-arrow-functions
    function getSampleFormModel(): object {
        return {
            propertyOne: "propertyOneValue",
            propertyTwo: "propertyOneValue",
            objectOne: {
                objectOnePropertyOne: "objectOnePropertyOneValue",
                objectOnePropertyTwo: "objectOnePropertyTwoValue",
                objectOneObjectOne: {
                    objectOneObjectOnePropertyOne: "objectOneObjectOnePropertyOneValue",
                },
                objectOneArrayOne: [
                    {
                        objectOneArrayOneIndexOnePropertyOne: "objectOneArrayOneIndexOnePropertyOneValue",
                    },
                ],
            },
            arrayOne: [
                {
                    arrayOneIndexOnePropertyOne: "arrayOneIndexOnePropertyOneValue",
                    arrayOneIndexOnePropertyTwo: "arrayOneIndexOnePropertyOneValue",
                    arrayOneIndexOneObjectOne: {
                        arrayOneIndexOneObjectOnePropertyOne: "arrayOneIndexOneObjectOnePropertyOneValue",
                    },
                },
                {
                    arrayOneIndexTwoPropertyOne: "arrayOneIndexTwoPropertyOneValue",
                    arrayOneIndexTwoPropertyTwo: "arrayOneIndexTwoPropertyOneValue",
                    arrayOneIndexTwoArrayOne: [
                        {
                            arrayOneIndexTwoArrayOneIndexOnePropertyOne: "arrayOneIndexTwoArrayOneIndexOnePropertyOne",
                        },
                    ],
                },
            ],
        };
    }
});
