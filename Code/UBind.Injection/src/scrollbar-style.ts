export const scrollbarStyle: string = `
    .ubind-modal ::-webkit-scrollbar {
        width: 14px;
        height: 18px;
        background: transparent;
    }
    .ubind-modal ::-webkit-scrollbar-thumb {
        height: 6px;
        border: 5px solid rgba(224, 224, 224, 0);
        background-clip: padding-box;
        -webkit-border-radius: 15px;
        background-color: #e0e0e0;
        -webkit-box-shadow: inset -1px -1px 0px rgba(224, 224, 224, 0.5), inset 1px 1px 0px rgba(224, 224, 224, 2.5);
    }
    .ubind-modal ::-webkit-scrollbar-button {
        width: 0;
        height: 0;
        display: none;
    }
    .ubind-modal ::-webkit-scrollbar-corner {
        background-color: transparent;
    }
    .ubind-modal * {
        scrollbar-color: #e0e0e0 transparent;
        scrollbar-width: thin;
    }
`;
