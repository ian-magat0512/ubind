import { Component, Input, OnInit, ViewEncapsulation } from '@angular/core';
import { DomSanitizer, SafeHtml } from '@angular/platform-browser';
import { EmailDetailViewModel } from '@app/viewmodels/email-detail.viewmodel';
import * as DOMPurify from 'dompurify';

/**
 * Component for displaying email content, including HTML messages.
 * Sanitizes HTML content using DOMPurify to prevent XSS attacks.
 */
@Component({
    selector: 'app-email-content',
    templateUrl: './email-content.component.html',
    encapsulation: ViewEncapsulation.ShadowDom, // Apply Shadow DOM
})
export class EmailContentComponent implements OnInit {
    @Input() public email: EmailDetailViewModel;
    protected htmlMessage: SafeHtml;

    public constructor(protected sanitiser: DomSanitizer) {
    }

    public ngOnInit(): void {
        if (this.hasHtmlMessage()) {
            // eslint-disable-next-line @typescript-eslint/naming-convention
            const sanitisedHtml: string = DOMPurify.sanitize(this.email.htmlMessage, { FORCE_BODY: true });
            this.htmlMessage = this.sanitiser.bypassSecurityTrustHtml(sanitisedHtml);
        }
    }

    // Method to check if the email has HTML content
    public hasHtmlMessage(): boolean {
        return !!this.email && !!this.email.htmlMessage;
    }
}
