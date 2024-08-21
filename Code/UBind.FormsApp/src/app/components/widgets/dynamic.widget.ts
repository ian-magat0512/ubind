import { AfterViewInit, OnInit, Directive } from '@angular/core';
import { Widget } from './widget';
import { ConfigService } from '../../services/config.service';
import { WorkflowService } from '../../services/workflow.service';
import { distinctUntilChanged, takeUntil } from 'rxjs/operators';
import { NavigationDirection } from '@app/models/navigation-direction.enum';
import { Observable, Subject } from 'rxjs';
import { WorkflowDestination } from '@app/models/workflow-destination';
import { WorkflowDestinationService } from '@app/services/workflow-destination.service';
import { TransitionState } from '@app/models/transition-state.enum';
import { TransitionAnimationOrientation } from '@app/models/transition-animation-orientation.enum';

/**
 * A DynamicWidget is one that changes it's contents or reacts to when the workflow step changes.
 * 
 * There are the following hooks which can be implemented by child components;
 * 
 * onChangeStepBeforeAnimateOut(previousStepName:string, stepName: string) 
 * - called before the animate out is started. Can be used to prepare the next step
 * onChangeStepAfterAnimateOut(previousStepName:string, stepName: string) 
 * - called after the animate out has completed. 
 * onChangeStepBeforeAnimateIn(previousStepName:string, stepName: string) 
 * - called before the animate in starts. Can be used to show the next step
 * onChangeStepAfterAnimateIn(previousStepName:string, stepName: string)
 * - called after the animate in has completed.
 */
@Directive()

export abstract class DynamicWidget extends Widget implements OnInit, AfterViewInit {

    public transitioningOut: boolean = false;
    public transitioningIn: boolean = false;
    protected transitionDelayBeforeMs: number = 0;
    protected transitionDelayBetweenMs: number = 0;
    public transitionState: string = TransitionState.None;
    public disableAnimations: boolean = true;
    private transitionDirection: NavigationDirection = NavigationDirection.Forward;
    private transitionStateSubject: Subject<string> = new Subject<string>();
    public transitionStateObservable: Observable<string>;
    private completedTransitionSubject: Subject<string> = new Subject<string>();
    public completedTransitionObservable: Observable<string>;

    public constructor(
        protected workflowService: WorkflowService,
        protected configService: ConfigService,
        protected workflowDestinationService: WorkflowDestinationService,
    ) {
        super();
        this.transitionStateObservable = this.transitionStateSubject.asObservable();
        this.completedTransitionObservable = this.completedTransitionSubject.asObservable();
    }

    public ngOnInit(): void {
        super.ngOnInit();
        this.workflowService.navigateToSubject
            .pipe(
                takeUntil(this.destroyed),
                distinctUntilChanged(),
            )
            .subscribe(async (destination: WorkflowDestination) => {
                if (destination) {
                    await this.onNavigate(destination);
                }
            });

        this.transitionDirection = this.workflowDestinationService.getNavigationDirection(
            this.workflowService.previousDestination,
            this.workflowService.currentDestination);
        this.transitionState = this.transitionDirection == NavigationDirection.Forward
            ? this.getForwardsTransitionState()
            : this.getBackwardsTransitionState();
        this.transitionStateSubject.next(this.transitionState);
    }

    public ngAfterViewInit(): void {
        // We must use setTimeout here to stop changes checked after run error. This causes another round of change
        // detection to run in response to the changes made here.
        setTimeout(async () => {
            this.disableAnimations = false;
            await this.onChangeStepBeforeAnimateIn(
                null,
                this.workflowService.currentDestination);
            this.transitioningIn = true;
            this.transitionState = TransitionState.None;
            this.transitionStateSubject.next(this.transitionState);
            if (this.workflowService.skipWorkflowAnimations) {
                this.onCompletedTransition();
            }
        }, 0);
    }

    protected async onNavigate(destination: WorkflowDestination): Promise<void> {
        await this.transitionOutCurrentStep();
    }

    /**
     * starts the transation out animation for the current step.
     */
    protected transitionOutCurrentStep(): Promise<void> {
        return new Promise((resolve: any, reject: any): void => {
            setTimeout(
                async () => {
                    if (this.workflowService.currentNavigation.to) {
                        this.transitionDirection = this.workflowDestinationService.getNavigationDirection(
                            this.workflowService.currentNavigation.from,
                            this.workflowService.currentNavigation.to);
                        await this.onChangeStepBeforeAnimateOut(
                            this.workflowService.currentNavigation.from,
                            this.workflowService.currentNavigation.to);
                    }
                    this.transitioningOut = true;
                    this.transitionState = this.transitionDirection == NavigationDirection.Forward
                        ? this.getBackwardsTransitionState()
                        : this.getForwardsTransitionState();
                    this.transitionStateSubject.next(this.transitionState);
                    if (this.workflowService.skipWorkflowAnimations) {
                        this.onCompletedTransition();
                    }
                    resolve();
                },
                this.transitionDelayBeforeMs);
        });
    }

    private getBackwardsTransitionState(): TransitionState {
        const orientation: TransitionAnimationOrientation
            = this.configService.configuration.theme?.transitionAnimationOrientation
                ?? TransitionAnimationOrientation.Horizontal;
        return orientation == TransitionAnimationOrientation.Vertical
            ? TransitionState.Up
            : TransitionState.Left;
    }

    private getForwardsTransitionState(): TransitionState {
        const orientation: TransitionAnimationOrientation
            = this.configService.configuration.theme?.transitionAnimationOrientation
                ?? TransitionAnimationOrientation.Horizontal;
        return orientation == TransitionAnimationOrientation.Vertical
            ? TransitionState.Down
            : TransitionState.Right;
    }

    /**
     * The [@.disabled] attribute does not seem to be stopping angular from running animations on first load.
     * This allows us to manually stop it from triggering our workflow transitions when it shouldn't.
     * @param $event 
     * 
     * TODO: move this to WebFormComponent since it's the only one which uses it?
     */
    public onAnimationDone($event: any): void {
        if (!this.disableAnimations) {
            this.onCompletedTransition();
        }
    }

    /**
     * Called when an animation has completed due to a transition from one step to another.
     * This will be called twice. First when the animate out completes, and second when the animate in completes.
     * @param index 
     */
    public async onCompletedTransition(index?: number): Promise<void> {
        this.completedTransitionSubject.next();
        if (this.transitioningOut) {
            this.transitioningOut = false;
            await this.onChangeStepAfterAnimateOut(
                this.workflowService.currentNavigation.from,
                this.workflowService.currentNavigation.to);
            setTimeout(
                async () => {
                    await this.onChangeStepBeforeAnimateIn(
                        this.workflowService.currentNavigation.from,
                        this.workflowService.currentNavigation.to);
                    this.transitionState = this.transitionDirection == NavigationDirection.Backward
                        ? this.getBackwardsTransitionState()
                        : this.getForwardsTransitionState();
                    this.transitionStateSubject.next(this.transitionState);
                    this.transitionInNextStep();
                },
                0);
        } else if (this.transitioningIn) {
            this.transitioningIn = false;
            await this.onChangeStepAfterAnimateIn(
                this.workflowService.currentNavigation?.from,
                this.workflowService.currentNavigation?.to);
        }
    }

    protected transitionInNextStep(): void {
        setTimeout(
            () => {
                this.transitioningIn = true;
                this.transitionState = TransitionState.None;
                this.transitionStateSubject.next(this.transitionState);
                if (this.workflowService.skipWorkflowAnimations) {
                    this.onCompletedTransition();
                }
            },
            this.transitionDelayBetweenMs);
    }

    protected async onChangeStepBeforeAnimateOut(
        previousDestination: WorkflowDestination,
        nextDestination: WorkflowDestination,
    ): Promise<void> {
    }

    protected async onChangeStepAfterAnimateOut(
        previousDestination: WorkflowDestination,
        nextDestination: WorkflowDestination,
    ): Promise<void> {
    }

    protected async onChangeStepBeforeAnimateIn(
        previousDestination: WorkflowDestination,
        nextDestination: WorkflowDestination,
    ): Promise<void> {
    }

    protected async onChangeStepAfterAnimateIn(
        previousDestination: WorkflowDestination,
        nextDestination: WorkflowDestination,
    ): Promise<void> {
    }
}
