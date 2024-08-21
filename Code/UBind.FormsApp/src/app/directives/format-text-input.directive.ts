import { Directive, HostListener, ElementRef, Input, OnInit, Output, EventEmitter } from '@angular/core';
import { CurrencyPipe } from '../pipes/currency.pipe';
import { TimePipe } from '../pipes/time.pipe';
import { PhoneNumberPipe } from '../pipes/phone-number.pipe';
import { CreditCardNumberPipe } from '../pipes/credit-card-number.pipe';
import { NumberPlatePipe } from '../pipes/number-plate.pipe';
import { AbnPipe } from '../pipes/abn.pipe';
import { BsbPipe } from '../pipes/bsb.pipe';
import { TextInputFormat } from '@app/models/text-input-format.enum';
import { Errors } from '@app/models/errors';
import { FormatTextInputPipe } from '@app/pipes/format-text-input-pipe';

/**
 * Export format text input directive class.
 * TODO: Write a better class header: Format text input functions.
 */
@Directive({
    selector: '[formatTextInput]',
})
export class FormatTextInputDirective implements OnInit {

    @Input('formatTextInput') public format: TextInputFormat;

    @Output() public cleanedValueEvent: EventEmitter<any> = new EventEmitter<any>();

    protected pipe: FormatTextInputPipe;

    public constructor(
        protected element: ElementRef,
        protected currencyPipe: CurrencyPipe,
        protected timePipe: TimePipe,
        protected phoneNumberPipe: PhoneNumberPipe,
        protected creditCardNumberPipe: CreditCardNumberPipe,
        protected abnPipe: AbnPipe,
        protected bsbPipe: BsbPipe,
        protected numberPlatePipe: NumberPlatePipe) {
    }

    public ngOnInit(): void {
        if (this.format) {
            this.pipe = this.getPipeForFormat(this.format);
            if (this.pipe) {
                this.element.nativeElement.value = this.pipe.transform(this.element.nativeElement.value);
            }
        }
    }

    @HostListener('focus', ['$event.target.value'])
    public onFocus(value: any): void {
        if (this.pipe) {
            this.element.nativeElement.value = this.pipe.restore(value);
        }
    }

    @HostListener('blur', ['$event.target.value'])
    public onBlur(value: any): void {
        if (this.pipe) {
            this.element.nativeElement.value = this.pipe.transform(value);
        }
    }

    @HostListener('input', ['$event.target.value'])
    public onInput(value: any): void {
        if (this.pipe && this.pipe.clean) {
            const cleanedValue: string = this.pipe.clean(value);
            if (value !== cleanedValue) {
                this.cleanedValueEvent.emit(cleanedValue);
            }
        }
    }

    private getPipeForFormat(format: TextInputFormat): any {
        switch (format) {
            case TextInputFormat.None:
                return null;
            case TextInputFormat.Currency:
                return this.currencyPipe;
            case TextInputFormat.Time:
                return this.timePipe;
            case TextInputFormat.PhoneNumber:
                return this.phoneNumberPipe;
            case TextInputFormat.CreditCardNumber:
                return this.creditCardNumberPipe;
            case TextInputFormat.Abn:
                return this.abnPipe;
            case TextInputFormat.Bsb:
                return this.bsbPipe;
            case TextInputFormat.NumberPlate:
                return this.numberPlatePipe;
            default:
                throw Errors.General.Unexpected(`When trying to get the pipe for the text input format "${format}, `
                    + 'we could not find a match. Please ensure you specify a supported text input format.');
        }
    }

}
