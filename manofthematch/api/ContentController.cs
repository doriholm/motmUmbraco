﻿using System;
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
using Terratype.Models;

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



                            if (item4.ContentType.Alias == "Matches")
                            {                 //if matches get selected players and compare
                                foreach (var item6 in matches)
                                {
                                    var playerpick = item6.GetValue<string>("playerPicker");
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
                                            obj["sport" + i]["clubs"]["club" + i2]["teams"]["team" + i3]["matches"]["match" + i4]["players"]["player" + i7] = new JObject
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
      public string getpicture(IContent page, string property)
        {
            var umbHelper = new UmbracoHelper(UmbracoContext.Current);
            var j2 = page.GetValue<Udi>(property);
            var smt2 = Umbraco.GetIdForUdi(j2);
            var k2 = umbHelper.Media(smt2);
            return k2.Url;
        }
        [HttpGet]
        public SingleClub GetSingleClub(int clubId)
        {
            var umbHelper = new UmbracoHelper(UmbracoContext.Current);
            var cs = Services.ContentService;
            var club = cs.GetById(clubId);
            var teams = cs.GetById(clubId).Children().Where(t => t.ContentType.Alias.Equals("clubItem"));
            List<Team> teamList = new List<Team>();             
           
            var c = new SingleClub();
            c.clubId = club.Id;
            c.clubName = (club.GetValue("clubName") != null) ? club.GetValue("clubName").ToString() : ""; 
            c.clubabout = (club.GetValue("aboutclub") != null) ? club.GetValue("aboutclub").ToString() : ""; 
            c.Sponsor = (club.GetValue("sponsorPick") != null) ? umbraco.library.GetPreValueAsString(int.Parse(club.GetValue("sponsorPick").ToString())) : "";
            c.clubPic = getpicture(club, "clublogo");
            c.stadiumPic = getpicture(club, "stadiumPic");
            if (teams != null)
            {
                foreach (var team in teams)
                {
                    var t = new Team();
                    t.teamId = team.Id;
                    t.teamName = (team.GetValue("nameTeam") != null)? team.GetValue("nameTeam").ToString(): "";
                   
                    List<Match> matchList = new List<Match>();
                    List<Player> playerList = new List<Player>();
                   

                    var matches = cs.GetById(team.Id).Descendants().Where(m => m.ContentType.Alias.Equals("matchItem"));
                    var players = cs.GetById(team.Id).Descendants().Where(p => p.ContentType.Alias.Equals("playerItem"));

                    if (players != null)
                    {
                        foreach (var player in players)
                        {
                            var p = new Player();
                            p.playerId = player.Id;
                            p.playerName = (player.GetValue("namePlayer") != null) ? player.GetValue("namePlayer").ToString() : "";
                            var playerGuid = player.Key.ToString();
                            p.playerGuid = playerGuid.Replace("-", string.Empty);
                            playerList.Add(p);
                        }

                    }

                    if (matches != null)
                    {
                        foreach (var match in matches)
                        {
                            var m = new Match();
                            m.matchId = match.Id;
                            m.matchName = (match.GetValue("matchName") != null) ? match.GetValue("matchName").ToString() : ""; ;
                            //Format DateTime
                            m.startDate = match.GetValue<DateTime>("startDate");
                            //location part
                           
                            var location = match.GetValue("locationPost").ToString();
                            string[] tok = location.Split(new string[] { "\"datum\": \"" }, StringSplitOptions.None);
                            string[] tok2 = tok[1].Split(new string[] { "\"\r\n  }\r\n" }, StringSplitOptions.None);
                            string[] gps = tok2[0].Split(',');
                            m.location = gps;
                            var playerpick = (match.GetValue<string>("playerPicker") != null) ? match.GetValue<string>("playerPicker").ToString() : "";
                            
                            string[] tokens = playerpick.Split(',');
                            List<string> token = new List<string> { };
                            foreach (var player in tokens)
                            {
                                var player2 = player.Replace("umb://document/", "");
                                token.Add(player2);
                            }
                         
                            List<Player> matchPlayersList = new List<Player>();
                            if (token[0] !=  "" )
                            {
                                foreach (var player in token)
                                {
                                    var playerExist = playerList.Find(x => x.playerGuid == player);

                                    var p = new Player();
                                    p.playerId = playerExist.playerId;
                                    p.playerName = playerExist.playerName;
                                    p.playerGuid = playerExist.playerGuid;
                                    matchPlayersList.Add(p);


                                }
                            }
                            m.players = matchPlayersList;
                            matchList.Add(m);
                        }

                    }


                    t.players = playerList;
                    t.matches = matchList;
                    teamList.Add(t);

                }
            }
            c.teams = teamList;
            return c;
        }
       

        //Only show clubs from specific sports
        //getClubs?sportsIds=1084&sportsIds=1093
        [HttpGet] // name clubhomecity picture, stadium picture id
        public List<Club> GetClubs([FromUri] int[] sportsIds)
        {
            var cs = Services.ContentService;
            List<Club> clubList = new List<Club>();

            foreach (int sportId in sportsIds)
            {
                var content = cs.GetById(sportId).Children();
                foreach (var club in content)
                {
                    var c = new Club();
                    c.clubName = (club.GetValue("clubName") != null) ? club.GetValue("clubName").ToString() : "";
                    c.Sponsor = (club.GetValue("sponsorPick") != null) ? umbraco.library.GetPreValueAsString(int.Parse(club.GetValue("sponsorPick").ToString())) : "";
                    c.clubPic = getpicture(club, "clublogo");
                    c.stadiumPic = getpicture(club, "stadiumPic");
                    c.clubId = club.Id;
                    c.Homecity = (club.GetValue("homeCity") != null) ? club.GetValue("homeCity").ToString() : "";
                    clubList.Add(c);
                }
            }
            return clubList;
        }
        [HttpGet] 
        public Club GetClub(int sportsId)
        {
            var cs = Services.ContentService;
            Club clubList = new Club();
                var content = cs.GetById(sportsId).Children();
                foreach (var club in content)
                {
                    var c = new Club();
                    clubList.clubName = (club.GetValue("clubName") != null) ? club.GetValue("clubName").ToString() : "";
                clubList.Sponsor = (club.GetValue("sponsorPick") != null) ? umbraco.library.GetPreValueAsString(int.Parse(club.GetValue("sponsorPick").ToString())) : "";
                clubList.clubPic = getpicture(club, "clublogo");
                clubList.stadiumPic = getpicture(club, "stadiumPic");
                clubList.clubId = club.Id;
                clubList.Homecity = (club.GetValue("homeCity") != null) ? club.GetValue("homeCity").ToString() : "";
                   
                }
            
            return clubList;
        }
        //  get all matches club id match id, club logo,club name, end date, start date, opponent, location
        [HttpGet] 
        public List<allmatches> Getmatches([FromUri] int[] sportids)
        {
            var cs = Services.ContentService;
            List<allmatches> matchlist = new List<allmatches>();

            foreach (int sportid in sportids)
            {
                var content = cs.GetById(sportid).Children();
                foreach (var club1 in content)
                {
                    var c = new allmatches();
                    //var clubs = GetClub(club1.Id);
                    var club = GetSingleClub(club1.Id);
                    c.clubid = club.clubId;
                    c.clublogo = club.clubPic;
                    c.clubname = club.clubName;
                    c.matches = new List<Match>();
                    foreach (var item in club.teams)
                    {

                        foreach (var match in item.matches)
                        {
                          
                                var matche = new Match();
                                matche.matchId = match.matchId;
                                matche.matchName = match.matchName;
                                matche.location = match.location;
                                matche.startDate = match.startDate;
                                matche.players = match.players;
                                c.matches.Add(matche);
                            
                        }
                    }


                    matchlist.Add(c);
                }
                //foreach (var club in content)
                //{
                //    var c = new Club();
                //    c.clubName = (club.GetValue("clubName") != null) ? club.GetValue("clubName").ToString() : "";
                //    c.Sponsor = (club.GetValue("sponsorPick") != null) ? umbraco.library.GetPreValueAsString(int.Parse(club.GetValue("sponsorPick").ToString())) : "";
                //    c.clubPic = getpicture(club, "clublogo");
                //    c.stadiumPic = getpicture(club, "stadiumPic");
                //    c.clubId = club.Id;
                //    c.Homecity = (club.GetValue("homeCity") != null) ? club.GetValue("homeCity").ToString() : "";
                //    clubList.Add(c);
                //}
            }
            return matchlist;
        }

        [HttpGet]
        public List<Sport> GetSports()
        {
            var cs = Services.ContentService;
            var content = cs.GetById(1082).Children();

            List<Sport> clubList = new List<Sport>();

            foreach (var sport in content)
            {
                var s = new Sport();
                s.sportId = sport.Id;
                s.sportName = (sport.Properties["namesport"].Value != null) ? sport.Properties["namesport"].Value.ToString() : "No name";
                clubList.Add(s);
            }

            return clubList;
        }

        //umbraco/oauth/token
        //Get token first then /umbraco/Api/postsapi/postmatch
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
        // /umbraco/Api/postsapi/playervote?playerid=1090
        [HttpGet]
        public object PlayerVote(int PlayerId)
        {
            var contentService = ApplicationContext.Current.Services.ContentService;
            var content = contentService.GetById(PlayerId);
            var item = content.GetValue<int>("votingScore");

            content.SetValue("votingScore", item = item + 1);

            var smth = contentService.SaveAndPublishWithStatus(content);
            return smth;
        }
        //reset the players  property to match motm 
        //end date for a match
        //models for API reguests
        // - - - - - - - - - - - -
        
        public class SingleClub
        {
            public string clubName { set; get; }
            public int clubId { set; get; }
            public string clubPic { set; get; }
            public string stadiumPic { set; get; }
            public object Sponsor { set; get; }
            public string clubabout { set; get; }
            public List<Team> teams { set; get; }
        }

        public class Team
        {
            public string teamName { set; get; }
            public int teamId { set; get; }
            public List<Match> matches { set; get; }
            public List<Player> players { set; get; }
        }


        public class allmatches
        {
            public int clubid { set; get; }
            public string clublogo { set; get; }
            public string clubname { set; get; }
            public List<Match> matches { set; get; }
            //public string opponent { set; get; }
           
            //public List<string> players { set; get; }


        }
        public class Match
        {
            public int matchId { set; get; }
            public string matchName { set; get; }
            public DateTime startDate { set; get; }
            public string[] location { set; get; }
            //public string opponent { set; get; }
            public List<Player> players { set; get; }
            //public List<string> players { set; get; }


        }

        public class Player
        {
            public string playerName { set; get; }
            public int playerId { set; get; }
            public string playerGuid { set; get; }
        }


        //Displaying all clubs
        public class Club
        {
        
            public string clubName { set; get; }
            public int clubId { set; get; }
            public string clubPic { set; get; }
            public string stadiumPic { set; get; }
            public string Sponsor { set; get; }
            public string Homecity { get; set; }
        }

        public class Sport
        {
            public string sportName { set; get; }
            public int sportId { get; set; }
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