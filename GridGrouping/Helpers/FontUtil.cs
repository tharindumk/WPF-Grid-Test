using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Text;
using System.Reflection;
using Mubasher.ClientTradingPlatform.Infrastructure.Module.Logger;
using System.IO;
using Mubasher.ClientTradingPlatform.Infrastructure.Module.Helpers;

namespace Mubasher.ClientTradingPlatform.Infrastructure.UI.Controls.GridGrouping.Helpers
{
    public class FontUtil
    {
        [ThreadStatic]
        private static PrivateFontCollection privateFonts = null;
        

        private static Dictionary<string, Font> fontsByKey = new Dictionary<string, Font>();
        private static FieldInfo fieldNativeFont = typeof(Font).GetField("nativeFont", BindingFlags.NonPublic | BindingFlags.Instance);

        /// <summary></summary>
        internal static PrivateFontCollection PrivateFonts
        {
            get
            {
                if (privateFonts == null)
                {
                    privateFonts = new PrivateFontCollection();
                }
                return privateFonts;
            }
        }

        public static FontFamily GetPrivateFont(string familyName)
        {
            if (privateFonts == null)
            {
                return null;
            }

            foreach (FontFamily ff in privateFonts.Families)
            {
                if (ff.Name == familyName)
                {
                    return ff;
                }
            }

            return null;
        }

        public static Font GetOrCreateImageFont()
        {
            Font imageFont = null;

            try
            {
                if (imageFont == null)
                {
                    PrivateFontCollection fontCollection = new PrivateFontCollection();
                    fontCollection.AddFontFile(Path.Combine(DirectoryHelper.AppExecutingPath, @"Resources\FontIcons.ttf"));
                    imageFont = new Font((FontFamily)fontCollection.Families[0], 11f);
                }
            }
            catch(Exception ex)
            {
                RootServiceProvider.ExceptionHandler.HandleException(ex);
            }

            return imageFont;
        }

        public static Font CreateFont(string facename, float size, FontStyle fontStyle)
        {
            string fontKey = string.Format("{0}~{1}~{2}", facename, size.ToString(), fontStyle.ToString());
            Font dpiFont = null;

            if (fontsByKey.TryGetValue(fontKey, out dpiFont) == false)
            {
                dpiFont = CreateNewFont(facename, size, fontStyle);
                fontsByKey.Add(fontKey, dpiFont);
            }
            else
            {
                try
                {
                    if (fieldNativeFont != null)
                    {
                        IntPtr value = (IntPtr)fieldNativeFont.GetValue(dpiFont);

                        if (value == IntPtr.Zero) // Means the font has been disposed
                        {
                            dpiFont = CreateNewFont(facename, size, fontStyle);
                            fontsByKey[fontKey] = dpiFont;
                        }
                    }
                    //
                    // Check whether the font settings are modified
                    //
                    if (dpiFont != null)
                    {
                        if (dpiFont.FontFamily.Name.ToLower() != facename.ToLower() ||
                            dpiFont.Size != size ||
                            fontStyle != dpiFont.Style)
                        {
                            dpiFont = CreateNewFont(facename, size, fontStyle);
                            fontsByKey[fontKey] = dpiFont;
                        }
                    }
                }
                catch (Exception ex)
                {
                    ExceptionsLogger.LogError(ex);
                }
            }

            // try regular style
            if (dpiFont == null)
            {
                try
                {
                    FontFamily ff = new FontFamily(facename);
                    if (ff.IsStyleAvailable(FontStyle.Regular))
                    {
                        dpiFont = new Font(facename, size, FontStyle.Regular);
                    }
                    else if (ff.IsStyleAvailable(FontStyle.Bold))
                    {
                        dpiFont = new Font(facename, size, FontStyle.Bold);
                    }
                    else if (ff.IsStyleAvailable(FontStyle.Italic))
                    {
                        dpiFont = new Font(facename, size, FontStyle.Italic);
                    }
                    else if (ff.IsStyleAvailable(FontStyle.Underline))
                    {
                        dpiFont = new Font(facename, size, FontStyle.Underline);
                    }
                }
                catch (ArgumentException ex)
                {
                    RootServiceProvider.ExceptionHandler.HandleException(ex, LoggerFileType.Grid);
                }
                catch (Exception ex)
                {
                    RootServiceProvider.ExceptionHandler.HandleException(ex, LoggerFileType.Grid);
                }

                // try different font family
                if (dpiFont == null)
                {
                    try
                    {
                        dpiFont = new Font(FontFamily.GenericSansSerif, size, fontStyle);
                    }
                    catch (ArgumentException ex)
                    {
                        RootServiceProvider.ExceptionHandler.HandleException(ex, LoggerFileType.Grid);
                    }
                    catch (Exception ex)
                    {
                        RootServiceProvider.ExceptionHandler.HandleException(ex, LoggerFileType.Grid);
                    }

                    try
                    {
                        dpiFont = new Font(FontFamily.GenericSansSerif, size, FontStyle.Regular);
                    }
                    catch (ArgumentException ex)
                    {
                        RootServiceProvider.ExceptionHandler.HandleException(ex, LoggerFileType.Grid);
                    }
                    catch (Exception ex)
                    {
                        RootServiceProvider.ExceptionHandler.HandleException(ex, LoggerFileType.Grid);
                    }
                }
            }

            return dpiFont;
        }

        private static Font CreateNewFont(string facename, float size, FontStyle fontStyle)
        {
            Font font = null;

            FontFamily privateFontFamily = FontUtil.GetPrivateFont(facename);

            if (privateFontFamily != null)
            {
                font = new Font(privateFontFamily, size, fontStyle);
                if (font != null)
                {
                    return font;
                }
            }

            try
            {
                font = new Font(facename, size, fontStyle);
            }
            catch (ArgumentException ex)
            {
                RootServiceProvider.ExceptionHandler.HandleException(ex, LoggerFileType.Grid);
            }
            catch (Exception ex)
            {
                RootServiceProvider.ExceptionHandler.HandleException(ex, LoggerFileType.Grid);
            }

            return font;
        }
    }
}
