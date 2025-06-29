using LingoEngine.Casts;
using LingoEngine.Core;
using LingoEngine.Inputs;
using LingoEngine.Members;
using LingoEngine.Movies;
using LingoEngine.Pictures;
using LingoEngine.Primitives;
using LingoEngine.Sounds;
using LingoEngine.Shapes;
using LingoEngine.Texts;
using LingoEngine.Gfx;
using LingoEngine.Sprites;
using LingoEngine.Stages;
using LingoEngine.Bitmaps;

namespace LingoEngine.FrameworkCommunication
{
    /// <summary>
    /// Factory used by the core engine to create framework-specific
    /// implementations of stages, sprites, members and input handlers.
    /// Each rendering adapter provides its own implementation.
    /// </summary>
    public interface ILingoFrameworkFactory
    {
        /// <summary>
        /// Creates the main <see cref="LingoStage"/> for a <see cref="LingoPlayer"/>.
        /// </summary>
        /// <param name="lingoPlayer">Current player instance.</param>
        LingoStage CreateStage(LingoPlayer lingoPlayer);
        /// <summary>
        /// Adds a movie to the specified stage.
        /// </summary>
        LingoMovie AddMovie(LingoStage stage, LingoMovie lingoMovie);


        #region Members
        /// <summary>Creates a new cast member instance.</summary>
        T CreateMember<T>(ILingoCast cast, int numberInCast, string name = "") where T : LingoMember;
        /// <summary>Creates a picture member.</summary>
        LingoMemberBitmap CreateMemberBitmap(ILingoCast cast,int numberInCast, string name = "", string? fileName = null,
            LingoPoint regPoint = default);
        /// <summary>Creates a sound member.</summary>
        LingoMemberSound CreateMemberSound(ILingoCast cast, int numberInCast, string name = "", string? fileName = null, LingoPoint regPoint = default);
        /// <summary>Creates a film loop member.</summary>
        LingoMemberFilmLoop CreateMemberFilmLoop(ILingoCast cast, int numberInCast, string name = "", string? fileName = null, LingoPoint regPoint = default);
        /// <summary>Creates a vector shape member.</summary>
        LingoMemberShape CreateMemberShape(ILingoCast cast, int numberInCast, string name = "", string? fileName = null, LingoPoint regPoint = default);
        /// <summary>Creates a field member.</summary>
        LingoMemberField CreateMemberField(ILingoCast cast, int numberInCast, string name = "", string? fileName = null,
            LingoPoint regPoint = default);
        /// <summary>Creates a text member.</summary>
        LingoMemberText CreateMemberText(ILingoCast cast, int numberInCast, string name = "", string? fileName = null,
            LingoPoint regPoint = default);
        /// <summary>Creates a placeholder member.</summary>
        LingoMember CreateEmpty(ILingoCast cast, int numberInCast, string name = "", string? fileName = null,
            LingoPoint regPoint = default);
        #endregion

        /// <summary>Creates a sound instance.</summary>
        LingoSound CreateSound(ILingoCastLibsContainer castLibsContainer);
        /// <summary>Creates a sound channel.</summary>
        LingoSoundChannel CreateSoundChannel(int number);
        /// <summary>Creates a mouse handler bound to the stage.</summary>
        LingoMouse CreateMouse(LingoStage stage);
        /// <summary>Creates a keyboard handler.</summary>
        LingoKey CreateKey();


        #region Gfx Elements
        /// <summary>
        /// Creates a generic drawing canvas instance.
        /// </summary>
        LingoGfxCanvas CreateGfxCanvas(string name, int width, int height);

        /// <summary>
        /// Creates a wrapping panel container.
        /// </summary>
        LingoGfxWrapPanel CreateWrapPanel(LingoOrientation orientation, string name);

        /// <summary>
        /// Creates a simple panel container for absolute positioning.
        /// </summary>
        LingoGfxPanel CreatePanel(string name);
        LingoGfxLayoutWrapper CreateLayoutWrapper(ILingoGfxNode content, float? x, float? y);

        /// <summary>
        /// Creates a tab container for organizing child panels.
        /// </summary>
        LingoGfxTabContainer CreateTabContainer(string name);
        LingoGfxTabItem CreateTabItem(string name, string title);

        /// <summary>Creates a scroll container.</summary>
        LingoGfxScrollContainer CreateScrollContainer(string name);

        /// <summary>Creates a single line text input.</summary>
        LingoGfxInputText CreateInputText(string name, int maxLength = 0, Action<string>? onChange = null);

        /// <summary>Creates a numeric input field.</summary>
        LingoGfxInputNumber CreateInputNumber(string name, float min = 0, float max = 100, Action<float>? onChange = null);

        /// <summary>Creates a spin box input.</summary>
        LingoGfxSpinBox CreateSpinBox(string name, float min = 0, float max = 100, Action<float>? onChange = null);

        /// <summary>Creates a checkbox input.</summary>
        LingoGfxInputCheckbox CreateInputCheckbox(string name, Action<bool>? onChange = null);

        /// <summary>Creates a combo box input.</summary>
        LingoGfxInputCombobox CreateInputCombobox(string name, Action<string?>? onChange = null);

        /// <summary>Creates a simple text label.</summary>
        LingoGfxLabel CreateLabel(string name, string text = "");

        /// <summary>Creates a clickable button.</summary>
        LingoGfxButton CreateButton(string name, string text = "");

        /// <summary>Creates a menu container.</summary>
        LingoGfxMenu CreateMenu(string name);

        /// <summary>Creates a menu item.</summary>
        LingoGfxMenuItem CreateMenuItem(string name, string? shortcut = null);

        #endregion

        /// <summary>Creates a sprite instance.</summary>
        T CreateSprite<T>(ILingoMovie movie, Action<LingoSprite> onRemoveMe) where T : LingoSprite;
        /// <summary>Creates a sprite behaviour.</summary>
        T CreateBehavior<T>(LingoMovie lingoMovie) where T : LingoSpriteBehavior;
        /// <summary>Creates a movie script.</summary>
        T CreateMovieScript<T>(LingoMovie lingoMovie) where T : LingoMovieScript;
        
    }
}
