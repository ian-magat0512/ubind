import { NgModule } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatDialogModule } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatListModule } from '@angular/material/list';
import { MatRadioModule } from '@angular/material/radio';
import { MatSelectModule } from '@angular/material/select';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatIconModule } from '@angular/material/icon';
import { MatIconRegistry } from '@angular/material/icon';
import { DomSanitizer } from '@angular/platform-browser';

/**
 * Module for angular material library to determine the
 * material component modules used in our application
 */
@NgModule({
    imports: [
        MatDialogModule,
        MatFormFieldModule,
        MatInputModule,
        MatButtonModule,
        MatRadioModule,
        MatListModule,
        MatSelectModule,
        MatIconModule,
        MatTooltipModule,
        MatCheckboxModule,
    ],
    exports: [
        MatDialogModule,
        MatFormFieldModule,
        MatInputModule,
        MatButtonModule,
        MatRadioModule,
        MatListModule,
        MatSelectModule,
        MatIconModule,
        MatTooltipModule,
        MatCheckboxModule,
    ],
})
export class MaterialModule {
    public constructor(matIconRegistry: MatIconRegistry, domSanitizer: DomSanitizer) {
        matIconRegistry.addSvgIconSet(
            domSanitizer.bypassSecurityTrustResourceUrl(
                "./assets/material-icon/mdi.svg",
            ),
        );
        matIconRegistry.addSvgIcon(
            "shield-add",
            domSanitizer.bypassSecurityTrustResourceUrl(
                "./assets/material-icon/shield-add.svg",
            ),
        );
        matIconRegistry.addSvgIcon(
            "shield-refresh",
            domSanitizer.bypassSecurityTrustResourceUrl(
                "./assets/material-icon/shield-refresh.svg",
            ),
        );
        matIconRegistry.addSvgIcon(
            "shield-ban",
            domSanitizer.bypassSecurityTrustResourceUrl(
                "./assets/material-icon/shield-ban.svg",
            ),
        );
        matIconRegistry.addSvgIcon(
            "shield-pen",
            domSanitizer.bypassSecurityTrustResourceUrl(
                "./assets/material-icon/shield-pen.svg",
            ),
        );
        matIconRegistry.addSvgIcon(
            "calculator-refresh",
            domSanitizer.bypassSecurityTrustResourceUrl(
                "./assets/material-icon/calculator-refresh.svg",
            ),
        );
        matIconRegistry.addSvgIcon(
            "calculator-ban",
            domSanitizer.bypassSecurityTrustResourceUrl(
                "./assets/material-icon/calculator-ban.svg",
            ),
        );
        matIconRegistry.addSvgIcon(
            "calculator-add",
            domSanitizer.bypassSecurityTrustResourceUrl(
                "./assets/material-icon/calculator-add.svg",
            ),
        );
        matIconRegistry.addSvgIcon(
            "calculator-pen",
            domSanitizer.bypassSecurityTrustResourceUrl(
                "./assets/material-icon/calculator-pen.svg",
            ),
        );
        matIconRegistry.addSvgIcon(
            "resume-quote",
            domSanitizer.bypassSecurityTrustResourceUrl(
                "./assets/material-icon/resume-quote.svg",
            ),
        );
        matIconRegistry.addSvgIcon(
            "cancel",
            domSanitizer.bypassSecurityTrustResourceUrl(
                "./assets/material-icon/cancel.svg",
            ),
        );
        matIconRegistry.addSvgIcon(
            "renew",
            domSanitizer.bypassSecurityTrustResourceUrl(
                "./assets/material-icon/renew.svg",
            ),
        );
        matIconRegistry.addSvgIcon(
            "numbers",
            domSanitizer.bypassSecurityTrustResourceUrl(
                "./assets/material-icon/numbers.svg",
            ),
        );
        matIconRegistry.addSvgIcon(
            "sync-calculator",
            domSanitizer.bypassSecurityTrustResourceUrl(
                "./assets/material-icon/sync-calculator.svg",
            ),
        );
        matIconRegistry.addSvgIcon(
            "sync-clipboard",
            domSanitizer.bypassSecurityTrustResourceUrl(
                "./assets/material-icon/sync-clipboard.svg",
            ),
        );
    }
}
