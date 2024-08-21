import { ConfigService } from "@app/services/config.service";
import { EventService } from "@app/services/event.service";
import { FieldMetadataService } from "@app/services/field-metadata.service";
import { FakeConfigServiceForMetadata } from "@app/services/test/fake-config-service-for-metadata";
import { take } from "rxjs/operators";
import { ExpressionInputSubjectService } from "./expression-input-subject.service";
import { TaggedFieldsSubjectService } from "./tagged-fields-subject.service";

describe('TaggedFieldsSubjectService', () => {

    let service: TaggedFieldsSubjectService;
    let eventService: EventService;
    let fieldMetadataService: FieldMetadataService;
    let configService: ConfigService;
    let fakeConfigServiceForMetadata: FakeConfigServiceForMetadata;
    let expressionInputSubjectService: ExpressionInputSubjectService;

    beforeEach(() => {
        eventService = new EventService();
        expressionInputSubjectService = new ExpressionInputSubjectService();
        fakeConfigServiceForMetadata = new FakeConfigServiceForMetadata();
        configService = <ConfigService><any>fakeConfigServiceForMetadata;
        fieldMetadataService = new FieldMetadataService(configService, eventService, expressionInputSubjectService);
        service = new TaggedFieldsSubjectService(eventService, fieldMetadataService, expressionInputSubjectService);
    });

    it('should publish a new array with the new fieldPath when a new fieldpath has the tag', () => {
        // Arrange
        let tag: string = 'disclosure';
        let fieldPath: string = 'claims[0].amount';
        fakeConfigServiceForMetadata.addTaggedMetadataResult(['disclosure'], 10);
        let observable: any = service.getFieldsWithTagObservable(tag);

        // Act
        eventService.fieldPathAddedSubject.next(fieldPath);

        // Assert
        observable.pipe(take(1)).subscribe((fieldPaths: Array<string>) => {
            expect(fieldPaths.length).toBe(1);
            expect(fieldPaths[0]).toBe(fieldPath);
        });
    });

    it('should publish a new array without the fieldPath when the tagged field removed', () => {
        // Arrange
        let tag: string = 'disclosure';
        let fieldPath1: string = 'claims[0].amount';
        let fieldPath2: string = 'claims[1].amount';
        let fieldPath3: string = 'claims[2].amount';
        let observable: any = service.getFieldsWithTagObservable(tag);
        fakeConfigServiceForMetadata.addTaggedMetadataResult(['disclosure'], 10);
        eventService.fieldPathAddedSubject.next(fieldPath1);
        eventService.fieldPathAddedSubject.next(fieldPath2);
        eventService.fieldPathAddedSubject.next(fieldPath3);
        let latestArray: Array<any> = new Array<any>();
        observable.pipe(take(1)).subscribe((fieldPaths: Array<string>) => {
            latestArray = fieldPaths;
        });
        expect(latestArray.length).toBe(3);

        // Act
        eventService.fieldPathRemovedSubject.next(fieldPath2);
        observable.pipe(take(1)).subscribe((fieldPaths: Array<string>) => {
            latestArray = fieldPaths;
        });

        // Assert
        expect(latestArray.length).toBe(2);
        expect(latestArray[0]).toBe(fieldPath1);
        expect(latestArray[1]).toBe(fieldPath3);
    });

    it('should include existing tagged fields know to the system when creating the subject', () => {
        // Arrange
        let tag: string = 'disclosure';
        let fieldPath1: string = 'claims[0].amount';
        let fieldPath2: string = 'claims[1].amount';
        let fieldPath3: string = 'claims[2].amount';
        fakeConfigServiceForMetadata.addTaggedMetadataResult(['disclosure'], 10);
        eventService.fieldPathAddedSubject.next(fieldPath1);
        eventService.fieldPathAddedSubject.next(fieldPath2);
        eventService.fieldPathAddedSubject.next(fieldPath3);

        // Act
        let observable: any = service.getFieldsWithTagObservable(tag);
        let latestArray: Array<string> = new Array<string>();
        observable.pipe(take(1)).subscribe((fieldPaths: Array<string>) => {
            latestArray = fieldPaths;
        });

        // Assert
        expect(latestArray.length).toBe(3);
        expect(latestArray[0]).toBe(fieldPath1);
        expect(latestArray[1]).toBe(fieldPath2);
        expect(latestArray[2]).toBe(fieldPath3);
    });

    it('fieldValuesForFieldsWithTagSubject should publish a new array with the '
        + 'latest field values when a field is updated', () => {
        // Arrange
        let tag: string = 'disclosure';
        let fieldPath1: string = 'claims[0].amount';
        let fieldPath2: string = 'claims[1].amount';
        let fieldPath3: string = 'claims[2].amount';
        fakeConfigServiceForMetadata.addTaggedMetadataResult(['disclosure'], 10);
        eventService.fieldPathAddedSubject.next('claims[0].amount');
        eventService.fieldPathAddedSubject.next('claims[1].amount');
        eventService.fieldPathAddedSubject.next('claims[2].amount');
        let field1Subject: any = expressionInputSubjectService.getFieldValueSubject(fieldPath1, 10);
        let field2Subject: any = expressionInputSubjectService.getFieldValueSubject(fieldPath2, 20);
        let field3Subject: any = expressionInputSubjectService.getFieldValueSubject(fieldPath3, 30);
        let latestArray: Array<any> = new Array<any>();
        service.getFieldValuesForFieldsWithTagObservable(tag)
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

    it('fieldValuesForFieldsWithTagSubject should publish a new array with the '
        + 'latest field values when a field that matches is added', () => {
        // Arrange
        let tag: string = 'disclosure';
        let fieldPath1: string = 'claims[0].amount';
        let fieldPath2: string = 'claims[1].amount';
        let fieldPath3: string = 'claims[2].amount';
        fakeConfigServiceForMetadata.addTaggedMetadataResult(['disclosure'], 10);
        eventService.fieldPathAddedSubject.next('claims[0].amount');
        eventService.fieldPathAddedSubject.next('claims[1].amount');
        expressionInputSubjectService.getFieldValueSubject(fieldPath1, 10);
        expressionInputSubjectService.getFieldValueSubject(fieldPath2, 20);
        let latestArray: Array<any> = new Array<any>();
        service.getFieldValuesForFieldsWithTagObservable(tag)
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

    it('fieldValuesForFieldsWithTagSubject should publish a new array '
        + 'with the latest field values when a field that matches is removed', () => {
        // Arrange
        let tag: string = 'disclosure';
        let fieldPath1: string = 'claims[0].amount';
        let fieldPath2: string = 'claims[1].amount';
        let fieldPath3: string = 'claims[2].amount';
        fakeConfigServiceForMetadata.addTaggedMetadataResult(['disclosure'], 10);
        eventService.fieldPathAddedSubject.next('claims[0].amount');
        eventService.fieldPathAddedSubject.next('claims[1].amount');
        eventService.fieldPathAddedSubject.next('claims[2].amount');
        expressionInputSubjectService.getFieldValueSubject(fieldPath1, 10);
        expressionInputSubjectService.getFieldValueSubject(fieldPath2, 20);
        expressionInputSubjectService.getFieldValueSubject(fieldPath3, 30);
        let latestArray: Array<any> = new Array<any>();
        service.getFieldValuesForFieldsWithTagObservable(tag)
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

});
