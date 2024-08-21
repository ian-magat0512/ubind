import { ErrorMessages } from './error-messages.constants';
import { HttpErrorResponse } from '@angular/common/http';

/** Represents Problem Details as per RFC 7807 */
export class ProblemDetails {

    // standard properties:
    private _type: string;
    private _title: string;
    private _status: number;
    private _detail: string;
    private _instance: string;

    // extension properties:
    private _code: string;
    private _additionalDetails: Array<string>;
    private _data: any;

    public constructor(
        code: string,
        title: string,
        detail: string,
        status: number,
        additionalDetails: Array<string> = new Array<string>(),
        data: any = null,
        type: string = null,
        instance: string = null,
    ) {
        this._code = code;
        this._title = title;
        this._detail = detail;
        this._status = status;
        this._additionalDetails = additionalDetails;
        this._data = data;
        this._type = type;
        this._instance = instance;
    }

    public static fromJson(json: any): ProblemDetails {
        let code: string = json['code'] != undefined ? json['code'] : "unknown";
        let title: string = json['title'] != undefined ? json['title'] : "Error";
        let detail: string = json['detail'] != undefined ? json['detail'] : ErrorMessages.ApplicationInternalError;
        let status: number = json['status'] != undefined ? json['status'] : 400;
        let additionalDetails: any = json['additionalDetails'] != undefined ?
            json['additionalDetails'] : new Array<string>();
        let data: any = json['data'] != undefined ? json['data'] : null;
        let type: string = json['type'] != undefined ? json['type'] : null;
        let instance: string = json['instance'] != undefined ? json['instance'] : null;
        return new ProblemDetails(code, title, detail, status, additionalDetails, data, type, instance);
    }

    public static isProblemDetailsResponse(err: HttpErrorResponse): boolean {
        if (!err.headers) {
            return false;
        }
        const contentTypeHeader: string = err.headers.get('Content-Type');
        return contentTypeHeader && contentTypeHeader.indexOf('application/problem+json') != -1;
    }

    public static isProblemDetails(obj: any): obj is ProblemDetails {
        return obj.title !== undefined && obj.detail !== undefined;
    }

    public get code(): string {
        return this._code;
    }

    public get title(): string {
        return this._title;
    }

    public get detail(): string {
        return this._detail;
    }

    public get httpStatusCode(): number {
        return this._status;
    }

    public get additionalDetails(): Array<string> {
        return this._additionalDetails;
    }

    public get data(): any {
        return this._data;
    }

    public get type(): string {
        return this._type;
    }

    public get instance(): string {
        return this._instance;
    }
}
