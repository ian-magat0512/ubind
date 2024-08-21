import { Component, OnInit, Input } from '@angular/core';
import { contentAnimation } from '@assets/animations';
import { scrollbarStyle } from '@assets/scrollbar';
import { AuthenticationService } from '@app/services/authentication.service';
import { Subscription } from 'rxjs';
import { saveAs } from 'file-saver';
import { ActivatedRoute } from '@angular/router';
import { FileAttachmentApiService } from '@app/services/api/file-attachment-api.service';
import { ErrorHandlerService } from '@app/services/error-handler.service';
import { NavProxyService } from '@app/services/nav-proxy.service';
import { finalize } from 'rxjs/operators';
import { QuestionAttachmentViewModel } from '@app/viewmodels/question-view.viewmodel';
import { RepeatingQuestionViewModel } from '@app/viewmodels/repeating-question.viewmodel';
import { QuestionViewModelGenerator } from '@app/helpers/question-view-model-generator';

/**
 * Export question view component class
 * This class of question's to be downloaded.
 */
@Component({
    selector: 'app-questions-view',
    templateUrl: './questions-view.component.html',
    animations: [contentAnimation],
    styleUrls: [
        './questions-view-component.scss',
        '../../../assets/css/scrollbar-segment.css',
        '../../../assets/css/scrollbar-div.css',
    ],
    styles: [
        scrollbarStyle,
    ],
})
export class QuestionsViewComponent implements OnInit {
    @Input() public displayType: string = QuestionViewModelGenerator.type.Quote;
    @Input() public displayFields: any;
    @Input() public formData: any;
    @Input() public questionAttachments: Array<string> = [];
    @Input() public questionItems: Array<QuestionAttachmentViewModel> = [];
    @Input() public repeatingQuestionItems: Array<RepeatingQuestionViewModel> = [];

    public types: any = QuestionViewModelGenerator.type;
    public keyAttachmentDictionary: Map<string, string> = new Map<string, string>();

    private id: string;
    private downloadFileAttachmentSubscription: Subscription;

    public constructor(
        private fileAttachmentApiService: FileAttachmentApiService,
        private route: ActivatedRoute,
        public authService: AuthenticationService,
        public errorHandlerService: ErrorHandlerService,
        public navProxy: NavProxyService,
    ) {
    }

    public ngOnInit(): void {
        if (this.questionItems) {
        // Do not display question items with no answers
            this.questionItems = this.questionItems.filter(
                (x: QuestionAttachmentViewModel) => x.questionValue != "",
            );
        }
    }

    public downloadFile(questionAttachment: QuestionAttachmentViewModel): void {
        if (questionAttachment.isAttachment) {
            const entityId: string = this.getIdFromParameters();
            this.downloadFileAttachmentSubscription = this.fileAttachmentApiService
                .downloadFileAttachment(this.displayType, entityId, questionAttachment.attachmentId)
                .pipe(finalize(() => {
                    this.downloadFileAttachmentSubscription.unsubscribe();
                }))
                .subscribe((blob: any) => {
                    saveAs(blob, questionAttachment.attachmentName);
                });
        }
    }

    private getIdFromParameters(): string {
        if (this.id) {
            return this.id;
        }

        let paramName: string;
        switch (this.displayType) {
            case QuestionViewModelGenerator.type.Policy:
                paramName = 'policyId';
                break;
            case QuestionViewModelGenerator.type.Claim:
                paramName = 'claimId';
                break;
            case QuestionViewModelGenerator.type.ClaimVersion:
                paramName = 'claimVersionId';
                break;
            default:
                paramName = 'quoteId';
                break;
        }
        this.id = this.route.snapshot.paramMap.get(paramName);
        return this.id;
    }
}
