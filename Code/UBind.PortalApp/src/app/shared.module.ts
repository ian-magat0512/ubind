import { NgModule } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { IonicModule } from '@ionic/angular';
import { CustomValidators } from './helpers/custom-validators';
import { AllowAccessDirective } from './directives/allow-access.directive';
import { FocusOnShowDirective } from './directives/focus-element.directive';
import { ReplacePipe } from './pipes/replace.pipe';
import { PhoneNumberPipe } from './pipes/phone-number.pipe';
import { ScrollDetailsDirective } from './directives/scroll-details-list.directive';
import { CurrencyPipe } from './pipes/currency.pipe';
import { BeautifyPipe } from './pipes/beautify.pipe';
import { ProfilePicUrlPipe } from './pipes/profile-pic-url.pipe';
import { SafeStylePipe } from './pipes/safe-style.pipe';
import { PoweredByComponent } from './components/powered-by/powered-by.component';
import { MaterialModule } from './material.module';
import { EnvironmentControlComponent } from './components/environment-control/environment-control.component';
import { SafeHtmlPipe } from './pipes/safe-html.pipe';
import { IonSegmentButtonDirective } from './directives/ion-segment-button.directive';
import { CssIdentifierPipe } from './pipes/css-identifier.pipe';

/**
 * A module which loads components which are shared and used by many other modules.
 */
@NgModule({
    declarations: [
        AllowAccessDirective,
        FocusOnShowDirective,
        IonSegmentButtonDirective,
        ReplacePipe,
        BeautifyPipe,
        PhoneNumberPipe,
        CurrencyPipe,
        ScrollDetailsDirective,
        ProfilePicUrlPipe,
        SafeStylePipe,
        SafeHtmlPipe,
        ScrollDetailsDirective,
        PoweredByComponent,
        EnvironmentControlComponent,
        CssIdentifierPipe,
    ],
    imports: [
        CommonModule,
        FormsModule,
        ReactiveFormsModule,
        IonicModule,
        MaterialModule,
    ],
    exports: [
        CommonModule,
        FormsModule,
        ReactiveFormsModule,
        IonicModule,
        MaterialModule,
        AllowAccessDirective,
        FocusOnShowDirective,
        IonSegmentButtonDirective,
        ReplacePipe,
        BeautifyPipe,
        PhoneNumberPipe,
        CurrencyPipe,
        ProfilePicUrlPipe,
        SafeStylePipe,
        SafeHtmlPipe,
        ScrollDetailsDirective,
        PoweredByComponent,
        EnvironmentControlComponent,
        CssIdentifierPipe,
    ],
    providers: [
        CustomValidators,
        DatePipe,
        ProfilePicUrlPipe,
        SafeStylePipe,
        SafeHtmlPipe,
    ],
})
export class SharedModule { }
