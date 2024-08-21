import * as _ from 'lodash-es';

/**
 * The stats of what was changed from calling the synchronise function. 
 */
export interface ArraySyncStats {
    added: number;
    removed: number;
}

/**
 * Provides helper functions for manipulating arrays.
 */
export class ArrayHelper {

    /**
     * Modifies the target array to be the same as the source array, only removing and inserting elements as necessary.
     * This is used to modify an array of items which angular is rendering in a view, so as to not need to completely
     * replace the array with the new one. By modifying the array, angular will only re-render those items which have
     * actually changed.
     * @param sourceArray the array which the target array should be exactly the same as
     * @param targetArray the array to modify to make it the same as the source array
     */
    public static synchronise<T>(
        sourceArray: Array<T>,
        targetArray: Array<T>,
        equalityFunction: (item1: T, item2: T) => boolean = _.isEqual,
        deletedFunction: (item: T) => void = null,
    ): ArraySyncStats {
        let stats: ArraySyncStats = {
            added: 0,
            removed: 0,
        };

        // remove any items which are no longer in the array
        for (let index: number = 0; index < targetArray.length; index++) {
            const targetItem: T = targetArray[index];
            const itemStillExists: boolean
                = sourceArray.find((sourceItem: T) => equalityFunction(sourceItem, targetItem)) !== undefined;
            if (!itemStillExists) {
                stats.removed++;
                const deletedItem: T = targetArray.splice(index, 1)[0];
                if (deletedFunction) {
                    deletedFunction(deletedItem);
                }
                index--;
            }
        }

        // add items which are new        
        const newItems: Array<T> = sourceArray.filter((sourceItem: T) =>
            targetArray.find((targetItem: T) => equalityFunction(sourceItem, targetItem)) === undefined);
        targetArray.push(...newItems);
        stats.added = newItems.length;

        // call the deleted function on items which were not added
        if (deletedFunction) {
            const existingItems: Array<T> = _.difference(sourceArray, newItems);
            existingItems.forEach(deletedFunction);
        }

        // reorder items to ensure the order of items matches the sourceArray
        for (let desiredIndex: number = 0; desiredIndex < sourceArray.length; desiredIndex++) {
            let actualIndex: number
                = targetArray.findIndex((targetItem: T) => equalityFunction(sourceArray[desiredIndex], targetItem));
            if (actualIndex != desiredIndex) {
                // move the item from the actualIndex to the desired index
                targetArray.splice(desiredIndex, 0, targetArray.splice(actualIndex, 1)[0]);
            }
        }

        return stats;
    }
}
