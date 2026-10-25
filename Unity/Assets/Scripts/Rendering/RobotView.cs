using UnityEngine;
using UnityEngine.UIElements;
namespace Mektra
{
    public class RobotView:VisualElement
    {
        AvatarView avatar;string characterId;
        public RobotView(Character character)
        {
            style.position=Position.Absolute;style.width=40;style.height=40;usageHints=UsageHints.DynamicTransform;pickingMode=PickingMode.Ignore;
            SetCharacter(character);
        }
        void SetCharacter(Character character)
        {
            Clear();characterId=character.id;avatar=new AvatarView(character,128);avatar.style.position=Position.Absolute;avatar.style.left=3;avatar.style.top=3;avatar.style.width=34;avatar.style.height=34;Add(avatar);
        }
        public void Refresh(Simulation sim)
        {
            if(characterId!=sim.character.id)SetCharacter(sim.character);
            bool shown=sim.phase!=Phase.Exploding&&sim.phase!=Phase.Over;style.display=shown?DisplayStyle.Flex:DisplayStyle.None;avatar.Animated=shown&&sim.phase==Phase.Playing;if(!shown)return;
            avatar.Walking=sim.phase==Phase.Playing&&!sim.flying;avatar.Flying=sim.flying;avatar.Happy=sim.time<sim.happyUntil?1:0;avatar.Look=(float)System.Math.Clamp(sim.flying?sim.velocity.x/GameConfig.JumpSpeed:sim.JumpDirection().x*.5,-1,1);
            var normal=sim.normal;style.rotate=new Rotate(Angle.Degrees((float)(System.Math.Atan2(normal.x,-normal.y)*Mathf.Rad2Deg)));
            style.scale=new Scale(new Vector3((float)sim.BodyScale,(float)sim.BodyScale,1));var point=sim.RenderPosition;style.translate=new Translate((float)point.x-20,(float)(point.y-sim.RenderCamera)-20);
        }
    }
}
