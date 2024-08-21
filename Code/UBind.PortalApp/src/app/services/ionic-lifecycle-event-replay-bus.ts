import { ReplaySubject } from "rxjs";
import { ElementRef } from "@angular/core";

/**
 * Export Ionic lifecycle Event replay bus class.
 * TODO: Write a better class header: ionic lifecycle events functions.
 */
export class IonicLifecycleEventReplayBus {

    private ionicLifecycleHooks: Array<string> =
        ['ionViewWillEnter', 'ionViewDidEnter', 'ionViewWillLeave', 'ionViewDidLeave'];

    private replaySubjects: Map<string, ReplaySubject<any>>;

    public constructor(source: ElementRef<HTMLElement>) {
        this.replaySubjects = new Map<string, ReplaySubject<any>>();
        this.ionicLifecycleHooks.forEach((hook: string) => {
            source.nativeElement.addEventListener(hook, (event: Event) => {
                this.replaySubjects.set(hook, new ReplaySubject(1));
                this.replaySubjects.get(hook).next(hook);
            });
        });
    }

    public subscribe(targetComponent: any): void {
        this.replaySubjects.forEach((value: ReplaySubject<any>, key: string) => {
            value.subscribe(() => {
                if (targetComponent[key] && typeof (targetComponent[key] == 'function')) {
                    targetComponent[key]();
                }
            });
        });
    }
}
