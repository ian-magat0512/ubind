import { Injectable } from "@angular/core";
import { ExpressionDependencies } from "@app/expressions/expression-dependencies";
import { TextWithExpressions } from "@app/expressions/text-with-expressions";
import { StringHelper } from "@app/helpers/string.helper";
import { WorkingConfiguration } from "@app/models/configuration/working-configuration";
import { filter } from "rxjs/operators";
import { ApplicationService } from "./application.service";
import { ConfigService } from "./config.service";
import { EventService } from "./event.service";

/**
 * Simple representation of a style sheet configuration
 */
export interface StyleSheet {
    id: string;
    url: string;
}

/**
 * Takes style sheet urls from the settings section of the configuration, and adds the style sheets to the
 * head element of the html. Style sheet urls are evaluated for expressions and updated as expressions
 * change them.
 */
@Injectable({
    providedIn: 'root',
})
export class StyleSheetManager {

    private urlExpressions: Array<TextWithExpressions> = new Array<TextWithExpressions>();
    private debugModeUsed: boolean;

    public constructor(
        private configService: ConfigService,
        private expressionDependencies: ExpressionDependencies,
        eventService: EventService,
        applicationService: ApplicationService,
    ) {
        eventService.loadedConfiguration.subscribe((config: WorkingConfiguration) => this.updateStyleSheets());
        eventService.updatedConfiguration.subscribe((config: WorkingConfiguration) => this.updateStyleSheets());
        applicationService.debugSubject.pipe(filter((enabled: boolean) => enabled))
            .subscribe((enabled: boolean) => {
                this.debugModeUsed = true;
                this.updateStyleSheets();
            });
    }

    private updateStyleSheets(): void {
        if (this.configService?.theme?.externalStyleSheetUrlExpressions || this.debugModeUsed) {
            let sources: Array<string> = new Array<string>();
            let themeSources: Array<string> = this.configService.theme?.externalStyleSheetUrlExpressions
                ?.filter((source: string) => !StringHelper.isNullOrEmpty(source));
            if (themeSources) {
                sources = [...themeSources];
            }
            if (this.debugModeUsed) {
                sources.push("https://fonts.googleapis.com/icon?family=Material+Icons");
            }
            for (let index: number = 0; index < sources.length; index++) {
                let source: string = sources[index];
                if (index < this.urlExpressions.length
                    && source == this.urlExpressions[index].source
                ) {
                    // no need to recreate this expression, since it's source hasn't changed.
                    continue;
                }
                let styleSheet: StyleSheet = {
                    id: `style-sheet-url-${index}`,
                    url: '',
                };
                let textExpression: TextWithExpressions = new TextWithExpressions(
                    source,
                    this.expressionDependencies,
                    'settings style sheet url');
                textExpression.nextResultObservable.subscribe((url: string) => {
                    styleSheet.url = url;
                    this.upsertStyleSheet(styleSheet);
                });
                if (index > this.urlExpressions.length - 1) {
                    this.urlExpressions.push(textExpression);
                } else {
                    let oldExpression: TextWithExpressions
                        = this.urlExpressions.splice(index, 1, textExpression)[0];
                    oldExpression.dispose();
                }
                textExpression.triggerEvaluation();
            }

            // remove any excess stylesheets
            for (let index: number = sources.length; index < this.urlExpressions.length; index++) {
                let id: string = `style-sheet-url-${index}`;
                let oldExpression: TextWithExpressions = this.urlExpressions.pop();
                oldExpression.dispose();
                this.removeStyleSheet(id);
            }
        }
    }

    private upsertStyleSheet(styleSheet: StyleSheet): void {
        let el: HTMLElement = document.getElementById(styleSheet.id);
        if (!el) {
            this.appendStyleSheet(styleSheet);
        } else {
            el.setAttribute('href', styleSheet.url);
        }
    }

    private appendStyleSheet(styleSheet: StyleSheet): void {
        let head: HTMLHeadElement = document.head || document.getElementsByTagName('head')[0];
        let el: HTMLLinkElement = document.createElement('link');
        el.id = styleSheet.id;
        el.rel = 'stylesheet';
        el.href = styleSheet.url;
        head.appendChild(el);
    }

    private removeStyleSheet(id: string): void {
        let el: HTMLElement = document.getElementById(id);
        el.remove();
    }
}
