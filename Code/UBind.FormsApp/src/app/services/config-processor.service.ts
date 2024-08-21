import { Injectable } from '@angular/core';
import { MessageService } from './message.service';
import { EvaluateService } from './evaluate.service';
import { EventService } from './event.service';
import { WorkingConfiguration } from '@app/models/configuration/working-configuration';
import { Errors } from '@app/models/errors';
import { ConfigurationV2Processor } from './configuration-v2-processor';

/**
 * Export config processor service class.
 * TODO: Write a better class header: config processor functions.
 */
@Injectable()
export class ConfigProcessorService {

    private response: any = {};
    private firstConfigurationPublished: boolean = false;

    public constructor(
        protected messageService: MessageService,
        protected evaluateService: EvaluateService,
        protected eventService: EventService,
        private configV2Processor: ConfigurationV2Processor,
    ) {
    }

    public onConfigurationResponse(response: any): void {
        this.response = response;
        let config: WorkingConfiguration = null;
        if (response.status == 'success') {
            if (!response['version']) {
                throw Errors.Configuration.Version1NotSupported();
            } else if ('' + response['version'].startsWith('2.')) {
                config = this.configV2Processor.process(response);
                config.version = response['version'];
            } else {
                throw Errors.General.Unexpected(`Unknown configuration version "${response['versiom']}".`);
            }
            this.publishConfiguration(config);
            this.notifyInjectorThatConfigurationHasLoaded();
            this.eventService.appLoadedSubject.next(true);
        } else {
            let payload: any = {
                'message': "There was an error response when trying to retreive the configuration " +
                    "of the web form from the server. Please contact us.",
                'severity': 3,
            };
            this.messageService.sendMessage('displayMessage', payload);
        }
    }

    public sendWebFormLoadedMessage() {
        let payload: any = {
            'status': this.response['status'],
            'message': this.response['message'],
        };
        this.messageService.sendMessage('webFormLoad', payload);
        this.response = null;
    }

    private publishConfiguration(parsedConfig: WorkingConfiguration): void {
        if (!this.firstConfigurationPublished) {
            this.firstConfigurationPublished = true;
            this.eventService.loadedConfiguration.next(parsedConfig);
        } else {
            this.eventService.updatedConfiguration.next(parsedConfig);
        }
    }

    private notifyInjectorThatConfigurationHasLoaded(): void {
        let payload: any = {
            'status': this.response['status'],
            'message': this.response['message'],
        };
        this.messageService.sendMessage('configurationLoaded', payload);
    }

}
