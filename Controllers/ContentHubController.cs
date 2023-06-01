using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Stylelabs.M.Base.Querying;
using Stylelabs.M.Framework.Essentials.LoadConfigurations;
using Stylelabs.M.Framework.Essentials.LoadOptions;
using Stylelabs.M.Sdk.Contracts.Querying;

namespace ContentHub_WebClientSDK_Assets.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ContentHubController : ControllerBase
    {
        public ContentHubController()
        {
        }

        [HttpGet(Name = "GetReportAssets")]
        public async Task<IActionResult> Get()
        {
            string currentTitle;
            string currentFileName;
            JToken properties;
            var workbookResult = new XLWorkbook();
            var worksheetResult = workbookResult.Worksheets.Add("Content Hub");
            int counter = 1;
            //Set the Excel column names
            worksheetResult.Cell("A" + counter).Value = "ID";
            worksheetResult.Cell("B" + counter).Value = "FileName";
            worksheetResult.Cell("C" + counter).Value = "Title";
            worksheetResult.Cell("D" + counter).Value = "Extension";
            try
            {
                await MClient.Client.TestConnectionAsync();
                Console.WriteLine("Connection is Successful!");
                var assetLoadConfiguration = new EntityLoadConfiguration()
                {
                    PropertyLoadOption = new PropertyLoadOption("Title", "FileName", "FileProperties"),
                    RelationLoadOption = RelationLoadOption.None,
                    CultureLoadOption = CultureLoadOption.Default,
                };

                //Query the assets

                var query = Query.CreateQuery(entities =>
                from e in entities
                where e.DefinitionName == "M.Asset"
                select e);

                //Here the documentation https://doc.sitecore.com/ch/en/developers/30/cloud-dev/sdk-common-documentation--querying-client.html
                //Iterate query result entitites
                IEntityIterator iterator = MClient.Client.Querying.CreateEntityIterator(query, assetLoadConfiguration);
                while (await iterator.MoveNextAsync())
                {
                    var entities = iterator.Current.Items;
                    //Foreach the entities type asset
                    foreach (var entity in entities)
                    {
                        //Get properties values
                        currentTitle = entity.GetPropertyValue<string>("Title");
                        currentFileName = entity.GetPropertyValue<string>("FileName");
                        properties = entity.GetPropertyValue<JToken>("FileProperties");
                        if (properties != null)
                        {
                            JObject inner = properties["properties"].Value<JObject>();
                            var token = inner.SelectToken("extension");
                            //Verify if extension exists
                            if (token != null)
                            {
                                //Get the extension value from FileProperties
                                var extension = inner["extension"].ToString();
                                counter++;
                                worksheetResult.Cell("A" + counter).Value = entity.Id;
                                worksheetResult.Cell("B" + counter).Value = currentFileName;
                                worksheetResult.Cell("C" + counter).Value = currentTitle;
                                worksheetResult.Cell("D" + counter).Value = extension;
                            }
                        }
                    }
                }
                var stream = new System.IO.MemoryStream();
                workbookResult.SaveAs(stream);
                stream.Position = 0;
                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "AssetsContentHub.xlsx");

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception Occured: {ex.Message}");
                return null;
            }
        }
    }
}