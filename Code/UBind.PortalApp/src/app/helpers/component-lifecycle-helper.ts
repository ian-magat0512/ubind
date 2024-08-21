
/**
 * Export component lifecycle helper class
 * Lifescycle helper
 */
export class ComponentLifecycleHelper {

    public static waitForNgOnInit(angularComponent: any): Promise<void> {
        const result = (resolve: any, reject: any): void => {
            let originalNgOnInit: any = angularComponent['ngOnInit'];
            angularComponent['ngOnInit'] = ((): void => {
                if (originalNgOnInit) {
                    originalNgOnInit.bind(angularComponent)();
                }
                resolve();
            }).bind(angularComponent);
        };
        return new Promise(result);
    }
}
