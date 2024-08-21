import { Component, HostListener, OnDestroy, AfterViewInit, OnInit, HostBinding } from '@angular/core';
import { trigger, state, style, animate, transition, sequence } from '@angular/animations';
import { FormService } from '@app/services/form.service';
import { CalculationService } from '@app/services/calculation.service';
import { WorkflowService } from '@app/services/workflow.service';
import { ConfigService } from '@app/services/config.service';
import { distinctUntilChanged, filter, take, takeUntil } from 'rxjs/operators';
import { TextWithExpressions } from '@app/expressions/text-with-expressions';
import { Expression } from '@app/expressions/expression';
import { ExpressionDependencies } from '@app/expressions/expression-dependencies';
import { StringHelper } from '@app/helpers/string.helper';
import { EventService } from '@app/services/event.service';
import { ApplicationService } from '@app/services/application.service';
import { ArrayHelper } from '@app/helpers/array.helper';
import { WorkflowDestination } from '@app/models/workflow-destination';
import { DynamicWidget } from '../dynamic.widget';
import { WorkflowDestinationService } from '@app/services/workflow-destination.service';
import { WorkflowStatusService } from '@app/services/workflow-status.service';
import { ProgressStep } from './progress-step';
import { ProgressStepIcons } from '@app/resource-models/configuration/progress-step-icons';
import { ProgressWidgetSettings } from '@app/resource-models/configuration/progress-widget-settings';
import * as _ from 'lodash-es';
import { TextElementsForStep } from '@app/models/configuration/working-configuration';
import { TextElement } from '@app/models/configuration/text-element';
import { QuoteState } from '@app/models/quote-state.enum';
import { ClaimState } from '@app/models/claim-state.enum';
import { FormType } from '@app/models/form-type.enum';

/**
 * The progress widget shows the users progress through the steps in the workflow.
 */
@Component({
    selector: 'progress-widget',
    templateUrl: './progress.widget.html',
    styleUrls: ['./progress.widget.scss'],
    animations: [
        trigger('progressBarAnimation', [
            state('visible', style({ 'height': '*', display: 'flex' })),
            state('hidden', style({ 'height': 0, display: 'none' })),
            state('initial', style({ transform: 'translateY(-100px)', 'height': '*' })),
            transition('visible => hidden',
                sequence([
                    animate('800ms ease-out', style({ 'height': 0 })),
                    style({ display: 'none' }),
                ]),
            ),
            transition('hidden => visible',
                sequence([
                    style({ display: 'flex', transform: 'translateY(-100px)', 'height': 0 }),
                    animate('600ms ease-in', style({ transform: 'translateY(0)', 'height': '*' })),
                ]),
            ),
            transition('initial => visible',
                sequence([
                    style({ display: 'flex', 'height': '*' }),
                    animate('600ms ease-in', style({ transform: 'translateY(0)' })),
                ]),
            ),
        ]),
        // the following are use for when an item appears or disappears and we want to animate them in/out 
        // and slide things across. It still needs to be implemented.
        /*
        trigger('tabAnimation', [
            state(TransitionState.None, style({ opacity: 100, transform: 'translateX(0)' })),
            state('out', style({ opacity: 0, transform: 'translateX(-30px)' })),
            state(TransitionState.Backward, style({ opacity: 0, transform: 'translateX(-30px)' })),
            state(TransitionState.Forward, style({ opacity: 0, transform: 'translateX(30px)' })),
            transition(`* => ${TransitionState.None}`, animate('700ms 900ms ease-out')),
            transition(`${TransitionState.None} => *`, animate('200ms 900ms ease-out')),
        ]),
        trigger('itemFade', [
            state('in', style({ opacity: 100, transform: 'translateX(0)' })),
            state('animate-in', style({ opacity: 100 })),
            state('out', style({ opacity: 0 })),
            state('animate-out', style({ opacity: 0, transform: 'translateX(-30px)' })),
            transition('* => animate-in', [
                animate('400ms 10ms ease-in-out'),
            ]),
            transition('* => animate-out', [
                animate('400ms 10ms ease-in-out'),
            ]),
        ]),
        trigger('itemSlide', [
            state('in', style({ width: '*' })),
            state('animate-in', style({ width: '*', transform: 'scaleX(1)' })),
            state('out', style({ width: 0 })),
            state('animate-out', style({ width: 0, transform: 'scale(2)' })),
            transition('* => animate-in', [
                animate('20ms 20ms ease-in-out'),
            ]),
            transition('* => animate-out', [
                animate('500ms 300ms ease-in-out'),
            ]),
        ]),
        */
    ],
})

export class ProgressWidget extends DynamicWidget implements OnDestroy, OnInit, AfterViewInit {

    @HostBinding('class.simple-mode') public isSimpleMode: boolean = false;

    public progressSteps: Array<ProgressStep>;
    public include: boolean = true;
    public transitionDelayBetweenMs: number = 1000;
    public stepTransitionState: Array<any> = [];
    public initialised: boolean = false;
    private minimumWidthInPixels: number = 150;
    private screenWidth: any;
    public showLeftStepsTruncatedIndicator: boolean;
    public showRightStepsTruncatedIndicator: boolean;
    public displayableItems: Array<any>;
    private activeStepIndex: number = -1;
    public activeRenderedStepIndex: number = -1;
    public visibleState: string = 'initial';
    private readyToUpdateVisibleState: boolean = false;
    public progressWidgetSettings: ProgressWidgetSettings;
    public showLineForLastStep: boolean;
    public showLineForCollapsedSteps: boolean;
    public collapsedStepLineWidth: string;

    /**
     * The percentage complete for the current step
     */
    public stepPercentageComplete: number = 0.0;
    public completionLinePart1WidthPercent: number = 0.0;
    public completionLinePart2WidthPercent: number = 0.0;

    public constructor(
        protected calculationService: CalculationService,
        protected workflowService: WorkflowService,
        protected configService: ConfigService,
        protected formService: FormService,
        protected expressionDependencies: ExpressionDependencies,
        private eventService: EventService,
        public applicationService: ApplicationService,
        workflowDestinationService: WorkflowDestinationService,
        private workflowStatusService: WorkflowStatusService,
    ) {
        super(workflowService, configService, workflowDestinationService);
        configService.configurationReadySubject.pipe(takeUntil(this.destroyed), filter((ready: boolean) => ready))
            .subscribe(() => {
                this.applySettings();
            });
    }

    @HostListener('window:resize', ['$event'])
    public getScreenWidthAndSetDisplayableItems(): void {
        this.screenWidth = window.innerWidth;
        this.isSimpleMode = this.progressWidgetSettings.showVisuals
            && this.screenWidth < (this.progressWidgetSettings.simpleModeBreakpointPixels ?? 600);
        this.showLineForLastStep = this.isSimpleMode
            && this.progressWidgetSettings.showLineForLastStepInSimpleMode !== false;

        if (this.progressSteps && this.initialised) {
            this.setItemsToDisplayAndRemoveItems();
        }
    }

    public async ngOnInit(): Promise<void> {
        super.ngOnInit();
        this.visibleState = 'initial';
        this.initialised = true;
        if (this.include) {
            this.generateSteps();
            this.onChangeActiveStep(this.applicationService.currentWorkflowDestination.stepName);
            this.getScreenWidthAndSetDisplayableItems();
        }
        this.listenToPageAnimationTransitionCompleted();
        this.listenToStepCompletion();
        this.onConfigurationUpdated();
    }

    private onConfigurationUpdated(): void {
        this.eventService.updatedConfiguration.pipe(takeUntil(this.destroyed))
            .subscribe(() => {
                this.applySettings();
                this.generateSteps();
                this.onChangeActiveStep(this.applicationService.currentWorkflowDestination.stepName);
                this.setItemsToDisplayAndRemoveItems();
            });
    }

    private applySettings(): void {
        this.progressWidgetSettings = this.configService.configuration.theme?.progressWidgetSettings;
        this.include = this.progressWidgetSettings?.showProgressWidget
            || this.configService.theme?.showProgressWidget;
        this.minimumWidthInPixels = this.progressWidgetSettings?.minimumStepWidthPixels
            || this.configService.theme?.minimumProgressStepWidthPixels
            || 150;
        if (this.progressWidgetSettings?.showLineForCollapsedSteps === undefined) {
            this.showLineForCollapsedSteps = this.progressWidgetSettings?.symbolContainerMargin == '0'
                || this.progressWidgetSettings?.symbolContainerMargin == '0px'
                || this.progressWidgetSettings?.symbolContainerMargin == '-1px'
                || this.progressWidgetSettings?.symbolContainerMargin === undefined;
        } else {
            this.showLineForCollapsedSteps = this.progressWidgetSettings?.showLineForCollapsedSteps;
        }
        this.collapsedStepLineWidth = this.showLineForCollapsedSteps
            ? this.progressWidgetSettings?.collapsedStepLineWidth ?? '6px'
            : '0px';
        if (!this.progressWidgetSettings) {
            this.progressWidgetSettings = <ProgressWidgetSettings>{};
        }
    }

    public ngAfterViewInit(): void {
        super.ngAfterViewInit();
        setTimeout(() => {
            this.eventService.webFormLoadedSubject.pipe(
                takeUntil(this.destroyed),
                filter((loaded: boolean) => loaded),
            )
                .subscribe((loaded: boolean) => {
                    this.readyToUpdateVisibleState = true;
                    this.setItemsToDisplayAndRemoveItems();
                });
        }, 100);
    }

    public ngOnDestroy(): void {
        super.ngOnDestroy();
    }

    /**
     * So that our animations are in sync with those of the WebFormComponent
     * We'll trigger completed transitions when it does.
     */
    private listenToPageAnimationTransitionCompleted(): void {
        this.eventService.pageAnimationTransitionCompletedSubject.pipe(takeUntil(this.destroyed))
            .subscribe(() => {
                this.onCompletedTransition();
            });
    }

    protected generateSteps(): void {
        let progressSteps: Array<ProgressStep> = new Array<ProgressStep>();
        for (let step in this.configService.workflowSteps) {
            let progressStep: ProgressStep = {
                name: step,
                title: '',
                titleIcon: null,
                first: false,
                last: false,
                hasNext: false,
                hasPrevious: false,
                active: false,
                nextTruncated: false,
                previousTruncated: false,
                nextActive: false,
                previousActive: false,
                future: false,
                past: false,
                render: false,
                truncated: false,
                tabIndex: null,
                index: null,
                renderIndex: null,
                icon: null,
                canClickToNavigate: false,
            };
            this.setupTitleExpression(step, progressStep);
            this.setupTabIndexExpression(step, progressStep);
            progressSteps.push(progressStep);
        }
        progressSteps = this.sortProgressSteps(progressSteps);
        if (!this.progressSteps || this.progressSteps.length == 0) {
            this.progressSteps = progressSteps;
        } else {
            ArrayHelper.synchronise<ProgressStep>(
                progressSteps,
                this.progressSteps,
                (item1: ProgressStep, item2: ProgressStep): boolean => {
                    return item1.name == item2.name
                        && item1.index == item2.index
                        && item1.tabIndex == item2.tabIndex
                        && item1.title == item2.title
                        && item1.titleIcon == item2.titleIcon;
                },
                (item: ProgressStep): void => {
                    if (item.titleExpression) {
                        item.titleExpression.dispose();
                    }
                    if (item.tabIndexExpression) {
                        item.tabIndexExpression.dispose();
                    }
                });
        }
    }

    private setVisibleState(visibleState: string) {
        if (this.visibleState != visibleState) {
            // ensure section widget has finished rendering
            if (this.workflowStatusService.workflowStepChangeInProgress) {
                this.eventService.sectionWidgetCompletedViewInitSubject.pipe(
                    take(1),
                ).subscribe(() => {
                    this.visibleState = visibleState;
                });
            } else {
                this.visibleState = visibleState;
            }
        }
    }

    private setupTitleExpression(step: string, progressStep: ProgressStep): void {
        let textElementsForStep: TextElementsForStep = this.configService.textElements?.workflow?.[step];
        let tabLabel: TextElement = textElementsForStep ? textElementsForStep.tabLabel : null;
        if (!tabLabel || !tabLabel.text) {
            // There is no tab label so this one doesn't want to be included
            // in the progress widget.
            progressStep.render = false;
            return;
        }
        progressStep.titleExpression = new TextWithExpressions(
            tabLabel.text,
            this.expressionDependencies,
            'progress widget title');
        progressStep.titleExpression.nextResultObservable.pipe(takeUntil(this.destroyed))
            .subscribe((value: string) => {
                progressStep.title = value;
                progressStep.render = progressStep.tabIndex != null && !StringHelper.isNullOrEmpty(progressStep.title);

                if (this.progressSteps) {
                    this.progressSteps = this.sortProgressSteps(this.progressSteps);
                }
                if (this.initialised) {
                    this.onChangeActiveStep(this.applicationService.currentWorkflowDestination.stepName);
                }
            });
        progressStep.titleExpression.triggerEvaluationWhenFormLoaded();

        // set the icon, if there is one defined
        progressStep.titleIcon = tabLabel.icon;
    }

    private setupTabIndexExpression(step: string, progressStep: ProgressStep): void {
        const index: any = this.configService.workflowSteps[step].tabIndex;
        let indexExpressionSource: any = this.configService.workflowSteps[step].tabIndexExpression;
        if (!index && !indexExpressionSource) {
            progressStep.tabIndex = null;
            progressStep.render = false;
            return;
        }
        if (index) {
            if (isNaN(index)) {
                indexExpressionSource = index;
            } else {
                progressStep.tabIndex = index;
                progressStep.render = progressStep.tabIndex != null && !StringHelper.isNullOrEmpty(progressStep.title);
            }
        }
        if (indexExpressionSource) {
            progressStep.tabIndexExpression = new Expression(
                indexExpressionSource,
                this.expressionDependencies,
                `progress widget step "${step}" index`);
            progressStep.tabIndexExpression.nextResultObservable.pipe(distinctUntilChanged(), takeUntil(this.destroyed))
                .subscribe((value: number) => {
                    progressStep.tabIndex = value;
                    progressStep.render = progressStep.tabIndex != null &&
                        !StringHelper.isNullOrEmpty(progressStep.title);
                    if (this.progressSteps) {
                        this.progressSteps = this.sortProgressSteps(this.progressSteps);
                    }
                    if (this.initialised) {
                        this.onChangeActiveStep(this.applicationService.currentWorkflowDestination.stepName);
                    }
                });
            progressStep.tabIndexExpression.triggerEvaluation();
        }
    }

    private onChangeActiveStep(stepName: string): void {
        if (!this.progressSteps || this.progressSteps.length == 0) {
            // we don't have any to update
            return;
        }
        let activeStepIndexWasSet: boolean = false;
        let firstStepWasSet: boolean = false;
        let previousRenderedStep: ProgressStep = null;
        for (let i: number = 0; i < this.progressSteps.length; i++) {
            let progressStep: ProgressStep = this.progressSteps[i];

            // set the "first" flag
            if (!firstStepWasSet && progressStep.render) {
                progressStep.first = true;
                firstStepWasSet = true;
            } else {
                progressStep.first = false;
            }

            // set the "active" flag
            progressStep.active = progressStep.name == stepName;
            if (progressStep.active) {
                this.activeStepIndex = i;
                activeStepIndexWasSet = true;
            }

            // set the "future" and "past" flags
            progressStep.future = !progressStep.active && activeStepIndexWasSet;
            progressStep.past = !progressStep.active && !activeStepIndexWasSet;

            // set the "nextActive" flag
            if (progressStep.active && previousRenderedStep) {
                previousRenderedStep.nextActive = true;
            } else if (previousRenderedStep) {
                previousRenderedStep.nextActive = false;
            }

            // set the "hasNext", "hasPrevious", "nextActive" and "previousActive" flags
            if (previousRenderedStep) {
                previousRenderedStep.hasNext = true;
                progressStep.hasPrevious = true;
                previousRenderedStep.nextActive = progressStep.active;
                progressStep.previousActive = previousRenderedStep.active;
            } else {
                progressStep.hasPrevious = false;
            }

            // store the previous rendered step so we can set the flags on it
            progressStep.last = false;
            if (progressStep.render) {
                previousRenderedStep = progressStep;
            }

            // determine whether we will allow the user to navigate back
            progressStep.canClickToNavigate = progressStep.past
                && this.doesQuoteOrClaimStateAllowBackwardsNavigation();
        }

        // set the "last" flag
        if (previousRenderedStep) {
            previousRenderedStep.last = true;
            previousRenderedStep.hasNext = false;
        }

        // set the icons on all of the steps
        if (this.progressWidgetSettings?.showIcons) {
            this.applyIcons();
        }

        if (!activeStepIndexWasSet) {
            this.activeStepIndex = -1;
        }
        if (this.initialised) {
            this.setItemsToDisplayAndRemoveItems();
        }
    }

    private doesQuoteOrClaimStateAllowBackwardsNavigation(): boolean {
        return (this.applicationService.formType == FormType.Quote
            && (this.applicationService.quoteState == QuoteState.Nascent
                || this.applicationService.quoteState == QuoteState.Incomplete))
            || (this.applicationService.formType == FormType.Claim
                && (this.applicationService.claimState == ClaimState.Nascent
                    || this.applicationService.claimState == ClaimState.Incomplete));
    }

    protected async onChangeStepBeforeAnimateIn(
        previousDestination: WorkflowDestination,
        nextDestination: WorkflowDestination,
    ): Promise<void> {
        this.onChangeActiveStep(nextDestination.stepName);
    }

    private sortProgressSteps(progressSteps: Array<ProgressStep>): Array<ProgressStep> {
        let sortedSteps: Array<ProgressStep> = progressSteps.sort((a: ProgressStep, b: ProgressStep): number => {
            return (a.tabIndex < b.tabIndex)
                ? -1
                : (a.tabIndex == b.tabIndex) ? 0 : 1;
        });
        for (let i: number = 0; i < sortedSteps.length; i++) {
            sortedSteps[i].index = i;
        }
        return sortedSteps;
    }

    private setItemsToDisplayAndRemoveItems(): void {
        if (!this.progressSteps) {
            return;
        }
        this.updateRenderIndex();
        let renderedProgressSteps: Array<ProgressStep> = this.progressSteps.filter((ps: ProgressStep) => ps.render);
        if (renderedProgressSteps.length && this.activeStepIndex > -1) {
            let numberOfItemsThatFit: number = this.getNumberOfItemsThatFitViewPort();
            const startAndEndIndexObj: any = this.getStartAndEndIndex(renderedProgressSteps, numberOfItemsThatFit);
            this.setTruncatedFlags(renderedProgressSteps, startAndEndIndexObj.startIndex, startAndEndIndexObj.endIndex);
            this.calculateStepsTruncatedIndictors(renderedProgressSteps,
                startAndEndIndexObj.startIndex, startAndEndIndexObj.endIndex);
        }
    }

    /**
     * updates the render index on the progress steps, and also sets activeRenderedStepIndex
     */
    private updateRenderIndex(): void {
        let renderIndex: number = 0;
        let activeRenderedStepIndexWasSet: boolean = false;
        this.progressSteps.forEach((ps: ProgressStep) => {
            if (ps.render) {
                ps.renderIndex = renderIndex;
                if (ps.index == this.activeStepIndex) {
                    this.activeRenderedStepIndex = renderIndex;
                    activeRenderedStepIndexWasSet = true;
                }
                renderIndex++;
            } else {
                ps.renderIndex = -1;
            }
        });
        if (activeRenderedStepIndexWasSet) {
            if (this.readyToUpdateVisibleState) {
                this.setVisibleState('visible');
            }
        } else {
            if (this.readyToUpdateVisibleState) {
                this.setVisibleState('hidden');
            }
        }
    }

    private getStartAndEndIndex(renderedProgressSteps: Array<ProgressStep>,
        numberOfItemsThatFit: any): { startIndex: number; endIndex: number } {
        // when the current index is near index zero, we tend to display more items near index zero 
        // and less items we remove from the left, but
        // when the current index is near the last item's index, we tend to display more items near last index 
        // and less items we remove from the right
        let renderedItemsLength: number = renderedProgressSteps.length;
        let isCurrentIndexNearTheIndexZero: boolean = false;
        let startIndex: number = 0;
        let endIndex: number = 0;
        let totalItemsMidpoint: number = renderedItemsLength / 2;
        let itemsThatFitsMidpoint: number = Math.floor(numberOfItemsThatFit / 2);
        isCurrentIndexNearTheIndexZero = totalItemsMidpoint > this.activeRenderedStepIndex;

        if ((numberOfItemsThatFit % 2) == 0) {
            if (isCurrentIndexNearTheIndexZero) {
                startIndex = this.activeRenderedStepIndex - itemsThatFitsMidpoint;
                endIndex = this.activeRenderedStepIndex + (itemsThatFitsMidpoint - 1);
            } else {
                startIndex = this.activeRenderedStepIndex - (itemsThatFitsMidpoint - 1);
                endIndex = this.activeRenderedStepIndex + itemsThatFitsMidpoint;
            }
        } else {
            startIndex = this.activeRenderedStepIndex - itemsThatFitsMidpoint;
            endIndex = this.activeRenderedStepIndex + itemsThatFitsMidpoint;
        }

        if (startIndex <= 0) {
            startIndex = 0;
            endIndex = numberOfItemsThatFit - 1;
        }

        if (renderedItemsLength <= endIndex + 1) {
            endIndex = renderedItemsLength - 1;
            startIndex = Math.max(0, renderedItemsLength - numberOfItemsThatFit);
        }

        return { startIndex: startIndex, endIndex: endIndex };
    }

    /**
     * determines whether the left and right steps truncated indicators are visible
     * @param startAndEndIndex 
     */
    private calculateStepsTruncatedIndictors(
        renderedProgressSteps: Array<ProgressStep>,
        startIndex: number,
        endIndex: number,
    ): void {
        this.showLeftStepsTruncatedIndicator = false;
        this.showRightStepsTruncatedIndicator = false;
        if (startIndex > 0) {
            this.showLeftStepsTruncatedIndicator = true;
        }

        if (endIndex < renderedProgressSteps.length - 1) {
            this.showRightStepsTruncatedIndicator = true;
        }
    }

    private getNumberOfItemsThatFitViewPort(): number {
        if (this.screenWidth == 0) {
            return 0;
        }

        if (this.isSimpleMode) {
            const symbolContainerSizePixels: number = this.progressWidgetSettings.symbolContainerSize?.endsWith('px')
                ? Number.parseInt(this.progressWidgetSettings.symbolContainerSize, 10)
                : 30;
            const emptySymbolContainerSizePixels: number
                = this.progressWidgetSettings.emptySymbolContainerSize?.endsWith('px')
                    ? Number.parseInt(this.progressWidgetSettings.emptySymbolContainerSize, 10)
                    : 20;
            const symbolContainerWidth: number = Math.max(symbolContainerSizePixels, emptySymbolContainerSizePixels);
            return Math.floor(this.screenWidth / symbolContainerWidth) - 2;
        }

        let numberOfItemsThatFit: number = Math.floor(this.screenWidth / this.minimumWidthInPixels) - 1;
        if (numberOfItemsThatFit > 0) {
            return numberOfItemsThatFit;
        } else {
            return 1;
        }
    }

    private setTruncatedFlags(
        renderedProgressSteps: Array<ProgressStep>,
        startIndex: number,
        endIndex: number,
    ): void {
        let previousStep: ProgressStep = null;
        for (let i: number = 0; i < renderedProgressSteps.length; i++) {
            let currentStep: ProgressStep = renderedProgressSteps[i];
            currentStep.truncated = (i < startIndex || i > endIndex);
            currentStep.previousTruncated = previousStep && previousStep.truncated;
            if (previousStep) {
                previousStep.nextTruncated = currentStep.truncated;
            }
            previousStep = currentStep;
        }
    }

    private applyIcons(): void {
        for (let progressStep of this.progressSteps) {
            if (this.configService.workflowSteps[progressStep.name].progressStepIcon) {
                progressStep.icon = this.configService.workflowSteps[progressStep.name].progressStepIcon;
            } else {
                const iconsConfig: ProgressStepIcons = _.merge(
                    {},
                    this.configService.configuration.theme?.progressWidgetSettings?.icons,
                    this.configService.workflowSteps[progressStep.name].progressStepIcons);
                if (progressStep.active && iconsConfig.active) {
                    progressStep.icon = iconsConfig.active;
                } else if (progressStep.first && iconsConfig.first) {
                    progressStep.icon = iconsConfig.first;
                } else if (progressStep.last && iconsConfig.last) {
                    progressStep.icon = iconsConfig.last;
                } else if (progressStep.past && iconsConfig.past) {
                    progressStep.icon = iconsConfig.past;
                } else if (progressStep.future && iconsConfig.future) {
                    progressStep.icon = iconsConfig.future;
                } else if (!progressStep.active && iconsConfig.inactive) {
                    progressStep.icon = iconsConfig.inactive;
                } else {
                    progressStep.icon = null;
                }
            }
        }
    }

    public onProgressStepClick(progressStep: ProgressStep): void {
        if (this.applicationService.debug
            || (progressStep.past && this.doesQuoteOrClaimStateAllowBackwardsNavigation())
        ) {
            this.workflowService.navigateTo({ stepName: progressStep.name });
        }
    }

    private listenToStepCompletion(): void {
        this.eventService.stepCompletionSubject.pipe(takeUntil(this.destroyed))
            .subscribe((percentage: number) => this.onStepCompletionPercentageUpdate(percentage));
    }

    private onStepCompletionPercentageUpdate(percentage: number): void {
        this.stepPercentageComplete = percentage;
        this.completionLinePart1WidthPercent = Math.min(this.stepPercentageComplete * 2.0, 100.0);
        this.completionLinePart2WidthPercent = Math.max(this.stepPercentageComplete * 2.0, 100.0) - 100.0;
    }
}
