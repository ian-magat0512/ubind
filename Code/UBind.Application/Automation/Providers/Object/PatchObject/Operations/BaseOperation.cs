// <copyright file="BaseOperation.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Automation.Providers.Object.PatchObject
{
    using System;
    using System.Dynamic;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.JsonPatch;
    using Microsoft.Json.Pointer;
    using Newtonsoft.Json.Linq;
    using UBind.Application.Automation.Enums;
    using UBind.Domain;
    using UBind.Domain.Exceptions;

    /// <summary>
    /// Base class of all path operation. This contains the common functions that will be used in the operation.
    /// </summary>
    public class BaseOperation
    {
        public virtual Task<PatchObjectModel> Execute(JObject obj, IProviderContext providerContext)
        {
            throw new NotImplementedException();
        }

        protected static string MergePath(string segment1, string segment2)
        {
            string result;
            if (segment1 == "/")
            {
                segment1 = string.Empty;
            }

            if (segment2.StartsWith("/"))
            {
                result = segment1 + segment2;
            }
            else
            {
                result = segment1 + "/" + segment2;
            }

            return !result.StartsWith("/")
                ? "/" + result
                : result;
        }

        protected Task<PatchObjectModel> Execute(JObject obj, PrePatchDirective? action, JsonPatchDocument patchDocument, Error error)
        {
            dynamic objectToPatch;
            switch (action)
            {
                case PrePatchDirective.Continue:
                    return Task.FromResult(new PatchObjectModel(PostPatchDirective.Continue, obj));
                case PrePatchDirective.End:
                    return Task.FromResult(new PatchObjectModel(PostPatchDirective.End, obj));
                case PrePatchDirective.Add:
                case PrePatchDirective.Replace:
                    objectToPatch = obj.ToObject<ExpandoObject>()!;
                    patchDocument.ApplyTo(objectToPatch);
                    obj = JObject.FromObject(objectToPatch);
                    return Task.FromResult(new PatchObjectModel(PostPatchDirective.Continue, obj));
                case PrePatchDirective.RaiseError:
                default:
                    throw new ErrorException(error);
            }
        }

        protected PrePatchDirective GetPrePatchAction(string actionName)
        {
            if (string.IsNullOrWhiteSpace(actionName))
            {
                return PrePatchDirective.None;
            }

            return (PrePatchDirective)Enum.Parse(typeof(PrePatchDirective), actionName, true);
        }

        protected bool IsPathExists(string path, JObject obj)
        {
            var jPointer = new JsonPointer(path);
            try
            {
                var obtainedValue = jPointer.Evaluate(obj);
                return true;
            }
            catch
            {
                return false;
            }
        }

        protected JObject GetGeneralPatchErrorDetails(Type childType, string? error, JObject obj)
        {
            return new JObject()
            {
                { "operation", $"{childType.Name.Replace("Operation", string.Empty)}" },
                { ErrorDataKey.ErrorMessage, error },
                { "object", obj.ToString() },
            };
        }
    }
}
