// <copyright file="RemoveOperation.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Automation.Providers.Object.PatchObject
{
    using System.Dynamic;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.JsonPatch;
    using Newtonsoft.Json.Linq;
    using UBind.Application.Automation.Extensions;
    using UBind.Domain;

    /// <summary>
    /// This class is responsible for performing the remove patch operation on the object.
    /// </summary>
    public class RemoveOperation : BaseOperation
    {
        private string path = null!;

        public RemoveOperation(
            IProvider<Data<string>> pathProvider,
            PrePatchDirective whenPropertyNotFoundAction)
        {
            this.PathProvider = pathProvider;
            this.WhenPropertyNotFoundAction = whenPropertyNotFoundAction;
        }

        public IProvider<Data<string>> PathProvider { get; set; }

        public PrePatchDirective WhenPropertyNotFoundAction { get; set; }

        public override async Task<PatchObjectModel> Execute(JObject obj, IProviderContext providerContext)
        {
            dynamic objectToPatch = obj.ToObject<ExpandoObject>()!;
            this.path = (await this.PathProvider.Resolve(providerContext)).GetValueOrThrowIfFailed();
            var patchDocument = new JsonPatchDocument();

            if (this.path != string.Empty && this.path != "/" && !this.IsPathExists(this.path, obj) &&
                this.WhenPropertyNotFoundAction != PrePatchDirective.None)
            {
                var details = this.GetGeneralPatchErrorDetails(this.GetType(), "Property not found", obj);
                details.Add("path", this.path);
                var error = Errors.Automation.Provider.PatchObject.PathNotFound("remove", "Property not found", details);
                return await this.Execute(obj, this.WhenPropertyNotFoundAction, patchDocument, error);
            }

            if (this.path == string.Empty || this.path == "/")
            {
                // since we're removing root, we'll just replace it with an empty object.
                obj = new JObject();
            }
            else
            {
                patchDocument.Remove(this.path);
                patchDocument.ApplyTo(objectToPatch);
                obj = JObject.FromObject(objectToPatch);
            }

            return new PatchObjectModel(PostPatchDirective.Continue, obj);
        }
    }
}
