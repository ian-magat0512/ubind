/**
 * The window sizes
 */
export enum WindowSize {
    /**
     * Small is typically for mobile, where there is only enough room to show the detail view,
     * but no master view or menu.
     */
    Small,

    /**
     * Medium is typically for a low res tablet, where there is enough room to show the master pane and 
     * the detail pane, but no menu
     */
    Medium,

    /**
     * MediumLarge is typically for a large tablet, where there is enough room to show the master pane and 
     * the detail pane, and just a collapsed menu
     */
    MediumLarge,

    /**
     * Large is typically for desktop, where there is enough room to show the menu, master pane, and detail pane.
     */
    Large
}
