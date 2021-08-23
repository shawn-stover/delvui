using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using Dalamud.Data;
using Dalamud.Game.ClientState;
using Dalamud.Game.ClientState.JobGauge;
using Dalamud.Game.ClientState.JobGauge.Types;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.Gui;
using Dalamud.Plugin;
using ImGuiNET;

namespace DelvUI.Interface
{
    public class MonkHudWindow : HudWindow
    {
        public override uint JobId => 20;


        private int DemolishHeight => PluginConfiguration.MNKDemolishHeight;

        private int DemolishWidth => PluginConfiguration.MNKDemolishWidth;

        private int DemolishXOffset => PluginConfiguration.MNKDemolishXOffset;

        private int DemolishYOffset => PluginConfiguration.MNKDemolishYOffset;

        private int ChakraHeight => PluginConfiguration.MNKChakraHeight;

        private int ChakraWidth => PluginConfiguration.MNKChakraWidth;

        private int ChakraXOffset => PluginConfiguration.MNKChakraXOffset;

        private int ChakraYOffset => PluginConfiguration.MNKChakraYOffset;

        private int BuffHeight => PluginConfiguration.MNKBuffHeight;

        private int BuffWidth => PluginConfiguration.MNKBuffWidth;

        private int BuffXOffset => PluginConfiguration.MNKBuffXOffset;

        private int BuffYOffset => PluginConfiguration.MNKBuffYOffset;

        private int TimeTwinXOffset => PluginConfiguration.MNKTimeTwinXOffset;

        private int TimeTwinYOffset => PluginConfiguration.MNKTimeTwinYOffset;

        private int TimeLeadenXOffset => PluginConfiguration.MNKTimeLeadenXOffset;

        private int TimeLeadenYOffset => PluginConfiguration.MNKTimeLeadenYOffset;

        private int TimeDemoXOffset => PluginConfiguration.MNKTimeDemoXOffset;

        private int TimeDemoYOffset => PluginConfiguration.MNKTimeDemoYOffset;

        private Dictionary<string, uint> DemolishColor => PluginConfiguration.JobColorMap[Jobs.MNK * 1000];

        private Dictionary<string, uint> ChakraColor => PluginConfiguration.JobColorMap[Jobs.MNK * 1000 + 1];

        private Dictionary<string, uint> LeadenFistColor => PluginConfiguration.JobColorMap[Jobs.MNK * 1000 + 2];

        private Dictionary<string, uint> TwinSnakesColor => PluginConfiguration.JobColorMap[Jobs.MNK * 1000 + 3];

        public MonkHudWindow(
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
            DrawTargetBar();
            ChakraBar();
            Demolish();
            ActiveBuffs();
            DrawFocusBar();
            DrawCastBar();
        }

        private void ActiveBuffs()
        {
            var target = ClientState.LocalPlayer;

            if (target == null) {
                return;
            }

            const int xPadding = 1;
            var barWidth = (BuffWidth / 2) - 1;
            var twinSnakes = target.StatusList.FirstOrDefault(o => o.StatusId == 101);
            var leadenFist = target.StatusList.FirstOrDefault(o => o.StatusId == 1861);

            var twinSnakesDuration = twinSnakes == null ? 0f : twinSnakes.RemainingTime;
            var leadenFistDuration = leadenFist == null ? 0f : leadenFist.RemainingTime;

            var xOffset = CenterX - BuffXOffset;
            var cursorPos = new Vector2(CenterX - BuffXOffset, CenterY + BuffYOffset - 8);
            var barSize = new Vector2(barWidth, BuffHeight);
            var drawList = ImGui.GetWindowDrawList();
            var twinXOffset = TimeTwinXOffset;
            var twinYOffset = TimeTwinYOffset;

            var buffStart = new Vector2(xOffset + barWidth - (barSize.X / 15) * twinSnakesDuration, CenterY + BuffYOffset - 8);

            drawList.AddRectFilled(cursorPos, cursorPos + barSize, 0x88000000);
            drawList.AddRectFilledMultiColor(
                buffStart, cursorPos + new Vector2(barSize.X, barSize.Y),
                TwinSnakesColor["gradientLeft"], TwinSnakesColor["gradientRight"], TwinSnakesColor["gradientRight"], TwinSnakesColor["gradientLeft"]
            );

            if (!PluginConfiguration.ShowBuffTime)
            {
                drawList.AddRect(cursorPos, cursorPos + barSize, 0xFF000000);

                cursorPos = new Vector2(cursorPos.X + barWidth + xPadding, cursorPos.Y);

                drawList.AddRectFilled(cursorPos, cursorPos + barSize, 0x88000000);
                drawList.AddRectFilledMultiColor(
                    cursorPos, cursorPos + new Vector2((barSize.X / 30) * leadenFistDuration, barSize.Y),
                    leadenFistDuration > 0 ? LeadenFistColor["gradientLeft"] : 0x00202E3,
                    leadenFistDuration > 0 ? LeadenFistColor["gradientRight"] : 0x00202E3,
                    leadenFistDuration > 0 ? LeadenFistColor["gradientRight"] : 0x00202E3,
                    leadenFistDuration > 0 ? LeadenFistColor["gradientLeft"] : 0x00202E3
                );
                drawList.AddRect(cursorPos, cursorPos + barSize, 0xFF000000);
            }
            else
            {
                drawList.AddRect(cursorPos, cursorPos + barSize, 0xFF000000);
                DrawOutlinedText(Math.Round(twinSnakesDuration).ToString(CultureInfo.InvariantCulture), new Vector2(twinXOffset, twinYOffset));

                cursorPos = new Vector2(cursorPos.X + barWidth + xPadding, cursorPos.Y);
                var leadenXOffset = TimeLeadenXOffset;
                var leadenYOffset = TimeLeadenYOffset;

                drawList.AddRectFilled(cursorPos, cursorPos + barSize, 0x88000000);
                drawList.AddRectFilledMultiColor(
                    cursorPos, cursorPos + new Vector2((barSize.X / 30) * leadenFistDuration, barSize.Y),
                    leadenFistDuration > 0 ? LeadenFistColor["gradientLeft"] : 0x00202E3,
                    leadenFistDuration > 0 ? LeadenFistColor["gradientRight"] : 0x00202E3,
                    leadenFistDuration > 0 ? LeadenFistColor["gradientRight"] : 0x00202E3,
                    leadenFistDuration > 0 ? LeadenFistColor["gradientLeft"] : 0x00202E3
                );
                drawList.AddRect(cursorPos, cursorPos + barSize, 0xFF000000);
                if (leadenFistDuration <= 0)
                    DrawOutlinedText("0", new Vector2(leadenXOffset, leadenYOffset));
                else
                    DrawOutlinedText(Math.Round(leadenFistDuration).ToString(CultureInfo.InvariantCulture), new Vector2(leadenXOffset, leadenYOffset));
            }
        }

        private void Demolish()
        {
            var actor = TargetManager.SoftTarget ?? TargetManager.Target;

            if (actor is not BattleChara target)
            {
                return;
            }

            const int xPadding = 2;
            var barWidth = (DemolishWidth) - 1;
            var demolish = target.StatusList.FirstOrDefault(o => o.StatusId == 246 || o.StatusId == 1309);

            var demolishDuration = demolish == null ? 0f : demolish.RemainingTime;
            var demolishColor = DemolishColor;

            var cursorPos = new Vector2(CenterX - DemolishXOffset - 255, CenterY + DemolishYOffset - 52);
            var barSize = new Vector2(barWidth, DemolishHeight);
            var drawList = ImGui.GetWindowDrawList();

            var demoXOffset = TimeDemoXOffset;
            var demoYOffset = TimeDemoYOffset;

            cursorPos = new Vector2(cursorPos.X + barWidth + xPadding, cursorPos.Y);

            drawList.AddRectFilled(cursorPos, cursorPos + barSize, 0x88000000);
            drawList.AddRectFilledMultiColor(
                cursorPos, cursorPos + new Vector2((barSize.X / 18) * demolishDuration, barSize.Y),
                demolishColor["gradientLeft"], demolishColor["gradientRight"], demolishColor["gradientRight"], demolishColor["gradientLeft"]
            );
            drawList.AddRect(cursorPos, cursorPos + barSize, 0xFF000000);

            if (PluginConfiguration.ShowDemolishTime) {
                DrawOutlinedText(Math.Round(demolishDuration).ToString(CultureInfo.InvariantCulture), new Vector2(demoXOffset, demoYOffset));
            }
        }

        private void ChakraBar()
        {
            var gauge = JobGauges.Get<MNKGauge>();

            const int xPadding = 2;
            var barWidth = (ChakraWidth - xPadding * 3) / 5;
            var barSize = new Vector2(barWidth, ChakraHeight);
            var xPos = CenterX - ChakraXOffset;
            var yPos = CenterY + ChakraYOffset - 30;
            var cursorPos = new Vector2(xPos, yPos);

            var drawList = ImGui.GetWindowDrawList();
            for (var i = 0; i <= 5 - 1; i++)
            {
                drawList.AddRectFilled(cursorPos, cursorPos + barSize, 0x88000000);
                if (gauge.Chakra > i)
                {
                    drawList.AddRectFilledMultiColor(
                        cursorPos, cursorPos + new Vector2(barSize.X, barSize.Y),
                        ChakraColor["gradientLeft"], ChakraColor["gradientRight"], ChakraColor["gradientRight"], ChakraColor["gradientLeft"]
                    );
                }
                
                drawList.AddRect(cursorPos, cursorPos + barSize, 0xFF000000);
                cursorPos = new Vector2(cursorPos.X + barWidth + xPadding, cursorPos.Y);
            }
        }
    }
}