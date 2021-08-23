using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Numerics;
using Dalamud.Data;
using Dalamud.Game.ClientState;
using Dalamud.Game.ClientState.JobGauge;
using Dalamud.Game.ClientState.JobGauge.Types;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Game.Gui;
using Dalamud.Plugin;
using ImGuiNET;

namespace DelvUI.Interface
{
    public class PaladinHudWindow : HudWindow
    {
        public override uint JobId => 19;

        private int ManaBarHeight => PluginConfiguration.PLDManaHeight;
        
        private int ManaBarWidth => PluginConfiguration.PLDManaWidth;
        
        private int ManaBarPadding => PluginConfiguration.PLDManaPadding;
        
        private new int XOffset => PluginConfiguration.PLDBaseXOffset;
        
        private new int YOffset => PluginConfiguration.PLDBaseYOffset;
        
        private int OathGaugeBarHeight => PluginConfiguration.PLDOathGaugeHeight;
        
        private int OathGaugeBarWidth => PluginConfiguration.PLDOathGaugeWidth;
        
        private int OathGaugeBarPadding => PluginConfiguration.PLDOathGaugePadding;
        
        private int OathGaugeXOffset => PluginConfiguration.PLDOathGaugeXOffset;
        
        private int OathGaugeYOffset => PluginConfiguration.PLDOathGaugeYOffset;
        
        private bool OathGaugeText => PluginConfiguration.PLDOathGaugeText;
        
        private int BuffBarHeight => PluginConfiguration.PLDBuffBarHeight;
        
        private int BuffBarWidth => PluginConfiguration.PLDBuffBarWidth;
        
        private int BuffBarXOffset => PluginConfiguration.PLDBuffBarXOffset;
        
        private int BuffBarYOffset => PluginConfiguration.PLDBuffBarYOffset;
        
        private int AtonementBarHeight => PluginConfiguration.PLDAtonementBarHeight;
        
        private int AtonementBarWidth => PluginConfiguration.PLDAtonementBarWidth;
        
        private int AtonementBarPadding => PluginConfiguration.PLDAtonementBarPadding;
        
        private int AtonementBarXOffset => PluginConfiguration.PLDAtonementBarXOffset;
        
        private int AtonementBarYOffset => PluginConfiguration.PLDAtonementBarYOffset;
        
        private int InterBarOffset => PluginConfiguration.PLDInterBarOffset;
        
        private Dictionary<string, uint> ManaColor => PluginConfiguration.JobColorMap[Jobs.PLD * 1000];
        
        private Dictionary<string, uint> OathGaugeColor => PluginConfiguration.JobColorMap[Jobs.PLD * 1000 + 1];
        
        private Dictionary<string, uint> FightOrFlightColor => PluginConfiguration.JobColorMap[Jobs.PLD * 1000 + 2];
        
        private Dictionary<string, uint> RequiescatColor => PluginConfiguration.JobColorMap[Jobs.PLD * 1000 + 3];
        
        private Dictionary<string, uint> EmptyColor => PluginConfiguration.JobColorMap[Jobs.PLD * 1000 + 4];
        
        private Dictionary<string, uint> AtonementColor => PluginConfiguration.JobColorMap[Jobs.PLD * 1000 + 5];
        
        public PaladinHudWindow(
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
            var nextHeight = DrawManaBar(0);
            nextHeight = DrawOathGauge(nextHeight);
            nextHeight = DrawBuffBar(nextHeight);
            DrawAtonementBar(nextHeight);
            DrawTargetBar();
            DrawFocusBar();
            DrawCastBar();
        }

        private int DrawManaBar(int initialHeight)
        {
            Debug.Assert(ClientState.LocalPlayer != null, "ClientState.LocalPlayer != null");
            var actor = ClientState.LocalPlayer;

            var barWidth = (ManaBarWidth - ManaBarPadding * 4f) / 5f;
            var posX = CenterX - XOffset;
            var posY = CenterY + YOffset;
            var cursorPos = new Vector2(posX + barWidth * 4 + ManaBarPadding * 4, posY);
            const int chunkSize = 2000;
            var barSize = new Vector2(barWidth, ManaBarHeight);

            var drawList = ImGui.GetWindowDrawList();

            for (var i = 5; i >= 1; --i)
            {
                var scale = Math.Max(Math.Min(actor.CurrentMp, chunkSize * i) - chunkSize * (i - 1), 0f) / chunkSize;
                drawList.AddRectFilled(cursorPos, cursorPos + barSize, 0x88000000);
                if (scale >= 1.0)
                    drawList.AddRectFilledMultiColor(cursorPos,
                        cursorPos + new Vector2(barWidth * scale, ManaBarHeight),
                        ManaColor["gradientLeft"], ManaColor["gradientRight"], ManaColor["gradientRight"], ManaColor["gradientLeft"]
                    );
                else
                    drawList.AddRectFilledMultiColor(
                        cursorPos, cursorPos + new Vector2(barWidth * scale, ManaBarHeight),
                        EmptyColor["gradientLeft"], EmptyColor["gradientRight"], EmptyColor["gradientRight"], EmptyColor["gradientLeft"]
                    );
                drawList.AddRect(cursorPos, cursorPos + barSize, 0xFF000000);
                cursorPos = new Vector2(cursorPos.X - barWidth - ManaBarPadding, cursorPos.Y);
            }

            return ManaBarHeight + initialHeight + InterBarOffset;
        }

        private int DrawOathGauge(int initialHeight)
        {
            var gauge = JobGauges.Get<PLDGauge>();

            var barWidth = (OathGaugeBarWidth - OathGaugeBarPadding) / 2f;
            var xPos = CenterX - XOffset + OathGaugeXOffset;
            var yPos = CenterY + YOffset + initialHeight + OathGaugeYOffset;
            var cursorPos = new Vector2(xPos + barWidth + OathGaugeBarPadding, yPos);
            const int chunkSize = 50;
            var barSize = new Vector2(barWidth, OathGaugeBarHeight);

            var drawList = ImGui.GetWindowDrawList();
            for (var i = 2; i >= 1; --i)
            {
                var scale = Math.Max(Math.Min(gauge.OathGauge, chunkSize * i) - chunkSize * (i - 1), 0f) / chunkSize;
                drawList.AddRectFilled(cursorPos, cursorPos + barSize, 0x88000000);
                if (scale >= 1.0)
                {
                    drawList.AddRectFilledMultiColor(
                        cursorPos, cursorPos + new Vector2(barWidth * scale, OathGaugeBarHeight),
                        OathGaugeColor["gradientLeft"], OathGaugeColor["gradientRight"], OathGaugeColor["gradientRight"], OathGaugeColor["gradientLeft"]
                    );
                    
                    if (OathGaugeText)
                    {
                        var text = (scale * chunkSize).ToString();
                        var textSize = ImGui.CalcTextSize(text);
                        DrawOutlinedText(text, new Vector2(cursorPos.X + barWidth / 2f - textSize.X / 2f, cursorPos.Y-2));                        
                    }
                }
                else
                {
                    drawList.AddRectFilledMultiColor(
                        cursorPos, cursorPos + new Vector2(barWidth * scale, OathGaugeBarHeight),
                        EmptyColor["gradientLeft"], EmptyColor["gradientRight"], EmptyColor["gradientRight"], EmptyColor["gradientLeft"]
                    );
                    
                    if (OathGaugeText)
                    {
                        var text = (scale * chunkSize).ToString();
                        var textSize = ImGui.CalcTextSize(text);
                        DrawOutlinedText(text, new Vector2(cursorPos.X + barWidth / 2f - textSize.X / 2f, cursorPos.Y-2));                        
                    }
                }
                drawList.AddRect(cursorPos, cursorPos + barSize, 0xFF000000);

                cursorPos = new Vector2(cursorPos.X - barWidth - OathGaugeBarPadding, cursorPos.Y);
            }

            return OathGaugeBarHeight + initialHeight + InterBarOffset;
        }

        private int DrawBuffBar(int initialHeight)
        {
            Debug.Assert(ClientState.LocalPlayer != null, "ClientState.LocalPlayer != null");
            var fightOrFlightBuff = ClientState.LocalPlayer.StatusList.Where(o => o.StatusId == 76);
            var requiescatBuff = ClientState.LocalPlayer.StatusList.Where(o => o.StatusId == 1368);
            
            var buffBarBarWidth = BuffBarWidth;
            var xPos = CenterX - XOffset + BuffBarXOffset;
            var yPos = CenterY + YOffset + initialHeight + BuffBarYOffset;
            var cursorPos = new Vector2(xPos, yPos);
            var buffBarBarHeight = BuffBarHeight;
            var barSize = new Vector2(buffBarBarWidth, buffBarBarHeight);
            
            var drawList = ImGui.GetWindowDrawList();
            
            drawList.AddRectFilled(cursorPos, cursorPos + barSize, 0x88000000);
            if (fightOrFlightBuff.Any() && requiescatBuff.Any())
            {
                var innerBarHeight = buffBarBarHeight / 2;
                barSize = new Vector2(buffBarBarWidth, innerBarHeight);
                
                var fightOrFlightDuration = Math.Abs(fightOrFlightBuff.First().RemainingTime);
                var requiescatDuration = Math.Abs(requiescatBuff.First().RemainingTime);
                
                drawList.AddRectFilledMultiColor(
                    cursorPos, cursorPos + new Vector2(barSize.X / 25f * fightOrFlightDuration, barSize.Y), 
                    FightOrFlightColor["gradientLeft"], FightOrFlightColor["gradientRight"], FightOrFlightColor["gradientRight"], FightOrFlightColor["gradientLeft"]
                );
                drawList.AddRectFilledMultiColor(
                    cursorPos + new Vector2(0.0f, innerBarHeight), cursorPos + new Vector2(barSize.X / 12f * requiescatDuration, barSize.Y * 2f),
                    RequiescatColor["gradientLeft"], RequiescatColor["gradientRight"], RequiescatColor["gradientRight"], RequiescatColor["gradientLeft"]
                );
                
                var fightOrFlightDurationText = fightOrFlightDuration == 0 ? "" : Math.Round(fightOrFlightDuration).ToString(CultureInfo.InvariantCulture);
                DrawOutlinedText(fightOrFlightDurationText, new Vector2(cursorPos.X + 5f, cursorPos.Y - 2f), PluginConfiguration.PLDFightOrFlightColor, new Vector4(0f, 0f, 0f, 1f));
                
                var requiescatDurationText = requiescatDuration == 0 ? "" : Math.Round(requiescatDuration).ToString(CultureInfo.InvariantCulture);
                DrawOutlinedText(requiescatDurationText, new Vector2(cursorPos.X + 27f, cursorPos.Y - 2f), PluginConfiguration.PLDRequiescatColor, new Vector4(0f, 0f, 0f, 1f));
                
                barSize = new Vector2(buffBarBarWidth, buffBarBarHeight);
            }
            else if (fightOrFlightBuff.Any())
            {
                var fightOrFlightDuration = Math.Abs(fightOrFlightBuff.First().RemainingTime);
                drawList.AddRectFilledMultiColor(
                    cursorPos, cursorPos + new Vector2(barSize.X / 25f * fightOrFlightDuration, barSize.Y), 
                    FightOrFlightColor["gradientLeft"], FightOrFlightColor["gradientRight"], FightOrFlightColor["gradientRight"], FightOrFlightColor["gradientLeft"]
                );
                
                var fightOrFlightDurationText = fightOrFlightDuration == 0 ? "" : Math.Round(fightOrFlightDuration).ToString(CultureInfo.InvariantCulture);
                DrawOutlinedText(fightOrFlightDurationText, new Vector2(cursorPos.X + 5f, cursorPos.Y - 2f), PluginConfiguration.PLDFightOrFlightColor, new Vector4(0f, 0f, 0f, 1f));
            }
            else if (requiescatBuff.Any())
            {
                var requiescatDuration = Math.Abs(requiescatBuff.First().RemainingTime);
                drawList.AddRectFilledMultiColor(
                    cursorPos, cursorPos + new Vector2(barSize.X / 12f * requiescatDuration, barSize.Y), 
                    RequiescatColor["gradientLeft"], RequiescatColor["gradientRight"], RequiescatColor["gradientRight"], RequiescatColor["gradientLeft"]
                );
                
                var requiescatDurationText = requiescatDuration == 0 ? "" : Math.Round(requiescatDuration).ToString(CultureInfo.InvariantCulture);
                DrawOutlinedText(requiescatDurationText, new Vector2(cursorPos.X + 5f, cursorPos.Y - 2f), PluginConfiguration.PLDRequiescatColor, new Vector4(0f, 0f, 0f, 1f));
            }

            drawList.AddRect(cursorPos, cursorPos + barSize, 0xFF000000);
            return BuffBarHeight + initialHeight + InterBarOffset;
        }

        private int DrawAtonementBar(int initialHeight)
        {
            Debug.Assert(ClientState.LocalPlayer != null, "ClientState.LocalPlayer != null");
            var atonementBuff = ClientState.LocalPlayer.StatusList.Where(o => o.StatusId == 1902);
            var stackCount = atonementBuff.Any() ? atonementBuff.First().StackCount : 0;
            
            var barWidth = (AtonementBarWidth - AtonementBarPadding * 2) / 3f;
            var xPos = CenterX - XOffset + AtonementBarXOffset;
            var yPos = CenterY + YOffset + initialHeight + AtonementBarYOffset;
            var cursorPos = new Vector2(xPos, yPos);
            var barSize = new Vector2(barWidth, AtonementBarHeight);
            
            var drawList = ImGui.GetWindowDrawList();

            for (var i = 0; i <= 2; i++)
            {
                drawList.AddRectFilled(cursorPos, cursorPos + barSize, 0x88000000);
                if (stackCount > 0)
                {
                    drawList.AddRectFilledMultiColor(
                        cursorPos, cursorPos + new Vector2(barWidth, AtonementBarHeight),
                        AtonementColor["gradientLeft"], AtonementColor["gradientRight"], AtonementColor["gradientRight"], AtonementColor["gradientLeft"]
                    );
                    stackCount--;
                }
                drawList.AddRect(cursorPos, cursorPos + barSize, 0xFF000000);

                cursorPos += new Vector2(barWidth + AtonementBarPadding, 0);
            }

            return AtonementBarHeight + initialHeight + InterBarOffset;
        }
    }
}