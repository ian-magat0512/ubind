import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { trigger, state, style, animate, transition } from '@angular/animations';
import { WorkflowService } from '@app/services/workflow.service';
import { ConfigService } from '@app/services/config.service';
import { distinctUntilChanged, filter, takeUntil } from 'rxjs/operators';
import { TextWithExpressions } from '@app/expressions/text-with-expressions';
import { ExpressionDependencies } from '@app/expressions/expression-dependencies';
import { Widget } from '../widget';
import { NavigationDirection } from '@app/models/navigation-direction.enum';
import { ApplicationService } from '@app/services/application.service';
import { WorkflowDestination } from '@app/models/workflow-destination';
import { WorkflowDestinationService } from '@app/services/workflow-destination.service';
import { EventService } from '@app/services/event.service';
import { TextElementsForStep } from '@app/models/configuration/working-configuration';
import { StringHelper } from '@app/helpers/string.helper';
import { TransitionState } from '@app/models/transition-state.enum';

/**
 * Export heading widget component class.
 * This class manage heading widget functions.
 */
@Component({
    selector: 'heading-widget',
    templateUrl: './heading.widget.html',
    animations: [
        trigger('headingAnimation', [
            state(TransitionState.None, style({ opacity: 1, transform: 'translateX(0)' })),
            state(TransitionState.Left, style({ opacity: 0, transform: 'translateX(-20px)' })),
            state(TransitionState.Right, style({ opacity: 0, transform: 'translateX(20px)' })),
            transition(`* => ${TransitionState.None}`, animate('800ms ease-out')),
            transition(`${TransitionState.None} => *`, animate('800ms ease-out')),
        ]),
    ],
    styleUrls: [
        './heading.widget.scss',
    ],
})

export class HeadingWidget extends Widget implements OnInit {

    private transitioningOut: boolean = false;
    private transitioningIn: boolean = true;
    public useTopHeading: boolean = true;
    public currentHeading: string;
    public oldHeading: string;
    public newHeading: string;
    public currentIcon: string;
    public oldIcon: string;
    public newIcon: string;
    public topHeadingExpression: TextWithExpressions;
    public debug: boolean = false;
    public transitionState: string = TransitionState.Right;
    private transitionDirection: NavigationDirection = NavigationDirection.Forward;
    private topHeadingTextForStep: string;
    private iconForStep: string;

    public constructor(
        protected workflowService: WorkflowService,
        protected configService: ConfigService,
        protected expressionDependencies: ExpressionDependencies,
        private changeDetectorRef: ChangeDetectorRef,
        public applicationService: ApplicationService,
        protected workflowDestinationService: WorkflowDestinationService,
        private eventService: EventService,
    ) {
        super();

        configService.configurationReadySubject.pipe(filter((ready: boolean) => ready))
            .subscribe(() => this.updateTopHeadingVisibility());
        this.eventService.updatedConfiguration.pipe(takeUntil(this.destroyed))
            .subscribe(() => this.updateTopHeadingVisibility());
    }

    public ngOnInit(): void {
        super.ngOnInit();
        this.topHeadingTextForStep = this.getTopHeadingTextForStep();
        this.iconForStep = this.getIconForStep();
        this.updateHeadingExpression(this.topHeadingTextForStep, this.iconForStep);

        this.workflowService.navigateToSubject
            .pipe(
                takeUntil(this.destroyed),
                distinctUntilChanged(),
            )
            .subscribe((destination: WorkflowDestination) => {
                if (destination) {
                    this.onNavigate(destination);
                }
            });

        this.onConfigurationUpdated();
    }

    private onConfigurationUpdated(): void {
        this.eventService.updatedConfiguration.pipe(takeUntil(this.destroyed))
            .subscribe(() => {
                const topHeadingTextForStep: string = this.getTopHeadingTextForStep();
                const iconForStep: string = this.getIconForStep();
                const headingChanged: boolean
                    = topHeadingTextForStep != this.topHeadingTextForStep || iconForStep != this.iconForStep;
                if (headingChanged) {
                    this.topHeadingTextForStep = topHeadingTextForStep;
                    this.iconForStep = iconForStep;
                    this.updateHeadingExpression(topHeadingTextForStep, iconForStep);
                }
            });
    }

    private updateTopHeadingVisibility(): void {
        this.useTopHeading = this.configService?.theme?.showTopHeading ?? false;
    }

    protected destroyExpressions(): void {
        this.topHeadingExpression?.dispose();
        this.topHeadingExpression = null;
        super.destroyExpressions();
    }

    protected async onNavigate(toDestination: WorkflowDestination): Promise<void> {
        const fromDestination: WorkflowDestination = this.workflowService.currentDestination;
        if (fromDestination && toDestination.stepName != fromDestination.stepName) {
            this.transitionDirection
                = this.workflowDestinationService.getNavigationDirection(fromDestination, toDestination);
            this.updateHeadingExpression(this.getTopHeadingTextForStep(), this.getIconForStep());
        }
    }

    private getTextElementsForStep(): TextElementsForStep {
        return this.configService.textElements?.workflow?.[this.workflowService.currentNavigation.to.stepName];
    }

    private getTopHeadingTextForStep(): string {
        let textElementsForStep: TextElementsForStep = this.getTextElementsForStep();
        return textElementsForStep && textElementsForStep.topHeading
            ? textElementsForStep.topHeading.text
            : '';
    }

    private getIconForStep(): string {
        let textElementsForStep: TextElementsForStep = this.getTextElementsForStep();
        return textElementsForStep && textElementsForStep.topHeading
            ? textElementsForStep.topHeading.icon
            : null;
    }

    private updateHeadingExpression(source: string, icon: string): void {
        let forceAnimationOut: boolean = true;
        if (this.topHeadingExpression) {
            this.topHeadingExpression.dispose();
            this.topHeadingExpression = null;
        }
        if (!StringHelper.isNullOrEmpty(source)) {
            this.topHeadingExpression = new TextWithExpressions(
                source,
                this.expressionDependencies,
                'heading widget top heading');
            this.topHeadingExpression.nextResultObservable.pipe(takeUntil(this.destroyed))
                .subscribe((value: string) => this.onNewHeading(value, icon, forceAnimationOut));
            this.topHeadingExpression.triggerEvaluationWhenFormLoaded();
        } else {
            this.onNewHeading(null, icon, forceAnimationOut);
        }
    }

    private onNewHeading(newHeading: string, icon: string, forceAnimationOut: boolean): void {
        this.oldHeading = this.currentHeading;
        this.oldIcon = this.currentIcon;
        this.newHeading = newHeading;
        this.newIcon = icon;
        const headingChanged: boolean = this.oldHeading != this.newHeading || this.oldIcon != this.newIcon;
        if (this.oldHeading && (forceAnimationOut || headingChanged)) {
            forceAnimationOut = false;
            this.transitioningOut = true;
            this.transitionState = this.transitionDirection == NavigationDirection.Forward
                ? TransitionState.Left
                : TransitionState.Right;
        } else if (headingChanged) {
            this.currentHeading = this.newHeading;
            this.currentIcon = this.newIcon;
            this.transitionState = TransitionState.None;
        }
    }

    /**
     * Called when an animation has completed due to a transition from one step to another.
     * This will be called twice. First when the animate out completes, and second when the animate in completes.
     * @param index 
     */
    public onCompletedTransition(animationName: string): void {
        if (this.transitioningOut) {
            this.transitioningOut = false;
            this.transitioningIn = true;
            this.currentHeading = this.newHeading;
            this.currentIcon = this.newIcon;
            this.transitionState = this.transitionDirection == NavigationDirection.Backward
                ? TransitionState.Left
                : TransitionState.Right;
            this.changeDetectorRef.detectChanges();
            // we need to add a setTimeout here otherwise during unit tests 
            // we'll get an ExpressionChangedAfterItHasBeenCheckedError
            setTimeout(() => {
                this.transitionState = TransitionState.None;
            }, 0);
        } else if (this.transitioningIn) {
            this.transitioningIn = false;
        }
    }
}
