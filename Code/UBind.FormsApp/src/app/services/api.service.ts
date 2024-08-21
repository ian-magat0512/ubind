import { Injectable } from '@angular/core';
import { HttpClient, HttpRequest, HttpEvent } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ApplicationService } from './application.service';
import { BroadcastService } from './broadcast.service';
import { AlertService } from './alert.service';
import { Alert } from '@app/models/alert';
import { HttpHeadersFactory, MediaType } from '@app/helpers/http-headers-factory';
import { FormType } from '@app/models/form-type.enum';

/**
 * Export API service class.
 * TODO: Write a better class header: API service functions.
 */
@Injectable()
export class ApiService {

    private apiOrigin: string = location.origin;

    public constructor(
        private applicationService: ApplicationService,
        protected httpClient: HttpClient,
        protected notificationService: AlertService,
        protected broadcast: BroadcastService,
    ) {
    }

    public setApiOrigin(origin: string): void {
        this.apiOrigin = origin ?? location.origin;
    }

    protected generateApiUrl(path: string): string {
        let tenantAlias: string = this.applicationService.tenantAlias;
        let productAlias: string = this.applicationService.productAlias;
        let environment: string = this.applicationService.environment;
        const isTestData: boolean = this.applicationService.isTestData;
        const formType: string = this.applicationService.formType;
        let url: string = this.apiOrigin + `/api/v1/${tenantAlias}/${environment}/${productAlias}`
            + this.getDifferentiationPath(formType, path)
            + (isTestData == true ? `?isTestData=${isTestData}` : '');

        return url;
    }

    protected generateRequestOptions(): any {
        let headersFactory: HttpHeadersFactory = HttpHeadersFactory
            .create()
            .withContentType(MediaType.Json)
            .withAccept(MediaType.Json, MediaType.Text, MediaType.Any);

        return { headers: headersFactory.build() };
    }

    // parses the object to url parameters 
    protected parseUrlParam(obj: any): string {

        if (obj == '' || obj == null) {
            return null;
        }

        if (typeof obj == 'object') {
            let str: string = "";
            for (let key in obj) {
                if (str != "") {
                    str += "&";
                }
                str += key + "=" + encodeURIComponent(obj[key]);
            }
            return str;
        }
        return null;
    }

    public get(
        path: string,
        paramData: any,
        timeout: number = 60000,
        reportProgress: boolean = false,
        customPath: boolean = false,
    ): Observable<HttpEvent<any>> {
        let url: string = customPath ? path : this.generateApiUrl(path);
        let options: any = this.generateRequestOptions();
        options['reportProgress'] = reportProgress;
        let urlParam: string = this.parseUrlParam(paramData);
        let separator: string = this.applicationService.isTestData ? '&' : '?';
        url = urlParam != null ? (url + separator + urlParam) : url;
        let request: HttpRequest<any> = new HttpRequest('GET', url, options);
        return this.httpClient.request(request);
    }

    public patch(
        path: string,
        patchData: any,
        timeout: number = 60000,
        reportProgress: boolean = false,
        customPath: boolean = false,
    ): Observable<HttpEvent<any>> {
        let url: any = customPath ? path : this.generateApiUrl(path);
        let options: any = this.generateRequestOptions();
        options['reportProgress'] = reportProgress;
        let request: HttpRequest<any> = new HttpRequest('PATCH', url, patchData, options);
        return this.httpClient.request(request);
    }

    public post(
        path: string,
        postData: any,
        timeout: number = 30000,
        reportProgress: boolean = false,
        customPath: boolean = false,
    ): Observable<HttpEvent<any>> {
        let url: any = customPath ? path : this.generateApiUrl(path);
        let options: any = this.generateRequestOptions();
        options['reportProgress'] = reportProgress;
        let request: HttpRequest<any> = new HttpRequest('POST', url, postData, options);
        return this.httpClient.request(request);
    }

    public forceShowErrorModal(message: string): void {
        this.notificationService.alert(new Alert("Error", message));
        setTimeout(() => {
            setTimeout(() => {
                this.broadcast.broadcast('Error500Event', {});
            }, 500);
        }, 500);
    }

    private getDifferentiationPath(formType: string, path: string): string {
        let newPath: string = (formType == FormType.Claim ? '/claim' : '') + `/${path}`;

        switch (path) {
            case 'load':
            case 'workflowStep':
            case 'calculation':
            case 'configuration':
            case 'formUpdate':
            case 'save':
            case 'attachment':
            case 'enquiry':
                newPath = `/${formType ? formType : 'quote'}/${path}`;
                break;
            default:
                break;
        }

        return newPath;
    }
}
