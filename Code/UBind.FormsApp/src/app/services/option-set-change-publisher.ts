import { Injectable } from "@angular/core";
import { WorkingConfiguration } from "@app/models/configuration/working-configuration";
import { OptionConfiguration } from "@app/resource-models/configuration/option.configuration";
import { EventService } from "./event.service";
import * as _ from 'lodash-es';

/**
 * Detects changes to option sets and publishes them.
 * This is used when reloading the configuration.
 */
@Injectable({
    providedIn: 'root',
})
export class OptionSetChangePublisher {

    private optionSets: Map<string, Array<OptionConfiguration>>;

    public constructor(
        private eventService: EventService,
    ) {
        this.onConfigurationLoaded();
        this.onConfigurationUpdated();
    }

    private onConfigurationLoaded(): void {
        this.eventService.loadedConfiguration.subscribe((config: WorkingConfiguration) => {
            this.optionSets = config.optionSets;
        });
    }

    private onConfigurationUpdated(): void {
        this.eventService.updatedConfiguration.subscribe((config: WorkingConfiguration) => {
            this.optionSets.forEach((existingOptions: Array<OptionConfiguration>, optionSetKey: string) => {
                let newOptions: Array<OptionConfiguration> = config.optionSets.get(optionSetKey);
                if (newOptions == null) {
                    this.eventService.getOptionSetUpdatedSubject(optionSetKey).next(null);
                } else if (!_.isEqual(existingOptions, newOptions)) {
                    this.eventService.getOptionSetUpdatedSubject(optionSetKey).next(newOptions);
                }
            });

            this.optionSets = config.optionSets;
        });
    }
}
