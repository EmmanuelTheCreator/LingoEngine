using ArkGodot.GodotLinks;
using Godot;
using LingoEngine.FrameworkCommunication;
using LingoEngine.Movies;
using LingoEngine.Pictures.LingoEngine;
using LingoEngineGodot.Pictures;

namespace LingoEngineGodot.Movies
{
    public class LingoGodotStage : ILingoFrameworkStage, IDisposable
    {
        private LingoStage _LingoStage;
        private HashSet<LingoGodotSprite> _drawnSprites = new();
        private Node2D _stageRoot = new Node2D();
        internal void Init(LingoStage lingoInstance)
        {
            _LingoStage = lingoInstance;
        }
        public void DrawSprite(LingoSprite sprite)
        {
            // Only handle picture members
            if (!(sprite.Member is LingoMemberPicture pictureMember)) return;
            var godotSprite = sprite.FrameworkObj<LingoGodotSprite>();
            if (godotSprite == null) return;
            
            // Set the texture using the ImageTexture from the picture member
            var texture = pictureMember.FrameworkObj<LingoGodotMemberPicture>().Texture;
            if (texture == null)
                return;
            godotSprite.Texture = texture;

            // Set initial position and visibility
            //godotSprite.Position = new Vector2(sprite.LocH, sprite.LocV);
            //godotSprite.Visible = sprite.Visible;

            // Add the sprite node to the stage root if it's not already added
            if (godotSprite.GetParent() == null && _stageRoot != null)
                _stageRoot.AddChild(godotSprite);
            _drawnSprites.Add(godotSprite);
        }

        public void RemoveSprite(LingoSprite sprite)
        {
            var godotSprite = sprite.FrameworkObj<LingoGodotSprite>();
            // Remove the Node2D from the scene tree
            if (godotSprite.GetParent() != null)
            {
                godotSprite.GetParent().RemoveChild(godotSprite);
            }
            // Free the node to release resources
            //godotSprite.QueueFree();
            _drawnSprites.Remove(godotSprite);
        }

        public void UpdateStage()
        {
            foreach (var godotSprite in _drawnSprites)
            {
                if (!(godotSprite.LingoSprite.Member is LingoMemberPicture pictureMember)) return;
                if (godotSprite.IsDirtyMember)
                {
                    var texture = pictureMember.FrameworkObj<LingoGodotMemberPicture>().Texture;
                    if (texture != null)
                        godotSprite.Texture = texture;
                    godotSprite.IsDirtyMember = false;
                }
                if (!godotSprite.IsDirty) continue;
                godotSprite.IsDirty = false;
            }
        }

        public void Dispose()
        {
        }

      
    }
}
