/// Copyright (C) 2012-2014 Soomla Inc.
///
/// Licensed under the Apache License, Version 2.0 (the "License");
/// you may not use this file except in compliance with the License.
/// You may obtain a copy of the License at
///
///      http://www.apache.org/licenses/LICENSE-2.0
///
/// Unless required by applicable law or agreed to in writing, software
/// distributed under the License is distributed on an "AS IS" BASIS,
/// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
/// See the License for the specific language governing permissions and
/// limitations under the License.

using UnityEngine;
using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Runtime.InteropServices;
#if UNITY_WP8
using SoomlaWpStore;
#endif

namespace Soomla.Store {

	/// <summary>
	/// This class provides functionality for event handling.
	/// </summary>
	public class StoreEvents : MonoBehaviour {

		private const string TAG = "SOOMLA StoreEvents";

#if UNITY_EDITOR
		public static StoreEvents Instance { get; private set; }
#else
		public static StoreEvents Instance = null;
#endif
		#pragma warning disable 0414
		private static StoreEventPusher sep = null;
		#pragma warning restore 0414

#if UNITY_IOS && !UNITY_EDITOR
		[DllImport ("__Internal")]
		private static extern void eventDispatcher_Init();
#endif


		/// <summary>
		/// Initializes StoreEvents before the game starts.
		/// </summary>
		void Awake(){
			if(Instance == null){ 	// making sure we only initialize one instance.
				Instance = this;
				GameObject.DontDestroyOnLoad(this.gameObject);
				Initialize();
			} else {				// Destroying unused instances.
				GameObject.Destroy(this.gameObject);
			}
		}

		public delegate void RunLaterDelegate();
		public void RunLater(RunLaterDelegate runLaterDelegate) {
			StartCoroutine(RunLaterPriv(runLaterDelegate));
		}
		private System.Collections.IEnumerator RunLaterPriv(RunLaterDelegate runLaterDelegate) {
			yield return new WaitForSeconds(0.1f);
			runLaterDelegate();
		}

		/// <summary>
		/// Initializes the different native event handlers in Android / iOS
		/// </summary>
		public static void Initialize() {
			SoomlaUtils.LogDebug (TAG, "Initializing StoreEvents ...");
#if UNITY_ANDROID && !UNITY_EDITOR
			AndroidJNI.PushLocalFrame(100);
			using(AndroidJavaClass jniEventHandler = new AndroidJavaClass("com.soomla.unity.StoreEventHandler")) {
				jniEventHandler.CallStatic("initialize");
			}
			AndroidJNI.PopLocalFrame(IntPtr.Zero);

			sep = new StoreEventPusherAndroid();
#elif UNITY_IOS && !UNITY_EDITOR
			eventDispatcher_Init();
			sep = new StoreEventPusherIOS();
            #elif UNITY_WP8
            SoomlaWpStore.StoreEvents.GetInstance().OnCurrencyBalanceChangedEvent += new SoomlaWpStore.CurrencyBalanceChangedEventHandler(onCurrencyBalanceChanged);
            SoomlaWpStore.StoreEvents.GetInstance().OnGoodBalanceChangedEvent += new SoomlaWpStore.GoodBalanceChangedEventHandler(onGoodBalanceChanged);
            SoomlaWpStore.StoreEvents.GetInstance().OnGoodEquippedEvent += new SoomlaWpStore.GoodEquippedEventHandler(onGoodEquipped);
            SoomlaWpStore.StoreEvents.GetInstance().OnGoodUnEquippedEvent += new SoomlaWpStore.GoodUnEquippedEventHandler(onGoodUnEquipped);

            SoomlaWpStore.StoreEvents.GetInstance().OnGoodUpgradeEvent += new SoomlaWpStore.GoodUpgradeEventHandler(onGoodUpgrade);
            SoomlaWpStore.StoreEvents.GetInstance().OnItemPurchasedEvent += new SoomlaWpStore.ItemPurchasedEventHandler(onItemPurchased);
            SoomlaWpStore.StoreEvents.GetInstance().OnItemPurchaseStartedEvent += new SoomlaWpStore.ItemPurchaseStartedEventHandler(onItemPurchaseStarted);
            SoomlaWpStore.StoreEvents.GetInstance().OnMarketPurchaseEvent += new SoomlaWpStore.MarketPurchaseEventHandler(onMarketPurchase);
            SoomlaWpStore.StoreEvents.GetInstance().OnMarketPurchaseCancelledEvent += new SoomlaWpStore.MarketPurchaseCancelledEventHandler(onMarketPurchaseCancelled);
            SoomlaWpStore.StoreEvents.GetInstance().OnMarketPurchaseStartedEvent += new SoomlaWpStore.MarketPurchaseStartedEventHandler(onMarketPurchaseStarted);
            //SoomlaWpStore.StoreEvents.GetInstance().OnMarketRefundEvent += new SoomlaWpStore.MarketRefundEventHandler(onMarketRefund);
            SoomlaWpStore.StoreEvents.GetInstance().OnRestoreTransactionsFinishedEvent += new SoomlaWpStore.RestoreTransactionsFinishedEventHandler(onRestoreTransactionsFinished);
            SoomlaWpStore.StoreEvents.GetInstance().OnRestoreTransactionsStartedEvent += new SoomlaWpStore.RestoreTransactionsStartedEventHandler(onRestoreTransactionsStarted);
            SoomlaWpStore.StoreEvents.GetInstance().OnMarketItemsRefreshStartedEvent += new SoomlaWpStore.MarketItemsRefreshStartedEventHandler(onMarketItemsRefreshStarted);
            SoomlaWpStore.StoreEvents.GetInstance().OnMarketItemsRefreshFinishedEvent += new SoomlaWpStore.MarketItemsRefreshFinishedEventHandler(onMarketItemsRefreshFinished);
            SoomlaWpStore.StoreEvents.GetInstance().OnUnexpectedStoreErrorEvent += new SoomlaWpStore.UnexpectedStoreErrorEventHandler(onUnexpectedErrorInStore);
            SoomlaWpStore.StoreEvents.GetInstance().OnSoomlaStoreInitializedEvent += new SoomlaWpStore.SoomlaStoreInitializedEventHandler(onSoomlaStoreInitialized);

#endif
        }

#if UNITY_WP8
        /// <summary>
        /// Handles an <c>onCurrencyBalanceChanged</c> event, which is fired when the balance of a specific
        /// <c>VirtualCurrency</c> has changed.
        /// </summary>
        /// <param name="message">Message that contains information about the currency whose balance has
        /// changed.</param>
        public static void onCurrencyBalanceChanged(SoomlaWpStore.domain.virtualCurrencies.VirtualCurrency currency, int balance, int amountAdded)
        {
            SoomlaUtils.LogDebug(TAG, "SOOMLA/UNITY onCurrencyBalanceChanged" + currency.getItemId() + " " + balance.ToString() + " " + amountAdded.ToString());

            VirtualCurrency vc = (VirtualCurrency)StoreInfo.GetItemByItemId(currency.getItemId());
            StoreEvents.OnCurrencyBalanceChanged(vc, balance, amountAdded);
        }

        /// <summary>
        /// Handles an <c>onGoodBalanceChanged</c> event, which is fired when the balance of a specific
        /// <c>VirtualGood</c> has changed.
        /// </summary>
        /// <param name="message">Message that contains information about the good whose balance has
        /// changed.</param>
        public static void onGoodBalanceChanged(SoomlaWpStore.domain.virtualGoods.VirtualGood good, int balance, int amountAdded)
        {
            SoomlaUtils.LogDebug(TAG, "SOOMLA/UNITY onGoodBalanceChanged:" + good.getItemId() + " " + balance.ToString() + " " + amountAdded.ToString());

            VirtualGood vg = (VirtualGood)StoreInfo.GetItemByItemId(good.getItemId());
            StoreEvents.OnGoodBalanceChanged(vg, balance, amountAdded);
        }

        /// <summary>
        /// Handles an <c>onGoodEquipped</c> event, which is fired when a specific <c>EquippableVG</c> has been
        /// equipped.
        /// </summary>
        /// <param name="message">Message that contains information about the <c>EquippableVG</c>.</param>
        public static void onGoodEquipped(SoomlaWpStore.domain.virtualGoods.EquippableVG good)
        {
            SoomlaUtils.LogDebug(TAG, "SOOMLA/UNITY onVirtualGoodEquipped:" + good.getItemId());

            EquippableVG vg = (EquippableVG)StoreInfo.GetItemByItemId(good.getItemId());
            StoreEvents.OnGoodEquipped(vg);
        }

        /// <summary>
        /// Handles an <c>onGoodUnequipped</c> event, which is fired when a specific <c>EquippableVG</c>
        /// has been unequipped.
        /// </summary>
        /// <param name="message">Message that contains information about the <c>EquippableVG</c>.</param>
        public static void onGoodUnEquipped(SoomlaWpStore.domain.virtualGoods.EquippableVG good)
        {
            SoomlaUtils.LogDebug(TAG, "SOOMLA/UNITY onVirtualGoodUnEquipped:" + good.getItemId());

            EquippableVG vg = (EquippableVG)StoreInfo.GetItemByItemId(good.getItemId());
            StoreEvents.OnGoodUnEquipped(vg);
        }

        /// <summary>
        /// Handles an <c>onGoodUpgrade</c> event, which is fired when a specific <c>UpgradeVG</c> has
        /// been upgraded/downgraded.
        /// </summary>
        /// <param name="message">Message that contains information about the good that has been
        /// upgraded/downgraded.</param>
        public static void onGoodUpgrade(SoomlaWpStore.domain.virtualGoods.VirtualGood good, SoomlaWpStore.domain.virtualGoods.UpgradeVG upgradeVG)
        {
            SoomlaUtils.LogDebug(TAG, "SOOMLA/UNITY onGoodUpgrade:" + good.getItemId() + " " + upgradeVG.getItemId());

            VirtualGood vg = (VirtualGood)StoreInfo.GetItemByItemId(good.getItemId());
            UpgradeVG vgu = null;
            if (upgradeVG != null)
            {
                vgu = (UpgradeVG)StoreInfo.GetItemByItemId(upgradeVG.getItemId());
            }
            StoreEvents.OnGoodUpgrade(vg, vgu);
        }

        /// <summary>
        /// Handles an <c>onItemPurchased</c> event, which is fired when a specific
        /// <c>PurchasableVirtualItem</c> has been purchased.
        /// </summary>
        /// <param name="message">Message that contains information about the good that has been purchased.</param>
        public static void onItemPurchased(SoomlaWpStore.domain.PurchasableVirtualItem purchasableVirtualItem, String payload)
        {
            SoomlaUtils.LogDebug(TAG, "SOOMLA/UNITY onItemPurchased:" + purchasableVirtualItem.getItemId() + " " +payload);
            PurchasableVirtualItem pvi = (PurchasableVirtualItem)StoreInfo.GetItemByItemId(purchasableVirtualItem.getItemId());
            StoreEvents.OnItemPurchased(pvi, payload);
        }

        /// <summary>
        /// Handles the <c>onItemPurchaseStarted</c> event, which is fired when a specific
        /// <c>PurchasableVirtualItem</c> purchase process has started.
        /// </summary>
        /// <param name="message">Message that contains information about the item being purchased.</param>
        public static void onItemPurchaseStarted(SoomlaWpStore.domain.PurchasableVirtualItem purchasableVirtualItem)
        {
            SoomlaUtils.LogDebug(TAG, "SOOMLA/UNITY onItemPurchaseStarted:" + purchasableVirtualItem.getItemId());

            PurchasableVirtualItem pvi = (PurchasableVirtualItem)StoreInfo.GetItemByItemId(purchasableVirtualItem.getItemId());
            StoreEvents.OnItemPurchaseStarted(pvi);
        }

        /// <summary>
        /// Handles the <c>onMarketPurchaseCancelled</c> event, which is fired when a Market purchase was cancelled
        /// by the user.
        /// </summary>
        /// <param name="message">Message that contains information about the market purchase that is being
        /// cancelled.</param>
        public static void onMarketPurchaseCancelled(SoomlaWpStore.domain.PurchasableVirtualItem purchasableVirtualItem)
        {
            SoomlaUtils.LogDebug(TAG, "SOOMLA/UNITY onMarketPurchaseCancelled: " + purchasableVirtualItem.getItemId());
            PurchasableVirtualItem pvi = (PurchasableVirtualItem)StoreInfo.GetItemByItemId(purchasableVirtualItem.getItemId());
            StoreEvents.OnMarketPurchaseCancelled(pvi);
        }

        /// <summary>
        /// Handles the <c>onMarketPurchase</c> event, which is fired when a Market purchase has occurred.
        /// </summary>
        /// <param name="message">Message that contains information about the market purchase.</param>
        public static void onMarketPurchase(SoomlaWpStore.domain.PurchasableVirtualItem purchasableVirtualItem, String payload, String token)
        {
            Debug.Log("SOOMLA/UNITY onMarketPurchase:" + purchasableVirtualItem.getItemId() + " " +payload+ " "+token);

            PurchasableVirtualItem pvi = (PurchasableVirtualItem)StoreInfo.GetItemByItemId(purchasableVirtualItem.getItemId());

            StoreEvents.OnMarketPurchase(pvi, token, payload,"");
        }

        /// <summary>
        /// Handles the <c>onMarketPurchaseStarted</c> event, which is fired when a Market purchase has started.
        /// </summary>
        /// <param name="message">Message that contains information about the maret purchase that is being
        /// started.</param>
        public static void onMarketPurchaseStarted(SoomlaWpStore.domain.PurchasableVirtualItem purchasableVirtualItem)
        {
            SoomlaUtils.LogDebug(TAG, "SOOMLA/UNITY onMarketPurchaseStarted: " + purchasableVirtualItem.getItemId());

            PurchasableVirtualItem pvi = (PurchasableVirtualItem)StoreInfo.GetItemByItemId(purchasableVirtualItem.getItemId());
            StoreEvents.OnMarketPurchaseStarted(pvi);
        }

        /// <summary>
        /// Handles the <c>onMarketRefund</c> event, which is fired when a Market refund has been issued.
        /// </summary>
        /// <param name="message">Message that contains information about the market refund that has occurred.</param>
        public static void onMarketRefund(string message)
        {
            SoomlaUtils.LogDebug(TAG, "SOOMLA/UNITY onMarketRefund NOT IMPLEMENTED");
/*
            PurchasableVirtualItem pvi = (PurchasableVirtualItem)StoreInfo.GetItemByItemId(message);
            StoreEvents.OnMarketPurchaseStarted(pvi);*/
        }

        
        /// <summary>
        /// Handles the <c>onRestoreTransactionsFinished</c> event, which is fired when the restore transactions
        /// process has finished.
        /// </summary>
        /// <param name="message">Message that contains information about the <c>restoreTransactions</c> process that
        /// has finished.</param>
        public static void onRestoreTransactionsFinished(bool success)
        {
            SoomlaUtils.LogDebug(TAG, "SOOMLA/UNITY onRestoreTransactionsFinished:" + success.ToString());
            StoreEvents.OnRestoreTransactionsFinished(success);
        }

        /// <summary>
        /// Handles the <c>onRestoreTransactionsStarted</c> event, which is fired when the restore transactions
        /// process has started.
        /// </summary>
        /// <param name="message">Message that contains information about the <c>restoreTransactions</c> process that
        /// has started.</param>
        public static void onRestoreTransactionsStarted()
        {
            SoomlaUtils.LogDebug(TAG, "SOOMLA/UNITY onRestoreTransactionsStarted");

            StoreEvents.OnRestoreTransactionsStarted();
        }

        /// <summary>
        /// Handles the <c>onMarketItemsRefreshStarted</c> event, which is fired when items associated with market
        /// refresh process has started.
        /// </summary>
        /// <param name="message">Message that contains information about the <c>market refresh</c> process that
        /// has started.</param>
        public static void onMarketItemsRefreshStarted()
        {
            SoomlaUtils.LogDebug(TAG, "SOOMLA/UNITY onMarketItemsRefreshStarted");

            StoreEvents.OnMarketItemsRefreshStarted();
        }

        /// <summary>
        /// Handles the <c>onMarketItemsRefreshFinished</c> event, which is fired when items associated with market are
        /// refreshed (prices, titles ...).
        /// </summary>
        /// <param name="message">Message that contains information about the process that is occurring.</param>
        public static void onMarketItemsRefreshFinished(List<SoomlaWpStore.domain.MarketItem> pluginMarketItems)
        {
            SoomlaUtils.LogDebug(TAG, "SOOMLA/UNITY onMarketItemsRefreshFinished: " + pluginMarketItems.Count);

            List<MarketItem> marketItems = new List<MarketItem>();
            foreach (SoomlaWpStore.domain.MarketItem mic in pluginMarketItems)
            {
                try
                {
                    PurchasableVirtualItem pvi = StoreInfo.GetPurchasableItemWithProductId(mic.getProductId());
                    MarketItem mi = ((PurchaseWithMarket)pvi.PurchaseType).MarketItem;
                    mi.MarketPrice = mic.getMarketPrice();
                    mi.MarketTitle = mic.getMarketTitle();
                    mi.MarketDescription = mic.getMarketDescription();
                    pvi.save();

                    marketItems.Add(mi);
                }
                catch (VirtualItemNotFoundException ex)
                {
                    SoomlaUtils.LogDebug(TAG, ex.Message);
                }
            }

            StoreEvents.OnMarketItemsRefreshFinished(marketItems);
        }

        /// <summary>
        /// Handles the <c>onItemPurchaseStarted</c> event, which is fired when an unexpected/unrecognized error
        /// occurs in store.
        /// </summary>
        /// <param name="message">Message that contains information about the error.</param>
        public static void onUnexpectedErrorInStore(string message)
        {
            SoomlaUtils.LogDebug(TAG, "SOOMLA/UNITY onUnexpectedErrorInStore" + message);

            StoreEvents.OnUnexpectedErrorInStore(message);
        }

        /// <summary>
        /// Handles the <c>onSoomlaStoreInitialized</c> event, which is fired when <c>SoomlaStore</c>
        /// is initialized.
        /// </summary>
        /// <param name="message">Not used here.</param>
        public static void onSoomlaStoreInitialized()
        {
            SoomlaUtils.LogDebug(TAG, "SOOMLA/UNITY onSoomlaStoreInitialized");

            StoreEvents.OnSoomlaStoreInitialized();
        }

#else
		/// <summary>
		/// Handles an <c>onBillingSupported</c> event, which is fired when SOOMLA knows that billing IS
		/// supported on the device.
		/// </summary>
		/// <param name="message">Not used here.</param>
		public void onBillingSupported(string message) {
			SoomlaUtils.LogDebug(TAG, "SOOMLA/UNITY onBillingSupported");

			StoreEvents.OnBillingSupported();
		}

		/// <summary>
		/// Handles an <c>onBillingNotSupported</c> event, which is fired when SOOMLA knows that billing is NOT
		/// supported on the device.
		/// </summary>
		/// <param name="message">Not used here.</param>
		public void onBillingNotSupported(string message) {
			SoomlaUtils.LogDebug(TAG, "SOOMLA/UNITY onBillingNotSupported");

			StoreEvents.OnBillingNotSupported();
		}

		/// <summary>
		/// Handles an <c>onCurrencyBalanceChanged</c> event, which is fired when the balance of a specific
		/// <c>VirtualCurrency</c> has changed.
		/// </summary>
		/// <param name="message">Message that contains information about the currency whose balance has
		/// changed.</param>
		public void onCurrencyBalanceChanged(string message) {
			onCurrencyBalanceChanged(message, false);
		}
		public void onCurrencyBalanceChanged(string message, bool alsoPush) {
			SoomlaUtils.LogDebug(TAG, "SOOMLA/UNITY onCurrencyBalanceChanged:" + message);

			JSONObject eventJSON = new JSONObject(message);

			VirtualCurrency vc = (VirtualCurrency)StoreInfo.GetItemByItemId(eventJSON["itemId"].str);
			int balance = (int)eventJSON["balance"].n;
			int amountAdded = (int)eventJSON["amountAdded"].n;

			StoreInventory.RefreshOnCurrencyBalanceChanged(vc, balance, amountAdded);

			StoreEvents.OnCurrencyBalanceChanged(vc, balance, amountAdded);

			if (alsoPush) {
#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
				sep.PushEventOnCurrencyBalanceChanged(vc, balance, amountAdded);
#endif
			}
		}

		/// <summary>
		/// Handles an <c>onGoodBalanceChanged</c> event, which is fired when the balance of a specific
		/// <c>VirtualGood</c> has changed.
		/// </summary>
		/// <param name="message">Message that contains information about the good whose balance has
		/// changed.</param>
		public void onGoodBalanceChanged(string message) {
			onGoodBalanceChanged(message, false);
		}
		public void onGoodBalanceChanged(string message, bool alsoPush) {
			SoomlaUtils.LogDebug(TAG, "SOOMLA/UNITY onGoodBalanceChanged:" + message);

			JSONObject eventJSON = new JSONObject(message);

			VirtualGood vg = (VirtualGood)StoreInfo.GetItemByItemId(eventJSON["itemId"].str);
			int balance = (int)eventJSON["balance"].n;
			int amountAdded = (int)eventJSON["amountAdded"].n;

			StoreInventory.RefreshOnGoodBalanceChanged(vg, balance, amountAdded);

			StoreEvents.OnGoodBalanceChanged(vg, balance, amountAdded);

			if (alsoPush) {
#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
				sep.PushEventOnGoodBalanceChanged(vg, balance, amountAdded);
#endif
			}
		}

		/// <summary>
		/// Handles an <c>onGoodEquipped</c> event, which is fired when a specific <c>EquippableVG</c> has been
		/// equipped.
		/// </summary>
		/// <param name="message">Message that contains information about the <c>EquippableVG</c>.</param>
		public void onGoodEquipped(string message) {
			onGoodEquipped(message, false);
		}
		public void onGoodEquipped(string message, bool alsoPush) {
			SoomlaUtils.LogDebug(TAG, "SOOMLA/UNITY onVirtualGoodEquipped:" + message);

			var eventJSON = new JSONObject(message);

			EquippableVG vg = (EquippableVG)StoreInfo.GetItemByItemId(eventJSON["itemId"].str);

			StoreInventory.RefreshOnGoodEquipped(vg);

			StoreEvents.OnGoodEquipped(vg);

			if (alsoPush) {
#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
				sep.PushEventOnGoodEquipped(vg);
#endif
			}
		}

		/// <summary>
		/// Handles an <c>onGoodUnequipped</c> event, which is fired when a specific <c>EquippableVG</c>
		/// has been unequipped.
		/// </summary>
		/// <param name="message">Message that contains information about the <c>EquippableVG</c>.</param>
		public void onGoodUnequipped(string message) {
			onGoodUnequipped(message, false);
		}
		public void onGoodUnequipped(string message, bool alsoPush) {
			SoomlaUtils.LogDebug(TAG, "SOOMLA/UNITY onVirtualGoodUnEquipped:" + message);

			var eventJSON = new JSONObject(message);

			EquippableVG vg = (EquippableVG)StoreInfo.GetItemByItemId(eventJSON["itemId"].str);

			StoreInventory.RefreshOnGoodUnEquipped(vg);

			StoreEvents.OnGoodUnEquipped(vg);

			if (alsoPush) {
#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
				sep.PushEventOnGoodUnequipped(vg);
#endif
			}
		}

		/// <summary>
		/// Handles an <c>onGoodUpgrade</c> event, which is fired when a specific <c>UpgradeVG</c> has
		/// been upgraded/downgraded.
		/// </summary>
		/// <param name="message">Message that contains information about the good that has been
		/// upgraded/downgraded.</param>
		public void onGoodUpgrade(string message) {
			onGoodUpgrade(message, false);
		}
		public void onGoodUpgrade(string message, bool alsoPush) {
			SoomlaUtils.LogDebug(TAG, "SOOMLA/UNITY onGoodUpgrade:" + message);

			var eventJSON = new JSONObject(message);

			VirtualGood vg = (VirtualGood)StoreInfo.GetItemByItemId(eventJSON["itemId"].str);
			UpgradeVG vgu = null;
			if (eventJSON.HasField("upgradeItemId") && !string.IsNullOrEmpty(eventJSON["upgradeItemId"].str)) {
				vgu = (UpgradeVG)StoreInfo.GetItemByItemId(eventJSON["upgradeItemId"].str);
		  	}

			StoreInventory.RefreshOnGoodUpgrade(vg, vgu);

			StoreEvents.OnGoodUpgrade(vg, vgu);

			if (alsoPush) {
#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
				sep.PushEventOnGoodUpgrade(vg, vgu);
#endif
			}
		}

		/// <summary>
		/// Handles an <c>onItemPurchased</c> event, which is fired when a specific
		/// <c>PurchasableVirtualItem</c> has been purchased.
		/// </summary>
		/// <param name="message">Message that contains information about the good that has been purchased.</param>
		public void onItemPurchased(string message) {
			onItemPurchased(message, false);
		}
		public void onItemPurchased(string message, bool alsoPush) {
			SoomlaUtils.LogDebug(TAG, "SOOMLA/UNITY onItemPurchased:" + message);

			var eventJSON = new JSONObject(message);

			PurchasableVirtualItem pvi = (PurchasableVirtualItem)StoreInfo.GetItemByItemId(eventJSON["itemId"].str);
			string payload = "";
			if (eventJSON.HasField("payload")) {
				payload = eventJSON["payload"].str;
			}

			StoreEvents.OnItemPurchased(pvi, payload);

			if (alsoPush) {
#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
				sep.PushEventOnItemPurchased(pvi, payload);
#endif
			}
		}

		/// <summary>
		/// Handles the <c>onItemPurchaseStarted</c> event, which is fired when a specific
		/// <c>PurchasableVirtualItem</c> purchase process has started.
		/// </summary>
		/// <param name="message">Message that contains information about the item being purchased.</param>
		public void onItemPurchaseStarted(string message) {
			onItemPurchaseStarted(message, false);
		}
		public void onItemPurchaseStarted(string message, bool alsoPush) {
			SoomlaUtils.LogDebug(TAG, "SOOMLA/UNITY onItemPurchaseStarted:" + message);

			var eventJSON = new JSONObject(message);

			PurchasableVirtualItem pvi = (PurchasableVirtualItem)StoreInfo.GetItemByItemId(eventJSON["itemId"].str);
			StoreEvents.OnItemPurchaseStarted(pvi);

			if (alsoPush) {
#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
				sep.PushEventOnItemPurchaseStarted(pvi);
#endif
			}
		}

		/// <summary>
		/// Handles the <c>onMarketPurchaseCancelled</c> event, which is fired when a Market purchase was cancelled
		/// by the user.
		/// </summary>
		/// <param name="message">Message that contains information about the market purchase that is being
		/// cancelled.</param>
		public void onMarketPurchaseCancelled(string message) {
			SoomlaUtils.LogDebug(TAG, "SOOMLA/UNITY onMarketPurchaseCancelled: " + message);

			var eventJSON = new JSONObject(message);

			PurchasableVirtualItem pvi = (PurchasableVirtualItem)StoreInfo.GetItemByItemId(eventJSON["itemId"].str);
			StoreEvents.OnMarketPurchaseCancelled(pvi);
		}

		/// <summary>
		/// Handles the <c>onMarketPurchase</c> event, which is fired when a Market purchase has occurred.
		/// </summary>
		/// <param name="message">Message that contains information about the market purchase.</param>
		public void onMarketPurchase(string message) {
			Debug.Log ("SOOMLA/UNITY onMarketPurchase:" + message);

			var eventJSON = new JSONObject(message);

			PurchasableVirtualItem pvi = (PurchasableVirtualItem)StoreInfo.GetItemByItemId(eventJSON["itemId"].str);
			string payload = "";
			var extra = new Dictionary<string, string>();
			if (eventJSON.HasField("payload")) {
				payload = eventJSON["payload"].str;
			}
			if (eventJSON.HasField("extra")) {
				var extraJSON = eventJSON["extra"];
				if (extraJSON.keys != null) {
					foreach(string key in extraJSON.keys) {
						if (extraJSON[key] != null) {
							extra.Add(key, extraJSON[key].str);
						}
					}
				}
			}

			StoreEvents.OnMarketPurchase(pvi, payload, extra);
		}

		/// <summary>
		/// Handles the <c>onMarketPurchaseStarted</c> event, which is fired when a Market purchase has started.
		/// </summary>
		/// <param name="message">Message that contains information about the maret purchase that is being
		/// started.</param>
		public void onMarketPurchaseStarted(string message) {
			SoomlaUtils.LogDebug(TAG, "SOOMLA/UNITY onMarketPurchaseStarted: " + message);

			var eventJSON = new JSONObject(message);

			PurchasableVirtualItem pvi = (PurchasableVirtualItem)StoreInfo.GetItemByItemId(eventJSON["itemId"].str);
			StoreEvents.OnMarketPurchaseStarted(pvi);
		}

		/// <summary>
		/// Handles the <c>onMarketRefund</c> event, which is fired when a Market refund has been issued.
		/// </summary>
		/// <param name="message">Message that contains information about the market refund that has occurred.</param>
		public void onMarketRefund(string message) {
			SoomlaUtils.LogDebug(TAG, "SOOMLA/UNITY onMarketRefund:" + message);

			var eventJSON = new JSONObject(message);

			PurchasableVirtualItem pvi = (PurchasableVirtualItem)StoreInfo.GetItemByItemId(eventJSON["itemId"].str);
			StoreEvents.OnMarketRefund(pvi);
		}

		/// <summary>
		/// Handles the <c>onRestoreTransactionsFinished</c> event, which is fired when the restore transactions
		/// process has finished.
		/// </summary>
		/// <param name="message">Message that contains information about the <c>restoreTransactions</c> process that
		/// has finished.</param>
		public void onRestoreTransactionsFinished(string message) {
			SoomlaUtils.LogDebug(TAG, "SOOMLA/UNITY onRestoreTransactionsFinished:" + message);

			var eventJSON = new JSONObject(message);

			bool success = eventJSON["success"].b;
			StoreEvents.OnRestoreTransactionsFinished(success);
		}

		/// <summary>
		/// Handles the <c>onRestoreTransactionsStarted</c> event, which is fired when the restore transactions
		/// process has started.
		/// </summary>
		/// <param name="message">Message that contains information about the <c>restoreTransactions</c> process that
		/// has started.</param>
		public void onRestoreTransactionsStarted(string message) {
			SoomlaUtils.LogDebug(TAG, "SOOMLA/UNITY onRestoreTransactionsStarted");

			StoreEvents.OnRestoreTransactionsStarted();
		}

		/// <summary>
		/// Handles the <c>onMarketItemsRefreshStarted</c> event, which is fired when items associated with market
		/// refresh process has started.
		/// </summary>
		/// <param name="message">Message that contains information about the <c>market refresh</c> process that
		/// has started.</param>
		public void onMarketItemsRefreshStarted(string message) {
			SoomlaUtils.LogDebug(TAG, "SOOMLA/UNITY onMarketItemsRefreshStarted");

			StoreEvents.OnMarketItemsRefreshStarted();
		}

		/// <summary>
		/// Handles the <c>onMarketItemsRefreshFailed</c> event, which is fired when items associated with market
		/// refresh process has failed.
		/// </summary>
		/// <param name="message">Message that contains information about the <c>market refresh</c> process that
		/// has failed.</param>
		public void onMarketItemsRefreshFailed(string message) {
			SoomlaUtils.LogDebug(TAG, "SOOMLA/UNITY onMarketItemsRefreshFailed");

			var eventJSON = new JSONObject(message);
			
			string errorMessage = eventJSON["errorMessage"].str;
			StoreEvents.OnMarketItemsRefreshFailed(errorMessage);
		}

		/// <summary>
		/// Handles the <c>onMarketItemsRefreshFinished</c> event, which is fired when items associated with market are
		/// refreshed (prices, titles ...).
		/// </summary>
		/// <param name="message">Message that contains information about the process that is occurring.</param>
		public void onMarketItemsRefreshFinished(string message) {
			SoomlaUtils.LogDebug(TAG, "SOOMLA/UNITY onMarketItemsRefreshFinished: " + message);

			var eventJSON = new JSONObject(message);

			List<VirtualItem> virtualItems = new List<VirtualItem>();
			List<MarketItem> marketItems = new List<MarketItem>();
			foreach (var micJSON in eventJSON.list) {
				string productId = micJSON[StoreJSONConsts.MARKETITEM_PRODUCT_ID].str;
				string marketPrice = micJSON[StoreJSONConsts.MARKETITEM_MARKETPRICE].str;
				string marketTitle = micJSON[StoreJSONConsts.MARKETITEM_MARKETTITLE].str;
				string marketDescription = micJSON[StoreJSONConsts.MARKETITEM_MARKETDESC].str;
				string marketCurrencyCode = micJSON[StoreJSONConsts.MARKETITEM_MARKETCURRENCYCODE].str;
				long marketPriceMicros = System.Convert.ToInt64(micJSON[StoreJSONConsts.MARKETITEM_MARKETPRICEMICROS].n);
				try {
					PurchasableVirtualItem pvi = StoreInfo.GetPurchasableItemWithProductId(productId);
					MarketItem mi = ((PurchaseWithMarket)pvi.PurchaseType).MarketItem;
					mi.MarketPriceAndCurrency = marketPrice;
					mi.MarketTitle = marketTitle;
					mi.MarketDescription = marketDescription;
					mi.MarketCurrencyCode = marketCurrencyCode;
					mi.MarketPriceMicros = marketPriceMicros;

					marketItems.Add(mi);
					virtualItems.Add(pvi);
				} catch (VirtualItemNotFoundException ex){
					SoomlaUtils.LogDebug(TAG, ex.Message);
				}
			}

			if (virtualItems.Count > 0) {
				// no need to save to DB since it's already saved in native
				// before this event is received
				StoreInfo.Save(virtualItems, false);
			}

			StoreEvents.OnMarketItemsRefreshFinished(marketItems);
		}

		/// <summary>
		/// Handles the <c>onItemPurchaseStarted</c> event, which is fired when an unexpected/unrecognized error
		/// occurs in store.
		/// </summary>
		/// <param name="message">Message that contains information about the error.</param>
		public void onUnexpectedErrorInStore(string message) {
			onUnexpectedErrorInStore(message, false);
		}
		public void onUnexpectedErrorInStore(string message, bool alsoPush) {
			SoomlaUtils.LogDebug(TAG, "SOOMLA/UNITY onUnexpectedErrorInStore");

			StoreEvents.OnUnexpectedErrorInStore(message);

			if (alsoPush) {
#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
				sep.PushEventSoomlaStoreInitialized();
#endif
			}
		}

		/// <summary>
		/// Handles the <c>onSoomlaStoreInitialized</c> event, which is fired when <c>SoomlaStore</c>
		/// is initialized.
		/// </summary>
		/// <param name="message">Not used here.</param>
		public void onSoomlaStoreInitialized(string message) {
			onSoomlaStoreInitialized(message, false);
		}
		public void onSoomlaStoreInitialized(string message, bool alsoPush) {
			SoomlaUtils.LogDebug(TAG, "SOOMLA/UNITY onSoomlaStoreInitialized");

			StoreInventory.RefreshLocalInventory();

			StoreEvents.OnSoomlaStoreInitialized();

			if (alsoPush) {
#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
				sep.PushEventSoomlaStoreInitialized();
#endif
			}
		}
#endif

#if UNITY_ANDROID && !UNITY_EDITOR
		public void onIabServiceStarted(string message) {
			SoomlaUtils.LogDebug(TAG, "SOOMLA/UNITY onIabServiceStarted");

			StoreEvents.OnIabServiceStarted();
		}

		public void onIabServiceStopped(string message) {
			SoomlaUtils.LogDebug(TAG, "SOOMLA/UNITY onIabServiceStopped");

			StoreEvents.OnIabServiceStopped();
		}
#endif


		public delegate void Action();

		public static Action OnBillingNotSupported = delegate {};

		public static Action OnBillingSupported = delegate {};

		public static Action<VirtualCurrency, int, int> OnCurrencyBalanceChanged = delegate {};

		public static Action<VirtualGood, int, int> OnGoodBalanceChanged = delegate {};

		public static Action<EquippableVG> OnGoodEquipped = delegate {};

		public static Action<EquippableVG> OnGoodUnEquipped = delegate {};

		public static Action<VirtualGood, UpgradeVG> OnGoodUpgrade = delegate {};

		public static Action<PurchasableVirtualItem, string> OnItemPurchased = delegate {};

		public static Action<PurchasableVirtualItem> OnItemPurchaseStarted = delegate {};

		public static Action<PurchasableVirtualItem> OnMarketPurchaseCancelled = delegate {};

		public static Action<PurchasableVirtualItem, string, Dictionary<string, string>> OnMarketPurchase = delegate {};

		public static Action<PurchasableVirtualItem> OnMarketPurchaseStarted = delegate {};

		public static Action<PurchasableVirtualItem> OnMarketRefund = delegate {};

		public static Action<bool> OnRestoreTransactionsFinished = delegate {};

		public static Action OnRestoreTransactionsStarted = delegate {};

		public static Action OnMarketItemsRefreshStarted = delegate {};

		public static Action<string> OnMarketItemsRefreshFailed = delegate {};

		public static Action<List<MarketItem>> OnMarketItemsRefreshFinished = delegate {};

		public static Action<string> OnUnexpectedErrorInStore = delegate {};

		public static Action OnSoomlaStoreInitialized = delegate {};

#if UNITY_ANDROID && !UNITY_EDITOR
		public static Action OnIabServiceStarted = delegate {};

		public static Action OnIabServiceStopped = delegate {};
#endif

		public class StoreEventPusher {
#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
			public StoreEventPusher() {}

			public void PushEventSoomlaStoreInitialized() {
				_pushEventSoomlaStoreInitialized("");
			}
			public void PushEventUnexpectedStoreError(string message) {
				_pushEventUnexpectedStoreError(message);
			}
			public void PushEventOnCurrencyBalanceChanged(VirtualCurrency currency, int balance, int amountAdded) {
				var eventJSON = new JSONObject();
				eventJSON.AddField("itemId", currency.ItemId);
				eventJSON.AddField("balance", balance);
				eventJSON.AddField("amountAdded", amountAdded);

				_pushEventCurrencyBalanceChanged(eventJSON.print());
			}
			public void PushEventOnGoodBalanceChanged(VirtualGood good, int balance, int amountAdded) {
				var eventJSON = new JSONObject();
				eventJSON.AddField("itemId", good.ItemId);
				eventJSON.AddField("balance", balance);
				eventJSON.AddField("amountAdded", amountAdded);

				_pushEventGoodBalanceChanged(eventJSON.print());
			}
			public void PushEventOnGoodEquipped(EquippableVG good) {
				var eventJSON = new JSONObject();
				eventJSON.AddField("itemId", good.ItemId);

				_pushEventGoodEquipped(eventJSON.print());
			}
			public void PushEventOnGoodUnequipped(EquippableVG good) {
				var eventJSON = new JSONObject();
				eventJSON.AddField("itemId", good.ItemId);

				_pushEventGoodUnequipped(eventJSON.print());
			}
			public void PushEventOnGoodUpgrade(VirtualGood good, UpgradeVG upgrade) {
				var eventJSON = new JSONObject();
				eventJSON.AddField("itemId", good.ItemId);
				eventJSON.AddField("upgradeItemId", (upgrade==null ? null : upgrade.ItemId));

				_pushEventGoodUpgrade(eventJSON.print());
			}
			public void PushEventOnItemPurchased(PurchasableVirtualItem item, string payload) {
				var eventJSON = new JSONObject();
				eventJSON.AddField("itemId", item.ItemId);
				eventJSON.AddField("payload", payload);

				_pushEventItemPurchased(eventJSON.print());
			}
			public void PushEventOnItemPurchaseStarted(PurchasableVirtualItem item) {
				var eventJSON = new JSONObject();
				eventJSON.AddField("itemId", item.ItemId);

				_pushEventItemPurchaseStarted(eventJSON.print());
			}


			// Event pushing back to native
			protected virtual void _pushEventSoomlaStoreInitialized(string message) {}
			protected virtual void _pushEventUnexpectedStoreError(string message) {}
			protected virtual void _pushEventCurrencyBalanceChanged(string message) {}
			protected virtual void _pushEventGoodBalanceChanged(string message) {}
			protected virtual void _pushEventGoodEquipped(string message) {}
			protected virtual void _pushEventGoodUnequipped(string message) {}
			protected virtual void _pushEventGoodUpgrade(string message) {}
			protected virtual void _pushEventItemPurchased(string message) {}
			protected virtual void _pushEventItemPurchaseStarted(string message) {}
#endif
		}


	}
}
