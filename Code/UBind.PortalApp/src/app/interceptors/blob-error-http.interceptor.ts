import { Injectable } from '@angular/core';
import { HttpInterceptor, HttpHandler, HttpRequest, HttpEvent, HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';

/**
 * Sometimes the browser interprets the ProblemDetails as a blob, and not json.
 * This fixes that by parsing it as json.
 */
@Injectable()
export class BlobErrorHttpInterceptor implements HttpInterceptor {
    public intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
        return next.handle(req).pipe(
            catchError((err: any) => {
                if (err instanceof HttpErrorResponse
                    && err.error instanceof Blob
                    && (err.error.type === "application/json" || err.error.type === "application/problem+json")
                ) {
                    // https://github.com/angular/angular/issues/19888
                    // When request of type Blob, the error is also in Blob instead of object of the json data
                    return new Promise<any>((_resolve: any, reject: any): void => {
                        let reader: FileReader = new FileReader();
                        reader.onload = (e: Event): void => {
                            try {
                                const errData: any = JSON.parse((<any>e.target).result);
                                reject(new HttpErrorResponse({
                                    error: errData,
                                    headers: err.headers,
                                    status: err.status,
                                    statusText: err.statusText,
                                    url: err.url,
                                }));
                            } catch (e) {
                                reject(err);
                            }
                        };
                        reader.onerror = (e: Event): void => {
                            reject(err);
                        };
                        reader.readAsText(err.error);
                    });
                }
                return throwError(err);
            }),
        );
    }
}
