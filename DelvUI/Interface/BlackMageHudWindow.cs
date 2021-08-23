using System;
using System.Numerics;
using Dalamud.Plugin;
using ImGuiNET;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Dalamud.Data;
using Dalamud.Game.ClientState;
using Dalamud.Game.ClientState.JobGauge;
using Dalamud.Game.ClientState.JobGauge.Types;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.Gui;

namespace DelvUI.Interface
{
    public class BlackMageHudWindow : HudWindow
    {
        public override uint JobId => Jobs.BLM;

        private float OriginX => CenterX + PluginConfiguration.BLMHorizontalOffset;
        private float OriginY => CenterY + YOffset + PluginConfiguration.BLMVerticalOffset;
        private int VerticalSpaceBetweenBars => PluginConfiguration.BLMVerticalSpaceBetweenBars;
        private int HorizontalSpaceBetweenBars => PluginConfiguration.BLMHorizontalSpaceBetweenBars;
        private int ManaBarWidth => PluginConfiguration.BLMManaBarWidth;
        private int ManaBarHeight => PluginConfiguration.BLMManaBarHeight;
        private int UmbralHeartHeight => PluginConfiguration.BLMUmbralHeartHeight;
        private int UmbralHeartWidth=> PluginConfiguration.BLMUmbralHeartWidth;
        private int PolyglotHeight => PluginConfiguration.BLMPolyglotHeight;
        private int PolyglotWidth => PluginConfiguration.BLMPolyglotWidth;
        private bool ShowManaValue => PluginConfiguration.BLMShowManaValue;
        private bool ShowManaThresholdMarker => PluginConfiguration.BLMShowManaThresholdMarker;
        private int ManaThresholdValue => PluginConfiguration.BLMManaThresholdValue;
        private bool ShowTripleCast => PluginConfiguration.BLMShowTripleCast;
        private int TripleCastHeight => PluginConfiguration.BLMTripleCastHeight;
        private int TripleCastWidth => PluginConfiguration.BLMTripleCastWidth;
        private bool ShowFirestarterProcs => PluginConfiguration.BLMShowFirestarterProcs;
        private bool ShowThundercloudProcs => PluginConfiguration.BLMShowThundercloudProcs;
        private int ProcsHeight => PluginConfiguration.BLMProcsHeight;
        private bool ShowDotTimer => PluginConfiguration.BLMShowDotTimer;
        private int DotTimerHeight => PluginConfiguration.BLMDotTimerHeight;

        private Dictionary<string, uint> ManaBarNoElementColor => PluginConfiguration.JobColorMap[Jobs.BLM * 1000];
        private Dictionary<string, uint> ManaBarIceColor => PluginConfiguration.JobColorMap[Jobs.BLM * 1000 + 1];
        private Dictionary<string, uint> ManaBarFireColor => PluginConfiguration.JobColorMap[Jobs.BLM * 1000 + 2];
        private Dictionary<string, uint> UmbralHeartColor => PluginConfiguration.JobColorMap[Jobs.BLM * 1000 + 3];
        private Dictionary<string, uint> PolyglotColor => PluginConfiguration.JobColorMap[Jobs.BLM * 1000 + 4];
        private Dictionary<string, uint> TriplecastColor => PluginConfiguration.JobColorMap[Jobs.BLM * 1000 + 5];
        private Dictionary<string, uint> FirestarterColor => PluginConfiguration.JobColorMap[Jobs.BLM * 1000 + 6];
        private Dictionary<string, uint> ThundercloudColor => PluginConfiguration.JobColorMap[Jobs.BLM * 1000 + 7];
        private Dictionary<string, uint> DotColor => PluginConfiguration.JobColorMap[Jobs.BLM * 1000 + 8];

        public BlackMageHudWindow(
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
            DrawFocusBar();
            DrawCastBar();
            DrawTargetBar();

            DrawEnochian();
            DrawManaBar();
            DrawUmbralHeartStacks();
            DrawPolyglot();

            if (ShowTripleCast)
            {
                DrawTripleCast();
            }

            if (ShowFirestarterProcs || ShowThundercloudProcs)
            {
                DrawProcs();
            }

            if (ShowDotTimer)
            {
                DrawDotTimer();
            }
        }

        protected virtual void DrawManaBar()
        {
            Debug.Assert(ClientState.LocalPlayer != null, "ClientState.LocalPlayer != null");

            var gauge = JobGauges.Get<BLMGauge>();
            var actor = ClientState.LocalPlayer;
            var scale = (float)actor.CurrentMp / actor.MaxMp;
            var barSize = new Vector2(ManaBarWidth, ManaBarHeight);
            var cursorPos = new Vector2(OriginX - barSize.X / 2, OriginY - ManaBarHeight);

            // mana bar
            var color = gauge.InAstralFire ? ManaBarFireColor : gauge.InUmbralIce ? ManaBarIceColor : ManaBarNoElementColor;

            var drawList = ImGui.GetWindowDrawList();
            drawList.AddRectFilled(cursorPos, cursorPos + barSize, color["background"]);
            drawList.AddRectFilledMultiColor(
                cursorPos, cursorPos + new Vector2(barSize.X * scale, barSize.Y),
                color["gradientLeft"], color["gradientRight"], color["gradientRight"], color["gradientLeft"]
            );

            drawList.AddRect(cursorPos, cursorPos + barSize, 0xFF000000);

            // threshold marker
            if (ShowManaThresholdMarker && gauge.InAstralFire)
            {
                var position = new Vector2(OriginX - barSize.X / 2 + (ManaThresholdValue / 10000f) * barSize.X, cursorPos.Y + barSize.Y);
                var size = new Vector2(3, barSize.Y);
                drawList.AddRectFilledMultiColor(
                    position, position - size,
                    0xFF000000, 0x00000000, 0x00000000, 0xFF000000
                );
            }

            // element timer
            if (gauge.InAstralFire || gauge.InUmbralIce)
            {
                var time = gauge.ElementTimeRemaining > 10 ? gauge.ElementTimeRemaining / 1000 + 1 : 0;
                var text = $"{time,0}";
                var textSize = ImGui.CalcTextSize(text);
                DrawOutlinedText(text, new Vector2(OriginX - textSize.X / 2f, OriginY - ManaBarHeight / 2f - textSize.Y / 2f));
            }

            // mana
            if (ShowManaValue) {
                var mana = ClientState.LocalPlayer.CurrentMp;
                var text = $"{mana,0}";
                var textSize = ImGui.CalcTextSize(text);
                DrawOutlinedText(text, new Vector2(OriginX - barSize.X / 2f + 2, OriginY - ManaBarHeight / 2f - textSize.Y / 2f));
            }
        }

        protected virtual void DrawEnochian()
        {
            var gauge = JobGauges.Get<BLMGauge>();
            if (!gauge.IsEnochianActive) {
                return;
            }

            var barSize = new Vector2(ManaBarWidth + 4, ManaBarHeight + 4);
            var cursorPos = new Vector2(OriginX - barSize.X / 2f, OriginY - ManaBarHeight - 2);

            var drawList = ImGui.GetWindowDrawList();
            drawList.AddRectFilled(cursorPos, cursorPos + barSize, 0x88FFFFFF);
        }

        protected virtual void DrawUmbralHeartStacks()
        {
            var gauge = JobGauges.Get<BLMGauge>();

            var barSize = new Vector2(UmbralHeartWidth, UmbralHeartHeight);
            var totalWidth = barSize.X * 3 + HorizontalSpaceBetweenBars * 2;
            var cursorPos = new Vector2(OriginX - totalWidth / 2, OriginY - ManaBarHeight - VerticalSpaceBetweenBars - UmbralHeartHeight);

            var drawList = ImGui.GetWindowDrawList();

            for (var i = 1; i <= 3; i++) {
                drawList.AddRectFilled(cursorPos, cursorPos + barSize, UmbralHeartColor["background"]);
                if (gauge.UmbralHearts >= i)
                {
                    drawList.AddRectFilledMultiColor(
                        cursorPos, cursorPos + new Vector2(barSize.X, barSize.Y),
                        UmbralHeartColor["gradientLeft"], UmbralHeartColor["gradientRight"], UmbralHeartColor["gradientRight"], UmbralHeartColor["gradientLeft"]
                    );
                }
                drawList.AddRect(cursorPos, cursorPos + barSize, 0xFF000000);

                cursorPos.X = cursorPos.X + barSize.X + HorizontalSpaceBetweenBars;
            }
        }

        protected virtual void DrawPolyglot()
        {
            var gauge = JobGauges.Get<BLMGauge>();
            var barSize = new Vector2(PolyglotWidth, PolyglotHeight);
            var scale = gauge.PolyglotStacks == 2 ? 0 : (gauge.IsEnochianActive ? gauge.EnochianTimer / 30000f : 1);
            scale = 1 - scale;

            var y = OriginY - ManaBarHeight - VerticalSpaceBetweenBars - UmbralHeartHeight - VerticalSpaceBetweenBars - PolyglotHeight;
            if (ShowTripleCast) {
                y = y - VerticalSpaceBetweenBars - TripleCastHeight;
            }
            
            // 1st stack (charged)
            var cursorPos = new Vector2(OriginX - barSize.X - (HorizontalSpaceBetweenBars / 2f), y);
            DrawPolyglotStack(cursorPos, barSize, gauge.PolyglotStacks == 0 ? scale : 1);

            // 2nd stack
            cursorPos.X = CenterX + (HorizontalSpaceBetweenBars / 2f);
            DrawPolyglotStack(cursorPos, barSize, gauge.PolyglotStacks >= 1 ? scale : 0);
        }

        private void DrawPolyglotStack(Vector2 position, Vector2 size, float scale)
        {
            Debug.Assert(ClientState.LocalPlayer != null, "ClientState.LocalPlayer != null");

            var drawList = ImGui.GetWindowDrawList();

            // glow
            if (scale == 1) {
                var glowPosition = new Vector2(position.X - 1, position.Y - 1);
                var glowSize = new Vector2(size.X + 2, size.Y + 2);
                drawList.AddRectFilled(glowPosition, glowPosition + glowSize, 0xAAFFFFFF);
            }

            // background
            drawList.AddRectFilled(position, position + size, PolyglotColor["background"]);

            // fill
            if (scale == 1) {
                drawList.AddRectFilledMultiColor(position, position + size,
                    PolyglotColor["gradientLeft"], PolyglotColor["gradientRight"], PolyglotColor["gradientRight"], PolyglotColor["gradientLeft"]
                );
            }
            else if (scale > 0) {
                var width = Math.Max(1, size.X * scale);
                drawList.AddRectFilled(position, position + new Vector2(width, size.Y), PolyglotColor["gradientLeft"]);
            }

            // black border
            drawList.AddRect(position, position + size, 0xFF000000);
        }

        protected virtual void DrawTripleCast()
        {
            Debug.Assert(ClientState.LocalPlayer != null, "ClientState.LocalPlayer != null");
            var tripleStackBuff = ClientState.LocalPlayer.StatusList.FirstOrDefault(o => o.StatusId == 1211);
            var barSize = new Vector2(TripleCastWidth, TripleCastHeight);
            var totalWidth = barSize.X * 3 + HorizontalSpaceBetweenBars * 2;
            var cursorPos = new Vector2(OriginX - totalWidth / 2, OriginY - ManaBarHeight - VerticalSpaceBetweenBars - UmbralHeartHeight - VerticalSpaceBetweenBars - barSize.Y);

            var drawList = ImGui.GetWindowDrawList();

            for (var i = 1; i <= 3; i++) {
                drawList.AddRectFilled(cursorPos, cursorPos + barSize, TriplecastColor["background"]);
                var stackCount = tripleStackBuff?.StackCount ?? 0;
                if (stackCount >= i)
                {
                    drawList.AddRectFilledMultiColor(
                        cursorPos, cursorPos + new Vector2(barSize.X, barSize.Y),
                        TriplecastColor["gradientLeft"], TriplecastColor["gradientRight"], TriplecastColor["gradientRight"], TriplecastColor["gradientLeft"]
                    );
                }
                drawList.AddRect(cursorPos, cursorPos + barSize, 0xFF000000);

                cursorPos.X = cursorPos.X + barSize.X + HorizontalSpaceBetweenBars;
            }
        }

        protected virtual void DrawProcs()
        {
            Debug.Assert(ClientState.LocalPlayer != null, "ClientState.LocalPlayer != null");
            var firestarter = ClientState.LocalPlayer.StatusList.FirstOrDefault(o => o.StatusId == 165);
            var firestarterTimer = ShowFirestarterProcs ? Math.Abs(firestarter?.RemainingTime ?? 0f) : 0;
            var thundercloud = ClientState.LocalPlayer.StatusList.FirstOrDefault(o => o.StatusId == 164);
            var thundercloudTimer = ShowThundercloudProcs ? thundercloud?.RemainingTime ?? 0f : 0;

            if (firestarterTimer == 0 && thundercloudTimer == 0) {
                return;
            }

            var totalHeight = firestarterTimer > 0 && thundercloudTimer > 0 ? ProcsHeight * 2 + VerticalSpaceBetweenBars : ProcsHeight;
            var x = OriginX - HorizontalSpaceBetweenBars * 2f - PolyglotWidth;
            var y = OriginY - ManaBarHeight - VerticalSpaceBetweenBars - UmbralHeartHeight - VerticalSpaceBetweenBars - PolyglotHeight / 2f + ProcsHeight;
            if (ShowTripleCast) {
                y = y - VerticalSpaceBetweenBars - TripleCastHeight;
            }

            // fire starter
            if (firestarterTimer > 0) {
                var position = new Vector2(x, y - totalHeight / 2f);
                var scale = firestarterTimer / 18f;

                DrawTimerBar(position, scale, ProcsHeight, FirestarterColor, true);
            }

            // thundercloud
            if (thundercloudTimer > 0) {
                var position = new Vector2(x, firestarterTimer == 0 ? y - totalHeight / 2f : y + VerticalSpaceBetweenBars / 2f);
                var scale = thundercloudTimer / 18f;

                DrawTimerBar(position, scale, ProcsHeight, ThundercloudColor, true);
            }
        }

        protected virtual void DrawDotTimer()
        {
            var actor = TargetManager.SoftTarget ?? TargetManager.Target;
            if (actor is not BattleChara target) {
                return;
            }

            // thunder 1 to 4
            var dotIDs = new[] { 161, 162, 163, 1210 };
            var dotDurations = new[] { 12f, 18f, 24f, 18f };

            float timer = 0;
            float maxDuration = 1;

            for (var i = 0; i < 4; i++) {
                var dot = target.StatusList.FirstOrDefault(o => o.StatusId == dotIDs[i]);
                timer = dot?.RemainingTime ?? 0f;
                
                if (timer > 0) {
                    maxDuration = dotDurations[i];
                    break;
                }
            }

            if (timer == 0)
            {
                return;
            }

            var x = OriginX + HorizontalSpaceBetweenBars * 2f + PolyglotWidth;
            var y = OriginY - ManaBarHeight - VerticalSpaceBetweenBars - UmbralHeartHeight - VerticalSpaceBetweenBars - PolyglotHeight / 2f - DotTimerHeight;
            if (ShowTripleCast) {
                y = y - VerticalSpaceBetweenBars - TripleCastHeight;
            }

            var position = new Vector2(x, y + DotTimerHeight / 2f);
            var scale = timer / maxDuration;
            
            DrawTimerBar(position, scale, DotTimerHeight, DotColor, false);
        }

        private void DrawTimerBar(Vector2 position, float scale, float height, Dictionary<string, uint> colorMap, bool inverted)
        {
            Debug.Assert(ClientState.LocalPlayer != null, "ClientState.LocalPlayer != null");

            var drawList = ImGui.GetWindowDrawList();
            var size = new Vector2((ManaBarWidth / 2f - PolyglotWidth - HorizontalSpaceBetweenBars * 2f) * scale, height);
            size.X = Math.Max(1, size.X);
            var endPoint = inverted ? position - size : position + size;

            drawList.AddRectFilledMultiColor(position, endPoint,
                colorMap["gradientLeft"], colorMap["gradientRight"], colorMap["gradientRight"], colorMap["gradientLeft"]
            );
        }
    }
}