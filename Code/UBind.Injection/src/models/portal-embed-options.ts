export interface PortalEmbedOptions {
    tenant?: string;
    organisation?: string;
    environment?: string;
    portal?: string;
    path?: string;

    /**
     * When set to true (which is the default if not set), the portal will take up the entire page
     * by writing in some css. When explicitly set to false this css is not output.
     */
    fullScreen?: boolean;
}
