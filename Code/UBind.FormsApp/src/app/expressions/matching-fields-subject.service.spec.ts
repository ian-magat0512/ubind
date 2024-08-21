import { EventService } from "@app/services/event.service";
import { take } from "rxjs/operators";
import { ExpressionInputSubjectService } from "./expression-input-subject.service";
import { MatchingFieldsSubjectService } from "./matching-fields-subject.service";

describe('MatchingFieldsSubjectService', () => {

    let service: MatchingFieldsSubjectService;
    let eventService: EventService;
    let expressionInputSubjectService: ExpressionInputSubjectService;

    beforeEach(() => {
        eventService = new EventService();
        expressionInputSubjectService = new ExpressionInputSubjectService();
        service = new MatchingFieldsSubjectService(eventService, expressionInputSubjectService);
    });

    it('should publish a new array with the new fieldPath when a new fieldpath matches the pattern', () => {
        // Arrange
        let fieldPathPattern: string = 'claims[*].amount';
        let fieldPath: string = 'claims[0].amount';
        let observable: any = service.getMatchingFieldsObservable(fieldPathPattern);

        // Act
        eventService.fieldPathAddedSubject.next(fieldPath);

        // Assert
        observable.pipe(take(1)).subscribe((matchedFieldPaths: Array<string>) => {
            expect(matchedFieldPaths.length).toBe(1);
            expect(matchedFieldPaths[0]).toBe(fieldPath);
        });
    });

    it('should publish a new array without the fieldPath when a fieldpath matching the pattern is removed', () => {
        // Arrange
        let fieldPathPattern: string = 'claims[*].amount';
        let fieldPath1: string = 'claims[0].amount';
        let fieldPath2: string = 'claims[1].amount';
        let fieldPath3: string = 'claims[2].amount';
        let observable: any = service.getMatchingFieldsObservable(fieldPathPattern);
        eventService.fieldPathAddedSubject.next(fieldPath1);
        eventService.fieldPathAddedSubject.next(fieldPath2);
        eventService.fieldPathAddedSubject.next(fieldPath3);
        let latestArray: Array<any> = new Array<any>();
        observable.pipe(take(1)).subscribe((matchedFieldPaths: Array<string>) => {
            latestArray = matchedFieldPaths;
        });
        expect(latestArray.length).toBe(3);

        // Act
        eventService.fieldPathRemovedSubject.next(fieldPath2);
        observable.pipe(take(1)).subscribe((matchedFieldPaths: Array<string>) => {
            latestArray = matchedFieldPaths;
        });

        // Assert
        expect(latestArray.length).toBe(2);
        expect(latestArray[0]).toBe(fieldPath1);
        expect(latestArray[1]).toBe(fieldPath3);
    });

    it('should use existing field value observables when creating the subject', () => {
        // Arrange
        let fieldPathPattern: string = 'claims[*].amount';
        let fieldPath1: string = 'claims[0].amount';
        let fieldPath2: string = 'claims[1].amount';
        let fieldPath3: string = 'claims[2].amount';
        eventService.fieldPathAddedSubject.next(fieldPath1);
        eventService.fieldPathAddedSubject.next(fieldPath2);
        eventService.fieldPathAddedSubject.next(fieldPath3);

        // Act
        let observable: any = service.getMatchingFieldsObservable(fieldPathPattern);
        let latestArray: Array<string> = new Array<string>();
        observable.pipe(take(1)).subscribe((matchedFieldPaths: Array<string>) => {
            latestArray = matchedFieldPaths;
        });

        // Assert
        expect(latestArray.length).toBe(3);
        expect(latestArray[0]).toBe(fieldPath1);
        expect(latestArray[1]).toBe(fieldPath2);
        expect(latestArray[2]).toBe(fieldPath3);
    });

    it('fieldValuesForMatchingFieldsSubject should publish a new array with the '
        + 'latest field values when a field is updated', () => {
        // Arrange
        let fieldPathPattern: string = 'claims[*].amount';
        let fieldPath1: string = 'claims[0].amount';
        let fieldPath2: string = 'claims[1].amount';
        let fieldPath3: string = 'claims[2].amount';
        eventService.fieldPathAddedSubject.next('claims[0].amount');
        eventService.fieldPathAddedSubject.next('claims[1].amount');
        eventService.fieldPathAddedSubject.next('claims[2].amount');
        let field1Subject: any = expressionInputSubjectService.getFieldValueSubject(fieldPath1, 10);
        let field2Subject: any = expressionInputSubjectService.getFieldValueSubject(fieldPath2, 20);
        let field3Subject: any = expressionInputSubjectService.getFieldValueSubject(fieldPath3, 30);
        let latestArray: Array<any> = new Array<any>();
        service.getFieldValuesForMatchingFieldsObservable(fieldPathPattern)
            .subscribe((values: Array<any>) => {
                latestArray = values;
            });
        expect(latestArray.length).toBe(3);
        expect(latestArray[0]).toBe(10);
        expect(latestArray[1]).toBe(20);
        expect(latestArray[2]).toBe(30);

        // Act + Assert
        field1Subject.next(11);
        expect(latestArray[0]).toBe(11);
        field2Subject.next(21);
        expect(latestArray[1]).toBe(21);
        field3Subject.next(31);
        expect(latestArray[2]).toBe(31);
        field2Subject.next(50);
        expect(latestArray[1]).toBe(50);
    });

    it('fieldValuesForMatchingFieldsSubject should publish a new array with the '
        + 'latest field values when a field that matches is added', () => {
        // Arrange
        let fieldPathPattern: string = 'claims[*].amount';
        let fieldPath1: string = 'claims[0].amount';
        let fieldPath2: string = 'claims[1].amount';
        let fieldPath3: string = 'claims[2].amount';
        eventService.fieldPathAddedSubject.next('claims[0].amount');
        eventService.fieldPathAddedSubject.next('claims[1].amount');
        expressionInputSubjectService.getFieldValueSubject(fieldPath1, 10);
        expressionInputSubjectService.getFieldValueSubject(fieldPath2, 20);
        let latestArray: Array<any> = new Array<any>();
        service.getFieldValuesForMatchingFieldsObservable(fieldPathPattern)
            .subscribe((values: Array<any>) => {
                latestArray = values;
            });
        expect(latestArray.length).toBe(2);
        expect(latestArray[0]).toBe(10);
        expect(latestArray[1]).toBe(20);

        // Act
        eventService.fieldPathAddedSubject.next('claims[2].amount');
        expressionInputSubjectService.getFieldValueSubject(fieldPath3, 30);

        // Assert
        expect(latestArray.length).toBe(3);
        expect(latestArray[0]).toBe(10);
        expect(latestArray[1]).toBe(20);
        expect(latestArray[2]).toBe(30);
    });

    it('fieldValuesForMatchingFieldsSubject should publish a new array '
        + 'with the latest field values when a field that matches is removed', () => {
        // Arrange
        let fieldPathPattern: string = 'claims[*].amount';
        let fieldPath1: string = 'claims[0].amount';
        let fieldPath2: string = 'claims[1].amount';
        let fieldPath3: string = 'claims[2].amount';
        eventService.fieldPathAddedSubject.next('claims[0].amount');
        eventService.fieldPathAddedSubject.next('claims[1].amount');
        eventService.fieldPathAddedSubject.next('claims[2].amount');
        expressionInputSubjectService.getFieldValueSubject(fieldPath1, 10);
        expressionInputSubjectService.getFieldValueSubject(fieldPath2, 20);
        expressionInputSubjectService.getFieldValueSubject(fieldPath3, 30);
        let latestArray: Array<any> = new Array<any>();
        service.getFieldValuesForMatchingFieldsObservable(fieldPathPattern)
            .subscribe((values: Array<any>) => {
                latestArray = values;
            });
        expect(latestArray.length).toBe(3);
        expect(latestArray[0]).toBe(10);
        expect(latestArray[1]).toBe(20);
        expect(latestArray[2]).toBe(30);

        // Act
        eventService.fieldPathRemovedSubject.next('claims[1].amount');

        // Assert
        expect(latestArray.length).toBe(2);
        expect(latestArray[0]).toBe(10);
        expect(latestArray[1]).toBe(30);
    });

    it('should provide all field paths within all instances of repeating question sets', () => {
        // Arrange
        let fieldPathPattern: string = 'claims';
        let fieldPath1: string = 'claims[0].amount';
        let fieldPath2: string = 'claims[0].date';
        let fieldPath3: string = 'claims[1].amount';
        let fieldPath4: string = 'claims[1].date';
        let fieldPath5: string = 'claims[2].amount';
        let fieldPath6: string = 'claims[2].date';
        eventService.fieldPathAddedSubject.next('claims[0].amount');
        eventService.fieldPathAddedSubject.next('claims[0].date');
        eventService.fieldPathAddedSubject.next('claims[1].amount');
        eventService.fieldPathAddedSubject.next('claims[1].date');
        eventService.fieldPathAddedSubject.next('claims[2].amount');
        eventService.fieldPathAddedSubject.next('claims[2].date');
        expressionInputSubjectService.getFieldValueSubject(fieldPath1, 10);
        expressionInputSubjectService.getFieldValueSubject(fieldPath2, '12/12/2002');
        expressionInputSubjectService.getFieldValueSubject(fieldPath3, 30);
        expressionInputSubjectService.getFieldValueSubject(fieldPath4, '12/12/2004');
        expressionInputSubjectService.getFieldValueSubject(fieldPath5, 50);
        expressionInputSubjectService.getFieldValueSubject(fieldPath6, '12/12/2006');

        // Act
        let paths: Array<string> = service.getFieldPathsMatchingPattern(fieldPathPattern);

        // Assert
        expect(paths.length).toBe(6);
        expect(paths[0]).toBe('claims[0].amount');
        expect(paths[1]).toBe('claims[0].date');
        expect(paths[2]).toBe('claims[1].amount');
        expect(paths[3]).toBe('claims[1].date');
        expect(paths[4]).toBe('claims[2].amount');
        expect(paths[5]).toBe('claims[2].date');
    });
});
