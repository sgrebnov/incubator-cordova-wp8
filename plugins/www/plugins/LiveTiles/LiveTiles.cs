/*
 * PhoneGap is available under *either* the terms of the modified BSD license *or* the
 * MIT License (2008). See http://opensource.org/licenses/alphabetical for full text.
 *
 * Copyright (c) 2005-2011, Nitobi Software Inc.
 * Copyright (c) 2011, Microsoft Corporation
 */

using System;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using WPCordovaClassLib.Cordova;
using WPCordovaClassLib.Cordova.Commands;
using WPCordovaClassLib.Cordova.JSON;

namespace Cordova.Extension.Commands
{
    /// <summary>
    /// Implementes access to application live tiles
    /// http://msdn.microsoft.com/en-us/library/hh202948(v=VS.92).aspx
    /// </summary>
    public class LiveTiles : BaseCommand
    {

        #region Live tiles options
        
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

            if (string.IsNullOrEmpty(liveTileOptions.Title) || string.IsNullOrEmpty(liveTileOptions.Image) || string.IsNullOrEmpty(liveTileOptions.SecondaryTileUri))
            {
                DispatchCommandResult(new PluginResult(PluginResult.Status.JSON_EXCEPTION));
                return;
            }
            try
            {
                ShellTile foundTile = ShellTile.ActiveTiles.FirstOrDefault(x => x.NavigationUri.ToString().Contains(liveTileOptions.SecondaryTileUri));                
                if (foundTile == null)
                {
                    FlipTileData secondaryTile = CreateFlipTileDate(liveTileOptions);
                    PhoneApplicationPage currentPage;
                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
                        currentPage = ((PhoneApplicationFrame)Application.Current.RootVisual).Content as PhoneApplicationPage;
                        string currentUri = currentPage.NavigationService.Source.ToString().Split('?')[0];
                        ShellTile.Create(new Uri(currentUri + "?Uri=" + liveTileOptions.SecondaryTileUri, UriKind.Relative), secondaryTile, true);                        
                        DispatchCommandResult(new PluginResult(PluginResult.Status.OK));
                    });                                                            
                }
                else
                {
                    DispatchCommandResult(new PluginResult(PluginResult.Status.ERROR,"Tile already exist"));
                }                
            }
            catch (Exception)
            {
                DispatchCommandResult(new PluginResult(PluginResult.Status.ERROR,"Error creating secondary live tile"));
            }
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
                    FlipTileData liveTile = this.CreateFlipTileDate(liveTileOptions);
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
        /// Creates cycle tile
        /// </summary>
        public void createCycleTile(string options)
        {
            LiveTilesOptions tileOptions;
            try
            {
                tileOptions = JsonHelper.Deserialize<LiveTilesOptions>(options);
            }
            catch 
            {
                DispatchCommandResult(new PluginResult(PluginResult.Status.JSON_EXCEPTION));
                return;
            }

            if (string.IsNullOrEmpty(tileOptions.SecondaryTileUri))
            {
                DispatchCommandResult(new PluginResult(PluginResult.Status.JSON_EXCEPTION));
                return;
            }

            try
            {
                ShellTile foundTile = ShellTile.ActiveTiles.FirstOrDefault(x => x.NavigationUri.ToString().Contains(tileOptions.SecondaryTileUri));
                if (foundTile != null)
                {
                    DispatchCommandResult(new PluginResult(PluginResult.Status.ERROR, "Tile already exist"));
                }
                else
                {
                    CycleTileData cycleTile = CreateCycleTileData(tileOptions);
                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
                        PhoneApplicationPage currentPage = ((PhoneApplicationFrame)Application.Current.RootVisual).Content as PhoneApplicationPage;
                        string currentUri = currentPage.NavigationService.Source.ToString().Split('?')[0];
                        ShellTile.Create(new Uri(currentUri + "?Uri=" + tileOptions.SecondaryTileUri, UriKind.Relative), cycleTile, true);
                        DispatchCommandResult(new PluginResult(PluginResult.Status.OK));
                    });   
                }
            }
            catch
            {
                DispatchCommandResult(new PluginResult(PluginResult.Status.ERROR, "Error creating cycle tile"));
            }
        }


        /// <summary>
        /// Cerates tile data
        /// </summary>
        private StandardTileData CreateStandardTileData(LiveTilesOptions liveTileOptions)
        {
            StandardTileData standardTile = new StandardTileData();
            if (!string.IsNullOrEmpty(liveTileOptions.Title))
            {
                standardTile.Title = liveTileOptions.Title;
            }
            if (!string.IsNullOrEmpty(liveTileOptions.Image))
            {
                standardTile.BackgroundImage = new Uri(liveTileOptions.Image, UriKind.RelativeOrAbsolute);
            }                        
            if (liveTileOptions.Count > 0)
            {
                standardTile.Count = liveTileOptions.Count;
            }
            if (!string.IsNullOrEmpty(liveTileOptions.BackTitle))
            {
                standardTile.BackTitle = liveTileOptions.BackTitle;
            }
            if (!string.IsNullOrEmpty(liveTileOptions.BackContent))
            {
                standardTile.BackContent = liveTileOptions.BackContent;
            }
            if (!string.IsNullOrEmpty(liveTileOptions.BackImage))
            {
                standardTile.BackBackgroundImage = new Uri(liveTileOptions.BackImage, UriKind.RelativeOrAbsolute);
            }
            return standardTile;
        }

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

    }
}