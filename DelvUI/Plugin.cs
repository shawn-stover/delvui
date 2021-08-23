using System;
using System.IO;
using System.Reflection;
using Dalamud.Data;
using Dalamud.Game.ClientState;
using Dalamud.Game.ClientState.JobGauge;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Game.Command;
using Dalamud.Game.Gui;
using Dalamud.Logging;
using Dalamud.Plugin;
using DelvUI.Interface;
using ImGuiNET;

namespace DelvUI {
    public class Plugin : IDalamudPlugin {
        public string Name => "DelvUI";

        private readonly ClientState _clientState;
        private readonly CommandManager _commandManager;
        private readonly ConfigurationWindow _configurationWindow;
        private readonly DalamudPluginInterface _pluginInterface;
        private readonly DataManager _dataManager;
        private readonly GameGui _gameGui;
        private readonly JobGauges _jobGauges;
        private readonly ObjectTable _objectTable;
        private readonly PluginConfiguration _pluginConfiguration;
        private readonly TargetManager _targetManager;

        private HudWindow _hudWindow;
        
        private bool _fontBuilt;
        private bool _fontLoadFailed;
        
        // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Global
        // ReSharper disable once MemberCanBePrivate.Global
        public string AssemblyLocation { get; set; } = Assembly.GetExecutingAssembly().Location;

        public Plugin (
            ClientState clientState,
            CommandManager commandManager,
            DalamudPluginInterface pluginInterface,
            DataManager dataManager,
            GameGui gameGui,
            JobGauges jobGauges,
            ObjectTable objectTable,
            TargetManager targetManager
        ) 
        {
            _clientState = clientState;
            _commandManager = commandManager;
            _dataManager = dataManager;
            _gameGui = gameGui;
            _jobGauges = jobGauges;
            _objectTable = objectTable;
            _pluginInterface = pluginInterface;
            _targetManager = targetManager;
            _pluginConfiguration = pluginInterface.GetPluginConfig() as PluginConfiguration ?? new PluginConfiguration();
            _pluginConfiguration.Init(_pluginInterface);
            _configurationWindow = new ConfigurationWindow(this, _pluginInterface, _pluginConfiguration);

            _commandManager?.AddHandler(
                "/pdelvui",
                new CommandInfo(PluginCommand)
                {
                    HelpMessage = "Opens the DelvUI configuration window.",
                    ShowInHelp = true
                }
            );

            _pluginInterface.UiBuilder.Draw += Draw;
            _pluginInterface.UiBuilder.BuildFonts += BuildFont;
            _pluginInterface.UiBuilder.OpenConfigUi += OpenConfigUi;
            
            if (!_fontBuilt && !_fontLoadFailed) {
                _pluginInterface.UiBuilder.RebuildFonts();
            }
        }
        
        private void BuildFont() {
            var fontFile = Path.Combine(Path.GetDirectoryName(AssemblyLocation) ?? "", "Media", "Fonts", "big-noodle-too.ttf");
            _fontBuilt = false;
            
            if (File.Exists(fontFile)) {
                try {
                    _pluginConfiguration.BigNoodleTooFont = ImGui.GetIO().Fonts.AddFontFromFileTTF(fontFile, 24);
                    _fontBuilt = true;
                } catch (Exception ex) {
                    PluginLog.Log($"Font failed to load. {fontFile}");
                    PluginLog.Log(ex.ToString());
                    _fontLoadFailed = true;
                }
            } else {
                PluginLog.Log($"Font doesn't exist. {fontFile}");
                _fontLoadFailed = true;
            }
        }

        private void PluginCommand(string command, string arguments) {
            _configurationWindow.IsVisible = !_configurationWindow.IsVisible;
        }

        private void Draw() {
            _pluginInterface.UiBuilder.OverrideGameCursor = false;
            
            if (_fontBuilt) {
                ImGui.PushFont(_pluginConfiguration.BigNoodleTooFont);
            }

            if (_hudWindow?.JobId != _clientState.LocalPlayer?.ClassJob.Id) {
                SwapJobs();
            }

            _configurationWindow.Draw();
            _hudWindow?.Draw();

            if (_fontBuilt) {
                ImGui.PopFont();
            }
        }

        private void SwapJobs() {
            _hudWindow = _clientState.LocalPlayer?.ClassJob.Id switch
            {
                //Tanks

                Jobs.DRK => new DarkKnightHudWindow(_clientState, _pluginInterface, _dataManager, _gameGui, _jobGauges, _objectTable, _pluginConfiguration, _targetManager),
                Jobs.GNB => new GunbreakerHudWindow(_clientState, _pluginInterface, _dataManager, _gameGui, _jobGauges, _objectTable, _pluginConfiguration, _targetManager),
                Jobs.WAR => new WarriorHudWindow(_clientState, _pluginInterface, _dataManager, _gameGui, _jobGauges, _objectTable, _pluginConfiguration, _targetManager),
                Jobs.PLD => new PaladinHudWindow(_clientState, _pluginInterface, _dataManager, _gameGui, _jobGauges, _objectTable, _pluginConfiguration, _targetManager),

                //Healers
                Jobs.WHM => new WhiteMageHudWindow(_clientState, _pluginInterface, _dataManager, _gameGui, _jobGauges, _objectTable, _pluginConfiguration, _targetManager),
                
                //Melee DPS
                Jobs.SAM => new SamuraiHudWindow(_clientState, _pluginInterface, _dataManager, _gameGui, _jobGauges, _objectTable, _pluginConfiguration, _targetManager),
                Jobs.MNK => new MonkHudWindow(_clientState, _pluginInterface, _dataManager, _gameGui, _jobGauges, _objectTable, _pluginConfiguration, _targetManager),
                
                //Ranged DPS
                Jobs.BRD => new BardHudWindow(_clientState, _pluginInterface, _dataManager, _gameGui, _jobGauges, _objectTable, _pluginConfiguration, _targetManager),
                Jobs.DNC => new DancerHudWindow(_clientState, _pluginInterface, _dataManager, _gameGui, _jobGauges, _objectTable, _pluginConfiguration, _targetManager),
                
                //Caster DPS
                Jobs.RDM => new RedMageHudWindow(_clientState, _pluginInterface, _dataManager, _gameGui, _jobGauges, _objectTable, _pluginConfiguration, _targetManager),
                Jobs.SMN => new SummonerHudWindow(_clientState, _pluginInterface, _dataManager, _gameGui, _jobGauges, _objectTable, _pluginConfiguration, _targetManager),
                Jobs.BLM => new BlackMageHudWindow(_clientState, _pluginInterface, _dataManager, _gameGui, _jobGauges, _objectTable, _pluginConfiguration, _targetManager),

                //Low jobs
                Jobs.MRD => new UnitFrameOnlyHudWindow(_clientState, _pluginInterface, _dataManager, _gameGui, _jobGauges, _objectTable, _pluginConfiguration, _targetManager),
                Jobs.GLD => new UnitFrameOnlyHudWindow(_clientState, _pluginInterface, _dataManager, _gameGui, _jobGauges, _objectTable, _pluginConfiguration, _targetManager),
                Jobs.CNJ => new UnitFrameOnlyHudWindow(_clientState, _pluginInterface, _dataManager, _gameGui, _jobGauges, _objectTable, _pluginConfiguration, _targetManager),
                Jobs.PGL => new UnitFrameOnlyHudWindow(_clientState, _pluginInterface, _dataManager, _gameGui, _jobGauges, _objectTable, _pluginConfiguration, _targetManager),
                Jobs.LNC => new UnitFrameOnlyHudWindow(_clientState, _pluginInterface, _dataManager, _gameGui, _jobGauges, _objectTable, _pluginConfiguration, _targetManager),
                Jobs.ROG => new UnitFrameOnlyHudWindow(_clientState, _pluginInterface, _dataManager, _gameGui, _jobGauges, _objectTable, _pluginConfiguration, _targetManager),
                Jobs.ARC => new UnitFrameOnlyHudWindow(_clientState, _pluginInterface, _dataManager, _gameGui, _jobGauges, _objectTable, _pluginConfiguration, _targetManager),
                Jobs.THM => new UnitFrameOnlyHudWindow(_clientState, _pluginInterface, _dataManager, _gameGui, _jobGauges, _objectTable, _pluginConfiguration, _targetManager),
                Jobs.ACN => new UnitFrameOnlyHudWindow(_clientState, _pluginInterface, _dataManager, _gameGui, _jobGauges, _objectTable, _pluginConfiguration, _targetManager),
                
                //Hand
                Jobs.CRP => new UnitFrameOnlyHudWindow(_clientState, _pluginInterface, _dataManager, _gameGui, _jobGauges, _objectTable, _pluginConfiguration, _targetManager),
                Jobs.BSM => new UnitFrameOnlyHudWindow(_clientState, _pluginInterface, _dataManager, _gameGui, _jobGauges, _objectTable, _pluginConfiguration, _targetManager),
                Jobs.ARM => new UnitFrameOnlyHudWindow(_clientState, _pluginInterface, _dataManager, _gameGui, _jobGauges, _objectTable, _pluginConfiguration, _targetManager),
                Jobs.GSM => new UnitFrameOnlyHudWindow(_clientState, _pluginInterface, _dataManager, _gameGui, _jobGauges, _objectTable, _pluginConfiguration, _targetManager),
                Jobs.LTW => new UnitFrameOnlyHudWindow(_clientState, _pluginInterface, _dataManager, _gameGui, _jobGauges, _objectTable, _pluginConfiguration, _targetManager),
                Jobs.WVR => new UnitFrameOnlyHudWindow(_clientState, _pluginInterface, _dataManager, _gameGui, _jobGauges, _objectTable, _pluginConfiguration, _targetManager),
                Jobs.ALC => new UnitFrameOnlyHudWindow(_clientState, _pluginInterface, _dataManager, _gameGui, _jobGauges, _objectTable, _pluginConfiguration, _targetManager),
                Jobs.CUL => new UnitFrameOnlyHudWindow(_clientState, _pluginInterface, _dataManager, _gameGui, _jobGauges, _objectTable, _pluginConfiguration, _targetManager),
                
                //Land
                Jobs.MIN => new UnitFrameOnlyHudWindow(_clientState, _pluginInterface, _dataManager, _gameGui, _jobGauges, _objectTable, _pluginConfiguration, _targetManager),
                Jobs.BOT => new UnitFrameOnlyHudWindow(_clientState, _pluginInterface, _dataManager, _gameGui, _jobGauges, _objectTable, _pluginConfiguration, _targetManager),
                Jobs.FSH => new UnitFrameOnlyHudWindow(_clientState, _pluginInterface, _dataManager, _gameGui, _jobGauges, _objectTable, _pluginConfiguration, _targetManager),
                
                //dont have packs yet
                Jobs.NIN => new UnitFrameOnlyHudWindow(_clientState, _pluginInterface, _dataManager, _gameGui, _jobGauges, _objectTable, _pluginConfiguration, _targetManager),
                Jobs.AST => new UnitFrameOnlyHudWindow(_clientState, _pluginInterface, _dataManager, _gameGui, _jobGauges, _objectTable, _pluginConfiguration, _targetManager),
                Jobs.DRG => new UnitFrameOnlyHudWindow(_clientState, _pluginInterface, _dataManager, _gameGui, _jobGauges, _objectTable, _pluginConfiguration, _targetManager),
                Jobs.BLU => new UnitFrameOnlyHudWindow(_clientState, _pluginInterface, _dataManager, _gameGui, _jobGauges, _objectTable, _pluginConfiguration, _targetManager),
                _ => _hudWindow
            };
        }
        
        private void OpenConfigUi() {
            _configurationWindow.IsVisible = !_configurationWindow.IsVisible;
        }

        protected virtual void Dispose(bool disposing) {
            if (!disposing) {
                return;
            }

            _configurationWindow.IsVisible = false;

            if (_hudWindow != null) {
                _hudWindow.IsVisible = false;
            }

            _commandManager.RemoveHandler("/pdelvui");
            _pluginInterface.UiBuilder.Draw -= Draw;
            _pluginInterface.UiBuilder.BuildFonts -= BuildFont;
            _pluginInterface.UiBuilder.OpenConfigUi -= OpenConfigUi;
            _pluginInterface.UiBuilder.RebuildFonts();
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}