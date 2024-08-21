import { Component, ViewChild, ViewContainerRef, ElementRef, OnInit, HostBinding, AfterViewInit } from '@angular/core';
import { ExpressionDependencies } from '@app/expressions/expression-dependencies';
import { FieldDataType } from '@app/models/field-data-type.enum';
import { QuestionMetadata } from '@app/models/question-metadata';
import { FieldMetadataService } from '@app/services/field-metadata.service';
import { takeUntil } from 'rxjs/operators';
import { merge } from "rxjs";
import { Wrapper } from '../wrapper';
import { FieldHelper } from '@app/helpers/field.helper';

/**
 * Export addons wrapper component class.
 * TODO: Write a better class header: addons wrapper functions.
 */
@Component({
    selector: 'addons-wrapper',
    templateUrl: './addons.wrapper.html',
})
export class AddonsWrapper extends Wrapper implements OnInit, AfterViewInit {
    @ViewChild('fieldComponent', { read: ViewContainerRef, static: true }) public fieldComponent: ViewContainerRef;
    @ViewChild('addOnleft') public addOnleft: ElementRef;
    @ViewChild('addOnRight') public addOnRight: ElementRef;

    protected isAddOnlefthasCustomBgColor: boolean = false;
    protected isAddOnRighthasCustomBgColor: boolean = false;
    protected hideBgColorOfAddonLeft: boolean = false;
    protected hideBgColorOfAddonRight: boolean = false;

    /**
     * Indicates whether the addon will display a currency code as text, so that
     * sufficient padding can be added to the field by default.
     */
    @HostBinding('class.currency-code') public hasCurrencyCode: boolean = false;

    /**
     * Indicates whether the addon will display a currency code as text, so that
     * sufficient padding can be added to the field by default.
     */
    @HostBinding('class.input-group') public hasInputGroupClass: boolean = true;


    public constructor(
        private elementRef: ElementRef,
        private expressionDependencies: ExpressionDependencies,
        private fieldMetadataService: FieldMetadataService,
    ) {
        super();
    }

    public ngOnInit(): void {
        super.ngOnInit();
        let metadata: QuestionMetadata = this.fieldMetadataService.getMetadataForField(this.fieldInstance.fieldPath);
        if (metadata && metadata.dataType == FieldDataType.Currency) {
            if (!this.field.templateOptions.addonLeft
                || (!this.field.templateOptions.addonLeft.class && !this.field.templateOptions.addonLeft.text)
            ) {
                this.applyCurrencySymbol(metadata);
            }
        }
    }

    public focusInput(): void {
        FieldHelper.getFieldElement(this.fieldInstance.fieldType, this.elementRef.nativeElement).focus();
    }

    public applyCurrencySymbol(metadata: QuestionMetadata): void {
        let currencyCode: string = metadata.currencyCode
            || this.expressionDependencies.expressionMethodService.getCurrencyCode();
        switch (currencyCode) {
            case 'ARS':
            case 'AUD':
            case 'BSD':
            case 'BBD':
            case 'BMD':
            case 'BND':
            case 'KHR':
            case 'CAD':
            case 'KYD':
            case 'CLP':
            case 'COP':
            case 'XCD':
            case 'SVC':
            case 'FJD':
            case 'GYD':
            case 'HKD':
            case 'LRD':
            case 'NAD':
            case 'NZD':
            case 'SGD':
            case 'SBD':
            case 'SRD':
            case 'TVD':
            case 'USD':
                this.field.templateOptions.addonLeft = {
                    class: 'fa fa-usd',
                };
                break;
            case 'PGK':
                this.field.templateOptions.addonLeft = {
                    class: 'icon-kina',
                };
                break;
            default:
                this.field.templateOptions.addonLeft = {
                    text: currencyCode,
                    class: 'currency-code',
                };
                this.hasCurrencyCode = true;
        }
    }

    public ngAfterViewInit(): void {
        // if workbook has custom background on the addon, preserve the background
        // if not, then we will override the bootstrap background color
        if (this.addOnleft && this.addOnleft.nativeElement) {
            const backGroundColor: string = window.getComputedStyle(this.addOnleft.nativeElement).backgroundColor;
            this.isAddOnlefthasCustomBgColor = backGroundColor !== 'rgba(0, 0, 0, 0)';
        }

        if (this.addOnRight && this.addOnRight.nativeElement) {
            const backGroundColor: string = window.getComputedStyle(this.addOnRight.nativeElement).backgroundColor;
            this.isAddOnRighthasCustomBgColor = backGroundColor !== 'rgba(0, 0, 0, 0)';
        }

        merge(
            this.formControl.statusChanges,
            this.formControl.valueChanges,
            this.fieldInstance.onTouchedSubject,
        )
            .pipe(takeUntil(this.destroyed))
            .subscribe(() => {
                const isInvalid: boolean = this.formControl.invalid && this.formControl.touched;
                if (this.addOnleft && (this.showError || isInvalid)) {
                    this.hideBgColorOfAddonLeft = !this.isAddOnlefthasCustomBgColor;
                }
                if (this.addOnRight && (this.showError || isInvalid)) {
                    this.hideBgColorOfAddonRight = !this.isAddOnRighthasCustomBgColor;
                }
            });
    }

}
