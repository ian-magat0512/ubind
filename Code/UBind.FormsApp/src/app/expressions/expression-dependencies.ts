import { Injectable } from "@angular/core";
import { EventService } from '@app/services/event.service';
import { ExpressionInputSubjectService } from './expression-input-subject.service';
import { ExpressionMethodService } from './expression-method.service';
import { ErrorHandlerService } from "@app/services/error-handler.service";
import { MatchingFieldsSubjectService } from "./matching-fields-subject.service";
import { TaggedFieldsSubjectService } from "./tagged-fields-subject.service";

/**
 * A class which contains the expression dependencies all in one.
 * 
 * This is so that when constructing expressions we have to pass only one dependency in,
 * rather than a long list. This makes the code shorter and more readable.
 */
@Injectable({
    providedIn: 'root',
})
export class ExpressionDependencies {
    public constructor(
        public expressionMethodService: ExpressionMethodService,
        public expressionInputSubjectService: ExpressionInputSubjectService,
        public matchingFieldsSubjectService: MatchingFieldsSubjectService,
        public eventService: EventService,
        public errorHandlerService: ErrorHandlerService,
        public taggedFieldsSubjectService: TaggedFieldsSubjectService,
    ) {
    }
}
