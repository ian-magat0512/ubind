import {
    Component, Input, OnInit, Output, EventEmitter, HostBinding, HostListener, OnDestroy,
} from '@angular/core';
import { trigger, state, style, animate, transition } from '@angular/animations';
import { WorkflowService } from '@app/services/workflow.service';
import { ConfigService } from '@app/services/config.service';
import { WindowScrollService } from '@app/services/window-scroll.service';
import { ExpressionDependencies } from '@app/expressions/expression-dependencies';
import { ActionButton } from './action-button';
import { ApplicationService } from '@app/services/application.service';
import { Widget } from '../widget';
import { EventService } from '@app/services/event.service';
import { WorkflowAction } from '@app/models/configuration/workflow-action';
import { ArrayHelper } from '@app/helpers/array.helper';
import * as _ from 'lodash-es';
import { BrowserDetectionService } from '@app/services/browser-detection.service';
import { install } from "resize-observer";
import { takeUntil } from 'rxjs/operators';
import { FormService } from '@app/services/form.service';
import { TransitionState } from '@app/models/transition-state.enum';

/**
 * Export actions widget component class.
 * This class manage actions widget functions.
 */
@Component({
    selector: 'actions-widget',
    templateUrl: './actions.widget.html',
    styleUrls: ['./actions.widget.scss'],
    animations: [
        trigger('actionsSlide', [
            state(TransitionState.Left, style({ transform: 'translateX(-25px)' })),
            state(TransitionState.None, style({ transform: 'translateX(0)' })),
            state(TransitionState.Right, style({ transform: 'translateX(25px)' })),
            transition(`* => ${TransitionState.None}`, animate('1000ms ease-out')),
            transition(`${TransitionState.None} => *`, animate('400ms ease-out')),
        ]),
        trigger('actionsFade', [
            state(TransitionState.Left, style({ opacity: 0 })),
            state(TransitionState.None, style({ opacity: 1 })),
            state(TransitionState.Right, style({ opacity: 0 })),
            transition(`* => ${TransitionState.None}`, animate('1000ms ease-out')),
            transition(`${TransitionState.None} => *`, animate('400ms ease-out')),
        ]),
        trigger('buttonFade', [ // note, not currently used
            state('in', style({ opacity: 1, height: '*', width: '*', transform: 'translateY(0) scaleY(1)' })),
            transition('* => in', [ // Should be 'void => *' (or query with ':enter') but neither work, so using this
                style({ opacity: 0, height: '0px', width: '0px' }),
                animate('250ms ease-in-out', style({ opacity: 0, height: '*', width: '*' })),
                animate('0ms', style({ transform: 'translateY(-10px) scaleY(0.9)' })),
                animate('150ms ease-out', style({ opacity: 1, transform: 'translateY(0) scaleY(1)' })),
            ]),
            transition('* => *', [ // Should be '* => void' (or query with ':leave') but neither work, so using this
                animate('150ms ease-in', style({ opacity: 0, transform: 'translateY(10px) scaleY(0.9)' })),
            ]),
        ]),
    ],
})

export class ActionsWidget extends Widget implements OnInit, OnDestroy {

    /**
     * Instead of using "@media (max-width: 767px) {" style queries, we instead
     * work of the following css classes to know if we're in mobile size.
     * The reasons for this are:
     * 1. The mobile breakpoint is set by the form configuration.
     * 2. When loading the webform in the portal, the viewport width returned by the media query
     * doesn't include the scroll bar, which it normally would. Using our css classes "mobile-size"
     * and "wider-than-mobile" ensures the scrollbar width is taken into account.
     */
    @HostBinding('class.mobile-width')
    public isMobileWidth: boolean = true;

    @HostBinding('class.wider-than-mobile')
    public isWiderThanMobile: boolean = false;

    @Input('location')
    public location: string;

    @Output()
    public empty: EventEmitter<boolean> = new EventEmitter<boolean>();

    /**
     * @deprecated please use location instead.
     */
    public widgetPosition: string;
    public actionButtons: Array<ActionButton>;
    public refSize: string = '';
    public debug: boolean = false;
    protected transitionDelayBetweenMs: number = 1000;
    public transitionState: string = TransitionState.None;
    public cssPrefix: string;
    private currentWorkflowStepName: string;

    public constructor(
        protected workflowService: WorkflowService,
        protected configService: ConfigService,
        protected windowScrollService: WindowScrollService,
        protected expressionDependencies: ExpressionDependencies,
        public applicationService: ApplicationService,
        private eventService: EventService,
        private browserDetectionService: BrowserDetectionService,
        private formService: FormService,
    ) {
        super();
        if (typeof window !== "undefined" && !window.ResizeObserver) {
            install();
        }
    }

    public ngOnInit(): void {
        super.ngOnInit();
        this.updateWidgetPositionFromLocation();
        this.generateActionButtons();
        this.listenForNextStep();
        this.listenForPageTransitionStateChanges();
        this.onConfigurationUpdated();
        this.cssPrefix = this.generatePrefix();
        this.onWindowResize(null);
    }

    public ngOnDestroy(): void {
        super.ngOnDestroy();
        this.formService.deregisterAutoTriggerActionButtons(this.currentWorkflowStepName);
    }

    private onConfigurationUpdated(): void {
        this.eventService.updatedConfiguration.pipe(takeUntil(this.destroyed))
            .subscribe(() => {
                this.formService.deregisterAutoTriggerActionButtons(this.currentWorkflowStepName);
                this.currentWorkflowStepName = null;
                this.updateWidgetPositionFromLocation();
                this.generateActionButtons();
            });
    }

    private generatePrefix(): string {
        return this.location
            ? this.location + '-'
            : _.uniqueId() + '-';
    }

    protected generateActionButtons(): void {
        if (!this.workflowService.currentDestination
            || this.currentWorkflowStepName == this.workflowService.currentDestination.stepName
        ) {
            return;
        }
        this.currentWorkflowStepName = this.workflowService.currentDestination.stepName;
        let actionButtons: Array<ActionButton> = new Array<ActionButton>();
        if (this.configService.workflowSteps[this.currentWorkflowStepName]) {
            let sourceActions: Array<WorkflowAction>
                = this.configService.workflowSteps[this.currentWorkflowStepName].actions;
            for (let actionName in sourceActions) {
                let workflowAction: WorkflowAction = sourceActions[actionName];
                this.workflowService.applyDefaultLocations(workflowAction);
                if (workflowAction.locations.includes(this.location)
                    || workflowAction.locationsWhenPrimaryAndQuestionSetsValid.includes(this.location)
                ) {
                    let actionButton: ActionButton = {
                        name: actionName,
                        definition: sourceActions[actionName],
                    };

                    actionButtons.push(actionButton);
                }
            }
        }
        if (!this.actionButtons) {
            this.actionButtons = actionButtons;
        } else {
            ArrayHelper.synchronise<ActionButton>(
                actionButtons,
                this.actionButtons,
                (item1: ActionButton, item2: ActionButton): boolean => _.isEqual(item1, item2));
        }
        this.empty.emit(actionButtons.length == 0);
    }

    protected listenForNextStep(): void {
        this.eventService.readyForNextStep$.pipe(takeUntil(this.destroyed)).subscribe(() => {
            this.formService.deregisterAutoTriggerActionButtons(this.currentWorkflowStepName);
            this.generateActionButtons();
        });
    }

    private listenForPageTransitionStateChanges(): void {
        this.eventService.pageAnimationTransitionStateSubject.pipe(takeUntil(this.destroyed))
            .subscribe((transitionState: string) => this.transitionState = transitionState);
    }

    @HostListener("window:resize", ['$event'])
    protected onWindowResize(event: any): void {
        this.isMobileWidth = this.browserDetectionService.isMobileWidth();
        this.isWiderThanMobile = !this.isMobileWidth;
    }

    private updateWidgetPositionFromLocation(): void {
        switch (this.location) {
            case 'calculationWidgetFooter':
                this.widgetPosition = 'sidebar';
                break;
            case 'formFooter':
                this.widgetPosition = 'footer';
                break;
        }
    }
}
