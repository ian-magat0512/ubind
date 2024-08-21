import { Injectable } from '@angular/core';
import { Observable, Subject } from 'rxjs';
import { GenericCache } from './generic-cache';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { HttpHeadersFactory, MediaType } from '@app/helpers/http-headers-factory';
import { finalize, tap } from 'rxjs/operators';

/**
 * This is an API Cache Service that is used to manage active API requests for caching
 */
@Injectable({
    providedIn: 'root',
})
export class ApiCacheService {

    // private activeApiRequestUrls: Array<string> = [];
    private activeApiRequestSubjects: Map<string, Subject<any>> = new Map<string, Subject<any>>();

    public constructor(
        public genericCache: GenericCache,
        private httpClient: HttpClient,
    ) {
    }

    public setCachedApiResponse(url: string, data: any): void {
        if (data) {
            this.genericCache.setCacheEntry(url, data);
        }
    }

    public getCachedApiResponse(url: string, cacheMaxAgeSeconds: number): any | null {
        return this.genericCache.getCachedDataOrNull(url, cacheMaxAgeSeconds);
    }

    public removeActiveApiRequestUrl(url: string): void {
        this.activeApiRequestSubjects.delete(url);
    }

    public hasActiveApiRequestUrl(url: string): boolean {
        return this.activeApiRequestSubjects.has(url);
    }

    public getActiveApiRequestObservable(url: string): Observable<any> {
        return this.activeApiRequestSubjects.get(url).asObservable();
    }

    public addActiveApiRequestSubject(url: string): Subject<any> {
        let subject: Subject<any> = this.activeApiRequestSubjects.get(url);
        if (!subject) {
            subject = new Subject<any>();
            this.activeApiRequestSubjects.set(url, subject);
        }
        return subject;
    }

    public processRequest<T = any>(
        httpVerb: string,
        url: string,
        body: any,
        cacheMaxAgeSeconds: number = null,
    ): Observable<T> {
        const isCacheEnabled: boolean = !!cacheMaxAgeSeconds;

        if (isCacheEnabled) {
            let cachedData: any = this.getCachedApiResponse(url, cacheMaxAgeSeconds);
            if (cachedData) {
                return new Observable((observer: any) => {
                    observer.next(cachedData);
                    observer.complete();
                });
            } else if (this.hasActiveApiRequestUrl(url)){
                return this.getActiveApiRequestObservable(url);
            }
            this.addActiveApiRequestSubject(url);
        }

        return this.sendRequest(httpVerb, url, body, isCacheEnabled);
    }

    private sendRequest<T = any>(
        httpVerb: string,
        url: string,
        body: any,
        isCacheEnabled: boolean = false,
    ): Observable<T> {
        return this.httpClient.request<any>(
            httpVerb,
            url,
            {
                headers: this.generateHeaders(),
                body: body,
            })
            .pipe(
                finalize(() => {
                    if (isCacheEnabled) this.removeActiveApiRequestUrl(url);
                }),
                tap((data: any) => {
                    if (isCacheEnabled) {
                        this.setCachedApiResponse(url, data);
                        this.activeApiRequestSubjects.get(url).next(data);
                        this.activeApiRequestSubjects.get(url).complete();
                    }
                }),
            );
    }

    private generateHeaders(): HttpHeaders {
        return HttpHeadersFactory
            .create()
            .withContentType(MediaType.Json)
            .withAccept(MediaType.Json, MediaType.Text, MediaType.Any)
            .build();
    }
}
