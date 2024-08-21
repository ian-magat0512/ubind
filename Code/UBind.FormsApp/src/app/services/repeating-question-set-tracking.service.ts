import { Injectable } from "@angular/core";
import { RepeatingFieldDisplayMode } from "@app/components/fields/repeating/repeating-field-display-mode.enum";
import { StringHelper } from "@app/helpers/string.helper";
import { WorkflowNavigation } from "@app/models/workflow-navigation";
import { Subject } from "rxjs";

/**
 * Holds tracking information about a question set. This is used by expressions.
 */
interface RepeatingFieldTrackingData {
    currentRepeatingInstanceIndex?: number;
    maxQuantity?: number;
    minQuantity?: number;
    count?: number;
    displayMode?: RepeatingFieldDisplayMode;
}

/**
 * Provides mechanisms for tracking repeating question sets status and indexes, so that
 * expression methods can be used for navigating between instance of a repeating question set.
 */
@Injectable({
    providedIn: 'root',
})
export class RepeatingQuestionSetTrackingService {

    private repeatingFieldTrackingDataMap: Map<string, RepeatingFieldTrackingData>
        = new Map<string, RepeatingFieldTrackingData>();
    public repeatingFieldChangeSubject: Subject<void> = new Subject<void>();

    /**
     * the last rendered or interacted with repeating field
     */
    private currentRepeatingFieldPath: string = '';

    /**
     * Here we store the target repeating instance index during navigation, in the case
     * that the target fieldpath is not specified. Once we know the field path, we'll
     * set the repeating instance index properly on the tracking data and store it in
     * the map.
     * 
     * This is needed when navigating to a step which contains a different repeating instance. If
     * we didn't use this, it would set the repeating instance index on the outgoing repeating field's
     * tracking data, but we want it set on the incoming repeating instance's tracking data.
     */
    private tempRepeatingInstanceIndex: number = undefined;

    private getOrCreateTrackingData(fieldPath: string): RepeatingFieldTrackingData {
        let trackingData: RepeatingFieldTrackingData = this.repeatingFieldTrackingDataMap.get(fieldPath);
        if (!trackingData) {
            trackingData = {};
            this.repeatingFieldTrackingDataMap.set(fieldPath, trackingData);
        }
        return trackingData;
    }

    public setCurrentRepeatingFieldPath(fieldPath: string): void {
        const changed: boolean = this.currentRepeatingFieldPath != fieldPath;
        this.currentRepeatingFieldPath = fieldPath;
        let trackingData: RepeatingFieldTrackingData = this.getOrCreateTrackingData(fieldPath);
        if (this.tempRepeatingInstanceIndex !== undefined) {
            trackingData.currentRepeatingInstanceIndex = this.tempRepeatingInstanceIndex;
            this.tempRepeatingInstanceIndex = undefined;
            this.repeatingFieldChangeSubject.next();
        } else if (changed) {
            this.repeatingFieldChangeSubject.next();
        }
    }

    public clearCurrentRepeatingFieldPath(): void {
        this.currentRepeatingFieldPath = null;
    }

    public getCurrentRepeatingFieldPath(): string {
        return this.currentRepeatingFieldPath;
    }

    public setRepeatingInstanceCount(fieldPath: string, count: number): void {
        let trackingData: RepeatingFieldTrackingData = this.getOrCreateTrackingData(fieldPath);
        trackingData.count = count;
        this.repeatingFieldChangeSubject.next();
    }

    public getRepeatingInstanceCount(fieldPath: string): number {
        fieldPath = StringHelper.isNullOrEmpty(fieldPath)
            ? this.currentRepeatingFieldPath
            : fieldPath;
        let trackingData: RepeatingFieldTrackingData = this.getOrCreateTrackingData(fieldPath);
        return trackingData.count || 0;
    }

    public setCurrentRepeatingInstanceIndex(fieldPath: string, index: number): void {
        let trackingData: RepeatingFieldTrackingData = this.getOrCreateTrackingData(fieldPath);
        trackingData.currentRepeatingInstanceIndex = isNaN(index) ? -1 : +index;
        this.repeatingFieldChangeSubject.next();
    }

    public getCurrentRepeatingInstanceIndex(fieldPath: string): number {
        if (this.tempRepeatingInstanceIndex) {
            return this.tempRepeatingInstanceIndex;
        } else {
            fieldPath = StringHelper.isNullOrEmpty(fieldPath)
                ? this.currentRepeatingFieldPath
                : fieldPath;
            let trackingData: RepeatingFieldTrackingData = this.repeatingFieldTrackingDataMap.get(fieldPath);
            return trackingData && !isNaN(trackingData.currentRepeatingInstanceIndex)
                ? +trackingData.currentRepeatingInstanceIndex
                : -1;
        }
    }

    public setRepeatingFieldMaxQuantity(fieldPath: string, quantity: number): void {
        let trackingData: RepeatingFieldTrackingData = this.getOrCreateTrackingData(fieldPath);
        trackingData.maxQuantity = quantity;
        this.repeatingFieldChangeSubject.next();
    }

    public getRepeatingFieldMaxQuantity(fieldPath: string): number {
        fieldPath = StringHelper.isNullOrEmpty(fieldPath)
            ? this.currentRepeatingFieldPath
            : fieldPath;
        let trackingData: RepeatingFieldTrackingData = this.repeatingFieldTrackingDataMap.get(fieldPath);
        return trackingData && trackingData.maxQuantity !== undefined
            ? trackingData.maxQuantity
            : -1;
    }

    public setRepeatingFieldMinQuantity(fieldPath: string, quantity: number): void {
        let trackingData: RepeatingFieldTrackingData = this.getOrCreateTrackingData(fieldPath);
        trackingData.minQuantity = quantity;
        this.repeatingFieldChangeSubject.next();
    }

    public getRepeatingFieldMinQuantity(fieldPath: string): number {
        fieldPath = StringHelper.isNullOrEmpty(fieldPath)
            ? this.currentRepeatingFieldPath
            : fieldPath;
        let trackingData: RepeatingFieldTrackingData = this.repeatingFieldTrackingDataMap.get(fieldPath);
        return trackingData && trackingData.minQuantity !== undefined
            ? trackingData.minQuantity
            : -1;
    }

    public isFirstRepeatingInstance(fieldPath: string): boolean {
        fieldPath = StringHelper.isNullOrEmpty(fieldPath)
            ? this.currentRepeatingFieldPath
            : fieldPath;
        let trackingData: RepeatingFieldTrackingData = this.repeatingFieldTrackingDataMap.get(fieldPath);
        return trackingData && trackingData.currentRepeatingInstanceIndex == 0;
    }

    public isLastRepeatingInstance(fieldPath: string): boolean {
        fieldPath = StringHelper.isNullOrEmpty(fieldPath)
            ? this.currentRepeatingFieldPath
            : fieldPath;
        let trackingData: RepeatingFieldTrackingData = this.repeatingFieldTrackingDataMap.get(fieldPath);
        return trackingData && trackingData.currentRepeatingInstanceIndex >= 0
            ? trackingData.currentRepeatingInstanceIndex == trackingData.count - 1
            : false;
    }

    public hasNextRepeatingInstance(fieldPath: string): boolean {
        fieldPath = StringHelper.isNullOrEmpty(fieldPath)
            ? this.currentRepeatingFieldPath
            : fieldPath;
        let trackingData: RepeatingFieldTrackingData = this.repeatingFieldTrackingDataMap.get(fieldPath);
        return trackingData
            && trackingData.displayMode == RepeatingFieldDisplayMode.Instance
            && trackingData.currentRepeatingInstanceIndex >= 0
            && trackingData.currentRepeatingInstanceIndex < trackingData.count - 1;
    }

    public hasPreviousRepeatingInstance(fieldPath: string): boolean {
        fieldPath = StringHelper.isNullOrEmpty(fieldPath)
            ? this.currentRepeatingFieldPath
            : fieldPath;
        let trackingData: RepeatingFieldTrackingData = this.repeatingFieldTrackingDataMap.get(fieldPath);
        return trackingData
            && trackingData.displayMode == RepeatingFieldDisplayMode.Instance
            && this.getCurrentRepeatingInstanceIndex(fieldPath) > 0;
    }

    public getNextRepeatingInstanceIndex(fieldPath: string): number {
        return this.hasNextRepeatingInstance(fieldPath)
            ? this.getCurrentRepeatingInstanceIndex(fieldPath) + 1
            : -1;
    }

    public getPreviousRepeatingInstanceIndex(fieldPath: string): number {
        return this.hasPreviousRepeatingInstance(fieldPath)
            ? this.getCurrentRepeatingInstanceIndex(fieldPath) - 1
            : -1;
    }

    public getLastRepeatingInstanceIndex(fieldPath: string): number {
        return Math.max(this.getRepeatingInstanceCount(fieldPath) - 1, 0);
    }

    public getFirstRepeatingInstanceIndex(fieldPath: string): number {
        fieldPath = StringHelper.isNullOrEmpty(fieldPath)
            ? this.currentRepeatingFieldPath
            : fieldPath;
        return StringHelper.isNullOrEmpty(fieldPath) ? -1 : 0;
    }

    public setRepeatingFieldDisplayMode(fieldPath: string, mode: RepeatingFieldDisplayMode): void {
        let trackingData: RepeatingFieldTrackingData = this.getOrCreateTrackingData(fieldPath);
        trackingData.displayMode = mode;
        this.repeatingFieldChangeSubject.next();
    }

    public getRepeatingFieldDisplayMode(fieldPath: string): RepeatingFieldDisplayMode {
        fieldPath = StringHelper.isNullOrEmpty(fieldPath)
            ? this.currentRepeatingFieldPath
            : fieldPath;
        let trackingData: RepeatingFieldTrackingData = this.repeatingFieldTrackingDataMap.get(fieldPath);
        return trackingData && trackingData.displayMode ? trackingData.displayMode : <any>'';
    }

    public updateRepeatingInstanceOnNavigation(navigation: WorkflowNavigation): void {
        if (navigation.to.repeatingInstanceIndex !== undefined) {
            // We can assume the repeating field path hasn't changed if it's not specified and the
            // step and article haven't changed.
            let targetRepeatingFieldPath: string = navigation.to.repeatingFieldPath;
            if (!targetRepeatingFieldPath && this.getCurrentRepeatingFieldPath()
                && !navigation.isDifferentStep() && !navigation.isDifferentArticleElement()
            ) {
                targetRepeatingFieldPath = this.getCurrentRepeatingFieldPath();
            }
            if (targetRepeatingFieldPath) {
                this.setCurrentRepeatingInstanceIndex(
                    targetRepeatingFieldPath,
                    navigation.to.repeatingInstanceIndex);
            } else {
                // set it temporarily until we know the fieldpath of the yet to be rendered repeating question set
                this.tempRepeatingInstanceIndex = navigation.to.repeatingInstanceIndex;
            }
        } else {
            this.clearCurrentRepeatingFieldPath();
        }
    }
}
