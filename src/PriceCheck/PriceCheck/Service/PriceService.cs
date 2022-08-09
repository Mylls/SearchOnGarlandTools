using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

using CheapLoc;
using Dalamud.DrunkenToad;
using Dalamud.Game.Text;
using Dalamud.Interface.Colors;
using Lumina.Excel.GeneratedSheets;

namespace PriceCheck
{
    /// <summary>
    /// Pricing service.
    /// </summary>
    public class PriceService
    {
        private readonly PriceCheckPlugin plugin;
        private readonly List<PricedItem> pricedItems = new();
        private readonly object locker = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="PriceService"/> class.
        /// </summary>
        /// <param name="plugin">price check plugin.</param>
        public PriceService(PriceCheckPlugin plugin)
        {
            this.plugin = plugin;
            this.LastPriceCheck = DateUtil.CurrentTime();
        }

        /// <summary>
        /// Gets or sets last price check conducted in unix timestamp.
        /// </summary>
        public long LastPriceCheck { get; set; }

        /// <summary>
        /// Get priced items.
        /// </summary>
        /// <returns>list of priced items.</returns>
        public IEnumerable<PricedItem> GetItems()
        {
            lock (this.locker)
            {
                return this.pricedItems.ToList();
            }
        }

        /// <summary>
        /// Clear all items.
        /// </summary>
        public void ClearItems()
        {
            lock (this.locker)
            {
                this.pricedItems.Clear();
            }
        }

        /// <summary>
        /// Conduct price check.
        /// </summary>
        /// <param name="itemId">item id to lookup.</param>
        /// <param name="isHQ">indicator if item is hq.</param>
        public void ProcessItemAsync(uint itemId, bool isHQ)
        {
            try
            {
                if (!this.plugin.ShouldPriceCheck()) return;

                // reject if invalid itemId
                if (itemId == 0)
                {
                    this.plugin.ItemCancellationTokenSource = null;
                    return;
                }

                // cancel if in-flight request
                if (this.plugin.ItemCancellationTokenSource != null)
                {
                    if (!this.plugin.ItemCancellationTokenSource.IsCancellationRequested)
                        this.plugin.ItemCancellationTokenSource.Cancel();
                    this.plugin.ItemCancellationTokenSource.Dispose();
                }

                // create new cancel token
                this.plugin.ItemCancellationTokenSource =
                    new CancellationTokenSource(this.plugin.Configuration.RequestTimeout * 2);

                // run price check
                Task.Run(async () =>
                {
                    await Task.Delay(
                                  this.plugin.Configuration.HoverDelay * 1000,
                                  this.plugin.ItemCancellationTokenSource!.Token)
                              .ConfigureAwait(false);
                    this.plugin.PriceService.ProcessItem(itemId);
                });
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Failed to process item.");
                this.plugin.ItemCancellationTokenSource = null;
                this.plugin.HoveredItemManager.ItemId = 0;
            }
        }

        private void ProcessItem(uint itemId)
        {
            // reject invalid item id
            if (itemId == 0) return;

            // create priced item
            Logger.LogDebug($"Pricing itemId={itemId}");
            var pricedItem = new PricedItem
            {
                ItemId = itemId
            };


            // run Wiki Lookup
            WikiLookup(itemId, pricedItem);

            // check for existing entry for this itemId
            for (var i = 0; i < this.pricedItems.Count; i++)
            {
                if (this.pricedItems[i].ItemId != pricedItem.ItemId) continue;
                this.pricedItems.RemoveAt(i);
                break;
            }

            // determine message and colors
            this.SetFieldsByResult(pricedItem);

            // add to overlay
            if (this.plugin.Configuration.ShowOverlay)
            {
                // remove items over max
                while (this.pricedItems.Count >= this.plugin.Configuration.MaxItemsInOverlay)
                {
                    this.pricedItems.RemoveAt(this.pricedItems.Count - 1);
                }

                // add item depending on result
                switch (pricedItem.Result)
                {
                    case ItemResult.None:
                        break;
                    case ItemResult.Success:
                        if (this.plugin.Configuration.ShowSuccessInOverlay) this.AddItemToOverlay(pricedItem);
                        break;
                    case ItemResult.FailedToProcess:
                        if (this.plugin.Configuration.ShowFailedToProcessInOverlay) this.AddItemToOverlay(pricedItem);
                        break;
                    case ItemResult.FailedToGetData:
                        if (this.plugin.Configuration.ShowFailedToGetDataInOverlay) this.AddItemToOverlay(pricedItem);
                        break;
                    case ItemResult.NoDataAvailable:
                        if (this.plugin.Configuration.ShowNoDataAvailableInOverlay) this.AddItemToOverlay(pricedItem);
                        break;
                    case ItemResult.NoRecentDataAvailable:
                        if (this.plugin.Configuration.ShowNoRecentDataAvailableInOverlay)
                            this.AddItemToOverlay(pricedItem);
                        break;
                    case ItemResult.BelowVendor:
                        if (this.plugin.Configuration.ShowBelowVendorInOverlay) this.AddItemToOverlay(pricedItem);
                        break;
                    case ItemResult.BelowMinimum:
                        if (this.plugin.Configuration.ShowBelowMinimumInOverlay) this.AddItemToOverlay(pricedItem);
                        break;
                    case ItemResult.Unmarketable:
                        if (this.plugin.Configuration.ShowUnmarketableInOverlay) this.AddItemToOverlay(pricedItem);
                        break;
                    default:
                        Logger.LogError("Unrecognized item result.");
                        break;
                }
            }

            // send chat message
            if (this.plugin.Configuration.ShowInChat)
            {
                switch (pricedItem.Result)
                {
                    case ItemResult.None:
                        break;
                    case ItemResult.Success:
                        if (this.plugin.Configuration.ShowSuccessInChat) this.plugin.PrintItemMessage(pricedItem);
                        break;
                    case ItemResult.FailedToProcess:
                        if (this.plugin.Configuration.ShowFailedToProcessInChat)
                            this.plugin.PrintItemMessage(pricedItem);
                        break;
                    case ItemResult.FailedToGetData:
                        if (this.plugin.Configuration.ShowFailedToGetDataInChat)
                            this.plugin.PrintItemMessage(pricedItem);
                        break;
                    case ItemResult.NoDataAvailable:
                        if (this.plugin.Configuration.ShowNoDataAvailableInChat)
                            this.plugin.PrintItemMessage(pricedItem);
                        break;
                    case ItemResult.NoRecentDataAvailable:
                        if (this.plugin.Configuration.ShowNoRecentDataAvailableInChat)
                            this.plugin.PrintItemMessage(pricedItem);
                        break;
                    case ItemResult.BelowVendor:
                        if (this.plugin.Configuration.ShowBelowVendorInChat) this.plugin.PrintItemMessage(pricedItem);
                        break;
                    case ItemResult.BelowMinimum:
                        if (this.plugin.Configuration.ShowBelowMinimumInChat) this.plugin.PrintItemMessage(pricedItem);
                        break;
                    case ItemResult.Unmarketable:
                        if (this.plugin.Configuration.ShowUnmarketableInChat) this.plugin.PrintItemMessage(pricedItem);
                        break;
                    default:
                        Logger.LogError("Unrecognized item result.");
                        break;
                }
            }

            // send toast
            if (this.plugin.Configuration.ShowToast)
            {
                PriceCheckPlugin.SendToast(pricedItem);
            }
        }

        private void AddItemToOverlay(PricedItem pricedItem)
        {
            this.plugin.WindowManager.MainWindow!.IsOpen = true;
            this.pricedItems.Insert(0, pricedItem);
        }

        private void SetFieldsByResult(PricedItem pricedItem)
        {
            switch (pricedItem.Result)
            {
                case ItemResult.Success:
                    pricedItem.Message = this.plugin.Configuration.ShowPrices
                                             ? pricedItem.MarketPrice.ToString("N0", CultureInfo.InvariantCulture)
                                             : Loc.Localize("OpenedGarlandTools", "Opened Garland Tools");
                    pricedItem.OverlayColor = ImGuiColors.HealerGreen;
                    pricedItem.ChatColor = 45;
                    break;
                case ItemResult.None:
                    pricedItem.Message = Loc.Localize("FailedToGetData", "Failed to get data");
                    pricedItem.OverlayColor = ImGuiColors.DPSRed;
                    pricedItem.ChatColor = 17;
                    break;
                case ItemResult.FailedToProcess:
                    pricedItem.Message = Loc.Localize("FailedToProcess", "Failed to process item");
                    pricedItem.OverlayColor = ImGuiColors.DPSRed;
                    pricedItem.ChatColor = 17;
                    break;
                case ItemResult.FailedToGetData:
                    pricedItem.Message = Loc.Localize("FailedToGetData", "Failed to get data");
                    pricedItem.OverlayColor = ImGuiColors.DPSRed;
                    pricedItem.ChatColor = 17;
                    break;
                case ItemResult.NoDataAvailable:
                    pricedItem.Message = Loc.Localize("NoDataAvailable", "No data available");
                    pricedItem.OverlayColor = ImGuiColors.DPSRed;
                    pricedItem.ChatColor = 17;
                    break;
                case ItemResult.NoRecentDataAvailable:
                    pricedItem.Message = Loc.Localize("NoRecentDataAvailable", "No recent data");
                    pricedItem.OverlayColor = ImGuiColors.DPSRed;
                    pricedItem.ChatColor = 17;
                    break;
                case ItemResult.BelowVendor:
                    pricedItem.Message = Loc.Localize("BelowVendor", "Sell to vendor");
                    pricedItem.OverlayColor = ImGuiColors.DalamudYellow;
                    pricedItem.ChatColor = 25;
                    break;
                case ItemResult.BelowMinimum:
                    pricedItem.Message = Loc.Localize("BelowMinimum", "Below minimum price");
                    pricedItem.OverlayColor = ImGuiColors.DalamudYellow;
                    pricedItem.ChatColor = 25;
                    break;
                case ItemResult.Unmarketable:
                    pricedItem.Message = Loc.Localize("Unmarketable", "Can't sell on marketboard");
                    pricedItem.OverlayColor = ImGuiColors.DalamudYellow;
                    pricedItem.ChatColor = 25;
                    break;
                default:
                    pricedItem.Message = Loc.Localize("FailedToProcess", "Failed to process item");
                    pricedItem.OverlayColor = ImGuiColors.DPSRed;
                    pricedItem.ChatColor = 17;
                    break;
            }

            Logger.LogDebug($"Message={pricedItem.Message}");
        }

        public static void WikiLookup(uint itemId, PricedItem pricedItem)
        {
            var url = $"https://www.garlandtools.org/db/#item/{itemId}";
            System.Diagnostics.Process.Start("explorer", url);
            pricedItem.Result = ItemResult.Success;
        }
    }
}
