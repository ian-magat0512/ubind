// <copyright file="EngineBaseService.cs" company="uBind">
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
    using UBind.Domain.Exceptions;
    using Application = Microsoft.Office.Interop.Word.Application;

    /// <summary>
    /// Base engine service.
    /// </summary>
    /// <typeparam name="TEngine">Concrete instance that inherits this base class.</typeparam>
    public abstract class EngineBaseService<TEngine>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EngineBaseService{TEngine}"/> class.
        /// </summary>
        /// <param name="logger">Logger service.</param>
        public EngineBaseService(ILogger<TEngine> logger)
        {
            this.Logger = logger;
        }

        public ILogger<TEngine> Logger { get; set; }

        /// <summary>
        /// Gets the value of error message key to be used as key to exception message.
        /// </summary>
        protected string ErrorMesssageKey
        {
            get
            {
                return "errorMessage";
            }
        }

        /// <summary>
        /// Tries to delete files, catching and logging any errors encountered.
        /// </summary>
        /// <param name="errorData">Additional operational data to include in any error logs.</param>
        /// <param name="files">Files to be deleted.</param>
        protected void TryDeleteFiles(JObject errorData, params string[] files)
        {
            foreach (var file in files)
            {
                var path = string.Empty;
                try
                {
                    path = Path.GetDirectoryName(file);
                    if (File.Exists(file))
                    {
                        this.Logger.LogInformation($"\"EngineBaseService\" deleting {file}");
                        File.Delete(file);
                        this.Logger.LogInformation($"\"EngineBaseService\" deleted {file}");
                    }
                }
                catch (Exception ex)
                {
                    errorData.Add("temporaryfileToBeDeleted", file);

                    var errorException = new ErrorException(
                        Domain.Errors.Automation.MergeFieldCleanUpError(
                        errorData,
                        path),
                        ex);
                    this.Logger.LogError(errorException, errorException.Error.ToString());
                }
            }
        }

        /// <summary>
        /// Release all the com objects.
        /// </summary>
        /// <param name="app">Microsoft.Office.Interop.Word.Application.</param>
        /// <param name="doc">Microsoft.Office.Interop.Word.Application.Document.</param>
        protected void ReleaseInteropObjects(Application app, Document doc)
        {
            if (doc != null)
            {
                var docName = doc.Name;
                this.Logger.LogInformation($"\"EngineBaseService\" Closing doc {docName}");
                doc.Close(false);
                Marshal.ReleaseComObject(doc);
                this.Logger.LogInformation($"\"EngineBaseService\" Closed doc {docName}");
            }

            if (app != null)
            {
                var appName = app.Name;
                this.Logger.LogInformation($"\"EngineBaseService\" Closing app {appName}");
                app.Quit();
                Marshal.ReleaseComObject(app);
                this.Logger.LogInformation($"\"EngineBaseService\" Closed app {appName}");
            }
        }

        /// <summary>
        /// Cleans up all the used resources such as files and interop objects.
        /// </summary>
        /// <param name="app">Microsoft.Office.Interop.Word.Application.</param>
        /// <param name="doc">Microsoft.Office.Interop.Word.Application.Document.</param>
        /// <param name="errorData">Additional operational data to include in any error logs.</param>
        /// <param name="files">Files to be deleted.</param>
        protected void CleanUp(
            Application app,
            Document doc,
            JObject errorData,
            params string[] files)
        {
            // In the event that the method below hasn't been invoked due to an exception thrown before its
            // invocation in the try block.
            this.ReleaseInteropObjects(app, doc);

            this.TryDeleteFiles(errorData, files);
        }
    }
}
