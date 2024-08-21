
import { Observable, SubscriptionLike } from 'rxjs';
import { Injectable } from '@angular/core';
import { HttpClient, HttpParams, HttpHeaders, HttpResponse } from '@angular/common/http';
import { AppConfigService } from '@app/services/app-config.service';
import { AppConfig } from '../models/app-config';

/**
 * Export API service Class
 * TODO: Write a better class header: API method functions.
 */
@Injectable({ providedIn: 'root' })
export class ApiService {
    private _baseURL: string = '';
    private _accountBaseURL: string = '';
    private subscriptions: Array<SubscriptionLike> = new Array<SubscriptionLike>();

    public constructor(
        private http: HttpClient,
        private appConfigService: AppConfigService,
    ) {

        this.subscriptions.push(this.appConfigService.appConfigSubject.subscribe((appConfig: AppConfig) => {
            this._baseURL = appConfig.portal.api.baseUrl + appConfig.portal.environment;
            this._accountBaseURL = appConfig.portal.api.accountUrl;
        }));
    }

    /**
     *
     * @param path API endpoint path
     * @param options optional parameter for passing additional http parameters
     * @param isPortalFeatures optional value indicating whether the call is from the portal feature service
     * remark: Portal Feature call errors do not trigger error modals/alerts and are handled at the backend only
     */
    public get(path: string, params?: HttpParams | { [param: string]: string | Array<string> }): Observable<any> {
        const url: string = this.generateApiUrl(path);
        return this.http.get(url, { params: params });
    }

    /**
     *
     * @param path API endpoint path
     * @param options optional parameter for passing additional http parameters
     */
    public getTyped<TResponse>(path: string, options?: HttpParams): Observable<any | TResponse> {
        const url: string = this.generateApiUrl(path);
        return this.http
            .get<TResponse>(url, { params: options });
    }

    /**
     *
     * @param path API endpoint keyword
     * @param options optional parameter for passing additional http parameters
     * @param customPath optional value indicating whether the call is to a custom path or not
     */
    public getBlob(path: string, options?: HttpParams, customPath?: boolean): Observable<any | Blob> {
        const url: string = customPath ? path : this.generateApiUrl(path);
        return this.http
            .get(url, { params: options, responseType: "blob" });
    }

    public postBlob(path: string, body: any, options?: HttpParams): Observable<HttpResponse<Blob>> {
        console.log(body);
        const url: string = this.generateApiUrl(path);
        return this.http
            .post(url, body, { params: options, responseType: "blob", observe: "response" });
    }

    /**
     *
     * @param path API endpoint keyword
     * @param body body of the post call
     * @param options optional parameter for passing additional http parameters
     * @param auth optional value indicating whether the call was from an authentication related service
     * remark: Authentication related errors for logins and account activation are handled separately by
     * respective service/pages and do not trigger alerts from the error-handler service.
     */
    public post(
        path: string,
        body: any,
        options?: HttpParams,
        auth?: boolean,
        isErrorHandlerIgnored?: boolean,
    ): Observable<any> {
        const url: string = auth === true ? this.generateApiAccountUrl(path) :
            this.generateApiUrl(path);
        return this.http.post(url, body, { params: options });
    }

    /**
     * 
     * @param path API endpoint keyword
     * @param body body of post call
     * @param options optional http parameters
     * @param auth optional value indicating whether the call was from an  authentication  related service
     * remark: Authentication related errors for logins and account activation are handled separately by
     * respective service/pages and do not trigger alerts from the error-handler service.
     */
    public postTyped<TResponse>(
        path: string,
        body: any,
        options?: HttpParams,
        auth?: boolean,
    ): Observable<any | TResponse> {
        const url: string = auth === true ? this.generateApiAccountUrl(path) :
            this.generateApiUrl(path);
        return this.http.post<TResponse>(url, body, { params: options });
    }

    /**
     * 
     * @param path API endpoint keyword
     * @param body body of post call
     * @param params http parameters for the post request
     * @param auth optional value indicating whether the call was from an authentication related service
     * remark: Authentication related errors for logins and account activation are handled separately by
     * respective service/pages and do not trigger alerts from the error-handler service.
     */
    public postJson(path: string, body: any, params: HttpParams, auth?: boolean): Observable<any> {
        const url: string = auth === true ? this.generateApiAccountUrl(path) :
            this.generateApiUrl(path);
        const headers: HttpHeaders = new HttpHeaders({ 'Content-Type': 'application/json' });
        return this.http.post(url, body, { params: params, headers: headers });
    }

    /**
     *
     * @param path API endpoint keyword
     * @param model the model to be updated
     * @param options optional parameters
     * @param isErrorHandlerIgnored is the error handler ignored?
     */
    public put(path: string, model: any, options?: HttpParams): Observable<any> {
        const url: string = this.generateApiUrl(path);
        return this.http.put(url, model, { params: options });
    }

    /**
     *
     * @param path API endpoint keyword
     * @param options http parameters for the deletion request
     */
    public delete(path: string, options: any): Observable<ArrayBuffer> {
        const url: string = this.generateApiUrl(path);
        return this.http.request('delete', url, options);
    }

    /**
     *
     * @param path API endpoint keyword
     * @param options optional parameters
     */
    public export(path: string, options?: HttpParams): Observable<Blob> {
        const url: string = this.generateApiUrl(path);
        return this.http.get(url, { params: options, responseType: 'blob' });
    }

    private generateApiUrl(path: string): string {
        const url: string = this._baseURL + path;
        return url;
    }

    private generateApiAccountUrl(path: string): string {
        const url: string = this._accountBaseURL + path;
        return url;
    }
}
