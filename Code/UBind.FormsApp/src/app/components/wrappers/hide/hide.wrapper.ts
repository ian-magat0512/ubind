import { Component, ViewChild, ViewContainerRef, OnInit, ElementRef, HostBinding } from '@angular/core';
import { FormService } from '../../../services/form.service';
import { Expression } from '@app/expressions/expression';
import { ExpressionDependencies } from '@app/expressions/expression-dependencies';
import { filter, takeUntil } from 'rxjs/operators';
import { Wrapper } from '../wrapper';
import { WorkflowStatusService } from '@app/services/workflow-status.service';
import { FieldConfiguration } from '@app/resource-models/configuration/fields/field.configuration';
import { EventService } from '@app/services/event.service';
import { ApplicationService } from '@app/services/application.service';
import { skip } from 'rxjs/operators';

/**
 * Export hide wrapper component class.
 * This class manage hide wrapper functions.
 */
@Component({
    selector: 'hide-wrapper',
    templateUrl: './hide.wrapper.html',
})

export class HideWrapper extends Wrapper implements OnInit {

    @ViewChild('fieldComponent', { read: ViewContainerRef, static: true }) public fieldComponent: ViewContainerRef;

    @HostBinding('class.no-layout')
    public hidden: boolean = true;

    protected hideExpression: Expression;

    /**
     * Flag which represents whether the hide condition has evaulated at least once.
     */
    private hideConditionEvaluated: boolean = false;
    private fieldConfiguration: FieldConfiguration;

    public constructor(
        protected forms: FormService,
        protected elementRef: ElementRef,
        protected expressionDependencies: ExpressionDependencies,
        private workflowStatusService: WorkflowStatusService,
        private eventService: EventService,
        private applicationService: ApplicationService,
    ) {
        super();
    }

    public ngOnInit(): void {
        this.fieldConfiguration = this.field.templateOptions.fieldConfiguration;
        super.ngOnInit();
        this.setupExpressions();
        this.onRecreateExpressions();
        this.listenForRevealChanges();
        this.listenForDebugChanges();
        this.onConfigurationUpdated();
    }

    private onConfigurationUpdated(): void {
        this.eventService.getFieldConfigUpdatedObservable(this.fieldKey).pipe(takeUntil(this.destroyed))
            .subscribe((configs: { old: FieldConfiguration; new: FieldConfiguration}) => {
                this.fieldConfiguration = configs.new;
                this.setupExpressions();
            });
    }

    protected setupExpressions(): void {
        if (this.fieldConfiguration.hideConditionExpression) {
            this.setupHideExpression();
        } else {
            if (this.hideExpression) {
                this.hideExpression.dispose();
                this.hideExpression = null;
            }
            this.hidden = false;
            this.applyHiddenState();
        }
    }

    protected destroyExpressions(): void {
        this.hideExpression?.dispose();
        this.hideExpression = null;
        super.destroyExpressions();
    }

    protected setupHideExpression(): void {
        if (this.hideExpression) {
            this.hideExpression.dispose();
            this.hideExpression = null;
        }
        this.hideExpression = new Expression(
            this.fieldConfiguration.hideConditionExpression,
            this.expressionDependencies,
            this.fieldKey + ' hide condition',
            this.fieldInstance.getFixedExpressionArguments(),
            this.fieldInstance.getObservableExpressionArguments(),
            this.fieldInstance.scope);
        this.hideExpression.nextResultObservable
            .pipe(
                takeUntil(this.destroyed),
                // don't bother hiding or showing if we're navigating this field
                // out, because some fields have hide/show rules based upon the
                // current workflow step, and so they will flicker during
                // navigation otherwise.
                takeUntil(this.workflowStatusService.navigatingOutStarted),
                filter(() => !this.hideConditionEvaluated || !this.workflowStatusService.isNavigatingOut),
            )
            .subscribe((hidden: any) => {
                this.hidden = hidden;
                this.applyHiddenState();
                this.eventService.formElementHiddenChangeSubject.next();
            });
        this.hideExpression.triggerEvaluation();
        this.hideConditionEvaluated = true;
    }

    protected listenForRevealChanges(): void {
        this.fieldInstance.revealSubject.pipe(takeUntil(this.destroyed)).subscribe((reveal: boolean) => {
            this.applyHiddenState();
        });
    }

    private listenForDebugChanges(): void {
        this.applicationService.debugSubject.pipe(
            skip(1),
            takeUntil(this.destroyed),
        ).subscribe((debug: boolean) => {
            this.applyHiddenState();
        });
    }

    private applyHiddenState(): void {
        let formlyFieldElement: HTMLElement = this.elementRef.nativeElement;
        if (formlyFieldElement.parentElement == null) {
            return;
        }
        while (formlyFieldElement.tagName != 'FORMLY-FIELD') {
            formlyFieldElement = formlyFieldElement.parentElement;
        }
        if (this.fieldInstance) {
            this.fieldInstance.setHidden(this.hidden);
        }
        let display: boolean = true;
        if (this.field?.className?.indexOf('no-layout') != -1) {
            // remove the layout unless debug mode is on
            display = this.applicationService.debug;
            this.fieldInstance.hasLayout = this.applicationService.debug;
        }
        if (display) {
            display = !this.hidden && this.fieldInstance.reveal;
        }
        if (display) {
            formlyFieldElement.classList.remove('no-display');
        } else {
            formlyFieldElement.classList.add('no-display');
        }
        this.field['hidden'] = this.hidden;

        // since a field is considered valid when hidden we need to re-check the 
        // validation on the field when the hidden state changes.
        this.formControl.updateValueAndValidity({ onlySelf: true, emitEvent: true });
    }

    /**
     * Recreate the expressions associated with fields in this question set.
     * This needs to happen when the question set was part of a repeating set and one was removed.
     */
    public onRecreateExpressions(): void {
        this.fieldInstance.recreateExpressionsSubject.pipe(takeUntil(this.destroyed)).subscribe(() => {
            this.setupExpressions();
        });
    }
}
