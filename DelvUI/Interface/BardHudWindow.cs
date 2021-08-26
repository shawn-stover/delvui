﻿using System.Linq;
using System.Numerics;
using Dalamud.Data;
using Dalamud.Game.ClientState;
using Dalamud.Game.ClientState.JobGauge;
using Dalamud.Game.ClientState.JobGauge.Enums;
using Dalamud.Game.ClientState.JobGauge.Types;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.Gui;
using Dalamud.Plugin;
using ImGuiNET;

namespace DelvUI.Interface {
    public class BardHudWindow : HudWindow {
        public override uint JobId => 23;

        private static int BarHeight => 20;
        private static int SmallBarHeight => 10;
        private static int BarWidth => 250;
        private new static int XOffset => 127;
        private new static int YOffset => 440;
        
        public BardHudWindow(
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

        protected override void Draw(bool _) {
            DrawHealthBar();
            DrawFocusBar();
            DrawCastBar();
            DrawTargetBar();
            
            DrawActiveDots();
            HandleCurrentSong();
            DrawSoulVoiceBar();
        }

        private void DrawActiveDots()
        {
            var target = TargetManager.SoftTarget ?? TargetManager.Target;

            if (target is not BattleChara actor) {
                return;
            }
            
            var expiryColor = 0xFF2E2EC7;
            var xPadding = 2;
            var barWidth = (BarWidth / 2) - 1;
            var cb = actor.StatusList.FirstOrDefault(o => o.StatusId is 1200 or 124);
            var sb = actor.StatusList.FirstOrDefault(o => o.StatusId is 1201 or 129);

            var cbDuration = cb == null ? 0f : cb.RemainingTime;
            var sbDuration = sb == null ? 0f : sb.RemainingTime;

            var cbColor = cbDuration > 5 ? 0xFFEB44B6 : expiryColor;
            var sbColor = sbDuration > 5 ? 0xFFCA7548 : expiryColor;
            
            var xOffset = CenterX - 127;
            var cursorPos = new Vector2(CenterX - 127, CenterY + YOffset - 34);
            var barSize = new Vector2(barWidth, SmallBarHeight);
            var drawList = ImGui.GetWindowDrawList();

            var dotStart = new Vector2(xOffset + barWidth - (barSize.X / 30) * cbDuration, CenterY + YOffset - 34);
            
            // Caustic Bite
            drawList.AddRectFilled(cursorPos, cursorPos + barSize, 0x88000000);
            drawList.AddRectFilledMultiColor(
                dotStart, cursorPos + new Vector2(barSize.X, barSize.Y),
                cbColor, cbColor, cbColor, cbColor
            );
            drawList.AddRect(cursorPos, cursorPos + barSize, 0xFF000000);
            
            cursorPos = new Vector2(cursorPos.X + barWidth + xPadding, cursorPos.Y);
            
            // Stormbite
            drawList.AddRectFilled(cursorPos, cursorPos + barSize, 0x88000000);
            drawList.AddRectFilledMultiColor(
                cursorPos, cursorPos + new Vector2((barSize.X / 30) * sbDuration, barSize.Y),
                sbColor, sbColor, sbColor, sbColor
            );
            drawList.AddRect(cursorPos, cursorPos + barSize, 0xFF000000);
            
        }

        private void HandleCurrentSong() {
            var gauge = JobGauges.Get<BRDGauge>();
            var songStacks = gauge.Repertoire;
            var song = gauge.Song;
            var songTimer = gauge.SongTimer;
            
            switch (song)
            {
                case Song.WANDERER:
                    DrawStacks(songStacks, 3, 0xFFE8D796);
                    DrawSongTimer(songTimer, 0xFF5CD15C);
                    break;
                case Song.MAGE:
                    DrawSongTimer(songTimer, 0xFF8F5A8F);
                    DrawBloodletterReady();
                    break;
                case Song.ARMY:
                    DrawStacks(songStacks, 4, 0xFFB1DE00);
                    DrawSongTimer(songTimer, 0xFF34CDCF);
                    break;
                case Song.NONE:
                    DrawSongTimer(0, 0x88000000);
                    break;
                default:
                    DrawSongTimer(0, 0x88000000);
                    break;
            }
        }

        private void DrawBloodletterReady()
        {
            // I want to draw Bloodletter procs here (just color entire bar red to indicate cooldown is ready).
            // But can't find a way yet to accomplish this.
        }

        private void DrawSongTimer(short songTimer, uint songColor)
        {
            var cursorPos = new Vector2(CenterX - 127, CenterY + YOffset);
            var barSize = new Vector2(BarWidth, BarHeight);
            var drawList = ImGui.GetWindowDrawList();
            drawList.AddRectFilled(cursorPos, cursorPos + new Vector2(barSize.X , barSize.Y), 0x88000000);
            drawList.AddRectFilledMultiColor(
                cursorPos, cursorPos + new Vector2(barSize.X * songTimer/30000, barSize.Y), 
                songColor, songColor, songColor, songColor
            ); 
            drawList.AddRect(cursorPos, cursorPos + new Vector2(barSize.X , barSize.Y), 0xFF000000);
            
            var songTimerAbbreviated = songTimer != 0 ? (songTimer / 1000).ToString() : "";
            var textSize = ImGui.CalcTextSize(songTimerAbbreviated);
            DrawOutlinedText(songTimerAbbreviated, new Vector2(cursorPos.X + BarWidth / 2f - textSize.X / 2f, cursorPos.Y-2));
        }

        private void DrawSoulVoiceBar() {
            var soulVoice = JobGauges.Get<BRDGauge>().SoulVoice;
            const uint soulVoiceColor = 0xFF00E3F8;

            var barSize = new Vector2(BarWidth, BarHeight);
            var cursorPos = new Vector2(CenterX - 127, CenterY + YOffset - 22);
            
            var drawList = ImGui.GetWindowDrawList();
            drawList.AddRectFilled(cursorPos, cursorPos + barSize, 0x88000000);
            drawList.AddRectFilledMultiColor(
                cursorPos, cursorPos + new Vector2(barSize.X * soulVoice/100, barSize.Y), 
                soulVoiceColor, soulVoiceColor, soulVoiceColor, soulVoiceColor
            );
            
            drawList.AddRect(cursorPos, cursorPos + barSize, 0xFF000000);
            var textSize = ImGui.CalcTextSize(soulVoice.ToString());
            DrawOutlinedText(soulVoice.ToString(), new Vector2(cursorPos.X + BarWidth / 2f - textSize.X / 2f, cursorPos.Y-2));
        }

        private void DrawStacks(int amount, int max, uint stackColor)
        {
            const int xPadding = 2;
            var barWidth = (BarWidth - xPadding * (max - 1)) / max;
            var barSize = new Vector2(barWidth, SmallBarHeight);
            var xPos = CenterX - XOffset;
            var yPos = CenterY + YOffset - 46;
            var cursorPos = new Vector2(xPos, yPos);

            var drawList = ImGui.GetWindowDrawList();
            for(var i = 0; i <= max - 1; i++)
            {
                drawList.AddRectFilled(cursorPos, cursorPos + barSize, 0x88000000);
                if(amount > i)
                {
                    drawList.AddRectFilledMultiColor(
                        cursorPos, cursorPos + new Vector2(barSize.X, barSize.Y),
                        stackColor, stackColor, stackColor, stackColor
                    );
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