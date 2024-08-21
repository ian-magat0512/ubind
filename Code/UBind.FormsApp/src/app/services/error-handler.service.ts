import { Injectable } from "@angular/core";
import { ProblemDetails } from '../models/problem-details';
import { AlertService } from './alert.service';
import { Alert } from '../models/alert';
import { WorkflowStatusService } from "./workflow-status.service";

/**
 * Export error handler service class.
 * TODO: Write a better class header: Error handler functions.
 */
@Injectable({
    providedIn: 'root',
})
export class ErrorHandlerService {
    public constructor(
        private alertService: AlertService,
        private workflowStatusService: WorkflowStatusService,
    ) {
    }

    public handleError(err: any): void {
        console.error(err);
        if (err instanceof ProblemDetails || ProblemDetails.isProblemDetails(err)) {
            this.handleProblemDetailsResponse(err);
        } else if (ProblemDetails.isProblemDetailsResponse(err)) {
            this.handleProblemDetailsResponse(ProblemDetails.fromJson(err.error));
        } else if (err instanceof DOMException && err.message.startsWith('Blocked a frame with origin')) {
            // Note: this is a temporary fix. A better way of handling it should be devised so that
            // a message won't be sent if the parent is unreachable (Same-origin policy)
        } else if (err instanceof Error) {
            this.alertService.alert(new Alert(
                'This is unexpected',
                err.message,
            ));
        } else {
            let additionalDetails: Array<string> = new Array<string>();
            if (typeof (err) == 'string') {
                additionalDetails.push(err);
            }
            this.alertService.alert(new Alert(
                'Something went wrong',
                "Something unusual has happened that we haven't got a solution for just yet. "
                + err?.error + ". "
                + "We've been notified about it and we'll address it soon. In the mean time, "
                + "please try again. If you're still having issues, "
                + "please get in touch with customer support.",
                additionalDetails,
            ));
        }
        this.resetState();
    }

    public handleProblemDetailsResponse(error: ProblemDetails): void {
        if (error.data) {
            if (error.data['questionAnswersForPastingIntoWorkbook']) {
                console.log('--- Question Answers in tabular format for pasting into the workbook ----');
                console.log('To debug the calculation issue, you can paste this directly into the "Question Sets" ' +
                    'column of the workbook, starting at row 4, with the header "Value":');
                console.log('--- START Question Answers ---');
                console.log(error.data['questionAnswersForPastingIntoWorkbook']);
                console.log('--- END Question Answers ---');
            }
            if (error.data['repeatingQuestionAnswersForPastingIntoWorkbook']) {
                console.log('--- Repeating Question Answers in tabular format for pasting into the workbook ----');
                console.log('To debug the calculation issue, you can paste this directly into the "Question Sets" ' +
                    'column of the workbook, starting at row 4, with the header "Value":');
                console.log('--- START Repeating Question Answers ---');
                console.log(error.data['repeatingQuestionAnswersForPastingIntoWorkbook']);
                console.log('--- END Repeating Question Answers ---');
            }
        }
        this.alertService.alert(Alert.fromError(error));
        this.resetState();
    }

    public resetState(): void {
        this.workflowStatusService.clearActionsInProgress();
        this.workflowStatusService.stopNavigation();
    }
}
