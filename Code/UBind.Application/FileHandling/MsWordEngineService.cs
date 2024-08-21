// <copyright file="MsWordEngineService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.FileHandling
{
    using System;
    using System.IO;
    using System.Runtime.InteropServices;
    using Microsoft.Extensions.Logging;
    using Microsoft.Office.Interop.Word;
    using Newtonsoft.Json.Linq;
    using UBind.Domain;
    using UBind.Domain.Exceptions;
    using Application = Microsoft.Office.Interop.Word.Application;
    using File = System.IO.File;

    /// <inheritdoc />
    public class MsWordEngineService : EngineBaseService<MsWordEngineService>, IMsWordEngineService
    {
        private const string MERGEFIELDKEY = "MERGEFIELD ";
        private const StringComparison MERGEFIELDVALUECASING = StringComparison.OrdinalIgnoreCase;

        /// <summary>
        /// Initializes a new instance of the <see cref="MsWordEngineService"/> class.
        /// </summary>
        /// <param name="logger">Logging service.</param>
        public MsWordEngineService(ILogger<MsWordEngineService> logger)
            : base(logger)
        {
        }

        /// <inheritdoc/>
        public byte[] PopulateFieldsToTemplatedData(
            string templateSourceFile,
            byte[] templateSource,
            DeploymentEnvironment environment,
            IMsWordFileFormat fileFormat,
            JObject errorData,
            Func<string, StringComparison, string> getFieldValue,
            Func<string, string> additionalProcessingOfFieldCode = null)
        {
            Application app = new Application
            {
                DisplayAlerts = WdAlertLevel.wdAlertsNone,
            };
            string filePath = this.TransferContentOfSourceTemplateIntoTemporaryFile(
                errorData, templateSource);

            Document doc = this.OpenTemporarySourceFile(filePath, app, errorData);

            this.ProcessFields(
                doc.Fields,
                errorData,
                app,
                environment,
                additionalProcessingOfFieldCode,
                getFieldValue,
                templateSourceFile);

            foreach (Section section in doc.Sections)
            {
                this.ProcessHeaderFooters(
                    section.Headers,
                    errorData,
                    app,
                    environment,
                    additionalProcessingOfFieldCode,
                    getFieldValue,
                    templateSourceFile);

                this.ProcessHeaderFooters(
                    section.Footers,
                    errorData,
                    app,
                    environment,
                    additionalProcessingOfFieldCode,
                    getFieldValue,
                    templateSourceFile);
            }

            var outputPath = string.Empty;

            try
            {
                outputPath = Path.ChangeExtension(filePath, fileFormat.FileExtension);
                errorData.Add("finalOutputPath", outputPath);
                doc.Fields.Update();

                if (fileFormat.SaveFormat == MsWordSaveFormatEnum.PDF)
                {
                    // sequence of printer setup command followed by export to generated good quality pdf.
                    dynamic wordBasic = doc.Application.WordBasic;
                    wordBasic.FilePrintSetup(Printer: "Microsoft XPS Document Writer", DoNotSetAsSysDefault: 1);
                    doc.ExportAsFixedFormat(OutputFileName: outputPath, ExportFormat: WdExportFormat.wdExportFormatPDF);
                }
                else
                {
                    doc.SaveAs2(FileName: outputPath, FileFormat: fileFormat.SaveFormat);
                }

                // Reason why we are releasing the interop objects so that the resource that holds the
                // outputPath will be released and there will be no problem when File.ReadAllBytes is invoked.
                this.ReleaseInteropObjects(app, doc);

                // This is to ensure that doc and app objects will not be garbage collected in the finally block. I tried assigning null
                // value inside the ReleaseInteropObjects but it didn't work.
                doc = null;
                app = null;

                return File.ReadAllBytes(outputPath);
            }
            catch (ArgumentException aex)
            {
                errorData.Add("fileExtension", fileFormat.FileExtension);
                errorData.Add(this.ErrorMesssageKey, aex.Message);
                throw new ErrorException(
                    Errors.Automation.ExtensionChangeFailed(errorData, filePath), aex);
            }
            catch (COMException comex)
            {
                errorData.Add(this.ErrorMesssageKey, comex.Message);
                throw new ErrorException(
                    Errors.Automation.ExportingContentToPdfOrMsDocFailed(errorData, outputPath),
                    comex);
            }
            catch (Exception ex)
            {
                errorData.Add(this.ErrorMesssageKey, ex.Message);
                throw new ErrorException(
                    Errors.Automation.FailedToReadFromFinalOutput(errorData, outputPath),
                    ex);
            }
            finally
            {
                if (!string.IsNullOrEmpty(outputPath))
                {
                    this.CleanUp(app, doc, errorData, filePath, outputPath);
                }
                else
                {
                    this.CleanUp(app, doc, errorData, filePath);
                }
            }
        }

        private void ProcessFields(
            Fields fields,
            JObject errorData,
            Application app,
            DeploymentEnvironment environment,
            Func<string, string> additionalProcessingOfFieldCode,
            Func<string, StringComparison, string> getFieldValue,
            string templateSourceFile)
        {
            foreach (Field field in fields)
            {
                if (field.Type == WdFieldType.wdFieldMergeField)
                {
                    var fieldCode = string.Empty;
                    try
                    {
                        field.Select();
                        fieldCode = field.Code.Text;
                        this.MsWordEngineEditingMergeField(
                            app, field, environment, additionalProcessingOfFieldCode, getFieldValue);
                        this.MsWordEnginePopulateMergeField(
                            app, field, environment, additionalProcessingOfFieldCode, getFieldValue);
                    }
                    catch (Exception ex)
                    {
                        this.RethrowApplicationFieldException(
                            fieldCode,
                            ex,
                            errorData,
                            templateSourceFile);
                    }
                }
            }
        }

        private void ProcessHeaderFooters(
            HeadersFooters headerFooters,
            JObject errorData,
            Application app,
            DeploymentEnvironment environment,
            Func<string, string> additionalProcessingOfFieldCode,
            Func<string, StringComparison, string> getFieldValue,
            string templateSourceFile)
        {
            if (headerFooters != null)
            {
                foreach (HeaderFooter footer in headerFooters)
                {
                    Fields fields = footer.Range.Fields;
                    this.ProcessFields(
                        fields,
                        errorData,
                        app,
                        environment,
                        additionalProcessingOfFieldCode,
                        getFieldValue,
                        templateSourceFile);
                }
            }
        }

        private void RethrowApplicationFieldException(
            string fieldCode,
            Exception ex,
            JObject errorData,
            string templateSourceFile)
        {
            if (!string.IsNullOrEmpty(fieldCode))
            {
                errorData.Add("fieldCode", fieldCode);
            }

            throw new ErrorException(
                Errors.Automation.TranslationOfFieldIntoDataFailure(errorData, templateSourceFile),
                ex);
        }

        private Document OpenTemporarySourceFile(
            string filePath,
            Application app,
            JObject errorData)
        {
            try
            {
                var doc = app.Documents.Open(filePath);
                doc.TrackRevisions = false;
                return doc;
            }
            catch (Exception ex)
            {
                errorData.Add(this.ErrorMesssageKey, ex.Message);
                throw new ErrorException(
                    Errors.Automation.UnableToOpenTemporarySourceTemplate(errorData),
                    ex);
            }
        }

        private string TransferContentOfSourceTemplateIntoTemporaryFile(
            JObject errorData,
            byte[] templateSource)
        {
            var filePath = string.Empty;
            try
            {
                filePath = Path.GetTempFileName();
                using (FileStream fstream = File.OpenWrite(filePath))
                {
                    fstream.Write(templateSource, 0, templateSource.Length);
                }

                errorData.Add("temporaryFilePath", filePath);

                return filePath;
            }
            catch (Exception ex)
            {
                errorData.Add(this.ErrorMesssageKey, ex.Message);
                throw new ErrorException(
                    Errors.Automation.WriteContentToTemporaryFileFailed(errorData, filePath, ex),
                    ex);
            }
        }

        private string ReadContentOfTheSourceFile(string filePath)
        {
            var content = string.Empty;
            var fileInfo = new FileInfo(filePath);
            if (fileInfo.Exists)
            {
                using (var streamReader = fileInfo.OpenText())
                {
                    content = streamReader.ReadToEnd();
                }
            }

            return content;
        }

        private void MsWordEngineEditingMergeField(
            Application application,
            Field field,
            DeploymentEnvironment environment,
            Func<string, string> additionalProcessingOfFieldCode,
            Func<string, StringComparison, string> getFieldValue)
        {
            if (environment != DeploymentEnvironment.Development)
            {
                return;
            }

            var fieldCode = additionalProcessingOfFieldCode?.Invoke(field.Code.Text);

            string fieldValue = getFieldValue(fieldCode, MERGEFIELDVALUECASING);

            if (fieldValue != null)
            {
                application.Selection.Range.HighlightColorIndex = WdColorIndex.wdYellow;
            }
            else
            {
                application.Selection.Range.HighlightColorIndex = WdColorIndex.wdRed;
            }
        }

        private void MsWordEnginePopulateMergeField(
            Application application,
            Field field,
            DeploymentEnvironment environment,
            Func<string, string> additionalProcessingOfFieldCode,
            Func<string, StringComparison, string> getFieldValue)
        {
            string fieldCode = field.Code.Text.Replace(MERGEFIELDKEY, string.Empty).Trim();

            fieldCode = additionalProcessingOfFieldCode?.Invoke(fieldCode);

            string fieldValue = getFieldValue(fieldCode, MERGEFIELDVALUECASING);

            if (!string.IsNullOrEmpty(fieldValue))
            {
                application.Selection.TypeText(fieldValue);
            }
            else if (fieldValue == string.Empty)
            {
                if (environment == DeploymentEnvironment.Development)
                {
                    // Replace empty strings with "[Deleted]" in DEV, so that it can be highlighted.
                    application.Selection.TypeText("[Deleted]");
                }
                else
                {
                    application.Selection.Delete();
                }
            }
            else
            {
                // Don't delete for null values in development, so errors are highlighted
                if (environment != DeploymentEnvironment.Development)
                {
                    application.Selection.Delete();
                }
            }
        }
    }
}
