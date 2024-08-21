import { Component, HostBinding, OnInit, ViewChild, ViewContainerRef } from '@angular/core';
import { FieldConfiguration } from '@app/resource-models/configuration/fields/field.configuration';
import { VisibleFieldConfiguration } from '@app/resource-models/configuration/fields/visible-field.configuration';
import { EventService } from '@app/services/event.service';
import { takeUntil } from 'rxjs/operators';
import { Wrapper } from '../wrapper';

/**
 * Export container wrapper component class.
 * This class manage container wrapper functions.
 */
@Component({
    selector: 'container-wrapper',
    templateUrl: './container.wrapper.html',
})

export class ContainerWrapper extends Wrapper implements OnInit {

    @ViewChild('fieldComponent', { read: ViewContainerRef, static: true }) public fieldComponent: ViewContainerRef;

    @HostBinding('class')
    public classes: string;

    @HostBinding('style')
    public styles: string;

    private visibleFieldConfiguration: VisibleFieldConfiguration;

    public constructor(
        private eventService: EventService,
    ) {
        super();
    }

    public ngOnInit(): void {
        this.visibleFieldConfiguration = this.field.templateOptions.fieldConfiguration;
        super.ngOnInit();
        this.generateContainerClasses();
        this.generateStyles();
        this.onConfigurationUpdated();
    }

    private onConfigurationUpdated(): void {
        this.eventService.getFieldConfigUpdatedObservable(this.fieldKey).pipe(takeUntil(this.destroyed))
            .subscribe((configs: { old: FieldConfiguration; new: FieldConfiguration}) => {
                this.visibleFieldConfiguration = <VisibleFieldConfiguration>configs.new;
                this.generateContainerClasses();
                this.generateStyles();
            });
    }

    private generateContainerClasses(): void {
        let className: string = 'field-container';
        if (this.visibleFieldConfiguration.containerClass) {
            className += ' ' + this.visibleFieldConfiguration.containerClass;
        }
        this.classes = className;
    }

    private generateStyles(): void {
        let styles: any = {};
        if (this.visibleFieldConfiguration.containerCss) {
            let properties: any = this.visibleFieldConfiguration.containerCss.split(';');
            for (let property of properties) {
                styles[property.split(':')[0]] = property.split(':')[1];
            }
        }
        const widgetCssWidth: string = this.visibleFieldConfiguration.widgetCssWidth;
        if (widgetCssWidth) {
            styles['width'] = widgetCssWidth;
        }
        this.styles = styles;
    }
}
