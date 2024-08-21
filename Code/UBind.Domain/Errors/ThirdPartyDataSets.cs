// <copyright file="ThirdPartyDataSets.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Domain;

using Newtonsoft.Json.Linq;
using System.Net;

/// <summary>
/// Allows enumeration of all application errors as outlined here: https://enterprisecraftsmanship.com/posts/advanced-error-handling-techniques/.
/// </summary>
public static partial class Errors
{
    /// <summary>
    /// A helper class for third party datasets error message builder.
    /// </summary>
    public static class ThirdPartyDataSets
    {
        public static Error NotFound(Guid updateJobId) => new Error(
            "updaterjob.job.not.found",
            $"We couldn't find the updater job id '{updateJobId}'",
            $"When trying to find the updater job id '{updateJobId}', nothing came up. Please check if you've entered the correct updater job Id.",
            HttpStatusCode.NotFound);

        public static Error InvalidPatchState(Guid updaterjobid, string patchState) => new Error(
            "dataset.patch.job.state.not.valid",
            $"We couldn't patch the updater job id '{updaterjobid}'",
            $"An attempt was made to patch the updater job with id '{updaterjobid}' with state '{patchState}', but the state was invalid. Please check if you've entered the correct job state.",
            HttpStatusCode.BadRequest,
            null,
            new JObject()
            {
                { "updaterjobid", updaterjobid.ToString() },
                { "patchState", patchState },
            });

        public static Error CannotCreateUpdaterJob() => new Error(
            "updaterjob.not.yet.created",
            $"Unable to create the updater job job",
            $"An attempt was made to create an updater job, but no updater job record has been created. Please get in touch with customer support.",
            HttpStatusCode.BadRequest);

        public static class Gnaf
        {
            public static Error CannotCancelCompletedJob() => new Error(
                "GnafUpdateJob.CantCancelCompleteJob",
                $"You can't cancel a completed update job",
                $"The target job cannot be cancelled anymore as it is already completed.",
                HttpStatusCode.Conflict);

            public static Error InvalidUpdateJobStatus() => new Error(
                "GnafUpdateJob.InvalidUpdateJobStatus",
                $"You specified an invalid update job status.",
                $"The specified update job status is invalid or null.",
                HttpStatusCode.BadRequest);
        }

        public static class Nfid
        {
            public static Error GnafAddressIdNotFound(string gnafAddressId) => new Error(
                "nfid.gnafaddressid.not.found",
                $"NFID information for the specified G-NAF Address ID could not be found",
                $"The NFID dataset does not contain information for G-NAF Address ID \"{gnafAddressId}\". Please check that you have entered the correct G-NAF Address ID.",
                HttpStatusCode.NotFound,
                new List<string>
                {
                    $"G-NAF Address ID: {gnafAddressId}",
                },
                new JObject()
                {
                    { "gnafAddressId", gnafAddressId },
                });

            public static Error NoValidDataSetFileFound(string datasetUrl) => new Error(
               "nfid.downloaded.file.did.not.contain.valid.dataset",
               $"Downloaded file did not contain a valid NFID dataset",
               $"The file downloaded from the specified dataset URL \"{datasetUrl}\" did not contain a valid NFID dataset. Please check that you have entered the correct dataset URL.",
               HttpStatusCode.BadRequest,
               new List<string>
               {
                    $"Dataset URL: {datasetUrl}",
               },
               new JObject()
               {
                    { "datasetUrl", datasetUrl },
               });

            public static Error DataSetCannotBeDownloaded(string datasetUrl) => new Error(
                "nfid.dataset.could.not.be.downloaded",
                $"NFID dataset could not be downloaded",
                $"An NFID dataset could not be downloaded using the specified dataset URL \"{datasetUrl}\". Please check that you have entered the correct dataset URL.",
                HttpStatusCode.NotFound,
                new List<string>
                {
                    $"Dataset URL: {datasetUrl}",
                },
                new JObject()
                {
                    { "datasetUrl", datasetUrl },
                });
        }

        public static class RedBook
        {
            public static Error RequiredParametersMissing(string[] parameterNames)
            {
                var parameters = string.Join(", ", parameterNames.Select(name => $"\"{name}\"").ToList());
                var verb = parameterNames.Length > 1 ? "were" : "was";
                var parameterNoun = parameterNames.Length > 1 ? "parameters" : "parameter";
                return new Error(
                  "required.request.parameter.missing",
                  $"A required request {parameterNoun} {verb} missing",
                  $"When trying to process your request, the attempt failed because the required {parameterNoun} ({parameters}) {verb} missing. To resolve the issue, please ensure that all required parameters are included and have valid values. If you require further assistance please contact technical support.",
                  HttpStatusCode.BadRequest);
            }

            public static Error YearGroupNotFound(string yearGroup) => new Error(
                "redbook.year.group.not.found",
                $"Vehicle year group not found",
                $"The Redbook year group \"{yearGroup}\" was not found in the Redbook dataset. Please check that you have entered the correct details.",
                HttpStatusCode.NotFound,
                null,
                new JObject()
                {
                    { "yeargroup", yearGroup.ToString() },
                });

            public static Error VehicleKeyNotFound(string vehicleKey) => new Error(
                "redbook.vehicle.key.not.found",
                $"Vehicle key not found",
                $"The Redbook vehicle key  ('{vehicleKey}') was not matched against a vehicle make in the Redbook dataset. Please check that you have entered the correct details.",
                HttpStatusCode.NotFound,
                null,
                new JObject()
                {
                    { "vehiclekey", vehicleKey },
                });

            public static Error MakeCodeNotFound(string makeCode) => new Error(
                "redbook.make.code.not.found",
                $"Vehicle make not found",
                $"The Redbook vehicle make code ('{makeCode}') was not matched against a vehicle make in the Redbook dataset. Please check that you have entered the correct details.",
                HttpStatusCode.NotFound,
                null,
                new JObject()
                {
                    { "makecode", makeCode },
                });

            public static Error MakeCodeAndFamilyCodeNotFound(string makeCode, string familyCode) => new Error(
                "redbook.family.code.not.found.under.make",
                $"Vehicle family not found under specified make",
                $"The Redbook family code ('{familyCode}') was not matched against a vehicle family under the specified make ('{makeCode}') in the Redbook dataset. Please check that you have entered the correct details.",
                HttpStatusCode.NotFound,
                null,
                new JObject()
                {
                    { "makecode", makeCode },
                    { "familycode", familyCode },
                });

            public static Error MakeCodeFamilyCodeAndYearNotFound(string makeCode, string familyCode, int year) => new Error(
                "redbook.makecodefamilycodeandyear.not.found",
                $"We couldn't find the RedBook vehicle year '{year}' for the specified family code '{familyCode}' and make code '{makeCode}'",
                $"When trying to find the RedBook vehicle year '{year}' for the specified family code '{familyCode}' for the specified make code '{makeCode}' , nothing came up. Please check if you've entered the correct RedBook vehicle year, family code and make code.",
                HttpStatusCode.NotFound,
                null,
                new JObject()
                {
                    { "makecode", makeCode },
                    { "familycode", familyCode },
                    { "year", year },
                });
        }

        public static class GlassGuide
        {
            public static Error MakeCodeParameterMissing() => new Error(
                $"required.request.parameter.missing",
                $"A required request parameter was missing",
                $"When trying to process your request, the attempt failed because the required parameter \"make\" was missing. "
                + "To resolve the issue, please ensure that all required parameters are included and have valid values. If you require further assistance please contact technical support.",
                HttpStatusCode.BadRequest);

            public static Error FamilyCodeParameterMissing() => new Error(
                $"required.request.parameter.missing",
                $"A required request parameter was missing",
                $"When trying to process your request, the attempt failed because the required parameter \"family\" was missing. "
                + "To resolve the issue, please ensure that all required parameters are included and have valid values. If you require further assistance please contact technical support.",
                HttpStatusCode.BadRequest);

            public static Error YearParameterMissing() => new Error(
                $"required.request.parameter.missing",
                $"A required request parameter was missing",
                $"When trying to process your request, the attempt failed because the required parameter \"year\" was missing. "
                + "To resolve the issue, please ensure that all required parameters are included and have valid values. If you require further assistance please contact technical support.",
                HttpStatusCode.BadRequest);

            public static Error GlassCodeParameterMissing() => new Error(
                $"required.request.parameter.missing",
                $"A required request parameter was missing",
                $"When trying to process your request, the attempt failed because the required parameter \"glassCode\" was missing. "
                + "To resolve the issue, please ensure that all required parameters are included and have valid values. If you require further assistance please contact technical support.",
                HttpStatusCode.BadRequest);

            public static Error YearGroupInvalid(int yearGroup) => new Error(
                $"request.parameter.invalid",
                $"A request parameter had an invalid value",
                $"When trying to process your request, the attempt failed because the \"year\" parameter had an invalid value (\"{yearGroup}\"). The parameter value was out of range. "
                + $"To resolve the issue, please ensure that the value is within the range of \"1900\" to \"{DateTime.Now.Year}\". If you require further assistance please contact technical support.",
                HttpStatusCode.BadRequest);

            public static Error YearGroupFormatInvalidOptionalValue(string yearGroup) => new Error(
                $"request.parameter.invalid",
                $"A request parameter had an invalid value",
                $"When trying to process your request, the attempt failed because the \"year\" parameter had an invalid value (\"{yearGroup}\"). The parameter value was non-numeric. "
                + $"To resolve the issue, either omit the \"year\" parameter or ensure that it has a numeric value within the range of \"1900\" to \"{DateTime.Now.Year}\". If you require further assistance please contact technical support.",
                HttpStatusCode.BadRequest);

            public static Error YearGroupFormatInvalidRequiredValue(string yearGroup) => new Error(
                $"request.parameter.invalid",
                $"A request parameter had an invalid value",
                $"When trying to process your request, the attempt failed because the \"year\" parameter had an invalid value (\"{yearGroup}\"). The parameter value was non-numeric. "
                + $"To resolve the issue, please ensure that the value is numeric within the range of \"1900\" to \"{DateTime.Now.Year}\". If you require further assistance please contact technical support.",
                HttpStatusCode.BadRequest);

            public static Error GlassCodeInvalid(string glassCode) => new Error(
                $"request.parameter.invalid",
                $"A request parameter had an invalid value",
                $"When trying to process your request, the attempt failed because the \"glasscode\" parameter had an invalid value (\"{glassCode}\"). The Glass's Guide glass code (\"{glassCode}\") was not found. "
                + $"To resolve the issue, please ensure that the value is in correct format (e.g. TOYLANGC194503FK2022), and it matches a vehicle make in the Glass's Guide dataset. If you require further assistance please contact technical support.",
                HttpStatusCode.BadRequest);
        }
    }
}
