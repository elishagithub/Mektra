using System;
namespace Mektra
{
    // Authored world progression. Seeds affect sparse rewards, never the layout order.
    public static class EndlessJourney
    {
        public static int KindCount(LevelData[] library){int count=0;foreach(var course in library)count=Math.Max(count,course.segmentKind+1);return count;}
        static uint Hash(int seed,int wave,int salt)
        {
            uint value=unchecked((uint)seed+(uint)wave*0x9e3779b9u+(uint)salt*0x85ebca6bu);
            value^=value>>16;value*=0x7feb352du;value^=value>>15;value*=0x846ca68bu;return value^(value>>16);
        }
        public static bool HasMagnet(int section,int seed)
        {
            if(section<8)return false;
            // One possible pickup in an eight-section wave, with an irregular four-to-twelve-section gap.
            int slot=1+(int)(Hash(seed,section/8,53)%5);
            return section%8==slot;
        }
        public static int Index(LevelData[] library,int section,int seed)
        {
            if(library[0].discovery)return section%library.Length;
#if UNITY_STANDALONE && MEKTRA_INVERT_PREVIEW
            if(library.Length==4)return section%4;
#endif
            // Authored first lap: four variants per world, with four recovery sections.
            int[] order={0,1,16,2,3,12,17,8,9,18,14,11,4,15,13,7,10,19,20,6};
            int slot=section%library.Length,kind=-1,variant=-1,cursor=0,recovery=0;
            foreach(int world in order)
            {
                for(int stage=0;stage<4;stage++,cursor++)
                    if(cursor==slot){kind=world;variant=stage;}
                if(world==1||world==9||world==15||world==19)
                {
                    if(cursor==slot){kind=5;variant=recovery;}
                    cursor++;recovery++;
                }
            }
            for(int i=0;i<library.Length;i++)if(library[i].segmentKind==kind&&library[i].variant==variant)return i;
            throw new InvalidOperationException("Missing authored journey section "+section);
        }
    }
}
