import { Injectable } from "@angular/core";
import { Subject } from "rxjs";
import { HttpErrorResponse } from "@angular/common/http";
import { ProblemDetails } from '@app/models/problem-details';
import { SharedAlertService } from '@app/services/shared-alert.service';
import { Errors } from "@app/models/errors";
import { BearerTokenService } from '@app/services/bearer-token.service';
import { EventService } from "./event.service";

/**
 * Service for handling and displaying errors in the application.
 * This service provides methods to handle HTTP errors and application-specific errors,
 * presenting appropriate alerts or taking actions based on the error type.
 *
 * @remarks
 * This service integrates with the {@link SharedAlertService} to display user-friendly error messages.
 *
 * @example
 * // To use the ErrorHandlerService, inject it into your component or service:
 * constructor(private errorHandlerService: ErrorHandlerService) {}
 *
 * // Then, use the service to handle errors:
 * this.errorHandlerService.handleError(error);
 */
@Injectable({ providedIn: 'root' })
export class ErrorHandlerService {

    public isCurrentlyActive: boolean = false;
    public navigateSubject: Subject<Array<any>> = new Subject<Array<any>>();
    private currentlyHandlingAnError: boolean = false;

    public constructor(
        private sharedAlertService: SharedAlertService,
        private bearerTokenService: BearerTokenService,
        private eventService: EventService,
    ) {
        this.eventService.userPasswordExpiredSubject$.subscribe(() => this.currentlyHandlingAnError = false);
    }

    public handleError(error: any): void {
        if (error) {
            console.error(error);
        }
        if (error instanceof HttpErrorResponse) {
            this.handleErrorResponse(error);
        } else if (error instanceof ProblemDetails || ProblemDetails.isProblemDetails(error)) {
            this.handleAppError(error);
        }
    }

    /**
     * Handles error responses returned from API calls
     * @param error the full API error response returned from the request
     * TODO: make this private
     */
    public handleErrorResponse(error: HttpErrorResponse): void {
        if (ProblemDetails.isProblemDetailsResponse(error)) {
            this.handleAppError(ProblemDetails.fromJson(error.error));
        } else if (error.status == 401) {
            this.handleAppError(Errors.User.Login.Required());
        } else if (error.status == 429) {
            this.handleAppError(ProblemDetails.fromJson(error.error));
        } else {
            let header: string = '';
            let subHeader: string = '';
            let errorStatus: number = error.status;
            if (errorStatus === 403) {
                header = 'Unauthorised Access';
                subHeader = `It appears that you have tried to access a resource that you do not have `
                    + `authorisation to view. If this problem persists, `
                    + `please contact your administrator for assistance.`;
            } else if (errorStatus === 404) {
                header = 'Resource Not Found';
                subHeader = `It appears that you have tried to access a resource that does not exist. `
                    + `If this problem persists, please contact your administrator for assistance.`;
            } else if (errorStatus == 500) {
                header = 'Error Encountered';
                subHeader = `An error has been encountered preventing the current operation from completing as `
                    + `expected. If the problem persists, please report the error to your administrator, `
                    + `including the information `
                    + `found by clicking More Information below.`;
            } else if (errorStatus == 0) {
                header = 'We couldn\'t reach the server';
                subHeader = `There was a problem communicating with the server. `
                    + `Please check your internet connection and try again.`;
            } else if (errorStatus >= 400 && errorStatus < 500) {
                header = 'Bad Request';
                subHeader = `An error has been encountered while processing your request. `
                    + `If the problem persists, please `
                    + `report the error to your administrator.`;
            } else {
                header = 'Something went wrong';
                subHeader = `Something went wrong trying to communicate with the server. `
                    + `Please get in touch with customer support.`;
            }

            if (error.message) {
                subHeader = subHeader + ` \n` + error.message;
            }

            this.sharedAlertService.showWithOk(header, subHeader);
        }
    }

    private handleAppError(appError: ProblemDetails): void {
        if (appError.HttpStatusCode == 401) {
            this.handle401(appError);
        } else if (appError.Code == 'user.password.expired') {
            this.navigateSubject.next(['login', 'password-expired']);
            this.currentlyHandlingAnError = true;
        } else {
            if (this.currentlyHandlingAnError) {
                console.log('Not presenting an alert for error since we are handling a 401 which will cause a logout.');
                console.log(appError);
            } else {
                this.currentlyHandlingAnError = true;
                let button: any = {
                    text: 'OK',
                    handler: (): any => {
                        this.currentlyHandlingAnError = false;
                    },
                };
                this.sharedAlertService.showErrorWithCustomButtons(appError, [button], false);
            }
        }
    }

    private handle401(appError: ProblemDetails): void {
        if (this.currentlyHandlingAnError) {
        // do not create a second 401 alert (or a 403) when there is already a 401 being displayed.
            console.log('Not creating a second dialog for the 401 since one is already being queued');
        } else {
            this.currentlyHandlingAnError = true;
            let buttonLabel: string = this.userAccessTokenExists() ? 'Sign back in' : 'Sign in';
            let button: any = {
                text: buttonLabel,
                handler: (): any => {
                    this.navigateSubject.next(['logout']);
                    this.currentlyHandlingAnError = false;
                },
            };
            let buttons: Array<any> = new Array();
            buttons.push(button);
            this.sharedAlertService.showErrorWithCustomButtons(appError, buttons, false);
        }
    }

    private userAccessTokenExists(): boolean {
        return this.bearerTokenService.getToken() != null;
    }
}
