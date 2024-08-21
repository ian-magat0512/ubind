import { TestDataService } from './services/testdata.service';
import { runWorkflow } from './runWorkflow';
let sampleDatas = new TestDataService().retrieveAll('Test');

describe('Integration Test', () => {
    for (var i = 0; i < sampleDatas.length; i++) {
        (function (sampleData) {
            runWorkflow(sampleData);
        })(sampleDatas[i]);
    }
});

