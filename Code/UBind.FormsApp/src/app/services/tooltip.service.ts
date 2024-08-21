import { Injectable } from '@angular/core';
import { StringHelper } from '@app/helpers/string.helper';
import { WorkingConfiguration } from '@app/models/configuration/working-configuration';
import { Subject } from 'rxjs';
import { ConfigService } from './config.service';
import { EventService } from './event.service';

/**
 * Export ToolTip Service class.
 * This class is used to store and retrieve previous tooltip id.
 */
@Injectable()
export class ToolTipService {

    public toolTipChangedSubject: Subject<any> = new Subject<any>();
    public tooltipIcon: string = 'fa fa-question-circle';

    public constructor(
        private configService: ConfigService,
        private eventService: EventService,
    ) {
        this.eventService.loadedConfiguration
            .subscribe((config: WorkingConfiguration) => this.determineTooltipIcon());
        this.eventService.updatedConfiguration
            .subscribe((config: WorkingConfiguration) => this.determineTooltipIcon());
    }

    public toolTipChange(): void {
        this.toolTipChangedSubject.next();
    }

    private determineTooltipIcon(): void {
        const hasTooltipIconSetting: boolean = this.configService.theme
            && !StringHelper.isNullOrEmpty(this.configService.theme.tooltipIcon);
        this.tooltipIcon = hasTooltipIconSetting
            ? this.configService.theme.tooltipIcon
            : 'fa fa-question-circle';

        // TODO: Remove this in the future, once all products have synced
        // This fixes a bug in the config generation in case someone forgets to add the "fa" class
        if (this.tooltipIcon.indexOf('fa-question-circle') != -1) {
            if (this.tooltipIcon.indexOf('fa ') == -1) {
                this.tooltipIcon = `fa ${this.tooltipIcon}`;
            }
        }
    }
}
