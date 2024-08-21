import { BehaviorSubject, Observable, Subject, SubscriptionLike } from 'rxjs';
import { distinctUntilChanged, filter, takeUntil, shareReplay } from 'rxjs/operators';
import { Disposable } from '@app/models/disposable';
import { Errors } from '@app/models/errors';
import { ExpressionDependencies } from './expression-dependencies';
import { RelativeJsonPointerResolver } from '@app/helpers/relative-json-pointer-resolver';
import { ExpressionPatterns } from './expression-patterns';
import { StringHelper } from '@app/helpers/string.helper';
import matchAll from "string.prototype.matchall";

/**
 * Structure which provides for an observable which observes a particular field's value,
 * and specifies the positions within the expression which it is referenced 
 * (and would therefore need to be inserted before evaluating the expression)
 */
interface FieldValueObservableAndIndex {
    fieldValueObservable: Observable<any>;
    latestValuesIndex: number;
}

/**
 * Fixed values that can be passed in to the expression as a named property, so that
 * when the expression is evaluated if that named property is referenced within the expression
 * it can be resolved to the value.
 */
export interface FixedArguments {
    [name: string]: any;
}

/**
 * Observable values that can be passed in to the expression as a named property,
 * so that when they change the expression is re-evaluated against the new value.
 */
export interface ObservableArguments {
    [name: string]: Observable<any>;
}

/**
 * Represents an expression that can be evaluated to return a result.
 * Expressions are used to determine thing like whether a field should be shown or hidden,
 * The value of a field, or really anything else.
 * 
 * An expression "listens" to other fields which it may be dependent upon, 
 * so that if one of those values changes, then the expression
 * recalculates it's value, and then sends an notification to subscribers containing the new value.
 * 
 * The subscriber of an expression is normally a field under which it's defined.
 * 
 * TODO: The parser is in this class is really basic and quite poor. It should be replaced with a
 * proper parser such as https://github.com/SAP/chevrotain or one of the ones listed here:
 * https://sap.github.io/chevrotain/performance/
 */
export class Expression implements Disposable {

    /**
     * The original raw string value of the expression
     */
    private originalExpressionSource: string;

    /**
     * The expression source after applying any backwards compatibility replacements
     */
    private _source: string;

    /**
     * fixed string values are extracted from the expression and
     * added to this array in order of their appearance
     */
    private strings: Array<string> = new Array<string>();
    /**
     * the regexPatterns extracted from the expression
     */
    private regexPatterns: Array<string> = new Array<string>();
    /**
     * The resolved arguments which is passed into each expression evaluation.
     * This is made up of:
     * a. Fixed values
     * b. Observable values
     */
    public arguments: Map<string, any> = new Map<string, any>();

    protected parsedExpression: string;
    private nextResultSubject: Subject<any> = new Subject<any>();
    private lastestResultSubject: BehaviorSubject<any> = new BehaviorSubject<any>(null);
    public nextResultObservable: Observable<any>;
    public latestResultObservable: Observable<any>;
    private questionSetPaths: Array<string> = new Array<string>();

    /**
     * A list of observables for each of the fields referred to in the expression, organised
     * by the fieldPath as a key. The observable is provided along with the index within the
     * latestFieldValues array that the last value will be stored.
     */
    private fieldValueObservableAndIndexMap: Map<string, FieldValueObservableAndIndex>
        = new Map<string, FieldValueObservableAndIndex>();

    private questionSetValidObservableMap: Map<string, Observable<boolean>> = new Map<string, Observable<boolean>>();

    private latestFieldValues: Array<any> = new Array<any>();

    /**
     * An array of true/false, for holding the output of each call to fieldIsValid(...), in the
     * order that the calls appear in the expression.
     */
    private latestFieldValidities: Array<boolean> = new Array<boolean>();

    private latestArrayValues: Array<Array<any>> = new Array<Array<any>>();
    private latestRepeatingCounts: Array<number> = new Array<number>();
    public latestResult: any;
    private destroyed: Subject<void> = new Subject<void>();

    /**
     * A map of expression method name to an observable of whether it should be re-evaluated since one of it's
     * dependencies have changed (e.g. the time, or an operation completed that changed state).
     */
    private expressionMethodObservableMap: Map<string, Observable<void>> = new Map<string, Observable<void>>();

    private parsingCompleted: boolean = false;

    /**
     * The field unique identifier which this expression originated from.
     * If this is passed in and there's an error, it will be reported along 
     * with the error to assist with debugging.
     */
    public debugIdentifier: string;

    private observableValueSubscriptions: Array<SubscriptionLike> = new Array<SubscriptionLike>();

    private publishEvenIfNotChanged: boolean = false;
    /**
     * 
     * @param source The expression string
     * @param dependencies an instance of ExpressionDependencies
     * @param debugIdentifier a string which is output when an experssion fails to 
     * help you identify it. Pass in anything you want.
     * @param fixedValues a set of Fixed argements (ie they never change) that can be 
     * refereced by name within the expression just like a variable
     * @param observableValues a set of values which are expected to change over time, e.g. the field's value.
     * @param scope a fieldPath determining scope for field value resolution. 
     * This is only passed in for repeating questions, and when it is, it's the fieldPath for the repeating 
     * instance, e.g. "claims[0]".
     */
    public constructor(
        source: string,
        protected dependencies: ExpressionDependencies,
        debugIdentifier: string,
        protected fixedValues?: FixedArguments,
        protected observableValues?: ObservableArguments,
        public scope?: string,
        publishEvenIfNotChanged: boolean = false,
    ) {
        this.debugIdentifier = debugIdentifier;
        if (!source) {
            let errorMessage: string = 'When creating an expression '
                + (debugIdentifier ? 'for field "' + debugIdentifier + '", ' : '')
                + "a null or undefined source was passed. "
                + "Please ensure you pass a value when constructing an expression.";
            throw new Error(errorMessage);
        }
        this.originalExpressionSource = source;
        this._source = source;
        this.nextResultObservable = this.nextResultSubject.asObservable();
        this.latestResultObservable = this.lastestResultSubject.asObservable();
        this.nextResultSubject.subscribe(this.lastestResultSubject);
        this.parsedExpression = this.parse();
        this.parsingCompleted = true;
        this.publishEvenIfNotChanged = publishEvenIfNotChanged;
        this.dependencies.eventService.expressionCreatedSubject.next(this);
    }

    public dispose(): void {
        this.dependencies.eventService.expressionDisposedSubject.next(this);
        this.destroyed.next();
        this.destroyed.complete();
        this.nextResultSubject.complete();
    }

    /**
     * An expression is constant if it doesn't have any dependencies.
     * It's constant if it can be evaluated without requiring any input
     * data from anything else that might change.
     * It's constant if it doesn't depend on another value that could change.
     * 
     * so, for example, the expression "true" is a constant, and 
     * the expression "'abc' + 'def'" is constant
     */
    public isConstant(): boolean {
        return this.fieldValueObservableAndIndexMap.size == 0
            && this.latestFieldValidities.length == 0
            && this.expressionMethodObservableMap.size == 0
            && this.observableValueSubscriptions.length == 0
            && this.questionSetValidObservableMap.size == 0
            && this.latestArrayValues.length == 0
            && this.latestRepeatingCounts.length == 0;
    }

    private parse(): string {
        if (typeof this._source == 'boolean'
            || typeof this._source == 'number'
            || this._source == '') {
            return this._source;
        }
        this._source = this.applyBackwardsCompatibilityReplacements(this._source);
        let expressionSource: string = this.parseForGetFieldValuesForFieldPathPattern(this._source);
        expressionSource = this.parseForRelativeFieldPaths(expressionSource);
        expressionSource = this.parseForRelativeFieldValues(expressionSource);
        expressionSource = this.parseForFieldIsValid(expressionSource);
        expressionSource = this.parseForRepeatingFieldCounts(expressionSource);
        expressionSource = this.parseForGenerateSummaryTableOfFields(expressionSource);
        expressionSource = this.parseForGetFieldValues(expressionSource);
        expressionSource = this.parseForGetFieldValue(expressionSource);
        expressionSource = this.parseForGetFieldSearchTerm(expressionSource);
        expressionSource = this.parseForgenerateSummaryTableOfFieldsWithTag(expressionSource);
        expressionSource = this.parseForGetFieldValuesWithTag(expressionSource);
        expressionSource = this.parseForGenerateSummaryTablesForRepeatingField(expressionSource);
        let returnValue: string = '';
        this.strings = expressionSource.match(/'([^'\\]|\\.)*'|"([^"\\]|\\.)*"/g);
        let expressionWithTokens: string =
            expressionSource.replace(/'([^'\\]|\\.)*'|"([^"\\]|\\.)*"/g, '%STRING_TOKEN%');
        expressionWithTokens = this.parseRegexPattern(expressionWithTokens);
        let betweenStringsArray: Array<string> = expressionWithTokens.split('%STRING_TOKEN%');
        for (let i: number = 0; i < betweenStringsArray.length; i++) {
            betweenStringsArray[i] = this.parseMethods(betweenStringsArray[i]);
        }
        for (let i: number = 0; i < betweenStringsArray.length; i++) {
            let modifiedString: string = betweenStringsArray[i];
            while (modifiedString.includes('%REGEX_TOKEN%') && this.regexPatterns.length > 0) {
                modifiedString = modifiedString.replace('%REGEX_TOKEN%', this.regexPatterns[0]);
                this.regexPatterns.shift();
            }
            returnValue += modifiedString;
            if (this.strings != null && this.strings[i] != null) {
                // here we replace newlines with escaped newline sequences so that the string literals can be
                // passed to the Function object as part of an overall string to be evaluated.
                returnValue += this.strings[i].replace(/\r/g, '\\r').replace(/\n/g, '\\n');
            }
        }
        this.observeValidityChangeOfQuestionSets();
        return returnValue;
    }

    private applyBackwardsCompatibilityReplacements(expressionSource: any): string {
        let updatedSource: any = expressionSource.replace(
            /sumRepeating\( *([a-zA-Z0-9_]*) *, *['"`]([a-zA-Z0-9_]*)['"`] *\)/g,
            "sum(getFieldValuesForFieldPathPattern('$1[*].$2'))");
        updatedSource = updatedSource.replace(
            /countRepeating\( *?([a-zA-Z0-9_]*?) *?\)/g,
            "countRepeating('$1')");
        updatedSource = updatedSource.replace(
            // eslint-disable-next-line no-useless-escape
            /getRepeatingValue\( *[a-zA-Z0-9_]* *, *['"]([a-zA-Z0-9_\/\[\]]*)['"] *\)/g,
            "getRelativeFieldValue(fieldPath, '1/$1')");
        return updatedSource;
    }

    /**
     * Some parameters to certain expression methods are question set names. Here 
     * we are going to subscribe to the validity changes of those question sets where 
     * the expression references them, so that we can re-evaluate when the validity changes.
     */
    private observeValidityChangeOfQuestionSets(): void {
        let pattern: RegExp = ExpressionPatterns.questionSetOrSetsValidity;
        let stringParts: Array<string> = this._source.split(pattern);
        for (let i: number = 0; i < stringParts.length; i++) {
            if (stringParts[i] == '') {
                continue;
            }
            pattern = ExpressionPatterns.questionSetsValidity;
            if (stringParts[i].match(pattern)) {
                // remove spaces between brackets
                stringParts[i] = stringParts[i].replace(pattern, '$1$2$3');
                let squareBracketPosition: number = stringParts[i].indexOf('[');
                let questionSetPathsString: string =
                    stringParts[i].substring(squareBracketPosition + 1, stringParts[i].length - 2);
                let questionSetPaths: Array<string> = questionSetPathsString.split(/\s*,\s*/);
                for (let questionSetPath of questionSetPaths) {
                    questionSetPath = questionSetPath.replace(/'/g, '').trim();
                    this.observeValidityChangeOfQuestionSet(questionSetPath);
                }
            } else {
                pattern = ExpressionPatterns.questionSetValidity;
                if (stringParts[i].match(pattern)) {
                    let bracketPosition: number = stringParts[i].indexOf('(');
                    let questionSetPath: string = stringParts[i].substring(
                        bracketPosition + 1,
                        stringParts[i].length - 1);
                    questionSetPath = questionSetPath.replace(/'/g, '').trim();
                    this.observeValidityChangeOfQuestionSet(questionSetPath);
                }
            }
        }
    }

    private observeValidityChangeOfQuestionSet(questionSetPath: string): void {
        this.questionSetPaths.push(questionSetPath);
        let questionSetValidObservable: any = this.questionSetValidObservableMap.get(questionSetPath);
        if (!questionSetValidObservable) {
            questionSetValidObservable =
                this.dependencies.expressionInputSubjectService.getQuestionSetValidObservable(questionSetPath);
            questionSetValidObservable.pipe(takeUntil(this.destroyed))
                .subscribe((): void => {
                    this.evaluate(false);
                });
            this.questionSetValidObservableMap.set(questionSetPath, questionSetValidObservable);
        }
    }

    private parseForFieldIsValid(expression: string): string {
        let returnValue: string = '';
        let pattern: RegExp = ExpressionPatterns.fieldIsValid;
        let matches: RegExpMatchArray = expression.match(pattern);
        if (!matches) {
            return expression;
        }
        let expressionWithTokens: string = expression.replace(pattern, '%FIELD_VALID_TOKEN%');
        let betweenMatchesArray: Array<string> = expressionWithTokens.split('%FIELD_VALID_TOKEN%');
        if (betweenMatchesArray != null) {
            for (let i: number = 0; i < matches.length; i++) {
                let match: string = matches[i];
                let stringStart: number = match.indexOf("'") + 1;
                let stringEnd: number = match.lastIndexOf("'");
                let fieldPathPattern: string = match.substring(stringStart, stringEnd);
                let fieldValidObservable: any =
                    this.dependencies.expressionInputSubjectService.getFieldValidObservable(fieldPathPattern);
                let currentIndex: number = i;
                matches[i] = `reservedfieldValidValues[${i}]`;
                fieldValidObservable.pipe(takeUntil(this.destroyed))
                    .subscribe((valid: boolean): void => {
                        this.latestFieldValidities[currentIndex] = valid;
                        this.evaluate(false);
                    });
            }
            for (let i: number = 0; i < betweenMatchesArray.length; i++) {
                returnValue += betweenMatchesArray[i];
                if (matches != null && matches[i] != null) {
                    returnValue += matches[i];
                }
            }
        }
        return returnValue;
    }

    /**
     * finds "getFieldValuesForFieldPathPattern('activity[*].percentage')" within the expression and sets up
     * subscriptions, then replaces it with an reservedArrayValues getter
     * @param expression 
     */
    private parseForGetFieldValuesForFieldPathPattern(expression: string): string {
        let returnValue: string = '';
        // eslint-disable-next-line no-useless-escape
        let pattern: RegExp = /getFieldValuesForFieldPathPattern\( *'[a-zA-Z0-9_\[\]\*\.]*' *\)/g;
        let matchesArray: RegExpMatchArray = expression.match(pattern);
        if (!matchesArray) {
            return expression;
        }
        let expressionWithTokens: string = expression.replace(pattern, '%ARRAY_VALUES_TOKEN%');
        let betweenMatchesArray: Array<string> = expressionWithTokens.split('%ARRAY_VALUES_TOKEN%');
        if (betweenMatchesArray != null) {
            for (let i: number = 0; i < matchesArray.length; i++) {
                let match: string = matchesArray[i];
                let stringStart: number = match.indexOf("'") + 1;
                let stringEnd: number = match.lastIndexOf("'");
                let fieldPathPattern: string = match.substring(stringStart, stringEnd);
                let fieldValuesForFieldPathPatternObservable: any
                    = this.dependencies.matchingFieldsSubjectService.getFieldValuesForMatchingFieldsObservable(
                        fieldPathPattern);
                let currentIndex: number = i;
                matchesArray[i] = `reservedArrayValues[${i}]`;
                fieldValuesForFieldPathPatternObservable.pipe(takeUntil(this.destroyed))
                    .subscribe((fieldValuesArray: Array<any>): void => {
                        this.latestArrayValues[currentIndex] = fieldValuesArray;
                        this.evaluate(false);
                    });
            }
            for (let i: number = 0; i < betweenMatchesArray.length; i++) {
                returnValue += betweenMatchesArray[i];
                if (matchesArray != null && matchesArray[i] != null) {
                    returnValue += matchesArray[i];
                }
            }
        }
        return returnValue;
    }

    /**
     * finds "countRepeating('claims')" within the expression and sets up
     * subscriptions, then replaces it with a reservedRepeatingCounts getter
     * @param expression 
     */
    private parseForRepeatingFieldCounts(expression: string): string {
        let returnValue: string = '';
        let pattern: RegExp = ExpressionPatterns.countRepeating;
        let matchesArray: RegExpMatchArray = expression.match(pattern);
        if (!matchesArray) {
            return expression;
        }
        let expressionWithTokens: string = expression.replace(
            pattern,
            '%REPEATING_COUNT_TOKEN%');
        let betweenMatchesArray: Array<string> = expressionWithTokens.split('%REPEATING_COUNT_TOKEN%');
        if (betweenMatchesArray != null) {
            for (let i: number = 0; i < matchesArray.length; i++) {
                let match: string = matchesArray[i];
                let stringStart: number = match.indexOf("'") + 1;
                let stringEnd: number = match.lastIndexOf("'");
                let fieldPath: string = match.substring(stringStart, stringEnd);
                let repeatingCountObservable: Observable<number>
                    = this.dependencies.expressionInputSubjectService.getFieldRepeatingCountObservable(fieldPath);
                let currentIndex: number = i;
                matchesArray[i] = `reservedRepeatingCounts[${i}]`;
                repeatingCountObservable.pipe(takeUntil(this.destroyed))
                    .subscribe((count: number): void => {
                        this.latestRepeatingCounts[currentIndex] = count;
                        this.evaluate(false);
                    });
            }
            for (let i: number = 0; i < betweenMatchesArray.length; i++) {
                returnValue += betweenMatchesArray[i];
                if (matchesArray != null && matchesArray[i] != null) {
                    returnValue += matchesArray[i];
                }
            }
        }
        return returnValue;
    }

    /**
     * Finds "getRelativeFieldValue(fieldPath, '<traversal spec>')" and replaces it with the
     * resolved fieldpath representing a field's value. It will then be subscribed to in later processing.
     * @param expression 
     */
    private parseForRelativeFieldValues(expression: string): string {
        let matchesArray: RegExpMatchArray
            = expression.match(ExpressionPatterns.getRelativeFieldValue);
        if (!matchesArray) {
            return expression;
        }
        let returnValue: string = '';
        let expressionWithTokens: string = expression.replace(
            ExpressionPatterns.getRelativeFieldValue,
            '%RELATIVE_FIELD_TOKEN%');
        let betweenMatchesArray: Array<string> = expressionWithTokens.split('%RELATIVE_FIELD_TOKEN%');
        if (betweenMatchesArray != null) {
            for (let i: number = 0; i < matchesArray.length; i++) {
                let match: string = matchesArray[i];
                const fieldPathStart: number = match.indexOf('(') + 1;
                const fieldPathEnd: number = match.indexOf(',');
                const fieldPathArgument: string = match.substring(fieldPathStart, fieldPathEnd).trim();
                let fieldPath: string;
                if (!fieldPathArgument.startsWith("'")) {
                    // resolve the fieldpath from the fieldPathArgment                
                    if (!this.isArgument(fieldPathArgument) || !this.isFixedValue(fieldPathArgument)) {
                        throw Errors.Expression.GetRelativeFieldValueMismatchedArgument(
                            fieldPathArgument, this.source, this.debugIdentifier);
                    }
                    fieldPath = this.fixedValues[fieldPathArgument];
                } else {
                    fieldPath = fieldPathArgument.replace(/'/g, '');
                }
                const jsonPointerStart: number = match.indexOf("'", fieldPathEnd) + 1;
                const jsonPointerEnd: number = match.lastIndexOf("'");
                const jsonPointer: string = match.substring(jsonPointerStart, jsonPointerEnd);
                const relativeFieldPath: string = RelativeJsonPointerResolver.resolve(fieldPath, jsonPointer);
                matchesArray[i] = relativeFieldPath;
            }
            for (let i: number = 0; i < betweenMatchesArray.length; i++) {
                returnValue += betweenMatchesArray[i];
                if (matchesArray != null && matchesArray[i] != null) {
                    returnValue += matchesArray[i];
                }
            }
        }
        return returnValue;
    }

    /**
     * Finds "getRelativeFieldPath(fieldPath, '<traversal spec>')" and replaces it with the
     * resolved fieldpath, as a string.
     * @param expression 
     */
    private parseForRelativeFieldPaths(expression: string): string {
        let matchesArray: RegExpMatchArray
            = expression.match(ExpressionPatterns.getRelativeFieldPath);
        if (!matchesArray) {
            return expression;
        }
        let returnValue: string = '';
        let expressionWithTokens: string = expression.replace(
            ExpressionPatterns.getRelativeFieldPath,
            '%RELATIVE_FIELD_TOKEN%');
        let betweenMatchesArray: Array<string> = expressionWithTokens.split('%RELATIVE_FIELD_TOKEN%');
        if (betweenMatchesArray != null) {
            for (let i: number = 0; i < matchesArray.length; i++) {
                let match: string = matchesArray[i];
                const fieldPathStart: number = match.indexOf('(') + 1;
                const fieldPathEnd: number = match.indexOf(',');
                const fieldPathArgument: string = match.substring(fieldPathStart, fieldPathEnd).trim();
                let fieldPath: string;
                if (!fieldPathArgument.startsWith("'")) {
                    // resolve the fieldpath from the fieldPathArgment                
                    if (!this.isArgument(fieldPathArgument) || !this.isFixedValue(fieldPathArgument)) {
                        throw Errors.Expression.GetRelativeFieldValueMismatchedArgument(
                            fieldPathArgument, this.source, this.debugIdentifier);
                    }
                    fieldPath = this.fixedValues[fieldPathArgument];
                } else {
                    fieldPath = fieldPathArgument.replace(/'/g, '');
                }
                const jsonPointerStart: number = match.indexOf("'", fieldPathEnd) + 1;
                const jsonPointerEnd: number = match.lastIndexOf("'");
                const jsonPointer: string = match.substring(jsonPointerStart, jsonPointerEnd);
                const relativeFieldPath: string = RelativeJsonPointerResolver.resolve(fieldPath, jsonPointer);
                this.observeFieldValueChange(relativeFieldPath);
                matchesArray[i] = `'${relativeFieldPath}'`;
            }
            for (let i: number = 0; i < betweenMatchesArray.length; i++) {
                returnValue += betweenMatchesArray[i];
                if (matchesArray != null && matchesArray[i] != null) {
                    returnValue += matchesArray[i];
                }
            }
        }
        return returnValue;
    }

    private parseForGenerateSummaryTableOfFields(expression: string): string {
        const matchesIterator: IterableIterator<RegExpMatchArray>
            = matchAll(expression, ExpressionPatterns.generateSummaryTableOfFields);
        if (matchesIterator) {
            const matchesArray: Array<RegExpMatchArray> = Array.from(matchesIterator);
            for (const match of matchesArray) {
                this.observeValueChangesForUncleanFieldPaths(match[1].split(','));
            }
        }
        return expression;
    }

    private parseForGetFieldValues(expression: string): string {
        const matchesIterator: IterableIterator<RegExpMatchArray>
            = matchAll(expression, ExpressionPatterns.getFieldValues);
        if (matchesIterator) {
            const matchesArray: Array<RegExpMatchArray> = Array.from(matchesIterator);
            for (const match of matchesArray) {
                this.observeValueChangesForUncleanFieldPaths(match[1].split(','));
            }
        }
        return expression;
    }

    private parseForGetFieldValue(expression: string): string {
        const matchesIterator: IterableIterator<RegExpMatchArray>
            = matchAll(expression, ExpressionPatterns.getFieldValue);
        if (matchesIterator) {
            const matchesArray: Array<RegExpMatchArray> = Array.from(matchesIterator);
            for (const match of matchesArray) {
                this.observeValueChangesForUncleanFieldPaths([match[1]]);
            }
        }
        return expression;
    }

    private parseForGetFieldSearchTerm(expression: string): string {
        return this.replaceAndObserveInnerMatches(
            expression,
            ExpressionPatterns.getFieldSearchTerm,
            this.addScopeToFieldPath.bind(this),
            (match: string) => this.observeSearchTermChangesForUncleanFieldPaths([match]),
        );
    }

    /**
     * Takes the expression, and calls the replacer function replaces the captured groups found with the regExp,
     * then calls the observerFunction with each of the replaced values so the expression can setup observers
     * for them.
     * @param expression 
     * @param regExp 
     * @param replacerFunction 
     * @param observerFunction 
     */
    private replaceAndObserveInnerMatches(
        expression: string,
        regExp: RegExp,
        replacerFunction: (match: string) => string,
        observerFunction: (match: string) => void,
    ): string {
        let updatedExpression: string = '';
        let matchesArray: RegExpMatchArray = expression.match(regExp);
        if (matchesArray == null) {
            return expression;
        }
        let expressionWithTokens: string = expression.replace(regExp, '%MATCH_TOKEN%');
        let betweenMatchesArray: Array<string> = expressionWithTokens.split('%MATCH_TOKEN%');
        for (let i: number = 0; i < matchesArray.length; i++) {
            let match: string = matchesArray[i];
            const innerMatchesIterator: IterableIterator<RegExpMatchArray>
                = matchAll(match, ExpressionPatterns.getFieldSearchTerm);
            if (innerMatchesIterator) {
                const innerMatchesArray: Array<RegExpMatchArray> = Array.from(innerMatchesIterator);
                for (let innerMatch of innerMatchesArray) {
                    match = match.replace(innerMatch[1], '%GROUP_TOKEN%');
                    let value: string = replacerFunction(innerMatch[1]);
                    observerFunction(value);
                    match = match.replace('%GROUP_TOKEN%', value);
                }
            }
            matchesArray[i] = match;
        }
        for (let i: number = 0; i < betweenMatchesArray.length; i++) {
            updatedExpression += betweenMatchesArray[i];
            if (matchesArray != null && matchesArray[i] != null) {
                updatedExpression += matchesArray[i];
            }
        }
        return updatedExpression;
    }

    private parseForGenerateSummaryTablesForRepeatingField(expression: string): string {
        const matchesIterator: IterableIterator<RegExpMatchArray>
            = matchAll(expression, ExpressionPatterns.generateSummaryTablesForRepeatingField);
        if (matchesIterator) {
            const matchesArray: Array<RegExpMatchArray> = Array.from(matchesIterator);
            for (const match of matchesArray) {
                this.observeValueChangesForUncleanFieldPaths([match[1]]);
            }
        }
        return expression;
    }

    private parseRegexPattern(expression: string): string {
        // we need to determine if pattern that seems like regex is actually a regex pattern or 
        // is actually just consecutive division operators, e.g. 1 / 2 / 3
        // so we need to check if it is preceded by stringContains or stringReplace methods
        // as well as the pattern itself, if the pattern matches,
        // then we extract the actual regex pattern and replace it with a token
        const matchesIterator: IterableIterator<RegExpMatchArray>
            = matchAll(expression, ExpressionPatterns.getRegexPattern);
        if (matchesIterator) {
            const matchesArray: Array<RegExpMatchArray> = Array.from(matchesIterator);
            for (const match of matchesArray) {
                // the pattern is in the fifth capture group
                let pattern: string = match[5];
                this.regexPatterns.push(pattern);
                expression = expression.replace(pattern, '%REGEX_TOKEN%');
            }
        }

        return expression;
    }

    private cleanFieldPathForObserving(fieldPath: string): string {
        fieldPath = fieldPath.replace(/'/g, '').trim();
        if (fieldPath.endsWith(']')) {
            fieldPath = fieldPath.substring(0, fieldPath.lastIndexOf('['));
        }
        return fieldPath;
    }

    private observeValueChangesForUncleanFieldPaths(uncleanFieldPaths: Array<string>): void {
        for (let unCleanFieldPath of uncleanFieldPaths) {
            let cleanFieldPath: string = this.cleanFieldPathForObserving(unCleanFieldPath);
            this.observeFieldValueChange(cleanFieldPath);
        }
    }

    private observeSearchTermChangesForUncleanFieldPaths(uncleanFieldPaths: Array<string>): void {
        for (let unCleanFieldPath of uncleanFieldPaths) {
            let cleanFieldPath: string = this.cleanFieldPathForObserving(unCleanFieldPath);
            this.observeFieldSearchTermChange(cleanFieldPath);
        }
    }

    private observeFieldValueChange(fieldPath: string): void {
        let fieldValueObservable: Observable<any> =
            this.dependencies.expressionInputSubjectService.getFieldValueObservable(fieldPath);
        fieldValueObservable.subscribe((): void => this.evaluate(false));
    }

    private observeFieldSearchTermChange(fieldPath: string): void {
        let fieldSearchTermObservable: Observable<any> =
            this.dependencies.expressionInputSubjectService
                .getFieldSearchTermObservable(fieldPath);
        fieldSearchTermObservable.subscribe((): void => this.evaluate(false));
    }

    private parseForgenerateSummaryTableOfFieldsWithTag(expression: string): string {
        const matchesIterator: IterableIterator<RegExpMatchArray>
            = matchAll(expression, ExpressionPatterns.generateSummaryTableOfFieldsWithTag);
        if (matchesIterator) {
            const matchesArray: Array<RegExpMatchArray> = Array.from(matchesIterator);
            for (const match of matchesArray) {
                this.observeValueChangesForFieldPathsWithTag(match[1]);
            }
        }
        return expression;
    }

    private parseForGetFieldValuesWithTag(expression: string): string {
        const matchesIterator: IterableIterator<RegExpMatchArray>
            = matchAll(expression, ExpressionPatterns.getFieldValuesWithTag);
        if (matchesIterator) {
            const matchesArray: Array<RegExpMatchArray> = Array.from(matchesIterator);
            for (const match of matchesArray) {
                this.observeValueChangesForFieldPathsWithTag(match[1]);
            }
        }
        return expression;
    }

    private observeValueChangesForFieldPathsWithTag(tag: string): void {
        this.dependencies.taggedFieldsSubjectService.getFieldValuesForFieldsWithTagObservable(tag)
            .pipe(takeUntil(this.destroyed)).subscribe((fieldValues: Array<any>): void => {
                this.evaluate(false);
            });
    }

    private parseMethods(expression: string): string {
        let returnValue: string = '';
        const methodCallStartExpression: RegExp = ExpressionPatterns.methodCallStart;
        let methodsArray: RegExpMatchArray = expression.match(methodCallStartExpression);
        let expressionWithTokens: string = expression.replace(methodCallStartExpression, '%METHOD_TOKEN%');
        let betweenMethodsArray: Array<string> = expressionWithTokens.split('%METHOD_TOKEN%');
        if (methodsArray != null) {
            for (let i: number = 0; i < methodsArray.length; i++) {
                let methodName: string = methodsArray[i].substring(0, methodsArray[i].length - 1);
                if (this.dependencies.expressionMethodService[methodName] != null) {
                    methodsArray[i] = 'expressionMethods.' + methodsArray[i];
                } else {
                    throw Errors.Expression.NoSuchExpressionMethod(
                        methodName,
                        this.source,
                        this.debugIdentifier);
                }
                let expressionMethodObservable: any =
                    this.dependencies.expressionInputSubjectService.getExpressionMethodSubjectObservable(methodName);
                // if it's an expression method which does not have any changing dependencies, 
                // then it will not have a subject, and so will return null
                if (expressionMethodObservable) {
                    this.expressionMethodObservableMap.set(methodName, expressionMethodObservable);
                    expressionMethodObservable
                        .pipe(
                            takeUntil(this.destroyed),
                            shareReplay({ bufferSize: 1, refCount: true }),
                        )
                        .subscribe((): void => this.evaluate(false));
                }
            }
        }
        for (let i: number = 0; i < betweenMethodsArray.length; i++) {
            if (betweenMethodsArray[i] != "" && !betweenMethodsArray[i].includes("%REGEX_TOKEN%") ) {
                betweenMethodsArray[i] = this.parseFieldsAndArguments(betweenMethodsArray[i]);
            }
        }
        for (let i: number = 0; i < betweenMethodsArray.length; i++) {
            returnValue += betweenMethodsArray[i];
            if (methodsArray != null && methodsArray[i] != null) {
                returnValue += methodsArray[i];
            }
        }
        return returnValue;
    }

    protected parseFieldsAndArguments(expression: string): string {
        let updatedExpression: string = '';
        const fieldsAndArgumentsExpression: RegExp = ExpressionPatterns.fieldsAndArguments;
        let entitiesArray: RegExpMatchArray = expression.match(fieldsAndArgumentsExpression);
        let expressionWithTokens: string = expression.replace(fieldsAndArgumentsExpression, '%REPLACE_TOKEN%');
        let betweenEntitiesArray: Array<string> = expressionWithTokens.split('%REPLACE_TOKEN%');
        if (entitiesArray != null) {
            for (let i: number = 0; i < entitiesArray.length; i++) {
                let entity: string = entitiesArray[i];
                if (isNaN(parseInt(entity, 10)) && isNaN(parseFloat(entity)) &&
                    entity != 'false' && entity != 'true' && entity != 'null'
                ) {
                    const argumentParts: Array<string> = this.splitArgumentAtDotOrSquareBracket(entity);
                    const itemName: string = argumentParts[0];
                    if (itemName == 'reservedArrayValues'
                        || itemName == 'reservedRepeatingCounts'
                        || itemName == 'reservedfieldValidValues'
                    ) {
                        // do nothing - these are special values and we don't want them to be confused with field values
                    } else {
                        if (this.isArgument(itemName)) {
                            if (this.isFixedValue(itemName)) {
                                this.arguments.set(itemName, this.fixedValues[itemName]);
                            } else if (this.isObservableValue(itemName)) {
                                // set the initial value for this argument to be an empty string
                                this.arguments.set(itemName, '');
                                this.observableValueSubscriptions.push(this.observableValues[itemName]
                                    .pipe(
                                        distinctUntilChanged(),
                                        takeUntil(this.destroyed),
                                    )
                                    .subscribe((value: any): void => {
                                        this.arguments.set(itemName, value);
                                        this.evaluate(this.publishEvenIfNotChanged);
                                    }));
                            }
                            entitiesArray[i] = 'arguments.get(\'' + itemName + '\')';
                            if (argumentParts.length > 1) {
                                entitiesArray[i] += argumentParts[1];
                            }
                        } else {
                            let fieldValueIndex: number = this.subscribeToFieldValue(entity);
                            entitiesArray[i] = 'fieldValues[' + fieldValueIndex + ']';
                        }
                    }
                }
            }
        }
        for (let i: number = 0; i < betweenEntitiesArray.length; i++) {
            updatedExpression += betweenEntitiesArray[i];
            if (entitiesArray != null && entitiesArray[i] != null) {
                updatedExpression += entitiesArray[i];
            }
        }
        return updatedExpression;
    }

    /**
     * Splits the argument into two parts, at the dot or open square bracket
     * e.g. "aaaaa.bbbbb" => ["aaaaa", ".bbbbb"]
     *      "ccccc[ddddd]" => ['ccccc", "[ddddd]"]
     * @param argument 
     */
    private splitArgumentAtDotOrSquareBracket(argument: string): Array<string> {
        let result: Array<string> = new Array<string>();
        let cutOffPos: number = -1;
        const firstOpenSquareBracketPos: number = argument.indexOf('[');
        const firstDotPos: number = argument.indexOf('.');
        if (firstOpenSquareBracketPos == -1) {
            cutOffPos = firstDotPos;
        } else if (firstDotPos == -1) {
            cutOffPos = firstOpenSquareBracketPos;
        } else {
            cutOffPos = Math.min(firstDotPos, firstOpenSquareBracketPos);
        }
        if (cutOffPos == -1) {
            result.push(argument);
        } else {
            result.push(argument.substring(0, cutOffPos));
            result.push(argument.substring(cutOffPos));
        }
        return result;
    }

    private isArgument(itemName: string): boolean {
        return this.isFixedValue(itemName) || this.isObservableValue(itemName);
    }

    private isFixedValue(itemName: string): boolean {
        return this.fixedValues && this.fixedValues[itemName] != undefined;
    }

    private isObservableValue(itemName: string): boolean {
        return this.observableValues && this.observableValues[itemName] != undefined;
    }

    /**
     * 
     * @param fieldPath 
     * @param position the position or index at which the field appears in the expression
     */
    private subscribeToFieldValue(fieldPath: string): number {
        fieldPath = this.addScopeToFieldPath(fieldPath);
        let fieldValueObservableAndIndex: FieldValueObservableAndIndex =
            this.fieldValueObservableAndIndexMap.get(fieldPath);
        if (!fieldValueObservableAndIndex) {
            let fieldValueIndex: number = this.latestFieldValues.length;
            // We put an empty string here since old expressions expect an empty string instead of null            
            this.latestFieldValues.push('');

            let fieldValueObservable: any =
                this.dependencies.expressionInputSubjectService.getFieldValueObservable(fieldPath);
            fieldValueObservableAndIndex = {
                fieldValueObservable: fieldValueObservable,
                latestValuesIndex: fieldValueIndex,
            };
            this.fieldValueObservableAndIndexMap.set(fieldPath, fieldValueObservableAndIndex);
            fieldValueObservable.pipe(takeUntil(this.destroyed))
                .subscribe((value: any): void => this.onFieldValueUpdated(fieldPath, value));
        }
        return fieldValueObservableAndIndex.latestValuesIndex;
    }

    /**
     * Adds the scope (if any) to the fieldPath, so that fields referencing siblings
     * inside repeating fieldsets can have "this.fieldKey" syntax.
     */
    private addScopeToFieldPath(fieldPath: string): string {
        if (fieldPath.startsWith('this.')) {
            fieldPath = fieldPath.substring(5);
            if (!StringHelper.isNullOrEmpty(this.scope)) {
                fieldPath = `${this.scope}.${fieldPath}`;
            }
        }
        return fieldPath;
    }

    private onFieldValueUpdated(fieldPath: string, value: any): void {
        // convert null to empty string since old expressions expect an empty string instead of null
        value = value == null ? '' : value;
        let fieldValueObservableAndIndex: FieldValueObservableAndIndex =
            this.fieldValueObservableAndIndexMap.get(fieldPath);
        this.latestFieldValues[fieldValueObservableAndIndex.latestValuesIndex] = value;
        this.evaluate(this.publishEvenIfNotChanged);
    }

    /**
     * Once the form has fully loaded, we want to trigger the initial evalution of expressions
     * so that fields can refect their initial state.
     */
    public triggerEvaluationWhenFormLoaded(): void {
        this.dependencies.eventService.webFormLoadedSubject.pipe(
            filter((loaded: boolean): boolean => loaded == true),
            takeUntil(this.destroyed),
        )
            .subscribe((): void => this.evaluate(false));
    }

    public triggerEvaluation(): void {
        this.evaluate();
    }

    /**
     * It's not recommended to call this function directly, because expression 
     * results will change over time as the form is being filled out. 
     * Instead subscribe to the latestResultObservable.
     * 
     * @param publishEvenIfNotChanged if set to false, if the resulting value has not changed, 
     * it will not be published via the latestResultObservable. By default it is published.
     */
    public evaluate(publishEvenIfNotChanged: boolean = true): any {
        if (!this.parsingCompleted) {
            // wait until parsing has completed before evaluating
            return;
        }
        let result: any;

        try {
            // eslint-disable-next-line @typescript-eslint/ban-types
            let expressionFunction: Function = new Function(
                // the function parameter names:
                `fieldValues, reservedfieldValidValues, reservedArrayValues, `
                + `reservedRepeatingCounts, expressionMethods, arguments`,
                // the function body:
                'return ' + this.parsedExpression + ';',
            );

            result = expressionFunction.apply(
                this,
                [
                    this.latestFieldValues,
                    this.latestFieldValidities,
                    this.latestArrayValues,
                    this.latestRepeatingCounts,
                    this.dependencies.expressionMethodService,
                    this.arguments,
                ]);
        } catch (error) {
            console.error(error);
            let additionalDetails: Array<string> = new Array<string>();
            let errorData: object = {};
            additionalDetails.push('Expression Source: ' + this._source);
            if (this._source != this.originalExpressionSource) {
                additionalDetails.push('Original Expression Source: ' + this.originalExpressionSource);
                errorData['originalExpressionSource'] = this.originalExpressionSource;
            }
            additionalDetails.push('Parsed Expression: ' + this.parsedExpression);
            if (this.latestFieldValues.length > 0) {
                additionalDetails.push('Field Values: "' + this.latestFieldValues.join('","') + '"');
                errorData['parsedExpression'] = this.parsedExpression;
            }
            if (this.arguments.size > 0) {
                errorData['arguments'] = {};
                let argumentsString: string = 'Arguments: ';
                this.arguments.forEach((value: any, key: string): void => {
                    argumentsString += ` ${key}="${value}" `;
                    errorData['arguments'][key] = value;
                });
                additionalDetails.push(argumentsString);
            }
            if (this.debugIdentifier) {
                additionalDetails.push('Debug Identifier: ' + this.debugIdentifier);
                errorData['debugIdentifier'] = this.debugIdentifier;
            }
            throw Errors.Expression.EvaluationFailed(
                error.message,
                additionalDetails,
                errorData);
        }

        if (publishEvenIfNotChanged || result !== this.latestResult) {
            this.latestResult = result;
            this.nextResultSubject.next(result);
        }

        return result;
    }

    /** 
     * For single use expressions which should not continue to subscribe to depencies after evaluation has completed
     */
    public evaluateAndDispose(): any {
        let result: any = this.evaluate();
        this.dispose();
        return result;
    }

    public get source(): string {
        return this._source;
    }
}
