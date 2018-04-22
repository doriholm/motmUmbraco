using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using Umbraco.Web.WebApi;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Umbraco.Core.Models;
using Umbraco.Core;
using Umbraco.Web;
using umbraco;

namespace manofthematch.Core.Controllers.WebAPI
{
    [Route("api/[controller]")]
    public class PostsApiController : UmbracoApiController
    {
        [HttpGet]
        public string Test()
        {
            //creating an object from result class
            result result = new result();
            var cs = Services.ContentService;
            // getting children of sports
            var content = cs.GetChildren(1082);

            dynamic obj = new JObject();
            var objplayer = new JObject
            {
                { "player", new JObject {
                    { "playerguid", "" },
                    { "playerid", "" },
                    { "playername", "" }
                }
                }
            };
            int i = 0;
            int i2 = 0;
            int i3 = 0;
            int i4 = 0;
            int i5 = 0;
            int i6 = 0;
            int i7 = 0;
            foreach (var item in content)
            {
                var j = item.GetValue<Udi>("imagepost");
                var smt = Umbraco.GetIdForUdi(j);
                var umbHelper = new UmbracoHelper(UmbracoContext.Current);
                var k = umbHelper.Media(smt);
                obj["sport" + i] = new JObject
                {
                    { "SportName", item.GetValue<string>("namesport")  },
                    { "SportPic", k.Url },
                    { "clubs" , new JObject {} }

                };
                var content2 = cs.GetChildren(item.Id); // getting clubs

                foreach (var item2 in content2)
                {
                    obj["sport" + i]["clubs"]["club" + i2] = new JObject
                 {
                     { "ClubName", item2.GetValue<string>("clubName") },
                     { "teams" , new JObject { } }
                 };
                    var content3 = cs.GetChildren(item2.Id);
                    foreach (var item3 in content3)  //getting teams
                    {
                        var j1 = item3.GetValue<Udi>("imageTeam");
                        var smt1 = Umbraco.GetIdForUdi(j1);

                        var k1 = umbHelper.Media(smt1);

                       
                        obj["sport" + i]["clubs"]["club" + i2]["teams"]["team" + i3] = new JObject
                        {
                        { "teamName", item3.GetValue<string>("nameTeam") },
                            { "teamPic", k1.Url },
                            { "matches" , new JObject{} }

                        };
                      
                        var children = cs.GetChildren(item3.Id);            //getting 2nodes from teams
                        foreach (var item4 in children)
                        {
                            var matches = cs.GetChildren(item4.Id);
                            if (item4.ContentType.Alias == "players")           //if players get all players and store in array
                            {
                                var playerchildren = item4.Id;
                                var playerall = cs.GetChildren(playerchildren);

                                foreach (var item5 in playerall)
                                {
                                    var key = item5.Key.ToString();
                                    key = key.Replace("umb://document/", "");
                                    key = key.Replace("-", "");
                                    if (i6 == 0)
                                    {
                                        objplayer["player"] = new JObject
                                    {
                                        { "playerid", item5.Id },
                                        { "playerguid", key },
                                        { "playername", item5.GetValue<string>("namePlayer") }
                                    };
                                    }
                                    else
                                    {

                                        objplayer["player" + item5.Id] = new JObject
                                    {
                                        { "playerid", item5.Id },
                                        { "playerguid", key },
                                        { "playername", item5.GetValue<string>("namePlayer") }
                                    };
                                    }
                                    i6++;
                                }
                                    
                                
                                    

                                }
                            


                            if (item4.ContentType.Alias == "matches") {                 //if matches get selected players and compare
                                foreach (var item6 in matches)
                                {
                                    var playerpick = item6.GetValue<string>("playerpicker");
                                    string[] tokens = playerpick.Split(',');
                                    List<string> token = new List<string> { };
                                    foreach (var item7 in tokens)
                                    {
                                        token.Add(tokens[i5].Replace("umb://document/", ""));
                                        i5++;
                                    }
                                    obj["sport" + i]["clubs"]["club" + i2]["teams"]["team" + i3]["matches"]["match" + i4] = new JObject
                                    {
                                                      { "startDate", item6.GetValue<DateTime>("startDate") },
                                        { "endDate", item6.GetValue<DateTime>("endDate") },
                                        {"players" ,  new JObject()}

                                    };
                                    //compare start
                                    // objplayer jsonobject
                                    // token list
                                    //var hij = 0;

                                    //foreach something in objplayer
                                    //get player guid 
                                    //compare to token if equal
                                    //push other values to obj
                                    foreach (var item8 in objplayer)
                                    {
                                        
                                        var playerguid = item8.Value["playerguid"].ToString();
                                        if (token.Contains(playerguid))
                                        {
                                            result.Name = playerguid;
                                            obj["sport" + i]["clubs"]["club" + i2]["teams"]["team" + i3]["matches"]["match" + i4]["players"]["player"+ i7] = new JObject
                                            {
                                                
                                                    { "playerid", item8.Value["playerid"].ToString() },
                                                     { "playername", item8.Value["playername"].ToString() }
                                                
                                            };
                                        }
                                        i7++;
                                        
                                    }
                                    //bool result1 = root.equals(root2);
                                    //int comparison = root.compareto(root2);
                                    //result1 = root.equals(root2, stringcomparison.ordinal);
                                    //if (result1 == true)
                                    //{
                                    //    hij = result.id;
                                    //}

                                    //var smt = cs.getbyid(hij);
                                    //compare end
                                  
                                    i4++;
                                }
                               
                            }
                             
                           
                        }

                        i3++;
                    }

                    i2++;
                }

                i++;
            }

            var output = JsonConvert.SerializeObject(obj);



            return output;


        }

        [Umbraco.Web.WebApi.UmbracoAuthorize]
        [HttpPost]
        public NewMatch PostMatch([FromBody] NewMatch nm)
        {
            var cs = Services.ContentService;
            var newMatch = cs.CreateContent(nm.Opponent, nm.TeamId, "matchItem");
            cs.SaveAndPublishWithStatus(newMatch);
            

            NewMatch team = new NewMatch
            {
                TeamId = nm.TeamId,
                Opponent = nm.Opponent
            };
           
            return team;
        }

        public class NewMatch
        {
            public int TeamId { set; get; }
            public string Opponent { set; get; }

        }

        public class result
        {
            public int Id { get; set; }
            public string key { get; set; }

            public object Name { get; set; }

        }
    }
}
//example place

//nesting content
//obj.key2 = "value2";
//obj.key1 = new JObject
//{
//    {"key3", "asdaddd" }
//};
//obj.key1.key3 = new JObject
//{
//    {"key4", "dddddd" }
//};
//result.Name = item.GetValue("imagepost");


//getting images
//var j = item.GetValue<Udi>("imagepost");
//var smt = Umbraco.GetIdForUdi(j);
//var umbHelper = new UmbracoHelper(UmbracoContext.Current);
//var k = umbHelper.Media(smt);
//result.Name = k.Url;

//looping children of sports node
//foreach (var item in content)
//{   
//    var j 
//obj.key = j;

//}

//get the players from player picker
//make the selected players neat, egzample "8676e6d1b9134bc3823ea5242f5d63e3"
//var j = content.GetValue<string>("playerpicker");

//string[] tokens = j.Split(',');
//var token = tokens[1].Replace("umb://document/","");


////get all players
//var temp = cs.GetChildren(1088);

//foreach (var item in temp)
//{
//    var id = item.Id;
//    var key = item.Key.ToString();
//    key = key.Replace("-", "");
//    result.Id = id;
//    result.key = key;
//}

//var hij = 0;           
//var root = token;
//var root2 = result.key;

//bool result1 = root.Equals(root2);
//int comparison = root.CompareTo(root2);
//result1 = root.Equals(root2, StringComparison.Ordinal);
//if (result1 == true)
//{
//    hij = result.Id;
//}

//var smt = cs.GetById(hij);