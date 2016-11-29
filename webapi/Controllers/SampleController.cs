using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web.Http;
using System.Xml.Serialization;
using webapi.Models;
using WebService_Tester.ServiceReference1;
using System.Dynamic;
using System.Threading.Tasks;
using System.Threading;

namespace webapi.Controllers
{
    public class SampleController : ApiController
    {
        [HttpPost]
        public async Task<IHttpActionResult> SST([FromBody]JObject json)
        {
            IHttpActionResult res = await Task.Run<IHttpActionResult>(() =>
            {
                dynamic info = new ExpandoObject();

                //............. request info
                if (json == null)
                {
                    return Ok("No UserName and Password supplied");
                }
                dynamic request = json;
                if (string.IsNullOrEmpty(request.UserName.ToString()) || string.IsNullOrEmpty(request.Password.ToString()))
                {
                    return Ok("No UserName and Password supplied");
                }

                string xml = request.xml.ToString();
                string userName = request.UserName.ToString();
                string password = request.Password.ToString();

                //............. init contentsTreeView
                info.type = "ok";
                ContentsTreeView<BulkRegistrationTransmissionType> contentsTreeView = new ContentsTreeView<BulkRegistrationTransmissionType>();

                contentsTreeView.ContentObject = new BulkRegistrationTransmissionType
                {
                    TransmissionHeader = new TransmissionHeaderType
                    {
                        Transmitter = new TransmissionHeaderTypeTransmitter()
                    },

                };
                contentsTreeView.RefreshTree();
                var p = contentsTreeView.GetType().GetProperty("ContentObject");
                var types =
                    Assembly.GetExecutingAssembly()
                        .GetTypes()
                        .Where(
                            t =>
                                String.Equals(t.Namespace, contentsTreeView.ContentObject.GetType().Namespace,
                                    StringComparison.Ordinal) && t.IsClass && (!t.IsAbstract))
                        .ToArray();

                //............. write xml string to stream
                MemoryStream stream = new MemoryStream();
                StreamWriter writer = new StreamWriter(stream);
                writer.Write(xml);
                writer.Flush();
                stream.Position = 0;
                var ser = new XmlSerializer(contentsTreeView.ContentObject.GetType(), types);
                p.SetValue(contentsTreeView, ser.Deserialize(stream));
                stream.Close();

                //............. sst check
                ApiServiceClient client = new ApiServiceClient
                {
                    ClientCredentials =
                    {
                        UserName =
                        {
                            UserName = userName,
                            Password = password
                        }
                    }
                };
                try
                {
                    BulkRegistrationAcknowledgementType ack = null;
                    try
                    {
                        ack = client.BulkRegistration(contentsTreeView.ContentObject);
                    }
                    catch (Exception e)
                    {
                        client.Abort();
                        //........ try to register again
                        try
                        {
                            Thread.Sleep(1000);
                            client = new ApiServiceClient
                            {
                                ClientCredentials =
                                {
                                    UserName =
                                    {
                                        UserName = userName,
                                        Password = password
                                    }
                                }
                            };
                            ack = client.BulkRegistration(contentsTreeView.ContentObject);
                        }
                        catch (Exception ex)
                        {
                            throw ex;
                        }
                    }
                    ContentsTreeView<BulkRegistrationAcknowledgementType> resp_contentsTreeView = 
                        new ContentsTreeView<BulkRegistrationAcknowledgementType>();
                    resp_contentsTreeView.ContentObject = ack;
                    resp_contentsTreeView.RefreshTree();


                    MemoryStream resp_stream = new MemoryStream();
                    StreamWriter resp_writer = new StreamWriter(resp_stream);
                    var resp_types = Assembly.GetExecutingAssembly().GetTypes().Where(t => String.Equals(t.Namespace, 
                        resp_contentsTreeView.ContentObject.GetType().Namespace, StringComparison.Ordinal) && t.IsClass && (!t.IsAbstract)).ToArray();
                    //var writer = new StreamWriter(saveFileDialog1.FileName);
                    var resp_ser = new XmlSerializer(resp_contentsTreeView.ContentObject.GetType(), types);
                    resp_ser.Serialize(resp_writer, resp_contentsTreeView.ContentObject);
                    resp_stream.Position = 0;
                    StreamReader reader = new StreamReader(resp_stream);
                    string text = reader.ReadToEnd();
                    stream.Close();
                    info.msg = text;

                    client.Abort();
                    //client.Close();
                }
                catch (TimeoutException toException)
                {
                    //The application timed out waiting for a response.
                    info.type = "error";
                    info.msg = string.Format("<exception>{0}</exception>", toException.ToString());
                    client.Abort();
                    return Ok(info);
                }
                return Ok(info);
            });
            return res;
        }

        [HttpPost]
        public IHttpActionResult Test2([FromBody]JObject json)
        {
            dynamic album = json;
            var a = album.id;
            album.name = "json";
            return Ok(album);
        }

    }
}
