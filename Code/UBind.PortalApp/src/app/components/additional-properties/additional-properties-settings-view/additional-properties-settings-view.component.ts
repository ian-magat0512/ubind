import { Component, Input, Output, OnChanges, SimpleChanges } from '@angular/core';
import {
    AdditionalPropertyDefinition,
} from '@app/models/additional-property-item-view.model';
import { EventEmitter } from '@angular/core';
import { EntityType } from '@app/models/entity-type.enum';
import { AdditionalPropertiesHelper } from '@app/helpers/additional-properties.helper';
import { AdditionalPropertyDefinitionContextType } from '@app/models/additional-property-context-type.enum';
import { IconLibrary } from '@app/models/icon-library.enum';
import {
    AdditionalPropertyDefinitionContextSettingItemViewModel,
} from '@app/viewmodels/additional-property-definition-context-setting-item.viewmodel';

/**
 * The view component for addtional properties inside under settings tab.
 */
@Component({
    selector: 'app-additional-properties-settings-view',
    templateUrl: './additional-properties-settings-view.component.html',
    styleUrls: ['./additional-properties-settings-view.component.scss'],
})
export class AdditionalPropertiesViewComponent implements OnChanges {

    @Input() public additionalPropertyDefinitions: Array<AdditionalPropertyDefinition>;
    @Input() public contextType: AdditionalPropertyDefinitionContextType;
    @Input() public isLoading: boolean;
    @Output() public handleOnClick: EventEmitter<any> = new EventEmitter<any>();

    public additionalPropertyContextSettingItems: Array<AdditionalPropertyDefinitionContextSettingItemViewModel> = [];
    public iconLibrary: typeof IconLibrary = IconLibrary;
    public constructor() {}

    public ngOnChanges(changes: SimpleChanges): void {
        this.updateAdditionalPropertyDefinitions();
    }

    private updateAdditionalPropertyDefinitions(): void {
        if (!this.additionalPropertyDefinitions) {
            return;
        }

        this.additionalPropertyContextSettingItems =
            AdditionalPropertiesHelper.generateAdditionalPropertyDefinitionsByContext(
                this.additionalPropertyDefinitions,
                this.contextType,
            );
    }

    public handleClick(entityType: EntityType): void {
        this.handleOnClick.emit(entityType);
    }
}
