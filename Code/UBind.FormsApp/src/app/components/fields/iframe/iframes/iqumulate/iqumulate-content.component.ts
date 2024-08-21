import {
    Component, ComponentFactoryResolver, ViewContainerRef, ViewChild, OnInit, OnDestroy, Output, EventEmitter,
} from '@angular/core';
import { DomSanitizer } from '@angular/platform-browser';
import { IqumulatePremiumFundingApiService } from '@app/services/api/iqumulate-premium-funding-api.service';
import { IQumulateMpfComponent } from './iqumulate-mpf.component';
import { GenericIframeComponent } from '../../generic-iframe.component';
import { FormService } from '@app/services/form.service';
import { WorkflowService } from '@app/services/workflow.service';
import { ConfigService } from '@app/services/config.service';
import { ApplicationService } from '@app/services/application.service';
import { MessageService } from '@app/services/message.service';
import { Iframeable } from '../../iframeable';
import { Expression } from '@app/expressions/expression';
import { takeUntil } from 'rxjs/operators';
import { ExpressionDependencies } from '@app/expressions/expression-dependencies';
import { IQumulateRequestResourceModel } from '@app/resource-models/iqumulate-request-resource-model';

/**
 * Export iqumulate content component class.
 * TODO: Write a better class header: component of iqumulate content payment.
 */
@Component({
    templateUrl: './iqumulate-content.component.html',
})

export class IqumulateContentComponent extends GenericIframeComponent implements Iframeable, OnInit, OnDestroy {

    @ViewChild('iQumulateMPFContainer', { read: ViewContainerRef })
    public iQumulateMPFContainer: ViewContainerRef;

    @Output() public fundingAccepted: EventEmitter<any> = new EventEmitter<any>();

    public dynamicComponent: IQumulateMpfComponent;
    public iQumulateTargetIframe: any;
    public acceptanceConfirmationField: string;
    public isFundingFinalized: boolean = false;
    public isFundingAccepted: boolean = false;
    public triggerExpressionValue: any;
    public fundingRequestData: any;
    public hasUserCancelledFunding: boolean;
    public size: string = '900px';

    public constructor(
        protected sanitizer: DomSanitizer,
        protected messageService: MessageService,
        public applicationService: ApplicationService,
        protected configService: ConfigService,
        protected formService: FormService,
        protected workflow: WorkflowService,
        protected iqumulateFunding: IqumulatePremiumFundingApiService,
        private resolver: ComponentFactoryResolver,
        protected expressionDependencies: ExpressionDependencies) {
        super(sanitizer, messageService, applicationService, configService, formService);
    }

    public ngOnInit(): void {
        super.ngOnInit();
        this.iqumulateFunding.getRequestData().pipe(takeUntil(this.destroyed))
            .subscribe((iqumulateRequestResourceModel: IQumulateRequestResourceModel) =>
                this.loadIQumulateMPFComponent(iqumulateRequestResourceModel));
        this.listenToCalculatedValueChanges();
    }

    public ngOnDestroy(): void {
        super.ngOnDestroy();
    }

    private listenToCalculatedValueChanges(): void {
        if (this.facade.calculatedValue != null && this.facade.calculatedValue != ''
            && this.facade.calculatedValue != undefined
        ) {
            let calculatedValueExpression: Expression = new Expression(
                this.facade.calculatedValue,
                this.expressionDependencies,
                "iQumulate component calculated value");
            calculatedValueExpression.nextResultObservable.pipe(takeUntil(this.destroyed))
                .subscribe((result: any) => {
                    if (this.hasUserCancelledFunding) {
                        if (this.triggerExpressionValue != result
                            && result
                            && result != null
                            && result != ''
                            && result != undefined
                        ) {
                            this.triggerExpressionValue = result;
                            this.loadIQumulateMPFComponent(this.fundingRequestData);
                        }
                    }

                });
            calculatedValueExpression.triggerEvaluation();
        }
    }

    private loadIQumulateMPFComponent(iQumulateRequestResourceModel: IQumulateRequestResourceModel): void {
        this.fundingRequestData = iQumulateRequestResourceModel;
        const componentFactory: any = this.resolver.resolveComponentFactory(IQumulateMpfComponent);
        this.iQumulateMPFContainer.clear();
        this.dynamicComponent = <IQumulateMpfComponent>this.iQumulateMPFContainer.createComponent(
            componentFactory).instance;
        if (iQumulateRequestResourceModel) {
            const actionUrl: any = iQumulateRequestResourceModel.actionUrl;
            const baseUrl: any = iQumulateRequestResourceModel.baseUrl;
            const messageOriginUrl: any = iQumulateRequestResourceModel.messageOriginUrl;

            this.dynamicComponent.parameters = iQumulateRequestResourceModel.iQumulateRequestData;
            this.dynamicComponent.targetIframe = this.iQumulateTargetIframe;
            this.dynamicComponent.action = this.sanitizer.bypassSecurityTrustResourceUrl(actionUrl);
            this.dynamicComponent.baseUrl = this.sanitizer.bypassSecurityTrustResourceUrl(baseUrl);
            this.dynamicComponent.messageOriginUrl = messageOriginUrl;
            this.dynamicComponent.isFundingAccepted = this.isFundingAccepted;
            this.dynamicComponent.fundingAccepted.pipe(takeUntil(this.destroyed)).subscribe((event: any) => {
                this.onFundingAccepted(event);
            });
            this.dynamicComponent.cancelFunding.pipe(takeUntil(this.destroyed)).subscribe((event: any) => {
                this.onIqumulateFundingCancelled(event);
            });
            this.dynamicComponent.finalizeFunding.pipe(takeUntil(this.destroyed)).subscribe((event: any) => {
                this.onIqumulateFinalizeFunding();
            });
            this.dynamicComponent.loadCompleted.pipe(takeUntil(this.destroyed)).subscribe((event: any) => {
                this.onIqumulateIFrameLoaded(event);
            });

            this.acceptanceConfirmationField = iQumulateRequestResourceModel.acceptanceConfirmationField;
        }

        this.iFrameResizerOptions.scrolling = true;
        this.iframeId = 'iQumulateMPFIframe';
        if (this.hasUserCancelledFunding) {
            this.onIqumulateIFrameLoaded();
            this.hasUserCancelledFunding = null;
        }
    }

    public onIqumulateIFrameLoaded(event?: any): void {
        this.initiateIframeResizer();
        this.manuallyResizeIframeIfNeeded();
    }

    public onFundingAccepted(premiumFundingResponseData: any): void {
        this.iqumulateFunding
            .postResponse(premiumFundingResponseData)
            .pipe(takeUntil(this.destroyed))
            .subscribe(() => this.acceptFundingConfirmation());
        this.fundingAccepted.emit(premiumFundingResponseData);
    }

    public acceptFundingConfirmation(): void {
        this.isFundingFinalized = true;
    }

    public onIqumulateFinalizeFunding(): void {
        if (!this.isFundingFinalized) {
            setTimeout(
                () => {
                    this.onIqumulateFinalizeFunding();
                },
                this.checkReadyIntervalMs);

        } else {
            // NOTE: Do not change the VALUE into boolean true, this should be in string format, i.e. 'true'
            this.updateFormControlFieldValue(this.acceptanceConfirmationField, 'true');
            this.clearComponents();
        }
    }

    public onIqumulateFundingCancelled(event: any): void {
        // NOTE: Do not change the VALUE into boolean false, this should be in string format, i.e. 'false'
        this.updateFormControlFieldValue(this.acceptanceConfirmationField, 'false');
        this.hasUserCancelledFunding = true;
        // There is an issue with Iqumulate footer, this is a temp fix
        this.clearComponents();
    }

    public clearComponents(): void {
        this.dynamicComponent = null;
        this.iQumulateMPFContainer.detach();
        this.iframeId = null;
        if (this.iframes) {
            this.iframes[0].style.height = null;
        }
    }
    protected initiateIframeResizer(): void {
        if (window['iFrameResize'] == null || document.getElementById(this.iframeId) == null) {
            setTimeout(
                () => {
                    this.initiateIframeResizer();
                },
                this.checkReadyIntervalMs);
        } else {
            this.iframes = window['iFrameResize'](this.iFrameResizerOptions, '#' + this.iframeId);
        }
    }

    private manuallyResizeIframeIfNeeded(): void {
        if (this.iframes) {
            this.iframes[0].style.height = this.size;
        }
    }

    protected closeIframe(): void {
        if (this.iframes) {
            for (const iframe of this.iframes) {
                if (iframe.id == this.iframeId) {
                    iframe.iFrameResizer.close();
                    return;
                }
            }
        }
    }
}
