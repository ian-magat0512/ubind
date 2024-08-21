import { HttpErrorResponse } from "@angular/common/http";
import { Component } from "@angular/core";
import { FormBuilder, FormGroup } from "@angular/forms";
import { Expression } from "@app/expressions/expression";
import { ExpressionDependencies } from "@app/expressions/expression-dependencies";
import * as _ from 'lodash-es';

/**
 * Represents an expression which is being watched for debug purposes.
 */
interface WatchedExpression {
    source: string;
    expression?: Expression;
    error?: any;
    errorIsObject?: boolean;
    resultAsString?: string;
    resultIsObject?: boolean;
}

/**
 * This component allows you to type or paste in an expression and evaluate it.
 */
@Component({
    selector: 'expression-watch',
    templateUrl: './expression-watch.component.html',
    styleUrls: ['./expression-watch.component.scss'],
})
export class ExpressionWatchComponent {

    public form: FormGroup = new FormGroup({});
    public watchedExpressions: Array<WatchedExpression> = new Array<WatchedExpression>();

    public constructor(
        formBuilder: FormBuilder,
        private expressionDependencies: ExpressionDependencies,
    ) {
        this.form = formBuilder.group({
            source: [''],
        });
    }

    public addButtonClicked(): void {
        if (this.form.value['source'] == '') {
            return;
        }
        let watchedExpression: WatchedExpression = {
            source: this.form.value['source'],
        };
        this.watchedExpressions.push(watchedExpression);
        try {
            watchedExpression.expression = new Expression(
                watchedExpression.source,
                this.expressionDependencies,
                'expression watch');
            this.form.reset();
            watchedExpression.expression.nextResultObservable.subscribe((result: any) => {
                if (_.isObject(result)) {
                    watchedExpression.resultIsObject = true;
                    watchedExpression.resultAsString = JSON.stringify(result);
                } else {
                    watchedExpression.resultIsObject = false;
                    watchedExpression.resultAsString =
                        result == null ? '(null)' : result;
                }
            },
            (err: HttpErrorResponse) => {
                this.processError(err, watchedExpression);
            });
            watchedExpression.expression.triggerEvaluation();
        } catch (err) {
            this.processError(err, watchedExpression);
        }
    }

    private processError(err: any, watchedExpression: WatchedExpression): void {
        if (err instanceof Error) {
            watchedExpression.error = err.message;
            watchedExpression.errorIsObject = false;
        } else {
            watchedExpression.error = err;
            watchedExpression.errorIsObject = _.isObject(err);
        }
    }

    public removeButtonClicked(index: number): void {
        if (this.watchedExpressions[index].expression) {
            this.watchedExpressions[index].expression.dispose();
        }

        // delete one watched expression at the given index
        this.watchedExpressions.splice(index, 1);
    }
}
