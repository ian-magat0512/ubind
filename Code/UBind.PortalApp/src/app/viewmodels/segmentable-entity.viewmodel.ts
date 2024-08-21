import { EntityViewModel } from "./entity.viewmodel";

/**
 * Represents an entity that can be segmented, e.g. by a status or other attribute
 */
export interface SegmentableEntityViewModel extends EntityViewModel {
    segment: string;
}
