#region License
/* SDL2# - C# Wrapper for SDL2
 *
 * Copyright (c) 2013-2024 Ethan Lee.
 *
 * This software is provided 'as-is', without any express or implied warranty.
 * In no event will the authors be held liable for any damages arising from
 * the use of this software.
 *
 * Permission is granted to anyone to use this software for any purpose,
 * including commercial applications, and to alter it and redistribute it
 * freely, subject to the following restrictions:
 *
 * 1. The origin of this software must not be misrepresented; you must not
 * claim that you wrote the original software. If you use this software in a
 * product, an acknowledgment in the product documentation would be
 * appreciated but is not required.
 *
 * 2. Altered source versions must be plainly marked as such, and must not be
 * misrepresented as being the original software.
 *
 * 3. This notice may not be removed or altered from any source distribution.
 *
 * Ethan "flibitijibibo" Lee <flibitijibibo@flibitijibibo.com>
 *
 */
#endregion

#region Using Statements
using System;
using System.Runtime.InteropServices;
#endregion

namespace LingoEngine.SDL2.SDLL
{
    public static class SDL_ttf
    {
        #region SDL2# Variables

        /* Used by DllImport to load the native library. */
        private const string nativeLibName = "SDL2_ttf";

        #endregion

        #region SDL_ttf.h

        /* Similar to the headers, this is the version we're expecting to be
		 * running with. You will likely want to check this somewhere in your
		 * program!
		 */
        public const int SDL_TTF_MAJOR_VERSION = 2;
        public const int SDL_TTF_MINOR_VERSION = 0;
        public const int SDL_TTF_PATCHLEVEL = 16;

        public const int UNICODE_BOM_NATIVE = 0xFEFF;
        public const int UNICODE_BOM_SWAPPED = 0xFFFE;

        public const int TTF_STYLE_NORMAL = 0x00;
        public const int TTF_STYLE_BOLD = 0x01;
        public const int TTF_STYLE_ITALIC = 0x02;
        public const int TTF_STYLE_UNDERLINE = 0x04;
        public const int TTF_STYLE_STRIKETHROUGH = 0x08;

        public const int TTF_HINTING_NORMAL = 0;
        public const int TTF_HINTING_LIGHT = 1;
        public const int TTF_HINTING_MONO = 2;
        public const int TTF_HINTING_NONE = 3;
        public const int TTF_HINTING_LIGHT_SUBPIXEL = 4; /* >= 2.0.16 */

        public static void SDL_TTF_VERSION(out SDL.SDL_version X)
        {
            X.major = SDL_TTF_MAJOR_VERSION;
            X.minor = SDL_TTF_MINOR_VERSION;
            X.patch = SDL_TTF_PATCHLEVEL;
        }

        [DllImport(nativeLibName, EntryPoint = "TTF_LinkedVersion", CallingConvention = CallingConvention.Cdecl)]
        private static extern nint INTERNAL_TTF_LinkedVersion();
        public static SDL.SDL_version TTF_LinkedVersion()
        {
            SDL.SDL_version result;
            nint result_ptr = INTERNAL_TTF_LinkedVersion();
            result = SDL.PtrToStructure<SDL.SDL_version>(
                result_ptr
            );
            return result;
        }

        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void TTF_ByteSwappedUNICODE(int swapped);

        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int TTF_Init();

        /* IntPtr refers to a TTF_Font* */
        [DllImport(nativeLibName, EntryPoint = "TTF_OpenFont", CallingConvention = CallingConvention.Cdecl)]
        private static extern unsafe nint INTERNAL_TTF_OpenFont(
            byte* file,
            int ptsize
        );
        public static unsafe nint TTF_OpenFont(string file, int ptsize)
        {
            byte* utf8File = SDL.Utf8EncodeHeap(file);
            nint handle = INTERNAL_TTF_OpenFont(
                utf8File,
                ptsize
            );
            Marshal.FreeHGlobal((nint)utf8File);
            return handle;
        }

        /* src refers to an SDL_RWops*, IntPtr to a TTF_Font* */
        /* THIS IS A PUBLIC RWops FUNCTION! */
        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern nint TTF_OpenFontRW(
            nint src,
            int freesrc,
            int ptsize
        );

        /* IntPtr refers to a TTF_Font* */
        [DllImport(nativeLibName, EntryPoint = "TTF_OpenFontIndex", CallingConvention = CallingConvention.Cdecl)]
        private static extern unsafe nint INTERNAL_TTF_OpenFontIndex(
            byte* file,
            int ptsize,
            long index
        );
        public static unsafe nint TTF_OpenFontIndex(
            string file,
            int ptsize,
            long index
        )
        {
            byte* utf8File = SDL.Utf8EncodeHeap(file);
            nint handle = INTERNAL_TTF_OpenFontIndex(
                utf8File,
                ptsize,
                index
            );
            Marshal.FreeHGlobal((nint)utf8File);
            return handle;
        }

        /* src refers to an SDL_RWops*, IntPtr to a TTF_Font* */
        /* THIS IS A PUBLIC RWops FUNCTION! */
        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern nint TTF_OpenFontIndexRW(
            nint src,
            int freesrc,
            int ptsize,
            long index
        );

        /* font refers to a TTF_Font*
		 * Only available in 2.0.16 or higher.
		 */
        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int TTF_SetFontSize(
            nint font,
            int ptsize
        );

        /* font refers to a TTF_Font* */
        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int TTF_GetFontStyle(nint font);

        /* font refers to a TTF_Font* */
        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void TTF_SetFontStyle(nint font, int style);

        /* font refers to a TTF_Font* */
        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int TTF_GetFontOutline(nint font);

        /* font refers to a TTF_Font* */
        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void TTF_SetFontOutline(nint font, int outline);

        /* font refers to a TTF_Font* */
        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int TTF_GetFontHinting(nint font);

        /* font refers to a TTF_Font* */
        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void TTF_SetFontHinting(nint font, int hinting);

        /* font refers to a TTF_Font* */
        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int TTF_FontHeight(nint font);

        /* font refers to a TTF_Font* */
        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int TTF_FontAscent(nint font);

        /* font refers to a TTF_Font* */
        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int TTF_FontDescent(nint font);

        /* font refers to a TTF_Font* */
        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int TTF_FontLineSkip(nint font);

        /* font refers to a TTF_Font* */
        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int TTF_GetFontKerning(nint font);

        /* font refers to a TTF_Font* */
        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void TTF_SetFontKerning(nint font, int allowed);

        /* font refers to a TTF_Font*.
		 * IntPtr is actually a C long! This ignores Win64!
		 */
        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern nint TTF_FontFaces(nint font);

        /* font refers to a TTF_Font* */
        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int TTF_FontFaceIsFixedWidth(nint font);

        /* font refers to a TTF_Font* */
        [DllImport(nativeLibName, EntryPoint = "TTF_FontFaceFamilyName", CallingConvention = CallingConvention.Cdecl)]
        private static extern nint INTERNAL_TTF_FontFaceFamilyName(
            nint font
        );
        public static string TTF_FontFaceFamilyName(nint font)
        {
            return SDL.UTF8_ToManaged(
                INTERNAL_TTF_FontFaceFamilyName(font)
            );
        }

        /* font refers to a TTF_Font* */
        [DllImport(nativeLibName, EntryPoint = "TTF_FontFaceStyleName", CallingConvention = CallingConvention.Cdecl)]
        private static extern nint INTERNAL_TTF_FontFaceStyleName(
            nint font
        );
        public static string TTF_FontFaceStyleName(nint font)
        {
            return SDL.UTF8_ToManaged(
                INTERNAL_TTF_FontFaceStyleName(font)
            );
        }

        /* font refers to a TTF_Font* */
        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int TTF_GlyphIsProvided(nint font, ushort ch);

        /* font refers to a TTF_Font*
		 * Only available in 2.0.16 or higher.
		 */
        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int TTF_GlyphIsProvided32(nint font, uint ch);

        /* font refers to a TTF_Font* */
        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int TTF_GlyphMetrics(
            nint font,
            ushort ch,
            out int minx,
            out int maxx,
            out int miny,
            out int maxy,
            out int advance
        );

        /* font refers to a TTF_Font*
		 * Only available in 2.0.16 or higher.
		 */
        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int TTF_GlyphMetrics32(
            nint font,
            uint ch,
            out int minx,
            out int maxx,
            out int miny,
            out int maxy,
            out int advance
        );

        /* font refers to a TTF_Font* */
        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int TTF_SizeText(
            nint font,
            [In()] [MarshalAs(UnmanagedType.LPStr)]
                string text,
            out int w,
            out int h
        );

        /* font refers to a TTF_Font* */
        [DllImport(nativeLibName, EntryPoint = "TTF_SizeUTF8", CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe int INTERNAL_TTF_SizeUTF8(
            nint font,
            byte* text,
            out int w,
            out int h
        );
        public static unsafe int TTF_SizeUTF8(
            nint font,
            string text,
            out int w,
            out int h
        )
        {
            byte* utf8Text = SDL.Utf8EncodeHeap(text);
            int result = INTERNAL_TTF_SizeUTF8(
                font,
                utf8Text,
                out w,
                out h
            );
            Marshal.FreeHGlobal((nint)utf8Text);
            return result;
        }

        /* font refers to a TTF_Font* */
        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int TTF_SizeUNICODE(
            nint font,
            [In()] [MarshalAs(UnmanagedType.LPWStr)]
                string text,
            out int w,
            out int h
        );

        /* font refers to a TTF_Font*
		 * Only available in 2.0.16 or higher.
		 */
        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int TTF_MeasureText(
            nint font,
            [In()] [MarshalAs(UnmanagedType.LPStr)]
                string text,
            int measure_width,
            out int extent,
            out int count
        );

        /* font refers to a TTF_Font*
		 * Only available in 2.0.16 or higher.
		 */
        [DllImport(nativeLibName, EntryPoint = "TTF_MeasureUTF8", CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe int INTERNAL_TTF_MeasureUTF8(
            nint font,
            byte* text,
            int measure_width,
            out int extent,
            out int count
        );
        public static unsafe int TTF_MeasureUTF8(
            nint font,
            string text,
            int measure_width,
            out int extent,
            out int count
        )
        {
            byte* utf8Text = SDL.Utf8EncodeHeap(text);
            int result = INTERNAL_TTF_MeasureUTF8(
                font,
                utf8Text,
                measure_width,
                out extent,
                out count
            );
            Marshal.FreeHGlobal((nint)utf8Text);
            return result;
        }

        /* font refers to a TTF_Font*
		 * Only available in 2.0.16 or higher.
		 */
        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int TTF_MeasureUNICODE(
            nint font,
            [In()] [MarshalAs(UnmanagedType.LPWStr)]
                string text,
            int measure_width,
            out int extent,
            out int count
        );

        /* IntPtr refers to an SDL_Surface*, font to a TTF_Font* */
        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern nint TTF_RenderText_Solid(
            nint font,
            [In()] [MarshalAs(UnmanagedType.LPStr)]
                string text,
            SDL.SDL_Color fg
        );

        /* IntPtr refers to an SDL_Surface*, font to a TTF_Font* */
        [DllImport(nativeLibName, EntryPoint = "TTF_RenderUTF8_Solid", CallingConvention = CallingConvention.Cdecl)]
        private static extern unsafe nint INTERNAL_TTF_RenderUTF8_Solid(
            nint font,
            byte* text,
            SDL.SDL_Color fg
        );
        public static unsafe nint TTF_RenderUTF8_Solid(
            nint font,
            string text,
            SDL.SDL_Color fg
        )
        {
            byte* utf8Text = SDL.Utf8EncodeHeap(text);
            nint result = INTERNAL_TTF_RenderUTF8_Solid(
                font,
                utf8Text,
                fg
            );
            Marshal.FreeHGlobal((nint)utf8Text);
            return result;
        }

        /* IntPtr refers to an SDL_Surface*, font to a TTF_Font* */
        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern nint TTF_RenderUNICODE_Solid(
            nint font,
            [In()] [MarshalAs(UnmanagedType.LPWStr)]
                string text,
            SDL.SDL_Color fg
        );

        /* IntPtr refers to an SDL_Surface*, font to a TTF_Font*
		 * Only available in 2.0.16 or higher.
		 */
        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern nint TTF_RenderText_Solid_Wrapped(
            nint font,
            [In()] [MarshalAs(UnmanagedType.LPStr)]
                string text,
            SDL.SDL_Color fg,
            uint wrapLength
        );

        /* IntPtr refers to an SDL_Surface*, font to a TTF_Font*
		 * Only available in 2.0.16 or higher.
		 */
        [DllImport(nativeLibName, EntryPoint = "TTF_RenderUTF8_Solid_Wrapped", CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe nint INTERNAL_TTF_RenderUTF8_Solid_Wrapped(
            nint font,
            byte* text,
            SDL.SDL_Color fg,
            uint wrapLength
        );
        public static unsafe nint TTF_RenderUTF8_Solid_Wrapped(
            nint font,
            string text,
            SDL.SDL_Color fg,
            uint wrapLength
        )
        {
            byte* utf8Text = SDL.Utf8EncodeHeap(text);
            nint result = INTERNAL_TTF_RenderUTF8_Solid_Wrapped(
                font,
                utf8Text,
                fg,
                wrapLength
            );
            Marshal.FreeHGlobal((nint)utf8Text);
            return result;
        }

        /* IntPtr refers to an SDL_Surface*, font to a TTF_Font*
		 * Only available in 2.0.16 or higher.
		 */
        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern nint TTF_RenderUNICODE_Solid_Wrapped(
            nint font,
            [In()] [MarshalAs(UnmanagedType.LPWStr)]
                string text,
            SDL.SDL_Color fg,
            uint wrapLength
        );

        /* IntPtr refers to an SDL_Surface*, font to a TTF_Font* */
        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern nint TTF_RenderGlyph_Solid(
            nint font,
            ushort ch,
            SDL.SDL_Color fg
        );

        /* IntPtr refers to an SDL_Surface*, font to a TTF_Font*
		 * Only available in 2.0.16 or higher.
		 */
        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern nint TTF_RenderGlyph32_Solid(
            nint font,
            uint ch,
            SDL.SDL_Color fg
        );

        /* IntPtr refers to an SDL_Surface*, font to a TTF_Font* */
        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern nint TTF_RenderText_Shaded(
            nint font,
            [In()] [MarshalAs(UnmanagedType.LPStr)]
                string text,
            SDL.SDL_Color fg,
            SDL.SDL_Color bg
        );

        /* IntPtr refers to an SDL_Surface*, font to a TTF_Font* */
        [DllImport(nativeLibName, EntryPoint = "TTF_RenderUTF8_Shaded", CallingConvention = CallingConvention.Cdecl)]
        private static extern unsafe nint INTERNAL_TTF_RenderUTF8_Shaded(
            nint font,
            byte* text,
            SDL.SDL_Color fg,
            SDL.SDL_Color bg
        );
        public static unsafe nint TTF_RenderUTF8_Shaded(
            nint font,
            string text,
            SDL.SDL_Color fg,
            SDL.SDL_Color bg
        )
        {
            byte* utf8Text = SDL.Utf8EncodeHeap(text);
            nint result = INTERNAL_TTF_RenderUTF8_Shaded(
                font,
                utf8Text,
                fg,
                bg
            );
            Marshal.FreeHGlobal((nint)utf8Text);
            return result;
        }

        /* IntPtr refers to an SDL_Surface*, font to a TTF_Font* */
        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern nint TTF_RenderUNICODE_Shaded(
            nint font,
            [In()] [MarshalAs(UnmanagedType.LPWStr)]
                string text,
            SDL.SDL_Color fg,
            SDL.SDL_Color bg
        );

        /* IntPtr refers to an SDL_Surface*, font to a TTF_Font* */
        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern nint TTF_RenderText_Shaded_Wrapped(
            nint font,
            [In()] [MarshalAs(UnmanagedType.LPStr)]
                string text,
            SDL.SDL_Color fg,
            SDL.SDL_Color bg,
            uint wrapLength
        );

        /* IntPtr refers to an SDL_Surface*, font to a TTF_Font*
		 * Only available in 2.0.16 or higher.
		 */
        [DllImport(nativeLibName, EntryPoint = "TTF_RenderUTF8_Shaded_Wrapped", CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe nint INTERNAL_TTF_RenderUTF8_Shaded_Wrapped(
            nint font,
            byte* text,
            SDL.SDL_Color fg,
            SDL.SDL_Color bg,
            uint wrapLength
        );
        public static unsafe nint TTF_RenderUTF8_Shaded_Wrapped(
            nint font,
            string text,
            SDL.SDL_Color fg,
            SDL.SDL_Color bg,
            uint wrapLength
        )
        {
            byte* utf8Text = SDL.Utf8EncodeHeap(text);
            nint result = INTERNAL_TTF_RenderUTF8_Shaded_Wrapped(
                font,
                utf8Text,
                fg,
                bg,
                wrapLength
            );
            Marshal.FreeHGlobal((nint)utf8Text);
            return result;
        }

        /* IntPtr refers to an SDL_Surface*, font to a TTF_Font* */
        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern nint TTF_RenderUNICODE_Shaded_Wrapped(
            nint font,
            [In()] [MarshalAs(UnmanagedType.LPWStr)]
                string text,
            SDL.SDL_Color fg,
            SDL.SDL_Color bg,
            uint wrapLength
        );

        /* IntPtr refers to an SDL_Surface*, font to a TTF_Font* */
        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern nint TTF_RenderGlyph_Shaded(
            nint font,
            ushort ch,
            SDL.SDL_Color fg,
            SDL.SDL_Color bg
        );

        /* IntPtr refers to an SDL_Surface*, font to a TTF_Font*
		 * Only available in 2.0.16 or higher.
		 */
        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern nint TTF_RenderGlyph32_Shaded(
            nint font,
            uint ch,
            SDL.SDL_Color fg,
            SDL.SDL_Color bg
        );

        /* IntPtr refers to an SDL_Surface*, font to a TTF_Font* */
        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern nint TTF_RenderText_Blended(
            nint font,
            [In()] [MarshalAs(UnmanagedType.LPStr)]
                string text,
            SDL.SDL_Color fg
        );

        /* IntPtr refers to an SDL_Surface*, font to a TTF_Font* */
        [DllImport(nativeLibName, EntryPoint = "TTF_RenderUTF8_Blended", CallingConvention = CallingConvention.Cdecl)]
        private static extern unsafe nint INTERNAL_TTF_RenderUTF8_Blended(
            nint font,
            byte* text,
            SDL.SDL_Color fg
        );
        public static unsafe nint TTF_RenderUTF8_Blended(
            nint font,
            string text,
            SDL.SDL_Color fg
        )
        {
            byte* utf8Text = SDL.Utf8EncodeHeap(text);
            nint result = INTERNAL_TTF_RenderUTF8_Blended(
                font,
                utf8Text,
                fg
            );
            Marshal.FreeHGlobal((nint)utf8Text);
            return result;
        }

        /* IntPtr refers to an SDL_Surface*, font to a TTF_Font* */
        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern nint TTF_RenderUNICODE_Blended(
            nint font,
            [In()] [MarshalAs(UnmanagedType.LPWStr)]
                string text,
            SDL.SDL_Color fg
        );

        /* IntPtr refers to an SDL_Surface*, font to a TTF_Font* */
        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern nint TTF_RenderText_Blended_Wrapped(
            nint font,
            [In()] [MarshalAs(UnmanagedType.LPStr)]
                string text,
            SDL.SDL_Color fg,
            uint wrapped
        );

        /* IntPtr refers to an SDL_Surface*, font to a TTF_Font* */
        [DllImport(nativeLibName, EntryPoint = "TTF_RenderUTF8_Blended_Wrapped", CallingConvention = CallingConvention.Cdecl)]
        private static extern unsafe nint INTERNAL_TTF_RenderUTF8_Blended_Wrapped(
            nint font,
            byte* text,
            SDL.SDL_Color fg,
            uint wrapped
        );
        public static unsafe nint TTF_RenderUTF8_Blended_Wrapped(
            nint font,
            string text,
            SDL.SDL_Color fg,
            uint wrapped
        )
        {
            byte* utf8Text = SDL.Utf8EncodeHeap(text);
            nint result = INTERNAL_TTF_RenderUTF8_Blended_Wrapped(
                font,
                utf8Text,
                fg,
                wrapped
            );
            Marshal.FreeHGlobal((nint)utf8Text);
            return result;
        }

        /* IntPtr refers to an SDL_Surface*, font to a TTF_Font* */
        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern nint TTF_RenderUNICODE_Blended_Wrapped(
            nint font,
            [In()] [MarshalAs(UnmanagedType.LPWStr)]
                string text,
            SDL.SDL_Color fg,
            uint wrapped
        );

        /* IntPtr refers to an SDL_Surface*, font to a TTF_Font* */
        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern nint TTF_RenderGlyph_Blended(
            nint font,
            ushort ch,
            SDL.SDL_Color fg
        );

        /* IntPtr refers to an SDL_Surface*, font to a TTF_Font*
		 * Only available in 2.0.16 or higher.
		 */
        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern nint TTF_RenderGlyph32_Blended(
            nint font,
            uint ch,
            SDL.SDL_Color fg
        );

        /* Only available in 2.0.16 or higher. */
        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int TTF_SetDirection(int direction);

        /* Only available in 2.0.16 or higher. */
        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int TTF_SetScript(int script);

        /* font refers to a TTF_Font* */
        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void TTF_CloseFont(nint font);

        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void TTF_Quit();

        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int TTF_WasInit();

        /* font refers to a TTF_Font* */
        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int SDL_GetFontKerningSize(
            nint font,
            int prev_index,
            int index
        );

        /* font refers to a TTF_Font*
		 * Only available in 2.0.15 or higher.
		 */
        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int TTF_GetFontKerningSizeGlyphs(
            nint font,
            ushort previous_ch,
            ushort ch
        );

        /* font refers to a TTF_Font*
		 * Only available in 2.0.16 or higher.
		 */
        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int TTF_GetFontKerningSizeGlyphs32(
            nint font,
            ushort previous_ch,
            ushort ch
        );

        public static string TTF_GetError()
        {
            return SDL.SDL_GetError();
        }

        public static void TTF_SetError(string fmtAndArglist)
        {
            SDL.SDL_SetError(fmtAndArglist);
        }

        #endregion
    }
}
