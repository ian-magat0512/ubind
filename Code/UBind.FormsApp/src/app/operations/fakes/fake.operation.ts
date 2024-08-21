import { Observable, Subject } from "rxjs";

/**
 * An operation that does nothing.
 */
export class FakeOperation {
    public operationName: string;

    public constructor(name: string = 'fakeOperation') {
        this.operationName = name;
    }

    public execute(args: any = {}, operationId: number = Date.now()): Observable<any> {
        return new Subject<any>().asObservable();
    }
}
