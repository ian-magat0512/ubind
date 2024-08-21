import { Component } from '@angular/core';
import { FormBuilder, FormGroup } from '@angular/forms';
import { FormService } from '@app/services/form.service';
import { Clipboard } from '@app/helpers/clipboard.helper';
import { WorkbookDataFormatter } from '@app/services/workbook-data-formatter';

/**
 * This component provides the ability to copy the questionAnswers and repeatingQuestionAnswers
 * to the clipboard, for pasting into the workbook.
 */
@Component({
    selector: 'workbook-tools',
    templateUrl: './workbook-tools.component.html',
    styleUrls: ['./workbook-tools.component.scss'],
})
export class WorkbookToolsComponent {
    public output: string;
    public form: FormGroup = new FormGroup({});

    public constructor(
        private formService: FormService,
        private workbookDataFormatter: WorkbookDataFormatter,
        formBuilder: FormBuilder,
    ) {
        this.form = formBuilder.group({
        });
    }

    public generateQuestionAnswers(): void {
        let formModel: any = this.formService.getValues(true, true, false, false);
        let questionAnswers: any = this.workbookDataFormatter.generateQuestionAnswers(formModel);
        const table: Array<Array<string>> = new Array<Array<string>>();
        for (let field of questionAnswers) {
            let value: string = field[0] || '';
            if (value.toString().indexOf('\n') !== -1) {
                field[0] = value.replace(/\n/g, "\\n");
            }
            table.push(field);
        }
        this.output = this.workbookDataFormatter.tabulate(table);
    }

    public generateRepeatingQuestionAnswers(): void {
        let formModel: any = this.formService.getValues(true, true, false, false);
        this.output = this.workbookDataFormatter.tabulate(
            this.workbookDataFormatter.generateRepeatingQuestionAnswers(formModel));
    }

    public clear(): void {
        this.output = '';
    }

    public copy(): void {
        Clipboard.copyTextToClipboard(this.output);
    }
}
