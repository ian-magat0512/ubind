// <copyright file="ReplaceOperation.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Automation.Providers.Object.PatchObject
{
    using System.Dynamic;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.JsonPatch;
    using Newtonsoft.Json.Linq;
    using UBind.Application.Automation.Extensions;
    using UBind.Application.Automation.Helper;
    using UBind.Domain;

    /// <summary>
    /// This class is responsible for performing the replace patch operation on the object.
    /// </summary>
    public class ReplaceOperation : BaseOperation
    {
        private dynamic value = null!;
        private string? message;
        private string path = null!;

        public ReplaceOperation(
            IProvider<Data<string>>? pathProvider,
            IProvider<IData> valueProvider,
            PrePatchDirective whenPropertyNotFoundAction,
            PrePatchDirective whenParentPropertyNotFoundAction)
        {
            this.PathProvider = pathProvider;
            this.ValueProvider = valueProvider;
            this.WhenPropertyNotFoundAction = whenPropertyNotFoundAction;
            this.WhenParentPropertyNotFoundAction = whenParentPropertyNotFoundAction;
        }

        public IProvider<Data<string>>? PathProvider { get; set; }

        public IProvider<IData> ValueProvider { get; set; }

        public PrePatchDirective WhenPropertyNotFoundAction { get; set; }

        public PrePatchDirective WhenParentPropertyNotFoundAction { get; set; }

        public override async Task<PatchObjectModel> Execute(JObject obj, IProviderContext providerContext)
        {
            this.value = (await this.ValueProvider.Resolve(providerContext)).GetValueOrThrowIfFailed().GetValueFromGeneric();
            this.path = (await this.PathProvider.ResolveValueIfNotNull(providerContext))?.DataValue ?? string.Empty;
            bool targetIsRoot = this.path == string.Empty || this.path == "/";
            bool valueIsObject = DataObjectHelper.IsObject(this.value);
            var patchDocument = new JsonPatchDocument();

            int lastSlashIndex = this.path.LastIndexOf("/");
            string parentPath = lastSlashIndex == -1
                ? string.Empty
                : this.path.Remove(lastSlashIndex);
            if (!this.IsPathExists(parentPath, obj)
                && this.WhenParentPropertyNotFoundAction != PrePatchDirective.None)
            {
                this.message = "Parent property not found";
                var details = this.GetErrorData(obj);
                var error = Errors.Automation.Provider.PatchObject.PathNotFound("replace", this.message, details);
                return await this.Execute(obj, this.WhenParentPropertyNotFoundAction, patchDocument, error);
            }

            if (!targetIsRoot && !valueIsObject && !this.IsPathExists(this.path, obj)
                && this.WhenPropertyNotFoundAction != PrePatchDirective.None)
            {
                patchDocument.Add(this.path, this.value);
                this.message = "Property not found";
                var details = this.GetErrorData(obj);
                var error = Errors.Automation.Provider.PatchObject.PathNotFound("replace", this.message, details);
                return await this.Execute(obj, this.WhenPropertyNotFoundAction, patchDocument, error);
            }

            if (targetIsRoot)
            {
                // since we're just replacing the whole thing, we'll just return the resolved value
                obj = JObject.FromObject(this.value);
            }
            else
            {
                dynamic objectToPatch = obj.ToObject<ExpandoObject>()!;
                patchDocument.Replace(this.path, this.value);
                patchDocument.ApplyTo(objectToPatch);
                obj = JObject.FromObject(objectToPatch);
            }

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
