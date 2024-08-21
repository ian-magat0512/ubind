import { ProblemDetails } from './problem-details';

/**
 * An alert is some information that someone needs to be alerted about. It's typically created when there is an error.
 */
export class Alert {
    private _title: string;
    private _message: string;
    private _additionalDetails: Array<string>;
    private _additionalDetailsTitle: string;
    private _error: ProblemDetails;

    public constructor(
        title: string,
        message: string,
        additionalDetails: Array<string> = new Array<string>(),
        error: ProblemDetails = null,
        additionalDetailsTitle: string = "Further details:",
    ) {
        this._title = title;
        this._message = message;
        this._additionalDetails = additionalDetails;
        this._additionalDetailsTitle = additionalDetailsTitle;
        this._error = error;
    }
    public static fromError(error: ProblemDetails): Alert {
        return new Alert(error.title, error.detail, error.additionalDetails, error);
    }

    public isValid(): string {
        return this._title && this._message;
    }

    public get title(): string {
        return this._title;
    }

    public get message(): string {
        return this._message;
    }

    public get additionalDetails(): Array<string> {
        return this._additionalDetails;
    }

    public get error(): ProblemDetails {
        return this._error;
    }

    public get additionalDetailsTitle(): string {
        return this._additionalDetailsTitle;
    }
}
