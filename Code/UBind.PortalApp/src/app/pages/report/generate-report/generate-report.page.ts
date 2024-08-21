import { Component, OnInit, Injector, ElementRef, OnDestroy } from '@angular/core';
import { FormBuilder, Validators, FormGroup } from '@angular/forms';
import { NavProxyService } from '@app/services/nav-proxy.service';
import { AppConfigService } from '@app/services/app-config.service';
import { ReportApiService } from '@app/services/api/report-api.service';
import { ActivatedRoute, Router } from '@angular/router';
import { ReportResourceModel, ReportGenerateModel } from '@app/resource-models/report.resource-model';
import { LayoutManagerService } from '@app/services/layout-manager.service';
import { DetailPage } from '@app/pages/master-detail/detail.page';
import { EventService } from '@app/services/event.service';
import { Subject } from 'rxjs';
import { finalize, takeUntil } from 'rxjs/operators';
import { DateHelper } from '@app/helpers/date.helper';
import { SharedAlertService } from '@app/services/shared-alert.service';

/**
 * Export generate report page component class.
 * TODO: Write a better class header: generation of report.
 */
@Component({
    selector: 'app-generate-report',
    templateUrl: './generate-report.page.html',
    styleUrls: ['./generate-report.page.scss'],
})
export class GenerateReportPage extends DetailPage implements OnInit, OnDestroy {

    public generateReportForm: FormGroup = this.formBuilder.group({
        fromDate: ['', [Validators.required]],
        toDate: ['', [Validators.required]],
        includeTestData: [false],
    });

    public formHasError: boolean;
    public filename: string;
    public report: ReportResourceModel;
    protected reportId: string;

    public constructor(
        public navProxy: NavProxyService,
        protected formBuilder: FormBuilder,
        protected appConfigService: AppConfigService,
        protected reportApiService: ReportApiService,
        protected route: ActivatedRoute,
        protected router: Router,
        protected sharedAlertService: SharedAlertService,
        public layoutManager: LayoutManagerService,
        eventService: EventService,
        elementRef: ElementRef,
        injector: Injector,
    ) {
        super(eventService, elementRef, injector);
    }

    public ngOnInit(): void {
        this.destroyed = new Subject<void>();
        this.route.params.subscribe((params: any) => {
            this.reportId = params['reportId'];
        });
        this.loadReport(this.reportId);
    }

    public ngOnDestroy(): void {
        this.destroyed.next();
        this.destroyed.complete();
    }

    public loadReport(id: string): void {
        this.isLoading = true;
        this.reportApiService.getById(id)
            .pipe(
                takeUntil(this.destroyed),
                finalize(() => this.isLoading = false),
            )
            .subscribe(
                (response: ReportResourceModel) => {
                    this.report = response;
                },
            );
    }

    public closeButtonClicked(): void {
        this.returnToPrevious();
    }

    public generateButtonClicked(): void {
        if (!this.generateReportForm.valid) {
            this.formHasError = true;
            return;
        }

        const generateReportResourceModel: ReportGenerateModel = {
            from: this.generateReportForm.value.fromDate,
            to: this.generateReportForm.value.toDate,
            includeTestData: this.generateReportForm.value.includeTestData,
            timeZoneId: DateHelper.getTimeZoneId(),
        };

        this.reportApiService.generateReportFile(this.reportId, generateReportResourceModel)
            .pipe(takeUntil(this.destroyed))
            .subscribe(
                (response: any) => {
                    this.returnToPrevious();
                    this.showPopupReportGeneratingReport();
                },
            );
    }

    private returnToPrevious(): void {
        this.navProxy.navigateBack(['report', this.reportId]);
    }

    private showPopupReportGeneratingReport(): void {
        this.sharedAlertService.showWithOk(
            'Report is being generated',
            'Your new report is generating in the background and will appear '
            + 'under the history tab when completed. '
            + 'Use the refresh button to check if it has completed. '
            + 'When the report appears you can simply click it to download and save the report as a local file.',
            true,
        );
    }
}
