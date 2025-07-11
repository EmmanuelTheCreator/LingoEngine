from .data import RayScoreKeyFrame, RaySprite

class RaySpriteFactory:
    @staticmethod
    def create_keyframe(sprite: RaySprite, sprite_num: int, frame: int) -> RayScoreKeyFrame:
        kf = RayScoreKeyFrame()
        kf.frame_num = frame
        kf.sprite_num = sprite_num
        kf.loc_h = sprite.loc_h
        kf.loc_v = sprite.loc_v
        kf.width = sprite.width
        kf.height = sprite.height
        kf.rotation = sprite.rotation
        kf.skew = sprite.skew
        kf.blend = sprite.blend
        kf.fore_color = sprite.fore_color
        kf.back_color = sprite.back_color
        kf.ink = sprite.ink
        return kf
