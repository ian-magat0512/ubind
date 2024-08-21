import { Injectable } from "@angular/core";
import { TriggerDisplayConfig } from "@app/models/trigger-display-config";

/**
 * Export trigger service class.
 * TODO: Write a better class header: trigger service functions.
 */
@Injectable({
    providedIn: 'root',
})
export class TriggerService {

    /**
     * A list of display configurations of the currently active triggers.
     * This is needed for two things:
     * 1. Expression methods getActiveTrigger and getActiveTriggerByType
     * 2. The calculation widget, which displays the details of the highest priority active trigger.
     */
    public activeTriggerDisplayConfigs: Array<TriggerDisplayConfig>;

    /**
     * The highest precedence trigger that's active.
     * This is typically displayed in the calculation widget
     */
    public activeTrigger: TriggerDisplayConfig;

    /**
     * Returns the first active trigger of the given type.
     * @param triggerType the type of the trigger, as a string
     */
    public getFirstActiveTriggerByType(triggerType: string): TriggerDisplayConfig {
        if (this.activeTriggerDisplayConfigs) {
            for (let triggerDisplayConfig of this.activeTriggerDisplayConfigs) {
                if (triggerDisplayConfig.type == triggerType) {
                    return triggerDisplayConfig;
                }
            }
        }

        // if there are no active triggers of this type, return null.
        return null;
    }

    public getActiveTriggersByType(triggerType: string): Array<TriggerDisplayConfig> {
        return this.activeTriggerDisplayConfigs
            ? this.activeTriggerDisplayConfigs.filter((t: TriggerDisplayConfig) => t.type == triggerType)
            : new Array<TriggerDisplayConfig>();
    }
}
