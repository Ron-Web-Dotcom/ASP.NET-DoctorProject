<%@ WebHandler Language="C#" Class="SymptomHandler" %>

using System;
using System.Web;
using System.Web.Script.Serialization;

public class SymptomHandler : IHttpHandler
{
    public void ProcessRequest(HttpContext context)
    {
        context.Response.ContentType = "application/json";
        context.Response.Cache.SetCacheability(HttpCacheability.NoCache);

        if (!context.Request.HttpMethod.Equals("POST", StringComparison.OrdinalIgnoreCase))
        {
            context.Response.StatusCode = 405;
            context.Response.Write("{\"error\":\"Method not allowed\"}");
            return;
        }

        string symptoms = context.Request.Form["symptoms"];

        if (string.IsNullOrWhiteSpace(symptoms))
        {
            context.Response.Write("{\"error\":\"No symptoms provided\"}");
            return;
        }

        if (symptoms.Length > 1000)
            symptoms = symptoms.Substring(0, 1000);

        OpenAIService.SpecialistResult result = OpenAIService.GetSpecialistRecommendation(symptoms);

        var serializer = new JavaScriptSerializer();
        context.Response.Write(serializer.Serialize(new
        {
            specialist = result.Specialist,
            reason     = result.Reason
        }));
    }

    public bool IsReusable { get { return false; } }
}
