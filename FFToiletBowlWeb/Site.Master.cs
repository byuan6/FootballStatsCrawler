using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace FFToiletBowlWeb
{
    public partial class SiteMaster : System.Web.UI.MasterPage
    {
        string[] _motd = new string[] {
            "The <b>Toilet Bowl</b> is when 2 x 0-12 teams, meet in week 13 of Fantasy Football Season.  It's like an eclipse.  It's special to have 2 teams that bad.",
            "<b>I am not a expert</b> at football, nor finance, nor gambling, but in the land of the blind, the one eye-man is king.",
            "I think the best definition for regressive gambling is a zero-sum game where for someone to win, <b>someone who actually knows what he's doing telling people what to do must lose</b>.",
            "I believe all games with an intelligent opponent, have <b>an unavoidable degree of uncertainty, usually at the worst possible moment</b>.",
            "I think that any belief of past results, indicating <b>future performance, needs to consider the increased risk added based on how much everyone else hates you</b>.",
            "I believe <b>when the Turing test is inappropriately applied to humans</b>, it takes a life of its own, whose outcome is to determine who is the intellectual loser between the tested and the tester.",
            "I believe it's ok for everyone to believe themselves a genius at any point in time, but just don't overdo it.",
            "My best year at fantasy football, <b>MY OWN ALGORITHM told me I was two wins better than I should have been, based on blind luck</b>.",
            "I feel it is always better to remind yourself, that the smarter you are, the more outnumbered you will be.",
            "I think it is just as important to for the intelligent to be heard, as it is wise to know when you're going to get imminently punched in the face.",
            "I'm not discouraging gambling, I'm avoiding a lawsuit that I'm liable for other people's human weaknesses",
            "Instinct is another word for mother nature compensating for lack of intelligence, bc intelligence is just too cumbersome to be effectively inherited.",
            "Intelligence is often understanding how you were the recipient of blind luck, which ironically makes you feel stupid.",
            "Social scientists have to be wiser than natural scienctists.  Nature doesn't fight back, when it learns the results.",
        };

        protected void Page_Load(object sender, EventArgs e)
        {
            this.Motd = _motd[(new Random(Environment.TickCount)).Next(_motd.Length)];
        }

        protected string Motd { get; set; }
    }
}
