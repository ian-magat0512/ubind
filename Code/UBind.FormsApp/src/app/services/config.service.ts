import { Injectable } from '@angular/core';
import { ApplicationService } from './application.service';
import { BehaviorSubject } from 'rxjs';
import { QuestionMetadata } from '@app/models/question-metadata';
import { WorkingConfiguration } from '@app/models/configuration/working-configuration';
import { QuestionSets } from '@app/models/configuration/question-sets';
import { Settings } from '@app/models/configuration/settings';
import { EventService } from './event.service';
import { OptionConfiguration } from '@app/resource-models/configuration/option.configuration';
import { ThemeConfiguration } from '@app/resource-models/configuration/theme.configuration';
import { WorkflowStep } from '@app/models/configuration/workflow-step';

/**
 * Export config service class.
 * TODO: Write a better class header: config functions.
 */
@Injectable()

export class ConfigService {

    private _configuration: WorkingConfiguration;
    public configurationReadySubject: BehaviorSubject<boolean> = new BehaviorSubject<boolean>(false);
    private questionKeyToMetadataMap: Map<string, QuestionMetadata> = new Map<string, QuestionMetadata>();
    private repeatingQuestionSetKeyToMetadataMap: Map<string, Map<string, QuestionMetadata>>
        = new Map<string, Map<string, QuestionMetadata>>();

    public constructor(
        protected eventService: EventService,
        protected applicationService: ApplicationService,
    ) {
        this.eventService.loadedConfiguration.subscribe((config: WorkingConfiguration) => this.onConfiguration(config));
        this.eventService.updatedConfiguration
            .subscribe((config: WorkingConfiguration) => this.onConfiguration(config));
    }

    protected onConfiguration(config: WorkingConfiguration): void {
        this._configuration = config;
        this.mapMetadataToQuestionKeys();
        this.mapMetadataToRepeatingQuestionSetKeys();
        this.configurationReadySubject.next(true);
    }

    public get configuration(): WorkingConfiguration {
        return this._configuration;
    }

    public get questionSets(): QuestionSets {
        return this.configuration.questionSets;
    }

    public get repeatingQuestionSets(): QuestionSets {
        return this.configuration.repeatingQuestionSets;
    }

    public get formModel(): any {
        return this.configuration.formModel;
    }

    public set formModel(formModel: any) {
        this.configuration.formModel = formModel;
    }

    public get workflowSteps(): { [key: string]: WorkflowStep } {
        return this.configuration.workflowSteps;
    }

    public get workflowRoles(): any {
        return this.configuration.workflowRoles;
    }

    public get dataLocators(): any {
        return this.configuration.dataLocators || {};
    }

    public get contextEntities(): any {
        return this.configuration.contextEntities;
    }

    public get privateFieldKeys(): Array<string> {
        return this.configuration.privateFieldKeys || [];
    }

    public get settings(): Settings {
        return this.configuration.settings;
    }

    public get theme(): ThemeConfiguration {
        return this.configuration?.theme;
    }

    public get textElements(): any {
        return this.configuration.textElements;
    }

    public get styles(): any {
        return this.configuration.styles;
    }

    public get triggers(): any {
        return this.configuration.triggers;
    }

    public get sidebarSummaryKeyMapping(): any {
        return this.configuration.sidebarSummaryKeyMapping;
    }

    public get questionMetadata(): any {
        return this.configuration.questionMetadata;
    }

    public get mobileBreakPointPixels(): number {
        return this.configuration?.theme?.mobileBreakpointPixels ?? 767;
    }

    public get debug(): boolean {
        return this.applicationService.debug;
    }

    /**
     * Gets the metadata associated with a field at the top level (ie not a repeating field).
     * @param key the field key.
     */
    public getQuestionMetadata(key: string): QuestionMetadata {
        return this.questionKeyToMetadataMap.get(key);
    }

    public getRepeatingQuestionMetadata(parentKey: string, key: string): QuestionMetadata {
        let repeatingQuestionSetMap: Map<string, QuestionMetadata>
            = this.repeatingQuestionSetKeyToMetadataMap.get(parentKey);
        return repeatingQuestionSetMap
            ? repeatingQuestionSetMap.get(key)
            : null;
    }

    private mapMetadataToQuestionKeys(): void {
        if (this.configuration.questionMetadata && this.configuration.questionMetadata.questionSets) {
            // eslint-disable-next-line no-unused-vars
            for (let [questionSetKey, questionSet]
                of Object.entries(this.configuration.questionMetadata.questionSets)) {
                for (let [questionKey, metadata] of Object.entries(questionSet)) {
                    this.questionKeyToMetadataMap.set(questionKey, metadata as QuestionMetadata);
                }
            }
        }
    }

    private mapMetadataToRepeatingQuestionSetKeys(): void {
        if (this.configuration.questionMetadata && this.configuration.questionMetadata.repeatingQuestionSets) {
            for (let [repeatingQuestionSetKey, questionSet]
                of Object.entries(this.configuration.questionMetadata.repeatingQuestionSets)) {
                let map: Map<string, QuestionMetadata> = new Map<string, QuestionMetadata>();
                for (let [questionKey, metadata] of Object.entries(questionSet)) {
                    map.set(questionKey, metadata);
                }
                this.repeatingQuestionSetKeyToMetadataMap.set(repeatingQuestionSetKey, map);
            }
        }
    }

    public getOptions(optionSetKey: string): Array<OptionConfiguration> {
        return this._configuration.optionSets.get(optionSetKey);
    }
}
