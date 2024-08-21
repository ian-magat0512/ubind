import { Component, Input, OnInit } from '@angular/core';
import { trigger, state, style, animate, transition } from '@angular/animations';
import { Widget } from '../widget';
import { ConfigService } from '@app/services/config.service';
import { ExpressionDependencies } from '@app/expressions/expression-dependencies';
import { takeUntil } from 'rxjs/operators';
import { TextWithExpressions } from '@app/expressions/text-with-expressions';
import { EventService } from '@app/services/event.service';

/**
 * Export aside widget component class.
 * This class manage Aside widget function.
 */
@Component({
    selector: 'aside-widget',
    templateUrl: './aside.widget.html',
    animations: [
        trigger('asideAnimation', [
            state('in', style({ transform: 'translateX(0)' })),
            transition('void => *', [
                style({
                    opacity: 0,
                    transform: 'translateX(-10px)',
                }),
                animate('400ms 600ms ease-out'),
            ]),
            transition('* => void', [
                animate('600ms ease-in', style({
                    opacity: 0,
                    transform: 'translateX(-10px)',
                })),
            ]),
        ]),
    ],
})

export class AsideWidget extends Widget implements OnInit {

    @Input('definition')
    private definition: any;

    protected transitionDelayBetweenMs: number = 1000;

    public header: string;
    public body: string;
    public footer: string;
    public id: string;

    private headerExpression: TextWithExpressions;
    private bodyExpression: TextWithExpressions;
    private footerExpression: TextWithExpressions;

    public constructor(
        protected configService: ConfigService,
        protected expressionDependencies: ExpressionDependencies,
        private eventService: EventService,
    ) {
        super();
    }

    public ngOnInit(): void {
        super.ngOnInit();
        this.id = this.definition.name;
        this.setupExpressions();
        this.onConfigurationUpdated();
    }

    protected destroyExpressions(): void {
        this.headerExpression?.dispose();
        this.headerExpression = null;
        this.bodyExpression?.dispose();
        this.bodyExpression = null;
        this.footerExpression?.dispose();
        this.footerExpression = null;
        super.destroyExpressions();
    }

    private onConfigurationUpdated(): void {
        this.eventService.updatedConfiguration.pipe(takeUntil(this.destroyed))
            .subscribe(() => {
                this.setupExpressions();
            });
    }

    protected setupExpressions(): void {
        this.setupHeaderExpression();
        this.setupBodyExpression();
        this.setupFooterExpression();
    }

    private setupHeaderExpression(): void {
        if (this.headerExpression) {
            this.headerExpression.dispose();
            this.headerExpression = null;
        }
        let hasHeaderText: any = this.definition?.name
            && this.configService.textElements?.sidebarPanels?.[this.definition.name + 'Header']?.text;
        if (!hasHeaderText) {
            this.header = '';
        } else {
            let expressionSource: string =
                this.configService.textElements.sidebarPanels[this.definition.name + 'Header'].text;
            this.headerExpression = new TextWithExpressions(
                expressionSource,
                this.expressionDependencies,
                'aside widget header');
            this.headerExpression.nextResultObservable.pipe(takeUntil(this.destroyed))
                .subscribe((value: string) => this.header = value);
            this.headerExpression.triggerEvaluationWhenFormLoaded();
        }
    }

    private setupBodyExpression(): void {
        if (this.bodyExpression) {
            this.bodyExpression.dispose();
            this.bodyExpression = null;
        }
        let hasBodyText: any = this.definition?.name
            && this.configService.textElements?.sidebarPanels?.[this.definition.name + 'Body']?.text;
        if (!hasBodyText) {
            this.body = '';
        } else {
            let expressionSource: string =
                this.configService.textElements.sidebarPanels[this.definition.name + 'Body'].text;
            this.bodyExpression = new TextWithExpressions(
                expressionSource,
                this.expressionDependencies,
                'aside widget body');
            this.bodyExpression.nextResultObservable.pipe(takeUntil(this.destroyed))
                .subscribe((value: string) => this.body = value);
            this.bodyExpression.triggerEvaluationWhenFormLoaded();
        }
    }

    private setupFooterExpression(): void {
        if (this.footerExpression) {
            this.footerExpression.dispose();
            this.footerExpression = null;
        }
        let hasFooterText: any = this.definition?.name
            && this.configService.textElements?.sidebarPanels?.[this.definition.name + 'Footer']?.text;
        if (!hasFooterText) {
            this.footer = '';
        } else {
            let expressionSource: string =
                this.configService.textElements.sidebarPanels[this.definition.name + 'Footer'].text;
            this.footerExpression = new TextWithExpressions(
                expressionSource,
                this.expressionDependencies,
                'aside widget footer');
            this.footerExpression.nextResultObservable.pipe(takeUntil(this.destroyed))
                .subscribe((value: string) => this.footer = value);
            this.footerExpression.triggerEvaluationWhenFormLoaded();
        }
    }
}
