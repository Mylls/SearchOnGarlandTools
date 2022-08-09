using System;
using System.Linq;
using System.Numerics;

using CheapLoc;
using Dalamud.DrunkenToad;
using Dalamud.Game.Text;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Components;
using ImGuiNET;

namespace PriceCheck
{
    /// <summary>
    /// Config window for the plugin.
    /// </summary>
    public class ConfigWindow : PluginWindow
    {
        private readonly PriceCheckPlugin plugin;
        private Tab currentTab = Tab.General;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigWindow"/> class.
        /// </summary>
        /// <param name="plugin">PriceCheck plugin.</param>
        public ConfigWindow(PriceCheckPlugin plugin)
            : base(plugin, "Search On Garland Config")
        {
            this.plugin = plugin;
            this.Size = new Vector2(600f, 600f);
            this.SizeCondition = ImGuiCond.Appearing;
        }

        private enum Tab
        {
            General,
            Chat,
            Toast,
            Keybind,
            Filters,
            Thresholds,
            ContextMenu,
        }

        /// <inheritdoc/>
        public override void Draw()
        {
            this.DrawTabs();
            switch (this.currentTab)
            {
                case Tab.General:
                {
                    this.DrawGeneral();
                    break;
                }

                case Tab.Chat:
                {
                    this.DrawChat();
                    break;
                }

                case Tab.Toast:
                {
                    this.DrawToast();
                    break;
                }

                case Tab.Keybind:
                {
                    this.DrawKeybind();
                    break;
                }

                case Tab.Filters:
                {
                    this.DrawFilters();
                    break;
                }

                case Tab.Thresholds:
                {
                    this.DrawThresholds();
                    break;
                }

                case Tab.ContextMenu:
                {
                    this.DrawContextMenu();
                    break;
                }

                default:
                    this.DrawGeneral();
                    break;
            }
        }

        private void DrawTabs()
        {
            if (ImGui.BeginTabBar("SearchOnGarlandSettingsTabBar", ImGuiTabBarFlags.NoTooltip))
            {
                if (ImGui.BeginTabItem(Loc.Localize("General", "General") + "###SearchOnGarland_General_Tab"))
                {
                    this.currentTab = Tab.General;
                    ImGui.EndTabItem();
                }

                if (ImGui.BeginTabItem(Loc.Localize("Chat", "Chat") + "###SearchOnGarland_Chat_Tab"))
                {
                    this.currentTab = Tab.Chat;
                    ImGui.EndTabItem();
                }

                if (ImGui.BeginTabItem(Loc.Localize("Toast", "Toast") + "###SearchOnGarland_Toast_Tab"))
                {
                    this.currentTab = Tab.Toast;
                    ImGui.EndTabItem();
                }

                if (ImGui.BeginTabItem(Loc.Localize("Keybind", "Keybind") + "###SearchOnGarland_Keybind_Tab"))
                {
                    this.currentTab = Tab.Keybind;
                    ImGui.EndTabItem();
                }

                if (ImGui.BeginTabItem(Loc.Localize("Filters", "Filters") + "###SearchOnGarland_Filters_Tab"))
                {
                    this.currentTab = Tab.Filters;
                    ImGui.EndTabItem();
                }

                if (ImGui.BeginTabItem(Loc.Localize("Thresholds", "Thresholds") + "###SearchOnGarland_Thresholds_Tab"))
                {
                    this.currentTab = Tab.Thresholds;
                    ImGui.EndTabItem();
                }

                if (ImGui.BeginTabItem(Loc.Localize("ContextMenu", "Context Menu") + "###SearchOnGarland_ContextMenu_Tab"))
                {
                    this.currentTab = Tab.ContextMenu;
                    ImGui.EndTabItem();
                }

                ImGui.EndTabBar();
                ImGui.Spacing();
            }
        }

        private void DrawGeneral()
        {
            var enabled = this.plugin.Configuration.Enabled;
            if (ImGui.Checkbox(
                Loc.Localize("PluginEnabled", "Plugin enabled") + "###SearchOnGarland_PluginEnabled_Checkbox",
                ref enabled))
            {
                this.plugin.Configuration.Enabled = enabled;
                this.plugin.SaveConfig();
            }

            ImGuiComponents.HelpMarker(Loc.Localize(
                "PluginEnabled_HelpMarker",
                "toggle the plugin on/off"));

            var allowKeybindAfterHover = this.plugin.Configuration.AllowKeybindAfterHover;
            if (ImGui.Checkbox(
                Loc.Localize("AllowKeybindAfterHover", "Allow keybind to be pressed after hovering over item") + "###SearchOnGarland_AllowKeybindAfterHover_Checkbox",
                ref allowKeybindAfterHover))
            {
                this.plugin.Configuration.AllowKeybindAfterHover = allowKeybindAfterHover;
                this.plugin.SaveConfig();
            }

            ImGuiComponents.HelpMarker(Loc.Localize(
                                           "AllowKeybindAfterHover_HelpMarker",
                                           "allows you to hold the keybind after the item tooltip has been opened (disable for legacy mode)"));

            ImGui.Spacing();
            ImGui.Text(Loc.Localize("HoverDelay", "Hover delay"));
            ImGuiComponents.HelpMarker(Loc.Localize(
                "HoverDelay_HelpMarker",
                "delay (in seconds) before processing after hovering"));
            var hoverDelay = this.plugin.Configuration.HoverDelay;
            if (ImGui.SliderInt("###SearchOnGarland_HoverDelay_Slider", ref hoverDelay, 0, 10))
            {
                this.plugin.Configuration.HoverDelay = hoverDelay;
                this.plugin.SaveConfig();
            }

            ImGui.Spacing();
            ImGui.Text(Loc.Localize("PriceMode", "Price mode"));
            ImGuiComponents.HelpMarker(Loc.Localize(
                "PriceMode_HelpMarker",
                "select price calculation to use"));
            var priceMode = this.plugin.Configuration.PriceMode;
            if (ImGui.Combo(
                "###PriceCheck_PriceMode_Combo",
                ref priceMode,
                PriceMode.PriceModeNames.ToArray(),
                PriceMode.PriceModeNames.Count))
            {
                this.plugin.Configuration.PriceMode = priceMode;
                this.plugin.SaveConfig();
            }

            ImGui.PushStyleColor(ImGuiCol.Text, ImGuiColors.HealerGreen);
            ImGui.TextWrapped(PriceMode.GetPriceModeByIndex(priceMode)?.Description);
            ImGui.PopStyleColor();

            ImGui.Spacing();
        }

        private void DrawChat()
        {
            ImGui.TextColored(ImGuiColors.DalamudViolet, Loc.Localize("DisplayHeading", "Display"));
            ImGui.Spacing();

            var showInChat = this.plugin.Configuration.ShowInChat;
            if (ImGui.Checkbox(
                Loc.Localize("ShowInChat", "Show in chat") + "###PriceCheck_ShowInChat_Checkbox",
                ref showInChat))
            {
                this.plugin.Configuration.ShowInChat = showInChat;
                this.plugin.SaveConfig();
            }

            ImGuiComponents.HelpMarker(Loc.Localize(
                "ShowInChat_HelpMarker",
                "show price check results in chat"));

            ImGui.Text(Loc.Localize("ChatChannel", "Chat channel"));
            ImGuiComponents.HelpMarker(Loc.Localize(
                                           "ChatChannel_HelpMarker",
                                           "set the chat channel to send messages"));
            var chatChannel = this.plugin.Configuration.ChatChannel;
            ImGui.SetNextItemWidth(ImGui.GetWindowSize().X / 3);
            if (ImGui.BeginCombo("###PriceCheck_ChatChannel_Combo", chatChannel.ToString()))
            {
                foreach (var type in Enum.GetValues(typeof(XivChatType)).Cast<XivChatType>())
                {
                    if (ImGui.Selectable(type.ToString(), type == chatChannel))
                    {
                        this.plugin.Configuration.ChatChannel = type;
                        this.plugin.SaveConfig();
                    }
                }

                ImGui.EndCombo();
            }

            ImGui.TextColored(ImGuiColors.DalamudViolet, Loc.Localize("StyleHeading", "Style"));
            ImGui.Spacing();

            var useChatColors = this.plugin.Configuration.UseChatColors;
            if (ImGui.Checkbox(
                Loc.Localize("UseChatColors", "Use chat colors") + "###PriceCheck_UseChatColors_Checkbox",
                ref useChatColors))
            {
                this.plugin.Configuration.UseChatColors = useChatColors;
                this.plugin.SaveConfig();
            }

            ImGuiComponents.HelpMarker(Loc.Localize(
                "UseChatColors_HelpMarker",
                "use different colors for chat based on result"));

            var useItemLinks = this.plugin.Configuration.UseItemLinks;
            if (ImGui.Checkbox(
                Loc.Localize("UseItemLinks", "Use item links") + "###PriceCheck_UseItemLinks_Checkbox",
                ref useItemLinks))
            {
                this.plugin.Configuration.UseItemLinks = useItemLinks;
                this.plugin.SaveConfig();
            }

            ImGuiComponents.HelpMarker(Loc.Localize(
                "UseItemLinks_HelpMarker",
                "use item links in chat results"));

            ImGui.Spacing();
            ImGui.TextColored(ImGuiColors.DalamudViolet, Loc.Localize("FiltersHeading", "Filters"));
            var showSuccessInChat = this.plugin.Configuration.ShowSuccessInChat;
            if (ImGui.Checkbox(
                Loc.Localize("ShowSuccessInChat", "Show successful price check") + "###PriceCheck_ShowSuccessInChat_Checkbox",
                ref showSuccessInChat))
            {
                this.plugin.Configuration.ShowSuccessInChat = showSuccessInChat;
                this.plugin.SaveConfig();
            }

            ImGuiComponents.HelpMarker(Loc.Localize(
                                           "ShowSuccessInChat_HelpMarker",
                                           "show successful price check"));

            var showFailedToProcessInChat = this.plugin.Configuration.ShowFailedToProcessInChat;
            if (ImGui.Checkbox(
                Loc.Localize("ShowFailedToProcessInChat", "Show failed to process error") + "###PriceCheck_ShowFailedToProcessInChat_Checkbox",
                ref showFailedToProcessInChat))
            {
                this.plugin.Configuration.ShowFailedToProcessInChat = showFailedToProcessInChat;
                this.plugin.SaveConfig();
            }

            ImGuiComponents.HelpMarker(Loc.Localize(
                                           "ShowFailedToProcessInChat_HelpMarker",
                                           "show error where something went wrong unexpectedly"));

            var showFailedToGetDataInChat = this.plugin.Configuration.ShowFailedToGetDataInChat;
            if (ImGui.Checkbox(
                Loc.Localize("ShowFailedToGetDataInChat", "Show failed to get data error") + "###PriceCheck_ShowFailedToGetDataInChat_Checkbox",
                ref showFailedToGetDataInChat))
            {
                this.plugin.Configuration.ShowFailedToGetDataInChat = showFailedToGetDataInChat;
                this.plugin.SaveConfig();
            }

            ImGuiComponents.HelpMarker(Loc.Localize(
                                           "ShowFailedToGetDataInChat_HelpMarker",
                                           "show error where the plugin couldn't connect to universalis to get the data - usually a problem with your connection or universalis is down"));

            var showNoDataAvailableInChat = this.plugin.Configuration.ShowNoDataAvailableInChat;
            if (ImGui.Checkbox(
                Loc.Localize("ShowNoDataAvailableInChat", "Show no data available warning") + "###PriceCheck_ShowNoDataAvailableInChat_Checkbox",
                ref showNoDataAvailableInChat))
            {
                this.plugin.Configuration.ShowNoDataAvailableInChat = showNoDataAvailableInChat;
                this.plugin.SaveConfig();
            }

            ImGuiComponents.HelpMarker(Loc.Localize(
                                           "ShowNoDataAvailableInChat_HelpMarker",
                                           "show warning where there was no data from universalis available for the item"));

            var showNoRecentDataAvailableInChat = this.plugin.Configuration.ShowNoRecentDataAvailableInChat;
            if (ImGui.Checkbox(
                Loc.Localize("ShowNoRecentDataAvailableInChat", "Show no recent data available warning") + "###PriceCheck_ShowNoRecentDataAvailableInChat_Checkbox",
                ref showNoRecentDataAvailableInChat))
            {
                this.plugin.Configuration.ShowNoRecentDataAvailableInChat = showNoRecentDataAvailableInChat;
                this.plugin.SaveConfig();
            }

            ImGuiComponents.HelpMarker(Loc.Localize(
                                           "ShowNoRecentDataAvailableInChat_HelpMarker",
                                           "show warning where there was no recent data from universalis available for the item within your threshold"));

            var showBelowVendorInChat = this.plugin.Configuration.ShowBelowVendorInChat;
            if (ImGui.Checkbox(
                Loc.Localize("ShowBelowVendorInChat", "Show cheaper than vendor price warning") + "###PriceCheck_ShowBelowVendorInChat_Checkbox",
                ref showBelowVendorInChat))
            {
                this.plugin.Configuration.ShowBelowVendorInChat = showBelowVendorInChat;
                this.plugin.SaveConfig();
            }

            ImGuiComponents.HelpMarker(Loc.Localize(
                                           "ShowBelowVendorInChat_HelpMarker",
                                           "show warning that the market price is cheaper than what you can sell it to a vendor for"));

            var showBelowMinimumInChat = this.plugin.Configuration.ShowBelowMinimumInChat;
            if (ImGui.Checkbox(
                Loc.Localize("ShowBelowMinimumInChat", "Show cheaper than minimum threshold warning") + "###PriceCheck_ShowBelowMinimumInChat_Checkbox",
                ref showBelowMinimumInChat))
            {
                this.plugin.Configuration.ShowBelowMinimumInChat = showBelowMinimumInChat;
                this.plugin.SaveConfig();
            }

            ImGuiComponents.HelpMarker(Loc.Localize(
                                           "ShowBelowMinimumInChat_HelpMarker",
                                           "show warning the price is below your minimum threshold"));

            var showUnmarketableInChat = this.plugin.Configuration.ShowUnmarketableInChat;
            if (ImGui.Checkbox(
                Loc.Localize("ShowUnmarketableInChat", "Show unmarketable warning") + "###PriceCheck_ShowUnmarketableInChat_Checkbox",
                ref showUnmarketableInChat))
            {
                this.plugin.Configuration.ShowUnmarketableInChat = showUnmarketableInChat;
                this.plugin.SaveConfig();
            }

            ImGuiComponents.HelpMarker(Loc.Localize(
                                           "ShowUnmarketableInChat_HelpMarker",
                                           "show warning that the item can't be sold on the market board"));
        }

        private void DrawToast()
        {
            var showToast = this.plugin.Configuration.ShowToast;
            if (ImGui.Checkbox(
                Loc.Localize("ShowToast", "Show toast") + "###PriceCheck_ShowToast_Checkbox",
                ref showToast))
            {
                this.plugin.Configuration.ShowToast = showToast;
                this.plugin.SaveConfig();
            }

            ImGuiComponents.HelpMarker(Loc.Localize(
                                           "ShowToast_HelpMarker",
                                           "show price check results in toasts"));
        }

        private void DrawKeybind()
        {
            var keybindEnabled = this.plugin.Configuration.KeybindEnabled;
            if (ImGui.Checkbox(
                Loc.Localize("KeybindEnabled", "Enable keybind") + "###PriceCheck_KeybindEnabled_Checkbox",
                ref keybindEnabled))
            {
                this.plugin.Configuration.KeybindEnabled = keybindEnabled;
                this.plugin.SaveConfig();
            }

            ImGuiComponents.HelpMarker(Loc.Localize(
                "KeybindEnabled_HelpMarker",
                "toggle if keybind is used or just hover"));

            var showKeybindInTitleBar = this.plugin.Configuration.ShowKeybindInTitleBar;
            if (ImGui.Checkbox(
                Loc.Localize("ShowKeybindInTitleBar", "Show keybind in titlebar") + "###PriceCheck_ShowKeybindInTitleBar_Checkbox",
                ref showKeybindInTitleBar))
            {
                this.plugin.Configuration.ShowKeybindInTitleBar = showKeybindInTitleBar;
                this.plugin.SaveConfig();
                this.plugin.WindowManager.MainWindow?.UpdateWindowTitle();
            }

            ImGuiComponents.HelpMarker(Loc.Localize(
                                           "ShowKeybindInTitleBar_HelpMarker",
                                           "toggle if keybind is displayed in titlebar"));

            ImGui.Spacing();
            ImGui.Text(Loc.Localize("ModifierKeybind", "Modifier"));
            ImGuiComponents.HelpMarker(Loc.Localize(
                "ModifierKeybind_HelpMarker",
                "set your modifier key (e.g. shift)"));
            var modifierKey =
                ModifierKey.EnumToIndex(this.plugin.Configuration.ModifierKey);
            if (ImGui.Combo(
                "###PriceCheck_ModifierKey_Combo",
                ref modifierKey,
                ModifierKey.Names.ToArray(),
                ModifierKey.Names.Length))
            {
                this.plugin.Configuration.ModifierKey =
                    ModifierKey.IndexToEnum(modifierKey);
                this.plugin.SaveConfig();
                this.plugin.WindowManager.MainWindow?.UpdateWindowTitle();
            }

            ImGui.Spacing();
            ImGui.Text(Loc.Localize("PrimaryKeybind", "Primary"));
            ImGuiComponents.HelpMarker(Loc.Localize(
                "PrimaryKeybind_HelpMarker",
                "set your primary key (e.g. None, Z)"));
            var primaryKey = PrimaryKey.EnumToIndex(this.plugin.Configuration.PrimaryKey);
            if (ImGui.Combo(
                "###PriceCheck_PrimaryKey_Combo",
                ref primaryKey,
                PrimaryKey.Names.ToArray(),
                PrimaryKey.Names.Length))
            {
                this.plugin.Configuration.PrimaryKey = PrimaryKey.IndexToEnum(primaryKey);
                this.plugin.SaveConfig();
                this.plugin.WindowManager.MainWindow?.UpdateWindowTitle();
            }
        }

        private void DrawFilters()
        {
            var restrictInCombat = this.plugin.Configuration.RestrictInCombat;
            if (ImGui.Checkbox(
                Loc.Localize("RestrictInCombat", "Don't process in combat") + "###PriceCheck_RestrictInCombat_Checkbox",
                ref restrictInCombat))
            {
                this.plugin.Configuration.RestrictInCombat = restrictInCombat;
                this.plugin.SaveConfig();
            }

            ImGuiComponents.HelpMarker(Loc.Localize(
                "RestrictInCombat_HelpMarker",
                "don't process price checks while in combat"));

            var restrictInContent = this.plugin.Configuration.RestrictInContent;
            if (ImGui.Checkbox(
                Loc.Localize("RestrictInContent", "Don't process in content") + "###PriceCheck_RestrictInContent_Checkbox",
                ref restrictInContent))
            {
                this.plugin.Configuration.RestrictInContent = restrictInContent;
                this.plugin.SaveConfig();
            }

            ImGuiComponents.HelpMarker(Loc.Localize(
                "RestrictInContent_HelpMarker",
                "don't process price checks while in content"));
        }

        private void DrawThresholds()
        {
            ImGui.Text(Loc.Localize("MinimumPrice", "Minimum Price"));
            ImGuiComponents.HelpMarker(Loc.Localize(
                "MinimumPrice_HelpMarker",
                "set minimum price at which actual average will be displayed"));
            var minPrice = this.plugin.Configuration.MinPrice;
            if (ImGui.InputInt("###PriceCheck_MinPrice_Slider", ref minPrice, 500, 500))
            {
                this.plugin.Configuration.MinPrice = Math.Abs(minPrice);
                this.plugin.SaveConfig();
            }

            ImGui.Spacing();
            ImGui.Text(Loc.Localize("MaxUploadDays", "Max Upload Days"));
            ImGuiComponents.HelpMarker(Loc.Localize(
                "MaxUploadDays_HelpMarker",
                "set maximum age to avoid using old data"));
            var maxUploadDays = this.plugin.Configuration.MaxUploadDays;
            if (ImGui.InputInt("###PriceCheck_MaxUploadDays_Slider", ref maxUploadDays, 5, 5))
            {
                this.plugin.Configuration.MaxUploadDays = Math.Abs(maxUploadDays);
                this.plugin.SaveConfig();
            }
        }

        private void DrawContextMenu()
        {
            var showContextMenu = this.plugin.Configuration.ShowContextMenu;
            if (ImGui.Checkbox(
                Loc.Localize("ShowContextMenu", "Show context menu") +
                "###PriceCheck_ShowContextMenu_Checkbox",
                ref showContextMenu))
            {
                this.plugin.Configuration.ShowContextMenu = showContextMenu;
                this.plugin.SaveConfig();
            }
        }
    }
}
