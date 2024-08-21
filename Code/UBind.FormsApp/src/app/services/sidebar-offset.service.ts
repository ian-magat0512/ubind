import { Injectable } from "@angular/core";
import { BreakpointOffset } from "@app/models/breakpoint-offset.model";
import { BootStrapBreakPointInfix as BootStrapClass } from "@app/models/bootstrap-breakpoint-infix.enum";
import { BootStrapBreakPoint } from "@app/models/bootstrap-breakpoint.model";
import { filter } from 'rxjs/operators';
import { ConfigService } from "./config.service";
import { ApplicationService } from "./application.service";
import { StringHelper } from "@app/helpers/string.helper";
import { BrowserDetectionService } from "./browser-detection.service";
import { EventService } from "./event.service";

/**
 * This class is used to calculate the sidebar offset of embedded web form.
 */
@Injectable({
    providedIn: 'root',
})
export class SidebarOffsetService {
    private bootstrapBreakpointsMap: Array<BootStrapBreakPoint> = [];
    private scrollOffsetTop: any = {};
    private bootStrapBreakpoints: Array<string> = [];
    private breakPointsSeparator: string = '|';
    private breakPointSeparator: string = ',';
    private sidebarHeight: number = 0;

    public constructor(
        protected configService: ConfigService,
        private applicationService: ApplicationService,
        private browserDetectionService: BrowserDetectionService,
        private eventService: EventService,
    ) {
        configService.configurationReadySubject.pipe(filter((ready: boolean) => ready))
            .subscribe(() => {
                if (this.configService.theme) {
                    this.scrollOffsetTop[BootStrapClass.Xs] = this.configService.theme.offsetTopExtraSmall;
                    this.scrollOffsetTop[BootStrapClass.Sm] = this.configService.theme.offsetTopSmall;
                    this.scrollOffsetTop[BootStrapClass.Md] = this.configService.theme.offsetTopMedium;
                    this.scrollOffsetTop[BootStrapClass.Lg] = this.configService.theme.offsetTopLarge;
                    this.scrollOffsetTop[BootStrapClass.Xl] = this.configService.theme.offsetTopExtraLarge;
                }
            });

        this.bootstrapBreakpointsMap = [
            <BootStrapBreakPoint>{ bootStrapClassName: BootStrapClass.Xs, breakPoint: 0 },
            <BootStrapBreakPoint>{ bootStrapClassName: BootStrapClass.Sm, breakPoint: 576 },
            <BootStrapBreakPoint>{ bootStrapClassName: BootStrapClass.Md, breakPoint: 768 },
            <BootStrapBreakPoint>{ bootStrapClassName: BootStrapClass.Lg, breakPoint: 992 },
            <BootStrapBreakPoint>{ bootStrapClassName: BootStrapClass.Xl, breakPoint: 1200 },
            <BootStrapBreakPoint>{ bootStrapClassName: BootStrapClass.Xxl, breakPoint: 1400 },

        ];

        this.bootStrapBreakpoints = [
            BootStrapClass.Xs,
            BootStrapClass.Sm,
            BootStrapClass.Md,
            BootStrapClass.Lg,
            BootStrapClass.Xl,
            BootStrapClass.Xxl,
        ];

        this.eventService.sidebarHeightSubject
            .subscribe((sidebarHeight: number) => this.sidebarHeight = sidebarHeight);
    }

    /**
     * @returns The distance in pixels from the top of iframe to the top of the sidebar.
     * This takes into account either the sidebar offset configuration specified during embedding, if any,
     * otherwise the sidebar offset configuration from the workbook.
     */
    public getOffsetTop(): number {
        let sidebarOffsetConfiguration: string = this.applicationService.sidebarOffsetConfiguration;
        let useWorkBookOffset: boolean = StringHelper.isNullOrEmpty(sidebarOffsetConfiguration);
        let breakpointOffsets: Array<BreakpointOffset>  = useWorkBookOffset ? this.buildWorkbookBreakPointOffsets() :
            this.buildEmbeddedBreakpointOffsets(sidebarOffsetConfiguration);
        let applicableOffset: number = this.getApplicableOffset(breakpointOffsets);
        return applicableOffset;
    }

    /**
     * @returns The distance in pixels from the top of iframe to the bottom of the sidebar.
     * This takes into account either the sidebar offset configuration specified during embedding, if any,
     * otherwise the sidebar offset configuration from the workbook.
     */
    public getOffsetBottom(): number {
        return this.getOffsetTop() + this.sidebarHeight;
    }

    /**
     * @returns The distance in pixels from the top of iframe to the start of visible content, 
     * taking into account that on mobile, a floating sidebar will cover some of the visible content.
     */
    public getOffsetToVisibleContent(): number {
        return this.browserDetectionService.isMobileWidth()
            ? this.getOffsetBottom()
            : this.getOffsetTop();
    }

    private getApplicableOffset(breakPointOffsets: Array<BreakpointOffset>): number {
        let sortedOffset: Array<BreakpointOffset> =
        breakPointOffsets.sort((first: BreakpointOffset, second: BreakpointOffset) => (first > second ? -1 : 1));

        let filteredOffset: Array<BreakpointOffset> = sortedOffset
            .filter((breakPointOffset: BreakpointOffset) =>
                breakPointOffset.breakPoint <= window.innerWidth);

        return filteredOffset.length > 0 ? filteredOffset[filteredOffset.length - 1].offset : 0;
    }

    public buildWorkbookBreakPointOffsets(): Array<BreakpointOffset> {
        let  breakpointOffsets: Array<BreakpointOffset> = [];
        this.bootStrapBreakpoints.forEach((bootStrapClass: string) => {
            if (!this.scrollOffsetTop[bootStrapClass]) {
                return;
            }
            const breakpointOffset: BreakpointOffset = <BreakpointOffset> {
                breakPoint: this.getBreakPointByBootStrapClassName(bootStrapClass).breakPoint,
                offset: this.scrollOffsetTop[bootStrapClass] ?
                    Number(this.scrollOffsetTop[bootStrapClass]) : 0,
            };
            breakpointOffsets.push(breakpointOffset);
        });
        return breakpointOffsets;
    }

    private getBreakPointByBootStrapClassName(bootStrapClassName: string): BootStrapBreakPoint {
        let bootStrapBreakPoint: BootStrapBreakPoint = this.bootstrapBreakpointsMap.
            find((breakpoint: BootStrapBreakPoint) => breakpoint.bootStrapClassName === bootStrapClassName);
        return bootStrapBreakPoint;
    }

    private buildEmbeddedBreakpointOffsets(embeddedSideBarOffset: string): Array<BreakpointOffset> {
        let breakPointOffsets: Array<BreakpointOffset> = [];
        if (!embeddedSideBarOffset) {
            return breakPointOffsets;
        }
        let isBootStrapStandardBreakPoint: boolean = this.isBootStrapStandardBreakPoint(embeddedSideBarOffset);
        let sideBarOffsets: Array<string> = embeddedSideBarOffset.split(this.breakPointsSeparator);
        sideBarOffsets.forEach((sideBarOffset: string) => {
            let offsetDetail: Array<string> = sideBarOffset.split(this.breakPointSeparator);
            if (offsetDetail.length != 2) {
                return;
            }
            if (isBootStrapStandardBreakPoint) {
                breakPointOffsets.push(this.createBootStrapSpecificBreakPoint(offsetDetail[0], offsetDetail[1]));
            } else {
                breakPointOffsets.push(this.createPixelSpecificBreakPoint(offsetDetail[0],
                    offsetDetail[1]));
            }
        });
        return breakPointOffsets;
    }

    private createBootStrapSpecificBreakPoint(bootStrapClass: string, offset: string): BreakpointOffset {
        let breakpointOffset: BreakpointOffset = <BreakpointOffset>{
            breakPoint: this.getBreakPointByBootStrapClassName(bootStrapClass).breakPoint,
            offset: Number(offset),
        };
        return breakpointOffset;
    }

    private createPixelSpecificBreakPoint(breakPoint: string, offset: string): BreakpointOffset {
        let breakPointOffset: BreakpointOffset = <BreakpointOffset>{
            breakPoint:  Number(breakPoint),
            offset: Number(offset),
        };

        return breakPointOffset;
    }

    private isBootStrapStandardBreakPoint(embeddedSideBarOffset: string): boolean {
        if (!embeddedSideBarOffset) {
            return false;
        }
        let sideBarOffsets: Array<string> = embeddedSideBarOffset.split(this.breakPointsSeparator);
        if (sideBarOffsets.length > 0) {
            let offsetDetail: Array<string> = sideBarOffsets[0].split(this.breakPointSeparator);
            if (offsetDetail.length == 2) {
                return this.bootStrapBreakpoints.includes(offsetDetail[0]);
            }
        }
        return false;
    }
}
