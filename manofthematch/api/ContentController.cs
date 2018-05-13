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
using Terratype.Models;

namespace manofthematch.Core.Controllers.WebAPI
{
    [Route("api/[controller]")]
    public class PostsApiController : UmbracoApiController
    {
        public string getpicture(IContent page, string property)
        {
            var umbHelper = new UmbracoHelper(UmbracoContext.Current);
            var j2 = page.GetValue<Udi>(property);
            var smt2 = Umbraco.GetIdForUdi(j2);
            var k2 = umbHelper.Media(smt2);
            return k2.Url;
        }
        [HttpGet] // /umbraco/api/PostsApi/getsingleclub?clubId=1085
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
            c.HomeCity = (club.GetValue("homeCity") != null) ? club.GetValue("homeCity").ToString() : "";
            if (teams != null)
            {
                foreach (var team in teams)
                {
                    var t = new Team();
                    t.teamId = team.Id;
                    t.teamName = (team.GetValue("nameTeam") != null) ? team.GetValue("nameTeam").ToString() : "";
                    t.highestT = GetHighestTrophies(team.Id);
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
                            p.Trophies = player.GetValue<int>("trophiesWon");
                            p.PlayerNumber = player.GetValue<int>("playerNumber");
                            p.PlayerPic = getpicture(player, "playerPicture");
                            playerList.Add(p);
                        }

                    }

                    if (matches != null)
                    {
                        foreach (var match in matches)
                        {
                            var m = new Match();
                            m.matchId = match.Id;
                            m.matchName = (match.GetValue("matchName") != null) ? match.GetValue("matchName").ToString() : "";
                            //Format DateTime
                            m.startDate = match.GetValue<DateTime>("startDate");
                            m.EndDate = match.GetValue<DateTime>("endDate");
                            m.opponent = match.GetValue<string>("opponentMatch");
                            //location part
                            var win = match.GetValue<int>("matchWinner");
                            if (win != 0)
                            {
                                m.winner = GetPlayer(win);
                            }
                                m.opponentLogo = getpicture(match, "opponentLogo");

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
                            if (token[0] != "")
                            {
                                foreach (var player in token)
                                {
                                    var playerExist = playerList.Find(x => x.playerGuid == player);

                                    var p = new Player();
                                    p.Trophies = playerExist.Trophies;
                                    p.playerId = playerExist.playerId;
                                    p.playerName = playerExist.playerName;
                                    p.PlayerNumber = playerExist.PlayerNumber;
                                    p.PlayerPic = playerExist.PlayerPic;
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
        
        [HttpGet]
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
        public List<allmatches> Getmatches([FromUri] int[] sportids) // /umbraco/api/PostsApi/Getmatches?sportids=1084&sportids=1093
        {
            var cs = Services.ContentService;
            List<allmatches> matchlist = new List<allmatches>();

            foreach (int sportid in sportids)
            {
                var content = cs.GetById(sportid).Children();
                foreach (var club1 in content)
                {

                    //var clubs = GetClub(club1.Id);
                    var club = GetSingleClub(club1.Id);

                    foreach (var item in club.teams)
                    {

                        foreach (var match in item.matches)
                        {

                            var matche = new allmatches();
                            matche.matchId = match.matchId;
                            matche.matchName = match.matchName;
                            matche.location = match.location;
                            matche.startDate = match.startDate;
                            matche.opponent = match.opponent;
                            matche.opponentLogo = match.opponentLogo;
                            matche.winner = match.winner;
                            matche.info = new List<allmatchesinfo>();
                            var info = new allmatchesinfo();
                            info.clubid = club.clubId;
                            info.clublogo = club.clubPic;
                            info.clubname = club.clubName;
                            info.teamId = item.teamId;
                            info.teamName = item.teamName;
                            info.players = match.players;
                            matche.info.Add(info);
                            matchlist.Add(matche);

                        }
                    }



                }

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

        public Player GetPlayer(int Playerid) // /umbraco/api/PostsApi/GetPlayer?PlayerId=1090
        {
            var play = new Player();
            var cs = Services.ContentService;
            var iplayer = cs.GetById(Playerid);
            play.playerName = (iplayer.GetValue<string>("namePlayer") != null) ? iplayer.GetValue("namePlayer").ToString() : "";
            play.playerId = iplayer.Id;
            play.Trophies = iplayer.GetValue<int>("trophiesWon");
            play.PlayerPic = getpicture(iplayer, "playerPicture");
            return play;
        }
        public Player GetHighestTrophies(int teamid) // /umbraco/api/PostsApi/GetHighestTrophies?teamid=1086
        {
            var cs = Services.ContentService;
            List<Player> playerList = new List<Player>();
            Player last = new Player();
            try
            {
                var players = cs.GetById(teamid).Descendants().Where(p => p.ContentType.Alias.Equals("playerItem"));

                if (players != null)
                {
                    foreach (var player in players)
                    {
                        var p = new Player();
                        p.playerId = player.Id;
                        p.playerName = (player.GetValue("namePlayer") != null) ? player.GetValue("namePlayer").ToString() : "";
                        p.Trophies = player.GetValue<int>("trophiesWon");
                        p.PlayerPic = getpicture(player, "playerPicture");

                        playerList.Add(p);
                    }
                    playerList = playerList.OrderBy(x => x.Trophies).ToList();
                    last = playerList.Last();
                }
            }
            catch
            {

            }
            //team id? club id?
            return last;
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

        [Umbraco.Web.WebApi.UmbracoAuthorize]
        [HttpPost] // /umbraco/api/PostsApi/Pickwinner?MatchId=1088        finds winner from match id, deletes scores, gives award to highest, assigns winner id to match
        public object PickWinner(int MatchId)
        {

            var cs = Services.ContentService;
            var content = cs.GetAncestors(MatchId);
            List<motmwinner> matchlist = new List<motmwinner>();
            var newlist = matchlist;
            motmwinner last = new motmwinner();
            foreach (var item in content)
            {
                if (item.ContentType.Alias == "club")
                {
                    var club = GetSingleClub(item.Id);
                    foreach (var item1 in club.teams)
                    {

                        foreach (var match in item1.matches)
                        {
                            if (match.matchId == MatchId)
                            {
                                if (match.winner == null)
                                {
                                    foreach (var player in match.players)
                                    {
                                        var e = new motmwinner();
                                        var iplayer = cs.GetById(player.playerId);
                                        e.scorenumb = iplayer.GetValue<int>("votingScore");
                                        e.Id = iplayer.Id;
                                        e.Name = iplayer.GetValue<string>("namePlayer");
                                        matchlist.Add(e);
                                        iplayer.SetValue("votingScore", 0);
                                        cs.SaveAndPublishWithStatus(iplayer);
                                    }
                                    newlist = matchlist.OrderBy(x => x.scorenumb).ToList();
                                    last = newlist.Last();
                                    var winner = cs.GetById(last.Id);
                                    var trophy = winner.GetValue<int>("trophiesWon");
                                    var matchwin = cs.GetById(MatchId);
                                    matchwin.SetValue("matchWinner", last.Id);
                                    winner.SetValue("trophiesWon", trophy = trophy + 1);
                                    cs.SaveAndPublishWithStatus(winner);
                                    cs.SaveAndPublishWithStatus(matchwin);
                                }
                                else
                                {
                                    last.Name = "already has a winner";
                                }
                            }
                        }
                    }
                }
            }

            return last;
        }

        //models for API reguests
        // - - - - - - - - - - - -

        public class SingleClub
        {
            public string clubName { set; get; }
            public int clubId { set; get; }
            public string HomeCity { set; get; }
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
            public Player highestT { set; get; }
            public List<Match> matches { set; get; }
            public List<Player> players { set; get; }
        }


        public class allmatches
        {
            public int matchId { set; get; }
            public string matchName { set; get; }
            public DateTime startDate { set; get; }
            public DateTime endDate { set; get; }
            public string[] location { set; get; }
            public string opponent { set; get; }
            public string opponentLogo { set; get; }
            public Player winner { set; get; }
            public List<allmatchesinfo> info { set; get; }
        }
        public class allmatchesinfo
        {
            public string teamName { set; get; }
            public int teamId { set; get; }
            public int clubid { set; get; }
            public string clublogo { set; get; }
            public string clubname { set; get; }
            public List<Player> players { set; get; }
        }
        public class Match
        {
            public int matchId { set; get; }
            public string matchName { set; get; }
            public DateTime startDate { set; get; }
            public DateTime EndDate { set; get; }
            public string[] location { set; get; }
            public string opponent { set; get; }
            public string opponentLogo { set; get; }
            public Player winner { set; get; }
            public List<Player> players { set; get; }
            //public List<string> players { set; get; }


        }

        public class Player
        {
            public string playerName { set; get; }
            public int playerId { set; get; }
            public string playerGuid { set; get; }
            public int PlayerNumber { set; get; }
            public string PlayerPic { set; get; }
            public int Trophies { set; get; }
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

        public class motmwinner
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public int scorenumb { get; set; }
        }
    }
}
