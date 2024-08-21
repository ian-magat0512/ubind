// <copyright file="PdfEngineService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.FileHandling
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;
    using Microsoft.Extensions.Logging;
    using Microsoft.Office.Interop.Word;
    using Newtonsoft.Json.Linq;
    using UBind.Application.Automation.Providers.File;
    using UBind.Domain;
    using UBind.Domain.Exceptions;
    using Application = Microsoft.Office.Interop.Word.Application;
    using SystemIo = System.IO;

    /// <summary>
    /// This object manages any related pdf tasks.
    /// </summary>
    public class PdfEngineService : EngineBaseService<PdfEngineService>, IPdfEngineService
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PdfEngineService"/> class.
        /// </summary>
        /// <param name="logger">Logger service.</param>
        public PdfEngineService(ILogger<PdfEngineService> logger)
            : base(logger)
        {
        }

        /// <inheritdoc/>So
        public byte[] OutputSourceFileBytesToPdfBytes(
            FileInfo sourceFileInfo,
            JObject additionalDetails)
        {
            this.Logger.LogInformation("\"PdfEngineService\" calling OutputBytesFromSourceFileIntoTemporaryFile()");
            var temporaryFileFullPath = this.OutputBytesFromSourceFileIntoTemporaryFile(
                sourceFileInfo, additionalDetails);
            this.Logger.LogInformation("\"PdfEngineService\" OutputBytesFromSourceFileIntoTemporaryFile Result - " + temporaryFileFullPath);

            this.Logger.LogInformation("\"PdfEngineService\" calling ExportBytesFromTemporaryPdf()");
            return this.ExportBytesFromTemporaryPdf(temporaryFileFullPath, sourceFileInfo, additionalDetails);
        }

        private byte[] ExportBytesFromTemporaryPdf(
            string temporaryFileFullPath, FileInfo sourceFileInfo, JObject errorData)
        {
            Application app = new Application
            {
                DisplayAlerts = WdAlertLevel.wdAlertsNone,
            };

            var temporaryPdfFullPath = string.Empty;

            this.Logger.LogInformation("\"PdfEngineService\" Opening document - " + temporaryFileFullPath);
            Document doc = this.OpenDocument(temporaryFileFullPath, app, errorData);
            try
            {
                var temporaryPdfTempPath = SystemIo.Path.GetTempPath();
                var temporaryPdfFileName = this.GetNewTempFileName(sourceFileInfo, ".pdf");
                temporaryPdfFullPath = SystemIo.Path.Combine(temporaryPdfTempPath, temporaryPdfFileName.ToString());

                errorData.Add("temporaryOutputFile", temporaryPdfFullPath);
                doc.TrackRevisions = false;

                dynamic wordBasic = doc.Application.WordBasic;
                this.Logger.LogInformation("\"PdfEngineService\" calling wordBasic.FilePrintSetup()");
                wordBasic.FilePrintSetup(Printer: "Microsoft XPS Document Writer", DoNotSetAsSysDefault: 1);
                this.Logger.LogInformation("\"PdfEngineService\" calling ExportAsFixedFormat()");
                doc.ExportAsFixedFormat(
                    OutputFileName: temporaryPdfFullPath,
                    ExportFormat: WdExportFormat.wdExportFormatPDF);

                this.Logger.LogInformation("\"PdfEngineService\" calling ReleaseInteropObjects()");
                this.ReleaseInteropObjects(app, doc);

                app = null;
                doc = null;

                this.Logger.LogInformation("\"PdfEngineService\" File.ReadAllBytes() - " + temporaryPdfFullPath);
                return SystemIo.File.ReadAllBytes(temporaryPdfFullPath);
            }
            catch (SecurityException secEx)
            {
                errorData.Add(this.ErrorMesssageKey, secEx.Message);
                throw new ErrorException(
                    Errors.Automation.UnableToCreateTemporaryPath(errorData, sourceFileInfo.ToString()),
                    secEx);
            }
            catch (COMException comEx)
            {
                errorData.Add(this.ErrorMesssageKey, comEx.Message);
                throw new ErrorException(
                    Errors.Automation.ExportingContentToPdfOrMsDocFailed(
                    errorData, temporaryPdfFullPath),
                    comEx);
            }
            catch (Exception ex)
            {
                errorData.Add(this.ErrorMesssageKey, ex.Message);
                throw new ErrorException(
                    Errors.Automation.FailedToReadFromFinalOutput(errorData, temporaryPdfFullPath),
                    ex);
            }
            finally
            {
                if (!string.IsNullOrEmpty(temporaryPdfFullPath))
                {
                    this.CleanUp(app, doc, errorData, temporaryFileFullPath, temporaryPdfFullPath);
                }
                else
                {
                    this.CleanUp(app, doc, errorData, temporaryFileFullPath);
                }
            }
        }

        private Document OpenDocument(string temporaryFileFullPath, Application app, JObject errorData)
        {
            try
            {
                return app.Documents.Open(temporaryFileFullPath);
            }
            catch (Exception ex)
            {
                errorData.Add(this.ErrorMesssageKey, ex.Message);
                throw new ErrorException(Errors.Automation.UnableToOpenTemporarySourceTemplate(errorData), ex);
            }
        }

        private string OutputBytesFromSourceFileIntoTemporaryFile(
            FileInfo sourceFileInfo,
            JObject errorData)
        {
            var temporaryFileFullPath = string.Empty;
            try
            {
                this.Logger.LogInformation("\"PdfEngineService\" creating tempPath");
                var temporaryPath = SystemIo.Path.GetTempPath();
                this.Logger.LogInformation("\"PdfEngineService\" created tempPath - " + temporaryPath);
                var newSrcFileName = this.GetNewTempFileName(sourceFileInfo);
                temporaryFileFullPath = SystemIo.Path.Combine(temporaryPath, newSrcFileName);
                this.Logger.LogInformation("\"PdfEngineService\" created temporaryFileFullPath - " + temporaryFileFullPath);

                errorData.Add("temporarySourceFilePath", temporaryFileFullPath);

                this.Logger.LogInformation("\"PdfEngineService\" Opening - " + temporaryFileFullPath);
                using (var fileStream = SystemIo.File.OpenWrite(temporaryFileFullPath))
                {
                    fileStream.Write(sourceFileInfo.Content, 0, sourceFileInfo.Content.Length);
                }

                this.Logger.LogInformation("\"PdfEngineService\" Returning File path - " + temporaryFileFullPath);
                return temporaryFileFullPath;
            }
            catch (SecurityException secEx)
            {
                errorData.Add(this.ErrorMesssageKey, secEx.Message);
                throw new ErrorException(
                    Errors.Automation.UnableToCreateTemporaryPath(errorData, sourceFileInfo.FileName.ToString()),
                    secEx);
            }
            catch (Exception ex)
            {
                errorData.Add(this.ErrorMesssageKey, ex.Message);
                throw new ErrorException(
                    Errors.Automation.WriteContentToTemporaryFileFailed(errorData, temporaryFileFullPath, ex), ex);
            }
        }

        /// <summary>
        /// Creates a new file name for temporary use.
        /// </summary>
        private string GetNewTempFileName(FileInfo sourceFileInfo, string newExtension = null)
        {
            var sourceFileName = SystemIo.Path.GetFileNameWithoutExtension(sourceFileInfo.FileName.ToString());
            var fileExtension = SystemIo.Path.GetExtension(sourceFileInfo.FileName.ToString());
            var fileName = sourceFileName + "-" + Guid.NewGuid() + (newExtension == null ? fileExtension : newExtension);
            return fileName;
        }
    }
}
