
export interface OperationConfiguration {
    name: string;
    backgroundExecution?: boolean;
    params?: any;
    operations?: Array<string | OperationConfiguration>;
}
