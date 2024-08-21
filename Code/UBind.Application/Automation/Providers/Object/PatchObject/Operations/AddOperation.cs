// <copyright file="AddOperation.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

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
    /// This class is responsible for performing the add patch operation on the object.
    /// </summary>
    public class AddOperation : BaseOperation
    {
        private dynamic? value;
        private string? message;
        private string? path;

        public AddOperation(
            IProvider<Data<string>>? pathProvider,
            IProvider<IData> valueProvider,
            PrePatchDirective? whenParentPropertyNotFoundAction,
            PrePatchDirective? whenPropertyAlreadyExistsAction)
        {
            this.PathProvider = pathProvider;
            this.ValueProvider = valueProvider;
            this.WhenParentPropertyNotFoundAction = whenParentPropertyNotFoundAction;
            this.WhenPropertyAlreadyExistsAction = whenPropertyAlreadyExistsAction;
        }

        private IProvider<Data<string>>? PathProvider { get; set; }

        private IProvider<IData> ValueProvider { get; set; }

        private PrePatchDirective? WhenParentPropertyNotFoundAction { get; set; }

        private PrePatchDirective? WhenPropertyAlreadyExistsAction { get; set; }

        public override async Task<PatchObjectModel> Execute(JObject obj, IProviderContext providerContext)
        {
            this.value = (await this.ValueProvider.Resolve(providerContext)).GetValueOrThrowIfFailed().GetValueFromGeneric();
            this.path = (await this.PathProvider.ResolveValueIfNotNull(providerContext))?.DataValue ?? string.Empty;
            bool targetIsRoot = this.path == string.Empty || this.path == "/";
            bool valueIsObject = DataObjectHelper.IsObject(this.value);
            var patchDocument = new JsonPatchDocument();
            bool pathExistsForGivenPath = this.IsPathExists(this.path, obj);

            int lastSlashIndex = this.path.LastIndexOf("/");
            string parentPath = lastSlashIndex == -1
                ? string.Empty
                : this.path.Remove(lastSlashIndex);
            if (!this.IsPathExists(parentPath, obj)
                && this.WhenParentPropertyNotFoundAction != PrePatchDirective.None)
            {
                this.message = "Parent property not found";
                var errorData = this.GetErrorData(obj);
                var error = Errors.Automation.Provider.PatchObject.PathNotFound("add", this.message, errorData);
                return await this.Execute(obj, this.WhenParentPropertyNotFoundAction, patchDocument, error);
            }

            if (!targetIsRoot && !valueIsObject && pathExistsForGivenPath
                && this.WhenPropertyAlreadyExistsAction != PrePatchDirective.None)
            {
                patchDocument.Replace(this.path, this.value);
                this.message = "Property already exists";
                var errorData = this.GetErrorData(obj);
                var error = Errors.Automation.Provider.PatchObject.PathAlreadyExists("add", this.message, errorData);
                return await this.Execute(obj, this.WhenPropertyAlreadyExistsAction, patchDocument, error);
            }

            if (targetIsRoot || valueIsObject)
            {
                if (this.value is IDictionary valueDictionary)
                {
                    // we'll have to add each of the properties one by one due to limitations of the JSONPatch library
                    if (pathExistsForGivenPath || targetIsRoot)
                    {
                        foreach (DictionaryEntry entry in valueDictionary)
                        {
                            patchDocument.Add(MergePath(this.path, entry.Key.ToString()!), entry.Value);
                        }
                    }
                    else
                    {
                        patchDocument.Add(this.path, valueDictionary);
                    }
                }
                else if (this.value is JObject valueJObject)
                {
                    if (pathExistsForGivenPath || targetIsRoot)
                    {
                        foreach (var jProperty in valueJObject)
                        {
                            patchDocument.Add(MergePath(this.path, jProperty.Key), jProperty.Value);
                        }
                    }
                    else
                    {
                        patchDocument.Add(this.path, valueJObject);
                    }
                }
                else if (valueIsObject)
                {
                    // value is a strongly-typed object, thus convert first then add the properties one by one
                    JObject valueObjectAsJObject = JObject.FromObject(this.value);
                    if (pathExistsForGivenPath || targetIsRoot)
                    {
                        foreach (var jProperty in valueObjectAsJObject)
                        {
                            patchDocument.Add(MergePath(this.path, jProperty.Key), jProperty.Value);
                        }
                    }
                    else
                    {
                        patchDocument.Add(this.path, valueObjectAsJObject);
                    }
                }
                else
                {
                    // If you're adding to an object, you need to add an object with properties - you can't just add a value.
                    var errorData = this.GetErrorData(obj);
                    var additionalDetails = new List<string>
                    {
                        $"path: {this.path}",
                    };
                    throw new ErrorException(Errors.Automation.Provider.PatchObject.CannotAddPrimitiveToObject(
                        this.value.ToString(),
                        errorData,
                        additionalDetails));
                }
            }
            else
            {
                patchDocument.Add(this.path, this.value);
            }

            ExpandoObject objectToPatch = obj.ToObject<ExpandoObject>()!;
            patchDocument.ApplyTo(objectToPatch);
            obj = JObject.FromObject(objectToPatch);
            return new PatchObjectModel(PostPatchDirective.Continue, obj);
        }

        private JObject GetErrorData(JObject obj)
        {
            JToken jValueToken = DataObjectHelper.IsPrimitive(this.value)
                ? new JValue(this.value)
                : DataObjectHelper.IsArray(this.value)
                    ? JArray.FromObject(this.value)
                    : JObject.FromObject(this.value);
            var details = this.GetGeneralPatchErrorDetails(this.GetType(), this.message, obj);
            details.Add("path", this.path);
            details.Add("value", jValueToken);
            return details;
        }
    }
}
