/*
 * PhoneGap is available under *either* the terms of the modified BSD license *or* the
 * MIT License (2008). See http://opensource.org/licenses/alphabetical for full text.
 *
 * Copyright (c) 2005-2011, Nitobi Software Inc.
 * Copyright (c) 2011, Microsoft Corporation
 */

using System;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Media;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using WPCordovaClassLib.Cordova;
using WPCordovaClassLib.Cordova.Commands;
using WPCordovaClassLib.Cordova.JSON;

namespace Cordova.Extension.Commands
{
    /// <summary>
    /// Implements access to application live tileTypes
    /// http://msdn.microsoft.com/en-us/library/hh202948(v=VS.92).aspx
    /// </summary>
    public class LiveTiles : BaseCommand
    {       
        #region fields and properties
        
        /// <summary>
        /// Name for app settings to store tile types
        /// </summary>
        private const string TileSettingsName = "LiveTileList";

        /// <summary>
        /// Stores type for created tiles
        /// </summary>
        private static Dictionary<string, TileType> tileTypes = new Dictionary<string, TileType>();

        #endregion

        #region tile types
        /// <summary>
        /// Tile type codes
        /// </summary>
        public enum TileType
        {
            Flip = 0,
            Cycle = 1,
            Iconic = 2
        }

        #endregion


        #region Live tileTypes options

        /// <summary>
        /// Represents LiveTile options
        /// </summary>
        [DataContract]
        public class LiveTilesOptions
        {
            /// <summary>
            /// Tile title
            /// </summary>
            [DataMember(IsRequired=false, Name="title")]
            public string Title { get; set; }

            /// <summary>
            /// Tile count
            /// </summary>
            [DataMember(IsRequired = false, Name = "count")]
            public int Count { get; set; }

            /// <summary>
            /// Tile image
            /// </summary>
            [DataMember(IsRequired = false, Name = "image")]
            public string Image { get; set; }

            /// <summary>
            /// Back tile title
            /// </summary>
            [DataMember(IsRequired = false, Name = "backTitle")]
            public string BackTitle { get; set; }

            /// <summary>
            /// Back tile content
            /// </summary>
            [DataMember(IsRequired = false, Name = "backContent")]
            public string BackContent { get; set; }

            /// <summary>
            /// Back tile image
            /// </summary>
            [DataMember(IsRequired = false, Name = "backImage")]
            public string BackImage { get; set; }

            /// <summary>
            /// Identifier for second tile
            /// </summary>
            [DataMember(IsRequired = false, Name = "secondaryTileUri")]
            public string SecondaryTileUri { get; set; }

            /// <summary>
            /// Small tile image
            /// </summary>
            [DataMember(IsRequired = false, Name = "smallImage")]
            public string SmallImage { get; set; }

            /// <summary>
            /// Wide tile image
            /// </summary>
            [DataMember(IsRequired = false, Name = "wideImage")]
            public string WideImage { get; set; }

            /// <summary>
            /// Wide tile back image
            /// </summary>
            [DataMember(IsRequired = false, Name = "wideBackImage")]
            public string WideBackImage { get; set; }

            /// <summary>
            /// Wide tile content
            /// </summary>
            [DataMember(IsRequired = false, Name = "wideContent")]
            public string WideContent { get; set; }

            /// <summary>
            /// Tile background color
            /// </summary>
            [DataMember(IsRequired = false, Name = "backgroundColor")]
            public string BackgroundColor { get; set; }

            /// <summary>
            /// Tile type
            /// </summary>
            [DataMember(IsRequired = false, Name = "tileType")]
            public TileType TileType { get; set; }

        }
        #endregion

        /// <summary>
        /// Updates application live tile
        /// </summary>
        public void updateAppTile(string options)
        {
            LiveTilesOptions liveTileOptions;
            try
            {
                liveTileOptions = JsonHelper.Deserialize<LiveTilesOptions>(options);
            }
            catch (Exception)
            {
                DispatchCommandResult(new PluginResult(PluginResult.Status.JSON_EXCEPTION));
                return;
            }

            try
            {
                ShellTile appTile = ShellTile.ActiveTiles.First();

                if (appTile != null)
                {
                    FlipTileData flipTile = CreateFlipTileDate(liveTileOptions);
                    appTile.Update(flipTile);
                    DispatchCommandResult(new PluginResult(PluginResult.Status.OK));
                }
                else
                {
                    DispatchCommandResult(new PluginResult(PluginResult.Status.ERROR, "Can't get application tile"));
                }
            }
            catch(Exception)
            {
                DispatchCommandResult(new PluginResult(PluginResult.Status.ERROR, "Error updating application tile"));
            }
        }

        /// <summary>
        /// Creates secondary tile
        /// </summary>
        public void createSecondaryTile(string options)
        {
            LiveTilesOptions tileOptions;

            if (!this.TryGetTileOptions(options, out tileOptions))
            {
                return;
            }

            this.CreateTile(tileOptions);
        }

        /// <summary>
        /// Updates secondary tile
        /// </summary>
        public void updateSecondaryTile(string options)
        {
            LiveTilesOptions liveTileOptions;
            try
            {
                liveTileOptions = JsonHelper.Deserialize<LiveTilesOptions>(options);
            }
            catch (Exception)
            {
                DispatchCommandResult(new PluginResult(PluginResult.Status.JSON_EXCEPTION));
                return;
            }

            if (string.IsNullOrEmpty(liveTileOptions.SecondaryTileUri))
            {
                DispatchCommandResult(new PluginResult(PluginResult.Status.JSON_EXCEPTION));
                return;
            }

            try
            {
                ShellTile foundTile = ShellTile.ActiveTiles.FirstOrDefault(x => x.NavigationUri.ToString().Contains(liveTileOptions.SecondaryTileUri));

                if (foundTile != null)
                {
                    TileType tileType = TileType.Flip;
                    
                    if (AppSettings.TryGetSetting(TileSettingsName,out tileTypes))
                    {
                        if (tileTypes.ContainsKey(foundTile.NavigationUri.OriginalString))
                        {
                            tileType = tileTypes[foundTile.NavigationUri.OriginalString];    
                        }
                    }
                                        
                    ShellTileData liveTile;

                    switch ((int)tileType)
                    {                        
                        case 1:
                            liveTile = CreateCycleTileData(liveTileOptions);
                            break;
                        case 2:
                            liveTile = CreateIconicTileData(liveTileOptions);
                            break;
                        default:
                            liveTile = CreateFlipTileDate(liveTileOptions);
                            break;
                    }                    

                    foundTile.Update(liveTile);
                    DispatchCommandResult(new PluginResult(PluginResult.Status.OK));
                }
                else
                {
                    DispatchCommandResult(new PluginResult(PluginResult.Status.ERROR, "Can't get secondary live tile"));
                }
            }
            catch (Exception)
            {
                DispatchCommandResult(new PluginResult(PluginResult.Status.ERROR,"Error updating secondary live tile"));
            }
        }

        /// <summary>
        /// Deletes secondary tile
        /// </summary>
        public void deleteSecondaryTile(string options)
        {
            LiveTilesOptions liveTileOptions;
            try
            {
                liveTileOptions = JsonHelper.Deserialize<LiveTilesOptions>(options);
            }
            catch (Exception)
            {
                DispatchCommandResult(new PluginResult(PluginResult.Status.JSON_EXCEPTION));
                return;
            }

            if (string.IsNullOrEmpty(liveTileOptions.SecondaryTileUri))
            {
                DispatchCommandResult(new PluginResult(PluginResult.Status.JSON_EXCEPTION));
                return;
            }
            try
            {
                ShellTile foundTile = ShellTile.ActiveTiles.FirstOrDefault(x => x.NavigationUri.ToString().Contains(liveTileOptions.SecondaryTileUri));
                if (foundTile != null)
                {
                    foundTile.Delete();
                    tileTypes.Remove(foundTile.NavigationUri.OriginalString);
                    AppSettings.StoreSetting(TileSettingsName, tileTypes);
                    DispatchCommandResult(new PluginResult(PluginResult.Status.OK));
                }
                else
                {
                    DispatchCommandResult(new PluginResult(PluginResult.Status.ERROR, "Can't get secondary live tile"));
                }   
            }
            catch (Exception)
            {
                DispatchCommandResult(new PluginResult(PluginResult.Status.ERROR, "Error deleting secondary live tile"));
            }
        }     

        
        
        /// <summary>
        /// Creates flip tile.
        /// </summary>
        /// <param name="options">tile options.</param>
        public void createFlipTile(string options)
        {
            LiveTilesOptions tileOptions;

            if (!this.TryGetTileOptions(options, out tileOptions))
            {
                return;
            }

            if (string.IsNullOrEmpty(tileOptions.Title) || string.IsNullOrEmpty(tileOptions.Image) || string.IsNullOrEmpty(tileOptions.SecondaryTileUri))
            {
                DispatchCommandResult(new PluginResult(PluginResult.Status.JSON_EXCEPTION));
                return;
            }

            tileOptions.TileType = TileType.Flip;
            this.CreateTile(tileOptions);
        }

        /// <summary>
        /// Creates cycle tile.
        /// </summary>
        /// <param name="options">tile options.</param>
        public void createCycleTile(string options)
        {
            LiveTilesOptions tileOptions;

            if (!this.TryGetTileOptions(options, out tileOptions))
            {
                return;
            }

            tileOptions.TileType = TileType.Cycle;
            this.CreateTile(tileOptions);
        }

        /// <summary>
        /// Creates iconic tile.
        /// </summary>
        /// <param name="options">tile options</param>
        public void createIconicTile(string options)
        {
            LiveTilesOptions tileOptions;

            if (!this.TryGetTileOptions(options, out tileOptions))
            {
                return;
            }

            tileOptions.TileType = TileType.Iconic;
            this.CreateTile(tileOptions);
        }       

        /// <summary>
        /// Creates tile
        /// </summary>
        private void CreateTile(LiveTilesOptions liveTileOptions)
        {
            if (string.IsNullOrEmpty(liveTileOptions.SecondaryTileUri))
            {
                DispatchCommandResult(new PluginResult(PluginResult.Status.JSON_EXCEPTION));
                return;
            }

            try
            {
                ShellTile foundTile = ShellTile.ActiveTiles.FirstOrDefault(x => x.NavigationUri.ToString().Contains(liveTileOptions.SecondaryTileUri));
                if (foundTile != null)
                {
                    DispatchCommandResult(new PluginResult(PluginResult.Status.ERROR, "Tile already exist"));
                }
                else
                {
                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
                        ShellTileData liveTile;

                        switch ((int)liveTileOptions.TileType)
                        {
                            case 1:
                                liveTile = CreateCycleTileData(liveTileOptions);
                                break;
                            case 2:
                                liveTile = CreateIconicTileData(liveTileOptions);
                                break;
                            default:
                                liveTile = CreateFlipTileDate(liveTileOptions);
                                break;
                        }

                        PhoneApplicationPage currentPage = ((PhoneApplicationFrame)Application.Current.RootVisual).Content as PhoneApplicationPage;
                        string currentUri = currentPage.NavigationService.Source.ToString().Split('?')[0];
                        ShellTile.Create(new Uri(currentUri + "?Uri=" + liveTileOptions.SecondaryTileUri, UriKind.Relative), liveTile, true);
                        tileTypes.Add(currentUri + "?Uri=" + liveTileOptions.SecondaryTileUri, liveTileOptions.TileType);
                        AppSettings.StoreSetting(TileSettingsName, tileTypes);
                        DispatchCommandResult(new PluginResult(PluginResult.Status.OK));
                    });
                }
            }
            catch
            {
                DispatchCommandResult(new PluginResult(PluginResult.Status.ERROR, "Error creating iconic tile"));
            }
        }

        /// <summary>
        /// Tries to parse options string
        /// </summary>
        /// <param name="options"></param>
        /// <param name="tileOptions"></param>
        /// <returns></returns>
        private bool TryGetTileOptions(string options, out LiveTilesOptions tileOptions)
        {
            bool result = false;

            try
            {
                tileOptions = JsonHelper.Deserialize<LiveTilesOptions>(options);
                result = true;
            }
            catch
            {
                tileOptions = null;
                DispatchCommandResult(new PluginResult(PluginResult.Status.JSON_EXCEPTION));
            }

            return result;
        }

        #region creating tile data

        /// <summary>
        /// Creates flip tile data
        /// </summary>
        private FlipTileData CreateFlipTileDate(LiveTilesOptions liveTileOptions)
        {
            FlipTileData flipTile = new FlipTileData();
            if (!string.IsNullOrEmpty(liveTileOptions.Title))
            {
                flipTile.Title = liveTileOptions.Title;
            }
            if (!string.IsNullOrEmpty(liveTileOptions.Image))
            {
                flipTile.BackgroundImage = new Uri(liveTileOptions.Image, UriKind.RelativeOrAbsolute);
            }
            if (liveTileOptions.Count > 0)
            {
                flipTile.Count = liveTileOptions.Count;
            }
            if (!string.IsNullOrEmpty(liveTileOptions.BackTitle))
            {
                flipTile.BackTitle = liveTileOptions.BackTitle;
            }
            if (!string.IsNullOrEmpty(liveTileOptions.BackContent))
            {
                flipTile.BackContent = liveTileOptions.BackContent;
            }
            if (!string.IsNullOrEmpty(liveTileOptions.BackImage))
            {
                flipTile.BackBackgroundImage = new Uri(liveTileOptions.BackImage, UriKind.RelativeOrAbsolute);
            }
            if (!string.IsNullOrEmpty(liveTileOptions.SmallImage))
            {
                flipTile.SmallBackgroundImage = new Uri(liveTileOptions.SmallImage, UriKind.RelativeOrAbsolute);
            }
            if (!string.IsNullOrEmpty(liveTileOptions.WideImage))
            {
                flipTile.WideBackgroundImage = new Uri(liveTileOptions.WideImage, UriKind.RelativeOrAbsolute);
            }
            if (!string.IsNullOrEmpty(liveTileOptions.WideContent))
            {
                flipTile.WideBackContent = liveTileOptions.WideContent;
            }
            if (!string.IsNullOrEmpty(liveTileOptions.WideBackImage))
            {
                flipTile.WideBackBackgroundImage = new Uri(liveTileOptions.WideBackImage, UriKind.RelativeOrAbsolute);
            }

            return flipTile;            
        }

        private CycleTileData CreateCycleTileData(LiveTilesOptions liveTileOptions)
        {
            CycleTileData tileData = new CycleTileData();
            if (!string.IsNullOrEmpty(liveTileOptions.Title))
            {
                tileData.Title = liveTileOptions.Title;
            }
            if (liveTileOptions.Count > 0)
            {
                tileData.Count = liveTileOptions.Count;
            }
            if (!string.IsNullOrEmpty(liveTileOptions.Image))
            {
                string[] imgs = liveTileOptions.Image.Split('|');
                Uri[] imgUris = new Uri[imgs.Length];
                for (int i = 0; i < imgs.Length; ++i)
                {
                    imgUris[i] = new Uri(imgs[i], UriKind.Relative);
                }
                tileData.CycleImages = imgUris;
                tileData.SmallBackgroundImage = imgUris[0];
            }
            return tileData;
        }

        /// <summary>
        /// Creates iconic tile data.
        /// </summary>        
        private IconicTileData CreateIconicTileData(LiveTilesOptions liveTileOptions)
        {
            IconicTileData tileData = new IconicTileData();
            if (!string.IsNullOrEmpty(liveTileOptions.Title))
            {
                tileData.Title = liveTileOptions.Title;
            }
            if (liveTileOptions.Count > 0)
            {
                tileData.Count = liveTileOptions.Count;
            }
            if (!string.IsNullOrEmpty(liveTileOptions.Image))
            {
                tileData.IconImage = new Uri(liveTileOptions.Image, UriKind.RelativeOrAbsolute);
            }
            if (!string.IsNullOrEmpty(liveTileOptions.SmallImage))
            {
                tileData.SmallIconImage = new Uri(liveTileOptions.SmallImage, UriKind.RelativeOrAbsolute);
            }
            if (!string.IsNullOrEmpty(liveTileOptions.BackgroundColor))
            {
                tileData.BackgroundColor = LiveTiles.GetColorFromHexString(liveTileOptions.BackgroundColor);
            }            
            if (!string.IsNullOrEmpty(liveTileOptions.WideContent))
            {
                string[] content = liveTileOptions.WideContent.Split('|');
                int length = content.Length;
                if (length >= 1)
                {
                    tileData.WideContent1 = content[0];
                }
                if (length >= 2)
                {
                    tileData.WideContent2 = content[1];
                }
                if (length >= 3)
                {
                    tileData.WideContent3 = content[2];
                }                               
            }
            return tileData;
        }

        #endregion

        #region util methods

        /// <summary>
        /// Converts string in hex color format to Color object. 
        /// </summary>
        /// <param name="hexColor">the string in hex color format.</param>
        /// <returns>Color object.</returns>
        private static Color GetColorFromHexString(string hexColor)
        {
            const int fromBase = 16;
            hexColor = hexColor.Trim().TrimStart('#');

            if (hexColor.Length != 8 && hexColor.Length != 6)
            {
                throw new ArgumentException("string is not in a valid hex color");
            }

            int parseIndex = 0;
            bool hasAlphaChannel = hexColor.Length == 8;
            byte alpha = 255;

            if (hasAlphaChannel)
            {
                alpha = Convert.ToByte(hexColor.Substring(0, 2), fromBase);
                parseIndex += 2;
            }

            byte red = Convert.ToByte(hexColor.Substring(parseIndex, 2), fromBase);
            parseIndex += 2;
            byte green = Convert.ToByte(hexColor.Substring(parseIndex, 2), fromBase);
            parseIndex += 2;
            byte blue = Convert.ToByte(hexColor.Substring(parseIndex, 2), fromBase);

            return Color.FromArgb(alpha, red, green, blue);
        }

        /// <summary>
        /// Provides access to the app settings
        /// </summary>
        public static class AppSettings
        {
            private static IsolatedStorageSettings Settings = IsolatedStorageSettings.ApplicationSettings;

            public static void StoreSetting(string settingName, string value)
            {
                StoreSetting<string>(settingName, value);
            }

            public static void StoreSetting<TValue>(string settingName, TValue value)
            {
                if (!Settings.Contains(settingName))
                {
                    Settings.Add(settingName, value);
                }
                else
                {
                    Settings[settingName] = value;
                }                    

                Settings.Save();
            }

            public static bool TryGetSetting<TValue>(string settingName, out TValue value)
            {
                bool result = false;
                value = default(TValue);
                try
                {
                    if (Settings.Contains(settingName))
                    {
                        value = (TValue)Settings[settingName];
                        result = true;
                    }
                }
                catch (Exception e)
                {
                }
                return result;
            }
        }
        #endregion
    }
}