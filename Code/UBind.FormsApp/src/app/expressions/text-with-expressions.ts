import { Expression, FixedArguments, ObservableArguments } from '@app/expressions/expression';
import { SecurityHelper } from '@app/helpers/security.helper';
import { Disposable } from '@app/models/disposable';
import * as DOMPurify from 'dompurify';
import { BehaviorSubject, Observable, Subject } from 'rxjs';
import { distinctUntilChanged, filter, takeUntil } from 'rxjs/operators';
import { ExpressionDependencies } from './expression-dependencies';

/**
 * Represents a string of text which may contain expressions within it, so can be 
 * subscribed to for updates.
 */
export class TextWithExpressions implements Disposable {

    protected resultParts: Array<string> = new Array<string>();
    private latestResult: string;
    private nextResultSubject: Subject<any> = new Subject<any>();
    private lastestResultSubject: BehaviorSubject<any> = new BehaviorSubject<any>('');
    public nextResultObservable: Observable<any>;
    public latestResultObservable: Observable<any>;
    private expressions: Array<Expression>;
    private destroyed: Subject<void> = new Subject<void>();
    private _source: string;

    /**
     * a stream of publish events that we can debounce so that if there are multiple results coming in
     * too quickly
     */
    private publishSubject: Subject<void> = new Subject<void>();

    public constructor(
        source: string,
        protected dependencies: ExpressionDependencies,
        protected debugIdentifier: string,
        protected fixedValues?: FixedArguments,
        protected observableValues?: ObservableArguments,
        protected scope?: string,
        protected doNotSanitize?: boolean,
    ) {
        if (!source) {
            let errorMessage: string = 'When creating a text expression '
                + (debugIdentifier ? 'for field "' + debugIdentifier + '", ' : '')
                + "a null or undefined source was passed. "
                + "Please ensure you pass a string value when constructing a text expression.";
            throw new Error(errorMessage);
        }
        if (typeof source != 'string') {
            let errorMessage: string = 'When creating a text expression '
                + (debugIdentifier ? 'for field "' + debugIdentifier + '", ' : '')
                + `a value of type "${typeof source}" was passed. `
                + "Please ensure you pass a string value when constructing a text expression.";
            throw new Error(errorMessage);
        }
        this._source = source;
        this.onPublish();
        this.nextResultObservable = this.nextResultSubject.asObservable();
        this.latestResultObservable = this.lastestResultSubject.asObservable();
        this.nextResultSubject.subscribe(this.lastestResultSubject);
        this.parse();
    }

    public dispose(): void {
        if (this.expressions) {
            this.expressions.forEach((e: Expression) => e.dispose());
        }
        this.destroyed.next();
        this.destroyed.complete();
        this.nextResultSubject.complete();
    }

    private parse(): void {
        if (!this._source) {
            return;
        }

        if (this._source.indexOf('%{') == -1) {
            this.latestResult = this._source;
        } else {
            this.expressions = new Array<Expression>();
            let regex: RegExp = /(\%\{.*?\}\%)/gs;
            let stringParts: Array<string> = this._source.split(regex);
            let expressionsCreated: boolean = false;
            for (let i: number = 0; i < stringParts.length; i++) {
                let partIsExpression: boolean = stringParts[i].startsWith('%{');
                this.resultParts.push(partIsExpression ? '' : stringParts[i]);
                if (partIsExpression) {
                    expressionsCreated = true;
                    let expressionSource: string = stringParts[i].substring(2, stringParts[i].length - 2);
                    let expression: Expression = new Expression(
                        expressionSource,
                        this.dependencies,
                        this.debugIdentifier,
                        this.fixedValues,
                        this.observableValues,
                        this.scope);
                    expression.nextResultObservable
                        .pipe(distinctUntilChanged())
                        .subscribe((result: any) => {
                            this.resultParts[i] = result;
                            this.publishSubject.next();
                        });
                    this.expressions.push(expression);
                }
            }

            if (!expressionsCreated) {
                this.latestResult = this._source;
            }
        }
    }

    private onPublish(): void {
        this.publishSubject.pipe(takeUntil(this.destroyed)).subscribe(() => this.publish());
    }

    private publish(): void {
        if (this.expressions) {
            this.latestResult = this.resultParts.join('');
        }
        this.nextResultSubject.next(this.latestResult);
    }

    /**
     * Once the form has fully loaded, we want to trigger the initial evalution of expressions
     * so that fields can refect their initial state.
     */
    public triggerEvaluationWhenFormLoaded(): void {
        if (this.expressions) {
            this.dependencies.eventService.webFormLoadedSubject.pipe(
                filter((loaded: boolean) => loaded == true),
                takeUntil(this.destroyed),
            )
                .subscribe(() => {
                    for (let expression of this.expressions) {
                        expression.evaluate(false);
                    }
                });
        } else {
            this.nextResultSubject.next(this.latestResult);
        }
    }

    public triggerEvaluation(): void {
        this.evaluate();
    }

    public evaluate(publishEvenIfNotChanged: boolean = true): string {
        if (this.expressions) {
            for (let expression of this.expressions) {
                // if we are going to force a publish of this result, then let's 
                // not force a publish of each of the expressions within otherwise
                // there would be duplicate publishes of the same result.
                expression.evaluate(!publishEvenIfNotChanged);
            }
        }
        let result: string = this.latestResult;
        if (this.doNotSanitize !== true) {
            result = <string>DOMPurify.sanitize(this.latestResult, SecurityHelper.getDomPurifyConfig());
        }
        if (publishEvenIfNotChanged) {
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
