import { debounceTime, takeUntil, throttleTime } from 'rxjs/operators';
import {
    Component, OnInit, HostListener, EventEmitter, Output, AfterViewInit, ElementRef, HostBinding,
    OnDestroy,
    DoCheck,
} from '@angular/core';
import { trigger, state, style, animate, transition } from '@angular/animations';
import { DynamicWidget } from '../dynamic.widget';
import { FormService } from '@app/services/form.service';
import { ConfigService } from '@app/services/config.service';
import { MessageService } from '@app/services/message.service';
import { WorkflowService } from '@app/services/workflow.service';
import { CalculationService } from '@app/services/calculation.service';
import { WindowScrollService } from '@app/services/window-scroll.service';
import { BroadcastService } from '@app/services/broadcast.service';
import { asyncScheduler, Subject, Subscription } from 'rxjs';
import { Expression } from '@app/expressions/expression';
import { ExpressionDependencies } from '@app/expressions/expression-dependencies';
import { ApplicationService } from '@app/services/application.service';
import { FieldType } from '@app/models/field-type.enum';
import { WorkflowDestination } from '@app/models/workflow-destination';
import { WorkflowStep } from '@app/models/configuration/workflow-step';
import { WorkflowDestinationService } from '@app/services/workflow-destination.service';
import { SidebarOffsetService } from '@app/services/sidebar-offset.service';
import { EventService } from '@app/services/event.service';
import { ArrayHelper } from '@app/helpers/array.helper';
import * as _ from 'lodash-es';
import { BrowserDetectionService } from '@app/services/browser-detection.service';
import { TransitionState } from '@app/models/transition-state.enum';

/**
 * An aside is rendered in the right hand column.
 */
export interface Aside {
    type: string;
    name: string;
    render: boolean;
    hiddenExpression: Expression;
}

/**
 * Export sidebar widget component class.
 * TODO: Write a better class header: sidebar widget to dispay the module assigned.
 */
@Component({
    selector: 'sidebar-widget',
    templateUrl: './sidebar.widget.html',
    animations: [
        trigger('actionsSlide', [
            state(TransitionState.Left, style({ transform: 'translateX(0)' })),
            state(TransitionState.None, style({ transform: 'translateX(0)' })),
            state(TransitionState.Right, style({ transform: 'translateX(0)' })),
            transition(`* => ${TransitionState.None}`, animate('10ms')),
            transition(`${TransitionState.None} => *`, animate('10ms')),
        ]),
        trigger('sidebarFade', [
            state('hidden', style({ opacity: 0 })),
            state('visible', style({ opacity: 1 })),
            state('start', style({ opacity: 1 })),
            transition('* => visible', animate('800ms ease')),
            transition('visible => *', animate('100ms ease')),
            transition('* => start', animate('0ms ease')),
        ]),
    ],
    styleUrls: [
        './sidebar.widget.scss',
    ],
})

export class SidebarWidget extends DynamicWidget implements OnInit, OnDestroy, AfterViewInit, DoCheck  {

    /**
     * Instead of using "@media (max-width: 767px) {" style queries, we instead
     * work off the following css classes to know if we're in mobile size.
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

    @HostBinding('class.scrolling')
    public scrolling: boolean = true;

    @HostBinding('class.scrolling-up')
    public scrollingUp: boolean = false;

    @HostBinding('class.scrolling-down')
    public scrollingDown: boolean = false;

    @Output() public asideCount: EventEmitter<any> = new EventEmitter<any>();

    public asides: Array<Aside>;

    public transitionDelayBeforeMs: number = 400;
    public transitionDelayBetweenMs: number = 300;

    protected mobileKeyboardVisible: boolean;

    /**
     * The number of pixels the window needs to scroll before the sidebar can be hidden on mobile when fields
     * get the focus.
     */
    private scrollHideThresholdPixels: number = 80; // px

    /**
     * We want this set to 0 so there is no wasted space at the top on mobile:
     */
    protected scrollMarginTopDesktop: number = 20; // px
    protected scrollMarginBottom: number = 20; // px

    protected lastScrollEventTimestamp: number;

    protected scrollSpeed: number = 1; // 0.5 is slow, 1 is medium, 5 is fast
    protected scrollFinalOffset: number;
    protected scrollOffsetProperty: any;
    protected scrollIntervalMs: number = 10;
    protected scrollTimeoutId: any;
    private hostElement: HTMLElement;
    private sidebarDivElement: HTMLElement;
    private columnSetElement: HTMLElement;
    private formColumnElement: HTMLElement;
    private sidebarHeight: number;
    private sidebarBottom: number;

    public hideState: any = 'visible';
    protected focused: boolean = false;
    private focusedFieldType: FieldType;
    protected fieldTypesThatHideResponsivePanel: Array<FieldType> = [
        FieldType.SingleLineText,
        FieldType.Currency,
        FieldType.TextArea,
        FieldType.DatePicker,
        FieldType.Password,
        FieldType.DropDownSelect,
        FieldType.SearchSelect,
    ];

    private broadcast$: Subscription;

    public isIos: boolean;
    public isAndroid: boolean;
    public isPortrait: boolean;

    public verticalScrollSubject: Subject<number> = new Subject<number>();
    public scrollDownToPositionSubject: Subject<number> = new Subject<number>();

    private divElement: HTMLElement;
    private appMinHeight: number;
    private resizeObserver: ResizeObserver;

    private sidebarFollowsOnMobile: boolean = true;
    private sidebarFollowsOnDesktop: boolean = true;

    public constructor(
        protected windowScroll: WindowScrollService,
        protected message: MessageService,
        protected calculationService: CalculationService,
        protected workflowService: WorkflowService,
        protected configService: ConfigService,
        protected formService: FormService,
        protected broadcast: BroadcastService,
        protected expressionDependencies: ExpressionDependencies,
        private applicationService: ApplicationService,
        workflowDestinationService: WorkflowDestinationService,
        private elementRef: ElementRef,
        private sidebarOffsetService: SidebarOffsetService,
        private eventService: EventService,
        private browserDetectionService: BrowserDetectionService,
    ) {
        super(workflowService, configService, workflowDestinationService);
    }

    public ngOnInit(): void {
        super.ngOnInit();
        this.formService.formFocus.pipe(
            takeUntil(this.destroyed))
            .subscribe((fieldType: FieldType) => {
                this.onFormFocus(fieldType);
            });
        this.formService.fieldBlur.pipe(
            takeUntil(this.destroyed))
            .subscribe((fieldType: FieldType) => {
                this.onFormBlur(fieldType);
            });
        this.message.windowResize.pipe(
            takeUntil(this.destroyed))
            .subscribe((data: string) => {
            });
        this.applicationService.calculationInProgressSubject.pipe(
            takeUntil(this.destroyed))
            .subscribe((inProgress: boolean) => {
                this.onPremiumCalculation(inProgress);
            });

        if (this.broadcast$) {
            this.broadcast$.unsubscribe();
        }
        this.broadcast$ = this.broadcast.on('ErrorPromptEvent').subscribe(() => {
            this.updateScrollPosition();
        });
        this.isIos = this.browserDetectionService.isIos;
        this.isAndroid = navigator.userAgent.match(/android/i) && navigator.userAgent.match(/android/i).length > 0;
        const screen: Screen = window.screen;
        this.isPortrait = screen?.orientation?.angle == 0 || screen?.orientation?.angle == 180;

        this.verticalScrollSubject.pipe(
            throttleTime(this.scrollIntervalMs, asyncScheduler, { leading: false, trailing: true }),
        ).subscribe((verticalScrollAmountPixels: number) => this.onScroll(verticalScrollAmountPixels));

        this.generateAsides(this.workflowService.currentDestination);
        this.onWindowResize(null);
        this.hostElement = this.elementRef.nativeElement;
        this.sidebarDivElement = document.getElementById('sidebar');
        this.columnSetElement = document.getElementById('column-set');
        this.formColumnElement = document.getElementById('section-column-ubind');
        this.scrollDownToPositionDebounced();
    }

    public ngOnDestroy(): void {
        this.eventService.sidebarHeightSubject.next(0);
    }

    public ngDoCheck(): void {
        this.publishSidebarHeight();
    }

    private onConfigurationUpdated(): void {
        this.eventService.updatedConfiguration.pipe(takeUntil(this.destroyed))
            .subscribe(() => {
                this.generateAsides(this.workflowService.currentDestination);
            });
    }

    public ngAfterViewInit(): void {
        super.ngAfterViewInit();
        const sidebarWidgetElement: HTMLElement = this.elementRef.nativeElement;
        this.divElement = <HTMLElement>sidebarWidgetElement.firstChild;
        if (ResizeObserver) {
            this.resizeObserver = new ResizeObserver(this.publishSidebarHeight.bind(this));
            if (this.resizeObserver) {
                this.resizeObserver.observe(this.divElement);
            }
        }
        this.publishSidebarHeight();
        this.onConfigurationUpdated();
    }

    /**
     * Set the minimum height on the app element so the sidebar doesn't get cut off.
     * This is the height starting from the top of the page to the bottom of the sidebar
     */
    public publishSidebarHeight(): void {
        const bounds: DOMRect = this.sidebarDivElement.getBoundingClientRect() as DOMRect;
        this.sidebarHeight = bounds.height;
        this.sidebarBottom = bounds.bottom;
        this.eventService.sidebarHeightSubject.next(this.sidebarHeight);
        this.eventService.sidebarBottomSubject.next(this.sidebarBottom);
    }

    protected onFormFocus(fieldType: FieldType): void {
        this.focused = true;
        this.focusedFieldType = fieldType;
        this.updateHideStateForFocus();
    }

    private updateHideStateForFocus(isScrolling: boolean = false): void {
        let sidebarOffset: number = this.sidebarDivElement
            ? Number.parseInt(this.sidebarDivElement.style.top, 10)
            : 0;
        sidebarOffset = this.scrollFinalOffset;
        if (this.isMobileWidth
            && !isScrolling
            && this.focused
            && this.fieldTypesThatHideResponsivePanel.indexOf(this.focusedFieldType) != -1
            && sidebarOffset >= this.scrollHideThresholdPixels
        ) {
            this.hideState = 'hidden';
        } else if (sidebarOffset < this.scrollHideThresholdPixels) {
            this.hideState = 'start';
        } else {
            this.hideState = 'visible';
        }
    }

    protected onFormBlur(fieldType: FieldType): void {
        if (this.isMobileWidth
            && this.fieldTypesThatHideResponsivePanel.indexOf(fieldType) != -1
            && this.focused
        ) {
            if (this.hideState != 'visible') {
                setTimeout(() => {
                    if (!this.focused) {
                        this.hideState = 'visible';
                    }
                }, 100);
            }
            this.focused = false;
        }
    }

    protected onPremiumCalculation(inProgress: boolean): void {
        if (this.isMobileWidth && inProgress && !this.focused
        ) {
            setTimeout(() => {
                this.hideState = 'visible';
            }, 100);
        }
    }

    protected async onChangeStepBeforeAnimateIn(
        previousDestination: WorkflowDestination,
        nextDestination: WorkflowDestination,
    ): Promise<void> {
        this.generateAsides(nextDestination);
    }

    protected generateAsides(workflowDestination: WorkflowDestination): Array<any> {
        if (!workflowDestination) {
            return;
        }
        let asides: Array<Aside> = new Array<Aside>();
        const workflowStep: WorkflowStep =
            this.configService.workflowSteps[workflowDestination.stepName];
        if (workflowStep) {
            let asideDefinitions: any = workflowStep.sidebar;
            if (asideDefinitions) {
                for (let asideDefinition of asideDefinitions) {
                    let aside: Aside = <Aside>{
                        type: asideDefinition.type,
                        name: asideDefinition.name,
                        render: true,
                    };
                    const hiddenExpressionSource: any = asideDefinition.hiddenExpression ?
                        asideDefinition.hiddenExpression : asideDefinition.hidden;
                    if (hiddenExpressionSource) {
                        aside.render = false;
                        let hiddenExpression: Expression = new Expression(hiddenExpressionSource,
                            this.expressionDependencies, aside.name + ' hidden');
                        hiddenExpression.nextResultObservable.pipe(takeUntil(this.destroyed))
                            .subscribe((result: boolean) => {
                                aside.render = !result;
                            });
                        hiddenExpression.triggerEvaluationWhenFormLoaded();
                        aside.hiddenExpression = hiddenExpression;
                    }
                    asides.push(aside);
                }
            }
        }
        if (!this.asides) {
            this.asides = asides;
        } else {
            ArrayHelper.synchronise<Aside>(
                asides,
                this.asides,
                _.isEqual,
                (item: Aside): void => {
                    if (item.hiddenExpression) {
                        item.hiddenExpression.dispose();
                    }
                });
        }
        this.asideCount.emit(this.asides.length);
    }

    public getUniqueIdentifier(index: number, item: Aside): any {
        return `${item.type}, ${item.name}, ${item.render}`;
    }

    @HostListener("window:resize", ['$event'])
    protected onWindowResize(event: any): void {
        if (this.columnSetElement && this.sidebarDivElement) {
            this.columnSetElement.style.minHeight = '' + this.sidebarDivElement.scrollHeight + 'px';
        }

        const wasMobileWidth: boolean = this.isMobileWidth;
        this.isMobileWidth = this.browserDetectionService.isMobileWidth();
        this.isWiderThanMobile = !this.isMobileWidth;

        if (wasMobileWidth !== this.isMobileWidth) {
            this.updateHideStateForFocus(true);
        }

        if (!this.canSidebarMove()) {
            if (this.sidebarDivElement) {
                this.sidebarDivElement.style.top = '0';
            }
            return;
        }
    }

    @HostListener("window:message", ['$event'])
    public onMessage(event: any): void {
        const screen: Screen = window.screen;
        this.isPortrait = screen?.orientation?.angle == 0 || screen?.orientation?.angle == 180;
        if (event.data.messageType == 'scroll' || event.data.messageType == 'resize') {
            this.recalculateSidebarOffSet(event.data.payload.verticalScrollAmountPixels);
        }
    }

    private recalculateSidebarOffSet(verticalScrollAmountPixels: number): void {
        if (!this.canSidebarMove()) {
            this.sidebarDivElement.style.top = '0';
            return;
        }
        this.verticalScrollSubject.next(verticalScrollAmountPixels);
    }

    protected onScroll(verticalScrollAmountPixels: number): void {
        this.publishSidebarHeight();
        this.scrollFinalOffset = this.calculateNewPosition(verticalScrollAmountPixels);
        let currentOffset: number = Number.parseInt(this.sidebarDivElement.style.top, 10);
        this.scrollingUp = currentOffset > this.scrollFinalOffset;
        this.scrollingDown = currentOffset < this.scrollFinalOffset;
        this.scrolling = currentOffset != this.scrollFinalOffset;

        if (this.isMobileWidth) {
            this.updateHideStateForFocus(true);
            if (this.scrollFinalOffset > this.sidebarHeight) {
                this.sidebarDivElement.style.visibility = 'hidden';
                setTimeout(() => this.scrollDownToPositionSubject.next(this.scrollFinalOffset), 0);
            } else {
                this.sidebarDivElement.style.visibility = 'visible';
                this.sidebarDivElement.style.top = '0px';
            }
        } else {
            this.lastScrollEventTimestamp = Date.now();
            clearTimeout(this.scrollTimeoutId);
            this.scrollTimeoutId = setTimeout(() => this.updateScrollPosition(), this.scrollIntervalMs);
        }
    }

    private scrollDownToPositionDebounced(): void {
        this.scrollDownToPositionSubject.pipe(
            takeUntil(this.destroyed),
            debounceTime(800),
        ).subscribe((position: number) => {
            if (!this.canSidebarMove()) {
                this.sidebarDivElement.style.top = '0';
                return;
            }
            this.sidebarDivElement.style.transition = 'none';
            this.sidebarDivElement.style.top = Math.max(this.scrollFinalOffset - 200, 0) + 'px';
            setTimeout(() => {
                this.sidebarDivElement.style.visibility = 'visible';
                this.sidebarDivElement.style.transition = 'top 800ms ease 0ms';
                this.sidebarDivElement.style.top = this.scrollFinalOffset + 'px';
                setTimeout(() => this.sidebarDivElement.style.transition = 'none', 800);
            }, 0);
        });
    }

    protected calculateNewPosition(verticalScrollAmountPixels: number): number {
        if (!this.canSidebarMove()) {
            this.sidebarDivElement.style.top = '0';
            return;
        }
        let formColumnElementPosition: number = this.windowScroll.getElementPosition(this.formColumnElement);
        let minOffset: number = 0;
        let offsetToVisibleContentStart: number = this.sidebarOffsetService.getOffsetTop();
        if (this.isMobileWidth) {
            offsetToVisibleContentStart += this.sidebarHeight;
        }

        let maxOffset: number;
        let wantedOffset: number;
        if (this.isMobileWidth) {
            maxOffset = this.formColumnElement.scrollHeight;
            wantedOffset = (verticalScrollAmountPixels - formColumnElementPosition) + offsetToVisibleContentStart;
        } else {
            maxOffset = (this.formColumnElement.scrollHeight - this.sidebarDivElement.scrollHeight) -
                this.scrollMarginBottom;
            wantedOffset = (verticalScrollAmountPixels - formColumnElementPosition) + this.scrollMarginTopDesktop
                + offsetToVisibleContentStart;
        }
        const result: number = Math.max(minOffset, Math.min(maxOffset, Math.round(wantedOffset)));
        return result;
    }

    protected updateScrollPosition(): void {
        if (!this.canSidebarMove()) {
            this.sidebarDivElement.style.top = '0';
            return;
        }
        let elapsedSinceLastEvent: number = Date.now() - this.lastScrollEventTimestamp;
        let timeBasedEffect: number = elapsedSinceLastEvent / this.scrollIntervalMs;
        let currentOffsetString: string = this.sidebarDivElement.style.top;
        currentOffsetString = currentOffsetString ?
            currentOffsetString.substring(0, currentOffsetString.indexOf('px')) : '';
        let currentOffsetPixels: number = isNaN(parseFloat(currentOffsetString)) ? 0 : parseFloat(currentOffsetString);
        let scrollSpeed: number = this.scrollSpeed / 10;
        let newOffset: number = (currentOffsetPixels +
            (this.scrollFinalOffset * scrollSpeed * timeBasedEffect)) / (1 + (scrollSpeed * timeBasedEffect));
        if (Math.abs(newOffset - this.scrollFinalOffset) < 1) {
            newOffset = this.scrollFinalOffset;
        }

        this.sidebarDivElement.style.top = newOffset + 'px';

        if (newOffset != this.scrollFinalOffset) {
            this.lastScrollEventTimestamp = Date.now();
            this.scrollTimeoutId = setTimeout(
                () => {
                    this.updateScrollPosition();
                },
                this.scrollIntervalMs);
        }
    }

    private canSidebarMove(): boolean {
        return this.isMobileWidth && this.sidebarFollowsOnMobile
            || !this.isMobileWidth && this.sidebarFollowsOnDesktop;
    }
}
