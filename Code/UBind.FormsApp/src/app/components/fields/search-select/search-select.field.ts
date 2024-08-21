import {
    Component, ElementRef, OnInit, AfterViewInit,
    Renderer2, ViewChild, ViewEncapsulation,
} from '@angular/core';
import { FormService } from '@app/services/form.service';
import { WorkflowService } from '@app/services/workflow.service';
import { WebhookService } from '@app/services/webhook.service';
import { ExpressionDependencies } from '@app/expressions/expression-dependencies';
import { ObservableArguments } from '@app/expressions/expression';
import { DomSanitizer } from '@angular/platform-browser';
import { BehaviorSubject, Observable, Subject, SubscriptionLike } from 'rxjs';
import { OptionsField } from '../options/options.field';
import { SelectOption } from '../options/select-option';
import { CalculationService } from '@app/services/calculation.service';
import { HttpClient } from '@angular/common/http';
import { NgSelectComponent } from '@ng-select/ng-select';
import { StringHelper } from '@app/helpers/string.helper';
import { Errors } from '@app/models/errors';
import * as _ from "lodash-es";
import { ApplicationService } from '@app/services/application.service';
import { FieldMetadataService } from '@app/services/field-metadata.service';
import { FieldType } from '@app/models/field-type.enum';
import { FieldSelector } from '@app/models/field-selectors.enum';
import { AbstractControl } from '@angular/forms';
import { takeUntil } from 'rxjs/operators';
import { ConfigService } from '@app/services/config.service';
import { EventService } from '@app/services/event.service';
import { FieldConfiguration } from '@app/resource-models/configuration/fields/field.configuration';
import { FieldEventLogRegistry } from '@app/services/debug/field-event-log-registry';
import { SearchSelectFieldConfiguration } from
    '@app/resource-models/configuration/fields/search-select-field-configuration';
/**
 * Event args for when a search term is typed.
 */
export interface SearchTermTypedEvent {
    term: string;
    items: Array<SelectOption>;
}

/**
 * Export search select field component class.
 * TODO: Write a better class header: search select field functions.
 */
@Component({
    selector: '' + FieldSelector.SearchSelect,
    templateUrl: './search-select.field.html',
    styleUrls: [
        './search-select.field.scss',
    ],
    encapsulation: ViewEncapsulation.None, // styles will be global
})
export class SearchSelectField extends OptionsField implements OnInit, AfterViewInit {
    public value: any;
    public selectedItem: any;
    public selectedValue: any;
    public inputFieldElement: HTMLInputElement;
    private clearButtonHtml: string = "<span class='ng-clear glyphicon glyphicon-remove'></span>";
    private initialSearchIconHtml: string = "";
    private searchSelectElement: HTMLElement;
    private ngSelectElement: HTMLElement;
    public searchTerm: string = '';
    public visibleSelectOptions: Array<SelectOption>;
    protected selectedOption: SelectOption;
    private isFocusing: boolean;
    public resizeObservable$: Observable<Event>;
    private resizeSubject: Subject<Event> = new Subject<Event>();
    private calculatedValueMatchingValueSubscription: SubscriptionLike;
    public isVirtualScrollEnabled: boolean = false;

    /**
     * This is needed so that we can pass the current search term to expressions.
     */
    private _searchTermSubject: BehaviorSubject<string>;

    /**
     * We need to store the field path used, so that if the field path changes
     * (e.g. during a repeating question set change) then we can create a new
     * subject with the new field path.
     */
    private fieldPathForSearchTermSubject: string;

    /**
     * This is needed so that we can pass the current field input value to expressions.
     */
    private fieldInputValueSubject: BehaviorSubject<string> = new BehaviorSubject<string>('');

    private classNames: any = {
        ngValueContainer: "ng-value-container",
        ngInput: "ng-input",
        ngClear: "ng-clear",
        ngInvalid: "ng-invalid",
        ngValid: "ng-valid",
        glyphiconRemove: 'glyphicon-remove',
        ngDropdownPanel: "ng-dropdown-panel",
        ngClearWrapper: "ng-clear-wrapper",
        ngArrowWrapper: "ng-arrow-wrapper",
        inputGroupAddon: "input-group-addon",
        ngOptionLabel: "ng-option",
        ngOptionMarked: "ng-option-marked",
    };

    private styleConstants: any = {
        display: "display",
        width: "width",
        hidden: "hidden",
        none: "none",
        color: "color",
        backgroundColor: "background-color",
        transparent: "transparent",
        paddingRight: "padding-right",
    };

    @ViewChild("searchLoader", { static: true }) public searchLoaderElement: ElementRef;
    @ViewChild("ngSelect", { read: NgSelectComponent, static: true }) public ngSelectComponent: NgSelectComponent;

    public constructor(
        public elementRef: ElementRef,
        public renderer: Renderer2,
        formService: FormService,
        webhookService: WebhookService,
        workflowService: WorkflowService,
        expressionDependencies: ExpressionDependencies,
        sanitizer: DomSanitizer,
        httpClient: HttpClient,
        calculationService: CalculationService,
        applicationService: ApplicationService,
        fieldMetadataService: FieldMetadataService,
        protected configService: ConfigService,
        protected eventService: EventService,
        fieldEventLogRegistry: FieldEventLogRegistry,
    ) {
        super(
            formService,
            webhookService,
            workflowService,
            expressionDependencies,
            sanitizer,
            httpClient,
            calculationService,
            applicationService,
            fieldMetadataService,
            eventService,
            configService,
            fieldEventLogRegistry,
        );
        this.fieldType = FieldType.SearchSelect;
    }

    private get searchSelectFieldConfiguration(): SearchSelectFieldConfiguration {
        return this.field.templateOptions.fieldConfiguration;
    }

    public ngOnInit(): void {
        // replace the showClear function so that ng-select always shows it, because we will show/hide it with css.
        // We need to do this because we will change the html of it and we don't want ng-select to 
        // keep removing and adding it to the DOM as we would be constantly having to change it's html.
        this.ngSelectComponent.showClear = (): boolean => {
            return true;
        };

        // get the HTML Elements
        this.searchSelectElement = this.elementRef.nativeElement;
        this.ngSelectElement = <HTMLElement>(this.searchSelectElement.getElementsByTagName('ng-select')[0]);
        this.inputFieldElement = <HTMLInputElement>(this.searchSelectElement.getElementsByClassName(
            this.classNames.ngInput)[0].firstElementChild);

        super.ngOnInit();
        // so that we don't confuse ng-select, if it's empty we'll set the value to null
        if (this.formControl.value == '') {
            this.formControl.setValue(null);
        }

        this.inputFieldElement.setAttribute('aria-label', this.ariaLabelByKey);
        this.elementRef.nativeElement.parentNode.classList.add("search-input");
    }

    protected initialiseField(): void {
        super.initialiseField();
        this.initialised = false;
        if (this.doesGetSelectedOptionFromApi) {
            if (this.selectedOption) {
                this.inputFieldElement.value = this.selectedOption.label;
            }
        } else {
            this.selectOptionMatchingValue(this.initialValue);
        }

        // Set the last known search term on the field
        this.searchTerm = this.searchTermSubject.value;
        // When the value is calculated, we want to select the matching option and display it
        this.listenForCalculatedValueAndSelectMatchingOption();
        this.applyEmptyAndNotEmptyCssClasses();
        this.initialised = true;
    }

    protected onConfigurationUpdated(): void {
        super.onConfigurationUpdated();
        this.eventService.getFieldConfigUpdatedObservable(<string>this.field.key).pipe(takeUntil(this.destroyed))
            .subscribe((configs: { old: FieldConfiguration; new: FieldConfiguration}) => {
                if (!this.doesGetSelectedOptionFromApi) {
                    this.selectOptionMatchingValue(this.initialValue);
                    this.listenForCalculatedValueAndSelectMatchingOption();
                }
            });
    }

    private listenForCalculatedValueAndSelectMatchingOption(): void {
        if (this.calculatedValueSubject) {
            if (this.calculatedValueMatchingValueSubscription) {
                this.calculatedValueMatchingValueSubscription.unsubscribe();
            }
            this.calculatedValueMatchingValueSubscription
                = this.calculatedValueSubject.pipe(takeUntil(this.destroyed))
                    .subscribe((value: string) => {
                        if(!value) {
                            this.clearSelection();
                        } else {
                            this.selectOptionMatchingValue(value);
                        }
                    });
        }
    }

    protected applyDefaultPlaceholderText(): void {
        this.placeholderText = 'Type to search...';
    }

    /**
     * Find the option matching the given value and pre-selects it in the list
     * @param string 
     * @param value 
     */
    private selectOptionMatchingValue(value: string): void {
        let matchingOptions: Array<SelectOption>
            = this.selectOptions.filter((option: SelectOption) => option.value == value);
        if (matchingOptions.length) {
            this.onSelectedOptionLoaded(matchingOptions[0]);
            // The set timeout is needed because otherwise angualr doesn't update the field
            setTimeout(() => {
                this.inputFieldElement.value = matchingOptions[0].label;
            }, 10);
        }
    }

    public ngAfterViewInit(): void {
        this.applyClearButtonHtml();

        // set the search term on the field if we already have one
        if (StringHelper.isNullOrEmpty(this.inputFieldElement.value)
            && !StringHelper.isNullOrEmpty(this.searchTerm)
            && !this.doesGetSelectedOptionFromApi
        ) {
            this.inputFieldElement.value = this.searchTerm;
            this.ngSelectComponent.writeValue(this.searchTerm);
        }

        // this is need to ensure it shows the clear x button
        setTimeout(() => this.applyEmptyAndNotEmptyCssClasses(), 0);
    }

    public get searchTermSubject(): BehaviorSubject<any> {
        if (!this._searchTermSubject || this.fieldPath != this.fieldPathForSearchTermSubject) {
            // store the field path for the field search term subject, so that if the field path changes, 
            // we can know that we need to get a new fieldSearchTermSubject
            this.fieldPathForSearchTermSubject = this.fieldPath;
            this._searchTermSubject =
                this.expressionDependencies.expressionInputSubjectService.getFieldSearchTermSubject(
                    this.fieldPath,
                    this.searchTerm);
        }
        return this._searchTermSubject;
    }

    public publishSearchTermForExpressions(): void {
        this.searchTermSubject.next(this.searchTerm);
    }

    private applyClearButtonHtml(): void {
        let clearButtonElement: HTMLElement = <HTMLElement>this.searchSelectElement.getElementsByClassName(
            this.classNames.ngClearWrapper)[0];
        if (clearButtonElement) {
            clearButtonElement.innerHTML = this.clearButtonHtml;
        } else {
            throw Errors.General.Unexpected(
                "An attempt was made to apply ubind clear button html to the ng-select component, "
                + "however the clear button wrapper element was not found in the DOM. "
                + "Please lodge a bug report for this.");
        }
    }

    public onChangeSelection(option: SelectOption): void {
        this.selectedOption = option;
        if (option) {
            option = this.extendOptionProperties(option);
            this.data = option.properties || option;
            this.inputFieldElement.value = option.label;
            this.ngSelectComponent.detectChanges();
        } else {
            this.inputFieldElement.value = null;
        }
        this.applyEmptyAndNotEmptyCssClasses();
        this.onChange();
    }

    public extendOptionProperties(option: SelectOption): SelectOption {
        if (option.properties) {
            option.properties['label'] ||= option.label;
            option.properties['value'] ||= option.value;
        }
        return option;
    }

    public onTypeSearchTerm($event: SearchTermTypedEvent): void {
        if (this.isFocusing) {
            // we simulate typing when focusing so that we can get the field to filter the list,
            // so we don't want to do normal processing as though someone we typing, since they weren't.
            return;
        }

        this.searchTerm = $event.term;
        this.publishSearchTermForExpressions();
        this.fieldInputValueSubject.next($event.term);
        let matchedOption: SelectOption = this.getOptionWhereTermMatchesLabel($event);
        if (matchedOption) {
            this.formControl.setValue(matchedOption.value);
            this.selectedItem = matchedOption.value;
            this.data = matchedOption.properties;
        } else {
            this.selectedOption = null;
            this.formControl.setValue(null);
        }
        this.onChange();
        this.applyEmptyAndNotEmptyCssClasses();
    }

    private applyEmptyAndNotEmptyCssClasses(): void {
        if (StringHelper.isNullOrEmpty(this.inputFieldElement.value)) {
            this.renderer.removeClass(this.ngSelectElement, 'not-empty');
            this.renderer.addClass(this.ngSelectElement, 'empty');
        } else {
            this.renderer.removeClass(this.ngSelectElement, 'empty');
            this.renderer.addClass(this.ngSelectElement, 'not-empty');
        }
    }

    /**
     * If the search term is an exact match for one of the filtered values, then return that option,
     * otherwise return null.
     */
    private getOptionWhereTermMatchesLabel($event: SearchTermTypedEvent): SelectOption {
        for (let option of $event.items) {
            if ($event.term == option.label) {
                return option;
            }
        }
        return null;
    }

    public onBlur(event: any): void {
        super.onBlur(event);
        this.ngSelectComponent.close();
    }

    public onClose(): void {
        let inputFieldValueSaved: string = this.inputFieldElement.value;
        // keep the search text in the input field so it doesn't go empty
        // The set timeout is needed because otherwise the input value is cleared after setting it by ngSelect
        setTimeout(() => {
            if (!StringHelper.isNullOrEmpty(inputFieldValueSaved)) {
                this.inputFieldElement.value = inputFieldValueSaved;
            }
        }, 10);
    }

    public onFocus(event: any): void {
        this.isFocusing = true;
        this.inputFieldElement.dispatchEvent(new Event('input'));
        this.isFocusing = false;
        super.onFocus(event);
    }

    public onClear(): void {
        if (this.formControl.disabled) {
            return;
        }
        this.searchTerm = '';
        this.selectedOption = null;
        this.inputFieldElement.value = '';
        this.ngSelectComponent.close();
        this.ngSelectComponent.open();
        this.publishSearchTermForExpressions();
        this.applyEmptyAndNotEmptyCssClasses();
    }

    protected onApiRequestStart(): void {
        this.setProgressIcon(true);
    }

    protected onApiRequestFinish(): void {
        if (this.activeApiCalls == 0) {
            this.setProgressIcon(false);
        }
    }

    private setProgressIcon(inProgress: any): void {
        let parentElement: HTMLElement = this.elementRef.nativeElement.parentElement;
        if (parentElement) {
            let inputGroupAddOn: HTMLElement =
                <HTMLElement>parentElement.getElementsByClassName(this.classNames.inputGroupAddon)[0];
            if (inputGroupAddOn) {
                if (inProgress && inputGroupAddOn.innerHTML == this.searchLoaderElement.nativeElement.innerHTML) return;
                if (inputGroupAddOn) {
                    if (this.initialSearchIconHtml.length == 0) {
                        this.initialSearchIconHtml = inputGroupAddOn.innerHTML;
                    }
                    if (inProgress) {
                        inputGroupAddOn.innerHTML = this.searchLoaderElement.nativeElement.innerHTML;
                    } else {
                        inputGroupAddOn.innerHTML = this.initialSearchIconHtml;
                    }
                    inputGroupAddOn.style[this.styleConstants.color] = "#666666";
                    inputGroupAddOn.style[this.styleConstants.backgroundColor] = this.styleConstants.transparent;
                }
            }
        }
    }

    protected onSelectedOptionLoaded(option: SelectOption): void {
        if (option) {
            this.onChangeSelection(option);
            this.selectedItem = option.value;
            this.fieldInputValueSubject.next(this.inputFieldElement.value);
        }
        super.onSelectedOptionLoaded(option);
    }

    /**
     * Can be overriden by child classes to provide additional observable arguments.
     * For example the search select may want to also provide the current search term 
     * so it can be used in expressions.
     */
    public getObservableExpressionArguments(): ObservableArguments {
        let parentObservableArguments: ObservableArguments = super.getObservableExpressionArguments();
        let thisObservableArguments: ObservableArguments = {
            fieldInputValue: this.fieldInputValueSubject.asObservable(),
        };
        // merge the two objects
        return _.merge({}, thisObservableArguments, parentObservableArguments);
    }

    public optionMatches(filterString: string, option: SelectOption): boolean {
        // we match all options unless we have been told to specifically filter by something.
        if (this.searchSelectFieldConfiguration.searchTextExpression) {
            return super.optionMatches(filterString, option);
        } else {
            return true;
        }
    }

    protected onSelectOptionsListChange(): void {
        this.visibleSelectOptions = this.selectOptions.filter((selectOption: SelectOption) => selectOption.render);
        this.isVirtualScrollEnabled = this.visibleSelectOptions.length > 200;
    }

    /**
     * Called when the currently selected select option becomes hidden
     */
    protected onSelectedOptionHidden(selectOption: SelectOption): void {
        this.clearSelection();
    }

    protected onSelectedOptionDisabled(): void {
        this.clearSelection();
    }

    private clearSelection(): void {
        this.onChangeSelection(null);
        setTimeout(
            () => {
                let formControl: AbstractControl = this.form.controls[this.fieldKey];
                formControl.setValue(null);
                formControl.markAsUntouched();
                formControl.markAsPristine();
                this.onChange();
            }, 0);
    }

    /**
     * Recreate the expressions associated with fields in this question set.
     * This would normally only be called if the question set was part of a repeating set and one was removed.
     */
    public recreateExpressions(): void {
        this.publishSearchTermForExpressions();
        super.recreateExpressions();
    }
}
