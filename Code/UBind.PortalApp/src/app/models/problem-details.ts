import { HttpErrorResponse } from "@angular/common/http";

/** Represents Problem Details as per RFC 7807 */
export class ProblemDetails {

    // standard properties:
    private type: string;
    private title: string;
    private status: number;
    private detail: string;
    private instance: string;

    // extension properties:
    private code: string;
    private additionalDetails: Array<string>;
    private data: any;

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
        this.code = code;
        this.title = title;
        this.detail = detail;
        this.status = status;
        this.additionalDetails = additionalDetails;
        this.data = data;
        this.type = type;
        this.instance = instance;
    }

    public static fromJson(json: any): ProblemDetails {
        let code: any = json['code'] != undefined ? json['code'] : "unknown";
        let title: any = json['title'] != undefined ? json['title'] : "Error";
        let detail: any = json['detail'] != undefined ? json['detail'] :
            "An unexpected error has occured. We apologise for the inconvenience. "
            + "Please get in touch with support.";
        let status: any = json['status'] != undefined ? json['status'] : 400;
        let additionalDetails: any = json['additionalDetails'] != undefined ?
            json['additionalDetails'] : new Array<string>();
        let data: any = json['data'] != undefined ? json['data'] : null;
        let type: any = json['type'] != undefined ? json['type'] : null;
        let instance: any = json['instance'] != undefined ? json['instance'] : null;
        return new ProblemDetails(code, title, detail, status, additionalDetails, data, type, instance);
    }

    public static isProblemDetailsResponse(err: HttpErrorResponse): boolean {
        if (err?.headers == null) {
            // this is not a http response as it has no headers
            return false;
        }
        const contentTypeHeader: string = err.headers.get('Content-Type');
        return contentTypeHeader?.indexOf('application/problem+json') != -1;
    }

    public static isProblemDetails(obj: any): obj is ProblemDetails {
        return obj.title !== undefined && obj.detail !== undefined;
    }

    public get Code(): string {
        return this.code;
    }

    public get Title(): string {
        return this.title;
    }

    public get Detail(): string {
        return this.detail;
    }

    public get HttpStatusCode(): number {
        return this.status;
    }

    public get AdditionalDetails(): Array<string> {
        return this.additionalDetails;
    }

    public get Data(): any {
        return this.data;
    }

    public get Type(): string {
        return this.type;
    }

    public get Instance(): string {
        return this.instance;
    }
}
