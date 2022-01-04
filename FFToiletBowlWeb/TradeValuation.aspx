<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="TradeValuation.aspx.cs" Inherits="FFToiletBowlWeb.TradeValuation" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
 <style>
        .altColorColumn
        {
            background-color:#f7f7f7;
        }
        td.lessPaddedColumn
        {
            padding-left:10px;
            padding-right:10px;
        }
        td.rightPadColumn
        {
            padding-left:4px;
            padding-right:10px;
        }
        td.leftPadColumn
        {
            padding-left:10px;
            padding-right:0px;
        }
        
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="captionBlock grow" style="display:inline-block; width:49%;vertical-align:top;">
        <h1>
            <asp:Label ID="Year" runat="server"/> Season<br/>
            Before Week <asp:Label ID="Week" runat="server"/>
        </h1>
        <h2>Trade Value</h2>
        <h3><asp:Literal ID="ModellingMethod" runat="server"></asp:Literal></h3>
    </div>


    <!--  -->
    <div class="graphPane">
        <asp:Image ID="PtsVsMvpPerPosGraph" runat="server" />
    </div>

    <div style="padding:20px;">
    Can display [projected points to be scored in future], or [Adjust future points scored in future, by their injury risk], or [Current points already scored].
    You can see the rank of each player, sorted by the metric of your choice.  
    The MVP% is experimental.  It is a factor representing the value of this player to a starting lineup, in a 16team league, with specific lineup configuration.  Theoretically, if you plan to replace a 0.560 player in the starting lineup with a .540 player you are going to trade for, with 10 games left in season, in a league where you start 9players you will lose on average 10games*0.20/9players a year.  The MVP% is supposed to produce a more apples to apples comparison between players playing in different positions.
    </div>

    <h2>Sortable Value Grid</h2>
    <span>Estimated Remaining Points | Adjusted For Injury | Points already scored</span>
    <span>Past MVP% | Rest of the Way Modeled MVP%</span>
    <div style="padding:0px;">
        <asp:GridView ID="RelativeValueGrid" DataKeyNames="PlayerID" AutoGenerateColumns="false" ShowHeader="true" CssClass="sortable" BorderStyle="None" runat="server">
        <Columns>
            <asp:BoundField HeaderText="#" DataField="OverallRank" ItemStyle-CssClass="lessPaddedColumn" />
            <asp:BoundField HeaderText="PosRk" DataField="PosRank2" ItemStyle-CssClass="lessPaddedColumn altColorColumn" />
            <asp:BoundField HeaderText="Flex" DataField="FlexRank" ItemStyle-CssClass="lessPaddedColumn" />
            <asp:BoundField HeaderText="Mvp%" DataField="MvpPct" DataFormatString="{0:0.0000}" ItemStyle-CssClass="lessPaddedColumn altColorColumn" />

            
            <asp:HyperLinkField HeaderText="QB" DataNavigateUrlFields="Player" DataNavigateUrlFormatString="/MemberPages/profile.aspx?ID={0}" DataTextField="QB"                                ItemStyle-HorizontalAlign="Left" ItemStyle-CssClass="leftPadColumn" />
            <asp:HyperLinkField HeaderText="Pts" DataNavigateUrlFields="Player" DataNavigateUrlFormatString="/MemberPages/profile.aspx?ID={0}" DataTextField="QBPts" DataTextFormatString="{0:0}"  ItemStyle-HorizontalAlign="Right" ItemStyle-CssClass="rightPadColumn" />
            
            <asp:HyperLinkField HeaderText="RB" DataNavigateUrlFields="Player" DataNavigateUrlFormatString="/MemberPages/profile.aspx?ID={0}" DataTextField="RB"                               ItemStyle-HorizontalAlign="Left" ItemStyle-CssClass="leftPadColumn altColorColumn"/>
            <asp:HyperLinkField HeaderText="Pts" DataNavigateUrlFields="Player" DataNavigateUrlFormatString="/MemberPages/profile.aspx?ID={0}" DataTextField="RBPts" DataTextFormatString="{0:0}" ItemStyle-HorizontalAlign="Right" ItemStyle-CssClass="rightPadColumn altColorColumn" />

            <asp:HyperLinkField HeaderText="WR" DataNavigateUrlFields="Player" DataNavigateUrlFormatString="/MemberPages/profile.aspx?ID={0}" DataTextField="WR"                               ItemStyle-HorizontalAlign="Left" ItemStyle-CssClass="leftPadColumn" />
            <asp:HyperLinkField HeaderText="Pts" DataNavigateUrlFields="Player" DataNavigateUrlFormatString="/MemberPages/profile.aspx?ID={0}" DataTextField="WRPts" DataTextFormatString="{0:0}" ItemStyle-HorizontalAlign="Right" ItemStyle-CssClass="rightPadColumn" />

            <asp:HyperLinkField HeaderText="TE" DataNavigateUrlFields="Player" DataNavigateUrlFormatString="/MemberPages/profile.aspx?ID={0}" DataTextField="TE"                                ItemStyle-HorizontalAlign="Left" ItemStyle-CssClass="leftPadColumn altColorColumn"/>
            <asp:HyperLinkField HeaderText="Pts" DataNavigateUrlFields="Player" DataNavigateUrlFormatString="/MemberPages/profile.aspx?ID={0}" DataTextField="TEPts" DataTextFormatString="{0:0}" ItemStyle-HorizontalAlign="Right" ItemStyle-CssClass="rightPadColumn altColorColumn"/>

            <asp:HyperLinkField HeaderText="K"  DataNavigateUrlFields="Player" DataNavigateUrlFormatString="/MemberPages/profile.aspx?ID={0}" DataTextField="K"                                ItemStyle-HorizontalAlign="Left" ItemStyle-CssClass="leftPadColumn" />
            <asp:HyperLinkField HeaderText="Pts"  DataNavigateUrlFields="Player" DataNavigateUrlFormatString="/MemberPages/profile.aspx?ID={0}" DataTextField="KPts" DataTextFormatString="{0:0}"   ItemStyle-HorizontalAlign="Right" ItemStyle-CssClass="rightPadColumn" />

            <asp:HyperLinkField HeaderText="DST" DataNavigateUrlFields="Player" DataNavigateUrlFormatString="/MemberPages/profile.aspx?ID={0}" DataTextField="DST"                              ItemStyle-HorizontalAlign="Left" ItemStyle-CssClass="leftPadColumn altColorColumn"/>
            <asp:HyperLinkField HeaderText="Pts" DataNavigateUrlFields="Player" DataNavigateUrlFormatString="/MemberPages/profile.aspx?ID={0}" DataTextField="DSTPts" DataTextFormatString="{0:0}"  ItemStyle-HorizontalAlign="Right" ItemStyle-CssClass="rightPadColumn altColorColumn"/>

        </Columns>
        </asp:GridView>
    </div>

</asp:Content>
