import { LogLevel } from './log-level.enum';

/**
 * Export log model class.
 * TODO: Write a better class header: log model.
 */
export class LogModel {
    public constructor(level: LogLevel, description: string, value: string) {
        this.level = level;
        this.description = description;
        this.value = value;
    }
    public level: LogLevel;
    public description: string;
    public value: string;
}
