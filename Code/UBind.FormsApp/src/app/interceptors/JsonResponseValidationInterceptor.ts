
import { tap } from 'rxjs/operators';
import { Injectable } from '@angular/core';
import { HttpInterceptor, HttpHandler, HttpRequest, HttpEvent, HttpResponse } from '@angular/common/http';
import { Observable } from 'rxjs';
import { MessageService } from '../services/message.service';
import { EventService } from '@app/services/event.service';

/**
 * Export JSON response validation interceptor class.
 * TODO: Write a better class header: JSON response validation functions.
 */
@Injectable()
export class JsonResponseValidationInterceptor implements HttpInterceptor {

    private appLoaded: boolean = false;

    public constructor(
        private messageService: MessageService,
        private eventService: EventService,
    ) {
        this.listenForAppLoadedEvent();
    }

    private listenForAppLoadedEvent(): void {
        this.eventService.appLoadedSubject.subscribe((appLoaded: boolean) => this.appLoaded = appLoaded);
    }

    public intercept(
        req: HttpRequest<any>,
        next: HttpHandler,
    ): Observable<HttpEvent<any>> {
        return next.handle(req).pipe(tap((evt: any) => {
            if (evt instanceof HttpResponse) {
                // if response type is json, check that it's not null
                if (req.responseType == 'json' && evt.body == null && evt.status != 204) {
                    let message: string = "A null body was received when expecting a structured json object. "
                        + 'This typically happens when the json object is invalid, and therefore cannot be '
                        + 'parsed by the browser. Check all json configuration files in a json validation '
                        + 'tool (e.g. JSLint) and correct any errors found.';
                    if (this.appLoaded) {
                        console.warn(message);
                    } else {
                        let payload: any = {
                            'message': message,
                            'severity': 4,
                        };
                        this.messageService.sendMessage('displayMessage', payload);
                    }
                }
            }
        }));
    }
}
