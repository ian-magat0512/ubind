import { Output, EventEmitter, Directive } from '@angular/core';
import { AbstractControl, AbstractControlOptions, AsyncValidatorFn, FormGroup, ValidatorFn } from '@angular/forms';
import { QuestionsWidget } from '../widgets/questions/questions.widget';

/**
 * The angular reactive form control which provides validation on fields and other
 * form related services.
 */
@Directive()

export class Form extends FormGroup {

    @Output() public fieldValueChanges: EventEmitter<any> = new EventEmitter<any>();
    @Output() public fieldKeyDown: EventEmitter<any> = new EventEmitter<any>();
    @Output() public fieldFocus: EventEmitter<any> = new EventEmitter<any>();
    @Output() public fieldBlur: EventEmitter<any> = new EventEmitter<any>();

    private questionSet: QuestionsWidget;

    public constructor(
        controls: {
            [key: string]: AbstractControl;
        },
        validatorOrOpts?: ValidatorFn | Array<ValidatorFn> | AbstractControlOptions | null,
        asyncValidator?: AsyncValidatorFn | Array<AsyncValidatorFn> | null,
    ) {
        super(controls, validatorOrOpts, asyncValidator);
    }

    public registerQuestionSet(newQuestionSet: QuestionsWidget): void {
        this.questionSet = newQuestionSet;
    }

    public deregisterQuestionSet(): void {
        delete this.questionSet;
    }

    public onFieldFocus(data: any): void {
        this.questionSet.onFieldFocus(data);
    }

    public onFieldBlur(data: any): void {
        this.questionSet.onFieldBlur(data);
    }
}
