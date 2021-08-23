using System;
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
    public class SamuraiHudWindow : HudWindow
    {
        public override uint JobId => 34;

        private bool GaugeEnabled => PluginConfiguration.SAMGaugeEnabled;
        private int GaugeHeight => PluginConfiguration.SAMGaugeHeight;
        private int GaugeWidth => PluginConfiguration.SAMGaugeWidth;
        private int GaugeXOffset => PluginConfiguration.SAMGaugeXOffset;
        private int GaugeYOffset => PluginConfiguration.SAMGaugeYOffset;

        private bool SenEnabled => PluginConfiguration.SAMSenEnabled;
        private int SenPadding => PluginConfiguration.SAMSenPadding;
        private int SenHeight => PluginConfiguration.SAMSenHeight;
        private int SenWidth => PluginConfiguration.SAMSenWidth;
        private int SenXOffset => PluginConfiguration.SAMSenXOffset;
        private int SenYOffset => PluginConfiguration.SAMSenYOffset;

        private bool MeditationEnabled => PluginConfiguration.SAMMeditationEnabled;
        private int MeditationPadding => PluginConfiguration.SAMMeditationPadding;
        private int MeditationHeight => PluginConfiguration.SAMMeditationHeight;
        private int MeditationWidth => PluginConfiguration.SAMMeditationWidth;
        private int MeditationXOffset => PluginConfiguration.SAMMeditationXOffset;
        private int MeditationYOffset => PluginConfiguration.SAMMeditationYOffset;

        public SamuraiHudWindow(
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

            if (GaugeEnabled) {
                DrawPrimaryResourceBar();
            }

            if (SenEnabled) {
                DrawSenResourceBar();
            }

            if (MeditationEnabled) {
                DrawMeditationResourceBar();
            }
            
            DrawTargetBar();
            DrawFocusBar();
            DrawCastBar();
        }

        protected override void DrawPrimaryResourceBar()
        {
            var gauge = JobGauges.Get<SAMGauge>();

            var xPos = CenterX - XOffset + GaugeXOffset;
            var yPos = CenterY + YOffset + GaugeHeight + GaugeYOffset;
            var cursorPos = new Vector2(xPos, yPos);
            const int chunkSize = 100;
            var barSize = new Vector2(GaugeWidth, GaugeHeight);

            // Kenki Gauge
            var kenki = Math.Min((int)gauge.Kenki, chunkSize);
            var scale = (float)kenki / chunkSize;
            var drawList = ImGui.GetWindowDrawList();
            drawList.AddRectFilled(cursorPos, cursorPos + barSize, 0x88000000);
            drawList.AddRectFilledMultiColor(
                cursorPos, cursorPos + new Vector2(GaugeWidth * scale, GaugeHeight),
                0xFF5252FF, 0xFF9C9CFF, 0xFF9C9CFF, 0xFF5252FF
            );

            drawList.AddRect(cursorPos, cursorPos + barSize, 0xFF000000);
            var textSize = ImGui.CalcTextSize(gauge.Kenki.ToString());
            DrawOutlinedText(gauge.Kenki.ToString(), new Vector2(cursorPos.X + GaugeWidth / 2f - textSize.X / 2f, cursorPos.Y + (barSize.Y / 2) - 12));

        }

        private void DrawSenResourceBar() {
            var gauge = JobGauges.Get<SAMGauge>();

            const int numChunks = 3;

            var senBarWidth = (SenWidth - SenPadding * (numChunks - 1)) / numChunks;
            var senBarSize = new Vector2(senBarWidth, SenHeight);
            var xPos = CenterX - SenXOffset;
            var yPos = CenterY + SenYOffset;
            var cursorPos = new Vector2(xPos - SenPadding - senBarWidth, yPos);

            var drawList = ImGui.GetWindowDrawList();

            // Setsu Bar
            cursorPos = new Vector2(cursorPos.X + SenPadding + senBarWidth, cursorPos.Y);
            drawList.AddRectFilled(cursorPos, cursorPos + senBarSize, gauge.HasSetsu ? 0xFFF7EA59 : 0x88000000);
            drawList.AddRect(cursorPos, cursorPos + senBarSize, 0xFF000000);

            // Getsu Bar
            cursorPos = new Vector2(cursorPos.X + SenPadding + senBarWidth, cursorPos.Y);
            drawList.AddRectFilled(cursorPos, cursorPos + senBarSize, gauge.HasGetsu ? 0xFFF77E59 : 0x88000000);
            drawList.AddRect(cursorPos, cursorPos + senBarSize, 0xFF000000);

            // Ka Bar
            cursorPos = new Vector2(cursorPos.X + SenPadding + senBarWidth, cursorPos.Y);
            drawList.AddRectFilled(cursorPos, cursorPos + senBarSize, gauge.HasKa ? 0XFF5959F7 : 0x88000000);
            drawList.AddRect(cursorPos, cursorPos + senBarSize, 0xFF000000);
        }


        private void DrawMeditationResourceBar()
        {
            var gauge = JobGauges.Get<SAMGauge>();

            const int numChunks = 3;

            var meditationBarWidth = (MeditationWidth - MeditationPadding * (numChunks - 1)) / numChunks;
            var meditationBarSize = new Vector2(meditationBarWidth, MeditationHeight);
            var xPos = CenterX - MeditationXOffset;
            var yPos = CenterY + MeditationYOffset + MeditationHeight;
            var cursorPos = new Vector2(xPos - MeditationPadding - meditationBarWidth, yPos);

            var drawList = ImGui.GetWindowDrawList();

            // Meditation Stacks
            for (var i = 1; i < 4; i++) {
                cursorPos = new Vector2(cursorPos.X + MeditationPadding + meditationBarWidth, cursorPos.Y);

                drawList.AddRectFilled(
                    cursorPos, cursorPos + meditationBarSize,
                    gauge.MeditationStacks >= i ? ImGui.ColorConvertFloat4ToU32(new Vector4(247 / 255f, 163 / 255f, 89 / 255f, 255f)) : 0x88000000
                );

                drawList.AddRect(cursorPos, cursorPos + meditationBarSize, 0xFF000000);
            }
        }
    }
}