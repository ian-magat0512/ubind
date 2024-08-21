import { ErrorHandler, Injectable } from '@angular/core';
import { ErrorHandlerService } from "../services/error-handler.service";

/**
 * Export global error handler class.
 * TODO: Write a better class header: global error handler functions.
 */
@Injectable({
    providedIn: 'root',
})
export class GlobalErrorHandler implements ErrorHandler {
    public constructor(private errorHandlerService: ErrorHandlerService) {
    }

    public handleError(error: any): void {
        // unwrap the error if it's inside a promise
        while (error.rejection) {
            error = error.rejection;
        }
        if (this.errorHandlerService) {
            this.errorHandlerService.handleError(error);
        } else {
            console.error(error);
        }
    }
}
