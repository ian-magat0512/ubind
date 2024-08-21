// <copyright file="CopyOperation.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Providers.Object.PatchObject
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Dynamic;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.JsonPatch;
    using Newtonsoft.Json.Linq;
    using UBind.Application.Automation.Extensions;
    using UBind.Application.Automation.Helper;
    using UBind.Domain;
    using UBind.Domain.Exceptions;

    /// <summary>
    /// This class is responsible for performing the copy patch operation on the object.
    /// </summary>
    public class CopyOperation : BaseOperation
    {
        private string? message;
        private string from = string.Empty;
        private string to = string.Empty;

        public CopyOperation(
            IProvider<Data<string>>? fromProvider,
            IProvider<Data<string>>? toProvider,
            PrePatchDirective whenSourcePropertyNotFoundAction,
            PrePatchDirective whenDestinationParentPropertyNotFoundAction,
            PrePatchDirective whenDestinationPropertyAlreadyExistsAction)
        {
            this.FromProvider = fromProvider;
            this.ToProvider = toProvider;
            this.WhenSourcePropertyNotFoundAction = whenSourcePropertyNotFoundAction;
            this.WhenDestinationParentPropertyNotFoundAction = whenDestinationParentPropertyNotFoundAction;
            this.WhenDestinationPropertyAlreadyExistsAction = whenDestinationPropertyAlreadyExistsAction;
        }

        private IProvider<Data<string>>? FromProvider { get; set; }

        private IProvider<Data<string>>? ToProvider { get; set; }

        private PrePatchDirective WhenSourcePropertyNotFoundAction { get; set; }

        private PrePatchDirective WhenDestinationParentPropertyNotFoundAction { get; set; }

        private PrePatchDirective WhenDestinationPropertyAlreadyExistsAction { get; set; }

        public override async Task<PatchObjectModel> Execute(JObject obj, IProviderContext providerContext)
        {
            dynamic objectToPatch = obj.ToObject<ExpandoObject>()!;
            var patchDocument = new JsonPatchDocument();
            this.from = (await this.FromProvider.ResolveValueIfNotNull(providerContext))?.DataValue ?? string.Empty;
            this.to = (await this.ToProvider.ResolveValueIfNotNull(providerContext))?.DataValue ?? string.Empty;

            if (!this.IsPathExists(this.from, obj)
                && this.WhenSourcePropertyNotFoundAction != PrePatchDirective.None)
            {
                this.message = "Source property not found";
                var details = this.GetErrorData(obj);
                var error = Errors.Automation.Provider.PatchObject.PathNotFound("copy", this.message, details);
                return await this.Execute(obj, this.WhenSourcePropertyNotFoundAction, patchDocument, error);
            }

            bool targetIsRoot = this.to == string.Empty || this.to == "/";
            var pointer = new PocoJsonPointer(this.from, "patchObject", this.GetErrorData(obj));
            var fromObject = pointer.Evaluate(obj).GetValueOrThrowIfFailed();
            bool valueIsObject = DataObjectHelper.IsObject(fromObject!);
            bool pathExistsForGivenPath = this.IsPathExists(this.to, obj);
            int lastSlashIndex = this.to.LastIndexOf("/");
            string parentPath = lastSlashIndex == -1
                ? string.Empty
                : this.to.Remove(lastSlashIndex);
            if (!this.IsPathExists(parentPath, obj)
                && this.WhenDestinationParentPropertyNotFoundAction != PrePatchDirective.None)
            {
                this.message = "Destination parent property not found";
                var details = this.GetErrorData(obj);
                var error = Errors.Automation.Provider.PatchObject.PathNotFound("copy", this.message, details);
                return await this.Execute(obj, this.WhenDestinationParentPropertyNotFoundAction, patchDocument, error);
            }

            if (!targetIsRoot && !valueIsObject && pathExistsForGivenPath
                && this.WhenDestinationPropertyAlreadyExistsAction != PrePatchDirective.None)
            {
                patchDocument.Remove(this.to);
                patchDocument.Copy(this.from, this.to);

                this.message = "Destination property already exists";
                var details = this.GetErrorData(obj);
                var error = Errors.Automation.Provider.PatchObject.PathAlreadyExists("copy", this.message, details);
                return await this.Execute(obj, this.WhenDestinationPropertyAlreadyExistsAction, patchDocument, error);
            }

            if (targetIsRoot || valueIsObject)
            {
                if (fromObject is IDictionary valueDictionary)
                {
                    // we'll have to add each of the properties one by one due to limitations of the JSONPatch library
                    if (pathExistsForGivenPath || targetIsRoot)
                    {
                        foreach (DictionaryEntry entry in valueDictionary)
                        {
                            patchDocument.Add(MergePath(this.to, entry.Key.ToString()!), entry.Value);
                        }
                    }
                    else
                    {
                        patchDocument.Copy(this.from, this.to);
                    }
                }
                else if (fromObject is JObject valueJObject)
                {
                    if (pathExistsForGivenPath || targetIsRoot)
                    {
                        foreach (var jProperty in valueJObject)
                        {
                            patchDocument.Add(MergePath(this.to, jProperty.Key), jProperty.Value);
                        }
                    }
                    else
                    {
                        patchDocument.Copy(this.from, this.to);
                    }
                }
                else if (valueIsObject)
                {
                    // value is a strongly-typed object, thus convert first then add the properties one by one
                    if (pathExistsForGivenPath || targetIsRoot)
                    {
                        JObject valueObjectAsJObject = JObject.FromObject(fromObject!);
                        foreach (var jProperty in valueObjectAsJObject)
                        {
                            patchDocument.Add(MergePath(this.to, jProperty.Key), jProperty.Value);
                        }
                    }
                    else
                    {
                        patchDocument.Copy(this.from, this.to);
                    }
                }
                else
                {
                    // If you're adding to the root, you need to add an object with properties - you can't just add a value.
                    var errorData = this.GetErrorData(obj);
                    var additionalDetails = new List<string>
                    {
                        $"from: {this.FromProvider}",
                        $"to: {this.ToProvider}",
                    };
                    throw new ErrorException(Errors.Automation.Provider.PatchObject.CannotCopyPrimitiveToObject(
                        objectToPatch.ToString(),
                        errorData,
                        additionalDetails));
                }
            }
            else
            {
                patchDocument.Copy(this.from, this.to);
            }

            patchDocument.ApplyTo(objectToPatch);
            obj = JObject.FromObject(objectToPatch);
            return new PatchObjectModel(PostPatchDirective.Continue, obj);
        }

        private JObject GetErrorData(JObject obj)
        {
            var details = this.GetGeneralPatchErrorDetails(this.GetType(), this.message, obj);
            details.Add("fromPath", this.from);
            details.Add("toPath", this.to);
            return details;
        }
    }
}
