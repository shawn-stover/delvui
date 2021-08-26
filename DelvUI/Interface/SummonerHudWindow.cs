using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using Dalamud.Data;
using Dalamud.Game.ClientState;
using Dalamud.Game.ClientState.JobGauge;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.Gui;
using Dalamud.Plugin;
using ImGuiNET;

namespace DelvUI.Interface
{
    public class SummonerHudWindow : HudWindow
    {
        public override uint JobId => 27;

        private int SmnRuinBarX => PluginConfiguration.SmnRuinBarX;
        private int SmnRuinBarY => PluginConfiguration.SmnRuinBarY;
        private int SmnRuinBarHeight => PluginConfiguration.SmnRuinBarHeight;
        private int SmnRuinBarWidth => PluginConfiguration.SmnRuinBarWidth;
        private int SmnDotBarX => PluginConfiguration.SmnDotBarX;
        private int SmnDotBarY => PluginConfiguration.SmnDotBarY;
        private int SmnDotBarHeight => PluginConfiguration.SmnDotBarHeight;
        private int SmnDotBarWidth => PluginConfiguration.SmnDotBarWidth;
        private int SmnAetherBarHeight => PluginConfiguration.SmnAetherBarHeight;
        private int SmnAetherBarWidth => PluginConfiguration.SmnAetherBarWidth;
        private int SmnAetherBarX => PluginConfiguration.SmnAetherBarX;
        private int SmnAetherBarY => PluginConfiguration.SmnAetherBarY;

        private Dictionary<string, uint> SmnAetherColor => PluginConfiguration.JobColorMap[Jobs.SMN * 1000];
        private Dictionary<string, uint> SmnRuinColor => PluginConfiguration.JobColorMap[Jobs.SMN * 1000 + 1];
        private Dictionary<string, uint> SmnEmptyColor => PluginConfiguration.JobColorMap[Jobs.SMN * 1000 + 2];
        private Dictionary<string, uint> SmnMiasmaColor => PluginConfiguration.JobColorMap[Jobs.SMN * 1000 + 3];
        private Dictionary<string, uint> SmnBioColor => PluginConfiguration.JobColorMap[Jobs.SMN * 1000 + 4];
        private Dictionary<string, uint> SmnExpiryColor => PluginConfiguration.JobColorMap[Jobs.SMN * 1000 + 5];


        public SummonerHudWindow(
            ClientState clientState,
            DalamudPluginInterface pluginInterface,
            DataManager dataManager,
            GameGui gameGui,
            JobGauges jobGauges,
            ObjectTable objectTable, 
            PluginConfiguration pluginConfiguration,
            TargetManager targetManager
        ) : base(
            clientState,
            pluginInterface,
            dataManager,
            gameGui,
            jobGauges,
            objectTable,
            pluginConfiguration,
            targetManager
        ) { }

        protected override void Draw(bool _)
        {
            DrawHealthBar();
            DrawRuinBar();
            DrawActiveDots();
            DrawAetherBar();
            DrawTargetBar();
            DrawFocusBar();
            DrawCastBar();
        }

        private void DrawActiveDots()
        {
            var target = TargetManager.SoftTarget ?? TargetManager.Target;

            if (target is not BattleChara actor)
            {
                return;
            }

            var xPadding = 2;

            var barWidth = (SmnDotBarWidth / 2) - 1;    
            var miasma = actor.StatusList.FirstOrDefault(o => o.StatusId is 1215 or 180);
            var bio = actor.StatusList.FirstOrDefault(o => o.StatusId is 1214 or 179 or 189);

            var miasmaDuration = miasma == null ? 0f : miasma.RemainingTime;
            var bioDuration = bio == null ? 0f : bio.RemainingTime;

            var miasmaColor = miasmaDuration > 5 ? SmnMiasmaColor["base"] : SmnExpiryColor["base"];
            var bioColor = bioDuration > 5 ? SmnBioColor["base"] : SmnExpiryColor["base"];

            var xOffset = CenterX - SmnDotBarX;
            var cursorPos = new Vector2(CenterX - SmnDotBarX, CenterY + SmnDotBarY - 46);
            var barSize = new Vector2(barWidth, SmnDotBarHeight);
            var drawList = ImGui.GetWindowDrawList();

            var dotStart = new Vector2(xOffset + barWidth - (barSize.X / 30) * miasmaDuration, CenterY + SmnDotBarY - 46);

            drawList.AddRectFilled(cursorPos, cursorPos + barSize, SmnEmptyColor["base"]);
            drawList.AddRectFilled(dotStart, cursorPos + new Vector2(barSize.X, barSize.Y), miasmaColor);
            drawList.AddRect(cursorPos, cursorPos + barSize, 0xFF000000);

            cursorPos = new Vector2(cursorPos.X + barWidth + xPadding, cursorPos.Y);

            drawList.AddRectFilled(cursorPos, cursorPos + barSize, SmnEmptyColor["base"]);
            drawList.AddRectFilled(cursorPos, cursorPos + new Vector2((barSize.X / 30) * bioDuration, barSize.Y), bioColor);
            drawList.AddRect(cursorPos, cursorPos + barSize, 0xFF000000);
        }
        private void DrawAetherBar()
        {
            Debug.Assert(ClientState.LocalPlayer != null, "ClientState.LocalPlayer != null");
            var aetherFlowBuff = ClientState.LocalPlayer.StatusList.FirstOrDefault(o => o.StatusId == 304);
            var xPadding = 2;
            var barWidth = (SmnAetherBarWidth / 2) - 1;
            var cursorPos = new Vector2(CenterX - 127, CenterY + SmnAetherBarY - 22);
            var barSize = new Vector2(barWidth, SmnAetherBarHeight);

            var drawList = ImGui.GetWindowDrawList();

            drawList.AddRectFilled(cursorPos, cursorPos + barSize, SmnEmptyColor["base"]);
            drawList.AddRect(cursorPos, cursorPos + barSize, 0xFF000000);
            cursorPos = new Vector2(cursorPos.X + barWidth + xPadding, cursorPos.Y);
            drawList.AddRectFilled(cursorPos, cursorPos + barSize, SmnEmptyColor["base"]);
            drawList.AddRect(cursorPos, cursorPos + barSize, 0xFF000000);
            cursorPos = new Vector2(CenterX - 127, CenterY + SmnAetherBarY - 22);

            var stackCount = aetherFlowBuff == null ? 0 : aetherFlowBuff.StackCount;
            switch (stackCount)
            {
                case 1:
                    drawList.AddRectFilled(cursorPos, cursorPos + barSize, SmnAetherColor["base"]);
                    drawList.AddRect(cursorPos, cursorPos + barSize, 0xFF000000);

                    break;
                case 2:
                    drawList.AddRectFilled(cursorPos, cursorPos + barSize, SmnAetherColor["base"]);
                    drawList.AddRect(cursorPos, cursorPos + barSize, 0xFF000000);
                    cursorPos = new Vector2(cursorPos.X + barWidth + xPadding, cursorPos.Y);
                    drawList.AddRectFilled(cursorPos, cursorPos + barSize, SmnAetherColor["base"]);
                    drawList.AddRect(cursorPos, cursorPos + barSize, 0xFF000000);
                    break;

            }

        }
        private void DrawRuinBar()
        {
            Debug.Assert(ClientState.LocalPlayer != null, "ClientState.LocalPlayer != null");
            var ruinBuff = ClientState.LocalPlayer.StatusList.FirstOrDefault(o => o.StatusId == 1212);
            var ruinStacks = ruinBuff == null ? 0 : ruinBuff.StackCount;

            const int xPadding = 2;
            var barWidth = (SmnRuinBarWidth - xPadding * 3) / 4;
            var barSize = new Vector2(barWidth, SmnRuinBarHeight);
            var xPos = CenterX - SmnRuinBarX;
            var yPos = CenterY + SmnRuinBarY - 34;
            var cursorPos = new Vector2(xPos, yPos);
            var barColor = SmnRuinColor["base"];
            var emptyColor = SmnEmptyColor["base"];

            var drawList = ImGui.GetWindowDrawList();
            for (var i = 0; i <= 4 - 1; i++)
            {
                drawList.AddRectFilled(cursorPos, cursorPos + barSize, emptyColor);
                if (ruinStacks > i)
                {
                    drawList.AddRectFilled(cursorPos, cursorPos + new Vector2(barSize.X, barSize.Y), barColor);
                }
                else
                {

                }
                drawList.AddRect(cursorPos, cursorPos + barSize, 0xFF000000);
                cursorPos = new Vector2(cursorPos.X + barWidth + xPadding, cursorPos.Y);
            }
        }
    }
}