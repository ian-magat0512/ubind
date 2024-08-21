import { } from "jasmine";
import { TimePipe } from "./time.pipe";

let timePipe: TimePipe = new TimePipe();

describe('TimePipe.transform', () => {
    it('should transform time string to time', () => {
        expect(timePipe.transform('12:30 am')).toEqual('12:30 AM');
        expect(timePipe.transform('12:30 AM')).toEqual('12:30 AM');
        expect(timePipe.transform('12:30 aM')).toEqual('12:30 AM');
        expect(timePipe.transform('12:30 Am')).toEqual('12:30 AM');
        expect(timePipe.transform('12:30 pm')).toEqual('12:30 PM');
        expect(timePipe.transform('12:30 PM')).toEqual('12:30 PM');
        expect(timePipe.transform('12:30 pM')).toEqual('12:30 PM');
        expect(timePipe.transform('12:30 Pm')).toEqual('12:30 PM');
        expect(timePipe.transform('    12:30 Pm    ')).toEqual('12:30 PM');
        expect(timePipe.transform('12:30')).toEqual('12:30 PM');
        expect(timePipe.transform('01:30')).toEqual('01:30 AM');
        expect(timePipe.transform('13:30')).toEqual('01:30 PM');
        expect(timePipe.transform('20:30')).toEqual('08:30 PM');
    });
    it('should not transform non-numeric string', () => {
        expect(timePipe.transform('123abc')).toEqual('123abc');
        expect(timePipe.transform('abc')).toEqual('abc');
        expect(timePipe.transform('abc123')).toEqual('abc123');
        expect(timePipe.transform('123.45.67')).toEqual('123.45.67');
        expect(timePipe.transform('.999')).toEqual('.999');
        expect(timePipe.transform('1pm')).toEqual('1pm');
        expect(timePipe.transform('1:00 cm')).toEqual('1:00 cm');
        expect(timePipe.transform('1:30')).toEqual('1:30');
    });
});
