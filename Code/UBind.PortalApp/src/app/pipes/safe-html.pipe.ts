import { Pipe, PipeTransform } from '@angular/core';
import { DomSanitizer } from '@angular/platform-browser';
import { SecurityHelper } from '@app/helpers/security.helper';
import * as DOMPurify from 'dompurify';

/**
 * A pipe which sanitises html so that hackers can't embed bad things to compromise security
 */
@Pipe({
    name: 'safeHtml',
})
export class SafeHtmlPipe implements PipeTransform {

    public constructor(protected sanitizer: DomSanitizer) {
    }

    public transform(value: any, type: string): any {
        const sanitizedContent: any = DOMPurify.sanitize(value, SecurityHelper.getDomPurifyConfig());
        return this.sanitizer.bypassSecurityTrustHtml(sanitizedContent);
    }
}
