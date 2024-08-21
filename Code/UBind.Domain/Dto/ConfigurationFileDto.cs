// <copyright file="ConfigurationFileDto.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Dto;

using NodaTime;

/// <summary>
/// Data transfer object for properties of OneDrive source files.
/// for a specific product.
/// </summary>
public class ConfigurationFileDto
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigurationFileDto"/> class.
    /// </summary>
    /// <remarks>Parameterless constructor for JSON deserializer.</remarks>
    public ConfigurationFileDto()
    {
    }

    /// <summary>
    /// Gets or sets the file id in OneDrive.
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// Gets or sets gets the name of the source file.
    /// </summary>
    public string Path { get; set; }

    public Instant? LastModifiedTimestamp { get; set; }

    /// <summary>
    /// Gets or sets the url for opening the file in browser.
    /// </summary>
    public string ResourceUrl { get; set; }

    /// <summary>
    /// Gets the source type.
    /// </summary>
    public string SourceType => this.GetSourceType();

    public bool IsBrowserViewable { get; set; } = true;

    /// <summary>
    /// Gets or sets the web form type.
    /// </summary>
    public WebFormAppType WebFormAppType { get; set; }

    private string GetSourceType()
    {
        if (this.Path.EndsWith(".docx"))
        {
            return "Document Template";
        }

        if (this.Path.EndsWith(".pdf"))
        {
            return "Static Document";
        }

        if (this.Path.EndsWith(".jpg")
            || this.Path.EndsWith(".jpeg")
            || this.Path.EndsWith(".gif")
            || this.Path.EndsWith(".png")
            || this.Path.EndsWith(".webp")
            || this.Path.EndsWith(".svg"))
        {
            return "Image";
        }

        if (this.Path.EndsWith(".css"))
        {
            return "Style Sheet";
        }

        if (this.Path.Contains("Workbook"))
        {
            return "Workbook";
        }

        if (this.Path.EndsWith("workflow.json"))
        {
            return "Workflow Configuration";
        }

        if (this.Path.EndsWith("payment.json"))
        {
            return "Back-End Payment Configuration";
        }

        if (this.Path.EndsWith("payment.form.json"))
        {
            return "Front-End Payment Configuration";
        }

        if (this.Path.EndsWith("funding.json"))
        {
            return "Premium Funding Configuration";
        }

        if (this.Path.EndsWith("product.json"))
        {
            return "Product Configuration";
        }

        if (this.Path.EndsWith("product.json"))
        {
            return "Product Configuration";
        }

        if (this.Path.EndsWith("form.model.json"))
        {
            return "Testing Form Model";
        }

        if (this.Path.EndsWith("integrations.json"))
        {
            return "Integrations Configuration";
        }

        if (this.Path.EndsWith("automations.json"))
        {
            return "Automations Configuration";
        }

        if (this.Path.Contains("email"))
        {
            return "Email Template";
        }

        if (this.Path.Contains("configuration"))
        {
            return "Configuration";
        }

        return "File";
    }
}
