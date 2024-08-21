import { catchError, take, takeUntil, timeout } from 'rxjs/operators';
import {
    Component, OnDestroy, ViewChild,
    ChangeDetectorRef, OnInit, AfterViewInit, ViewChildren, QueryList, HostListener, HostBinding,
} from '@angular/core';
import { ConfigProcessorService } from '../../services/config-processor.service';
import { SidebarWidget } from '../widgets/sidebar/sidebar.widget';
import { MessageService } from '../../services/message.service';
import { EventService } from '@app/services/event.service';
import { WorkflowService } from '@app/services/workflow.service';
import { RenderMethod } from '@app/models/render-method.enum';
import { DynamicWidget } from '../widgets/dynamic.widget';
import { ConfigService } from '@app/services/config.service';
import { animate, state, style, transition, trigger } from '@angular/animations';
import { WindowScrollService } from '@app/services/window-scroll.service';
import { Subject, of } from 'rxjs';
import { ApplicationService } from '@app/services/application.service';
import { SectionWidget } from '../widgets/section/section.widget';
import { WorkflowStatusService } from '@app/services/workflow-status.service';
import { WorkflowDestination } from '@app/models/workflow-destination';
import { WorkflowDestinationService } from '@app/services/workflow-destination.service';
import { ArrayHelper } from '@app/helpers/array.helper';
import { WorkflowStep } from '@app/models/configuration/workflow-step';
import { BrowserDetectionService } from '@app/services/browser-detection.service';
import { FormType } from '@app/models/form-type.enum';
import { Expression } from '@app/expressions/expression';
import { ExpressionDependencies } from '@app/expressions/expression-dependencies';
import { TransitionState } from '@app/models/transition-state.enum';
import { AlertService } from '@app/services/alert.service';
import { Alert } from '@app/models/alert';

/**
 * Represents a section widget within the web form, which renders a page or step.
 */
export interface Section {
    stepName: string;
    render: boolean;
    definition: WorkflowStep;
}

/**
 * Export web form component class.
 * TODO: Write a better class header: webForm functions.
 */
@Component({
    selector: 'web-form',
    templateUrl: './web-form.html',
    animations: [
        trigger('sectionSlide', [
            state(TransitionState.Up, style({ transform: 'translateY(-25px)' })),
            state(TransitionState.Left, style({ transform: 'translateX(-25px)' })),
            state(TransitionState.None, style({ transform: 'translateX(0), translateY(0)' })),
            state(TransitionState.Right, style({ transform: 'translateX(25px)' })),
            state(TransitionState.Down, style({ transform: 'translateY(25px)' })),
            transition(`* => ${TransitionState.None}`, animate('500ms 500ms ease-out')),
            transition(`${TransitionState.None} => *`, animate('300ms 100ms ease-out')),
        ]),
        trigger('sectionFade', [
            state(TransitionState.Up, style({ opacity: 0 })),
            state(TransitionState.Left, style({ opacity: 0 })),
            state(TransitionState.None, style({ opacity: 100 })),
            state(TransitionState.Right, style({ opacity: 0 })),
            state(TransitionState.Down, style({ opacity: 0 })),
            transition(`* => ${TransitionState.None}`, animate('500ms 500ms ease-out')),
            transition(`${TransitionState.None} => *`, animate('300ms 100ms ease-out')),
        ]),
        trigger('loaderAnimate', [
            state('show', style({ opacity: 1, height: '40px' })),
            state('hide', style({ opacity: 0, height: 0 })),
            transition('* => *', animate('1000ms 1000ms ease-out')),
        ]),
    ],
})

export class WebFormComponent extends DynamicWidget implements OnInit, AfterViewInit, OnDestroy {

    @ViewChild(SidebarWidget)
    protected sidebarWidget: SidebarWidget;

    @ViewChildren(SectionWidget)
    protected sectionWidgets: QueryList<SectionWidget>;

    @HostBinding('class') public classes: string;

    public sidebarPresentClass: string = 'sidebar-not-present';
    public isIos: boolean;

    public sections: Array<Section> = new Array<Section>();

    public renderMethod: RenderMethod = RenderMethod.Lazy;
    // eslint-disable-next-line @typescript-eslint/naming-convention
    public RenderMethod: typeof RenderMethod = RenderMethod;
    public showLoader: boolean = false;
    private loaderRenderedSubject: Subject<boolean> = new Subject<boolean>();
    public sidebarWidthPixels: number = -1;

    public actionInProgress: boolean = false;

    /**
     * Instead of using "@media (max-width: 767px) {" style queries, we instead
     * work off the following css classes to know if we're in mobile size.
     * The reasons for this are:
     * 1. The mobile breakpoint is set by the form configuration.
     * 2. When loading the webform in the portal, the viewport width returned by the media query
     * doesn't include the scroll bar, which it normally would. Using our css classes "mobile-size"
     * and "wider-than-mobile" ensures the scrollbar width is taken into account.
     */
    public isMobileWidth: boolean;
    public isSidebarPresent: boolean;

    /**
     * The app element, so we can control it's height
     */
    private appElement: HTMLElement;

    public constructor(
        protected configProcessor: ConfigProcessorService,
        protected messageService: MessageService,
        protected eventService: EventService,
        protected configService: ConfigService,
        protected workflowService: WorkflowService,
        private windowScrollService: WindowScrollService,
        private changeDetectorRef: ChangeDetectorRef,
        public applicationService: ApplicationService,
        public workflowStatusService: WorkflowStatusService,
        protected workflowDestinationService: WorkflowDestinationService,
        private browserDetectionService: BrowserDetectionService,
        private expressionDependencies: ExpressionDependencies,
        private alertService: AlertService,
    ) {
        super(workflowService, configService, workflowDestinationService);
        this.isIos = browserDetectionService.isIos;
    }

    public ngOnInit(): void {
        this.appElement = document.getElementById('app');
        super.ngOnInit();
        this.generateSections();
        this.publishWorkflowTransitionStateChanges();
        this.onConfigurationUpdated();
        this.applySidebarWidthStyle();
        this.setupStateExpression();
        this.listenForActionInProgress();
    }

    private listenForActionInProgress(): void {
        this.workflowStatusService.actionInProgressSubject.pipe(takeUntil(this.destroyed))
            .subscribe((actionInProgress: boolean) => {
                this.actionInProgress = actionInProgress;
            });
    }

    private setupStateExpression(): void {
        let expressionSource: string = 'hasActiveTrigger() + getActiveTriggerType()';
        if (this.applicationService.formType == FormType.Quote) {
            expressionSource += ' + getQuoteCalculationState() + getQuoteState()';
        } else {
            expressionSource += ' + getClaimCalculationState() + getClaimState()';
        }

        let stateExpression: Expression = new Expression(
            expressionSource,
            this.expressionDependencies,
            'web form state expression');
        stateExpression.nextResultObservable
            .pipe(takeUntil(this.destroyed)).subscribe(() => this.generateStateClasses());
        stateExpression.triggerEvaluation();
    }

    private generateStateClasses(): void {
        let classes: Array<string> = new Array<string>();
        if (this.applicationService.formType == FormType.Quote && this.applicationService.latestQuoteResult) {
            classes.push(`calculation-state-${this.applicationService.latestQuoteResult.calculationState}`);
            let quoteState: string = this.expressionDependencies.expressionMethodService.getQuoteState();
            classes.push(`quote-state-${quoteState}`);
        } else if (this.applicationService.formType == FormType.Claim && this.applicationService.latestClaimResult) {
            let claimState: string = this.expressionDependencies.expressionMethodService.getClaimState();
            classes.push(`calculation-state-${this.applicationService.latestClaimResult.calculationState}`);
            classes.push(`claim-state-${claimState}`);
        }
        if (this.expressionDependencies.expressionMethodService.hasActiveTrigger()) {
            classes.push('has-trigger');
        } else {
            classes.push('no-trigger');
        }

        const activeTriggerType: string =
            this.expressionDependencies.expressionMethodService.getActiveTriggerType();

        if (activeTriggerType) {
            classes.push(`has-${activeTriggerType}-trigger`);
        }

        this.classes = classes.join(' ');
    }

    private applySidebarWidthStyle(): void {
        const existingStyle: HTMLStyleElement = <HTMLStyleElement>document.getElementById('sidebar-width-style');
        if (existingStyle) {
            existingStyle.remove();
        }
        this.isMobileWidth = this.browserDetectionService.isMobileWidth();
        this.sidebarWidthPixels = this.configService.theme?.sidebarWidthPixels ?? -1;
        if (this.sidebarWidthPixels > -1) {
            const css: string
                = `.sidebar-present.wider-than-mobile #sidebar-column-ubind {\n`
                + `    width: ${this.sidebarWidthPixels}px;\n`
                + `}\n`
                + `.sidebar-present.wider-than-mobile #section-column-ubind {\n`
                + `    margin-right: ${this.sidebarWidthPixels + 15}px;\n`
                + `}\n`;
            const head: HTMLHeadElement = document.getElementsByTagName('head')[0];
            const style: HTMLStyleElement = document.createElement('style');
            style.id = 'sidebar-width-style';
            style.appendChild(document.createTextNode(css));
            head.appendChild(style);
        }
    }

    public ngAfterViewInit(): void {
        super.ngAfterViewInit();
        this.listenForChangesToActiveSectionWidget();
        this.sectionWidgets.notifyOnChanges();
        this.sidebarWidget.asideCount.pipe(takeUntil(this.destroyed))
            .subscribe((asideCount: any) => {
                this.updateSidebarPresentClass(asideCount > 0);
            });
        // we need to do this in a separate thread to avoid angular timing issues.
        // without the setTimeout we get ExpressionChangedAfterItHasBeenCheckedError during unit tests.
        setTimeout(() => {
            this.eventService.webFormLoadedSubject.next(true);
            this.configProcessor.sendWebFormLoadedMessage();
        }, 0);
    }

    private updateSidebarPresentClass(sidebarPresent: boolean): void {
        if (this.isSidebarPresent != sidebarPresent) {
            // we need to do this in a separate thread to avoid angular timing issues.
            // without the setTimeout we get ExpressionChangedAfterItHasBeenCheckedError during unit tests.
            setTimeout(() => this.isSidebarPresent = sidebarPresent, 0);
        }
    }

    public ngOnDestroy(): void {
        if (this.destroyed) {
            this.destroyed.next();
            this.destroyed.complete();
        }
    }

    public dismissKeyboard(e: any): void {
        let activeElement: HTMLElement = <HTMLElement>e.target;
        let elementTagName: string = activeElement.tagName;
        let isFormElement: boolean = elementTagName == 'INPUT' ||
            elementTagName == 'TEXTAREA' || elementTagName == 'SELECT' ||
            elementTagName == 'FORMLY-FORM';
        if (!this.isIos || isFormElement) {
            return;
        }
        activeElement.blur();
    }

    private generateSections(): void {
        let sections: Array<Section> = new Array<Section>();
        let sourceItems: any = this.configService.workflowSteps;
        for (let stepName in sourceItems) {
            let section: Section = {
                stepName: stepName,
                render: stepName == this.workflowService.currentDestination.stepName,
                definition: this.configService.workflowSteps[stepName],
            };
            sections.push(section);
        }
        ArrayHelper.synchronise<any>(
            sections,
            this.sections,
            (item1: Section, item2: Section): boolean => {
                return item1.stepName == item2.stepName
                    && item1.definition.displayMode == item2.definition.displayMode
                    && item1.definition.displayModeExpression == item2.definition.displayModeExpression
                    && item1.definition.startScreenExpression == item2.definition.startScreenExpression
                    && item1.definition.tabIndexExpression == item2.definition.tabIndexExpression;
            });
    }

    protected async onChangeStepBeforeAnimateOut(
        previousDestination: WorkflowDestination,
        nextDestination: WorkflowDestination,
    ): Promise<void> {
        // set the minimum height to the current height, so the page doesn't jump in size
        if (this.appElement) {
            this.appElement.style.minHeight = this.appElement.clientHeight + 'px';
        }
    }

    protected async onChangeStepAfterAnimateOut(
        previousDestination: WorkflowDestination,
        nextDestination: WorkflowDestination,
    ): Promise<void> {
        return new Promise((resolve: any, reject: any): void => {
            this.showLoader = true;
            let element: HTMLBodyElement = document.getElementsByTagName('body')[0];

            const onScrollingFinished: () => void = () => {
                if (previousDestination.stepName != nextDestination.stepName) {
                    for (let section of this.sections) {
                        section.render = false;
                    }
                }
                resolve();
            };

            this.eventService.scrollingFinishedSubject
                .pipe(
                    take(1),
                    timeout(10000),
                    catchError((err: any) => {
                        if (err.name === 'TimeoutError') {
                            onScrollingFinished();
                            this.alertService.alert(new Alert(
                                "Unable to scroll up to the top",
                                'After navigating pages, we attempt to scroll the page up to the top. '
                                + 'however this doesn\'t seemed to have happened within our timeout window '
                                + 'of 10 seconds. This can happen if you have loaded the web form without using '
                                + 'the ubind.js embed code or the portal, or it could happen if your device is slow '
                                + 'or overloaded.'));
                        }
                        return of(null);
                    }),
                )
                .subscribe((finished: boolean) => {
                    if (finished != null) {
                        onScrollingFinished();
                    }
                });
            this.windowScrollService.scrollElementIntoView(
                element,
                { notifyWhenScrollingFinished: true });
            // we do this in WebFormComponent since it's the main respondant to workflow steps.
            this.workflowService.completedNavigationOut();
        });
    }

    public onLoaderRendered(): void {
        this.loaderRenderedSubject.next(true);
    }

    public onArticleRendered(): void {
        this.showLoader = false;
    }

    protected async onChangeStepBeforeAnimateIn(
        previousDestination: WorkflowDestination,
        nextDestination: WorkflowDestination,
    ): Promise<void> {
        this.eventService.readyForNextStepSubject.next();
        return new Promise((resolve: any, reject: any): void => {
            if (previousDestination?.stepName != nextDestination.stepName) {
                for (let section of this.sections) {
                    section.render = nextDestination.stepName == section.stepName;
                }
                this.eventService.renderFooterSubject
                    .next(this.configService.workflowSteps[nextDestination.stepName]?.footer !== undefined);
                this.eventService.renderHeaderSubject
                    .next(this.configService.workflowSteps[nextDestination.stepName]?.header !== undefined);
            }
            this.changeDetectorRef.detectChanges();
            resolve();
        });
    }

    protected async onChangeStepAfterAnimateIn(
        previousDestination: WorkflowDestination,
        nextDestination: WorkflowDestination,
    ): Promise<void> {
        this.showLoader = false;
        // We only need to do this in one place so the here in the main web form is the right place.
        this.workflowService.completedNavigationIn();

        // clear the minimum height we set at the start of navigation, or reset it to the default
        if (this.appElement) {
            if (this.applicationService.embedOptions?.minimumHeight) {
                this.appElement.style.minHeight = this.applicationService.embedOptions.minimumHeight;
            } else {
                this.appElement.style.minHeight = null;
            }
        }
    }

    private publishWorkflowTransitionStateChanges(): void {
        this.transitionStateObservable.subscribe(this.eventService.pageAnimationTransitionStateSubject);
        this.completedTransitionObservable.subscribe(this.eventService.pageAnimationTransitionCompletedSubject);
    }

    private listenForChangesToActiveSectionWidget(): void {
        this.sectionWidgets.changes.pipe(takeUntil(this.destroyed))
            .subscribe((sectionWidgets: QueryList<SectionWidget>) => {
                for (let sectionWidget of sectionWidgets.toArray()) {
                    if (sectionWidget.stepName == this.applicationService.currentWorkflowDestination.stepName) {
                        if (this.workflowStatusService.currentSectionWidgetStatus != sectionWidget) {
                            this.workflowStatusService.currentSectionWidgetStatus = sectionWidget;
                            this.applicationService.sectionWidgetStatusSubject.next(sectionWidget);
                        }
                        return;
                    }
                }
            });
    }

    private onConfigurationUpdated(): void {
        this.eventService.updatedConfiguration.pipe(takeUntil(this.destroyed))
            .subscribe(() => {
                this.generateSections();
                const newSidebarWidthPixels: number = this.configService.theme?.sidebarWidthPixels ?? -1;
                if (this.sidebarWidthPixels != newSidebarWidthPixels) {
                    this.applySidebarWidthStyle();
                }
            });
    }

    @HostListener("window:resize", ['$event'])
    protected onWindowResize(e: any): void {
        this.isMobileWidth = this.browserDetectionService.isMobileWidth();
    }

    @HostListener('document:keydown.tab', ['$event'])
    @HostListener('document:keydown.shift.tab', ['$event'])
    public onFocusChangeKeydownHandler(event: KeyboardEvent): void {
        // Replacing the browser's default tab-handling addresses an issue 
        // with buggy focusing on a form field outside the viewport
        if (this.actionInProgress) {
            event.preventDefault();
            return;
        }
        event.preventDefault();
        let thisField: HTMLElement = event.target as HTMLElement;
        let fieldsArray: any = [].slice.call(
            document.querySelectorAll(
                'input:not([type="hidden"], [type="radio"]), select, button, textarea, .tabbable',
            ));
        let focusNextField: boolean = false;
        let index: number = event.shiftKey ? fieldsArray.length - 1 : 0;
        while (index >= 0 && index < fieldsArray.length) {
            if (fieldsArray[index].type != 'radio' &&
                fieldsArray[index].type != 'checkbox' &&
                !fieldsArray[index].disabled &&
                this.fieldIsVisible(fieldsArray[index])) {

                if (thisField.nodeName == 'BODY') {
                    fieldsArray[index].focus();
                    break;
                }

                if (focusNextField) {
                    fieldsArray[index].focus();
                    break;
                }
                if (fieldsArray[index] == thisField) {
                    focusNextField = true;
                }
            }
            index += event.shiftKey ? -1 : 1;
        }
    }

    private fieldIsVisible(el: any): boolean {
        let bounding: any = el.getBoundingClientRect();

        return bounding.boolean > 0 ||
            bounding.height > 0 ||
            bounding.left > 0 ||
            bounding.right > 0 ||
            bounding.top > 0 ||
            bounding.width > 0 ||
            bounding.x > 0 ||
            bounding.y > 0;
    }
}
