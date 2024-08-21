import { Injectable } from '@angular/core';
import { ConfigService } from './config.service';
import { ExpressionDependencies } from '@app/expressions/expression-dependencies';
import { TextWithExpressions } from '@app/expressions/text-with-expressions';
import { EventService } from './event.service';
import { WorkingConfiguration } from '@app/models/configuration/working-configuration';
import * as _ from 'lodash-es';

/**
 * Evaluates expressions within the product css and writes it into the head of the html document.
 */
@Injectable()
export class CssProcessorService {

    private cssSource: string;

    public constructor(
        protected expressionDependencies: ExpressionDependencies,
        protected config: ConfigService,
        eventService: EventService,
    ) {
        eventService.loadedConfiguration.subscribe((config: WorkingConfiguration) => this.applyStyles());
        eventService.updatedConfiguration.subscribe((config: WorkingConfiguration) => this.applyStyles());
    }

    public applyStyles(): void {
        const oldCssSource: string = this.cssSource;
        const newCssSource: string = this.config.styles
            ? this.config.styles.css
            : null;
        this.cssSource = newCssSource;
        if (_.isEqual(oldCssSource, newCssSource)) {
            return;
        }
        let processedCss: string;
        if (newCssSource) {
            processedCss = new TextWithExpressions(
                newCssSource,
                this.expressionDependencies,
                'product css').evaluateAndDispose();
        }
        if (oldCssSource && newCssSource) {
            // remove the old css first
            let el: HTMLStyleElement = <HTMLStyleElement>document.getElementById('form-inline-styles');
            el.remove();
        }

        // insert the style tag
        let head: HTMLHeadElement = document.head || document.getElementsByTagName('head')[0];
        let el: HTMLStyleElement = document.createElement('style');
        el.id = 'form-inline-styles';
        el.appendChild(document.createTextNode(processedCss));
        head.appendChild(el);
    }
}
