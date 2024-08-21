import { Subject } from "rxjs";

/**
 * Fake tooltip service class
 */
export class FakeToolTipService {

    public toolTipChangedSubject: Subject<any>;
    public constructor() {
        this.toolTipChangedSubject = new Subject<any>();
    }
    public toolTipChange(): void {
    }
}
