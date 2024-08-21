export interface WebFormEmbedOptions {
    /**
     * A model of data with which to pre-fill the form with.
     */
    seedFormData?: any;
    overwriteFormData?: any;

    title?: string;
    environment?: string;
    formType?: string;
    quoteId?: string;
    policyId?: string;
    claimId?: string;
    portal?: string;
    quoteVersion?: number;
    quoteType?: string;
    sidebarOffset?: string;
    mode?: string;
    isTestData?: boolean;
    debug?: boolean;
    debugLevel?: number;
    tenant?: string;
    product?: string;
    organisation?: string;

    /**
     * The background of the loader, before the web form actually loads.
     * Takes a css compatible string value for the background css property.
     * Defaults to "white".
     */
    loaderBackground?: string;

    /**
     * When true, causes the iframe to resize to fit the content.
     * Defaults to true.
     */
    autoResize?: boolean;

    /**
     * If set to true, instead of embedding in a div inline, the webform opens in a modal popup
     */
    modalPopup?: boolean;

    /**
     * The CSS z-index to apply to the embedded modal popup and it's backdrop, to ensure that it
     * sits on top of all other page elements.
     * Defaults to 10001.
     */
    modalZIndex?: number;

    /**
     * Determines whether scrolling should happen on the window, or inside the popup.
     * Defaults to true, meaning the scrollbars will show inside the popup modal, not on the window.
     */
    scrollInsidePopup?: boolean;

    /**
     * Whether to show a shaded backdrop when showing a popup modal.
     * Defaults to true.
     */
    modalBackdrop?: boolean;

    /**
     * Defaults to 80%
     */
    width?: string;

    /**
     * Defaults to 320px
     */
    minimumWidth?: string;

    /**
     * Makes the popup take up the full width when the window width is below this threshold.
     * Defaults to 576.
     */
    fullWidthBelowPixels?: number;

    /**
     * Defaults to 1000px
     */
    maximumWidth?: string;

    /**
     * Constrains the popups height, and adds internal scrolling.
     * The maximum height, if set, will not be allowed to exceed the window height.
     * The reason is that it would add an additional scrollbar on the window and cause confusion for users.
     */
    maximumHeight?: string;

    /**
     * Adds corner rounding to the popup modal.
     * Defaults to 4px.
     */
    borderRadius?: string;

    accentColor1?: string;
    accentColor2?: string;
    accentColor3?: string;
    accentColor4?: string;

    /**
     * Padding to apply to the app element on viewports < 576px
     * Defaults to 10px for popup modals
     */
    paddingXs?: string;

    /**
     * Padding to apply to the app element on viewports >= 576px
     * Defaults to 15px for popup modals
     */
    paddingSm?: string;

    /**
     * Padding to apply to the app element on viewports >= 768px
     * Defaults to 20px for popup modals
     */
    paddingMd?: string;

    /**
     * Padding to apply to the app element on viewports >= 992px
     */
    paddingLg?: string;

    /**
     * Padding to apply to the app element on viewports >= 1200px
     */
    paddingXl?: string;

    /**
     * Padding to apply to the app element on viewports >= 1400px
     */
    paddingXxl?: string;

    /**
     * Ensures the web form has a minimum height, even if there is no content.
     */
    minimumHeight?: string;
}
