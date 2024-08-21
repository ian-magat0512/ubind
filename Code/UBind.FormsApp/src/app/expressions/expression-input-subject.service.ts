import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable, Subject } from 'rxjs';

/**
 * This class acts as a central registration for rxjs subjects which provide value
 * updates for when a field's value changes, or something else changes which is an input to
 * an expression.
 * 
 * It is needed for updating expressions. When a field value updates, any expressions 
 * which may be referencing that field will need to re-evaluate.
 * 
 * This class provides the following services
 * 1. Provides an Observable for a field of the given path, to be used by expressions
 * 2. Provides access to the underlying subject so that a field can post it's value changes to it (also by it's path)
 * 
 */
@Injectable({
    providedIn: 'root',
})
export class ExpressionInputSubjectService {
    /**
     * A map from fieldPath to a subject for the value updates of a field
     */
    private fieldValueSubjects: Map<string, BehaviorSubject<any>> = new Map<string, BehaviorSubject<any>>();

    /**
     * A map from fieldPath to a subject for the search term updates of a field
     */
    private fieldSearchTermSubjects: Map<string, BehaviorSubject<any>> = new Map<string, BehaviorSubject<any>>();

    /**
     * A map from fieldPath to a subject for the whether a field is valid
     */
    private fieldValidSubjects: Map<string, BehaviorSubject<boolean>> = new Map<string, BehaviorSubject<boolean>>();

    /**
     * A map from fieldPath (for a repeating field) to a subject for the repeated count
     */
    private fieldRepeatingCountSubjects: Map<string, BehaviorSubject<number>> =
        new Map<string, BehaviorSubject<number>>();

    /**
     * A map from fieldPath to a subject for the validity of a given question set
     */
    private questionSetValidSubjects: Map<string, Subject<boolean>> = new Map<string, Subject<boolean>>();

    /**
     * A map from expression method names to a subject to say that they are out of date.
     * When something that would make an expression method return a different result changes,
     * then we want to notify any expressions so they can evaluate themselves.
     */
    private expressionMethodSubjects: Map<string, Subject<void>> = new Map<string, Subject<void>>();

    /**
     * When a field path is removed (due to a repeating question set deleting an instance),
     * we publish that it's field paths were removed.
     * However we do not remove the value and valid subjects, since the field path
     * could be added back again when a repeating instance is added back, and if
     * we deleted it then the expression would have an old reference to the subject, and theres
     * no way to tell the expression to get a new reference and start watching it once the
     * expression has been created.
     * So we keep the value and valid subjects just in case an instance is added, and
     * then when it is added we just re-use it.
     * 
     * When an instance is readded, we need to make sure the event is called, so 
     * we are tracking which ones are removed but not readded, and so when
     * someone tries to get the field value subject for one that was removed
     * we then know it was readded, so we can publish that it was added, and remove
     * it's field path from the list of removed field paths.
     */
    private removedButNotReAddedFieldPaths: Array<string> = new Array<string>();

    /**
     * gets (or creates if it doesn't exist) an rxjs subject to publish value 
     * changes for a given field into, identified by it's path.
     * 
     * @param fieldPath The path of the field. Normally this is just the fieldname, however 
     * in the case of repeating field set it could be myfield[1].firstName 
     * or some kind of path to the specific field.
     */
    public getFieldValueSubject(fieldPath: string, initialValue?: any): BehaviorSubject<any> {
        let subject: BehaviorSubject<any> = this.fieldValueSubjects.get(fieldPath);
        if (!subject) {
            subject = new BehaviorSubject<any>(initialValue);
            this.fieldValueSubjects.set(fieldPath, subject);
        } else {
            if (initialValue !== undefined) {
                subject.next(initialValue);
            }
        }
        return subject;
    }

    /**
     * When a repeating fields is reordered, we need to set the value of the field to null
     * from the map so when its reuse the value will still be null.
     * @param fieldPath 
     */
    public removeFieldValueSubject(fieldPath: string): void {
        let subject: BehaviorSubject<any> = this.fieldValueSubjects.get(fieldPath);
        if (subject == null) {
            throw new Error(`Unexpected: when trying to delete a field value subject with field path ${fieldPath}, `
                + 'it was not found.');
        }
        subject.next(null);
    }

    public deleteAllFieldValueSubjects(): void {
        this.fieldValueSubjects.clear();
    }

    /**
     * gets (or creates if it doesn't exist) an rxjs subject to publish search term 
     * changes for a given field into, identified by it's path.
     * 
     * @param fieldPath The path of the field. Normally this is just the fieldname, however 
     * in the case of repeating field set it could be myfield[1].firstName 
     * or some kind of path to the specific field.
     */
    public getFieldSearchTermSubject(fieldPath: string, initialValue: any): BehaviorSubject<any> {
        let subject: BehaviorSubject<any> = this.fieldSearchTermSubjects.get(fieldPath);
        if (!subject) {
            subject = new BehaviorSubject<any>(initialValue);
            this.fieldSearchTermSubjects.set(fieldPath, subject);
        }
        return subject;
    }

    /**
     * When a repeating fields is reordered, we need to remove the field value subject
     * from the map so it's not re-used when its fieldPath changes
     * @param fieldPath 
     */
    public deleteFieldSearchTermSubject(fieldPath: string): void {
        let subject: BehaviorSubject<any> = this.fieldSearchTermSubjects.get(fieldPath);
        if (subject == null) {
            // it's normal for a field search term subject to not exist.
            return;
        }
        this.fieldSearchTermSubjects.delete(fieldPath);
        subject.complete();
    }

    /**
     * Gets the latest field value for a field with the given fieldPath.
     * @param fieldPath 
     * @returns the latest field value for a field with the given fieldPath.
     */
    public getLatestFieldValue(fieldPath: string): any {
        let subject: BehaviorSubject<any> = this.fieldValueSubjects.get(fieldPath);
        return subject != null
            ? subject.value
            : null;
    }

    /**
     * Gets the all field values.
     * @param fieldPath 
     * @returns the field values.
     */
    public getAllFieldPaths(): Array<string> {
        let values: Array<string> = new Array<string>();
        this.fieldValueSubjects.forEach(
            (value: BehaviorSubject<any>, key: string) => {
                if (value) {
                    values.push(key);
                }
            });
        return values;
    }

    /**
     * Gets the latest field search term for a field with the given fieldPath.
     * @param fieldPath 
     * @returns the latest field search term for a field with the given fieldPath.
     */
    public getLatestFieldSearchTerm(fieldPath: string): any {
        let subject: BehaviorSubject<any> = this.fieldSearchTermSubjects.get(fieldPath);
        return subject != null
            ? subject.value
            : null;
    }

    /**
     * Gets an observable which subscribers can get updates of the latest field value, 
     * identified by the field path.
     * 
     * @param fieldPath The path of the field. Normally this is just the fieldname, however 
     * in the case of repeating field set it could be myfield[1].firstName 
     * or some kind of path to the specific field.
     */
    public getFieldValueObservable(fieldPath: string): Observable<any> {
        let subject: BehaviorSubject<any> = this.fieldValueSubjects.get(fieldPath);
        if (!subject) {
            subject = this.getFieldValueSubject(fieldPath, '');
        }
        return subject.asObservable();
    }

    /**
     * Gets an observable which subscribers can get updates of the latest field search term, 
     * identified by the field path.
     * 
     * @param fieldPath The path of the field. Normally this is just the fieldname, however 
     * in the case of repeating field set it could be myfield[1].firstName 
     * or some kind of path to the specific field.
     */
    public getFieldSearchTermObservable(fieldPath: string): Observable<any> {
        let subject: BehaviorSubject<any> = this.fieldSearchTermSubjects.get(fieldPath);
        if (!subject) {
            subject = this.getFieldSearchTermSubject(fieldPath, '');
        }
        return subject.asObservable();
    }

    /**
     * gets (or creates if it doesn't exist) an rxjs subject to publish validity changes for a given field, 
     * identified by it's path.
     * 
     * @param fieldPath The path of the field. Normally this is just the fieldname, however 
     * in the case of repeating field set it could be myfield[1].firstName 
     * or some kind of path to the specific field.
     */
    public getFieldValidSubject(fieldPath: string, initialValue: boolean): BehaviorSubject<any> {
        let subject: BehaviorSubject<boolean> = this.fieldValidSubjects.get(fieldPath);
        if (!subject) {
            subject = new BehaviorSubject<boolean>(initialValue);
            this.fieldValidSubjects.set(fieldPath, subject);
        } else {
            subject.next(initialValue);
        }
        return subject;
    }

    /**
     * When a repeating fields is reordered, we need to set the value of the field subject to null
     * from the map so when its reuse the value is null.
     * @param fieldPath 
     */
    public removeFieldValidValueSubject(fieldPath: string): void {
        let subject: BehaviorSubject<any> = this.fieldValidSubjects.get(fieldPath);
        if (subject == null) {
            throw new Error(`Unexpected: when trying to delete a field valid subject with field path ${fieldPath}, `
                + 'it was not found.');
        }
        subject.next(null);
    }

    /**
     * Gets an observable which subscribers can get updates of the latest field validity, 
     * identified by the field path.
     * 
     * @param fieldPath The path of the field. Normally this is just the fieldname, however 
     * in the case of repeating field set it could be myfield[1].firstName 
     * or some kind of path to the specific field.
     */
    public getFieldValidObservable(fieldPath: string): Observable<any> {
        let subject: BehaviorSubject<boolean> = this.fieldValidSubjects.get(fieldPath);
        if (!subject) {
            subject = this.getFieldValidSubject(fieldPath, false);
        }
        return subject.asObservable();
    }

    /**
     * gets (or creates if it doesn't exist) an rxjs subject to publish the 
     * repeating count changes for a given repeating field, identified by it's path.
     * 
     * @param fieldPath The path of the field 
     * @param initialValue an initial value to publish
     */
    public getFieldRepeatingCountSubject(fieldPath: string, initialValue: number): BehaviorSubject<number> {
        let subject: BehaviorSubject<number> = this.fieldRepeatingCountSubjects.get(fieldPath);
        if (!subject) {
            subject = new BehaviorSubject<number>(initialValue);
            this.fieldRepeatingCountSubjects.set(fieldPath, subject);
        } else {
            subject.next(initialValue);
        }
        return subject;
    }

    /**
     * Gets an observable which subscribers can get updates of the latest field value, 
     * identified by the field path.
     * 
     * @param fieldPath The path of the field. Normally this is just the fieldname, however 
     * in the case of repeating field set it could be myfield[1].firstName 
     * or some kind of path to the specific field.
     */
    public getFieldRepeatingCountObservable(fieldPath: string): Observable<number> {
        let subject: BehaviorSubject<number> = this.fieldRepeatingCountSubjects.get(fieldPath);
        if (!subject) {
            subject = this.getFieldRepeatingCountSubject(fieldPath, 0);
        }
        return subject.asObservable();
    }

    /**
     * Gets or creates an rxjs subject to publish validity changes for a given question set into, 
     * identified by it's path.
     * 
     * @param questionSetPath The path of the question set.
     * Examples:
     * ratingPrimary
     * ratingPrimary.theVehicles[0]
     * ratingPrimary.theDrivers[1].theClaims[0]
     */
    public getQuestionSetValidSubject(questionSetPath: string): Subject<boolean> {
        let subject: Subject<boolean> = this.questionSetValidSubjects.get(questionSetPath);
        if (!subject) {
            subject = new Subject<boolean>();
            this.questionSetValidSubjects.set(questionSetPath, subject);
        }
        return subject;
    }

    /**
     * Gets an observable which subscribers can get updates of the latest validity status of, 
     * a question set identified by the field path.
     * 
     * @param questionSetPath The path of the question set.
     * Examples:
     * ratingPrimary
     * ratingPrimary.theVehicles[0]
     * ratingPrimary.theDrivers[1].theClaims[0]
     */
    public getQuestionSetValidObservable(fieldPath: string): Observable<boolean> {
        return this.getQuestionSetValidSubject(fieldPath).asObservable();
    }

    /**
     * Create an rxjs subject to publish a notification when a possible underlying dependency 
     * or input to the execution of an expression method changes.
     * 
     * This allows an expression to know that it should reevaluate when something changes.
     * 
     * @param expressionMethodName the name of the expression method.
     */
    public createExpressionMethodSubject(expressionMethodName: string): Subject<void> {
        let subject: Subject<void> = this.expressionMethodSubjects.get(expressionMethodName);
        if (subject) {
            // uncomment the following to check your code:
            /*
            throw Errors.Product.Configuration("You are trying to create an expression method subject for the "
                + `expression method named "${expressionMethodName}", however one already exists.`);
            */
        } else {
            subject = new Subject<void>();
            this.expressionMethodSubjects.set(expressionMethodName, subject);
        }
        return subject;
    }

    /**
     * Gets an observable which can be subscribed to for the purpose of know when something which
     * is an input or dependency for an expression method has changed. This is used by an expression
     * to know when it needs to evaluate and update it's result.
     * 
     * @param expressionMethodName The name of the expression method.
     * @returns the observable, if one exists, otherwise null.
     */
    public getExpressionMethodSubjectObservable(expressionMethodName: string): Observable<void> {
        let subject: Subject<void> = this.expressionMethodSubjects.get(expressionMethodName);
        if (!subject) {
            return null;
        }
        return subject.asObservable();
    }
}
