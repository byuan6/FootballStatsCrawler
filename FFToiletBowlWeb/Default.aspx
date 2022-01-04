<%@ Page Title="ToiletBowl" Language="C#" MasterPageFile="~/Site.master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="FFToiletBowlWeb._Default" %>

<asp:Content ID="HeaderContent" runat="server" ContentPlaceHolderID="HeadContent">
<style>
    #rosterSpace 
    {
        border:1px solid grey;
        border-radius:4px;
        
        background-color:#EEEEEE;
        padding:4px;
    }
    .rosterList 
    {
        border:1px solid grey;
        border-radius:4px;
        background-color:white;
        margin:4px;
        border:1px solid silver;
        padding:4px;
        min-width:75px;
        
        display:inline-block;
        vertical-align:top;
        text-align:left;
    }
    .rosterAbbr 
    {
        min-width:35px;
        background-color:#EEEEEE;
        display:inline-block;
    }
</style>
</asp:Content>

<asp:Content ID="BodyContent" runat="server" ContentPlaceHolderID="MainContent">
    <h2>
        Below are the stats and projections for 2001 to 2019.  No expert advice accepted on this website.  Only the computer makes recommendations based on the intelligence we build into it.
    </h2>
    <br />
    <div>
        <details>
        <summary>I am not a expert at football, nor finance, nor gambling, but in the land of the blind, the one eye-man is king</summary>
        <br />
        In my opinion<span style="color:silver">,(and I programmed applications for living for 19 years, have MBA from CUNY, 
        have Mechanical Engineering degree from SUNY, NONE at the top of my class, though but I have the degrees, 
        so you decide if I comprehend the subject matter better than you),</span> 
        all "expert" gambling websites you read, aren't really expert <i>advice</i> though they may be experts, 
        otherwise they'd be doing this for a living, and having you have same level of information they do, simply hinders that.
        In gambling, it is a zero-sum game.  This means for someone to win, someone else or ELSES must lose.
        Unlike cash for services transactions, which cash are meant to be stand-ins for material goods or services in exchange for something
        that somone else did at one point, and presumably can be repeated for same benefit or transferred.

        <p>Knowing that, there is no reason why you can't indulge a LITTLE, in believing you are a genius:  
        Can beat the house.
        Can outsmart the world in making a better HTML parser for browsers, better travelling salesman algorithm, or
        find a better way of measuring worth of a player in fantasyfootball.  
        As long as you understand it's just a fantasy for your entertainment, you shouldn't overdo it.</p>

        <p>
        All of that said, <b>I'm going to do what most gambling websites do</b>, 
        which is put a bunch of stats up, so you can be fooled into thinking the thing we all intuitively believe 
        but isnt true.  
        That past results is a indicator of future performance.  If that were true in a way that was dependable 
        (dependable like natural science modelled by math), so many people wouldn't be paid, so much money,
        try to get projections right, the weather right, and find "tells" when those predictions -based on past results- won't be right.
        Because quite frankly, most people want to believe themselves geniuses, and it entertains them to do so, and
        I <b>want to attracting readers</b>, and hopefully, I'll see something that gives me a better idea that I can
        do something with, that will allow people one day to laud me a (convenient) genius.  
        Not all of us are given a silver spoon of purpose.  It's a usually consolation gift given for lacking something else.
        </p>
        <p>
        And whenever you start to <b>wrongfully believe that past results is an indicator of future performance</b>, I'd just
        like to remind you of that quote from that movie about breaking the Nazi codes: The most difficult time to 
        lie to someone is when they are expecting to be lied to.  When you start believing the lie, is when everyone else
        around you is going to stop making it true.
        </p>
        </details>
    </div>
    <br />
    <br />
    
    <script>
        //https://www.w3schools.com/howto/tryit.asp?filename=tryhow_js_autocomplete
        function autocomplete(inp, arr) {
            /*the autocomplete function takes two arguments,
            the text field element and an array of possible autocompleted values:*/
            var currentFocus;
            inp.arr = arr;
            console.log(inp.arr);
            inp.addEventListener("input", function (e) {
                var a, b, i, val = this.innerText;
                closeAllLists();
                if (!val) { return false; }
                currentFocus = -1;
                a = document.createElement("DIV");
                a.setAttribute("id", this.id + "autocomplete-list");
                a.setAttribute("class", "autocomplete-items");
                this.parentNode.appendChild(a);
                a.style.left = this.offsetLeft + "px";
                a.style.top = (this.offsetTop + this.offsetHeight) + "px";

                for (i = 0; i < this.arr.length; i++) {
                    //var lead = this.arr[i].substr(0, val.length);
                    var orig = this.arr[i];
                    var found = orig.toUpperCase().indexOf(val.toUpperCase());
                    if (found >= 0) {
                        b = document.createElement("DIV");
                        //b.innerHTML = "<strong>" + lead + "</strong>";
                        //b.innerHTML += this.arr[i].substr(val.length);
                        
                        var text = orig.substr(0, found);
                        text += "<strong>";
                        text += orig.substr(found, val.length);
                        text += "</strong>";
                        text += orig.substr(found+val.length);
                        b.innerHTML = text;
                        b.innerHTML += "<input type='hidden' value='" + this.arr[i] + "'>";
                        b.addEventListener("click", function (e) {
                            inp.innerText = this.getElementsByTagName("input")[0].value;
                            closeAllLists();
                        });
                        a.appendChild(b);
                    }
                }
            });

            inp.addEventListener("keydown", function (e) {
                var x = document.getElementById(this.id + "autocomplete-list");
                if (x) x = x.getElementsByTagName("div");
                if (e.keyCode == 40) {
                    /*If the arrow DOWN key is pressed,
                    increase the currentFocus variable:*/
                    currentFocus++;
                    addActive(x);
                } else if (e.keyCode == 38) { //up
                    /*If the arrow UP key is pressed,
                    decrease the currentFocus variable:*/
                    currentFocus--;
                    addActive(x);
                } else if (e.keyCode == 13) {
                    /*If the ENTER key is pressed, prevent the form from being submitted,*/
                    e.preventDefault();
                    if (currentFocus > -1) {
                        if (x) x[currentFocus].click();
                    }
                }
            });
            function addActive(x) {
                /*a function to classify an item as "active":*/
                if (!x) return false;
                /*start by removing the "active" class on all items:*/
                removeActive(x);
                if (currentFocus >= x.length) currentFocus = 0;
                if (currentFocus < 0) currentFocus = (x.length - 1);
                /*add class "autocomplete-active":*/
                x[currentFocus].classList.add("autocomplete-active");
            }
            function removeActive(x) {
                /*a function to remove the "active" class from all autocomplete items:*/
                for (var i = 0; i < x.length; i++) {
                    x[i].classList.remove("autocomplete-active");
                }
            }
            function closeAllLists(elmnt) {
                /*close all autocomplete lists in the document,
                except the one passed as an argument:*/
                var x = document.getElementsByClassName("autocomplete-items");
                for (var i = 0; i < x.length; i++) {
                    if (elmnt != x[i] && elmnt != inp) {
                        x[i].parentNode.removeChild(x[i]);
                    }
                }
            }
            /*execute a function when someone clicks in the document:*/
            document.addEventListener("click", function (e) {
                closeAllLists(e.target);
            });
        }
    </script>

    
    
    <div>
        <div id="rosterSpace">
            <button style="float:right;background-color:lightblue;">Load roster, by pasting URL</button>
            Your league roster:<br />
            <div class="rosterList">
                Team: <div class="rosterAbbr" contenteditable="true"></div>
                <div contenteditable="false">asdasd<button>x</button></div>
                <div contenteditable="true" id="initRoster"></div>
                Player(x)<br />
                Player(x)<br />
                Player(x)<br />
                Player(x)<br />
                Player(x)<br />
                Player(x)<br />
            </div>
            <div class="rosterList">
                Team: <div class="rosterAbbr" contenteditable="true"></div>
                <div contenteditable="false"><div id="Div4" style="display:inline-block;width:75px">Weeks</div> <div style="width:30px; display:inline-block;text-align:center;font-weight:bold;" title="vs HOU">1</div><div style="width:30px; display:inline-block;text-align:center;font-weight:bold;" title="vs TB">2</div><div contenteditable="true" style="width:30px; display:inline-block;text-align:center;" title="vs NE">3</div><div style="width:30px; display:inline-block;text-align:center;">4</div><div style="width:30px; display:inline-block;text-align:center;">5</div><div style="width:30px; display:inline-block;text-align:center;">6</div><div style="width:30px; display:inline-block;text-align:center;">7</div><div style="width:30px; display:inline-block;text-align:center;">8</div><div style="width:30px; display:inline-block;text-align:center;">9</div><div style="width:30px; display:inline-block;text-align:center;">10</div><div style="width:30px; display:inline-block;text-align:center;">11</div><div style="width:30px; display:inline-block;text-align:center;">12</div><div style="width:30px; display:inline-block;text-align:center;">13</div><div style="width:30px; display:inline-block;text-align:center;">14</div><div style="width:30px; display:inline-block;text-align:center;">15</div><div style="width:30px; display:inline-block;text-align:center;">16</div><div style="width:30px; display:inline-block;text-align:center;">17</div><div style="width:30px; display:inline-block;text-align:center;">18</div>=<div style="width:30px; display:inline-block;text-align:center;">Total</div></div>
                <div contenteditable="false"><div contenteditable="true" id="Div3" style="display:inline-block">Player(x)</div><button>x</button> <div style="width:30px; display:inline-block;text-align:center;font-weight:bold;" title="vs HOU">28</div><div style="width:30px; display:inline-block;text-align:center;font-weight:bold;" title="vs TB">12</div><div contenteditable="false" style="width:30px; display:inline-block;text-align:center;" title="vs NE" ondblclick="var a=document.getElementById('dashboard'); console.log(a);a.insertBefore(document.createElement('div'),a.firstChild);a.firstChild.innerText='Show me 1.changes to players projected end+thru automated formula+thru manual changes 2.players total projected end-changes / games 3. new player coeffic*defense %+playeravg 5. show history of prediction for player and stdev';" onclick="this.contentEditable=true;this.focus();var r = document.createRange();r.selectNodeContents(this);" onblur="this.contentEditable=false;">21</div><div style="width:30px; display:inline-block;text-align:center;">21</div><div style="width:30px; display:inline-block;text-align:center;">21</div><div style="width:30px; display:inline-block;text-align:center;">21</div><div style="width:30px; display:inline-block;text-align:center;">21</div><div style="width:30px; display:inline-block;text-align:center;">21</div><div style="width:30px; display:inline-block;text-align:center;">21</div><div style="width:30px; display:inline-block;text-align:center;">21</div><div style="width:30px; display:inline-block;text-align:center;">21</div><div style="width:30px; display:inline-block;text-align:center;">21</div><div style="width:30px; display:inline-block;text-align:center;">21</div><div style="width:30px; display:inline-block;text-align:center;">21</div><div style="width:30px; display:inline-block;text-align:center;">21</div><div style="width:30px; display:inline-block;text-align:center;">21</div><div style="width:30px; display:inline-block;text-align:center;">21</div><div style="width:30px; display:inline-block;text-align:center;">21</div>=<div style="width:30px; display:inline-block;text-align:center;">300</div></div>
                <div contenteditable="false"><div contenteditable="true" id="Div2" style="display:inline-block">Player(x)</div><button>x</button></div>
                <div contenteditable="true" id="Div1">Player(x)</div>
            </div>
        </div>
        <button id="addRoster">+</button>

        <hr />
        <button>Load schedule, by pasting URL</button><br />
        Your schedule:<br/>
        <div>
            <div style="width:50px; display:inline-block">
            Week<br />
            Versus
            </div>
            <div style="width:30px; display:inline-block">
            1<br />
            HOU
            </div>
            <div style="width:30px; display:inline-block">
            2<br />
            TB
            </div>
            <div style="width:30px; display:inline-block">
            3<br />
            NE
            </div>
            <div style="width:30px; display:inline-block">
            4<br />
            WAS
            </div>
            <div style="width:30px; display:inline-block">
            5<br />
            WAS
            </div>
            <div style="width:30px; display:inline-block">
            6<br />
            WAS
            </div>
            <div style="width:30px; display:inline-block">
            7<br />
            WAS
            </div>
            <div style="width:30px; display:inline-block">
            8<br />
            WAS
            </div>
            <div style="width:30px; display:inline-block">
            9<br />
            WAS
            </div>
            <div style="width:30px; display:inline-block">
            10<br />
            WAS
            </div>
            <div style="width:30px; display:inline-block">
            11<br />
            WAS
            </div>
            <div style="width:30px; display:inline-block">
            12<br />
            WAS
            </div>
            <div style="width:30px; display:inline-block">
            13<br />
            WAS
            </div>
            <div style="width:30px; display:inline-block">
            14<br />
            WAS
            </div>
            <div style="width:30px; display:inline-block">
            15<br />
            WAS
            </div>
            <div style="width:30px; display:inline-block">
            16<br />
            WAS
            </div>
            <div style="width:30px; display:inline-block">
            17<br />
            WAS
            </div>
            <div style="width:30px; display:inline-block">
            18<br />
            WAS
            </div>

        </div>
        See Rest of schedule...
    </div>

    <div style="float:right; width:600px" id="dashboard">
        <div  style="width:500px;border:1px solid black;border-radius:5px; padding:10px; display:inline-block; margin:5px;" >
            Trade Analysis , Injury Discount Rate 
            <div>
                <div style="width:100px; display:inline-block">
                    Samyy Watkins
                </div>
                <div style="width:100px; display:inline-block">
                    <a name="sdf">8.5%</a>
                </div>
                <div style="width:20px; display:inline-block">
                    <a name="sdf" title="CAPM:Avg=5.8-1.5(1.2)">1</a>
                </div>
                <div style="width:20px; display:inline-block">
                    2
                </div>
                <div style="width:20px; display:inline-block">
                    3
                </div>
                <div style="width:20px; display:inline-block">
                    4
                </div>
                <div style="width:20px; display:inline-block">
                    5
                </div>
                <div style="width:20px; display:inline-block">
                    6
                </div>
                <div style="width:20px; display:inline-block">
                    7
                </div>
                <div style="width:20px; display:inline-block">
                    8
                </div>
                <div style="width:20px; display:inline-block">
                    9
                </div>
                <div style="width:20px; display:inline-block">
                    10
                </div>
                <div style="width:20px; display:inline-block">
                    11
                </div>
                <div style="width:20px; display:inline-block">
                    12
                </div>
                <div style="width:20px; display:inline-block">
                    13
                </div>
                <div style="width:20px; display:inline-block">
                    14
                </div>
                <div style="width:20px; display:inline-block">
                    15
                </div>
                <div style="width:20px; display:inline-block">
                    16
                </div>
                <div style="width:20px; display:inline-block">
                    17
                </div>
            </div>
            <div>
                <div style="width:100px; display:inline-block">
                    Sam Bradford
                </div>
                <div style="width:100px; display:inline-block">
                    8.5%
                </div>
                <div style="width:20px; display:inline-block">
                    1
                </div>
                <div style="width:20px; display:inline-block">
                    2
                </div>
                <div style="width:20px; display:inline-block">
                    3
                </div>
                <div style="width:20px; display:inline-block">
                    4
                </div>
                <div style="width:20px; display:inline-block">
                    5
                </div>
                <div style="width:20px; display:inline-block">
                    6
                </div>
                <div style="width:20px; display:inline-block">
                    7
                </div>
                <div style="width:20px; display:inline-block">
                    8
                </div>
                <div style="width:20px; display:inline-block">
                    9
                </div>
                <div style="width:20px; display:inline-block">
                    10
                </div>
                <div style="width:20px; display:inline-block">
                    11
                </div>
                <div style="width:20px; display:inline-block">
                    12
                </div>
                <div style="width:20px; display:inline-block">
                    13
                </div>
                <div style="width:20px; display:inline-block">
                    14
                </div>
                <div style="width:20px; display:inline-block">
                    15
                </div>
                <div style="width:20px; display:inline-block">
                    16
                </div>
                <div style="width:20px; display:inline-block">
                    17
                </div>
            </div>
        </div>
        <div  style="width:250px;border:1px solid black;border-radius:5px; padding:10px; display:inline-block; margin:5px;" >
            Scoring Distribution by position
            <br /><input type="checkbox" />Show projected
            <br /><input type="checkbox" />Include Non-roster (include cutoff for your league size)
            <br /><canvas></canvas>
            Bellcurve of roster-able player pt output
        </div>
        <div  style="width:250px;border:1px solid black;border-radius:5px; padding:10px; display:inline-block; margin:5px;" >
            Your Player Rank
            All | QB | RB | WR | TE | K |DST
            <div>
                <div style="width:100px; display:inline-block">
                    Player
                </div>
                <div style="width:30px; display:inline-block">
                    Last
                </div>
                <div style="width:30px; display:inline-block">
                    Next
                </div>
                <div style="width:30px; display:inline-block">
                    Proj
                </div>
                <div style="width:30px; display:inline-block">
                    MVP%
                </div>
            </div>
            <div>
                <div style="width:100px; display:inline-block">
                    1.Player
                </div>
                <div style="width:30px; display:inline-block">
                    10
                </div>
                <div style="width:30px; display:inline-block">
                    22
                </div>
                <div style="width:30px; display:inline-block">
                    230
                </div>
                <div style="width:30px; display:inline-block">
                    .6
                </div>
            </div>
        </div>
        <div  style="width:250px;border:1px solid black;border-radius:5px; padding:10px; display:inline-block; margin:5px;" >
            Reserves on other teams, better than your starters
            Players that should be reserves that are better than your starters
        </div>
        <div  style="width:250px;border:1px solid black;border-radius:5px; padding:10px; display:inline-block; margin:5px;" >
            Players that were not projected to be rosterable, that now are projected to be better than your reserves
        </div>
        <div  style="width:250px;border:1px solid black;border-radius:5px; padding:10px; display:inline-block; margin:5px;" >
            Projected S-draft and projected season standings
            <div>
                <div style="width:100px; display:inline-block">
                    Team
                </div>
                <div style="width:20px; display:inline-block">
                    W
                </div>
                <div style="width:20px; display:inline-block">
                    L
                </div>
                <div style="width:20px; display:inline-block">
                    For
                </div>
                <div style="width:20px; display:inline-block">
                    Vs
                </div>
            </div>
            <div>
                <div style="width:100px; display:inline-block">
                    1.Team
                </div>
                <div style="width:20px; display:inline-block">
                    10
                </div>
                <div style="width:20px; display:inline-block">
                    3
                </div>
                <div style="width:20px; display:inline-block">
                    200
                </div>
                <div style="width:20px; display:inline-block">
                    170
                </div>
            </div>
            <div>
                <div style="width:100px; display:inline-block">
                    2.Team
                </div>
                <div style="width:20px; display:inline-block">
                    9
                </div>
                <div style="width:20px; display:inline-block">
                    4
                </div>
                <div style="width:20px; display:inline-block">
                    200
                </div>
                <div style="width:20px; display:inline-block">
                    180
                </div>
            </div>
        </div>
        <div  style="width:500px;border:1px solid black;border-radius:5px; padding:10px; display:inline-block; margin:5px;" >
            2021 Player Projections
            <br /><input type="checkbox" />Update latest projection (less time, chances of hitting strike price)
            <br /><input type="checkbox" />Update with actual score
            <br /><input type="checkbox" />filter:only show your players
            <div>
                <div style="width:100px; display:inline-block">
                    Player
                </div>
                <div style="width:20px; display:inline-block">
                    1
                </div>
                <div style="width:20px; display:inline-block">
                    2
                </div>
                <div style="width:20px; display:inline-block">
                    3
                </div>
                <div style="width:20px; display:inline-block">
                    4
                </div>
                <div style="width:20px; display:inline-block">
                    5
                </div>
                <div style="width:20px; display:inline-block">
                    6
                </div>
                <div style="width:20px; display:inline-block">
                    7
                </div>
                <div style="width:20px; display:inline-block">
                    8
                </div>
                <div style="width:20px; display:inline-block">
                    9
                </div>
                <div style="width:20px; display:inline-block">
                    10
                </div>
                <div style="width:20px; display:inline-block">
                    11
                </div>
                <div style="width:20px; display:inline-block">
                    12
                </div>
                <div style="width:20px; display:inline-block">
                    13
                </div>
                <div style="width:20px; display:inline-block">
                    14
                </div>
                <div style="width:20px; display:inline-block">
                    15
                </div>
                <div style="width:20px; display:inline-block">
                    16
                </div>
                <div style="width:20px; display:inline-block">
                    17
                </div>
            </div>
            <div>
                <div style="width:100px; display:inline-block">
                    Rank 1.34, Implied 205pts
                </div>
                <div style="width:20px; display:inline-block">
                    1
                </div>
                <div style="width:20px; display:inline-block">
                    2
                </div>
                <div style="width:20px; display:inline-block">
                    3
                </div>
                <div style="width:20px; display:inline-block">
                    4
                </div>
                <div style="width:20px; display:inline-block">
                    5
                </div>
                <div style="width:20px; display:inline-block">
                    6
                </div>
                <div style="width:20px; display:inline-block">
                    7
                </div>
                <div style="width:20px; display:inline-block">
                    8
                </div>
                <div style="width:20px; display:inline-block">
                    9
                </div>
                <div style="width:20px; display:inline-block">
                    10
                </div>
                <div style="width:20px; display:inline-block">
                    11
                </div>
                <div style="width:20px; display:inline-block">
                    12
                </div>
                <div style="width:20px; display:inline-block">
                    13
                </div>
                <div style="width:20px; display:inline-block">
                    14
                </div>
                <div style="width:20px; display:inline-block">
                    15
                </div>
                <div style="width:20px; display:inline-block">
                    16
                </div>
                <div style="width:20px; display:inline-block">
                    17
                </div>
            </div>
        </div>
        <div  style="width:500px;border:1px solid black;border-radius:5px; padding:10px; display:inline-block; margin:5px;" >
            Schdule
            <div>
                <div style="width:50px; display:inline-block">
                Team<br />
                A<br />
                B<br />
                C<br />
                D<br />
                E<br />
                F<br />
                G<br />
                H<br />
                I<br />
                J<br />
                K<br />
                L<br />
                M<br />
                N<br />
                O<br />
                P<br />
                Q<br />
                </div>
                <div style="width:30px; display:inline-block">
                1<br />
                A<br />
                B<br />
                C<br />
                D<br />
                E<br />
                F<br />
                G<br />
                H<br />
                I<br />
                J<br />
                K<br />
                L<br />
                M<br />
                N<br />
                O<br />
                P<br />
                Q<br />
                </div>
                <div style="width:30px; display:inline-block">
                2<br />
                A<br />
                B<br />
                C<br />
                D<br />
                E<br />
                F<br />
                G<br />
                H<br />
                I<br />
                J<br />
                K<br />
                L<br />
                M<br />
                N<br />
                O<br />
                P<br />
                Q<br />
                </div>
                <div style="width:30px; display:inline-block">
                3<br />
                A<br />
                B<br />
                C<br />
                D<br />
                E<br />
                F<br />
                G<br />
                H<br />
                I<br />
                J<br />
                K<br />
                L<br />
                M<br />
                N<br />
                O<br />
                P<br />
                Q<br />
                </div>
                <div style="width:30px; display:inline-block">
                4<br />
                A<br />
                B<br />
                C<br />
                D<br />
                E<br />
                F<br />
                G<br />
                H<br />
                I<br />
                J<br />
                K<br />
                L<br />
                M<br />
                N<br />
                O<br />
                P<br />
                Q<br />
                </div>
            </div>
        </div>
        </div>
    </div>

    <asp:GridView ID="Seasons" DataKeyNames="Year" AutoGenerateColumns="false" ShowHeader="false" BorderStyle="None" OnRowDataBound="OnRowDataBound" runat="server">
    <Columns>
        <asp:TemplateField>
            <ItemTemplate>
                <h2><asp:Label ID="Year" Text='<% #Bind("Year") %>' runat="server"/> (<asp:Label ID="Label1" Text='<% #Bind("StartDate","{0:MMM/dd/yyyy}") %>' runat="server"/> to <asp:Label ID="Label2" Text='<% #Bind("EndDate","{0:MMM/dd/yyyy}") %>' runat="server"/>)</h2>
                <asp:GridView ID="Weeks" AutoGenerateColumns="false" ShowHeader="false" RowStyle-CssClass="WeekRow" runat="server">
                <Columns>
                    <asp:TemplateField>
                        <ItemTemplate>
                            <span class="WeekLabel">Week <asp:Label ID="Week" Text='<% #Bind("Week") %>' runat="server"/></span>
                            <asp:HyperLink ID="DefenseLink"     Text="Defense Ratings"  NavigateUrl='<% #Bind("YearWeek", "defenserating.aspx?yearweek={0}") %>' Visible='<% #Eval("IsDefenseEvaluationReady") %>' runat="server" />
                            <asp:HyperLink ID="RankingsLink"    Text="Rankings"         NavigateUrl='<% #Bind("YearWeek", "projectedrankings.aspx?yearweek={0}") %>' Visible='<% #Eval("IsEstimatesReady") %>' runat="server" />
                            <asp:HyperLink ID="VolatilityLink"  Text="Volatility Alert" NavigateUrl='<% #Bind("YearWeek", "volatilitycandidates.aspx?yearweek={0}") %>' Visible='<% #Eval("IsVolatilityAlertReady") %>' runat="server" />
                            <asp:HyperLink ID="TradeValue"      Text="Trade Value"      NavigateUrl='<% #Bind("YearWeek", "tradevaluation.aspx?yearweek={0}") %>' Visible='<% #Eval("IsVolatilityAlertReady") %>'  runat="server" />
                            <asp:HyperLink ID="Leaderboard"     Text="Week's Leaders"   NavigateUrl='<% #Bind("YearWeek", "leaderboard.aspx?yearweek={0}") %>' Visible='<% #Eval("IsLeadersReady") %>' runat="server" />
                            <asp:HyperLink ID="ResultsLink"     Text="Estimate Results" NavigateUrl='<% #Bind("YearWeek", "rankingresults.aspx?yearweek={0}") %>' Visible='<% #Eval("IsEstimateResultsReady") %>' runat="server" />
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
                </asp:GridView>

                <span title="Registering Hypothetical Rosters and schedules should give a assist here, but base functionality is to compare Mvp% and PV">Trade calculator</span>, Loaded 3/19/2018<br/>

                <asp:HyperLink ID="DraftSimulation"     Text="Draft Simulation"  NavigateUrl='<% #Bind("Year", "draftsimulationresult.aspx?year={0}") %>' Enabled='<% #Eval("IsDraftSimulationReady") %>' runat="server" />
                <br/>
                <span title="Point projections assume strength of schedule was already included, so 2x volatility added to projections">Computer Draft Rankings and Projections</span>, Loaded 3/19/2018<br/>
                Expert Rankings Published (FFToday), Loaded 3/19/2018<br/>
                Expert Rankings Published (Matthew Berry, ESPN), Loaded 3/19/2018<br/>
                <asp:HyperLink ID="SeasonSchedule"     Text="Schedule Pending"  NavigateUrl='<% #Bind("Year", "viewschedule.aspx?year={0}") %>' Enabled='<% #Eval("IsDraftSimulationReady") %>' ToolTip="As the season goes on, the schedule will update with strength of defense vs fantasy production of offensive players, as that calculation becomes clear" runat="server" />
                

                
            </ItemTemplate>
        </asp:TemplateField>
    </Columns>
    </asp:GridView>

    
    <script>
        function getXHR() {
            if (window.XMLHttpRequest) {
                return new XMLHttpRequest();
            } else {
                return new ActiveXObject('Microsoft.XMLHTTP');
            }
        }
        function ajaxPost(fm, callback, error) {
            var req = getXHR();
            if (!req) return false;

            req.onload = callback;
            req.onerror = error;

            var len = fm.length;
            var delimiter = "";
            var postdata = "";
            for (var i = 0; i < len; i++) {
                var field = fm.elements[i];
                if (field.tagName == "INPUT") {
                    var fieldType = field.type;
                    if (fieldType == "radio" || fieldType == "checkbox") {
                        if (field.checked)
                            postdata += delimiter + encodeURIComponent(field.name) + "=" + encodeURIComponent(field.value);
                    } else if (fieldType == "text" || fieldType == "hidden")
                        postdata += delimiter + encodeURIComponent(field.name) + "=" + encodeURIComponent(field.value);
                }
                else if (field.tagName == "TEXTAREA")
                    postdata += delimiter + encodeURIComponent(field.name) + "=" + encodeURIComponent(field.value);
                if (delimiter == "" && postdata != "")
                    delimiter = "&";
            }
            var url = fm.action;

            if ('withCredentials' in req)
                req.open('post', url, true);
            else {
                req = new XDomainRequest();
                req.open('post', url);
            }

            console.log("Sending " + postdata + " to " + url);
            req.setRequestHeader('Content-type', 'application/x-www-form-urlencoded'); //https://stackoverflow.com/questions/9713058/send-post-data-using-xmlhttprequest
            req.withCredentials = true;
            req.send(postdata);

            cancellableXHR = req;
            return true;
        }
        function getJson(jsonURL, stateForCallback, callback, errorback) {
            var req = getXHR();
            if (callback != null)
                req.onload = function () {
                    var json = this.response;
                    var deserialized = JSON.parse(json);
                    callback(deserialized, stateForCallback);
                };
            if (errorback != null)
                req.error = errorback
            //var url = "http://www.tictawf.net:80/CartographySalesman/Ajax/Heartbeat?timestamp=" + (new Date()).toDateString();
            var url = relativeUrl(jsonURL);
            req.open('get', url, true);
            req.send(null);
        }

        function relativeUrl(filename) {
            var me = location.href;
            var parts = me.split("/");
            parts[parts.length - 1] = "";
            var path = parts.join("/")
            return path + filename
        }
    </script>
    <script>
        var playerIndex = [];
        function loadPlayerIndex() {
            getJson("Json/PlayerIndex.json", null, function (json) {
                var players = json.map(s=>s.player + ", " + s.pos + ", " + s.team);
                playerIndex.push(...players);
                autocomplete(document.getElementById("initRoster"), playerIndex);
            }, function (json) {
                console.log("retry...");
                setTimeout(loadDestination, 1000); //retry every second until success
            });
        }
        loadPlayerIndex();

        document.getElementById("addRoster").addEventListener("click", function (e) {
            e.preventDefault();
            var roster = document.getElementById("rosterSpace").appendChild(document.createElement("div"));
            roster.className = "rosterList";

            roster.appendChild(document.createTextNode("Team:"));
            var abbr = roster.appendChild(document.createElement("div"));
            //abbr.innerText="text";
            abbr.contentEditable = true;
            abbr.className="rosterAbbr";
            abbr.addEventListener("keyup", function(e) { 
                if(this.innerText.length>3)
                    this.innerText.length=3;
            });
            var spot = roster.appendChild(document.createElement("div"));
            spot.contentEditable = true;
            autocomplete(spot, playerIndex);
        });
    </script>
</asp:Content>
