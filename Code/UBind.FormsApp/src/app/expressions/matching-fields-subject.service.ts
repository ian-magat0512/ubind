import { Injectable } from "@angular/core";
import { StringHelper } from "@app/helpers/string.helper";
import { EventService } from "@app/services/event.service";
import { Observable, BehaviorSubject } from "rxjs";
import { map } from "rxjs/operators";
import { ExpressionInputSubjectService } from "./expression-input-subject.service";

/**
 * Maintains rxjs subjects which publish a list of fieldPaths that match 
 * the given field path pattern. This is needed for expression methods that sum up
 * a field within a repeating question. If the fieldPathPattern matches a field 
 * within the repeating question set, then as new instances of the repeating question 
 * set are added, their fieldPaths are added to the list.
 * 
 * Then, the list of fieldPaths can be converted by the Expression to an array of value subscriptions, 
 * that can then keep an internal list updated with the latest values, and as the values change, can
 * pass them to the sum() expression method which can add them up.
 */
@Injectable({
    providedIn: 'root',
})
export class MatchingFieldsSubjectService {

    /**
     * A map from a fieldPathPattern to a subject which returns a list of matching fieldPaths that exist.
     */
    private matchingFieldsSubjects: Map<string, BehaviorSubject<Array<string>>>
        = new Map<string, BehaviorSubject<Array<string>>>();

    /**
     * A map from a fieldPathPattern to a subject which returns a list of field values for fields whose 
     * fieldPaths match.
     */
    private fieldValuesForMatchingFieldsObservables: Map<string, Observable<Array<any>>>
        = new Map<string, Observable<Array<any>>>();

    private allFieldPaths: Set<string> = new Set<string>();

    public constructor(
        private eventService: EventService,
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
     * Gets or creates an rxjs subject which publishes a list of fieldPaths that match 
     * the given field path pattern. This is needed for expression methods that sum up
     * a field within a repeating question set. If the fieldPathPattern matches a field 
     * within the repeating question set, then as new instances of the repeating question 
     * set are added, their fieldPaths are added to the list.
     * @param fieldPathPattern a pattern for matching fields into the form model. You can use a * as a wildcard
     * //TODO: currently the fieldPathPattern needs to be in jsonpath style format, but we may want to consider
     * using JSON Pointer.
     * 
     * Example fieldPathPattern: claims[*].amount
     */
    public getMatchingFieldsSubject(fieldPathPattern: string): BehaviorSubject<Array<string>> {
        let subject: BehaviorSubject<Array<string>> = this.matchingFieldsSubjects.get(fieldPathPattern);
        if (!subject) {
            let currentFieldPaths: Array<string> = new Array<string>();
            this.allFieldPaths.forEach((fieldPath: string) => {
                if (this.matchesPattern(fieldPath, fieldPathPattern)) {
                    currentFieldPaths.push(fieldPath);
                }
            });
            subject = new BehaviorSubject<Array<string>>(currentFieldPaths);
            this.matchingFieldsSubjects.set(fieldPathPattern, subject);
        }
        return subject;
    }

    /**
     * Gets a list of fieldPaths that match the given field path pattern. This is needed for summary tables
     * where we are trying to render all of the fields within a repeating question set.
     * @param fieldPathPattern a pattern for matching fields into the form model. You can use a * as a wildcard
     * //TODO: currently the fieldPathPattern needs to be in jsonpath style format, but we may want to consider
     * using JSON Pointer.
     * 
     * Example fieldPathPattern: claims[*].amount
     */
    public getFieldPathsMatchingPattern(fieldPathPattern: string): Array<string> {
        return this.getMatchingFieldsSubject(fieldPathPattern).value;
    }

    /**
     * Gets an observable which subscribers can get updates on the list of fieldPaths in existence 
     * which match the given fieldPathPattern.
     * 
     * @param fieldPathPattern a pattern for matching fields into the form model. You can use a * as a wildcard
     * //TODO: currently the fieldPathPattern needs to be in jsonpath style format, but we may want to consider
     * using JSON Pointer.
     */
    public getMatchingFieldsObservable(fieldPathPattern: string): Observable<Array<string>> {
        return this.getMatchingFieldsSubject(fieldPathPattern).asObservable();
    }

    /**
     * When a new fieldPath is known to exist, it updates any matchingFieldsSubjects that may match.
     */
    private handleFieldPathAdded(fieldPath: string): void {
        this.allFieldPaths.add(fieldPath);
        this.matchingFieldsSubjects.forEach((
            mapEntryValue: BehaviorSubject<Array<string>>,
            mapEntryKey: string,
            map: any) => {
            let fieldPathPattern: string = mapEntryKey;
            let subject: BehaviorSubject<Array<string>> = mapEntryValue;
            if (this.matchesPattern(fieldPath, fieldPathPattern)) {
                let currentFieldPaths: Array<string> = subject.value;
                if (!currentFieldPaths.includes(fieldPath)) {
                    let newFieldPaths: Array<string> = [...currentFieldPaths];
                    newFieldPaths.push(fieldPath);
                    subject.next(newFieldPaths);
                }
            }
        });
    }

    /**
     * When we remove a repeating question set we should also remove it's fieldpath
     * @param fieldPath 
     */
    private handleFieldPathRemoved(fieldPath: string): void {
        this.allFieldPaths.delete(fieldPath);
        this.matchingFieldsSubjects.forEach((
            mapEntryValue: BehaviorSubject<Array<string>>,
            mapEntryKey: string,
            map: any) => {
            let fieldPathPattern: string = mapEntryKey;
            let subject: BehaviorSubject<Array<string>> = mapEntryValue;
            if (this.matchesPattern(fieldPath, fieldPathPattern)) {
                let currentFieldPaths: Array<string> = subject.value;
                let index: number = currentFieldPaths.indexOf(fieldPath);
                if (index != -1) {
                    let newFieldPaths: Array<string> = [...currentFieldPaths];
                    newFieldPaths.splice(index, 1);
                    subject.next(newFieldPaths);
                }
            }
        });
    }

    private matchesPattern(fieldPath: string, fieldPathPattern: string): boolean {
        let regex: RegExp = this.convertFieldPathPatternToRegularExpression(fieldPathPattern);
        return regex.test(fieldPath);
    }

    private convertFieldPathPatternToRegularExpression(fieldPathPattern: string): RegExp {
        let regexPattern: string = `^${fieldPathPattern}`;
        regexPattern = StringHelper.replaceAll(regexPattern, '[', '\\[');
        regexPattern = StringHelper.replaceAll(regexPattern, ']', '\\]');
        regexPattern = StringHelper.replaceAll(regexPattern, '.', '\\.');
        regexPattern = StringHelper.replaceAll(regexPattern, '*', '.*');
        return new RegExp(regexPattern);
    }

    /**
     * Gets or creates a subscription to an array of field values where the fields match
     * the given fieldPathPattern
     */
    public getFieldValuesForMatchingFieldsObservable(fieldPathPattern: any): Observable<Array<any>> {
        let observable: Observable<Array<any>> = this.fieldValuesForMatchingFieldsObservables.get(fieldPathPattern);
        if (!observable) {
            observable = this.createFieldValuesForMatchingFieldsObservable(fieldPathPattern);
            this.fieldValuesForMatchingFieldsObservables.set(fieldPathPattern, observable);
        }
        return observable;
    }

    private createFieldValuesForMatchingFieldsObservable(fieldPathPattern: any): Observable<Array<any>> {
        const matchingFieldsObservable: Observable<Array<any>> =
            this.getMatchingFieldsObservable(fieldPathPattern);
        let valuesArraySubject: BehaviorSubject<Array<any>> = new BehaviorSubject<Array<any>>(new Array<any>());
        matchingFieldsObservable.pipe(
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
}
