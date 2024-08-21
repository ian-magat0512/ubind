import { ErrorMessages } from './error-messages.constants';

/**
 * Export App error class.
 * TODO: Write a better class header: App error model.
 */
export class AppError {
    private _code: string;
    private _title: string;
    private _message: string;
    private _httpStatusCode: number;
    private _additionalDetails: Array<string>;

    public constructor(code: string, title: string, message: string,
        httpStatusCode: number, additionalDetails: Array<string> = new Array<string>()) {
        this._code = code;
        this._title = title;
        this._message = message;
        this._httpStatusCode = httpStatusCode;
        this._additionalDetails = additionalDetails;
    }

    public static fromJson(error: any, httpStatusCode?: number): AppError {
        let code: string = error['title'] != undefined ? error.code : "unknown";
        let title: string = error['title'] != undefined ? error.title : "Error";
        let message: string = error['message'] != undefined ?
            error.message : ErrorMessages.ApplicationInternalError;
        let localHttpStatusCode: number = httpStatusCode != undefined ?
            httpStatusCode : error['httpStatusCode'] != undefined ? error['httpStatusCode'] : 400;
        let additionalDetails: any = error['additionalDetails'] != undefined ?
            error.additionalDetails : new Array<string>();
        return new AppError(code, title, message, localHttpStatusCode, additionalDetails);
    }

    public get code(): string {
        return this._code;
    }

    public get title(): string {
        return this._title;
    }

    public get message(): string {
        return this._message;
    }

    public get httpStatusCode(): number {
        return this._httpStatusCode;
    }

    public get additionalDetails(): Array<string> {
        return this._additionalDetails;
    }
}
