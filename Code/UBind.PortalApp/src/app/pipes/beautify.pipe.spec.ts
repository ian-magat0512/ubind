import { } from "jasmine";
import { BeautifyPipe } from "./beautify.pipe";

describe('BeautifyPipe.transform', () => {

    it('Capitalizes first letter and gives proper spacing if camel case', () => {
        let pipe: BeautifyPipe = new BeautifyPipe();
        expect(pipe.transform("beautifyMe")).toBe("Beautify Me");
        expect(pipe.transform("beautify100")).toBe("Beautify 100");
        expect(pipe.transform("beautify me please")).toBe("Beautify me please");
        expect(pipe.transform("beautify-100")).toBe("Beautify- 100");
        expect(pipe.transform("beautify-200")).toBe("Beautify- 200");
        expect(pipe.transform("BeautifyMePlease200")).toBe("Beautify Me Please 200");
        expect(pipe.transform("")).toBe("");
        expect(pipe.transform(null)).toBe(null);
    });
});
