import { Pipe, PipeTransform } from '@angular/core';
import { DomSanitizer, SafeStyle } from '@angular/platform-browser';

/**
 * Export Safe style pipe class.
 * TODO: Write a better class header: safestyle pipe function.
 */
@Pipe({ name: 'safeStyle' })
export class SafeStylePipe implements PipeTransform {
    public constructor(private sanitizer: DomSanitizer) { }

    public transform(style: string): SafeStyle {
        return this.sanitizer.bypassSecurityTrustStyle(style);
    }
}
