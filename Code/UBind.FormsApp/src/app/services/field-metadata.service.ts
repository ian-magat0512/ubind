import { Injectable } from "@angular/core";
import { QuestionMetadata } from "@app/models/question-metadata";
import { ExpressionInputSubjectService } from "@app/expressions/expression-input-subject.service";
import { ConfigService } from "./config.service";
import { EventService } from "./event.service";

/**
 * Provides tools for access metadata for a field by it's fieldPath
 */
@Injectable({
    providedIn: 'root',
})
export class FieldMetadataService {

    private tagFieldPathMap: Map<string, Set<string>> = new Map<string, Set<string>>();

    public constructor(
        private configService: ConfigService,
        private eventService: EventService,
        private expressionInputSubjectService: ExpressionInputSubjectService,
    ) {
        this.onFieldPathAddedUpdateTagMap();
        this.onFieldPathRemovedUpdateTagMap();
    }

    /**
     * @param tag 
     * @returns a set of field paths, for a given tag.
     */
    public getFieldPathsWithTag(tag: string): Set<string> {
        let set: Set<string> = this.tagFieldPathMap.get(tag);
        return set ? set : new Set<string>();
    }

    /**
     * @param tag 
     * @returns a set of field paths that doesnt have a given tag.
     */
    public getFieldPathsWithoutTag(tag: string): Set<string> {
        let fieldPathWithTagSet: Set<string> = this.tagFieldPathMap.get(tag) || new Set<string>();
        const fieldPathSet: Set<string> = new Set(this.expressionInputSubjectService.getAllFieldPaths());

        for (const fieldPathsWithTag of fieldPathWithTagSet) {
            fieldPathSet.delete(fieldPathsWithTag);
        }

        return fieldPathSet;
    }

    /**
     * Retreives the Question metadata for a particular field. The Question Metadata contains
     * information such as the field type which is commonly used to know how to format the value
     * when rendering it for end users to view it.
     * @param fieldPath the field path that uniquely identifies the field within the form.
     * @returns the QuestionMetadata.
     */
    public getMetadataForField(fieldPath: string): QuestionMetadata {
        // if it contains a dot grab the first part
        const firstDotPosition: number = fieldPath.indexOf('.');
        let key: string = fieldPath;
        if (firstDotPosition != -1) {
            key = fieldPath.substring(0, firstDotPosition);
            key = key.replace(/(\[\d+\])/, '');
            let subKey: string = fieldPath.substring(firstDotPosition + 1);
            return this.configService.getRepeatingQuestionMetadata(key, subKey);
        }
        return this.configService.getQuestionMetadata(key);
    }

    private onFieldPathAddedUpdateTagMap(): void {
        this.eventService.fieldPathAddedSubject.subscribe((fieldPath: string) => {
            let metadata: QuestionMetadata = this.getMetadataForField(fieldPath);
            if (metadata && metadata.tags) {
                for (let tag of metadata.tags) {
                    let fieldPathsForTag: Set<string> = this.tagFieldPathMap.get(tag);
                    if (!fieldPathsForTag) {
                        fieldPathsForTag = new Set<string>();
                        this.tagFieldPathMap.set(tag, fieldPathsForTag);
                    }
                    fieldPathsForTag.add(fieldPath);
                }
            }
        });
    }

    private onFieldPathRemovedUpdateTagMap(): void {
        this.eventService.fieldPathRemovedSubject.subscribe((fieldPath: string) => {
            let metadata: QuestionMetadata = this.getMetadataForField(fieldPath);
            if (metadata && metadata.tags) {
                for (let tag of metadata.tags) {
                    let fieldPathsForTag: Set<string> = this.tagFieldPathMap.get(tag);
                    if (fieldPathsForTag) {
                        fieldPathsForTag.delete(fieldPath);
                    }
                }
            }
        });
    }

    public isFieldDisplayable(fieldPath: string): boolean {
        let metadata: QuestionMetadata = this.getMetadataForField(fieldPath);
        return metadata
            ? metadata.displayable
            : true;
    }

    /**
     * This is called when configuration is updated and a field has a tag added to it
     */
    public onTagsAddedToField(fieldPath: string, tagsAdded: Array<string>): void {
        for (let tag of tagsAdded) {
            let fieldPathsForTag: Set<string> = this.tagFieldPathMap.get(tag);
            if (!fieldPathsForTag) {
                fieldPathsForTag = new Set<string>();
                this.tagFieldPathMap.set(tag, fieldPathsForTag);
            }
            fieldPathsForTag.add(fieldPath);
        }
    }

    /**
     * This is called when configuration is updated and a field has a tag removed from it
     */
    public onTagsRemovedFromField(fieldPath: string, tagsRemoved: Array<string>): void {
        for (let tag of tagsRemoved) {
            let fieldPathsForTag: Set<string> = this.tagFieldPathMap.get(tag);
            if (!fieldPathsForTag) {
                throw new Error("When trying to remove a tag from a field, the set of fieldpaths for that tag "
                    + " was empty, which is unexpected.");
            }
            let deleted: boolean = fieldPathsForTag.delete(fieldPath);
            if (!deleted) {
                throw new Error("When trying to remove a tag from a field, the set of fieldpaths for that tag "
                    + " did not contain the fieldPath, which is unexpected.");
            }
        }
    }
}
