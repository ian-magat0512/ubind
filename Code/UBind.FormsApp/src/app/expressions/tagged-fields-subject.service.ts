import { Injectable } from "@angular/core";
import { QuestionMetadata } from "@app/models/question-metadata";
import { EventService } from "@app/services/event.service";
import { FieldMetadataService } from "@app/services/field-metadata.service";
import { Observable } from "rxjs";
import { BehaviorSubject } from "rxjs";
import { map } from "rxjs/operators";
import { ExpressionInputSubjectService } from "./expression-input-subject.service";

/**
 * Maintains rxjs subjects which publish a list of fieldPaths for fields
 * which have a given tag, as they come into existence and disappear.
 */
@Injectable({
    providedIn: 'root',
})
export class TaggedFieldsSubjectService {

    /**
     * A map from a tag to a subject which returns a list of matching fieldPaths that exist.
     */
    private fieldsWithTagSubjects: Map<string, BehaviorSubject<Array<string>>>
        = new Map<string, BehaviorSubject<Array<string>>>();

    /**
     * A map from a tag to a subject which returns a list of field values for fields who have the given tag.
     */
    private fieldValuesForFieldsWithTagObservables: Map<string, Observable<Array<any>>>
        = new Map<string, Observable<Array<any>>>();

    public constructor(
        private eventService: EventService,
        private fieldMetadataService: FieldMetadataService,
        private expressionInputSubjectService: ExpressionInputSubjectService,
    ) {
        this.listenForFieldPathsAddedOrRemoved();
    }

    private listenForFieldPathsAddedOrRemoved(): void {
        this.eventService.fieldPathAddedSubject
            .subscribe((fieldPath: string) => this.handleFieldPathAdded(fieldPath));
        this.eventService.fieldPathRemovedSubject
            .subscribe((fieldPath: string) => this.handleFieldPathRemoved(fieldPath));
    }

    /**
     * Gets an rxjs subject which publishes a new array each time the list of fields with a given tag changes.
     * @param tag the tag
     */
    public getFieldsWithTagSubject(tag: string): BehaviorSubject<Array<string>> {
        let subject: BehaviorSubject<Array<string>> = this.fieldsWithTagSubjects.get(tag);
        if (!subject) {
            subject = new BehaviorSubject<Array<string>>([]);
            this.fieldsWithTagSubjects.set(tag, subject);
        }
        return subject;
    }

    /**
     * Gets an rxjs observable which publishes a new array each time the list of fields with a given tag changes.
     * @param tag the tag
     */
    public getFieldsWithTagObservable(tag: string): Observable<Array<string>> {
        return this.getFieldsWithTagSubject(tag).asObservable();
    }

    /**
     * Gets or creates a subscription to an array of field values where the fields match
     * the given fieldPathPattern
     */
    public getFieldValuesForFieldsWithTagObservable(tag: any): Observable<Array<any>> {
        let observable: Observable<Array<any>> = this.fieldValuesForFieldsWithTagObservables.get(tag);
        if (!observable) {
            observable = this.createFieldValuesForFieldsWithTagObservable(tag);
            this.fieldValuesForFieldsWithTagObservables.set(tag, observable);
        }
        return observable;
    }

    private createFieldValuesForFieldsWithTagObservable(tag: any): Observable<Array<any>> {
        const fieldsWithTagObservable: Observable<Array<any>> =
            this.getFieldsWithTagObservable(tag);
        let valuesArraySubject: BehaviorSubject<Array<any>> = new BehaviorSubject<Array<any>>(new Array<any>());
        fieldsWithTagObservable.pipe(
            // convert the array of field paths to an array of observable of field values
            map((fieldPaths: Array<string>) => fieldPaths.map((fieldPath: string) => {
                return this.expressionInputSubjectService.getFieldValueObservable(fieldPath);
            })),
        )
            .subscribe((fieldValueObservables: Array<Observable<any>>) => {
                let valuesArray: Array<any> = new Array<any>();
                let completedOnce: boolean = false;
                for (let i: number = 0; i < fieldValueObservables.length; i++) {
                    // we have to copy the index so that the function inside the subscribe has a local copy 
                    // for later, so it updates the correct results array. Without this, future values would
                    // always update the latest value of i, which would be fieldPathObservables.length -1.
                    let currentObservableIndex: number = i;
                    fieldValueObservables[i].subscribe((value: any) => {
                        valuesArray[currentObservableIndex] = value;
                        if (completedOnce) {
                            valuesArraySubject.next(valuesArray);
                        }
                    });
                }
                completedOnce = true;
                valuesArraySubject.next(valuesArray);
            });
        return valuesArraySubject.asObservable();
    }

    /**
     * When a new fieldPath is known to exist, it updates any matchingFieldsSubjects that may match.
     */
    private handleFieldPathAdded(fieldPath: string): void {
        let metadata: QuestionMetadata = this.fieldMetadataService.getMetadataForField(fieldPath);
        if (metadata && metadata.tags && metadata.tags.length) {
            for (let tag of metadata.tags) {
                let subject: BehaviorSubject<Array<string>> = this.fieldsWithTagSubjects.get(tag);
                if (!subject) {
                    subject = new BehaviorSubject<Array<string>>([fieldPath]);
                    this.fieldsWithTagSubjects.set(tag, subject);
                } else {
                    let currentFieldPaths: Array<string> = subject.value;
                    if (!currentFieldPaths.includes(fieldPath)) {
                        let newFieldPaths: Array<string> = [...currentFieldPaths];
                        newFieldPaths.push(fieldPath);
                        subject.next(newFieldPaths);
                    }
                }
            }
        }
    }

    /**
     * When we remove a repeating question set we should also remove it's fieldpath
     * @param fieldPath 
     */
    private handleFieldPathRemoved(fieldPath: string): void {
        this.fieldsWithTagSubjects.forEach((
            mapEntryValue: BehaviorSubject<Array<string>>,
            mapEntryKey: string,
            map: any) => {
            let subject: BehaviorSubject<Array<string>> = mapEntryValue;
            let currentFieldPaths: Array<string> = subject.value;
            let index: number = currentFieldPaths.indexOf(fieldPath);
            if (index != -1) {
                let newFieldPaths: Array<string> = [...currentFieldPaths];
                newFieldPaths.splice(index, 1);
                subject.next(newFieldPaths);
            }
        });
    }

    /**
     * This is called when configuration is updated and a field has a tag added to it
     */
    public onTagsAddedToField(fieldPath: string, tagsAdded: Array<string>): void {
        for (let tag of tagsAdded) {
            let subject: BehaviorSubject<Array<string>> = this.fieldsWithTagSubjects.get(tag);
            if (!subject) {
                subject = new BehaviorSubject<Array<string>>([fieldPath]);
                this.fieldsWithTagSubjects.set(tag, subject);
            } else {
                let currentFieldPaths: Array<string> = subject.value;
                if (!currentFieldPaths.includes(fieldPath)) {
                    let newFieldPaths: Array<string> = [...currentFieldPaths];
                    newFieldPaths.push(fieldPath);
                    subject.next(newFieldPaths);
                }
            }
        }
    }

    /**
     * This is called when configuration is updated and a field has a tag removed from it
     */
    public onTagsRemovedFromField(fieldPath: string, tagsRemoved: Array<string>): void {
        for (let tag of tagsRemoved) {
            let subject: BehaviorSubject<Array<string>> = this.fieldsWithTagSubjects.get(tag);
            let currentFieldPaths: Array<string> = subject.value;
            let index: number = currentFieldPaths.indexOf(fieldPath);
            if (index != -1) {
                currentFieldPaths.splice(index, 1);
                subject.next(currentFieldPaths);
            }
        }
    }
}
