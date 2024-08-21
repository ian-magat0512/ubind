import {
    Component, ViewChild, OnInit, ElementRef, OnDestroy, AfterViewInit, HostListener, Renderer2, Input, SimpleChanges,
    OnChanges,
} from '@angular/core';
import { createPopper, Instance, Options, Placement } from '@popperjs/core';
import { fromEvent } from 'rxjs';
import { debounceTime, filter, takeUntil } from 'rxjs/operators';
import { ToolTipService } from '@app/services/tooltip.service';
import { TextWithExpressions } from '@app/expressions/text-with-expressions';
import { ExpressionDependencies } from '@app/expressions/expression-dependencies';
import { Widget } from '../widget';

// eslint-disable-next-line no-unused-vars
declare let $: any; // please don't tamper with this line

export enum IconPosition {
    Before = 'before',
    After = 'after'
}

/**
 * Export tooltip wrapper component class.
 * This class contains configuration of the Tooltip.
 * It is used to handle tooltip hide and show and flipping functionality. 
 */
@Component({
    selector: 'ubind-tooltip-widget-ng',
    templateUrl: './tooltip.widget.html',
    styleUrls: ['./tooltip.widget.scss'],
})

export class TooltipWidget extends Widget implements OnInit, OnDestroy, AfterViewInit, OnChanges {

    @ViewChild('tooltipTarget') private tooltipTarget: ElementRef<HTMLAnchorElement>;
    @ViewChild('tooltip') private tooltip: ElementRef<HTMLDivElement>;
    @ViewChild('arrow') private arrow: ElementRef<HTMLAnchorElement>;

    @Input('content')
    public contentExpressionSource: string;

    @Input('show-icon')
    public showIcon: string;

    @Input('icon')
    public customIcon: string;

    @Input('icon-position')
    public iconPosition: IconPosition;

    // eslint-disable-next-line @typescript-eslint/naming-convention
    public IconPosition: typeof IconPosition = IconPosition;
    public tooltipPlacement: Placement = 'top';
    public popperInstance: Instance;
    private isToolTipClicked: boolean = false;
    public toolTipIn: boolean = false;
    public toolTipOut: boolean = true;
    private verticalScrollAmountPixels: number;
    private mobileSidebarScrollHeight: number;
    private contentExpression: TextWithExpressions;
    public tooltipContent: string;
    private columnSetElement: HTMLElement;
    public constructor(
        public tooltipService: ToolTipService,
        private expressionDependencies: ExpressionDependencies,
        private renderer: Renderer2,
    ) {
        super();
    }

    public ngOnInit(): void {
        super.ngOnInit();
        this.setupContentExpression();
        this.verticalScrollAmountPixels = 0;
        this.mobileSidebarScrollHeight = 0;
        fromEvent(window, 'message')
            .pipe(
                filter((event: any) => event.data.messageType == 'scroll'),
                debounceTime(200),
                takeUntil(this.destroyed),
            )
            .subscribe((event: any) => {
                this.verticalScrollAmountPixels = event.data.payload.scrollOffset;

                if (window.innerWidth < 768) {
                    this.mobileSidebarScrollHeight = document.getElementById('sidebar').scrollHeight;
                }

                this.updateTooltipPlacement();
            });

        this.tooltipService.toolTipChangedSubject.subscribe(() => this.hideToolTip());
    }

    public ngOnChanges(changes: SimpleChanges): void {
        if (changes.contentExpressionSource) {
            this.setupContentExpression();
        }
    }

    public ngAfterViewInit(): void {
        this.createPopperInstance();
        this.columnSetElement = document.getElementById("column-set");
        if (this.columnSetElement) {
            this.renderer.appendChild(this.columnSetElement, this.tooltip.nativeElement);
        }
    }

    private createPopperInstance(): void {
        if (this.tooltipTarget && this.tooltip.nativeElement) {
            const boundaryElement: HTMLElement = document.getElementById("column-set");
            const defaultConfig: Partial<Options> = {
                placement: this.tooltipPlacement,
                modifiers: [
                    {
                        name: "offset",
                        options: {
                            offset: [0, 8],
                        },
                    },
                    {
                        name: 'preventOverflow',
                        options: {
                            boundary: boundaryElement,
                        },
                    },
                ],
            };

            this.popperInstance = createPopper(
                this.tooltipTarget.nativeElement, this.tooltip.nativeElement, defaultConfig);
        }
    }

    @HostListener('document:click', ['$event'])
    public documentClick(event: MouseEvent): void {
        const className: any = (event.target as Element).className;
        const clickOutSideTooltip: boolean = (typeof className === 'string')
            && (className.indexOf("tooltip-anchor") == -1 && className.indexOf("tooltip-icon") == -1);
        if (clickOutSideTooltip) {
            this.isToolTipClicked = false;
            this.hideToolTip();
        }
    }

    public mouseClick(): void {
        this.tooltipService.toolTipChange();
        if (this.isToolTipClicked) {
            this.isToolTipClicked = false;
        } else {
            this.isToolTipClicked = true;
            this.showToolTip();
        }
        this.updateTooltipPlacement();
    }

    public mouseEnter(): void {
        this.tooltipService.toolTipChange();
        this.showToolTip();
        this.updateTooltipPlacement();
    }

    public mouseLeave(): void {
        if (!this.isToolTipClicked) {
            this.hideToolTip();
        }
        this.updateTooltipPlacement();
    }

    public showToolTip(): void {
        this.toolTipIn = true;
        this.toolTipOut = false;
    }

    public hideToolTip(): void {
        this.toolTipIn = false;
        this.toolTipOut = true;
    }

    public ngOnDestroy(): void {
        this.popperInstance?.destroy();
        super.ngOnDestroy();
    }

    protected destroyExpressions(): void {
        this.contentExpression?.dispose();
        this.contentExpression = null;
    }

    private updateTooltipPlacement(): void {
        if (this.tooltipTarget) {
            const arrowHeight: number = (<DOMRect>this.arrow.nativeElement.getBoundingClientRect()).height;
            const tooltipTargetYOffset: number = (<DOMRect>this.tooltipTarget.nativeElement.getBoundingClientRect()).y;
            const tooltipHeight: number = (<DOMRect>this.tooltip.nativeElement.getBoundingClientRect()).height;
            const tooltipTop: number = tooltipTargetYOffset - this.mobileSidebarScrollHeight
                - tooltipHeight - this.verticalScrollAmountPixels - arrowHeight;

            if (tooltipTop < 0) {
                this.popperInstance.setOptions({ placement: "bottom" });
            } else {
                this.popperInstance.setOptions({ placement: "top" });
            }

            this.popperInstance.update();
            this.tooltipPlacement = this.popperInstance.state.placement;
        }
    }

    protected setupContentExpression(): void {
        if (this.contentExpression) {
            this.contentExpression.dispose();
            this.contentExpression = null;
        }
        if (this.contentExpressionSource) {
            this.contentExpression = new TextWithExpressions(
                this.contentExpressionSource,
                this.expressionDependencies,
                `tooltip widget text`);
            this.contentExpression.nextResultObservable.pipe(takeUntil(this.destroyed))
                .subscribe((content: string) => this.tooltipContent = content);
            this.contentExpression.triggerEvaluation();
        } else {
            this.tooltipContent = null;
        }
    }
}
