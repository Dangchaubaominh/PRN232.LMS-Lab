using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Xml;
using System.Xml.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Options;

namespace PRN232.LMS.API.Formatters;

/// <summary>
/// Writes responses as XML for "Accept: application/xml" (or text/xml).
/// <para>
/// The value is first serialized with the API's JSON settings and the resulting tree is converted to
/// XML, so both formats always carry the same members and names. The framework's XmlSerializer and
/// DataContractSerializer formatters cannot be used here: they reject records, dictionaries (field
/// selection, validation errors) and members typed as <see cref="object"/>.
/// </para>
/// Mapping: the root element is &lt;response&gt;; an object becomes child elements named after its
/// properties; an array becomes repeated &lt;item&gt; elements; null becomes xsi:nil="true". Names
/// that are not valid XML (e.g. the "$.fullName" key of a JSON parse error) are escaped with
/// <see cref="XmlConvert.EncodeLocalName"/>.
/// </summary>
public class ApiXmlOutputFormatter : TextOutputFormatter
{
    private static readonly XNamespace Xsi = "http://www.w3.org/2001/XMLSchema-instance";

    public ApiXmlOutputFormatter()
    {
        SupportedMediaTypes.Add("application/xml");
        SupportedMediaTypes.Add("text/xml");
        SupportedEncodings.Add(Encoding.UTF8);
        SupportedEncodings.Add(Encoding.Unicode);
    }

    protected override bool CanWriteType(Type? type) => true;

    public override async Task WriteResponseBodyAsync(OutputFormatterWriteContext context, Encoding selectedEncoding)
    {
        var jsonOptions = context.HttpContext.RequestServices.GetRequiredService<IOptions<JsonOptions>>().Value.JsonSerializerOptions;
        var json = JsonSerializer.SerializeToNode(context.Object, context.ObjectType ?? typeof(object), jsonOptions);

        var root = ToElement("response", json);
        root.Add(new XAttribute(XNamespace.Xmlns + "xsi", Xsi));
        var document = new XDocument(new XDeclaration("1.0", selectedEncoding.WebName, null), root);

        await using var writer = context.WriterFactory(context.HttpContext.Response.Body, selectedEncoding);
        await document.SaveAsync(writer, SaveOptions.None, context.HttpContext.RequestAborted);
        await writer.FlushAsync();
    }

    private static XElement ToElement(string name, JsonNode? node)
    {
        var element = new XElement(XmlConvert.EncodeLocalName(name));
        switch (node)
        {
            case null:
                element.Add(new XAttribute(Xsi + "nil", true));
                break;
            case JsonObject jsonObject:
                foreach (var (key, value) in jsonObject)
                {
                    element.Add(ToElement(key, value));
                }
                break;
            case JsonArray jsonArray:
                foreach (var item in jsonArray)
                {
                    element.Add(ToElement("item", item));
                }
                break;
            default:
                // A JsonValue: strings come back unquoted, numbers and booleans as their JSON text.
                element.Value = node.ToString();
                break;
        }

        return element;
    }
}
