import { Component } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { NavProxyService } from '@app/services/nav-proxy.service';

/**
 * Export error page component class
 * This class is the Error Page
 */
@Component({
    selector: 'app-error',
    templateUrl: './error.page.html',
})
export class ErrorPage {
    public response: HttpErrorResponse = null;
    public log: any = null;

    public constructor(private navProxy: NavProxyService) {
    }

    public userDidTapBack(): void {
    }
}
