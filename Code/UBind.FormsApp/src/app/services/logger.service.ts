import { Injectable } from '@angular/core';
import { ApiService } from '../services/api.service';
import { LogModel } from '../models/log.model';
import { LogLevel } from '../models/log-level.enum';
import { ApplicationService } from './application.service';

/**
 * Export Logger service class.
 * TODO: Write a better class header: Logger functions.
 */
@Injectable()
export class LoggerService {
    public constructor(
        private apiService: ApiService,
        private applicationService: ApplicationService,
    ) {
    }

    public log(level: LogLevel, description: string, value: any): void {
        const logModel: LogModel = new LogModel(LogLevel.Information, description, value);
        this.apiService.post('log', logModel).subscribe(() => {
        },
        (err: any) => {
            console.log(err);
        });
    }

    public debug(message: string) {
        if (this.applicationService.debug) {
            console.log(message);
        }
    }
}
