import { ExpressionInputSubjectService } from '@app/expressions/expression-input-subject.service';
import { ConfigService } from './config.service';
import { EventService } from './event.service';
import { UnifiedFormModelService } from './unified-form-model.service';

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

describe('UnifiedFormModelService', () => {
    let configService: any;
    let expressionInputSubjectService: any;
    let eventService: EventService;

    beforeEach(() => {
        eventService = new EventService();
        expressionInputSubjectService = new ExpressionInputSubjectService();
        configService = new ConfigService(eventService, null);
    });

    it('load ensures expression field value subjects are created', () => {
        // Arrange
        let sut: UnifiedFormModelService = new UnifiedFormModelService(
            configService,
            expressionInputSubjectService,
            eventService);
        let formModel: object = getSampleFormModel();

        // Act
        sut.apply(formModel);

        // Assert
        expressionInputSubjectService.getFieldValueObservable('propertyOne').subscribe((value: string) => {
            expect(value).toBe('propertyOneValue');
        });
        expressionInputSubjectService.getFieldValueObservable(
            'objectOne.objectOnePropertyOne',
        ).subscribe((value: string) => {
            expect(value).toBe('objectOnePropertyOneValue');
        });
        expressionInputSubjectService.getFieldValueObservable(
            'arrayOne[0].arrayOneIndexOnePropertyOne',
        ).subscribe((value: string) => {
            expect(value).toBe('arrayOneIndexOnePropertyOneValue');
        });
        expressionInputSubjectService.getFieldValueObservable(
            'arrayOne[0].arrayOneIndexOneObjectOne.arrayOneIndexOneObjectOnePropertyOne',
        ).subscribe((value: string) => {
            expect(value).toBe('arrayOneIndexOneObjectOnePropertyOneValue');
        });
        expressionInputSubjectService.getFieldValueObservable(
            'arrayOne[1].arrayOneIndexTwoArrayOne[0].arrayOneIndexTwoArrayOneIndexOnePropertyOne',
        ).subscribe((value: string) => {
            expect(value).toBe('arrayOneIndexTwoArrayOneIndexOnePropertyOne');
        });
    });

    it('should have an empty (not null) unified form model for new quotes', () => {
        // Arrange + Act
        let sut: UnifiedFormModelService = new UnifiedFormModelService(
            configService,
            expressionInputSubjectService,
            eventService);

        // Assert
        let expected: any = {};
        expect(sut.strictFormModel.model).toEqual(expected);
    });
});
